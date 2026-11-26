# Access Requests API — F6.6, F6.8 and F6.10

All routes require `Authorization: Bearer <JWT>`. Enums are JSON strings and all
timestamps are UTC ISO-8601 values. Date-only inputs use `YYYY-MM-DD`.

## Integration boundary

The F6.1 flow owned by the other team creates the Pending `AccessRequest` and
opens this flow. F6.6/F6.8/F6.10 intentionally do not create requests; they
only list, decide, manage, and enforce access for that persisted request.

## Patient or verified guardian acting for a dependent

A guardian must first use the existing account-switch flow so the JWT's active
account is the dependent. The server uses the token claims and verified
guardian relationship; the client never submits an actor ID.

| Method | Route | Body |
| --- | --- | --- |
| GET | `/api/access-requests/patient` | — |
| GET | `/api/access-requests/patient/{requestId}` | — |
| POST | `/api/access-requests/patient/{requestId}/accept` | `{ "durationType": "Permanent" }`, `{ "durationType": "TreatmentDuration" }`, or `{ "durationType": "Custom", "customEndDate": "2026-12-10" }` |
| POST | `/api/access-requests/patient/{requestId}/reject` | `{ "confirmed": true }` |
| POST | `/api/access-requests/patient/{requestId}/rejection/undo` | — |
| POST | `/api/access-requests/patient/{requestId}/close` | `{ "confirmed": true }` |
| POST | `/api/access-requests/patient/{requestId}/extend` | `{ "newEndDate": "2026-12-10" }` |

Omitting `durationType` on acceptance selects `TreatmentDuration`. `Custom`
requires `customEndDate`. `confirmed` must be true for reject and close.

Patient responses contain the effective `status`, UTC `requestExpiresAt`, and
`rejectionUndoUntil` so timers can be rebuilt after an app restart. The
`canAccept`, `canReject`, `canUndoRejection`, `canClose`, and `canExtend` flags
are authoritative UI-action flags; the server still revalidates every action.

## Verified doctor

| Method | Route | Body |
| --- | --- | --- |
| GET | `/api/access-requests/doctor` | — |
| GET | `/api/access-requests/doctor/{requestId}` | — |
| POST | `/api/access-requests/doctor/{requestId}/close` | `{ "confirmed": true }` |

Doctor responses expose only `displayStatus`: `Pending`, `Active`, `Ended`, or
`NotAccepted`. Rejected and request-expired states are deliberately not
distinguishable to the doctor. Patient identity is limited to a masked name and
the final four National ID digits. `hasActiveAccess` is the current server-side
answer for whether access is permitted; it must never replace a server-side
authorization check for medical-record operations.

## Action errors

Action endpoints return RFC 7807 problem details. The frontend should reload
the request on `409 Conflict`, display `400` validation errors, and treat `410
Gone` as an expired request. `403` indicates the authenticated account is not
allowed to act on that request.

