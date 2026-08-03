/// API'den kullanıcıya gösterilebilecek hata mesajını taşır.
class ApiException implements Exception {
  const ApiException(this.message);

  final String message;

  @override
  String toString() => message;
}
