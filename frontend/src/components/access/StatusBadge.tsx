import type { AccessStatus } from "@/features/access/types";
import { cn } from "@/lib/cn";

export const STATUS_LABEL: Record<AccessStatus, string> = {
  active: "نشطة",
  pending: "معلقة",
  expired: "منتهية",
  rejected: "لم تُقبَل",
};

const STYLE: Record<AccessStatus, string> = {
  active: "bg-[#D1FAE5] text-[#047857]",
  pending: "bg-[#FEF3C7] text-[#D97706]",
  expired: "bg-[#E2E8F0] text-[#475569]",
  rejected: "bg-[#FEE2E2] text-[#B91C1C]",
};

export function StatusBadge({ status, className }: { status: AccessStatus; className?: string }) {
  return (
    <span className={cn("inline-flex items-center rounded-md px-2 py-0.5 text-xs leading-5 font-normal whitespace-nowrap", STYLE[status], className)}>
      {STATUS_LABEL[status]}
    </span>
  );
}
