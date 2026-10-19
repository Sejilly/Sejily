import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:mobile/auth/cubitstates.dart/states.dart';
import 'package:mobile/auth/server.dart';

class Cubiteauth extends Cubit<Stateauth> {
  String? requestId;
  Cubiteauth():super(Intialauthstates());
  void registration({required nationalID,required fullName,required address,required dateOfBirth ,required bloodType ,required phoneNumber,required password,required confirmPassword,required healthCardNumber})async{
    emit(RegLooding());
    try{
      final Response=await server.post(
      '/api/auth/register/patient',
      data: {
          "nationalID": nationalID,
          "fullName": fullName,
          "address": address,
          "dateOfBirth": dateOfBirth,
          "bloodType": bloodType,
          "phoneNumber":phoneNumber,
          "password": password,
          "confirmPassword": confirmPassword,
          "healthCardNumber": healthCardNumber
      }
    );
    emit(Regsuccess());
    }catch(e){
     if(e is DioException){
      final status=e.response?.statusCode;
      final data=e.response?.data;
      if(status==400){
       if(data is String)emit(Regerrordublicatedid());
       else if(data is List){
        emit(Regerrorpass());
       }
      }
      else emit(Regerror());
     }else emit(Regerror());
    } }
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
        emit(Loginsuccess());
      }catch(e){
         if(e is DioException){
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
          final massege=e.response?.statusMessage;
          if( status==400);
          else if(status==429)emit(ResendOtpCooldown());
          else{   emit(ResendOtperror());}

        
         
      }}}
      void otp({required otp})async{
      emit(Otploading());
      try{
        final Response= await server.post(
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
        final Response= await server.post(
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
        if(massege!=null&& status==400&&massege.contains('expired'))emit(Otpexpired());
        emit(Resetpasserror());}

        
        emit(Resetpasserror());

        
         
      }}

    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    }
