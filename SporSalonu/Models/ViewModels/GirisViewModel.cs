// =====================================================
// GİRİŞ (LOGIN) VİEW MODEL SINIFI
// Bu dosya, kullanıcı giriş formunda kullanılacak
// view model sınıfını tanımlar
// =====================================================

using System.ComponentModel.DataAnnotations;

namespace SporSalonu.Models.ViewModels
{
    /// <summary>
    /// GirisViewModel - Kullanıcı girişi için kullanılan view model
    /// E-posta ve şifre bilgilerini alır
    /// </summary>
    public class GirisViewModel
    {
        /// <summary>
        /// Kullanıcının e-posta adresi
        /// Sisteme giriş için kullanılır
        /// </summary>
        [Required(ErrorMessage = "E-posta adresi zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Kullanıcının şifresi
        /// </summary>
        [Required(ErrorMessage = "Şifre zorunludur")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Sifre { get; set; } = string.Empty;

        /// <summary>
        /// Beni hatırla seçeneği
        /// true ise kullanıcı oturumu uzun süre açık kalır
        /// </summary>
        [Display(Name = "Beni Hatırla")]
        public bool BeniHatirla { get; set; }

        /// <summary>
        /// Giriş sonrası yönlendirilecek URL
        /// Kullanıcı korumalı sayfaya erişmeye çalıştıysa o sayfaya yönlendirilir
        /// </summary>
        public string? ReturnUrl { get; set; }
    }
}

