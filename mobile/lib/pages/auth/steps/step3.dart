import 'package:flutter/material.dart';
import 'package:mobile/widgets/field/dropdown.dart';
import 'package:mobile/widgets/field/multiselectfield.dart';
import 'package:mobile/widgets/field/texttfild.dart';


class Step3 extends StatefulWidget {
  final isSubmitted;
  final List<TextEditingController> controllertext;
  List<List<String>?> selectedItemsLists;

   Step3({required this.selectedItemsLists,this.isSubmitted,required this.controllertext, super.key});

  @override
  State<Step3> createState() => _Step3State();
}

class _Step3State extends State<Step3> {
  List<String> chronicDiseasesList = [
  "ارتفاع ضغط الدم",
    "السكري - النوع الأول",
    "السكري - النوع الثاني",
    "أمراض القلب الشريانية",
    "الربو الشعبي",
    "قصور القلب المزمن",
    "الفشل الكلوي المزمن",
    "قصور الغدة الدرقية",
    "فرط نشاط الغدة الدرقية",
    "التهاب المفاصل الروماتويدي",
    "الصرع",
    "قرحة المعدة المزمنة",
    "أمراض الكبد المزمنة",
    "التهاب القولون التقرحي",
    "مرض كرونز",
    "هشاشة العظام",
    "الأنيميا المزمنة (فقر الدم)",
    "ارتفاع دهون الدم (الكوليسترول والدهون الثلاثية)",
    "أمراض القلب والأوعية الدموية",
    "أمراض الجهاز التنفسي المزمنة - كالربو",
    "السرطان",
    "ارتفاع الكوليسترول",
    "مرض الانسداد الرئوي المزمن",
    "הالقولون العصبي / القولون التقرحي",
    "مرض النقرس",
    "الكبد الدهني / أمراض الكبد المزمنة",
    "الشقيقة / الصداع النصفي المزمن",
    "لا يوجد"];
    List<String> medicalAllergiesList = [
  "حساسية البنسلين",
  "حساسية الأسبرين ومسكنات الألم",
  "حساسية المضادات الحيوية",
  "حساسية الأنسولين",
  "حساسية التخدير",
  "حساسية اللاتكس / المطاط الطبي",
  "حساسية صبغة الأشعة",
  "حساسية المكسرات والفول السوداني",
  "حساسية القمح / السيلياك",
  "حساسية الألبان / اللاكتوز",
  "حساسية البيض",
  "حساسية السمك",
  "حساسية السمسم",
  "حساسية الصويا",
  "حساسية حشرة الفراش / الغبار",
  "حساسية قرص النحل / الحشرات",
  "لا يوجد"
 ];
  List<String> geneticDiseasesList = [
    "أنيميا الخلايا المنجلية",
    "أمراض الثلاسيميا / أنيميا البحر المتوسط",
    "الهيموفيليا / نزاف الدم",
    "مرض التكيس الكبدي الكلوي",
    "التليف الكيسي",
    "الضمور العضلي الشوكي / دوشين",
    "ضمور العضلات الوراثي",
    "ارتفاع الكوليسترول الوراثي",
    "ورم العصب الفايبري",
    "مرض هانتينغتون"
  ];
     bool isopen=false;
     String? selected;
     List<String>? selectedItems1=[];
  @override
  Widget build(BuildContext context) {
    return  Column(
      
      children: [
         Multiselectfield(onChanged: (List<String> list) {
            setState(() {
               widget.selectedItemsLists[0]=list;
            });
         },selectedItems: widget.selectedItemsLists[0] , hintText: 'ابحث عن المرض المزمن...', lable: 'الأمراض المزمنة',optional: true, suggestions: chronicDiseasesList, controller: widget.controllertext[8],),
         Multiselectfield(onChanged: (List<String> list) {
            setState(() {
               widget.selectedItemsLists[1]=list;
            });
         },selectedItems: widget.selectedItemsLists[1] , hintText: 'ابحث عن الحساسية الطبية...', lable: 'الحساسية الطبية',optional: true, suggestions: medicalAllergiesList, controller: widget.controllertext[9],),
         Multiselectfield(onChanged: (List<String> list) {
            setState(() {
               widget.selectedItemsLists[2]=list;
            });
         },selectedItems: widget.selectedItemsLists[2] ,odd: true, hintText: 'ابحث عن التاريخ المرضي العائلي...', lable: 'التاريخ المرضي العائلي',optional: true, suggestions: geneticDiseasesList, controller: widget.controllertext[10],),
        
           Texttfild(isSubmitted:widget.isSubmitted,
          control:widget.controllertext[11],
          lable:"الأدوية المنتظمة" ,
          hint:"مثل: أدوية الضغط، فيتامينات..." ,
          optional: true
          ,
          validator:(value) {
            
              return null;
            }
            ),
          Dropdown(lable: 'فصيلة الدم',
          hintText: 'اختر فصيلة الدم', array: [
            'لا أعلم',
            'A+',
            'A-',
            'B+',
            'B-',
            'AB+',
            'AB-',
            'O+',
            'O-',], controller: widget.controllertext[12],optional: true,),
          SizedBox(height: 32,)
           
        
          

      ],
    );
  }
}