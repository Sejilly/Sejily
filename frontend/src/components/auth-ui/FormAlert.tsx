import type { ReactNode } from "react";
import { MdErrorOutline } from "react-icons/md";

export default function FormAlert({
    message,
}: {
    message: ReactNode;
}) {
    return (
        <div
            role="alert"
            className="flex w-full items-center gap-2 rounded-lg border border-[#E8A5A5] bg-[#FBF0F0] px-4 py-2.5"
        >
            <MdErrorOutline
                aria-hidden
                size={22}
                className="shrink-0 text-[#DC2626]"
            />

            <p className="flex-1 text-right text-xs leading-5 text-danger">
                {message}
            </p>
        </div>
    );
}