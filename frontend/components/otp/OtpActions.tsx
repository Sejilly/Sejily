import { Button } from "../ui/Button";
import FormAlert from "../ui/FormAlert";
import SubmitButton from "../ui/SubmitButton";

type Props = {
    time: string;
    expired: boolean;
    verifying: boolean;
    resending: boolean;
    resendLeft: number;
    formError: string;
    onResend: () => void;
};

export default function OtpActions({
    time, expired, verifying, resending, resendLeft, formError, onResend,
}: Props) {
    return (
        <div className="flex w-full flex-col items-center gap-5">
            <p className="text-center text-xs leading-4.5 font-normal text-[#475569]">
                {expired ? (
                    "انتهت صلاحية هذا الرمز"
                ) : (
                    <>
                        تنتهي صلاحية هذا الرمز خلال{" "}
                        <span dir="ltr" className="inline-block tabular-nums">
                            {time}
                        </span>
                    </>
                )}
            </p>

            {formError && <FormAlert message={formError} />}

            <div className="flex w-full flex-col gap-4">
                <SubmitButton loading={verifying} loadingText="جارٍ التحقق..." disabled={resending}>
                    التالي
                </SubmitButton>
                <Button
                    type="button"
                    variant="secondary"
                    className="font-normal!"
                    disabled={verifying || resending || resendLeft > 0}
                    onClick={onResend}
                >
                    {resending
                        ? "جارٍ الإرسال..."
                        : resendLeft > 0
                        ? `إعادة الإرسال بعد ${resendLeft} ثانية`
                        : "إعادة إرسال رمز التحقق"}
                </Button>
            </div>
        </div>
    );
}