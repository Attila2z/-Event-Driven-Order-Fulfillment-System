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

        // Demo toggle: set SHIPPING_FAILS=true to simulate the carrier API being down.
        var shippingFails = Environment.GetEnvironmentVariable("SHIPPING_FAILS") == "true";
        if (shippingFails)
        {
            _logger.LogError("SHIPPING failing on purpose for OrderId: {OrderId}", msg.OrderId);
            throw new Exception("Carrier API unavailable (simulated)");
        }

        _logger.LogInformation(
            "SHIPPING received OrderConfirmed — OrderId: {OrderId}, CorrelationId: {CorrelationId}. Generating shipping label.",
            msg.OrderId, msg.CorrelationId);

        await Task.CompletedTask;
    }
}