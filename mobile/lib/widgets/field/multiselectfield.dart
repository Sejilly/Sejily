

import 'package:flutter/material.dart';
import 'package:flutter_svg/svg.dart';
import 'package:mobile/model/icd11model.dart';

class Multiselectfield extends StatefulWidget {
  final String lable;
  final String hintText;
  final Function(bool)? onFocusChange;
  final TextEditingController controller;
  final List<Icd11model> suggestions ;
  final List<Icd11model>? selectedItems;
  final Function(List<Icd11model>)? onChanged;
  final bool lastpadding;
  final bool odd;
  final List<String>?result;


  final bool optional;
  const Multiselectfield({ this.result,this.odd=false,this.lastpadding=true, this.onChanged,required this.selectedItems,this.optional=false,required this.hintText,required this.lable,required this.suggestions,required this.controller,this.onFocusChange,super.key});

  @override
  State<Multiselectfield> createState() => _MultiselectfieldState();
}

class _MultiselectfieldState extends State<Multiselectfield> {
  late FocusNode _focusNode;
  bool foucss=false;
 List<Icd11model> filtereditem=[];
 @override
  void initState(){
    super.initState();
   filtereditem=widget.suggestions;
   _focusNode=FocusNode();
   _focusNode.addListener((){
     if (mounted)
    setState(() {

      foucss=_focusNode.hasFocus;
    });
   });
  
    
  }
  @override
  void dispose() {
    // TODO: implement dispose
   
    _focusNode.dispose();
     super.dispose();
  }
  @override
  void didUpdateWidget(covariant Multiselectfield oldWidget) {
    // TODO: implement didUpdateWidget
    super.didUpdateWidget(oldWidget);
    if(oldWidget.suggestions!=widget.suggestions){
    
        filtereditem = widget.suggestions;

     
    }
  }
  
  void toggle(Icd11model item){
    final List<Icd11model> update=List.from(widget.selectedItems??[]);
    if(update.contains(item)){
      update.remove(item);
    }
    else{
      update.add(item);
    }
    if( widget.onChanged!=null)
    widget.onChanged!(update);
  }
  void addCustom(){
   final String text=widget.controller.text.trim();
   if(text.isEmpty)return;
   if(widget.result!=null){
   if ( !widget.result!.contains(text)){
    setState(() {
       widget.result!.add(text);
       widget.controller.clear();
    });
   
   }
   }
  }

  
  void onchange(String text){
    if(text.trim().isEmpty){
      setState(() {
         filtereditem=widget.suggestions;
      });
     
    }
    else{
      setState(() {
        filtereditem=widget.suggestions.where((item)=>item.arabtitle.toLowerCase().replaceAll(RegExp(r'[\u064B-\u065F\u0670\u0640]'), '').contains(text.toLowerCase().replaceAll(RegExp(r'[\u064B-\u065F\u0670\u0640]'), ''))).toList();
      });
    }
    if(!_menu.isOpen)openmenu();
  }
  
  
  MenuController _menu=MenuController();
  bool isopen=false;
  bool _reserve = false;
  GlobalKey _safespace=GlobalKey();
  void openmenu(){
    if(_menu.isOpen)return;
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
    debugPrint('${widget.lable}: suggestions=${widget.suggestions.length} filtered=${filtereditem.length}');
    final select=widget.selectedItems??[];
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text.rich(TextSpan(text:  widget.lable,style: TextStyle(color: Color(0xff0B192C)),
          children: widget.optional?[TextSpan(text:widget.odd? ' (إن وُجد)':' (إن وُجدت)',style: TextStyle(color: Color(0XFF94A3B8)))]:[]),),
          
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
                    if (!mounted) return;
                    setState(() {
                      isopen=true;
                       
                    });
                  },
                  crossAxisUnconstrained: false,
                  style: MenuStyle(
                    padding: const WidgetStatePropertyAll(EdgeInsets.zero),
                    minimumSize: WidgetStatePropertyAll(Size(constrained.maxWidth,0)),
                    maximumSize: WidgetStatePropertyAll(Size(constrained.maxWidth,144)),
                    alignment: const Alignment(1, 1.24),
                    backgroundColor: const WidgetStatePropertyAll(Color(0xffffffff)),
                    surfaceTintColor: WidgetStatePropertyAll(Colors.transparent),
                    side:WidgetStatePropertyAll(BorderSide(color: Color(0xff2B6CB0,),width: 1),),
                    shape: WidgetStatePropertyAll(RoundedRectangleBorder(borderRadius: BorderRadius.circular(8))),
                    shadowColor: WidgetStatePropertyAll(Color(0XFF2B6CB0).withOpacity(0)) ,
                    elevation: WidgetStatePropertyAll(5),
                    
                  ),
                  controller: _menu,
                  menuChildren: [
                    if(!(widget.result!=null))
                    ...filtereditem.take(50).map((e){
                    final bool isselect=widget.selectedItems?.contains(e)??false;
                    return MenuItemButton(
                      closeOnActivate: false,
                      onPressed: ()=>toggle(e),
                      child: Padding(
                        padding: const EdgeInsets.only(right: 2,bottom: 0,top: 0),
                        child: Row(
                          children: [
                            SvgPicture.asset(isselect?"assets/images/icons/isselect.svg":"assets/images/icons/noselect.svg"),
                            SizedBox(width: 8,),
                            Flexible(
                              child: Text.rich(
                                TextSpan(
                                 text:e.arabtitle,style: TextStyle(color: Color(isselect?0xff0B192C:0xff475569)),),),
                            )
                          ],
                        ),
                      ),
                    );

                  }),if(!(widget.result!=null))
                  if(filtereditem.isEmpty)
                    MenuItemButton(
                      closeOnActivate: false,
                      
                      child: Padding(
                        padding: const EdgeInsets.only(right: 2,bottom: 0,top: 0),
                        child: Row(
                          children: [
                            
                            
                            Flexible(
                              child: Text.rich(
                                TextSpan(
                               text:  'لا توجد نتائج ',style: TextStyle(color: Color(0xff475569)),),),
                            )
                          ],
                        ),
                      ),
                    ),
                  if(widget.result!=null)
                    if(widget.controller.text.trim().isNotEmpty&&!(widget.result?.contains(widget.controller.text.trim())??false))
                    MenuItemButton(
                      closeOnActivate: false,
                      onPressed: addCustom,
                      child: Padding(
                        padding: const EdgeInsets.only(right: 2,bottom: 0,top: 0),
                        child: Row(
                          children: [
                            
                            
                            Flexible(
                              child: Text.rich(
                                TextSpan(
                               text:  'اضافه ${widget.controller.text.trim()}',style: TextStyle(color: Color(0xff475569)),),),
                            )
                          ],
                        ),
                      ),
                    )
                  
                  ],
                  builder: (context, controller, child) {
                    final field=
                       
                        TextField(
                                    controller: widget.controller,
                                    onChanged: onchange,
                                    onTap: () {
                                      if(controller.isOpen){
                                        controller.close();
                                      }else{
                                        openmenu();
                                      }
                                    },
                                    focusNode: _focusNode,
                                    style: const TextStyle(color:Color(0xff0B192C),fontSize: 14),
                                    decoration:InputDecoration(
                                    isDense: true,
                                   
                                    hintText:(widget.result != null)
                                    ? (widget.result!.isEmpty ? widget.hintText : '') // لو الحقل مخصص للـ Custom Results
                                    : (select.isEmpty ? widget.hintText : ''),
                                    hintStyle: TextStyle(color: Color(0xff94A3B8)),
                                    contentPadding: EdgeInsets.zero,
                                    filled: true,
                                    fillColor: Color(0xffffffff),
                                    border: InputBorder.none,
                                    enabledBorder: InputBorder.none,
                                    focusedBorder: InputBorder.none,
                                    disabledBorder: InputBorder.none,
                                    errorBorder: InputBorder.none,
                                    
                                    
                                                            ));
                    
                    return SizedBox(
                      height: 50,
                      child: InputDecorator(
                        
                        isFocused: foucss,
                        isEmpty: select.isEmpty&&widget.controller.text.trim().isEmpty,
                        decoration: InputDecoration(
                        contentPadding: EdgeInsets.symmetric(horizontal:select.isEmpty&&!foucss?1:16,vertical: 12),
                        filled: true,
                        fillColor: Color(0xffffffff),
                         focusedBorder:OutlineInputBorder(
                          borderSide:  BorderSide(color: Color(0xff2B6CB0)),
                          borderRadius: BorderRadius.circular(8)
                        ),
                        enabledBorder:OutlineInputBorder(
                          borderRadius: BorderRadius.circular(8),
                          borderSide: BorderSide(width:1,color: Color(0xffCBD5E1),
                         ),
                         
                        ) ,
                        border: OutlineInputBorder(
                          borderRadius: BorderRadius.circular(8)
                          ,borderSide: BorderSide(color:Color(0xff94A3B8)))
                        
                      
                        ),
                        child: Row(
                          mainAxisAlignment: MainAxisAlignment.start,
                          children: [
                            Flexible(
                              child: SingleChildScrollView(scrollDirection: Axis.horizontal,
                              reverse: false,
                              child:Row(
                                children: [
                                  SizedBox(width: 10,),
                                   if( select.isEmpty&&!foucss&&widget.result==null)
                                  SvgPicture.asset('assets/images/icons/akar-icons_search.svg'),
                                   if( select.isEmpty&&!foucss&&widget.result==null)SizedBox(width: 10,),
                                  if(select.isNotEmpty)
                                  Row(
                                    
                                    children:select.map((item){
                                     return Padding(padding: EdgeInsets.only(left: 4),
                                     child: Chip(
                                      padding: const EdgeInsets.only(right: 8, top: 0,bottom:10),
                                      labelPadding: EdgeInsets.zero,
                                      visualDensity: VisualDensity.compact,
                                     
                                      label: Text( item.arabtitle,style: TextStyle(color: Color(0xff0B192C),fontSize: 12),),
                                     materialTapTargetSize: MaterialTapTargetSize.shrinkWrap,
                                     
                                     backgroundColor: Color(0xff2B6CB0).withOpacity(.35),
                                     deleteIcon: SvgPicture.asset("assets/images/icons/delete.svg"),
                                     onDeleted:()=> toggle(item),
                                     shape: RoundedRectangleBorder(
                                      borderRadius: BorderRadius.circular(50),
                                      side: BorderSide(color: Color(0Xff2B6CB0))
                                     ),
                                     
                                     
                                     
                                     
                                      ),);
                                    }).toList(),
                                  ),
                                  if(widget.result!=null&&widget.result!.isNotEmpty)
                                    Row(
                                    
                                    children:widget.result!.map((item){
                                     return Padding(padding: EdgeInsets.only(left: 4),
                                     child: Chip(
                                      padding: const EdgeInsets.only(right: 8, top: 0,bottom:10),
                                      labelPadding: EdgeInsets.zero,
                                      visualDensity: VisualDensity.compact,
                                     
                                      label: Text( item,style: TextStyle(color: Color(0xff0B192C),fontSize: 12),),
                                     materialTapTargetSize: MaterialTapTargetSize.shrinkWrap,
                                     
                                     backgroundColor: Color(0xff2B6CB0).withOpacity(.35),
                                     deleteIcon: SvgPicture.asset("assets/images/icons/delete.svg"),
                                     onDeleted:(){

                                      setState(() {
                                        widget.result!.remove(item);
                                      });
                                     },
                                     shape: RoundedRectangleBorder(
                                      borderRadius: BorderRadius.circular(50),
                                      side: BorderSide(color: Color(0Xff2B6CB0))
                                     ),
                                     
                                     
                                     
                                     
                                      ),);
                                    }).toList(),
                                  ),
                                   
                                  SizedBox(width: 300,child: field,)
                                ],
                              ) ,),
                            ),
                           
                          ],
                        ),),
                    )
                    
                    
                   ;
                  },
                  
                  
                  
                  );}
              ),
              
              
              
              
              
            ),
            SizedBox(
              key: _safespace,
              height:isopen||_reserve ?(filtereditem.length>2?144:filtereditem.length>1&&widget.controller.text.trim().isNotEmpty?144
              :filtereditem.length>0&&widget.controller.text.trim().isNotEmpty?96:widget.controller.text.trim().isNotEmpty||foucss?41:0):0,),
               widget.lastpadding?SizedBox(height: 20,):SizedBox.shrink()
        ],
       
      ),
    );
  }
}