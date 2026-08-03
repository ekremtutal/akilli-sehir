/// API'den dönen arıza kaydının mobil uygulamadaki karşılığıdır.
class ArizaModel {
  const ArizaModel({
    required this.id,
    required this.title,
    required this.description,
    required this.latitude,
    required this.longitude,
    required this.photoUrl,
    required this.status,
    required this.urgency,
    required this.type,
    required this.assignedDepartment,
    required this.createdAt,
    this.citizenPreferredDepartment,
    this.aiConfidenceScore,
    this.aiReason,
    this.assignedPersonnelId,
  });

  final int id;
  final String title;
  final String description;
  final double latitude;
  final double longitude;
  final String photoUrl;
  final String status;
  final String urgency;
  final String type;
  final String assignedDepartment;
  final DateTime createdAt;
  final String? citizenPreferredDepartment;
  final double? aiConfidenceScore;
  final String? aiReason;
  final int? assignedPersonnelId;

  bool get isPending => status == 'Beklemede';
  bool get isInProgress => status == 'Onarimda';

  String get createdAtLabel {
    final day = createdAt.day.toString().padLeft(2, '0');
    final month = createdAt.month.toString().padLeft(2, '0');
    final hour = createdAt.hour.toString().padLeft(2, '0');
    final minute = createdAt.minute.toString().padLeft(2, '0');
    return '$day.$month.${createdAt.year} · $hour:$minute';
  }

  String get departmentLabel => departmentName(assignedDepartment);

  static String departmentName(String code) {
    const departments = {
      'YolVeAltyapi': 'Yol ve Altyapı',
      'SuVeKanalizasyon': 'Su ve Kanalizasyon',
      'ElektrikVeAydinlatma': 'Elektrik ve Aydınlatma',
      'ParkVeBahceler': 'Park ve Bahçeler',
      'CevreKorumaVeTemizlik': 'Çevre ve Temizlik',
      'UlasimHizmetleri': 'Ulaşım Hizmetleri',
      'Zabita': 'Zabıta',
      'Diger': 'Diğer Birimler',
    };
    return departments[code] ?? code;
  }

  factory ArizaModel.fromJson(Map<String, dynamic> json) {
    return ArizaModel(
      id: (json['id'] as num?)?.toInt() ?? 0,
      title: json['baslik']?.toString() ?? 'Başlıksız arıza',
      description: json['aciklama']?.toString() ?? '',
      latitude: (json['enlem'] as num?)?.toDouble() ?? 0,
      longitude: (json['boylam'] as num?)?.toDouble() ?? 0,
      photoUrl: json['fotografUrl']?.toString() ?? '',
      status: json['durum']?.toString() ?? 'Beklemede',
      urgency: json['aciliyet']?.toString() ?? 'Orta',
      type: json['arizaTuru']?.toString() ?? 'Diger',
      assignedDepartment: json['yonlendirilenBirim']?.toString() ?? 'Diger',
      createdAt:
          DateTime.tryParse(json['kayitTarihi']?.toString() ?? '') ??
          DateTime.now(),
      citizenPreferredDepartment: json['vatandasSecilenBirim']?.toString(),
      aiConfidenceScore: (json['yapayZekaGuvenSkoru'] as num?)?.toDouble(),
      aiReason: json['yapayZekaGerekcesi']?.toString(),
      assignedPersonnelId: (json['atananPersonelId'] as num?)?.toInt(),
    );
  }
}
