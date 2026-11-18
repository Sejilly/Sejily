import 'package:flutter/material.dart';
import 'package:flutter_svg/flutter_svg.dart';

class Notificationcard extends StatelessWidget {
  final bool isaccess;
  final bool isreport;
  final bool islight;
   final bool iskey;
   final bool isread;
   final VoidCallback? ontap;

   Notificationcard({super.key,required this.isread, required this.isaccess, required this.isreport, required this.islight, required this.iskey, this.ontap});
  
  @override
  Widget build(BuildContext context) {
     String filename=(isaccess)?'access':(isreport)?'report':(islight)?'light':'hugeicons_system_update';
    return GestureDetector(
      onTap: ontap,
      child: Row(
       
        crossAxisAlignment: CrossAxisAlignment.center,
        children: [
        isread?SizedBox(width: 8, height: 8,):
         SvgPicture.asset('assets/images/icons/notifications/notificationcard/Ellipse 1.svg'),
         SizedBox(width: 8,),
         SvgPicture.asset('assets/images/icons/notifications/notificationcard/${filename}.svg'),
         SizedBox(width: 12,),
        Expanded(
          child: const  Column(
            crossAxisAlignment: CrossAxisAlignment.start,
             children: [
               Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                 children: [
                   Text.rich( TextSpan(text:  'نتيجة فحص جديدة',style:TextStyle(color: Color(0xff0B192C) ) ,)),
                   Text.rich(TextSpan(text:  'منذ 12 دقيقة',style:TextStyle(color: Color(0xff475569) ) ))
                 ],
               ),
                 Text.rich(TextSpan(text:  'تم إرسال تحليل الدم إلى ملفك الطبي.',style:TextStyle(color: Color(0xff475569),fontSize: 12 )))
             ],
          
           ),
        ),
       
        ],
      ),
    );
  }
}