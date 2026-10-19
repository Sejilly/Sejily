export const NATIONAL_ID_ERROR = "رقم قومى خاطئ، يرجى إعادة الادخال";
export const NATIONAL_ID_INCOMPLETE_ERROR = "الرقم القومي غير مكتمل";
export const NATIONAL_ID_REQUIRED_ERROR = "الرقم القومي مطلوب";

export const PASSWORD_REQUIRED_ERROR = "كلمة المرور مطلوبة";
export const PASSWORD_ERROR =
  "كلمة السر يجب أن تكون على الأقل 8 خانات من حروف و أرقام";
export const WRONG_PASSWORD_ERROR = "الرقم القومي أو كلمة السر غير صحيحة";

export const PHONE_ERROR = "برجاء إدخال رقم تليفون صحيح";
export const PHONE_REQUIRED_ERROR = "رقم الهاتف مطلوب";
export const PHONE_INCOMPLETE_ERROR = "رقم الهاتف غير مكتمل";

export const GENERIC_ERROR = "حدث خطأ غير متوقع، يرجى المحاولة مرة أخرى";
export const NETWORK_ERROR = "تعذر الاتصال بالخادم، تحقق من اتصالك بالإنترنت";
export const RESET_MISMATCH_ERROR = "البيانات المدخلة غير مطابقة لأي حساب";

export const OTP_INCOMPLETE_ERROR = "أدخل رمز التحقق المكون من 6 أرقام";
export const OTP_INVALID_ERROR = "رمز التحقق غير صحيح أو منتهي الصلاحية";
export const OTP_RESTART_ERROR = "انتهت صلاحية الطلب، يرجى البدء من جديد";
export const TOO_MANY_ERROR = "تم تجاوز عدد المحاولات، يرجى المحاولة لاحقاً";
export const RESEND_WAIT_ERROR = "يرجى الانتظار قبل طلب رمز جديد";
export const RESET_SESSION_EXPIRED_ERROR = "انتهت صلاحية الجلسة، يرجى إعادة طلب رمز التحقق";

export const PASSWORD_CONFIRM_REQUIRED_ERROR = "يرجى تأكيد كلمة المرور";
export const PASSWORD_MISMATCH_ERROR = "كلمة المرور غير متطابقة";


export const phoneLiveError = (v: string) =>
  v.length > 0 && validatePhone(v) === PHONE_ERROR ? PHONE_ERROR : undefined;

export const validateConfirmPassword = (confirm: string, password: string) => {
  if (confirm.length === 0) return PASSWORD_CONFIRM_REQUIRED_ERROR;
  return confirm === password ? undefined : PASSWORD_MISMATCH_ERROR;
};

export const toEnglishDigits = (v: string) =>
  v
    .replace(/[\u0660-\u0669]/g, (d) => String(d.charCodeAt(0) - 0x0660))
    .replace(/[\u06F0-\u06F9]/g, (d) => String(d.charCodeAt(0) - 0x06f0))
    .replace(/\D/g, "");

export function getPasswordRules(value: string) {
  return [
    { id: "length", label: "يجب أن تحتوى على 8 رموز على الأقل.", passed: value.length >= 8 },
    { id: "letters", label: "يجب أن تحتوي على حروف.", passed: /\p{L}/u.test(value) },
    { id: "digits", label: "يجب أن تحتوي على أرقام.", passed: /\p{Nd}/u.test(value) },
  ];
}

export const validateNationalId = (v: string) => {
  if (v.length === 0) return NATIONAL_ID_REQUIRED_ERROR;
  if (/^\d{14}$/.test(v)) return undefined;
  if (/^\d{1,13}$/.test(v)) return NATIONAL_ID_INCOMPLETE_ERROR; // غير مكتمل
  return NATIONAL_ID_ERROR;
};

export const validatePassword = (v: string) => {
  if (v.length === 0) return PASSWORD_REQUIRED_ERROR;
  return getPasswordRules(v).every((r) => r.passed) ? undefined : PASSWORD_ERROR;
};


const VALID_PREFIXES = ["010", "011", "012", "015"];

export const validatePhone = (v: string) => {
  if (v.length === 0) return PHONE_REQUIRED_ERROR;
  if (/^01[0125]\d{8}$/.test(v)) return undefined;

  const head = v.slice(0, 3);
  const startsWrong =
    (v.length >= 1 && v[0] !== "0") ||
    (v.length >= 2 && !v.startsWith("01")) ||
    (v.length >= 3 && !VALID_PREFIXES.includes(head));
  if (startsWrong) return PHONE_ERROR;

  if (v.length < 11) return PHONE_INCOMPLETE_ERROR;

  return PHONE_ERROR;
};

