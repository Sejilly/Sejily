import 'dart:async';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_svg/svg.dart';
import 'package:mobile/pages/auth/sign_in/Reset_Password.dart';
import 'package:mobile/widgets/button.dart';

class OTP extends StatefulWidget {
  
  const OTP({super.key});

 

  @override
  State<OTP> createState() => _OTPState();
}

class _OTPState extends State<OTP> {
  late Timer _timer;
  late List<FocusNode> foucsnode;
  late List<TextEditingController>_controller;
  List<bool> isfill=List.generate(6,(_)=>false);
  int seconds=180;
  int minutes=0;
  @override
  void initState() {
    
    super.initState();
    foucsnode=List.generate(6, (_)=>FocusNode());
    _controller= List.generate(6, (_)=>TextEditingController());
   _starttimer();
  }
  void _starttimer(){
     _timer= Timer.periodic(Duration(seconds: 1), (callback){
      if(!mounted){return;}
      if(seconds>0)setState(() {
        seconds--;
      });else _timer.cancel();
     
    });
  }
  String get gettimer{
     minutes=seconds~/60;
     return " تنتهي صلاحية هذا الرمز خلال ${minutes.toString().padLeft(2,"0")}:${(seconds%60).toString().padLeft(2,"0")}";

  }
  @override
  Widget build(BuildContext context) {
    return SafeArea(
      child: Scaffold(
        backgroundColor: Color(0xffF8FAFC),
        body: Padding(
                  padding: const EdgeInsets.symmetric(horizontal:24,vertical: 16),
                  child: Stack(
                    children: [
                      SingleChildScrollView(
                        child: Container(
                         
                         decoration: BoxDecoration(color: const Color(0xffffffff),borderRadius: BorderRadius.circular(12)),
                         child: Padding(
                           padding: const EdgeInsets.symmetric(vertical: 16),
                           child: Column(
                            children: [
                            Text("إدخال رمز التحقق",style: const TextStyle(color: Color(0xff0B192C),fontSize: 18,fontWeight: FontWeight.w600),),
                            const SizedBox(height: 4,),
                            Padding(
                              padding: const EdgeInsets.symmetric(horizontal: 58),
                              child: Text.rich(textAlign: TextAlign.center,TextSpan(text:"أدخل رمز التحقق المرسل في رسالة نصية إلى رقم هاتفك (*********01)",style:TextStyle(color: Color(0xff475569),fontSize: 12)  )),
                            )
                            ,const SizedBox(height: 32,),
                             Form(
                              
                               child: Padding(
                                 padding: const EdgeInsets.symmetric(horizontal: 16),
                                 child: Column(
                                   children: [
                                      AnimatedBuilder(animation: Listenable.merge(_controller),builder: (context, child) => Row(
                                        children: List.generate(11, (index){
                                        if (index%2==0){
                                        final int i= index~/2;
                                        
                                        
                                        return Expanded(
                                            child: TextFormField(
                                              controller: _controller[i],
                                              cursorColor: Color(0xff2B6CB0),
                                              cursorRadius: Radius.circular(2),
                                              cursorWidth: 1,
                                              textAlign: TextAlign.center,
                                              
                                              keyboardType: TextInputType.number,
                                              inputFormatters: [FilteringTextInputFormatter.digitsOnly,LengthLimitingTextInputFormatter(6)],
                                              decoration: InputDecoration(
                                                counterText: "",
                                                filled: true,
                                                fillColor: Color(0xffffffff),
                                                enabledBorder: OutlineInputBorder(
                                                 
                                                  borderSide: BorderSide(color:  Color(_controller[i].text.length>0?0XFF2B6CB0:0xffCBD5E1),width: 1),
                                                  borderRadius: BorderRadius.circular(8)
                                                ),
                                                focusedBorder: OutlineInputBorder(
                                                  borderSide: BorderSide(color: Color(0XFF2B6CB0),width: 1),
                                                  borderRadius: BorderRadius.circular(8)
                                            
                                                )
                                              ),
                                              focusNode:foucsnode[i] ,
                                              autofocus:i==5?true:false,
                                              onChanged: (value) {
                                               if(i>0&&value.isNotEmpty){
                                                if(value.length>1){
                                                  String pastext= value.replaceAll(RegExp(r'\D'), '');
                                                  List past=pastext.split('').reversed.toList();
                                                  for(int ind=5;ind>=0;ind--){
                                                    _controller[ind].text=past[ind];
                                                  }
                                                 
                                                }
                                                FocusScope.of(context).requestFocus(foucsnode[i-1]);
                                               }else if(i<5&&value.trim().isEmpty){
                                                 FocusScope.of(context).requestFocus(foucsnode[i+1]);
                                               }
                                               
                                                String rotp=_controller.map((c)=>c.text).join();
                                                String otp=rotp.split('').reversed.join();
                                              
                                                if(otp=="524444"){
                                                  Navigator.pushReplacement(context, MaterialPageRoute(builder: (_)=>Reset_Password()));
                                                
                                               }
                                              },
                                            ),
                                          );}return SizedBox(width: 8,);}
                                       ),),)
                                       
                                       , SizedBox(height: 32,),
                                       
                                         Column(
                                          crossAxisAlignment: CrossAxisAlignment.center,
                                           children: [
                                             Text(gettimer,style: TextStyle(color: Color(0xff475569),fontSize: 12),),
                                             SizedBox(height: 16,),
                                             Buttonn(width: double.infinity, text: 'التالي', onPressed: () { 
                                              String rotp=_controller.map((c)=>c.text).join();
                                                String otp=rotp.split('').reversed.join();
                                               
                                                if(otp=="524444"){
                                                  Navigator.pushReplacement(context, MaterialPageRoute(builder: (_)=>Reset_Password()));
                                                }
                                              },),
                                             SizedBox(height: 16,),
                                             Buttonn(width: double.infinity, text: 'إعادة إرسال الرمز', onPressed: ( ){setState(() {
                                               seconds=180;
                                             });},backcolor: false,)
                                           ],
                                         )
                                   ],
                                 ),
                               ),
                                
                             ),
                           
                                   
                           
                            ],
                           ),
                         ),
                        
                        
                                      
                          
                        ),
                        
                    ),
                        Positioned(
                           top: 4,
                            right: 4,
                           child:IconButton(onPressed: (){
                            Navigator.pop(context);
                           }, icon: SvgPicture.asset("assets/images/icons/eva_arrow-back-outline.svg")))
        ])),
      ),
    );
  }
}