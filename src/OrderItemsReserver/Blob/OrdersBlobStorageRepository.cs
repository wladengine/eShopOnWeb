using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using Shop.Functions.Configs;
using Shop.Functions.Dto;

namespace Shop.Functions.Blob;
public class OrdersBlobStorageRepository
{
    private readonly OrderItemsReserverStorageConfig _storageConfig;

    public OrdersBlobStorageRepository(IOptions<OrderItemsReserverStorageConfig> storageConfig)
    {
        _storageConfig = storageConfig.Value;
    }

    public async Task<string> DumpOrderToBlobStorage(Order order)
    {
        var orderData = new
        {
            OrderId = order.Id,
            Items = order.OrderItems.Select(oi => new OrderDetails()
            {
                ItemId = oi.ItemOrdered.CatalogItemId,
                Quantity = oi.Units,
                Price = oi.UnitPrice,
            }),
            Total = order.OrderItems.Sum(item => item.Units * item.UnitPrice),
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
