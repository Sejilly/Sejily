"use client";

import { useRouter } from "next/navigation";
import { CalendarIcon } from "@/components/icons";
import { Button } from "@/components/ui/Button";
import { Field } from "@/components/ui/Field";
import { TextInput } from "@/components/ui/TextInput";
import { FormHeader } from "@/components/register/FormHeader";
import { useRegister } from "@/features/register/RegisterContext";
import { useForm } from "@/features/register/UseForm";
import { validatePersonal } from "@/features/register/validation";
import { RegisterShell } from "@/components/register/RegisterShell";
import { ar } from "date-fns/locale";
import "react-datepicker/dist/react-datepicker.css";

export default function PersonalPage() {
  const router = useRouter();
  const { data, update } = useRegister();
  const { errors, bind, handleSubmit, setFieldValue } = useForm(
  data.personal,
  validatePersonal,
);

  return (
    <RegisterShell>
      <FormHeader title="البيانات الشخصية" description="يرجى إدخال البيانات الشخصية الأساسية بدقة." />
      <form
        noValidate
        className="space-y-5"
        onSubmit={handleSubmit((v) => {
          update("personal", v);
          router.push("/register/professional");
        })}
      >
        <Field id="nationalId" label="الرقم القومي" error={errors.nationalId}>
          <TextInput id="nationalId" numeric maxLength={14} placeholder="أدخل الرقم القومي" {...bind("nationalId")} />
        </Field>
        <Field id="fullName" label="الاسم الكامل" error={errors.fullName}>
          <TextInput id="fullName" placeholder="الاسم كما ببطاقة الهوية" {...bind("fullName")} />
        </Field>
        <Field id="address" label="العنوان" error={errors.address}>
          <TextInput id="address" placeholder="العنوان كما ببطاقة الهوية" {...bind("address")} />
        </Field>


        <Field id="birthDate" label="تاريخ الميلاد" error={errors.birthDate}>
        <TextInput id="birthDate" type="text" placeholder="اليوم/الشهر/السنة"
          invalid={Boolean(errors.birthDate)}
          endAdornment={<CalendarIcon className="size-5" />}
          datePicker={{
            selected: data.personal.birthDate
              ? new Date(`${data.personal.birthDate}T00:00:00`)
              : null,
            onChange: (date) => {
              const birthDate = date
                ? [
                    date.getFullYear(),
                    String(date.getMonth() + 1).padStart(2, "0"),
                    String(date.getDate()).padStart(2, "0"),
                  ].join("-")
                : "";

              setFieldValue("birthDate", birthDate);

              update("personal", {
                ...data.personal,
                birthDate,
              });
            },
            dateFormat: "dd/MM/yyyy",
            locale: ar,
            calendarStartDay: 6,
          }}/>
          </Field>


        <div className="pt-6">
          <Button type="submit">التالي</Button>
        </div>
      </form>
    </RegisterShell>
  );
}
