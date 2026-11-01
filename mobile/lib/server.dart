import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
Dio server=Dio(BaseOptions(
  baseUrl: 'https://clobber-dawn-storage.ngrok-free.dev',
  connectTimeout: Duration(seconds: 10),
  receiveTimeout: Duration(seconds: 30),
  headers: {
    'Content-Type': 'application/json',
    'Accept': 'application/json',

  },
))..interceptors.add(LogInterceptor(
  requestBody: true,
    responseBody: true,
    error: true,
    requestHeader: false,     // مهم: عشان الـ Authorization token ما يتطبعش
    responseHeader: true,
    logPrint: (o) => debugPrint(o.toString()),   
))..interceptors.add(InterceptorsWrapper(
  onRequest: (options, handler) async{
   try {final token=await FlutterSecureStorage().read(key: 'token');
   if(token!=null){
      options.headers['Authorization']='Bearer $token';
    } 
   
   }catch(e){
     debugPrint('SECURE STORAGE ERROR: $e');
   }
   
    handler.next(options);
  },
));
