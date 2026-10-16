import 'package:flutter/material.dart';
import 'package:mobile/widgets/success_process.dart';

class GuardianSuccess extends StatelessWidget {
  
  const GuardianSuccess({ super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Color(0xffF8FAFC),
      body: SuccessProcess(title:"تم إضافة الحساب بنجاح!",subtitle: 'ممتاز، أصبحت وصي موثق الآن!',disception:'تم ربط حساب التابع بنجاح وأصبح بإمكانك إدارته ومتابعة سجله الطبي من حسابك.',)
    );
  }
}