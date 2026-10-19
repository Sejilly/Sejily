import type { ReactNode } from "react";
import { Button } from "./Button";

type Props = {
    loading: boolean;
    loadingText: string;
    children: ReactNode;
    disabled?: boolean;
};

export default function SubmitButton({ loading, loadingText, children, disabled }: Props) {
    return (
        <Button type="submit" disabled={loading || disabled} aria-busy={loading}>
            {loading ? loadingText : children}
        </Button>
    );
}