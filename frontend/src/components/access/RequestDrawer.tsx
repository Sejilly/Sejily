"use client";

import { useState, type ReactNode } from "react";
import { LuCircleAlert } from "react-icons/lu";
import { Drawer } from "@/components/ui/Drawer";
import { Button } from "@/components/ui/Button";
import { formatLongDate, hoursLeft } from "@/features/access/format";
import type { AccessRequest } from "@/features/access/types";
import { StatusBadge } from "./StatusBadge";

const END_LABEL = {
  active: "تاريخ انتهاء صلاحية الطلب",
  pending: "التاريخ المحدد لانتهاء الطلب",
  expired: "تاريخ انتهاء الصلاحية",
  rejected: "تاريخ انتهاء الطلب",
} as const;

function Banner({ children }: { children: ReactNode }) {
  return (
    <div role="status" className="flex items-center gap-2 rounded-lg border border-warning-border bg-warning-bg px-3 py-2.5 text-sm text-secondary">
      <svg width="28" height="28" viewBox="0 0 28 28" fill="none" xmlns="http://www.w3.org/2000/svg">
<rect width="28" height="28" rx="14" fill="#FEF3C7"/>
<path d="M13.9607 5.6665C9.38734 5.6665 5.6665 9.40484 5.6665 13.9998C5.6665 18.5948 9.40484 22.3332 13.9998 22.3332C18.5948 22.3332 22.3332 18.5948 22.3332 13.9998C22.3332 9.40484 18.5773 5.6665 13.9607 5.6665ZM13.9998 20.6665C10.324 20.6665 7.33317 17.6757 7.33317 13.9998C7.33317 10.324 10.3057 7.33317 13.9607 7.33317C17.659 7.33317 20.6665 10.324 20.6665 13.9998C20.6665 17.6757 17.6757 20.6665 13.9998 20.6665Z" fill="#F59E0B"/>
<path d="M13.1665 9.8335H14.8332V15.6668H13.1665V9.8335ZM13.1665 16.5002H14.8332V18.1668H13.1665V16.5002Z" fill="#F59E0B"/>
</svg>

      {children}
    </div>
  );
}

function Card({ rows }: { rows: Array<[string, ReactNode]> }) {
  return (
    <dl className="divide-y divide-border-disabled/70 rounded-lg border border-border-disabled bg-surface px-4">
      {rows.map(([label, value]) => (
        <div key={label} className="flex items-center justify-between gap-4 py-3">
          <dt className="text-sm text-secondary">{label}</dt>
          <dd className="text-sm font-normal text-ink">{value}</dd>
        </div>
      ))}
    </dl>
  );
}

type Props = {
  request: AccessRequest | null;
  onClose: () => void;
  onViewRecord: (r: AccessRequest) => void;
  onEnd: (r: AccessRequest) => Promise<void>;
};

export function RequestDrawer({ request, onClose, onViewRecord, onEnd }: Props) {
  const [confirming, setConfirming] = useState(false);
  const [busy, setBusy] = useState(false);

  const close = () => {
    if (busy) return;
    setConfirming(false);
    onClose();
  };

  const confirmEnd = async () => {
    if (!request) return;
    setBusy(true);
    await onEnd(request);
    setBusy(false);
    setConfirming(false);
  };

  const r = request;
  const isActive = r?.status === "active";

  return (
    <Drawer
      open={Boolean(r)}
      onClose={close}
      locked={busy}
      title="تفاصيل طلب الوصول"
      badge={r && <StatusBadge status={r.status} />}
      footer={
        r && isActive ? (
          <>
            <Button variant="green" onClick={() => onViewRecord(r)}>عرض السجل الطبي</Button>
            <Button variant="outlineDanger" onClick={() => setConfirming(true)}>إنهاء الوصول</Button>
          </>
        ) : undefined
      }
      overlay={
        r && confirming ? (
          <div className="absolute inset-0 z-10 flex items-end">
            <div className="sj-fade-in absolute inset-0 bg-ink/30" onClick={() => !busy && setConfirming(false)} aria-hidden />
            <div
              role="alertdialog"
              aria-labelledby="end-title"
              aria-describedby="end-desc"
              className="sj-sheet-up relative w-full space-y-4 rounded-t-2xl bg-white p-5 shadow-2xl"
            >
              <div className="space-y-2">
                <h3 id="end-title" className="text-lg font-normal text-ink">
                  إنهاء الوصول مع {r.patientName} ؟
                </h3>
                <p id="end-desc" className="text-sm leading-5 text-secondary font-normal">
                  هل أنت متأكد من إغلاق الوصول مع المريض؟ سيتم إيقاف صلاحية الاطلاع على سجله الطبي فوراً.
                </p>
              </div>
              <div className="space-y-2.5">
                <Button variant="danger" onClick={confirmEnd} disabled={busy}>
                  {busy ? "جارٍ الإنهاء..." : "تأكيد الإنهاء"}
                </Button>
                <Button variant="outlineBlue" onClick={() => setConfirming(false)} disabled={busy}>
                  إلغاء
                </Button>
              </div>
            </div>
          </div>
        ) : null
      }
    >
      {r && (
        <>
          {r.status === "active" && r.extendedByPatient && <Banner>قام المريض بتمديد الوصول بينكما.</Banner>}
          {r.status === "pending" && <Banner>هذا الطلب ينتهي خلال {hoursLeft(r.endsAt)} ساعة من الآن!</Banner>}

          <Card
            rows={[
              ["اسم المريض", r.patientName],
              ["الرقم القومي", <span key="id" dir="ltr" className="inline-block tabular-nums">{r.nationalId}</span>],
              ["جهة العمل", r.workplace],
              ["نوع الوصول", r.type === "emergency" ? "طوارئ" : "عادي"],
            ]}
          />
          <Card
            rows={[
              ["تاريخ الطلب", formatLongDate(r.requestedAt)],
              [END_LABEL[r.status], formatLongDate(r.endsAt)],
            ]}
          />
        </>
      )}
    </Drawer>
  );
}
