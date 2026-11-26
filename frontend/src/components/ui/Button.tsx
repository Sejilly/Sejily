import Link from "next/link";
import type { ButtonHTMLAttributes, ReactNode } from "react";
import { cn } from "@/lib/cn";
import { ChevronRightIcon } from "@/components/icons";

type Variant = "primary" | "outline" | "green" | "soft" | "danger" | "outlineDanger" | "outlineBlue";

const styles: Record<Variant, string> = {
  primary:
    "h-12 w-full bg-brand-navy text-white hover:bg-brand-navy/90 disabled:opacity-60",
  outline:
    "h-10 shrink-0 gap-2 border border-brand-blue px-4 text-brand-blue font-normal hover:bg-brand-blue/5 ",
    /* dashboard / access-management variants */
  green:
    "h-12 w-full bg-brand-green font-semibold text-white hover:bg-brand-green-hover disabled:opacity-60",
  soft:
    "h-12 w-full bg-border-disabled font-semibold text-secondary hover:bg-line/60 disabled:opacity-60",
  danger:
    "h-12 w-full bg-danger font-semibold text-white hover:bg-danger/90 disabled:opacity-60",
  outlineDanger:
    "h-12 w-full border border-danger font-semibold text-danger hover:bg-danger/5 disabled:opacity-60",
  outlineBlue:
    "h-12 w-full border border-brand-blue font-semibold text-brand-blue hover:bg-brand-blue/5 disabled:opacity-60",
};

export function Button({
  variant = "primary",
  className,
  ...rest
}: ButtonHTMLAttributes<HTMLButtonElement> & { variant?: Variant }) {
  return (
    <button
      {...rest}
      className={cn(
        "inline-flex items-center justify-center rounded-lg text-sm font-normal transition-colors",
        "focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-brand-blue",
        "disabled:cursor-not-allowed",
        styles[variant],
        className,
      )}
    />
  );
}

/* "Previous step button" */
export function BackLink({
  href,
  onClick,
  children = "الخطوة السابقة",
}: {
  href?: string;
  onClick?: () => void;
  children?: ReactNode;
}) {
  const cls =
    "mx-auto mt-6 flex w-fit items-center gap-2 text-base font-normal text-brand-blue hover:underline";
  const content = (
    <>
      <ChevronRightIcon className="size-4" />
      {children}
    </>
  );
  return href ? (
    <Link href={href} className={cls}>{content}</Link>
  ) : (
    <button type="button" onClick={onClick} className={cls}>{content}</button>
  );
}
