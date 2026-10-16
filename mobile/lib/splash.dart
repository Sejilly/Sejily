import 'package:flutter/material.dart';
import 'package:flutter_svg/flutter_svg.dart';
import 'package:mobile/pages/Onboarding_Screen.dart';


class Splash extends StatefulWidget {
  const Splash({super.key}); 

  @override
  State<Splash> createState() => _SplashState();
}

class _SplashState extends State<Splash> {
  @override
  void initState() {

    super.initState();
    Future.delayed(const Duration(seconds: 3),(){
     if(!mounted)return;
     Navigator.pushReplacement(context,MaterialPageRoute(builder: (context)=>onboarding_Screen()));
    });
  }
  @override
  Widget build(BuildContext context) {

    return Scaffold(
      resizeToAvoidBottomInset: true,
      body: Container(
        width: double.infinity,
        height: double.infinity,
        decoration: const BoxDecoration(
          gradient: LinearGradient(
           begin : Alignment.topLeft,
           end : Alignment.bottomRight,
           colors: [Color(0xFFD9E5EF),Color(0xFFF8FAFC),Color(0xFFD9E5EF)])
        ),
        child: SizedBox.expand(
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.center,
            children: [
              Spacer(),
              Column(
                mainAxisAlignment: MainAxisAlignment.center,
                crossAxisAlignment: CrossAxisAlignment.center,
                mainAxisSize: MainAxisSize.min,
                children: [
                 SvgPicture.asset("assets/images/Sejilly_Logo.svg"),
                 Transform.translate
                 (offset:const Offset(0, -40),
                  child: const Text("سِجِلّي",style: TextStyle(fontWeight: FontWeight.w700, fontSize: 32,color:Color(0xFF0B192C) ),))
                ],
              ),
              const Spacer(),
              const Padding(
                padding: EdgeInsets.all(18),
                child: Text("جميع الحقوق محفوظة © ٢٠٢٧ سِجِلّي.",style: TextStyle(fontSize: 12,color: Color(0xff0B192C)),),
              )
              
            ],
          ),
        ),
      ),
    );
  }
}