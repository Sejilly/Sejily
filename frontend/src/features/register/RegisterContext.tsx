"use client";

import { createContext, useContext, useMemo, useState } from "react";
import type { RegisterData } from "./types";

const initialData: RegisterData = {
  personal: { nationalId: "", fullName: "", address: "", birthDate: "" },
  professional: { syndicateNumber: "", licenseNumber: "", specialty: "" },
  workplaces: [],
  account: { phone: "", email: "", password: "", confirmPassword: "" },
};

interface Ctx {
  data: RegisterData;
  update: <K extends keyof RegisterData>(key: K, value: RegisterData[K]) => void;
}

const RegisterContext = createContext<Ctx | null>(null);

export function RegisterProvider({ children }: { children: React.ReactNode }) {
  const [data, setData] = useState(initialData);
  const value = useMemo<Ctx>(
    () => ({ data, update: (key, v) => setData((d) => ({ ...d, [key]: v })) }),
    [data],
  );
  return <RegisterContext.Provider value={value}>{children}</RegisterContext.Provider>;
}

export function useRegister() {
  const ctx = useContext(RegisterContext);
  if (!ctx) throw new Error("useRegister must be used inside <RegisterProvider>");
  return ctx;
}
