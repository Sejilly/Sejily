import { NextResponse } from "next/server";
import { API_BASE_URL, backendHeaders } from "../../../../../lib/serverApi";

export async function POST(request: Request) {
  const { requestId } = await request.json();

  let res: Response;
  try {
    res = await fetch(`${API_BASE_URL}/api/Auth/resend-otp`, {
      method: "POST",
      headers: backendHeaders,
      body: JSON.stringify({ requestId }),
    });
  } catch (err) {
    console.error("resend-otp: backend unreachable", err);
    return NextResponse.json({ code: "NETWORK" }, { status: 502 });
  }

  if (!res.ok) {
    return NextResponse.json({ code: "BACKEND" }, { status: res.status });
  }
  return NextResponse.json({ ok: true });
}