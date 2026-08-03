import 'package:flutter/material.dart';
import 'package:geolocator/geolocator.dart';

import '../constants/app_constants.dart';
import '../models/api_exception.dart';
import '../models/duyuru_model.dart';
import '../models/user_session.dart';
import '../services/api_service.dart';
import '../widgets/app_widgets.dart';

/// Konum izni verildiğinde yakın çevreyi, izin verilmediğinde şehir genelini gösterir.
class AnnouncementsScreen extends StatefulWidget {
  const AnnouncementsScreen({super.key, required this.session});

  final UserSession session;

  @override
  State<AnnouncementsScreen> createState() => _AnnouncementsScreenState();
}

class _AnnouncementsScreenState extends State<AnnouncementsScreen> {
  final _api = ApiService.instance;
  bool _isLoading = true;
  bool _isUsingLocation = false;
  String? _error;
  List<DuyuruModel> _announcements = const [];

  @override
  void initState() {
    super.initState();
    _loadAnnouncements();
  }

  Future<void> _loadAnnouncements({double? latitude, double? longitude}) async {
    setState(() {
      _isLoading = true;
      _error = null;
    });
    try {
      final announcements = await _api.getAnnouncements(
        accessToken: widget.session.accessToken,
        latitude: latitude,
        longitude: longitude,
      );
      if (mounted) {
        setState(() {
          _announcements = announcements;
          _isUsingLocation = latitude != null && longitude != null;
        });
      }
    } on ApiException catch (error) {
      if (mounted) setState(() => _error = error.message);
    } catch (_) {
      if (mounted) setState(() => _error = AppStrings.genericConnectionError);
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  Future<void> _useMyLocation() async {
    if (!await Geolocator.isLocationServiceEnabled()) {
      _showMessage('Konum hizmetini açtıktan sonra tekrar deneyiniz.');
      return;
    }
    var permission = await Geolocator.checkPermission();
    if (permission == LocationPermission.denied) {
      permission = await Geolocator.requestPermission();
    }
    if (permission == LocationPermission.denied ||
        permission == LocationPermission.deniedForever) {
      _showMessage('Yakınındaki duyurular için konum izni gereklidir.');
      return;
    }

    try {
      final position = await Geolocator.getCurrentPosition(
        locationSettings: const LocationSettings(
          accuracy: LocationAccuracy.medium,
        ),
      );
      await _loadAnnouncements(
        latitude: position.latitude,
        longitude: position.longitude,
      );
    } catch (_) {
      _showMessage('Konum alınamadı. Şehir geneli duyurular gösteriliyor.');
    }
  }

  void _showMessage(String message) {
    ScaffoldMessenger.of(
      context,
    ).showSnackBar(SnackBar(content: Text(message)));
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Mahalle duyuruları')),
      body: _isLoading
          ? const Center(child: CircularProgressIndicator())
          : _error != null
          ? ErrorState(message: _error!, onRetry: _loadAnnouncements)
          : RefreshIndicator(
              onRefresh: () => _loadAnnouncements(),
              child: ListView.separated(
                padding: const EdgeInsets.fromLTRB(20, 16, 20, 28),
                itemCount: _announcements.length + 1,
                separatorBuilder: (_, _) => const SizedBox(height: 12),
                itemBuilder: (context, index) {
                  if (index == 0) {
                    return _LocationNotice(
                      isUsingLocation: _isUsingLocation,
                      onUseLocation: _useMyLocation,
                      onShowCity: () => _loadAnnouncements(),
                    );
                  }
                  return _AnnouncementCard(
                    announcement: _announcements[index - 1],
                  );
                },
              ),
            ),
    );
  }
}

class _LocationNotice extends StatelessWidget {
  const _LocationNotice({
    required this.isUsingLocation,
    required this.onUseLocation,
    required this.onShowCity,
  });

  final bool isUsingLocation;
  final VoidCallback onUseLocation;
  final VoidCallback onShowCity;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(17),
      decoration: BoxDecoration(
        color: const Color(0xFFE8F3FF),
        borderRadius: BorderRadius.circular(22),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              const Icon(Icons.near_me_outlined, color: AppColors.blue),
              const SizedBox(width: 9),
              Expanded(
                child: Text(
                  isUsingLocation
                      ? 'Yakınındaki duyurular gösteriliyor'
                      : 'Adana geneli duyurular gösteriliyor',
                  style: const TextStyle(fontWeight: FontWeight.w800),
                ),
              ),
            ],
          ),
          const SizedBox(height: 7),
          const Text(
            'Konumunu yalnızca yakınındaki hizmet duyurularını filtrelemek için kullanırız.',
            style: TextStyle(color: Colors.blueGrey, height: 1.35),
          ),
          const SizedBox(height: 10),
          TextButton.icon(
            onPressed: isUsingLocation ? onShowCity : onUseLocation,
            icon: Icon(
              isUsingLocation
                  ? Icons.public_rounded
                  : Icons.my_location_rounded,
            ),
            label: Text(
              isUsingLocation ? 'Tüm Adana’yı göster' : 'Konumumu kullan',
            ),
          ),
        ],
      ),
    );
  }
}

class _AnnouncementCard extends StatelessWidget {
  const _AnnouncementCard({required this.announcement});

  final DuyuruModel announcement;

  @override
  Widget build(BuildContext context) {
    final color = announcement.isUrgent
        ? Colors.redAccent
        : announcement.isImportant
        ? AppColors.orange
        : AppColors.blue;
    return Container(
      padding: const EdgeInsets.all(17),
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
                  color: color.withValues(alpha: 0.13),
                  borderRadius: BorderRadius.circular(14),
                ),
                child: Icon(_categoryIcon(announcement.category), color: color),
              ),
              const SizedBox(width: 11),
              Expanded(
                child: Text(
                  announcement.categoryLabel,
                  style: const TextStyle(fontWeight: FontWeight.w700),
                ),
              ),
              Text(
                announcement.publishedAtLabel,
                style: const TextStyle(fontSize: 12, color: Colors.blueGrey),
              ),
            ],
          ),
          const SizedBox(height: 14),
          Text(
            announcement.title,
            style: const TextStyle(fontSize: 17, fontWeight: FontWeight.w800),
          ),
          const SizedBox(height: 7),
          Text(
            announcement.content,
            style: const TextStyle(height: 1.4, color: Colors.blueGrey),
          ),
          if (announcement.priority != 'Bilgi') ...[
            const SizedBox(height: 12),
            Container(
              padding: const EdgeInsets.symmetric(horizontal: 9, vertical: 5),
              decoration: BoxDecoration(
                color: color.withValues(alpha: 0.12),
                borderRadius: BorderRadius.circular(10),
              ),
              child: Text(
                announcement.priorityLabel,
                style: TextStyle(
                  color: color,
                  fontSize: 12,
                  fontWeight: FontWeight.w800,
                ),
              ),
            ),
          ],
        ],
      ),
    );
  }
}

IconData _categoryIcon(String category) {
  switch (category) {
    case 'SuKesintisi':
      return Icons.water_drop_outlined;
    case 'YolCalismasi':
      return Icons.construction_outlined;
    case 'AcilDurum':
      return Icons.warning_amber_rounded;
    case 'Etkinlik':
      return Icons.celebration_outlined;
    default:
      return Icons.campaign_outlined;
  }
}
