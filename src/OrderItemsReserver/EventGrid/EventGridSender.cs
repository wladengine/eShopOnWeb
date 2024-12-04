using Azure;
using Azure.Identity;
using Azure.Messaging.EventGrid;
using Microsoft.Extensions.Options;
using Shop.Functions.Configs;

namespace Shop.Functions.EventGrid;
public class EventGridSender
{
    private readonly EventGridConfig _eventGridConfig;

    public EventGridSender(IOptions<EventGridConfig> eventGridConfig) => _eventGridConfig = eventGridConfig.Value;

    // see the https://github.com/Azure-Samples/Serverless-Eventing-Platform-for-Microservices/blob/master/shared/src/ContentReactor.Shared/EventGridPublisherService.cs#L16
    public void PostEventGridEvent<T>(string type, string subject, T payload)
    {
        // get the connection details for the Event Grid topic
        var topicEndpointUri = new Uri(_eventGridConfig.TopicEndpoint);

        // prepare the events for submission to Event Grid
        var events = new List<EventGridEvent>
        {
            new(subject: subject, eventType: type, dataVersion: "1", data: payload) {
                Id = Guid.NewGuid().ToString(),
                EventTime = DateTime.UtcNow,
            }
        };

        // publish the events

        EventGridPublisherClient client = new EventGridPublisherClient(topicEndpointUri, new AzureKeyCredential(_eventGridConfig.TopicKey));
        client.SendEvents(events);
    }
}
