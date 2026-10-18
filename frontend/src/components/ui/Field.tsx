import type { ReactNode } from "react";

export function Field({
  id,
  label,
  required,
  optional,
  error,
  children,
}: {
  id: string;
  label: string;
  required?: boolean;
  optional?: boolean;
  error?: string;
  children: ReactNode;
}) {
  return (
    <div className="space-y-2">
      <label htmlFor={id} className="block text-sm font-normal text-ink">
        {label}
        {required && <span className="ms-1 text-danger" aria-hidden>*</span>}
        {optional && <span className="ms-1 font-normal text-muted">(اختياري)</span>}
      </label>
      {children}
      {error && (
        <p id={`${id}-error`} role="alert" className="text-xs text-danger">
          {error}
        </p>
      )}
    </div>
  );
}