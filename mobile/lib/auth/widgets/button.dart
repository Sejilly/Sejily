import 'package:flutter/material.dart';
import 'package:flutter_svg/flutter_svg.dart';

class Buttonn extends StatelessWidget {
  final Function()? onPressed;
  final String text;
  final bool canpressed;
  final double width;
  final bool backcolor;
  final bool isloading;
  const Buttonn({this.isloading=false,this.backcolor=true,required this.width,this.canpressed=true,required this.text,required this.onPressed,super.key});


  @override
  Widget build(BuildContext context) {
    return SizedBox(
        height: 48,
        width: width,
        child: ElevatedButton(
                  style: ElevatedButton.styleFrom(
                    backgroundColor:canpressed? Color(isloading?0xff164A78: backcolor?0xff102A43:0xffffffff):Color( 0xffE2E8F0),
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12), side:isloading||backcolor?BorderSide.none: BorderSide(color:Color(0xff2B6CB0)))),
                  onPressed:isloading || !canpressed ? null: onPressed,
                  child: Row(
                    mainAxisSize: MainAxisSize.min,
                    mainAxisAlignment: MainAxisAlignment.center,
                    crossAxisAlignment: CrossAxisAlignment.center,
                    children: [
                      Text(text ,style: TextStyle(fontWeight:backcolor? FontWeight.w600:FontWeight.w400,fontSize: 16,color:canpressed? Color(isloading?0xff94A3B8 :backcolor?0xffFFFFFF:0xff2B6CB0):Color( 0xff94A3B8) ),),
                      if(isloading)...[SizedBox(width: 10,),
                      SvgPicture.asset("assets/images/icons/isloading.svg")]
                    ],
                  )),
      );
        
    }
    }