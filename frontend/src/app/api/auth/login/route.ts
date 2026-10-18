import { NextResponse } from "next/server";
import { API_BASE_URL, TOKEN_COOKIE, backendHeaders } from "../../../../../lib/serverApi";

export async function POST(request: Request) {
  const { nationalId, password } = await request.json();

  let res: Response;
  try {
    res = await fetch(`${API_BASE_URL}/api/Auth/login/doctor`, {
      method: "POST",
      headers: backendHeaders,
      body: JSON.stringify({ nationalID: nationalId, password }),
    });
  } catch {
    return NextResponse.json({ code: "NETWORK" }, { status: 502 });
  }

  if (!res.ok) {
    return NextResponse.json({ code: "BACKEND" }, { status: res.status });
  }

  const { token } = (await res.json()) as { token: string };

  const response = NextResponse.json({ ok: true });
  response.cookies.set(TOKEN_COOKIE, token, {
    httpOnly: true,
    secure: process.env.NODE_ENV === "production",
    sameSite: "lax",
    path: "/",
    maxAge: 60 * 60,
  });
  return response;
}