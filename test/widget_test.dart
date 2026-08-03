import 'package:flutter_test/flutter_test.dart';
import 'package:sehirtakip_app/main.dart';

void main() {
  testWidgets('Rol seçim ekranı açılır', (WidgetTester tester) async {
    await tester.pumpWidget(const SehirTakipUygulamasi());

    expect(find.text('Vatandaş Girişi'), findsOneWidget);
    expect(find.text('Saha Personeli Girişi'), findsOneWidget);
  });
}
