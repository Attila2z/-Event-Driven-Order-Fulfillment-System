using Contracts;
using MassTransit;
using OrderService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// --- MassTransit setup ---
builder.Services.AddSingleton<OrderStore>();

var rabbitHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<PaymentSucceededConsumer>();
    x.AddConsumer<StockUnavailableConsumer>();
    x.AddConsumer<PaymentFailedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitHost, "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// --- Place order endpoint ---
app.MapPost("/orders", async (PlaceOrderRequest request, IPublishEndpoint publishEndpoint, OrderStore store) =>
{
    var orderId = Guid.NewGuid();
    var correlationId = Guid.NewGuid();

    var order = new OrderPlaced
    {
        OrderId = orderId,
        CorrelationId = correlationId,
        CustomerId = request.CustomerId,
        Items = request.Items,
        TotalAmount = request.TotalAmount,
        PlacedAt = DateTime.UtcNow
    };

    store.Save(order);
    await publishEndpoint.Publish(order);

    return Results.Created($"/orders/{orderId}", new { orderId, status = "Pending" });
});

// TEST ENDPOINT — publishes the same message twice with a fixed MessageId
// to demonstrate idempotency (duplicate delivery). Remove for production.
app.MapPost("/test/duplicate", async (IPublishEndpoint publishEndpoint, OrderStore store) =>
{
    var orderId = Guid.NewGuid();
    var correlationId = Guid.NewGuid();
    var fixedMessageId = Guid.NewGuid();

    var order = new OrderPlaced
    {
        OrderId = orderId,
        CorrelationId = correlationId,
        CustomerId = "dup-test",
        Items = new List<OrderItem> { new() { ProductId = "prod-1", Quantity = 1 } },
        TotalAmount = 10m,
        PlacedAt = DateTime.UtcNow
    };

    store.Save(order);

    // Publish twice with the SAME MessageId to demonstrate idempotency
    await publishEndpoint.Publish(order, ctx => ctx.MessageId = fixedMessageId);
    await publishEndpoint.Publish(order, ctx => ctx.MessageId = fixedMessageId);

    return Results.Ok(new { orderId, fixedMessageId, note = "published twice with same MessageId" });
});

app.Run();

// Request body shape the customer sends
record PlaceOrderRequest(string CustomerId, List<OrderItem> Items, decimal TotalAmount);