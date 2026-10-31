import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:mobile/auth/cubitstates.dart/cubite.dart';
import 'package:mobile/auth/cubitstates.dart/states.dart';
import 'package:mobile/pages/auth/sign_in/card.dart';
import 'package:mobile/pages/auth/sign_in/otp.dart';
import 'package:mobile/widgets/field/texttfild.dart';

class RestorePassword extends StatefulWidget {
  const RestorePassword({super.key});

  @override
  State<RestorePassword> createState() => _SignInState();
}

class _SignInState extends State<RestorePassword> {
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
  listener: (context, stateS) {
    if (stateS is Forgetsuccess) {
      print('sucsseslogin');
      Navigator.push(context, MaterialPageRoute(builder: (_)=>OTP()));
    }
  },
   child:  BlocBuilder<Cubiteauth,Stateauth>(builder: (context,State){
       var cubit=BlocProvider.of<Cubiteauth>(context);
    return Scaffold(
                backgroundColor: const Color(0xffF8FAFC),
          body: 
          SafeArea(
            child: Cardd(error: (cubit.state is Forgetwrongidornumber)?'عفواً، هذا الرقم القومي أو رقم الهاتف غير مسجل لدينا، يرجى التأكد من البيانات المدخلة.':
            (cubit.state is Forgeterrorconnrction)?'تعذر إرسال رسالة التحقق القصيرة حالياً، يرجى المحاولة لاحقاً':
            cubit.state is Forgeterror?'حدث خطأ ما، يرجى المحاولة لاحقاً. ':'',haserror: (cubit.state is Forgetwrongidornumber)|| (cubit.state is Forgeterrorconnrction)|| cubit.state is Forgeterror?,title: "إستعادة كلمة المرور", subtitle: "أدخل بياناتك المسجلة للتحقق من هويتك واستعادة الحساب", 
            wedget1: Texttfild(
                                enable: !(cubit.state is Forgetloading),
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
                                   enable: !(cubit.state is Forgetloading),
                                  isSubmitted: isSubmitted,
                                    control: _controller[1],
                                    lable: "رقم الهاتف المحمول", 
                                    hint: "01X  XXXX  XXXX",
                                    validator: (value) {
                                      if(value==null||value.trim().isEmpty){
                                        return "برجاء إدخال رقم الهاتف";
                                      }
                                      else if(value.length!=11){
                                        return "رقم الهاتف غير صحيح";
                                      }
                                      return null;
                                    },
                                      icon:"assets/images/icons/carbon_phone-incoming.svg",
                                      isicon: true,), formkey: _formkey,onPressed: () {
                                         setState(() {
                                            isSubmitted=true;
                                          });
                                        if(_formkey.currentState!.validate()) { 
                                          cubit.Forgetpassword(id: _controller[0].text.trim(), number: _controller[1].text.trim());
                                          }
                                      },)
           
          ));}));
  }
}