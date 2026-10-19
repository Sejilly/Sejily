import TextField, { type TextFieldProps } from "./TextField";
import { FiPhoneIncoming } from "react-icons/fi";

type PhoneFieldProps = Omit<TextFieldProps, "type" | "leading" | "trailing">;

export default function PhoneField({ className = "", ...props }: PhoneFieldProps) {
    return (
        <TextField
            type="tel"
            inputMode="numeric"
            maxLength={11}
            dir="ltr"
            leading={
                <>
                    <FiPhoneIncoming size={18} />
                    <span className="h-6 w-[1.25px] bg-[#CBD5E1]" />
                </>
            }
            className={className}
            {...props}
        />
    );
}