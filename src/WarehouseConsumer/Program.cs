using WarehouseConsumer;
using MassTransit;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderConfirmedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ReceiveEndpoint("warhouse-service", e =>
        {
            e.ConfigureConsumer<OrderConfirmedConsumer>(context);
        });
    });
});

var host = builder.Build();
host.Run();