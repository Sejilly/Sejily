"use client";

import { useEffect } from "react";
import { LuCircleCheck } from "react-icons/lu";

export function Toast({ message, onDone }: { message: string; onDone: () => void }) {
  useEffect(() => {
    const t = setTimeout(onDone, 3500);
    return () => clearTimeout(t);
  }, [message, onDone]);

  return (
    <div role="status" aria-live="polite" className="fixed inset-x-0 bottom-6 z-60 flex justify-center px-4">
      <div className="sj-pop-in flex items-center gap-2 rounded-xl bg-ink px-4 py-3 text-sm text-white shadow-lg">
        <LuCircleCheck className="size-5 text-emerald-400" />
        {message}
      </div>
    </div>
  );
}
