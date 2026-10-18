export function getApiBase(): string | null {
  const raw = process.env.API_BASE_URL ?? process.env.NEXT_PUBLIC_API_BASE_URL;
  if (!raw?.trim()) return null;
  return raw.trim().replace(/\/swagger.*$/i, "").replace(/\/+$/, "");
}

/** fetch() against the backend with the headers every call needs. */
export function backendFetch(base: string, path: string, init: RequestInit & { headers?: Headers } = {}) {
  const headers = new Headers(init.headers);
  // ngrok's free tier shows an HTML interstitial to "browser-like" clients unless this is set.
  headers.set("ngrok-skip-browser-warning", "true");
  return fetch(`${base}${path}`, {
    ...init,
    headers,
    cache: "no-store",
    redirect: "manual",
  });
}

export const NOT_CONFIGURED = {
  message: "API base URL is not configured. Set API_BASE_URL in .env.local.",
};