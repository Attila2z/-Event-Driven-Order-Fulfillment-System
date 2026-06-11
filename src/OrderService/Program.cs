using Contracts;
using MassTransit;
using OrderService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// --- MassTransit setup ---
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<PaymentSucceededConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
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
app.MapPost("/orders", async (PlaceOrderRequest request, IPublishEndpoint publishEndpoint) =>
{
    var orderId = Guid.NewGuid();
    var correlationId = Guid.NewGuid();

    // In a real system we'd persist the order as "Pending" here first.

    await publishEndpoint.Publish(new OrderPlaced
    {
        OrderId = orderId,
        CorrelationId = correlationId,
        CustomerId = request.CustomerId,
        Items = request.Items,
        TotalAmount = request.TotalAmount,
        PlacedAt = DateTime.UtcNow
    });

    return Results.Created($"/orders/{orderId}", new { orderId, status = "Pending" });
});

app.Run();

// Request body shape the customer sends
record PlaceOrderRequest(string CustomerId, List<OrderItem> Items, decimal TotalAmount);