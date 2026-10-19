import Background from "@/components/auth-ui/Background";
import LoginForm from "@/components/login/LoginForm";
import Aside from "@/components/HomePageSide/Aside";

export default function Home() {
  return (
    <div className="relative min-h-screen overflow-hidden bg-[#F8FAFC]">
      <Background />
      <div
        dir="ltr"
        className="relative z-10 mx-auto flex min-h-screen w-full max-w-360 flex-col items-center justify-center gap-10 px-4 py-4 sm:px-8 sm:py-8 md:pb-16 lg:flex-row lg:px-10 lg:py-5 xl:gap-28 2xl:py-20"
      >
        <LoginForm />
        <Aside />
      </div>
    </div>
  );
}