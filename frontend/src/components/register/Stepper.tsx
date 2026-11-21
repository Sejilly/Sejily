
"use client";

import { usePathname } from "next/navigation";
import { cn } from "@/lib/cn";
import { CheckIcon, ExcIcon } from "@/components/icons";
import { STEPS, VERIFYING_HREF } from "@/features/register/constants";
import { useRegister } from "@/features/register/RegisterContext";

function useCurrentStep() {
  const pathname = usePathname();

  if (pathname.startsWith(VERIFYING_HREF)) {
    return STEPS.length - 1;
  }

  const i = STEPS.findIndex((s) => pathname.startsWith(s.href));
  return i < 0 ? 0 : i;
}

function Circle({
  index,
  current,
  small,
  error = false,
}: {
  index: number;
  current: number;
  small?: boolean;
  error?: boolean;
}) {
  const done = index < current;
  const active = index === current;

  return (
    <span
      className={cn(
        "grid shrink-0 place-items-center rounded-full font-semibold",
        small ? "size-10 text-sm" : "size-11 text-xl",
        error
          ? ""
          : active
            ? "bg-brand-blue text-white"
            : done
              ? "bg-focus-ring text-white"
              : "bg-border-disabled text-secondary",
      )}
      aria-label={error ? `الخطوة ${index + 1} بها خطأ` : undefined}
    >
{error ? (
<ExcIcon
  className="shrink-0"
/>
) : done ? (
  <CheckIcon width={44} height={44} className="text-white" />
) : (
  index + 1
)}
    </span>
  );
}

export function Stepper() {
  const current = useCurrentStep();
  const { duplicateField } = useRegister();

  const errorHref =
    duplicateField === "nationalID"
      ? "/register/personal"
      : duplicateField === "syndicateCardNumber" ||
          duplicateField === "medicalLicenseNumber"
        ? "/register/professional"
        : null;

  const errorStep = errorHref
    ? STEPS.findIndex((step) => errorHref.startsWith(step.href))
    : -1;

  return (
    <nav aria-label="خطوات إنشاء الحساب">
      {/* Mobile / tablet */}
      <div className="lg:hidden">
        <ol className="flex items-center">
          {STEPS.map((s, i) => (
            <li
              key={s.key}
              aria-current={i === current ? "step" : undefined}
              className="flex flex-1 items-center last:flex-none"
            >
              <Circle
                index={i}
                current={current}
                small
                error={i === errorStep}
              />

              {i < STEPS.length - 1 && (
                <span
                  aria-hidden
                  className={cn(
                    "mx-3 h-0.5 flex-1 rounded",
                    i < current ? "bg-brand-blue" : "bg-line",
                  )}
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

      {/* Desktop */}
      <ol className="hidden lg:block">
        {STEPS.map((s, i) => (
          <li key={s.key} aria-current={i === current ? "step" : undefined}>
            <div className="flex items-center gap-4">
              <Circle
                index={i}
                current={current}
                error={i === errorStep}
              />

              <span
                className={cn(
                  "text-base",
                  i === errorStep
                    ? "font-semibold text-be-error"
                    : i === current
                      ? "font-semibold text-ink"
                      : i < current
                        ? "font-medium text-ink"
                        : "text-disabled",
                )}
              >
                {s.label}
              </span>
            </div>

            {i < STEPS.length - 1 && (
              <div
                aria-hidden
                className={cn(
                  "my-3 ms-[19px] h-19 w-0.5",
                  i < current ? "bg-brand-blue" : "bg-line",
                )}
              />
            )}
          </li>
        ))}
      </ol>
    </nav>
  );
}
