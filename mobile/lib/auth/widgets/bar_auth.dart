import 'package:flutter/gestures.dart';
import 'package:flutter/material.dart';
import 'package:flutter_svg/svg.dart';

// ignore: must_be_immutable
class BarAuth extends StatefulWidget {
  List array;
  String title;
  String ?subtitle;
  int index;
  Function? ontap;
   BarAuth({this.ontap ,required this.index,required this.title,required this.subtitle,required this.array, super.key});

  @override
  State<BarAuth> createState() => _BarAuthState();
}

class _BarAuthState extends State<BarAuth> {
  late TapGestureRecognizer bar;
  @override
  void initState() {
    // TODO: implement initState
    super.initState();
    bar=TapGestureRecognizer()..onTap=widget.ontap as GestureTapCallback?;
  }
  @override
  void dispose() {
    bar.dispose();
    super.dispose();
    
  }
  
  @override
  Widget build(BuildContext context) {
    return Padding(
                  padding: const EdgeInsets.only(top: 24, bottom: 24,right: 16,left: 16),
                  child: Column(
                    children: [
                      Text(widget.title,style: const TextStyle(color: Color(0xff0B192C),fontSize: 18,fontWeight: FontWeight.w600),),
                      const SizedBox(height: 4,),
                      widget.index<2? Text(widget.subtitle!,style: const TextStyle(color: Color(0xff475569),fontSize: 12),):
                      Text.rich(
                      textAlign: TextAlign.center,
                      TextSpan(
                      children: [
                        const TextSpan(text:  "أكمل البيانات الصحية التالية أو .",
                        style: TextStyle(color: Colors.transparent,fontSize: 12,shadows: [Shadow(color: Color(0xff475569),offset: Offset(0, -1))])),
                        TextSpan(
                        text: "يمكنك تخطيها",
                        style: const TextStyle(color: Colors.transparent,fontSize: 12,
                        shadows: [
                          Shadow(
                            color: Color(0xff2B6CB0),
                            offset: Offset(0, -1) )]),
                            recognizer: bar)
                      ]
                    )),
                      const SizedBox(height: 20,),
                      Row(
                        children: List.generate(widget.array.length, (ind){
                          return 
                              Expanded(
                                child: Padding(
                                  padding:ind<2? const EdgeInsets.only(left: 4):const EdgeInsets.only(left: 0),
                                  child: Column(
                                    crossAxisAlignment: CrossAxisAlignment.start,
                                    children: [
                                      Container(height:3,decoration: BoxDecoration(borderRadius: BorderRadius.circular(3),color: Color(widget.index>ind?0xff5F8FB3:widget.index==ind?0xff2B6CB0:0xffCBD5E1),)),
                                      const SizedBox(height: 8,),
                                      Row(
                                        mainAxisAlignment: MainAxisAlignment.start,
                                        children: [SvgPicture.asset(widget.index>ind?"assets/images/icons/mdi_check-circle.svg":widget.index==ind?"assets/images/icons/akar-icons_radio-fill.svg":"assets/images/icons/Circule.svg"),
                                        const SizedBox(width: 4,),
                                        Text(widget.array[ind],style: TextStyle(color:Color(widget.index>ind?0xff475569:widget.index==ind?0xff0B192C:0xff94A3B8),fontSize: 12),)],)
                                    ],),
                                ),
                              );
                           
                           
                        }),
                          
                      )
                    ],
                  ),
                );
  }
}