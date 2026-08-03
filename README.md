# Adana Akıllı Şehir

Vatandaşların arıza bildirimi oluşturabildiği, saha personelinin kendisine
atanan işleri yönetebildiği Flutter mobil uygulaması ve .NET Web API projesi.

## Proje yapısı

- `lib/`: Flutter mobil uygulaması
- `backend/`: ASP.NET Core Web API ve N-Tier katmanları

## Flutter uygulamasını çalıştırma

```powershell
flutter pub get
flutter run
```

Gerçek cihazda API adresini bilgisayarın yerel ağ IP'si ile vermek için:

```powershell
flutter run --dart-define=API_BASE_URL=http://192.168.1.XX:5000
```

## Backend'i çalıştırma

1. `backend/AkilliSehir.API/appsettings.Development.example.json` dosyasını
   `appsettings.Development.json` adıyla kopyalayın.
2. Örnek dosyadaki yerel gizli anahtarları güvenli değerlerle güncelleyin.
3. Veritabanı ve API için aşağıdaki komutları çalıştırın:

```powershell
cd backend
dotnet ef database update --project .\AkilliSehir.DataAccess --startup-project .\AkilliSehir.API
dotnet run --project .\AkilliSehir.API --urls http://0.0.0.0:5000
```

## Güvenlik notu

Yerel geliştirme anahtarları, üretim ayarları ve kullanıcı tarafından yüklenen
fotoğraflar Git'e eklenmez. Bu dosyalar `.gitignore` ile hariç tutulur.
