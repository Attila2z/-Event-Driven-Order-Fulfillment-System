using Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace StockConsumer;

public class OrderPlacedConsumer : IConsumer<OrderPlaced>
{
    private readonly ILogger<OrderPlacedConsumer> _logger;
    private readonly StockDbContext _db;

    public OrderPlacedConsumer(ILogger<OrderPlacedConsumer> logger, StockDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    public async Task Consume(ConsumeContext<OrderPlaced> context)
    {
        var order = context.Message;
        var messageId = context.MessageId ?? Guid.Empty;

        // --- IDEMPOTENCY: atomic via unique constraint ---
        // Try to record this MessageId. If the DB rejects it as a duplicate
        // (unique constraint on MessageId), we've already processed it — skip.
        _db.ProcessedMessages.Add(new ProcessedMessage
        {
            MessageId = messageId,
            ProcessedAt = DateTime.UtcNow
        });

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Insert failed because MessageId already exists → duplicate delivery.
            _logger.LogWarning(
                "STOCK skipping DUPLICATE message — MessageId: {MessageId}, OrderId: {OrderId}",
                messageId, order.OrderId);
            return; // ack and discard, do not process twice
        }
        // --- END IDEMPOTENCY ---

        _logger.LogInformation(
            "STOCK received OrderPlaced — OrderId: {OrderId}, CorrelationId: {CorrelationId}",
            order.OrderId, order.CorrelationId);

        var canReserve = order.Items.All(item => item.ProductId != "OUT-OF-STOCK");

        if (canReserve)
        {
            _logger.LogInformation("STOCK reserved for OrderId: {OrderId}", order.OrderId);
            await context.Publish(new StockReserved
            {
                OrderId = order.OrderId,
                CorrelationId = order.CorrelationId
            });
        }
        else
        {
            _logger.LogWarning("STOCK unavailable for OrderId: {OrderId}", order.OrderId);
            await context.Publish(new StockUnavailable
            {
                OrderId = order.OrderId,
                CorrelationId = order.CorrelationId,
                Reason = "One or more items are out of stock"
            });
        }
    }
}