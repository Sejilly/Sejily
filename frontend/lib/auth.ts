import { api, toApiFailure } from "./api";

export type LoginResult =
  | { ok: true }
  | { ok: false; reason: "invalid_credentials" | "too_many" | "server" | "network" };

export async function login(nationalId: string, password: string): Promise<LoginResult> {
  try {
    await api.post("/auth/login", { nationalId, password });
    return { ok: true };
  } catch (err) {
    const { status } = toApiFailure(err);
    if (status === null || status === 502) return { ok: false, reason: "network" };
    if (status === 429) return { ok: false, reason: "too_many" };
    if (status === 400 || status === 401) return { ok: false, reason: "invalid_credentials" };
    return { ok: false, reason: "server" };
  }
}

export type ResetRequestResult =
  | { ok: true; requestId: string }
  | { ok: false; reason: "too_many" | "server" | "network" };

export async function requestPasswordReset(
  nationalId: string,
  phone: string
): Promise<ResetRequestResult> {
  try {
    const { data } = await api.post("/auth/forgot-password", { nationalId, phone });
    return { ok: true, requestId: data.requestId as string };
  } catch (err) {
    const { status } = toApiFailure(err);
    if (status === null || status === 502) return { ok: false, reason: "network" };
    if (status === 429) return { ok: false, reason: "too_many" };
    return { ok: false, reason: "server" };
  }
}

export type VerifyOtpResult =
  | { ok: true }
  | { ok: false; reason: "invalid" | "server" | "network" };

export async function verifyOtp(requestId: string, code: string): Promise<VerifyOtpResult> {
  try {
    await api.post("/auth/verify-otp", { requestId, code });
    return { ok: true };
  } catch (err) {
    const { status } = toApiFailure(err);
    if (status === null || status === 502) return { ok: false, reason: "network" };
    if (status === 400) return { ok: false, reason: "invalid" };
    return { ok: false, reason: "server" };
  }
}

export type ResendOtpResult =
  | { ok: true }
  | { ok: false; reason: "too_many" | "restart" | "server" | "network" };

export async function resendOtp(requestId: string): Promise<ResendOtpResult> {
  try {
    await api.post("/auth/resend-otp", { requestId });
    return { ok: true };
  } catch (err) {
    const { status } = toApiFailure(err);
    if (status === null || status === 502) return { ok: false, reason: "network" };
    if (status === 429) return { ok: false, reason: "too_many" };
    // 400: الطلب منتهي/مستخدم، و503: فشل الإرسال والباك مسح الطلب
    if (status === 400 || status === 503) return { ok: false, reason: "restart" };
    return { ok: false, reason: "server" };
  }
}

export type ResetPasswordResult =
  | { ok: true }
  | { ok: false; reason: "expired" | "weak" | "server" | "network" };

export async function resetPassword(
  requestId: string,
  password: string
): Promise<ResetPasswordResult> {
  try {
    await api.post("/auth/reset-password", { requestId, password });
    return { ok: true };
  } catch (err) {
    const { status } = toApiFailure(err);
    if (status === null || status === 502) return { ok: false, reason: "network" };
    if (status === 400) return { ok: false, reason: "weak" };
    return { ok: false, reason: "server" };
  }
}