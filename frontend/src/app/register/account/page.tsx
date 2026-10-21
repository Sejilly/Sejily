"use client";

import { useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { SmallCheckIcon,CheckIcon, PhoneIcon, BeError } from "@/components/icons";
import { BackLink, Button } from "@/components/ui/Button";
import { Field } from "@/components/ui/Field";
import { PasswordInput } from "@/components/ui/PasswordInput";
import { TextInput } from "@/components/ui/TextInput";
import { FormHeader } from "@/components/register/FormHeader";
import { VERIFYING_HREF } from "@/features/register/constants";
import { useRegister } from "@/features/register/RegisterContext";
import { useForm } from "@/features/register/UseForm";
import { validateAccount,getPasswordRequirements} from "@/features/register/validation";
import { RegisterShell } from "@/components/register/RegisterShell";
import { registerDoctor } from "@/lib/api/auth";
import { ApiError } from "@/lib/api/client";
import { MSG_GENERIC } from "@/lib/api/messages";
import { isRegistrationIncomplete, toRegisterDoctorRequest } from "@/features/register/mapper";

export default function AccountPage() {
  const router = useRouter();
const {
  data,
  update,
  duplicateField,
  setDuplicateField,
} = useRegister();
 const { values, errors, bind, handleSubmit } = useForm(
  data.account,
  validateAccount,
);

const passwordRequirements = getPasswordRequirements(values.password);
///
  const [submitting, setSubmitting] = useState(false);

const [serverErrors, setServerErrors] = useState<string[]>([]);


   const onValid = async (account: typeof data.account) => {
    if (submitting) return;
    update("account", account);
setServerErrors([]);
setDuplicateField(null);

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
    const body = e.body as {
      field?: string;
      message?: string;
    } | null;

    const field = body?.field;

    if (
      (e.status === 400 || e.status === 409) &&
      (
        field === "nationalID" ||
        field === "syndicateCardNumber" ||
        field === "medicalLicenseNumber"
      )
    ) {
      setDuplicateField(field);
      setServerErrors([]);
    } else {
      setServerErrors(e.messages);
      setDuplicateField(null);
    }
  } else {
    setServerErrors([MSG_GENERIC]);
    setDuplicateField(null);
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
  <PasswordInput
    id="password"
    autoComplete="new-password"
    placeholder="••••••••"
    {...bind("password")}
  />

  <ul dir="rtl" className="mt-3 space-y-2">
    {passwordRequirements.map((requirement) => (
      <li
        key={requirement.id}
        className={`flex items-center gap-2 text-xs transition-colors ${
          requirement.passed ? "text-success" : "text-disabled"
        }`}
      >
        <span
          aria-hidden="true"
        >
          {requirement.passed ? <SmallCheckIcon className="text-success" /> : <SmallCheckIcon className="text-disabled" />}
        </span>

        <span>{requirement.label}</span>
      </li>
    ))}
  </ul>
</Field>

        <Field id="confirmPassword" label="تأكيد كلمة المرور" error={errors.confirmPassword}>
          <PasswordInput id="confirmPassword" autoComplete="new-password" placeholder="••••••••" {...bind("confirmPassword")} />
        </Field>
        {/*new for integration */}

{/* ///////////////////////////////////////////////////////////////// */}
{(serverErrors.length > 0 || duplicateField) && (
  <div
    role="alert"
    className="flex gap-2  rounded-lg border border-be-error/50 bg-be-error/5 px-4 py-3 text-sm text-be-error"
  >
   {duplicateField ? (
      <>
      <BeError/>
        <p className="font-normal">
          {duplicateField === "nationalID"
            ? "الرقم القومي مُسجَل مسبقاً!"
            : duplicateField === "syndicateCardNumber"
              ? "رقم قيد النقابة مُسجَل مسبقاً!  "
              : "رقم ترخيص مزاولة المهنة مُسجَل مسبقاً!  "}
          <Link
            href={
              duplicateField === "nationalID"
                ? "/register/personal"
                : "/register/professional"
            }
            className="font-bold text-be-error-link underline"
          >
            اضغط لتعديله
          </Link>

          {" أو هل لديك حساب بالفعل؟ "}

          <Link
            href="/login"
            className="font-bold text-be-error-link underline"
          >
            تسجيل الدخول
          </Link>
        </p>
      </>
 ) : (
      serverErrors.map((message, index) => (
        <p key={index}>{message}</p>
      ))
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
