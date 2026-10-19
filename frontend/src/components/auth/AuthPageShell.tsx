import type { ReactNode } from "react";
import FormFooter from "../login/FormFooter";

type Props = { header: ReactNode; children: ReactNode };

export default function AuthPageShell({ header, children }: Props) {
    return (
        <div className="flex min-h-dvh flex-col bg-[#F8FAFC] px-4 sm:px-10
            lg:h-dvh lg:overflow-hidden lg:px-20 lg:py-12 lg:[@media(max-height:850px)]:py-6">
            {header}
            <main className="mt-6 flex flex-col sm:mt-0
                lg:min-h-0 lg:flex-1 max-sm:[@media(min-height:800px)]:justify-center">
                {children}
                <div className="flex justify-center py-4 lg:min-h-12 lg:py-0 lg:mt-3">
                    <FormFooter />
                </div>
            </main>
        </div>
    );
}