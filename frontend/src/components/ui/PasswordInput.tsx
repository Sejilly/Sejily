"use client";

import { useState, type InputHTMLAttributes, type FocusEvent} from "react";
import { EyeIcon, EyeOffIcon } from "@/components/icons";
import { TextInput } from "./TextInput";

export function PasswordInput({
  onBlur,
  ...props
}: Omit<InputHTMLAttributes<HTMLInputElement>, "type"> & {
  invalid?: boolean;
}) {
  const [show, setShow] = useState(false);
const handleBlur = (event: FocusEvent<HTMLDivElement>) => {
  const nextTarget = event.relatedTarget as Node | null;

  if (!event.currentTarget.contains(nextTarget)) {
    setShow(false);
    onBlur?.(event as unknown as React.FocusEvent<HTMLInputElement>);
  }
};
  return (
    <div onBlur={handleBlur}>
    <TextInput
      {...props}
      type={show ? "text" : "password"}
      dir="ltr"
      className="text-right"
      adornmentPosition="end"
            endAdornment={
        <button
          type="button"
          onClick={() => setShow((s) => !s)}
          aria-label={show ? "إخفاء كلمة المرور" : "إظهار كلمة المرور"}
          className="rounded text-muted hover:text-ink focus-visible:outline-2 focus-visible:outline-brand-blue "
        >
          {show ? <EyeIcon /> : <EyeOffIcon />}
        </button>
      }
    />
    </div>
  );
}