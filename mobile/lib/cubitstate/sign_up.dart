import 'package:equatable/equatable.dart';

class SignUp extends Equatable {
  final String name;
  final int nationalid;
  final String adress;
  final String birthdate;
  final int phone;
  final int? healthcardnumber;
  final String password;
  final String?  chronicdiseases;
  final String? allergies ;
  final String? familyhistory;
  final String? medications;
  final String bloodType;
  final int currentindex;
  const SignUp({this.name="", this.nationalid=0, this.adress="", this.birthdate="", this.phone=0, this.healthcardnumber, this.password="", this.chronicdiseases, this.allergies, this.familyhistory, this.medications, this.bloodType="", this.currentindex=0});
  @override
  List<Object?> get props => [
    name, nationalid,adress,birthdate,phone,healthcardnumber,password,chronicdiseases, allergies ,familyhistory,medications, bloodType,currentindex];
  SignUp copywith(  
      String? name,
      int? nationalid,
      String? adress,
      String? birthdate,
      int? phone,
      int? healthcardnumber,
      String? password,
      String?  chronicdiseases,
      String? allergies ,
      String? familyhistory,
      String? medications,
      String? bloodType,
      int? currentindex,){
    return SignUp(
        name:name??this.name,
        nationalid:nationalid??this.nationalid,
        adress:adress??this.adress,
        birthdate:birthdate??this.birthdate,
        phone:phone??this.phone,
        healthcardnumber:healthcardnumber??this.healthcardnumber,
        password:password??this.password,
        chronicdiseases:chronicdiseases??this.chronicdiseases,
        allergies:allergies??this.allergies ,
        familyhistory:familyhistory??this.familyhistory,
        medications:medications??this.medications,
        bloodType:bloodType??this.bloodType,
        currentindex:currentindex??this.currentindex

    );
   }


}