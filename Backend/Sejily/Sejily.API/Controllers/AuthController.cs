using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Sejily.API.Data;
using Sejily.API.DTOs.Auth;
using Sejily.API.Models.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Sejily.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly SejilyDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(
        UserManager<User> userManager,
        SejilyDbContext context,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _context = context;
        _configuration = configuration;
    }

    // =========================
    // Register Patient
    // =========================

    [HttpPost("register/patient")]
    public async Task<IActionResult> RegisterPatient(
        RegisterPatientRequest request)
    {
        // Check if National ID already exists
        var existingUser = await _userManager
            .FindByNameAsync(request.NationalID);

        if (existingUser != null)
        {
            return BadRequest("National ID already exists.");
        }

        // Create Identity User
        var user = new User
        {
            UserName = request.NationalID,
            NationalID = request.NationalID,
            FullName = request.FullName,
            Address = request.Address,
            DateOfBirth = request.DateOfBirth,
            BloodType = request.BloodType,
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
        // Check if National ID already exists
        var existingUser = await _userManager
            .FindByNameAsync(request.NationalID);

        if (existingUser != null)
        {
            return BadRequest("National ID already exists.");
        }

        // Create Identity User
        var user = new User
        {
            UserName = request.NationalID,
            NationalID = request.NationalID,
            FullName = request.FullName,
            Address = request.Address,
            DateOfBirth = request.DateOfBirth,
            BloodType = request.BloodType,
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

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        LoginRequest request)
    {
        // Find user by National ID
        var user = await _userManager
            .FindByNameAsync(request.NationalID);

        if (user == null)
        {
            return Unauthorized(
                "Invalid National ID or password.");
        }

        // Check password
        var passwordCorrect = await _userManager
            .CheckPasswordAsync(user, request.Password);

        if (!passwordCorrect)
        {
            return Unauthorized(
                "Invalid National ID or password.");
        }

        // Create JWT claims
        var claims = new List<Claim>
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()),

            new Claim(
                ClaimTypes.Name,
                user.UserName!)
        };

        // Read JWT settings
        var key = _configuration["Jwt:Key"];
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];

        // Create security key
        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(key!));

        // Create signing credentials
        var credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);

        // Create JWT
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        // Convert JWT to string
        var tokenString = new JwtSecurityTokenHandler()
            .WriteToken(token);

        return Ok(new
        {
            token = tokenString
        });
    }
}