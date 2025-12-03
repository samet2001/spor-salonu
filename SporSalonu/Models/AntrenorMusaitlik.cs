// =====================================================
// ANTRENÖR MÜSAİTLİK MODEL SINIFI
// Bu dosya, antrenörlerin hangi gün ve saatlerde
// müsait olduğunu tanımlar
// Üyeler bu müsaitlik bilgilerine göre randevu alabilir
// =====================================================

using System.ComponentModel.DataAnnotations;          // Veri doğrulama
using System.ComponentModel.DataAnnotations.Schema;   // Veritabanı şema yapılandırması

namespace SporSalonu.Models
{
    /// <summary>
    /// AntrenorMusaitlik sınıfı - Antrenörlerin müsaitlik saatlerini tanımlar
    /// Her antrenör için haftalık müsaitlik takvimi oluşturulabilir
    /// </summary>
    public class AntrenorMusaitlik
    {
        // =====================================================
        // PRIMARY KEY (BİRİNCİL ANAHTAR)
        // Her müsaitlik kaydı için benzersiz ID
        // =====================================================
        [Key]
        public int Id { get; set; }

        // =====================================================
        // ANTRENÖR FOREIGN KEY
        // Müsaitlik kaydının ait olduğu antrenör
        // =====================================================
        [Required(ErrorMessage = "Antrenör seçimi zorunludur")]
        [Display(Name = "Antrenör")]
        public int AntrenorId { get; set; }

        // =====================================================
        // HAFTANIN GÜNÜ
        // Müsaitliğin geçerli olduğu gün (0=Pazar, 1=Pazartesi, vb.)
        // =====================================================
        [Required(ErrorMessage = "Gün seçimi zorunludur")]
        [Display(Name = "Gün")]
        public DayOfWeek Gun { get; set; }

        // =====================================================
        // BAŞLANGIÇ SAATİ
        // Antrenörün o gün müsait olmaya başladığı saat
        // =====================================================
        [Required(ErrorMessage = "Başlangıç saati zorunludur")]
        [Display(Name = "Başlangıç Saati")]
        [DataType(DataType.Time)]
        public TimeSpan BaslangicSaati { get; set; }

        // =====================================================
        // BİTİŞ SAATİ
        // Antrenörün o gün müsaitliğinin sona erdiği saat
        // =====================================================
        [Required(ErrorMessage = "Bitiş saati zorunludur")]
        [Display(Name = "Bitiş Saati")]
        [DataType(DataType.Time)]
        public TimeSpan BitisSaati { get; set; }

        // =====================================================
        // MÜSAİT Mİ?
        // Bu zaman diliminde randevu alınabilir mi?
        // false ise antrenör izinli veya meşgul demektir
        // =====================================================
        [Display(Name = "Müsait mi?")]
        public bool MusaitMi { get; set; } = true;

        // =====================================================
        // NOT / AÇIKLAMA
        // Müsaitlikle ilgili ek bilgi (örn: "Sadece grup dersi")
        // =====================================================
        [StringLength(200, ErrorMessage = "Not en fazla 200 karakter olabilir")]
        [Display(Name = "Not")]
        public string? Not { get; set; }

        // =====================================================
        // NAVİGASYON ÖZELLİKLERİ
        // =====================================================

        /// <summary>
        /// Bu müsaitlik kaydının ait olduğu antrenör
        /// Foreign key ilişkisi
        /// </summary>
        [ForeignKey("AntrenorId")]
        public virtual Antrenor? Antrenor { get; set; }
    }

    // =====================================================
    // GÜN ADLARI YARDIMCI SINIFI
    // DayOfWeek enum'ının Türkçe karşılıkları için
    // =====================================================
    /// <summary>
    /// Haftanın günlerinin Türkçe karşılıklarını döndüren yardımcı sınıf
    /// </summary>
    public static class GunAdlari
    {
        /// <summary>
        /// DayOfWeek enum değerini Türkçe gün adına çevirir
        /// </summary>
        /// <param name="gun">Haftanın günü (DayOfWeek enum)</param>
        /// <returns>Türkçe gün adı</returns>
        public static string TurkceAd(this DayOfWeek gun)
        {
            // Switch expression ile gün adını döndür
            return gun switch
            {
                DayOfWeek.Monday => "Pazartesi",
                DayOfWeek.Tuesday => "Salı",
                DayOfWeek.Wednesday => "Çarşamba",
                DayOfWeek.Thursday => "Perşembe",
                DayOfWeek.Friday => "Cuma",
                DayOfWeek.Saturday => "Cumartesi",
                DayOfWeek.Sunday => "Pazar",
                _ => gun.ToString() // Bilinmeyen değer için enum adını döndür
            };
        }

        /// <summary>
        /// Tüm günlerin listesini Türkçe adlarıyla döndürür
        /// Dropdown listeler için kullanışlıdır
        /// </summary>
        /// <returns>Gün numarası ve Türkçe adı içeren liste</returns>
        public static List<(DayOfWeek Gun, string Ad)> TumGunler()
        {
            return new List<(DayOfWeek, string)>
            {
                (DayOfWeek.Monday, "Pazartesi"),
                (DayOfWeek.Tuesday, "Salı"),
                (DayOfWeek.Wednesday, "Çarşamba"),
                (DayOfWeek.Thursday, "Perşembe"),
                (DayOfWeek.Friday, "Cuma"),
                (DayOfWeek.Saturday, "Cumartesi"),
                (DayOfWeek.Sunday, "Pazar")
            };
        }
    }
}

