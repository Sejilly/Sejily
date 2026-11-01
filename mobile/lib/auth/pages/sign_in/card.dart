import 'package:flutter/gestures.dart';
import 'package:flutter/material.dart';
import 'package:flutter_svg/svg.dart';
import 'package:mobile/auth/pages/sign_in/forgetpassword.dart';
import 'package:mobile/auth/widgets/button.dart';

class Cardd extends StatefulWidget {
  final GlobalKey<FormState> formkey;
  final String title;
  final String subtitle;
  final Widget wedget1;
  final Widget wedget2;
  final bool issignpage;
  final dynamic Function()? onPressed;
  final bool haserror;
  final String error;
  const Cardd({required this.error,required this.haserror,required this.onPressed,super.key, required this.title, required this.subtitle, required this.wedget1, required this.wedget2, required this.formkey, this.issignpage=false});

 

  @override
  State<Cardd> createState() => _CarddState();
}

class _CarddState extends State<Cardd> {
  @override
  Widget build(BuildContext context) {
    return Padding(
              padding: const EdgeInsets.symmetric(horizontal:24,vertical: 16),
              child: Stack(
                children: [
                  SingleChildScrollView(
                    child: Container(
                     
                     decoration: BoxDecoration(color: const Color(0xffffffff),borderRadius: BorderRadius.circular(12)),
                     child: Padding(
                       padding: const EdgeInsets.symmetric(vertical: 16),
                       child: Column(
                        children: [
                        Text(widget.title,style: const TextStyle(color: Color(0xff0B192C),fontSize: 18,fontWeight: FontWeight.w600),),
                        const SizedBox(height: 4,),
                        Padding(
                          padding: const EdgeInsets.symmetric(horizontal: 58),
                          child: Text.rich(textAlign: TextAlign.center,TextSpan(text:widget.subtitle,style:TextStyle(color: Color(0xff475569),fontSize: 12)  )),
                        )
                        ,const SizedBox(height: 32,),
                         Form(
                          key:widget.formkey,
                           child: Column(
                             children: [
                                widget.wedget1,
                                widget.wedget2,
                                  SizedBox(height: 4,),
                                   Padding(
                                     padding: const EdgeInsets.symmetric(horizontal: 16),
                                     child: SizedBox(
                                      width: double.infinity,
                                       child: Column(
                                        crossAxisAlignment: CrossAxisAlignment.end,
                                         children: [
                                          
                                           widget.issignpage? Text.rich(
                                            textAlign: TextAlign.end,
                                            TextSpan(
                                              text: "نسيت كلمة المرور؟",
                                              style:  TextStyle(decorationColor:Color(0xff2B6CB0) ,decoration: TextDecoration.underline,color: Colors.transparent,fontSize: 12,
                                              shadows: [
                                                Shadow(
                                                  color: Color(0xff2B6CB0),
                                                  offset: Offset(0, -1) )]),
                                                  recognizer: TapGestureRecognizer()..onTap=(){Navigator.push(context, MaterialPageRoute(builder: (_)=> RestorePassword()));})):SizedBox.shrink(),
                                                  SizedBox(height: 15,),
                                                  if (widget.haserror) Container(
                                                    width: double.infinity,
                                                   
                                                    decoration: BoxDecoration(
                                                      color: Color(0xffDC2626).withOpacity(0.05),
                                                      borderRadius: BorderRadius.circular(8),
                                                      border: Border.all(color: Color(0xffDC2626).withOpacity(0.5))
                                                    ),
                                                    child:Padding(
                                                      padding: const EdgeInsets.only(right: 8,left: 16,top: 12,bottom: 12),
                                                      child: Row(children: [
                                                        SvgPicture.asset("assets/images/icons/error-circle.svg"),SizedBox(width: 8,),
                                                        Flexible(
                                                          child: Text.rich(TextSpan(text:  widget.error,
                                                          style: TextStyle(color: Color(0xffDC2626)))),
                                                        )

                                                      ],),
                                                    ) ,
                                                  ) ,
                                                  if (widget.haserror) const SizedBox(height: 32,) ,
                                                   Buttonn(width: double.infinity, text: 'التالي', onPressed:widget.onPressed,)
                                         ],
                                       ),
                                     ),
                                   )
                             ],
                           ),
                            
                         ),
                       
                               
                       
                        ],
                       ),
                     ),
                    
                    
                                  
                      
                    ),
                    
                ),
                    Positioned(
                       top: 4,
                        right: 4,
                       child:IconButton(onPressed: (){
                        Navigator.pop(context);
                       }, icon: SvgPicture.asset("assets/images/icons/eva_arrow-back-outline.svg")))
]));
  }
}