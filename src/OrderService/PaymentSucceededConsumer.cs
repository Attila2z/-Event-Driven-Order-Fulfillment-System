using Contracts;
using MassTransit;

namespace OrderService;

public class PaymentSucceededConsumer : IConsumer<PaymentSucceeded>
{
    private readonly ILogger<PaymentSucceededConsumer> _logger;
    private readonly OrderStore _store;

    public PaymentSucceededConsumer(ILogger<PaymentSucceededConsumer> logger, OrderStore store)
    {
        _logger = logger;
        _store = store;
    }

    public async Task Consume(ConsumeContext<PaymentSucceeded> context)
    {
        var msg = context.Message;

        _logger.LogInformation(
            "ORDER received PaymentSucceeded — OrderId: {OrderId}, marking Confirmed",
            msg.OrderId);

        var original = _store.Get(msg.OrderId);

        await context.Publish(new OrderConfirmed
        {
            OrderId = msg.OrderId,
            CorrelationId = msg.CorrelationId,
            Items = original?.Items ?? [],
            ConfirmedAt = DateTime.UtcNow
        });

        _logger.LogInformation("ORDER published OrderConfirmed — OrderId: {OrderId}", msg.OrderId);
    }
}