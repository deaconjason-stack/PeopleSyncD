var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume();
var database = postgres.AddDatabase("peoplesyncd");
var redis = builder.AddRedis("redis");

var api = builder.AddProject<Projects.PeopleSyncD_Api>("api")
    .WithReference(database)
    .WithReference(redis)
    .WaitFor(database)
    .WaitFor(redis);

builder.AddNpmApp("web", "../PeopleSyncD.Web", "dev")
    .WithReference(api)
    .WithEnvironment("NEXT_PUBLIC_API_BASE_URL", "http://localhost:5000")
    .WithHttpEndpoint(port: 3000, env: "PORT");

builder.Build().Run();
