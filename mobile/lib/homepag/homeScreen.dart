import 'package:flutter/material.dart';
import 'package:mobile/homepag/accessRequestCard.dart';
import 'package:mobile/homepag/homeheader.dart';

class HomeScreen extends StatefulWidget {
  const HomeScreen({super.key});

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen> {
  @override
  Widget build(BuildContext context) {
    return const Scaffold(
      backgroundColor: Color(0xffF8FAFC),
      body: SafeArea(
        child: SingleChildScrollView(
          child: Column(
            children: [
              Homeheader(),
              AccessRequestCard()

              
            ],
          ),
        ),
      ),
    );
  }
}