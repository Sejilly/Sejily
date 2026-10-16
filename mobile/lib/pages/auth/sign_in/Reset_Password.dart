import 'package:flutter/material.dart';
import 'package:mobile/pages/auth/sign_in/card.dart';
import 'package:mobile/widgets/rule/passwordrule.dart';
import 'package:mobile/widgets/texttfild.dart';

class Reset_Password extends StatefulWidget {
  const Reset_Password({super.key});

  @override
  State<Reset_Password> createState() => _SignInState();
}

class _SignInState extends State<Reset_Password> {
  late GlobalKey<FormState>_formkey;
  late List<TextEditingController> _controller;
  bool isSubmitted=false;
  @override
  void initState() {
    // TODO: implement initState
    super.initState();
    _formkey=GlobalKey<FormState>();
    _controller=List.generate(2, (_)=>TextEditingController());
  }
  @override
  void dispose() {
   for(var x in _controller){
    x.dispose();
   }
    super.dispose();
    
  }
  bool fouc=false;
  @override
  Widget build(BuildContext context) {
    return Scaffold(
                backgroundColor: const Color(0xffF8FAFC),
          body: 
          SafeArea(
            child: Cardd(title: "إعادة تعيين كلمة المرور", subtitle: "أنشئ كلمة مرور قوية وجديدة لحسابك.", 
            wedget1:Texttfild(
                  isSubmitted: isSubmitted,
                  control: _controller[0],
                  lable: "كلمة المرور", 
                  hint: "••••••••••••••••",
                  validator: (value) {
                    if(value==null||value.trim().isEmpty){
                      return "برجاء إدخال كلمة المرور ";
                    }
                    else{ 
                    if( value.length <8){
                    return "يجب أن تحتوى على 8 رموز على الأقل.";}
                    if(!value.contains(RegExp(r'[a-zA-Z]')))
                    {return "يجب أن تحتوي على حروف.";}
                    if(!value.contains(RegExp(r'[0-9]'))){
                      return "يجب أن تحتوي على أرقام.";
                    }

                    return null;}
                  },
                  ispassword: true,), 
                  wedget2: Column(
                    children: [Focus(
                  onFocusChange: (value) => setState(() {
                    fouc=value;
                  }),
                  child:  Texttfild(
                  isSubmitted: isSubmitted,
                  control: _controller[1],
                  lable: "تأكيد كلمة المرور", 
                  hint: "••••••••••••••••",
                  validator: (value) {
                    if(value==null||value.trim().isEmpty){
                      return "برجاء إدخال كلمة المرور ";
                    }
                    else{ 
                    if( value.length <8){
                    return "يجب أن تحتوى على 8 رموز على الأقل.";}
                    if(!value.contains(RegExp(r'[a-zA-Z]')))
                    {return "يجب أن تحتوي على حروف.";}
                    if(!value.contains(RegExp(r'[0-9]'))){
                      return "يجب أن تحتوي على أرقام.";
                    }

                    return null;}
                  },
                  ispassword: true,))
                  ,fouc?
                  Passwordrule(passcontroller: _controller[1]):SizedBox.shrink()
          ,],
                  ), formkey: _formkey,onPressed: (){},)
            /**Padding(
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
                        const Text("إستعادة كلمة المرور",style: TextStyle(color: Color(0xff0B192C),fontSize: 18,fontWeight: FontWeight.w600),),
                        const SizedBox(height: 4,),
                        Padding(
                          padding: const EdgeInsets.symmetric(horizontal: 58),
                          child: Text.rich(textAlign: TextAlign.center,TextSpan(text:"أدخل بياناتك المسجلة للتحقق من هويتك واستعادة الحساب.",style:TextStyle(color: Color(0xff475569),fontSize: 12)  )),
                        ),
                        const SizedBox(height: 32,),
                         Form(
                          key:_formkey,
                           child: Column(
                             children: [
                               Texttfild(
                                isSubmitted: isSubmitted,
                                control:_controller[0], 
                                lable: "الرقم القومي", 
                                hint: "الرقم القومي",
                                validator: (value) {
                                  if(value==null||value.trim().isEmpty){
                                    return "برجاء إدخال الرقم القومي";
                                  }
                                  else if( value.length <14){
                                  return "الرقم القومي غيرمكتمل!";}
                                  else if(value.length>14)
                                  return "الرقم القومي غير صحيح";
                                  return null;
                                },),
                                Texttfild(
                                  isSubmitted: isSubmitted,
                                    control: _controller[1],
                                    lable: "رقم الهاتف المحمول", 
                                    hint: "01X  XXXX  XXXX",
                                    validator: (value) {
                                      if(value==null||value.trim().isEmpty){
                                        return "برجاء إدخال رقم الهاتف";
                                      }
                                      else if(value.length!=11){
                                        return "رقم الهاتف غير صحيح";
                                      }
                                      return null;
                                    },
                                      icon:"assets/images/icons/carbon_phone-incoming.svg",
                                      isicon: true,),
                                   Padding(
                                     padding: const EdgeInsets.symmetric(horizontal: 16),
                                     child: SizedBox(
                                      width: double.infinity,
                                       child: Column(
                                        crossAxisAlignment: CrossAxisAlignment.end,
                                         children: [
                                            SizedBox(height: 15,),
                                            Buttonn(width: double.infinity, text: 'التالي', onPressed: () {  },)
                                         ],
                                       ),
                                     ),
                                   )
                             ],
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
])),**/
          ));
  }
}