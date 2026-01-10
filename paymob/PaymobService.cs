using paymob.paymob;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace paymob.paymob
{
    public class PaymobService : IPaymobService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public PaymobService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        // 1. Matches Interface: Task<string>
        public async Task<string> ProcessPaymentAsync(decimal amount, string paymentMethod)
        {
            string secretKey = _configuration["Paymob:SecretKey"] ?? throw new Exception("Secret Key missing");
            string publicKey = _configuration["Paymob:PublicKey"] ?? throw new Exception("Public Key missing");

            string testReference = "TEST_" + DateTime.Now.Ticks;

            var payload = new
            {
                amount = (int)(amount * 100),
                currency = "EGP",
                payment_methods = new[] { int.Parse(DetermineIntegrationId(paymentMethod)) },
                billing_data = new
                {
                    first_name = "Test",
                    last_name = "User",
                    email = "test@example.com",
                    phone_number = "01012345678",
                    country = "EGY",
                    city = "Cairo",
                    apartment = "NA",
                    floor = "NA",
                    street = "NA",
                    building = "NA",
                    state = "NA"
                },
                special_reference = testReference
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://accept.paymob.com/v1/intention/");
            request.Headers.Authorization = new AuthenticationHeaderValue("Token", secretKey);
            request.Content = JsonContent.Create(payload);

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode) throw new Exception(content);

            using var doc = JsonDocument.Parse(content);
            var clientSecret = doc.RootElement.GetProperty("client_secret").GetString();

            return $"https://accept.paymob.com/unifiedcheckout/?publicKey={publicKey}&clientSecret={clientSecret}";
        }

        // 2. Matches Interface: string
        public string ComputeHmacSHA512(string data, string secret)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secret);
            using var hmac = new HMACSHA512(keyBytes);
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        // Helper Method (Not in Interface because it is private)
        private string DetermineIntegrationId(string paymentMethod)
        {
            return paymentMethod.ToLower() switch
            {
                "card" => _configuration["Paymob:CardIntegrationId"] ?? "0",
                "wallet" => _configuration["Paymob:MobileIntegrationId"] ?? "0",
                _ => throw new Exception("Invalid Payment Method")
            };
        }
    }


}
#region
//public class PaymobService : IPaymobService
//{
//    private readonly IConfiguration _configuration;

//    public PaymobService(IConfiguration configuration)
//    {
//        _configuration = configuration;
//    }


//    public async Task<string> ProcessPaymentAsync(string paymentMethod, decimal amount)
//    {

//        // Create HTTP client for direct API calls to Paymob
//        var httpClient = new HttpClient();

//        // Get API key from configuration
//        string apiKey = _configuration["Paymob:APIKey"] ??
//            throw new ArgumentException("Paymob API key not configured");

//        string secretKey = _configuration["Paymob:SecretKey"] ??
//            throw new ArgumentException("Paymob secret key not configured");

//        string publicKey = _configuration["Paymob:PublicKey"] ??
//            throw new ArgumentException("Paymob public key not configured");

//        // Generate a special reference for this transaction

//        int specialReference = RandomNumberGenerator.GetInt32(1000000, 9999999);


//        var amountCents = (int)(amount * 100);



//        // Get wallet integration ID
//        var integrationId = int.Parse(DetermineIntegrationId(paymentMethod));

//        // Prepare intention request payload
//        var payload = new
//        {
//            amount = amountCents,
//            currency = "EGP",
//            payment_methods = new[] { integrationId },
//            items = new[]
//            {
//                    new
//                    {
//                        name = $"Enrollment #{specialReference-145}",
//                        amount = amountCents,

//                        quantity = 1
//                    }
//                },


//            special_reference = specialReference,
//            expiration = 3600, // 1 hour expiration
//            merchant_order_id = specialReference.ToString()
//        };

//        // Create HTTP request for Paymob's intention API
//        var requestMessage = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, "https://accept.paymob.com/v1/intention/");
//        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Token", secretKey);
//        requestMessage.Content = JsonContent.Create(payload);

//        // Send the request and process response
//        var response = await httpClient.SendAsync(requestMessage);
//        var responseContent = await response.Content.ReadAsStringAsync();

//        if (!response.IsSuccessStatusCode)
//        {
//            throw new Exception($"Paymob Intention API call failed with status {response.StatusCode}: {responseContent}");
//        }

//        // Parse the response to get client_secret
//        var resultJson = JsonDocument.Parse(responseContent);
//        var clientSecret = resultJson.RootElement.GetProperty("client_secret").GetString();

//        // Create payment record




//        // Generate payment URL for the unified checkout
//        string redirectUrl = $"https://accept.paymob.com/unifiedcheckout/?publicKey={publicKey}&clientSecret={clientSecret}";

//        return (redirectUrl);
//    }

//    private string DetermineIntegrationId(string paymentMethod)
//    {
//        return paymentMethod?.ToLower() switch
//        {
//            "card" => _configuration["Paymob:CardIntegrationId"] ?? throw new ArgumentException("Card integration ID not configured"),
//            "wallet" => _configuration["Paymob:MobileIntegrationId"] ?? throw new ArgumentException("Wallet integration ID not configured"),
//            _ => throw new ArgumentException($"Invalid payment method: {paymentMethod}")
//        };
//    }


//    public string ComputeHmacSHA512(string data, string secret)
//    {
//        var keyBytes = Encoding.UTF8.GetBytes(secret);
//        var dataBytes = Encoding.UTF8.GetBytes(data);

//        using (var hmac = new HMACSHA512(keyBytes))
//        {
//            var hash = hmac.ComputeHash(dataBytes);
//            return BitConverter.ToString(hash).Replace("-", "").ToLower();
//        }
//    }
//}
#endregion
#region

#endregion