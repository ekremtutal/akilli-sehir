import 'package:flutter/material.dart';

import 'constants/app_constants.dart';
import 'constants/app_theme.dart';
import 'screens/role_selection_screen.dart';

void main() {
  runApp(const SehirTakipUygulamasi());
}

/// Uygulama başlangıcı, tema ve kök yönlendirme tanımı burada tutulur.
class SehirTakipUygulamasi extends StatelessWidget {
  const SehirTakipUygulamasi({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      debugShowCheckedModeBanner: false,
      title: AppStrings.appTitle,
      theme: AppTheme.light,
      initialRoute: AppRoutes.roleSelection,
      routes: {AppRoutes.roleSelection: (_) => const RoleSelectionScreen()},
    );
  }
}
