import type { ReactNode } from "react";

export default function TextDivider({ children }: { children: ReactNode }) {
    return (
        <div className="flex w-full items-center gap-5">
        <span className="h-px flex-1 bg-line" />
        <span className="text-sm leading-5.5 whitespace-nowrap text-disabled">
            {children}
        </span>
        <span className="h-px flex-1 bg-line" />
        </div>
    );
}
