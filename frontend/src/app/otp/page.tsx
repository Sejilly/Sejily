import AuthPageShell from "@/features/auth/components/layout/AuthPageShell";
import BackHeader from "@/features/auth/components/layout/BackHeader";
import OtpFlow from "@/features/auth/components/otp/OtpFlow";
import { ROUTES } from "@/lib/routes";


export default function OtpPage() {
    return (
        <AuthPageShell header={<BackHeader href={ROUTES.forgotPassword} />}>
                <OtpFlow />
        </AuthPageShell>
    );
}