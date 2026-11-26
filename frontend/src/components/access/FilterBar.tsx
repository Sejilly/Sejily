"use client";

import { useEffect, useRef, useState } from "react";
import { LuChevronDown, LuSearch } from "react-icons/lu";
import { cn } from "@/lib/cn";

export type Option = { value: string; label: string };

function FilterMenu({
  label,
  value,
  options,
  onChange,
}: {
  label: string;
  value: string;
  options: Option[];
  onChange: (v: string) => void;
}) {
  const [open, setOpen] = useState(false);
  const ref = useRef<HTMLDivElement>(null);

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

  const active = options.find((o) => o.value === value);
  const isSet = value !== "";

  return (
    <div ref={ref} className="relative">
      <button
        type="button"
        aria-haspopup="listbox"
        aria-expanded={open}
        onClick={() => setOpen((o) => !o)}
        className={cn(
          "flex h-10 items-center gap-2 px-3 text-sm whitespace-nowrap hover:bg-surface",
          isSet ? "font-semibold text-brand-blue" : "text-secondary",
        )}
      >
        {isSet && active ? active.label : label}
        <LuChevronDown className="size-4 text-disabled" />
      </button>
      {open && (
        <ul
          role="listbox"
          className="sj-pop-in absolute start-0 top-11 z-20 min-w-44 overflow-hidden rounded-lg border border-border-disabled bg-white py-1 shadow-lg"
        >
          {[{ value: "", label: `${label}: الكل` }, ...options].map((o) => (
            <li key={o.value || "all"} role="option" aria-selected={o.value === value}>
              <button
                type="button"
                onClick={() => {
                  onChange(o.value);
                  setOpen(false);
                }}
                className={cn(
                  "block w-full px-4 py-2 text-start text-xs hover:bg-surface",
                  o.value === value ? "font-semibold text-brand-blue" : "text-ink",
                )}
              >
                {o.label}
              </button>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}

export type Filters = { query: string; sort: string; workplace: string; type: string };

type Props = {
  filters: Filters;
  workplaces: string[];
  onChange: (patch: Partial<Filters>) => void;
};

export function FilterBar({ filters, workplaces, onChange }: Props) {
  return (
    <div className="flex flex-wrap items-center justify-between gap-3">
      <label className="relative block w-full sm:w-72">
        <span className="sr-only">بحث باسم المريض</span>
        <LuSearch className="pointer-events-none absolute start-3 top-1/2 size-4.5 -translate-y-1/2 text-disabled" />
        <input
          type="search"
          value={filters.query}
          onChange={(e) => onChange({ query: e.target.value })}
          placeholder="ابحث بإسم المريض..."
          className="h-10 w-full rounded-lg border font-normal border-line bg-white ps-10 pe-3 text-sm text-ink outline-none placeholder:text-disabled focus:border-brand-blue focus:ring-2 focus:ring-brand-blue/20"
        />
      </label>

      <div className="flex divide-x divide-line  rounded-lg border border-line bg-white">
        <FilterMenu
          label="تاريخ الإرسال"
          value={filters.sort}
          onChange={(sort) => onChange({ sort })}
          options={[
            { value: "newest", label: "الأحدث أولاً" },
            { value: "oldest", label: "الأقدم أولاً" },
          ]}
        />
        <FilterMenu
          label="جهة العمل"
          value={filters.workplace}
          onChange={(workplace) => onChange({ workplace })}
          options={workplaces.map((w) => ({ value: w, label: w }))}
        />
        <FilterMenu
          label="نوع الوصول"
          value={filters.type}
          onChange={(type) => onChange({ type })}
          options={[
            { value: "normal", label: "عادي" },
            { value: "emergency", label: "طوارئ" },
          ]}
        />
      </div>
    </div>
  );
}
