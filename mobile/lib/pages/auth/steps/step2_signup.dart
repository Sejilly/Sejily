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
          enable: true,
            isSubmitted: widget.isSubmitted,
            control: widget.controllertext[4],
            lable: "رقم الهاتف المحمول", 
            hint: "01X  XXXX  XXXX",
            validator: (value) {
              if(value==null||value.trim().isEmpty){
                return "هذا الحقل إلزامى، يرجى استكمال البيانات";
              }
              else if(value.length!=11||!RegExp(r'^(010|011|012|015)[0-9]{8}$').hasMatch(value)){
                return "برجاء إدخال رقم تليفون صحيح";
              }
              return null;
            },
              icon:"assets/images/icons/carbon_phone-incoming.svg",
              isicon: true,
              ),
        Texttfild(
          
          enable: true,
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
            enable: true,
          isSubmitted: widget.isSubmitted,
          control: widget.controllertext[6],
          lable: "تعيين كلمة المرور", 
          hint: "••••••••••••••••",
          validator: (value) {
            if(value==null||value.trim().isEmpty){
              return "هذا الحقل إلزامى، يرجى استكمال البيانات ";
            }
            else{ 
            if( value.length <8){
            return "كلمة المرور يجب أن تكون على الأقل 8 خانات من حروف و أرقام";}
            else if(!value.contains(RegExp(r'^(?=.*[a-z])(?=.*[A-Z])(?=.*[^a-zA-Z0-9])(?=.*[0-9])')))
            {return "كلمة المرور يجب أن تحتوي على حرف كبير، حرف صغير، رقم، ورمز خاص واحد على الأقل، وألا تقل عن 8 أحرف ";}
            

            return null;}
          },
          ispassword: true,))
        ,fouc?
          Passwordrule(passcontroller: widget.controllertext[6]):SizedBox.shrink()
          ,
          Texttfild(enable: true,
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