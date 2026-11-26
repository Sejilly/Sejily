"use client";

import { useEffect, useRef, type ReactNode } from "react";
import { CloseIcon } from "@/components/icons";
import { cn } from "@/lib/cn";

type Props = {
  open: boolean;
  onClose: () => void;
  title: string;
  description?: string;
  children: ReactNode;
  className?: string;
  /** Block closing (backdrop / Esc) while something is submitting */
  locked?: boolean;
};

/** Centered dialog with backdrop, Esc-to-close and focus handling. */
export function Modal({ open, onClose, title, description, children, className, locked }: Props) {
  const panelRef = useRef<HTMLDivElement>(null);

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
    panelRef.current?.focus();
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
    <div className="fixed inset-0 z-50 flex items-end justify-center sm:items-center sm:p-4">
      <div
        className="sj-fade-in absolute inset-0 bg-ink/50"
        onClick={() => !locked && onClose()}
        aria-hidden
      />
      <div
        ref={panelRef}
        role="dialog"
        aria-modal="true"
        aria-labelledby="modal-title"
        tabIndex={-1}
        className={cn(
          "sj-modal-in relative max-h-[92dvh] w-full overflow-y-auto rounded-t-2xl bg-white p-5 pb-[max(1.25rem,env(safe-area-inset-bottom))] shadow-xl outline-none sm:max-h-[calc(100dvh-2rem)] sm:max-w-120 sm:rounded-2xl sm:p-6",
          className,
        )}
      >
        <div className="flex items-start justify-between gap-4">
          <div className="space-y-2">
            <h2 id="modal-title" className="text-xl font-semibold text-ink">{title}</h2>
            {description && <p className="text-sm text-secondary font-normal">{description}</p>}
          </div>
          <button
            type="button"
            onClick={onClose}
            disabled={locked}
            aria-label="إغلاق"
            className="-m-1 rounded-md p-1 text-secondary hover:bg-surface disabled:opacity-50"
          >
            <CloseIcon className="size-5" />
          </button>
        </div>
        <div className="mt-5 sm:mt-6">{children}</div>
      </div>
    </div>
  );
}