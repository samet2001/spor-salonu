// =====================================================
// KAYIT (REGISTER) VİEW MODEL SINIFI
// Bu dosya, kullanıcı kayıt formunda kullanılacak
// view model sınıfını tanımlar
// Kullanıcıdan alınacak tüm kayıt bilgileri burada bulunur
// =====================================================

using System.ComponentModel.DataAnnotations;

namespace SporSalonu.Models.ViewModels
{
    /// <summary>
    /// KayitViewModel - Yeni üye kaydı için kullanılan view model
    /// Formdan gelen verileri doğrular ve controller'a iletir
    /// </summary>
    public class KayitViewModel
    {
        // =====================================================
        // TEMEL BİLGİLER
        // =====================================================

        /// <summary>
        /// Kullanıcının adı
        /// </summary>
        [Required(ErrorMessage = "Ad zorunludur")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Ad 2-50 karakter arasında olmalıdır")]
        [Display(Name = "Ad")]
        public string Ad { get; set; } = string.Empty;

        /// <summary>
        /// Kullanıcının soyadı
        /// </summary>
        [Required(ErrorMessage = "Soyad zorunludur")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Soyad 2-50 karakter arasında olmalıdır")]
        [Display(Name = "Soyad")]
        public string Soyad { get; set; } = string.Empty;

        /// <summary>
        /// Kullanıcının e-posta adresi (aynı zamanda kullanıcı adı olarak kullanılır)
        /// </summary>
        [Required(ErrorMessage = "E-posta adresi zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Kullanıcının telefon numarası
        /// </summary>
        [Required(ErrorMessage = "Telefon numarası zorunludur")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
        [Display(Name = "Telefon")]
        public string Telefon { get; set; } = string.Empty;

        // =====================================================
        // ŞİFRE BİLGİLERİ
        // =====================================================

        /// <summary>
        /// Kullanıcının şifresi
        /// Minimum 6 karakter olmalı
        /// </summary>
        [Required(ErrorMessage = "Şifre zorunludur")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Şifre en az 3 karakter olmalıdır")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Sifre { get; set; } = string.Empty;

        /// <summary>
        /// Şifre tekrarı - Şifre ile aynı olmalı
        /// </summary>
        [Required(ErrorMessage = "Şifre tekrarı zorunludur")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre Tekrarı")]
        [Compare("Sifre", ErrorMessage = "Şifreler eşleşmiyor")] // Sifre alanı ile karşılaştır
        public string SifreTekrar { get; set; } = string.Empty;

        // =====================================================
        // FİZİKSEL BİLGİLER (OPSİYONEL)
        // =====================================================

        /// <summary>
        /// Kullanıcının doğum tarihi
        /// </summary>
        [Display(Name = "Doğum Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? DogumTarihi { get; set; }

        /// <summary>
        /// Kullanıcının cinsiyeti
        /// </summary>
        [Display(Name = "Cinsiyet")]
        public Cinsiyet? Cinsiyet { get; set; }

        /// <summary>
        /// Kullanıcının boyu (cm)
        /// </summary>
        [Range(100, 250, ErrorMessage = "Boy 100-250 cm arasında olmalıdır")]
        [Display(Name = "Boy (cm)")]
        public int? Boy { get; set; }

        /// <summary>
        /// Kullanıcının kilosu (kg)
        /// </summary>
        [Range(30, 300, ErrorMessage = "Kilo 30-300 kg arasında olmalıdır")]
        [Display(Name = "Kilo (kg)")]
        public decimal? Kilo { get; set; }

        /// <summary>
        /// Kullanıcının fitness hedefi
        /// </summary>
        [Display(Name = "Fitness Hedefi")]
        public FitnessHedefi? FitnessHedefi { get; set; }

        /// <summary>
        /// Kullanıcının deneyim seviyesi
        /// </summary>
        [Display(Name = "Deneyim Seviyesi")]
        public DeneyimSeviyesi? DeneyimSeviyesi { get; set; }
    }
}

