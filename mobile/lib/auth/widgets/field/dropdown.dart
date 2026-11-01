import 'package:flutter/material.dart';
import 'package:flutter_svg/svg.dart';

class Dropdown extends StatefulWidget {
  final String lable;
  final String hintText;
  final TextEditingController controller;
  final List array;
  final Function()? onselect;
  final bool isloading;
  final bool isVerified;
  final bool selected;
  final String? Function(String?)? validator;
  final bool optional;
  const Dropdown({required this.validator,this.optional=false,required this.hintText,required this.lable,this.selected=false,this.isVerified=false,this.isloading=false,this.onselect,required this.array,required this.controller,super.key});

  @override
  State<Dropdown> createState() => _DropdownState();
}

class _DropdownState extends State<Dropdown> {
  MenuController _menu=MenuController();
  bool isopen=false;
  bool _reserve = false;
  GlobalKey _safespace=GlobalKey();
  void openmenu(){
    if(_menu.isOpen||widget.isloading)return;
    setState(() {
      _reserve=true;
    });
    
    WidgetsBinding.instance.addPostFrameCallback((callback)async{
      if (!mounted) return;
      final c=_safespace.currentContext;
       if(c!=null&&c.mounted){
      await Scrollable.ensureVisible(c,curve: Curves.fastOutSlowIn,
      duration: Duration(milliseconds: 100),alignmentPolicy: ScrollPositionAlignmentPolicy.keepVisibleAtEnd);
    }
    if(mounted&&!_menu.isOpen){
          _menu.open();

    }
    }
    );
   
    
  }
  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text.rich(TextSpan(text:  widget.lable,style: TextStyle(color: Color(0xff0B192C)),
          children: widget.optional?[TextSpan(text: ' (اختياري)',style: TextStyle(color: Color(0XFF94A3B8)))]:[]),),
          
            SizedBox(height: 4,),
            SizedBox(
              height: 50,
              width: double.infinity,
              child:LayoutBuilder(
                builder:(context,constrained){
                  return MenuAnchor(
                  
                  onClose: () {
                    if (!mounted) return;
                    setState(() {
                      isopen=false;
                      _reserve=false;
                    });
                  },
                  onOpen: (){
                    setState(() {
                      isopen=true;
                       
                    });
                  },
                  crossAxisUnconstrained: false,
                  style: MenuStyle(
                    minimumSize: WidgetStatePropertyAll(Size(constrained.maxWidth,0)),
                    maximumSize: WidgetStatePropertyAll(Size(constrained.maxWidth,200)),
                    alignment: Alignment(1, 1.24),
                    backgroundColor: WidgetStatePropertyAll(Color(0xffffffff)),
                    surfaceTintColor: WidgetStatePropertyAll(Colors.transparent),
                    side:WidgetStatePropertyAll(BorderSide(color: Color(0xff2B6CB0,),width: 1),),
                    shape: WidgetStatePropertyAll(RoundedRectangleBorder(borderRadius: BorderRadius.circular(8))),
                    shadowColor: WidgetStatePropertyAll(Color(0XFF2B6CB0).withOpacity(0)) ,
                    elevation: WidgetStatePropertyAll(5)
                  ),
                  controller: _menu,
                  menuChildren:  widget.array.map((e){
                    return MenuItemButton(
                      onPressed: (){
                        setState(() {
                          widget.controller.text=e;
                        
                        });
                          widget.onselect!=null? widget.onselect!():null;
                      },
                      child: Text(e,style: TextStyle(color: Color(0xff475569)),),
                    );
                  }).toList(),
                  builder: (context, controller, child) {
                    return TextFormField(
                      controller: widget.controller,
                      onTap: () {
                        if(controller.isOpen){
                          controller.close();
                        }else{
                          openmenu();
                        }
                      },
                      
                      style: const TextStyle(color:Color(0xff0B192C),fontSize: 14),
                       readOnly: true,
                       enabled: !widget.isloading,
                      validator:widget.validator,
                      decoration:InputDecoration(
                        hintText: widget.hintText,
                        hintStyle: TextStyle(color: Color(0xff94A3B8)),
                      contentPadding: EdgeInsets.symmetric(horizontal: 16,vertical: 12),
                      filled: true,
                      fillColor: Color(0xffffffff),
                      suffixIcon: 
                      isopen? Row( mainAxisAlignment:MainAxisAlignment.end,mainAxisSize: MainAxisSize.min,children: [Container(width: 1,height: 24,color: Color(0xffCBD5E1),),SizedBox(width: 16,),SvgPicture.asset("assets/images/icons/ant-design_caret-up-filled.svg"),SizedBox(width: 16,)],):
                      widget.isVerified?Padding(
                        padding: const EdgeInsets.symmetric(horizontal: 16,vertical: 12),
                        child: SvgPicture.asset("assets/images/icons/truegreen.svg"),
                      ):
                      Row( mainAxisAlignment:MainAxisAlignment.end,mainAxisSize: MainAxisSize.min,children: [Container(width: 1,height: 24,color: Color(0xffCBD5E1),),SizedBox(width: 16,),SvgPicture.asset("assets/images/icons/dropdown.svg"),SizedBox(width: 16,)],),
                       focusedBorder:OutlineInputBorder(
                        borderSide:  BorderSide(color: Color(widget.isVerified?0xff059669 :widget.selected||isopen? 0xff2B6CB0:0xffCBD5E1)),
                        borderRadius: BorderRadius.circular(8)
                      ),
                      enabledBorder:OutlineInputBorder(
                        borderRadius: BorderRadius.circular(8),
                        borderSide: BorderSide(width:1,color: Color(widget.isVerified?0xff059669 :widget.selected||isopen? 0xff2B6CB0:0xffCBD5E1),
                       ),
                       
                      ) ,
                      border: OutlineInputBorder(
                        borderRadius: BorderRadius.circular(8)
                        ,borderSide: BorderSide(color:Color(0xff94A3B8)))
                      
      
                      )
      
                 
                    );
                  },
                  
                  
                  
                  );}
              ),
              
              
              
              
              
            ),
            SizedBox(
              key: _safespace,
              height:isopen||_reserve ?200:0,)
        ],
      ),
    );
  }
}