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

            options.AddPolicy("VerifiedDoctor", p =>
            {
                p.RequireClaim("AccountType", "Doctor");
                p.AddRequirements(new Sejily.API.Authorization.VerifiedDoctorRequirement());
            });

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
        builder.Services.AddScoped<IPatientHealthService, PatientHealthService>();
        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddScoped<IAccountSwitchingService, AccountSwitchingService>();
        builder.Services.AddScoped<IDoctorWorkplaceService, DoctorWorkplaceService>();
        builder.Services.AddScoped<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, Sejily.API.Authorization.VerifiedDoctorHandler>();

        // builder.Services.AddHttpClient<IOtpSmsSender, SmsMisrOtpSmsSender>();
        builder.Services.AddScoped<IOtpSmsSender, ConsoleOtpSmsSender>();
        // F6 (T02): shared clock + notification settings bound from configuration
        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.Configure<Sejily.API.Settings.NotificationSettings>(
            builder.Configuration.GetSection(Sejily.API.Settings.NotificationSettings.SectionName));
        // F6 (T05): temporary pass-through until Dev 1's NameMasker is merged
        builder.Services.AddScoped<Sejily.API.Services.Notifications.INameMasker,
            Sejily.API.Services.Notifications.PassThroughNameMasker>();
        // F6 (T07): read-side notification queries
        builder.Services.AddScoped<Sejily.API.Services.Notifications.NotificationComposer>();
        builder.Services.AddScoped<Sejily.API.Services.Notifications.INotificationQueries,
            Sejily.API.Services.Notifications.NotificationQueries>();
        // F6 (T11): device-token register/remove
        builder.Services.AddScoped<Sejily.API.Services.Notifications.IDeviceTokenService,
            Sejily.API.Services.Notifications.DeviceTokenService>();
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

        builder.Services.AddHostedService<
   PatientRegistrationCleanupService>();
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

        //clean the mobile requestid registration records
       
        // 9. Run the application

        app.Run();
    }
}
