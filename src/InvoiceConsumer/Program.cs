using InvoiceConsumer;
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

        cfg.ReceiveEndpoint("invoice-service", e =>
        {
            e.ConfigureConsumer<OrderConfirmedConsumer>(context);
        });
    });
});

var host = builder.Build();
host.Run();