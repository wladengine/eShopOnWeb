using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Shop.Functions;
public static class WebHostExtentions
{
    public static IHostBuilder ConfigureOptions(this IHostBuilder builder)
    {
        builder.ConfigureServices((hostContext, services) =>
         {
             services.AddOptions<OrderItemsReserverStorageConfig>().Configure<IConfiguration>((settings, configuration) =>
             {
                 configuration.GetSection("OrderItemsReserverStorageConfig").Bind(settings);
             });
         });

        return builder;
    }
}
