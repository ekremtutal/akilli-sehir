/// Belediye API'sinden gelen, konuma göre filtrelenebilen duyuru kaydıdır.
class DuyuruModel {
  const DuyuruModel({
    required this.id,
    required this.title,
    required this.content,
    required this.category,
    required this.priority,
    required this.publishedAt,
    this.expiresAt,
  });

  final int id;
  final String title;
  final String content;
  final String category;
  final String priority;
  final DateTime publishedAt;
  final DateTime? expiresAt;

  bool get isUrgent => priority == 'Acil';
  bool get isImportant => priority == 'Onemli';

  String get priorityLabel {
    switch (priority) {
      case 'Acil':
        return 'Acil';
      case 'Onemli':
        return 'Önemli';
      default:
        return 'Bilgilendirme';
    }
  }

  String get categoryLabel {
    switch (category) {
      case 'SuKesintisi':
        return 'Su ve kanalizasyon';
      case 'YolCalismasi':
        return 'Yol çalışması';
      case 'Etkinlik':
        return 'Kültür ve etkinlik';
      case 'AcilDurum':
        return 'Acil durum';
      default:
        return 'Belediye duyurusu';
    }
  }

  String get publishedAtLabel {
    final now = DateTime.now();
    final local = publishedAt.toLocal();
    final difference = now.difference(local);
    if (difference.inDays == 0) return 'Bugün';
    if (difference.inDays == 1) return 'Dün';
    final day = local.day.toString().padLeft(2, '0');
    final month = local.month.toString().padLeft(2, '0');
    return '$day.$month.${local.year}';
  }

  factory DuyuruModel.fromJson(Map<String, dynamic> json) {
    return DuyuruModel(
      id: (json['id'] as num?)?.toInt() ?? 0,
      title: json['baslik']?.toString() ?? 'Belediye duyurusu',
      content: json['icerik']?.toString() ?? '',
      category: json['kategori']?.toString() ?? 'Diger',
      priority: json['oncelik']?.toString() ?? 'Bilgi',
      publishedAt:
          DateTime.tryParse(json['yayinBaslangicTarihi']?.toString() ?? '') ??
          DateTime.now(),
      expiresAt: DateTime.tryParse(json['yayinBitisTarihi']?.toString() ?? ''),
    );
  }
}
