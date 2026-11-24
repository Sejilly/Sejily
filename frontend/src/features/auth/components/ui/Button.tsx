import Link from "next/link";
import type { ButtonHTMLAttributes, ComponentProps } from "react";

type Variant = "primary" | "secondary";

const base =
  "flex h-12 w-full items-center justify-center rounded-xl p-3 text-base leading-6 font-semibold transition-colors disabled:cursor-not-allowed";

const variants: Record<Variant, string> = {
  primary: "cursor-pointer bg-[#102A43] text-white hover:bg-[#123C60] disabled:bg-[#E2E8F0] disabled:text-[#94A3B8] active:bg-[#164A78]",
  secondary:
    "cursor-pointer bg-transparent border border-[#2B6CB0] text-[#2B6CB0] hover:border-[#1F5A91] hover:text-[#1F5A91] active:border-[#164A78] active:text-[#164A78] disabled:border-[#E2E8F0] disabled:text-[#E2E8F0]",
};

type ButtonProps = ButtonHTMLAttributes<HTMLButtonElement> & {
  variant?: Variant;
};

export function Button({
  variant = "primary",
  className = "",
  ...props
}: ButtonProps) {
  return (
    <button
      className={`${base} ${variants[variant]} ${className}`}
      {...props}
    />
  );
}

type ButtonLinkProps = ComponentProps<typeof Link> & {
  variant?: Variant;
};

export function ButtonLink({
  variant = "secondary",
  className = "",
  ...props
}: ButtonLinkProps) {
  return (
    <Link className={`${base} ${variants[variant]} ${className}`} {...props} />
  );
}
