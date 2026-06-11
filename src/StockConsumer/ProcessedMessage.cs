namespace StockConsumer;

// One row per message this service has processed.
// EF maps this class to a "ProcessedMessages" table.
public class ProcessedMessage
{
    public Guid MessageId { get; set; }       // the unique key — MassTransit's message id
    public DateTime ProcessedAt { get; set; }  // when we handled it (for debugging/audit)
}