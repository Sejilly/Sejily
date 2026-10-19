import 'package:flutter/material.dart';
import 'package:mobile/widgets/rule/passwordrule.dart';
import 'package:mobile/widgets/field/texttfild.dart';


class Step2 extends StatefulWidget {
  final isSubmitted;
  final List<TextEditingController> controllertext;
  const Step2({this.isSubmitted,required this.controllertext, super.key});

  @override
  State<Step2> createState() => _Step2State();
}

class _Step2State extends State<Step2> {
  bool fouc=false;
  @override
  Widget build(BuildContext context) {
    return  Column(
      
      children: [
        Texttfild(
            isSubmitted: widget.isSubmitted,
            control: widget.controllertext[4],
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
              isicon: true,
              ),
        Texttfild(
          isSubmitted: widget.isSubmitted,
          optional: true,
          control: widget.controllertext[5],
          lable:"رقم البطاقة الصحية (إن وُجدت)" ,
          hint:"رقم البطاقة الصحية" ,
          validator:(value) {
            return null;} ,),
        Focus(
          onFocusChange: (value) => setState(() {
            fouc=value;
          }),
          child:  Texttfild(
          isSubmitted: widget.isSubmitted,
          control: widget.controllertext[6],
          lable: "تعيين كلمة المرور", 
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
          Passwordrule(passcontroller: widget.controllertext[6]):SizedBox.shrink()
          ,
          Texttfild(
          isSubmitted: widget.isSubmitted,
          control: widget.controllertext[7],
          lable: "تأكيد كلمة المرور", 
          hint: "••••••••••••••••",
          validator: (value) {
            if(value==null||value.trim().isEmpty){
              return "برجاء تأكيد كلمة المرور ";
            }
            else if(value!=widget.controllertext[6].text){
              return "كلمتا المرور غير متطابقتين";
            }
            return null;
          },
          ispassword: true,)]);
          
      
  }
}