export interface PersonalData {
  nationalId: string;
  fullName: string;
  address: string;
  birthDate: string;
}
export interface ProfessionalData {
  syndicateNumber: string;
  licenseNumber: string;
  specialty: string;
}
export interface WorkplaceValues {
  type: string;
  name: string;
  address: string;
  licenseNumber: string;
}
export interface Workplace extends WorkplaceValues {
  id: string;
}
export interface AccountData {
  phone: string;
  email: string;
  password: string;
  confirmPassword: string;
}
export interface RegisterData {
  personal: PersonalData;
  professional: ProfessionalData;
  workplaces: Workplace[];
  account: AccountData;
}
