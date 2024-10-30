using System.Net;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shop.Functions.Dto;

namespace Shop.Functions;

public class OrderItemsReserver
{
    private readonly ILogger _logger;
    private readonly OrderItemsReserverStorageConfig _storageConfig;

    public OrderItemsReserver(ILoggerFactory loggerFactory, IOptions<OrderItemsReserverStorageConfig> storageConfig)
    {
        _logger = loggerFactory.CreateLogger<OrderItemsReserver>();
        _storageConfig = storageConfig.Value;
    }

    [Function("OrderItemsReserver")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "reserve-order-items")] HttpRequestData req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        var order = await JsonHelper.DeserializeRequestAsync<Order>(req);

        string blobName = await DumpOrderToBlobStorage(order);

        var response = req.CreateResponse(HttpStatusCode.OK);

        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

        await response.WriteStringAsync($"Order data was successfully written to {blobName}!");

        return response;
    }

    private async Task<string> DumpOrderToBlobStorage(Order order)
    {
        var orderData = new
        {
            OrderId = order.Id,
            Items = order.OrderItems.Select(oi => new OrderDetails()
            {
                ItemId = oi.ItemOrdered.CatalogItemId,
                Quantity = oi.Units,
                Price = oi.UnitPrice,
            })
        };

        BlobServiceClient blobServiceClient = new BlobServiceClient(_storageConfig.ConnectionString);

        // Get the container (folder) the file will be saved in
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_storageConfig.FileContainerName);

        // Get the Blob Client used to interact with (including create) the blob
        string blobName = $"{order.Id}_{order.OrderDate.ToUnixTimeSeconds()}.json";

        BlobClient blobClient = containerClient.GetBlobClient(blobName);
        
        blobClient.DeleteIfExists();

        // Upload the blob
        using var fileStream = new MemoryStream();
        await JsonHelper.SerializeAsync(fileStream, orderData);
        fileStream.Position = 0;
        await blobClient.UploadAsync(fileStream);

        return blobName;
    }
}
