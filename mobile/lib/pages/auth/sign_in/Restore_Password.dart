import 'package:flutter/material.dart';
import 'package:mobile/pages/auth/sign_in/card.dart';
import 'package:mobile/pages/auth/sign_in/otp.dart';
import 'package:mobile/widgets/texttfild.dart';

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
    return Scaffold(
                backgroundColor: const Color(0xffF8FAFC),
          body: 
          SafeArea(
            child: Cardd(title: "إستعادة كلمة المرور", subtitle: "أدخل بياناتك المسجلة للتحقق من هويتك واستعادة الحساب", 
            wedget1: Texttfild(
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
                                         
                                          Navigator.push(context, MaterialPageRoute(builder: (_)=>OTP()));}
                                      },)
           
          ));
  }
}