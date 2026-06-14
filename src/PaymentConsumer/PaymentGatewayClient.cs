using System.Net.Http.Json;

namespace PaymentConsumer;

public class PaymentGatewayClient
{
    private readonly HttpClient _http;

    public PaymentGatewayClient(HttpClient http) => _http = http;

    public async Task<bool> ChargeAsync(Guid orderId, decimal amount)
    {
        var response = await _http.PostAsJsonAsync("/charge", new { orderId, amount });
        return response.IsSuccessStatusCode;
    }
}
