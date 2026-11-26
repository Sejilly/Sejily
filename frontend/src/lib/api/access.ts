/**
 * Access-request service.
 *
 * Right now this is backed by an in-memory mock so the whole UX from the
 * design can be exercised without a backend. To go live, replace the bodies
 * of these functions with `apiFetch(...)` calls (keep the signatures) — the
 * UI doesn't need to change.
 *
 *   listRequests()    -> GET    /api/Access/requests
 *   lookupPatient()   -> GET    /api/Access/patients/{nationalId}
 *   createRequest()   -> POST   /api/Access/requests
 *   endAccess()       -> POST   /api/Access/requests/{id}/end
 */
import { toLocalIso } from "@/features/access/format";
import type {
  AccessRequest,
  CreateRequestInput,
  CreateRequestResult,
  Doctor,
  PatientLookup,
  Workplace,
} from "@/features/access/types";

export const CURRENT_DOCTOR: Doctor = {
  name: "د. سارة أحمد",
  specialty: "الباطنة والجهاز الهضمي",
};

export const WORKPLACES: Workplace[] = [
  { id: "wp-nile", name: "مستشفى النيل العام" },
  { id: "wp-clinic", name: "عيادة د. سارة أحمد" },
  { id: "wp-shifa", name: "مستشفى دار الشفاء" },
];

const delay = (ms = 450) => new Promise((r) => setTimeout(r, ms));

/* ----------------------------- mock directory ----------------------------- */

type MockPatient = { last4: string; name: string };

const PATIENTS: MockPatient[] = [
  { last4: "1042", name: "منى ع*** إ*****" },
  { last4: "8831", name: "خالد م***** ح****" },
  { last4: "4519", name: "رحاب ع*** م****" },
  { last4: "6703", name: "عمرو ح*** ك****" },
  { last4: "9124", name: "سارة أ**** م*****" },
  { last4: "3351", name: "محمود ط*** ع*****" },
  { last4: "7782", name: "فاطمة ه*** س***" },
  { last4: "5094", name: "إبراهيم ع*** ق****" },
  { last4: "6215", name: "نور م***** ف*****" },
  { last4: "2589", name: "أحمد م*** ع***" },
  { last4: "7310", name: "ياسمين ح*** ب****" },
  { last4: "4476", name: "مصطفى س**** ر***" },
  { last4: "1908", name: "هبة ك*** ن*****" },
  { last4: "8265", name: "طارق و*** ج****" },
  { last4: "5537", name: "ليلى ز***** أ***" },
  { last4: "3092", name: "حسام د*** ل****" },
  { last4: "6841", name: "دينا ص**** ط***" },
  { last4: "2754", name: "كريم ف*** ع*****" },
  { last4: "9403", name: "آية ن***** م***" },
  { last4: "1167", name: "عادل ب*** ش****" },
  { last4: "7025", name: "رنا ق**** ي***" },
  { last4: "4388", name: "وليد ت*** ه****" },
  { last4: "5912", name: "سلمى ع***** خ***" },
  { last4: "3679", name: "زياد أ*** ح****" },
  { last4: "8150", name: "مريم ل*** ص*****" },
  { last4: "2243", name: "بسمة ر**** ك***" },
  { last4: "6598", name: "أسامة غ*** د****" },
  { last4: "1476", name: "جميلة ث***** ا***" },
  { last4: "7834", name: "هشام م**** ف***" },
  { last4: "4021", name: "إيمان ع*** س****" },
  { last4: "9265", name: "شريف ن*** ق*****" },
  { last4: "5348", name: "ندى ح**** و***" },
  { last4: "6120", name: "ياسر ع*** م****" },
  { last4: "8807", name: "علا ف**** ز***" },
];

/** Full (unmasked) national ID of a mock patient — demo only. */
const fullId = (last4: string) => `2990101100${last4}`; //ex: fullId("1042"); gives 29901011001042
const maskId = (last4: string) => `${"*".repeat(10)}${last4}`; //ex: maskId("1042"); gives **********1042
const byLast4 = (last4: string) => PATIENTS.find((p) => p.last4 === last4)!; //searches for patient with same last 4 numbers in id

function row(
  n: number,
  last4: string,
  workplace: string,
  requestedAt: string,
  status: AccessRequest["status"],
  type: AccessRequest["type"],
  endsInHours: number,
  extended = false,
): AccessRequest {
  return {
    id: `req-${n}`,
    patientName: byLast4(last4).name,
    nationalId: maskId(last4),
    workplace,
    requestedAt,
    endsAt: toLocalIso(new Date(Date.now() + endsInHours * 36e5)),
    status,
    type,
    extendedByPatient: extended,
  };
}

const NILE = "مستشفى النيل العام";
const CLINIC = "عيادة د. سارة أحمد";
const SHIFA = "مستشفى دار الشفاء";
const DAY = 24;

function seed(): AccessRequest[] {
  const rows: AccessRequest[] = [
    // first ten rows — exactly as in the design
    row(1, "1042", NILE, "2026-10-21T15:20", "active", "normal", 128 * DAY, true),
    row(2, "8831", CLINIC, "2026-10-18T11:30", "rejected", "normal", 0),
    row(3, "4519", SHIFA, "2026-10-08T13:15", "active", "emergency", 90 * DAY),
    row(4, "6703", NILE, "2026-10-10T16:50", "expired", "normal", -3 * DAY),
    row(5, "9124", CLINIC, "2026-10-18T11:30", "rejected", "normal", 0),
    row(6, "3351", CLINIC, "2026-10-22T10:45", "expired", "normal", -2 * DAY),
    row(7, "7782", NILE, "2026-10-10T16:50", "expired", "emergency", -5 * DAY),
    row(8, "5094", CLINIC, "2026-10-08T13:15", "pending", "normal", 33),
    row(9, "6215", NILE, "2026-10-22T10:45", "active", "normal", 100 * DAY),
    row(10, "2589", SHIFA, "2026-10-22T10:45", "active", "normal", 110 * DAY),
  ];

  // filler so the tab counters match the design: 32 / 12 / 10 / 6 / 4
  const places = [NILE, CLINIC, SHIFA];
  const plan: Array<[AccessRequest["status"], number]> = [
    ["active", 8],
    ["pending", 9],
    ["expired", 3],
    ["rejected", 2],
  ];
  let n = 11;
  let p = 10;
  for (const [status, count] of plan) {
    for (let i = 0; i < count; i++, n++, p++) {
      const day = 1 + ((n * 3) % 20);
      const hour = 8 + ((n * 7) % 11);
      const iso = `2026-10-${String(day).padStart(2, "0")}T${String(hour).padStart(2, "0")}:${n % 2 ? "15" : "40"}`;
      const ends =
        status === "active" ? (60 + n) * DAY :
        status === "pending" ? 4 + ((n * 5) % 44) :
        status === "expired" ? -n : 0;
      rows.push(
        row(n, PATIENTS[p % PATIENTS.length].last4, places[n % 3], iso, status, n % 6 === 0 ? "emergency" : "normal", ends),
      );
    }
  }
  return rows;
}

let store: AccessRequest[] = seed();

/* ------------------------------- public API ------------------------------- */

export async function listRequests(): Promise<AccessRequest[]> {
  await delay(350);
  return [...store];
}

export async function lookupPatient(nationalId: string): Promise<PatientLookup | null> {
  await delay(250);
  const p = PATIENTS.find((x) => fullId(x.last4) === nationalId);
  return p ? { nationalId, maskedName: p.name } : null;
}

export async function createRequest(
  input: CreateRequestInput,
): Promise<CreateRequestResult> {
  await delay(600);
  const patient = PATIENTS.find((x) => fullId(x.last4) === input.nationalId);
  if (!patient) return { ok: false, reason: "not_found" };

  const masked = maskId(patient.last4);
  const hasOpen = store.some(
    (r) => r.nationalId === masked && (r.status === "pending" || r.status === "active"),
  );
  if (hasOpen) return { ok: false, reason: "duplicate" };

  const workplace = WORKPLACES.find((w) => w.id === input.workplaceId);
  if (!workplace) return { ok: false, reason: "server" };

  const now = new Date();
  const request: AccessRequest = {
    id: `req-${Date.now()}`,
    patientName: patient.name,
    nationalId: masked,
    workplace: workplace.name,
    requestedAt: toLocalIso(now),
    endsAt: toLocalIso(new Date(now.getTime() + 48 * 36e5)),
    status: "pending",
    type: input.type,
  };
  store = [request, ...store];
  return { ok: true, request };
}

export async function endAccess(id: string): Promise<AccessRequest | null> {
  await delay(500);
  let updated: AccessRequest | null = null;
  store = store.map((r) => {
    if (r.id !== id) return r;
    updated = { ...r, status: "expired", endsAt: toLocalIso(new Date()) };
    return updated;
  });
  return updated;
}

/** Demo helper: the full national ID of a mock patient (last 4 digits). */
export const DEMO_IDS = {
  free: fullId("3351"), // only an expired request -> goes through (as in the design)
  hasOpen: fullId("5094"), // pending already -> duplicate error
};
