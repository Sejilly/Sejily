namespace Sejily.API.Services;

public interface IOtpSmsSender
{
    Task SendOtpAsync(string phoneNumber, string otp);
}

