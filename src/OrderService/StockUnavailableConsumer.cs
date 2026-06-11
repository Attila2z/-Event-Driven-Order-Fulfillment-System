using Contracts;
using MassTransit;

namespace OrderService;

public class StockUnavailableConsumer : IConsumer<StockUnavailable>
{
    private readonly ILogger<StockUnavailableConsumer> _logger;

    public StockUnavailableConsumer(ILogger<StockUnavailableConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<StockUnavailable> context)
    {
        var msg = context.Message;

        _logger.LogWarning(
            "ORDER received StockUnavailable — OrderId: {OrderId}, Reason: {Reason}. Cancelling order. Payment NOT invoked.",
            msg.OrderId, msg.Reason);

        // In a real system: update order status to "Cancelled" in Order's DB.

        await context.Publish(new OrderCancelled
        {
            OrderId = msg.OrderId,
            CorrelationId = msg.CorrelationId,
            Reason = msg.Reason
        });

        _logger.LogInformation("ORDER published OrderCancelled — OrderId: {OrderId}", msg.OrderId);
    }
}