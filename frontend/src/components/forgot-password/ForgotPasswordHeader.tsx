import Link from "next/link";
import Logo from "@/components/auth-ui/Logo";
import { ROUTES } from "../../lib/routes";

export default function ForgotPasswordHeader() {
    return (
        <header className="flex shrink-0 items-center justify-between gap-4 lg:h-9" dir="ltr">
            <p
                className="shrink-0 text-xs leading-5.5 font-normal text-[#475569] sm:text-sm"
            >
                لا تملك حساب بعد ؟{" "}
                <Link href={ROUTES.register} className="block text-[#2B6CB0] md:inline">
                    إنشاء حساب
                </Link>
            </p>
            <div className="flex items-center -me-5">
                <Logo />
            </div>
        </header>
    );
}