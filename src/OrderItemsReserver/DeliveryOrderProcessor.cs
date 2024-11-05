using System.Net;
using Azure.Identity;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shop.Functions.Dto;

namespace Shop.Functions;

public class DeliveryOrderProcessor
{
    private readonly ILogger<DeliveryOrderProcessor> _logger;
    private readonly DeliveryOrderProcessorStorageConfig _storageConfig;

    public DeliveryOrderProcessor(
        ILogger<DeliveryOrderProcessor> logger, 
        IOptions<DeliveryOrderProcessorStorageConfig> storageConfig)
    {
        _logger = logger;
        _storageConfig = storageConfig.Value;
    }

    [Function("DeliveryOrderProcessor")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "process-order-delivery")] HttpRequestData req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        var order = await JsonHelper.DeserializeRequestAsync<Order>(req);

        var (IsSuccess, DTUs) = await DumpOrderToCosmosDB(new OrderWithUniqueId(order));

        HttpResponseData response = req.CreateResponse(IsSuccess ? HttpStatusCode.OK : HttpStatusCode.InternalServerError);
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
        if (IsSuccess)
        {
            await response.WriteStringAsync(
                $"Order data was successfully written to {_storageConfig.DatabaseName}.{_storageConfig.ContainerName} and took {DTUs} DTUs!");
        }
        else
        {
            await response.WriteStringAsync(
                $"Unable to write data to {_storageConfig.DatabaseName}.{_storageConfig.ContainerName}.");
        }

        return response;
    }

    private async Task<(bool IsSuccess, double DTUs)> DumpOrderToCosmosDB(OrderWithUniqueId order)
    {
        CosmosClient client = new CosmosClient(_storageConfig.AccountEndpoint, new DefaultAzureCredential());
        
        Container container = client.GetContainer(_storageConfig.DatabaseName, _storageConfig.ContainerName);
        try
        {
            var response = await container.CreateItemAsync(order, new PartitionKey(order.BuyerId));
            return (response.StatusCode == HttpStatusCode.Created, response.RequestCharge);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error writing to CosmosDB");
            return (false, 0);
        }
    }
}
