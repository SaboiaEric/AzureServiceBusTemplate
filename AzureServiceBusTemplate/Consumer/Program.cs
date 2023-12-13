using Consumer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<IWorker>(_ =>
            new Worker(string.Empty, string.Empty));
    })
    .Build();

host.Run();