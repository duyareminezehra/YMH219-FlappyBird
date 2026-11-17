using System.Collections.Generic;
using System.Drawing;

namespace YMH219_FlappyBird.Models
{
    /// <summary>
    /// Oyun arka plan temalarını tanımlar.
    /// </summary>
    public enum BackgroundTheme
    {
        Gunduz,
        GunBatimi,
        Gece,
        KarliKis,
        NeonSiber
    }

    /// <summary>
    /// Oyuncunun seçebileceği kuş karakteri/renk görünümlerini tanımlar.
    /// </summary>
    public enum BirdSkin
    {
        SariKanarya,
        KizilAnka,
        MaviFirtina,
        ZumrutKusu,
        MorGolge,
        PolisKusu,
        AltinKral,
        SiberRoket
    }

    /// <summary>
    /// Seçilen arka plan teması ve kuş görünümüne ait renk paletlerini döndüren OOP yardımcı sınıfı.
    /// </summary>
    public static class ThemePalette
    {
        public static readonly Dictionary<BirdSkin, (string Name, int Price, string Description)> SkinStoreCatalog = new()
        {
            { BirdSkin.SariKanarya, ("Sarı Kanarya", 0, "Klasik sevimli kanarya. (Başlangıç Kuşu)") },
            { BirdSkin.KizilAnka, ("Kızıl Anka", 40, "Küllerinden doğan ateş kuşu.") },
            { BirdSkin.MaviFirtina, ("Mavi Fırtına", 80, "Gökyüzünün hızlı şimşek kuşu.") },
            { BirdSkin.ZumrutKusu, ("Zümrüt Kuşu", 120, "Zümrüt yeşili parlak tüyler.") },
            { BirdSkin.MorGolge, ("Mor Gölge", 160, "Gece süzülen gizemli gölge kuşu.") },
            { BirdSkin.PolisKusu, ("Polis Kuşu 👮", 200, "Resmi polis kepi, kravatı ve altın brövesi!") },
            { BirdSkin.AltinKral, ("Kral Kuş 👑", 300, "Saf altın taç ve kraliyet pelerini!") },
            { BirdSkin.SiberRoket, ("Siber Kuş 🚀", 400, "Sibernetik kask ve arka jet iticisi!") }
        };

        public static readonly Dictionary<BackgroundTheme, (string Name, int Price, string Description)> ThemeStoreCatalog = new()
        {
            { BackgroundTheme.Gunduz, ("Gündüz (Klasik)", 0, "Ferah mavi gökyüzü ve yeşil zemin.") },
            { BackgroundTheme.GunBatimi, ("Gün Batımı", 50, "Büyüleyici turuncu ve kızıl ufuk.") },
            { BackgroundTheme.Gece, ("Yıldızlı Gece", 100, "Hilal, parlak yıldızlar ve koyu lacivert gökyüzü.") },
            { BackgroundTheme.KarliKis, ("Karlı Kış", 150, "Buz mavisi kış manzarası ve kar örtüsü.") },
            { BackgroundTheme.NeonSiber, ("Neon Siber Şehir", 250, "Siberpunk mor-mavi gökyüzü ve neon yeşil borular!") }
        };

        public static (Color SkyTop, Color SkyBottom, Color GroundSoil, Color GroundGrass, Color GroundStripe, Color PipeLight, Color PipeDark, bool ShowStars, bool IsNeon) GetBackgroundColors(BackgroundTheme theme)
        {
            return theme switch
            {
                BackgroundTheme.GunBatimi => (
                    Color.FromArgb(124, 45, 18),
                    Color.FromArgb(251, 146, 60),
                    Color.FromArgb(120, 53, 15),
                    Color.FromArgb(180, 83, 9),
                    Color.FromArgb(217, 119, 6),
                    Color.FromArgb(20, 184, 166),
                    Color.FromArgb(15, 118, 110),
                    false,
                    false
                ),
                BackgroundTheme.Gece => (
                    Color.FromArgb(15, 23, 42),
                    Color.FromArgb(49, 46, 129),
                    Color.FromArgb(30, 41, 59),
                    Color.FromArgb(16, 185, 129),
                    Color.FromArgb(52, 211, 153),
                    Color.FromArgb(56, 189, 248),
                    Color.FromArgb(3, 105, 161),
                    true,
                    false
                ),
                BackgroundTheme.KarliKis => (
                    Color.FromArgb(125, 211, 252),
                    Color.FromArgb(240, 249, 255),
                    Color.FromArgb(148, 163, 184),
                    Color.FromArgb(248, 250, 252),
                    Color.FromArgb(226, 232, 240),
                    Color.FromArgb(59, 130, 246),
                    Color.FromArgb(29, 78, 216),
                    false,
                    false
                ),
                BackgroundTheme.NeonSiber => (
                    Color.FromArgb(49, 10, 82),
                    Color.FromArgb(15, 23, 42),
                    Color.FromArgb(30, 10, 60),
                    Color.FromArgb(236, 72, 153),
                    Color.FromArgb(244, 114, 182),
                    Color.FromArgb(34, 211, 238),
                    Color.FromArgb(6, 182, 212),
                    true,
                    true
                ),
                _ => (
                    Color.FromArgb(56, 189, 248),
                    Color.FromArgb(186, 230, 253),
                    Color.FromArgb(217, 160, 102),
                    Color.FromArgb(101, 163, 13),
                    Color.FromArgb(132, 204, 22),
                    Color.FromArgb(34, 197, 94),
                    Color.FromArgb(21, 128, 61),
                    false,
                    false
                )
            };
        }

        public static (Color BodyColor, Color WingColor, Color BeakColor, Color OutlineColor) GetBirdColors(BirdSkin skin)
        {
            return skin switch
            {
                BirdSkin.KizilAnka => (
                    Color.FromArgb(239, 68, 68),
                    Color.FromArgb(253, 224, 71),
                    Color.FromArgb(249, 115, 22),
                    Color.FromArgb(127, 29, 29)
                ),
                BirdSkin.MaviFirtina => (
                    Color.FromArgb(14, 165, 233),
                    Color.FromArgb(224, 242, 254),
                    Color.FromArgb(250, 204, 21),
                    Color.FromArgb(12, 74, 110)
                ),
                BirdSkin.ZumrutKusu => (
                    Color.FromArgb(16, 185, 129),
                    Color.FromArgb(167, 243, 208),
                    Color.FromArgb(249, 115, 22),
                    Color.FromArgb(6, 78, 59)
                ),
                BirdSkin.MorGolge => (
                    Color.FromArgb(168, 85, 247),
                    Color.FromArgb(245, 208, 254),
                    Color.FromArgb(250, 204, 21),
                    Color.FromArgb(88, 28, 135)
                ),
                BirdSkin.PolisKusu => (
                    Color.FromArgb(30, 58, 138),   // Lacivert polis üniforması
                    Color.FromArgb(59, 130, 246),  // Mavi kanat
                    Color.FromArgb(249, 115, 22),  // Turuncu gaga
                    Color.FromArgb(15, 23, 42)     // Koyu dış hat
                ),
                BirdSkin.AltinKral => (
                    Color.FromArgb(245, 158, 11),  // Parlak altın gövde
                    Color.FromArgb(254, 240, 138), // Açık altın kanat
                    Color.FromArgb(249, 115, 22),  // Turuncu gaga
                    Color.FromArgb(146, 64, 14)    // Bronz dış hat
                ),
                BirdSkin.SiberRoket => (
                    Color.FromArgb(71, 85, 105),   // Siber metal gövde
                    Color.FromArgb(56, 189, 248),  // Neon mavi kanat
                    Color.FromArgb(236, 72, 153),  // Neon pembe gaga/sensör
                    Color.FromArgb(15, 23, 42)     // Koyu dış hat
                ),
                _ => (
                    Color.FromArgb(250, 204, 21),
                    Color.FromArgb(254, 240, 138),
                    Color.FromArgb(249, 115, 22),
                    Color.FromArgb(113, 63, 18)
                )
            };
        }
    }
}
