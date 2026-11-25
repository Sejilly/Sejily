using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Sejily.API.Application.Features.DependencyInjection;
using Sejily.API.Application.Repositories.DependencyInjection;
using Sejily.API.Data;
using Sejily.API.Extensions;
using Sejily.API.Infrastructure.Diseases.JsonProviders.DependencyInjection;
using Sejily.API.Models.Entities;
using Sejily.API.Services;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Channels;
namespace Sejily.API;

public class Program
{
    public static void Main(string[] args)
    {
        // 1. Create the application builder
        var builder = WebApplication.CreateBuilder(args);

        // 2. Register DbContext
        // Notifications: the push interceptors are registered as "scoped" (one per request) so they share
        // the same PendingPushCollector as NotificationService, and they are attached to the DbContext below.
        builder.Services.AddScoped<Sejily.API.Services.Notifications.PendingPushCollector>();
        builder.Services.AddScoped<Sejily.API.Services.Notifications.PushOnCommitInterceptor>();
        builder.Services.AddScoped<Sejily.API.Services.Notifications.PushOnSaveInterceptor>();
        builder.Services.AddScoped<Sejily.API.Services.Audit.AuditAppendOnlyInterceptor>();
        builder.Services.AddDbContext<SejilyDbContext>((sp, options) =>
            options.UseSqlServer(
                builder.Configuration.GetConnectionString("DefaultConnection"))
                .AddInterceptors(
                    sp.GetRequiredService<Sejily.API.Services.Notifications.PushOnCommitInterceptor>(),
                    sp.GetRequiredService<Sejily.API.Services.Notifications.PushOnSaveInterceptor>(),
                    sp.GetRequiredService<Sejily.API.Services.Audit.AuditAppendOnlyInterceptor>()));

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
        // Clock (so tests can fake the time) and notification limits from appsettings.json
        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.Configure<Sejily.API.Settings.NotificationSettings>(
            builder.Configuration.GetSection(Sejily.API.Settings.NotificationSettings.SectionName));
        builder.Services.Configure<Sejily.API.Settings.AccessTimingSettings>(
            builder.Configuration.GetSection(Sejily.API.Settings.AccessTimingSettings.SectionName));
        builder.Services.Configure<Sejily.API.Settings.EscalationSettings>(
            builder.Configuration.GetSection(Sejily.API.Settings.EscalationSettings.SectionName));
        builder.Services.Configure<Sejily.API.Settings.RateLimitSettings>(
            builder.Configuration.GetSection(Sejily.API.Settings.RateLimitSettings.SectionName));
        // Audit rows are added by services in the caller's transaction; the interceptor above
        // keeps them append-only
        builder.Services.AddScoped<Sejily.API.Services.Audit.IAuditService,
            Sejily.API.Services.Audit.AuditService>();
        // Emergency access confirmation page
        builder.Services.AddScoped<Sejily.API.Services.Emergency.IEmergencyReviewService,
            Sejily.API.Services.Emergency.EmergencyReviewService>();
        // TEMPORARY: shows full patient names to doctors. Replace with the real name masker before release.
        builder.Services.AddScoped<Sejily.API.Services.Notifications.INameMasker,
            Sejily.API.Services.Notifications.PassThroughNameMasker>();
        // Reading notifications (list, count, read, delete)
        builder.Services.AddScoped<Sejily.API.Services.Notifications.NotificationComposer>();
        builder.Services.AddScoped<Sejily.API.Services.Notifications.INotificationQueries,
            Sejily.API.Services.Notifications.NotificationQueries>();
        // Saving and removing device tokens
        builder.Services.AddScoped<Sejily.API.Services.Notifications.IDeviceTokenService,
            Sejily.API.Services.Notifications.DeviceTokenService>();
        // Creating notifications (other features call this; it does not call SaveChanges itself)
        builder.Services.AddScoped<Sejily.API.Services.Notifications.INotificationService,
            Sejily.API.Services.Notifications.NotificationService>();
        // Push queue (one shared Channel) and the background worker that sends the pushes
        builder.Services.AddSingleton<Channel<Sejily.API.Services.Notifications.PushJob>>(
            Channel.CreateUnbounded<Sejily.API.Services.Notifications.PushJob>());
        builder.Services.AddHostedService<Sejily.API.Services.Notifications.PushDispatchService>();
        // Use the real Firebase sender only if the service-account file exists; otherwise use
        // NullPushSender (sends nothing) so the API still works. The file path is kept in
        // user-secrets / environment variables, never in the repo.
        var firebaseServiceAccount = builder.Configuration[
            Sejily.API.Services.Notifications.FcmPushSender.ConfigKey];
        if (!string.IsNullOrWhiteSpace(firebaseServiceAccount)
            && File.Exists(firebaseServiceAccount))
        {
            builder.Services.AddScoped<Sejily.API.Services.Notifications.IPushSender,
                Sejily.API.Services.Notifications.FcmPushSender>();
        }
        else
        {
            builder.Services.AddScoped<Sejily.API.Services.Notifications.IPushSender,
                Sejily.API.Services.Notifications.NullPushSender>();
        }
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

        builder.Services
            .AddDiseaseJsonProviders()
            .AddRepositories()
            .AddApplicationFeatures();

        // Rate limiting: three policies (general / write / emergency-respond), counted per logged-in user
        builder.Services.AddSejilyRateLimiting();

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

        // After authorization, so the limiter can see who the logged-in user is
        app.UseRateLimiter();

        app.MapControllers();

        //clean the mobile requestid registration records
       
        // 9. Run the application

        app.Run();
    }
}
