// =====================================================
// HİZMET MODEL SINIFI
// Bu dosya, spor salonlarının sunduğu hizmetleri tanımlar
// Örnek hizmetler: Fitness, Yoga, Pilates, Grup Dersi vb.
// =====================================================

using System.ComponentModel.DataAnnotations;          // Veri doğrulama için
using System.ComponentModel.DataAnnotations.Schema;   // Veritabanı şema yapılandırması için

namespace SporSalonu.Models
{
    /// <summary>
    /// Hizmet sınıfı - Spor salonlarının sunduğu hizmet türlerini tanımlar
    /// Her hizmetin adı, süresi, ücreti ve açıklaması bulunur
    /// </summary>
    public class Hizmet
    {
        // =====================================================
        // PRIMARY KEY (BİRİNCİL ANAHTAR)
        // Her hizmeti benzersiz şekilde tanımlayan ID
        // =====================================================
        [Key]
        public int Id { get; set; }

        // =====================================================
        // HİZMET ADI
        // Zorunlu alan - örn: "Fitness", "Yoga", "Pilates"
        // =====================================================
        [Required(ErrorMessage = "Hizmet adı zorunludur")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Hizmet adı 2-100 karakter arasında olmalıdır")]
        [Display(Name = "Hizmet Adı")]
        public string Ad { get; set; } = string.Empty;

        // =====================================================
        // HİZMET AÇIKLAMASI
        // Hizmet hakkında detaylı bilgi
        // =====================================================
        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir")]
        [Display(Name = "Açıklama")]
        [DataType(DataType.MultilineText)]
        public string? Aciklama { get; set; }

        // =====================================================
        // HİZMET SÜRESİ (DAKİKA)
        // Hizmetin standart süresi dakika cinsinden
        // Minimum 15 dakika, maksimum 240 dakika (4 saat)
        // =====================================================
        [Required(ErrorMessage = "Hizmet süresi zorunludur")]
        [Range(15, 240, ErrorMessage = "Süre 15-240 dakika arasında olmalıdır")]
        [Display(Name = "Süre (Dakika)")]
        public int SureDakika { get; set; }

        // =====================================================
        // HİZMET ÜCRETİ (TL)
        // Tek seans için ücret, ondalık sayı olarak tutulur
        // Column attribute ile veritabanı tipini belirtiyoruz
        // =====================================================
        [Required(ErrorMessage = "Ücret zorunludur")]
        [Range(0, 10000, ErrorMessage = "Ücret 0-10000 TL arasında olmalıdır")]
        [Display(Name = "Ücret (TL)")]
        [Column(TypeName = "decimal(10,2)")] // Veritabanında 10 basamak, 2 ondalık
        [DataType(DataType.Currency)]
        public decimal Ucret { get; set; }

        // =====================================================
        // HİZMET KATEGORİSİ
        // Hizmetin ait olduğu kategori (enum olarak)
        // =====================================================
        [Required(ErrorMessage = "Kategori seçimi zorunludur")]
        [Display(Name = "Kategori")]
        public HizmetKategorisi Kategori { get; set; }

        // =====================================================
        // MAKSİMUM KATılımcı SAYISI
        // Grup dersleri için maksimum kişi sayısı
        // 1 ise birebir antrenman anlamına gelir
        // =====================================================
        [Required(ErrorMessage = "Maksimum katılımcı sayısı zorunludur")]
        [Range(1, 50, ErrorMessage = "Katılımcı sayısı 1-50 arasında olmalıdır")]
        [Display(Name = "Maks. Katılımcı")]
        public int MaksimumKatilimci { get; set; } = 1;

        // =====================================================
        // AKTİFLİK DURUMU
        // Hizmetin aktif olup olmadığını belirtir
        // =====================================================
        [Display(Name = "Aktif mi?")]
        public bool Aktif { get; set; } = true;

        // =====================================================
        // FOREIGN KEY (YABANCI ANAHTAR)
        // Bu hizmeti sunan spor salonunun ID'si
        // =====================================================
        [Required(ErrorMessage = "Salon seçimi zorunludur")]
        [Display(Name = "Spor Salonu")]
        public int SporSalonuId { get; set; }

        // =====================================================
        // NAVİGASYON ÖZELLİKLERİ
        // =====================================================

        /// <summary>
        /// Bu hizmeti sunan spor salonu
        /// Foreign key ilişkisi (Many-to-One)
        /// </summary>
        [ForeignKey("SporSalonuId")]
        public virtual SporSalonu? SporSalonu { get; set; }

        /// <summary>
        /// Bu hizmeti verebilen antrenörlerin listesi
        /// Çoka-çok (Many-to-Many) ilişki - ara tablo ile
        /// </summary>
        public virtual ICollection<AntrenorHizmet> AntrenorHizmetleri { get; set; } = new List<AntrenorHizmet>();

        /// <summary>
        /// Bu hizmet için alınan randevuların listesi
        /// Bire-çok (One-to-Many) ilişki
        /// </summary>
        public virtual ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();
    }

    // =====================================================
    // HİZMET KATEGORİSİ ENUM
    // Hizmetlerin kategorilerini tanımlar
    // =====================================================
    /// <summary>
    /// Hizmet kategorilerini tanımlayan enum
    /// Yeni kategoriler eklenebilir
    /// </summary>
    public enum HizmetKategorisi
    {
        [Display(Name = "Fitness")]
        Fitness = 1,

        [Display(Name = "Yoga")]
        Yoga = 2,

        [Display(Name = "Pilates")]
        Pilates = 3,

        [Display(Name = "Kardio")]
        Kardio = 4,

        [Display(Name = "Kas Geliştirme")]
        KasGelistirme = 5,

        [Display(Name = "Kilo Verme")]
        KiloVerme = 6,

        [Display(Name = "Crossfit")]
        Crossfit = 7,

        [Display(Name = "Yüzme")]
        Yuzme = 8,

        [Display(Name = "Boks")]
        Boks = 9,

        [Display(Name = "Grup Dersi")]
        GrupDersi = 10,

        [Display(Name = "Kişisel Antrenman")]
        KisiselAntrenman = 11,

        [Display(Name = "Diğer")]
        Diger = 99
    }
}

