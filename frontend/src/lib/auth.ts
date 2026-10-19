
import { apiFetch, ApiError } from "./api/client";

const ACCOUNT_TYPE = "Doctor";

type Failure<R extends string> = {
  ok: false;
  reason: R;
};

function statusOf(err: unknown): number | null {
  return err instanceof ApiError ? err.status : null;
}

export type LoginResult =
  | { ok: true }
  | Failure<"invalid_credentials" | "too_many" | "server" | "network">;

export async function login(
  nationalId: string,
  password: string
): Promise<LoginResult> {
  try {
    const data = await apiFetch<{ token: string }>(
      "/api/Auth/login/doctor",
      {
        method: "POST",
        body: { nationalID: nationalId, password },
      }
    );

    if (data?.token) {
      sessionStorage.setItem("sejily_token", data.token);
    }

    return { ok: true };
  } catch (err: unknown) {
    const status = statusOf(err);

    if (status === 0) {
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

export type ResetRequestResult =
  | { ok: true; requestId: string }
  | Failure<"too_many" | "server" | "network">;

export async function requestPasswordReset(
  nationalId: string,
  phone: string
): Promise<ResetRequestResult> {
  try {
    const data = await apiFetch<{ requestId?: string }>(
      "/api/Auth/forgot-password",
      {
        method: "POST",
        body: {
          nationalID: nationalId,
          phoneNumber: phone,
          accountType: ACCOUNT_TYPE,
        },
      }
    );

    if (!data?.requestId) {
      return { ok: false, reason: "server" };
    }

    return { ok: true, requestId: data.requestId };
  } catch (err: unknown) {
    const status = statusOf(err);

    if (status === 0) {
      return { ok: false, reason: "network" };
    }

    if (status === 429) {
      return { ok: false, reason: "too_many" };
    }

    return { ok: false, reason: "server" };
  }
}

export type VerifyOtpResult =
  | { ok: true }
  | Failure<"invalid" | "server" | "network">;

export async function verifyOtp(
  requestId: string,
  code: string
): Promise<VerifyOtpResult> {
  try {
    await apiFetch("/api/Auth/verify-otp", {
      method: "POST",
      body: { requestId, otp: code },
    });

    return { ok: true };
  } catch (err: unknown) {
    const status = statusOf(err);

    if (status === 0) {
      return { ok: false, reason: "network" };
    }

    if (status === 400) {
      return { ok: false, reason: "invalid" };
    }

    return { ok: false, reason: "server" };
  }
}

export type ResendOtpResult =
  | { ok: true }
  | Failure<"too_many" | "restart" | "server" | "network">;

export async function resendOtp(
  requestId: string
): Promise<ResendOtpResult> {
  try {
    await apiFetch("/api/Auth/resend-otp", {
      method: "POST",
      body: { requestId },
    });

    return { ok: true };
  } catch (err: unknown) {
    const status = statusOf(err);

    if (status === 0) {
      return { ok: false, reason: "network" };
    }

    if (status === 429) {
      return { ok: false, reason: "too_many" };
    }

    if (status === 400 || status === 503) {
      return { ok: false, reason: "restart" };
    }

    return { ok: false, reason: "server" };
  }
}

export type ResetPasswordResult =
  | { ok: true }
  | Failure<"expired" | "weak" | "server" | "network">;

export async function resetPassword(
  requestId: string,
  password: string
): Promise<ResetPasswordResult> {
  try {
    await apiFetch("/api/Auth/reset-password", {
      method: "POST",
      body: {
        requestId,
        newPassword: password,
        confirmPassword: password,
      },
    });

    return { ok: true };
  } catch (err: unknown) {
    const status = statusOf(err);

    if (status === 0) {
      return { ok: false, reason: "network" };
    }

    if (status === 410) {
      return { ok: false, reason: "expired" };
    }

    if (status === 400 || status === 422) {
      return { ok: false, reason: "weak" };
    }

    return { ok: false, reason: "server" };
  }
}
