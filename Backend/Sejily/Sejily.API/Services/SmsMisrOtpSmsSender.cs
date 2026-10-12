
using System.Text.Json;

namespace Sejily.API.Services;

/*public class SmsMisrOtpSmsSender : IOtpSmsSender
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public SmsMisrOtpSmsSender(
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task SendOtpAsync(
        string phoneNumber,
        string otp)
    {
        var username = _configuration["SmsMisr:Username"];
        var password = _configuration["SmsMisr:Password"];
        var sender = _configuration["SmsMisr:Sender"];
        var template = _configuration["SmsMisr:Template"];

        if (string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(sender) ||
            string.IsNullOrWhiteSpace(template))
        {
            throw new InvalidOperationException(
                "SMS Misr settings are missing.");
        }

        // Convert Egyptian mobile number to international format.
        phoneNumber = phoneNumber.Trim();

        if (phoneNumber.StartsWith("+"))
            phoneNumber = phoneNumber[1..];

        if (phoneNumber.StartsWith("01") &&
            phoneNumber.Length == 11)
        {
            phoneNumber = "2" + phoneNumber;
        }

        if (phoneNumber.Length == 12 &&
            phoneNumber.StartsWith("20"))
        {
            // Already in international format.
        }
        else
        {
            throw new InvalidOperationException(
                "Phone number must be a valid Egyptian mobile number.");
        }

        var data = new Dictionary<string, string>
        {
            ["environment"] = "2",
            ["username"] = username,
            ["password"] = password,
            ["sender"] = sender,
            ["mobile"] = phoneNumber,
            ["template"] = template,
            ["otp"] = otp
        };

        using var content = new FormUrlEncodedContent(data);

        using var response = await _httpClient.PostAsync(
            "https://smsmisr.com/api/OTP/",
            content);

        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();

        Console.WriteLine("SMS Misr Response Body:");
        Console.WriteLine(responseBody);

        using var json = JsonDocument.Parse(responseBody);

        if (!json.RootElement.TryGetProperty("Code", out var codeElement))
        {
            throw new InvalidOperationException(
                "SMS Misr response does not contain a 'Code' field.");
        }

        var code = codeElement.ToString();

        if (code != "4901")
        {
            throw new InvalidOperationException(
                $"SMS Misr rejected the OTP request. Code: {code}");
        }









    }
}*/
public class ConsoleOtpSmsSender : IOtpSmsSender
{
    public Task SendOtpAsync(string phoneNumber, string otp)
    {
        Console.WriteLine("========== OTP ==========");
        Console.WriteLine($"Phone Number: {phoneNumber}");
        Console.WriteLine($"OTP Code: {otp}");
        Console.WriteLine("=========================");

        return Task.CompletedTask;
    }
}
