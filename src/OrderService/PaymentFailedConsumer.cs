using Contracts;
using MassTransit;

namespace OrderService;

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
            "ORDER received PaymentFailed — OrderId: {OrderId}, Reason: {Reason}. Cancelling order.",
            msg.OrderId, msg.Reason);

        await context.Publish(new OrderCancelled
        {
            OrderId = msg.OrderId,
            CorrelationId = msg.CorrelationId,
            Reason = msg.Reason
        });
    }
}