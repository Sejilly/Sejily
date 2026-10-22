import type { InputHTMLAttributes, ReactNode } from "react";
import { BsExclamationCircleFill } from "react-icons/bs";

export type TextFieldProps = InputHTMLAttributes<HTMLInputElement> & {
    id: string;
    label: string;
    leading?: ReactNode;
    trailing?: ReactNode;
    error?: string;
    visualError?: boolean;
    action?: ReactNode;
};

export default function TextField({
    id,
    label,
    leading,
    trailing,
    error,
    visualError = false,
    action,
    className = "",
    ...inputProps
}: TextFieldProps) {
    const errorId = `${id}-error`;
    const hasError = Boolean(error) || visualError;

    return (
        <div className="flex w-full flex-col items-start gap-1">
            <label
                htmlFor={id}
                className="w-full text-right text-sm leading-5 text-ink"
            >
                {label}
            </label>

            <div
                className={`group flex h-12.5 w-full items-center gap-2.5 rounded-lg border bg-white px-4 py-3
                    transition-[border-color,box-shadow] ${
                    hasError
                        ? "border-danger shadow-[0_0_6px_rgba(185,28,28,0.25)]"
                        : "border-line focus-within:border-[#2B6CB0] focus-within:shadow-[0_0_6px_rgba(43,108,176,0.25)]"
                }`}
            >
                {leading && (
                    <span
                        aria-hidden
                        className={`flex shrink-0 items-center gap-2.5 transition-colors ${
                            hasError
                                ? "text-[#DC2626]"
                                : "text-[#64748B] group-focus-within:text-[#2B6CB0]"
                        }`}
                    >
                        {leading}
                    </span>
                )}

                <input
                    id={id}
                    aria-invalid={hasError}
                    aria-describedby={error ? errorId : undefined}
                    className={`min-w-0 flex-1 bg-transparent text-right text-sm leading-5.5 text-ink outline-none placeholder:text-disabled ${className}`}
                    {...inputProps}
                />

                {trailing ??
                    (hasError && (
                        <BsExclamationCircleFill
                            aria-hidden
                            size={24}
                            className="shrink-0 text-[#DC2626]"
                        />
                    ))}
            </div>

            {(error || action) && (
                <div className="flex w-full items-start justify-between gap-4">
                    {error ? (
                        <p
                            id={errorId}
                            role="alert"
                            className="flex-1 text-right text-xs leading-4.5 text-danger"
                        >
                            {error}
                        </p>
                    ) : (
                        <span className="flex-1" />
                    )}

                    {action && (
                        <div className="shrink-0">{action}</div>
                    )}
                </div>
            )}
        </div>
    );
}