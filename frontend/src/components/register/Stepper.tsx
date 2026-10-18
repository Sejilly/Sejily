"use client";

import { usePathname } from "next/navigation";
import { cn } from "@/lib/cn";
import { CheckIcon } from "@/components/icons";
import { STEPS, VERIFYING_HREF } from "@/features/register/constants";

function useCurrentStep() {
  const pathname = usePathname();
  if (pathname.startsWith(VERIFYING_HREF)) return STEPS.length - 1;
  const i = STEPS.findIndex((s) => pathname.startsWith(s.href));
  return i < 0 ? 0 : i;
}

function Circle({ index, current, small }: { index: number; current: number; small?: boolean }) {
  const done = index < current;
  const active = index === current;
  return (
    <span
      className={cn(
        "grid shrink-0 place-items-center rounded-full font-semibold",
        small ? "size-10 text-sm" : "size-11 text-xl",
        done || active ? "bg-brand-blue text-white text-xl" : "bg-border-disabled text-secondary",
      )}
    >
      {done ? <CheckIcon width={44} height={44} /> : index + 1}
    </span>
  );
}

export function Stepper() {
  const current = useCurrentStep();

  return (
    <nav aria-label="خطوات إنشاء الحساب">
      {/* Mobile / tablet: compact horizontal progress */}
      <div className="lg:hidden">
        <ol className="flex items-center">
          {STEPS.map((s, i) => (
            <li
              key={s.key}
              aria-current={i === current ? "step" : undefined}
              className="flex flex-1 items-center last:flex-none"
            >
              <Circle index={i} current={current} small />
              {i < STEPS.length - 1 && (
                <span
                  aria-hidden
                  className={cn("mx-3 h-0.5 flex-1 rounded", i < current ? "bg-brand-blue" : "bg-line")}
                />
              )}
              <span className="sr-only">{s.label}</span>
            </li>
          ))}
        </ol>
        <p className="mt-3 text-sm font-semibold text-ink">
          {`الخطوة ${current + 1} من ${STEPS.length} · ${STEPS[current].label}`}
        </p>
      </div>

      {/* Desktop: vertical stepper */}
      <ol className="hidden lg:block">
        {STEPS.map((s, i) => (
          <li key={s.key} aria-current={i === current ? "step" : undefined}>
            <div className="flex items-center gap-4">
              <Circle index={i} current={current} />
              <span
                className={cn(
                  "text-base",
                  i === current ? "font-semibold text-ink" : i < current ? "font-medium text-ink" : "text-disabled",
                )}
              >
                {s.label}
              </span>
            </div>
            {i < STEPS.length - 1 && (
              <div
                aria-hidden
                className={cn("my-3 ms-[19px] h-19 w-0.5 ", i < current ? "bg-brand-blue" : "bg-line")}
              />
            )}
          </li>
        ))}
      </ol>
    </nav>
  );
}
