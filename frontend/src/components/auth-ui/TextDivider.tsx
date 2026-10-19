import type { ReactNode } from "react";

export default function TextDivider({ children }: { children: ReactNode }) {
    return (
        <div className="flex w-full items-center gap-5">
        <span className="h-px flex-1 bg-[#CBD5E1]" />
        <span className="text-sm leading-5.5 whitespace-nowrap text-[#94A3B8]">
            {children}
        </span>
        <span className="h-px flex-1 bg-[#CBD5E1]" />
        </div>
    );
}
