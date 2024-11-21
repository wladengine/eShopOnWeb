using Microsoft.Extensions.Hosting;
using Shop.Functions;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureOptions()
    .ConfigureServices()
    .Build();

host.Run();

