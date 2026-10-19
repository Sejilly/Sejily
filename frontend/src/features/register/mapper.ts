import type { RegisterDoctorRequest } from "@/lib/api/types";
import type { RegisterData } from "./types";

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

export function isRegistrationIncomplete(d: RegisterData): boolean {
  return (
    !d.personal.nationalId ||
    !d.personal.birthDate ||
    !d.professional.licenseNumber ||
    d.workplaces.length === 0
  );
}
