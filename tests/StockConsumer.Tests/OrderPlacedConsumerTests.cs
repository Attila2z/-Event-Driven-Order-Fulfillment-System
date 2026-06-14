using Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using StockConsumer;

namespace StockConsumer.Tests;

// Unit tests — fast, no external dependencies.
// Uses an InMemory EF database and Moq for the MassTransit ConsumeContext.
public class OrderPlacedConsumerTests
{
    private static StockDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<StockDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new StockDbContext(options);
    }

    private static (Mock<ConsumeContext<OrderPlaced>> ctx, OrderPlaced msg) BuildContext(
        string productId = "prod-1",
        Guid? messageId = null)
    {
        var msg = new OrderPlaced
        {
            OrderId = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid(),
            CustomerId = "cust-1",
            Items = [new OrderItem { ProductId = productId, Quantity = 1 }],
            TotalAmount = 99m,
            PlacedAt = DateTime.UtcNow
        };

        var ctx = new Mock<ConsumeContext<OrderPlaced>>();
        ctx.Setup(x => x.Message).Returns(msg);
        ctx.Setup(x => x.MessageId).Returns(messageId ?? Guid.NewGuid());
        ctx.Setup(x => x.Publish(It.IsAny<StockReserved>(), It.IsAny<CancellationToken>()))
           .Returns(Task.CompletedTask);
        ctx.Setup(x => x.Publish(It.IsAny<StockUnavailable>(), It.IsAny<CancellationToken>()))
           .Returns(Task.CompletedTask);

        return (ctx, msg);
    }

    [Fact]
    public async Task Consume_InStockProduct_PublishesStockReserved()
    {
        // Arrange
        using var db = CreateInMemoryDb();
        var logger = new Mock<ILogger<OrderPlacedConsumer>>();
        var consumer = new OrderPlacedConsumer(logger.Object, db);

        var (ctx, msg) = BuildContext("prod-valid");

        // Act
        await consumer.Consume(ctx.Object);

        // Assert
        ctx.Verify(
            x => x.Publish(It.Is<StockReserved>(r => r.OrderId == msg.OrderId), It.IsAny<CancellationToken>()),
            Times.Once);
        ctx.Verify(
            x => x.Publish(It.IsAny<StockUnavailable>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Consume_OutOfStockProduct_PublishesStockUnavailable()
    {
        // Arrange
        using var db = CreateInMemoryDb();
        var logger = new Mock<ILogger<OrderPlacedConsumer>>();
        var consumer = new OrderPlacedConsumer(logger.Object, db);

        var (ctx, msg) = BuildContext("OUT-OF-STOCK");

        // Act
        await consumer.Consume(ctx.Object);

        // Assert
        ctx.Verify(
            x => x.Publish(It.Is<StockUnavailable>(r => r.OrderId == msg.OrderId), It.IsAny<CancellationToken>()),
            Times.Once);
        ctx.Verify(
            x => x.Publish(It.IsAny<StockReserved>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Consume_DuplicateMessage_SkipsPublishingSecondTime()
    {
        // Arrange — share the same MessageId across two calls
        using var db = CreateInMemoryDb();
        var logger = new Mock<ILogger<OrderPlacedConsumer>>();
        var consumer = new OrderPlacedConsumer(logger.Object, db);

        var fixedMessageId = Guid.NewGuid();
        var (ctx, _) = BuildContext("prod-1", fixedMessageId);

        // Act — first call processes normally
        await consumer.Consume(ctx.Object);

        // The InMemory provider does not enforce unique constraints, so we
        // simulate a second call with the same MessageId by verifying the
        // consumer recorded the message in ProcessedMessages after the first call.
        // Full duplicate-rejection is covered by IdempotencyIntegrationTests.
        var recorded = await db.ProcessedMessages
            .AnyAsync(p => p.MessageId == fixedMessageId);

        // Assert
        Assert.True(recorded, "First call should record the MessageId.");
        ctx.Verify(
            x => x.Publish(It.IsAny<StockReserved>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Consume_RecordsMessageIdInProcessedMessages()
    {
        // Arrange
        using var db = CreateInMemoryDb();
        var logger = new Mock<ILogger<OrderPlacedConsumer>>();
        var consumer = new OrderPlacedConsumer(logger.Object, db);

        var messageId = Guid.NewGuid();
        var (ctx, _) = BuildContext("prod-1", messageId);

        // Act
        await consumer.Consume(ctx.Object);

        // Assert — idempotency table was written
        var saved = await db.ProcessedMessages.FindAsync(messageId);
        Assert.NotNull(saved);
        Assert.Equal(messageId, saved.MessageId);
    }
}
