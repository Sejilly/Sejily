import { useState } from "react";

type Validator<T> = (value: string, all: T) => string | undefined;

export function useFormFields<K extends string>(
    initial: Record<K, string>,
    validators: Record<K, Validator<Record<K, string>>>,
    ids: Record<K, string>,
    liveValidators: Partial<Record<K, Validator<Record<K, string>>>> = {}
) {
    const order = Object.keys(initial) as K[];
    const [values, setValues] = useState(initial);
    const [errors, setErrors] = useState<Partial<Record<K, string>>>({});

    const focusField = (k: K) => document.getElementById(ids[k])?.focus();

    const onChange = (k: K, v: string) => {
        const next = { ...values, [k]: v };
        setValues(next);
        setErrors((prev) => {
            const updated = { ...prev };
            for (const key of order) {
                if (prev[key]) {
                    const error = validators[key](next[key], next);
                    if (error) updated[key] = error;
                    else delete updated[key];
                } else if (key === k) {
                    const live = liveValidators[key]?.(next[key], next);
                    if (live) updated[key] = live;
                }
            }
            return updated;
        });
    };

    const onBlur = (k: K, v: string) =>
        setErrors((prev) => {
            const updated = { ...prev };
            const error = validators[k](v, { ...values, [k]: v });
            if (error) updated[k] = error;
            else delete updated[k];
            return updated;
        });

    const validateAll = () => {
        const next: Partial<Record<K, string>> = {};
        for (const k of order) {
            const error = validators[k](values[k], values);
            if (error) next[k] = error;
        }
        setErrors(next);
        const firstBad = order.find((k) => next[k]);
        if (firstBad) {
            focusField(firstBad);
            return false;
        }
        return true;
    };

    return { values, errors, setErrors, onChange, onBlur, validateAll, focusField };
}