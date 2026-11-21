import AuthPageShell from "@/features/auth/components/layout/AuthPageShell";
import AuthCard from "@/features/auth/components/layout/AuthCard";
import AuthIntro from "@/features/auth/components/layout/AuthIntro";
import ForgotPasswordHeader from "@/features/auth/components/forgot-password/ForgotPasswordHeader";
import ForgotPasswordForm from "@/features/auth/components/forgot-password/ForgotPasswordForm";

export default function ForgotPasswordPage() {
    return (
        <AuthPageShell header={<ForgotPasswordHeader />}>
                <AuthCard>
                    <AuthIntro
                        src="/Sejily/password.svg"
                        alt="نسيت كلمة المرور"
                        title="نسيت كلمة المرور ؟"
                        description="أدخل بياناتك المسجلة للتحقق من هويتك واستعادة الحساب."
                    />
                    <ForgotPasswordForm />
                </AuthCard>
        </AuthPageShell>
    );
}