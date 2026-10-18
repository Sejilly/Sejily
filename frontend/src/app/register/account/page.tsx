"use client";

import { useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { PhoneIcon } from "@/components/icons";
import { BackLink, Button } from "@/components/ui/Button";
import { Field } from "@/components/ui/Field";
import { PasswordInput } from "@/components/ui/PasswordInput";
import { TextInput } from "@/components/ui/TextInput";
import { FormHeader } from "@/components/register/FormHeader";
import { VERIFYING_HREF } from "@/features/register/constants";
import { useRegister } from "@/features/register/RegisterContext";
import { useForm } from "@/features/register/UseForm";
import { validateAccount } from "@/features/register/validation";
import { RegisterShell } from "@/components/register/RegisterShell";
import { registerDoctor } from "@/lib/api/auth";
import { ApiError } from "@/lib/api/client";
import { DUPLICATE_ACCOUNT_RE, MSG_GENERIC } from "@/lib/api/messages";
import { isRegistrationIncomplete, toRegisterDoctorRequest } from "@/features/register/mapper";

export default function AccountPage() {
  const router = useRouter();
  const { data, update } = useRegister();
  const { errors, bind, handleSubmit } = useForm(data.account, validateAccount);
///
  const [submitting, setSubmitting] = useState(false);
  const [serverErrors, setServerErrors] = useState<string[]>([]);
  const [duplicate, setDuplicate] = useState(false);

   const onValid = async (account: typeof data.account) => {
    if (submitting) return;
    update("account", account);
    setServerErrors([]);
    setDuplicate(false);

    const next = { ...data, account };
    if (isRegistrationIncomplete(next)) {
      setServerErrors(["بيانات الخطوات السابقة غير مكتملة. يرجى استكمالها أولًا."]);
      return;
    }

    setSubmitting(true);
    try {
      await registerDoctor(toRegisterDoctorRequest(next));
      // Don't keep passwords in memory once the account exists.
      update("account", { ...account, password: "", confirmPassword: "" });
      router.push(VERIFYING_HREF);
    } catch (e) {
      if (e instanceof ApiError) {
        setServerErrors(e.messages);
        setDuplicate((e.status === 400 || e.status === 409) && e.raw.some((m) => DUPLICATE_ACCOUNT_RE.test(m)));
      } else {
        setServerErrors([MSG_GENERIC]);
      }
      setSubmitting(false);
    }
  };


///
  return (
    <RegisterShell >
      <FormHeader title="بيانات الحساب" description="قم بتعيين بيانات تسجيل الدخول لحسابك لتأمين حسابك." />
      {/* <form 
        noValidate
        className="space-y-5"
        onSubmit={handleSubmit((v) => {
          update("account", v);
          router.push(VERIFYING_HREF);
        })}
      >*/}
            <form
        noValidate
        className="space-y-5"
        onSubmit={handleSubmit(onValid)}
      >
        <Field id="phone" label="رقم الهاتف المحمول" error={errors.phone}>
          <TextInput
            id="phone"
            type="tel"
            numeric
            placeholder="01X  XXXX  XXXX"
            endAdornment={<PhoneIcon className="size-5" />}
            {...bind("phone")}
          />
        </Field>
        <Field id="email" label="البريد الإلكتروني" optional error={errors.email}>
          <TextInput id="email" type="email" dir="ltr" className="text-right" placeholder="أدخل البريد الإلكتروني" {...bind("email")} />
        </Field>
        <Field id="password" label="تعيين كلمة المرور" error={errors.password}>
          <PasswordInput id="password" autoComplete="new-password" placeholder="••••••••" {...bind("password")} />
        </Field>
        <Field id="confirmPassword" label="تأكيد كلمة المرور" error={errors.confirmPassword}>
          <PasswordInput id="confirmPassword" autoComplete="new-password" placeholder="••••••••" {...bind("confirmPassword")} />
        </Field>
        {/*new for integration */}
        {serverErrors.length > 0 && (
          <div role="alert" className="space-y-1 rounded-lg border border-danger/30 bg-danger/5 px-4 py-3 text-sm text-danger">
            {serverErrors.map((m, i) => (
              <p key={i}>{m}</p>
            ))}
            {duplicate && (
              <p className="pt-1 text-ink">
                <Link href="/register/personal" className="font-semibold text-brand-blue hover:underline">
                  تعديل الرقم القومي
                </Link>
                {" · "}
                <Link href="/login" className="font-semibold text-brand-blue hover:underline">
                  تسجيل الدخول
                </Link>
              </p>
            )}
          </div>
        )}
        
        {/*}
        <div className="pt-6">
          <Button type="submit">إنشاء الحساب</Button>
        </div>
      </form>
      <BackLink href="/register/workplaces" />
    </RegisterShell>
    */}
        <div className="pt-6">
          <Button type="submit" disabled={submitting} aria-busy={submitting}>
            {submitting ? "جاري إنشاء الحساب..." : "إنشاء الحساب"}
          </Button>
        </div>
      </form>
      <BackLink href="/register/workplaces" />
    </RegisterShell>

  );

}
