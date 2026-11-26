"use client";

import Image from "next/image";
import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import {
  LuChevronsLeft,
  LuChevronsRight,
  LuLayoutDashboard,
  LuLogOut,
  LuMapPin,
  LuScanFace,
  LuSettings,
  LuShieldCheck,
} from "react-icons/lu";
import type { ComponentType } from "react";
import { ROUTES } from "@/lib/routes";
import { cn } from "@/lib/cn";
import {
  DashboardIcon,
  AccessIcon,
  AssistantIcon,
  WorkplacesIcon,
  SettingsIcon,
  LogoutIcon,
} from "@/components/icons";

type Item = {
  label: string;
  href: string;
  icon: ComponentType<{ className?: string }>;
  match: (path: string) => boolean;
};

const trim = (p: string) => p.replace(/\/+$/, "") || "/";

const MAIN: Item[] = [
  { label: "لوحة التحكم", href: ROUTES.dashboard, icon: DashboardIcon, match: (p) => trim(p) === ROUTES.dashboard },
  { label: "إدارة الوصول", href: ROUTES.access, icon: AccessIcon, match: (p) => trim(p).startsWith(ROUTES.access) },
  { label: "المساعد الذكي", href: `${ROUTES.dashboard}/assistant`, icon: AssistantIcon, match: (p) => p.includes("/assistant") },
];

const GENERAL: Item[] = [
  { label: "جهات العمل", href: `${ROUTES.dashboard}/workplaces`, icon: WorkplacesIcon, match: (p) => p.includes("/workplaces") },
  { label: "الإعدادات", href: `${ROUTES.dashboard}/settings`, icon: SettingsIcon, match: (p) => p.includes("/settings") },
];

type Props = {
  collapsed: boolean;
  onToggle: () => void;
  /** Mobile off-canvas: always expanded, no collapse button */
  mobile?: boolean;
  onNavigate?: () => void;
};

export function Sidebar({ collapsed, onToggle, mobile, onNavigate }: Props) {
  const pathname = usePathname();
  const router = useRouter();
  const slim = collapsed && !mobile;

  const logout = () => {
    try {
      sessionStorage.removeItem("sejily_token");
    } catch {}
    router.push(ROUTES.login);
  };

  const renderItem = (item: Item) => {
    const active = item.match(pathname);
    const Icon = item.icon;
    return (
      <li key={item.href}>
        <Link
          href={item.href}
          onClick={onNavigate}
          aria-current={active ? "page" : undefined}
          title={slim ? item.label : undefined}
          className={cn(
            "flex h-11 items-center gap-3 rounded-xl px-3 text-base transition-colors font-normal ",
            slim && "justify-center px-0",
            active
              ? "bg-sidebar-active font-semibold text-brand-navy"
              : "text-secondary hover:bg-white/60",
          )}
        >
          <Icon className="size-5 shrink-0" />
          {!slim && <span>{item.label}</span>}
        </Link>
      </li>
    );
  };

  return (
    <div
      className={cn(
        "flex h-full flex-col bg-linear-to-b from-[#D9E5EF] via-[#EFF8F5] to-[#F0F5F9] px-3 py-4 transition-[width] duration-200",
        mobile ? "w-64" : slim ? "w-20" : "w-60",
      )}
    >
      <div className={cn("flex items-center", slim ? "flex-col gap-3" : "justify-between")}>
        <div dir="ltr" className="flex items-center">
          {!slim && (
            <span dir="rtl" className="text-2xl font-bold whitespace-nowrap text-[#053755] -me-1">
              سِجِلّي
            </span>
          )}
          <Image src="/Sejily/logo-icon.svg" alt="شعار سجلّي" width={48} height={48} priority className="size-12 shrink-0" />
        </div>
        {!mobile && (
          <button
            type="button"
            onClick={onToggle}
            aria-label={collapsed ? "توسيع القائمة" : "طيّ القائمة"}
            aria-expanded={!collapsed}
            className="grid size-7 place-items-center rounded-md bg-white/70 text-secondary hover:bg-white"
          >
            {collapsed ? <LuChevronsLeft className="size-4" /> : <LuChevronsRight className="size-4" />}
          </button>
        )}
      </div>

      <nav className="mt-6 flex-1 space-y-6" aria-label="القائمة الرئيسية">
        <div>
          {!slim && <p className="mb-2 px-2 text-sm text-disabled font-semibold">القائمة</p>}
          <ul className="space-y-1">{MAIN.map(renderItem)}</ul>
        </div>
        <div>
          {!slim && <p className="mb-2 px-2 text-sm font-semibold text-disabled">عام</p>}
          <ul className="space-y-1">
            {GENERAL.map(renderItem)}
            <li>
              <button
                type="button"
                onClick={logout}
                title={slim ? "تسجيل الخروج" : undefined}
                className={cn(
                  "flex h-11 w-full items-center gap-3 rounded-xl px-3 text-sm text-secondary transition-colors hover:bg-white/60",
                  slim && "justify-center px-0",
                )}
              >
                <LogoutIcon className="size-5 shrink-0" />
                {!slim && <span className="text-base transition-colors font-normal">تسجيل الخروج</span>}
              </button>
            </li>
          </ul>
        </div>
      </nav>
    </div>
  );
}
