import type { ReactNode } from "react";

export function FormHeader({
  title,
  hint,
  description,
  action,
}: {
  title: string;
  /** Muted text beside the title,for example "( 1 جهة عمل مُضافة )" */
  hint?: string;
  description: string;
  /** Button shown at the inline-end (left in RTL) */
  action?: ReactNode;
}) {
  return (
    <div className="mb-9 flex flex-wrap items-start justify-between gap-3">
      <div>
        <h2 className="text-xl font-semibold text-ink">
          {title}
          {hint && <span className="ms-2  text-sm font-normal text-disabled">{hint}</span>}
        </h2>
        <p className="mt-2 text-sm font-normal text-ink">{description}</p>
      </div>
      {action}
    </div>
  );
}