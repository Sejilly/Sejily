import 'dart:convert';

import 'package:flutter/foundation.dart';
import 'package:flutter/services.dart';
import 'package:mobile/model/icd11model.dart';

class Icd11servece {
  static bool isloaded=false;
  static List<Icd11model> icd11list=[];
  static Future<void> loaddata()async{
   if(isloaded)return;
   print("--- [1] محاولة قراءة ملف الـ JSON ---");
   try{
     final String data= await rootBundle.loadString('assets/json/icd11.json') ;
     print("--- [2] تم قراءة الملف بنجاح، طول البيانات: ${data.length} ---");
  icd11list=await compute(_jsontolis, data) as List<Icd11model>;
  isloaded=true;
 print("--- [3] تم تحويل الـ JSON بنجاح، عدد العناصر: ${icd11list.length} ---");
  }catch(e,stackTrace){
    print("Error loading JSON: $e");
    print(stackTrace);
  }

   }
   static List<Icd11model> _jsontolis(String data) {
    List<dynamic> datadecode=json.decode(data);
    return datadecode.map((e)=>Icd11model.fromjson(e)).toList();
      }
    
  
}