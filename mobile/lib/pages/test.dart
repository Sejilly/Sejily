import 'package:flutter/material.dart';

class Selectcard extends StatefulWidget {
  final int index;
  final IconData icon;
  final String title;
  final String description;

  const Selectcard({required this.icon,required this.title,required this.description,required this.index, super.key});

  @override
  State<Selectcard> createState() => _SelectcardState();
}

class _SelectcardState extends State<Selectcard> {
  int backgroundColor=0xffFFFFFF;
  int bordercolor=0xffCBD5E1;
  bool isselect=false;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: 
    GestureDetector(
      onTap: () {
        setState(() {
          isselect=true;
        });
      },
      child:Padding(
        padding: const EdgeInsets.all(50),
        child: AnimatedContainer(
          width: 150,
          height: 139,
          duration: const Duration(milliseconds: 300),
          decoration: BoxDecoration(
            borderRadius: BorderRadius.circular(8),
            color: isselect?const Color(0xffD9E5EF):const Color(0xffFFFFFF),
            border: Border.all(
              color:isselect?const Color(0xff2B6CB0) :Color(bordercolor)
            )),
          child: Padding(
            padding: const EdgeInsets.symmetric(vertical: 30),
            child: Column(children: [
              Icon(widget.icon,size: 32,color:isselect?const Color(0xff2B6CB0): const Color(0xff64748B),),
              Text(widget.title,style: TextStyle(color: isselect?const Color(0xff2B6CB0):const Color(0xff475569),fontSize: 16,fontWeight: FontWeight.w600),),
              Text(widget.description,style: TextStyle(color: isselect?const Color(0xff2B6CB0):const Color(0xff475569),fontSize: 14,fontWeight: FontWeight.w400),)
            ],),
          ),
        ),
      ) ,),);
  }
}
