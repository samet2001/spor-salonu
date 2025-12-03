// =====================================================
// YAPAY ZEKA ÖNERİ VİEW MODEL SINIFI
// Bu dosya, yapay zeka tabanlı egzersiz ve diyet
// önerisi alma işlemlerinde kullanılır
// Kullanıcının fiziksel bilgilerini alır ve öneriler sunar
// =====================================================

using System.ComponentModel.DataAnnotations;

namespace SporSalonu.Models.ViewModels
{
    /// <summary>
    /// AIOneriIstek - Yapay zekadan öneri almak için gönderilen istek modeli
    /// Kullanıcının fiziksel özellikleri ve hedeflerini içerir
    /// </summary>
    public class AIOneriIstek
    {
        // =====================================================
        // FİZİKSEL BİLGİLER
        // =====================================================

        /// <summary>
        /// Kullanıcının yaşı
        /// </summary>
        [Required(ErrorMessage = "Yaş zorunludur")]
        [Range(10, 100, ErrorMessage = "Yaş 10-100 arasında olmalıdır")]
        [Display(Name = "Yaş")]
        public int Yas { get; set; }

        /// <summary>
        /// Kullanıcının cinsiyeti
        /// </summary>
        [Required(ErrorMessage = "Cinsiyet seçimi zorunludur")]
        [Display(Name = "Cinsiyet")]
        public Cinsiyet Cinsiyet { get; set; }

        /// <summary>
        /// Kullanıcının boyu (cm)
        /// </summary>
        [Required(ErrorMessage = "Boy zorunludur")]
        [Range(100, 250, ErrorMessage = "Boy 100-250 cm arasında olmalıdır")]
        [Display(Name = "Boy (cm)")]
        public int Boy { get; set; }

        /// <summary>
        /// Kullanıcının kilosu (kg)
        /// </summary>
        [Required(ErrorMessage = "Kilo zorunludur")]
        [Range(30, 300, ErrorMessage = "Kilo 30-300 kg arasında olmalıdır")]
        [Display(Name = "Kilo (kg)")]
        public decimal Kilo { get; set; }

        // =====================================================
        // HEDEF VE DENEYİM BİLGİLERİ
        // =====================================================

        /// <summary>
        /// Kullanıcının fitness hedefi
        /// </summary>
        [Required(ErrorMessage = "Fitness hedefi seçimi zorunludur")]
        [Display(Name = "Hedefiniz")]
        public FitnessHedefi FitnessHedefi { get; set; }

        /// <summary>
        /// Kullanıcının spor deneyimi
        /// </summary>
        [Required(ErrorMessage = "Deneyim seviyesi seçimi zorunludur")]
        [Display(Name = "Deneyim Seviyesi")]
        public DeneyimSeviyesi DeneyimSeviyesi { get; set; }

        /// <summary>
        /// Haftalık spor yapabilme süresi (gün)
        /// </summary>
        [Required(ErrorMessage = "Haftalık gün sayısı zorunludur")]
        [Range(1, 7, ErrorMessage = "Haftalık gün sayısı 1-7 arasında olmalıdır")]
        [Display(Name = "Haftada Kaç Gün Spor Yapabilirsiniz?")]
        public int HaftalikGun { get; set; } = 3;

        /// <summary>
        /// Günlük antrenman süresi (dakika)
        /// </summary>
        [Required(ErrorMessage = "Günlük süre zorunludur")]
        [Range(15, 180, ErrorMessage = "Günlük süre 15-180 dakika arasında olmalıdır")]
        [Display(Name = "Günlük Antrenman Süresi (dk)")]
        public int GunlukSureDakika { get; set; } = 60;

        // =====================================================
        // EK BİLGİLER
        // =====================================================

        /// <summary>
        /// Kullanıcının sağlık sorunları veya kısıtlamaları
        /// </summary>
        [StringLength(500, ErrorMessage = "Sağlık notu en fazla 500 karakter olabilir")]
        [Display(Name = "Sağlık Durumu / Kısıtlamalar")]
        [DataType(DataType.MultilineText)]
        public string? SaglikNotu { get; set; }

        /// <summary>
        /// Önerilerde odaklanılacak vücut bölgesi
        /// </summary>
        [Display(Name = "Odak Bölge")]
        public VucutBolgesi? OdakBolge { get; set; }

        /// <summary>
        /// Hangi tür öneri isteniyor?
        /// </summary>
        [Required(ErrorMessage = "Öneri türü seçimi zorunludur")]
        [Display(Name = "Öneri Türü")]
        public OneriTuru OneriTuru { get; set; } = OneriTuru.HerIkisi;

        // =====================================================
        // HESAPLANAN ALANLAR
        // =====================================================

        /// <summary>
        /// BMI hesaplama
        /// </summary>
        public decimal BMI => Math.Round(Kilo / ((Boy / 100m) * (Boy / 100m)), 2);

        /// <summary>
        /// BMI kategorisi
        /// </summary>
        public string BMIKategorisi
        {
            get
            {
                if (BMI < 18.5m) return "Zayıf";
                if (BMI < 25m) return "Normal";
                if (BMI < 30m) return "Fazla Kilolu";
                return "Obez";
            }
        }
    }

    /// <summary>
    /// AIOneriSonuc - Yapay zekadan gelen öneri sonucunu içerir
    /// </summary>
    public class AIOneriSonuc
    {
        /// <summary>
        /// İşlem başarılı mı?
        /// </summary>
        public bool Basarili { get; set; }

        /// <summary>
        /// Hata mesajı (başarısız ise)
        /// </summary>
        public string? HataMesaji { get; set; }

        /// <summary>
        /// Egzersiz programı önerisi
        /// </summary>
        public string? EgzersizOnerisi { get; set; }

        /// <summary>
        /// Diyet/beslenme önerisi
        /// </summary>
        public string? DiyetOnerisi { get; set; }

        /// <summary>
        /// Genel tavsiyeler
        /// </summary>
        public string? GenelTavsiyeler { get; set; }

        /// <summary>
        /// BMI değeri ve yorumu
        /// </summary>
        public string? BMIYorumu { get; set; }

        /// <summary>
        /// Önerinin oluşturulma tarihi (UTC formatında - PostgreSQL uyumlu)
        /// </summary>
        public DateTime OlusturmaTarihi { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Kullanıcının girdiği bilgiler (referans için)
        /// </summary>
        public AIOneriIstek? Istek { get; set; }
    }

    /// <summary>
    /// AIOneriViewModel - AI öneri sayfasında kullanılan view model
    /// İstek ve sonucu bir arada tutar
    /// </summary>
    public class AIOneriViewModel
    {
        /// <summary>
        /// Kullanıcının girdiği bilgiler
        /// </summary>
        public AIOneriIstek Istek { get; set; } = new AIOneriIstek();

        /// <summary>
        /// AI'dan gelen öneri sonucu
        /// </summary>
        public AIOneriSonuc? Sonuc { get; set; }

        /// <summary>
        /// Form gönderildi mi?
        /// </summary>
        public bool FormGonderildi { get; set; }
    }

    // =====================================================
    // YARDIMCI ENUM'LAR
    // =====================================================

    /// <summary>
    /// Vücut bölgeleri
    /// </summary>
    public enum VucutBolgesi
    {
        [Display(Name = "Tüm Vücut")]
        TumVucut = 1,

        [Display(Name = "Üst Vücut")]
        UstVucut = 2,

        [Display(Name = "Alt Vücut")]
        AltVucut = 3,

        [Display(Name = "Karın")]
        Karin = 4,

        [Display(Name = "Sırt")]
        Sirt = 5,

        [Display(Name = "Göğüs")]
        Gogus = 6,

        [Display(Name = "Kol")]
        Kol = 7,

        [Display(Name = "Bacak")]
        Bacak = 8
    }

    /// <summary>
    /// Öneri türleri
    /// </summary>
    public enum OneriTuru
    {
        [Display(Name = "Sadece Egzersiz")]
        Egzersiz = 1,

        [Display(Name = "Sadece Beslenme")]
        Beslenme = 2,

        [Display(Name = "Egzersiz + Beslenme")]
        HerIkisi = 3
    }
}

