import 'package:flutter/material.dart';

class Termsnotice extends StatelessWidget {
  const Termsnotice({super.key});

  @override
  Widget build(BuildContext context) {
    return const Center(
              child: SizedBox(
                width: 185,
                child: Text.rich(
                  textAlign: TextAlign.center,
                   TextSpan(
                  children: [
                    TextSpan(text:  "بالضغط على «التالي»، فإنك توافق على ",
                    style: TextStyle(color: Colors.transparent,fontSize: 12,shadows: [Shadow(color: Color(0xff475569),offset: Offset(0, -1))])),
                    TextSpan(text: "شروط الخدمة وسياسة الخصوصية",
                    style: TextStyle(decorationColor: Color(0xff2B6CB0),decoration: TextDecoration.underline,color: Colors.transparent,fontSize: 12,
                    shadows: [
                      Shadow(
                        color: Color(0xff2B6CB0),
                        offset: Offset(0, -1)

                    )]))
                  ]
                )),
              ),
            );
  }
}