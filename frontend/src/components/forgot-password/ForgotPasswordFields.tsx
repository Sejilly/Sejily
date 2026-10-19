import TextField from "@/components/auth-ui/TextField";
import PhoneField from "@/components/auth-ui/PhoneField";
import { toEnglishDigits } from "../../lib/validators";

export type ForgotField = "nationalId" | "phone";

type Props = {
    values: Record<ForgotField, string>;
    errors: Partial<Record<ForgotField, string>>;
    onChange: (field: ForgotField, value: string) => void;
    onBlur: (field: ForgotField, value: string) => void;
};

export default function ForgotPasswordFields({ values, errors, onChange, onBlur }: Props) {
    return (
        <div className="flex w-full flex-col gap-5">
            <TextField
                id="national-id"
                name="nationalId"
                label="الرقم القومي"
                type="text"
                inputMode="numeric"
                maxLength={14}
                autoComplete="off"
                value={values.nationalId}
                onChange={(e) => onChange("nationalId", toEnglishDigits(e.target.value))}
                onBlur={(e) => onBlur("nationalId", e.target.value)}
                placeholder="أدخل الرقم القومي"
                error={errors.nationalId}
            />
            <PhoneField
                id="phone"
                name="phone"
                label="رقم الهاتف المحمول"
                autoComplete="tel"
                value={values.phone}
                onChange={(e) => onChange("phone", toEnglishDigits(e.target.value))}
                onBlur={(e) => onBlur("phone", e.target.value)}
                placeholder="01X XXXX XXXX"
                error={errors.phone}
            />
        </div>
    );
}