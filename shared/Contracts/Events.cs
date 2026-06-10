// shared/Contracts/OrderPlaced.cs
namespace Contracts;

public record OrderItem
{
    public string ProductId { get; init; }
    public int Quantity { get; init; }
}

public record OrderPlaced

{
    public Guid OrderId { get; init; }
    public Guid CorrelationId { get; init; }
    public string CustomerId { get; init; }
    public List<OrderItem> Items { get; init; }
    public decimal TotalAmount { get; init; }
    public DateTime PlacedAt { get; init; }
}

public record PaymentFailed
{
    public Guid OrderId { get; init; }
    public Guid CorrelationId { get; init; }
    public string Reason { get; init; }
}

public record StockReserved
{
    public Guid OrderId { get; init; }
    public Guid CorrelationId { get; init; }
}

public record StockReleased
{
    public Guid OrderId { get; init; }
}

public record StockUnavailable
{
    public Guid OrderId { get; init; }
    public Guid CorrelationId { get; init; }
    public string Reason { get; init; }
}

public record PaymentSucceeded
{
    public Guid OrderId { get; init; }
    public Guid CorrelationId { get; init; }
}

public record OrderConfirmed 
{
    public Guid OrderId { get; init; }
    public Guid CorrelationId { get; init; }
    public List<OrderItem> Items { get; init; }
    public DateTime ConfirmedAt { get; init; }
}

public record OrderCancelled
{
    public Guid OrderId { get; init; }
    public Guid CorrelationId { get; init; }
    public string Reason { get; init; }
}