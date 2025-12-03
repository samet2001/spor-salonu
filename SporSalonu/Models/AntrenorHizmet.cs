// =====================================================
// ANTRENÖR-HİZMET ARA TABLO MODEL SINIFI
// Bu dosya, antrenörler ve hizmetler arasındaki
// çoka-çok (Many-to-Many) ilişkiyi yönetir
// Bir antrenör birden fazla hizmet verebilir,
// bir hizmet birden fazla antrenör tarafından verilebilir
// =====================================================

using System.ComponentModel.DataAnnotations;          // Veri doğrulama
using System.ComponentModel.DataAnnotations.Schema;   // Veritabanı şema yapılandırması

namespace SporSalonu.Models
{
    /// <summary>
    /// AntrenorHizmet sınıfı - Antrenör ve Hizmet arasındaki ilişkiyi tanımlar
    /// Bu ara tablo sayesinde hangi antrenörün hangi hizmetleri verebildiğini takip ederiz
    /// </summary>
    public class AntrenorHizmet
    {
        // =====================================================
        // PRIMARY KEY (BİRİNCİL ANAHTAR)
        // Her kayıt için benzersiz ID
        // =====================================================
        [Key]
        public int Id { get; set; }

        // =====================================================
        // ANTRENÖR FOREIGN KEY
        // İlişkilendirilecek antrenörün ID'si
        // =====================================================
        [Required(ErrorMessage = "Antrenör seçimi zorunludur")]
        [Display(Name = "Antrenör")]
        public int AntrenorId { get; set; }

        // =====================================================
        // HİZMET FOREIGN KEY
        // İlişkilendirilecek hizmetin ID'si
        // =====================================================
        [Required(ErrorMessage = "Hizmet seçimi zorunludur")]
        [Display(Name = "Hizmet")]
        public int HizmetId { get; set; }

        // =====================================================
        // ANTRENÖRÜN BU HİZMET İÇİN SERTİFİKASI VAR MI?
        // Bazı hizmetler sertifika gerektirir
        // =====================================================
        [Display(Name = "Sertifikalı mı?")]
        public bool SertifikaliMi { get; set; } = false;

        // =====================================================
        // SERTİFİKA TARİHİ
        // Antrenörün bu hizmet için sertifika aldığı tarih
        // =====================================================
        [Display(Name = "Sertifika Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? SertifikaTarihi { get; set; }

        // =====================================================
        // İLİŞKİ EKLENDİĞİ TARİH
        // Antrenöre bu hizmetin ne zaman atandığı
        // =====================================================
        [Display(Name = "Atanma Tarihi")]
        [DataType(DataType.DateTime)]
        public DateTime AtanmaTarihi { get; set; } = DateTime.UtcNow;

        // =====================================================
        // NAVİGASYON ÖZELLİKLERİ
        // İlişkili entity'lere erişim için
        // =====================================================

        /// <summary>
        /// İlişkilendirilen antrenör
        /// </summary>
        [ForeignKey("AntrenorId")]
        public virtual Antrenor? Antrenor { get; set; }

        /// <summary>
        /// İlişkilendirilen hizmet
        /// </summary>
        [ForeignKey("HizmetId")]
        public virtual Hizmet? Hizmet { get; set; }
    }
}

