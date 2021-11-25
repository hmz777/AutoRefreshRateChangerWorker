using AutoRefreshRateChangerWorker;
using AutoRefreshRateChangerWorker.Services;

IHost host = Host.CreateDefaultBuilder(args)
    .UseWindowsService(options =>
    {
        options.ServiceName = "Refresh Rate Switcher Service";
    })
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
        services.AddSingleton<PowerManagementService>();
        services.AddSingleton<RefreshRateService>();
    })
    .Build();

await host.RunAsync();