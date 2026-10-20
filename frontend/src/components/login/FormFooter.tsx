import Link from "next/link";
function FormFooter() {
    return (
        <nav className="flex w-full flex-wrap items-center justify-center gap-4 text-xs leading-4.5 font-semibold text-disabled">
            <Link href="#" className="hover:text-brand-color">
            سياسة الخصوصية
            </Link>
            <span aria-hidden>•</span>
            <Link href="#" className="hover:text-brand-color">
            الشروط والأحكام
            </Link>
            <span aria-hidden>•</span>
            <Link href="#" className="hover:text-brand-color">
            اتصل بنا
            </Link>
        </nav>
    )
}

export default FormFooter