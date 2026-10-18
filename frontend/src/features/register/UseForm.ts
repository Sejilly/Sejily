"use client";

import { useState, type ChangeEvent, type FormEvent } from "react";
import type { Errors } from "./validation";

/** Tiny controlled-form helper (swap for react-hook-form + zod if the team prefers). */
export function useForm<T extends object>(
  initial: T,
  validate: (values: T) => Errors<T>,
) {
  const [values, setValues] = useState(initial);
  const [errors, setErrors] = useState<Errors<T>>({});

  const bind = (key: keyof T) => ({
    value: values[key] as string,
    onChange: (e: ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
      setValues((v) => ({ ...v, [key]: e.target.value }));
      setErrors((p) => ({ ...p, [key]: undefined }));
    },
    invalid: Boolean(errors[key]),
  });

  const handleSubmit = (onValid: (values: T) => void) => (e: FormEvent) => {
    e.preventDefault();
    const next = validate(values);
    setErrors(next);
    if (!Object.values(next).some(Boolean)) onValid(values);
  };

  const reset = () => {
    setValues(initial);
    setErrors({});
  };

  const setFieldValue = <K extends keyof T>(
  key: K,
  value: T[K],
) => {
  setValues((v) => ({ ...v, [key]: value }));
  setErrors((p) => ({ ...p, [key]: undefined }));
};

  return {
  values,
  errors,
  bind,
  handleSubmit,
  reset,
  setFieldValue,
};
}
