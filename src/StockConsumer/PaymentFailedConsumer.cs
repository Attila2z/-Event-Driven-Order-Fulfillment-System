using Contracts;
using MassTransit;

namespace StockConsumer;

public class PaymentFailedConsumer : IConsumer<PaymentFailed>
{
    private readonly ILogger<PaymentFailedConsumer> _logger;

    public PaymentFailedConsumer(ILogger<PaymentFailedConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentFailed> context)
    {
        var msg = context.Message;

        _logger.LogWarning(
            "STOCK received PaymentFailed — OrderId: {OrderId}. Releasing reservation (compensation).",
            msg.OrderId);

        // In a real system: release the held inventory back to available stock.

        await context.Publish(new StockReleased
        {
            OrderId = msg.OrderId,
            CorrelationId = msg.CorrelationId
        });

        _logger.LogInformation("STOCK released reservation for OrderId: {OrderId}", msg.OrderId);
    }
}