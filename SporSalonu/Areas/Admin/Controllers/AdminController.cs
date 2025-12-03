// =====================================================
// ADMİN CONTROLLER SINIFI
// Bu dosya, admin paneli işlemlerini yönetir
// CRUD işlemleri: Antrenör, Hizmet, Randevu yönetimi
// Sadece Admin rolündeki kullanıcılar erişebilir
// =====================================================

using Microsoft.AspNetCore.Authorization;         // Yetkilendirme
using Microsoft.AspNetCore.Identity;              // Identity
using Microsoft.AspNetCore.Mvc;                   // MVC
using Microsoft.AspNetCore.Mvc.Rendering;         // SelectList
using Microsoft.EntityFrameworkCore;              // Entity Framework
using SporSalonu.Data;                            // DbContext
using SporSalonu.Models;                          // Model sınıfları

namespace SporSalonu.Areas.Admin.Controllers
{
    // =====================================================
    // ADMİN AREA YAPILANDIRMASI
    // =====================================================
    [Area("Admin")]                                       // Admin area'sına ait
    [Authorize(Roles = "Admin")]                          // Sadece Admin rolü erişebilir
    public class AdminController : Controller
    {
        // =====================================================
        // BAĞIMLILIKLAR
        // =====================================================
        
        private readonly ApplicationDbContext _context;       // Veritabanı
        private readonly UserManager<Uye> _userManager;       // Kullanıcı yönetimi
        private readonly ILogger<AdminController> _logger;    // Loglama

        // =====================================================
        // CONSTRUCTOR
        // =====================================================
        public AdminController(
            ApplicationDbContext context,
            UserManager<Uye> userManager,
            ILogger<AdminController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // =====================================================
        // ADMIN DASHBOARD (ANA SAYFA)
        // URL: /Admin
        // =====================================================
        /// <summary>
        /// Admin paneli ana sayfası - Dashboard
        /// İstatistikleri ve özet bilgileri gösterir
        /// </summary>
        public async Task<IActionResult> Index()
        {
            // Dashboard istatistikleri
            var viewModel = new AdminDashboardViewModel
            {
                // Toplam sayılar
                ToplamUye = await _context.Users.CountAsync(),
                ToplamAntrenor = await _context.Antrenorler.CountAsync(a => a.Aktif),
                ToplamHizmet = await _context.Hizmetler.CountAsync(h => h.Aktif),
                ToplamRandevu = await _context.Randevular.CountAsync(),

                // Bugünün randevuları (UTC zamanına göre)
                BugunRandevuSayisi = await _context.Randevular
                    .CountAsync(r => r.RandevuTarihi.Date == DateTime.UtcNow.Date),

                // Bekleyen randevular (onay bekleyen)
                BekleyenRandevuSayisi = await _context.Randevular
                    .CountAsync(r => r.Durum == RandevuDurumu.Beklemede),

                // Son 5 randevu
                SonRandevular = await _context.Randevular
                    .Include(r => r.Uye)
                    .Include(r => r.Antrenor)
                    .Include(r => r.Hizmet)
                    .OrderByDescending(r => r.OlusturmaTarihi)
                    .Take(5)
                    .ToListAsync(),

                // Son 5 üye
                SonUyeler = await _context.Users
                    .OrderByDescending(u => u.UyelikTarihi)
                    .Take(5)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        // =====================================================
        // ANTRENÖR YÖNETİMİ - LİSTE
        // URL: /Admin/Antrenorler
        // =====================================================
        /// <summary>
        /// Tüm antrenörleri listeler
        /// </summary>
        public async Task<IActionResult> Antrenorler()
        {
            var antrenorler = await _context.Antrenorler
                .Include(a => a.SporSalonu)
                .OrderBy(a => a.Ad)
                .ToListAsync();

            return View(antrenorler);
        }

        // =====================================================
        // ANTRENÖR OLUŞTUR - GET
        // =====================================================
        /// <summary>
        /// Yeni antrenör oluşturma formu
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> AntrenorOlustur()
        {
            await HazirlaAntrenorDropdownlar();
            return View(new Antrenor
            {
                MesaiBaslangic = new TimeSpan(9, 0, 0),
                MesaiBitis = new TimeSpan(18, 0, 0),
                Aktif = true
            });
        }

        // =====================================================
        // ANTRENÖR OLUŞTUR - POST
        // =====================================================
        /// <summary>
        /// Yeni antrenör kaydı oluşturur
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AntrenorOlustur(Antrenor antrenor)
        {
            if (ModelState.IsValid)
            {
                antrenor.KayitTarihi = DateTime.UtcNow;
                _context.Antrenorler.Add(antrenor);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Yeni antrenör oluşturuldu: {Ad} {Soyad}", antrenor.Ad, antrenor.Soyad);
                TempData["Basari"] = "Antrenör başarıyla oluşturuldu.";

                return RedirectToAction(nameof(Antrenorler));
            }

            await HazirlaAntrenorDropdownlar();
            return View(antrenor);
        }

        // =====================================================
        // ANTRENÖR DETAY - GET
        // =====================================================
        /// <summary>
        /// Antrenör detay sayfası
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> AntrenorDetay(int? id)
        {
            if (id == null) return NotFound();

            var antrenor = await _context.Antrenorler
                .Include(a => a.SporSalonu)
                .Include(a => a.AntrenorHizmetleri)
                    .ThenInclude(ah => ah.Hizmet)
                .Include(a => a.Musaitlikler)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (antrenor == null) return NotFound();

            return View(antrenor);
        }

        // =====================================================
        // ANTRENÖR DÜZENLE - GET
        // =====================================================
        /// <summary>
        /// Antrenör düzenleme formu
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> AntrenorDuzenle(int? id)
        {
            if (id == null) return NotFound();

            var antrenor = await _context.Antrenorler.FindAsync(id);
            if (antrenor == null) return NotFound();

            await HazirlaAntrenorDropdownlar();
            return View(antrenor);
        }

        // =====================================================
        // ANTRENÖR DÜZENLE - POST
        // =====================================================
        /// <summary>
        /// Antrenör bilgilerini günceller
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AntrenorDuzenle(int id, Antrenor antrenor)
        {
            if (id != antrenor.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(antrenor);
                    await _context.SaveChangesAsync();

                    TempData["Basari"] = "Antrenör başarıyla güncellendi.";
                    return RedirectToAction(nameof(Antrenorler));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await AntrenorVarMi(antrenor.Id))
                        return NotFound();
                    throw;
                }
            }

            await HazirlaAntrenorDropdownlar();
            return View(antrenor);
        }

        // =====================================================
        // ANTRENÖR SİL
        // =====================================================
        /// <summary>
        /// Antrenörü siler (soft delete - pasif yapar)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AntrenorSil(int id)
        {
            var antrenor = await _context.Antrenorler.FindAsync(id);
            if (antrenor == null) return NotFound();

            // Soft delete: Aktif = false yap
            antrenor.Aktif = false;
            await _context.SaveChangesAsync();

            TempData["Basari"] = "Antrenör başarıyla pasif yapıldı.";
            return RedirectToAction(nameof(Antrenorler));
        }

        // =====================================================
        // HİZMET YÖNETİMİ - LİSTE
        // URL: /Admin/Hizmetler
        // =====================================================
        /// <summary>
        /// Tüm hizmetleri listeler
        /// </summary>
        public async Task<IActionResult> Hizmetler()
        {
            var hizmetler = await _context.Hizmetler
                .Include(h => h.SporSalonu)
                .OrderBy(h => h.Kategori)
                .ThenBy(h => h.Ad)
                .ToListAsync();

            return View(hizmetler);
        }

        // =====================================================
        // HİZMET OLUŞTUR - GET
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> HizmetOlustur()
        {
            await HazirlaHizmetDropdownlar();
            return View(new Hizmet { SureDakika = 60, MaksimumKatilimci = 1, Aktif = true });
        }

        // =====================================================
        // HİZMET OLUŞTUR - POST
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HizmetOlustur(Hizmet hizmet)
        {
            if (ModelState.IsValid)
            {
                _context.Hizmetler.Add(hizmet);
                await _context.SaveChangesAsync();

                TempData["Basari"] = "Hizmet başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Hizmetler));
            }

            await HazirlaHizmetDropdownlar();
            return View(hizmet);
        }

        // =====================================================
        // HİZMET DÜZENLE - GET
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> HizmetDuzenle(int? id)
        {
            if (id == null) return NotFound();

            var hizmet = await _context.Hizmetler.FindAsync(id);
            if (hizmet == null) return NotFound();

            await HazirlaHizmetDropdownlar();
            return View(hizmet);
        }

        // =====================================================
        // HİZMET DÜZENLE - POST
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HizmetDuzenle(int id, Hizmet hizmet)
        {
            if (id != hizmet.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(hizmet);
                    await _context.SaveChangesAsync();

                    TempData["Basari"] = "Hizmet başarıyla güncellendi.";
                    return RedirectToAction(nameof(Hizmetler));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await HizmetVarMi(hizmet.Id))
                        return NotFound();
                    throw;
                }
            }

            await HazirlaHizmetDropdownlar();
            return View(hizmet);
        }

        // =====================================================
        // HİZMET SİL
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HizmetSil(int id)
        {
            var hizmet = await _context.Hizmetler.FindAsync(id);
            if (hizmet == null) return NotFound();

            hizmet.Aktif = false;
            await _context.SaveChangesAsync();

            TempData["Basari"] = "Hizmet başarıyla pasif yapıldı.";
            return RedirectToAction(nameof(Hizmetler));
        }

        // =====================================================
        // RANDEVU YÖNETİMİ - LİSTE
        // URL: /Admin/Randevular
        // =====================================================
        /// <summary>
        /// Tüm randevuları listeler (filtreleme ile)
        /// </summary>
        public async Task<IActionResult> Randevular(
            DateTime? tarihBas = null,
            DateTime? tarihBit = null,
            RandevuDurumu? durum = null)
        {
            // Sorgu oluştur
            var sorgu = _context.Randevular
                .Include(r => r.Uye)
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .AsQueryable();

            // Filtreleri uygula
            if (tarihBas.HasValue)
                sorgu = sorgu.Where(r => r.RandevuTarihi >= tarihBas.Value.Date);
            
            if (tarihBit.HasValue)
                sorgu = sorgu.Where(r => r.RandevuTarihi <= tarihBit.Value.Date);
            
            if (durum.HasValue)
                sorgu = sorgu.Where(r => r.Durum == durum.Value);

            var randevular = await sorgu
                .OrderByDescending(r => r.RandevuTarihi)
                .ThenByDescending(r => r.BaslangicSaati)
                .ToListAsync();

            // Filtre değerlerini ViewBag'e aktar
            ViewBag.TarihBas = tarihBas?.ToString("yyyy-MM-dd");
            ViewBag.TarihBit = tarihBit?.ToString("yyyy-MM-dd");
            ViewBag.Durum = durum?.ToString();

            return View(randevular);
        }

        // =====================================================
        // RANDEVU ONAYLA
        // =====================================================
        /// <summary>
        /// Randevuyu onaylar
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RandevuOnayla(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu == null) return NotFound();

            randevu.Durum = RandevuDurumu.Onaylandi;
            randevu.GuncellemeTarihi = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["Basari"] = "Randevu onaylandı.";
            return RedirectToAction(nameof(Randevular));
        }

        // =====================================================
        // RANDEVU REDDETİ
        // =====================================================
        /// <summary>
        /// Randevuyu reddeder/iptal eder
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RandevuReddet(int id, string? iptalNedeni)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu == null) return NotFound();

            randevu.Durum = RandevuDurumu.IptalEdildi;
            randevu.IptalNedeni = iptalNedeni ?? "Admin tarafından iptal edildi";
            randevu.GuncellemeTarihi = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["Basari"] = "Randevu iptal edildi.";
            return RedirectToAction(nameof(Randevular));
        }

        // =====================================================
        // RANDEVU TAMAMLA
        // =====================================================
        /// <summary>
        /// Randevuyu tamamlandı olarak işaretler
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RandevuTamamla(int id, string? antrenorNotu)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu == null) return NotFound();

            randevu.Durum = RandevuDurumu.Tamamlandi;
            randevu.AntrenorNotu = antrenorNotu;
            randevu.GuncellemeTarihi = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["Basari"] = "Randevu tamamlandı olarak işaretlendi.";
            return RedirectToAction(nameof(Randevular));
        }

        // =====================================================
        // ÜYE YÖNETİMİ - LİSTE
        // URL: /Admin/Uyeler
        // =====================================================
        /// <summary>
        /// Tüm üyeleri listeler
        /// </summary>
        public async Task<IActionResult> Uyeler()
        {
            var uyeler = await _context.Users
                .OrderByDescending(u => u.UyelikTarihi)
                .ToListAsync();

            return View(uyeler);
        }

        // =====================================================
        // ÜYE SİL (Pasif Yap)
        // =====================================================
        /// <summary>
        /// Üyeyi pasif duruma alır (soft delete)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UyeSil(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var uye = await _userManager.FindByIdAsync(id);
            if (uye == null) return NotFound();

            // Soft delete: Aktifliği kapat
            uye.AktifUyelik = false;
            var result = await _userManager.UpdateAsync(uye);

            if (result.Succeeded)
            {
                TempData["Basari"] = $"{uye.Ad} {uye.Soyad} başarıyla pasif yapıldı.";
            }
            else
            {
                TempData["Hata"] = "Üye pasif yapılırken hata oluştu.";
            }

            return RedirectToAction(nameof(Uyeler));
        }

        // =====================================================
        // ÜYE TAMAMEN SİL (Hard Delete)
        // =====================================================
        /// <summary>
        /// Üyeyi veritabanından tamamen siler (hard delete)
        /// DİKKAT: Bu işlem geri alınamaz!
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UyeTamamenSil(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var uye = await _userManager.FindByIdAsync(id);
            if (uye == null) return NotFound();

            // Kullanıcının randevularını kontrol et
            var randevuSayisi = await _context.Randevular
                .Where(r => r.UyeId == id)
                .CountAsync();

            if (randevuSayisi > 0)
            {
                TempData["Hata"] = $"Bu üyenin {randevuSayisi} adet randevusu var. Önce randevuları silin veya üyeyi pasif yapın.";
                return RedirectToAction(nameof(Uyeler));
            }

            // Üyeyi tamamen sil
            var result = await _userManager.DeleteAsync(uye);

            if (result.Succeeded)
            {
                _logger.LogWarning("Üye tamamen silindi: {Email} - {Id}", uye.Email, id);
                TempData["Basari"] = $"{uye.Ad} {uye.Soyad} sistemden tamamen silindi.";
            }
            else
            {
                TempData["Hata"] = "Üye silinirken hata oluştu: " + string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction(nameof(Uyeler));
        }

        // =====================================================
        // ÜYE AKTİFLEŞTİR
        // =====================================================
        /// <summary>
        /// Pasif üyeyi tekrar aktif duruma getirir
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UyeAktifle(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var uye = await _userManager.FindByIdAsync(id);
            if (uye == null) return NotFound();

            // Üyeyi aktif yap
            uye.AktifUyelik = true;
            var result = await _userManager.UpdateAsync(uye);

            if (result.Succeeded)
            {
                TempData["Basari"] = $"{uye.Ad} {uye.Soyad} başarıyla aktifleştirildi.";
            }
            else
            {
                TempData["Hata"] = "Üye aktifleştirilirken hata oluştu.";
            }

            return RedirectToAction(nameof(Uyeler));
        }

        // =====================================================
        // ÜYE AKTİFLİK DEĞİŞTİR
        // =====================================================
        /// <summary>
        /// Üyenin aktiflik durumunu değiştirir
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UyeAktiflikDegistir(string id)
        {
            var uye = await _userManager.FindByIdAsync(id);
            if (uye == null) return NotFound();

            uye.AktifUyelik = !uye.AktifUyelik;
            await _userManager.UpdateAsync(uye);

            TempData["Basari"] = uye.AktifUyelik 
                ? "Üye aktif edildi." 
                : "Üye pasif yapıldı.";

            return RedirectToAction(nameof(Uyeler));
        }

        // =====================================================
        // YARDIMCI METODLAR
        // =====================================================

        /// <summary>
        /// Antrenör formu için dropdown listelerini hazırlar
        /// </summary>
        private async Task HazirlaAntrenorDropdownlar()
        {
            var salonlar = await _context.SporSalonlari
                .Where(s => s.Aktif)
                .OrderBy(s => s.Ad)
                .ToListAsync();

            ViewBag.SporSalonlari = new SelectList(salonlar, "Id", "Ad");
        }

        /// <summary>
        /// Hizmet formu için dropdown listelerini hazırlar
        /// </summary>
        private async Task HazirlaHizmetDropdownlar()
        {
            var salonlar = await _context.SporSalonlari
                .Where(s => s.Aktif)
                .OrderBy(s => s.Ad)
                .ToListAsync();

            ViewBag.SporSalonlari = new SelectList(salonlar, "Id", "Ad");
            ViewBag.Kategoriler = Enum.GetValues<HizmetKategorisi>()
                .Select(k => new SelectListItem 
                { 
                    Value = ((int)k).ToString(), 
                    Text = k.ToString() 
                });
        }

        private async Task<bool> AntrenorVarMi(int id)
        {
            return await _context.Antrenorler.AnyAsync(a => a.Id == id);
        }

        private async Task<bool> HizmetVarMi(int id)
        {
            return await _context.Hizmetler.AnyAsync(h => h.Id == id);
        }
    }

    // =====================================================
    // ADMIN DASHBOARD VIEW MODEL
    // =====================================================
    /// <summary>
    /// Admin paneli ana sayfa için view model
    /// </summary>
    public class AdminDashboardViewModel
    {
        public int ToplamUye { get; set; }
        public int ToplamAntrenor { get; set; }
        public int ToplamHizmet { get; set; }
        public int ToplamRandevu { get; set; }
        public int BugunRandevuSayisi { get; set; }
        public int BekleyenRandevuSayisi { get; set; }
        public List<Randevu> SonRandevular { get; set; } = new List<Randevu>();
        public List<Uye> SonUyeler { get; set; } = new List<Uye>();
    }
}

