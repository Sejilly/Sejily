import type { Metadata } from "next";
import { Vazirmatn } from "next/font/google";
import "./globals.css";
import { RegisterProvider } from "@/features/register/RegisterContext";
import "react-datepicker/dist/react-datepicker.css";

const font = Vazirmatn({
  subsets: ["arabic", "latin"],
  weight: ["400", "500", "600", "700"],
  variable: "--font-arabic",
  display: "swap",
});

export const metadata: Metadata = {
  title: "سجلّي",
  description: "إنشاء حساب جديد",
};

export default function RootLayout({ children }: LayoutProps<"/">) {
  return (
    <html lang="ar" dir="rtl" className={`${font.variable} h-full antialiased`}>
      <body className="min-h-full flex flex-col font-sans">
        <RegisterProvider>{children}</RegisterProvider>
      </body>
    </html>
  );
}