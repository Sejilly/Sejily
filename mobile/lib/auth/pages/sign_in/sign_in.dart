import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:mobile/auth/cubitstates.dart/cubite.dart';
import 'package:mobile/auth/cubitstates.dart/states.dart';
import 'package:mobile/auth/pages/guardian_signup/guardian_signup.dart';
import 'package:mobile/auth/pages/sign_in/card.dart';
import 'package:mobile/auth/widgets/field/texttfild.dart';

class SignIn extends StatefulWidget {
  const SignIn({super.key});

  @override
  State<SignIn> createState() => _SignInState();
}

class _SignInState extends State<SignIn> {
  late GlobalKey<FormState>_formkey;
  late List<TextEditingController> _controller;
  
  bool isSubmitted=false;
  @override
  void initState() {
    // TODO: implement initState
    super.initState();
    _formkey=GlobalKey<FormState>();
    _controller=List.generate(2, (_)=>TextEditingController());
  }
  @override
  void dispose() {
   for(var x in _controller){
    x.dispose();
   }
    super.dispose();
    
  }
 
  @override
  Widget build(BuildContext context) {
   return  BlocListener<Cubiteauth, Stateauth>(
  listener: (context, state) {
         debugPrint('STATE: ${state.runtimeType}  ');

    if (state is Loginsuccess) {
      print('sucsseslogin');
      // هنا تضعين الانتقال للشاشة الرئيسية مثلاً:
       Navigator.pushReplacement(context, MaterialPageRoute(builder: (_) => GuardianSignup()));
    }
  },
   child:  BlocBuilder<Cubiteauth,Stateauth>(builder: (context,State){
       var cubit=BlocProvider.of<Cubiteauth>(context);
      return Scaffold(
                resizeToAvoidBottomInset: true,
                backgroundColor: const Color(0xffF8FAFC),
          body: 
          SafeArea(
            child:Cardd(
              error:(cubit.state is Loginwrongpassorid)?'الرقم القومي أو كلمة المرور غير صحيحة، يرجى التحقق من البيانات المدخلة.':
              (cubit.state is Loginerror )?'تعذر تسجيل الدخول حالياً، يرجى التحقق من اتصال الإنترنت والمحاولة لاحقاً.':'',
              haserror: cubit.state is Loginsuccess||(cubit.state is Loginwrongpassorid)||(cubit.state is Loginerror),title: "تسجيل الدخول", subtitle: "أهلاً بعودتك، يرجى إدخال بيانات حسابك",
             wedget1: Texttfild(number: true, maxlength: 14,
              enable: cubit.state is! Loginloading,
                               haserror: cubit.state is Loginsuccess||(cubit.state is Loginwrongpassorid)||(cubit.state is Loginerror),
                                isSubmitted: isSubmitted,
                                control:_controller[0], 
                                lable: "الرقم القومي", 
                                hint: "الرقم القومي",
                               
                                validator: (value) {
                                  if(value==null||value.trim().isEmpty){
                                    return "برجاء إدخال الرقم القومي";
                                  }
                                  else if( value.length <14){
                                  return "الرقم القومي غيرمكتمل!";}
                                  else if(value.length>14)
                                  return "الرقم القومي غير صحيح";
                                  return null;
                                },),
                                 wedget2: Texttfild(
                                  haserror: cubit.state is Loginsuccess||(cubit.state is Loginwrongpassorid)||(cubit.state is Loginerror),
                                  enable: cubit.state is! Loginloading,
                                  isSubmitted: isSubmitted,
                                  control: _controller[1],
                                  lable: "كلمة المرور", 
                                  hint: "••••••••••••••••",
                                  lastpadding: false,
                                  validator: (value) {
                                    if(value==null||value.trim().isEmpty){
                                      return "برجاء إدخال كلمة المرور ";
                                    }
                                    else{ 
                                    if( value.length <8){
                                    return "يجب أن تحتوى على 8 رموز على الأقل.";}
                                    if(!value.contains(RegExp(r'[a-zA-Z]')))
                                    {return "يجب أن تحتوي على حروف.";}
                                    if(!value.contains(RegExp(r'[0-9]'))){
                                      return "يجب أن تحتوي على أرقام.";
                                    }
                       
                                    return null;}
                                  },
                                  ispassword: true,), formkey: _formkey,issignpage: true,onPressed: () {
                                  isSubmitted=true;
                                  if(_formkey.currentState!.validate()){
                                    cubit.login(id: _controller[0].text.trim(), password: _controller[1].text.trim()); 
                                                              }
                                    
                                  },)));}));
  }
}