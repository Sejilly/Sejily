"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import { LuPlus } from "react-icons/lu";
import { AccessTable } from "./AccessTable";
import { CreateRequestModal } from "./CreateRequestModal";
import { FilterBar, type Filters } from "./FilterBar";
import { RequestDrawer } from "./RequestDrawer";
import { StatusTabs } from "./StatusTabs";
import { Toast } from "./Toast";
import type { AccessRequest, StatusFilter } from "@/features/access/types";
import { WORKPLACES, endAccess, listRequests } from "@/lib/api/access";

const EMPTY_FILTERS: Filters = { query: "", sort: "", workplace: "", type: "" };

export function AccessManagement() {
  const [requests, setRequests] = useState<AccessRequest[]>([]);
  const [loading, setLoading] = useState(true);
  const [tab, setTab] = useState<StatusFilter>("all");
  const [filters, setFilters] = useState<Filters>({ ...EMPTY_FILTERS });
  const [createOpen, setCreateOpen] = useState(false);
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [toast, setToast] = useState("");

  useEffect(() => {
    let alive = true;
    listRequests().then((rows) => {
      if (!alive) return;
      setRequests(rows);
      setLoading(false);
    });
    return () => {
      alive = false;
    };
  }, []);

  const counts = useMemo(() => {
    const c: Record<StatusFilter, number> = { all: requests.length, active: 0, pending: 0, expired: 0, rejected: 0 };
    for (const r of requests) c[r.status]++;
    return c;
  }, [requests]);

  const rows = useMemo(() => {
    const q = filters.query.trim();
    const out = requests.filter(
      (r) =>
        (tab === "all" || r.status === tab) &&
        (!filters.workplace || r.workplace === filters.workplace) &&
        (!filters.type || r.type === filters.type) &&
        (!q || r.patientName.includes(q)),
    );
    
    out.sort((a, b) =>
      filters.sort === "oldest"
        ? a.requestedAt.localeCompare(b.requestedAt)
        : 0,
    );
    return out;
  }, [requests, tab, filters]);

  const selected = requests.find((r) => r.id === selectedId) ?? null;

  const handleCreated = useCallback((r: AccessRequest) => {
    setRequests((prev) => [r, ...prev]);
    setTab("all");
    setFilters({ ...EMPTY_FILTERS });
    setToast("تم إرسال طلب الوصول بنجاح");
  }, []);

  const handleEnd = useCallback(async (r: AccessRequest) => {
    const updated = await endAccess(r.id);
    if (updated) {
      setRequests((prev) => prev.map((x) => (x.id === r.id ? updated : x)));
      setToast("تم إنهاء الوصول مع المريض");
    }
    setSelectedId(null);
  }, []);

  return (
    <div className="mx-auto w-full max-w-360 space-y-6">
      <div className="flex flex-wrap items-start justify-between gap-4">
        <div className="space-y-1.5">
          <h1 className="text-2xl font-bold text-ink">إدارة الوصول</h1>
          <p className="text-base font-normal text-secondary">متابعة حالة طلباتك وإرسال طلبات وصول جديدة للمرضى</p>
        </div>
        <button
          type="button"
          onClick={() => setCreateOpen(true)}
          className="inline-flex h-11 items-center gap-2 rounded-xl bg-brand-green px-5 text-base font-semibold text-white transition-colors hover:bg-brand-green-hover focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-brand-green"
        >
          <LuPlus className="size-5" />
          إنشاء طلب وصول جديد
        </button>
      </div>

      <StatusTabs value={tab} counts={counts} onChange={setTab} />

      <FilterBar
        filters={filters}
        workplaces={WORKPLACES.map((w) => w.name)}
        onChange={(patch) => setFilters((f) => ({ ...f, ...patch }))}
      />

      <AccessTable rows={rows} loading={loading} onView={(r) => setSelectedId(r.id)} />

      <CreateRequestModal open={createOpen} onClose={() => setCreateOpen(false)} onCreated={handleCreated} />

      <RequestDrawer
        key={selectedId ?? "none"}
        request={selected}
        onClose={() => setSelectedId(null)}
        onViewRecord={() => setToast("عرض السجل الطبي سيتاح في المرحلة القادمة")}
        onEnd={handleEnd}
      />

      {toast && <Toast message={toast} onDone={() => setToast("")} />}
    </div>
  );
}
