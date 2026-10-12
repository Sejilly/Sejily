using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Sejily.API.Data;
using Sejily.API.DTOs.Auth;
using Sejily.API.Models.Entities;
using Sejily.API.Models.Enums;
using Sejily.API.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Sejily.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly SejilyDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IOtpSmsSender _otpSmsSender;
    public AuthController(
        UserManager<User> userManager,
        SejilyDbContext context,
        IConfiguration configuration,
        IOtpSmsSender otpSmsSender
        )
    {
        _userManager = userManager;
        _context = context;
        _configuration = configuration;
        _otpSmsSender= otpSmsSender;
    }

    // =========================
    // Register Patient
    // =========================

    [HttpPost("register/patient")]
    public async Task<IActionResult> RegisterPatient(
        RegisterPatientRequest request)
    {
        // Check if National ID with accountType already exists
        var existingUser = await _context.Users
      .AnyAsync(u =>
          u.NationalID == request.NationalID &&
          u.AccountType == AccountType.Patient);

        if (existingUser)
        {
            return BadRequest(
                "A Patient account with this National ID already exists.");
        }

        // Create Identity User
        var user = new User
        {
            UserName = $"{request.NationalID}_Patient",
            AccountType = AccountType.Patient,
            NationalID = request.NationalID,
            FullName = request.FullName,
            Address = request.Address,
            DateOfBirth = request.DateOfBirth,
           
            PhoneNumber = request.PhoneNumber
        };

        var result = await _userManager
            .CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        // Create Patient profile
        var patient = new Patient
        {
            UserId = user.Id,
            BloodType = request.BloodType,
            HealthCardNumber = request.HealthCardNumber
        };

        _context.Patients.Add(patient);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Patient registered successfully.",
            userId = user.Id
        });
    }


    // =========================
    // Register Doctor
    // =========================

    [HttpPost("register/doctor")]
    public async Task<IActionResult> RegisterDoctor(
        RegisterDoctorRequest request)
    {
        // Check if National ID with accountType already exists
        var existingUser = await _context.Users
    .AnyAsync(u =>
        u.NationalID == request.NationalID &&
        u.AccountType == AccountType.Doctor);

        if (existingUser)
        {
            return BadRequest(
                "A Doctor account with this National ID already exists.");
        }
        // Create Identity User
        var user = new User
        {
            UserName = $"{request.NationalID}_Doctor",
            NationalID = request.NationalID,
            AccountType = AccountType.Doctor,
            FullName = request.FullName,
            Address = request.Address,
            DateOfBirth = request.DateOfBirth,
            
            PhoneNumber = request.PhoneNumber
        };

        var result = await _userManager
            .CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        // Create Doctor profile
        var doctor = new Doctor
        {
            UserId = user.Id,
           
            SyndicateCardNumber = request.SyndicateCardNumber,
            MedicalLicenseNumber = request.MedicalLicenseNumber,
            Specialization = request.Specialization,
            IsVerified = false
        };

        _context.Doctors.Add(doctor);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Doctor registered successfully.",
            userId = user.Id
        });
    }


    // =========================
    // Login
    // =========================

    [HttpPost("login/patient")]
    public async Task<IActionResult> LoginPatient(LoginRequest request)
    {
        return await LoginByType(request, AccountType.Patient);
    }

    [HttpPost("login/doctor")]
    public async Task<IActionResult> LoginDoctor(LoginRequest request)
    {
        return await LoginByType(request, AccountType.Doctor);
    }

    private async Task<IActionResult> LoginByType(
        LoginRequest request,
        AccountType accountType)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u =>
            u.NationalID == request.NationalID &&
            u.AccountType == accountType);

        if (user == null)
            return Unauthorized("Invalid National ID or password.");

        var passwordCorrect = await _userManager
            .CheckPasswordAsync(user, request.Password);

        if (!passwordCorrect)
            return Unauthorized("Invalid National ID or password.");

        var claims = new List<Claim>
    {
        new Claim(
            ClaimTypes.NameIdentifier,
            user.Id.ToString()),

        new Claim(
            ClaimTypes.Name,
            user.UserName!),

        new Claim(
            "AccountType",
            user.AccountType.ToString())
    };

        var key = _configuration["Jwt:Key"];
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];

        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(key!));

        var credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler()
            .WriteToken(token);

        return Ok(new { token = tokenString });
    
}
    ////////////////////
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
    ForgotPasswordRequest request)
    {
        var genericResponse = new
        {
            message = "If the account exists and has a phone number, " +
                      "a reset code will be sent."
        };

        var user = await _context.Users.FirstOrDefaultAsync(u =>
            u.NationalID == request.NationalID &&
            u.AccountType == request.AccountType);

        if (user == null ||
            string.IsNullOrWhiteSpace(user.PhoneNumber))
        {
            return Ok(genericResponse);
        }

        var otp = RandomNumberGenerator
            .GetInt32(0, 1_000_000)
            .ToString("D6");

        var resetRequest = new PasswordResetOtp
        {
            UserId = user.Id,
            OtpHash = HashSecret(otp),
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(5),
            FailedAttempts = 0,
            OtpConsumed = false,
            ResetUsed = false
        };

        var oldRequests = await _context.PasswordResetOtps
            .Where(x => x.UserId == user.Id && !x.ResetUsed)
            .ToListAsync();


        _context.PasswordResetOtps.RemoveRange(oldRequests);
        _context.PasswordResetOtps.Add(resetRequest);

        await _context.SaveChangesAsync();

        try
        {
            await _otpSmsSender.SendOtpAsync(
                user.PhoneNumber,
                otp);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SMS Error: {ex.GetType().Name}");
            Console.WriteLine($"SMS Error Message: {ex.Message}");

            _context.PasswordResetOtps.Remove(resetRequest);
            await _context.SaveChangesAsync();

            return StatusCode(503, new
            {
                message = "Unable to send the SMS. Check the server logs."
            });
        }

        return Ok(genericResponse);
    }


    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp(VerifyOtpRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u =>
            u.NationalID == request.NationalID &&
            u.AccountType == request.AccountType);

        if (user == null)
        {
            return BadRequest("Invalid or expired OTP.");
        }

        var resetRequest = await _context.PasswordResetOtps
            .Where(x =>
                x.UserId == user.Id &&
                !x.OtpConsumed &&
                x.ExpiresAtUtc > DateTime.UtcNow)
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync();

        if (resetRequest == null || resetRequest.FailedAttempts >= 5)
        {
            return BadRequest("Invalid or expired OTP.");
        }

        var submittedHash = HashSecret(request.Otp);

        var expectedBytes = Encoding.UTF8.GetBytes(resetRequest.OtpHash);
        var submittedBytes = Encoding.UTF8.GetBytes(submittedHash);

        if (!CryptographicOperations.FixedTimeEquals(
                expectedBytes, submittedBytes))
        {
            resetRequest.FailedAttempts++;

            if (resetRequest.FailedAttempts >= 5)
            {
                resetRequest.OtpConsumed = true;
            }

            await _context.SaveChangesAsync();

            return BadRequest("Invalid or expired OTP.");
        }

        // Consume the OTP so it cannot be used again.
        resetRequest.OtpConsumed = true;
        resetRequest.VerifiedAtUtc = DateTime.UtcNow;

        // Generate a separate token for resetting the password.
        var resetToken = Convert.ToBase64String(
            RandomNumberGenerator.GetBytes(32));

        resetRequest.ResetTokenHash = HashSecret(resetToken);
        resetRequest.ResetTokenExpiresAtUtc =
            DateTime.UtcNow.AddMinutes(10);
        resetRequest.ResetUsed = false;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "OTP verified successfully.",
            resetToken
        });
    }


    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(
    ResetPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ResetToken))
        {
            return BadRequest("Invalid or expired reset token.");
        }

        var tokenHash = HashSecret(request.ResetToken);
        var now = DateTime.UtcNow;

        var resetRequest = await _context.PasswordResetOtps
            .FirstOrDefaultAsync(x =>
                x.ResetTokenHash == tokenHash &&
                x.VerifiedAtUtc != null &&
                !x.ResetUsed &&
                x.ResetTokenExpiresAtUtc > now);

        if (resetRequest == null)
        {
            return BadRequest("Invalid or expired reset token.");
        }

        var user = await _userManager.FindByIdAsync(
            resetRequest.UserId.ToString());

        if (user == null)
        {
            return BadRequest("Unable to reset the password.");
        }

        var identityToken =
            await _userManager.GeneratePasswordResetTokenAsync(user);

        var result = await _userManager.ResetPasswordAsync(
            user,
            identityToken,
            request.NewPassword);

        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                message = "Password reset failed.",
                errors = result.Errors.Select(e => e.Description)
            });
        }

        resetRequest.ResetUsed = true;

        var otherRequests = await _context.PasswordResetOtps
            .Where(x =>
                x.UserId == user.Id &&
                x.Id != resetRequest.Id)
            .ToListAsync();

        foreach (var item in otherRequests)
        {
            item.OtpConsumed = true;
            item.ResetUsed = true;
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Password reset successfully."
        });
    }

    private string HashSecret(string value)
    {
        var hashKey = _configuration["Otp:HashKey"];

        if (string.IsNullOrWhiteSpace(hashKey))
        {
            throw new InvalidOperationException(
                "OTP hash key is not configured.");
        }

        using var hmac = new HMACSHA256(
            Encoding.UTF8.GetBytes(hashKey));

        var hash = hmac.ComputeHash(
            Encoding.UTF8.GetBytes(value));

        return Convert.ToHexString(hash);
    }

}