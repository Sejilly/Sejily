"use client";

import {
  forwardRef,
  useRef,
  type InputHTMLAttributes,
  type ReactNode,
} from "react";
import DatePicker from "react-datepicker";
import type { Locale } from "date-fns";
import { cn } from "@/lib/cn";

type DatePickerOptions = {
  selected: Date | null;
  onChange: (date: Date | null) => void;
  locale?: Locale;
  dateFormat?: string;
  placeholderText?: string;
  calendarStartDay?: 0 | 1 | 2 | 3 | 4 | 5 | 6;
};

type Props = InputHTMLAttributes<HTMLInputElement> & {
  endAdornment?: ReactNode;
  invalid?: boolean;
  numeric?: boolean;
  datePicker?: DatePickerOptions;
  adornmentPosition?: "start" | "end";
  success?: boolean;
};

export const shellClass = (invalid?: boolean, disabled?: boolean, success?: boolean,) =>
  cn(
    "relative flex h-12 items-center rounded-lg border bg-white transition-colors overflow-hidden",
    "focus-within:border-brand-blue focus-within:ring-2 focus-within:ring-brand-blue/20",
    success
      ? "border-emerald-600 border ring-emerald-600/20"
      : invalid
        ? "border-danger"
        : "border-line",
    disabled && "bg-surface",
  );

export const TextInput = forwardRef<HTMLInputElement, Props>(
  function TextInput(
    {
      endAdornment,
      invalid,
      success,
      numeric,
      datePicker,
      adornmentPosition = "start",
      className,
      ...rest
    },
    ref,
  ) {
    const datePickerRef = useRef<DatePicker>(null);

    const inputClass = cn(
      "h-full min-w-0 flex-1 bg-transparent px-4 py-3 text-sm text-ink outline-none",
      "placeholder:text-disabled disabled:text-disabled read-only:text-disabled",
      numeric && "text-right",
      className,
    );

    const containerClass = cn(
      shellClass(invalid, rest.disabled || rest.readOnly, success),
      adornmentPosition === "end" && "flex-row-reverse",
    );

    return (
      <div className={containerClass}>
        {datePicker ? (
          <>
            {endAdornment && (
              <button
                type="button"
                aria-label="فتح التقويم"
                disabled={rest.disabled || rest.readOnly}
                onClick={() => datePickerRef.current?.setOpen(true)}
                className="flex h-6 shrink-0 items-center border-e border-line px-3 text-disabled disabled:cursor-not-allowed"
              >
                {endAdornment}
              </button>
            )}

            <DatePicker
              showMonthDropdown
              showYearDropdown
              dropdownMode="select"
              yearDropdownItemNumber={100}
              scrollableYearDropdown
              ref={datePickerRef}
              selectsRange={false}
              id={rest.id}
              name={rest.name}
              selected={datePicker.selected}
              onChange={datePicker.onChange}
              dateFormat={datePicker.dateFormat ?? "dd/MM/yyyy"}
              locale={datePicker.locale}
              placeholderText={
                datePicker.placeholderText ?? rest.placeholder
              }
              calendarStartDay={datePicker.calendarStartDay}
              disabled={rest.disabled}
              readOnly={rest.readOnly}
              required={rest.required}
              aria-invalid={invalid ? "true" : undefined}
              aria-describedby={
                invalid && rest.id
                  ? `${rest.id}-error`
                  : undefined
              }
              wrapperClassName="h-full min-w-0 flex-1"
              className={cn(inputClass, "w-full")}
              calendarClassName="!font-sans"
            />
          </>
        ) : (
          <>
            {endAdornment && (
              <div className={ cn("flex h-6 shrink-0 items-center border-line px-3 text-disabled",
                adornmentPosition === "start" && "border-e border-line"

              )}>
                {endAdornment}
              </div>
            )}

            <input
              ref={ref}
              dir={numeric ? "ltr" : undefined}
              inputMode={numeric ? "numeric" : undefined}
              aria-invalid={invalid || undefined}
              aria-describedby={
                invalid && rest.id
                  ? `${rest.id}-error`
                  : undefined
              }
              className={inputClass}
              {...rest}
            />
          </>
        )}
      </div>
    );
  },
);

TextInput.displayName = "TextInput";