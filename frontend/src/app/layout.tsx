import type { Metadata } from "next";
import { Vazirmatn} from "next/font/google";
import "./globals.css";

const vazirmatn = Vazirmatn({
  variable: "--font-vazirmatn",
  subsets: ["arabic","latin"],
  weight: ["400", "600", "700"],
});

export const metadata: Metadata = {
  title: "Sejily",
};

export default function RootLayout({ children }: LayoutProps<"/">) {
  return (
    <html
      lang="ar" dir="rtl" 
      className={`${vazirmatn.variable} h-full antialiased`}
    >
      <body className="min-h-full flex flex-col font-sans">{children}</body>
    </html>
  );
}
