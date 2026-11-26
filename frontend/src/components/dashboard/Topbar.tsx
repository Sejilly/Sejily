"use client";

import { LuChevronDown, LuMenu, LuUser } from "react-icons/lu";
import { NotificationsMenu } from "@/components/notifications/NotificationsMenu";

export function Topbar({ title, onMenu }: { title: string; onMenu: () => void }) {
  return (
    <header className="flex h-14 shrink-0 items-center justify-between border-b border-border-disabled/60 bg-surface px-4 sm:px-6">
      <div className="flex items-center gap-3">
        <button
          type="button"
          onClick={onMenu}
          aria-label="فتح القائمة"
          className="grid size-9 place-items-center rounded-lg text-secondary hover:bg-white lg:hidden"
        >
          <LuMenu className="size-5" />
        </button>
        <span className="text-base font-normal text-ink">{title}</span>
      </div>

      <div className="flex items-center gap-2">
        <NotificationsMenu />
        <button
          type="button"
          aria-label="الحساب"
          className="flex h-9 items-center gap-1 rounded-full border border-border-disabled bg-white ps-2 pe-2.5 text-secondary hover:bg-surface"
        >
          <LuUser className="size-4.5" />
          <LuChevronDown className="size-3.5 text-disabled" />
        </button>
      </div>
    </header>
  );
}
