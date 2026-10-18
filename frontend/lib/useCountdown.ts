import { useCallback, useEffect, useState } from "react";

export function useCountdown(seconds: number) {
    const [endsAt, setEndsAt] = useState(() => Date.now() + seconds * 1000);
    const [left, setLeft] = useState(seconds);

    useEffect(() => {
        const tick = () => setLeft(Math.max(0, Math.ceil((endsAt - Date.now()) / 1000)));
        tick();
        const id = setInterval(tick, 1000);
        return () => clearInterval(id);
    }, [endsAt]);

    const restart = useCallback(() => setEndsAt(Date.now() + seconds * 1000), [seconds]);

    return { left, restart };
}

export const formatTime = (s: number) =>
    `${String(Math.floor(s / 60)).padStart(2, "0")}:${String(s % 60).padStart(2, "0")}`;