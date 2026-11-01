import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:mobile/auth/cubitstates.dart/states.dart';
import 'package:mobile/server.dart';

class Cubiteauth extends Cubit<Stateauth> {
  String? requestId;
   int? dependentId;
   String? healthid;
  Cubiteauth():super(Intialauthstates());
  void registrationstep1({required nationalID,required fullName,required address,required dateOfBirth })async{
    emit(RegLooding());
    try{
      final  Response=await server.post(
      '/api/auth/register/patient/start',
      data: {
          "nationalID": nationalID,
          "fullName": fullName,
          "address": address,
          "dateOfBirth": dateOfBirth.toIso8601String().split('.').first,
         
      }
    );
      requestId= Response.data['requestId'];
    emit(Regsuccess());
    }catch(e){
     if(e is DioException){
      debugPrint('data=${e.response?.data}');
      final status=e.response?.statusCode;
      
      if(status==409){
       emit(Regerrordublicatedid());}
      else {print(status); emit(Regerror());}
     
    } else emit(Regerror());}}

      void registrationstep2({ required phoneNumber,required password,required confirmPassword,required healthCardNumber})async{
    emit(RegLooding());
    try{
      final Response=await server.post(
      '/api/auth/register/patient/complete',
      data: {
          "requestId": requestId,
          "phoneNumber": phoneNumber,
          "healthCardNumber": healthCardNumber,
          "password":password,
          "confirmPassword":confirmPassword
      }
    );

    healthid=Response.data['healthRequestId'];


    emit(Regsuccess());
    }catch(e){
     if(e is DioException){
          debugPrint('REG2 status=${e.response?.statusCode} data=${e.response?.data} type=${e.type} err=${e.error}');
      final status=e.response?.statusCode;
      final data=e.response?.data;
      debugPrint('data=${e.response?.data}');
      if(status==400){
          if(data is String){
            emit(Regexpierd());
          }
          else if(data is Map){
            emit(Regerrorpass());

          }
          else emit(Regerror());
        }else emit(Regerror());
     
    } else emit(Regerror());}}

    

    void guardianstep1({required nationalID,required fullName,required address,required dateOfBirth ,healthCardNumber})async{
    emit(guardianLooding());
    try{
      final Response=await server.post(
      '/api/dependents/registration/start',
      data:{
        "fullName":fullName,
        "nationalID":nationalID,
        "address": address,
        "dateOfBirth": dateOfBirth.toIso8601String().split('.').first ,
        "healthCardNumber": healthCardNumber
      }
    );
      requestId= Response.data['requestId'];
    emit(guardiansuccess());
    }catch(e){
     if(e is DioException){
      debugPrint('data=${e.response?.data}');
      final status=e.response?.statusCode;
      final data=e.response?.data;
      
      if(status==409 && data.toString().toLowerCase().contains(('National').toLowerCase())){
       emit(guardianerrordublicatedid());}
      else   if(status==409 && data.toString().toLowerCase().contains(('Health card').toLowerCase())){

       emit(guardianerrordublicatedHealthcard());}
      else {
         print(status); emit(guardianerror());}
     
    } else {
       emit(guardianerror());
     }}}

     void guardianstep2()async{
    emit(guardianLooding());
    try{
      final Response=await server.post(
      '/api/dependents/registration/complete',
      data: {
          "requestId":requestId,
          
      }

    );


     healthid=Response.data['healthRequestId'];
    dependentId=Response.data['dependentId'];

    emit(guardiansuccess());
    }catch(e){
     if(e is DioException){
          debugPrint('guardian2 status=${e.response?.statusCode} data=${e.response?.data} type=${e.type} err=${e.error}');
      final status=e.response?.statusCode;
      final data=e.response?.data;
      if(status==400){
          if(data.toString().toLowerCase().contains(('Invalid or expired registration request.').toLowerCase())){
            emit(guardianexpired());
          }else if(data.toString().toLowerCase().contains(('Kinship must be verified').toLowerCase())){
            emit(guardianverifiederror());
          }
          else emit(guardianerror());
        }  else if (status==403){ emit(guardianpolicyerror());}
        else if(status==401){emit(guardianexpiredtokenerror());}
        else {emit(guardianerror());}
     
    } else{ emit(guardianerror());}}}






    void verificationkinship({required String relationship})async{
        emit(verificationkinshiploading());
    try{ 
      await server.post('/api/dependents/registration/verify-kinship',
     data: {
          "requestId":requestId,
          "relationship": relationship
      }
     );
     emit(verificationkinshipsuccess()); 
     
    }catch(e){
      if(e is DioException){
        debugPrint('data=${e.response?.data}');
            final status=e.response?.statusCode;
            
             if(status==401){
              emit(verificationkinshiptokenerror());
            }
            else if(status==403){
              emit(verificationkinshipwrongaccounttype());
            }
            else if(status==400){
              emit(verificationkinshipexpired());
            }
            else{emit(verificationkinshiperror());}
            
            }else emit(verificationkinshiperror());


    }
    }
    
    void login({required id,required password})async{
      emit(Loginloading());
      try{
        final Response= await server.post(
          '/api/auth/login/patient',
          data: {
          "nationalID": id,
          "password": password
        }
        );
        await const  FlutterSecureStorage().write(key: 'token',value: Response.data['token']);
        emit(Loginsuccess());
      }catch(e){
         if(e is DioException){
          debugPrint('data=${e.response?.data}');
          
          final data =e.response?.data;
          if(data is String)emit(Loginwrongpassorid());
          else emit(Loginerror());
         }else emit(Loginerror());
      }}
   
     void  Forgetpassword({required id,required number})async{
      emit(Forgetloading());
      try{
        final Response=await server.post(
          '/api/auth/forgot-password',
          data: {
          "nationalID": id,
          "phoneNumber": number,
          "accountType": "Patient"
          }
        );
       
         requestId= Response.data['requestId'];
          emit(Forgetsuccess());
      }catch(e){
         if(e is DioException){
          debugPrint('data=${e.response?.data}');
          final status=e.response?.statusCode;
          final data =e.response?.data;
          if(status==503)
          {data['message'].contains('Unable to send the reset code')?
          emit(Forgeterrorconnrction()):emit(Forgeterror());}
          else if(status==400){emit(Forgetwrongidornumber());}
          else {
            emit(Forgeterror());
          }} else{emit(Forgeterror());} } }
    void Resendotp()async{
      emit(ResendOtploading());
      try{
        await server.post(
          '/api/auth/resend-otp',
          data:{
          "requestId": requestId
        }
        );
        emit(ResendOtpsuccess());
      }catch(e){
         if(e is DioException){
            debugPrint('RESEND ERROR status=${e.response?.statusCode} data=${e.response?.data} type=${e.type} msg=${e.message}');
          final status =e.response?.statusCode;
          
          if( status==400)emit(ResendOtpexpired());
          else if(status==429)emit(ResendOtpCooldown());
          else{   emit(ResendOtperror());}

        
         
      }}}
      void otp({required otp})async{
      emit(Otploading());
      try{
         await server.post(
          '/api/auth/verify-otp',
          data:{
          "requestId": requestId,
          "otp": otp
        }
        );
        emit(Otpsuccess());
      }catch(e){
        emit(Otpiswrong());

        
         
      }}
      void resetpassword({required password,required confirmPassword})async{
      emit(Resetpassloading());
      try{
         await server.post(
          '/api/auth/reset-password',
          data:{
          "requestId": requestId,
          "newPassword": password,
          "confirmPassword": confirmPassword
        }
        );
        emit(Resetpasssuccess());
      }catch(e){
        if(e is DioException){
        final status =e.response?.statusCode;
        final massege=e.response?.statusMessage;
        debugPrint('data=${e.response?.data}');
        if(massege!=null&& status==400&&massege.contains('expired'))emit(Otpexpired());
        emit(Resetpasserror());}

        
        emit(Resetpasserror());

        
         
      }}
    
    
    void guardianstep3({required bloodType,required diseaseIcdCodes,required allergyIcdCodes,required familyDiseaseIcdCodes ,medicationIds})async{
    emit(guardianLooding());
    try{
      await server.post(
      '/api/dependents/registration/health',
      data:{
       "requestId": healthid,
        "bloodType": bloodType,
        "diseaseIcdCodes":diseaseIcdCodes,
        "allergyIcdCodes":allergyIcdCodes,
        "familyDiseaseIcdCodes": familyDiseaseIcdCodes,
        "medicationIds": medicationIds
          
      }
    );
     
    emit(guardiansuccess());
    }catch(e){
     if(e is DioException){
      final status=e.response?.statusCode;
      final data=e.response?.data;
      debugPrint('data=${e.response?.data}');
      if(status==400&&data.toString().toLowerCase().contains(('Invalid or expired registration request.').toLowerCase())){
       emit(guardianexpired());}
           else  if(status==400&&data.toString().toLowerCase().contains(('ICD').toLowerCase())){

      print(    "ICD code 'ZZ99' is missing or ambiguous; provide a unique code in the catalog."
         );
          emit(guardianerror());
         }
          else  if(status==400&&data.toString().toLowerCase().contains(('medication').toLowerCase())){
      print(    "Medication identifier 'X00XX00' is missing or ambiguous in the medication catalog.");
      emit(guardianerror());
         ;}else if(status==400){
          print( 'The JSON value could not be converted to Sejily.API.Models.Enums.BloodTyp');
          emit(guardianerror());
         }
         
      else {print(status); emit(guardianerror());}
     
    } else emit(guardianerror());}}
    
     void guardiannothingfieldstep3()async{
    emit(guardianLooding());
    try{
      await server.post(
      '/api/dependents/registration/health',
      data:{
       "requestId": healthid,
       
      }
    );
      
    emit(guardiansuccess());
    }catch(e){
     emit(guardianerror());}}
    
     void registrationstep3({required bloodType,required diseaseIcdCodes,required allergyIcdCodes,required familyDiseaseIcdCodes ,required medicationIds})async{
    emit(RegLooding());
    try{
      await server.post(
      '/api/auth/register/patient/health',
      data:{
       "requestId": healthid,
        "bloodType": bloodType,
        "diseaseIcdCodes":diseaseIcdCodes,
        "allergyIcdCodes":allergyIcdCodes,
        "familyDiseaseIcdCodes": familyDiseaseIcdCodes,
        "medicationIds": medicationIds
          
      }
    );
     
    emit(Regsuccess());
    }catch(e){
     if(e is DioException){
      final status=e.response?.statusCode;
      final data=e.response?.data;
      debugPrint('data=${e.response?.data}');
      
      if(status==400&&data.toString().toLowerCase().contains(('expired').toLowerCase())){
       emit(Regexpierd());}
      else if(status==400&&data.toString().toLowerCase().contains(('ICD').toLowerCase())){
      print(    "ICD code 'ZZ99' is missing or ambiguous; provide a unique code in the catalog."
         );
          emit(Regerror());}
      else if(status==400&&data.toString().toLowerCase().contains(('medication').toLowerCase())){
      print(    "Medication identifier 'X00XX00' is missing or ambiguous in the medication catalog.");
      emit(Regerror());
         ;}else if(status==400){
          print( 'The JSON value could not be converted to Sejily.API.Models.Enums.BloodTyp');
          emit(Regerror());
         }
         
      else {print(status); emit(Regerror());}
     
    } else emit(Regerror());}}
    
     void registrationnothingfieldstep3()async{
    emit(RegLooding());
    try{
      await server.post(
      '/api/auth/register/patient/health',
      data:{
       "requestId": healthid,
       
      }
    );
      
    emit(Regsuccess());
    }catch(e){
      if(e is DioException)
       debugPrint('data=${e.response?.data}');
     emit(Regerror());}}
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    }
