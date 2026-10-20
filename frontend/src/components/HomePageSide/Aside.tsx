import Logo from "@/components/auth-ui/Logo";
import Content from "./Content";

function Aside() {
    return (
        <aside dir="rtl" className=" order-first flex w-full max-w-154 flex-col items-center justify-center gap-8
            lg:order-0 lg:min-h-181.5 lg:max-w-138 lg:flex-1 lg:self-center xl:gap-16 2xl:[@media(max-height:800px)]:h-full
            2xl:[@media(max-height:800px)]:min-h-0 2xl:[@media(max-height:800px)]:gap-8"
        >
            <div dir="ltr" className=" flex h-12 w-full shrink-0 items-center justify-end -me-13 2xl:[@media(max-height:800px)]:h-9
                2xl:[@media(max-height:800px)]:-me-6
                "
            >
                <Logo />
            </div>

            <Content />
        </aside>
    );
}

export default Aside;