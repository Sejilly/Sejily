"use client";

import { useEffect, useState, type SyntheticEvent } from "react";
import { useRouter } from "next/navigation";

import PasswordField from "@/components/auth-ui/PasswordField";
import FormAlert from "@/components/auth-ui/FormAlert";
import SubmitButton from "@/components/auth-ui/SubmitButton";
import PasswordRules from "@/components/auth-ui/PasswordRules";

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
import {
    clearResetFlow,
    readResetFlow,
    type ResetFlow,
} from "../../lib/resetFlow";

export default function ResetPasswordForm() {
    const router = useRouter();
    const [isPasswordFocused, setIsPasswordFocused] = useState(false);
    const [formError, setFormError] = useState("");
    const [loading, setLoading] = useState(false);
    const [flow, setFlow] = useState<ResetFlow | null>(null);
    const [showPasswordRuleErrors, setShowPasswordRuleErrors] =
        useState(false);

    const {
        values,
        errors,
        onChange,
        onBlur,
        validateAll,
        focusField,
    } = useFormFields(
        {
            password: "",
            confirm: "",
        },
        {
            password: (v) => validatePassword(v),
            confirm: (v, all) =>
                validateConfirmPassword(v, all.password),
        },
        {
            password: "new-password",
            confirm: "confirm-password",
        }
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

    const handleChange = (
        field: "password" | "confirm",
        value: string
    ) => {
        setFormError("");
        onChange(field, value);

        if (
            field === "password" &&
            validatePassword(value) === undefined
        ) {
            setShowPasswordRuleErrors(false);
        }
    };

    const handleSubmit = async (
        e: SyntheticEvent<HTMLFormElement>
    ) => {
        e.preventDefault();

        if (loading || !flow) return;

        setFormError("");
        setShowPasswordRuleErrors(true);

        if (!validateAll()) return;

        setLoading(true);

        try {
            const result = await resetPassword(
                flow.requestId,
                values.password
            );

            if (result.ok) {
                clearResetFlow();
                router.replace(ROUTES.login);
                return;
            }

            if (result.reason === "weak") {
                setFormError(
                    result.messages?.join(" • ") || PASSWORD_ERROR
                );

                focusField("password");
                return;
            }

            setFormError(
                result.reason === "expired"
                    ? RESET_SESSION_EXPIRED_ERROR
                    : result.reason === "network"
                        ? NETWORK_ERROR
                        : GENERIC_ERROR
            );
        } catch {
            setFormError(GENERIC_ERROR);
        } finally {
            setLoading(false);
        }
    };

    if (!flow) return null;

    return (
        <form
            onSubmit={handleSubmit}
            noValidate
            className="flex w-full flex-col gap-8 sm:gap-11"
        >
            <div
                className="flex w-full flex-col gap-5"
            >
                <PasswordField
                    id="new-password"
                    name="password"
                    label="كلمة المرور"
                    autoComplete="new-password"
                    placeholder="أدخل كلمة المرور"
                    value={values.password}
                    onFocus={() => setIsPasswordFocused(true)}
                    onChange={(e) =>
                        handleChange("password", e.target.value)
                    }
                    onBlur={(e) => {
                        const value = e.target.value;
                        const nextTarget = e.relatedTarget;

                        const stayedInsideField =
                            nextTarget instanceof Node &&
                            Boolean(
                                e.currentTarget.parentElement?.contains(nextTarget)
                            );

                        // لا ننفذ الخروج الفعلي لو التركيز انتقل
                        // إلى أيقونة العين داخل الحقل.
                        if (stayedInsideField) return;

                        onBlur("password", value);
                        setIsPasswordFocused(false);

                        setShowPasswordRuleErrors(
                            value.length > 0 &&
                                validatePassword(value) !== undefined
                        );
                    }}
                    error={
                        errors.password === PASSWORD_ERROR
                            ? undefined
                            : errors.password
                    }
                    visualError={
                        showPasswordRuleErrors &&
                        values.password.length > 0 &&
                        validatePassword(values.password) !== undefined
                    }
                />

                {(isPasswordFocused || showPasswordRuleErrors) && (
                    <PasswordRules
                        password={values.password}
                        showErrors={showPasswordRuleErrors}
                    />
                )}


                <PasswordField
                    id="confirm-password"
                    name="confirm"
                    label="تأكيد كلمة المرور"
                    autoComplete="new-password"
                    placeholder="أعد إدخال كلمة المرور"
                    value={values.confirm}
                    onChange={(e) =>
                        handleChange("confirm", e.target.value)
                    }
                    onBlur={(e) =>
                        onBlur("confirm", e.target.value)
                    }
                    error={errors.confirm}
                />
            </div>

            <div
                className="flex w-full flex-col gap-4"
            >
                {formError && <FormAlert message={formError} />}

                <SubmitButton
                    loading={loading}
                    loadingText="جارٍ الحفظ..."
                >
                    تسجيل الدخول
                </SubmitButton>
            </div>
        </form>
    );
}