#  FitLife Spor Salonu Yönetim Sistemi

ASP.NET Core MVC ile geliştirilmiş modern bir spor salonu yönetim ve randevu sistemi.

##  Özellikler

###  Temel Özellikler
-   *Spor Salonu Yönetimi** - Salon bilgileri, çalışma saatleri, hizmet tanımları
-  **Antrenör Yönetimi** - Uzmanlık alanları, müsaitlik saatleri, hizmet atamaları
-  **Üye Sistemi** - Kayıt, giriş, profil yönetimi, BMI hesaplama
-  **Randevu Sistemi** - Randevu alma, çakışma kontrolü, onay mekanizması
-  **Admin Paneli** - Tüm varlıklar için CRUD işlemleri
-  **AI Önerileri** - Groq API (Llama 3.3) ile kişiselleştirilmiş egzersiz ve diyet önerileri
-  **REST API** - 8 endpoint, 85+ LINQ sorgusu
-  **BMI Kategorisi** - Otomatik hesaplama ve gösterim (Zayıf/Normal/Fazla Kilolu/Obez)

###  Güvenlik
- Role-based authorization (Admin/Üye)
- Password hashing (ASP.NET Core Identity)
- Anti-forgery tokens
- Client & Server side validation

###  Kullanıcı Arayüzü
- Bootstrap 5 responsive tasarım
- Modern ve kullanıcı dostu arayüz
- Mobil uyumlu

##  Kurulum

### Gereksinimler
- .NET 10.0 SDK
- PostgreSQL 14+
- Groq API Key (ücretsiz: https://console.groq.com)

### 1. Projeyi İndirin
```bash
git clone https://github.com/samet2001/spor-salonu.git
cd spor-salonu/SporSalonu
```

### 2. Veritabanı Yapılandırması

`appsettings.json` dosyasında PostgreSQL bağlantı bilgilerinizi güncelleyin:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=SporSalonuDB;Username=KULLANICI_ADINIZ;Password=SIFRENIZ"
  }
}
```

### 3. Groq API Key Yapılandırması

 **ÖNEMLİ:** API key'inizi asla kaynak kodda saklamayın!

```bash
# User secrets ile güvenli şekilde ekleyin:
dotnet user-secrets init
dotnet user-secrets set "Groq:ApiKey" "BURAYA_GROQ_API_KEYINIZI_YAPISTIIRIN"
```

**Groq API Key nasıl alınır?**
1. https://console.groq.com adresine gidin
2. Ücretsiz hesap oluşturun
3. API Keys bölümünden yeni key oluşturun
4. Günlük limit: 14,400 istek (ücretsiz)

### 4. Veritabanı Migration'ları
```bash
dotnet ef database update
```

### 5. Uygulamayı Çalıştırın
```bash
dotnet run
```

Tarayıcıda açın: http://localhost:5000

## Demo Hesaplar

### Admin Girişi
- **Email:** b211210036@sakarya.edu.tr
- **Şifre:** sau

### Test Üyesi
- **Email:** test@test.com
- **Şifre:** Test123!

## API Kullanımı

### REST API Endpoints

```bash
# Tüm antrenörleri listele
GET /api/sporsalonu/antrenorler

# Belirli tarihte müsait antrenörler
GET /api/sporsalonu/antrenorler/musait?tarih=2024-12-10

# Hizmetleri kategoriye göre filtrele
GET /api/sporsalonu/hizmetler?kategori=Fitness

# Üye randevularını getir
GET /api/sporsalonu/randevular/uye/{uyeId}

# Günlük rapor (Admin)
GET /api/sporsalonu/raporlar/gunluk?tarih=2024-12-03

# Arama
GET /api/sporsalonu/ara?q=yoga

# Salon bilgileri
GET /api/sporsalonu/bilgi
```

##  Teknoloji Stack

- **Framework:** ASP.NET Core MVC 10.0
- **ORM:** Entity Framework Core
- **Veritabanı:** PostgreSQL
- **AI:** Groq API (Llama 3.3 70B)
- **Frontend:** Bootstrap 5, jQuery, HTML5, CSS3
- **Authentication:** ASP.NET Core Identity

## Proje Yapısı

```
SporSalonu/
├── Controllers/
│   ├── HomeController.cs          # Ana sayfa, hizmetler, antrenörler
│   ├── HesapController.cs         # Kayıt, giriş, profil
│   ├── RandevuController.cs       # Randevu işlemleri
│   ├── AIController.cs            # AI önerileri
│   ├── Api/
│   │   └── ApiController.cs       # REST API endpoints
│   └── Areas/Admin/
│       └── AdminController.cs     # Admin paneli
├── Models/
│   ├── SporSalonu.cs              # Salon bilgileri
│   ├── Antrenor.cs                # Antrenör modeli
│   ├── Hizmet.cs                  # Hizmet modeli
│   ├── Randevu.cs                 # Randevu modeli
│   ├── Uye.cs                     # Üye modeli (Identity)
│   └── ViewModels/                # View modeller
├── Services/
│   └── AIService.cs               # Groq API entegrasyonu
├── Data/
│   ├── ApplicationDbContext.cs    # EF Core DbContext
│   └── SeedData.cs                # Başlangıç verileri
└── Views/                         # Razor views

```

## Proje Gereksinimleri (Karşılanan)

-  ASP.NET Core MVC
-  PostgreSQL + Entity Framework Core
-  CRUD işlemleri (Create, Read, Update, Delete)
- Client & Server side validation
-  Admin paneli
-  Kullanıcı kayıt/giriş sistemi
-  Role-based authorization (Admin, Üye)
-  REST API + LINQ sorguları (85+ operasyon)
-  AI entegrasyonu (Groq API)
-  BMI hesaplama ve kategorizasyon
-  Responsive tasarım (Bootstrap 5)

##  Geliştirme

### Migration Oluşturma
```bash
dotnet ef migrations add MigrationAdi
dotnet ef database update
```

### Veritabanını Sıfırlama
```bash
dotnet ef database drop
dotnet ef database update
```

##  Notlar

- Tüm DateTime değerleri UTC formatında saklanır (PostgreSQL uyumluluğu)
- API anahtarları user secrets ile güvenli şekilde saklanmalıdır
- Seed data otomatik olarak demo antrenör, hizmet ve üyeler oluşturur
- BMI kategorileri: Zayıf (<18.5), Normal (18.5-25), Fazla Kilolu (25-30), Obez (>30)

**Geliştirici:** Samet Fırıncı  
**GitHub:** https://github.com/samet2001


