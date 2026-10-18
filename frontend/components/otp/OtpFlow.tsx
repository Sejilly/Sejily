"use client";

import { useEffect, useState, type SyntheticEvent } from "react";
import { useRouter } from "next/navigation";
import AuthCard from "../auth/AuthCard";
import AuthIntro from "../auth/AuthIntro";
import OtpInputs, { focusOtpBox } from "./OtpInputs";
import OtpActions from "./OtpActions";
import { resendOtp, verifyOtp } from "../../lib/auth";
import { formatTime, useCountdown } from "../../lib/useCountdown";
import {
    GENERIC_ERROR,
    NETWORK_ERROR,
    OTP_RESTART_ERROR,
    OTP_INCOMPLETE_ERROR,
    OTP_INVALID_ERROR,
    RESEND_WAIT_ERROR,
    TOO_MANY_ERROR,
} from "../../lib/validators";
import { ROUTES } from "../../lib/routes";
import { readResetFlow, saveResetFlow, type ResetFlow } from "../../lib/resetFlow";

const OTP_LENGTH = 6;
const EXPIRY_SECONDS = 300;
const RESEND_COOLDOWN_SECONDS = 60;

const emptyDigits = () => Array<string>(OTP_LENGTH).fill("");
const maskPhone = (p: string) => "*".repeat(Math.max(p.length - 2, 0)) + p.slice(-2);

export default function OtpFlow() {
    const router = useRouter();
    const [flow, setFlow] = useState<ResetFlow | null>(null);
    const [digits, setDigits] = useState(emptyDigits);
    const [codeError, setCodeError] = useState("");
    const [formError, setFormError] = useState("");
    const [verifying, setVerifying] = useState(false);
    const [resending, setResending] = useState(false);
    const { left, restart } = useCountdown(EXPIRY_SECONDS);
    const { left: resendLeft, restart: restartResend } = useCountdown(RESEND_COOLDOWN_SECONDS);

    useEffect(() => {
        const found = readResetFlow();
        if (found?.requestId && found?.phone) {
            // eslint-disable-next-line react-hooks/set-state-in-effect
            setFlow(found);
        } else {
            router.replace(ROUTES.forgotPassword);
        }
    }, [router]);

    const handleChange = (next: string[]) => {
        setDigits(next);
        setCodeError("");
        setFormError("");
    };

const failureMessage = (reason: "too_many" | "restart" | "server" | "network") =>
    reason === "too_many"
        ? TOO_MANY_ERROR
        : reason === "restart"
        ? OTP_RESTART_ERROR
        : reason === "network"
            ? NETWORK_ERROR
            : GENERIC_ERROR;

    const handleSubmit = async (e: SyntheticEvent<HTMLFormElement>) => {
        e.preventDefault();
        if (verifying || resending || !flow) return;

        const code = digits.join("");
        setFormError("");

        if (code.length < OTP_LENGTH) {
            setCodeError(OTP_INCOMPLETE_ERROR);
            focusOtpBox(Math.max(digits.findIndex((d) => !d), 0));
            return;
        }

        setVerifying(true);
        const result = await verifyOtp(flow.requestId, code);
        setVerifying(false);

if (result.ok) {
    saveResetFlow({ ...flow, verified: true });
    router.push(ROUTES.resetPassword);
    return;
}

if (result.reason === "invalid") {
    setCodeError(OTP_INVALID_ERROR);
    setDigits(emptyDigits());
    focusOtpBox(0);
    return;
}
setFormError(failureMessage(result.reason));
    };

    const handleResend = async () => {
    if (verifying || resending || resendLeft > 0 || !flow) return;
    setResending(true);
    setCodeError("");
    setFormError("");
    const result = await resendOtp(flow.requestId);
    setResending(false);

        if (result.ok) {
            setDigits(emptyDigits());
            restart();
            restartResend();
            focusOtpBox(0);
            return;
        }

        if (result.reason === "too_many") restartResend();
        setFormError(
            result.reason === "too_many" ? RESEND_WAIT_ERROR : failureMessage(result.reason)
        );
    };

    if (!flow) return null;

    return (
        <AuthCard>
            <AuthIntro
                src="/Sejily/otp.svg"
                alt="رمز التحقق"
                title="إدخال رمز التحقق"
                description={
                    <>
                        أدخل رمز التحقق المرسل في رسالة نصية إلى رقم هاتفك (
                        <span dir="ltr" className="whitespace-nowrap">{maskPhone(flow.phone)}</span>)
                    </>
                }
            />

            <form onSubmit={handleSubmit} noValidate className="flex w-full flex-col gap-8 sm:gap-11">
                <div className="flex w-full flex-col gap-2.5">
                    <OtpInputs
                        digits={digits}
                        onChange={handleChange}
                        hasError={Boolean(codeError)}
                        disabled={verifying}
                    />
                    {codeError && (
                        <p role="alert" className="text-center text-xs leading-4.5 text-[#B91C1C]">
                            {codeError}
                        </p>
                    )}
                </div>

                <OtpActions
                    time={formatTime(left)}
                    expired={left === 0}
                    verifying={verifying}
                    resending={resending}
                    resendLeft={resendLeft}
                    formError={formError}
                    onResend={handleResend}
                />
            </form>
        </AuthCard>
    );
}