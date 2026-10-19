import { BsCheck2 } from "react-icons/bs";
import { getPasswordRules } from "../../lib/validators";

export default function PasswordRules({ password }: { password: string }) {
    return (
        <ul className="flex w-full flex-col gap-1" aria-live="polite">
            {getPasswordRules(password).map((rule) => (
                <li
                    key={rule.id}
                    className={`flex items-center gap-1 text-right text-xs leading-4.5 ${
                        rule.passed ? "text-[#059669]" : "text-[#94A3B8]"
                    }`}
                >
                    <BsCheck2 size={18} aria-hidden />
                    <span>{rule.label}</span>
                </li>
            ))}
        </ul>
    );
}