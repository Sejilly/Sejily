export const STEPS = [
  { key: "personal", label: "البيانات الشخصية", href: "/register/personal" },
  { key: "professional", label: "البيانات المهنية", href: "/register/professional" },
  { key: "workplaces", label: "بيانات أماكن العمل", href: "/register/workplaces" },
  { key: "account", label: "بيانات الحساب", href: "/register/account" },
] as const;

export const VERIFYING_HREF = "/register/verifying";

// TODO: replace with data from the API
export const SPECIALTIES = [
  "باطنة",
  "جراحة عامة",
  "أطفال",
  "نساء وتوليد",
  "عظام",
  "أسنان",
  "صيدلة",
].map((l) => ({ value: l, label: l }));

/*export const WORKPLACE_TYPES = [
  "مستشفى حكومي",
  "مستشفى خاص",
  "مركز تحاليل طبية",
  "عيادة خاصة",
  "صيدلية",
  "مركز أشعة",
].map((l) => ({ value: l, label: l }));*/
export const WORKPLACE_TYPES = [
  { value: "GovernmentHospital", label: "مستشفى حكومي" },
  { value: "PrivateHospital", label: "مستشفى خاص" },
  { value: "MedicalLab", label: "مركز تحاليل طبية" },
  { value: "PrivateClinic", label: "عيادة خاصة" },
  { value: "Pharmacy", label: "صيدلية" },
  { value: "RadiologyCenter", label: "مركز أشعة" },
];

export const workplaceTypeLabel = (value: string) =>
  WORKPLACE_TYPES.find((t) => t.value === value)?.label ?? value;