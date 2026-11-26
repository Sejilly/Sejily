"use client";

import { useEffect, useRef, type ReactNode } from "react";
import { CloseIcon } from "@/components/icons";

type Props = {
  open: boolean;
  onClose: () => void;
  title: string;
  badge?: ReactNode;
  children: ReactNode;
  footer?: ReactNode;
  /** Rendered on top of the drawer content (e.g. confirmation sheet) */
  overlay?: ReactNode;
  locked?: boolean;
};

/** Side panel that slides in from the physical right edge, as in the design. */
export function Drawer({ open, onClose, title, badge, children, footer, overlay, locked }: Props) {
  const ref = useRef<HTMLElement>(null);

  // Always call the latest onClose/locked without re-running the effects below.
  const latest = useRef({ onClose, locked });
  useEffect(() => {
    latest.current = { onClose, locked };
  });

  // Scroll lock + initial focus: only when opening, NOT on every re-render
  // (otherwise typing in an input would steal focus back to the panel).
  useEffect(() => {
    if (!open) return;
    const prev = document.body.style.overflow;
    document.body.style.overflow = "hidden";
    ref.current?.focus();
    return () => {
      document.body.style.overflow = prev;
    };
  }, [open]);

  useEffect(() => {
    if (!open) return;
    const onKey = (e: KeyboardEvent) => {
      if (e.key === "Escape" && !latest.current.locked) latest.current.onClose();
    };
    document.addEventListener("keydown", onKey);
    return () => document.removeEventListener("keydown", onKey);
  }, [open]);

  if (!open) return null;

  return (
    <div className="fixed inset-0 z-50">
      <div className="sj-fade-in absolute inset-0 bg-ink/40" onClick={() => !locked && onClose()} aria-hidden />
      <aside
        ref={ref}
        role="dialog"
        aria-modal="true"
        aria-labelledby="drawer-title"
        tabIndex={-1}
        dir="rtl"
        className="sj-drawer-in absolute inset-y-0 right-0 flex h-dvh w-full max-w-100 flex-col overflow-hidden bg-white shadow-2xl outline-none"
      >
        <header className="flex items-center justify-between gap-3 px-5 pt-6 pb-4">
          <div className="flex items-center gap-3 ">
            <h2 id="drawer-title" className="text-xl font-semibold text-black">{title}</h2>
            {badge}
          </div>
          <button
            type="button"
            onClick={onClose}
            disabled={locked}
            aria-label="إغلاق"
            className="rounded-md p-1 text-secondary hover:bg-surface disabled:opacity-50"
          >
            <CloseIcon className="size-5" />
          </button>
        </header>
        <div className="flex-1 space-y-4 overflow-y-auto px-5 pb-4">{children}</div>
        {footer && <footer className="space-y-3 px-5 pt-2 pb-[max(1.5rem,env(safe-area-inset-bottom))]">{footer}</footer>}
        {overlay}
      </aside>
    </div>
  );
}
