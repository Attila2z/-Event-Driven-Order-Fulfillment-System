using PaymentConsumer;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace StockConsumer.Tests;

// Tests that verify PaymentGatewayClient behaves correctly against different
// HTTP responses from the payment gateway — without needing a real gateway running.
// WireMock.Net starts a real HTTP server on a random port and lets us define
// exactly what it should return.
public class PaymentGatewayClientTests : IDisposable
{
    private readonly WireMockServer _server = WireMockServer.Start();
    private readonly PaymentGatewayClient _client;

    public PaymentGatewayClientTests()
    {
        _client = new PaymentGatewayClient(
            new HttpClient { BaseAddress = new Uri(_server.Url!) });
    }

    public void Dispose() => _server.Stop();

    [Fact]
    public async Task ChargeAsync_GatewayReturns200_ReturnsTrue()
    {
        _server
            .Given(Request.Create().WithPath("/charge").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200));

        var result = await _client.ChargeAsync(Guid.NewGuid(), 49.99m);

        Assert.True(result);
    }

    [Fact]
    public async Task ChargeAsync_GatewayReturns402_ReturnsFalse()
    {
        // 402 Payment Required = card declined
        _server
            .Given(Request.Create().WithPath("/charge").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(402));

        var result = await _client.ChargeAsync(Guid.NewGuid(), 49.99m);

        Assert.False(result);
    }

    [Fact]
    public async Task ChargeAsync_SendsOrderIdAndAmountInBody()
    {
        _server
            .Given(Request.Create().WithPath("/charge").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200));

        var orderId = Guid.NewGuid();
        await _client.ChargeAsync(orderId, 99.00m);

        var body = _server.LogEntries.Single().RequestMessage.Body;
        Assert.Contains(orderId.ToString(), body);
        Assert.Contains("99", body);
    }
}
