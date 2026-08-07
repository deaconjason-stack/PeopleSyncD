using System.Buffers.Binary;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using PeopleSyncD.Application.Identity;
using PeopleSyncD.Infrastructure.Identity;
using Xunit;

namespace PeopleSyncD.Api.Tests;

public sealed class MfaSessionAssuranceApiTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    [Fact]
    public async Task TotpEnrollmentInvalidatesPasswordSessionAndLoginCompletesWithMfa()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();
        var account = await RegisterAndVerifyAsync(factory, client);

        client.DefaultRequestHeaders.Authorization = Bearer(account.Session.AccessToken);
        var enrollmentResponse = await client.PostAsync("/api/v1/auth/mfa/totp/enroll", null);
        Assert.Equal(HttpStatusCode.OK, enrollmentResponse.StatusCode);
        var enrollment = await enrollmentResponse.Content.ReadFromJsonAsync<MfaTotpEnrollmentDto>(JsonOptions);
        Assert.NotNull(enrollment);
        Assert.False(string.IsNullOrWhiteSpace(enrollment.ManualEntryKey));
        Assert.StartsWith("otpauth://totp/", enrollment.OtpauthUri, StringComparison.Ordinal);

        var enrollmentCode = await GenerateTotpAsync(factory, account.Session.User.Id, -1);
        var confirmationResponse = await client.PostAsJsonAsync(
            "/api/v1/auth/mfa/totp/confirm",
            new ConfirmTotpEnrollmentRequest(enrollmentCode),
            JsonOptions);
        Assert.Equal(HttpStatusCode.OK, confirmationResponse.StatusCode);
        var recovery = await confirmationResponse.Content.ReadFromJsonAsync<RecoveryCodeBatchDto>(JsonOptions);
        Assert.NotNull(recovery);
        Assert.Equal(10, recovery.RecoveryCodes.Count);
        Assert.Equal(10, recovery.RecoveryCodes.Distinct(StringComparer.Ordinal).Count());

        var staleSession = await client.GetAsync("/api/v1/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, staleSession.StatusCode);

        client.DefaultRequestHeaders.Authorization = null;
        var loginResponse = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new LoginRequest(account.Email, account.Password),
            JsonOptions);
        Assert.Equal(HttpStatusCode.Unauthorized, loginResponse.StatusCode);
        var challenge = await loginResponse.Content.ReadFromJsonAsync<MfaChallengeDto>(JsonOptions);
        Assert.NotNull(challenge);
        Assert.Equal("login", challenge.Purpose);
        Assert.Contains("totp", challenge.Methods);
        Assert.Contains("recovery_code", challenge.Methods);

        var loginCode = await GenerateTotpAsync(factory, account.Session.User.Id);
        var completionResponse = await client.PostAsJsonAsync(
            "/api/v1/auth/mfa/complete",
            new MfaChallengeRequest(challenge.ChallengeToken, "totp", loginCode),
            JsonOptions);
        Assert.Equal(HttpStatusCode.OK, completionResponse.StatusCode);
        var mfaSession = await completionResponse.Content.ReadFromJsonAsync<AccessTokenDto>(JsonOptions);
        Assert.NotNull(mfaSession);
        Assert.Equal("mfa", mfaSession.AssuranceLevel);
        Assert.NotNull(mfaSession.SessionFamilyId);
        Assert.NotNull(mfaSession.RefreshToken);

        var replayResponse = await client.PostAsJsonAsync(
            "/api/v1/auth/mfa/complete",
            new MfaChallengeRequest(challenge.ChallengeToken, "totp", loginCode),
            JsonOptions);
        Assert.Equal(HttpStatusCode.Unauthorized, replayResponse.StatusCode);

        client.DefaultRequestHeaders.Authorization = Bearer(mfaSession.AccessToken);
        var security = await client.GetFromJsonAsync<AccountSecurityDto>("/api/v1/auth/security", JsonOptions);
        Assert.NotNull(security);
        Assert.True(security.MfaEnabled);
        Assert.False(security.PasswordOnlyLoginAllowed);
        Assert.Equal(10, security.RecoveryCodesRemaining);

        var sessions = await client.GetFromJsonAsync<IReadOnlyCollection<SessionSummaryDto>>(
            "/api/v1/auth/sessions",
            JsonOptions);
        var current = Assert.Single(sessions!);
        Assert.True(current.IsCurrent);
        Assert.Equal("mfa", current.AssuranceLevel);
    }

    [Fact]
    public async Task TotpCounterCannotBeReusedAcrossDistinctChallenges()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();
        var account = await RegisterVerifyAndEnableMfaAsync(factory, client);
        var code = await GenerateTotpAsync(factory, account.UserId);

        var firstChallenge = await LoginForChallengeAsync(client, account.Email, account.Password);
        var firstCompletion = await client.PostAsJsonAsync(
            "/api/v1/auth/mfa/complete",
            new MfaChallengeRequest(firstChallenge.ChallengeToken, "totp", code),
            JsonOptions);
        Assert.Equal(HttpStatusCode.OK, firstCompletion.StatusCode);
        var firstSession = await firstCompletion.Content.ReadFromJsonAsync<AccessTokenDto>(JsonOptions);
        Assert.NotNull(firstSession);

        var secondChallenge = await LoginForChallengeAsync(client, account.Email, account.Password);
        var replay = await client.PostAsJsonAsync(
            "/api/v1/auth/mfa/complete",
            new MfaChallengeRequest(secondChallenge.ChallengeToken, "totp", code),
            JsonOptions);
        Assert.Equal(HttpStatusCode.Unauthorized, replay.StatusCode);

        client.DefaultRequestHeaders.Authorization = Bearer(firstSession.AccessToken);
        var events = await client.GetFromJsonAsync<IReadOnlyCollection<SecurityEventDto>>(
            "/api/v1/auth/security-events",
            JsonOptions);
        Assert.NotNull(events);
        Assert.Contains(events, item => item.EventType == "identity.mfa.totp_replay_denied");
    }

    [Fact]
    public async Task RecoveryCodeIsSingleUseAcrossLoginChallenges()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();
        var account = await RegisterVerifyAndEnableMfaAsync(factory, client);
        var recoveryCode = Assert.Single(account.RecoveryCodes.Take(1));

        var firstChallenge = await LoginForChallengeAsync(client, account.Email, account.Password);
        var firstCompletion = await client.PostAsJsonAsync(
            "/api/v1/auth/mfa/complete",
            new MfaChallengeRequest(firstChallenge.ChallengeToken, "recovery_code", recoveryCode),
            JsonOptions);
        Assert.Equal(HttpStatusCode.OK, firstCompletion.StatusCode);
        var firstSession = await firstCompletion.Content.ReadFromJsonAsync<AccessTokenDto>(JsonOptions);
        Assert.NotNull(firstSession);
        Assert.Equal("mfa", firstSession.AssuranceLevel);

        var secondChallenge = await LoginForChallengeAsync(client, account.Email, account.Password);
        var reusedRecoveryCode = await client.PostAsJsonAsync(
            "/api/v1/auth/mfa/complete",
            new MfaChallengeRequest(secondChallenge.ChallengeToken, "recovery_code", recoveryCode),
            JsonOptions);
        Assert.Equal(HttpStatusCode.Unauthorized, reusedRecoveryCode.StatusCode);
    }

    [Fact]
    public async Task RevokedSessionFamilyIsRejectedImmediately()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();
        var account = await RegisterVerifyAndEnableMfaAsync(factory, client);

        var first = await CompleteTotpLoginAsync(factory, client, account, 0);
        var second = await CompleteTotpLoginAsync(factory, client, account, 1);
        Assert.NotNull(first.SessionFamilyId);
        Assert.NotNull(second.SessionFamilyId);
        Assert.NotEqual(first.SessionFamilyId, second.SessionFamilyId);

        client.DefaultRequestHeaders.Authorization = Bearer(first.AccessToken);
        var sessions = await client.GetFromJsonAsync<IReadOnlyCollection<SessionSummaryDto>>(
            "/api/v1/auth/sessions",
            JsonOptions);
        Assert.NotNull(sessions);
        Assert.True(sessions.Count >= 2);

        var revoke = await client.DeleteAsync($"/api/v1/auth/sessions/{second.SessionFamilyId:D}");
        Assert.Equal(HttpStatusCode.NoContent, revoke.StatusCode);

        client.DefaultRequestHeaders.Authorization = Bearer(second.AccessToken);
        var revokedAccess = await client.GetAsync("/api/v1/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, revokedAccess.StatusCode);

        client.DefaultRequestHeaders.Authorization = Bearer(first.AccessToken);
        var currentStillValid = await client.GetAsync("/api/v1/auth/me");
        Assert.Equal(HttpStatusCode.OK, currentStillValid.StatusCode);
    }

    private static async Task<EnabledMfaAccount> RegisterVerifyAndEnableMfaAsync(
        WebApplicationFactory<Program> factory,
        HttpClient client)
    {
        var account = await RegisterAndVerifyAsync(factory, client);
        client.DefaultRequestHeaders.Authorization = Bearer(account.Session.AccessToken);
        var enrollmentResponse = await client.PostAsync("/api/v1/auth/mfa/totp/enroll", null);
        enrollmentResponse.EnsureSuccessStatusCode();
        var enrollmentCode = await GenerateTotpAsync(factory, account.Session.User.Id, -1);
        var confirmationResponse = await client.PostAsJsonAsync(
            "/api/v1/auth/mfa/totp/confirm",
            new ConfirmTotpEnrollmentRequest(enrollmentCode),
            JsonOptions);
        confirmationResponse.EnsureSuccessStatusCode();
        var recovery = (await confirmationResponse.Content.ReadFromJsonAsync<RecoveryCodeBatchDto>(JsonOptions))!;
        client.DefaultRequestHeaders.Authorization = null;
        return new EnabledMfaAccount(
            account.Session.User.Id,
            account.Email,
            account.Password,
            recovery.RecoveryCodes);
    }

    private static async Task<RegisteredAccount> RegisterAndVerifyAsync(
        WebApplicationFactory<Program> factory,
        HttpClient client)
    {
        var suffix = Guid.NewGuid().ToString("N");
        var email = $"m23-{suffix}@example.test";
        const string password = "ValidPassword!2026";
        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/register-tenant",
            new RegisterTenantRequest(
                "M2.3 Assurance Test",
                $"m23-{suffix}",
                "M2.3 Owner",
                email,
                password),
            JsonOptions);
        response.EnsureSuccessStatusCode();
        var session = (await response.Content.ReadFromJsonAsync<AccessTokenDto>(JsonOptions))!;
        await ApiFoundationTests.ConfirmEmailAsync(factory, client, session.User.Id);
        return new RegisteredAccount(email, password, session);
    }

    private static async Task<MfaChallengeDto> LoginForChallengeAsync(
        HttpClient client,
        string email,
        string password)
    {
        client.DefaultRequestHeaders.Authorization = null;
        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new LoginRequest(email, password),
            JsonOptions);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var challenge = await response.Content.ReadFromJsonAsync<MfaChallengeDto>(JsonOptions);
        Assert.NotNull(challenge);
        return challenge;
    }

    private static async Task<AccessTokenDto> CompleteTotpLoginAsync(
        WebApplicationFactory<Program> factory,
        HttpClient client,
        EnabledMfaAccount account,
        int counterOffset)
    {
        var challenge = await LoginForChallengeAsync(client, account.Email, account.Password);
        var code = await GenerateTotpAsync(factory, account.UserId, counterOffset);
        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/mfa/complete",
            new MfaChallengeRequest(challenge.ChallengeToken, "totp", code),
            JsonOptions);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AccessTokenDto>(JsonOptions))!;
    }

    private static async Task<string> GenerateTotpAsync(
        WebApplicationFactory<Program> factory,
        Guid userId,
        int counterOffset = 0)
    {
        using var scope = factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await users.FindByIdAsync(userId.ToString("D"));
        Assert.NotNull(user);
        var secret = await users.GetAuthenticatorKeyAsync(user);
        Assert.False(string.IsNullOrWhiteSpace(secret));
        return ComputeTotp(secret, counterOffset);
    }

    private static string ComputeTotp(string base32Secret, int counterOffset)
    {
        var key = DecodeBase32(base32Secret);
        var counter = (DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 30) + counterOffset;
        Span<byte> counterBytes = stackalloc byte[8];
        BinaryPrimitives.WriteInt64BigEndian(counterBytes, counter);
#pragma warning disable CA5350 // ASP.NET authenticator tokens use RFC 6238's interoperable HMAC-SHA1 profile.
        var digest = HMACSHA1.HashData(key, counterBytes);
#pragma warning restore CA5350
        var offset = digest[^1] & 0x0f;
        var binary = ((digest[offset] & 0x7f) << 24)
            | (digest[offset + 1] << 16)
            | (digest[offset + 2] << 8)
            | digest[offset + 3];
        return (binary % 1_000_000).ToString("D6", CultureInfo.InvariantCulture);
    }

    private static byte[] DecodeBase32(string value)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var output = new List<byte>();
        var buffer = 0;
        var bits = 0;
        foreach (var raw in value)
        {
            if (raw is '=' or ' ' or '-')
            {
                continue;
            }

            var index = alphabet.IndexOf(char.ToUpperInvariant(raw));
            if (index < 0)
            {
                throw new FormatException("Authenticator key is not valid Base32.");
            }

            buffer = (buffer << 5) | index;
            bits += 5;
            if (bits < 8)
            {
                continue;
            }

            bits -= 8;
            output.Add((byte)(buffer >> bits));
            buffer &= bits == 0 ? 0 : (1 << bits) - 1;
        }

        return output.ToArray();
    }

    private static AuthenticationHeaderValue Bearer(string token) => new("Bearer", token);

    private sealed record RegisteredAccount(string Email, string Password, AccessTokenDto Session);

    private sealed record EnabledMfaAccount(
        Guid UserId,
        string Email,
        string Password,
        IReadOnlyCollection<string> RecoveryCodes);

    private static WebApplicationFactory<Program> CreateFactory() =>
        new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder.UseSetting("Database:Provider", "InMemory"));
}
