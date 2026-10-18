import { NextResponse, type NextRequest } from "next/server";
import { backendFetch, getApiBase, NOT_CONFIGURED } from "@/lib/server/backend";

/**
 * Thin server-side proxy:  /api/proxy/api/Auth/register/doctor  ->  <API_BASE_URL>/api/Auth/register/doctor
 *
 * Why a proxy instead of calling the ngrok URL straight from the browser?
 *  - No CORS: the backend's Program.cs has no CORS policy, so direct browser calls would be blocked.
 *  - ngrok's interstitial header is added for us.
 */
export const dynamic = "force-dynamic";

type Ctx = { params: Promise<{ path: string[] }> };

async function handler(req: NextRequest, { params }: Ctx) {
  const base = getApiBase();
  if (!base) return NextResponse.json(NOT_CONFIGURED, { status: 500 });

  const { path } = await params;
  if (path.some((s) => s === "." || s === "..")) {
    return NextResponse.json({ message: "Bad path." }, { status: 400 });
  }

  const headers = new Headers();
  const contentType = req.headers.get("content-type");
  if (contentType) headers.set("content-type", contentType);
  headers.set("accept", "application/json, text/plain, */*");

  const auth = req.headers.get("authorization");
  if (auth) headers.set("authorization", auth);

  const hasBody = !["GET", "HEAD"].includes(req.method);
  const target = `/${path.map(encodeURIComponent).join("/")}${req.nextUrl.search}`;

  let upstream: Response;
  try {
    upstream = await backendFetch(base, target, {
      method: req.method,
      headers,
      body: hasBody ? await req.arrayBuffer() : undefined,
    });
  } catch {
    return NextResponse.json({ message: "Unable to reach the API." }, { status: 502 });
  }

  const out = new Headers();
  const upstreamType = upstream.headers.get("content-type");
  if (upstreamType) out.set("content-type", upstreamType);
  const retryAfter = upstream.headers.get("retry-after");
  if (retryAfter) out.set("retry-after", retryAfter);

  // We never follow redirects (e.g. an http->https redirect would drop the body); surface it clearly.
  if (upstream.status >= 300 && upstream.status < 400) {
    return NextResponse.json({ message: "Unexpected redirect from the API." }, { status: 502 });
  }

  const noBody = upstream.status === 204 || upstream.status === 205;
  return new NextResponse(noBody ? null : await upstream.arrayBuffer(), {
    status: upstream.status,
    headers: out,
  });
}

export { handler as GET, handler as POST, handler as PUT, handler as PATCH, handler as DELETE };
