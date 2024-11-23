using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shop.Functions.Blob;
using Shop.Functions.Configs;
using Shop.Functions.EventGrid;

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

            services.AddOptions<DeliveryOrderProcessorStorageConfig>().Configure<IConfiguration>((settings, configuration) =>
            {
                configuration.GetSection("DeliveryOrderProcessorStorageConfig").Bind(settings);
            });

            services.AddOptions<EventGridConfig>().Configure<IConfiguration>((settings, configuration) =>
            {
                configuration.GetSection("EventGridConfig").Bind(settings);
            });
        });

        return builder;
    }

    public static IHostBuilder ConfigureServices(this IHostBuilder builder)
    {
        builder.ConfigureServices((hostContext, services) =>
        {
            services.AddTransient<OrdersBlobStorageRepository>();
        });
        
        builder.ConfigureServices((hostContext, services) =>
        {
            services.AddTransient<EventGridSender>();
        });

        return builder;
    }
}
