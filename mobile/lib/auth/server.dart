import 'package:dio/dio.dart';
Dio server=Dio(BaseOptions(
  baseUrl: 'https://raisin-luxurious-humped.ngrok-free.dev/',
  connectTimeout: Duration(seconds: 10),
  receiveTimeout: Duration(seconds: 10),
  headers: {
    'Content-Type': 'application/json',
    'Accept': 'application/json',

  },
));
