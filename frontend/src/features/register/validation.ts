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

export function validateProfessional(v: ProfessionalData): Errors<ProfessionalData> {
  return {
    syndicateNumber: req(v.syndicateNumber),
    licenseNumber: req(v.licenseNumber),
    specialty: req(v.specialty),
  };
}

export function validateWorkplace(v: WorkplaceValues): Errors<WorkplaceValues> {
  return {
    type: req(v.type),
    name: req(v.name),
    address: req(v.address),
    licenseNumber: req(v.licenseNumber),
  };
}

export function validateAccount(v: AccountData): Errors<AccountData> {
  return {
    phone: !v.phone.trim()
      ? REQUIRED
      : /^01[0125]\d{8}$/.test(v.phone.replace(/\s/g, ""))
        ? undefined
        : "برجاء إدخال رقم هاتف صحيح",
    email:
      v.email && !/^\S+@\S+\.\S+$/.test(v.email) ? "البريد الإلكتروني غير صحيح" : undefined,
    password: !v.password
      ? REQUIRED
      : v.password.length < 8 ||
          !/\p{L}/u.test(v.password) ||
          !/\d/.test(v.password)
        ? "كلمة السر يجب أن تكون على الأقل 8 خانات من حروف و أرقام"
        : undefined,
    confirmPassword: !v.confirmPassword
      ? REQUIRED
      : v.confirmPassword !== v.password
        ? "كلمتا المرور غير متطابقتين"
        : undefined,
  };
}
