import type { Metadata } from "next";
import { AccessManagement } from "@/components/access/AccessManagement";

export const metadata: Metadata = { title: "إدارة الوصول | سجلّي" };

export default function AccessPage() {
  return <AccessManagement />;
}
