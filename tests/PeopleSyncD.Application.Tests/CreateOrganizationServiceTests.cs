using PeopleSyncD.Application.Interfaces;
using PeopleSyncD.Application.Organizations;
using PeopleSyncD.Domain.Organizations;
using PeopleSyncD.SharedKernel;
using Xunit;

namespace PeopleSyncD.Application.Tests;

public sealed class CreateOrganizationServiceTests
{
    [Fact]
    public async Task ExecuteAsyncWithUniqueSlugPersistsOrganization()
    {
        var repository = new FakeOrganizationRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = new CreateOrganizationService(
            repository,
            unitOfWork,
            new CreateOrganizationValidator(),
            new FixedClock());

        var result = await service.ExecuteAsync(new CreateOrganizationRequest("PeopleSyncD", "peoplesyncd"));

        Assert.True(result.IsSuccess);
        Assert.Equal("peoplesyncd", result.Value.Slug);
        Assert.NotNull(repository.Added);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task ExecuteAsyncWithInvalidSlugDoesNotPersist()
    {
        var repository = new FakeOrganizationRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = new CreateOrganizationService(
            repository,
            unitOfWork,
            new CreateOrganizationValidator(),
            new FixedClock());

        var result = await service.ExecuteAsync(new CreateOrganizationRequest("PeopleSyncD", "Invalid Slug"));

        Assert.True(result.IsFailure);
        Assert.Null(repository.Added);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    private sealed class FakeOrganizationRepository : IOrganizationRepository
    {
        public Organization? Added { get; private set; }

        public Task AddAsync(Organization organization, CancellationToken cancellationToken = default)
        {
            Added = organization;
            return Task.CompletedTask;
        }

        public Task<Organization?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult<Organization?>(null);

        public Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default) =>
            Task.FromResult(false);
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveCount { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveCount++;
            return Task.FromResult(1);
        }
    }

    private sealed class FixedClock : IClock
    {
        public DateTimeOffset UtcNow => new(2026, 8, 5, 0, 0, 0, TimeSpan.Zero);
    }
}
