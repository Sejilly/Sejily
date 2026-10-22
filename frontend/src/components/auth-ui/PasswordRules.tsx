"use client";

import { getPasswordRules } from "../../lib/validators";

type Props = {
    password: string;
    showErrors?: boolean;
};

function RuleIcon({
    status,
}: {
    status: "passed" | "failed" | "pending";
}) {
    return (
        <svg
            width="18"
            height="18"
            viewBox="0 0 16 16"
            fill="none"
            aria-hidden="true"
            className="shrink-0"
        >
            {status === "failed" ? (
                <path
                    d="M5 5L11 11M11 5L5 11"
                    stroke="currentColor"
                    strokeWidth="1.5"
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    vectorEffect="non-scaling-stroke"
                />
            ) : (
                <polyline
                    points="2.75 8.75 6.25 12.25 13.25 4.75"
                    stroke="currentColor"
                    strokeWidth="1.5"
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    vectorEffect="non-scaling-stroke"
                />
            )}
        </svg>
    );
}

export default function PasswordRules({
    password,
    showErrors = false,
}: Props) {
    const rules = getPasswordRules(password);

    return (
        <ul
            dir="ltr"
            aria-label="شروط كلمة المرور"
            className="flex flex-col items-end gap-1 p-0"
        >
            {rules.map((rule) => {
                const status = rule.passed
                    ? "passed"
                    : showErrors
                        ? "failed"
                        : "pending";

                const color =
                    status === "passed"
                        ? "text-[#059669]"
                        : status === "failed"
                            ? "text-[#DC2626]"
                            : "text-[#94A3B8]";

                return (
                    <li
                        key={rule.id}
                        className={`flex w-full items-center justify-end gap-1 text-xs font-normal leading-4.5 ${color}`}
                    >
                        <span dir="rtl" className="text-right">
                            {rule.label}
                        </span>

                        <RuleIcon status={status} />
                    </li>
                );
            })}
        </ul>
    );
}