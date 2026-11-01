import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_svg/flutter_svg.dart';
import 'package:mobile/auth/cubitstates.dart/cubite.dart';
import 'package:mobile/auth/cubitstates.dart/states.dart';
import 'package:mobile/auth/model/icd11model.dart';
import 'package:mobile/auth/pages/sign_up/sign_up_Process_Success.dart';
import 'package:mobile/auth/pages/steps/step1.dart';
import 'package:mobile/auth/pages/steps/step2_signup.dart';
import 'package:mobile/auth/pages/steps/step3.dart';
import 'package:mobile/auth/widgets/bar_auth.dart';
import 'package:mobile/auth/widgets/button.dart';
import 'package:mobile/auth/widgets/termsNotice.dart';

class SignUpProcess extends StatefulWidget {
  const SignUpProcess({super.key});

  @override
  State<SignUpProcess> createState() => _SignUpProcess_1State();
}

class _SignUpProcess_1State extends State<SignUpProcess> {
   late List<List<Icd11model>?> selectedItemsLists;
  bool isSubmitted=false;
  int currentindex=0;
   DateTime? date;
  late List<TextEditingController> controllertext;
  PageController sign=PageController();
  List arr=["البيانات","الأمان","الصحية"];
  late final List<GlobalKey<FormState>> _formkey ;
   List<String> Medicines=[];
  @override
  void initState() {
    // TODO: implement initState
    super.initState();
    selectedItemsLists=List.generate(3, (_)=>null);
    controllertext=List.generate(13, (_)=>TextEditingController());
    _formkey=List.generate(3, (_)=> GlobalKey<FormState>()) ;
    
  }
  @override
  void dispose() {

    
        for(var x in controllertext){
           x.dispose();

    }
    sign.dispose();
    super.dispose();
  }
  void selectdate({required DateTime dateOfBirth}){
    date=dateOfBirth;

  }
  @override
  Widget build(BuildContext context) {
       return  BlocListener<Cubiteauth, Stateauth>(
  listener: (context, state) {
     debugPrint('STATE: ${state.runtimeType}  step=$currentindex');
    if (state is Regsuccess) {
      print('sucsseslogin');
      if(currentindex<2){
      setState(() {
        currentindex++; 
      });} else if(currentindex==2){
        Navigator.pushReplacement(context, MaterialPageRoute(builder: (_)=>Signup_Process_Success(name: controllertext[0].text.trim().split(' ').first,)));
      }
    }
  },
   child:  BlocBuilder<Cubiteauth,Stateauth>(builder: (context,state){
    
       var cubit=BlocProvider.of<Cubiteauth>(context);
         List<Widget>  steps=[Step1(guardian: false,loading: state is RegLooding,haserror:state is Regerrordublicatedid ,selectdate:selectdate,controllertext: controllertext,isSubmitted: isSubmitted,),Step2(isloading: state is RegLooding,isSubmitted: isSubmitted,controllertext: controllertext,),Step3(Medicines: Medicines,selectedItemsLists: selectedItemsLists,isSubmitted:isSubmitted ,controllertext: controllertext)];


    return SafeArea(
      child: PopScope(
        canPop: currentindex==0,
        onPopInvokedWithResult: (didPop, result) {
           if(didPop)return ;
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
                                cubit.registrationnothingfieldstep3();
                               
                              } ,),
                              AnimatedSwitcher(
      
                                duration: Duration(milliseconds: 300),
                                child: Form(
                                  key: _formkey[currentindex],
                                  child: steps[currentindex],
                                  ),
                              ),
                                const SizedBox(height: 4,),
                                 if (state is Regerror ||cubit.state is Regexpierd) SizedBox(height: 16,),
                                if(cubit.state is Regexpierd||state is Regerror)
                                Padding(
                                  padding: const EdgeInsets.symmetric(horizontal: 16.0),
                                  child: Container(
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
                                                            child: Text.rich(TextSpan(text:cubit.state is Regexpierd?  "انتهت صلاحية طلب التسجيل أو أصبح غير صالح، يرجى إعادة المحاولة من البداية":  'حدث خطا ما حاول مجددا',
                                                            style: TextStyle(color: Color(0xffDC2626)))),
                                                          )
                                  
                                                        ],),
                                                      ) ,
                                                    ),
                                ) ,
                                SizedBox(height: 16,),
                               
                                
                                
                                Padding(
                                  padding: const EdgeInsets.symmetric(horizontal: 16),
                                  child: Buttonn(text: currentindex==2?"إنشاء الحساب":"التالي",width: 313,onPressed: (){
                                    if (cubit.state is RegLooding) return;
                                   setState(() {
                                    isSubmitted=true;
                                   });
                                    if(_formkey[currentindex].currentState!.validate()){
                                    setState(() {
                                      isSubmitted=false;
                                     
                                  });
                                   if(currentindex==0){
                                        cubit.registrationstep1(nationalID: controllertext[1].text.trim(), fullName: controllertext[0].text.trim(), address: controllertext[2].text.trim(), dateOfBirth: date);
                                      }else if(currentindex==1){
                                        cubit.registrationstep2(phoneNumber: controllertext[4].text.trim(), password: controllertext[6].text.trim(), confirmPassword: controllertext[7].text.trim(), healthCardNumber: controllertext[5].text.trim());
                                      }
                                      else if(currentindex==2){
                                        cubit.registrationstep3(bloodType: controllertext[12].text.trim(), diseaseIcdCodes: selectedItemsLists[0]!=null? selectedItemsLists[0]!.map((e)=>e.code).toList():[], allergyIcdCodes: selectedItemsLists[1]!=null? selectedItemsLists[1]!.map((e)=>e.code).toList():[], familyDiseaseIcdCodes:selectedItemsLists[2]!=null? selectedItemsLists[2]!.map((e)=>e.code).toList():[], medicationIds:Medicines );
                                      }
                                      }},),
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
                                                 cubit.registrationnothingfieldstep3();
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
    );}));
  }
}