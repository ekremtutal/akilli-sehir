import 'package:flutter/material.dart';

import '../constants/app_constants.dart';
import '../models/api_exception.dart';
import '../services/api_service.dart';
import '../widgets/app_widgets.dart';
import 'citizen_home_screen.dart';
import 'citizen_registration_screen.dart';
import 'personnel_tasks_screen.dart';

enum LoginMode { citizen, personnel }

class LoginScreen extends StatefulWidget {
  const LoginScreen({super.key, required this.mode});

  final LoginMode mode;

  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  final _formKey = GlobalKey<FormState>();
  final _identifierController = TextEditingController();
  final _passwordController = TextEditingController();
  final _api = ApiService.instance;
  bool _hidePassword = true;
  bool _isLoading = false;

  bool get _isCitizen => widget.mode == LoginMode.citizen;

  @override
  void dispose() {
    _identifierController.dispose();
    _passwordController.dispose();
    super.dispose();
  }

  Future<void> _login() async {
    if (!_formKey.currentState!.validate()) return;
    setState(() => _isLoading = true);

    try {
      final session = _isCitizen
          ? await _api.loginCitizen(
              usernameOrEmail: _identifierController.text.trim(),
              password: _passwordController.text,
            )
          : await _api.loginPersonnel(
              corporateEmail: _identifierController.text.trim(),
              password: _passwordController.text,
            );

      if (!mounted) return;
      final destination = session.isPersonnel
          ? PersonnelTasksScreen(session: session)
          : CitizenHomeScreen(session: session);
      Navigator.of(context).pushAndRemoveUntil(
        MaterialPageRoute(builder: (_) => destination),
        (route) => false,
      );
    } on ApiException catch (error) {
      _showMessage(error.message);
    } catch (_) {
      _showMessage(AppStrings.genericConnectionError);
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  void _showMessage(String message) {
    ScaffoldMessenger.of(
      context,
    ).showSnackBar(SnackBar(content: Text(message)));
  }

  @override
  Widget build(BuildContext context) {
    final identifierLabel = _isCitizen
        ? 'Kullanıcı adı veya e-posta'
        : 'Kurumsal e-posta adresi';
    final subtitle = _isCitizen
        ? 'Belediye hizmetlerine hızlı ve güvenli biçimde eriş.'
        : 'Kurumsal e-posta hesabınla birimine gelen görevleri yönet.';

    return Scaffold(
      appBar: AppBar(),
      body: SafeArea(
        top: false,
        child: ListView(
          padding: const EdgeInsets.fromLTRB(24, 10, 24, 30),
          children: [
            _LoginHero(isCitizen: _isCitizen),
            const SizedBox(height: 28),
            Text(
              _isCitizen ? 'Tekrar hoş geldin' : 'Saha ekibine hoş geldin',
              style: const TextStyle(fontSize: 27, fontWeight: FontWeight.w800),
            ),
            const SizedBox(height: 8),
            Text(subtitle, style: const TextStyle(color: Colors.blueGrey)),
            const SizedBox(height: 26),
            Form(
              key: _formKey,
              autovalidateMode: AutovalidateMode.onUserInteraction,
              child: Column(
                children: [
                  TextFormField(
                    controller: _identifierController,
                    keyboardType: _isCitizen
                        ? TextInputType.text
                        : TextInputType.emailAddress,
                    decoration: InputDecoration(
                      labelText: identifierLabel,
                      prefixIcon: const Icon(Icons.alternate_email_rounded),
                    ),
                    validator: (value) => (value ?? '').trim().isEmpty
                        ? '$identifierLabel zorunludur.'
                        : null,
                  ),
                  const SizedBox(height: 14),
                  TextFormField(
                    controller: _passwordController,
                    obscureText: _hidePassword,
                    decoration: InputDecoration(
                      labelText: 'Parola',
                      prefixIcon: const Icon(Icons.lock_outline_rounded),
                      suffixIcon: IconButton(
                        onPressed: () =>
                            setState(() => _hidePassword = !_hidePassword),
                        icon: Icon(
                          _hidePassword
                              ? Icons.visibility_outlined
                              : Icons.visibility_off_outlined,
                        ),
                      ),
                    ),
                    validator: (value) =>
                        (value ?? '').isEmpty ? 'Parola zorunludur.' : null,
                  ),
                  const SizedBox(height: 22),
                  PrimaryButton(
                    label: _isLoading
                        ? 'Giriş yapılıyor...'
                        : 'Güvenle giriş yap',
                    icon: Icons.arrow_forward_rounded,
                    isLoading: _isLoading,
                    onPressed: _login,
                  ),
                  if (_isCitizen) ...[
                    const SizedBox(height: 12),
                    FooterLink(
                      question: 'Henüz hesabın yok mu?',
                      linkLabel: 'Kayıt ol',
                      onTap: () => Navigator.of(context).push(
                        MaterialPageRoute(
                          builder: (_) => const CitizenRegistrationScreen(),
                        ),
                      ),
                    ),
                  ],
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _LoginHero extends StatelessWidget {
  const _LoginHero({required this.isCitizen});

  final bool isCitizen;

  @override
  Widget build(BuildContext context) {
    final color = isCitizen ? AppColors.blue : AppColors.turquoise;
    final icon = isCitizen ? Icons.person_rounded : Icons.engineering_rounded;
    return Container(
      height: 158,
      padding: const EdgeInsets.all(22),
      decoration: BoxDecoration(
        gradient: LinearGradient(
          colors: [AppColors.navy, color.withValues(alpha: 0.92)],
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
        ),
        borderRadius: BorderRadius.circular(28),
      ),
      child: Row(
        children: [
          Expanded(
            child: Text(
              isCitizen
                  ? 'Şehrin için\nbir fark yarat.'
                  : 'Görevlerini\nkolayca yönet.',
              style: const TextStyle(
                color: Colors.white,
                fontSize: 24,
                height: 1.15,
                fontWeight: FontWeight.w800,
              ),
            ),
          ),
          Container(
            width: 72,
            height: 72,
            decoration: BoxDecoration(
              color: Colors.white.withValues(alpha: 0.18),
              borderRadius: BorderRadius.circular(24),
            ),
            child: Icon(icon, color: Colors.white, size: 38),
          ),
        ],
      ),
    );
  }
}
