import 'package:flutter/material.dart';
import 'package:mobile/widgets/texttfild.dart';


class Step3 extends StatelessWidget {
  final isSubmitted;
  final List<TextEditingController> controllertext;
  const Step3({this.isSubmitted,required this.controllertext, super.key});
 
  @override
  Widget build(BuildContext context) {
    return  Column(
      
      children: [
        Texttfild(
          isSubmitted: isSubmitted,
          control: controllertext[8],
          lable:"الأمراض المزمنة (إن وُجدت)" ,
          hint:"مثل: ضغط دم، سكري..." ,
          validator:(value) {
            
              return null;
            }
            ),
            Texttfild(
              isSubmitted: isSubmitted,
          control: controllertext[9],
          lable:"الحساسية الطبية (إن وُجدت)" ,
          hint:"مثل: حساسية بنسلين، طعام..." ,
          validator:(value) {
            
              return null;
            }
            ),
            Texttfild(
              isSubmitted: isSubmitted,
          control: controllertext[10],
          lable:"التاريخ المرضي العائلي (إن وُجدت)" ,
          hint:"مثل: أمراض قلب، سكري..." ,
          validator:(value) {
            
              return null;
            }
            ),
            Texttfild(isSubmitted: isSubmitted,
          control: controllertext[11],
          lable:"الأدوية المنتظمة (إن وُجدت)" ,
          hint:"مثل: أدوية الضغط، فيتامينات..." ,
          validator:(value) {
            
              return null;
            }
            ),
            Texttfild(isSubmitted: isSubmitted,
          control: controllertext[12],
          lable:"فصيلة الدم" ,
          hint:"مثل: +A, O-, B+..." ,
          validator:(value) {
            
              return null;
            }
            ),
        
          

      ],
    );
  }
}