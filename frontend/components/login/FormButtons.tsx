import { ButtonLink } from "../ui/Button";
import SubmitButton from "../ui/SubmitButton";
import TextDivider from "../ui/TextDivider";

function FormButtons({ loading = false }: { loading?: boolean }) {
    return (
        <div className="flex w-full flex-col items-center justify-center gap-8">
            <SubmitButton loading={loading} loadingText="جارٍ تسجيل الدخول...">
                سجل الدخول
            </SubmitButton>
            <TextDivider>لا تملك حساب بعد ؟</TextDivider>
            <ButtonLink href="/register">إنشاء حساب جديد</ButtonLink>
        </div>
    );
}

export default FormButtons;