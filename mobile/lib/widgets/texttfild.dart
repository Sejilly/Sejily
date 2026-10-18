import 'package:flutter/material.dart';
import 'package:flutter_svg/flutter_svg.dart';

class Texttfild extends StatefulWidget {
  final haserror;
  final TextEditingController control;
  final String lable;
  final String hint;
  final String? Function(String?)? validator;
  final Function()? ontap;
  final bool readonly;
  final bool isicon;
  final String? icon;
  final bool curser;
  final ispassword;
  final bool isSubmitted;
  final bool lastpadding;
  const Texttfild({this.lastpadding=true, required this.isSubmitted,this.haserror,this.ispassword=false,this.curser=true,this.icon,this.isicon=false,required this.control, super.key, required this.lable,required this.hint,this.validator,this.ontap,this.readonly=false});

  @override
  State<Texttfild> createState() => _TexttfildState();
}

class _TexttfildState extends State<Texttfild> {
  
  late  FocusNode foucs;
  bool f=false;
  late bool hidden=true;
   bool haserror=false;
   String? errorMessage;
 
  
  @override
  void initState() {
  
    super.initState();
    foucs=FocusNode();
    if(mounted){
     widget.control.addListener(() 
      {if(widget.isSubmitted&&mounted){
     setState(() {
       
           errorMessage = widget.validator?.call(widget.control.text);
           haserror = widget.isSubmitted && errorMessage != null;
        });
      }});
     foucs.addListener((){
       setState(() {
       if (mounted) {
      setState(() {
        f = foucs.hasFocus;
      });
    } 
      }
    );
     });}
     
  }
  @override
  void dispose() {
    
    super.dispose();
    foucs.dispose();
  }
  @override
  Widget build(BuildContext context) {
    errorMessage = widget.validator?.call(widget.control.text);
           haserror = widget.isSubmitted && errorMessage != null;
    
     final iconColor =haserror?0xffDC2626:f?0xff2B6CB0:0xff64748B;
     
    return Padding(
          
          padding: const EdgeInsets.symmetric(horizontal: 16),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(widget.lable,style: const TextStyle(fontSize: 14,color: Color(0xff0B192C)),),
              const SizedBox(height: 4,),
              SizedBox(
                height: 50,
                child: Container(
                  decoration: BoxDecoration(
                    borderRadius: BorderRadius.circular(12),
                    boxShadow:
                     [BoxShadow(
                        blurRadius: 6,
                      spreadRadius: 0,
                      offset: Offset(0, haserror?-3: 0),color:haserror&&f? Color(0xffB91C1C).withOpacity(0.10):f? Color(0xff2B6CB0).withOpacity(0.25):Color(0xffffffff))]
                    
                  ),
                  child: TextFormField(
                    
                    autovalidateMode:widget.isSubmitted?AutovalidateMode.onUserInteraction:AutovalidateMode.disabled,
                    style: const TextStyle(color:Color(0xff0B192C),fontSize: 14),
                    controller:widget.control ,
                    onTap: widget.ontap,
                    readOnly: widget.readonly,
                    validator: widget.validator,
                    decoration:InputDecoration(
                    constraints: BoxConstraints(
                      maxHeight: 50,
                      minHeight: 50
                    ),
                    contentPadding: EdgeInsets.symmetric(horizontal: 16,vertical: 12),
                    filled: true,
                    fillColor: Color(0xffffffff),
                    suffixIcon:widget.ispassword? IconButton(onPressed: ( ){setState(() { hidden= !hidden;
                    });}, icon: SvgPicture.asset(hidden?"assets/images/icons/close_eye-slash.svg":"assets/images/icons/bi_eye.svg",colorFilter: ColorFilter.mode(Color(haserror?0xffB91C1C:0xff334155), BlendMode.srcIn),)):
                    haserror?Padding(
                      padding: const EdgeInsets.symmetric(horizontal: 16,vertical: 10),
                      child: SvgPicture.asset("assets/images/icons/clarity_error-standard-solid.svg",width: 18,),
                    ):null,
                    
                    prefixIcon:widget.isicon? Row(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                      const SizedBox(width: 16,),
                      SvgPicture.asset("assets/images/icons/calender.svg",colorFilter: ColorFilter.mode( Color(iconColor), BlendMode.srcIn),),const SizedBox(width: 10,),Container(width: 1.25,height: 24,color: const Color(0xffCBD5E1),),const SizedBox(width: 10,)] ,):null,
                      focusedBorder:OutlineInputBorder(
                      borderSide: const BorderSide(color: Color(0xff2B6CB0)),
                      borderRadius: BorderRadius.circular(8)
                    ),
                    enabledBorder:OutlineInputBorder(
                      borderRadius: BorderRadius.circular(8),
                      borderSide: const BorderSide(width:1,color: Color(0xffCBD5E1),
                     )
                    ) ,
                    errorBorder: OutlineInputBorder(
                      borderSide: BorderSide(width: 1,color: Color(0xffB91C1C)),
                      borderRadius: BorderRadius.circular(8)
                    ),
                    focusedErrorBorder: OutlineInputBorder(
                      borderSide: BorderSide(color: Color(0xffB91C1C)),
                      borderRadius: BorderRadius.circular(8)
                    ),
                    hintText: widget.hint,
                    errorStyle: TextStyle(height: 0,fontSize: 0),
                    hintStyle: const TextStyle(fontSize: 14, color:  Color(0xff475569),fontWeight: FontWeight.w400),),
                    showCursor: widget.curser,
                    cursorColor: const Color(0xff2B6CB0),
                    focusNode: foucs,
                    obscureText: widget.ispassword? hidden:false,
                    
                  ),
                ),
              ),
  
              haserror?Text(errorMessage!,style: TextStyle(color: Color(0xffB91C1C),fontSize: 12),):SizedBox.shrink(),

             widget.lastpadding? const SizedBox(height: 20,):SizedBox.shrink()
            ],
          ),
        );
  }
}