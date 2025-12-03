
##  Geliştirici

SAMET FIRINCI B211210036
2. Öğretim A grubu
Sakarya Üniversitesi Bilgisayar Mühendisliği


# FitLife Spor Salonu Yönetim Sistemi

ASP.NET Core MVC kullanılarak geliştirilmiş kapsamlı bir spor salonu yönetim ve randevu sistemi.

##  Proje Hakkında

Bu proje, spor salonlarının günlük operasyonlarını yönetmek, üye kayıtlarını tutmak, antrenör-üye randevularını organize etmek ve yapay zeka destekli fitness önerileri sunmak için geliştirilmiş tam kapsamlı bir web uygulamasıdır.

##  Özellikler

###  Spor Salonu Yönetimi
- Spor salonu tanımları (çalışma saatleri, hizmetler, ücretler)
- Hizmet yönetimi (Fitness, Yoga, Pilates, Crossfit, vb.)
- Çoklu salon desteği

### Antrenör Yönetimi
- Antrenör profilleri ve uzmanlık alanları
- Müsaitlik takvimi ve çalışma saatleri
- Antrenör-hizmet eşleştirme

###  Randevu Sistemi
- Çakışma kontrolü ile akıllı randevu oluşturma
- Randevu onay/red/iptal mekanizması
- Üye ve admin tarafında randevu yönetimi
- Müsait saatleri otomatik gösterme

###  AI Fitness Danışmanı
- BMI hesaplama ve analiz
- Kişiselleştirilmiş egzersiz programı önerileri
- Beslenme ve diyet önerileri
- Vücut tipi analizi

###  Kullanıcı Yönetimi
- Rol tabanlı yetkilendirme (Admin, Üye)
- ASP.NET Core Identity entegrasyonu
- Kullanıcı profil yönetimi
- Şifre değiştirme

###  Admin Paneli
- Dashboard ile genel istatistikler
- CRUD işlemleri (Antrenör, Hizmet, Randevu)
- Üye listesi ve yönetimi
- Raporlama özellikleri

###  REST API
- LINQ ile veri filtreleme
- JSON tabanlı API endpoint'leri
- Antrenör, hizmet ve randevu sorgulama

##  Teknolojiler

### Backend
- **Framework**: ASP.NET Core MVC 8.0
- **ORM**: Entity Framework Core
- **Veritabanı**: PostgreSQL
- **Authentication**: ASP.NET Core Identity

### Frontend
- **UI Framework**: Bootstrap 5
- **Icons**: Bootstrap Icons
- **JavaScript**: jQuery
- **CSS**: Custom CSS3

### Diğer
- **AI Integration**: Kural tabanlı öneri sistemi
- **Validation**: Client-side & Server-side
- **Architecture**: MVC Pattern

##  Gereksinimler

- .NET 8.0 SDK veya üzeri
- PostgreSQL 12 veya üzeri
- Web tarayıcısı

##  Kurulum

### 1. Repository'yi Klonlayın
```bash
git clone https://github.com/samet2001/spor-salonu.git
cd spor-salonu
```

### 2. PostgreSQL Veritabanını Hazırlayın
```bash
# PostgreSQL'de veritabanı oluşturun
createdb SporSalonuDB
```

### 3. Bağlantı Ayarlarını Yapılandırın
`SporSalonu/appsettings.json` dosyasındaki bağlantı dizesini güncelleyin:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=SporSalonuDB;Username=KULLANICI_ADINIZ;Password=SIFRENIZ"
  }
}
```

### 4. Projeyi Çalıştırın
```bash
cd SporSalonu
dotnet restore
dotnet run
```

Uygulama `http://localhost:5000` adresinde çalışacaktır.

##  Demo Hesapları

### Admin
- **E-posta**: b211210036@sakarya.edu.tr
- **Şifre**: sau

### Üye
- **E-posta**: uye@test.com
- **Şifre**: 123



##  Ana Rotalar

### Genel
- `/` - Ana sayfa
- `/Home/Hizmetlerimiz` - Hizmetler
- `/Home/Antrenorler` - Antrenörler
- `/Home/Iletisim` - İletişim

### Kullanıcı
- `/Hesap/KayitOl` - Kayıt ol
- `/Hesap/Giris` - Giriş yap
- `/Hesap/Profil` - Profil
- `/Randevu` - Randevularım

### AI
- `/AI` - AI Fitness Danışmanı

### Admin (Yetki gerektirir)
- `/Admin/Admin` - Dashboard
- `/Admin/Admin/Uyeler` - Üye yönetimi
- `/Admin/Admin/Antrenorler` - Antrenör yönetimi
- `/Admin/Admin/Randevular` - Randevu yönetimi

### API
- `/api/sporsalonu/antrenorler` - Antrenör listesi
- `/api/sporsalonu/hizmetler` - Hizmet listesi
- `/api/sporsalonu/raporlar/gunluk` - Günlük rapor







