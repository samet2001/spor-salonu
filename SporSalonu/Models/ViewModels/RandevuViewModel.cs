// =====================================================
// RANDEVU VİEW MODEL SINIFI
// Bu dosya, randevu oluşturma ve listeleme işlemleri
// için kullanılan view model sınıflarını tanımlar
// =====================================================

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering; // SelectList için

namespace SporSalonu.Models.ViewModels
{
    /// <summary>
    /// RandevuOlusturViewModel - Yeni randevu oluşturma formunda kullanılır
    /// Kullanıcının antrenör, hizmet ve zaman seçmesini sağlar
    /// </summary>
    public class RandevuOlusturViewModel
    {
        // =====================================================
        // FORM ALANLARI
        // Kullanıcıdan alınacak bilgiler
        // =====================================================

        /// <summary>
        /// Seçilen antrenörün ID'si
        /// </summary>
        [Required(ErrorMessage = "Lütfen bir antrenör seçiniz")]
        [Display(Name = "Antrenör")]
        public int AntrenorId { get; set; }

        /// <summary>
        /// Seçilen hizmetin ID'si
        /// </summary>
        [Required(ErrorMessage = "Lütfen bir hizmet seçiniz")]
        [Display(Name = "Hizmet")]
        public int HizmetId { get; set; }

        /// <summary>
        /// Randevu tarihi
        /// Bugünden önceki tarih seçilemez
        /// </summary>
        [Required(ErrorMessage = "Randevu tarihi zorunludur")]
        [Display(Name = "Tarih")]
        [DataType(DataType.Date)]
        public DateTime RandevuTarihi { get; set; } = DateTime.UtcNow.AddDays(1); // Varsayılan yarın

        /// <summary>
        /// Randevu başlangıç saati
        /// </summary>
        [Required(ErrorMessage = "Başlangıç saati zorunludur")]
        [Display(Name = "Saat")]
        [DataType(DataType.Time)]
        public TimeSpan BaslangicSaati { get; set; } = new TimeSpan(9, 0, 0); // Varsayılan 09:00

        /// <summary>
        /// Üyenin randevu için eklemek istediği not
        /// </summary>
        [StringLength(500, ErrorMessage = "Not en fazla 500 karakter olabilir")]
        [Display(Name = "Not (İsteğe bağlı)")]
        [DataType(DataType.MultilineText)]
        public string? UyeNotu { get; set; }

        // =====================================================
        // DROPDOWN LİSTELERİ
        // View'da select elementleri için kullanılır
        // =====================================================

        /// <summary>
        /// Antrenör seçimi için dropdown listesi
        /// </summary>
        public SelectList? Antrenorler { get; set; }

        /// <summary>
        /// Hizmet seçimi için dropdown listesi
        /// </summary>
        public SelectList? Hizmetler { get; set; }

        /// <summary>
        /// Müsait saatler listesi
        /// </summary>
        public List<SelectListItem>? MusaitSaatler { get; set; }
    }

    /// <summary>
    /// RandevuListeViewModel - Randevu listesi görüntülemede kullanılır
    /// Filtreleme ve sayfalama desteği sunar
    /// </summary>
    public class RandevuListeViewModel
    {
        /// <summary>
        /// Gösterilecek randevuların listesi
        /// </summary>
        public List<Randevu> Randevular { get; set; } = new List<Randevu>();

        /// <summary>
        /// Tarih filtresi başlangıç
        /// </summary>
        [Display(Name = "Başlangıç Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? BaslangicTarihi { get; set; }

        /// <summary>
        /// Tarih filtresi bitiş
        /// </summary>
        [Display(Name = "Bitiş Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? BitisTarihi { get; set; }

        /// <summary>
        /// Durum filtresi
        /// </summary>
        [Display(Name = "Durum")]
        public RandevuDurumu? DurumFiltresi { get; set; }

        /// <summary>
        /// Antrenör filtresi
        /// </summary>
        [Display(Name = "Antrenör")]
        public int? AntrenorFiltresi { get; set; }

        /// <summary>
        /// Mevcut sayfa numarası (sayfalama için)
        /// </summary>
        public int Sayfa { get; set; } = 1;

        /// <summary>
        /// Toplam randevu sayısı
        /// </summary>
        public int ToplamKayit { get; set; }

        /// <summary>
        /// Sayfa başına kayıt sayısı
        /// </summary>
        public int SayfaBasinaKayit { get; set; } = 10;

        /// <summary>
        /// Toplam sayfa sayısı
        /// </summary>
        public int ToplamSayfa => (int)Math.Ceiling((double)ToplamKayit / SayfaBasinaKayit);

        /// <summary>
        /// Antrenör listesi (filtre dropdown için)
        /// </summary>
        public SelectList? AntrenorListesi { get; set; }
    }

    /// <summary>
    /// RandevuDetayViewModel - Randevu detay sayfasında kullanılır
    /// </summary>
    public class RandevuDetayViewModel
    {
        /// <summary>
        /// Randevu bilgileri
        /// </summary>
        public Randevu Randevu { get; set; } = null!;

        /// <summary>
        /// Randevu iptal edilebilir mi?
        /// (Geçmiş randevular iptal edilemez)
        /// </summary>
        public bool IptalEdilebilir { get; set; }

        /// <summary>
        /// Randevu düzenlenebilir mi?
        /// </summary>
        public bool Duzenlenebilir { get; set; }
    }

    /// <summary>
    /// RandevuOnayViewModel - Admin randevu onaylama için kullanılır
    /// </summary>
    public class RandevuOnayViewModel
    {
        /// <summary>
        /// Randevu ID'si
        /// </summary>
        public int RandevuId { get; set; }

        /// <summary>
        /// Yeni durum
        /// </summary>
        [Required(ErrorMessage = "Durum seçimi zorunludur")]
        [Display(Name = "Durum")]
        public RandevuDurumu YeniDurum { get; set; }

        /// <summary>
        /// Antrenör notu
        /// </summary>
        [StringLength(500)]
        [Display(Name = "Not")]
        public string? AntrenorNotu { get; set; }

        /// <summary>
        /// İptal nedeni (iptal durumunda zorunlu)
        /// </summary>
        [StringLength(300)]
        [Display(Name = "İptal Nedeni")]
        public string? IptalNedeni { get; set; }
    }
}

