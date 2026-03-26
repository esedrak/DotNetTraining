using Temporal.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<TemporalWorkerService>();

var host = builder.Build();
host.Run();

