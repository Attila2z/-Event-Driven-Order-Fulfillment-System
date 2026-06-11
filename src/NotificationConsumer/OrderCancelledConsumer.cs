using Contracts;
using MassTransit;

namespace NotificationConsumer;

public class OrderCancelledConsumer : IConsumer<OrderCancelled>
{
    private readonly ILogger<OrderCancelledConsumer> _logger;

    public OrderCancelledConsumer(ILogger<OrderCancelledConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCancelled> context)
    {
        var msg = context.Message;

        _logger.LogWarning(
            "NOTIFICATION received OrderCancelled — OrderId: {OrderId}, Reason: {Reason}. Sending cancellation email.",
            msg.OrderId, msg.Reason);

        await Task.CompletedTask;
    }
}