
import type {
  AccountData,
  PersonalData,
  ProfessionalData,
  WorkplaceValues,
} from "./types";

export type Errors<T> = Partial<Record<keyof T, string>>;

const REQUIRED = "هذا الحقل إلزامى، يرجى استكمال البيانات";

const req = (v: string) => (v.trim() ? undefined : REQUIRED);

export function validatePersonal(v: PersonalData): Errors<PersonalData> {
  return {
    nationalId: !v.nationalId.trim()
      ? REQUIRED
      : /^\d{14}$/.test(v.nationalId)
        ? undefined
        : "رقم قومى خاطئ، يرجى إعادة الادخال",

    fullName: req(v.fullName),
    address: req(v.address),
    birthDate: req(v.birthDate),
  };
}

export function validateProfessional(
  v: ProfessionalData,
): Errors<ProfessionalData> {
  return {
    syndicateNumber: req(v.syndicateNumber),
    licenseNumber: req(v.licenseNumber),
    specialty: req(v.specialty),
  };
}

export function validateWorkplace(
  v: WorkplaceValues,
): Errors<WorkplaceValues> {
  return {
    type: req(v.type),
    name: req(v.name),
    address: req(v.address),
    licenseNumber: req(v.licenseNumber),
  };
}

export const PASSWORD_REQUIREMENTS = [
  {
    id: "length",
    label: "يجب أن تحتوى على 8 رموز على الأقل.",
    test: (password: string) => password.length >= 8,
  },
  {
    id: "number",
    label: "يجب ان تحتوى على رقم واحد على الأقل.",
    test: (password: string) => /[0-9]/.test(password),
  },
  {
    id: "uppercase",
    label: "حرف إنجليزي كبير واحد على الأقل (A-Z).",
    test: (password: string) => /[A-Z]/.test(password),
  },
  {
    id: "lowercase",
    label: "حرف إنجليزي صغير واحد على الأقل (a-z).",
    test: (password: string) => /[a-z]/.test(password),
  },

  {
    id: "special",
    label: "رمز خاص واحد على الأقل (مثل !@#$%).",
    test: (password: string) => /[^A-Za-z0-9]/.test(password),
  },
] as const;

export function getPasswordRequirements(password: string) {
  return PASSWORD_REQUIREMENTS.map((requirement) => ({
    ...requirement,
    passed: requirement.test(password),
  }));
}

export function validateAccount(v: AccountData): Errors<AccountData> {
  return {
    phone: !v.phone.trim()
      ? REQUIRED
      : /^01[0125]\d{8}$/.test(v.phone.replace(/\s/g, ""))
        ? undefined
        : "برجاء إدخال رقم هاتف صحيح",

    email:
      v.email && !/^\S+@\S+\.\S+$/.test(v.email)
        ? "البريد الإلكتروني غير صحيح"
        : undefined,

    password: !v.password
      ? REQUIRED
      : getPasswordRequirements(v.password).some(
            (requirement) => !requirement.passed,
          )
        ? "كلمة المرور لا تحقق جميع الشروط المطلوبة"
        : undefined,

    confirmPassword: !v.confirmPassword
      ? REQUIRED
      : v.confirmPassword !== v.password
        ? "كلمتا المرور غير متطابقتين"
        : undefined,
  };
}
