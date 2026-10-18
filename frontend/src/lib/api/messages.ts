
export const MSG_NETWORK = "تعذر الاتصال بالخادم. يرجى التحقق من الإنترنت والمحاولة مرة أخرى.";
export const MSG_SERVER = "حدث خطأ غير متوقع في الخادم. يرجى المحاولة لاحقًا.";
export const MSG_GENERIC = "حدث خطأ ما. يرجى المحاولة مرة أخرى.";
export const MSG_UNAUTHORIZED = "انتهت الجلسة. يرجى تسجيل الدخول مرة أخرى.";
export const MSG_FORBIDDEN = "ليس لديك صلاحية للقيام بهذا الإجراء.";

/** ASP.NET Identity error codes (returned as [{ code, description }]). */
const IDENTITY_CODES: Record<string, string> = {
  DuplicateUserName: "يوجد حساب بهذا الرقم القومي بالفعل.",
  InvalidUserName: "الرقم القومي غير صالح.",
  PasswordTooShort: "كلمة المرور قصيرة جدًا.",
  PasswordRequiresDigit: "كلمة المرور يجب أن تحتوي على رقم واحد على الأقل.",
  PasswordRequiresLower: "كلمة المرور يجب أن تحتوي على حرف إنجليزي صغير (a-z).",
  PasswordRequiresUpper: "كلمة المرور يجب أن تحتوي على حرف إنجليزي كبير (A-Z).",
  PasswordRequiresNonAlphanumeric: "كلمة المرور يجب أن تحتوي على رمز خاص واحد على الأقل (مثل @ أو #).",
  PasswordRequiresUniqueChars: "كلمة المرور تحتاج إلى أحرف مختلفة أكثر.",
};

/** Plain-string messages the API returns (matched case-insensitively). */
const KNOWN_MESSAGES: Array<[RegExp, string]> = [
  [/already exists/i, "يوجد حساب بهذا الرقم القومي بالفعل."],
  [/invalid national id or password/i, "الرقم القومي أو كلمة المرور غير صحيحة."],
  [/invalid or expired otp/i, "رمز التحقق غير صحيح أو منتهي الصلاحية."],
  [/invalid or expired reset request/i, "طلب إعادة التعيين غير صالح أو منتهي. يرجى البدء من جديد."],
  [/wait before requesting another otp/i, "يرجى الانتظار قليلًا قبل طلب رمز جديد."],
  [/unable to send the reset code/i, "تعذر إرسال رمز التحقق. يرجى المحاولة لاحقًا."],
  [/unable to reset the password/i, "تعذر إعادة تعيين كلمة المرور."],
  [/doctor not found/i, "لم يتم العثور على حساب الطبيب."],
  [/account not found or not available/i, "الحساب غير متاح."],
];

export function localizeCode(code: string): string | undefined {
  return IDENTITY_CODES[code];
}

export function localizeMessage(message: string): string {
  for (const [re, ar] of KNOWN_MESSAGES) if (re.test(message)) return ar;
  return message;
}

export const DUPLICATE_ACCOUNT_RE = /already exists|DuplicateUserName/i;
