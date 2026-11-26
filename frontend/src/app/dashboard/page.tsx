import Link from "next/link";
import { ROUTES } from "@/lib/routes";

export default function DashboardPage() {
  return (
    <div className="mx-auto max-w-3xl space-y-4 pt-10 text-center">
      <h1 className="text-2xl font-bold text-ink">لوحة التحكم</h1>
      <p className="text-secondary">ستظهر هنا ملخصات النشاط قريباً.</p>
      <Link href={ROUTES.access} className="inline-flex h-11 items-center rounded-lg bg-brand-green px-5 text-sm font-semibold text-white hover:bg-brand-green-hover">
        الانتقال إلى إدارة الوصول
      </Link>
    </div>
  );
}
