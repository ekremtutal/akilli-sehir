import 'package:flutter/material.dart';

import '../constants/app_constants.dart';
import '../models/user_session.dart';
import 'complaint_report_screen.dart';
import 'role_selection_screen.dart';

/// Vatandaş oturumunun ana kabuğu; ana sayfa, bildirim formu ve hesap sekmelerini yönetir.
class CitizenHomeScreen extends StatefulWidget {
  const CitizenHomeScreen({super.key, required this.session});

  final UserSession session;

  @override
  State<CitizenHomeScreen> createState() => _CitizenHomeScreenState();
}

class _CitizenHomeScreenState extends State<CitizenHomeScreen> {
  int _selectedTab = 0;

  @override
  Widget build(BuildContext context) {
    final pages = [
      _CitizenDashboard(
        session: widget.session,
        openReport: () => setState(() => _selectedTab = 1),
      ),
      ComplaintReportScreen(session: widget.session),
      _CitizenAccountScreen(session: widget.session),
    ];

    return Scaffold(
      body: SafeArea(child: pages[_selectedTab]),
      bottomNavigationBar: NavigationBar(
        selectedIndex: _selectedTab,
        onDestinationSelected: (index) => setState(() => _selectedTab = index),
        indicatorColor: const Color(0xFFD9EAFB),
        destinations: const [
          NavigationDestination(
            icon: Icon(Icons.home_outlined),
            selectedIcon: Icon(Icons.home_rounded),
            label: 'Ana sayfa',
          ),
          NavigationDestination(
            icon: Icon(Icons.add_location_alt_outlined),
            selectedIcon: Icon(Icons.add_location_alt_rounded),
            label: 'Bildir',
          ),
          NavigationDestination(
            icon: Icon(Icons.person_outline_rounded),
            selectedIcon: Icon(Icons.person_rounded),
            label: 'Hesabım',
          ),
        ],
      ),
    );
  }
}

class _CitizenDashboard extends StatelessWidget {
  const _CitizenDashboard({required this.session, required this.openReport});

  final UserSession session;
  final VoidCallback openReport;

  @override
  Widget build(BuildContext context) {
    return CustomScrollView(
      slivers: [
        SliverToBoxAdapter(
          child: Padding(
            padding: const EdgeInsets.fromLTRB(20, 18, 20, 8),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  children: [
                    Container(
                      width: 42,
                      height: 42,
                      decoration: const BoxDecoration(
                        color: AppColors.navy,
                        shape: BoxShape.circle,
                      ),
                      child: const Icon(
                        Icons.location_city_rounded,
                        color: Colors.white,
                      ),
                    ),
                    const SizedBox(width: 10),
                    const Expanded(
                      child: Text(
                        'Adana Akıllı Şehir',
                        style: TextStyle(
                          fontSize: 16,
                          fontWeight: FontWeight.w800,
                        ),
                      ),
                    ),
                    IconButton(
                      onPressed: () {},
                      icon: const Icon(Icons.notifications_none_rounded),
                    ),
                  ],
                ),
                const SizedBox(height: 26),
                Text(
                  'Merhaba, ${session.firstName}',
                  style: const TextStyle(
                    fontSize: 29,
                    fontWeight: FontWeight.w800,
                  ),
                ),
                const SizedBox(height: 6),
                const Text(
                  'Şehrin için fark yaratmaya hazır mısın?',
                  style: TextStyle(color: Colors.blueGrey),
                ),
                const SizedBox(height: 22),
                _ReportBanner(onTap: openReport),
                const SizedBox(height: 24),
                const Text(
                  'Hızlı işlemler',
                  style: TextStyle(fontSize: 18, fontWeight: FontWeight.w800),
                ),
                const SizedBox(height: 12),
                Row(
                  children: [
                    Expanded(
                      child: _QuickActionCard(
                        icon: Icons.add_location_alt_rounded,
                        title: 'Arıza bildir',
                        color: const Color(0xFFE1EFFF),
                        onTap: openReport,
                      ),
                    ),
                    const SizedBox(width: 12),
                    Expanded(
                      child: _QuickActionCard(
                        icon: Icons.support_agent_rounded,
                        title: 'Destek al',
                        color: const Color(0xFFE8F5E9),
                        onTap: () => ScaffoldMessenger.of(context).showSnackBar(
                          const SnackBar(
                            content: Text('Destek merkezi yakında burada.'),
                          ),
                        ),
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 28),
                const Text(
                  'Nasıl çalışır?',
                  style: TextStyle(fontSize: 18, fontWeight: FontWeight.w800),
                ),
                const SizedBox(height: 12),
                const _HowItWorksCard(),
                const SizedBox(height: 20),
              ],
            ),
          ),
        ),
      ],
    );
  }
}

class _ReportBanner extends StatelessWidget {
  const _ReportBanner({required this.onTap});

  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return Material(
      color: Colors.transparent,
      child: InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(26),
        child: Ink(
          padding: const EdgeInsets.all(22),
          decoration: BoxDecoration(
            gradient: const LinearGradient(
              colors: [AppColors.navy, AppColors.blue],
              begin: Alignment.topLeft,
              end: Alignment.bottomRight,
            ),
            borderRadius: BorderRadius.circular(26),
          ),
          child: Row(
            children: [
              const Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      'Bir sorun mu gördün?',
                      style: TextStyle(
                        color: Colors.white,
                        fontSize: 20,
                        fontWeight: FontWeight.w800,
                      ),
                    ),
                    SizedBox(height: 6),
                    Text(
                      'Konum, fotoğraf ve kısa bir açıklama yeterli.',
                      style: TextStyle(color: Color(0xFFDDEEFF), height: 1.35),
                    ),
                  ],
                ),
              ),
              Container(
                width: 54,
                height: 54,
                decoration: BoxDecoration(
                  color: Colors.white.withValues(alpha: 0.16),
                  borderRadius: BorderRadius.circular(18),
                ),
                child: const Icon(
                  Icons.add_location_alt_rounded,
                  color: Colors.white,
                  size: 29,
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _QuickActionCard extends StatelessWidget {
  const _QuickActionCard({
    required this.icon,
    required this.title,
    required this.color,
    required this.onTap,
  });

  final IconData icon;
  final String title;
  final Color color;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return Material(
      color: Colors.transparent,
      child: InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(20),
        child: Ink(
          height: 118,
          padding: const EdgeInsets.all(16),
          decoration: BoxDecoration(
            color: color,
            borderRadius: BorderRadius.circular(20),
          ),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Icon(icon, color: AppColors.navy),
              const Spacer(),
              Text(title, style: const TextStyle(fontWeight: FontWeight.w800)),
            ],
          ),
        ),
      ),
    );
  }
}

class _HowItWorksCard extends StatelessWidget {
  const _HowItWorksCard();

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(18),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(22),
      ),
      child: const Row(
        children: [
          _StepBadge(number: '1'),
          Expanded(child: Text('Konumu ve sorunu paylaş.')),
          Icon(Icons.arrow_forward_rounded, color: Colors.blueGrey),
          _StepBadge(number: '2'),
          Expanded(child: Text('Doğru birime yönlendirelim.')),
        ],
      ),
    );
  }
}

class _StepBadge extends StatelessWidget {
  const _StepBadge({required this.number});

  final String number;

  @override
  Widget build(BuildContext context) {
    return Container(
      width: 26,
      height: 26,
      alignment: Alignment.center,
      margin: const EdgeInsets.only(right: 7),
      decoration: const BoxDecoration(
        color: AppColors.turquoise,
        shape: BoxShape.circle,
      ),
      child: Text(
        number,
        style: const TextStyle(
          color: Colors.white,
          fontWeight: FontWeight.w800,
        ),
      ),
    );
  }
}

class _CitizenAccountScreen extends StatelessWidget {
  const _CitizenAccountScreen({required this.session});

  final UserSession session;

  @override
  Widget build(BuildContext context) {
    return ListView(
      padding: const EdgeInsets.fromLTRB(20, 24, 20, 30),
      children: [
        const Text(
          'Hesabım',
          style: TextStyle(fontSize: 28, fontWeight: FontWeight.w800),
        ),
        const SizedBox(height: 20),
        Container(
          padding: const EdgeInsets.all(18),
          decoration: BoxDecoration(
            color: Colors.white,
            borderRadius: BorderRadius.circular(24),
          ),
          child: Row(
            children: [
              CircleAvatar(
                radius: 28,
                backgroundColor: const Color(0xFFDDEBFA),
                child: Text(
                  session.firstName.substring(0, 1).toUpperCase(),
                  style: const TextStyle(
                    color: AppColors.navy,
                    fontSize: 22,
                    fontWeight: FontWeight.w800,
                  ),
                ),
              ),
              const SizedBox(width: 14),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      session.fullName,
                      style: const TextStyle(
                        fontWeight: FontWeight.w800,
                        fontSize: 17,
                      ),
                    ),
                    const SizedBox(height: 4),
                    Text(
                      session.email,
                      style: const TextStyle(color: Colors.blueGrey),
                    ),
                  ],
                ),
              ),
            ],
          ),
        ),
        const SizedBox(height: 18),
        ListTile(
          tileColor: Colors.white,
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(18),
          ),
          leading: const Icon(Icons.logout_rounded, color: Colors.redAccent),
          title: const Text('Güvenli çıkış yap'),
          onTap: () => Navigator.of(context).pushAndRemoveUntil(
            MaterialPageRoute(builder: (_) => const RoleSelectionScreen()),
            (route) => false,
          ),
        ),
      ],
    );
  }
}
