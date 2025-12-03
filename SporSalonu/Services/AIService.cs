// =====================================================
// AI SERVİS SINIFI
// Bu dosya, yapay zeka tabanlı egzersiz ve diyet
// önerileri üreten servis sınıfını tanımlar
// Groq API (Llama 3.1) kullanır - Ücretsiz!
// =====================================================

using SporSalonu.Models;
using SporSalonu.Models.ViewModels;
using System.Text;
using System.Text.Json;

namespace SporSalonu.Services
{
    // =====================================================
    // AI SERVİS INTERFACE
    // Dependency Injection için interface tanımı
    // =====================================================
    /// <summary>
    /// IAIService - Yapay zeka servis interface'i
    /// Groq API ile fitness önerileri üretir
    /// </summary>
    public interface IAIService
    {
        /// <summary>
        /// Kullanıcı bilgilerine göre AI destekli öneri üretir
        /// </summary>
        /// <param name="istek">Kullanıcının fiziksel bilgileri ve hedefleri</param>
        /// <returns>AI tarafından oluşturulmuş egzersiz ve diyet önerileri</returns>
        Task<AIOneriSonuc> OneriUretAsync(AIOneriIstek istek);
    }

    // =====================================================
    // AI SERVİS SINIFI
    // Groq API kullanarak yapay zeka önerilerini üretir
    // =====================================================
    /// <summary>
    /// AIService - Groq API (Llama 3.1) ile çalışan AI servis
    /// OpenAI uyumlu API formatı kullanır
    /// Ücretsiz tier: Günde 14,400 istek limiti
    /// </summary>
    public class AIService : IAIService
    {
        // =====================================================
        // BAĞIMLILIKLAR (Dependencies)
        // =====================================================
        
        private readonly IConfiguration _configuration; // API key için
        private readonly HttpClient _httpClient;        // HTTP istekleri için
        private readonly ILogger<AIService> _logger;    // Loglama için

        // =====================================================
        // CONSTRUCTOR (YAPICI METOD)
        // =====================================================
        /// <summary>
        /// AIService yapıcı metodu
        /// Dependency Injection ile gerekli servisler alınır
        /// </summary>
        public AIService(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ILogger<AIService> logger)
        {
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient();
            _logger = logger;
        }

        // =====================================================
        // ANA ÖNERİ METODU - GROQ API KULLANIMI
        // =====================================================
        /// <summary>
        /// Groq API kullanarak kişiselleştirilmiş fitness önerisi üretir
        /// Llama 3.1 70B modeli ile çalışır
        /// </summary>
        public async Task<AIOneriSonuc> OneriUretAsync(AIOneriIstek istek)
        {
            try
            {
                // Groq API anahtarını ve model bilgisini al
                var apiKey = _configuration["Groq:ApiKey"];
                var model = _configuration["Groq:Model"] ?? "llama-3.1-70b-versatile";

                // API anahtarı kontrolü
                if (string.IsNullOrEmpty(apiKey))
                {
                    _logger.LogError("Groq API anahtarı yapılandırılmamış!");
                    throw new InvalidOperationException("Groq API anahtarı yapılandırılmamış. Lütfen appsettings.json dosyasını kontrol edin.");
                }

                _logger.LogInformation("Groq API ile öneri üretiliyor. Model: {Model}", model);

                // API endpoint - Groq OpenAI uyumlu API kullanır
                var apiUrl = "https://api.groq.com/openai/v1/chat/completions";

                // Kullanıcı için özel prompt oluştur
                var prompt = OlusturPrompt(istek);

                // API isteği için JSON body oluştur
                var requestBody = new
                {
                    model = model,
                    messages = new[]
                    {
                        new 
                        { 
                            role = "system", 
                            content = @"Sen profesyonel bir fitness koçu ve beslenme uzmanısın. 
                            Türkçe yanıt ver. Yanıtlarını detaylı ve pratik yapılandır.
                            
                            Yanıtını şu formatta ver:
                            
                            ## 📊 BMI ANALİZİ
                            [BMI değerlendirmesi ve genel durum]
                            
                            ## 🏋️ HAFTALIK EGZERSİZ PROGRAMI
                            [Gün gün detaylı program - egzersizler, set/tekrar sayıları]
                            
                            ## 🥗 BESLENME ÖNERİLERİ
                            [Günlük kalori hedefi, makro besinler, örnek menü]
                            
                            ## 💡 GENEL TAVSİYELER VE UYARILAR
                            [Motivasyon, dinlenme, uyarılar]" 
                        },
                        new { role = "user", content = prompt }
                    },
                    max_tokens = 2500,      // Maksimum yanıt uzunluğu
                    temperature = 0.7       // Yaratıcılık seviyesi (0-1 arası)
                };

                // HTTP isteği hazırla (JSON formatında)
                var requestContent = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json"
                );

                // Authorization header ekle (Bearer token)
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                // API'ye POST isteği gönder
                var response = await _httpClient.PostAsync(apiUrl, requestContent);
                var responseContent = await response.Content.ReadAsStringAsync();

                // Yanıt başarılı mı kontrol et
                if (response.IsSuccessStatusCode)
                {
                    // JSON yanıtı parse et
                    using var document = JsonDocument.Parse(responseContent);
                    var root = document.RootElement;

                    // AI'dan gelen yanıt metnini çıkar
                    var aiYanit = root
                        .GetProperty("choices")[0]
                        .GetProperty("message")
                        .GetProperty("content")
                        .GetString() ?? "";

                    _logger.LogInformation("Groq API başarılı yanıt döndü. Karakter sayısı: {Length}", aiYanit.Length);

                    // Başarılı sonuç döndür
                    return new AIOneriSonuc
                    {
                        Basarili = true,
                        EgzersizOnerisi = aiYanit,                    // AI yanıtını olduğu gibi kullan
                        BMIYorumu = $"BMI: {istek.BMI} ({istek.BMIKategorisi})",
                        Istek = istek,
                        OlusturmaTarihi = DateTime.UtcNow             // UTC zamanı kullan (PostgreSQL uyumlu)
                    };
                }
                else
                {
                    // API hatası - hata mesajını logla
                    _logger.LogError("Groq API hatası: {StatusCode} - {Response}", 
                        response.StatusCode, responseContent);

                    // Hata sonucu döndür
                    return new AIOneriSonuc
                    {
                        Basarili = false,
                        HataMesaji = $"API Hatası ({response.StatusCode}). Lütfen daha sonra tekrar deneyin.",
                        Istek = istek,
                        OlusturmaTarihi = DateTime.UtcNow
                    };
                }
            }
            catch (HttpRequestException httpEx)
            {
                // Network/HTTP hataları
                _logger.LogError(httpEx, "Groq API'ye bağlanırken ağ hatası oluştu");

                return new AIOneriSonuc
                {
                    Basarili = false,
                    HataMesaji = "İnternet bağlantısı hatası. Lütfen bağlantınızı kontrol edin.",
                    Istek = istek,
                    OlusturmaTarihi = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                // Diğer tüm hatalar
                _logger.LogError(ex, "AI öneri üretilirken beklenmeyen hata oluştu");

                return new AIOneriSonuc
                {
                    Basarili = false,
                    HataMesaji = $"Beklenmeyen hata: {ex.Message}",
                    Istek = istek,
                    OlusturmaTarihi = DateTime.UtcNow
                };
            }
        }

        // =====================================================
        // PROMPT OLUŞTURMA
        // Kullanıcı bilgilerinden AI için prompt metni oluşturur
        // =====================================================
        /// <summary>
        /// AI'ya gönderilecek prompt metnini kullanıcı bilgilerinden oluşturur
        /// </summary>
        private string OlusturPrompt(AIOneriIstek istek)
        {
            var sb = new StringBuilder();

            // Başlık
            sb.AppendLine("Aşağıdaki bilgilere sahip bir kullanıcı için kişiselleştirilmiş, detaylı ve uygulanabilir bir fitness ve beslenme programı hazırla:");
            sb.AppendLine();

            // Kişisel bilgiler
            sb.AppendLine("👤 KİŞİSEL BİLGİLER:");
            sb.AppendLine($"• Yaş: {istek.Yas}");
            sb.AppendLine($"• Cinsiyet: {(istek.Cinsiyet == Cinsiyet.Erkek ? "Erkek" : "Kadın")}");
            sb.AppendLine($"• Boy: {istek.Boy} cm");
            sb.AppendLine($"• Kilo: {istek.Kilo} kg");
            sb.AppendLine($"• BMI: {istek.BMI} - Kategori: {istek.BMIKategorisi}");
            sb.AppendLine();

            // Hedefler ve tercihler
            sb.AppendLine("🎯 HEDEFLER VE TERCİHLER:");
            sb.AppendLine($"• Ana Hedef: {GetFitnessHedefiAdi(istek.FitnessHedefi)}");
            sb.AppendLine($"• Deneyim Seviyesi: {GetDeneyimSeviyesiAdi(istek.DeneyimSeviyesi)}");
            sb.AppendLine($"• Haftada Antrenman Günü: {istek.HaftalikGun} gün");
            sb.AppendLine($"• Günlük Antrenman Süresi: {istek.GunlukSureDakika} dakika");

            // Odak bölge varsa ekle
            if (istek.OdakBolge.HasValue)
            {
                sb.AppendLine($"• Odaklanmak İstediği Bölge: {GetVucutBolgesiAdi(istek.OdakBolge.Value)}");
            }

            // Sağlık notu varsa ekle
            if (!string.IsNullOrEmpty(istek.SaglikNotu))
            {
                sb.AppendLine();
                sb.AppendLine($"⚠️ SAĞLIK NOTU: {istek.SaglikNotu}");
                sb.AppendLine("(Bu durumu göz önünde bulundurarak öneriler sun)");
            }

            sb.AppendLine();
            sb.AppendLine("Lütfen yukarıdaki bilgilere göre:");
            sb.AppendLine("1. BMI analizi ve genel değerlendirme yap");
            sb.AppendLine("2. Haftalık egzersiz programı oluştur (gün gün, hangi hareketler, kaç set/tekrar)");
            sb.AppendLine("3. Beslenme önerileri sun (günlük kalori, protein/karbonhidrat/yağ oranları, örnek menü)");
            sb.AppendLine("4. Genel tavsiyeler ve uyarılar ver");
            sb.AppendLine();
            sb.AppendLine("Profesyonel, motive edici ve uygulanabilir bir program hazırla.");

            return sb.ToString();
        }

        // =====================================================
        // YARDIMCI METODLAR - Enum Adlarını Türkçeleştir
        // =====================================================
        
        /// <summary>
        /// Fitness hedefi enum'unu Türkçe metne çevirir
        /// </summary>
        private string GetFitnessHedefiAdi(FitnessHedefi hedef)
        {
            return hedef switch
            {
                FitnessHedefi.KiloVermek => "Kilo Vermek",
                FitnessHedefi.KasKazanmak => "Kas Kazanmak",
                FitnessHedefi.FormKorumak => "Form Korumak",
                FitnessHedefi.GucArtirmak => "Güç Artırmak",
                FitnessHedefi.EsneklikKazanmak => "Esneklik Kazanmak",
                FitnessHedefi.DayaniklilikArtirmak => "Dayanıklılık Artırmak",
                FitnessHedefi.StresAtmak => "Stres Atmak",
                FitnessHedefi.SaglikliYasam => "Sağlıklı Yaşam",
                _ => hedef.ToString()
            };
        }

        /// <summary>
        /// Deneyim seviyesi enum'unu Türkçe metne çevirir
        /// </summary>
        private string GetDeneyimSeviyesiAdi(DeneyimSeviyesi seviye)
        {
            return seviye switch
            {
                DeneyimSeviyesi.Baslangic => "Başlangıç",
                DeneyimSeviyesi.OrtaDuzey => "Orta Düzey",
                DeneyimSeviyesi.IleriDuzey => "İleri Düzey",
                DeneyimSeviyesi.Profesyonel => "Profesyonel",
                _ => seviye.ToString()
            };
        }

        /// <summary>
        /// Vücut bölgesi enum'unu Türkçe metne çevirir
        /// </summary>
        private string GetVucutBolgesiAdi(VucutBolgesi bolge)
        {
            return bolge switch
            {
                VucutBolgesi.TumVucut => "Tüm Vücut",
                VucutBolgesi.UstVucut => "Üst Vücut",
                VucutBolgesi.AltVucut => "Alt Vücut",
                VucutBolgesi.Karin => "Karın",
                VucutBolgesi.Sirt => "Sırt",
                VucutBolgesi.Gogus => "Göğüs",
                VucutBolgesi.Kol => "Kol",
                VucutBolgesi.Bacak => "Bacak",
                _ => bolge.ToString()
            };
        }
    }
}
