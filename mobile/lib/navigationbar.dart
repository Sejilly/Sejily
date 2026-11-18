import 'package:flutter/material.dart';
import 'package:flutter_svg/flutter_svg.dart';
import 'package:mobile/homepag/homeScreen.dart';


class Navigationbar extends StatefulWidget {
  const Navigationbar({super.key});

  @override
  State<Navigationbar> createState() => _NavigationbarState();
}

class _NavigationbarState extends State<Navigationbar> {
  int crruntindex=0;  
  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Color(0xffF8FAFC),
      body: HomeScreen(),
     bottomNavigationBar:  DecoratedBox(
  
       decoration: BoxDecoration(
        color: Color(0xffffffffff),
        borderRadius: BorderRadius.only(topLeft: Radius.circular(20),topRight: Radius.circular(20)),
        boxShadow: [
        BoxShadow(
          color: Color(0xff000000).withOpacity(0.05),
            blurRadius:15,
          
            ),
        
       ]),
       child: ClipRRect(
        borderRadius: BorderRadius.only(topLeft: Radius.circular(20),topRight: Radius.circular(20)),
         child: Padding(

           padding: const EdgeInsets.symmetric(vertical: 12),
           child: Theme(
             data: Theme.of(context).copyWith(
              splashColor: Colors.transparent,
              highlightColor: Colors.transparent
             ),
             child: BottomNavigationBar(
              
                type: BottomNavigationBarType.fixed,
                currentIndex: crruntindex,
                elevation: 0,
                backgroundColor: Color(0xffffffff),
                selectedItemColor: Color(0xff2B6CB0),
                unselectedItemColor: Color(0xff94A3B8),
                selectedFontSize: 12,
                unselectedFontSize: 12,
                selectedLabelStyle: TextStyle(color:Color(0xff2B6CB0) ,fontWeight: FontWeight.w600),
                unselectedLabelStyle:TextStyle(color:Color(0xff94A3B8) ,fontWeight: FontWeight.w600) ,
                iconSize: 24,
                
                
                onTap: (index){
                  setState(() {
                     crruntindex=index;
                  });
                },
                items: [
                  BottomNavigationBarItem(
                    icon: Padding(
                      padding: const EdgeInsets.only(bottom: 8),
                      child: SvgPicture.asset('assets/images/icons/navigationbar/fluent_home-32-regular.svg'),
                    ),
                    activeIcon: Padding(
                      padding: const EdgeInsets.only( bottom: 8.0),
                      child: SvgPicture.asset('assets/images/icons/navigationbar/fluent_home-32-filled.svg',),
                    )
                  ,label: "الرئيسية"),
                              BottomNavigationBarItem(
                    icon: Padding(
                      padding: const EdgeInsets.only(bottom:  8.0),
                      child: SvgPicture.asset('assets/images/icons/navigationbar/basil_lock-time-outline.svg'),
                    ),
                    activeIcon: Padding(
                      padding: const EdgeInsets.only(bottom:  8.0),
                      child: SvgPicture.asset('assets/images/icons/navigationbar/basil_lock-time-solid.svg',),
                    )
                  ,label: "الأذونات"),
             
                  BottomNavigationBarItem(
                    icon: Padding(
                      padding: const EdgeInsets.only(bottom:  8.0),
                      child: SvgPicture.asset('assets/images/icons/navigationbar/clarity_flask-line.svg'),
                    ),
                    activeIcon: Padding(
                      padding: const EdgeInsets.only(bottom:  8.0),
                      child: SvgPicture.asset('assets/images/icons/navigationbar/clarity_flask-line.svg',colorFilter: ColorFilter.mode(Color(0xff2B6CB0), BlendMode.srcIn),),
                    )
                  ,label: "الفحوصات"),
             
                  BottomNavigationBarItem(
                    icon: Padding(
                      padding: const EdgeInsets.only(bottom: 8,),
                      child: SvgPicture.asset('assets/images/icons/navigationbar/ep_user.svg'),
                    ),
                    activeIcon: Padding(
                      padding: const EdgeInsets.only(bottom:  8.0),
                      child: SvgPicture.asset('assets/images/icons/navigationbar/ep_user.svg',colorFilter: ColorFilter.mode(Color(0xff2B6CB0), BlendMode.srcIn),),
                    )
                  ,label: "حسابي"),
             
                ]),
           ),
         ),
       ),
     ),
    
    );
  }
}