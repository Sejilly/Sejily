import type { ReactNode } from "react";

export default function AuthCard({ children }: { children: ReactNode }) {
    return (
        <div className="mx-auto flex w-full max-w-138 flex-col  gap-8 rounded-2xl
            bg-white p-5 shadow-[0px_0px_10px_rgba(56,161,105,0.15)] sm:gap-11 lg:p-8 lg:mt-14.5
            lg:min-h-0 lg:shrink lg:[@media(max-height:785px)]:mt-2 lg:[@media(max-height:850px)]:gap-8 lg:[@media(max-height:850px)]:p-6">
            {children}
        </div>
    );
}