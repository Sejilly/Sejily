import 'package:flutter/material.dart';
import 'package:mobile/auth/function.dart';
import 'package:mobile/auth/widgets/field/texttfild.dart';


class Step1 extends StatelessWidget {
  final isSubmitted;
  final bool haserror;
  final bool loading;
  final Function selectdate;
  final List<TextEditingController> controllertext;
  final bool guardian;
   Step1({required this.guardian, required this.loading,required this.haserror,required this.selectdate,required this.controllertext, super.key,  required this.isSubmitted});

 
  @override
  Widget build(BuildContext context) {
    
    return  Column(
      
      children: [
        Texttfild(

          enable: !loading,
          isSubmitted: isSubmitted,
          control: controllertext[0],
          lable:"الاسم الكامل" ,
          hint:"الاسم كما ببطاقة الهوية" ,
          validator:(value) {
            if(value==null||value.trim().isEmpty){
              return "هذا الحقل إلزامى، يرجى استكمال البيانات";
            }
            return null;} ,),
        Texttfild(
           maxlength: 14,
           number: true,
          enable: !loading,
          haserror: haserror,
          errorms: 'هذا الرقم القومي مسجل مسبقاً!',
          isSubmitted: isSubmitted,
          control: controllertext[1],
          lable: "الرقم القومي", 
          hint: "الرقم القومي",
          validator: (value) {
            if(value==null||value.trim().isEmpty){
              return "هذا الحقل إلزامى، يرجى استكمال البيانات";
            }
            else if( value.length !=14|| value.contains(RegExp(r'\D'))){
            return 'الرقم القومى خاطئ، يرجى إعادة الادخال';}
            return null;
          },),
          Texttfild(
            enable: !loading,
          isSubmitted: isSubmitted,
          control:controllertext[2] ,
          lable: "العنوان", 
          hint: "العنوان كما ببطاقة الهوية",
          validator: (value) {
            if(value==null||value.trim().isEmpty){
              return "هذا الحقل إلزامى، يرجى استكمال البيانات";
            }
            return null;
          },),
          Texttfild(
            enable: !loading,
            isSubmitted: isSubmitted,
            control: controllertext[3],
            lable: "تاريخ الميلاد", 
            hint: "اليوم/الشهر/السنة",
            validator: (value) {
              if(value==null||value.trim().isEmpty){
                return "هذا الحقل إلزامى، يرجى استكمال البيانات";
              }
              return null;
            },ontap: ()async {
              
              final date=await showDatePicker(
                context: context,initialDate: DateTime(2000), firstDate: DateTime(1900), lastDate: DateTime.now(),
                builder:(context, child) {
                    return Theme(
                      data: Theme.of(context).copyWith(
                        colorScheme: const ColorScheme.light(
                          primary: Color(0xff2B6CB0),     // الهيدر واليوم المختار
                          onPrimary: Colors.white,        // لون النص فوق اللون الأساسي
                          surface: Color(0xffF0F5F9) ,
                                   // خلفية التقويم
                          onSurface: Color(0xff0B192C),   // لون الأرقام والنصوص
                        ),
                        datePickerTheme: const DatePickerThemeData(
                          dividerColor: Color(0xffE2E8F0),  ), // لون الخط الفاصل

      
                        textButtonTheme: TextButtonThemeData(
                          style: TextButton.styleFrom(
                            foregroundColor: const Color(0xff2B6CB0),   // زراري "موافق" و"إلغاء"
                          ),
                        ),
                      ),
                      child: child!,
    );
                },);
              if(date!=null){
                selectdate(dateOfBirth:date);
                controllertext[3].text=toArabicDigits("${date.year}/${date.month}/${date.day}");
              }
              
              
          },
          icon:"assets/images/icons/calender.svg",
          isicon: true,curser: false,
          readonly: true,),
          if(guardian)
          Texttfild(
          
          enable: true,
          isSubmitted:isSubmitted,
          optional: true,
          control:controllertext[4],
          lable:"رقم البطاقة الصحية (إن وُجدت)" ,
          hint:"رقم البطاقة الصحية" ,
          validator:(value) {
            return null;} ,),
          

          

      ],
    );
  }
}