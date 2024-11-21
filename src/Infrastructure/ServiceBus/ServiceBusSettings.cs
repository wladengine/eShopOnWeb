namespace Microsoft.eShopWeb.Infrastructure.ServiceBus;
public class ServiceBusSettings
{
    public const string CONFIG_NAME = "ServiceBus";

    public string ConnectionString { get; set; }
    public string TopicName { get; set; }
}
