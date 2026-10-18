"use client";

import { Field } from "@/components/ui/Field";
import { TextInput } from "@/components/ui/TextInput";
import { ChevronDownIcon, ChevronUpIcon, TrashIcon } from "@/components/icons";
import type { Workplace } from "@/features/register/types";
import { workplaceTypeLabel } from "@/features/register/constants";

export function WorkplaceCard({
  workplace,
  expanded,
  onToggle,
  onDelete,
}: {
  workplace: Workplace;
  expanded: boolean;
  onToggle: () => void;
  onDelete: () => void;
}) {
  const panelId = `wp-panel-${workplace.id}`;
  return (
    <div className="rounded-lg border border-line bg-white">
      <div className="flex items-center gap-1 p-2 ps-3">
        <div className="min-w-0 flex-1">
          <p className="truncate text-base text-secondary">{workplaceTypeLabel(workplace.type)}</p>
          <p className="truncate text-sm text-secondary">{workplace.name}</p>
        </div>
        <button
          type="button"
          onClick={onDelete}
          aria-label={`حذف ${workplace.name}`}
          className="grid size-9 place-items-center rounded text-muted hover:bg-danger/10 hover:text-danger"
        >
          <TrashIcon />
        </button>
        <button
          type="button"
          onClick={onToggle}
          aria-expanded={expanded}
          aria-controls={panelId}
          aria-label={expanded ? "إخفاء التفاصيل" : "عرض التفاصيل"}
          className="grid size-9 place-items-center rounded text-disabled hover:bg-surface"
        >
          {expanded ? <ChevronUpIcon /> : <ChevronDownIcon />}
        </button>
      </div>

      {expanded && (
        <div id={panelId} className="space-y-4 px-3 pb-4 pt-2">
          <Field id={`${panelId}-type`} label="نوع الجهة" required>
            <TextInput id={`${panelId}-type`} className="text-secondary" readOnly value={workplaceTypeLabel(workplace.type)} />
          </Field>
          <Field id={`${panelId}-name`} label="اسم جهة العمل" required>
            <TextInput id={`${panelId}-name`} className="text-secondary" readOnly value={workplace.name} />
          </Field>
          <Field id={`${panelId}-address`} label="عنوان جهة العمل" required>
            <TextInput id={`${panelId}-address`} className="text-secondary" readOnly value={workplace.address} />
          </Field>
          <Field id={`${panelId}-license`} label="ترخيص مكان العمل" required>
            <TextInput id={`${panelId}-license`} className="text-secondary" readOnly numeric value={workplace.licenseNumber} />
          </Field>
        </div>
      )}
    </div>
  );
}