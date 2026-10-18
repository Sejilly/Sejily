import { NextResponse } from "next/server";
import { API_BASE_URL, backendHeaders } from "../../../../../lib/serverApi";

export async function POST(request: Request) {
  const { nationalId, phone } = await request.json();

  let res: Response;
  try {
    res = await fetch(`${API_BASE_URL}/api/Auth/forgot-password`, {
      method: "POST",
      headers: backendHeaders,
      body: JSON.stringify({
        nationalID: nationalId,
        phoneNumber: phone,
        accountType: "Doctor",
      }),
    });
  } catch (err) {
    console.error("forgot-password: backend unreachable", err);
    return NextResponse.json({ code: "NETWORK" }, { status: 502 });
  }

  if (!res.ok) {
    return NextResponse.json({ code: "BACKEND" }, { status: res.status });
  }

  const data = (await res.json()) as { requestId?: string };
  if (!data.requestId) {
    return NextResponse.json({ code: "BAD_RESPONSE" }, { status: 502 });
  }
  return NextResponse.json({ requestId: data.requestId });
}