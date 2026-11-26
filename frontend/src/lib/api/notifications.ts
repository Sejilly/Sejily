/**
 * Notifications service — in-memory mock for now (same approach as access.ts).
 *
 *   listNotifications() -> GET  /api/Notifications
 *   markRead(id)        -> POST /api/Notifications/{id}/read
 */
import type { AppNotification } from "@/features/notifications/types";

const delay = (ms = 250) => new Promise((r) => setTimeout(r, ms));

const MIN = 6e4;
const HOUR = 36e5;
const NAME = "أحمد م*** ح**";

function seed(): AppNotification[] {
  const now = Date.now();
  const n = (
    id: string,
    kind: AppNotification["kind"],
    ago: number,
    read: boolean,
  ): AppNotification => ({ id, kind, patientName: NAME, createdAt: now - ago, read });

  return [
    n("n1", "accepted", 12 * MIN, false),
    n("n2", "not_accepted", 4 * HOUR, true),
    n("n3", "ended", 23 * HOUR, false),
    n("n4", "expired", 36 * HOUR, false),
    n("n5", "extended", 46 * HOUR, true),
    n("n6", "accepted", 5 * 24 * HOUR, true),
  ];
}

let store: AppNotification[] | null = null;

export async function listNotifications(): Promise<AppNotification[]> {
  await delay();
  store ??= seed();
  return [...store];
}

export async function markRead(id: string): Promise<void> {
  await delay(100);
  store = (store ?? []).map((x) => (x.id === id ? { ...x, read: true } : x));
}
