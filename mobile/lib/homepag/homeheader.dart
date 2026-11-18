import 'package:flutter/material.dart';
import 'package:flutter_svg/flutter_svg.dart';
import 'package:mobile/notification/notificationsScreen.dart';

class Homeheader extends StatefulWidget {
  const Homeheader({super.key});

  @override
  State<Homeheader> createState() => _HomeheaderState();
}

class _HomeheaderState extends State<Homeheader> {
  bool isread=false;
  @override
  Widget build(BuildContext context) {
    return Container(
      
      decoration: BoxDecoration(
        color: Color(0xffffffff),
        boxShadow: [
        BoxShadow(
          color: Color(0xff000000).withOpacity(0.05),
           blurRadius: 15,
           spreadRadius: 0,
        )
        ]
      ),
      child: Padding(
        padding: const EdgeInsets.only(bottom:12,top: 10,right: 24,left: 24 ),
        child: Row(
          children: [
            SvgPicture.asset('assets/images/icons/homescreen/Sejilly Logo - Light 1.svg' ,),
            SizedBox(width: 16,),
             Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text.rich(TextSpan(text: 'مرحباً،  محمد علي',style: TextStyle(color: Color(0xff0B192C),fontSize: 16)
                  ,)),
                  SizedBox(height: 2,),
                  Row(
                    mainAxisAlignment: MainAxisAlignment.end,
                    children: [
                    Text('تبديل الحساب',style: TextStyle(color: Color(0xff475569),fontSize: 12),),
                    SizedBox(width: 4,),
                    SvgPicture.asset('assets/images/icons/homescreen/akar-icons_chevron-down.svg')
                  ],),
              ],
            ),
            Spacer(),
            GestureDetector(
              onTap:(){
                Navigator.push(context, PageRouteBuilder(
                  transitionDuration: Duration(milliseconds: 300),
                  reverseTransitionDuration: Duration(milliseconds: 300),
                  transitionsBuilder: (context, animation, secondaryAnimation, child) {
                    return FadeTransition(
                      opacity: CurvedAnimation(parent: animation, curve: Curves.easeOut),
                      child: child,);
                      
                  },
                  pageBuilder:(BuildContext, Animation, Animationn){
                    return NotificationsScreen();
                    
                  } ));
              } ,
              child: Container(
                width: 38,
                height: 38,
                alignment: Alignment.center,
                decoration: BoxDecoration(
                  border: Border.all(color: Color(0xffE2E8F0)),
                  borderRadius: BorderRadius.circular(25)),
                  child: SvgPicture.asset( isread? 'assets/images/icons/homescreen/clarity_notification-line.svg':'assets/images/icons/homescreen/clarity_notification-outline-badged.svg'),
              ),
            ) 
              
          
          ],
        ),
      ),
    );
  }
}