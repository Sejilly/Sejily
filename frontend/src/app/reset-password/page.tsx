import AuthPageShell from "@/components/auth/AuthPageShell";
import AuthCard from "@/components/auth/AuthCard";
import AuthIntro from "@/components/auth/AuthIntro";
import BackHeader from "@/components/auth/BackHeader";
import ResetPasswordForm from "@/components/reset-password/ResetPasswordForm";
import { ROUTES } from "@/lib/routes";

export default function ResetPasswordPage() {
    return (
        <AuthPageShell header={<BackHeader href={ROUTES.otp} />}>
            <AuthCard>
                <AuthIntro
                    src="/Sejily/reset.svg"
                    alt="إعادة تعيين كلمة المرور"
                    title="إعادة تعيين كلمة المرور"
                    description="أنشئ كلمة مرور قوية وجديدة لحسابك."
                />
                <ResetPasswordForm />
            </AuthCard>
        </AuthPageShell>
    );
}