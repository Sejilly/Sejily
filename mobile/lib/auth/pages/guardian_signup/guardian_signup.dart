import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_svg/svg.dart';
import 'package:mobile/auth/cubitstates.dart/cubite.dart';
import 'package:mobile/auth/cubitstates.dart/states.dart';
import 'package:mobile/auth/model/icd11model.dart';
import 'package:mobile/auth/pages/guardian_signup/guardian_success.dart';
import 'package:mobile/auth/pages/steps/step1.dart';
import 'package:mobile/auth/pages/steps/step2_guargian.dart';
import 'package:mobile/auth/pages/steps/step3.dart';
import 'package:mobile/auth/widgets/bar_auth.dart';
import 'package:mobile/auth/widgets/button.dart';
import 'package:mobile/auth/widgets/termsNotice.dart';

class GuardianSignup extends StatefulWidget {
  const GuardianSignup({super.key});

  @override
  State<GuardianSignup> createState() => _GuardianSignupState();
}

class _GuardianSignupState extends State<GuardianSignup> {
  late List<List<Icd11model>?> selectedItemsLists;
   bool isSubmitted=false;
  int currentindex=0;
  late List<TextEditingController> controllertext;
  PageController sign=PageController();
  List arr=["البيانات","الوصاية","الصحية"];
  late final List<GlobalKey<FormState>> _formkey ;
 
   DateTime? date;
    List<String> Medicines=[];
     Map<String, String> relationshipMap = {
  'أب': 'Father',
  'أم': 'Mother',
  'ابن': 'Son',
  'ابنة': 'Daughter',
  'زوج': 'Husband',
  'زوجة': 'Wife',
  'أخ': 'Brother',
  'أخت': 'Sister',
  'أخرى': 'Other',
};
  
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
  @override
  Widget build(BuildContext context) {
    return  BlocListener<Cubiteauth, Stateauth>(
  listener: (context, state) {
    BlocProvider.of<Cubiteauth>(context);
    if (state is guardiansuccess) {
      print('sucsseslogin');
      if(currentindex<2){
        setState(() {
          currentindex++;
        });
      }else if(currentindex==2){
        Navigator.pushReplacement(context, MaterialPageRoute(builder: (_)=>GuardianSuccess()));
      }
    
    
    }
  },
   child:  BlocBuilder<Cubiteauth,Stateauth>(builder: (context,state){
    
       var cubit=BlocProvider.of<Cubiteauth>(context);
         List<Widget>  steps=[Step1(guardian: true,controllertext: controllertext,isSubmitted: isSubmitted, loading: state is guardianLooding, haserror:state is guardianerrordublicatedid, 
         selectdate: ({required DateTime dateOfBirth}){
          date=dateOfBirth;

        },),
        Step2Guargian(relationshipMap: relationshipMap,isVerified: state is verificationkinshipsuccess,onselect:(){
          final  mapped=relationshipMap[controllertext[5].text.trim()] ;
          if(mapped!=null) {
            cubit.verificationkinship(relationship: mapped);
          }} ,isloading: state is verificationkinshiploading,controllertext: controllertext,),Step3(Medicines: Medicines,selectedItemsLists: selectedItemsLists,isSubmitted:isSubmitted ,controllertext: controllertext)];

    return SafeArea(
      child: PopScope(
        canPop: currentindex==0,
        onPopInvokedWithResult: (didPop, result) {
           if(didPop) {
             return ;
           } else if (currentindex<3&&currentindex>0)
        {
          setState(() {
            currentindex--;
          });
        }
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
                                cubit.guardiannothingfieldstep3();
                              } ,),
                              AnimatedSwitcher(
      
                                duration: Duration(milliseconds: 300),
                                child: Form(
                                  key: _formkey[currentindex],
                                  child: steps[currentindex],
                                  ),
                              ),
                               const SizedBox(height: 4,),
                              

                                  if ([ guardianerror ,guardianexpired,guardianexpiredtokenerror, guardianverifiederror,guardianpolicyerror,verificationkinshiperror,verificationkinshipexpired,verificationkinshipwrongaccounttype,verificationkinshiptokenerror].contains(state.runtimeType))
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
                                                            child: Text.rich(TextSpan(
                                                              text:
                                                              [guardianpolicyerror,verificationkinshipwrongaccounttype].contains(state.runtimeType)?  'عذراً، ليس لديك الصلاحية لتنفيذ هذا الإجراء .':
                                                              [guardianexpired,verificationkinshipexpired].contains(state.runtimeType)?'انتهى وقت الطلب، يرجى إعادة المحاولة من جديد.':
                                                              [guardianexpiredtokenerror,verificationkinshiptokenerror].contains(state.runtimeType)?'انتهت صلاحية الجلسة، يرجى تسجيل الدخول مرة أخرى':
                                                              state is guardianverifiederror?'لم يتم التحقق من صلة القرابة بعد، يرجى التحقق منها أولاً ثم إكمال التسجيل':
                                                              '  حدث خطا ما تاكد من الاتصال بالإنترنت وحاول مره اخرى'
                                                              ,
                                                            style: TextStyle(color: Color(0xffDC2626)))),
                                                          )
                                  
                                                        ],),
                                                      ) ,
                                                    ),
                                ) ,
                                 
                                if ([ guardianerror ,guardianexpired,guardianexpiredtokenerror, guardianpolicyerror,verificationkinshiperror,verificationkinshipexpired,verificationkinshipwrongaccounttype,verificationkinshiptokenerror].contains(state.runtimeType)) SizedBox(height: 16,),

                                Padding(
                                  padding: const EdgeInsets.symmetric(horizontal: 16),
                                  child: Buttonn(isloading: state is verificationkinshiploading,text: currentindex==2?"إنشاء الحساب":state is verificationkinshiploading?"جاري التحقق":"التالي",width: 313,onPressed: (){
                                   setState(() {
                                    isSubmitted=true;
                                   });
                                    if(_formkey[currentindex].currentState!.validate()){
                                    setState(() {
                                      isSubmitted=false;
                                  });
                                   if(currentindex==0)
                                       {cubit.guardianstep1(nationalID: controllertext[1].text.trim(), fullName: controllertext[0].text.trim(), address: controllertext[2].text.trim(), dateOfBirth: date!,healthCardNumber: controllertext[4].text.trim());}
                                       else if(currentindex==1){
                                        cubit.guardianstep2();
                                       }
                                       else if(currentindex==2){
                                        cubit.guardianstep3(bloodType: controllertext[12].text.trim(), diseaseIcdCodes: selectedItemsLists[0]!=null? selectedItemsLists[0]!.map((e)=>e.code).toList():[], allergyIcdCodes: selectedItemsLists[1]!=null? selectedItemsLists[1]!.map((e)=>e.code).toList():[], familyDiseaseIcdCodes:selectedItemsLists[2]!=null? selectedItemsLists[2]!.map((e)=>e.code).toList():[], medicationIds:Medicines );
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
                                                cubit.guardiannothingfieldstep3();
                                                },
                                                 child: Text("تخطي" ,style: TextStyle(fontWeight: FontWeight.w600,fontSize: 16,color:Color(0xff2B6CB0) ),)),
                                  ),
                                ):SizedBox.shrink(),
                                currentindex==2?SizedBox(height:16 ,):SizedBox.shrink()
                        
                             
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