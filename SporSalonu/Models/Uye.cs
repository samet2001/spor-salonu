// =====================================================
// ÜYE (KULLANICI) MODEL SINIFI
// Bu dosya, ASP.NET Core Identity kullanarak
// sistemin kullanıcı modelini genişletir
// Üyelerin profil bilgileri ve fiziksel özellikleri tutulur
// =====================================================

using Microsoft.AspNetCore.Identity;  // ASP.NET Core Identity için
using System.ComponentModel.DataAnnotations;          // Veri doğrulama
using System.ComponentModel.DataAnnotations.Schema;   // Veritabanı şema yapılandırması

namespace SporSalonu.Models
{
    /// <summary>
    /// Uye sınıfı - IdentityUser'dan türetilmiş özel kullanıcı sınıfı
    /// ASP.NET Core Identity ile entegre çalışır
    /// Standart kullanıcı bilgilerine ek olarak fitness ile ilgili alanlar içerir
    /// </summary>
    public class Uye : IdentityUser
    {
        // NOT: IdentityUser'dan miras alınan özellikler:
        // - Id (string - GUID formatında)
        // - UserName (kullanıcı adı)
        // - Email (e-posta)
        // - PasswordHash (şifrelenmiş parola)
        // - PhoneNumber (telefon)
        // - EmailConfirmed, PhoneNumberConfirmed, TwoFactorEnabled vb.

        // =====================================================
        // ÜYE ADI
        // Kullanıcının gerçek adı
        // =====================================================
        [Required(ErrorMessage = "Ad zorunludur")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Ad 2-50 karakter arasında olmalıdır")]
        [Display(Name = "Ad")]
        public string Ad { get; set; } = string.Empty;

        // =====================================================
        // ÜYE SOYADI
        // Kullanıcının gerçek soyadı
        // =====================================================
        [Required(ErrorMessage = "Soyad zorunludur")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Soyad 2-50 karakter arasında olmalıdır")]
        [Display(Name = "Soyad")]
        public string Soyad { get; set; } = string.Empty;

        // =====================================================
        // TAM İSİM (HESAPLANAN ALAN)
        // Ad ve Soyadı birleştiren özellik
        // Veritabanında saklanmaz
        // =====================================================
        [NotMapped]
        [Display(Name = "Ad Soyad")]
        public string TamIsim => $"{Ad} {Soyad}";

        // =====================================================
        // DOĞUM TARİHİ
        // Yaş hesaplaması ve sağlık önerileri için kullanılır
        // =====================================================
        [Display(Name = "Doğum Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? DogumTarihi { get; set; }

        // =====================================================
        // CİNSİYET
        // Fitness önerileri için kullanılır
        // =====================================================
        [Display(Name = "Cinsiyet")]
        public Cinsiyet? Cinsiyet { get; set; }

        // =====================================================
        // BOY (CM)
        // Kullanıcının boyu santimetre cinsinden
        // BMI hesaplaması için kullanılır
        // =====================================================
        [Range(100, 250, ErrorMessage = "Boy 100-250 cm arasında olmalıdır")]
        [Display(Name = "Boy (cm)")]
        public int? Boy { get; set; }

        // =====================================================
        // KİLO (KG)
        // Kullanıcının kilosu kilogram cinsinden
        // BMI hesaplaması için kullanılır
        // =====================================================
        [Range(30, 300, ErrorMessage = "Kilo 30-300 kg arasında olmalıdır")]
        [Display(Name = "Kilo (kg)")]
        [Column(TypeName = "decimal(5,2)")] // Örn: 75.50 kg
        public decimal? Kilo { get; set; }

        // =====================================================
        // VÜCUT KİTLE İNDEKSİ (BMI) - HESAPLANAN ALAN
        // BMI = Kilo / (Boy/100)^2
        // Veritabanında saklanmaz, anlık hesaplanır
        // =====================================================
        [NotMapped]
        [Display(Name = "BMI")]
        public decimal? BMI
        {
            get
            {
                // Boy ve Kilo değerleri varsa BMI hesapla
                if (Boy.HasValue && Kilo.HasValue && Boy.Value > 0)
                {
                    // Boy'u metreye çevir ve BMI formülünü uygula
                    decimal boyMetre = Boy.Value / 100m;
                    return Math.Round(Kilo.Value / (boyMetre * boyMetre), 2);
                }
                return null; // Değerler eksikse null döndür
            }
        }

        // =====================================================
        // BMI KATEGORİSİ - HESAPLANAN ALAN
        // BMI değerine göre kategori belirler
        // =====================================================
        [NotMapped]
        [Display(Name = "BMI Kategorisi")]
        public string? BMIKategorisi
        {
            get
            {
                if (!BMI.HasValue) return null;

                // BMI aralıklarına göre kategori belirleme
                if (BMI.Value < 18.5m) return "Zayıf";
                if (BMI.Value < 25m) return "Normal";
                if (BMI.Value < 30m) return "Fazla Kilolu";
                return "Obez";
            }
        }

        // =====================================================
        // FİTNESS HEDEFİ
        // Kullanıcının spor yapma amacı
        // =====================================================
        [Display(Name = "Fitness Hedefi")]
        public FitnessHedefi? FitnessHedefi { get; set; }

        // =====================================================
        // DENEYİM SEVİYESİ
        // Kullanıcının spor deneyimi
        // =====================================================
        [Display(Name = "Deneyim Seviyesi")]
        public DeneyimSeviyesi? DeneyimSeviyesi { get; set; }

        // =====================================================
        // SAĞLIK NOTU
        // Önemli sağlık bilgileri (örn: kalp rahatsızlığı)
        // =====================================================
        [StringLength(500, ErrorMessage = "Sağlık notu en fazla 500 karakter olabilir")]
        [Display(Name = "Sağlık Notu")]
        [DataType(DataType.MultilineText)]
        public string? SaglikNotu { get; set; }

        // =====================================================
        // PROFİL FOTOĞRAFI URL
        // Kullanıcının profil resmi
        // =====================================================
        [StringLength(500, ErrorMessage = "Fotoğraf URL'si en fazla 500 karakter olabilir")]
        [Display(Name = "Profil Fotoğrafı")]
        [Url(ErrorMessage = "Geçerli bir URL giriniz")]
        public string? ProfilFotoUrl { get; set; }

        // =====================================================
        // ÜYELİK BAŞLANGIÇ TARİHİ
        // Kullanıcının sisteme kayıt olduğu tarih
        // =====================================================
        [Display(Name = "Üyelik Tarihi")]
        [DataType(DataType.DateTime)]
        public DateTime UyelikTarihi { get; set; } = DateTime.UtcNow;

        // =====================================================
        // AKTİF ÜYELİK DURUMU
        // Kullanıcının üyeliği aktif mi?
        // =====================================================
        [Display(Name = "Aktif Üye mi?")]
        public bool AktifUyelik { get; set; } = true;

        // =====================================================
        // NAVİGASYON ÖZELLİKLERİ
        // =====================================================

        /// <summary>
        /// Üyenin aldığı randevuların listesi
        /// Bire-çok (One-to-Many) ilişki
        /// </summary>
        public virtual ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();
    }

    // =====================================================
    // CİNSİYET ENUM
    // Kullanıcının cinsiyetini tanımlar
    // =====================================================
    public enum Cinsiyet
    {
        [Display(Name = "Erkek")]
        Erkek = 1,

        [Display(Name = "Kadın")]
        Kadin = 2,

        [Display(Name = "Belirtmek İstemiyorum")]
        Belirtilmemis = 3
    }

    // =====================================================
    // FİTNESS HEDEFİ ENUM
    // Kullanıcının spor yapma amacını tanımlar
    // =====================================================
    public enum FitnessHedefi
    {
        [Display(Name = "Kilo Vermek")]
        KiloVermek = 1,

        [Display(Name = "Kas Kazanmak")]
        KasKazanmak = 2,

        [Display(Name = "Form Korumak")]
        FormKorumak = 3,

        [Display(Name = "Güç Artırmak")]
        GucArtirmak = 4,

        [Display(Name = "Esneklik Kazanmak")]
        EsneklikKazanmak = 5,

        [Display(Name = "Dayanıklılık Artırmak")]
        DayaniklilikArtirmak = 6,

        [Display(Name = "Stres Atmak")]
        StresAtmak = 7,

        [Display(Name = "Sağlıklı Yaşam")]
        SaglikliYasam = 8
    }

    // =====================================================
    // DENEYİM SEVİYESİ ENUM
    // Kullanıcının spor deneyimini tanımlar
    // =====================================================
    public enum DeneyimSeviyesi
    {
        [Display(Name = "Başlangıç")]
        Baslangic = 1,

        [Display(Name = "Orta Düzey")]
        OrtaDuzey = 2,

        [Display(Name = "İleri Düzey")]
        IleriDuzey = 3,

        [Display(Name = "Profesyonel")]
        Profesyonel = 4
    }
}

