import 'package:flutter/gestures.dart';
import 'package:flutter/material.dart';
import 'package:flutter_svg/flutter_svg.dart';
import 'package:mobile/notification/notificationsScreen.dart';

class AccessRequestCard extends StatefulWidget {
  const AccessRequestCard({super.key});

  @override
  State<AccessRequestCard> createState() => _AccessRequestCardState();
}

class _AccessRequestCardState extends State<AccessRequestCard> {
  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 15,horizontal: 24),
      child: Container(
        decoration: BoxDecoration(
          color: Color(0xffFFFBEB),
          borderRadius: BorderRadius.circular(12)
        ),
        child: Padding(
          padding: const EdgeInsets.all(10),
          child: Column(
            children: [
              Stack(
                  alignment: Alignment.topLeft,
                 clipBehavior: Clip.none,

                children: [
                  Container(
                    child: Container(
                       width: double.infinity,
                      padding: const EdgeInsets.all(8.0),
                      decoration: BoxDecoration(
                        color: Color(0xffffffff),
                        borderRadius: BorderRadius.circular(8),
                        boxShadow: [
                          BoxShadow(
                            offset: Offset(0, 2),
                            color: Color(0xff000000).withOpacity(.25),
                            blurRadius: 5
                          )
                        ]
                      ),
                      
                        child: Row(
                          mainAxisAlignment: MainAxisAlignment.spaceBetween,
                         crossAxisAlignment: CrossAxisAlignment.end,
                          children: [
                            Row(
                               crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                SvgPicture.asset('assets/images/icons/homescreen/Frame 1.svg'),
                                SizedBox(width: 8,),
                                  const Row(
                                   mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                   crossAxisAlignment: CrossAxisAlignment.end,
                                   children: [
                                     Column(
                                      mainAxisAlignment: MainAxisAlignment.start,
                                      crossAxisAlignment: CrossAxisAlignment.start,
                                       children: [
                                        Text( 'طلب وصول ينتظر موافقتك!',style: TextStyle(color: Color(0xff0B192C),fontSize: 14)),
                                        SizedBox(height: 2,),
                                        Text.rich(TextSpan(text: 'دكتور أحمد محمد - مستشفى الشفاء',style: TextStyle(color: Color(0xff475569),fontSize: 12))),
                                        SizedBox(height: 2,),
                                        Text.rich(TextSpan(text: 'سينتهي بعد 23 ساعة',style: TextStyle(color: Color(0xffB91C1C),fontSize: 10))),
                                       ],
                                     ),
                                          
                                   ],
                                   
                                 ),
                              ],
                            ),
                            
                                                 ],
                        ),
                      ),
                  ),
                  Positioned(
                    left: 8,
                    bottom: -5,
                    child: ElevatedButton(
                                style: ElevatedButton.styleFrom(
                                  padding: const EdgeInsets.symmetric(vertical: 2, horizontal: 12),
                                  minimumSize:Size.zero,
                                  backgroundColor:Color(0xff146851),
                                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(4))),
                                onPressed:(){  Navigator.push(context, PageRouteBuilder(
                                    transitionDuration: Duration(milliseconds: 300),
                                    reverseTransitionDuration: Duration(milliseconds: 300),
                                    transitionsBuilder: (context, animation, secondaryAnimation, child) {
                                      return FadeTransition(
                                        opacity: CurvedAnimation(parent: animation, curve: Curves.easeOut),
                                        child: child,);
                                        
                                    },
                                    pageBuilder:(BuildContext, Animation, Animationn){
                                      return NotificationsScreen();
                                      
                                    } ));},
                                child:   Text('عرض' ,style: TextStyle(fontWeight:FontWeight.w400,fontSize: 12,color: Color(0xffFFFFFF) ),),))
                ],
              ),
              SizedBox(height: 10,),
              Text.rich(TextSpan(text: 'لديك 4 طلبات وصول أخرى. ',style: TextStyle(fontSize: 10,color: Color(0xff475569)),
              children: [TextSpan(text: 'عرض الكل',style: TextStyle(color: Color(0xff0B192C),decoration: TextDecoration.underline,),
              recognizer: TapGestureRecognizer()..onTap=(){
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
              })]))
              
            ],
          ),
        ),
      ),
    );
  }
}