namespace Shop.Functions.Dto;

public record Order(
    int Id, 
    string BuyerId, 
    DateTimeOffset OrderDate, 
    Address ShipToAddress, 
    IReadOnlyCollection<OrderItem> OrderItems);
