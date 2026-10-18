import Link from "next/link";
import { IoArrowForwardSharp } from "react-icons/io5";
import Logo from "../ui/Logo";

export default function BackHeader({ href }: { href: string }) {
    return (
        <header className="flex shrink-0 items-center justify-between gap-4 lg:h-9" dir="ltr">
            <Link
                href={href}
                dir="rtl"
                className="flex shrink-0 items-center gap-2 text-sm leading-5.5 font-normal
                text-[#475569] hover:text-[#0B192C]"
            >
                <IoArrowForwardSharp aria-hidden className="text-[#334155]" />
                رجوع
            </Link>
            <div className="flex items-center -me-5">
                <Logo />
            </div>
        </header>
    );
}