import Image from "next/image";

function Logo() {
    return (
        <div className="flex items-center">
            <h2
                dir="rtl"
                className="text-2xl leading-8 font-bold whitespace-nowrap text-[#053755] -ms-2"
            >
                سِجِلّي
            </h2>
            <Image
                src="/logo-icon.svg"
                alt="شعار سجلّي"
                width={80}
                height={80}
                priority
                className="h-20 w-20 shrink-0"
            />
        </div>
    );
}

export default Logo;