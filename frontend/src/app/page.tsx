import Background from "@/components/auth-ui/Background";
import LoginForm from "@/components/login/LoginForm";
import Aside from "@/components/HomePageSide/Aside";

export default function Home() {
  return (
    <div
      className="relative min-h-screen overflow-hidden bg-surface 2xl:[@media(max-height:800px)]:h-dvh 
      2xl:[@media(max-height:800px)]:min-h-0"
    >
      <Background />
      <div
        dir="ltr"
        className=" relative z-10 mx-auto flex min-h-screen w-full max-w-360 flex-col items-center justify-center
          gap-10 px-4 py-4 sm:px-8 sm:py-8 md:pb-16 lg:flex-row lg:px-10 lg:py-5 xl:gap-28
          2xl:px-0 2xl:py-20 2xl:[@media(max-height:800px)]:h-full 2xl:[@media(max-height:800px)]:min-h-0
          2xl:[@media(max-height:800px)]:gap-10 2xl:[@media(max-height:800px)]:px-10 2xl:[@media(max-height:800px)]:py-8"
      >
        <LoginForm />
        <Aside />
      </div>
    </div>
  );
}