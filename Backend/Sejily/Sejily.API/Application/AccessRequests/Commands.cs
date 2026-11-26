using Microsoft.EntityFrameworkCore;
using Sejily.API.Models.Entities;
using Sejily.API.Models.Enums.AccessRequest;

namespace Sejily.API.Application.AccessRequests;

public sealed record ActingContext(int UserId, AccessActorRole Role, int? OnBehalfOfPatientId = null);
public sealed record AcceptAccessRequest(
    int RequestId,
    ActingContext Actor,
    AccessValidityType? DurationType = null,
    DateOnly? CustomEndDate = null);
public sealed record RejectAccessRequest(int RequestId, ActingContext Actor);
public sealed record UndoRejection(int RequestId, ActingContext Actor);
public sealed record CloseAccessByPatient(int RequestId, bool Confirmed, ActingContext Actor);
public sealed record ExtendAccess(int RequestId, DateOnly NewEndDate, ActingContext Actor);
public sealed record CloseAccessByDoctor(int RequestId, bool Confirmed, ActingContext Actor);

public interface ICommandHandler<in TCommand>
{
    Task HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}

public sealed class AccessRequestCommandService(
    IAccessRequestRepository requests,
    IGuardianshipService guardianship,
    IClock clock,
    IUnitOfWork unitOfWork,
    IAccessDomainEventPublisher eventPublisher)
{
    public async Task ExecuteAsync(int requestId, ActingContext context, Action<AccessRequest, AccessActor, DateTimeOffset> transition, CancellationToken ct)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var request = await requests.GetAsync(requestId, ct)
                ?? throw new AccessRequestUseCaseException(AccessRequestUseCaseError.NotFound, "Access request was not found.");
            var actor = await CreateActorAsync(context, request, ct);
            transition(request, actor, clock.UtcNow);
            var events = request.DequeueDomainEvents();
            await eventPublisher.PublishAsync(request, events, ct);
            await unitOfWork.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch (AccessRequestDomainException ex)
        {
            await transaction.RollbackAsync(ct);
            throw new AccessRequestUseCaseException(ex.Error switch
            {
                AccessRequestError.Forbidden => AccessRequestUseCaseError.Forbidden,
                AccessRequestError.InvalidState => AccessRequestUseCaseError.InvalidState,
                AccessRequestError.Expired => AccessRequestUseCaseError.Expired,
                AccessRequestError.Validation => AccessRequestUseCaseError.Validation,
                _ => AccessRequestUseCaseError.InvalidState
            }, ex.Message, ex);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await transaction.RollbackAsync(ct);
            throw new AccessRequestUseCaseException(AccessRequestUseCaseError.Conflict, "The access request changed during this operation. Reload it and try again.", ex);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    private async Task<AccessActor> CreateActorAsync(ActingContext context, AccessRequest request, CancellationToken ct)
    {
        if (context.Role == AccessActorRole.Doctor)
            return new AccessActor(context.UserId, context.Role, request.RecipientPatientId);

        if (context.Role == AccessActorRole.Patient && context.OnBehalfOfPatientId is null)
        {
            if (context.UserId != request.RecipientPatientId)
                throw new AccessRequestUseCaseException(AccessRequestUseCaseError.Forbidden, "A patient can only manage their own access.");
            return new AccessActor(context.UserId, context.Role, request.RecipientPatientId);
        }

        if (context.Role == AccessActorRole.Guardian && context.OnBehalfOfPatientId == request.RecipientPatientId &&
            await guardianship.IsVerifiedGuardianAsync(context.UserId, request.RecipientPatientId, ct))
            return new AccessActor(context.UserId, context.Role, request.RecipientPatientId);

        throw new AccessRequestUseCaseException(AccessRequestUseCaseError.Forbidden, "The actor is not authorized to act for this patient.");
    }
}

public sealed class AcceptAccessRequestHandler(AccessRequestCommandService commands) : ICommandHandler<AcceptAccessRequest>
{
    public Task HandleAsync(AcceptAccessRequest command, CancellationToken ct = default) => commands.ExecuteAsync(command.RequestId, command.Actor,
        (request, actor, now) => request.Accept(actor, command.DurationType, command.CustomEndDate, now), ct);
}

public sealed class RejectAccessRequestHandler(AccessRequestCommandService commands) : ICommandHandler<RejectAccessRequest>
{
    public Task HandleAsync(RejectAccessRequest command, CancellationToken ct = default) => commands.ExecuteAsync(command.RequestId, command.Actor,
        (request, actor, now) => request.Reject(actor, now), ct);
}

public sealed class UndoRejectionHandler(AccessRequestCommandService commands) : ICommandHandler<UndoRejection>
{
    public Task HandleAsync(UndoRejection command, CancellationToken ct = default) => commands.ExecuteAsync(command.RequestId, command.Actor,
        (request, actor, now) => request.UndoRejection(actor, now), ct);
}

public sealed class CloseAccessByPatientHandler(AccessRequestCommandService commands) : ICommandHandler<CloseAccessByPatient>
{
    public Task HandleAsync(CloseAccessByPatient command, CancellationToken ct = default) => commands.ExecuteAsync(command.RequestId, command.Actor,
        (request, actor, now) => request.CloseByPatient(actor, command.Confirmed, now), ct);
}

public sealed class ExtendAccessHandler(AccessRequestCommandService commands) : ICommandHandler<ExtendAccess>
{
    public Task HandleAsync(ExtendAccess command, CancellationToken ct = default) => commands.ExecuteAsync(command.RequestId, command.Actor,
        (request, actor, now) => request.Extend(actor, command.NewEndDate, now), ct);
}

public sealed class CloseAccessByDoctorHandler(AccessRequestCommandService commands) : ICommandHandler<CloseAccessByDoctor>
{
    public Task HandleAsync(CloseAccessByDoctor command, CancellationToken ct = default) => commands.ExecuteAsync(command.RequestId, command.Actor,
        (request, actor, now) => request.CloseByDoctor(actor, command.Confirmed, now), ct);
}
