import { BsExclamationCircleFill } from "react-icons/bs";

export default function FormAlert({ message }: { message: string }) {
    return (
        <div
            role="alert"
            className="flex w-full items-center gap-2 rounded-lg border border-[#B91C1C]/30 bg-[#FEF2F2] px-4 py-3"
        >
            <BsExclamationCircleFill aria-hidden size={18} className="shrink-0 text-[#DC2626]" />
            <p className="flex-1 text-right text-sm leading-5.5 text-[#B91C1C]">{message}</p>
        </div>
    );
}