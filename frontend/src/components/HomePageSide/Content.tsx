import Image from "next/image";

function Content() {
    return (
        <div className="flex w-full flex-1 flex-col items-start gap-8 xl:gap-18">
            <div className="flex w-full shrink-0 flex-col items-start gap-5">
                <h1 className="w-full text-right text-2xl leading-8 font-bold text-[#0B192C] 
                    sm:text-3xl sm:leading-10 xl:text-[40px] xl:leading-12">
                    إدارة ذكية <span className="text-[#5E8F4A]">لسجلات المرضى</span>{" "}
                    وتفاصيل حالاتهم الطبية.
                </h1>
                <p className="w-full text-right leading-7 font-normal text-[#0B192C] xl:text-lg">
                    نظّم ملفات مرضاك؛ امتلك السجل الكامل والتاريخ المرضي لمرضاك،
                    وتابع تقارير المعامل والأشعة بدقة من خلال منصة موحدة وآمنة لإدارة
                    عيادتك.
                </p>
            </div>
            <div className="relative hidden aspect-552/368 w-full lg:aspect-auto lg:flex lg:min-h-75 lg:flex-1">
                <Image
                    src="/Sejily/home.svg"
                    alt="لوحة إدارة سجلات المرضى"
                    fill
                    priority
                    sizes="(min-width: 1280px) 552px, (min-width: 1024px) 40vw, 100vw"
                    className="object-contain object-bottom"
                />
            </div>
        </div>
    )
}

export default Content