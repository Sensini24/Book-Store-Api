using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace BookStoreApi.Sevices;

public class PaypalService
{
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _mode;

    public PaypalService(IConfiguration configuration)
    {
        _clientId = configuration["Paypal:ClientId"];
        _clientSecret = configuration["Paypal:ClientSecret"];
        _mode = configuration["Paypal:Mode"];
    }

    private readonly string _baseUrl = "https://api-m.sandbox.paypal.com/";
    private async Task<string> GetAccessToken()
    {
        using var client = new HttpClient();
        var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_clientId}:{_clientSecret}"));
        
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
        var content = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");
        var response = await client.PostAsync($"{_baseUrl}v1/oauth2/token", content);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
        
            return JsonDocument.Parse(result).RootElement.GetProperty("access_token").GetString();
        }

        string message = "No se pudo obtener el token";
        return message;
    }
    
    public async Task<(string orderId, string approveUrl)> CreateOrder(decimal amount, string currency)
    {
        var accessToken = await GetAccessToken();
        using var client = new HttpClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        
        var body = new
        {
            intent = "CAPTURE",
            purchase_units = new[]
            {
                new
                {
                    amount = new
                    {
                        currency_code = currency,
                        value = amount.ToString("F2", CultureInfo.InvariantCulture)
                    }
                }
            },
            application_context = new
            {
                return_url = "http://localhost:5173/books",
                cancel_url = "https://tu-sitio.com/cancel"
            }
        };

        var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        var response = await client.PostAsync($"{_baseUrl}v2/checkout/orders", content);
        var responseContent = await response.Content.ReadAsStringAsync();
        
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Error en la solicitud a PayPal: {response.StatusCode} - {responseContent}");
        }

        var json = JsonDocument.Parse(responseContent);
        string orderId = json.RootElement.GetProperty("id").GetString(); // Obtener Order ID
        string approveUrl = json.RootElement.GetProperty("links").EnumerateArray()
            .First(link => link.GetProperty("rel").GetString() == "approve")
            .GetProperty("href").GetString();

        return (orderId, approveUrl);
    }
    
    // public async Task<bool> CaptureOrder(string orderId)
    // {
    //     var accessToken = await GetAccessToken();
    //     using var client = new HttpClient();
    //
    //     client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    //     client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    //
    //     var response = await client.PostAsync($"{_baseUrl}v2/checkout/orders/{orderId}/capture", null);
    //     return response.IsSuccessStatusCode;
    // }
    
    public async Task<string> CaptureOrder(string orderId)
    {
        var accessToken = await GetAccessToken();
        using var client = new HttpClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // var response = await client.PostAsync($"{_baseUrl}v2/checkout/orders/{orderId}/capture", null);
        var response = await client.PostAsync($"{_baseUrl}v2/checkout/orders/{orderId}/capture",
            new StringContent("{}", Encoding.UTF8, "application/json")
        );
        
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Error al capturar el pago: {response.StatusCode} - {responseContent}");
        }

        return responseContent;
    }



}