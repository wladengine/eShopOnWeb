using Newtonsoft.Json;

namespace Shop.Functions.Dto;
internal class OrderWithUniqueId
{
    [JsonProperty(PropertyName = "id")]
    public string UniqueId { get; set; }

    public int Id { get; set; }
    public string BuyerId { get; set; }
    public DateTimeOffset OrderDate { get; set; }
    public Address ShipToAddress { get; set; }
    public IReadOnlyCollection<OrderItem> OrderItems { get; set; }

    public OrderWithUniqueId() { } // For deserialization from CosmosDB

    public OrderWithUniqueId(Order baseOrder)
    {
        Id = baseOrder.Id;
        BuyerId = baseOrder.BuyerId;
        OrderDate = baseOrder.OrderDate;
        ShipToAddress = baseOrder.ShipToAddress;
        OrderItems = baseOrder.OrderItems;
        UniqueId = $"{Id}_{OrderDate.ToUnixTimeSeconds()}";
    }
}
