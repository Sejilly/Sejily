import 'package:flutter/material.dart';
import 'package:mobile/widgets/field/dropdown.dart';


class Step2Guargian extends StatefulWidget {
    final  bool isloading;
    final VoidCallback onselect;
    final bool isVerified;
  final List<TextEditingController> controllertext;
   const Step2Guargian({required this.onselect, this.isVerified=false,this.isloading=false,required this.controllertext, super.key});
   
  @override
  State<Step2Guargian> createState() => _Step2GuargianState();
}

class _Step2GuargianState extends State<Step2Guargian> {

 
 
 String? selected;

  
  @override
  Widget build(BuildContext context) {
    List<String> Kinship=["أب",
  "أم",
  "جد",
  "جدة",
  "عم",
  "عمة",
  "خال",
  "خالة",
  "أخ (بالغ)",
  "أخت (بالغة)",
  "وصي بحكم محكمة",
  "وكيل بموجب توكيل رسمي",
  "أخرى",];
   
    return  Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Dropdown(
          lable: "صلة قرابتك بالتابع",
          hintText: 'اختر صلة القرابة',
          array: Kinship,controller: widget.controllertext[4],selected: selected!=null ,isVerified: widget.isVerified,isloading: widget.isloading,
        onselect:widget.onselect, 
                )
     ,
        widget.isVerified?Column(children: [
          SizedBox(height: 4,),
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 16),
            child: Text("تم التحقق من صلة القرابة بنجاح !",style: TextStyle(fontSize: 12,color: Color(0xff059669)),),
          )
        ],):SizedBox.shrink(),
        SizedBox(height: 44,)
      ],
    );
  }
}