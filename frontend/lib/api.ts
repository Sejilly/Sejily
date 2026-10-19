import axios, { isAxiosError } from "axios";

export const api = axios.create({
    baseURL: `${(process.env.NEXT_PUBLIC_API_BASE_URL ?? "").replace(/\/+$/, "")}/api`,
  timeout: 15000,
  headers: { "Content-Type": "application/json" },
});

export type ApiFailure = {
  status: number | null;
};

export function toApiFailure(err: unknown): ApiFailure {
  if (isAxiosError(err)) {
    return { status: err.response?.status ?? null };
  }
  return { status: null };
}