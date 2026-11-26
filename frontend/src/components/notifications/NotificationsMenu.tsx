"use client";

import { useEffect, useMemo, useRef, useState, type ComponentType } from "react";
import {
  LuAlarmClockPlus,
  LuBan,
  LuBell,
  LuCalendarClock,
  LuShieldCheck,
  LuTimerOff,
} from "react-icons/lu";
import {
  NOTIFICATION_COPY,
  groupNotifications,
  relativeTime,
} from "@/features/notifications/format";
import type { AppNotification, NotificationKind } from "@/features/notifications/types";
import { listNotifications, markRead } from "@/lib/api/notifications";
import { cn } from "@/lib/cn";

const VISUAL: Record<
  NotificationKind,
  { icon: ComponentType<{ className?: string }>; wrap: string; color: string }
> = {
  accepted: { icon: LuShieldCheck, wrap: "bg-[#e3f3ea]", color: "text-[#1f8a5b]" },
  not_accepted: { icon: LuTimerOff, wrap: "bg-[#e6ecf3]", color: "text-secondary" },
  ended: { icon: LuBan, wrap: "bg-[#fde4e4]", color: "text-be-error" },
  expired: { icon: LuCalendarClock, wrap: "bg-[#fdf2cf]", color: "text-[#d28a0a]" },
  extended: { icon: LuAlarmClockPlus, wrap: "bg-[#e2edf8]", color: "text-brand-blue" },
};

export function NotificationsMenu() {
  const [open, setOpen] = useState(false);
  const [items, setItems] = useState<AppNotification[] | null>(null);
  const [now, setNow] = useState(0);
  const ref = useRef<HTMLDivElement>(null);

  useEffect(() => {
    let alive = true;
    listNotifications().then((list) => {
      if (!alive) return;
      setItems(list);
      setNow(Date.now());
    });
    return () => {
      alive = false;
    };
  }, []);

  useEffect(() => {
    if (!open) return;
    const onDown = (e: MouseEvent) => {
      if (!ref.current?.contains(e.target as Node)) setOpen(false);
    };
    const onKey = (e: KeyboardEvent) => e.key === "Escape" && setOpen(false);
    document.addEventListener("mousedown", onDown);
    document.addEventListener("keydown", onKey);
    return () => {
      document.removeEventListener("mousedown", onDown);
      document.removeEventListener("keydown", onKey);
    };
  }, [open]);

  const groups = useMemo(() => (items ? groupNotifications(items, now) : []), [items, now]);
  const unread = items?.filter((n) => !n.read).length ?? 0;

  const read = (id: string) => {
    setItems((prev) => prev?.map((n) => (n.id === id ? { ...n, read: true } : n)) ?? prev);
    markRead(id);
  };

  return (
    <div ref={ref} className="relative">
      <button
        type="button"
        onClick={() => {
          setNow(Date.now());
          setOpen((o) => !o);
        }}
        aria-label={unread ? `الإشعارات (${unread} غير مقروءة)` : "الإشعارات"}
        aria-haspopup="dialog"
        aria-expanded={open}
        className="relative grid size-9 place-items-center rounded-full border border-border-disabled bg-white text-secondary hover:bg-surface"
      >
        <LuBell className="size-4.5" />
        {unread > 0 && <span className="absolute end-2 top-2 size-1.5 rounded-full bg-be-error" aria-hidden />}
      </button>

      {open && (
        <div
          role="dialog"
          aria-label="الإشعارات"
          className="sj-pop-in fixed inset-x-4 top-16 z-40 max-h-[75dvh] overflow-y-auto rounded-2xl border border-border-disabled bg-white shadow-xl sm:absolute sm:inset-x-auto sm:top-11 sm:left-0 sm:w-107.5"
        >
          {items === null ? (
            <div className="space-y-4 p-5">
              {[0, 1, 2].map((i) => (
                <div key={i} className="flex gap-3">
                  <div className="size-10 animate-pulse rounded-full bg-border-disabled" />
                  <div className="flex-1 space-y-2">
                    <div className="h-3 w-1/2 animate-pulse rounded bg-border-disabled" />
                    <div className="h-3 w-3/4 animate-pulse rounded bg-border-disabled" />
                  </div>
                </div>
              ))}
            </div>
          ) : groups.length === 0 ? (
            <p className="px-5 py-12 text-center text-sm text-secondary">لا توجد إشعارات حالياً.</p>
          ) : (
            groups.map((g) => (
              <section key={g.label} className="px-5 pt-5 last:pb-3">
                <h3 className="mb-2 text-xl font-semibold text-ink">{g.label}</h3>
                <ul className="divide-y divide-border-disabled">
                  {g.items.map((n) => {
                    const v = VISUAL[n.kind];
                    const Icon = v.icon;
                    const copy = NOTIFICATION_COPY[n.kind];
                    return (
                      <li key={n.id}>
                        <button
                          type="button"
                          onClick={() => read(n.id)}
                          className="flex w-full items-start gap-3 py-3.5 text-start hover:bg-surface/70 focus-visible:outline-2 focus-visible:outline-brand-blue"
                        >
                                                    <span
                            className={cn("mt-4 size-2 shrink-0 rounded-full", n.read ? "bg-transparent" : "bg-be-error")}
                            aria-label={n.read ? undefined : "غير مقروء"}
                          />
                          <span className={cn("grid size-10 shrink-0 place-items-center rounded-full", v.wrap)}>
                            <Icon className={cn("size-5", v.color)} />
                          </span>
                          <span className="min-w-0 flex-1 space-y-1">
                            <span className="flex items-start justify-between gap-3">
                              <span className="text-sm font-normal text-ink">{copy.title}</span>
                              <span className="shrink-0 text-xs text-secondary">
                                {relativeTime(n.createdAt, now)}
                              </span>
                            </span>
                            <span className="block text-xs leading-5 text-secondary">
                              {copy.body(n.patientName)}
                            </span>
                          </span>

                        </button>
                      </li>
                    );
                  })}
                </ul>
              </section>
            ))
          )}
        </div>
      )}
    </div>
  );
}
