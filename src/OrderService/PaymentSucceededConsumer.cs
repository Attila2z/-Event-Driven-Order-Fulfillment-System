using Contracts;
using MassTransit;

namespace OrderService;

public class PaymentSucceededConsumer : IConsumer<PaymentSucceeded>
{
    private readonly ILogger<PaymentSucceededConsumer> _logger;

    public PaymentSucceededConsumer(ILogger<PaymentSucceededConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentSucceeded> context)
    {
        var msg = context.Message;

        _logger.LogInformation(
            "ORDER received PaymentSucceeded — OrderId: {OrderId}, marking Confirmed",
            msg.OrderId);

        // In a real system: update the order row status to "Confirmed" here.

        await context.Publish(new OrderConfirmed
        {
            OrderId = msg.OrderId,
            CorrelationId = msg.CorrelationId,
            Items = new List<OrderItem>(), // see note below
            ConfirmedAt = DateTime.UtcNow
        });

        _logger.LogInformation("ORDER published OrderConfirmed — OrderId: {OrderId}", msg.OrderId);
    }
}