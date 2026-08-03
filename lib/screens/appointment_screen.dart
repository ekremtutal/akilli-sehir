import 'package:flutter/material.dart';

import '../constants/app_constants.dart';
import '../models/api_exception.dart';
import '../models/randevu_model.dart';
import '../models/user_session.dart';
import '../services/api_service.dart';
import '../widgets/app_widgets.dart';

/// Vatandaşın uygun birimi ve saati seçerek dijital randevu oluşturduğu ekran.
class AppointmentScreen extends StatefulWidget {
  const AppointmentScreen({super.key, required this.session});

  final UserSession session;

  @override
  State<AppointmentScreen> createState() => _AppointmentScreenState();
}

class _AppointmentScreenState extends State<AppointmentScreen> {
  static const _departments = <String, String>{
    'YolVeAltyapi': 'Yol ve Altyapı',
    'SuVeKanalizasyon': 'Su ve Kanalizasyon',
    'ElektrikVeAydinlatma': 'Elektrik ve Aydınlatma',
    'ParkVeBahceler': 'Park ve Bahçeler',
    'CevreKorumaVeTemizlik': 'Çevre ve Temizlik',
    'UlasimHizmetleri': 'Ulaşım Hizmetleri',
    'Zabita': 'Zabıta',
  };

  final _api = ApiService.instance;
  final _formKey = GlobalKey<FormState>();
  final _subjectController = TextEditingController();
  late DateTime _selectedDate;
  String? _selectedDepartment;
  String? _selectedTime;
  List<String> _availableTimes = const [];
  List<RandevuModel> _appointments = const [];
  bool _isLoadingAppointments = true;
  bool _isLoadingTimes = false;
  bool _isSubmitting = false;
  String? _error;

  @override
  void initState() {
    super.initState();
    final tomorrow = DateTime.now().add(const Duration(days: 1));
    _selectedDate = DateTime(tomorrow.year, tomorrow.month, tomorrow.day);
    _loadAppointments();
  }

  @override
  void dispose() {
    _subjectController.dispose();
    super.dispose();
  }

  Future<void> _loadAppointments() async {
    setState(() {
      _isLoadingAppointments = true;
      _error = null;
    });
    try {
      final appointments = await _api.getMyAppointments(
        accessToken: widget.session.accessToken,
      );
      if (mounted) setState(() => _appointments = appointments);
    } on ApiException catch (error) {
      if (mounted) setState(() => _error = error.message);
    } catch (_) {
      if (mounted) setState(() => _error = AppStrings.genericConnectionError);
    } finally {
      if (mounted) setState(() => _isLoadingAppointments = false);
    }
  }

  Future<void> _loadAvailableTimes() async {
    final department = _selectedDepartment;
    if (department == null) return;

    setState(() {
      _isLoadingTimes = true;
      _selectedTime = null;
      _availableTimes = const [];
    });
    try {
      final times = await _api.getAvailableAppointmentTimes(
        department: department,
        date: _selectedDate,
        accessToken: widget.session.accessToken,
      );
      if (mounted) setState(() => _availableTimes = times);
    } on ApiException catch (error) {
      if (mounted) _showMessage(error.message);
    } catch (_) {
      if (mounted) _showMessage(AppStrings.genericConnectionError);
    } finally {
      if (mounted) setState(() => _isLoadingTimes = false);
    }
  }

  Future<void> _selectDate() async {
    final selected = await showDatePicker(
      context: context,
      initialDate: _selectedDate,
      firstDate: DateTime.now(),
      lastDate: DateTime.now().add(const Duration(days: 30)),
      helpText: 'Randevu gününü seç',
      cancelText: 'Vazgeç',
      confirmText: 'Seç',
    );
    if (selected == null) return;
    setState(() => _selectedDate = selected);
    await _loadAvailableTimes();
  }

  Future<void> _createAppointment() async {
    if (!_formKey.currentState!.validate()) return;
    if (_selectedDepartment == null) {
      _showMessage('Lütfen randevu almak istediğiniz birimi seçiniz.');
      return;
    }
    if (_selectedTime == null) {
      _showMessage('Lütfen uygun saatlerden birini seçiniz.');
      return;
    }

    final timeParts = _selectedTime!.split(':');
    final dateTime = DateTime(
      _selectedDate.year,
      _selectedDate.month,
      _selectedDate.day,
      int.parse(timeParts[0]),
      int.parse(timeParts[1]),
    );

    setState(() => _isSubmitting = true);
    try {
      final appointment = await _api.createAppointment(
        department: _selectedDepartment!,
        dateTime: dateTime,
        subject: _subjectController.text.trim(),
        accessToken: widget.session.accessToken,
      );
      if (!mounted) return;
      setState(() {
        _appointments = [appointment, ..._appointments];
        _selectedTime = null;
        _subjectController.clear();
      });
      await _loadAvailableTimes();
      if (mounted) {
        _showMessage(
          'Randevunuz oluşturuldu. Hatırlatma için burayı kontrol edebilirsiniz.',
        );
      }
    } on ApiException catch (error) {
      _showMessage(error.message);
    } catch (_) {
      _showMessage('Randevu oluşturulamadı. Lütfen tekrar deneyiniz.');
    } finally {
      if (mounted) setState(() => _isSubmitting = false);
    }
  }

  Future<void> _cancelAppointment(RandevuModel appointment) async {
    final shouldCancel = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Randevu iptal edilsin mi?'),
        content: Text(
          '${appointment.departmentLabel} randevun iptal edilecek.',
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context, false),
            child: const Text('Vazgeç'),
          ),
          TextButton(
            onPressed: () => Navigator.pop(context, true),
            child: const Text('İptal et'),
          ),
        ],
      ),
    );
    if (shouldCancel != true) return;

    try {
      await _api.cancelAppointment(
        appointmentId: appointment.id,
        accessToken: widget.session.accessToken,
      );
      await _loadAppointments();
      if (mounted) _showMessage('Randevunuz iptal edildi.');
    } on ApiException catch (error) {
      _showMessage(error.message);
    } catch (_) {
      _showMessage('Randevu iptal edilemedi.');
    }
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
    return Scaffold(
      appBar: AppBar(title: const Text('Belediye randevusu')),
      body: RefreshIndicator(
        onRefresh: _loadAppointments,
        child: ListView(
          padding: const EdgeInsets.fromLTRB(20, 16, 20, 30),
          children: [
            const Text(
              'Randevu al',
              style: TextStyle(fontSize: 25, fontWeight: FontWeight.w800),
            ),
            const SizedBox(height: 5),
            const Text(
              'İlgili birimi, günü ve sana uygun saati seç.',
              style: TextStyle(color: Colors.blueGrey),
            ),
            const SizedBox(height: 18),
            Container(
              padding: const EdgeInsets.all(18),
              decoration: BoxDecoration(
                color: Colors.white,
                borderRadius: BorderRadius.circular(22),
              ),
              child: Form(
                key: _formKey,
                autovalidateMode: AutovalidateMode.onUserInteraction,
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    const FormSectionHeader(
                      number: '1',
                      title: 'Belediye birimi',
                      description: 'İşleminle ilgilenecek birimi seç.',
                    ),
                    const SizedBox(height: 12),
                    DropdownButtonFormField<String>(
                      initialValue: _selectedDepartment,
                      isExpanded: true,
                      decoration: const InputDecoration(
                        prefixIcon: Icon(Icons.account_tree_outlined),
                      ),
                      hint: const Text('Birim seçiniz'),
                      items: _departments.entries
                          .map(
                            (item) => DropdownMenuItem(
                              value: item.key,
                              child: Text(item.value),
                            ),
                          )
                          .toList(),
                      onChanged: (value) async {
                        setState(() => _selectedDepartment = value);
                        await _loadAvailableTimes();
                      },
                    ),
                    const SizedBox(height: 22),
                    const FormSectionHeader(
                      number: '2',
                      title: 'Gün ve saat',
                      description:
                          'Uygun saatler seçtiğin güne göre güncellenir.',
                    ),
                    const SizedBox(height: 12),
                    OutlinedButton.icon(
                      onPressed: _selectDate,
                      icon: const Icon(Icons.calendar_today_outlined),
                      label: Text(_dateLabel(_selectedDate)),
                    ),
                    const SizedBox(height: 13),
                    if (_selectedDepartment == null)
                      const Text(
                        'Önce birim seçiniz.',
                        style: TextStyle(color: Colors.blueGrey),
                      )
                    else if (_isLoadingTimes)
                      const Padding(
                        padding: EdgeInsets.symmetric(vertical: 12),
                        child: Center(child: CircularProgressIndicator()),
                      )
                    else if (_availableTimes.isEmpty)
                      const Text(
                        'Bu gün için uygun saat bulunamadı. Başka bir gün seçiniz.',
                        style: TextStyle(color: Colors.blueGrey),
                      )
                    else
                      Wrap(
                        spacing: 8,
                        runSpacing: 8,
                        children: _availableTimes
                            .map(
                              (time) => ChoiceChip(
                                label: Text(time),
                                selected: _selectedTime == time,
                                onSelected: (_) =>
                                    setState(() => _selectedTime = time),
                                selectedColor: const Color(0xFFD9EAFB),
                                side: BorderSide.none,
                              ),
                            )
                            .toList(),
                      ),
                    const SizedBox(height: 22),
                    const FormSectionHeader(
                      number: '3',
                      title: 'Randevu konusu',
                      description:
                          'Kısa bir açıklama, doğru hazırlık yapılmasına yardımcı olur.',
                    ),
                    const SizedBox(height: 12),
                    TextFormField(
                      controller: _subjectController,
                      minLines: 2,
                      maxLines: 3,
                      maxLength: 300,
                      decoration: const InputDecoration(
                        hintText:
                            'Örn. Su aboneliği hakkında bilgi almak istiyorum',
                        alignLabelWithHint: true,
                      ),
                      validator: (value) =>
                          value == null || value.trim().length < 4
                          ? 'Randevu konusunu en az 4 karakterle yazınız.'
                          : null,
                    ),
                    const SizedBox(height: 10),
                    PrimaryButton(
                      label: _isSubmitting
                          ? 'Oluşturuluyor...'
                          : 'Randevu oluştur',
                      icon: Icons.calendar_month_rounded,
                      isLoading: _isSubmitting,
                      onPressed: _createAppointment,
                    ),
                  ],
                ),
              ),
            ),
            const SizedBox(height: 28),
            const Text(
              'Randevularım',
              style: TextStyle(fontSize: 19, fontWeight: FontWeight.w800),
            ),
            const SizedBox(height: 12),
            if (_isLoadingAppointments)
              const Padding(
                padding: EdgeInsets.all(20),
                child: Center(child: CircularProgressIndicator()),
              )
            else if (_error != null)
              ErrorState(message: _error!, onRetry: _loadAppointments)
            else if (_appointments.isEmpty)
              const _EmptyAppointmentState()
            else
              ..._appointments.map(
                (appointment) => Padding(
                  padding: const EdgeInsets.only(bottom: 10),
                  child: _AppointmentCard(
                    appointment: appointment,
                    onCancel: appointment.isPlanned
                        ? () => _cancelAppointment(appointment)
                        : null,
                  ),
                ),
              ),
          ],
        ),
      ),
    );
  }
}

class _AppointmentCard extends StatelessWidget {
  const _AppointmentCard({required this.appointment, this.onCancel});

  final RandevuModel appointment;
  final VoidCallback? onCancel;

  @override
  Widget build(BuildContext context) {
    final isCancelled = appointment.status == 'IptalEdildi';
    final color = isCancelled ? Colors.redAccent : AppColors.turquoise;
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(20),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Container(
                width: 42,
                height: 42,
                decoration: BoxDecoration(
                  color: color.withValues(alpha: 0.12),
                  borderRadius: BorderRadius.circular(14),
                ),
                child: Icon(Icons.calendar_month_outlined, color: color),
              ),
              const SizedBox(width: 11),
              Expanded(
                child: Text(
                  appointment.departmentLabel,
                  style: const TextStyle(fontWeight: FontWeight.w800),
                ),
              ),
              _AppointmentStatus(status: appointment.status),
            ],
          ),
          const SizedBox(height: 13),
          Text(appointment.subject, style: const TextStyle(height: 1.35)),
          const SizedBox(height: 9),
          Row(
            children: [
              const Icon(
                Icons.schedule_rounded,
                size: 17,
                color: Colors.blueGrey,
              ),
              const SizedBox(width: 6),
              Text(
                appointment.dateTimeLabel,
                style: const TextStyle(
                  color: Colors.blueGrey,
                  fontWeight: FontWeight.w700,
                ),
              ),
              const Spacer(),
              if (onCancel != null)
                TextButton(onPressed: onCancel, child: const Text('İptal et')),
            ],
          ),
        ],
      ),
    );
  }
}

class _AppointmentStatus extends StatelessWidget {
  const _AppointmentStatus({required this.status});

  final String status;

  @override
  Widget build(BuildContext context) {
    final color = status == 'IptalEdildi'
        ? Colors.redAccent
        : AppColors.turquoise;
    final label = status == 'IptalEdildi'
        ? 'İptal'
        : status == 'Tamamlandi'
        ? 'Tamamlandı'
        : 'Planlandı';
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
      decoration: BoxDecoration(
        color: color.withValues(alpha: 0.12),
        borderRadius: BorderRadius.circular(9),
      ),
      child: Text(
        label,
        style: TextStyle(
          fontSize: 11,
          color: color,
          fontWeight: FontWeight.w800,
        ),
      ),
    );
  }
}

class _EmptyAppointmentState extends StatelessWidget {
  const _EmptyAppointmentState();

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(20),
      ),
      child: const Row(
        children: [
          Icon(Icons.event_available_outlined, color: AppColors.blue),
          SizedBox(width: 12),
          Expanded(
            child: Text(
              'Yaklaşan bir randevun bulunmuyor.',
              style: TextStyle(color: Colors.blueGrey),
            ),
          ),
        ],
      ),
    );
  }
}

String _dateLabel(DateTime date) {
  const months = [
    'Ocak',
    'Şubat',
    'Mart',
    'Nisan',
    'Mayıs',
    'Haziran',
    'Temmuz',
    'Ağustos',
    'Eylül',
    'Ekim',
    'Kasım',
    'Aralık',
  ];
  return '${date.day} ${months[date.month - 1]} ${date.year}';
}
