import Image from "next/image";
import Background from "@/components/auth-ui/Background";
import Logo from "@/components/auth-ui/Logo";
import { ButtonLink } from "@/components/auth-ui/Button";

function success() {
    return (
        <div className="relative flex h-screen items-center justify-center bg-[#F8FAFC] px-4 py-24 sm:px-8 lg:px-20">
            <Background />
            <div
                dir="ltr"
                className="absolute inset-x-0 top-8 z-10 flex h-9 items-center justify-center ps-5 sm:top-10 lg:top-12"
            >
                <Logo />
            </div>
            <div className="relative z-10 flex w-full max-w-138 flex-col items-center gap-8 rounded-2xl bg-white p-5 
                shadow-[0px_0px_10px_rgba(56,161,105,0.15)] sm:gap-11 sm:p-8">
                <div className="flex w-full flex-col items-center gap-6">
                    <Image
                        src="/Sejily/success.svg"
                        alt="success-icon"
                        width={132}
                        height={132}
                        priority
                        className="h-24 w-24 sm:h-33 sm:w-33"
                    />
                    <div className="flex w-full flex-col gap-3 text-center text-[#0B192C] sm:gap-4">
                        <h3 className="text-lg font-semibold leading-7 sm:text-xl">
                            ممتاز، أصبح حسابك موثقاً الآن!
                        </h3>
                        <p className="text-sm font-normal leading-6.5 sm:text-base">
                            تم التحقق من بياناتك وتراخيصك بنجاح. يمكنك الآن بدء استخدام منصة &quot;سِجِلّي&quot; وإدارة السجلات الطبية لمرضاك بكل سهولة.
                        </p>
                    </div>
                </div>

                <div className="flex w-full justify-center">
                    <ButtonLink variant="primary" href="/">
                        الانتقال إلى الصفحة الرئيسية
                    </ButtonLink>
                </div>
            </div>
        </div>
    );
}

export default success;