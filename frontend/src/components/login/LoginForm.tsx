import FormHeader from "./FormHeader";
import FormContent from "./FormContent";
import FormFooter from "./FormFooter";

function LoginForm() {
    return (
        <div dir="rtl" className="flex w-full max-w-154 min-w-0 flex-col items-center justify-between gap-12 rounded-2xl
        bg-white px-4 pt-10 pb-8 shadow-[0px_4px_20px_rgba(43,108,176,0.24)] sm:px-8 lg:min-h-181.5 lg:flex-1 lg:gap-10 lg:pt-16
        2xl:[@media(max-height:800px)]:h-full 2xl:[@media(max-height:800px)]:min-h-0 2xl:[@media(max-height:800px)]:max-w-154
        2xl:[@media(max-height:800px)]:gap-5 2xl:[@media(max-height:800px)]:px-9 2xl:[@media(max-height:800px)]:pt-9
        2xl:[@media(max-height:800px)]:pb-7">
            <div
            className="flex w-full flex-col items-start gap-12 2xl:[@media(max-height:800px)]:gap-7
            "
            >
                <FormHeader />
                <FormContent />
            </div>
            <FormFooter />
        </div>
    );
}

export default LoginForm;