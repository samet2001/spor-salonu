// =====================================================
// PROFİL VİEW MODEL SINIFLARI
// Bu dosya, profil düzenleme ve şifre değiştirme
// işlemlerinde kullanılan view model sınıflarını tanımlar
// =====================================================

using System.ComponentModel.DataAnnotations;

namespace SporSalonu.Models.ViewModels
{
    /// <summary>
    /// ProfilDuzenleViewModel - Profil düzenleme formunda kullanılır
    /// Kullanıcının güncelleyebileceği tüm alanları içerir
    /// </summary>
    public class ProfilDuzenleViewModel
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
        /// Kullanıcının telefon numarası
        /// </summary>
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
        [Display(Name = "Telefon")]
        public string? Telefon { get; set; }

        // =====================================================
        // FİZİKSEL BİLGİLER
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

        // =====================================================
        // FİTNESS BİLGİLERİ
        // =====================================================

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

        /// <summary>
        /// Kullanıcının sağlık notu
        /// </summary>
        [StringLength(500, ErrorMessage = "Sağlık notu en fazla 500 karakter olabilir")]
        [Display(Name = "Sağlık Notu")]
        [DataType(DataType.MultilineText)]
        public string? SaglikNotu { get; set; }
    }

    /// <summary>
    /// SifreDegistirViewModel - Şifre değiştirme formunda kullanılır
    /// </summary>
    public class SifreDegistirViewModel
    {
        /// <summary>
        /// Kullanıcının mevcut şifresi
        /// </summary>
        [Required(ErrorMessage = "Mevcut şifre zorunludur")]
        [DataType(DataType.Password)]
        [Display(Name = "Mevcut Şifre")]
        public string MevcutSifre { get; set; } = string.Empty;

        /// <summary>
        /// Yeni şifre
        /// </summary>
        [Required(ErrorMessage = "Yeni şifre zorunludur")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Şifre en az 3 karakter olmalıdır")]
        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre")]
        public string YeniSifre { get; set; } = string.Empty;

        /// <summary>
        /// Yeni şifre tekrarı
        /// </summary>
        [Required(ErrorMessage = "Şifre tekrarı zorunludur")]
        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre Tekrarı")]
        [Compare("YeniSifre", ErrorMessage = "Şifreler eşleşmiyor")]
        public string YeniSifreTekrar { get; set; } = string.Empty;
    }
}

