#!/bin/bash
echo "=== GitHub'a Yükleme Başlıyor ==="

# Git başlat
git init
echo "✓ Git repository başlatıldı"

# Dosyaları ekle
git add .
echo "✓ Dosyalar eklendi"

# İlk commit
git commit -m "İlk commit: FitLife Spor Salonu Yönetim Sistemi - ASP.NET Core MVC, PostgreSQL, AI Entegrasyonu"
echo "✓ İlk commit oluşturuldu"

echo ""
echo "=== Şimdi GitHub'dan aldığınız URL'yi girin ==="
echo "Örnek: https://github.com/kullaniciadi/spor-salonu-yonetim-sistemi.git"
read -p "GitHub Repository URL: " REPO_URL

# Remote ekle
git remote add origin "$REPO_URL"
echo "✓ Remote repository eklendi"

# Branch adını main yap
git branch -M main
echo "✓ Branch main olarak ayarlandı"

# GitHub'a push et
git push -u origin main
echo "✓ Kod GitHub'a yüklendi!"

echo ""
echo "=== Tamamlandı! ==="
echo "Projeniz GitHub'a başarıyla yüklendi."
echo "URL: $REPO_URL"
