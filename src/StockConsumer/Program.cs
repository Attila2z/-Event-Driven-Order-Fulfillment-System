using Microsoft.EntityFrameworkCore;
using MassTransit;
using StockConsumer;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<StockDbContext>(options =>
    options.UseNpgsql("Host=localhost;Port=5432;Database=stockdb;Username=postgres;Password=postgres"));

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderPlacedConsumer>();
    x.AddConsumer<PaymentFailedConsumer>();

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

var host = builder.Build();

// Ensure the ProcessedMessages table exists (demo approach; production would use migrations)
using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<StockDbContext>();
    db.Database.EnsureCreated();
}

host.Run();