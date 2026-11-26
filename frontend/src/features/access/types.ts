export type AccessStatus = "active" | "pending" | "expired" | "rejected";
export type AccessType = "normal" | "emergency";
export type StatusFilter = "all" | AccessStatus;

export type AccessRequest = {
  id: string;
  patientName: string; // already masked by the backend
  nationalId: string; // already masked, only the last 4 digits are visible
  workplace: string;
  /** Local wall-clock ISO string, e.g. "2026-10-21T15:20" */
  requestedAt: string;
  /** When the request/access ends (expiry for active, deadline for pending) */
  endsAt: string;
  status: AccessStatus;
  type: AccessType;
  /** Patient extended the access (shows a notice for active requests) */
  extendedByPatient?: boolean;
};

export type Workplace = { id: string; name: string };

export type Doctor = { name: string; specialty: string };

export type PatientLookup = { nationalId: string; maskedName: string };

export type CreateRequestInput = {
  workplaceId: string;
  type: AccessType;
  nationalId: string;
};

export type CreateRequestResult =
  | { ok: true; request: AccessRequest }
  | { ok: false; reason: "duplicate" | "not_found" | "server" };
