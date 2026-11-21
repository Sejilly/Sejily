"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { CheckIcon } from "@/components/icons";
import { Field } from "@/components/ui/Field";
import { TextInput } from "@/components/ui/TextInput";
import { FormHeader } from "@/components/register/FormHeader";
import { useRegister } from "@/features/register/RegisterContext";
import { RegisterShell } from "@/components/register/RegisterShell";

export default function VerifyingPage() {
  const router = useRouter();
  const { data } = useRegister();
  const [verifiedCount, setVerifiedCount] = useState(0);

  useEffect(() => {
    const timers = [
      setTimeout(() => setVerifiedCount(1), 1000),
      setTimeout(() => setVerifiedCount(2), 2000),
      setTimeout(() => setVerifiedCount(3), 3000),
      setTimeout(() => router.replace("/success"), 5000),
    ];

    return () => timers.forEach(clearTimeout);
  }, [router]);

  const fields = [
    {
      id: "v-nid",
      label: "الرقم القومي",
      value: data.personal.nationalId,
    },
    {
      id: "v-syn",
      label: "رقم قيد النقابة",
      value: data.professional.syndicateNumber,
    },
    {
      id: "v-lic",
      label: "رقم ترخيص مزاولة المهنة",
      value: data.professional.licenseNumber,
    },
  ];

  return (
    <RegisterShell>
      <div aria-busy={verifiedCount < 3} aria-live="polite">
        <FormHeader
          title="جاري التحقق..."
          description="جاري التحقق من الهوية والتراخيص لإتمام إنشاء الحساب."
        />

        <div className="space-y-5">
          {fields.map((field, index) => {
            const isVerified = index < verifiedCount;

            return (
              <div key={field.id} className="[&_label]:text-disabled!">
                <Field id={field.id} label={field.label} >
                  <TextInput
                    id={field.id}
                    disabled
                    value={field.value}
                    success={isVerified}
                    adornmentPosition="end"
                  
                    endAdornment={
                      isVerified ? (
                        <span className="flex size-8 items-center justify-center rounded-full bg-success text-white">
                          <CheckIcon width={30} height={30}/>
                        </span>
                      ) : undefined
                    }
                  />
                </Field>
              </div>
            );
          })}
        </div>

        
      </div>
    </RegisterShell>
  );
}