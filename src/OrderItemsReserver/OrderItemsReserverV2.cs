using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Shop.Functions.Blob;
using Shop.Functions.Dto;
using Shop.Functions.EventGrid;

namespace Shop.Functions;

public class OrderItemsReserverV2
{
    private readonly ILogger<OrderItemsReserverV2> _logger;
    private readonly OrdersBlobStorageRepository _blobStorageRepository;
    private readonly EventGridSender _gridSender;

    public OrderItemsReserverV2(
        ILogger<OrderItemsReserverV2> logger, 
        OrdersBlobStorageRepository blobStorageRepository,
        EventGridSender gridSender)
    {
        _logger = logger;
        _blobStorageRepository = blobStorageRepository;
        _gridSender = gridSender;
    }

    [Function(nameof(OrderItemsReserverV2))]
    public async Task Run(
        [ServiceBusTrigger("sbq-eshop-new-orders", Connection = "ServiceBusConnection", AutoCompleteMessages = false)]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        _logger.LogInformation("Message ID: {id}", message.MessageId);
        _logger.LogInformation("Message Body: {body}", message.Body);
        _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);

        var order = await message.DeserializeAsync<Order>();

        bool isSuccess = false;
        int retryCount = 0;
        while (!isSuccess && retryCount < 3)
        {
            try
            {
                // Process the message
                await _blobStorageRepository.DumpOrderToBlobStorage(order);

                isSuccess = true;
            }
            catch (Exception ex)
            {
                retryCount++;
                _logger.LogError(ex, "An error occurred processing the message. Retrying in 5 seconds.");
                await Task.Delay(5000);
            }
        }

        if (!isSuccess)
        {
            _logger.LogError("Failed to process the message after 3 retries. Moving to the dead letter queue.");
            await messageActions.DeadLetterMessageAsync(message);

            await _gridSender.PostEventGridEventAsync(
                "orderProvisioningFail", 
                $"Failed to create reservation in blob for order (Id = {order.Id})", 
                order);

            return;
        }

        // Complete the message
        await messageActions.CompleteMessageAsync(message);
    }
}
