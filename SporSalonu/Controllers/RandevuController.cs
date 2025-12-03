// =====================================================
// RANDEVU CONTROLLER SINIFI
// Bu dosya, üyelerin randevu işlemlerini yönetir
// Randevu oluşturma, listeleme, iptal etme işlemleri
// =====================================================

using Microsoft.AspNetCore.Authorization;         // Yetkilendirme
using Microsoft.AspNetCore.Identity;              // Identity
using Microsoft.AspNetCore.Mvc;                   // MVC
using Microsoft.AspNetCore.Mvc.Rendering;         // SelectList
using Microsoft.EntityFrameworkCore;              // Entity Framework
using SporSalonu.Data;                            // DbContext
using SporSalonu.Models;                          // Model sınıfları
using SporSalonu.Models.ViewModels;               // ViewModel sınıfları

namespace SporSalonu.Controllers
{
    /// <summary>
    /// RandevuController - Üyelerin randevu işlemlerini yöneten controller
    /// Sadece giriş yapmış kullanıcılar erişebilir
    /// </summary>
    [Authorize] // Tüm action'lar için giriş zorunlu
    public class RandevuController : Controller
    {
        // =====================================================
        // BAĞIMLILIKLAR
        // =====================================================
        
        private readonly ApplicationDbContext _context;       // Veritabanı
        private readonly UserManager<Uye> _userManager;       // Kullanıcı yönetimi
        private readonly ILogger<RandevuController> _logger;  // Loglama

        // =====================================================
        // CONSTRUCTOR
        // =====================================================
        public RandevuController(
            ApplicationDbContext context,
            UserManager<Uye> userManager,
            ILogger<RandevuController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // =====================================================
        // RANDEVULARIM (LİSTE)
        // URL: /Randevu/Index
        // =====================================================
        /// <summary>
        /// Kullanıcının randevularını listeler
        /// </summary>
        public async Task<IActionResult> Index()
        {
            // Mevcut kullanıcıyı al
            var kullanici = await _userManager.GetUserAsync(User);
            
            if (kullanici == null)
            {
                return NotFound();
            }

            // Kullanıcının randevularını getir (en yeniden eskiye)
            var randevular = await _context.Randevular
                .Where(r => r.UyeId == kullanici.Id) // Sadece bu kullanıcının randevuları
                .Include(r => r.Antrenor)            // Antrenör bilgisini dahil et
                .Include(r => r.Hizmet)              // Hizmet bilgisini dahil et
                .OrderByDescending(r => r.RandevuTarihi) // Tarihe göre sırala
                    .ThenByDescending(r => r.BaslangicSaati)
                .ToListAsync();

            return View(randevular);
        }

        // =====================================================
        // RANDEVU OLUŞTUR - GET
        // URL: /Randevu/Olustur
        // =====================================================
        /// <summary>
        /// Randevu oluşturma formunu gösterir
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Olustur()
        {
            // ViewModel oluştur
            var model = new RandevuOlusturViewModel
            {
                RandevuTarihi = DateTime.UtcNow.AddDays(1), // Varsayılan yarın
                BaslangicSaati = new TimeSpan(9, 0, 0)      // Varsayılan 09:00
            };

            // Dropdown listelerini hazırla
            await HazirlaDropdownlar(model);

            return View(model);
        }

        // =====================================================
        // RANDEVU OLUŞTUR - POST
        // Randevu kaydını oluşturur
        // =====================================================
        /// <summary>
        /// Randevu formunu işler ve yeni randevu oluşturur
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Olustur(RandevuOlusturViewModel model)
        {
            if (ModelState.IsValid)
            {
                // DateTime Kind sorununu önlemek için tarihi UTC'ye çevir
                model.RandevuTarihi = DateTime.SpecifyKind(model.RandevuTarihi, DateTimeKind.Utc);
                
                // Mevcut kullanıcıyı al
                var kullanici = await _userManager.GetUserAsync(User);
                
                if (kullanici == null)
                {
                    return NotFound();
                }

                // Seçilen hizmeti al (süre için)
                var hizmet = await _context.Hizmetler.FindAsync(model.HizmetId);
                
                if (hizmet == null)
                {
                    ModelState.AddModelError("HizmetId", "Geçersiz hizmet seçimi.");
                    await HazirlaDropdownlar(model);
                    return View(model);
                }

                // Seçilen antrenörü al
                var antrenor = await _context.Antrenorler.FindAsync(model.AntrenorId);
                
                if (antrenor == null)
                {
                    ModelState.AddModelError("AntrenorId", "Geçersiz antrenör seçimi.");
                    await HazirlaDropdownlar(model);
                    return View(model);
                }

                // Bitiş saatini hesapla (başlangıç + hizmet süresi)
                var bitisSaati = model.BaslangicSaati.Add(TimeSpan.FromMinutes(hizmet.SureDakika));

                // Tarih kontrolü (geçmiş tarih seçilemez)
                var bugun = DateTime.UtcNow.Date;
                if (model.RandevuTarihi.Date < bugun)
                {
                    ModelState.AddModelError("RandevuTarihi", "Geçmiş bir tarih seçemezsiniz.");
                    await HazirlaDropdownlar(model);
                    return View(model);
                }

                // Bugün ise saat kontrolü
                var simdikiSaat = DateTime.UtcNow.TimeOfDay;
                if (model.RandevuTarihi.Date == bugun && model.BaslangicSaati <= simdikiSaat)
                {
                    ModelState.AddModelError("BaslangicSaati", "Geçmiş bir saat seçemezsiniz.");
                    await HazirlaDropdownlar(model);
                    return View(model);
                }

                // Antrenör müsaitlik kontrolü
                var gunMusaitligi = await _context.AntrenorMusaitlikleri
                    .Where(m => m.AntrenorId == model.AntrenorId 
                             && m.Gun == model.RandevuTarihi.DayOfWeek 
                             && m.MusaitMi)
                    .FirstOrDefaultAsync();

                if (gunMusaitligi == null)
                {
                    ModelState.AddModelError("", "Seçilen antrenör bu gün müsait değil.");
                    await HazirlaDropdownlar(model);
                    return View(model);
                }

                // Saat aralığı kontrolü
                if (model.BaslangicSaati < gunMusaitligi.BaslangicSaati || 
                    bitisSaati > gunMusaitligi.BitisSaati)
                {
                    ModelState.AddModelError("BaslangicSaati", 
                        $"Antrenör {gunMusaitligi.BaslangicSaati:hh\\:mm} - {gunMusaitligi.BitisSaati:hh\\:mm} saatleri arasında müsaittir.");
                    await HazirlaDropdownlar(model);
                    return View(model);
                }

                // Çakışan randevu kontrolü
                var cakisanRandevu = await _context.Randevular
                    .Where(r => r.AntrenorId == model.AntrenorId
                             && r.RandevuTarihi.Date == model.RandevuTarihi.Date
                             && r.Durum != RandevuDurumu.IptalEdildi // İptal edilenler hariç
                             && ((model.BaslangicSaati >= r.BaslangicSaati && model.BaslangicSaati < r.BitisSaati) ||
                                 (bitisSaati > r.BaslangicSaati && bitisSaati <= r.BitisSaati) ||
                                 (model.BaslangicSaati <= r.BaslangicSaati && bitisSaati >= r.BitisSaati)))
                    .FirstOrDefaultAsync();

                if (cakisanRandevu != null)
                {
                    ModelState.AddModelError("", 
                        $"Bu saatte antrenörün başka bir randevusu var. Lütfen farklı bir saat seçin.");
                    await HazirlaDropdownlar(model);
                    return View(model);
                }

                // Randevu oluştur
                var randevu = new Randevu
                {
                    UyeId = kullanici.Id,
                    AntrenorId = model.AntrenorId,
                    HizmetId = model.HizmetId,
                    RandevuTarihi = model.RandevuTarihi,
                    BaslangicSaati = model.BaslangicSaati,
                    BitisSaati = bitisSaati,
                    Ucret = hizmet.Ucret + antrenor.SeansUcreti, // Hizmet + Antrenör ücreti
                    UyeNotu = model.UyeNotu,
                    Durum = RandevuDurumu.Beklemede, // Onay bekliyor
                    OlusturmaTarihi = DateTime.UtcNow
                };

                // Veritabanına ekle
                _context.Randevular.Add(randevu);
                await _context.SaveChangesAsync();

                // Log kaydı
                _logger.LogInformation("Yeni randevu oluşturuldu: {RandevuId} - Üye: {UyeId}", randevu.Id, kullanici.Id);

                // Başarı mesajı
                TempData["Basari"] = "Randevunuz başarıyla oluşturuldu. Onay için bekleyiniz.";

                return RedirectToAction(nameof(Index));
            }

            // Hata varsa formu tekrar göster
            await HazirlaDropdownlar(model);
            return View(model);
        }

        // =====================================================
        // RANDEVU DETAY
        // URL: /Randevu/Detay/5
        // =====================================================
        /// <summary>
        /// Randevu detaylarını gösterir
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Detay(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kullanici = await _userManager.GetUserAsync(User);
            
            if (kullanici == null)
            {
                return NotFound();
            }

            // Randevuyu getir
            var randevu = await _context.Randevular
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .Include(r => r.Uye)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (randevu == null)
            {
                return NotFound();
            }

            // Yetki kontrolü: Sadece kendi randevusunu veya admin görebilir
            var isAdmin = await _userManager.IsInRoleAsync(kullanici, "Admin");
            
            if (randevu.UyeId != kullanici.Id && !isAdmin)
            {
                return Forbid();
            }

            // ViewModel oluştur
            var viewModel = new RandevuDetayViewModel
            {
                Randevu = randevu,
                IptalEdilebilir = !randevu.GecmisMi && 
                                  randevu.Durum != RandevuDurumu.IptalEdildi &&
                                  randevu.Durum != RandevuDurumu.Tamamlandi,
                Duzenlenebilir = !randevu.GecmisMi && 
                                 randevu.Durum == RandevuDurumu.Beklemede
            };

            return View(viewModel);
        }

        // =====================================================
        // RANDEVU İPTAL ET
        // URL: /Randevu/IptalEt/5
        // =====================================================
        /// <summary>
        /// Randevuyu iptal eder
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IptalEt(int id, string? iptalNedeni)
        {
            var kullanici = await _userManager.GetUserAsync(User);
            
            if (kullanici == null)
            {
                return NotFound();
            }

            // Randevuyu getir
            var randevu = await _context.Randevular.FindAsync(id);

            if (randevu == null)
            {
                return NotFound();
            }

            // Yetki kontrolü
            var isAdmin = await _userManager.IsInRoleAsync(kullanici, "Admin");
            
            if (randevu.UyeId != kullanici.Id && !isAdmin)
            {
                return Forbid();
            }

            // İptal edilebilir mi kontrol et
            if (randevu.GecmisMi)
            {
                TempData["Hata"] = "Geçmiş randevular iptal edilemez.";
                return RedirectToAction(nameof(Index));
            }

            if (randevu.Durum == RandevuDurumu.IptalEdildi)
            {
                TempData["Hata"] = "Bu randevu zaten iptal edilmiş.";
                return RedirectToAction(nameof(Index));
            }

            // İptal et
            randevu.Durum = RandevuDurumu.IptalEdildi;
            randevu.IptalNedeni = iptalNedeni ?? "Üye tarafından iptal edildi";
            randevu.GuncellemeTarihi = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Log kaydı
            _logger.LogInformation("Randevu iptal edildi: {RandevuId}", randevu.Id);

            TempData["Basari"] = "Randevunuz başarıyla iptal edildi.";

            return RedirectToAction(nameof(Index));
        }

        // =====================================================
        // MÜSAİT SAATLERİ GETİR (AJAX)
        // Seçilen antrenör ve tarihe göre müsait saatleri döndürür
        // =====================================================
        /// <summary>
        /// AJAX ile müsait saatleri döndürür
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> MusaitSaatler(int antrenorId, DateTime tarih, int hizmetId)
        {
            // Tarihi UTC olarak ayarla (Kind=Unspecified hatası önleme)
            tarih = DateTime.SpecifyKind(tarih, DateTimeKind.Utc);
            
            // Hizmet süresini al
            var hizmet = await _context.Hizmetler.FindAsync(hizmetId);
            
            if (hizmet == null)
            {
                return Json(new List<string>());
            }

            // Antrenörün o gün müsaitliğini al
            var musaitlik = await _context.AntrenorMusaitlikleri
                .Where(m => m.AntrenorId == antrenorId && m.Gun == tarih.DayOfWeek && m.MusaitMi)
                .FirstOrDefaultAsync();

            if (musaitlik == null)
            {
                return Json(new List<string>());
            }

            // Mevcut randevuları al
            var mevcutRandevular = await _context.Randevular
                .Where(r => r.AntrenorId == antrenorId 
                         && r.RandevuTarihi.Date == tarih.Date
                         && r.Durum != RandevuDurumu.IptalEdildi)
                .Select(r => new { r.BaslangicSaati, r.BitisSaati })
                .ToListAsync();

            // Müsait saatleri hesapla (30 dakikalık aralıklarla)
            var musaitSaatler = new List<string>();
            var baslangic = musaitlik.BaslangicSaati;
            var bitis = musaitlik.BitisSaati.Subtract(TimeSpan.FromMinutes(hizmet.SureDakika));

            while (baslangic <= bitis)
            {
                var seansBitis = baslangic.Add(TimeSpan.FromMinutes(hizmet.SureDakika));
                
                // Bu saat boş mu kontrol et
                var uygun = !mevcutRandevular.Any(r => 
                    (baslangic >= r.BaslangicSaati && baslangic < r.BitisSaati) ||
                    (seansBitis > r.BaslangicSaati && seansBitis <= r.BitisSaati));

                // Bugün ise geçmiş saatleri atla
                var bugun = DateTime.UtcNow.Date;
                var simdikiSaat = DateTime.UtcNow.TimeOfDay;
                if (tarih.Date == bugun && baslangic <= simdikiSaat)
                {
                    uygun = false;
                }

                if (uygun)
                {
                    musaitSaatler.Add(baslangic.ToString(@"hh\:mm"));
                }

                baslangic = baslangic.Add(TimeSpan.FromMinutes(30)); // 30 dakikalık aralıklar
            }

            return Json(musaitSaatler);
        }

        // =====================================================
        // HİZMETLERİ GETİR (AJAX)
        // Seçilen antrenörün verebileceği hizmetleri döndürür
        // =====================================================
        /// <summary>
        /// AJAX ile antrenörün hizmetlerini döndürür
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> AntrenorHizmetleri(int antrenorId)
        {
            var hizmetler = await _context.AntrenorHizmetleri
                .Where(ah => ah.AntrenorId == antrenorId)
                .Include(ah => ah.Hizmet)
                .Where(ah => ah.Hizmet!.Aktif)
                .Select(ah => new 
                { 
                    id = ah.HizmetId, 
                    ad = ah.Hizmet!.Ad,
                    sure = ah.Hizmet.SureDakika,
                    ucret = ah.Hizmet.Ucret
                })
                .ToListAsync();

            return Json(hizmetler);
        }

        // =====================================================
        // YARDIMCI METODLAR
        // =====================================================

        /// <summary>
        /// Dropdown listelerini hazırlar
        /// </summary>
        private async Task HazirlaDropdownlar(RandevuOlusturViewModel model)
        {
            // Aktif antrenörleri getir
            var antrenorler = await _context.Antrenorler
                .Where(a => a.Aktif)
                .OrderBy(a => a.Ad)
                .Select(a => new { a.Id, AdSoyad = a.Ad + " " + a.Soyad })
                .ToListAsync();

            model.Antrenorler = new SelectList(antrenorler, "Id", "AdSoyad", model.AntrenorId);

            // Aktif hizmetleri getir
            var hizmetler = await _context.Hizmetler
                .Where(h => h.Aktif)
                .OrderBy(h => h.Ad)
                .Select(h => new { h.Id, h.Ad })
                .ToListAsync();

            model.Hizmetler = new SelectList(hizmetler, "Id", "Ad", model.HizmetId);
        }
    }
}

