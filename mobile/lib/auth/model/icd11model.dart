class Icd11model {
  final String code;
  final String arabtitle;
  final String engtitle;



  Icd11model({required this.code, required this.arabtitle, required this.engtitle});

  factory  Icd11model.fromjson(Map<String,dynamic> json){
    return Icd11model(code: json['code'].toString().replaceAll(RegExp(r'^([\-\–\—\s]+)'), '').trim(), arabtitle: json['arabicTitle'].toString().replaceAll(RegExp(r'^([\-\–\—\s]+)'), '').trim(), engtitle: json['englishTitle'].toString().replaceAll(RegExp(r'^([\-\–\—\s]+)'), '').trim());
  }
  
  }