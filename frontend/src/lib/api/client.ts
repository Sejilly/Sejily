import {
  localizeCode,
  localizeMessage,
  MSG_FORBIDDEN,
  MSG_GENERIC,
  MSG_NETWORK,
  MSG_SERVER,
  MSG_UNAUTHORIZED,
} from "./messages";

/** Browser calls go to our own Next.js proxy (see app/api/proxy), never straight to ngrok. */
const PROXY_BASE = "/api/proxy";

export class ApiError extends Error {
  /** HTTP status, or 0 when the request never reached the server. */
  readonly status: number;
  /** Parsed body (object / array / string) or null. */
  readonly body: unknown;
  /** Ready-to-display Arabic messages (never empty). */
  readonly messages: string[];
  /** Untranslated server messages/codes (for matching, e.g. duplicate account). */
  readonly raw: string[];

  constructor(status: number, body: unknown, messages: string[], raw: string[]) {
    super(messages[0] ?? MSG_GENERIC);
    this.name = "ApiError";
    this.status = status;
    this.body = body;
    this.messages = messages;
    this.raw = raw;
  }
}

/** The API answers with JSON, plain strings, or an empty body — parse defensively. */
async function readBody(res: Response): Promise<unknown> {
  const text = await res.text();
  if (!text) return null;
  try {
    return JSON.parse(text);
  } catch {
    return text;
  }
}

/**
 * Handles every error shape from the guide:
 *  - plain string                       "A Doctor account with this National ID already exists."
 *  - Identity array                     [{ code, description }]
 *  - { message }                        e.g. 429 / 503 / reset failures
 *  - { message, errors: string[] }      reset-password failure
 *  - { errors: string[] }               dependents / switching
 *  - ProblemDetails { errors: { Field: string[] } }   model-validation 400
 */
function extract(body: unknown): { messages: string[]; raw: string[] } {
  const messages: string[] = [];
  const raw: string[] = [];
  const push = (m: unknown, code?: unknown) => {
    if (typeof m !== "string" || !m.trim()) return;
    raw.push(m);
    if (typeof code === "string") raw.push(code);
    messages.push((typeof code === "string" && localizeCode(code)) || localizeMessage(m));
  };

  if (typeof body === "string") {
    // An HTML page (e.g. ngrok interstitial / 404) is not a useful message.
    if (!/^\s*</.test(body)) push(body);
  } else if (Array.isArray(body)) {
    for (const item of body) {
      if (typeof item === "string") push(item);
      else if (item && typeof item === "object") {
        const o = item as { code?: unknown; description?: unknown };
        push(o.description, o.code);
      }
    }
  } else if (body && typeof body === "object") {
    const o = body as { message?: unknown; errors?: unknown; title?: unknown };
    const hasErrors = o.errors && (Array.isArray(o.errors) || typeof o.errors === "object");
    if (hasErrors) {
      const list = Array.isArray(o.errors) ? o.errors : Object.values(o.errors as object).flat();
      for (const e of list) push(e);
    }
    if (!messages.length) push(o.message);
    if (!messages.length) push(o.title);
  }
  return { messages, raw };
}

function fallbackFor(status: number): string {
  if (status === 0) return MSG_NETWORK;
  if (status === 401) return MSG_UNAUTHORIZED;
  if (status === 403) return MSG_FORBIDDEN;
  if (status >= 500) return MSG_SERVER;
  return MSG_GENERIC;
}

async function toError(res: Response): Promise<ApiError> {
  const body = await readBody(res);
  const { messages, raw } = extract(body);
  // 5xx / empty bodies: use a friendly generic message rather than server internals.
  const list = res.status >= 500 || !messages.length ? [fallbackFor(res.status)] : messages;
  return new ApiError(res.status, body, list, raw);
}

export interface RequestOptions {
  method?: "GET" | "POST" | "PUT" | "PATCH" | "DELETE";
  body?: unknown;
  signal?: AbortSignal;
}

/** Low-level request: returns parsed JSON (or null) on 2xx, throws ApiError otherwise. */
export async function request<T>(url: string, { method = "GET", body, signal }: RequestOptions = {}): Promise<T> {
  let res: Response;
  try {
    res = await fetch(url, {
      method,
      signal,
      headers: body !== undefined ? { "Content-Type": "application/json" } : undefined,
      body: body !== undefined ? JSON.stringify(body) : undefined,
      cache: "no-store",
    });
  } catch (e) {
    if (e instanceof DOMException && e.name === "AbortError") throw e;
    throw new ApiError(0, null, [MSG_NETWORK], []);
  }
  if (!res.ok) throw await toError(res);
  return (await readBody(res)) as T;
}

/** Call a backend route, e.g. apiFetch("/api/Auth/register/doctor", { method: "POST", body }). */
export function apiFetch<T>(path: string, options?: RequestOptions): Promise<T> {
  return request<T>(`${PROXY_BASE}${path.startsWith("/") ? path : `/${path}`}`, options);
}
