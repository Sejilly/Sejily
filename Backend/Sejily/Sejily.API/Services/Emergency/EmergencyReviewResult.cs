namespace Sejily.API.Services.Emergency;

// Result of confirm / deny / undo. The controller maps each value to 200 / 404 / 409.
public enum EmergencyReviewResult
{
    Ok,
    NotFound,
    AlreadyResponded,
    ResponseWindowClosed,
    NothingToUndo,
    UndoWindowClosed,
    ConcurrentUpdate
}
