import type { AccessRequest } from "@/features/access/types";
import { formatTableDate } from "@/features/access/format";
import { StatusBadge } from "./StatusBadge";
import { cn } from "@/lib/cn";

/**
 * Responsive strategy
 *  - < md  : stacked cards (no horizontal scrolling, big touch targets)
 *  - md    : compact table — name, ID, date, status, action
 *  - lg+   : full table — adds workplace and access type
 * Anything hidden at a size is still available in the details drawer.
 */
const COLUMNS: Array<{ label: string; className?: string }> = [
  { label: "اسم المريض" },
  { label: "الرقم القومي" },
  { label: "جهة العمل", className: "hidden lg:table-cell" },
  { label: "تاريخ الطلب" },
  { label: "حالة الطلب" },
  { label: "نوع الوصول", className: "hidden lg:table-cell" },
  { label: "إجراءات" },
];

type Props = {
  rows: AccessRequest[];
  loading: boolean;
  onView: (r: AccessRequest) => void;
};

const typeLabel = (r: AccessRequest) => (r.type === "emergency" ? "طوارئ" : "عادي");

function Empty() {
  return (
    <div className="rounded-xl border border-border-disabled px-5 py-14 text-center text-sm text-secondary">
      لا توجد طلبات وصول مطابقة.
    </div>
  );
}

function CardList({ rows, loading, onView }: Props) {
  if (loading) {
    return (
      <ul className="space-y-3" aria-hidden>
        {Array.from({ length: 4 }).map((_, i) => (
          <li key={i} className="space-y-3 rounded-xl border border-border-disabled p-4">
            <div className="h-4 w-1/2 animate-pulse rounded bg-border-disabled" />
            <div className="h-3 w-3/4 animate-pulse rounded bg-border-disabled" />
            <div className="h-3 w-2/3 animate-pulse rounded bg-border-disabled" />
          </li>
        ))}
      </ul>
    );
  }
  if (rows.length === 0) return <Empty />;

  return (
    <ul className="space-y-3">
      {rows.map((r) => (
        <li key={r.id} className="space-y-3 rounded-xl border border-border-disabled bg-white p-4">
          <div className="flex items-start justify-between gap-3">
            <p className="text-sm font-semibold text-ink">{r.patientName}</p>
            <StatusBadge status={r.status} />
          </div>

          <dl className="grid grid-cols-2 gap-x-4 gap-y-2.5 text-xs">
            <div>
              <dt className="text-secondary">الرقم القومي</dt>
              <dd className="mt-0.5 text-ink">
                <span dir="ltr" className="inline-block tabular-nums">{r.nationalId}</span>
              </dd>
            </div>
            <div>
              <dt className="text-secondary">نوع الوصول</dt>
              <dd className="mt-0.5 text-ink">{typeLabel(r)}</dd>
            </div>
            <div className="col-span-2">
              <dt className="text-secondary">جهة العمل</dt>
              <dd className="mt-0.5 text-ink">{r.workplace}</dd>
            </div>
            <div className="col-span-2">
              <dt className="text-secondary">تاريخ الطلب</dt>
              <dd className="mt-0.5 text-ink">{formatTableDate(r.requestedAt)}</dd>
            </div>
          </dl>

          {r.status !== "rejected" && (
            <button
              type="button"
              onClick={() => onView(r)}
              aria-label={`عرض طلب ${r.patientName}`}
              className="h-11 w-full rounded-lg border border-line bg-white text-sm font-medium text-ink hover:bg-surface focus-visible:outline-2 focus-visible:outline-brand-blue"
            >
              عرض التفاصيل
            </button>
          )}
        </li>
      ))}
    </ul>
  );
}

function DataTable({ rows, loading, onView }: Props) {
  return (
    <div className="overflow-x-auto rounded-xl  border-[1px] border-border-disabled">
      <table className="w-full border-collapse text-xs text-ink">
        <thead>
          <tr className="bg-[#F0F5F9] text-secondary">
            {COLUMNS.map((c) => (
              <th key={c.label} scope="col" className={cn("px-4 py-3.5 text-start text-secondary text-sm font-semibold lg:px-5", c.className)}>
                {c.label}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {loading &&
            Array.from({ length: 6 }).map((_, i) => (
              <tr key={i} className={i % 2 ? "bg-surface" : "bg-white"}>
                {COLUMNS.map((c) => (
                  <td key={c.label} className={cn("px-4 py-4 lg:px-5", c.className)}>
                    <div className="h-3 w-3/4 animate-pulse rounded bg-border-disabled" />
                  </td>
                ))}
              </tr>
            ))}

          {!loading && rows.length === 0 && (
            <tr>
              <td colSpan={COLUMNS.length} className="px-5 py-14 text-center text-sm text-secondary">
                لا توجد طلبات وصول مطابقة.
              </td>
            </tr>
          )}

          {!loading &&
            rows.map((r, i) => (
              <tr key={r.id} className={i % 2 ? "bg-surface text-ink text-sm font-normal" : "bg-white text-ink text-sm font-normal" }>
                <td className="px-4 py-3.5 lg:px-5">{r.patientName}</td>
                <td className="px-4 py-3.5 lg:px-5">
                  <span dir="ltr" className="inline-block tabular-nums">{r.nationalId}</span>
                </td>
                <td className="hidden px-4 py-3.5 lg:table-cell lg:px-5">{r.workplace}</td>
                <td className="px-4 py-3.5 whitespace-nowrap lg:px-5">{formatTableDate(r.requestedAt)}</td>
                <td className="px-4 py-3.5 lg:px-5"><StatusBadge status={r.status} /></td>
                <td className="hidden px-4 py-3.5 lg:table-cell lg:px-5">{typeLabel(r)}</td>
                <td className="px-4 py-3.5 lg:px-5">
                  {r.status === "rejected" ? (
                    <span aria-label="لا توجد إجراءات" className="text-disabled justify-center mr-5 font-extrabold">—</span>
                  ) : (
                    <button
                      type="button"
                      onClick={() => onView(r)}
                      aria-label={`عرض طلب ${r.patientName}`}
                      className="h-7 rounded-md border border-line bg-surface px-3 text-sm text-ink hover:bg-surface focus-visible:outline-2 focus-visible:outline-brand-blue"
                    >
                      عرض
                    </button>
                  )}
                </td>
              </tr>
            ))}
        </tbody>
      </table>
    </div>
  );
}

export function AccessTable(props: Props) {
  return (
    <>
      <div className="md:hidden"><CardList {...props} /></div>
      <div className="hidden md:block"><DataTable {...props} /></div>
    </>
  );
}
