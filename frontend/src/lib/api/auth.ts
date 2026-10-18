import { apiFetch } from "./client";
import type { RegisterDoctorRequest, RegisterDoctorResponse } from "./types";

/** POST /api/Auth/register/doctor */
export const registerDoctor = (data: RegisterDoctorRequest) =>
  apiFetch<RegisterDoctorResponse>("/api/Auth/register/doctor", { method: "POST", body: data });
