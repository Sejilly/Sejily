import 'package:flutter/material.dart';
import 'package:flutter_svg/flutter_svg.dart';

class Passwordrule extends StatefulWidget {
  final TextEditingController passcontroller;
   Passwordrule({super.key, required this.passcontroller});

  @override
  State<Passwordrule> createState() => _PasswordruleState();
}

class _PasswordruleState extends State<Passwordrule> {
  
  @override
  Widget build(BuildContext context) {
    return 
    ValueListenableBuilder<TextEditingValue>(valueListenable: widget.passcontroller, builder: (context, value, Widget? child,){
          bool isempty=widget.passcontroller==null|| widget.passcontroller.text.isEmpty;
          bool iscorrectlength=widget.passcontroller.text.length>8;
          bool ishasletter=widget.passcontroller.text.contains(RegExp(r'[a-zA-Z]'));
          bool ishasspecialchar=widget.passcontroller.text.contains(RegExp(r'[^a-zA-Z0-9]'));
          bool ishasnumber=widget.passcontroller.text.contains(RegExp(r'[0-9]'));
          final Color Colorlength=Color(iscorrectlength?0xff059669:0xff94A3B8); 
          final Color Colorspecialchar=Color(ishasspecialchar?0xff059669:0xff94A3B8); 
          final Color Colorleter=Color(ishasletter?0xff059669:0xff94A3B8); 
          final Color Colornumber=Color(ishasnumber?0xff059669:0xff94A3B8); 

          return Transform.translate(offset: Offset(0, -12),child:  Padding(
            padding: const EdgeInsets.only(right:16),
            child: isempty?SizedBox.shrink():
            Column(
              children: [
                Row(children: [
                  SvgPicture.asset("assets/images/icons/true.svg",colorFilter: ColorFilter.mode(Colorlength, BlendMode.srcIn),),const SizedBox(width: 4,),
                  Text("يجب أن تحتوى على 8 رموز على الأقل.",style: TextStyle(fontSize: 12 ,color:Colorlength ),)
                ],),
                Row(children: [
                  SvgPicture.asset("assets/images/icons/true.svg",colorFilter: ColorFilter.mode(Colorleter, BlendMode.srcIn),),const SizedBox(width: 4,),
                  Text("يجب أن تحتوي على حروف.",style: TextStyle(fontSize: 12 ,color:Colorleter ),)
                ],),
                Row(children: [
                  SvgPicture.asset("assets/images/icons/true.svg",colorFilter: ColorFilter.mode(Colornumber, BlendMode.srcIn),),const SizedBox(width: 4,),
                  Text("يجب أن تحتوي على أرقام.",style: TextStyle(fontSize: 12 ,color:Colornumber ),)
                ],),
                Row(children: [
                  SvgPicture.asset("assets/images/icons/true.svg",colorFilter: ColorFilter.mode(Colorspecialchar, BlendMode.srcIn),),const SizedBox(width: 4,),
                  Text("يجب أن تحتوي على رمز خاص واحد على الأقل .",style: TextStyle(fontSize: 12 ,color:Colorspecialchar ),)
                ],),
              ],
            ),
          ) );});
        }
          
        }
