import 'dart:io';

import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:flutter_map/flutter_map.dart';
import 'package:image_picker/image_picker.dart';
import 'package:latlong2/latlong.dart';

import '../constants/app_constants.dart';
import '../models/api_exception.dart';
import '../models/ariza_model.dart';
import '../models/user_session.dart';
import '../services/api_service.dart';
import '../widgets/app_widgets.dart';

class ComplaintReportScreen extends StatefulWidget {
  const ComplaintReportScreen({super.key, required this.session});

  final UserSession session;

  @override
  State<ComplaintReportScreen> createState() => _ComplaintReportScreenState();
}

class _ComplaintReportScreenState extends State<ComplaintReportScreen> {
  static const _types = [
    _ComplaintType(
      code: 'YolVeKaldirim',
      label: 'Yol',
      icon: Icons.route_rounded,
      color: Color(0xFFFFE3D5),
    ),
    _ComplaintType(
      code: 'SuVeKanalizasyon',
      label: 'Su',
      icon: Icons.water_drop_rounded,
      color: Color(0xFFD7F2F2),
    ),
    _ComplaintType(
      code: 'Aydinlatma',
      label: 'Aydınlatma',
      icon: Icons.lightbulb_rounded,
      color: Color(0xFFFFF0BF),
    ),
    _ComplaintType(
      code: 'ParkVeYesilAlan',
      label: 'Park',
      icon: Icons.park_rounded,
      color: Color(0xFFDDF1D8),
    ),
    _ComplaintType(
      code: 'TemizlikVeAtik',
      label: 'Temizlik',
      icon: Icons.delete_outline_rounded,
      color: Color(0xFFE7E9EE),
    ),
    _ComplaintType(
      code: 'TrafikVeUlasim',
      label: 'Ulaşım',
      icon: Icons.traffic_rounded,
      color: Color(0xFFE1E9FF),
    ),
  ];

  static const _departments = <String, String>{
    'YolVeAltyapi': 'Yol ve Altyapı',
    'SuVeKanalizasyon': 'Su ve Kanalizasyon',
    'ElektrikVeAydinlatma': 'Elektrik ve Aydınlatma',
    'ParkVeBahceler': 'Park ve Bahçeler',
    'CevreKorumaVeTemizlik': 'Çevre ve Temizlik',
    'UlasimHizmetleri': 'Ulaşım Hizmetleri',
    'Zabita': 'Zabıta',
  };

  final _formKey = GlobalKey<FormState>();
  final _titleController = TextEditingController();
  final _descriptionController = TextEditingController();
  final _imagePicker = ImagePicker();
  final _api = ApiService.instance;
  LatLng? _selectedLocation;
  String? _selectedType;
  String? _preferredDepartment;
  String _urgency = 'Orta';
  XFile? _selectedImage;
  bool _isSubmitting = false;

  @override
  void dispose() {
    _titleController.dispose();
    _descriptionController.dispose();
    super.dispose();
  }

  Future<void> _pickImage(ImageSource source) async {
    final image = await _imagePicker.pickImage(
      source: source,
      imageQuality: 75,
    );
    if (image != null && mounted) setState(() => _selectedImage = image);
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;
    if (_selectedLocation == null) {
      _showMessage('Lütfen harita üzerinden arıza konumunu seçiniz.');
      return;
    }
    if (_selectedType == null) {
      _showMessage('Lütfen arıza türünü seçiniz.');
      return;
    }

    setState(() => _isSubmitting = true);
    try {
      final photoUrl = _selectedImage == null
          ? ''
          : await _api.uploadComplaintPhoto(
              photo: _selectedImage!,
              accessToken: widget.session.accessToken,
            );
      final complaint = await _api.createComplaint(
        accessToken: widget.session.accessToken,
        request: {
          'baslik': _titleController.text.trim(),
          'aciklama': _descriptionController.text.trim(),
          'enlem': _selectedLocation!.latitude,
          'boylam': _selectedLocation!.longitude,
          'fotografUrl': photoUrl,
          'arizaTuru': _selectedType,
          'vatandasSecilenBirim': _preferredDepartment,
          'aciliyet': _urgency,
        },
      );

      if (!mounted) return;
      await _showRoutingResult(complaint);
      _clearForm();
    } on ApiException catch (error) {
      _showMessage(error.message);
    } catch (_) {
      _showMessage('İhbar gönderilemedi. Bağlantınızı kontrol ediniz.');
    } finally {
      if (mounted) setState(() => _isSubmitting = false);
    }
  }

  Future<void> _showRoutingResult(ArizaModel complaint) {
    return showModalBottomSheet<void>(
      context: context,
      isScrollControlled: true,
      builder: (context) => SafeArea(
        child: Padding(
          padding: const EdgeInsets.fromLTRB(24, 22, 24, 30),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Container(
                width: 42,
                height: 4,
                decoration: BoxDecoration(
                  color: Colors.grey.shade300,
                  borderRadius: BorderRadius.circular(8),
                ),
              ),
              const SizedBox(height: 22),
              Container(
                width: 68,
                height: 68,
                decoration: const BoxDecoration(
                  color: Color(0xFFDDF4E9),
                  shape: BoxShape.circle,
                ),
                child: const Icon(
                  Icons.check_rounded,
                  color: Color(0xFF168456),
                  size: 38,
                ),
              ),
              const SizedBox(height: 14),
              const Text(
                'İhbarın alındı',
                style: TextStyle(fontSize: 23, fontWeight: FontWeight.w800),
              ),
              const SizedBox(height: 8),
              Text(
                'Akıllı yönlendirme, kaydını ${complaint.departmentLabel} birimine iletti.',
                textAlign: TextAlign.center,
                style: const TextStyle(color: Colors.blueGrey, height: 1.4),
              ),
              const SizedBox(height: 16),
              InfoCard(
                icon: Icons.auto_awesome_rounded,
                message: complaint.aiReason ?? 'Yönlendirme tamamlandı.',
              ),
              const SizedBox(height: 20),
              PrimaryButton(
                label: 'Tamam',
                icon: Icons.done_rounded,
                onPressed: () => Navigator.pop(context),
              ),
            ],
          ),
        ),
      ),
    );
  }

  void _clearForm() {
    setState(() {
      _titleController.clear();
      _descriptionController.clear();
      _selectedLocation = null;
      _selectedType = null;
      _preferredDepartment = null;
      _selectedImage = null;
      _urgency = 'Orta';
    });
  }

  void _showMessage(String message) {
    ScaffoldMessenger.of(
      context,
    ).showSnackBar(SnackBar(content: Text(message)));
  }

  @override
  Widget build(BuildContext context) {
    return CustomScrollView(
      slivers: [
        SliverToBoxAdapter(
          child: Padding(
            padding: const EdgeInsets.fromLTRB(20, 18, 20, 10),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                const Text(
                  'Arıza bildir',
                  style: TextStyle(fontSize: 28, fontWeight: FontWeight.w800),
                ),
                const SizedBox(height: 5),
                const Text(
                  'Konumu seç, sorunu anlat; doğru birime biz ulaştıralım.',
                  style: TextStyle(color: Colors.blueGrey),
                ),
                const SizedBox(height: 20),
                _LocationCard(
                  location: _selectedLocation,
                  onLocationSelected: (location) =>
                      setState(() => _selectedLocation = location),
                ),
                const SizedBox(height: 22),
                const Text(
                  'Sorun hangi alanda?',
                  style: TextStyle(fontSize: 17, fontWeight: FontWeight.w800),
                ),
                const SizedBox(height: 12),
                Wrap(
                  spacing: 10,
                  runSpacing: 10,
                  children: _types.map((type) {
                    final selected = _selectedType == type.code;
                    return ChoiceChip(
                      selected: selected,
                      onSelected: (_) =>
                          setState(() => _selectedType = type.code),
                      avatar: Icon(
                        type.icon,
                        size: 18,
                        color: selected ? Colors.white : AppColors.navy,
                      ),
                      label: Text(type.label),
                      labelStyle: TextStyle(
                        color: selected ? Colors.white : AppColors.text,
                        fontWeight: FontWeight.w700,
                      ),
                      selectedColor: AppColors.navy,
                      backgroundColor: type.color,
                      side: BorderSide.none,
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(14),
                      ),
                    );
                  }).toList(),
                ),
                const SizedBox(height: 24),
                Form(
                  key: _formKey,
                  autovalidateMode: AutovalidateMode.onUserInteraction,
                  child: Column(
                    children: [
                      TextFormField(
                        controller: _titleController,
                        decoration: const InputDecoration(
                          labelText: 'Kısa başlık',
                          hintText: 'Örn. Sokak lambası yanmıyor',
                          prefixIcon: Icon(Icons.title_rounded),
                        ),
                        validator: (value) =>
                            value == null || value.trim().length < 4
                            ? 'Başlık en az 4 karakter olmalıdır.'
                            : null,
                      ),
                      const SizedBox(height: 12),
                      TextFormField(
                        controller: _descriptionController,
                        minLines: 4,
                        maxLines: 6,
                        decoration: const InputDecoration(
                          labelText: 'Sorunu anlat',
                          hintText:
                              'Ekiplerin sorunu daha hızlı anlamasına yardımcı ol.',
                          alignLabelWithHint: true,
                        ),
                        validator: (value) =>
                            value == null || value.trim().length < 10
                            ? 'Açıklama en az 10 karakter olmalıdır.'
                            : null,
                      ),
                      const SizedBox(height: 12),
                      DropdownButtonFormField<String>(
                        key: ValueKey(_preferredDepartment),
                        initialValue: _preferredDepartment,
                        isExpanded: true,
                        decoration: const InputDecoration(
                          labelText: 'İlgilenmesini düşündüğün birim',
                          prefixIcon: Icon(Icons.account_tree_outlined),
                        ),
                        hint: const Text('Yapay zekâ karar versin'),
                        items: _departments.entries
                            .map(
                              (department) => DropdownMenuItem(
                                value: department.key,
                                child: Text(department.value),
                              ),
                            )
                            .toList(),
                        onChanged: (value) =>
                            setState(() => _preferredDepartment = value),
                      ),
                      const SizedBox(height: 18),
                      _UrgencySelector(
                        value: _urgency,
                        onChanged: (value) => setState(() => _urgency = value),
                      ),
                      const SizedBox(height: 20),
                      _PhotoCard(
                        image: _selectedImage,
                        openCamera: () => _pickImage(ImageSource.camera),
                        openGallery: () => _pickImage(ImageSource.gallery),
                      ),
                      const SizedBox(height: 26),
                      PrimaryButton(
                        label: _isSubmitting
                            ? 'Yönlendiriliyor...'
                            : 'İhbarı yapay zekâya ilet',
                        icon: Icons.auto_awesome_rounded,
                        isLoading: _isSubmitting,
                        onPressed: _submit,
                      ),
                      const SizedBox(height: 30),
                    ],
                  ),
                ),
              ],
            ),
          ),
        ),
      ],
    );
  }
}

class _ComplaintType {
  const _ComplaintType({
    required this.code,
    required this.label,
    required this.icon,
    required this.color,
  });

  final String code;
  final String label;
  final IconData icon;
  final Color color;
}

class _LocationCard extends StatelessWidget {
  const _LocationCard({
    required this.location,
    required this.onLocationSelected,
  });

  final LatLng? location;
  final ValueChanged<LatLng> onLocationSelected;

  @override
  Widget build(BuildContext context) {
    return Container(
      height: 250,
      clipBehavior: Clip.antiAlias,
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(24),
        color: Colors.white,
      ),
      child: Stack(
        children: [
          FlutterMap(
            options: MapOptions(
              initialCenter: LatLng(37.0000, 35.3213),
              initialZoom: 13,
              onTap: (_, point) => onLocationSelected(point),
            ),
            children: [
              TileLayer(
                urlTemplate: ApiConfig.openStreetMapTiles,
                userAgentPackageName: 'com.example.sehirtakip_app',
              ),
              if (location != null)
                MarkerLayer(
                  markers: [
                    Marker(
                      point: location!,
                      width: 40,
                      height: 40,
                      alignment: Alignment.topCenter,
                      child: const Icon(
                        Icons.location_on_rounded,
                        color: Colors.red,
                        size: 40,
                      ),
                    ),
                  ],
                ),
            ],
          ),
          Positioned(
            left: 12,
            right: 12,
            top: 12,
            child: Container(
              padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 9),
              decoration: BoxDecoration(
                color: Colors.white.withValues(alpha: 0.94),
                borderRadius: BorderRadius.circular(14),
              ),
              child: Row(
                children: [
                  const Icon(
                    Icons.touch_app_rounded,
                    size: 18,
                    color: AppColors.blue,
                  ),
                  const SizedBox(width: 7),
                  Expanded(
                    child: Text(
                      location == null
                          ? 'Haritaya dokunarak konumu belirle'
                          : 'Konum seçildi · ${location!.latitude.toStringAsFixed(4)}, ${location!.longitude.toStringAsFixed(4)}',
                      style: const TextStyle(
                        fontSize: 12,
                        fontWeight: FontWeight.w700,
                      ),
                    ),
                  ),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class _UrgencySelector extends StatelessWidget {
  const _UrgencySelector({required this.value, required this.onChanged});

  final String value;
  final ValueChanged<String> onChanged;

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        const Text(
          'Aciliyet seviyesi',
          style: TextStyle(fontWeight: FontWeight.w800),
        ),
        const SizedBox(height: 10),
        SegmentedButton<String>(
          segments: const [
            ButtonSegment(value: 'Dusuk', label: Text('Düşük')),
            ButtonSegment(value: 'Orta', label: Text('Orta')),
            ButtonSegment(value: 'Yuksek', label: Text('Yüksek')),
          ],
          selected: {value},
          onSelectionChanged: (values) => onChanged(values.first),
        ),
      ],
    );
  }
}

class _PhotoCard extends StatelessWidget {
  const _PhotoCard({
    required this.image,
    required this.openCamera,
    required this.openGallery,
  });

  final XFile? image;
  final VoidCallback openCamera;
  final VoidCallback openGallery;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(14),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(22),
      ),
      child: Row(
        children: [
          ClipRRect(
            borderRadius: BorderRadius.circular(14),
            child: SizedBox(
              width: 64,
              height: 64,
              child: image == null
                  ? const ColoredBox(
                      color: Color(0xFFEAF0F7),
                      child: Icon(
                        Icons.add_a_photo_outlined,
                        color: AppColors.blue,
                      ),
                    )
                  : kIsWeb
                  ? Image.network(image!.path, fit: BoxFit.cover)
                  : Image.file(File(image!.path), fit: BoxFit.cover),
            ),
          ),
          const SizedBox(width: 12),
          const Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  'Fotoğraf ekle',
                  style: TextStyle(fontWeight: FontWeight.w800),
                ),
                SizedBox(height: 3),
                Text(
                  'Ekiplerin sorunu tanımasına yardımcı olur.',
                  style: TextStyle(fontSize: 12, color: Colors.blueGrey),
                ),
              ],
            ),
          ),
          PopupMenuButton<ImageSource>(
            icon: const Icon(Icons.more_horiz_rounded),
            onSelected: (source) =>
                source == ImageSource.camera ? openCamera() : openGallery(),
            itemBuilder: (context) => const [
              PopupMenuItem(
                value: ImageSource.camera,
                child: ListTile(
                  leading: Icon(Icons.camera_alt_outlined),
                  title: Text('Kamera'),
                ),
              ),
              PopupMenuItem(
                value: ImageSource.gallery,
                child: ListTile(
                  leading: Icon(Icons.photo_library_outlined),
                  title: Text('Galeri'),
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }
}
