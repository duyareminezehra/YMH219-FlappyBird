using System;

namespace YMH219_FlappyBird.Models
{
    /// <summary>
    /// Liderlik tablosunda saklanan yüksek skor kaydı.
    /// </summary>
    public class HighScoreEntry
    {
        public int Score { get; set; }
        public int Coins { get; set; }
        public DateTime Date { get; set; }
        public string BirdSkinName { get; set; } = "Sarı Kanarya";

        public HighScoreEntry() { }

        public HighScoreEntry(int score, int coins, string birdSkinName)
        {
            Score = score;
            Coins = coins;
            Date = DateTime.Now;
            BirdSkinName = birdSkinName;
        }
    }
}
