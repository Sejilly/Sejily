import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:mobile/auth/cubitstates.dart/cubite.dart';
import 'package:mobile/auth/cubitstates.dart/states.dart';
import 'package:mobile/pages/auth/sign_in/card.dart';
import 'package:mobile/pages/auth/sign_in/sign_in.dart';
import 'package:mobile/widgets/rule/passwordrule.dart';
import 'package:mobile/widgets/field/texttfild.dart';

class Reset_Password extends StatefulWidget {
  const Reset_Password({super.key});

  @override
  State<Reset_Password> createState() => _SignInState();
}

class _SignInState extends State<Reset_Password> {
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
  bool fouc=false;
  @override
  Widget build(BuildContext context) {
     return  BlocListener<Cubiteauth, Stateauth>(
  listener: (context, stateS) {
    if (stateS is Resetpasssuccess) {
      print('sucsseslogin');
      Navigator.pushReplacement(context, MaterialPageRoute(builder: (_)=>SignIn()));
    }
     
  },
   child:  BlocBuilder<Cubiteauth,Stateauth>(builder: (context,State){
    
       var cubit=BlocProvider.of<Cubiteauth>(context);
    return Scaffold(
                backgroundColor: const Color(0xffF8FAFC),
          body: 
          SafeArea(
            child: Cardd(haserror: cubit.state is Resetpasserror|| cubit.state is Resetpassexpired,error: 
           cubit.state is Resetpassexpired? 'انتهت صلاحية طلب إعادة تعيين كلمة المرور، يرجى البدء من جديد':
           cubit.state is Resetpasserror?'حدث خطأ ما أثناء إعادة تعيين كلمة المرور، يرجى المحاولة مرة أخرى':''
           ,title: "إعادة تعيين كلمة المرور", subtitle: "أنشئ كلمة مرور قوية وجديدة لحسابك.", 
            wedget1:Texttfild(
                  isSubmitted: isSubmitted,
                  control: _controller[0],
                  lable: "كلمة المرور", 
                  hint: "••••••••••••••••",
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
                  ispassword: true,), 
                  wedget2: Column(
                    children: [Focus(
                  onFocusChange: (value) => setState(() {
                    fouc=value;
                  }),
                  child:  Texttfild(
                  isSubmitted: isSubmitted,
                  control: _controller[1],
                  lable: "تأكيد كلمة المرور", 
                  hint: "••••••••••••••••",
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
                  ispassword: true,))
                  ,fouc?
                  Passwordrule(passcontroller: _controller[1]):SizedBox.shrink()
          ,],
                  ), formkey: _formkey,onPressed: (){
                    if(_formkey.currentState!.validate()){
                      cubit.resetpassword(password: _controller[0].text.trim(), confirmPassword: _controller[1].text.trim());
                    }
                  },)
            
          ));}));
  }
}