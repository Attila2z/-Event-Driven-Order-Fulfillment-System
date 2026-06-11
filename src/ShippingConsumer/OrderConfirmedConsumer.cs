using Contracts;
using MassTransit;

namespace ShippingConsumer;

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
            "SHIPPING received OrderConfirmed — OrderId: {OrderId}, CorrelationId: {CorrelationId}. Creating shipping task.",
            msg.OrderId, msg.CorrelationId);

        await Task.CompletedTask;
    }
}