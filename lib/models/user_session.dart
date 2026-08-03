/// Başarılı girişten sonra uygulama belleğinde tutulan kullanıcı özeti ve JWT.
class UserSession {
  const UserSession({
    required this.id,
    required this.fullName,
    required this.email,
    required this.username,
    required this.role,
    required this.accessToken,
    this.department,
  });

  final int id;
  final String fullName;
  final String email;
  final String username;
  final String role;
  final String accessToken;
  final String? department;

  bool get isCitizen => role == 'Vatandas';
  bool get isPersonnel => role == 'SahaPersoneli';

  String get firstName {
    final value = fullName.trim();
    return value.isEmpty ? 'Kullanıcı' : value.split(RegExp(r'\s+')).first;
  }

  factory UserSession.fromJson(
    Map<String, dynamic> json, {
    required String accessToken,
  }) {
    return UserSession(
      id: (json['id'] as num?)?.toInt() ?? 0,
      fullName: json['adSoyad']?.toString() ?? '',
      email: json['email']?.toString() ?? '',
      username: json['kullaniciAdi']?.toString() ?? '',
      role: json['rol']?.toString() ?? '',
      accessToken: accessToken,
      department: json['calistigiBirim']?.toString(),
    );
  }
}
