import Image from "next/image";
import Link from "next/link";
import { Stepper } from "./Stepper";

const footerLinks = [
  { href: "/privacy", label: "سياسة الخصوصية" },
  { href: "/terms", label: "الشروط والأحكام" },
  { href: "/contact", label: "اتصل بنا" },
];

/**
 * Two-pane layout from the design.
 * - Desktop (lg+): sidebar on the right (RTL start) + grey form panel.
 * - Mobile/tablet: sidebar stacks on top with a compact stepper.
 */
export function RegisterShell({ children }: { children: React.ReactNode }) {
  return (
    <div className="grid min-h-dvh lg:grid-cols-[minmax(300px,31%)_1fr]">
      <aside className="relative z-10 bg-white shadow-[4px_0_25px_0_#0000000D] px-5 py-6 sm:px-10 lg:px-20 lg:pl-6 lg:pr-22">
        <Image src="/Sejily/logo.svg" alt="سجلّي" width={120} height={40} priority className="h-9 w-auto" />
        <h1 className="mt-6 text-xl font-bold text-ink lg:mt-12 lg:text-2xl">إنشاء حساب جديد</h1>
        <p className="mt-2 text-base text-ink">يرجى ملء البيانات التالية بدقة لإنشاء حسابك.</p>
        <div className="mt-6 lg:mt-10">
          <Stepper />
        </div>
      </aside>

      <div className="flex flex-col bg-surface px-5 py-6 sm:px-10 lg:px-12 lg:py-10">
        <p className="self-end text-sm text-ink/70 pl-8">
          لديك حساب بالفعل؟{" "}
          <Link href="/" className="font-semibold text-brand-blue hover:underline">
            تسجيل الدخول
          </Link>
        </p>

        <main className="mx-auto w-full max-w-2xl flex-1 py-8 lg:pt-14">{children}</main>

        <footer className="flex flex-wrap items-center gap-x-3 gap-y-1 self-end pt-6 text-xs text-muted pl-8">
          {footerLinks.map((l, i) => (
            <span key={l.href} className="flex items-center gap-3">
              {i > 0 && <span aria-hidden className="text-disabled" >•</span>}
              <Link href={l.href} className="text-disabled text-xs font-semibold hover:text-ink hover:underline">{l.label}</Link>
            </span>
          ))}
        </footer>
      </div>
    </div>
  );
}

