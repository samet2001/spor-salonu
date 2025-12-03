// =====================================================
// AI CONTROLLER SINIFI
// Bu dosya, yapay zeka tabanlı egzersiz ve diyet
// önerisi alma işlemlerini yönetir
// =====================================================

using Microsoft.AspNetCore.Authorization;         // Yetkilendirme
using Microsoft.AspNetCore.Identity;              // Identity
using Microsoft.AspNetCore.Mvc;                   // MVC
using SporSalonu.Models;                          // Model sınıfları
using SporSalonu.Models.ViewModels;               // ViewModel sınıfları
using SporSalonu.Services;                        // Servis sınıfları

namespace SporSalonu.Controllers
{
    /// <summary>
    /// AIController - Yapay zeka önerileri için controller
    /// Kullanıcı bilgilerine göre kişiselleştirilmiş öneriler sunar
    /// </summary>
    public class AIController : Controller
    {
        // =====================================================
        // BAĞIMLILIKLAR
        // =====================================================
        
        private readonly IAIService _aiService;           // AI servisi
        private readonly UserManager<Uye> _userManager;   // Kullanıcı yönetimi
        private readonly ILogger<AIController> _logger;   // Loglama

        // =====================================================
        // CONSTRUCTOR
        // =====================================================
        public AIController(
            IAIService aiService,
            UserManager<Uye> userManager,
            ILogger<AIController> logger)
        {
            _aiService = aiService;
            _userManager = userManager;
            _logger = logger;
        }

        // =====================================================
        // ÖNERİ FORMU - GET
        // URL: /AI/Index veya /AI
        // =====================================================
        /// <summary>
        /// AI öneri formunu gösterir
        /// Giriş yapmış kullanıcının bilgileri otomatik doldurulur
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var viewModel = new AIOneriViewModel
            {
                Istek = new AIOneriIstek
                {
                    // Varsayılan değerler
                    Yas = 25,
                    Boy = 170,
                    Kilo = 70,
                    Cinsiyet = Cinsiyet.Erkek,
                    FitnessHedefi = FitnessHedefi.FormKorumak,
                    DeneyimSeviyesi = DeneyimSeviyesi.Baslangic,
                    HaftalikGun = 3,
                    GunlukSureDakika = 60,
                    OneriTuru = OneriTuru.HerIkisi
                }
            };

            // Kullanıcı giriş yapmışsa bilgilerini doldur
            if (User.Identity?.IsAuthenticated == true)
            {
                var kullanici = await _userManager.GetUserAsync(User);
                
                if (kullanici != null)
                {
                    // Kullanıcı bilgilerini forma aktar
                    if (kullanici.DogumTarihi.HasValue)
                    {
                        viewModel.Istek.Yas = DateTime.UtcNow.Year - kullanici.DogumTarihi.Value.Year;
                    }
                    
                    if (kullanici.Cinsiyet.HasValue)
                    {
                        viewModel.Istek.Cinsiyet = kullanici.Cinsiyet.Value;
                    }
                    
                    if (kullanici.Boy.HasValue)
                    {
                        viewModel.Istek.Boy = kullanici.Boy.Value;
                    }
                    
                    if (kullanici.Kilo.HasValue)
                    {
                        viewModel.Istek.Kilo = kullanici.Kilo.Value;
                    }
                    
                    if (kullanici.FitnessHedefi.HasValue)
                    {
                        viewModel.Istek.FitnessHedefi = kullanici.FitnessHedefi.Value;
                    }
                    
                    if (kullanici.DeneyimSeviyesi.HasValue)
                    {
                        viewModel.Istek.DeneyimSeviyesi = kullanici.DeneyimSeviyesi.Value;
                    }
                    
                    if (!string.IsNullOrEmpty(kullanici.SaglikNotu))
                    {
                        viewModel.Istek.SaglikNotu = kullanici.SaglikNotu;
                    }
                }
            }

            return View(viewModel);
        }

        // =====================================================
        // ÖNERİ AL - POST
        // Formu işler ve AI'dan öneri alır
        // =====================================================
        /// <summary>
        /// Form bilgilerini alır ve AI'dan öneri üretir
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(AIOneriViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // AI'dan öneri al
                    var sonuc = await _aiService.OneriUretAsync(model.Istek);

                    // Sonucu modele ekle
                    model.Sonuc = sonuc;
                    model.FormGonderildi = true;

                    // Log kaydı
                    _logger.LogInformation("AI öneri üretildi - Hedef: {Hedef}", model.Istek.FitnessHedefi);

                    // Başarı mesajı
                    if (sonuc.Basarili)
                    {
                        TempData["Basari"] = "Kişiselleştirilmiş öneriniz hazırlandı!";
                    }
                    else
                    {
                        TempData["Hata"] = sonuc.HataMesaji;
                    }
                }
                catch (Exception ex)
                {
                    // Hata durumu
                    _logger.LogError(ex, "AI öneri üretilirken hata oluştu");
                    
                    model.FormGonderildi = true;
                    model.Sonuc = new AIOneriSonuc
                    {
                        Basarili = false,
                        HataMesaji = "Öneri üretilirken bir hata oluştu. Lütfen tekrar deneyin."
                    };
                    
                    TempData["Hata"] = "Öneri üretilirken bir hata oluştu.";
                }
            }

            return View(model);
        }

        // =====================================================
        // HIZLI ÖNERİ (AJAX)
        // URL: /AI/HizliOneri
        // =====================================================
        /// <summary>
        /// AJAX ile hızlı öneri döndürür (JSON formatında)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> HizliOneri([FromBody] AIOneriIstek istek)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var sonuc = await _aiService.OneriUretAsync(istek);
                return Json(sonuc);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hızlı öneri üretilirken hata");
                
                return Json(new AIOneriSonuc
                {
                    Basarili = false,
                    HataMesaji = "Öneri üretilirken hata oluştu."
                });
            }
        }

        // =====================================================
        // BMI HESAPLAMA (AJAX)
        // URL: /AI/HesaplaBMI
        // =====================================================
        /// <summary>
        /// AJAX ile BMI hesaplar
        /// </summary>
        [HttpGet]
        public IActionResult HesaplaBMI(int boy, decimal kilo)
        {
            if (boy <= 0 || kilo <= 0)
            {
                return Json(new { bmi = 0, kategori = "Geçersiz değer" });
            }

            // BMI hesapla
            decimal boyMetre = boy / 100m;
            decimal bmi = Math.Round(kilo / (boyMetre * boyMetre), 2);

            // Kategori belirle
            string kategori;
            if (bmi < 18.5m) kategori = "Zayıf";
            else if (bmi < 25m) kategori = "Normal";
            else if (bmi < 30m) kategori = "Fazla Kilolu";
            else kategori = "Obez";

            return Json(new { bmi, kategori });
        }
    }
}

