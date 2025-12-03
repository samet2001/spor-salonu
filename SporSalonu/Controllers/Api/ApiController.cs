// =====================================================
// REST API CONTROLLER SINIFI
// Bu dosya, RESTful API endpoint'lerini tanımlar
// LINQ sorguları ile veritabanı işlemleri yapılır
// Antrenör, Hizmet, Randevu ve üye verilerini JSON olarak döndürür
// =====================================================

using Microsoft.AspNetCore.Authorization;         // Yetkilendirme
using Microsoft.AspNetCore.Mvc;                   // MVC
using Microsoft.EntityFrameworkCore;              // Entity Framework
using SporSalonu.Data;                            // DbContext
using SporSalonu.Models;                          // Model sınıfları

namespace SporSalonu.Controllers.Api
{
    // =====================================================
    // API CONTROLLER
    // Route: /api/sporsalonu
    // =====================================================
    /// <summary>
    /// REST API Controller - Veritabanı verilerine JSON erişimi sağlar
    /// LINQ sorguları ile filtreleme ve raporlama yapılır
    /// </summary>
    [ApiController]                               // API controller olarak işaretle
    [Route("api/sporsalonu")]                     // Temel route
    public class ApiController : ControllerBase
    {
        // =====================================================
        // BAĞIMLILIKLAR
        // =====================================================
        
        private readonly ApplicationDbContext _context;   // Veritabanı bağlamı
        private readonly ILogger<ApiController> _logger;  // Loglama

        // =====================================================
        // CONSTRUCTOR
        // =====================================================
        public ApiController(
            ApplicationDbContext context,
            ILogger<ApiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // =====================================================
        // TÜM ANTRENÖRLERİ LİSTELE
        // GET: /api/sporsalonu/antrenorler
        // LINQ: Tüm aktif antrenörleri döndürür
        // =====================================================
        /// <summary>
        /// Tüm aktif antrenörleri listeler
        /// </summary>
        /// <returns>Antrenör listesi (JSON)</returns>
        [HttpGet("antrenorler")]
        public async Task<IActionResult> TumAntrenorleriGetir()
        {
            _logger.LogInformation("API: Tüm antrenörler istendi");

            // LINQ sorgusu: Tüm aktif antrenörleri seç
            var antrenorler = await _context.Antrenorler
                .Where(a => a.Aktif)                      // Aktif olanları filtrele
                .Include(a => a.SporSalonu)               // Salon bilgisini dahil et
                .Include(a => a.AntrenorHizmetleri)       // Hizmet ilişkilerini dahil et
                    .ThenInclude(ah => ah.Hizmet)         // Hizmet detaylarını dahil et
                .OrderBy(a => a.Ad)                       // Ada göre sırala
                .Select(a => new                          // DTO'ya dönüştür (döngüsel referansları önle)
                {
                    a.Id,
                    a.Ad,
                    a.Soyad,
                    TamIsim = a.Ad + " " + a.Soyad,
                    a.Eposta,
                    a.Telefon,
                    a.UzmanlikAlanlari,
                    a.Biyografi,
                    a.MesaiBaslangic,
                    a.MesaiBitis,
                    a.SeansUcreti,
                    a.DeneyimYili,
                    a.FotografUrl,
                    SporSalonu = a.SporSalonu != null ? a.SporSalonu.Ad : null,
                    Hizmetler = a.AntrenorHizmetleri
                        .Where(ah => ah.Hizmet != null && ah.Hizmet.Aktif)
                        .Select(ah => new { ah.Hizmet!.Id, ah.Hizmet.Ad, ah.Hizmet.Kategori })
                        .ToList()
                })
                .ToListAsync();

            // Toplam sayı ile birlikte döndür
            return Ok(new 
            { 
                Basarili = true,
                ToplamKayit = antrenorler.Count,
                Veriler = antrenorler 
            });
        }

        // =====================================================
        // ANTRENÖR DETAYI
        // GET: /api/sporsalonu/antrenorler/5
        // LINQ: ID'ye göre antrenör detayı
        // =====================================================
        /// <summary>
        /// Belirli bir antrenörün detaylarını döndürür
        /// </summary>
        /// <param name="id">Antrenör ID</param>
        [HttpGet("antrenorler/{id}")]
        public async Task<IActionResult> AntrenorDetay(int id)
        {
            // LINQ: ID'ye göre antrenör bul
            var antrenor = await _context.Antrenorler
                .Where(a => a.Id == id && a.Aktif)
                .Include(a => a.SporSalonu)
                .Include(a => a.AntrenorHizmetleri)
                    .ThenInclude(ah => ah.Hizmet)
                .Include(a => a.Musaitlikler)
                .Select(a => new
                {
                    a.Id,
                    a.Ad,
                    a.Soyad,
                    TamIsim = a.Ad + " " + a.Soyad,
                    a.Eposta,
                    a.Telefon,
                    a.UzmanlikAlanlari,
                    a.Biyografi,
                    MesaiBaslangic = a.MesaiBaslangic.ToString(@"hh\:mm"),
                    MesaiBitis = a.MesaiBitis.ToString(@"hh\:mm"),
                    a.SeansUcreti,
                    a.DeneyimYili,
                    a.FotografUrl,
                    SporSalonu = a.SporSalonu != null ? new { a.SporSalonu.Id, a.SporSalonu.Ad } : null,
                    Hizmetler = a.AntrenorHizmetleri
                        .Where(ah => ah.Hizmet != null)
                        .Select(ah => new { ah.Hizmet!.Id, ah.Hizmet.Ad, ah.Hizmet.Ucret })
                        .ToList(),
                    Musaitlikler = a.Musaitlikler
                        .Where(m => m.MusaitMi)
                        .Select(m => new 
                        { 
                            m.Gun, 
                            GunAdi = m.Gun.TurkceAd(),
                            Baslangic = m.BaslangicSaati.ToString(@"hh\:mm"),
                            Bitis = m.BitisSaati.ToString(@"hh\:mm")
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (antrenor == null)
            {
                return NotFound(new { Basarili = false, Mesaj = "Antrenör bulunamadı" });
            }

            return Ok(new { Basarili = true, Veri = antrenor });
        }

        // =====================================================
        // BELİRLİ TARİHTE MÜSAİT ANTRENÖRLER
        // GET: /api/sporsalonu/antrenorler/musait?tarih=2024-01-15
        // LINQ: Tarih filtrelemesi ile müsait antrenörler
        // =====================================================
        /// <summary>
        /// Belirli bir tarihte müsait olan antrenörleri listeler
        /// </summary>
        /// <param name="tarih">Tarih (yyyy-MM-dd formatında)</param>
        [HttpGet("antrenorler/musait")]
        public async Task<IActionResult> MusaitAntrenorler([FromQuery] DateTime tarih)
        {
            _logger.LogInformation("API: {Tarih} tarihi için müsait antrenörler istendi", tarih);

            // Haftanın günü
            var gun = tarih.DayOfWeek;

            // LINQ: Belirtilen gün müsait olan antrenörleri getir
            var musaitAntrenorler = await _context.Antrenorler
                .Where(a => a.Aktif)                                      // Aktif antrenörler
                .Where(a => a.Musaitlikler.Any(m =>                       // Müsaitlik kaydı olan
                    m.Gun == gun && m.MusaitMi))                          // O gün müsait
                .Include(a => a.Musaitlikler.Where(m => m.Gun == gun))    // Sadece o günün müsaitliği
                .Select(a => new
                {
                    a.Id,
                    a.Ad,
                    a.Soyad,
                    TamIsim = a.Ad + " " + a.Soyad,
                    a.UzmanlikAlanlari,
                    a.SeansUcreti,
                    Musaitlik = a.Musaitlikler
                        .Where(m => m.Gun == gun && m.MusaitMi)
                        .Select(m => new
                        {
                            Baslangic = m.BaslangicSaati.ToString(@"hh\:mm"),
                            Bitis = m.BitisSaati.ToString(@"hh\:mm")
                        })
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(new
            {
                Basarili = true,
                Tarih = tarih.ToString("yyyy-MM-dd"),
                Gun = gun.TurkceAd(),
                ToplamKayit = musaitAntrenorler.Count,
                Veriler = musaitAntrenorler
            });
        }

        // =====================================================
        // TÜM HİZMETLERİ LİSTELE
        // GET: /api/sporsalonu/hizmetler
        // LINQ: Kategori filtrelemesi destekli
        // =====================================================
        /// <summary>
        /// Tüm hizmetleri listeler, opsiyonel kategori filtresi
        /// </summary>
        /// <param name="kategori">Opsiyonel: Hizmet kategorisi</param>
        [HttpGet("hizmetler")]
        public async Task<IActionResult> TumHizmetleriGetir([FromQuery] HizmetKategorisi? kategori = null)
        {
            // LINQ: Hizmetleri filtrele
            var sorgu = _context.Hizmetler
                .Where(h => h.Aktif);                     // Aktif hizmetler

            // Kategori filtresi varsa uygula
            if (kategori.HasValue)
            {
                sorgu = sorgu.Where(h => h.Kategori == kategori.Value);
            }

            // Sorguyu çalıştır
            var hizmetler = await sorgu
                .Include(h => h.SporSalonu)
                .OrderBy(h => h.Kategori)
                .ThenBy(h => h.Ad)
                .Select(h => new
                {
                    h.Id,
                    h.Ad,
                    h.Aciklama,
                    h.SureDakika,
                    h.Ucret,
                    Kategori = h.Kategori.ToString(),
                    h.MaksimumKatilimci,
                    SporSalonu = h.SporSalonu != null ? h.SporSalonu.Ad : null
                })
                .ToListAsync();

            return Ok(new
            {
                Basarili = true,
                Filtre = kategori?.ToString(),
                ToplamKayit = hizmetler.Count,
                Veriler = hizmetler
            });
        }

        // =====================================================
        // ÜYE RANDEVULARI
        // GET: /api/sporsalonu/randevular/uye/{uyeId}
        // LINQ: Üye ID'sine göre randevuları listele
        // =====================================================
        /// <summary>
        /// Belirli bir üyenin randevularını listeler
        /// </summary>
        /// <param name="uyeId">Üye ID</param>
        [HttpGet("randevular/uye/{uyeId}")]
        [Authorize] // Yetkilendirme gerekli
        public async Task<IActionResult> UyeRandevulari(string uyeId)
        {
            // LINQ: Üyenin randevularını getir
            var randevular = await _context.Randevular
                .Where(r => r.UyeId == uyeId)             // Üye filtresi
                .Include(r => r.Antrenor)                 // Antrenör bilgisi
                .Include(r => r.Hizmet)                   // Hizmet bilgisi
                .OrderByDescending(r => r.RandevuTarihi)  // Tarihe göre sırala (yeniden eskiye)
                .Select(r => new
                {
                    r.Id,
                    Tarih = r.RandevuTarihi.ToString("yyyy-MM-dd"),
                    BaslangicSaati = r.BaslangicSaati.ToString(@"hh\:mm"),
                    BitisSaati = r.BitisSaati.ToString(@"hh\:mm"),
                    r.Durum,
                    DurumAdi = r.Durum.ToString(),
                    r.Ucret,
                    Antrenor = r.Antrenor != null ? new { r.Antrenor.Id, TamIsim = r.Antrenor.Ad + " " + r.Antrenor.Soyad } : null,
                    Hizmet = r.Hizmet != null ? new { r.Hizmet.Id, r.Hizmet.Ad } : null,
                    r.UyeNotu,
                    r.AntrenorNotu,
                    r.OlusturmaTarihi
                })
                .ToListAsync();

            return Ok(new
            {
                Basarili = true,
                UyeId = uyeId,
                ToplamKayit = randevular.Count,
                Veriler = randevular
            });
        }

        // =====================================================
        // ANTRENÖR RANDEVULARI
        // GET: /api/sporsalonu/randevular/antrenor/{antrenorId}
        // LINQ: Antrenör ID ve tarih aralığına göre filtrele
        // =====================================================
        /// <summary>
        /// Antrenörün randevularını listeler
        /// </summary>
        /// <param name="antrenorId">Antrenör ID</param>
        /// <param name="baslangic">Başlangıç tarihi (opsiyonel)</param>
        /// <param name="bitis">Bitiş tarihi (opsiyonel)</param>
        [HttpGet("randevular/antrenor/{antrenorId}")]
        public async Task<IActionResult> AntrenorRandevulari(
            int antrenorId,
            [FromQuery] DateTime? baslangic = null,
            [FromQuery] DateTime? bitis = null)
        {
            // LINQ sorgusu oluştur
            var sorgu = _context.Randevular
                .Where(r => r.AntrenorId == antrenorId)
                .Where(r => r.Durum != RandevuDurumu.IptalEdildi); // İptal edilenler hariç

            // Tarih filtreleri
            if (baslangic.HasValue)
            {
                sorgu = sorgu.Where(r => r.RandevuTarihi >= baslangic.Value.Date);
            }
            if (bitis.HasValue)
            {
                sorgu = sorgu.Where(r => r.RandevuTarihi <= bitis.Value.Date);
            }

            // Sorguyu çalıştır
            var randevular = await sorgu
                .Include(r => r.Uye)
                .Include(r => r.Hizmet)
                .OrderBy(r => r.RandevuTarihi)
                .ThenBy(r => r.BaslangicSaati)
                .Select(r => new
                {
                    r.Id,
                    Tarih = r.RandevuTarihi.ToString("yyyy-MM-dd"),
                    BaslangicSaati = r.BaslangicSaati.ToString(@"hh\:mm"),
                    BitisSaati = r.BitisSaati.ToString(@"hh\:mm"),
                    DurumAdi = r.Durum.ToString(),
                    Uye = r.Uye != null ? new { r.Uye.Id, TamIsim = r.Uye.Ad + " " + r.Uye.Soyad } : null,
                    Hizmet = r.Hizmet != null ? r.Hizmet.Ad : null
                })
                .ToListAsync();

            return Ok(new
            {
                Basarili = true,
                AntrenorId = antrenorId,
                FiltreBas = baslangic?.ToString("yyyy-MM-dd"),
                FiltreBit = bitis?.ToString("yyyy-MM-dd"),
                ToplamKayit = randevular.Count,
                Veriler = randevular
            });
        }

        // =====================================================
        // RAPOR: GÜNLÜK RANDEVU İSTATİSTİKLERİ
        // GET: /api/sporsalonu/raporlar/gunluk?tarih=2024-01-15
        // LINQ: Gruplama ve istatistik hesaplama
        // =====================================================
        /// <summary>
        /// Günlük randevu istatistiklerini döndürür
        /// </summary>
        /// <param name="tarih">Tarih (varsayılan: bugün)</param>
        [HttpGet("raporlar/gunluk")]
        [Authorize(Roles = "Admin")] // Sadece adminler erişebilir
        public async Task<IActionResult> GunlukRapor([FromQuery] DateTime? tarih = null)
        {
            var raporTarihi = tarih ?? DateTime.UtcNow.Date; // UTC zamanı kullan (PostgreSQL uyumlu)

            // LINQ: Günlük randevu istatistikleri
            var randevular = await _context.Randevular
                .Where(r => r.RandevuTarihi.Date == raporTarihi.Date)
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .ToListAsync();

            // Durumlara göre grupla
            var durumGruplari = randevular
                .GroupBy(r => r.Durum)
                .Select(g => new
                {
                    Durum = g.Key.ToString(),
                    Adet = g.Count()
                })
                .ToList();

            // Antrenörlere göre grupla
            var antrenorGruplari = randevular
                .Where(r => r.Antrenor != null)
                .GroupBy(r => new { r.AntrenorId, TamIsim = r.Antrenor!.Ad + " " + r.Antrenor.Soyad })
                .Select(g => new
                {
                    g.Key.AntrenorId,
                    g.Key.TamIsim,
                    RandevuSayisi = g.Count(),
                    ToplamKazanc = g.Sum(r => r.Ucret)
                })
                .OrderByDescending(x => x.RandevuSayisi)
                .ToList();

            // Hizmetlere göre grupla
            var hizmetGruplari = randevular
                .Where(r => r.Hizmet != null)
                .GroupBy(r => new { r.HizmetId, r.Hizmet!.Ad })
                .Select(g => new
                {
                    g.Key.HizmetId,
                    HizmetAdi = g.Key.Ad,
                    Adet = g.Count()
                })
                .OrderByDescending(x => x.Adet)
                .ToList();

            return Ok(new
            {
                Basarili = true,
                Tarih = raporTarihi.ToString("yyyy-MM-dd"),
                Gun = raporTarihi.DayOfWeek.TurkceAd(),
                Ozet = new
                {
                    ToplamRandevu = randevular.Count,
                    ToplamKazanc = randevular.Where(r => r.Durum == RandevuDurumu.Tamamlandi).Sum(r => r.Ucret),
                    BekleyenRandevu = randevular.Count(r => r.Durum == RandevuDurumu.Beklemede),
                    OnaylananRandevu = randevular.Count(r => r.Durum == RandevuDurumu.Onaylandi),
                    IptalEdilen = randevular.Count(r => r.Durum == RandevuDurumu.IptalEdildi)
                },
                DurumaGore = durumGruplari,
                AntrenoreGore = antrenorGruplari,
                HizmeteGore = hizmetGruplari
            });
        }

        // =====================================================
        // ARAMA
        // GET: /api/sporsalonu/ara?q=yoga
        // LINQ: Metin araması
        // =====================================================
        /// <summary>
        /// Antrenör ve hizmetlerde arama yapar
        /// </summary>
        /// <param name="q">Arama terimi</param>
        [HttpGet("ara")]
        public async Task<IActionResult> Ara([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            {
                return BadRequest(new { Basarili = false, Mesaj = "Arama terimi en az 2 karakter olmalıdır" });
            }

            var aramaTerimi = q.ToLower();

            // LINQ: Antrenörlerde ara
            var antrenorler = await _context.Antrenorler
                .Where(a => a.Aktif)
                .Where(a => a.Ad.ToLower().Contains(aramaTerimi) ||
                           a.Soyad.ToLower().Contains(aramaTerimi) ||
                           a.UzmanlikAlanlari.ToLower().Contains(aramaTerimi))
                .Select(a => new
                {
                    Tip = "Antrenör",
                    a.Id,
                    Baslik = a.Ad + " " + a.Soyad,
                    Aciklama = a.UzmanlikAlanlari
                })
                .Take(5)
                .ToListAsync();

            // LINQ: Hizmetlerde ara
            var hizmetler = await _context.Hizmetler
                .Where(h => h.Aktif)
                .Where(h => h.Ad.ToLower().Contains(aramaTerimi) ||
                           (h.Aciklama != null && h.Aciklama.ToLower().Contains(aramaTerimi)))
                .Select(h => new
                {
                    Tip = "Hizmet",
                    h.Id,
                    Baslik = h.Ad,
                    Aciklama = h.Aciklama ?? ""
                })
                .Take(5)
                .ToListAsync();

            // Sonuçları birleştir
            var sonuclar = antrenorler.Cast<object>().Concat(hizmetler.Cast<object>()).ToList();

            return Ok(new
            {
                Basarili = true,
                AramaTerimi = q,
                ToplamSonuc = sonuclar.Count,
                Sonuclar = sonuclar
            });
        }

        // =====================================================
        // SPOR SALONU BİLGİLERİ
        // GET: /api/sporsalonu/bilgi
        // =====================================================
        /// <summary>
        /// Spor salonu genel bilgilerini döndürür
        /// </summary>
        [HttpGet("bilgi")]
        public async Task<IActionResult> SalonBilgisi()
        {
            var salon = await _context.SporSalonlari
                .Where(s => s.Aktif)
                .Select(s => new
                {
                    s.Id,
                    s.Ad,
                    s.Adres,
                    s.Telefon,
                    s.Eposta,
                    AcilisSaati = s.AcilisSaati.ToString(@"hh\:mm"),
                    KapanisSaati = s.KapanisSaati.ToString(@"hh\:mm"),
                    s.Aciklama,
                    s.ResimUrl
                })
                .FirstOrDefaultAsync();

            // İstatistikler
            var istatistikler = new
            {
                ToplamAntrenor = await _context.Antrenorler.CountAsync(a => a.Aktif),
                ToplamHizmet = await _context.Hizmetler.CountAsync(h => h.Aktif),
                ToplamUye = await _context.Users.CountAsync(),
                BugunRandevu = await _context.Randevular.CountAsync(r => r.RandevuTarihi.Date == DateTime.UtcNow.Date)
            };

            return Ok(new
            {
                Basarili = true,
                Salon = salon,
                Istatistikler = istatistikler
            });
        }
    }
}

