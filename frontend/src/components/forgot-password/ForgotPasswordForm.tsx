"use client";

import { useState, type SyntheticEvent } from "react";
import { useRouter } from "next/navigation";
import ForgotPasswordFields from "./ForgotPasswordFields";
import ForgotPasswordActions from "./ForgotPasswordActions";
import { useFormFields } from "../../lib/useFormFields";
import { requestPasswordReset } from "../../lib/auth";
import {
    phoneLiveError,
    GENERIC_ERROR,
    NETWORK_ERROR,
    TOO_MANY_ERROR,
    validateNationalId,
    validatePhone,
} from "../../lib/validators";
import { ROUTES } from "../../lib/routes";
import { saveResetFlow } from "../../lib/resetFlow";

export default function ForgotPasswordForm() {
    const router = useRouter();
    const [loading, setLoading] = useState(false);
    const [formError, setFormError] = useState("");

    const { values, errors, onChange, onBlur, validateAll} = useFormFields(
        { nationalId: "", phone: "" },
        { nationalId: validateNationalId, phone: validatePhone },
        { nationalId: "national-id", phone: "phone" },
        { phone: phoneLiveError }
    );

    const handleChange = (field: "nationalId" | "phone", value: string) => {
        setFormError("");
        onChange(field, value);
    };

    const handleSubmit = async (e: SyntheticEvent<HTMLFormElement>) => {
        e.preventDefault();
        if (loading) return;
        setFormError("");
        if (!validateAll()) return;

        setLoading(true);
        const result = await requestPasswordReset(values.nationalId, values.phone);
        setLoading(false);

        if (result.ok) {
            saveResetFlow({ requestId: result.requestId, phone: values.phone });
            router.push(ROUTES.otp);
            return;
        }
        setFormError(
            result.reason === "too_many"
                ? TOO_MANY_ERROR
                : result.reason === "network"
                ? NETWORK_ERROR
                : GENERIC_ERROR
        );
    };

    return (
        <form
            onSubmit={handleSubmit}
            noValidate
            className="flex w-full shrink-0 flex-col gap-8 sm:gap-11"
        >
            <ForgotPasswordFields
                values={values}
                errors={errors}
                onChange={handleChange}
                onBlur={onBlur}
            />
            <ForgotPasswordActions loading={loading} formError={formError} />
        </form>
    );
}