import 'package:flutter/material.dart';
import 'package:flutter_svg/flutter_svg.dart';
import 'package:mobile/navigationbar.dart';
import 'package:mobile/notification/notificationcard.dart';

class NotificationsScreen extends StatefulWidget {
  const NotificationsScreen({super.key});

  @override
  State<NotificationsScreen> createState() => _NotificationsScreenState();
}

class _NotificationsScreenState extends State<NotificationsScreen> {
  bool isempty=false;
  @override
  Widget build(BuildContext context) {
    return  Scaffold(
       body: SafeArea(child: Scaffold(
        backgroundColor: const Color(0xffF8FAFC),
        body: Column(
          children: [
            Stack(
              children: [
                Container(
                  alignment: Alignment.center,
                  decoration: BoxDecoration(
                    color: Color(0xffffffff),boxShadow: [BoxShadow(color: Color(0xff000000).withOpacity(0.05),blurRadius: 15,spreadRadius: 0)]),
                
                  child: Padding(
                    padding: const EdgeInsets.only(bottom: 12,top: 8),
                    child: Text('الإشعارات',style: TextStyle(color: Color(0xff0B192C),fontSize: 18,fontWeight: FontWeight.w600),),
                  ),
                ),
                Positioned(
                    right: 24,
                    top: 8,
               child:  GestureDetector(
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
                    return Navigationbar();
                    
                  } ));
              } ,
                 
                    child: SvgPicture.asset('assets/images/icons/homescreen/eva_arrow-back-outline.svg')),
                )
              ],
            ),
            if(isempty)
            Expanded(
              child: Padding(
                padding: const EdgeInsets.symmetric(horizontal: 40),
                child: Align(
                  alignment: Alignment(0, -.18),
                  child: Column(
                    mainAxisSize: MainAxisSize.min,
                    mainAxisAlignment: MainAxisAlignment.center,
                    crossAxisAlignment: CrossAxisAlignment.center,
                    children: [
                      SvgPicture.asset('assets/images/icons/notifications/streamline-plump-color_notification-alarm-snooze.svg'),
                      SizedBox(height: 14,),
                      Text.rich(TextSpan(text: 'صندوق الإشعارات فارغ',style: TextStyle(color: Color(0xff0B192C),fontSize: 18,fontWeight: FontWeight.w600),
                      )),
                      SizedBox(height: 8,),
                      Text.rich(
                        textAlign: TextAlign.center,
                        TextSpan(text: 'ستظهر هنا جميع التنبيهات الخاصة بطلبات الوصول، التقارير الطبية، وتحديثات حسابك فور حدوثها.'
                      ,style: TextStyle(color: Color(0xff475569),fontSize: 14,fontWeight: FontWeight.w400)))
                  
                    ],
                  ),
                ),
              ),
            ),
            if(!isempty)
            Expanded(
              child: Padding(
                padding: const EdgeInsets.only(top: 24,right: 24,left: 24),
                child: ListView.builder(
                  itemCount:15,
                  itemBuilder: (context,index){
                    bool start=index==0;
                    
                     return  Column(
                      crossAxisAlignment:CrossAxisAlignment.start ,
                       children: [
                        if (start)
                        const Column(
                          children: [
                            SizedBox(height: 20,),
                             Text('اليوم',style: TextStyle(color: Color(0xff0B192C),fontSize: 16,fontWeight: FontWeight.w600),),
                          ],
                        ),
                       
                        SizedBox(height: 20,),
                         Notificationcard(isread:false,isaccess: false, isreport: false, islight: false, iskey: false,),
                         SizedBox(height: 20,),
                         Container(decoration: BoxDecoration(border: Border.all(color: Color(0xffCBD5E1),width: .5)),)
                       ],
                     );
                }),
              ),
            ),
          ],
        ),
       )),
    );
  }
}