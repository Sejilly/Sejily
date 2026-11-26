"use client";

import { useSyncExternalStore } from "react";

/**
 * SSR-safe media query hook. The server (and the first client render) report
 * `false`, so hydration never mismatches; it then updates to the real value.
 */
export function useMediaQuery(query: string): boolean {
  return useSyncExternalStore(
    (notify) => {
      const mql = window.matchMedia(query);
      mql.addEventListener("change", notify);
      return () => mql.removeEventListener("change", notify);
    },
    () => window.matchMedia(query).matches,
    () => false,
  );
}
