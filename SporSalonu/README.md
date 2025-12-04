# FitLife - Spor Salonu Yönetim ve Randevu Sistemi

## Proje Hakkında

Bu proje, ASP.NET Core MVC kullanarak geliştirilmiş bir Spor Salonu Yönetim ve Randevu Sistemidir. Sistem, spor salonlarının sunduğu hizmetleri, antrenörlerin uzmanlık alanlarını, üyelerin randevularını ve yapay zeka tabanlı egzersiz önerilerini yönetebilecek niteliktedir.

## Özellikler

### 1. Spor Salonu Tanımlamaları
- Spor salonu bilgileri (ad, adres, telefon, çalışma saatleri)
- Hizmet türleri tanımlama (fitness, yoga, pilates vb.)
- Hizmet süre ve ücret yönetimi

### 2. Antrenör Yönetimi
- Antrenör profil bilgileri
- Uzmanlık alanları tanımlama
- Müsaitlik saatleri yönetimi

### 3. Üye ve Randevu Sistemi
- Kullanıcı kayıt/giriş sistemi
- Online randevu oluşturma
- Randevu çakışma kontrolü
- Randevu onay mekanizması

### 4. REST API
- Antrenör listeleme ve filtreleme
- Hizmet listeleme
- Randevu sorgulama
- LINQ sorguları ile veri filtreleme

### 5. Yapay Zeka Entegrasyonu
- BMI hesaplama ve değerlendirme
- Kişiselleştirilmiş egzersiz programı önerisi
- Beslenme ve diyet önerileri
- OpenAI API desteği (opsiyonel)

## Teknolojiler

- **Backend:** ASP.NET Core MVC (.NET 10)
- **ORM:** Entity Framework Core
- **Veritabanı:** PostgreSQL
- **Frontend:** Bootstrap 5, HTML5, CSS3, JavaScript, jQuery
- **Kimlik Doğrulama:** ASP.NET Core Identity

## Kurulum

### Gereksinimler
- .NET 10 SDK
- PostgreSQL 14+
- Visual Studio Code veya Visual Studio 2022

### Adımlar

1. **PostgreSQL Ayarları**
   ```
   appsettings.json dosyasındaki connection string'i güncelleyin:
   Host=localhost;Port=5432;Database=SporSalonuDB;Username=postgres;Password=postgres
   ```

2. **Projeyi Çalıştırma**
   ```bash
   cd SporSalonu
   dotnet run
   ```

3. **Tarayıcıda Açma**
   ```
   http://localhost:5000
   ```

## Demo Hesapları

| Rol | E-posta | Şifre |
|-----|---------|-------|
| Admin | b211210036@sakarya.edu.tr | sau |
| Üye | uye@test.com | 123 |

## API Endpoint'leri

| Endpoint | Metod | Açıklama |
|----------|-------|----------|
| `/api/sporsalonu/antrenorler` | GET | Tüm antrenörleri listeler |
| `/api/sporsalonu/antrenorler/{id}` | GET | Antrenör detayı |
| `/api/sporsalonu/antrenorler/musait?tarih=2024-01-15` | GET | Belirli tarihte müsait antrenörler |
| `/api/sporsalonu/hizmetler` | GET | Tüm hizmetleri listeler |
| `/api/sporsalonu/hizmetler?kategori=Yoga` | GET | Kategoriye göre hizmetler |
| `/api/sporsalonu/randevular/uye/{uyeId}` | GET | Üye randevuları |
| `/api/sporsalonu/randevular/antrenor/{antrenorId}` | GET | Antrenör randevuları |
| `/api/sporsalonu/raporlar/gunluk` | GET | Günlük rapor (Admin) |
| `/api/sporsalonu/ara?q=yoga` | GET | Arama |
| `/api/sporsalonu/bilgi` | GET | Salon bilgileri |

## Proje Yapısı

```
SporSalonu/
├── Areas/
│   └── Admin/
│       ├── Controllers/
│       └── Views/
├── Controllers/
│   ├── Api/
│   │   └── ApiController.cs
│   ├── AIController.cs
│   ├── HesapController.cs
│   ├── HomeController.cs
│   └── RandevuController.cs
├── Data/
│   ├── ApplicationDbContext.cs
│   └── SeedData.cs
├── Models/
│   ├── ViewModels/
│   ├── Antrenor.cs
│   ├── AntrenorHizmet.cs
│   ├── AntrenorMusaitlik.cs
│   ├── Hizmet.cs
│   ├── Randevu.cs
│   ├── SporSalonu.cs
│   └── Uye.cs
├── Services/
│   └── AIService.cs
├── Views/
│   ├── AI/
│   ├── Hesap/
│   ├── Home/
│   ├── Randevu/
│   └── Shared/
├── Program.cs
└── appsettings.json
```

## Yapay Zeka Özelliği

Sistem, kullanıcıların fiziksel bilgileri (yaş, boy, kilo, cinsiyet) ve hedeflerine göre kişiselleştirilmiş öneriler sunar:

1. **BMI Analizi:** Vücut kitle indeksi hesaplaması ve kategorilendirme
2. **Egzersiz Programı:** Haftalık antrenman planı
3. **Beslenme Önerisi:** Günlük kalori ve makro besin dağılımı

Ücretsiz api olduğundan her pc de yeniden almak gerekebilir.

## Geliştirici Notları

- Validation hem client hem server tarafında uygulanmıştır
- Rol bazlı yetkilendirme (Admin, Üye) mevcuttur
- Entity Framework Code First yaklaşımı kullanılmıştır



