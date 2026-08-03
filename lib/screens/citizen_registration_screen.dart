import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

import '../constants/app_constants.dart';
import '../models/api_exception.dart';
import '../services/api_service.dart';
import '../widgets/app_widgets.dart';
import 'citizen_home_screen.dart';

/// Vatandaşların kendi hesaplarını oluşturduğu ekran.
class CitizenRegistrationScreen extends StatefulWidget {
  const CitizenRegistrationScreen({super.key});

  @override
  State<CitizenRegistrationScreen> createState() =>
      _CitizenRegistrationScreenState();
}

class _CitizenRegistrationScreenState extends State<CitizenRegistrationScreen> {
  final _formKey = GlobalKey<FormState>();
  final _fullNameController = TextEditingController();
  final _phoneController = TextEditingController();
  final _emailController = TextEditingController();
  final _nationalIdController = TextEditingController();
  final _usernameController = TextEditingController();
  final _passwordController = TextEditingController();
  final _confirmPasswordController = TextEditingController();
  final _api = ApiService.instance;
  bool _hidePassword = true;
  bool _isLoading = false;

  @override
  void dispose() {
    _fullNameController.dispose();
    _phoneController.dispose();
    _emailController.dispose();
    _nationalIdController.dispose();
    _usernameController.dispose();
    _passwordController.dispose();
    _confirmPasswordController.dispose();
    super.dispose();
  }

  Future<void> _register() async {
    if (!_formKey.currentState!.validate()) return;
    setState(() => _isLoading = true);

    try {
      final session = await _api.registerCitizen(
        fullName: _fullNameController.text.trim(),
        email: _emailController.text.trim(),
        phoneNumber: _phoneController.text.trim(),
        nationalId: _nationalIdController.text.trim(),
        username: _usernameController.text.trim(),
        password: _passwordController.text,
      );

      if (!mounted) return;
      Navigator.of(context).pushAndRemoveUntil(
        MaterialPageRoute(builder: (_) => CitizenHomeScreen(session: session)),
        (route) => false,
      );
    } on ApiException catch (error) {
      _showMessage(error.message);
    } catch (_) {
      _showMessage(
        'Kayıt sırasında ${AppStrings.genericConnectionError.toLowerCase()}',
      );
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
    return Scaffold(
      appBar: AppBar(title: const Text('Vatandaş hesabı oluştur')),
      body: SafeArea(
        top: false,
        child: Form(
          key: _formKey,
          autovalidateMode: AutovalidateMode.onUserInteraction,
          child: ListView(
            padding: const EdgeInsets.fromLTRB(20, 8, 20, 30),
            children: [
              const InfoCard(
                icon: Icons.lock_person_outlined,
                message:
                    'Bilgilerin yalnızca belediye hizmetlerine erişimin için güvenle işlenir.',
              ),
              const SizedBox(height: 22),
              const FormSectionHeader(
                number: '1',
                title: 'Kişisel bilgiler',
                description: 'Sana özel hizmet sunabilmemiz için gerekli.',
              ),
              const SizedBox(height: 14),
              TextFormField(
                controller: _fullNameController,
                textCapitalization: TextCapitalization.words,
                decoration: const InputDecoration(
                  labelText: 'Ad soyad',
                  prefixIcon: Icon(Icons.person_outline),
                  helperText: 'En az 3 karakter giriniz.',
                ),
                validator: _validateFullName,
              ),
              const SizedBox(height: 12),
              TextFormField(
                controller: _phoneController,
                keyboardType: TextInputType.phone,
                inputFormatters: [
                  FilteringTextInputFormatter.allow(RegExp(r'[0-9+ ]')),
                ],
                decoration: const InputDecoration(
                  labelText: 'Telefon numarası',
                  prefixIcon: Icon(Icons.phone_outlined),
                  helperText: '10–15 rakam giriniz.',
                ),
                validator: _validatePhone,
              ),
              const SizedBox(height: 12),
              TextFormField(
                controller: _emailController,
                keyboardType: TextInputType.emailAddress,
                decoration: const InputDecoration(
                  labelText: 'E-posta adresi',
                  prefixIcon: Icon(Icons.mail_outline),
                  helperText: 'Örnek: adiniz@eposta.com',
                ),
                validator: _validateEmail,
              ),
              const SizedBox(height: 12),
              TextFormField(
                controller: _nationalIdController,
                keyboardType: TextInputType.number,
                maxLength: 11,
                inputFormatters: [FilteringTextInputFormatter.digitsOnly],
                decoration: const InputDecoration(
                  labelText: 'T.C. kimlik numarası',
                  prefixIcon: Icon(Icons.badge_outlined),
                  counterText: '',
                  helperText:
                      '11 haneli, geçerli T.C. kimlik numaranızı giriniz.',
                ),
                validator: _validateNationalId,
              ),
              const SizedBox(height: 24),
              const FormSectionHeader(
                number: '2',
                title: 'Giriş bilgileri',
                description: 'Bu bilgilerle daha sonra hesabına erişeceksin.',
              ),
              const SizedBox(height: 14),
              TextFormField(
                controller: _usernameController,
                decoration: const InputDecoration(
                  labelText: 'Kullanıcı adı',
                  prefixIcon: Icon(Icons.alternate_email),
                  helperText:
                      '3–50 karakter; boşluk ve Türkçe karakter kullanmayınız.',
                ),
                validator: _validateUsername,
              ),
              const SizedBox(height: 12),
              TextFormField(
                controller: _passwordController,
                obscureText: _hidePassword,
                decoration: InputDecoration(
                  labelText: 'Parola',
                  prefixIcon: const Icon(Icons.lock_outline),
                  helperText: 'En az 8 karakter kullanınız.',
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
                validator: _validatePassword,
              ),
              const SizedBox(height: 12),
              TextFormField(
                controller: _confirmPasswordController,
                obscureText: _hidePassword,
                decoration: const InputDecoration(
                  labelText: 'Parolayı tekrar yaz',
                  prefixIcon: Icon(Icons.lock_reset_outlined),
                ),
                validator: _validatePasswordConfirmation,
              ),
              const SizedBox(height: 24),
              PrimaryButton(
                label: _isLoading
                    ? 'Hesabın hazırlanıyor...'
                    : 'Hesabımı oluştur',
                icon: Icons.check_circle_outline,
                isLoading: _isLoading,
                onPressed: _register,
              ),
            ],
          ),
        ),
      ),
    );
  }

  String? _validateFullName(String? value) {
    if ((value ?? '').trim().length < 3) {
      return 'Ad soyad en az 3 karakter olmalıdır.';
    }
    return null;
  }

  String? _validatePhone(String? value) {
    final digits = (value ?? '').replaceAll(RegExp(r'\D'), '');
    if (digits.length < 10 || digits.length > 15) {
      return 'Telefon numarası 10 ile 15 rakam arasında olmalıdır.';
    }
    return null;
  }

  String? _validateEmail(String? value) {
    final email = (value ?? '').trim();
    if (!RegExp(r'^[^\s@]+@[^\s@]+\.[^\s@]+$').hasMatch(email)) {
      return 'Geçerli bir e-posta adresi giriniz.';
    }
    return null;
  }

  String? _validateNationalId(String? value) {
    final nationalId = (value ?? '').trim();
    if (nationalId.length != 11 || !RegExp(r'^\d{11}$').hasMatch(nationalId)) {
      return 'T.C. kimlik numarası 11 haneli olmalıdır.';
    }
    if (nationalId.startsWith('0')) {
      return 'T.C. kimlik numarası 0 ile başlayamaz.';
    }

    final digits = nationalId.codeUnits.map((unit) => unit - 48).toList();
    final odd = digits[0] + digits[2] + digits[4] + digits[6] + digits[8];
    final even = digits[1] + digits[3] + digits[5] + digits[7];
    final tenthDigit = ((odd * 7) - even) % 10;
    final eleventhDigit =
        digits.take(10).reduce((sum, digit) => sum + digit) % 10;
    if (tenthDigit != digits[9] || eleventhDigit != digits[10]) {
      return 'T.C. kimlik numarasını kontrol ediniz.';
    }
    return null;
  }

  String? _validateUsername(String? value) {
    final username = (value ?? '').trim();
    if (username.length < 3 || username.length > 50) {
      return 'Kullanıcı adı 3 ile 50 karakter arasında olmalıdır.';
    }
    if (!RegExp(r'^[a-zA-Z0-9._-]+$').hasMatch(username)) {
      return 'Yalnız harf, rakam, nokta, alt çizgi ve tire kullanabilirsiniz.';
    }
    return null;
  }

  String? _validatePassword(String? value) {
    if ((value ?? '').length < 8) {
      return 'Parola en az 8 karakter olmalıdır.';
    }
    return null;
  }

  String? _validatePasswordConfirmation(String? value) {
    if ((value ?? '').isEmpty) {
      return 'Lütfen parolanızı tekrar yazınız.';
    }
    if (value != _passwordController.text) {
      return 'Parolalar eşleşmiyor.';
    }
    return null;
  }
}
