
export interface WorkPlacePayload {
  /** Enum name accepted by the backend's MedicalWorkPlace.type, e.g. "PrivateHospital". */
  type: string;
  name: string;
  address: string;
  licenseNumber: string;
}

export interface RegisterDoctorRequest {
  nationalID: string;
  fullName: string;
  address: string;
  /** ISO date, "YYYY-MM-DD". */
  dateOfBirth: string;
  phoneNumber: string;
  password: string;
  confirmPassword: string;
  syndicateCardNumber: string;
  medicalLicenseNumber: string;
  specialization: string;
  workPlaces: WorkPlacePayload[];
}

export interface RegisterDoctorResponse {
  message: string;
  userId: number;
  doctor: {
    syndicateCardNumber: string;
    medicalLicenseNumber: string;
    specialization: string;
    isVerified: boolean;
  };
  /** Shape depends on IDoctorWorkplaceService — not specified in the guide. */
  workplaces: unknown[];
}
