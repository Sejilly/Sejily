"use client";

import { useEffect, useState, type SyntheticEvent } from "react";
import { useRouter } from "next/navigation";
import PasswordField from "../ui/PasswordField";
import FormAlert from "../ui/FormAlert";
import SubmitButton from "../ui/SubmitButton";
import { useFormFields } from "../../lib/useFormFields";
import { resetPassword } from "../../lib/auth";
import {
    GENERIC_ERROR,
    NETWORK_ERROR,
    PASSWORD_ERROR,
    RESET_SESSION_EXPIRED_ERROR,
    validateConfirmPassword,
    validatePassword,
} from "../../lib/validators";
import { ROUTES } from "../../lib/routes";
import { clearResetFlow, readResetFlow, type ResetFlow } from "../../lib/resetFlow";

export default function ResetPasswordForm() {
    const router = useRouter();
    const [formError, setFormError] = useState("");
    const [loading, setLoading] = useState(false);
    const [flow, setFlow] = useState<ResetFlow | null>(null);
    const { values, errors, setErrors, onChange, onBlur, validateAll, focusField } = useFormFields(
        { password: "", confirm: "" },
        {
            password: (v) => validatePassword(v),
            confirm: (v, all) => validateConfirmPassword(v, all.password),
        },
        { password: "new-password", confirm: "confirm-password" }
    );

    useEffect(() => {
        const found = readResetFlow();
        if (found?.requestId && found.verified) {
            // eslint-disable-next-line react-hooks/set-state-in-effect
            setFlow(found);
        } else {
            router.replace(ROUTES.forgotPassword);
        }
    }, [router]);

    const handleChange = (field: "password" | "confirm", value: string) => {
        setFormError("");
        onChange(field, value);
    };

    const handleSubmit = async (e: SyntheticEvent<HTMLFormElement>) => {
        e.preventDefault();
        if (loading || !flow) return;
        setFormError("");
        if (!validateAll()) return;

        setLoading(true);
        const result = await resetPassword(flow.requestId, values.password);
        setLoading(false);

        if (result.ok) {
            clearResetFlow();
            router.replace(ROUTES.login);
            return;
        }
        if (result.reason === "weak") {
            setErrors((p) => ({ ...p, password: PASSWORD_ERROR }));
            return focusField("password");
        }
        setFormError(
            result.reason === "expired"
                ? RESET_SESSION_EXPIRED_ERROR
                : result.reason === "network"
                ? NETWORK_ERROR
                : GENERIC_ERROR
        );
    };

    if (!flow) return null;

    return (
        <form onSubmit={handleSubmit} noValidate className="flex w-full flex-col gap-8 sm:gap-11">
            <div className="flex w-full flex-col gap-5">
                <PasswordField
                    id="new-password"
                    name="password"
                    label="كلمة المرور"
                    autoComplete="new-password"
                    placeholder="أدخل كلمة المرور"
                    value={values.password}
                    onChange={(e) => handleChange("password", e.target.value)}
                    onBlur={(e) => onBlur("password", e.target.value)}
                    error={errors.password}
                />
                <PasswordField
                    id="confirm-password"
                    name="confirmPassword"
                    label="تأكيد كلمة المرور"
                    autoComplete="new-password"
                    placeholder="أعد إدخال كلمة المرور"
                    value={values.confirm}
                    onChange={(e) => handleChange("confirm", e.target.value)}
                    onBlur={(e) => onBlur("confirm", e.target.value)}
                    error={errors.confirm}
                />
            </div>

            <div className="flex w-full flex-col gap-4">
                {formError && <FormAlert message={formError} />}
                <SubmitButton loading={loading} loadingText="جارٍ الحفظ...">
                    تسجيل الدخول
                </SubmitButton>
            </div>
        </form>
    );
}