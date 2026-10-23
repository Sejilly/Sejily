using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Sejily.API.Data;
using Sejily.API.Models.Entities;
using Sejily.API.Services;
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

        //add policies for account switching logic
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("PatientOnly", p =>
                p.RequireClaim("AccountType", "Patient"));

            options.AddPolicy("DoctorOnly", p =>
                p.RequireClaim("AccountType", "Doctor"));

            options.AddPolicy("PersonalAccountOnly", p =>
            {
                p.RequireClaim("AccountType", "Patient");
                p.RequireAssertion(ctx =>
                    !ctx.User.HasClaim(c => c.Type == "actor_id"));
            });
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
        //builder.Services.AddSwaggerGen();

        //Add Authentication capability in swagger
        builder.Services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Paste the JWT only (without the word Bearer)."
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
        });

        builder.Services.AddScoped<IDependentService, DependentService>();
        builder.Services.AddScoped<IKinshipVerificationService, KinshipVerificationService>();
        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddScoped<IAccountSwitchingService, AccountSwitchingService>();
        builder.Services.AddScoped<IDoctorWorkplaceService, DoctorWorkplaceService>();

        // builder.Services.AddHttpClient<IOtpSmsSender, SmsMisrOtpSmsSender>();
        builder.Services.AddScoped<IOtpSmsSender, ConsoleOtpSmsSender>();
        // 7. Build the application


        builder.Services.AddCors(options =>
        {
            options.AddPolicy("FrontendPolicy", policy =>
            {
                policy
                    .WithOrigins("http://localhost:3000")
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("Frontend", policy =>
                policy.WithOrigins(
                        "https://sejilly.github.io")
                      .WithHeaders("Content-Type", "Authorization")
                      .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE"));
        });

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SejilyDbContext>();
            db.Database.Migrate();
        }


        // 8. Configure HTTP request pipeline
        if (app.Environment.IsProduction() || app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        
        app.UseRouting();

        app.UseCors("Frontend");

        app.UseAuthentication();

        app.UseAuthorization();

        app.MapControllers();


        // 9. Run the application
        app.Run();
    }
}
