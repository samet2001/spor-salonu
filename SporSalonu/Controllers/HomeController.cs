// =====================================================
// HOME CONTROLLER SINIFI
// Bu dosya, ana sayfa ve genel sayfaları yönetir
// Ziyaretçilerin ilk gördüğü sayfalardır
// =====================================================

using System.Diagnostics;                         // Activity sınıfı için
using Microsoft.AspNetCore.Mvc;                   // MVC controller
using Microsoft.EntityFrameworkCore;              // Entity Framework
using SporSalonu.Data;                            // DbContext
using SporSalonu.Models;                          // Model sınıfları

namespace SporSalonu.Controllers
{
    /// <summary>
    /// HomeController - Ana sayfa ve genel içerik controller'ı
    /// Giriş yapmamış ziyaretçiler de erişebilir
    /// </summary>
    public class HomeController : Controller
    {
        // =====================================================
        // BAĞIMLILIKLAR
        // =====================================================
        
        private readonly ApplicationDbContext _context;   // Veritabanı bağlamı
        private readonly ILogger<HomeController> _logger; // Loglama

        // =====================================================
        // CONSTRUCTOR
        // =====================================================
        public HomeController(
            ApplicationDbContext context,
            ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // =====================================================
        // ANA SAYFA
        // URL: / veya /Home/Index
        // =====================================================
        /// <summary>
        /// Ana sayfayı gösterir
        /// Spor salonu bilgileri, hizmetler ve antrenörleri listeler
        /// </summary>
        public async Task<IActionResult> Index()
        {
            // Ana sayfada gösterilecek verileri hazırla
            var viewModel = new AnaSayfaViewModel();

            // Aktif spor salonunu getir (ilk salon)
            viewModel.SporSalonu = await _context.SporSalonlari
                .Where(s => s.Aktif) // Sadece aktif salonlar
                .FirstOrDefaultAsync();

            // Aktif hizmetleri getir
            viewModel.Hizmetler = await _context.Hizmetler
                .Where(h => h.Aktif) // Sadece aktif hizmetler
                .OrderBy(h => h.Kategori) // Kategoriye göre sırala
                .Take(8) // İlk 8 hizmeti al
                .ToListAsync();

            // Aktif antrenörleri getir
            viewModel.Antrenorler = await _context.Antrenorler
                .Where(a => a.Aktif) // Sadece aktif antrenörler
                .Include(a => a.AntrenorHizmetleri) // Hizmet ilişkilerini dahil et
                    .ThenInclude(ah => ah.Hizmet) // Hizmet detaylarını dahil et
                .OrderBy(a => a.Ad) // Ada göre sırala
                .Take(4) // İlk 4 antrenörü al
                .ToListAsync();

            // Toplam istatistikler
            viewModel.ToplamUye = await _context.Users.CountAsync();
            viewModel.ToplamAntrenor = await _context.Antrenorler.CountAsync(a => a.Aktif);
            viewModel.ToplamHizmet = await _context.Hizmetler.CountAsync(h => h.Aktif);

            return View(viewModel);
        }

        // =====================================================
        // HAKKIMIZDA SAYFASI
        // URL: /Home/Hakkimizda
        // =====================================================
        /// <summary>
        /// Hakkımızda sayfasını gösterir
        /// </summary>
        public async Task<IActionResult> Hakkimizda()
        {
            // Spor salonu bilgilerini getir
            var salon = await _context.SporSalonlari
                .Where(s => s.Aktif)
                .FirstOrDefaultAsync();

            return View(salon);
        }

        // =====================================================
        // HİZMETLERİMİZ SAYFASI
        // URL: /Home/Hizmetlerimiz
        // =====================================================
        /// <summary>
        /// Tüm hizmetleri listeler
        /// </summary>
        public async Task<IActionResult> Hizmetlerimiz()
        {
            // Tüm aktif hizmetleri getir
            var hizmetler = await _context.Hizmetler
                .Where(h => h.Aktif)
                .Include(h => h.SporSalonu)
                .OrderBy(h => h.Kategori)
                .ThenBy(h => h.Ad)
                .ToListAsync();

            return View(hizmetler);
        }

        // =====================================================
        // ANTRENÖRLER SAYFASI
        // URL: /Home/Antrenorler
        // =====================================================
        /// <summary>
        /// Tüm antrenörleri listeler
        /// </summary>
        public async Task<IActionResult> Antrenorler()
        {
            // Tüm aktif antrenörleri getir
            var antrenorler = await _context.Antrenorler
                .Where(a => a.Aktif)
                .Include(a => a.SporSalonu)
                .Include(a => a.AntrenorHizmetleri)
                    .ThenInclude(ah => ah.Hizmet)
                .OrderBy(a => a.Ad)
                .ToListAsync();

            return View(antrenorler);
        }

        // =====================================================
        // ANTRENÖR DETAY SAYFASI
        // URL: /Home/AntrenorDetay/5
        // =====================================================
        /// <summary>
        /// Seçilen antrenörün detaylarını gösterir
        /// </summary>
        /// <param name="id">Antrenör ID</param>
        public async Task<IActionResult> AntrenorDetay(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Antrenörü tüm ilişkileriyle getir
            var antrenor = await _context.Antrenorler
                .Include(a => a.SporSalonu)
                .Include(a => a.AntrenorHizmetleri)
                    .ThenInclude(ah => ah.Hizmet)
                .Include(a => a.Musaitlikler)
                .FirstOrDefaultAsync(a => a.Id == id && a.Aktif);

            if (antrenor == null)
            {
                return NotFound();
            }

            return View(antrenor);
        }

        // =====================================================
        // İLETİŞİM SAYFASI
        // URL: /Home/Iletisim
        // =====================================================
        /// <summary>
        /// İletişim sayfasını gösterir
        /// </summary>
        public async Task<IActionResult> Iletisim()
        {
            // Spor salonu iletişim bilgilerini getir
            var salon = await _context.SporSalonlari
                .Where(s => s.Aktif)
                .FirstOrDefaultAsync();

            return View(salon);
        }

        // =====================================================
        // GİZLİLİK POLİTİKASI
        // URL: /Home/Privacy
        // =====================================================
        /// <summary>
        /// Gizlilik politikası sayfası
        /// </summary>
        public IActionResult Privacy()
        {
            return View();
        }

        // =====================================================
        // HATA SAYFASI
        // Hata durumlarında gösterilir
        // =====================================================
        /// <summary>
        /// Hata sayfasını gösterir
        /// </summary>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // Hata bilgileriyle view'ı döndür
            return View(new ErrorViewModel 
            { 
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier 
            });
        }
    }

    // =====================================================
    // ANA SAYFA VİEW MODEL
    // Ana sayfada gösterilecek verileri tutar
    // =====================================================
    /// <summary>
    /// AnaSayfaViewModel - Ana sayfa için gerekli tüm verileri içerir
    /// </summary>
    public class AnaSayfaViewModel
    {
        /// <summary>
        /// Spor salonu bilgileri
        /// </summary>
        public Models.SporSalonu? SporSalonu { get; set; }

        /// <summary>
        /// Öne çıkan hizmetler
        /// </summary>
        public List<Hizmet> Hizmetler { get; set; } = new List<Hizmet>();

        /// <summary>
        /// Öne çıkan antrenörler
        /// </summary>
        public List<Antrenor> Antrenorler { get; set; } = new List<Antrenor>();

        /// <summary>
        /// Toplam üye sayısı
        /// </summary>
        public int ToplamUye { get; set; }

        /// <summary>
        /// Toplam antrenör sayısı
        /// </summary>
        public int ToplamAntrenor { get; set; }

        /// <summary>
        /// Toplam hizmet sayısı
        /// </summary>
        public int ToplamHizmet { get; set; }
    }
}
