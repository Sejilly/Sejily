import { apiFetch } from "./client";
import type { RegisterDoctorRequest, RegisterDoctorResponse } from "./types";

export const registerDoctor = (data: RegisterDoctorRequest) =>
  apiFetch<RegisterDoctorResponse>("/api/Auth/register/doctor", { method: "POST", body: data });
