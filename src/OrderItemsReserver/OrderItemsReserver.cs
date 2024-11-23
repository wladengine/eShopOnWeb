using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Shop.Functions.Blob;
using Shop.Functions.Dto;

namespace Shop.Functions;

public class OrderItemsReserver
{
    private readonly ILogger _logger;
    private readonly OrdersBlobStorageRepository _blobStorageRepository;

    public OrderItemsReserver(ILoggerFactory loggerFactory, OrdersBlobStorageRepository blobStorageRepository)
    {
        _logger = loggerFactory.CreateLogger<OrderItemsReserver>();
        _blobStorageRepository = blobStorageRepository;
    }

    [Function("OrderItemsReserver")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "reserve-order-items")] HttpRequestData req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        var order = await JsonHelper.DeserializeRequestAsync<Order>(req);

        string blobName = await _blobStorageRepository.DumpOrderToBlobStorage(order);

        var response = req.CreateResponse(HttpStatusCode.OK);

        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

        await response.WriteStringAsync($"Order data was successfully written to {blobName}!");

        return response;
    }

    
}
