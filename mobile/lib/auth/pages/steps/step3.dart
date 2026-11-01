import 'package:flutter/material.dart';
import 'package:mobile/auth/model/icd11model.dart';
import 'package:mobile/auth/model/icd11servece.dart';
import 'package:mobile/auth/widgets/field/dropdown.dart';
import 'package:mobile/auth/widgets/field/multiselectfield.dart';


// ignore: must_be_immutable
class Step3 extends StatefulWidget {
  final isSubmitted;
  final List<TextEditingController> controllertext;
  List<List<Icd11model>?> selectedItemsLists;
  List<String>? Medicines;

   Step3({required this.Medicines,required this.selectedItemsLists,this.isSubmitted,required this.controllertext, super.key});

  @override
  State<Step3> createState() => _Step3State();
}

class _Step3State extends State<Step3> {
  
     bool isopen=false;
     String? selected;
     List<String>? selectedItems1=[];
     @override
  void initState() {
    // TODO: implement initState
    super.initState();
    _loded();
  }
  Future <void> _loded() async{
    if(!Icd11servece.isloaded){
      await Icd11servece.loaddata();
      if (mounted){
        setState(() {
          
        });
      }
    }

  }
  @override
  Widget build(BuildContext context) {
     debugPrint('hhhhhhhhhhhhhhhhhhhhhhhhhhhhhh${Icd11servece.icd11list.length}');
    return  Column(
      
      children: [
         Multiselectfield(onChanged: (List<Icd11model> list) {
            setState(() {
               widget.selectedItemsLists[0]=list;
               
            });
         },selectedItems: widget.selectedItemsLists[0] , hintText: 'ابحث عن المرض المزمن...', lable: 'الأمراض المزمنة',optional: true, suggestions: Icd11servece.icd11list, controller: widget.controllertext[8],),
         Multiselectfield(onChanged: (List<Icd11model> list) {
            setState(() {
               widget.selectedItemsLists[1]=list;
            });
         },selectedItems: widget.selectedItemsLists[1] , hintText: 'ابحث عن الحساسية الطبية...', lable: 'الحساسية الطبية',optional: true, suggestions: Icd11servece.icd11list, controller: widget.controllertext[9],),
         Multiselectfield(onChanged: (List<Icd11model> list) {
            setState(() {
               widget.selectedItemsLists[2]=list;
            });
         },selectedItems: widget.selectedItemsLists[2] ,odd: true, hintText: 'ابحث عن التاريخ المرضي العائلي...', lable: 'التاريخ المرضي العائلي',optional: true, suggestions: Icd11servece.icd11list, controller: widget.controllertext[10],),
          Multiselectfield(result: widget.Medicines,selectedItems: const [],odd: true, hintText: 'مثل: أدوية الضغط، فيتامينات... ', lable: 'الأدوية المنتظمة',optional: true, suggestions: const [], controller: widget.controllertext[11],),

           
          Dropdown(
            validator: (v){return null;},
            lable: 'فصيلة الدم',
          hintText: 'اختر فصيلة الدم', array: const [
          'A_Positive',
          'A_Negative',
          'B_Positive',
          'B_Negative',
          'AB_Positive',
          'AB_Negative',
          'O_Positive',
          'O_Negative'
      ], controller: widget.controllertext[12],optional: true,),
          const SizedBox(height: 32,)
           
        
          

      ],
    );
  }
}