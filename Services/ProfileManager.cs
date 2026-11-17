using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using YMH219_FlappyBird.Models;

namespace YMH219_FlappyBird.Services
{
    public class SaveDataModel
    {
        public int CoinsBalance { get; set; } = 0;
        public int HighScore { get; set; } = 0;
        public List<string> UnlockedSkins { get; set; } = new() { BirdSkin.SariKanarya.ToString() };
        public List<string> UnlockedThemes { get; set; } = new() { BackgroundTheme.Gunduz.ToString() };
        public List<HighScoreEntry> Leaderboard { get; set; } = new();
    }

    /// <summary>
    /// Oyuncunun altın bakiyesini, satın aldığı kostümleri/temaları ve
    /// en yüksek 5 skordan oluşan Liderlik Tablosunu yerel dosyada kalıcı olarak yöneten OOP servisi.
    /// </summary>
    public class ProfileManager
    {
        private static ProfileManager? _instance;
        public static ProfileManager Instance => _instance ??= new ProfileManager();

        private readonly string _saveFilePath;
        private readonly string _legacyHighScorePath;
        private SaveDataModel _data = new();

        public int CoinsBalance => _data.CoinsBalance;
        public int HighScore => _data.HighScore;
        public IReadOnlyList<HighScoreEntry> Leaderboard => _data.Leaderboard;

        public event EventHandler? BalanceChanged;

        private ProfileManager()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            _saveFilePath = Path.Combine(baseDir, "flappy_save.json");
            _legacyHighScorePath = Path.Combine(baseDir, "highscore.txt");
            Load();
        }

        public bool IsSkinUnlocked(BirdSkin skin)
        {
            return _data.UnlockedSkins.Contains(skin.ToString());
        }

        public bool IsThemeUnlocked(BackgroundTheme theme)
        {
            return _data.UnlockedThemes.Contains(theme.ToString());
        }

        public bool TryBuySkin(BirdSkin skin)
        {
            if (IsSkinUnlocked(skin)) return true;

            int price = ThemePalette.SkinStoreCatalog[skin].Price;
            if (_data.CoinsBalance >= price)
            {
                _data.CoinsBalance -= price;
                _data.UnlockedSkins.Add(skin.ToString());
                Save();
                BalanceChanged?.Invoke(this, EventArgs.Empty);
                return true;
            }
            return false;
        }

        public bool TryBuyTheme(BackgroundTheme theme)
        {
            if (IsThemeUnlocked(theme)) return true;

            int price = ThemePalette.ThemeStoreCatalog[theme].Price;
            if (_data.CoinsBalance >= price)
            {
                _data.CoinsBalance -= price;
                _data.UnlockedThemes.Add(theme.ToString());
                Save();
                BalanceChanged?.Invoke(this, EventArgs.Empty);
                return true;
            }
            return false;
        }

        public void AddCoins(int amount)
        {
            if (amount <= 0) return;
            _data.CoinsBalance += amount;
            Save();
            BalanceChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool RecordGameResult(int score, int coinsEarned, BirdSkin skinUsed)
        {
            bool isNewRecord = false;
            if (score > _data.HighScore)
            {
                _data.HighScore = score;
                isNewRecord = true;
            }

            string skinName = ThemePalette.SkinStoreCatalog[skinUsed].Name;
            _data.Leaderboard.Add(new HighScoreEntry(score, coinsEarned, skinName));
            _data.Leaderboard.Sort((a, b) => b.Score.CompareTo(a.Score));

            // Sadece en iyi 5 skoru tut
            if (_data.Leaderboard.Count > 5)
            {
                _data.Leaderboard.RemoveRange(5, _data.Leaderboard.Count - 5);
            }

            Save();
            return isNewRecord;
        }

        private void Load()
        {
            try
            {
                if (File.Exists(_saveFilePath))
                {
                    string json = File.ReadAllText(_saveFilePath);
                    var loaded = JsonSerializer.Deserialize<SaveDataModel>(json);
                    if (loaded != null)
                    {
                        _data = loaded;
                    }
                }
                else if (File.Exists(_legacyHighScorePath))
                {
                    // Eski highscore.txt dosyasını profile taşı
                    string text = File.ReadAllText(_legacyHighScorePath).Trim();
                    if (int.TryParse(text, out int legacyHigh))
                    {
                        _data.HighScore = legacyHigh;
                    }
                }
            }
            catch
            {
                _data = new SaveDataModel();
            }

            // Varsayılan kilitleri kontrol et
            if (!_data.UnlockedSkins.Contains(BirdSkin.SariKanarya.ToString()))
                _data.UnlockedSkins.Add(BirdSkin.SariKanarya.ToString());
            if (!_data.UnlockedThemes.Contains(BackgroundTheme.Gunduz.ToString()))
                _data.UnlockedThemes.Add(BackgroundTheme.Gunduz.ToString());
        }

        public void Save()
        {
            try
            {
                string json = JsonSerializer.Serialize(_data, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_saveFilePath, json);

                // Eski dosyayı da uyumluluk için güncelle
                File.WriteAllText(_legacyHighScorePath, _data.HighScore.ToString());
            }
            catch
            {
                // Yetki veya dosya kilidi durumunda sessizce devam et
            }
        }
    }
}
