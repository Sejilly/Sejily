import 'package:flutter/material.dart';
import 'package:flutter_svg/flutter_svg.dart';
import 'package:mobile/pages/auth/sign_up/sign_up_Process_Success.dart';
import 'package:mobile/pages/auth/steps/step1.dart';
import 'package:mobile/pages/auth/steps/step2_signup.dart';
import 'package:mobile/pages/auth/steps/step3.dart';
import 'package:mobile/widgets/bar_auth.dart';
import 'package:mobile/widgets/button.dart';
import 'package:mobile/widgets/termsNotice.dart';

class SignUpProcess extends StatefulWidget {
  const SignUpProcess({super.key});

  @override
  State<SignUpProcess> createState() => _SignUpProcess_1State();
}

class _SignUpProcess_1State extends State<SignUpProcess> {
  bool isSubmitted=false;
  int currentindex=0;
  List<Widget> get steps=>[Step1(controllertext: controllertext,isSubmitted: isSubmitted,),Step2(isSubmitted: isSubmitted,controllertext: controllertext),Step3(isSubmitted:isSubmitted ,controllertext: controllertext)];
  late List<TextEditingController> controllertext;
  PageController sign=PageController();
  List arr=["البيانات","الأمان","الصحية"];
  late final _formkey ;
  @override
  void initState() {
    // TODO: implement initState
    super.initState();
    controllertext=List.generate(13, (_)=>TextEditingController());
    _formkey=List.generate(3, (_)=> GlobalKey<FormState>()) ;
  }
  @override
  void dispose() {

    super.dispose();
        for(var x in controllertext){
           x.dispose();

    }
    sign.dispose();
  }
  @override
  Widget build(BuildContext context) {
    return SafeArea(
      child: PopScope(
        canPop: currentindex==0,
        onPopInvokedWithResult: (didPop, result) {
           if(didPop)return null;
          else if (currentindex<3&&currentindex>0)
          setState(() {
            currentindex--;
          });
         
        },
        child: SafeArea(
          child: Scaffold(
            backgroundColor: const Color(0xffF8FAFC),
            body: 
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 24,vertical: 16),
              child: Stack(
                children: [
                  SingleChildScrollView(
                    child: Column(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        Container(
                          
                          decoration: BoxDecoration(color: const Color(0xffffffff),borderRadius: BorderRadius.circular(12)),
                          child: Column(
                            
                            children: [
                              BarAuth(array:arr,title: "إنشاء حساب جديد",subtitle:currentindex<2? "أكمل الخطوات التالية لإنشاء حسابك":null,index: currentindex,
                              ontap:(){
                                Navigator.push(context, MaterialPageRoute(builder: (_)=>Signup_Process_Success(name:  controllertext[0].text.trim().split(' ').first,)));
                              } ,),
                              AnimatedSwitcher(
      
                                duration: Duration(milliseconds: 300),
                                child: Form(
                                  key: _formkey[currentindex],
                                  child: steps[currentindex],
                                  ),
                              ),
                                const SizedBox(height: 4,),
                                Padding(
                                  padding: const EdgeInsets.symmetric(horizontal: 16),
                                  child: Buttonn(text: currentindex==2?"إنشاء الحساب":"التالي",width: 313,onPressed: (){
                                   setState(() {
                                    isSubmitted=true;
                                   });
                                    if(_formkey[currentindex].currentState!.validate()){
                                    setState(() {
                                      isSubmitted=false;
                                      if(currentindex<2)
                                       currentindex++;
                                       else
                                       Navigator.pushReplacement(context, MaterialPageRoute(builder: (_)=>Signup_Process_Success(name:  controllertext[0].text.trim().split(' ').first)));
                                  });}},),
                                )
                                ,const SizedBox(height: 16,),
                                currentindex==2?
                                Padding(
                                  padding: const EdgeInsets.symmetric(horizontal: 16),
                                  child: SizedBox(
                                    height: 48,
                                    width: double.infinity,
                                    child: ElevatedButton(
                                              style: ElevatedButton.styleFrom(
                                                backgroundColor:Color(0xffffffff),
                                                shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12),side: BorderSide(color:Color(0xff2B6CB0)))),
                                                onPressed: (){
                                                  Navigator.pushReplacement(context, MaterialPageRoute(builder: (_)=>Signup_Process_Success(name: controllertext[0].text.trim().split(' ').first,)));
                                                },
                                                 child: Text("تخطي" ,style: TextStyle(fontWeight: FontWeight.w600,fontSize: 16,color:Color(0xff2B6CB0) ),)),
                                  ),
                                ):SizedBox.shrink(),
                                SizedBox(height:16 ,)
                        
                             
                            ],
                          ),
                        ),
                        const SizedBox(height: 16,),
                        const Termsnotice()
                      ],
                    ),
                  ),
                  Positioned(
                     top: 4,
                    child:  IconButton(onPressed: (){
                      if(currentindex>0){
                      setState(() {
                      currentindex--;
                    });}
                    else if(currentindex==0){
                      Navigator.pop(context);
                    }}, icon: SvgPicture.asset("assets/images/icons/eva_arrow-back-outline.svg")))
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}