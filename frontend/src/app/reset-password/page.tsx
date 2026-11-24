import AuthPageShell from "@/features/auth/components/layout/AuthPageShell";
import AuthCard from "@/features/auth/components/layout/AuthCard";
import AuthIntro from "@/features/auth/components/layout/AuthIntro";
import BackHeader from "@/features/auth/components/layout/BackHeader";
import ResetPasswordForm from "@/features/auth/components/reset-password/ResetPasswordForm";
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