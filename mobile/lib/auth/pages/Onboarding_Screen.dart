
import 'package:flutter/material.dart';
import 'package:mobile/auth/pages/auth_type_screen.dart';
import 'package:mobile/auth/widgets/button.dart';
class OnboardingScreendata {
   final String title;
   final String description;
   OnboardingScreendata({required this.title,required this.description});
   
}
// ignore: camel_case_types
class onboarding_Screen extends StatefulWidget {
    const onboarding_Screen({  super.key});

  @override
  State<onboarding_Screen> createState() => _onboarding_ScreenState();
}
// ignore: camel_case_types
class _onboarding_ScreenState extends State<onboarding_Screen> {
   final PageController _onboardingcontrolle=PageController();
  @override
  void dispose() {
    super.dispose();
    _onboardingcontrolle.dispose();
  }
    var currentindex=0;
  
  var onboard=[
   OnboardingScreendata(title: "سجلك الطبي الموحد في مكان واحد",
    description:"تابع تاريخك الصحي بكل سهولة بين جميع المستشفيات الحكومية، الخاصة، والعيادات الخارجية." ),
   OnboardingScreendata(title: "أمان تام وتحكم كامل بصلاحيات ملفك",
   description: "ارفع أشعتك وتحاليلك بأمان، وتحكم بنفسك في من يطلع على سجلك الصحي من الأطباء."),
   OnboardingScreendata(title: "رعاية أذكى.. وتاريخ صحي بين يديك بلا عناء",
   description: "نلخص لك استشاراتك الطبية بالذكاء الاصطناعي، لنضمن لك تجربة صحية مريحة ومرنة في كل الأوقات."),
  ];

  @override
  Widget build(BuildContext context) {
    return Scaffold(
       resizeToAvoidBottomInset: true,
      backgroundColor: const Color(0xffffffff),
      body:Stack(
        children: [
          Column(
            children: [
              Expanded(
              
                child: PageView.builder(
                  controller: _onboardingcontrolle,
                  itemCount: onboard.length,
                  onPageChanged: (value) {
                    setState(() {
                        currentindex=value;
                    });
                  
                  },
                  itemBuilder: (contx,index){
              
                  return SizedBox(
               
                child: Padding(
                  padding: const EdgeInsets.all(0),
                  child: Column(
                    mainAxisAlignment: MainAxisAlignment.start,
                    crossAxisAlignment: CrossAxisAlignment.center,
                    children: [
                      SizedBox(height: index==0?59:index==1?62:0,),
                       Image.asset("assets/images/onboarding_screen/Onboarding_Screen_${index+1}.png",),
                       const SizedBox(height: 70,),
                       Text(onboard[index].title,style: const TextStyle(fontWeight: FontWeight.bold,fontSize: 20,color: Color(0xff0B192C)),),
                       Padding(
                      padding: const EdgeInsets.symmetric(horizontal:30,vertical: 15),
                      child: SizedBox(
                        
                        child: Text(onboard[index].description,textAlign: TextAlign.center,style: const TextStyle(color: Color(0xff475569),fontWeight: FontWeight.normal,fontSize: 16))),
                    ),
              ],
                    
                  ),
                ),
              );
                  
                  }
                  ),
              ),
                          
                Padding(
                  padding: const EdgeInsets.symmetric(horizontal:25,vertical: 30),
                  child:Buttonn(width: double.infinity,text: currentindex==2?"سجل الآن":"التالي", onPressed: (){
                        _onboardingcontrolle.nextPage(duration: const Duration(milliseconds: 300), curve: Curves.linear); 
                        if(currentindex==onboard.length-1){
                          Navigator.pushReplacement(context, MaterialPageRoute(builder: (_)=>const AuthTypeScreen()));
                        }
                        
                        
                      },)
                )
            ],
          ),
          Positioned(
            top: 514,
            left: 0 ,
            right: 0,
            child: Center(
              child: Row(
               mainAxisAlignment: MainAxisAlignment.center,
               
               children: [
               Container(width:currentindex==0? 22:15,height: 6,decoration: BoxDecoration(color:currentindex==0? const Color(0xff2B6CB0):const Color(0xffD9E5EF), borderRadius: BorderRadius.circular(10)),),const SizedBox(width: 8,),
               Container(width:currentindex==1? 22:15,height: 6,decoration: BoxDecoration(color:currentindex==1? const Color(0xff2B6CB0):const Color(0xffD9E5EF), borderRadius: BorderRadius.circular(10)),),const SizedBox(width: 8,),
               Container(width: currentindex==2? 22:15,height: 6,decoration: BoxDecoration(color: currentindex==2? const Color(0xff2B6CB0):const Color(0xffD9E5EF), borderRadius: BorderRadius.circular(10)),),const SizedBox(width: 8,),
              ],),
            ))
        ],
      )

     
    );
  }
}