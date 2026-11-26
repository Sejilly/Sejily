"use client";

import { useEffect, useState, type ReactNode } from "react";
import { usePathname } from "next/navigation";
import { Sidebar } from "./Sidebar";
import { Topbar } from "./Topbar";
import { ROUTES } from "@/lib/routes";
import { useMediaQuery } from "@/lib/hooks/useMediaQuery";

const TITLES: Array<[string, string]> = [
  [ROUTES.access, "إدارة الوصول"],
  [ROUTES.dashboard, "لوحة التحكم"],
];

function titleFor(path: string) {
  const p = path.replace(/\/+$/, "");
  return TITLES.find(([prefix]) => p === prefix || p.startsWith(`${prefix}/`))?.[1] ?? "لوحة التحكم";
}

export function DashboardShell({ children }: { children: ReactNode }) {
  const pathname = usePathname();
  // Tablets / small laptops start with the icon-only sidebar; the user can still toggle it.
  const compact = useMediaQuery("(max-width: 1279px)");
  const [userCollapsed, setUserCollapsed] = useState<boolean | null>(null);
  const collapsed = userCollapsed ?? compact;
  const [mobileOpen, setMobileOpen] = useState(false);

  useEffect(() => {
    if (!mobileOpen) return;
    const onKey = (e: KeyboardEvent) => e.key === "Escape" && setMobileOpen(false);
    document.addEventListener("keydown", onKey);
    return () => document.removeEventListener("keydown", onKey);
  }, [mobileOpen]);

  return (
    <div className="flex min-h-dvh bg-white">
      {/* desktop sidebar (first child => right side in RTL) */}
      <div className="sticky top-0 hidden h-dvh shrink-0 lg:block">
        <Sidebar collapsed={collapsed} onToggle={() => setUserCollapsed(!collapsed)} />
      </div>

      {/* mobile off-canvas */}
      {mobileOpen && (
        <div className="fixed inset-0 z-40 lg:hidden">
          <div className="sj-fade-in absolute inset-0 bg-ink/40" onClick={() => setMobileOpen(false)} aria-hidden />
          <div className="sj-drawer-in absolute inset-y-0 right-0 max-w-[85vw]">
            <Sidebar mobile collapsed={false} onToggle={() => {}} onNavigate={() => setMobileOpen(false)} />
          </div>
        </div>
      )}

      <div className="flex min-w-0 flex-1 flex-col">
        <Topbar title={titleFor(pathname)} onMenu={() => setMobileOpen(true)} />
        <main className="flex-1 px-4 py-5 pb-[max(1.25rem,env(safe-area-inset-bottom))] sm:px-6 sm:py-6 lg:px-8">{children}</main>
      </div>
    </div>
  );
}
