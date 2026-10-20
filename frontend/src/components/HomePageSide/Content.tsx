import Image from "next/image";

function Content() {
    return (
        <div className="flex w-full min-h-0 flex-1 flex-col items-start gap-8 xl:gap-18 2xl:[@media(max-height:800px)]:gap-7">
            <div className=" flex w-full shrink-0 flex-col items-start gap-5 2xl:[@media(max-height:800px)]:gap-5">
                <h1
                className="w-full text-right text-2xl leading-8 font-bold text-ink sm:text-3xl sm:leading-10
                    xl:text-[40px] xl:leading-12 2xl:[@media(max-height:800px)]:text-3xl 2xl:[@media(max-height:800px)]:leading-9">
                إدارة ذكية{" "}
                <span className="text-[#5E8F4A]">
                    لسجلات المرضى
                </span>{" "}
                وتفاصيل حالاتهم الطبية.
                </h1>

                <p
                className="w-full text-right leading-7 font-normal text-ink xl:text-lg 2xl:[@media(max-height:800px)]:text-base
                    2xl:[@media(max-height:800px)]:leading-6"
                >
                نظّم ملفات مرضاك؛ امتلك السجل الكامل والتاريخ المرضي
                لمرضاك، وتابع تقارير المعامل والأشعة بدقة من خلال
                منصة موحدة وآمنة لإدارة عيادتك.
                </p>
            </div>

            <div className="relative hidden aspect-552/368 w-full lg:flex lg:min-h-75 lg:flex-1 lg:aspect-auto 
            2xl:[@media(max-height:800px)]:min-h-0">
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
    );
}

export default Content;