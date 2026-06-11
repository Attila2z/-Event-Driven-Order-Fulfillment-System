using Contracts;
using MassTransit;

namespace WarehouseConsumer;

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
            "WAREHOUSE received OrderConfirmed — OrderId: {OrderId}, CorrelationId: {CorrelationId}. Creating picking task.",
            msg.OrderId, msg.CorrelationId);

        await Task.CompletedTask;
    }
}