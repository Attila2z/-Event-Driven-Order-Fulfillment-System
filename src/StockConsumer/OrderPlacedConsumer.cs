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
            "STOCK received OrderPlaced — OrderId: {OrderId}, CorrelationId: {CorrelationId}",
            order.OrderId, order.CorrelationId);

        // Decide if we can reserve. For the demo, any item with
        // ProductId "OUT-OF-STOCK" fails; everything else succeeds.
        var canReserve = order.Items.All(item => item.ProductId != "OUT-OF-STOCK");

        if (canReserve)
        {
            _logger.LogInformation("STOCK reserved for OrderId: {OrderId}", order.OrderId);

            await context.Publish(new StockReserved
            {
                OrderId = order.OrderId,
                CorrelationId = order.CorrelationId
            });
        }
        else
        {
            _logger.LogWarning("STOCK unavailable for OrderId: {OrderId}", order.OrderId);

            await context.Publish(new StockUnavailable
            {
                OrderId = order.OrderId,
                CorrelationId = order.CorrelationId,
                Reason = "One or more items are out of stock"
            });
        }
    }
}