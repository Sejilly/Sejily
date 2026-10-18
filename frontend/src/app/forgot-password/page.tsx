import AuthPageShell from "../../../components/auth/AuthPageShell";
import AuthCard from "../../../components/auth/AuthCard";
import AuthIntro from "../../../components/auth/AuthIntro";
import ForgotPasswordHeader from "../../../components/forgot-password/ForgotPasswordHeader";
import ForgotPasswordForm from "../../../components/forgot-password/ForgotPasswordForm";

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