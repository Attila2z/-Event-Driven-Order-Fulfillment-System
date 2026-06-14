using Contracts;
using MassTransit;

namespace PaymentConsumer;

public class StockReservedConsumer : IConsumer<StockReserved>
{
    private readonly ILogger<StockReservedConsumer> _logger;
    private readonly PaymentGatewayClient _gateway;

    public StockReservedConsumer(ILogger<StockReservedConsumer> logger, PaymentGatewayClient gateway)
    {
        _logger = logger;
        _gateway = gateway;
    }

    public async Task Consume(ConsumeContext<StockReserved> context)
    {
        var msg = context.Message;

        _logger.LogInformation(
            "PAYMENT received StockReserved — OrderId: {OrderId}, CorrelationId: {CorrelationId}",
            msg.OrderId, msg.CorrelationId);

        var succeeded = await _gateway.ChargeAsync(msg.OrderId, 0m);

        if (succeeded)
        {
            _logger.LogInformation("PAYMENT succeeded for OrderId: {OrderId}", msg.OrderId);
            await context.Publish(new PaymentSucceeded
            {
                OrderId = msg.OrderId,
                CorrelationId = msg.CorrelationId
            });
        }
        else
        {
            _logger.LogWarning("PAYMENT failed for OrderId: {OrderId}", msg.OrderId);
            await context.Publish(new PaymentFailed
            {
                OrderId = msg.OrderId,
                CorrelationId = msg.CorrelationId,
                Reason = "Card declined"
            });
        }
    }
}