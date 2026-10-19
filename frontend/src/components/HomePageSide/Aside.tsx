import Logo from "@/components/auth-ui/Logo";
import Content from "./Content";

function Aside() {
    return (
        <aside
            dir="rtl"
            className="order-first flex w-full max-w-154 flex-col items-center justify-center gap-8
            lg:order-0 lg:min-h-181.5 lg:max-w-138 lg:flex-1 lg:self-center xl:gap-16"
        >
            <div
                dir="ltr"
                className="flex h-12 w-full items-center justify-end -me-13"
            >
                <Logo/>
            </div>
            <Content/>
        </aside>
    )
}

export default Aside