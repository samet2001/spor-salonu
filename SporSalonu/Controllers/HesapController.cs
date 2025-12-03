// =====================================================
// HESAP CONTROLLER SINIFI
// Bu dosya, kullanıcı hesap işlemlerini yönetir
// Kayıt olma, giriş yapma, çıkış yapma ve profil işlemleri
// =====================================================

using Microsoft.AspNetCore.Authorization;         // Yetkilendirme attribute'ları
using Microsoft.AspNetCore.Identity;              // Identity servisleri
using Microsoft.AspNetCore.Mvc;                   // MVC controller
using SporSalonu.Models;                          // Model sınıfları
using SporSalonu.Models.ViewModels;               // ViewModel sınıfları

namespace SporSalonu.Controllers
{
    /// <summary>
    /// HesapController - Kullanıcı hesap yönetimi için controller
    /// Kayıt, giriş, çıkış ve profil işlemlerini yönetir
    /// </summary>
    public class HesapController : Controller
    {
        // =====================================================
        // BAĞIMLILIKLAR (Dependencies)
        // Dependency Injection ile alınan servisler
        // =====================================================
        
        private readonly UserManager<Uye> _userManager;       // Kullanıcı yönetimi
        private readonly SignInManager<Uye> _signInManager;   // Oturum yönetimi
        private readonly ILogger<HesapController> _logger;    // Loglama

        // =====================================================
        // CONSTRUCTOR (YAPICI METOD)
        // Dependency Injection ile servisler alınır
        // =====================================================
        public HesapController(
            UserManager<Uye> userManager,
            SignInManager<Uye> signInManager,
            ILogger<HesapController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        // =====================================================
        // KAYIT OL - GET
        // Kayıt formunu gösterir
        // URL: /Hesap/KayitOl
        // =====================================================
        /// <summary>
        /// Kayıt formu sayfasını gösterir
        /// </summary>
        [HttpGet]
        public IActionResult KayitOl()
        {
            // Kullanıcı zaten giriş yapmışsa ana sayfaya yönlendir
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            // Boş ViewModel ile formu göster
            return View(new KayitViewModel());
        }

        // =====================================================
        // KAYIT OL - POST
        // Form verilerini alır ve yeni kullanıcı oluşturur
        // =====================================================
        /// <summary>
        /// Kayıt formunu işler ve yeni kullanıcı oluşturur
        /// </summary>
        /// <param name="model">Form verileri</param>
        [HttpPost]
        [ValidateAntiForgeryToken] // CSRF koruması
        public async Task<IActionResult> KayitOl(KayitViewModel model)
        {
            // Model doğrulama başarılı mı kontrol et
            if (ModelState.IsValid)
            {
                // Yeni kullanıcı oluştur
                var kullanici = new Uye
                {
                    UserName = model.Email,          // Kullanıcı adı = e-posta
                    Email = model.Email,
                    Ad = model.Ad,
                    Soyad = model.Soyad,
                    PhoneNumber = model.Telefon,
                    DogumTarihi = model.DogumTarihi,
                    Cinsiyet = model.Cinsiyet,
                    Boy = model.Boy,
                    Kilo = model.Kilo,
                    FitnessHedefi = model.FitnessHedefi,
                    DeneyimSeviyesi = model.DeneyimSeviyesi,
                    UyelikTarihi = DateTime.UtcNow,
                    AktifUyelik = true,
                    EmailConfirmed = true            // E-posta onayı şimdilik otomatik
                };

                // Identity ile kullanıcıyı oluştur
                var result = await _userManager.CreateAsync(kullanici, model.Sifre);

                if (result.Succeeded)
                {
                    // Kullanıcıya "Uye" rolünü ata
                    await _userManager.AddToRoleAsync(kullanici, "Uye");

                    // Log kaydı
                    _logger.LogInformation("Yeni kullanıcı kaydoldu: {Email}", model.Email);

                    // Otomatik giriş yap
                    await _signInManager.SignInAsync(kullanici, isPersistent: false);

                    // Başarı mesajı
                    TempData["Basari"] = "Kayıt işlemi başarılı! Hoş geldiniz.";

                    // Ana sayfaya yönlendir
                    return RedirectToAction("Index", "Home");
                }

                // Hata varsa ModelState'e ekle
                foreach (var error in result.Errors)
                {
                    // Identity hatalarını Türkçe'ye çevir
                    var turkceHata = TurkceHataMesaji(error.Code, error.Description);
                    ModelState.AddModelError(string.Empty, turkceHata);
                }
            }

            // Hata varsa formu tekrar göster
            return View(model);
        }

        // =====================================================
        // GİRİŞ YAP - GET
        // Giriş formunu gösterir
        // URL: /Hesap/Giris
        // =====================================================
        /// <summary>
        /// Giriş formu sayfasını gösterir
        /// </summary>
        /// <param name="returnUrl">Giriş sonrası yönlendirilecek URL</param>
        [HttpGet]
        public IActionResult Giris(string? returnUrl = null)
        {
            // Kullanıcı zaten giriş yapmışsa ana sayfaya yönlendir
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            // ReturnUrl'i ViewData'ya aktar
            ViewData["ReturnUrl"] = returnUrl;

            return View(new GirisViewModel { ReturnUrl = returnUrl });
        }

        // =====================================================
        // GİRİŞ YAP - POST
        // Giriş formunu işler ve oturum açar
        // =====================================================
        /// <summary>
        /// Giriş formunu işler ve kullanıcı oturumu açar
        /// </summary>
        /// <param name="model">Giriş bilgileri</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Giris(GirisViewModel model)
        {
            if (ModelState.IsValid)
            {
                // E-posta ile kullanıcıyı bul
                var kullanici = await _userManager.FindByEmailAsync(model.Email);

                if (kullanici == null)
                {
                    ModelState.AddModelError(string.Empty, "E-posta adresi bulunamadı.");
                    return View(model);
                }

                // Üyelik aktif mi kontrol et
                if (!kullanici.AktifUyelik)
                {
                    ModelState.AddModelError(string.Empty, "Üyeliğiniz pasif durumda. Lütfen yönetici ile iletişime geçin.");
                    return View(model);
                }

                // Şifre ile giriş dene
                var result = await _signInManager.PasswordSignInAsync(
                    kullanici,                    // Kullanıcı
                    model.Sifre,                  // Şifre
                    model.BeniHatirla,            // Beni hatırla
                    lockoutOnFailure: true        // Hatalı girişte kilitle
                );

                if (result.Succeeded)
                {
                    // Log kaydı
                    _logger.LogInformation("Kullanıcı giriş yaptı: {Email}", model.Email);

                    // ReturnUrl varsa oraya, yoksa ana sayfaya yönlendir
                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }

                    // Admin ise admin paneline yönlendir
                    if (await _userManager.IsInRoleAsync(kullanici, "Admin"))
                    {
                        return RedirectToAction("Index", "Admin", new { area = "Admin" });
                    }

                    return RedirectToAction("Index", "Home");
                }

                // Hesap kilitli mi?
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("Hesap kilitlendi: {Email}", model.Email);
                    ModelState.AddModelError(string.Empty, "Hesabınız geçici olarak kilitlendi. Lütfen 5 dakika bekleyin.");
                    return View(model);
                }

                // Genel hata
                ModelState.AddModelError(string.Empty, "E-posta veya şifre hatalı.");
            }

            return View(model);
        }

        // =====================================================
        // ÇIKIŞ YAP
        // Oturumu kapatır
        // URL: /Hesap/Cikis
        // =====================================================
        /// <summary>
        /// Kullanıcı oturumunu kapatır
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cikis()
        {
            // Oturumu kapat
            await _signInManager.SignOutAsync();

            // Log kaydı
            _logger.LogInformation("Kullanıcı çıkış yaptı");

            // Ana sayfaya yönlendir
            return RedirectToAction("Index", "Home");
        }

        // =====================================================
        // ERİŞİM ENGELLENDİ
        // Yetkisiz erişim durumunda gösterilir
        // =====================================================
        /// <summary>
        /// Yetkisiz erişim sayfası
        /// </summary>
        [HttpGet]
        public IActionResult ErisimEngellendi()
        {
            return View();
        }

        // =====================================================
        // PROFİL - GET
        // Kullanıcı profil sayfasını gösterir
        // =====================================================
        /// <summary>
        /// Kullanıcı profil sayfasını gösterir
        /// </summary>
        [Authorize] // Sadece giriş yapmış kullanıcılar erişebilir
        [HttpGet]
        public async Task<IActionResult> Profil()
        {
            // Mevcut kullanıcıyı al
            var kullanici = await _userManager.GetUserAsync(User);

            if (kullanici == null)
            {
                return NotFound("Kullanıcı bulunamadı.");
            }

            return View(kullanici);
        }

        // =====================================================
        // PROFİL DÜZENLE - GET
        // Profil düzenleme formunu gösterir
        // =====================================================
        /// <summary>
        /// Profil düzenleme formu
        /// </summary>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ProfilDuzenle()
        {
            var kullanici = await _userManager.GetUserAsync(User);

            if (kullanici == null)
            {
                return NotFound("Kullanıcı bulunamadı.");
            }

            // ViewModel'e dönüştür
            var model = new ProfilDuzenleViewModel
            {
                Ad = kullanici.Ad,
                Soyad = kullanici.Soyad,
                Telefon = kullanici.PhoneNumber ?? "",
                DogumTarihi = kullanici.DogumTarihi,
                Cinsiyet = kullanici.Cinsiyet,
                Boy = kullanici.Boy,
                Kilo = kullanici.Kilo,
                FitnessHedefi = kullanici.FitnessHedefi,
                DeneyimSeviyesi = kullanici.DeneyimSeviyesi,
                SaglikNotu = kullanici.SaglikNotu
            };

            return View(model);
        }

        // =====================================================
        // PROFİL DÜZENLE - POST
        // Profil güncellemelerini kaydeder
        // =====================================================
        /// <summary>
        /// Profil güncellemelerini kaydeder
        /// </summary>
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProfilDuzenle(ProfilDuzenleViewModel model)
        {
            if (ModelState.IsValid)
            {
                var kullanici = await _userManager.GetUserAsync(User);

                if (kullanici == null)
                {
                    return NotFound("Kullanıcı bulunamadı.");
                }

                // Bilgileri güncelle
                kullanici.Ad = model.Ad;
                kullanici.Soyad = model.Soyad;
                kullanici.PhoneNumber = model.Telefon;
                kullanici.DogumTarihi = model.DogumTarihi;
                kullanici.Cinsiyet = model.Cinsiyet;
                kullanici.Boy = model.Boy;
                kullanici.Kilo = model.Kilo;
                kullanici.FitnessHedefi = model.FitnessHedefi;
                kullanici.DeneyimSeviyesi = model.DeneyimSeviyesi;
                kullanici.SaglikNotu = model.SaglikNotu;

                // Değişiklikleri kaydet
                var result = await _userManager.UpdateAsync(kullanici);

                if (result.Succeeded)
                {
                    TempData["Basari"] = "Profiliniz başarıyla güncellendi.";
                    return RedirectToAction(nameof(Profil));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        // =====================================================
        // ŞİFRE DEĞİŞTİR - GET
        // Şifre değiştirme formunu gösterir
        // =====================================================
        /// <summary>
        /// Şifre değiştirme formu
        /// </summary>
        [Authorize]
        [HttpGet]
        public IActionResult SifreDegistir()
        {
            return View(new SifreDegistirViewModel());
        }

        // =====================================================
        // ŞİFRE DEĞİŞTİR - POST
        // Şifreyi günceller
        // =====================================================
        /// <summary>
        /// Şifreyi günceller
        /// </summary>
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SifreDegistir(SifreDegistirViewModel model)
        {
            if (ModelState.IsValid)
            {
                var kullanici = await _userManager.GetUserAsync(User);

                if (kullanici == null)
                {
                    return NotFound("Kullanıcı bulunamadı.");
                }

                // Şifreyi değiştir
                var result = await _userManager.ChangePasswordAsync(
                    kullanici,
                    model.MevcutSifre,
                    model.YeniSifre
                );

                if (result.Succeeded)
                {
                    // Yeniden giriş yap (güvenlik için)
                    await _signInManager.RefreshSignInAsync(kullanici);

                    TempData["Basari"] = "Şifreniz başarıyla değiştirildi.";
                    return RedirectToAction(nameof(Profil));
                }

                foreach (var error in result.Errors)
                {
                    var turkceHata = TurkceHataMesaji(error.Code, error.Description);
                    ModelState.AddModelError(string.Empty, turkceHata);
                }
            }

            return View(model);
        }

        // =====================================================
        // YARDIMCI METODLAR
        // =====================================================

        /// <summary>
        /// Identity hata mesajlarını Türkçe'ye çevirir
        /// </summary>
        private string TurkceHataMesaji(string code, string defaultMessage)
        {
            return code switch
            {
                "DuplicateUserName" => "Bu kullanıcı adı zaten kullanılıyor.",
                "DuplicateEmail" => "Bu e-posta adresi zaten kayıtlı.",
                "InvalidEmail" => "Geçersiz e-posta adresi.",
                "PasswordTooShort" => "Şifre çok kısa.",
                "PasswordRequiresDigit" => "Şifre en az bir rakam içermelidir.",
                "PasswordRequiresLower" => "Şifre en az bir küçük harf içermelidir.",
                "PasswordRequiresUpper" => "Şifre en az bir büyük harf içermelidir.",
                "PasswordRequiresNonAlphanumeric" => "Şifre en az bir özel karakter içermelidir.",
                "PasswordMismatch" => "Mevcut şifre hatalı.",
                _ => defaultMessage
            };
        }
    }
}

