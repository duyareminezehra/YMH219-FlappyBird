# YMH219-FlappyBird — Flappy Bird Deluxe Masaüstü Oyunu

**Nesne Tabanlı Programlama (YMH 219)** dersi 1. uygulama ödevi kapsamında geliştirilmiş, C# ve Windows Forms tabanlı kapsamlı ve zenginleştirilmiş **Flappy Bird Deluxe** masaüstü oyunudur. Proje; zamanlayıcı (`Timer`) ile oyun döngüsü yönetimini, klavye/fare olaylarını, çarpışma algılamayı (`Rectangle.IntersectsWith`), toplanabilir altın ve güçlendiricileri, parçacık efektlerini, mağaza/gardırop sistemini ve saf C# tabanlı prosedürel ses sentezleyicisini nesne tabanlı (OOP) mimariyle sunmaktadır.

---

## Canlı Web / Mobil Sürüm
Oyunu tarayıcı ve akıllı telefonlarda denemek için:
👉 **[Flappy Bird Deluxe Web Oyunu](https://duyareminezehra.github.io/YMH219-FlappyBird/)**

---

## Kullanılan Teknoloji ve Kütüphaneler

* **Dil:** C# (.NET 10 / Windows Forms)
* **Grafik Altyapısı:** `System.Drawing` / `System.Drawing.Drawing2D` (Çift tamponlu `DoubleBufferedPanel` ile 60 FPS titreşimsiz GDI+ çizimi)
* **Ses Altyapısı:** `Services/SoundEffects.cs` (Bellekte anlık PCM WAV başlığı ve dalga formu üreten prosedürel sentezleyici — harici ses dosyası gerektirmez!)
* **Kalıcı Depolama:** `System.Text.Json` (`flappy_save.json` profil, cüzdan ve liderlik tablosu)
* **Harici NuGet Paketi:** Yok (Tamamen standart .NET bileşenleri ile geliştirilmiş, taşınabilir ve güvenilir)

---

## Sınıf ve Mimari Yapı (OOP Tasarımı)

Proje, sorumlulukların ayrılması (Separation of Concerns) prensibine uygun olarak katmanlı sınıflarla tasarlanmıştır:

* **`Models/Bird.cs`:** Oyuncunun kontrol ettiği kuşun konumunu (`X`, `Y`), dikey hızını (`VerticalVelocity`), yer çekimi ivmesini (`Gravity`), zıplama kuvvetini (`JumpStrength`) ve çarpışma sınırlarını (`Bounds`) kapsüller.
* **`Models/Coin.cs`:** Havada süzülen altın paraları, salınım açısını (`BobbingAngle`), mıknatıs çekimini (`AttractTowards`) ve toplanma durumunu yönetir.
* **`Models/PowerUp.cs`:** Boruların arasında beliren 4 farklı sürpriz güçlendirici küresini (`Shield`, `Magnet`, `SlowMo`, `DoubleScore`) kapsüller.
* **`Models/Particle.cs`:** Zıplama tüyleri, altın pırıltıları, kalkan patlama kıvılcımları, konfetiler ve havada süzülen yazı efektlerini (`FloatingText`) yönetir.
* **`Models/PipePair.cs`:** Ekranda sağdan sola kayan engel çiftini, geçiş boşluğunu (`GapTopY`, `GapHeight`) ve yeniden konumlandırmayı (`Reposition`) yönetir.
* **`Models/DifficultyLevel.cs`:** `Kolay`, `Normal` ve `Zor` seviyelerine ait hız, boru aralığı ve yer çekimi parametrelerini tutar.
* **`Models/VisualThemes.cs`:** 5 farklı temayı (`Gündüz`, `Gün Batımı`, `Gece`, `Karlı Kış`, `Neon Siber`) ve 8 farklı kuş karakterini (`Sarı Kanarya`, `Kızıl Anka`, `Mavi Fırtına`, `Zümrüt Kuşu`, `Mor Gölge`, `Polis Kuşu 👮`, `Kral Kuş 👑`, `Siber Kuş 🚀`) mağaza fiyatları ve renk paletleriyle yönetir.
* **`Models/HighScoreEntry.cs`:** Liderlik tablosundaki skor, altın, tarih ve kuş karakteri kayıtlarını tutar.
* **`Services/ProfileManager.cs`:** Kalıcı altın cüzdanını (`CoinsBalance`), kilit açılan kostüm ve temaları ve en iyi 5 skoru `flappy_save.json` dosyasında yönetir.
* **`Services/SoundEffects.cs`:** Harici WAV dosyası gerektirmeden bellekte prosedürel retro ses dalgaları (çırpma, altın, kalkan, kaza, yeni rekor) üreten ses motoru.
* **`Services/GameManager.cs`:** Oyun döngüsünü (`UpdateFrame`), boru/altın/güçlendirici hareketlerini, kalkan koruma mantığını (`HasShield`), mıknatıs çekimini ve skor yönetimini kontrol eder.
* **`Forms/ShopDialog.cs`:** Oyuncunun topladığı altınlarla kostüm ve tema satın alabildiği mağaza penceresi.
* **`Forms/LeaderboardDialog.cs`:** En iyi 5 oyun sonucunu listeleyen sıralama tablosu.
* **`Forms/MainForm.cs` & `Forms/DoubleBufferedPanel.cs`:** Kullanıcı arayüzünü, klavye/fare etkileşimlerini, HUD sayaçlarını ve 60 FPS GDI+ çizimini yönetir.

---

## Oyun Mekanikleri & Özellikler

### 1. Toplanabilir Altınlar ve Cüzdan
* Boru boşluklarında ve uçuş yollarında altın paralar belirir.
* Altın toplandığında pırıltı efekti saçar, ses efekti çalar ve cüzdana eklenir.
* Toplanan altınlar kalıcıdır; oyunu kapatsanız bile silinmez!

### 2. Sürpriz Güçlendiriciler (Power-Ups)
* 🛡️ **Kalkan (Shield):** Kuşun etrafında mavi enerji küresi oluşturur. **Bir boru çarpışmasından kuşu tamamen korur!** Çarpışmada kalkan patlar ve kuş hayatta kalarak uçmaya devam eder.
* 🧲 **Altın Mıknatısı (Magnet):** 8 saniye boyunca ekrandaki tüm altınları kuşa doğru çeker.
* ⏱️ **Ağır Çekim (Slow-Motion):** 5 saniye boyunca oyun hızını ve yer çekimini %45 yavaşlatarak oyuncuya rahat manevra sağlar.
* 🌟 **2x Skor Çarpanı (Double Score):** 8 saniye boyunca borulardan ve altınlardan 2 kat puan kazandırır.

### 3. Kuş & Tema Mağazası (Shop)
Toplanan altınlarla dükkandan özel görünümler açılabilir:
* **Kuş Kostümleri:** Sarı Kanarya (Ücretsiz), Kızıl Anka, Mavi Fırtına, Zümrüt Kuşu, Mor Gölge, **Polis Kuşu 👮** (Kasket, kravat ve altın rozet), **Kral Kuş 👑** (Altın taç ve kırmızı pelerin), **Siber Kuş 🚀** (Sibernetik kask ve jet alevi).
* **Arka Plan Temaları:** Gündüz, Gün Batımı, Yıldızlı Gece, Karlı Kış, **Neon Siber Şehir 🌆** (Neon binalar ve siberpunk ızgara zemin).

### 4. Parçacık Efektleri (Particle Engine)
* Kanat çırpma tüyleri, altın toplama parıltıları, kalkan kırılma kıvılcımları, yeni rekor konfeti patlaması ve uçuşan metinler (`+1 Altın`, `🛡️ KALKAN!`).

### 5. Liderlik Tablosu (Leaderboard)
* En yüksek 5 skor, kazanılan altın, tarih ve kullanılan kuş karakteri ile kaydedilir ve arayüzdeki "Sıralama 🏆" butonundan görüntülenebilir.

---

## Kurulum ve Çalıştırma

1. Depoyu bilgisayarınıza klonlayın:
   ```bash
   git clone https://github.com/duyareminezehra/YMH219-FlappyBird.git
   cd YMH219-FlappyBird
   ```
2. Terminal üzerinden derleyip çalıştırın:
   ```bash
   dotnet build
   dotnet run
   ```
3. **Oyun Kontrolleri:**
   * **`SPACE` / `Yukarı Ok` / `W` / `Sol Tık`:** Kuşu zıplatır / oyunu başlatır.
   * **`P` Tuşu veya `Duraklat` Butonu:** Oyunu duraklatır / devam ettirir.
   * **`M` Tuşu:** Sesi açar / kapatır.
   * **`Dükkan 🛍️` Butonu:** Kostüm ve tema mağazasını açar.
   * **`Sıralama 🏆` Butonu:** Liderlik tablosunu açar.

---

## Ekran Görüntüleri

### 1. Oyun İçi Aktif Görünüm (Kral Kuş & Neon Tema)
![Flappy Bird Oyun İçi Görünüm](screenshots/flappybird_gameplay.png)

### 2. Çarpışma ve Game Over Seçenek Ekranı (Polis Kuşu)
![Flappy Bird Game Over Görünümü](screenshots/flappybird_gameover.png)

---

## Öğrenci Bilgileri

* **Adı Soyadı:** Emine Zehra Duyar
* **Öğrenci Numarası:** 250541027
* **Ders:** YMH 219 — Nesne Tabanlı Programlama
