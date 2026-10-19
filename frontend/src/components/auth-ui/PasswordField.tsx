"use client";

import { useState } from "react";
import { BsEye, BsEyeSlash } from "react-icons/bs";
import TextField, { type TextFieldProps } from "./TextField";
type PasswordFieldProps = Omit<TextFieldProps, "type" | "trailing">;

export default function PasswordField(props: PasswordFieldProps) {
    const [show, setShow] = useState(false);
    const hasError = Boolean(props.error);

    return (
        <TextField
            {...props}
            type={show ? "text" : "password"}
            trailing={
                <button
                    type="button"
                    onClick={() => setShow((prev) => !prev)}
                    aria-label={show ? "إخفاء كلمة المرور" : "إظهار كلمة المرور"}
                    className={`shrink-0 transition-colors ${
                        hasError
                            ? "text-[#DC2626]"
                            : show
                            ? "text-[#334155]"
                            : "text-[#64748B] hover:text-[#0B192C]"
                    }`}
                >
                    {show ? <BsEye size={20} /> : <BsEyeSlash size={20} />}
                </button>
            }
        />
    );
}