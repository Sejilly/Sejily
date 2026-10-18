import type { RegisterDoctorRequest } from "@/lib/api/types";
import type { RegisterData } from "./types";

/**
 * Converts the multi-step form state into the body of POST /api/Auth/register/doctor.
 *  - `email` is not part of the API contract, so it is intentionally not sent.
 *  - `bloodType` is not sent (the guide's doctor example does not include it).
 */
export function toRegisterDoctorRequest(d: RegisterData): RegisterDoctorRequest {
  return {
    nationalID: d.personal.nationalId.trim(),
    fullName: d.personal.fullName.trim(),
    address: d.personal.address.trim(),
    dateOfBirth: d.personal.birthDate, // already "YYYY-MM-DD"
    phoneNumber: d.account.phone.replace(/\s/g, ""),
    password: d.account.password,
    confirmPassword: d.account.confirmPassword,
    syndicateCardNumber: d.professional.syndicateNumber.trim(),
    medicalLicenseNumber: d.professional.licenseNumber.trim(),
    specialization: d.professional.specialty,
    workPlaces: d.workplaces.map((w) => ({
      type: w.type,
      name: w.name.trim(),
      address: w.address.trim(),
      licenseNumber: w.licenseNumber.trim(),
    })),
  };
}

/** True when the earlier steps were skipped (e.g. the user opened /register/account directly). */
export function isRegistrationIncomplete(d: RegisterData): boolean {
  return (
    !d.personal.nationalId ||
    !d.personal.birthDate ||
    !d.professional.licenseNumber ||
    d.workplaces.length === 0
  );
}
