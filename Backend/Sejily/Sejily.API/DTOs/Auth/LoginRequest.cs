namespace Sejily.API.DTOs.Auth;

public class LoginRequest
{
    public string NationalID { get; set; } = null!;

    public string Password { get; set; } = null!;
    
    
}