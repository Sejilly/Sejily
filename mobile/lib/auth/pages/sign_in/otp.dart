import 'dart:async';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_svg/svg.dart';
import 'package:mobile/auth/cubitstates.dart/cubite.dart';
import 'package:mobile/auth/cubitstates.dart/states.dart';
import 'package:mobile/auth/pages/sign_in/Reset_Password.dart';
import 'package:mobile/auth/widgets/button.dart';

class OTP extends StatefulWidget {
  
  const OTP({super.key});

 

  @override
  State<OTP> createState() => _OTPState();
}

class _OTPState extends State<OTP> {
  late GlobalKey<FormState> _formkey;
  late Timer _timer;
  late List<FocusNode> foucsnode;
  late List<TextEditingController>_controller;
  List<bool> isfill=List.generate(6,(_)=>false);
  int seconds=180;
  int minutes=0;
  bool haserror=false;
  bool isSubmitted=false;
  bool haserrorform=false;
  @override
  void initState() {
    
    super.initState();
    _formkey=GlobalKey<FormState>();
    foucsnode=List.generate(6, (_)=>FocusNode());
    _controller= List.generate(6, (_)=>TextEditingController());
   _starttimer();
  }
  @override
  void dispose() {
    // TODO: implement dispose
    super.dispose();
    for(var i in _controller){
      i.dispose();
    }
   
  }
  void _starttimer(){
     _timer= Timer.periodic(Duration(seconds: 1), (callback){
      if(!mounted){return;}
      if(seconds>0) {
        setState(() {
        seconds--;
      });
      } else _timer.cancel();
     
    });
  }
  String get gettimer{
     minutes=seconds~/60;
     return " تنتهي صلاحية هذا الرمز خلال ${minutes.toString().padLeft(2,"0")}:${(seconds%60).toString().padLeft(2,"0")}";

  }
  @override
  Widget build(BuildContext context) {
     return  BlocListener<Cubiteauth, Stateauth>(
  listener: (context, stateS) {
    if (stateS is Otpsuccess ) {
      print('sucsseslogin');
      Navigator.pushReplacement(context, MaterialPageRoute(builder: (_)=>Reset_Password()));
    }
     if(stateS is ResendOtpsuccess)
        {setState(() {
          seconds=180;
        });}
  },
   child:  BlocBuilder<Cubiteauth,Stateauth>(builder: (context,State){
    
       var cubit=BlocProvider.of<Cubiteauth>(context);
      
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
                              key: _formkey,
                               child: Padding(
                                 padding: const EdgeInsets.symmetric(horizontal: 16),
                                 child: Column(
                                   children: [
                                      AnimatedBuilder(animation: Listenable.merge(_controller),builder: (context, child) => Row(
                                        children: List.generate(11, (index){
                                        if (index%2==0){
                                        final int i= index~/2;
                                        
                                        
                                        return Expanded(
                                            child: Focus(
                                              onKeyEvent: (node, event) {
                                                if(event is KeyDownEvent && event.logicalKey==LogicalKeyboardKey.backspace &&i<5&&_controller[i].text.trim().isEmpty){
                                                  FocusScope.of(context).requestFocus(foucsnode[i+1]);
                                                  _controller[i+1].clear();
                                                  return KeyEventResult.handled;
                                                }
                                                return KeyEventResult.ignored;
                                              },
                                              child: TextFormField(
                                                controller: _controller[i],
                                                cursorColor: Color(0xff2B6CB0),
                                                cursorRadius: Radius.circular(2),
                                                cursorWidth: 1,
                                                textAlign: TextAlign.center,
                                                enabled: !(cubit.state is ResendOtploading||cubit.state is Otploading),
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
                                                   errorBorder: OutlineInputBorder(
                                                   
                                                    borderSide: BorderSide(color:  Color(0xffB91C1C),width: 1),
                                                    borderRadius: BorderRadius.circular(8)
                                                  ),
                                                  disabledBorder: OutlineInputBorder(
                                                    borderSide: BorderSide(color:  Color(0xffCBD5E1),width: 1),
                                                    borderRadius: BorderRadius.circular(8)
                                                  ), 
                                                  focusedErrorBorder: OutlineInputBorder(
                                                   
                                                    borderSide: BorderSide(color:  Color(0xffB91C1C),width: 1),
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
                                                 }

                                                 
                                                 
                                                  String rotp=_controller.map((c)=>c.text).join();
                                                  String otp=rotp.split('').reversed.join();
                                                
                                                   if(otp.length==6){
                                                    cubit.otp(otp: otp);
                                                   }
                                                },
                                                validator: (value) {
                                                  if(value==null|| value.isEmpty){
                                                    return '';
                                                  }
                                                  return null;
                                                },
                                              ),
                                            ),
                                          );}return SizedBox(width: 8,);}
                                       ),),)
                                       ,SizedBox(height: 16,)
                                       ,if(haserrorform|| cubit.state is ResendOtpCooldown||cubit.state is ResendOtperror||cubit.state is ResendOtpexpired||
                                            cubit.state is Otpexpired||cubit.state is Otpiswrong)
                                            Container(
                                                    width: double.infinity,
                                                   
                                                    decoration: BoxDecoration(
                                                      color: Color(0xffDC2626).withOpacity(0.05),
                                                      borderRadius: BorderRadius.circular(8),
                                                      border: Border.all(color: Color(0xffDC2626).withOpacity(0.5))
                                                    ),
                                                    child:Padding(
                                                      padding: const EdgeInsets.only(right: 8,left: 16,top: 12,bottom: 12),
                                                      child: Row(children: [
                                                        SvgPicture.asset("assets/images/icons/error-circle.svg"),SizedBox(width: 8,),
                                                        Flexible(
                                                          child: Text.rich(TextSpan(text:
                                                          cubit.state is ResendOtpCooldown?
                                                          ' يرجى الانتظار قريباً قليلاً قبل محاولة إعادة إرسال الرمز مرة أخرى.':
                                                          cubit.state is ResendOtperror?
                                                          'تعذر إعادة إرسال الرمز حالياً، يرجى المحاولة لاحقاً.':
                                                          cubit.state is ResendOtpexpired?
                                                          'انتهت صلاحية طلب إعادة الإرسال، يرجى البدء من جديد.':
                                                          cubit.state is Otpexpired?
                                                          'انتهت صلاحية رمز التحقق (OTP) المدخل، يرجى طلب رمز جديد.':
                                                          cubit.state is Otpiswrong? 
                                                          'رمز التحقق (OTP) الذي أدخلته غير صحيح، يرجى التحقق منه والمحاولة مرة أخرى.' :haserrorform?
                                                          'برجاء إدخال رمز التحقق (OTP)':'',
                                                          style: TextStyle(color: Color(0xffDC2626)))),
                                                        )

                                                      ],),
                                                    ) ,
                                                  ),
                                        SizedBox(height: 32,),
                                       
                                         Column(
                                          crossAxisAlignment: CrossAxisAlignment.center,
                                           children: [
                                            
                                             Text(gettimer,style: const TextStyle(color: Color(0xff475569),fontSize: 12),),
                                             const SizedBox(height: 16,),
                                             Buttonn(width: double.infinity, text: 'التالي', onPressed: () { 
                                              if(_formkey.currentState!.validate()){
                                               setState(() {
                                                 haserrorform=false;
                                                 isSubmitted=true;
                                               });
                                                String rotp=_controller.map((c)=>c.text).join();
                                                String otp=rotp.split('').reversed.join();
                                                cubit.otp(otp: otp);
                                               
                                              }else{
                                                setState(() {
                                                  haserrorform=true;
                                                });
                                              }
                                             
                                               
                                               
                                              },),
                                             const SizedBox(height: 16,),
                                             Buttonn(width: double.infinity, text: 'إعادة إرسال الرمز', onPressed: ( ){
                                              cubit.Resendotp();
                                              },backcolor: false,)
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
    );}));
  }
}