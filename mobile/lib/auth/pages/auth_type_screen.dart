import 'package:flutter/material.dart';
import 'package:flutter_svg/flutter_svg.dart';
import 'package:mobile/auth/pages/sign_in/sign_in.dart';
import 'package:mobile/auth/pages/sign_up/Sign_up_Process%20.dart';
import 'package:mobile/auth/widgets/button.dart';
import 'package:mobile/auth/widgets/termsNotice.dart';

class AuthTypeScreen extends StatefulWidget {
  const AuthTypeScreen({super.key});

  @override
  State<AuthTypeScreen> createState() => _AuthTypeScreenState();
}


class _AuthTypeScreenState extends State<AuthTypeScreen> {
  int? selectionoption;
  
  @override
  Widget build(BuildContext context) {
    return  Scaffold(
      backgroundColor: Color(0xffF8FAFC),
      body: SafeArea(child: 
      Center(
        child: ListView(
          
          children: [
            SizedBox(height: 16,),
            Padding(
              padding: const EdgeInsets.all(16),
              child: Container(
                decoration:BoxDecoration(borderRadius: BorderRadius.circular(12),color: Color(0xffffffff)),
                child: Column(
                  children: [
                    Padding(
                      padding: const EdgeInsets.symmetric(horizontal: 61,vertical: 16),
                      child: Column(
                        children: [
                          SvgPicture.asset("assets/images/Sejilly_Logo.svg",width:118.65 ,),
                          Text("أهلا بك في سِجِلّي",style: TextStyle(fontSize: 18,fontWeight: FontWeight.w600,color: Color(0xff0B192C)),textAlign: TextAlign.center,),
                          SizedBox(height: 8,),
                          Text("سجل دخولك إذا كان لديك حساب، أو أنشئ حساباً جديداً للبدء.",style: TextStyle(fontSize: 14,color: Color(0xff475569),),textAlign: TextAlign.center,)
                    
                        ],
                      ),
                    ),
                    const SizedBox(height: 8,),
                     Row(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        _Selectcard(icon: "assets/images/icons/codicon_sign-in.svg", title: "تسجيل الدخول", description: "أملك حساباً بالفعل",isselect:selectionoption==0, index:0,ontap: ()=>setState(() {
                           selectionoption=0;
                        }),),
                        const SizedBox(width: 16,),
                        _Selectcard(icon: "assets/images/icons/basil_user-plus-outline.svg", title: " حساب جديد", description: "للتسجيل لأول مرة",isselect:selectionoption==1,index:1,ontap:()=>setState(() {
                          selectionoption=1;
                        }),)
                      ],
                    ),
                    const SizedBox(height: 44,),
                    Buttonn(width: 316,onPressed:  (){
                                  if(selectionoption==1){
                                    Navigator.push(context, MaterialPageRoute(builder: (_)=>const SignUpProcess()));
                        
                                  }else if(selectionoption==0){Navigator.push(context, MaterialPageRoute(builder: (_)=>const SignIn()));}
                              },canpressed: selectionoption !=null,text:"التالي" ,)
                   ,
                    SizedBox(height: 16,)
                     
                  ],
                ),
              ),
            ),
            
            selectionoption!=null?
            Termsnotice()
            :SizedBox.shrink()
        
          ],
        ),
      )),
    );
  }
}

class _Selectcard extends StatefulWidget {
   final bool isselect;
   final int index;
   final String icon;
   final String title;
   final String description;
   final VoidCallback ontap;

   const _Selectcard({required this.isselect,required this.ontap, required this.icon,required this.title,required this.description,required this.index});
  @override
  State<_Selectcard> createState() => __SelectcardState();
}

class __SelectcardState extends State<_Selectcard> {
  
  @override
  Widget build(BuildContext context) {
    return     GestureDetector(
      onTap: widget.ontap,
      child:AnimatedContainer(
        width: 150,
        height: 139,
        duration: const Duration(milliseconds: 300),
        decoration: BoxDecoration(
          borderRadius: BorderRadius.circular(8),
          color: widget.isselect?const Color(0xffD9E5EF):const Color(0xffFFFFFF),
          border: Border.all(
            color:widget.isselect?const Color(0xff2B6CB0) :const Color(0xffCBD5E1)
          )),
        child: Column(
          mainAxisAlignment:MainAxisAlignment.center,
          children: [
          SvgPicture.asset(widget.icon,colorFilter:ColorFilter.mode(widget.isselect?const Color(0xff2B6CB0): const Color(0xff64748B), BlendMode.srcIn) ,),
          const SizedBox(height: 4,),
          Text(widget.title,style: TextStyle(color: widget.isselect?const Color(0xff2B6CB0):const Color(0xff475569),fontSize: 16,fontWeight: FontWeight.w600),),
          const SizedBox(height: 4,),
          Text(widget.description,style: TextStyle(color:widget.isselect?const Color(0xff2B6CB0):const Color(0xff475569),fontSize: 14,fontWeight: FontWeight.w400),)
        ],),
      ) ,);

  }
}
