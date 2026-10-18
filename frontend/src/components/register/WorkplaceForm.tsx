"use client";

import { Button } from "@/components/ui/Button";
import { Field } from "@/components/ui/Field";
import { Select } from "@/components/ui/Select";
import { TextInput } from "@/components/ui/TextInput";
import { WORKPLACE_TYPES } from "@/features/register/constants";
import type { WorkplaceValues } from "@/features/register/types";
import { useForm } from "@/features/register/UseForm";
import { validateWorkplace } from "@/features/register/validation";

const empty: WorkplaceValues = { type: "", name: "", address: "", licenseNumber: "" };

export function WorkplaceForm({ onSave }: { onSave: (v: WorkplaceValues) => void }) {
  const { errors, bind, handleSubmit } = useForm(empty, validateWorkplace);

  return (
    <form onSubmit={handleSubmit(onSave)} noValidate className="space-y-5">
      <Field id="wp-type" label="نوع الجهة" required error={errors.type}>
        <Select
          id="wp-type"
          options={WORKPLACE_TYPES}
          placeholder="اختر نوع الجهة (مستشفى حكومي،... )"
          {...bind("type")}
        />
      </Field>
      <Field id="wp-name" label="اسم جهة العمل" required error={errors.name}>
        <TextInput id="wp-name" placeholder="أدخل اسم جهة العمل" {...bind("name")} />
      </Field>
      <Field id="wp-address" label="عنوان جهة العمل" required error={errors.address}>
        <TextInput id="wp-address" placeholder="المحافظة، المدينة، التفاصيل" {...bind("address")} />
      </Field>
      <Field id="wp-license" label="رقم ترخيص مكان العمل" required error={errors.licenseNumber}>
        <TextInput id="wp-license" numeric placeholder="أدخل رقم ترخيص المنشأة" {...bind("licenseNumber")} />
      </Field>
      <div className="pt-3">
        <Button type="submit">حفظ جهة العمل</Button>
      </div>
    </form>
  );
}