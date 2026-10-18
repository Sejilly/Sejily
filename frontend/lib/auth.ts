
import { api, toApiFailure } from "./api";

type NetworkStatus = number | null;

function isNetworkFailure(status: NetworkStatus): boolean {
  return status === null || status === 502;
}

// -------------------- Login --------------------

export type LoginResult =
  | { ok: true }
  | {
      ok: false;
      reason: "invalid_credentials" | "too_many" | "server" | "network";
    };

export async function login(
  nationalId: string,
  password: string
): Promise<LoginResult> {
  try {
    await api.post("/auth/login", { nationalId, password });

    return { ok: true };
  } catch (err: unknown) {
    const { status } = toApiFailure(err);

    if (isNetworkFailure(status)) {
      return { ok: false, reason: "network" };
    }

    if (status === 429) {
      return { ok: false, reason: "too_many" };
    }

    if (status === 400 || status === 401) {
      return { ok: false, reason: "invalid_credentials" };
    }

    return { ok: false, reason: "server" };
  }
}

// -------------------- Password Reset Request --------------------

export type ResetRequestResult =
  | { ok: true; requestId: string }
  | { ok: false; reason: "too_many" | "server" | "network" };

export async function requestPasswordReset(
  nationalId: string,
  phone: string
): Promise<ResetRequestResult> {
  try {
    const { data } = await api.post<{ requestId?: unknown }>(
      "/auth/forgot-password",
      { nationalId, phone }
    );

    // تأكد أن الـ Backend رجّع requestId صالحًا.
    if (
      typeof data?.requestId !== "string" ||
      data.requestId.trim().length === 0
    ) {
      return { ok: false, reason: "server" };
    }

    return {
      ok: true,
      requestId: data.requestId,
    };
  } catch (err: unknown) {
    const { status } = toApiFailure(err);

    if (isNetworkFailure(status)) {
      return { ok: false, reason: "network" };
    }

    if (status === 429) {
      return { ok: false, reason: "too_many" };
    }

    return { ok: false, reason: "server" };
  }
}

// -------------------- Verify OTP --------------------

export type VerifyOtpResult =
  | { ok: true }
  | { ok: false; reason: "invalid" | "server" | "network" };

export async function verifyOtp(
  requestId: string,
  code: string
): Promise<VerifyOtpResult> {
  try {
    await api.post("/auth/verify-otp", { requestId, code });

    return { ok: true };
  } catch (err: unknown) {
    const { status } = toApiFailure(err);

    if (isNetworkFailure(status)) {
      return { ok: false, reason: "network" };
    }

    if (status === 400) {
      return { ok: false, reason: "invalid" };
    }

    return { ok: false, reason: "server" };
  }
}

// -------------------- Resend OTP --------------------

export type ResendOtpResult =
  | { ok: true }
  | {
      ok: false;
      reason: "too_many" | "restart" | "server" | "network";
    };

export async function resendOtp(
  requestId: string
): Promise<ResendOtpResult> {
  try {
    await api.post("/auth/resend-otp", { requestId });

    return { ok: true };
  } catch (err: unknown) {
    const { status } = toApiFailure(err);

    if (isNetworkFailure(status)) {
      return { ok: false, reason: "network" };
    }

    if (status === 429) {
      return { ok: false, reason: "too_many" };
    }

    // وفقًا لعقد الـ Backend:
    // 400: الطلب منتهي أو مستخدم.
    // 503: فشل الإرسال وتم حذف الطلب.
    if (status === 400 || status === 503) {
      return { ok: false, reason: "restart" };
    }

    return { ok: false, reason: "server" };
  }
}

// -------------------- Reset Password --------------------

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
  } catch (err: unknown) {
    const { status } = toApiFailure(err);

    if (isNetworkFailure(status)) {
      return { ok: false, reason: "network" };
    }

    // نفترض أن الـ Backend يستخدم 410 لانتهاء صلاحية الطلب.
    if (status === 410) {
      return { ok: false, reason: "expired" };
    }

    // نفترض أن 400 و422 يُستخدمان لرفض كلمة المرور.
    if (status === 400 || status === 422) {
      return { ok: false, reason: "weak" };
    }

    return { ok: false, reason: "server" };
  }
}
