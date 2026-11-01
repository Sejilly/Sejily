import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:mobile/auth/cubitstates.dart/cubite.dart';
import 'package:mobile/auth/model/icd11servece.dart';
import 'package:mobile/splash.dart';
import 'package:flutter_localizations/flutter_localizations.dart';

Future<void> main() async {
  WidgetsFlutterBinding.ensureInitialized();
await Icd11servece.loaddata();
runApp(const MyApp());
}

class MyApp extends StatelessWidget {
const MyApp({super.key});

@override
Widget build(BuildContext context) {
  return BlocProvider(create: (_)=>Cubiteauth(),
    child:  MaterialApp(
    debugShowCheckedModeBanner: false,
    title: 'Sejily',
    theme: ThemeData(
      fontFamily: 'Vazirmatin',
      useMaterial3: true,
    ),
    locale: const Locale('ar', 'EG'), // أو Locale('ar')

    supportedLocales: const [

    Locale('ar', 'EG'), // العربية

    Locale('en', 'US'), // الإنجليزية

    ],

    localizationsDelegates: const [

    GlobalMaterialLocalizations.delegate,

    GlobalWidgetsLocalizations.delegate,

    GlobalCupertinoLocalizations.delegate,

    ], 


    builder: (context, child) {
      return Directionality(
      textDirection: TextDirection.rtl,
       child: child!,
    );
    },
    
    home:  const Splash())
  );
}
}