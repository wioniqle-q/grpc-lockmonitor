using LockMonitor.Extensions;
using MonitorGrpcServer.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLockService();
builder.Services.AddGrpc();

var app = builder.Build();

app.MapGrpcService<GrpcLockManager>();
app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

await app.RunAsync();