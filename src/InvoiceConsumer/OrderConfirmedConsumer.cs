using Contracts;
using MassTransit;

namespace InvoiceConsumer;

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
            "INVOICE received OrderConfirmed — OrderId: {OrderId}, CorrelationId: {CorrelationId}. Generating invoice.",
            msg.OrderId, msg.CorrelationId);

        // Thin service: in production this would generate a PDF invoice and store it.
        // For the demo it records that it processed the event.

        await Task.CompletedTask;
    }
}