import type { ReactNode } from "react";
import TextField from "@/components/auth-ui/TextField";
import PasswordField from "@/components/auth-ui/PasswordField";
import { toEnglishDigits } from "../../lib/validators";

export type LoginField = "nationalId" | "password";

type Props = {
    values: Record<LoginField, string>;
    errors: Partial<Record<LoginField, string>>;
    onChange: (field: LoginField, value: string) => void;
    onBlur: (field: LoginField, value: string) => void;
    passwordAction?: ReactNode;
};

function FormInputs({ values, errors, onChange, onBlur, passwordAction }: Props) {
    return (
        <>
            <TextField
                id="national-id"
                name="nationalId"
                label="الرقم القومي"
                type="text"
                inputMode="numeric"
                autoComplete="username"
                maxLength={14}
                value={values.nationalId}
                onChange={(e) => onChange("nationalId", toEnglishDigits(e.target.value))}
                onBlur={(e) => onBlur("nationalId", e.target.value)}
                placeholder="أدخل الرقم القومي"
                error={errors.nationalId}
            />

            <PasswordField
                id="password"
                name="password"
                label="كلمة المرور"
                autoComplete="current-password"
                value={values.password}
                onChange={(e) => onChange("password", e.target.value)}
                onBlur={(e) => onBlur("password", e.target.value)}
                placeholder="أدخل كلمة المرور"
                error={errors.password}
                action={passwordAction}
            />
        </>
    );
}

export default FormInputs;