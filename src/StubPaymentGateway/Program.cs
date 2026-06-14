var app = WebApplication.Create(args);

var paymentFails = Environment.GetEnvironmentVariable("PAYMENT_FAILS") == "true";

app.MapPost("/charge", () => paymentFails
    ? Results.StatusCode(402)
    : Results.Ok(new { status = "charged" }));

app.Run();
