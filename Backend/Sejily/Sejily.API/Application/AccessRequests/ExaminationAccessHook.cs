using Sejily.API.Models.Entities;

namespace Sejily.API.Application.AccessRequests;

public interface IExaminationAccessHook
{
    Task OnExaminationRecordedAsync(
        AccessRequest request,
        int doctorId,
        int patientId,
        DateOnly examinationDate,
        int treatmentLengthDays,
        CancellationToken cancellationToken = default);

    Task OnExaminationUpdatedAsync(
        AccessRequest request,
        int doctorId,
        int patientId,
        DateOnly examinationDate,
        int treatmentLengthDays,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// F5 calls this from inside IAccessAuthorizationService.WriteAsDoctorAsync and saves the encounter,
/// access end-date update, audit entry, and notification intent in that same transaction.
/// </summary>
public sealed class ExaminationAccessHook(
    IClock clock,
    IAccessDomainEventPublisher eventPublisher) : IExaminationAccessHook
{
    public Task OnExaminationRecordedAsync(
        AccessRequest request,
        int doctorId,
        int patientId,
        DateOnly examinationDate,
        int treatmentLengthDays,
        CancellationToken cancellationToken = default) =>
        ApplyAsync(request, doctorId, patientId, examinationDate, treatmentLengthDays, request.OnExaminationRecorded, cancellationToken);

    public Task OnExaminationUpdatedAsync(
        AccessRequest request,
        int doctorId,
        int patientId,
        DateOnly examinationDate,
        int treatmentLengthDays,
        CancellationToken cancellationToken = default) =>
        ApplyAsync(request, doctorId, patientId, examinationDate, treatmentLengthDays, request.OnExaminationUpdated, cancellationToken);

    private async Task ApplyAsync(
        AccessRequest request,
        int doctorId,
        int patientId,
        DateOnly examinationDate,
        int treatmentLengthDays,
        Action<DateOnly, int, DateTimeOffset> transition,
        CancellationToken ct)
    {
        if (request.DoctorId != doctorId || request.RecipientPatientId != patientId)
            throw new AccessRequestUseCaseException(AccessRequestUseCaseError.Forbidden, "The active request does not belong to this doctor and patient.");

        try
        {
            transition(examinationDate, treatmentLengthDays, clock.UtcNow);
            await eventPublisher.PublishAsync(request, request.DequeueDomainEvents(), ct);
        }
        catch (AccessRequestDomainException ex)
        {
            throw new AccessRequestUseCaseException(ex.Error switch
            {
                AccessRequestError.Forbidden => AccessRequestUseCaseError.Forbidden,
                AccessRequestError.Expired => AccessRequestUseCaseError.Expired,
                AccessRequestError.Validation => AccessRequestUseCaseError.Validation,
                _ => AccessRequestUseCaseError.InvalidState
            }, ex.Message, ex);
        }
    }
}
