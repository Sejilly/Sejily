const MONTHS = [
  "يناير", "فبراير", "مارس", "أبريل", "مايو", "يونيو",
  "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر",
];

const pad = (n: number) => String(n).padStart(2, "0");

function time12(d: Date) {
  const h = d.getHours();
  const suffix = h >= 12 ? "م" : "ص";
  const h12 = h % 12 === 0 ? 12 : h % 12;
  return `${pad(h12)}:${pad(d.getMinutes())} ${suffix}`;
}

/** 21/10/2026 - 03:20 م  (Latin digits, as in the design) */
export function formatTableDate(iso: string) {
  const d = new Date(iso);
  return `${pad(d.getDate())}/${pad(d.getMonth() + 1)}/${d.getFullYear()} - ${time12(d)}`;
}

/** 23 أكتوبر 2026 · 09:15 ص */
export function formatLongDate(iso: string) {
  const d = new Date(iso);
  return `${d.getDate()} ${MONTHS[d.getMonth()]} ${d.getFullYear()} · ${time12(d)}`;
}

export function hoursLeft(endsAtIso: string, now = Date.now()) {
  const ms = new Date(endsAtIso).getTime() - now;
  return Math.max(0, Math.ceil(ms / 36e5));
}

export function toLocalIso(d: Date) {
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
}
