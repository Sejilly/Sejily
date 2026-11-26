"use client";

import { useState } from "react";
import { LuCircleAlert, LuCircleCheck } from "react-icons/lu";
import { Modal } from "@/components/ui/Modal";
import { Button } from "@/components/ui/Button";
import { Field } from "@/components/ui/Field";
import { Select } from "@/components/ui/Select";
import { TextInput } from "@/components/ui/TextInput";
import { formatTableDate, toLocalIso } from "@/features/access/format";
import type { AccessRequest, AccessType, PatientLookup } from "@/features/access/types";
import { CURRENT_DOCTOR, WORKPLACES, createRequest, lookupPatient } from "@/lib/api/access";
import { GENERIC_ERROR, toEnglishDigits, validateNationalId } from "@/lib/validators";
import { cn } from "@/lib/cn";
import { RadiocheckIcon, RadioemptyIcon, RadioIcon } from "../icons";

const TYPES: Array<{ id: AccessType; title: string; hint?: string; desc: string }> = [
  {
    id: "normal",
    title: "وصول عادي",
    hint: "(الافتراضي)",
    desc: "يتطلب موافقة المريض للحصول على الصلاحية خلال الأوقات الاعتيادية.",
  },
  {
    id: "emergency",
    title: "وصول طوارئ",
    desc: "للحالات الحرجة العاجلة؛ يمنح وصولاً فورياً مؤقتاً مع تسجيل الحالة للرقابة.",
  },
];

const EMERGENCY_ACK =
  "أقر بأن هذه حالة طوارئ طبية تستدعي الوصول لسجل المريض دون موافقته المسبقة، وأن هذا الدخول سيتم تسجيله ومراجعته.";

const DUPLICATE_ERROR = "يوجد طلب معلق أو وصول مفتوح بالفعل مع هذا المريض.";
const NOT_FOUND_ERROR = "لم يتم العثور على مريض بهذا الرقم القومي.";
const WORKPLACE_REQUIRED = "جهة العمل مطلوبة";
const ACK_REQUIRED = "يجب الإقرار بحالة الطوارئ للمتابعة";

type Step = 1 | 2;

function Stepper({ step }: { step: Step }) {
  const items = [
    { n: 1 as Step, label: "بيانات الطلب" },
    { n: 2 as Step, label: "تأكيد بيانات الطلب" },
  ];
  return (
    <ol className="grid grid-cols-2 gap-3" aria-label="خطوات إنشاء الطلب">
      {items.map((it) => {
        const done = step > it.n;
        const current = step === it.n;
        return (
          <li key={it.n} aria-current={current ? "step" : undefined} className="space-y-2">
            <div className={cn("h-1 rounded-full", done ? "bg-brand-blue/60" : current ? "bg-brand-blue" : "bg-line")} />
            <div className={cn("flex items-center gap-1.5 text-sm", current || done ? "text-ink" : "text-disabled")}>
              {done ? (
                <RadiocheckIcon className="size-4 text-focus-ring" />
              ) : current ? (<RadioIcon className="size-4"/> ): (<RadioemptyIcon/>

              )}
              {it.label}
            </div>
          </li>
        );
      })}
    </ol>
  );
}

function Row({ label, value, strong }: { label: string; value: string; strong?: boolean }) {
  return (
    <div className="flex items-center justify-between gap-4 py-1.5">
      <dt className="text-xs text-secondary">{label}</dt>
      <dd className={cn("text-sm text-ink", strong && "font-semibold")}>{value}</dd>
    </div>
  );
}

type Props = {
  open: boolean;
  onClose: () => void;
  onCreated: (r: AccessRequest) => void;
};

export function CreateRequestModal({ open, onClose, onCreated }: Props) {
  const [step, setStep] = useState<Step>(1);
  const [workplaceId, setWorkplaceId] = useState("");
  const [type, setType] = useState<AccessType>("normal");
  const [ack, setAck] = useState(false);
  const [nationalId, setNationalId] = useState("");
  const [errors, setErrors] = useState<{ workplace?: string; nationalId?: string; ack?: string }>({});
  const [patient, setPatient] = useState<PatientLookup | null>(null);
  const [submitError, setSubmitError] = useState("");
  const [busy, setBusy] = useState(false);
  const [openedAt, setOpenedAt] = useState(() => toLocalIso(new Date()));

  const workplace = WORKPLACES.find((w) => w.id === workplaceId);

  const reset = () => {
    setStep(1);
    setWorkplaceId("");
    setType("normal");
    setAck(false);
    setNationalId("");
    setErrors({});
    setPatient(null);
    setSubmitError("");
    setBusy(false);
    setOpenedAt(toLocalIso(new Date()));
  };

  const close = () => {
    if (busy) return;
    onClose();
    // wait for the unmount before resetting so there is no flash of step 1
    setTimeout(reset, 0);
  };

  const next = async () => {
    const e = {
      workplace: workplaceId ? undefined : WORKPLACE_REQUIRED,
      nationalId: validateNationalId(nationalId),
      ack: type === "emergency" && !ack ? ACK_REQUIRED : undefined,
    };
    setErrors(e);
    if (e.workplace || e.nationalId || e.ack) return;

    setBusy(true);
    const found = await lookupPatient(nationalId);
    setBusy(false);
    if (!found) {
      setErrors({ nationalId: NOT_FOUND_ERROR });
      return;
    }
    setPatient(found);
    setSubmitError("");
    setOpenedAt(toLocalIso(new Date()));
    setStep(2);
  };

  const submit = async () => {
    if (busy) return;
    setBusy(true);
    setSubmitError("");
    const res = await createRequest({ workplaceId, type, nationalId });
    setBusy(false);

    if (res.ok) {
      onCreated(res.request);
      onClose();
      setTimeout(reset, 0);
      return;
    }
    setSubmitError(
      res.reason === "duplicate" ? DUPLICATE_ERROR : res.reason === "not_found" ? NOT_FOUND_ERROR : GENERIC_ERROR,
    );
  };

  return (
    <Modal
      open={open}
      onClose={close}
      locked={busy}
      title="إنشاء طلب وصول جديد"
      description="أرسل طلب وصول إلى السجل الطبي لمريض عبر جهة عملك الحالية"
    >
      <div className="space-y-6">
        <Stepper step={step} />

        {step === 1 ? (
          <form
            noValidate
            onSubmit={(e) => {
              e.preventDefault();
              next();
            }}
            className="space-y-5"
          >
            <Field id="cr-workplace" label="جهة العمل" required error={errors.workplace}>
              <Select
                id="cr-workplace"
                value={workplaceId}
                invalid={Boolean(errors.workplace)}
                onChange={(e) => {
                  setWorkplaceId(e.target.value);
                  setErrors((p) => ({ ...p, workplace: undefined }));
                }}
                placeholder="اختر جهة العمل التي ترسل منها الطلب.."
                options={WORKPLACES.map((w) => ({ value: w.id, label: w.name }))}
              />
            </Field>

            <fieldset className="space-y-2">
              <legend className="mb-2 text-sm font-normal text-ink">
                نوع الوصول<span className="ms-1 text-danger" aria-hidden>*</span>
              </legend>
              {TYPES.map((t) => {
                const selected = type === t.id;
                return (
                  <label
                    key={t.id}
                    className={cn(
                      "flex cursor-pointer text-sm font-normal items-start gap-3 rounded-lg border p-3 transition-colors",
                      "focus-within:ring-2 focus-within:ring-brand-blue/30",
                      selected ? "border-brand-blue bg-[#D9E5EF]" : "border-line bg-white hover:bg-surface",
                    )}
                  >
                    <input
                      type="radio"
                      name="access-type"
                      value={t.id}
                      checked={selected}
                      onChange={() => {
                        setType(t.id);
                        setErrors((p) => ({ ...p, ack: undefined }));
                      }}
                      className="mt-1 size-4 shrink-0 accent-brand-blue"
                    />
                    <span className="space-y-1">
                      <span className="block text-sm font-semibold text-ink">
                        {t.title}
                        {t.hint && <span className="ms-1 font-normal text-sm text-secondary">{t.hint}</span>}
                      </span>
                      <span className="block text-xs font-normal text-secondary">{t.desc}</span>
                    </span>
                  </label>
                );
              })}

              {type === "emergency" && (
                <div className="sj-fade-in pt-1">
                  <label className="flex cursor-pointer font-semibold items-start gap-2.5 text-xs leading-5 text-secondary">
                  <input
                    type="checkbox"
                    checked={ack}
                    onChange={(e) => {
                      setAck(e.target.checked);
                      setErrors((p) => ({ ...p, ack: undefined }));
                    }}
                    className="mt-0.5 size-[18px] shrink-0 appearance-none
                      rounded-[1px] border-2 border-[#D1D5DB]
                      bg-white cursor-pointer relative
                      checked:border-brand-blue
                      checked:bg-checkbox
                      checked:after:content-['']
                      checked:after:block
                      checked:after:w-[5px]
                      checked:after:h-[11px]
                      checked:after:border-r-[2px]
                      checked:after:border-b-[2px]
                      checked:after:border-brand-blue
                      checked:after:rotate-45
                      checked:after:mx-auto
                      checked:after:my-auto
                    "
                  />
                    {EMERGENCY_ACK}
                  </label>
                  {errors.ack && (
                    <p role="alert" className="mt-1 text-xs text-danger">{errors.ack}</p>
                  )}
                </div>
              )}
            </fieldset>

            <Field id="cr-national-id" label="الرقم القومي للمريض" required error={errors.nationalId}>
              <TextInput
                id="cr-national-id"
                numeric
                maxLength={14}
                value={nationalId}
                invalid={Boolean(errors.nationalId)}
                onChange={(e) => {
                  setNationalId(toEnglishDigits(e.target.value).slice(0, 14));
                  setErrors((p) => ({ ...p, nationalId: undefined }));
                }}
                placeholder="أدخل الرقم القومي الخاص بالمريض"
              />
            </Field>

            <div className="grid grid-cols-2 gap-3 pt-1">
              <Button type="submit" variant="green" disabled={busy}>
                {busy ? "جارٍ التحقق..." : "التالي"}
              </Button>
              <Button type="button" variant="soft" onClick={close} disabled={busy}>
                إلغاء
              </Button>
            </div>
          </form>
        ) : (
          <div className="space-y-5">
            <dl className="divide-y divide-border-disabled/70 rounded-lg bg-surface px-4 py-2">
              <Row label="اسم الطبيب" value={CURRENT_DOCTOR.name} strong />
              <Row label="التخصص" value={CURRENT_DOCTOR.specialty} strong />
              <Row label="جهة العمل" value={workplace?.name ?? ""} strong />
              <Row label="اليوم والتاريخ" value={formatTableDate(openedAt)} />
            </dl>

            <dl className="rounded-lg bg-surface px-4 py-2">
              <Row label="اسم المريض" value={patient?.maskedName ?? ""} />
            </dl>

            {submitError && (
              <div role="alert" className="flex items-center gap-2 rounded-lg border border-be-error/40 bg-[#fef2f2] px-3 py-2.5 text-xs text-be-error">
                <LuCircleAlert className="size-5 shrink-0" />
                {submitError}
              </div>
            )}

            <div className="grid grid-cols-2 gap-3">
              <Button type="button" variant="green" onClick={submit} disabled={busy}>
                {busy ? "جارٍ الإرسال..." : "إرسال طلب الوصول"}
              </Button>
              <Button
                type="button"
                variant="soft"
                disabled={busy}
                onClick={() => {
                  setSubmitError("");
                  setStep(1);
                }}
              >
                الخطوة السابقة
              </Button>
            </div>
          </div>
        )}
      </div>
    </Modal>
  );
}
