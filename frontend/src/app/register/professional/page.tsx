"use client";

import { useRouter } from "next/navigation";
import { BackLink, Button } from "@/components/ui/Button";
import { Field } from "@/components/ui/Field";
import { Select } from "@/components/ui/Select";
import { TextInput } from "@/components/ui/TextInput";
import { FormHeader } from "@/components/register/FormHeader";
import { SPECIALTIES } from "@/features/register/constants";
import { useRegister } from "@/features/register/RegisterContext";
import { useForm } from "@/features/register/UseForm";
import { validateProfessional } from "@/features/register/validation";
import { RegisterShell } from "@/components/register/RegisterShell";

export default function ProfessionalPage() {
  const router = useRouter();
  const { data, update } = useRegister();
  const { errors, bind, handleSubmit } = useForm(data.professional, validateProfessional);

  return (
    <RegisterShell>
    <>
      <FormHeader title="البيانات المهنية" description="يرجى ملء البيانات المهنية بدقة للمتابعة." />
      <form
        noValidate
        className="space-y-5"
        onSubmit={handleSubmit((v) => {
          update("professional", v);
          router.push("/register/workplaces");
        })}
      >
        <Field id="syndicateNumber" label="رقم قيد النقابة" error={errors.syndicateNumber}>
          <TextInput id="syndicateNumber" numeric placeholder="رقم قيد النقابة الموجود بكارنيه النقابة" {...bind("syndicateNumber")} />
        </Field>
        <Field id="licenseNumber" label="رقم ترخيص مزاولة المهنة" error={errors.licenseNumber}>
          <TextInput id="licenseNumber" numeric placeholder="أدخل رقم ترخيص المزاولة الساري" {...bind("licenseNumber")} />
        </Field>
        <Field id="specialty" label="التخصص الطبي" error={errors.specialty}>
          <Select id="specialty" options={SPECIALTIES} placeholder="اختر التخصص" {...bind("specialty")} />
        </Field>
        <div className="pt-6">
          <Button type="submit">التالي</Button>
        </div>
      </form>
      <BackLink href="/register/personal" />
    </>
    </RegisterShell>
  );
}