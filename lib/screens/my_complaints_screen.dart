import 'package:flutter/material.dart';

import '../constants/app_constants.dart';
import '../models/api_exception.dart';
import '../models/ariza_model.dart';
import '../models/user_session.dart';
import '../services/api_service.dart';
import '../widgets/app_widgets.dart';

/// Vatandaşın sadece kendi bildirimlerini, atanma ve çözüm durumlarıyla izlemesini sağlar.
class MyComplaintsScreen extends StatefulWidget {
  const MyComplaintsScreen({super.key, required this.session});

  final UserSession session;

  @override
  State<MyComplaintsScreen> createState() => _MyComplaintsScreenState();
}

class _MyComplaintsScreenState extends State<MyComplaintsScreen> {
  final _api = ApiService.instance;
  bool _isLoading = true;
  String? _error;
  List<ArizaModel> _complaints = const [];

  @override
  void initState() {
    super.initState();
    _loadComplaints();
  }

  Future<void> _loadComplaints() async {
    setState(() {
      _isLoading = true;
      _error = null;
    });
    try {
      final complaints = await _api.getMyComplaints(
        accessToken: widget.session.accessToken,
      );
      if (mounted) setState(() => _complaints = complaints);
    } on ApiException catch (error) {
      if (mounted) setState(() => _error = error.message);
    } catch (_) {
      if (mounted) setState(() => _error = AppStrings.genericConnectionError);
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Arıza takibi')),
      body: _isLoading
          ? const Center(child: CircularProgressIndicator())
          : _error != null
          ? ErrorState(message: _error!, onRetry: _loadComplaints)
          : RefreshIndicator(
              onRefresh: _loadComplaints,
              child: _complaints.isEmpty
                  ? ListView(
                      padding: const EdgeInsets.all(28),
                      children: const [_EmptyComplaintState()],
                    )
                  : ListView.separated(
                      padding: const EdgeInsets.fromLTRB(20, 16, 20, 28),
                      itemCount: _complaints.length + 1,
                      separatorBuilder: (_, _) => const SizedBox(height: 12),
                      itemBuilder: (context, index) {
                        if (index == 0) {
                          return _TrackingHeader(count: _complaints.length);
                        }
                        final complaint = _complaints[index - 1];
                        return _ComplaintTrackingCard(
                          complaint: complaint,
                          onTap: () => _showDetail(complaint),
                        );
                      },
                    ),
            ),
    );
  }

  void _showDetail(ArizaModel complaint) {
    showModalBottomSheet<void>(
      context: context,
      isScrollControlled: true,
      showDragHandle: true,
      builder: (context) => SafeArea(
        child: Padding(
          padding: const EdgeInsets.fromLTRB(22, 4, 22, 30),
          child: SingleChildScrollView(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  complaint.title,
                  style: const TextStyle(
                    fontSize: 22,
                    fontWeight: FontWeight.w800,
                  ),
                ),
                const SizedBox(height: 8),
                _StatusBadge(status: complaint.status),
                const SizedBox(height: 18),
                Text(
                  complaint.description,
                  style: const TextStyle(height: 1.45),
                ),
                const SizedBox(height: 20),
                _DetailItem(
                  icon: Icons.account_tree_outlined,
                  label: 'İlgili birim',
                  value: complaint.departmentLabel,
                ),
                _DetailItem(
                  icon: Icons.schedule_rounded,
                  label: 'Bildirim zamanı',
                  value: complaint.createdAtLabel,
                ),
                const SizedBox(height: 22),
                const Text(
                  'Süreç',
                  style: TextStyle(fontSize: 17, fontWeight: FontWeight.w800),
                ),
                const SizedBox(height: 14),
                _ProgressStep(
                  title: 'Bildirim alındı',
                  subtitle: 'Kaydın doğru birime iletildi.',
                  isDone: true,
                  isLast: false,
                ),
                _ProgressStep(
                  title: 'Ekip süreci',
                  subtitle: complaint.isPending
                      ? 'İlgili ekibin incelemesi bekleniyor.'
                      : 'Ekip görevi üzerine aldı ve çalışıyor.',
                  isDone: !complaint.isPending,
                  isLast: false,
                ),
                _ProgressStep(
                  title: 'Çözüm',
                  subtitle: complaint.status == 'Cozuldu'
                      ? 'Arıza çözülmüş olarak işaretlendi.'
                      : 'Çözüm tamamlandığında burada bilgi verilecek.',
                  isDone: complaint.status == 'Cozuldu',
                  isLast: true,
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}

class _TrackingHeader extends StatelessWidget {
  const _TrackingHeader({required this.count});

  final int count;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(18),
      decoration: BoxDecoration(
        color: const Color(0xFFE8F3FF),
        borderRadius: BorderRadius.circular(22),
      ),
      child: Row(
        children: [
          const Icon(
            Icons.notifications_active_outlined,
            color: AppColors.blue,
          ),
          const SizedBox(width: 12),
          Expanded(
            child: Text(
              '$count bildiriminin güncel durumunu buradan takip edebilirsin.',
              style: const TextStyle(fontWeight: FontWeight.w700, height: 1.35),
            ),
          ),
        ],
      ),
    );
  }
}

class _ComplaintTrackingCard extends StatelessWidget {
  const _ComplaintTrackingCard({required this.complaint, required this.onTap});

  final ArizaModel complaint;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return Material(
      color: Colors.white,
      borderRadius: BorderRadius.circular(20),
      child: InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(20),
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Row(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Container(
                width: 44,
                height: 44,
                decoration: BoxDecoration(
                  color: _statusColor(complaint.status).withValues(alpha: 0.13),
                  borderRadius: BorderRadius.circular(14),
                ),
                child: Icon(
                  Icons.build_outlined,
                  color: _statusColor(complaint.status),
                ),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      complaint.title,
                      maxLines: 2,
                      overflow: TextOverflow.ellipsis,
                      style: const TextStyle(
                        fontWeight: FontWeight.w800,
                        fontSize: 16,
                      ),
                    ),
                    const SizedBox(height: 8),
                    _StatusBadge(status: complaint.status),
                    const SizedBox(height: 9),
                    Text(
                      '${complaint.departmentLabel} · ${complaint.createdAtLabel}',
                      style: const TextStyle(
                        fontSize: 12,
                        color: Colors.blueGrey,
                      ),
                    ),
                  ],
                ),
              ),
              const Icon(Icons.chevron_right_rounded, color: Colors.blueGrey),
            ],
          ),
        ),
      ),
    );
  }
}

class _StatusBadge extends StatelessWidget {
  const _StatusBadge({required this.status});

  final String status;

  @override
  Widget build(BuildContext context) {
    final color = _statusColor(status);
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 5),
      decoration: BoxDecoration(
        color: color.withValues(alpha: 0.12),
        borderRadius: BorderRadius.circular(10),
      ),
      child: Text(
        _statusLabel(status),
        style: TextStyle(
          color: color,
          fontWeight: FontWeight.w800,
          fontSize: 12,
        ),
      ),
    );
  }
}

class _ProgressStep extends StatelessWidget {
  const _ProgressStep({
    required this.title,
    required this.subtitle,
    required this.isDone,
    required this.isLast,
  });

  final String title;
  final String subtitle;
  final bool isDone;
  final bool isLast;

  @override
  Widget build(BuildContext context) {
    final color = isDone ? AppColors.turquoise : Colors.blueGrey.shade300;
    return IntrinsicHeight(
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Column(
            children: [
              Icon(
                isDone ? Icons.check_circle_rounded : Icons.circle_outlined,
                color: color,
              ),
              if (!isLast)
                Expanded(
                  child: Container(
                    width: 2,
                    color: color.withValues(alpha: 0.5),
                  ),
                ),
            ],
          ),
          const SizedBox(width: 12),
          Expanded(
            child: Padding(
              padding: EdgeInsets.only(bottom: isLast ? 0 : 18),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    title,
                    style: const TextStyle(fontWeight: FontWeight.w800),
                  ),
                  const SizedBox(height: 3),
                  Text(
                    subtitle,
                    style: const TextStyle(
                      color: Colors.blueGrey,
                      height: 1.35,
                    ),
                  ),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class _DetailItem extends StatelessWidget {
  const _DetailItem({
    required this.icon,
    required this.label,
    required this.value,
  });

  final IconData icon;
  final String label;
  final String value;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 12),
      child: Row(
        children: [
          Icon(icon, size: 20, color: AppColors.blue),
          const SizedBox(width: 10),
          Expanded(
            child: Text(label, style: const TextStyle(color: Colors.blueGrey)),
          ),
          Flexible(
            child: Text(
              value,
              textAlign: TextAlign.end,
              style: const TextStyle(fontWeight: FontWeight.w700),
            ),
          ),
        ],
      ),
    );
  }
}

class _EmptyComplaintState extends StatelessWidget {
  const _EmptyComplaintState();

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        const SizedBox(height: 110),
        Container(
          width: 78,
          height: 78,
          decoration: const BoxDecoration(
            color: Color(0xFFE8F3FF),
            shape: BoxShape.circle,
          ),
          child: const Icon(
            Icons.assignment_outlined,
            size: 38,
            color: AppColors.blue,
          ),
        ),
        const SizedBox(height: 18),
        const Text(
          'Henüz bir bildirimin yok',
          style: TextStyle(fontSize: 19, fontWeight: FontWeight.w800),
        ),
        const SizedBox(height: 7),
        const Text(
          'Oluşturduğun arızaların sürecini burada takip edebilirsin.',
          textAlign: TextAlign.center,
          style: TextStyle(color: Colors.blueGrey),
        ),
      ],
    );
  }
}

Color _statusColor(String status) {
  switch (status) {
    case 'Cozuldu':
      return const Color(0xFF168456);
    case 'Onarimda':
      return AppColors.orange;
    default:
      return AppColors.blue;
  }
}

String _statusLabel(String status) {
  switch (status) {
    case 'Cozuldu':
      return 'Çözüldü';
    case 'Onarimda':
      return 'Ekip çalışıyor';
    default:
      return 'İnceleme bekliyor';
  }
}
