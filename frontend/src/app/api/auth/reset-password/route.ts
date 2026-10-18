import { NextResponse } from "next/server";
import { API_BASE_URL, backendHeaders } from "../../../../../lib/serverApi";

export async function POST(request: Request) {
  const { requestId, password } = await request.json();

  let res: Response;
  try {
    res = await fetch(`${API_BASE_URL}/api/Auth/reset-password`, {
      method: "POST",
      headers: backendHeaders,
      body: JSON.stringify({ requestId, newPassword: password, confirmPassword: password }),
    });
  } catch (err) {
    console.error("reset-password: backend unreachable", err);
    return NextResponse.json({ code: "NETWORK" }, { status: 502 });
  }

  if (!res.ok) {
    const body = await res.json().catch(() => null);
    return NextResponse.json({ code: "BACKEND", body }, { status: res.status });
  }
  return NextResponse.json({ ok: true });
}