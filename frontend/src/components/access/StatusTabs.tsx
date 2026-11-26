"use client";

import type { StatusFilter } from "@/features/access/types";
import { cn } from "@/lib/cn";

const TABS: Array<{ id: StatusFilter; label: string }> = [
  { id: "all", label: "الكل" },
  { id: "active", label: "نشطة" },
  { id: "pending", label: "معلقة" },
  { id: "expired", label: "منتهية" },
  { id: "rejected", label: "لم تُقبل" },
];

type Props = {
  value: StatusFilter;
  counts: Record<StatusFilter, number>;
  onChange: (v: StatusFilter) => void;
};

export function StatusTabs({ value, counts, onChange }: Props) {
  return (
    <div role="tablist" aria-label="حالة الطلب" className="flex  border-b border-border-disabled sm:gap-1">
      {TABS.map((t) => {
        const selected = value === t.id;
        return (
          <button
            key={t.id}
            role="tab"
            type="button"
            aria-selected={selected}
            onClick={() => onChange(t.id)}
            className={cn(
              "-mb-px flex min-w-0 flex-1 flex-col items-center justify-center gap-1 border-b-2 px-1 pb-2.5  transition-colors sm:flex-none sm:flex-row sm:gap-2 sm:px-3 sm:pb-3 sm:text-sm",
              selected
                ? "border-brand-blue border-b-[3px] text-base font-semibold text-brand-blue"
                : "border-transparent text-secondary hover:text-ink font-normal text-base",
            )}
          >
            <span className="whitespace-nowrap">{t.label}</span>
            <span
              className={cn(
                "min-w-7 rounded-md px-1.5 py-0.5 text-center text-base font-normal",
                selected ? "bg-[#F0F5F9] text-brand-blue" : "bg-surface text-disabled",
              )}
            >
              {counts[t.id]}
            </span>
          </button>
        );
      })}
    </div>
  );
}
