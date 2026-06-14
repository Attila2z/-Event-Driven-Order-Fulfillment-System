using Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using StockConsumer;
using Testcontainers.PostgreSql;

namespace StockConsumer.Tests;

// Integration tests — spin up a real Postgres via Testcontainers to verify
// that the unique constraint on ProcessedMessages actually rejects duplicates.
// These are slower than unit tests; run them separately if needed.
[Collection("Postgres")]
public class IdempotencyIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithDatabase("stockdb")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public async Task InitializeAsync() => await _postgres.StartAsync();
    public async Task DisposeAsync() => await _postgres.DisposeAsync();

    private StockDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<StockDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;
        var db = new StockDbContext(options);
        db.Database.EnsureCreated();
        return db;
    }

    [Fact]
    public async Task UniqueConstraint_RealPostgres_RejectsDuplicateMessageId()
    {
        // Arrange
        await using var db1 = CreateDb();
        var messageId = Guid.NewGuid();

        db1.ProcessedMessages.Add(new ProcessedMessage
        {
            MessageId = messageId,
            ProcessedAt = DateTime.UtcNow
        });
        await db1.SaveChangesAsync();

        // Act — try to insert same MessageId in a new context (simulates a redelivered message)
        await using var db2 = CreateDb();
        db2.ProcessedMessages.Add(new ProcessedMessage
        {
            MessageId = messageId,
            ProcessedAt = DateTime.UtcNow
        });

        // Assert — real Postgres enforces the unique constraint and throws
        await Assert.ThrowsAsync<DbUpdateException>(() => db2.SaveChangesAsync());
    }

    [Fact]
    public async Task Consumer_DeliveredTwiceWithSameMessageId_OnlyProcessesOnce()
    {
        // Arrange
        await using var db = CreateDb();
        var logger = new Mock<ILogger<OrderPlacedConsumer>>();
        var consumer = new OrderPlacedConsumer(logger.Object, db);

        var messageId = Guid.NewGuid();
        var msg = new OrderPlaced
        {
            OrderId = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid(),
            CustomerId = "cust-1",
            Items = [new OrderItem { ProductId = "prod-1", Quantity = 1 }],
            TotalAmount = 50m,
            PlacedAt = DateTime.UtcNow
        };

        var ctx = new Mock<ConsumeContext<OrderPlaced>>();
        ctx.Setup(x => x.Message).Returns(msg);
        ctx.Setup(x => x.MessageId).Returns((Guid?)messageId);
        ctx.Setup(x => x.Publish(It.IsAny<StockReserved>(), It.IsAny<CancellationToken>()))
           .Returns(Task.CompletedTask);
        ctx.Setup(x => x.Publish(It.IsAny<StockUnavailable>(), It.IsAny<CancellationToken>()))
           .Returns(Task.CompletedTask);

        // Act — call Consume twice with identical MessageId
        await consumer.Consume(ctx.Object);
        await consumer.Consume(ctx.Object);

        // Assert — StockReserved published exactly once (second call was idempotency-rejected)
        ctx.Verify(
            x => x.Publish(It.IsAny<StockReserved>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

// Prevent Testcontainers from starting a new Postgres per test class
[CollectionDefinition("Postgres")]
public class PostgresCollection : ICollectionFixture<PostgreSqlContainer> { }
