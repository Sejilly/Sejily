import Image from "next/image";
import type { ReactNode } from "react";

type Props = {
    src: string;
    alt: string;
    title: string;
    description: ReactNode;
};

export default function AuthIntro({ src, alt, title, description }: Props) {
    return (
        <div className="flex w-full flex-col items-center gap-5">
            <div className="relative h-28 w-40 sm:h-33 lg:[@media(max-height:850px)]:h-24">
                <Image src={src} alt={alt} fill sizes="179px" priority className="object-contain" />
            </div>
            <div className="flex w-full flex-col items-center gap-2">
                <h1 className="text-center text-2xl leading-8 font-bold text-ink">{title}</h1>
                <p className="max-w-md text-center text-xs leading-4.5 font-normal text-secondary">
                    {description}
                </p>
            </div>
        </div>
    );
}