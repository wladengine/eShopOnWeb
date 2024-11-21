using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.Extensions.Options;

namespace Microsoft.eShopWeb.Infrastructure.ServiceBus.NewOrders;

public class NewOrderMessageSender
{
    private readonly ServiceBusSettings _settings;
    public NewOrderMessageSender(IOptions<ServiceBusSettings> settings) => _settings = settings.Value;

    public async Task SendNewOrderMessageAsync(Order order)
    {
        await using var client = new ServiceBusClient(_settings.ConnectionString);
        
        var sender = client.CreateSender(_settings.TopicName);

        var orderJson = System.Text.Json.JsonSerializer.Serialize(order);

        var message = new ServiceBusMessage(orderJson);

        await sender.SendMessageAsync(message);
    }
}
