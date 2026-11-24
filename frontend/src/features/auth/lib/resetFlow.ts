export type ResetFlow = {
  requestId: string;
  phone: string;
  verified?: boolean;
};

const KEY = "reset-flow";

export function readResetFlow(): ResetFlow | null {
  try {
    const raw = sessionStorage.getItem(KEY);
    return raw ? (JSON.parse(raw) as ResetFlow) : null;
  } catch {
    return null;
  }
}

export const saveResetFlow = (flow: ResetFlow) =>
  sessionStorage.setItem(KEY, JSON.stringify(flow));

export const clearResetFlow = () => sessionStorage.removeItem(KEY);