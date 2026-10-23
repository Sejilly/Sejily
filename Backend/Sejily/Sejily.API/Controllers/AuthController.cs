
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
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
    private readonly IDoctorWorkplaceService _doctorWorkplaceService;

    private const int OtpLifetimeMinutes = 5;
    private const int OtpResendCooldownSeconds = 60;
    private const int MaxOtpAttempts = 5;
    private const int PasswordResetWindowMinutes = 10;

    public AuthController(
        UserManager<User> userManager,
        SejilyDbContext context,
        IConfiguration configuration,
        IOtpSmsSender otpSmsSender,
        IDoctorWorkplaceService doctorWorkplaceService)
    {
        _userManager = userManager;
        _context = context;
        _configuration = configuration;
        _otpSmsSender = otpSmsSender;
        _doctorWorkplaceService = doctorWorkplaceService;
    }

    // =====================================
    // Register Patient
    // =====================================

    [HttpPost("register/patient")]
    public async Task<IActionResult> RegisterPatient(
        RegisterPatientRequest request)
    {
        var existingUser = await _context.Users.AnyAsync(u =>
            u.NationalID == request.NationalID &&
            u.AccountType == AccountType.Patient);

        if (existingUser)
        {
            return BadRequest(
                "A Patient account with this National ID already exists.");
        }

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

        var result = await _userManager.CreateAsync(
            user, request.Password);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

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

    // =====================================
    // Register Doctor
    // =====================================

    [HttpPost("register/doctor")]
    public async Task<IActionResult> RegisterDoctor(
     RegisterDoctorRequest request)
    {
        var existingUser = await _context.Users.AnyAsync(u =>
            u.NationalID == request.NationalID &&
            u.AccountType == AccountType.Doctor);

        if (existingUser)
        {
            return Conflict(new
            {
                field = "nationalID",
                message = "A Doctor account with this National ID already exists."
            });
        }

        var existingMedicalLicense = await _context.Doctors
            .AnyAsync(d =>
                d.MedicalLicenseNumber == request.MedicalLicenseNumber);

        if (existingMedicalLicense)
        {
            return Conflict(new
            {
                field = "medicalLicenseNumber",
                message = "This medical license number is already registered."
            });
        }

        var existingSyndicateCard = await _context.Doctors
            .AnyAsync(d =>
                d.SyndicateCardNumber == request.SyndicateCardNumber);

        if (existingSyndicateCard)
        {
            return Conflict(new
            {
                field = "syndicateCardNumber",
                message = "This syndicate card number is already registered."
            });
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
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

            var result = await _userManager.CreateAsync(
                user, request.Password);

            if (!result.Succeeded)
            {
                await transaction.RollbackAsync();

                return BadRequest(new
                {
                    errors = result.Errors
                        .Select(e => e.Description)
                });
            }

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

            var workplaces = await _doctorWorkplaceService.AddWorkplacesAsync(
                doctor.UserId,
                request.WorkPlaces);

            await transaction.CommitAsync();

            return Ok(new
            {
                message = "Doctor registered successfully.",
                userId = user.Id,
                doctor = new
                {
                    doctor.SyndicateCardNumber,
                    doctor.MedicalLicenseNumber,
                    doctor.Specialization,
                    doctor.IsVerified
                },
                workplaces
            });
        }
        catch (DbUpdateException ex)
        {
            await transaction.RollbackAsync();

            var sqlException = ex.GetBaseException() as SqlException;

            if (sqlException != null &&
                sqlException.Number is 2601 or 2627)
            {
                var errorMessage = sqlException.Message;

                if (errorMessage.Contains(
                    "MedicalLicenseNumber",
                    StringComparison.OrdinalIgnoreCase))
                {
                    return Conflict(new
                    {
                        field = "medicalLicenseNumber",
                        message = "This medical license number is already registered."
                    });
                }

                if (errorMessage.Contains(
                    "SyndicateCardNumber",
                    StringComparison.OrdinalIgnoreCase))
                {
                    return Conflict(new
                    {
                        field = "syndicateCardNumber",
                        message = "This syndicate card number is already registered."
                    });
                }

                if (errorMessage.Contains(
                    "NationalID",
                    StringComparison.OrdinalIgnoreCase))
                {
                    return Conflict(new
                    {
                        field = "nationalID",
                        message = "A Doctor account with this National ID already exists."
                    });
                }

                return Conflict(new
                {
                    message = "One of the unique values is already registered."
                });
            }

            throw;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // =====================================
    // Login
    // =====================================

    [HttpPost("login/patient")]
    public async Task<IActionResult> LoginPatient(
        LoginRequest request)
    {
        return await LoginByType(request, AccountType.Patient);
    }

    [HttpPost("login/doctor")]
    public async Task<IActionResult> LoginDoctor(
        LoginRequest request)
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
        {
            return Unauthorized(
                "Invalid National ID or password.");
        }

        var passwordCorrect = await _userManager
            .CheckPasswordAsync(user, request.Password);

        if (!passwordCorrect)
        {
            return Unauthorized(
                "Invalid National ID or password.");
        }

        var claims = new List<Claim>
        {
            new(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()),

            new(
                ClaimTypes.Name,
                user.UserName!),

            new(
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

    // =====================================
    // Step 1: Forgot Password
    // National ID + Phone + Account Type
    // =====================================

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
        ForgotPasswordRequest request)
    {
        const string message =
            "If the account exists and the phone number matches, " +
            "a reset code will be sent.";

        var requestId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var user = await _context.Users.FirstOrDefaultAsync(u =>
            u.NationalID == request.NationalID &&

            u.AccountType == request.AccountType!.Value);

        if (user == null ||
            string.IsNullOrWhiteSpace(user.PhoneNumber) ||
            NormalizeEgyptianPhone(user.PhoneNumber) !=
            NormalizeEgyptianPhone(request.PhoneNumber))
        {
            // Use the same response format even if no account matches.
            return Ok(new
            {
                message,
                requestId
            });
        }

        // Avoid sending another code within the cooldown period.
        var recentRequest = await _context.PasswordResetOtps
            .Where(x =>
                x.UserId == user.Id &&
                x.RequestId != null &&
                x.LastSentAtUtc != null &&
                x.LastSentAtUtc > now.AddSeconds(
                    -OtpResendCooldownSeconds) &&
                x.VerifiedAtUtc == null &&
                !x.ResetUsed)
            .OrderByDescending(x => x.LastSentAtUtc)
            .FirstOrDefaultAsync();

        if (recentRequest?.RequestId is Guid existingRequestId)
        {
            return Ok(new
            {
                message,
                requestId = existingRequestId
            });
        }

        try
        {
            await CreateAndSendOtpAsync(user, requestId);

            return Ok(new
            {
                message,
                requestId
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"OTP sending failed: {ex.GetType().Name}");

            return StatusCode(503, new
            {
                message = "Unable to send the reset code. Please try again later."
            });
        }
    }

    // =====================================
    // Resend OTP
    // RequestId is sent in the background
    // =====================================

    [HttpPost("resend-otp")]
    public async Task<IActionResult> ResendOtp(
        ResendOtpRequest request)
    {
        if (request.RequestId == null ||
            request.RequestId == Guid.Empty)
        {
            return BadRequest("Invalid or expired reset request.");
        }

        var resetRequest = await _context.PasswordResetOtps
            .FirstOrDefaultAsync(x =>
                x.RequestId == request.RequestId.Value);

        if (resetRequest == null ||
            resetRequest.VerifiedAtUtc != null ||
            resetRequest.ResetUsed)
        {
            return BadRequest("Invalid or expired reset request.");
        }

        var now = DateTime.UtcNow;

        if (resetRequest.LastSentAtUtc.HasValue &&
            now < resetRequest.LastSentAtUtc.Value.AddSeconds(
                OtpResendCooldownSeconds))
        {
            return StatusCode(429, new
            {
                message =
                    "Please wait before requesting another OTP."
            });
        }

        var user = await _userManager.FindByIdAsync(
            resetRequest.UserId.ToString());

        if (user == null ||
            string.IsNullOrWhiteSpace(user.PhoneNumber))
        {
            return BadRequest("Invalid or expired reset request.");
        }

        var otp = GenerateOtp();

        // Keep the same RequestId; only the OTP changes.
        resetRequest.OtpHash = HashSecret(otp);
        resetRequest.ExpiresAtUtc =
            now.AddMinutes(OtpLifetimeMinutes);
        resetRequest.LastSentAtUtc = now;
        resetRequest.FailedAttempts = 0;
        resetRequest.OtpConsumed = false;

        resetRequest.VerifiedAtUtc = null;
        resetRequest.ResetTokenHash = null;
        resetRequest.ResetTokenExpiresAtUtc = null;
        resetRequest.ResetUsed = false;

        await _context.SaveChangesAsync();

        try
        {
            await _otpSmsSender.SendOtpAsync(
                user.PhoneNumber,
                otp);
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"OTP sending failed: {ex.GetType().Name}");

            _context.PasswordResetOtps.Remove(resetRequest);
            await _context.SaveChangesAsync();

            return StatusCode(503, new
            {
                message = "Unable to send the reset code."
            });
        }

        return Ok(new
        {
            message = "A new OTP has been sent if delivery succeeded.",
            requestId = request.RequestId
        });
    }

    // =====================================
    // Step 2: Verify OTP
    // User enters the OTP only.
    // RequestId is sent in the background.
    // =====================================

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp(
        VerifyOtpRequest request)
    {
        if (request.RequestId == null ||
            request.RequestId == Guid.Empty)
        {
            return BadRequest("Invalid or expired OTP.");
        }

        var now = DateTime.UtcNow;

        var resetRequest = await _context.PasswordResetOtps
            .FirstOrDefaultAsync(x =>
                x.RequestId == request.RequestId.Value);

        if (resetRequest == null ||
            resetRequest.OtpConsumed ||
            resetRequest.VerifiedAtUtc != null ||
            resetRequest.ExpiresAtUtc <= now ||
            resetRequest.FailedAttempts >= MaxOtpAttempts)
        {
            return BadRequest("Invalid or expired OTP.");
        }

        var submittedHash = HashSecret(request.Otp);

        var expectedBytes = Encoding.UTF8.GetBytes(
            resetRequest.OtpHash);

        var submittedBytes = Encoding.UTF8.GetBytes(
            submittedHash);

        if (!CryptographicOperations.FixedTimeEquals(
            expectedBytes, submittedBytes))
        {
            resetRequest.FailedAttempts++;

            if (resetRequest.FailedAttempts >= MaxOtpAttempts)
            {
                resetRequest.OtpConsumed = true;
            }

            await _context.SaveChangesAsync();

            return BadRequest("Invalid or expired OTP.");
        }

        // OTP is now consumed and cannot be verified again.
        resetRequest.OtpConsumed = true;
        resetRequest.VerifiedAtUtc = now;

        // This expiry now limits the password-reset stage.
        // No reset token is sent to the user.
        resetRequest.ResetTokenHash = null;
        resetRequest.ResetTokenExpiresAtUtc =
            now.AddMinutes(PasswordResetWindowMinutes);
        resetRequest.ResetUsed = false;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message =
                "OTP verified successfully. You can now reset your password."
        });
    }

    // =====================================
    // Step 3: Reset Password
    // New password + confirmation
    // =====================================

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(
        ResetPasswordRequest request)
    {
        if (request.RequestId == null ||
            request.RequestId == Guid.Empty)
        {
            return BadRequest("Invalid or expired reset request.");
        }

        var now = DateTime.UtcNow;

        var resetRequest = await _context.PasswordResetOtps
            .FirstOrDefaultAsync(x =>
                x.RequestId == request.RequestId.Value &&
                x.VerifiedAtUtc != null &&
                !x.ResetUsed &&
                x.ResetTokenExpiresAtUtc != null &&
                x.ResetTokenExpiresAtUtc > now);

        if (resetRequest == null)
        {
            return BadRequest("Invalid or expired reset request.");
        }

        var user = await _userManager.FindByIdAsync(
            resetRequest.UserId.ToString());

        if (user == null)
        {
            return BadRequest("Unable to reset the password.");
        }

        // Keep the password update and request consumption together.
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            // Identity token is generated and used internally.
            // It is never returned to the frontend.
            var identityToken =
                await _userManager.GeneratePasswordResetTokenAsync(user);

            var result = await _userManager.ResetPasswordAsync(
                user,
                identityToken,
                request.NewPassword);

            if (!result.Succeeded)
            {
                await transaction.RollbackAsync();

                return BadRequest(new
                {
                    message = "Password reset failed.",
                    errors = result.Errors.Select(
                        e => e.Description)
                });
            }

            resetRequest.ResetUsed = true;
            resetRequest.OtpConsumed = true;

            // Invalidate every other reset request for this user.
            var otherRequests = await _context.PasswordResetOtps
                .Where(x =>
                    x.UserId == user.Id &&
                    x.Id != resetRequest.Id)
                .ToListAsync();

            foreach (var item in otherRequests)
            {
                item.OtpConsumed = true;
                item.ResetUsed = true;
                item.ResetTokenExpiresAtUtc = now;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new
            {
                message = "Password reset successfully."
            });
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // =====================================
    // Helper Methods
    // =====================================

    private async Task CreateAndSendOtpAsync(
        User user,
        Guid requestId)
    {
        var now = DateTime.UtcNow;

        // Invalidate any older requests for this user.
        var oldRequests = await _context.PasswordResetOtps
            .Where(x => x.UserId == user.Id)
            .ToListAsync();

        _context.PasswordResetOtps.RemoveRange(oldRequests);

        var otp = GenerateOtp();

        var resetRequest = new PasswordResetOtp
        {
            UserId = user.Id,
            RequestId = requestId,
            LastSentAtUtc = now,

            OtpHash = HashSecret(otp),
            ExpiresAtUtc = now.AddMinutes(OtpLifetimeMinutes),

            FailedAttempts = 0,
            OtpConsumed = false,
            VerifiedAtUtc = null,

            ResetTokenHash = null,
            ResetTokenExpiresAtUtc = null,
            ResetUsed = false
        };

        _context.PasswordResetOtps.Add(resetRequest);
        await _context.SaveChangesAsync();

        try
        {
            await _otpSmsSender.SendOtpAsync(
                user.PhoneNumber!,
                otp);
        }
        catch
        {
            _context.PasswordResetOtps.Remove(resetRequest);
            await _context.SaveChangesAsync();
            throw;
        }
    }
    /// <summary>
    /// ////////////////////mobile1
    /// </summary>
    /// <returns></returns>
    /// 



    [HttpPost("register/patient/start")]
    public async Task<IActionResult> StartPatientRegistration(
        StartPatientRegistrationRequest request)
    {
        var existingUser = await _context.Users.AnyAsync(u =>
            u.NationalID == request.NationalID &&
            u.AccountType == AccountType.Patient);

        if (existingUser)
        {
            return Conflict(new
            {
                message =
                    "A Patient account with this National ID already exists."
            });
        }

        var registrationRequestId = Guid.NewGuid();

        var draft = new PatientRegistrationDraft
        {
            RequestId = registrationRequestId,
            FullName = request.FullName,
            NationalID = request.NationalID,
            Address = request.Address,
            DateOfBirth = request.DateOfBirth,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(30),
            IsCompleted = false
        };

        _context.PatientRegistrationDrafts.Add(draft);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Registration started successfully.",
            requestId = registrationRequestId
        });
    }

    //mobile registration 2

    [HttpPost("register/patient/complete")]
    public async Task<IActionResult> CompletePatientRegistration(
        CompletePatientRegistrationRequest request)
    {
        if (request.RequestId == Guid.Empty)
        {
            return BadRequest("Invalid or expired registration request.");
        }

        var now = DateTime.UtcNow;

        var draft = await _context.PatientRegistrationDrafts
            .FirstOrDefaultAsync(x =>
                x.RequestId == request.RequestId &&
                !x.IsCompleted &&
                x.ExpiresAtUtc > now);

        if (draft == null)
        {
            return BadRequest("Invalid or expired registration request.");
        }

        var existingUser = await _context.Users.AnyAsync(u =>
            u.NationalID == draft.NationalID &&
            u.AccountType == AccountType.Patient);

        if (existingUser)
        {
            return Conflict(
                "A Patient account with this National ID already exists.");
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var user = new User
            {
                UserName = $"{draft.NationalID}_Patient",
                AccountType = AccountType.Patient,
                NationalID = draft.NationalID,
                FullName = draft.FullName,
                Address = draft.Address,
                DateOfBirth = draft.DateOfBirth,
                PhoneNumber = request.PhoneNumber
            };

            var result = await _userManager.CreateAsync(
                user, request.Password);

            if (!result.Succeeded)
            {
                await transaction.RollbackAsync();

                return BadRequest(new
                {
                    message = "Patient registration failed.",
                    errors = result.Errors.Select(e => e.Description)
                });
            }

            var patient = new Patient
            {
                UserId = user.Id,
                HealthCardNumber = request.HealthCardNumber
            };

            _context.Patients.Add(patient);

            // draft.IsCompleted = true;
            // Delete the temporary registration data.
            _context.PatientRegistrationDrafts.Remove(draft);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new
            {
                message = "Patient registered successfully.",
                userId = user.Id
            });
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }










    private static string GenerateOtp()
    {
        return RandomNumberGenerator
            .GetInt32(0, 1_000_000)
            .ToString("D6");
    }

    private static string NormalizeEgyptianPhone(string phone)
    {
        var digits = new string(
            phone.Where(char.IsDigit).ToArray());

        if (digits.Length == 11 &&
            digits.StartsWith("01"))
        {
            return "2" + digits;
        }

        return digits;
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
