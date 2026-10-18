import type { SelectHTMLAttributes } from "react";
import { cn } from "@/lib/cn";
import { TriangleDownIcon } from "@/components/icons";
import { shellClass } from "./TextInput";

type Props = SelectHTMLAttributes<HTMLSelectElement> & {
  options: { value: string; label: string }[];
  placeholder: string;
  invalid?: boolean;
};

export function Select({ options, placeholder, invalid, className, ...rest }: Props) {
  return (
    <div className={shellClass(invalid, rest.disabled)}>
      <select
        aria-invalid={invalid || undefined}
        aria-describedby={invalid && rest.id ? `${rest.id}-error` : undefined}
        className={cn(
          "h-full min-w-0 flex-1 cursor-pointer appearance-none bg-transparent px-4 text-sm text-disabled outline-none",
          rest.value ? "text-ink" : "text-muted",
          className,
        )}
        {...rest}
      >
        <option value="" disabled>{placeholder}</option>
        {options.map((o) => (
          <option key={o.value} value={o.value} className="text-ink">{o.label}</option>
        ))}
      </select>
      <div className="pointer-events-none flex h-6 items-center border-s border-line px-3 text-muted">
        <TriangleDownIcon  />
      </div>
    </div>
  );
}
