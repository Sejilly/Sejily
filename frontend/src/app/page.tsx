import Background from "../../components/ui/Background";
import LoginForm from "../../components/login/LoginForm";
import Aside from '../../components/HomePageSide/Aside';

export default function Home() {
  return (
    <div className="relative min-h-screen bg-white">
      <Background/>
      <div
        dir="ltr"
        className="relative z-10 min-h-screen mx-auto flex w-full max-w-360 flex-col items-center justify-center gap-10 
        px-4 py-4 sm:px-8 sm:py-8 md:pb-16  lg:flex-row  lg:py-5 lg:px-10  xl:gap-28 2xl:py-20"
      >
        <LoginForm/>
        <Aside/>
      </div>
    </div>
  );
}
