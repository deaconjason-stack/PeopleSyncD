var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume();

var database = postgres.AddDatabase("peoplesyncd");

var redis = builder.AddRedis("cache")
    .WithDataVolume();

builder.AddProject<Projects.PeopleSyncD_Api>("api")
    .WithReference(database)
    .WithReference(redis)
    .WaitFor(database)
    .WaitFor(redis);

builder.Build().Run();
