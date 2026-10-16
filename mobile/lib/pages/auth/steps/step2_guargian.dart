import 'package:flutter/material.dart';
import 'package:flutter_svg/flutter_svg.dart';


class Step2Guargian extends StatefulWidget {
    final isloading;
    final VoidCallback onselect;
    final isVerified;
  final List<TextEditingController> controllertext;
   Step2Guargian({required this.onselect, this.isVerified=false,this.isloading=false,required this.controllertext, super.key});
   
  @override
  State<Step2Guargian> createState() => _Step2GuargianState();
}

class _Step2GuargianState extends State<Step2Guargian> {

 
 
 String? selected;
 bool isopen=false;

  
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
   
    return  Padding(
      padding: const EdgeInsets.all(16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text("صلة قرابتك بالتابع",style: TextStyle(color: Color(0xff0B192C)),),
          SizedBox(height: 4,),
          SizedBox(
            height: 50,
            
            child: Focus(
              onFocusChange: (value) {
                if (!mounted) return;
                setState(() {
                   
                  isopen=value;
                });
              },
              child: DropdownMenu<String>(
                controller: widget.controllertext[4],
                expandedInsets: EdgeInsets.zero,  
                dropdownMenuEntries:Kinship.map((item){
                  return DropdownMenuEntry<String>(
                    value: item,
                    label: item,
                    style:ButtonStyle(textStyle:WidgetStatePropertyAll(TextStyle(color: Color(0xff475569),fontFamily: 'Vazirmatin'),) )
                    );
                    
                }).toList(),
                onSelected: (newvalue)
                {setState(() {
                  selected=newvalue;
                  
                  
                });
                FocusScope.of(context).unfocus();
                widget.onselect();
                  },
                enabled:! widget.isloading,
                requestFocusOnTap: true,
                hintText: 'اختر صلة القرابة',
                textStyle:TextStyle(color: Color(0xff475569),fontSize: 14,fontWeight: FontWeight.w400) ,
                selectedTrailingIcon: Row( mainAxisAlignment:MainAxisAlignment.end,mainAxisSize: MainAxisSize.min,children: [Container(width: 1,height: 24,color: Color(0xffCBD5E1),),SizedBox(width: 16,),SvgPicture.asset("assets/images/icons/ant-design_caret-up-filled.svg")],),
                trailingIcon: widget.isVerified?SvgPicture.asset("assets/images/icons/truegreen.svg"):
                Row( mainAxisAlignment:MainAxisAlignment.end,mainAxisSize: MainAxisSize.min,children: [Container(width: 1,height: 24,color: Color(0xffCBD5E1),),SizedBox(width: 16,),SvgPicture.asset("assets/images/icons/dropdown.svg")],),
                menuStyle: MenuStyle(
                  maximumSize: WidgetStatePropertyAll(Size.fromHeight(200)),
                  
                  alignment: Alignment(1,1.24),
                  surfaceTintColor: WidgetStatePropertyAll(Colors.transparent),
                  backgroundColor: WidgetStatePropertyAll(Colors.white),
                  side:WidgetStatePropertyAll(BorderSide(color: Color(0xff2B6CB0,),width: 1),),
                  shape: WidgetStatePropertyAll(RoundedRectangleBorder(borderRadius: BorderRadius.circular(8))),
                  shadowColor: WidgetStatePropertyAll(Color(0XFF2B6CB0).withOpacity(0)) ,
                  elevation: WidgetStatePropertyAll(5)
                ),
                
                inputDecorationTheme: InputDecorationTheme(
                
                enabledBorder:OutlineInputBorder(
                            borderRadius: BorderRadius.circular(8),
                            borderSide:  BorderSide(color: Color(widget.isVerified?0xff059669 :selected!=null||isopen? 0xff2B6CB0:0xffCBD5E1),
                           )
                          ) ,
                focusedBorder:OutlineInputBorder(
                            borderSide: const BorderSide(width: 1,color: Color(0xff2B6CB0)),
                            borderRadius: BorderRadius.circular(8)
                          ),
                  
                ),
               ),
            ),
          ),
          widget.isVerified?Column(children: [
            SizedBox(height: 4,),
            Text("تم التحقق من صلة القرابة بنجاح !",style: TextStyle(fontSize: 12,color: Color(0xff059669)),)
          ],):SizedBox.shrink(),
         isopen? SizedBox(height: 244,):SizedBox.shrink()
        ],
      ),
    );
  }
}