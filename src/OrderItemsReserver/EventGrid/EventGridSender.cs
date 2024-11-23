using Azure.Identity;
using Microsoft.Azure.EventGrid;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Extensions.Options;
using Shop.Functions.Configs;

namespace Shop.Functions.EventGrid;
public class EventGridSender
{
    private readonly EventGridConfig _eventGridConfig;

    public EventGridSender(IOptions<EventGridConfig> eventGridConfig) => _eventGridConfig = eventGridConfig.Value;

    // see the https://github.com/Azure-Samples/Serverless-Eventing-Platform-for-Microservices/blob/master/shared/src/ContentReactor.Shared/EventGridPublisherService.cs#L16
    public Task PostEventGridEventAsync<T>(string type, string subject, T payload)
    {
        // get the connection details for the Event Grid topic
        var topicEndpointUri = new Uri(_eventGridConfig.TopicEndpoint);
        var topicEndpointHostname = topicEndpointUri.Host;
        var topicCredentials = new TopicCredentials(_eventGridConfig.TopicKey);

        // prepare the events for submission to Event Grid
        var events = new List<EventGridEvent>
        {
            new() {
                Id = Guid.NewGuid().ToString(),
                EventType = type,
                Subject = subject,
                EventTime = DateTime.UtcNow,
                Data = payload,
                DataVersion = "1"
            }
        };

        // publish the events
        var client = new EventGridClient(topicCredentials);
        return client.PublishEventsWithHttpMessagesAsync(topicEndpointHostname, events);
    }
}
