FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Directory.Build.props Directory.Packages.props global.json ./
COPY src/PeopleSyncD.Domain/PeopleSyncD.Domain.csproj src/PeopleSyncD.Domain/
COPY src/PeopleSyncD.Application/PeopleSyncD.Application.csproj src/PeopleSyncD.Application/
COPY src/PeopleSyncD.Infrastructure/PeopleSyncD.Infrastructure.csproj src/PeopleSyncD.Infrastructure/
COPY src/PeopleSyncD.Api/PeopleSyncD.Api.csproj src/PeopleSyncD.Api/
RUN dotnet restore src/PeopleSyncD.Api/PeopleSyncD.Api.csproj
COPY src/ src/
RUN dotnet publish src/PeopleSyncD.Api/PeopleSyncD.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PeopleSyncD.Api.dll"]
