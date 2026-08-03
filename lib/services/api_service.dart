import 'dart:convert';

import 'package:http/http.dart' as http;
import 'package:http_parser/http_parser.dart';
import 'package:image_picker/image_picker.dart';

import '../constants/app_constants.dart';
import '../models/api_exception.dart';
import '../models/ariza_model.dart';
import '../models/duyuru_model.dart';
import '../models/randevu_model.dart';
import '../models/user_session.dart';

/// Uygulamanın HTTP erişimi için tek giriş noktasıdır.
/// Ekranlar endpoint veya HTTP paketi bilgisi içermez.
class ApiService {
  ApiService._();

  static final ApiService instance = ApiService._();

  Future<UserSession> loginCitizen({
    required String usernameOrEmail,
    required String password,
  }) {
    return _login(
      ApiConfig.citizenLogin,
      usernameOrEmail: usernameOrEmail,
      password: password,
    );
  }

  Future<UserSession> loginPersonnel({
    required String corporateEmail,
    required String password,
  }) {
    return _login(
      ApiConfig.personnelLogin,
      usernameOrEmail: corporateEmail,
      password: password,
    );
  }

  Future<UserSession> _login(
    String path, {
    required String usernameOrEmail,
    required String password,
  }) async {
    final response = await http.post(
      ApiConfig.endpoint(path),
      headers: const {'Content-Type': 'application/json'},
      body: jsonEncode({
        'kullaniciAdiVeyaEmail': usernameOrEmail,
        'parola': password,
      }),
    );

    final body = _jsonMap(response);
    if (response.statusCode != 200 || body['basarili'] != true) {
      throw ApiException(body['mesaj']?.toString() ?? 'Giriş yapılamadı.');
    }
    return _sessionFromResponse(body);
  }

  Future<UserSession> registerCitizen({
    required String fullName,
    required String email,
    required String phoneNumber,
    required String nationalId,
    required String username,
    required String password,
  }) async {
    final response = await http.post(
      ApiConfig.endpoint(ApiConfig.citizenRegister),
      headers: const {'Content-Type': 'application/json'},
      body: jsonEncode({
        'adSoyad': fullName,
        'email': email,
        'telefonNumarasi': phoneNumber,
        'tcKimlikNo': nationalId,
        'kullaniciAdi': username,
        'parola': password,
      }),
    );

    final body = _jsonMap(response);
    if (response.statusCode != 201 || body['basarili'] != true) {
      throw ApiException(_errorMessage(response));
    }
    return _sessionFromResponse(body);
  }

  Future<ArizaModel> createComplaint({
    required Map<String, dynamic> request,
    required String accessToken,
  }) async {
    final response = await http.post(
      ApiConfig.endpoint(ApiConfig.complaints),
      headers: _jsonHeaders(accessToken),
      body: jsonEncode(request),
    );
    if (response.statusCode != 201) {
      throw ApiException(_errorMessage(response));
    }
    return ArizaModel.fromJson(_jsonMap(response));
  }

  Future<List<ArizaModel>> getComplaints({required String accessToken}) async {
    final response = await http.get(
      ApiConfig.endpoint(ApiConfig.complaints),
      headers: _authHeaders(accessToken),
    );
    if (response.statusCode != 200) {
      throw ApiException(_errorMessage(response));
    }

    final body = jsonDecode(response.body) as List<dynamic>;
    return body
        .map((item) => ArizaModel.fromJson(Map<String, dynamic>.from(item)))
        .toList();
  }

  /// Vatandaşın yalnızca kendi hesabıyla oluşturduğu arıza kayıtlarını getirir.
  Future<List<ArizaModel>> getMyComplaints({
    required String accessToken,
  }) async {
    final response = await http.get(
      ApiConfig.endpoint(ApiConfig.myComplaints),
      headers: _authHeaders(accessToken),
    );
    if (response.statusCode != 200) {
      throw ApiException(_errorMessage(response));
    }
    return _jsonList(
      response,
    ).map((item) => ArizaModel.fromJson(item)).toList();
  }

  /// Konum bilgisi verilirse yalnızca kullanıcının çevresini etkileyen aktif
  /// duyurular döner; konum izni yoksa şehir geneli akış gösterilir.
  Future<List<DuyuruModel>> getAnnouncements({
    required String accessToken,
    double? latitude,
    double? longitude,
  }) async {
    final query = <String, String>{};
    if (latitude != null && longitude != null) {
      query['enlem'] = latitude.toString();
      query['boylam'] = longitude.toString();
    }
    final response = await http.get(
      ApiConfig.endpoint(ApiConfig.announcements, query: query),
      headers: _authHeaders(accessToken),
    );
    if (response.statusCode != 200) {
      throw ApiException(_errorMessage(response));
    }
    return _jsonList(
      response,
    ).map((item) => DuyuruModel.fromJson(item)).toList();
  }

  Future<List<RandevuModel>> getMyAppointments({
    required String accessToken,
  }) async {
    final response = await http.get(
      ApiConfig.endpoint(ApiConfig.myAppointments),
      headers: _authHeaders(accessToken),
    );
    if (response.statusCode != 200) {
      throw ApiException(_errorMessage(response));
    }
    return _jsonList(
      response,
    ).map((item) => RandevuModel.fromJson(item)).toList();
  }

  Future<List<String>> getAvailableAppointmentTimes({
    required String department,
    required DateTime date,
    required String accessToken,
  }) async {
    final dateLabel =
        '${date.year.toString().padLeft(4, '0')}-${date.month.toString().padLeft(2, '0')}-${date.day.toString().padLeft(2, '0')}';
    final response = await http.get(
      ApiConfig.endpoint(
        ApiConfig.availableAppointmentTimes,
        query: {'birim': department, 'tarih': dateLabel},
      ),
      headers: _authHeaders(accessToken),
    );
    if (response.statusCode != 200) {
      throw ApiException(_errorMessage(response));
    }
    try {
      return (jsonDecode(response.body) as List<dynamic>)
          .map((item) => item.toString())
          .toList();
    } catch (_) {
      throw const ApiException('Uygun randevu saatleri okunamadı.');
    }
  }

  Future<RandevuModel> createAppointment({
    required String department,
    required DateTime dateTime,
    required String subject,
    required String accessToken,
  }) async {
    final response = await http.post(
      ApiConfig.endpoint(ApiConfig.appointments),
      headers: _jsonHeaders(accessToken),
      body: jsonEncode({
        'birim': department,
        'tarihSaat': dateTime.toUtc().toIso8601String(),
        'konu': subject,
      }),
    );
    if (response.statusCode != 201) {
      throw ApiException(_errorMessage(response));
    }
    return RandevuModel.fromJson(_jsonMap(response));
  }

  Future<void> cancelAppointment({
    required int appointmentId,
    required String accessToken,
  }) async {
    final response = await http.put(
      ApiConfig.endpoint('${ApiConfig.appointments}/$appointmentId/iptal'),
      headers: _authHeaders(accessToken),
    );
    if (response.statusCode != 204) {
      throw ApiException(_errorMessage(response));
    }
  }

  Future<void> updateComplaintStatus({
    required int complaintId,
    required String status,
    required String accessToken,
  }) async {
    final response = await http.put(
      ApiConfig.endpoint('${ApiConfig.complaints}/$complaintId/durum'),
      headers: _jsonHeaders(accessToken),
      body: jsonEncode({'durum': status}),
    );
    if (response.statusCode != 204) {
      throw ApiException(_errorMessage(response));
    }
  }

  Future<String> uploadComplaintPhoto({
    required XFile photo,
    required String accessToken,
  }) async {
    final extension = photo.name.split('.').last.toLowerCase();
    final mimeTypes = <String, MediaType>{
      'jpg': MediaType('image', 'jpeg'),
      'jpeg': MediaType('image', 'jpeg'),
      'png': MediaType('image', 'png'),
      'webp': MediaType('image', 'webp'),
    };
    final mimeType = mimeTypes[extension];
    if (mimeType == null) {
      throw const ApiException(
        'Fotoğraf JPG, PNG veya WEBP formatında olmalıdır.',
      );
    }

    final request = http.MultipartRequest(
      'POST',
      ApiConfig.endpoint(ApiConfig.complaintPhoto),
    )..headers.addAll(_authHeaders(accessToken));
    request.files.add(
      http.MultipartFile.fromBytes(
        'fotograf',
        await photo.readAsBytes(),
        filename: photo.name,
        contentType: mimeType,
      ),
    );

    final streamedResponse = await request.send();
    final response = await http.Response.fromStream(streamedResponse);
    if (response.statusCode != 201) {
      throw ApiException(_errorMessage(response));
    }

    final photoUrl = _jsonMap(response)['fotografUrl']?.toString() ?? '';
    if (photoUrl.isEmpty) {
      throw const ApiException('Fotoğraf sunucuda saklanamadı.');
    }
    return photoUrl;
  }

  UserSession _sessionFromResponse(Map<String, dynamic> body) {
    final token = body['token']?.toString() ?? '';
    final user = body['kullanici'];
    if (token.isEmpty || user is! Map) {
      throw const ApiException(
        'Oturum bilgisi alınamadı. Lütfen tekrar giriş yapınız.',
      );
    }
    return UserSession.fromJson(
      Map<String, dynamic>.from(user),
      accessToken: token,
    );
  }

  Map<String, String> _authHeaders(String accessToken) {
    return {'Authorization': 'Bearer $accessToken'};
  }

  Map<String, String> _jsonHeaders(String accessToken) {
    return {'Content-Type': 'application/json', ..._authHeaders(accessToken)};
  }

  Map<String, dynamic> _jsonMap(http.Response response) {
    try {
      return Map<String, dynamic>.from(jsonDecode(response.body) as Map);
    } catch (_) {
      return {};
    }
  }

  List<Map<String, dynamic>> _jsonList(http.Response response) {
    try {
      return (jsonDecode(response.body) as List<dynamic>)
          .whereType<Map>()
          .map((item) => Map<String, dynamic>.from(item))
          .toList();
    } catch (_) {
      throw const ApiException('Sunucudan gelen liste okunamadı.');
    }
  }

  String _errorMessage(http.Response response) {
    if (response.statusCode == 401) {
      return 'Oturumunuz sona erdi. Lütfen yeniden giriş yapınız.';
    }
    if (response.statusCode == 403) {
      return 'Bu işlem için yetkiniz bulunmuyor.';
    }

    final body = _jsonMap(response);
    final fieldErrors = body['errors'];
    if (fieldErrors is Map) {
      for (final messages in fieldErrors.values) {
        if (messages is List && messages.isNotEmpty) {
          return messages.first.toString();
        }
      }
    }
    return body['mesaj']?.toString() ??
        body['title']?.toString() ??
        'İşlem gerçekleştirilemedi (${response.statusCode}).';
  }
}
