import 'package:flutter/material.dart';
import 'package:mobile/auth/widgets/field/dropdown.dart';


class Step2Guargian extends StatefulWidget {
    final  bool isloading;
    final VoidCallback onselect;
    final bool isVerified;
    final Map<String, String> relationshipMap;
  final List<TextEditingController> controllertext;
    Step2Guargian({required this.relationshipMap, required this.onselect, this.isVerified=false,this.isloading=false,required this.controllertext, super.key});
   
  @override
  State<Step2Guargian> createState() => _Step2GuargianState();
}

class _Step2GuargianState extends State<Step2Guargian> {

 
 
 String? selected;

  
  @override
  Widget build(BuildContext context) {
   
  
   
    return  Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Dropdown(
          validator:  (value) {
                        if(value==null||value.isEmpty ){
                          return 'ادخل صلة قرابتك بالتابع';
      
                        }else if(value.contains(RegExp(r'/d'))){
                          return 'ادخل صلة قرابتك بالتابع بشكل صحيح';
                        }
                        
                        return null;
                      },
          lable: "صلة قرابتك بالتابع",
          hintText: 'اختر صلة القرابة',
          array: widget.relationshipMap.keys.toList(),controller: widget.controllertext[5],selected: selected!=null ,isVerified: widget.isVerified,isloading: widget.isloading,
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