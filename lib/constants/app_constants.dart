import 'dart:io';

import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';

/// Uygulama boyunca kullanılan renkleri merkezi olarak yönetir.
abstract final class AppColors {
  static const navy = Color(0xFF0B2748);
  static const blue = Color(0xFF1667B7);
  static const orange = Color(0xFFFF8B3D);
  static const turquoise = Color(0xFF16A6A1);
  static const background = Color(0xFFF4F7FB);
  static const text = Color(0xFF14213D);
}

/// Ortam bağımsız API adreslerini ve uç noktaları tek noktada tutar.
///
/// Gerçek cihaz veya yayımlanmış ortam için örnek kullanım:
/// `flutter run --dart-define=API_BASE_URL=http://192.168.1.25:5000`
abstract final class ApiConfig {
  static const _configuredBaseUrl = String.fromEnvironment('API_BASE_URL');

  static String get baseUrl {
    if (_configuredBaseUrl.isNotEmpty) {
      return _configuredBaseUrl.replaceFirst(RegExp(r'/$'), '');
    }
    if (kIsWeb) return 'http://localhost:5000';
    if (Platform.isAndroid) return 'http://10.0.2.2:5000';
    return 'http://localhost:5000';
  }

  static Uri endpoint(String path, {Map<String, String>? query}) {
    return Uri.parse('$baseUrl$path').replace(queryParameters: query);
  }

  static String fileUrl(String path) {
    if (path.trim().isEmpty) return '';
    final uri = Uri.tryParse(path);
    if (uri != null && uri.hasScheme) return path;
    return '$baseUrl${path.startsWith('/') ? path : '/$path'}';
  }

  static const citizenLogin = '/api/Auth/vatandas-giris';
  static const personnelLogin = '/api/Auth/personel-giris';
  static const citizenRegister = '/api/Auth/vatandas-kayit';
  static const complaints = '/api/Arizalar';
  static const complaintPhoto = '/api/Dosyalar/ariza-fotografi';
  static const openStreetMapTiles =
      'https://tile.openstreetmap.org/{z}/{x}/{y}.png';
}

abstract final class AppRoutes {
  static const roleSelection = '/';
}

abstract final class AppStrings {
  static const appTitle = 'Adana Akıllı Şehir';
  static const genericConnectionError =
      'Sunucuya bağlanılamadı. İnternet ve API bağlantınızı kontrol ediniz.';
}
