using Contracts;
using MassTransit;

namespace PaymentConsumer;

public class StockReservedConsumer : IConsumer<StockReserved>
{
    private readonly ILogger<StockReservedConsumer> _logger;

    public StockReservedConsumer(ILogger<StockReservedConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<StockReserved> context)
    {
        var msg = context.Message;

        _logger.LogInformation(
            "PAYMENT received StockReserved — OrderId: {OrderId}, CorrelationId: {CorrelationId}",
            msg.OrderId, msg.CorrelationId);

        // Decide if payment succeeds. For the demo, any order whose Id
        // we can't process fails — but for now, simple rule: succeed,
        // unless we deliberately simulate a decline (added later).
        var paymentSucceeded = true;

        if (paymentSucceeded)
        {
            _logger.LogInformation("PAYMENT succeeded for OrderId: {OrderId}", msg.OrderId);

            await context.Publish(new PaymentSucceeded
            {
                OrderId = msg.OrderId,
                CorrelationId = msg.CorrelationId
            });
        }
        else
        {
            _logger.LogWarning("PAYMENT failed for OrderId: {OrderId}", msg.OrderId);

            await context.Publish(new PaymentFailed
            {
                OrderId = msg.OrderId,
                CorrelationId = msg.CorrelationId,
                Reason = "Card declined"
            });
        }
    }
}