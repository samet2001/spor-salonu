// =====================================================
// PROGRAM.CS - UYGULAMA GİRİŞ NOKTASI
// Bu dosya, ASP.NET Core uygulamasının başlangıç noktasıdır
// Servis yapılandırması, middleware pipeline ve uygulama ayarları burada tanımlanır
// =====================================================

using Microsoft.AspNetCore.Identity;           // ASP.NET Core Identity (kimlik doğrulama)
using Microsoft.EntityFrameworkCore;           // Entity Framework Core (ORM)
using SporSalonu.Data;                          // Veritabanı bağlam sınıfı
using SporSalonu.Models;                        // Model sınıfları
using SporSalonu.Services;                      // Servis sınıfları

// =====================================================
// BUILDER OLUŞTURMA
// WebApplication.CreateBuilder ile uygulama yapılandırıcısı oluşturulur
// args: Komut satırı argümanları
// =====================================================
var builder = WebApplication.CreateBuilder(args);

// =====================================================
// SERVİS YAPILANDIRMASI (Dependency Injection Container)
// Uygulamada kullanılacak tüm servisler burada kayıt edilir
// =====================================================

// -----------------------------------------------------
// PostgreSQL VERİTABANI BAĞLANTISI
// Entity Framework Core ile PostgreSQL kullanımı
// Connection string appsettings.json'dan okunur
// -----------------------------------------------------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"), // Connection string
        npgsqlOptions => npgsqlOptions.EnableRetryOnFailure() // Bağlantı hatalarında otomatik yeniden deneme
    )
);

// -----------------------------------------------------
// ASP.NET CORE IDENTITY YAPILANDIRMASI
// Kullanıcı kimlik doğrulama ve yetkilendirme sistemi
// Uye sınıfı özel kullanıcı modeli olarak kullanılır
// -----------------------------------------------------
builder.Services.AddIdentity<Uye, IdentityRole>(options =>
{
    // ŞİFRE GEREKSİNİMLERİ
    // Güvenlik için şifre kuralları belirlenir
    options.Password.RequireDigit = false;           // Rakam zorunlu DEĞİL
    options.Password.RequireLowercase = false;       // Küçük harf zorunlu DEĞİL
    options.Password.RequireUppercase = false;       // Büyük harf zorunlu DEĞİL
    options.Password.RequireNonAlphanumeric = false; // Özel karakter zorunlu DEĞİL
    options.Password.RequiredLength = 3;             // Minimum 3 karakter (kolay test için)
    options.Password.RequiredUniqueChars = 1;        // En az 1 farklı karakter

    // KULLANICI AYARLARI
    options.User.RequireUniqueEmail = true;          // E-posta benzersiz olmalı
    options.User.AllowedUserNameCharacters =         // Kullanıcı adında izin verilen karakterler
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

    // OTURUM AÇMA AYARLARI
    options.SignIn.RequireConfirmedEmail = false;    // E-posta onayı zorunlu DEĞİL
    options.SignIn.RequireConfirmedAccount = false;  // Hesap onayı zorunlu DEĞİL

    // HESAP KİLİTLEME AYARLARI
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // 5 dk kilitleme
    options.Lockout.MaxFailedAccessAttempts = 5;     // 5 hatalı denemede kilitle
    options.Lockout.AllowedForNewUsers = true;       // Yeni kullanıcılar için kilitleme aktif
})
.AddEntityFrameworkStores<ApplicationDbContext>()    // Identity verilerini EF Core ile sakla
.AddDefaultTokenProviders();                          // Şifre sıfırlama vb. için token üretici

// -----------------------------------------------------
// COOKIE YAPILANDIRMASI (Oturum Yönetimi)
// Kullanıcı oturum bilgileri cookie'lerde saklanır
// -----------------------------------------------------
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Hesap/Giris";              // Giriş yapılmamışsa yönlendir
    options.LogoutPath = "/Hesap/Cikis";             // Çıkış yapılınca yönlendir
    options.AccessDeniedPath = "/Hesap/ErisimEngellendi"; // Yetkisiz erişimde yönlendir
    options.ExpireTimeSpan = TimeSpan.FromDays(7);   // Cookie geçerlilik süresi (7 gün)
    options.SlidingExpiration = true;                // Her istekte süre yenilenir
    options.Cookie.Name = "SporSalonuAuth";          // Cookie adı
    options.Cookie.HttpOnly = true;                  // JavaScript erişimi engelle (güvenlik)
});

// -----------------------------------------------------
// MVC VE RAZOR PAGES SERVİSLERİ
// Controller ve View desteği
// -----------------------------------------------------
builder.Services.AddControllersWithViews();

// -----------------------------------------------------
// API VE JSON YAPILANDIRMASI
// API endpoint'leri için JSON serileştirme ayarları
// -----------------------------------------------------
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Türkçe karakter desteği için encoding ayarı
        options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        // Enum'ları string olarak serileştir
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        // Null değerleri JSON'a dahil etme
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// -----------------------------------------------------
// AI SERVİSİ KAYDI
// Yapay zeka önerileri için servis
// -----------------------------------------------------
builder.Services.AddScoped<IAIService, AIService>();

// -----------------------------------------------------
// HTTP CLIENT FACTORY
// Dış API'lere istek göndermek için (OpenAI vb.)
// -----------------------------------------------------
builder.Services.AddHttpClient();

// =====================================================
// UYGULAMA OLUŞTURMA
// Builder'dan uygulama instance'ı oluşturulur
// =====================================================
var app = builder.Build();

// =====================================================
// VERİTABANI VE SEED DATA OLUŞTURMA
// Uygulama başlangıcında veritabanını hazırla
// =====================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // DbContext'i al
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        // Veritabanını oluştur (migration uygula)
        // EnsureCreated: Veritabanı yoksa oluşturur
        await context.Database.EnsureCreatedAsync();

        // Seed data'yı çalıştır (varsayılan veriler)
        await SeedData.Initialize(services);
    }
    catch (Exception ex)
    {
        // Hata durumunda log'a yaz
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Veritabanı oluşturulurken bir hata oluştu.");
    }
}

// =====================================================
// MIDDLEWARE PIPELINE YAPILANDIRMASI
// HTTP isteklerinin işlenme sırası burada belirlenir
// =====================================================

// -----------------------------------------------------
// GELİŞTİRME ORTAMI HATA SAYFASI
// Development modunda detaylı hata sayfası gösterilir
// -----------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Detaylı hata sayfası
}
else
{
    // Üretim ortamında özel hata sayfası
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // HTTP Strict Transport Security
}

// -----------------------------------------------------
// HTTPS YÖNLENDİRME
// HTTP isteklerini HTTPS'e yönlendirir
// -----------------------------------------------------
app.UseHttpsRedirection();

// -----------------------------------------------------
// STATİK DOSYALAR
// wwwroot klasöründeki dosyaları sunar (CSS, JS, resimler)
// -----------------------------------------------------
app.UseStaticFiles();

// -----------------------------------------------------
// ROUTING (YÖNLENDİRME)
// URL'leri controller action'larına eşleştirir
// -----------------------------------------------------
app.UseRouting();

// -----------------------------------------------------
// KİMLİK DOĞRULAMA (AUTHENTICATION)
// Kullanıcının kim olduğunu belirler
// UseAuthorization'dan ÖNCE gelmelidir!
// -----------------------------------------------------
app.UseAuthentication();

// -----------------------------------------------------
// YETKİLENDİRME (AUTHORIZATION)
// Kullanıcının yetkilerini kontrol eder
// -----------------------------------------------------
app.UseAuthorization();

// =====================================================
// ROUTE (YOL) TANIMLARI
// URL şablonları ve controller eşleştirmeleri
// =====================================================

// -----------------------------------------------------
// ADMIN ALANI ROUTE'U
// /Admin/... şeklindeki URL'ler için özel alan
// -----------------------------------------------------
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
);

// -----------------------------------------------------
// VARSAYILAN ROUTE
// Ana sayfa ve genel sayfalar için
// Örnek: /Home/Index, /Randevu/Create
// -----------------------------------------------------
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

// -----------------------------------------------------
// API ROUTE'LARI
// REST API endpoint'leri için
// -----------------------------------------------------
app.MapControllers();

// =====================================================
// UYGULAMAYI BAŞLAT
// Sunucuyu çalıştır ve istekleri dinlemeye başla
// =====================================================
app.Run();
