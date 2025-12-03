// =====================================================
// SEED DATA SINIFI
// Bu dosya, veritabanı ilk oluşturulduğunda
// varsayılan verileri ekler (Admin kullanıcı, roller,
// örnek spor salonu, antrenörler ve hizmetler)
// =====================================================

using Microsoft.AspNetCore.Identity;         // Identity için
using Microsoft.EntityFrameworkCore;         // Entity Framework
using SporSalonu.Models;                      // Model sınıfları

namespace SporSalonu.Data
{
    /// <summary>
    /// SeedData - Veritabanı başlangıç verilerini oluşturan statik sınıf
    /// Uygulama ilk çalıştığında rolleri, admin kullanıcısını ve örnek verileri ekler
    /// </summary>
    public static class SeedData
    {
        /// <summary>
        /// Veritabanı başlangıç verilerini oluşturur
        /// </summary>
        /// <param name="serviceProvider">Dependency Injection servis sağlayıcısı</param>
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            // =====================================================
            // SERVİSLERİ AL
            // =====================================================
            
            // RoleManager: Rolleri yönetmek için (Admin, Uye vb.)
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            
            // UserManager: Kullanıcıları yönetmek için
            var userManager = serviceProvider.GetRequiredService<UserManager<Uye>>();
            
            // DbContext: Veritabanı işlemleri için
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // =====================================================
            // ROLLERİ OLUŞTUR
            // Sistemde iki rol var: Admin ve Uye
            // =====================================================
            
            // Admin rolü
            string adminRolu = "Admin";
            if (!await roleManager.RoleExistsAsync(adminRolu))
            {
                // Admin rolü yoksa oluştur
                await roleManager.CreateAsync(new IdentityRole(adminRolu));
            }

            // Üye rolü
            string uyeRolu = "Uye";
            if (!await roleManager.RoleExistsAsync(uyeRolu))
            {
                // Üye rolü yoksa oluştur
                await roleManager.CreateAsync(new IdentityRole(uyeRolu));
            }

            // =====================================================
            // ADMİN KULLANICISINI OLUŞTUR
            // E-posta: b211210036@sakarya.edu.tr
            // Şifre: sau
            // =====================================================
            
            string adminEmail = "b211210036@sakarya.edu.tr";
            string adminSifre = "sau";

            // Admin kullanıcısı var mı kontrol et
            var adminKullanici = await userManager.FindByEmailAsync(adminEmail);
            
            if (adminKullanici == null)
            {
                // Admin kullanıcısı yoksa oluştur
                adminKullanici = new Uye
                {
                    UserName = adminEmail,           // Kullanıcı adı = e-posta
                    Email = adminEmail,              // E-posta adresi
                    Ad = "Admin",                    // Ad
                    Soyad = "Kullanıcı",             // Soyad
                    PhoneNumber = "05001234567",     // Telefon
                    EmailConfirmed = true,           // E-posta onaylı
                    AktifUyelik = true,              // Aktif üye
                    UyelikTarihi = DateTime.UtcNow,     // Üyelik tarihi
                    Cinsiyet = Cinsiyet.Erkek,       // Cinsiyet
                    DeneyimSeviyesi = DeneyimSeviyesi.Profesyonel // Deneyim
                };

                // Kullanıcıyı oluştur (şifre ile)
                var result = await userManager.CreateAsync(adminKullanici, adminSifre);
                
                if (result.Succeeded)
                {
                    // Admin rolünü ata
                    await userManager.AddToRoleAsync(adminKullanici, adminRolu);
                }
            }
            else
            {
                // Admin varsa, Admin rolüne sahip mi kontrol et
                if (!await userManager.IsInRoleAsync(adminKullanici, adminRolu))
                {
                    await userManager.AddToRoleAsync(adminKullanici, adminRolu);
                }
            }

            // =====================================================
            // ÖRNEK ÜYE KULLANICISI OLUŞTUR
            // Test amaçlı normal üye
            // =====================================================
            
            string uyeEmail = "uye@test.com";
            string uyeSifre = "123";

            var uyeKullanici = await userManager.FindByEmailAsync(uyeEmail);
            
            if (uyeKullanici == null)
            {
                uyeKullanici = new Uye
                {
                    UserName = uyeEmail,
                    Email = uyeEmail,
                    Ad = "Test",
                    Soyad = "Üye",
                    PhoneNumber = "05009876543",
                    EmailConfirmed = true,
                    AktifUyelik = true,
                    UyelikTarihi = DateTime.UtcNow,
                    Cinsiyet = Cinsiyet.Erkek,
                    Boy = 175,
                    Kilo = 75,
                    FitnessHedefi = FitnessHedefi.FormKorumak,
                    DeneyimSeviyesi = DeneyimSeviyesi.OrtaDuzey
                };

                var result = await userManager.CreateAsync(uyeKullanici, uyeSifre);
                
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(uyeKullanici, uyeRolu);
                }
            }

            // =====================================================
            // ÖRNEK SPOR SALONU OLUŞTUR
            // =====================================================
            
            if (!await context.SporSalonlari.AnyAsync())
            {
                var sporSalonu = new Models.SporSalonu
                {
                    Ad = "FitLife Spor Merkezi",
                    Adres = "Sakarya Üniversitesi Kütüphanesi, Esentepe Kampüsü, 54050 Serdivan/Sakarya",
                    Telefon = "0264 123 45 67",
                    Eposta = "info@fitlife.com",
                    AcilisSaati = new TimeSpan(7, 0, 0),    // 07:00
                    KapanisSaati = new TimeSpan(23, 0, 0),  // 23:00
                    Aciklama = "FitLife Spor Merkezi, modern ekipmanları ve uzman kadrosuyla sağlıklı yaşam yolculuğunuzda yanınızda. Fitness, yoga, pilates ve daha fazlası için bizi ziyaret edin!",
                    Aktif = true,
                    KayitTarihi = DateTime.UtcNow
                };

                context.SporSalonlari.Add(sporSalonu);
                await context.SaveChangesAsync();

                // =====================================================
                // ÖRNEK HİZMETLER OLUŞTUR
                // =====================================================
                
                var hizmetler = new List<Hizmet>
                {
                    new Hizmet
                    {
                        Ad = "Fitness (Birebir)",
                        Aciklama = "Kişiye özel fitness antrenmanı. Hedeflerinize uygun program hazırlanır.",
                        SureDakika = 60,
                        Ucret = 250,
                        Kategori = HizmetKategorisi.Fitness,
                        MaksimumKatilimci = 1,
                        SporSalonuId = sporSalonu.Id,
                        Aktif = true
                    },
                    new Hizmet
                    {
                        Ad = "Yoga Seansı",
                        Aciklama = "Zihin ve beden uyumu için yoga dersi. Stres atın, esneklik kazanın.",
                        SureDakika = 75,
                        Ucret = 150,
                        Kategori = HizmetKategorisi.Yoga,
                        MaksimumKatilimci = 15,
                        SporSalonuId = sporSalonu.Id,
                        Aktif = true
                    },
                    new Hizmet
                    {
                        Ad = "Pilates",
                        Aciklama = "Core kaslarını güçlendirin, duruşunuzu düzeltin.",
                        SureDakika = 60,
                        Ucret = 175,
                        Kategori = HizmetKategorisi.Pilates,
                        MaksimumKatilimci = 10,
                        SporSalonuId = sporSalonu.Id,
                        Aktif = true
                    },
                    new Hizmet
                    {
                        Ad = "Kardio Antrenmanı",
                        Aciklama = "Yüksek tempolu kardio egzersizleri ile kalori yakın.",
                        SureDakika = 45,
                        Ucret = 100,
                        Kategori = HizmetKategorisi.Kardio,
                        MaksimumKatilimci = 20,
                        SporSalonuId = sporSalonu.Id,
                        Aktif = true
                    },
                    new Hizmet
                    {
                        Ad = "Kas Geliştirme Programı",
                        Aciklama = "Kas kütlesi artırmak isteyenler için özel program.",
                        SureDakika = 90,
                        Ucret = 300,
                        Kategori = HizmetKategorisi.KasGelistirme,
                        MaksimumKatilimci = 1,
                        SporSalonuId = sporSalonu.Id,
                        Aktif = true
                    },
                    new Hizmet
                    {
                        Ad = "Kilo Verme Programı",
                        Aciklama = "Sağlıklı kilo vermek için kombine antrenman ve beslenme desteği.",
                        SureDakika = 60,
                        Ucret = 275,
                        Kategori = HizmetKategorisi.KiloVerme,
                        MaksimumKatilimci = 1,
                        SporSalonuId = sporSalonu.Id,
                        Aktif = true
                    },
                    new Hizmet
                    {
                        Ad = "Crossfit",
                        Aciklama = "Yüksek yoğunluklu fonksiyonel antrenman.",
                        SureDakika = 60,
                        Ucret = 200,
                        Kategori = HizmetKategorisi.Crossfit,
                        MaksimumKatilimci = 12,
                        SporSalonuId = sporSalonu.Id,
                        Aktif = true
                    },
                    new Hizmet
                    {
                        Ad = "Boks Antrenmanı",
                        Aciklama = "Boks teknikleri ve kondisyon antrenmanı.",
                        SureDakika = 60,
                        Ucret = 225,
                        Kategori = HizmetKategorisi.Boks,
                        MaksimumKatilimci = 8,
                        SporSalonuId = sporSalonu.Id,
                        Aktif = true
                    }
                };

                context.Hizmetler.AddRange(hizmetler);
                await context.SaveChangesAsync();

                // =====================================================
                // ÖRNEK ANTRENÖRLER OLUŞTUR
                // =====================================================
                
                var antrenorler = new List<Antrenor>
                {
                    new Antrenor
                    {
                        Ad = "Ahmet",
                        Soyad = "Yılmaz",
                        Eposta = "ahmet.yilmaz@fitlife.com",
                        Telefon = "0532 111 22 33",
                        UzmanlikAlanlari = "Fitness, Kas Geliştirme, Kilo Verme",
                        Biyografi = "10 yıllık deneyime sahip fitness eğitmeni. IFBB sertifikalı.",
                        MesaiBaslangic = new TimeSpan(8, 0, 0),
                        MesaiBitis = new TimeSpan(18, 0, 0),
                        SeansUcreti = 200,
                        DeneyimYili = 10,
                        SporSalonuId = sporSalonu.Id,
                        Aktif = true,
                        KayitTarihi = DateTime.UtcNow
                    },
                    new Antrenor
                    {
                        Ad = "Elif",
                        Soyad = "Demir",
                        Eposta = "elif.demir@fitlife.com",
                        Telefon = "0533 222 33 44",
                        UzmanlikAlanlari = "Yoga, Pilates, Esneklik",
                        Biyografi = "Uluslararası yoga eğitmeni sertifikası. 7 yıllık deneyim.",
                        MesaiBaslangic = new TimeSpan(9, 0, 0),
                        MesaiBitis = new TimeSpan(19, 0, 0),
                        SeansUcreti = 175,
                        DeneyimYili = 7,
                        SporSalonuId = sporSalonu.Id,
                        Aktif = true,
                        KayitTarihi = DateTime.UtcNow
                    },
                    new Antrenor
                    {
                        Ad = "Mehmet",
                        Soyad = "Kaya",
                        Eposta = "mehmet.kaya@fitlife.com",
                        Telefon = "0534 333 44 55",
                        UzmanlikAlanlari = "Crossfit, Kardio, Fonksiyonel Antrenman",
                        Biyografi = "CrossFit Level 2 sertifikalı. Eski milli sporcu.",
                        MesaiBaslangic = new TimeSpan(7, 0, 0),
                        MesaiBitis = new TimeSpan(17, 0, 0),
                        SeansUcreti = 225,
                        DeneyimYili = 8,
                        SporSalonuId = sporSalonu.Id,
                        Aktif = true,
                        KayitTarihi = DateTime.UtcNow
                    },
                    new Antrenor
                    {
                        Ad = "Zeynep",
                        Soyad = "Arslan",
                        Eposta = "zeynep.arslan@fitlife.com",
                        Telefon = "0535 444 55 66",
                        UzmanlikAlanlari = "Boks, Kickboks, Dövüş Sanatları",
                        Biyografi = "Türkiye şampiyonu eski boksör. 12 yıllık deneyim.",
                        MesaiBaslangic = new TimeSpan(10, 0, 0),
                        MesaiBitis = new TimeSpan(20, 0, 0),
                        SeansUcreti = 250,
                        DeneyimYili = 12,
                        SporSalonuId = sporSalonu.Id,
                        Aktif = true,
                        KayitTarihi = DateTime.UtcNow
                    }
                };

                context.Antrenorler.AddRange(antrenorler);
                await context.SaveChangesAsync();

                // =====================================================
                // ANTRENÖR-HİZMET İLİŞKİLERİNİ OLUŞTUR
                // Hangi antrenör hangi hizmeti verebilir
                // =====================================================
                
                var antrenorHizmetler = new List<AntrenorHizmet>();

                // Ahmet: Fitness, Kas Geliştirme, Kilo Verme
                var ahmet = antrenorler[0];
                var fitnessHizmet = hizmetler.First(h => h.Kategori == HizmetKategorisi.Fitness);
                var kasHizmet = hizmetler.First(h => h.Kategori == HizmetKategorisi.KasGelistirme);
                var kiloHizmet = hizmetler.First(h => h.Kategori == HizmetKategorisi.KiloVerme);
                
                antrenorHizmetler.Add(new AntrenorHizmet { AntrenorId = ahmet.Id, HizmetId = fitnessHizmet.Id, SertifikaliMi = true });
                antrenorHizmetler.Add(new AntrenorHizmet { AntrenorId = ahmet.Id, HizmetId = kasHizmet.Id, SertifikaliMi = true });
                antrenorHizmetler.Add(new AntrenorHizmet { AntrenorId = ahmet.Id, HizmetId = kiloHizmet.Id, SertifikaliMi = true });

                // Elif: Yoga, Pilates
                var elif = antrenorler[1];
                var yogaHizmet = hizmetler.First(h => h.Kategori == HizmetKategorisi.Yoga);
                var pilatesHizmet = hizmetler.First(h => h.Kategori == HizmetKategorisi.Pilates);
                
                antrenorHizmetler.Add(new AntrenorHizmet { AntrenorId = elif.Id, HizmetId = yogaHizmet.Id, SertifikaliMi = true });
                antrenorHizmetler.Add(new AntrenorHizmet { AntrenorId = elif.Id, HizmetId = pilatesHizmet.Id, SertifikaliMi = true });

                // Mehmet: Crossfit, Kardio
                var mehmet = antrenorler[2];
                var crossfitHizmet = hizmetler.First(h => h.Kategori == HizmetKategorisi.Crossfit);
                var kardioHizmet = hizmetler.First(h => h.Kategori == HizmetKategorisi.Kardio);
                
                antrenorHizmetler.Add(new AntrenorHizmet { AntrenorId = mehmet.Id, HizmetId = crossfitHizmet.Id, SertifikaliMi = true });
                antrenorHizmetler.Add(new AntrenorHizmet { AntrenorId = mehmet.Id, HizmetId = kardioHizmet.Id, SertifikaliMi = true });

                // Zeynep: Boks
                var zeynep = antrenorler[3];
                var boksHizmet = hizmetler.First(h => h.Kategori == HizmetKategorisi.Boks);
                
                antrenorHizmetler.Add(new AntrenorHizmet { AntrenorId = zeynep.Id, HizmetId = boksHizmet.Id, SertifikaliMi = true });

                context.AntrenorHizmetleri.AddRange(antrenorHizmetler);
                await context.SaveChangesAsync();

                // =====================================================
                // ANTRENÖR MÜSAİTLİKLERİNİ OLUŞTUR
                // Haftalık müsaitlik programı
                // =====================================================
                
                var musaitlikler = new List<AntrenorMusaitlik>();

                // Her antrenör için Pazartesi-Cumartesi müsaitlik ekle
                foreach (var antrenor in antrenorler)
                {
                    // Pazartesi - Cuma
                    for (int i = 1; i <= 5; i++) // 1=Pazartesi, 5=Cuma
                    {
                        musaitlikler.Add(new AntrenorMusaitlik
                        {
                            AntrenorId = antrenor.Id,
                            Gun = (DayOfWeek)i,
                            BaslangicSaati = antrenor.MesaiBaslangic,
                            BitisSaati = antrenor.MesaiBitis,
                            MusaitMi = true
                        });
                    }

                    // Cumartesi (yarım gün)
                    musaitlikler.Add(new AntrenorMusaitlik
                    {
                        AntrenorId = antrenor.Id,
                        Gun = DayOfWeek.Saturday,
                        BaslangicSaati = new TimeSpan(9, 0, 0),
                        BitisSaati = new TimeSpan(14, 0, 0),
                        MusaitMi = true,
                        Not = "Hafta sonu yarım gün çalışma"
                    });
                }

                context.AntrenorMusaitlikleri.AddRange(musaitlikler);
                await context.SaveChangesAsync();
            }
        }
    }
}

