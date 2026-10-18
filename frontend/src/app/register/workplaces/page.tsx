"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { CloseIcon, PlusIcon } from "@/components/icons";
import { BackLink, Button } from "@/components/ui/Button";
import { FormHeader } from "@/components/register/FormHeader";
import { WorkplaceCard } from "@/components/register/WorkplaceCard";
import { WorkplaceForm } from "@/components/register/WorkplaceForm";
import { useRegister } from "@/features/register/RegisterContext";
import type { WorkplaceValues } from "@/features/register/types";
import { RegisterShell } from "@/components/register/RegisterShell";

/**
 * Three states from the design:
 *  1. no workplaces yet          -> form
 *  2. at least one saved         -> list of collapsible cards (+ "add" button)
 *  3. adding another             -> form with count + "cancel" button
 */
export default function WorkplacesPage() {
  const router = useRouter();
  const { data, update } = useRegister();
  const list = data.workplaces;

  const [adding, setAdding] = useState(false);
  const [expandedId, setExpandedId] = useState<string | null>(null);

  const showForm = list.length === 0 || adding;

  const save = (values: WorkplaceValues) => {
    const id = crypto.randomUUID();
    // in case EmanMaged wants: the very first saved workplace opens expanded
    //setExpandedId(list.length === 0 ? id : null);
    update("workplaces", [...list, { id, ...values }]);
    setAdding(false);
  };

  const remove = (id: string) => update("workplaces", list.filter((w) => w.id !== id));

  if (showForm) {
    const isExtra = list.length > 0;
    return (
        <RegisterShell>
      <>
        <FormHeader
          title="بيانات أماكن العمل"
          hint={isExtra ? `( ${list.length} جهة عمل مُضافة )` : undefined}
          description={
            isExtra
              ? "يرجى إدخال تفاصيل جهة العمل الإضافية الخاصة بك."
              : "يرجى إدخال تفاصيل جهة العمل الخاصة بك."
          }
          action={
            adding && (
              <Button variant="outline" onClick={() => setAdding(false)}>
                <CloseIcon className="size-4" />
                إلغاء الإضافة
              </Button>
            )
          }
        />
        <WorkplaceForm onSave={save} />
        <BackLink href="/register/professional" />
      </>
      </RegisterShell>
    );
  }

  return (
    <RegisterShell>
    <>
      <FormHeader
        title="بيانات أماكن العمل"
        description="تم حفظ جهة العمل بنجاح. يمكنك المتابعة أو إضافة جهة عمل أخرى."
        action={
          <Button variant="outline" onClick={() => setAdding(true)}>
            <PlusIcon className="size-4" />
            إضافة جهة عمل
          </Button>
        }
      />
      <div className="space-y-3">
        {list.map((w) => (
          <WorkplaceCard
            key={w.id}
            workplace={w}
            expanded={expandedId === w.id}
            onToggle={() => setExpandedId((cur) => (cur === w.id ? null : w.id))}
            onDelete={() => remove(w.id)}
          />
        ))}
      </div>
      <div className="pt-8">
        <Button onClick={() => router.push("/register/account")}>التالي</Button>
      </div>
      <BackLink href="/register/professional" />
    </>
</RegisterShell>
  );
}
