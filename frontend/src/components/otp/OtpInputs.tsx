"use client";

import type { ChangeEvent, ClipboardEvent, KeyboardEvent } from "react";
import { toEnglishDigits } from "../../lib/validators";

type Props = {
    digits: string[];
    onChange: (digits: string[]) => void;
    hasError?: boolean;
    disabled?: boolean;
};

export const focusOtpBox = (i: number) => {
    const el = document.getElementById(`otp-${i}`) as HTMLInputElement | null;
    el?.focus();
    el?.select();
};

export default function OtpInputs({ digits, onChange, hasError = false, disabled = false }: Props) {
    const last = digits.length - 1;

    const fill = (start: number, chars: string) => {
        const next = [...digits];
        let i = start;
        for (const ch of chars) {
            if (i > last) break;
            next[i++] = ch;
        }
        onChange(next);
        focusOtpBox(Math.min(i, last));
    };

    const handleChange = (i: number, e: ChangeEvent<HTMLInputElement>) => {
        const chars = toEnglishDigits(e.target.value);
        if (!chars) {
            const next = [...digits];
            next[i] = "";
            onChange(next);
            return;
        }
        fill(i, chars);
    };

    const handleKeyDown = (i: number, e: KeyboardEvent<HTMLInputElement>) => {
        if (e.key === "Backspace" && !digits[i] && i > 0) {
            e.preventDefault();
            const next = [...digits];
            next[i - 1] = "";
            onChange(next);
            focusOtpBox(i - 1);
        } else if (e.key === "ArrowLeft" && i > 0) {
            e.preventDefault();
            focusOtpBox(i - 1);
        } else if (e.key === "ArrowRight" && i < last) {
            e.preventDefault();
            focusOtpBox(i + 1);
        }
    };

    const handlePaste = (e: ClipboardEvent<HTMLInputElement>) => {
        e.preventDefault();
        const chars = toEnglishDigits(e.clipboardData.getData("text"));
        if (chars) fill(0, chars);
    };

    return (
        <div dir="ltr" className="flex w-full gap-1.5 sm:gap-2">
            {digits.map((digit, i) => (
                <input
                    key={i}
                    id={`otp-${i}`}
                    type="text"
                    inputMode="numeric"
                    autoComplete={i === 0 ? "one-time-code" : "off"}
                    autoFocus={i === 0}
                    aria-label={`الرقم ${i + 1} من ${digits.length}`}
                    aria-invalid={hasError}
                    disabled={disabled}
                    value={digit}
                    onChange={(e) => handleChange(i, e)}
                    onKeyDown={(e) => handleKeyDown(i, e)}
                    onPaste={handlePaste}
                    onFocus={(e) => e.target.select()}
                    className={`h-12 min-w-0 flex-1 rounded-lg border bg-white text-center text-xl font-bold
                        text-ink outline-none transition-[border-color,box-shadow] sm:h-16 sm:text-2xl md:h-18 ${
                        hasError
                            ? "border-danger shadow-[0_0_6px_rgba(185,28,28,0.25)]"
                            : "border-line focus:border-[#2B6CB0] focus:shadow-[0_0_6px_rgba(43,108,176,0.25)]"
                    }`}
                />
            ))}
        </div>
    );
}