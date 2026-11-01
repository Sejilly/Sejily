import 'package:flutter/material.dart';
import 'package:flutter_svg/flutter_svg.dart';
import 'package:mobile/auth/widgets/button.dart';

class SuccessProcess extends StatelessWidget {
  final String title;
  final String subtitle;
  final String disception;

   SuccessProcess({required this.title,required this.subtitle,required this.disception,super.key});

  @override
  Widget build(BuildContext context) {
    return Padding(
        padding: const EdgeInsets.symmetric(horizontal: 24,vertical: 59),
        child: Container(
          width: 345,
          decoration: BoxDecoration(color:Color(0xffffffff),borderRadius:BorderRadius.circular(12)),
          child:  Padding(
            padding: const EdgeInsets.symmetric(horizontal: 16),
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                const SizedBox(height: 32,),
                Column(
                  mainAxisSize:MainAxisSize.min,
                  children: [ 
                     Text(title,style: TextStyle(fontSize: 18,fontWeight: FontWeight.w600),),
                    const SizedBox(height: 32,),
                    SvgPicture.asset("assets/images/icons/checkmark-circle-sparkle-24-regular.svg"),
                    const SizedBox(height: 32,),
                    Text.rich(
                      textAlign: TextAlign.center,
                      TextSpan(
                        
                        text:"$subtitle\n" ,
                        style: TextStyle(color: Color(0xff0B192C), ),
                        children: [
                          TextSpan(text: disception,
                          style: TextStyle(color: Color(0xff475569)))
                        ]
                      )
                    )
                  ],
                ),
                SizedBox(height: 80,),
                Buttonn(width: double.infinity, text: 'ابدأ الآن', onPressed: () {  },),
                SizedBox(height: 16,)
              ],
            ),
          ),
        ),
      );
  }
}