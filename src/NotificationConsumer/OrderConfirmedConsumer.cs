using Contracts;
using MassTransit;

namespace NotificationConsumer;

public class OrderConfirmedConsumer : IConsumer<OrderConfirmed>
{
    private readonly ILogger<OrderConfirmedConsumer> _logger;

    public OrderConfirmedConsumer(ILogger<OrderConfirmedConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderConfirmed> context)
    {
        var msg = context.Message;

        _logger.LogInformation(
            "NOTIFICATION received OrderConfirmed — OrderId: {OrderId}. Sending confirmation email.",
            msg.OrderId);

        await Task.CompletedTask;
    }
}