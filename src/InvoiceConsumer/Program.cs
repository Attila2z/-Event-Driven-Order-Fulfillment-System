using InvoiceConsumer;
using MassTransit;

var builder = Host.CreateApplicationBuilder(args);

var rabbitHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderConfirmedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitHost, "/", h =>
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