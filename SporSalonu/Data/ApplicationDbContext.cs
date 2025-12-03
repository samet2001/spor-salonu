// =====================================================
// APPLICATION DB CONTEXT SINIFI
// Bu dosya, Entity Framework Core ile veritabanı
// bağlantısını ve tabloları yöneten DbContext sınıfını tanımlar
// PostgreSQL veritabanı ile çalışır
// =====================================================

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;  // Identity DbContext için
using Microsoft.EntityFrameworkCore;                       // Entity Framework Core
using SporSalonu.Models;                                   // Model sınıfları

namespace SporSalonu.Data
{
    /// <summary>
    /// ApplicationDbContext - Uygulamanın ana veritabanı bağlam sınıfı
    /// IdentityDbContext'ten türetilmiştir (ASP.NET Core Identity desteği için)
    /// Uye sınıfı özel kullanıcı modeli olarak kullanılır
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<Uye>
    {
        // =====================================================
        // CONSTRUCTOR (YAPICI METOD)
        // DbContext options dependency injection ile alınır
        // =====================================================
        /// <summary>
        /// DbContext yapıcı metodu
        /// </summary>
        /// <param name="options">Veritabanı bağlantı ayarları</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) // Base class constructor'ı çağır
        {
            // Yapıcı metod - özel bir işlem yok
        }

        // =====================================================
        // DB SET'LER (VERİTABANI TABLOLARI)
        // Her DbSet bir veritabanı tablosuna karşılık gelir
        // =====================================================

        /// <summary>
        /// Spor Salonları tablosu
        /// Sistemdeki tüm spor salonlarını içerir
        /// </summary>
        public DbSet<Models.SporSalonu> SporSalonlari { get; set; }

        /// <summary>
        /// Hizmetler tablosu
        /// Salonların sunduğu hizmetleri içerir (Yoga, Pilates, Fitness vb.)
        /// </summary>
        public DbSet<Hizmet> Hizmetler { get; set; }

        /// <summary>
        /// Antrenörler tablosu
        /// Sistemdeki tüm antrenörleri içerir
        /// </summary>
        public DbSet<Antrenor> Antrenorler { get; set; }

        /// <summary>
        /// Antrenör-Hizmet ilişki tablosu
        /// Hangi antrenörün hangi hizmetleri verebildiğini tanımlar
        /// </summary>
        public DbSet<AntrenorHizmet> AntrenorHizmetleri { get; set; }

        /// <summary>
        /// Antrenör müsaitlik tablosu
        /// Antrenörlerin hangi gün ve saatlerde müsait olduğunu tanımlar
        /// </summary>
        public DbSet<AntrenorMusaitlik> AntrenorMusaitlikleri { get; set; }

        /// <summary>
        /// Randevular tablosu
        /// Üyelerin aldığı randevuları içerir
        /// </summary>
        public DbSet<Randevu> Randevular { get; set; }

        // NOT: Uye tablosu IdentityDbContext'ten miras alınır (AspNetUsers)

        // =====================================================
        // MODEL OLUŞTURMA (FLUENT API)
        // Entity ilişkilerini ve veritabanı yapılandırmasını tanımlar
        // =====================================================
        /// <summary>
        /// Model oluşturma metodu - Entity ilişkileri burada tanımlanır
        /// </summary>
        /// <param name="modelBuilder">Model oluşturucu</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Base class'ın OnModelCreating metodunu çağır (Identity tabloları için gerekli)
            base.OnModelCreating(modelBuilder);

            // =====================================================
            // SPOR SALONU YAPILANDIRMASI
            // =====================================================
            modelBuilder.Entity<Models.SporSalonu>(entity =>
            {
                // Tablo adını belirle
                entity.ToTable("SporSalonlari");

                // Primary key
                entity.HasKey(s => s.Id);

                // Ad alanı zorunlu ve benzersiz olmalı
                entity.HasIndex(s => s.Ad).IsUnique();

                // Açılış ve kapanış saatleri için varsayılan değerler
                entity.Property(s => s.AcilisSaati).HasDefaultValue(new TimeSpan(7, 0, 0));
                entity.Property(s => s.KapanisSaati).HasDefaultValue(new TimeSpan(22, 0, 0));
            });

            // =====================================================
            // HİZMET YAPILANDIRMASI
            // =====================================================
            modelBuilder.Entity<Hizmet>(entity =>
            {
                entity.ToTable("Hizmetler");
                entity.HasKey(h => h.Id);

                // Spor Salonu ile ilişki (Many-to-One)
                // Bir hizmet bir salona aittir, bir salonun birden fazla hizmeti olabilir
                entity.HasOne(h => h.SporSalonu)
                      .WithMany(s => s.Hizmetler)
                      .HasForeignKey(h => h.SporSalonuId)
                      .OnDelete(DeleteBehavior.Cascade); // Salon silinirse hizmetler de silinir

                // Kategori alanını enum olarak sakla
                entity.Property(h => h.Kategori)
                      .HasConversion<int>(); // Enum değerini integer olarak sakla
            });

            // =====================================================
            // ANTRENÖR YAPILANDIRMASI
            // =====================================================
            modelBuilder.Entity<Antrenor>(entity =>
            {
                entity.ToTable("Antrenorler");
                entity.HasKey(a => a.Id);

                // E-posta benzersiz olmalı
                entity.HasIndex(a => a.Eposta).IsUnique();

                // Spor Salonu ile ilişki (Many-to-One)
                entity.HasOne(a => a.SporSalonu)
                      .WithMany(s => s.Antrenorler)
                      .HasForeignKey(a => a.SporSalonuId)
                      .OnDelete(DeleteBehavior.Restrict); // Salon silinirse antrenörler korunsun
            });

            // =====================================================
            // ANTRENÖR-HİZMET İLİŞKİ YAPILANDIRMASI
            // Many-to-Many ilişki için ara tablo
            // =====================================================
            modelBuilder.Entity<AntrenorHizmet>(entity =>
            {
                entity.ToTable("AntrenorHizmetleri");
                entity.HasKey(ah => ah.Id);

                // Aynı antrenör-hizmet kombinasyonu tekrar edilemez (Unique Index)
                entity.HasIndex(ah => new { ah.AntrenorId, ah.HizmetId }).IsUnique();

                // Antrenör ile ilişki
                entity.HasOne(ah => ah.Antrenor)
                      .WithMany(a => a.AntrenorHizmetleri)
                      .HasForeignKey(ah => ah.AntrenorId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Hizmet ile ilişki
                entity.HasOne(ah => ah.Hizmet)
                      .WithMany(h => h.AntrenorHizmetleri)
                      .HasForeignKey(ah => ah.HizmetId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // =====================================================
            // ANTRENÖR MÜSAİTLİK YAPILANDIRMASI
            // =====================================================
            modelBuilder.Entity<AntrenorMusaitlik>(entity =>
            {
                entity.ToTable("AntrenorMusaitlikleri");
                entity.HasKey(am => am.Id);

                // Gün alanını integer olarak sakla
                entity.Property(am => am.Gun).HasConversion<int>();

                // Antrenör ile ilişki
                entity.HasOne(am => am.Antrenor)
                      .WithMany(a => a.Musaitlikler)
                      .HasForeignKey(am => am.AntrenorId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // =====================================================
            // RANDEVU YAPILANDIRMASI
            // =====================================================
            modelBuilder.Entity<Randevu>(entity =>
            {
                entity.ToTable("Randevular");
                entity.HasKey(r => r.Id);

                // Durum alanını integer olarak sakla
                entity.Property(r => r.Durum).HasConversion<int>();

                // Üye ile ilişki (Many-to-One)
                entity.HasOne(r => r.Uye)
                      .WithMany(u => u.Randevular)
                      .HasForeignKey(r => r.UyeId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Antrenör ile ilişki (Many-to-One)
                entity.HasOne(r => r.Antrenor)
                      .WithMany(a => a.Randevular)
                      .HasForeignKey(r => r.AntrenorId)
                      .OnDelete(DeleteBehavior.Restrict); // Antrenör silinirse randevular korunsun

                // Hizmet ile ilişki (Many-to-One)
                entity.HasOne(r => r.Hizmet)
                      .WithMany(h => h.Randevular)
                      .HasForeignKey(r => r.HizmetId)
                      .OnDelete(DeleteBehavior.Restrict); // Hizmet silinirse randevular korunsun

                // Tarih ve saat için index (sorgu performansı için)
                entity.HasIndex(r => r.RandevuTarihi);
                entity.HasIndex(r => new { r.AntrenorId, r.RandevuTarihi, r.BaslangicSaati });
            });

            // =====================================================
            // ÜYE (IDENTITY USER) YAPILANDIRMASI
            // =====================================================
            modelBuilder.Entity<Uye>(entity =>
            {
                // Cinsiyet, FitnessHedefi ve DeneyimSeviyesi alanlarını integer olarak sakla
                entity.Property(u => u.Cinsiyet).HasConversion<int?>();
                entity.Property(u => u.FitnessHedefi).HasConversion<int?>();
                entity.Property(u => u.DeneyimSeviyesi).HasConversion<int?>();
            });
        }
    }
}

