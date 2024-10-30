namespace Shop.Functions.Dto;

public record OrderItem(int Id, CatalogItemOrdered ItemOrdered, decimal UnitPrice, int Units);
