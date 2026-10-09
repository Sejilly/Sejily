using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Sejily.API.Data;
using Sejily.API.Services;
using Sejily.API.Models.Entities;
using System.Text;
using System.Text.Json.Serialization;
namespace Sejily.API;

public class Program
{
    public static void Main(string[] args)
    {
        // 1. Create the application builder
        var builder = WebApplication.CreateBuilder(args);

        // 2. Register DbContext
        builder.Services.AddDbContext<SejilyDbContext>(options =>
            options.UseSqlServer(
                builder.Configuration.GetConnectionString("DefaultConnection")));

        // 3. Register ASP.NET Core Identity
        builder.Services
            .AddIdentity<User, IdentityRole<int>>()
            .AddEntityFrameworkStores<SejilyDbContext>()
            .AddDefaultTokenProviders();

            builder.Services.AddScoped<IDependentService, DependentService>();

        // 4. Configure JWT Authentication
        builder.Services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                    JwtBearerDefaults.AuthenticationScheme;

                options.DefaultChallengeScheme =
                    JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer =
                            builder.Configuration["Jwt:Issuer"],

                        ValidAudience =
                            builder.Configuration["Jwt:Audience"],

                        IssuerSigningKey =
                            new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(
                                    builder.Configuration["Jwt:Key"]!))
                    };
            });

        // 5. Register Controllers
        builder.Services
     .AddControllers()
     .AddJsonOptions(options =>
     {
         options.JsonSerializerOptions.Converters.Add(
             new JsonStringEnumConverter());
     });

        // 6. Swagger
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();


        // 7. Build the application
        var app = builder.Build();


        // 8. Configure HTTP request pipeline
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();

        app.UseAuthorization();

        app.MapControllers();


        // 9. Run the application
        app.Run();
    }
}