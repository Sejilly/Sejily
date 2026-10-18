import Link from "next/link";
function FormFooter() {
    return (
        <nav className="flex w-full flex-wrap items-center justify-center gap-4 text-xs leading-4.5 font-semibold text-[#94A3B8]">
            <Link href="#" className="hover:text-[#2B6CB0]">
            سياسة الخصوصية
            </Link>
            <span aria-hidden>•</span>
            <Link href="#" className="hover:text-[#2B6CB0]">
            الشروط والأحكام
            </Link>
            <span aria-hidden>•</span>
            <Link href="#" className="hover:text-[#2B6CB0]">
            اتصل بنا
            </Link>
        </nav>
    )
}

export default FormFooter