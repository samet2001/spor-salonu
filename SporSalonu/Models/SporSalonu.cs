// =====================================================
// SPOR SALONU MODEL SINIFI
// Bu dosya, spor salonu entity'sini (varlık) tanımlar
// Veritabanında "SporSalonlari" tablosuna karşılık gelir
// =====================================================

using System.ComponentModel.DataAnnotations;          // Veri doğrulama attribute'ları için
using System.ComponentModel.DataAnnotations.Schema;   // Veritabanı şema attribute'ları için

namespace SporSalonu.Models
{
    /// <summary>
    /// Spor Salonu sınıfı - Sistemdeki spor salonlarının bilgilerini tutar
    /// Her spor salonunun adı, adresi, çalışma saatleri ve iletişim bilgileri bulunur
    /// </summary>
    public class SporSalonu
    {
        // =====================================================
        // PRIMARY KEY (BİRİNCİL ANAHTAR)
        // Her spor salonunu benzersiz şekilde tanımlayan ID
        // =====================================================
        [Key] // Bu alan veritabanında birincil anahtar olarak işaretlenir
        public int Id { get; set; }

        // =====================================================
        // SALON ADI
        // Zorunlu alan, en fazla 100 karakter olabilir
        // =====================================================
        [Required(ErrorMessage = "Salon adı zorunludur")] // Boş bırakılamaz
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Salon adı 2-100 karakter arasında olmalıdır")]
        [Display(Name = "Salon Adı")] // View'larda gösterilecek etiket
        public string Ad { get; set; } = string.Empty;

        // =====================================================
        // ADRES BİLGİSİ
        // Salonun fiziksel adresi, en fazla 300 karakter
        // =====================================================
        [Required(ErrorMessage = "Adres zorunludur")]
        [StringLength(300, ErrorMessage = "Adres en fazla 300 karakter olabilir")]
        [Display(Name = "Adres")]
        public string Adres { get; set; } = string.Empty;

        // =====================================================
        // TELEFON NUMARASI
        // Format: 05XX XXX XX XX şeklinde olmalı
        // =====================================================
        [Required(ErrorMessage = "Telefon numarası zorunludur")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
        [StringLength(15, ErrorMessage = "Telefon numarası en fazla 15 karakter olabilir")]
        [Display(Name = "Telefon")]
        public string Telefon { get; set; } = string.Empty;

        // =====================================================
        // E-POSTA ADRESİ
        // Geçerli bir e-posta formatında olmalı
        // =====================================================
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        [StringLength(100, ErrorMessage = "E-posta en fazla 100 karakter olabilir")]
        [Display(Name = "E-posta")]
        public string? Eposta { get; set; }

        // =====================================================
        // AÇILIŞ SAATİ
        // Salonun günlük açılış saati (örn: 07:00)
        // =====================================================
        [Required(ErrorMessage = "Açılış saati zorunludur")]
        [Display(Name = "Açılış Saati")]
        [DataType(DataType.Time)] // Saat formatında veri girişi
        public TimeSpan AcilisSaati { get; set; }

        // =====================================================
        // KAPANIŞ SAATİ
        // Salonun günlük kapanış saati (örn: 22:00)
        // =====================================================
        [Required(ErrorMessage = "Kapanış saati zorunludur")]
        [Display(Name = "Kapanış Saati")]
        [DataType(DataType.Time)]
        public TimeSpan KapanisSaati { get; set; }

        // =====================================================
        // SALON AÇIKLAMASI
        // Salon hakkında detaylı bilgi (isteğe bağlı)
        // =====================================================
        [StringLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir")]
        [Display(Name = "Açıklama")]
        [DataType(DataType.MultilineText)] // Çok satırlı metin girişi
        public string? Aciklama { get; set; }

        // =====================================================
        // SALON RESMİ URL
        // Salonun tanıtım fotoğrafının URL'si
        // =====================================================
        [StringLength(500, ErrorMessage = "Resim URL'si en fazla 500 karakter olabilir")]
        [Display(Name = "Salon Resmi")]
        [Url(ErrorMessage = "Geçerli bir URL giriniz")]
        public string? ResimUrl { get; set; }

        // =====================================================
        // AKTİFLİK DURUMU
        // Salonun aktif olup olmadığını belirtir
        // Pasif salonlar sistemde görünmez
        // =====================================================
        [Display(Name = "Aktif mi?")]
        public bool Aktif { get; set; } = true; // Varsayılan olarak aktif

        // =====================================================
        // KAYIT TARİHİ
        // Salonun sisteme eklendiği tarih
        // =====================================================
        [Display(Name = "Kayıt Tarihi")]
        [DataType(DataType.DateTime)]
        public DateTime KayitTarihi { get; set; } = DateTime.UtcNow;

        // =====================================================
        // NAVİGASYON ÖZELLİKLERİ (NAVIGATION PROPERTIES)
        // İlişkili tablolara erişim için kullanılır
        // =====================================================

        /// <summary>
        /// Bu salonda sunulan hizmetlerin listesi
        /// Bir salonun birden fazla hizmeti olabilir (One-to-Many ilişki)
        /// </summary>
        public virtual ICollection<Hizmet> Hizmetler { get; set; } = new List<Hizmet>();

        /// <summary>
        /// Bu salonda çalışan antrenörlerin listesi
        /// Bir salonda birden fazla antrenör çalışabilir (One-to-Many ilişki)
        /// </summary>
        public virtual ICollection<Antrenor> Antrenorler { get; set; } = new List<Antrenor>();
    }
}

