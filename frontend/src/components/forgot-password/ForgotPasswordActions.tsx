import { ROUTES } from "../../lib/routes";
import { ButtonLink } from "@/components/auth-ui/Button";
import FormAlert from "@/components/auth-ui/FormAlert";
import SubmitButton from "@/components/auth-ui/SubmitButton";

type Props = { loading: boolean; formError: string };

export default function ForgotPasswordActions({ loading, formError }: Props) {
    return (
        <div className="flex w-full flex-col gap-4">
            {formError && <FormAlert message={formError} />}
            <SubmitButton loading={loading} loadingText="جارٍ الإرسال...">
                إرسال رمز التحقق
            </SubmitButton>
            <ButtonLink
                href={ROUTES.login}
                className="border-0 font-normal! text-[#2B6CB0]! hover:bg-transparent"
            >
                عودة إلى صفحة تسجيل الدخول
            </ButtonLink>
        </div>
    );
}