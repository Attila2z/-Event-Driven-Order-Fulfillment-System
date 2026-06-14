using MassTransit;
using PaymentConsumer;

var builder = Host.CreateApplicationBuilder(args);

var rabbitHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
var gatewayUrl = Environment.GetEnvironmentVariable("PAYMENT_GATEWAY_URL") ?? "http://localhost:9090";

builder.Services.AddSingleton(new PaymentConsumer.PaymentGatewayClient(
    new HttpClient { BaseAddress = new Uri(gatewayUrl) }));

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<StockReservedConsumer>();

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

var host = builder.Build();
host.Run();