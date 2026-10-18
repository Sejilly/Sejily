import 'package:flutter/material.dart';
import 'package:mobile/widgets/success_process.dart';

class Signup_Process_Success extends StatelessWidget {
  final String name;
  const Signup_Process_Success({required this.name, super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Color(0xffF8FAFC),
      body: SuccessProcess(title:"تم إنشاء حسابك بنجاح!",subtitle: 'أهلاً بكِ يا ${name} في تطبيق "سجلي".\n',disception: 'أصبح بإمكانك الآن متابعة سجلك الطبي بكل سهولة وأمان.',)
    );
  }
}