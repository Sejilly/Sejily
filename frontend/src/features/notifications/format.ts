import type { AppNotification, NotificationKind } from "./types";

export const NOTIFICATION_COPY: Record<
  NotificationKind,
  { title: string; body: (patient: string) => string }
> = {
  accepted: {
    title: "قبول طلب وصولك",
    body: (p) => `المريض [${p}] وافق على طلب وصولك.`,
  },
  not_accepted: {
    title: "لم يتم القبول لطلب الوصول",
    body: (p) => `انتهت مهلة الـ 48 ساعة لطلب الوصول للمريض [${p}] دون رد.`,
  },
  ended: {
    title: "تم إنهاء الوصول النشط",
    body: (p) => `تم إنهاء الوصول مع المريض [${p}].`,
  },
  expired: {
    title: "انتهت فترة الوصول",
    body: (p) => `انتهت فترة الوصول بينك وبين المريض [${p}].`,
  },
  extended: {
    title: "تم تمديد فترة الوصول",
    body: (p) => `قام المريض [${p}] بتمديد صلاحية الوصول بينكما.`,
  },
};

/** Arabic count + noun: 1 -> "ساعة", 2 -> "ساعتان"/"ساعتين", 3-10 -> plural, 11+ -> singular */
function counted(n: number, one: string, two: string, few: string, many: string) {
  if (n === 1) return one;
  if (n === 2) return two;
  if (n >= 3 && n <= 10) return `${n} ${few}`;
  return `${n} ${many}`;
}

const minutes = (n: number) => counted(n, "دقيقة", "دقيقتين", "دقائق", "دقيقة");
const hours = (n: number) => counted(n, "ساعة", "ساعتين", "ساعات", "ساعة");
const days = (n: number) => counted(n, "يوم", "يومين", "أيام", "يوم");

export function relativeTime(createdAt: number, now = Date.now()) {
  const mins = Math.max(0, Math.floor((now - createdAt) / 6e4));
  if (mins < 1) return "الآن";
  if (mins < 60) return `منذ ${minutes(mins)}`;
  const h = Math.floor(mins / 60);
  if (h < 24) return `منذ ${hours(h)}`;
  const d = Math.floor(h / 24);
  const restH = h % 24;
  if (d < 2) return restH ? `منذ يوم و${hours(restH)}` : "منذ يوم";
  return `منذ ${days(d)}`;
}

export type NotificationGroup = { label: string; items: AppNotification[] };

/** Groups by age: under 24h = today, 24–48h = yesterday, older = last week. */
export function groupNotifications(list: AppNotification[], now = Date.now()): NotificationGroup[] {
  const sorted = [...list].sort((a, b) => b.createdAt - a.createdAt);
  const buckets: NotificationGroup[] = [
    { label: "اليوم", items: [] },
    { label: "أمس", items: [] },
    { label: "الأسبوع الماضي", items: [] },
  ];
  for (const n of sorted) {
    const h = (now - n.createdAt) / 36e5;
    buckets[h < 24 ? 0 : h < 48 ? 1 : 2].items.push(n);
  }
  return buckets.filter((b) => b.items.length > 0);
}
