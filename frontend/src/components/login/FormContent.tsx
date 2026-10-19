"use client";

import { useState, type SyntheticEvent } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import FormButtons from "./FormButtons";
import FormInputs from "./FormInputs";
import FormAlert from "@/components/auth-ui/FormAlert";
import { useFormFields } from "../../lib/useFormFields";
import { login } from "../../lib/auth";
import {
    GENERIC_ERROR,
    NETWORK_ERROR,
    WRONG_PASSWORD_ERROR,
    validateNationalId,
    validatePassword,
    TOO_MANY_ERROR
} from "../../lib/validators";
import { ROUTES } from "../../lib/routes";

function FormContent() {
    const router = useRouter();
    const [formError, setFormError] = useState("");
    const [loading, setLoading] = useState(false);

    const { values, errors, onChange, onBlur, validateAll, focusField } = useFormFields(
        { nationalId: "", password: "" },
        { nationalId: validateNationalId, password: validatePassword },
        { nationalId: "national-id", password: "password" }
    );

    const handleChange = (field: "nationalId" | "password", value: string) => {
        setFormError("");
        onChange(field, value);
    };

    const handleSubmit = async (e: SyntheticEvent<HTMLFormElement>) => {
        e.preventDefault();
        if (loading) return;
        setFormError("");
        if (!validateAll()) return;

        setLoading(true);
        const result = await login(values.nationalId, values.password);
        setLoading(false);

        if (result.ok) {
            router.push(ROUTES.dashboard);
            return;
        }
        if (result.reason === "invalid_credentials") {
            setFormError(WRONG_PASSWORD_ERROR);
            return focusField("nationalId");
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
            className="flex w-full flex-col items-start justify-center gap-10"
        >
            <div className="flex w-full flex-col items-start gap-5">
                <FormInputs
                    values={values}
                    errors={errors}
                    onChange={handleChange}
                    onBlur={onBlur}
                    passwordAction={
                        <Link
                            href={ROUTES.forgotPassword}
                            className="text-xs leading-4.5 font-normal text-[#2B6CB0] underline"
                        >
                            نسيت كلمة المرور؟
                        </Link>
                    }
                />
            </div>

            <div className="flex w-full flex-col gap-4">
                {formError && <FormAlert message={formError} />}
                <FormButtons loading={loading} />
            </div>
        </form>
    );
}

export default FormContent;