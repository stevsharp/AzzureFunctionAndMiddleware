using AzzureFunctionAndMidlware.Middleware;

using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(worker => worker.UseMiddleware<ValidateUserMiddleware>())
    .ConfigureServices(_ =>
    {


    })
    .Build();

await host.RunAsync();
