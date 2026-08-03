import 'ariza_model.dart';

/// Vatandaşın oluşturduğu belediye randevusunun mobil karşılığıdır.
class RandevuModel {
  const RandevuModel({
    required this.id,
    required this.department,
    required this.dateTime,
    required this.subject,
    required this.status,
  });

  final int id;
  final String department;
  final DateTime dateTime;
  final String subject;
  final String status;

  bool get isPlanned => status == 'Planlandi';

  String get departmentLabel => ArizaModel.departmentName(department);

  String get statusLabel {
    switch (status) {
      case 'IptalEdildi':
        return 'İptal edildi';
      case 'Tamamlandi':
        return 'Tamamlandı';
      default:
        return 'Planlandı';
    }
  }

  String get dateTimeLabel {
    final local = dateTime.toLocal();
    final day = local.day.toString().padLeft(2, '0');
    final month = local.month.toString().padLeft(2, '0');
    final hour = local.hour.toString().padLeft(2, '0');
    final minute = local.minute.toString().padLeft(2, '0');
    return '$day.$month.${local.year} · $hour:$minute';
  }

  factory RandevuModel.fromJson(Map<String, dynamic> json) {
    return RandevuModel(
      id: (json['id'] as num?)?.toInt() ?? 0,
      department: json['birim']?.toString() ?? 'Diger',
      dateTime:
          DateTime.tryParse(json['tarihSaat']?.toString() ?? '') ??
          DateTime.now(),
      subject: json['konu']?.toString() ?? '',
      status: json['durum']?.toString() ?? 'Planlandi',
    );
  }
}
