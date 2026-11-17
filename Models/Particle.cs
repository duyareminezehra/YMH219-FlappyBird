using System;
using System.Drawing;

namespace YMH219_FlappyBird.Models
{
    public enum ParticleType
    {
        Feather,       // Zıplama tüyü
        Sparkle,       // Altın toplama pırıltısı
        ShieldBurst,   // Kalkan kırılma kıvılcımı
        Confetti,      // Yeni rekor konfetisi
        FloatingText   // +1 Altın, KALKAN! vb. havada süzülen yazı
    }

    /// <summary>
    /// Görsel zenginlik katan parçacık nesnesi.
    /// Konum, hız, ömür (Life) ve şeffaflık (Fade) yönetimini OOP ile gerçekleştirir.
    /// </summary>
    public class Particle
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Vx { get; set; }
        public float Vy { get; set; }
        public float Life { get; set; }
        public float MaxLife { get; }
        public Color BaseColor { get; }
        public ParticleType Type { get; }
        public float Size { get; set; }
        public string Text { get; set; } = string.Empty;

        public bool IsDead => Life <= 0f;
        public float AlphaNormalized => Math.Clamp(Life / MaxLife, 0f, 1f);

        public Particle(float x, float y, float vx, float vy, float life, Color color, ParticleType type, float size = 5f, string text = "")
        {
            X = x;
            Y = y;
            Vx = vx;
            Vy = vy;
            Life = life;
            MaxLife = life;
            BaseColor = color;
            Type = type;
            Size = size;
            Text = text;
        }

        public void Update()
        {
            X += Vx;
            Y += Vy;
            Life -= 1f;

            if (Type == ParticleType.Feather)
            {
                Vy += 0.05f; // Hafif süzülme
                Vx *= 0.95f;
            }
            else if (Type == ParticleType.Sparkle || Type == ParticleType.ShieldBurst)
            {
                Vx *= 0.92f;
                Vy *= 0.92f;
            }
            else if (Type == ParticleType.FloatingText)
            {
                Vy *= 0.96f; // Yavaşça yukarı süzülür
            }
            else if (Type == ParticleType.Confetti)
            {
                Vy += 0.08f;
                Vx *= 0.98f;
            }
        }
    }
}
