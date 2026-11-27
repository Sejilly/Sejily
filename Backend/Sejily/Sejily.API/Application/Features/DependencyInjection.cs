using Sejily.API.Application.Features.PatientHealthDataEntry;
using Sejily.API.Application.AccessRequests;

namespace Sejily.API.Application.Features.DependencyInjection;


public static class DependencyInjection
{
    public static IServiceCollection AddApplicationFeatures(this IServiceCollection services)
    {
        services.AddScoped<AddPatientHealthData>();
        services.AddScoped<IClock, SystemClock>();
        services.AddScoped<IAccessRequestRepository, EfAccessRequestRepository>();
        services.AddScoped<IAuditLog, EfAccessAuditLog>();
        services.AddScoped<IAccessNotificationOutbox, EfAccessNotificationOutbox>();
        services.AddScoped<IGuardianshipService, EfGuardianshipService>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped<IAccessDomainEventPublisher, AccessDomainEventPublisher>();
        services.AddScoped<AccessRequestCommandService>();
        services.AddScoped<ICommandHandler<AcceptAccessRequest>, AcceptAccessRequestHandler>();
        services.AddScoped<ICommandHandler<RejectAccessRequest>, RejectAccessRequestHandler>();
        services.AddScoped<ICommandHandler<UndoRejection>, UndoRejectionHandler>();
        services.AddScoped<ICommandHandler<CloseAccessByPatient>, CloseAccessByPatientHandler>();
        services.AddScoped<ICommandHandler<ExtendAccess>, ExtendAccessHandler>();
        services.AddScoped<ICommandHandler<CloseAccessByDoctor>, CloseAccessByDoctorHandler>();
        services.AddScoped<IAccessAuthorizationService, AccessAuthorizationService>();
        services.AddScoped<IExaminationAccessHook, ExaminationAccessHook>();
        services.AddScoped<IAccessRequestMaintenanceJobs, AccessRequestMaintenanceJobs>();
        services.AddScoped<ICommandHandler<ExpirePendingRequestsCommand>, ExpirePendingRequestsHandler>();
        services.AddScoped<ICommandHandler<FinalizeRejectionsCommand>, FinalizeRejectionsHandler>();
        services.AddScoped<ICommandHandler<ExpireTemporaryAccessCommand>, ExpireTemporaryAccessHandler>();
        services.AddScoped<ICommandHandler<ExpireAccessByEndDateCommand>, ExpireAccessByEndDateHandler>();
        return services;
    }
}
