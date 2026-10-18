"use client";

import { useState, type InputHTMLAttributes } from "react";
import { EyeIcon, EyeOffIcon } from "@/components/icons";
import { TextInput } from "./TextInput";

export function PasswordInput(
  props: Omit<InputHTMLAttributes<HTMLInputElement>, "type"> & { invalid?: boolean },
) {
  const [show, setShow] = useState(false);
  return (
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
  );
}