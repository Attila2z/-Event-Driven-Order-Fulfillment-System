using Contracts;
using MassTransit;

namespace StockConsumer;

public class OrderPlacedConsumer : IConsumer<OrderPlaced>
{
    private readonly ILogger<OrderPlacedConsumer> _logger;

    public OrderPlacedConsumer(ILogger<OrderPlacedConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderPlaced> context)
    {
        var order = context.Message;

        _logger.LogInformation(
            "STOCK received OrderPlaced — OrderId: {OrderId}, CorrelationId: {CorrelationId}, Items: {Count}",
            order.OrderId, order.CorrelationId, order.Items.Count);

        await Task.CompletedTask;
    }
}