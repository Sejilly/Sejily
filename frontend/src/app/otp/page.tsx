import AuthPageShell from "../../../components/auth/AuthPageShell";
import BackHeader from "../../../components/auth/BackHeader";
import OtpFlow from "../../../components/otp/OtpFlow";
import { ROUTES } from "../../../lib/routes";


export default function OtpPage() {
    return (
        <AuthPageShell header={<BackHeader href={ROUTES.forgotPassword} />}>
            <OtpFlow />
        </AuthPageShell>
    );
}