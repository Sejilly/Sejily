import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

const String apiUrl = String.fromEnvironment(
  'API_URL',
  defaultValue: 'https://sejily.runasp.net',
);

Dio server = Dio(BaseOptions(
  baseUrl: apiUrl,
  connectTimeout: const Duration(seconds: 10),
  receiveTimeout: const Duration(seconds: 30),
  headers: {
    'Content-Type': 'application/json',
    'Accept': 'application/json',
  },
))
  ..interceptors.add(LogInterceptor(
    requestBody: true,
    responseBody: true,
    error: true,
    requestHeader: false,
    responseHeader: true,
    logPrint: (o) => debugPrint(o.toString()),
  ))
  ..interceptors.add(InterceptorsWrapper(
    onRequest: (options, handler) async {
      try {
        final token = await const FlutterSecureStorage().read(key: 'token');
        if (token != null) {
          options.headers['Authorization'] = 'Bearer $token';
        }
      } catch (e) {
        debugPrint('SECURE STORAGE ERROR: $e');
      }
      handler.next(options);
    },
  ));
