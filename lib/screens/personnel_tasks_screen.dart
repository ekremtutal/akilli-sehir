import 'package:flutter/material.dart';

import '../constants/app_constants.dart';
import '../models/api_exception.dart';
import '../models/ariza_model.dart';
import '../models/user_session.dart';
import '../services/api_service.dart';
import '../widgets/app_widgets.dart';
import 'role_selection_screen.dart';

/// Saha personelinin yalnız kendi birimine yönlendirilen görevleri yönettiği ekran.
class PersonnelTasksScreen extends StatefulWidget {
  const PersonnelTasksScreen({super.key, required this.session});

  final UserSession session;

  @override
  State<PersonnelTasksScreen> createState() => _PersonnelTasksScreenState();
}

class _PersonnelTasksScreenState extends State<PersonnelTasksScreen> {
  final _api = ApiService.instance;
  List<ArizaModel> _complaints = [];
  bool _isLoading = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    _loadTasks();
  }

  Future<void> _loadTasks() async {
    if (widget.session.department == null) {
      setState(() {
        _isLoading = false;
        _error = 'Bu personel hesabına birim atanmamış.';
      });
      return;
    }

    setState(() {
      _isLoading = true;
      _error = null;
    });
    try {
      final complaints = await _api.getComplaints(
        accessToken: widget.session.accessToken,
      );
      if (mounted) setState(() => _complaints = complaints);
    } on ApiException catch (error) {
      if (mounted) setState(() => _error = error.message);
    } catch (_) {
      if (mounted) setState(() => _error = 'Görevler sunucudan alınamadı.');
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  Future<void> _claimTask(
    ArizaModel complaint,
    BuildContext tabContext,
    BuildContext sheetContext,
  ) async {
    try {
      await _api.updateComplaintStatus(
        complaintId: complaint.id,
        status: 'Onarimda',
        accessToken: widget.session.accessToken,
      );
      await _loadTasks();
      if (!mounted || !sheetContext.mounted) return;
      Navigator.pop(sheetContext);
      DefaultTabController.of(tabContext).animateTo(1);
      _showMessage('Görev üzerine alındı.');
    } on ApiException catch (error) {
      _showMessage(error.message);
    } catch (_) {
      _showMessage('Görev güncellenemedi.');
    }
  }

  Future<void> _markResolved(
    ArizaModel complaint,
    BuildContext sheetContext,
  ) async {
    try {
      await _api.updateComplaintStatus(
        complaintId: complaint.id,
        status: 'Cozuldu',
        accessToken: widget.session.accessToken,
      );
      await _loadTasks();
      if (!mounted || !sheetContext.mounted) return;
      Navigator.pop(sheetContext);
      _showMessage('Görev çözüldü olarak işaretlendi.');
    } on ApiException catch (error) {
      _showMessage(error.message);
    } catch (_) {
      _showMessage('Görev güncellenemedi.');
    }
  }

  Future<void> _logout() async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (dialogContext) => AlertDialog(
        icon: const Icon(Icons.logout_rounded, color: Colors.redAccent),
        title: const Text('Çıkış yapılsın mı?'),
        content: const Text('Bu cihazdaki saha personeli oturumu kapatılacak.'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(dialogContext, false),
            child: const Text('Vazgeç'),
          ),
          FilledButton(
            style: FilledButton.styleFrom(backgroundColor: Colors.redAccent),
            onPressed: () => Navigator.pop(dialogContext, true),
            child: const Text('Çıkış yap'),
          ),
        ],
      ),
    );

    if (confirmed != true || !mounted) return;
    Navigator.of(context).pushAndRemoveUntil(
      MaterialPageRoute(builder: (_) => const RoleSelectionScreen()),
      (route) => false,
    );
  }

  void _showMessage(String message) {
    if (mounted) {
      ScaffoldMessenger.of(
        context,
      ).showSnackBar(SnackBar(content: Text(message)));
    }
  }

  @override
  Widget build(BuildContext context) {
    final pending = _complaints.where((item) => item.isPending).toList();
    final mine = _complaints
        .where(
          (item) =>
              item.isInProgress &&
              item.assignedPersonnelId == widget.session.id,
        )
        .toList();

    return DefaultTabController(
      length: 2,
      child: Scaffold(
        appBar: AppBar(
          title: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              const Text(
                'Saha görevleri',
                style: TextStyle(fontWeight: FontWeight.w800),
              ),
              Text(
                ArizaModel.departmentName(widget.session.department ?? 'Diger'),
                style: const TextStyle(fontSize: 12, color: Colors.blueGrey),
              ),
            ],
          ),
          actions: [
            IconButton(
              tooltip: 'Görevleri yenile',
              onPressed: _loadTasks,
              icon: const Icon(Icons.refresh_rounded),
            ),
            TextButton.icon(
              onPressed: _logout,
              style: TextButton.styleFrom(foregroundColor: Colors.redAccent),
              icon: const Icon(Icons.logout_rounded),
              label: const Text('Çıkış'),
            ),
          ],
          bottom: const TabBar(
            tabs: [
              Tab(text: 'Bekleyen'),
              Tab(text: 'Üzerimde'),
            ],
          ),
        ),
        body: Builder(
          builder: (tabContext) => TabBarView(
            children: [
              _TaskList(
                isLoading: _isLoading,
                error: _error,
                complaints: pending,
                emptyMessage: 'Birimine atanmış bekleyen görev yok.',
                isMine: false,
                onRetry: _loadTasks,
                onTap: (complaint) =>
                    _showDetails(complaint, false, tabContext),
              ),
              _TaskList(
                isLoading: _isLoading,
                error: _error,
                complaints: mine,
                emptyMessage: 'Şu an üzerinde açık görev yok.',
                isMine: true,
                onRetry: _loadTasks,
                onTap: (complaint) => _showDetails(complaint, true, tabContext),
              ),
            ],
          ),
        ),
      ),
    );
  }

  void _showDetails(
    ArizaModel complaint,
    bool isMine,
    BuildContext tabContext,
  ) {
    showModalBottomSheet<void>(
      context: context,
      isScrollControlled: true,
      builder: (sheetContext) => SafeArea(
        child: SingleChildScrollView(
          padding: const EdgeInsets.fromLTRB(20, 14, 20, 28),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Center(
                child: Container(
                  width: 42,
                  height: 4,
                  decoration: BoxDecoration(
                    color: Colors.grey.shade300,
                    borderRadius: BorderRadius.circular(8),
                  ),
                ),
              ),
              const SizedBox(height: 18),
              Text(
                complaint.title,
                style: const TextStyle(
                  fontSize: 23,
                  fontWeight: FontWeight.w800,
                ),
              ),
              const SizedBox(height: 10),
              Wrap(
                spacing: 8,
                runSpacing: 8,
                children: [
                  _Tag(
                    label: complaint.departmentLabel,
                    color: const Color(0xFFE1EFFF),
                  ),
                  _Tag(
                    label: complaint.urgency,
                    color: complaint.urgency == 'Yuksek'
                        ? const Color(0xFFFFE0DD)
                        : const Color(0xFFFFF1CC),
                  ),
                ],
              ),
              const SizedBox(height: 16),
              if (complaint.photoUrl.isNotEmpty)
                ClipRRect(
                  borderRadius: BorderRadius.circular(20),
                  child: Image.network(
                    ApiConfig.fileUrl(complaint.photoUrl),
                    width: double.infinity,
                    height: 190,
                    fit: BoxFit.cover,
                    errorBuilder: (_, _, _) => const _NoPhoto(),
                  ),
                )
              else
                const _NoPhoto(),
              const SizedBox(height: 16),
              Text(complaint.description, style: const TextStyle(height: 1.45)),
              const SizedBox(height: 14),
              _DetailRow(
                icon: Icons.location_on_outlined,
                text:
                    '${complaint.latitude.toStringAsFixed(5)}, ${complaint.longitude.toStringAsFixed(5)}',
              ),
              _DetailRow(
                icon: Icons.calendar_today_outlined,
                text: complaint.createdAtLabel,
              ),
              if (complaint.aiReason != null) ...[
                const SizedBox(height: 12),
                InfoCard(
                  icon: Icons.auto_awesome_rounded,
                  message: complaint.aiReason!,
                ),
              ],
              const SizedBox(height: 24),
              if (!isMine)
                PrimaryButton(
                  label: 'Görevi üzerime al',
                  icon: Icons.assignment_turned_in_rounded,
                  onPressed: () =>
                      _claimTask(complaint, tabContext, sheetContext),
                )
              else
                PrimaryButton(
                  label: 'Görevi çözüldü yap',
                  icon: Icons.task_alt_rounded,
                  onPressed: () => _markResolved(complaint, sheetContext),
                ),
            ],
          ),
        ),
      ),
    );
  }
}

class _TaskList extends StatelessWidget {
  const _TaskList({
    required this.isLoading,
    required this.error,
    required this.complaints,
    required this.emptyMessage,
    required this.isMine,
    required this.onRetry,
    required this.onTap,
  });

  final bool isLoading;
  final String? error;
  final List<ArizaModel> complaints;
  final String emptyMessage;
  final bool isMine;
  final Future<void> Function() onRetry;
  final ValueChanged<ArizaModel> onTap;

  @override
  Widget build(BuildContext context) {
    if (isLoading) return const Center(child: CircularProgressIndicator());
    if (error != null) {
      return ErrorState(message: error!, onRetry: () => onRetry());
    }
    if (complaints.isEmpty) {
      return Center(
        child: Text(
          emptyMessage,
          style: const TextStyle(color: Colors.blueGrey),
        ),
      );
    }

    return RefreshIndicator(
      onRefresh: onRetry,
      child: ListView.separated(
        padding: const EdgeInsets.fromLTRB(16, 16, 16, 28),
        itemCount: complaints.length,
        separatorBuilder: (_, _) => const SizedBox(height: 12),
        itemBuilder: (context, index) => _TaskCard(
          complaint: complaints[index],
          isMine: isMine,
          onTap: () => onTap(complaints[index]),
        ),
      ),
    );
  }
}

class _TaskCard extends StatelessWidget {
  const _TaskCard({
    required this.complaint,
    required this.isMine,
    required this.onTap,
  });

  final ArizaModel complaint;
  final bool isMine;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    final urgencyColor = complaint.urgency == 'Yuksek'
        ? Colors.redAccent
        : complaint.urgency == 'Orta'
        ? Colors.orange
        : AppColors.turquoise;
    return Card(
      elevation: 0,
      color: Colors.white,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(20)),
      child: InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(20),
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                children: [
                  Container(
                    width: 42,
                    height: 42,
                    decoration: BoxDecoration(
                      color: urgencyColor.withValues(alpha: 0.12),
                      borderRadius: BorderRadius.circular(14),
                    ),
                    child: Icon(
                      Icons.build_circle_outlined,
                      color: urgencyColor,
                    ),
                  ),
                  const SizedBox(width: 12),
                  Expanded(
                    child: Text(
                      complaint.title,
                      maxLines: 2,
                      overflow: TextOverflow.ellipsis,
                      style: const TextStyle(
                        fontWeight: FontWeight.w800,
                        fontSize: 16,
                      ),
                    ),
                  ),
                  const Icon(
                    Icons.chevron_right_rounded,
                    color: Colors.blueGrey,
                  ),
                ],
              ),
              const SizedBox(height: 14),
              Row(
                children: [
                  _Tag(
                    label: complaint.urgency,
                    color: urgencyColor.withValues(alpha: 0.14),
                  ),
                  const SizedBox(width: 8),
                  _Tag(
                    label: isMine ? 'Üzerimde' : complaint.departmentLabel,
                    color: const Color(0xFFEAF0F7),
                  ),
                  const Spacer(),
                  Text(
                    complaint.createdAtLabel,
                    style: const TextStyle(
                      fontSize: 11,
                      color: Colors.blueGrey,
                    ),
                  ),
                ],
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _Tag extends StatelessWidget {
  const _Tag({required this.label, required this.color});

  final String label;
  final Color color;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 9, vertical: 5),
      decoration: BoxDecoration(
        color: color,
        borderRadius: BorderRadius.circular(10),
      ),
      child: Text(
        label,
        style: const TextStyle(fontSize: 11, fontWeight: FontWeight.w700),
      ),
    );
  }
}

class _DetailRow extends StatelessWidget {
  const _DetailRow({required this.icon, required this.text});

  final IconData icon;
  final String text;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 8),
      child: Row(
        children: [
          Icon(icon, size: 18, color: AppColors.blue),
          const SizedBox(width: 8),
          Expanded(child: Text(text)),
        ],
      ),
    );
  }
}

class _NoPhoto extends StatelessWidget {
  const _NoPhoto();

  @override
  Widget build(BuildContext context) {
    return Container(
      width: double.infinity,
      height: 130,
      alignment: Alignment.center,
      decoration: BoxDecoration(
        color: const Color(0xFFEAF0F7),
        borderRadius: BorderRadius.circular(20),
      ),
      child: const Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(Icons.image_not_supported_outlined, color: Colors.blueGrey),
          SizedBox(height: 6),
          Text('Bu ihbar için fotoğraf eklenmemiş.'),
        ],
      ),
    );
  }
}
