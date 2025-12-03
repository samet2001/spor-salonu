// =====================================================
// RANDEVU MODEL SINIFI
// Bu dosya, üyelerin antrenörlerle yaptığı
// randevuları tanımlar
// Randevu tarihi, saati, hizmet bilgisi ve durumu tutulur
// =====================================================

using System.ComponentModel.DataAnnotations;          // Veri doğrulama
using System.ComponentModel.DataAnnotations.Schema;   // Veritabanı şema yapılandırması

namespace SporSalonu.Models
{
    /// <summary>
    /// Randevu sınıfı - Üye, Antrenör ve Hizmet arasındaki randevu ilişkisini tanımlar
    /// Her randevunun tarihi, saati, durumu ve ücret bilgisi bulunur
    /// </summary>
    public class Randevu
    {
        // =====================================================
        // PRIMARY KEY (BİRİNCİL ANAHTAR)
        // Her randevuyu benzersiz şekilde tanımlayan ID
        // =====================================================
        [Key]
        public int Id { get; set; }

        // =====================================================
        // RANDEVU TARİHİ
        // Randevunun yapılacağı tarih
        // Bugünden önceki tarih seçilemez (validation ile)
        // =====================================================
        [Required(ErrorMessage = "Randevu tarihi zorunludur")]
        [Display(Name = "Randevu Tarihi")]
        [DataType(DataType.Date)]
        public DateTime RandevuTarihi { get; set; }

        // =====================================================
        // BAŞLANGIÇ SAATİ
        // Randevunun başlayacağı saat
        // =====================================================
        [Required(ErrorMessage = "Başlangıç saati zorunludur")]
        [Display(Name = "Başlangıç Saati")]
        [DataType(DataType.Time)]
        public TimeSpan BaslangicSaati { get; set; }

        // =====================================================
        // BİTİŞ SAATİ
        // Randevunun biteceği saat (hizmet süresine göre)
        // =====================================================
        [Required(ErrorMessage = "Bitiş saati zorunludur")]
        [Display(Name = "Bitiş Saati")]
        [DataType(DataType.Time)]
        public TimeSpan BitisSaati { get; set; }

        // =====================================================
        // RANDEVU DURUMU
        // Randevunun mevcut durumu (beklemede, onaylandı, iptal vb.)
        // =====================================================
        [Required(ErrorMessage = "Randevu durumu zorunludur")]
        [Display(Name = "Durum")]
        public RandevuDurumu Durum { get; set; } = RandevuDurumu.Beklemede;

        // =====================================================
        // TOPLAM ÜCRET (TL)
        // Randevu için belirlenen ücret
        // Hizmet ücreti + Antrenör ücreti olabilir
        // =====================================================
        [Required(ErrorMessage = "Ücret zorunludur")]
        [Range(0, 50000, ErrorMessage = "Ücret 0-50000 TL arasında olmalıdır")]
        [Display(Name = "Ücret (TL)")]
        [Column(TypeName = "decimal(10,2)")]
        [DataType(DataType.Currency)]
        public decimal Ucret { get; set; }

        // =====================================================
        // RANDEVU NOTU
        // Üyenin randevu için eklediği not
        // Örn: "Bel ağrısı var, dikkatli olunmalı"
        // =====================================================
        [StringLength(500, ErrorMessage = "Not en fazla 500 karakter olabilir")]
        [Display(Name = "Üye Notu")]
        [DataType(DataType.MultilineText)]
        public string? UyeNotu { get; set; }

        // =====================================================
        // ANTRENÖR NOTU
        // Antrenörün randevu sonrası eklediği not
        // Örn: "Kardio programı uygulandı, bacak ağrısı var"
        // =====================================================
        [StringLength(500, ErrorMessage = "Not en fazla 500 karakter olabilir")]
        [Display(Name = "Antrenör Notu")]
        [DataType(DataType.MultilineText)]
        public string? AntrenorNotu { get; set; }

        // =====================================================
        // İPTAL NEDENİ
        // Randevu iptal edildiyse nedeni
        // =====================================================
        [StringLength(300, ErrorMessage = "İptal nedeni en fazla 300 karakter olabilir")]
        [Display(Name = "İptal Nedeni")]
        public string? IptalNedeni { get; set; }

        // =====================================================
        // RANDEVU OLUŞTURULMA TARİHİ
        // Randevunun sisteme kaydedildiği tarih/saat
        // =====================================================
        [Display(Name = "Oluşturulma Tarihi")]
        [DataType(DataType.DateTime)]
        public DateTime OlusturmaTarihi { get; set; } = DateTime.UtcNow;

        // =====================================================
        // RANDEVU GÜNCELLEME TARİHİ
        // Randevunun son güncellendiği tarih/saat
        // =====================================================
        [Display(Name = "Güncellenme Tarihi")]
        [DataType(DataType.DateTime)]
        public DateTime? GuncellemeTarihi { get; set; }

        // =====================================================
        // FOREIGN KEYS (YABANCI ANAHTARLAR)
        // =====================================================

        /// <summary>
        /// Randevuyu alan üyenin ID'si (IdentityUser'dan string)
        /// </summary>
        [Required(ErrorMessage = "Üye seçimi zorunludur")]
        [Display(Name = "Üye")]
        public string UyeId { get; set; } = string.Empty;

        /// <summary>
        /// Randevu verilen antrenörün ID'si
        /// </summary>
        [Required(ErrorMessage = "Antrenör seçimi zorunludur")]
        [Display(Name = "Antrenör")]
        public int AntrenorId { get; set; }

        /// <summary>
        /// Randevunun alındığı hizmetin ID'si
        /// </summary>
        [Required(ErrorMessage = "Hizmet seçimi zorunludur")]
        [Display(Name = "Hizmet")]
        public int HizmetId { get; set; }

        // =====================================================
        // NAVİGASYON ÖZELLİKLERİ
        // Entity Framework ilişkileri için
        // =====================================================

        /// <summary>
        /// Randevuyu alan üye
        /// </summary>
        [ForeignKey("UyeId")]
        public virtual Uye? Uye { get; set; }

        /// <summary>
        /// Randevu verilen antrenör
        /// </summary>
        [ForeignKey("AntrenorId")]
        public virtual Antrenor? Antrenor { get; set; }

        /// <summary>
        /// Randevunun alındığı hizmet
        /// </summary>
        [ForeignKey("HizmetId")]
        public virtual Hizmet? Hizmet { get; set; }

        // =====================================================
        // YARDIMCI METODLAR
        // =====================================================

        /// <summary>
        /// Randevunun geçmişte olup olmadığını kontrol eder
        /// </summary>
        [NotMapped]
        public bool GecmisMi => RandevuTarihi.Date < DateTime.UtcNow.Date ||
                               (RandevuTarihi.Date == DateTime.UtcNow.Date && BitisSaati < DateTime.UtcNow.TimeOfDay);

        /// <summary>
        /// Randevunun bugün olup olmadığını kontrol eder
        /// </summary>
        [NotMapped]
        public bool BugunMu => RandevuTarihi.Date == DateTime.UtcNow.Date;

        /// <summary>
        /// Randevu süresini dakika cinsinden döndürür
        /// </summary>
        [NotMapped]
        public int SureDakika => (int)(BitisSaati - BaslangicSaati).TotalMinutes;
    }

    // =====================================================
    // RANDEVU DURUMU ENUM
    // Randevunun mevcut durumunu tanımlar
    // =====================================================
    public enum RandevuDurumu
    {
        /// <summary>
        /// Randevu alındı, onay bekliyor
        /// </summary>
        [Display(Name = "Beklemede")]
        Beklemede = 1,

        /// <summary>
        /// Antrenör/Admin tarafından onaylandı
        /// </summary>
        [Display(Name = "Onaylandı")]
        Onaylandi = 2,

        /// <summary>
        /// Randevu iptal edildi
        /// </summary>
        [Display(Name = "İptal Edildi")]
        IptalEdildi = 3,

        /// <summary>
        /// Randevu tamamlandı (gerçekleşti)
        /// </summary>
        [Display(Name = "Tamamlandı")]
        Tamamlandi = 4,

        /// <summary>
        /// Üye randevuya gelmedi
        /// </summary>
        [Display(Name = "Gelmedi")]
        Gelmedi = 5,

        /// <summary>
        /// Randevu ertelendi, yeni tarih bekliyor
        /// </summary>
        [Display(Name = "Ertelendi")]
        Ertelendi = 6
    }
}

