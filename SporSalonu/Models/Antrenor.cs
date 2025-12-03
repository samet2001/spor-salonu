// =====================================================
// ANTRENÖR MODEL SINIFI
// Bu dosya, spor salonlarında çalışan antrenörleri tanımlar
// Antrenörlerin kişisel bilgileri, uzmanlık alanları ve
// müsaitlik saatleri bu sınıfta tutulur
// =====================================================

using System.ComponentModel.DataAnnotations;          // Veri doğrulama attribute'ları
using System.ComponentModel.DataAnnotations.Schema;   // Veritabanı şema attribute'ları

namespace SporSalonu.Models
{
    /// <summary>
    /// Antrenör sınıfı - Spor salonlarında görev yapan eğitmenleri tanımlar
    /// Her antrenörün kişisel bilgileri, uzmanlık alanları ve çalışma saatleri bulunur
    /// </summary>
    public class Antrenor
    {
        // =====================================================
        // PRIMARY KEY (BİRİNCİL ANAHTAR)
        // Her antrenörü benzersiz şekilde tanımlayan ID
        // =====================================================
        [Key]
        public int Id { get; set; }

        // =====================================================
        // ANTRENÖR ADI
        // Zorunlu alan
        // =====================================================
        [Required(ErrorMessage = "Antrenör adı zorunludur")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Ad 2-50 karakter arasında olmalıdır")]
        [Display(Name = "Ad")]
        public string Ad { get; set; } = string.Empty;

        // =====================================================
        // ANTRENÖR SOYADI
        // Zorunlu alan
        // =====================================================
        [Required(ErrorMessage = "Antrenör soyadı zorunludur")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Soyad 2-50 karakter arasında olmalıdır")]
        [Display(Name = "Soyad")]
        public string Soyad { get; set; } = string.Empty;

        // =====================================================
        // TAM İSİM (HESAPLANAN ALAN)
        // Ad ve Soyadı birleştiren salt okunur özellik
        // Veritabanında saklanmaz (NotMapped)
        // =====================================================
        [NotMapped] // Bu alan veritabanına yazılmaz
        [Display(Name = "Ad Soyad")]
        public string TamIsim => $"{Ad} {Soyad}";

        // =====================================================
        // E-POSTA ADRESİ
        // Antrenörün iletişim e-postası
        // =====================================================
        [Required(ErrorMessage = "E-posta adresi zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        [StringLength(100, ErrorMessage = "E-posta en fazla 100 karakter olabilir")]
        [Display(Name = "E-posta")]
        public string Eposta { get; set; } = string.Empty;

        // =====================================================
        // TELEFON NUMARASI
        // Antrenörün iletişim telefonu
        // =====================================================
        [Required(ErrorMessage = "Telefon numarası zorunludur")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
        [StringLength(15, ErrorMessage = "Telefon numarası en fazla 15 karakter olabilir")]
        [Display(Name = "Telefon")]
        public string Telefon { get; set; } = string.Empty;

        // =====================================================
        // UZMANLIK ALANLARI
        // Antrenörün uzmanlaştığı alanlar (virgülle ayrılmış)
        // Örn: "Kas Geliştirme, Kilo Verme, Fitness"
        // =====================================================
        [Required(ErrorMessage = "En az bir uzmanlık alanı belirtilmelidir")]
        [StringLength(500, ErrorMessage = "Uzmanlık alanları en fazla 500 karakter olabilir")]
        [Display(Name = "Uzmanlık Alanları")]
        public string UzmanlikAlanlari { get; set; } = string.Empty;

        // =====================================================
        // BİYOGRAFİ / HAKKINDA
        // Antrenörün kendini tanıttığı metin
        // =====================================================
        [StringLength(2000, ErrorMessage = "Biyografi en fazla 2000 karakter olabilir")]
        [Display(Name = "Biyografi")]
        [DataType(DataType.MultilineText)]
        public string? Biyografi { get; set; }

        // =====================================================
        // PROFİL FOTOĞRAFI URL
        // Antrenörün profil fotoğrafının URL'si
        // =====================================================
        [StringLength(500, ErrorMessage = "Fotoğraf URL'si en fazla 500 karakter olabilir")]
        [Display(Name = "Profil Fotoğrafı")]
        [Url(ErrorMessage = "Geçerli bir URL giriniz")]
        public string? FotografUrl { get; set; }

        // =====================================================
        // ÇALIŞMA BAŞLANGIÇ SAATİ
        // Antrenörün günlük mesai başlangıç saati
        // =====================================================
        [Required(ErrorMessage = "Çalışma başlangıç saati zorunludur")]
        [Display(Name = "Mesai Başlangıcı")]
        [DataType(DataType.Time)]
        public TimeSpan MesaiBaslangic { get; set; }

        // =====================================================
        // ÇALIŞMA BİTİŞ SAATİ
        // Antrenörün günlük mesai bitiş saati
        // =====================================================
        [Required(ErrorMessage = "Çalışma bitiş saati zorunludur")]
        [Display(Name = "Mesai Bitişi")]
        [DataType(DataType.Time)]
        public TimeSpan MesaiBitis { get; set; }

        // =====================================================
        // SEANS ÜCRETİ (TL)
        // Antrenörün saatlik/seans ücreti
        // =====================================================
        [Required(ErrorMessage = "Seans ücreti zorunludur")]
        [Range(0, 5000, ErrorMessage = "Seans ücreti 0-5000 TL arasında olmalıdır")]
        [Display(Name = "Seans Ücreti (TL)")]
        [Column(TypeName = "decimal(10,2)")]
        [DataType(DataType.Currency)]
        public decimal SeansUcreti { get; set; }

        // =====================================================
        // DENEYİM YILI
        // Antrenörün toplam deneyim süresi (yıl)
        // =====================================================
        [Range(0, 50, ErrorMessage = "Deneyim yılı 0-50 arasında olmalıdır")]
        [Display(Name = "Deneyim (Yıl)")]
        public int DeneyimYili { get; set; }

        // =====================================================
        // AKTİFLİK DURUMU
        // Antrenörün aktif çalışıp çalışmadığını belirtir
        // =====================================================
        [Display(Name = "Aktif mi?")]
        public bool Aktif { get; set; } = true;

        // =====================================================
        // KAYIT TARİHİ
        // Antrenörün sisteme eklendiği tarih
        // =====================================================
        [Display(Name = "Kayıt Tarihi")]
        [DataType(DataType.DateTime)]
        public DateTime KayitTarihi { get; set; } = DateTime.UtcNow;

        // =====================================================
        // FOREIGN KEY (YABANCI ANAHTAR)
        // Antrenörün çalıştığı spor salonunun ID'si
        // =====================================================
        [Required(ErrorMessage = "Salon seçimi zorunludur")]
        [Display(Name = "Çalıştığı Salon")]
        public int SporSalonuId { get; set; }

        // =====================================================
        // NAVİGASYON ÖZELLİKLERİ
        // Entity Framework ilişkileri için kullanılır
        // =====================================================

        /// <summary>
        /// Antrenörün çalıştığı spor salonu
        /// Foreign key ilişkisi (Many-to-One)
        /// </summary>
        [ForeignKey("SporSalonuId")]
        public virtual SporSalonu? SporSalonu { get; set; }

        /// <summary>
        /// Antrenörün verebileceği hizmetler
        /// Çoka-çok (Many-to-Many) ilişki - ara tablo ile
        /// </summary>
        public virtual ICollection<AntrenorHizmet> AntrenorHizmetleri { get; set; } = new List<AntrenorHizmet>();

        /// <summary>
        /// Antrenörün müsaitlik saatleri
        /// Bire-çok (One-to-Many) ilişki
        /// </summary>
        public virtual ICollection<AntrenorMusaitlik> Musaitlikler { get; set; } = new List<AntrenorMusaitlik>();

        /// <summary>
        /// Antrenöre ait randevular
        /// Bire-çok (One-to-Many) ilişki
        /// </summary>
        public virtual ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();
    }
}

