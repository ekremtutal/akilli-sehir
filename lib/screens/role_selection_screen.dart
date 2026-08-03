import 'package:flutter/material.dart';

import '../constants/app_constants.dart';
import 'login_screen.dart';

/// Uygulamaya giren kişinin vatandaş veya saha personeli akışını seçtiği ekran.
class RoleSelectionScreen extends StatelessWidget {
  const RoleSelectionScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: DecoratedBox(
        decoration: const BoxDecoration(
          gradient: LinearGradient(
            colors: [AppColors.navy, Color(0xFF104C85)],
            begin: Alignment.topCenter,
            end: Alignment.bottomCenter,
          ),
        ),
        child: SafeArea(
          child: Padding(
            padding: const EdgeInsets.fromLTRB(24, 20, 24, 28),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                const Row(
                  children: [
                    _MunicipalityBadge(lightBackground: false),
                    SizedBox(width: 10),
                    Text(
                      'ADANA BÜYÜKŞEHİR',
                      style: TextStyle(
                        color: Colors.white,
                        letterSpacing: 1.1,
                        fontSize: 12,
                        fontWeight: FontWeight.w700,
                      ),
                    ),
                  ],
                ),
                const Spacer(flex: 2),
                const Text(
                  'Şehrinle\nbağlantıda kal.',
                  style: TextStyle(
                    color: Colors.white,
                    fontSize: 38,
                    height: 1.08,
                    fontWeight: FontWeight.w800,
                  ),
                ),
                const SizedBox(height: 14),
                const Text(
                  'Bir arızayı birkaç adımda bildir; süreç boyunca neler olduğunu takip et.',
                  style: TextStyle(
                    color: Color(0xFFD9E9F9),
                    height: 1.45,
                    fontSize: 16,
                  ),
                ),
                const Spacer(),
                _RoleCard(
                  icon: Icons.person_rounded,
                  title: 'Vatandaş Girişi',
                  description: 'Arıza bildir, süreci takip et',
                  color: Colors.white,
                  textColor: AppColors.navy,
                  onTap: () => _goTo(
                    context,
                    const LoginScreen(mode: LoginMode.citizen),
                  ),
                ),
                const SizedBox(height: 14),
                _RoleCard(
                  icon: Icons.engineering_rounded,
                  title: 'Saha Personeli Girişi',
                  description: 'Birimine gelen görevleri yönet',
                  color: const Color(0xFF1A5D96),
                  textColor: Colors.white,
                  borderColor: const Color(0xFF4C89BC),
                  onTap: () => _goTo(
                    context,
                    const LoginScreen(mode: LoginMode.personnel),
                  ),
                ),
                const SizedBox(height: 24),
                const Row(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    Icon(
                      Icons.verified_user_outlined,
                      color: Color(0xFFB9D6EE),
                      size: 17,
                    ),
                    SizedBox(width: 7),
                    Text(
                      'Güvenli belediye hizmet platformu',
                      style: TextStyle(color: Color(0xFFB9D6EE), fontSize: 12),
                    ),
                  ],
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }

  void _goTo(BuildContext context, Widget screen) {
    Navigator.of(context).push(MaterialPageRoute(builder: (_) => screen));
  }
}

class _MunicipalityBadge extends StatelessWidget {
  const _MunicipalityBadge({required this.lightBackground});

  final bool lightBackground;

  @override
  Widget build(BuildContext context) {
    return Container(
      width: 34,
      height: 34,
      alignment: Alignment.center,
      decoration: BoxDecoration(
        shape: BoxShape.circle,
        color: lightBackground ? AppColors.navy : Colors.white,
      ),
      child: Icon(
        Icons.location_city_rounded,
        size: 20,
        color: lightBackground ? Colors.white : AppColors.navy,
      ),
    );
  }
}

class _RoleCard extends StatelessWidget {
  const _RoleCard({
    required this.icon,
    required this.title,
    required this.description,
    required this.color,
    required this.textColor,
    required this.onTap,
    this.borderColor,
  });

  final IconData icon;
  final String title;
  final String description;
  final Color color;
  final Color textColor;
  final Color? borderColor;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return Material(
      color: Colors.transparent,
      child: InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(22),
        child: Ink(
          padding: const EdgeInsets.all(18),
          decoration: BoxDecoration(
            color: color,
            borderRadius: BorderRadius.circular(22),
            border: borderColor == null
                ? null
                : Border.all(color: borderColor!),
          ),
          child: Row(
            children: [
              Container(
                width: 48,
                height: 48,
                decoration: BoxDecoration(
                  color: textColor.withValues(alpha: 0.10),
                  borderRadius: BorderRadius.circular(16),
                ),
                child: Icon(icon, color: textColor),
              ),
              const SizedBox(width: 14),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      title,
                      style: TextStyle(
                        color: textColor,
                        fontWeight: FontWeight.w800,
                        fontSize: 16,
                      ),
                    ),
                    const SizedBox(height: 3),
                    Text(
                      description,
                      style: TextStyle(
                        color: textColor.withValues(alpha: 0.72),
                      ),
                    ),
                  ],
                ),
              ),
              Icon(Icons.arrow_forward_rounded, color: textColor),
            ],
          ),
        ),
      ),
    );
  }
}
