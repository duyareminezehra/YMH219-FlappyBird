using System;
using System.Drawing;

namespace YMH219_FlappyBird.Models
{
    /// <summary>
    /// Oyuncunun kontrol ettiği kuş nesnesini temsil eden OOP sınıfı.
    /// Konum, hız, yer çekimi ve çarpışma sınırlarını (Bounds) yönetir.
    /// </summary>
    public class Bird
    {
        public float X { get; private set; }
        public float Y { get; private set; }
        public int Width { get; }
        public int Height { get; }
        public float VerticalVelocity { get; private set; }
        public float Gravity { get; set; }
        public float JumpStrength { get; set; }
        public int WingPhase { get; private set; }

        private const float MaxFallSpeed = 6.0f;

        /// <summary>
        /// Çarpışma kontrolü (IntersectsWith) için kuşun dikdörtgen sınırlarını döndürür.
        /// Görsel adalet için 4 piksellik iç pay (hitbox daraltma) uygulanır.
        /// </summary>
        public Rectangle Bounds => new Rectangle(
            (int)Math.Round(X) + 4,
            (int)Math.Round(Y) + 4,
            Width - 8,
            Height - 8);

        public Bird(float startX, float startY, int width = 40, int height = 30)
        {
            X = startX;
            Y = startY;
            Width = width;
            Height = height;
            Gravity = 0.25f;
            JumpStrength = -6.0f;
            VerticalVelocity = 0f;
        }

        /// <summary>
        /// Kullanıcı zıplama tuşuna (Space / Yukarı Ok) bastığında kuşu yukarı doğru ivmelendirir.
        /// </summary>
        public void Jump()
        {
            VerticalVelocity = JumpStrength;
        }

        /// <summary>
        /// Her zamanlayıcı (Timer) adımında yer çekimini uygular ve kuşun dikey konumunu günceller.
        /// </summary>
        public void Update()
        {
            VerticalVelocity += Gravity;
            if (VerticalVelocity > MaxFallSpeed)
            {
                VerticalVelocity = MaxFallSpeed;
            }

            Y += VerticalVelocity;
            WingPhase = (WingPhase + 1) % 12;

            // Ekranın üst sınırından dışarı taşmasını engelle
            if (Y < 0)
            {
                Y = 0;
                VerticalVelocity = 0;
            }
        }

        /// <summary>
        /// Yeni oyun başlatıldığında kuşu başlangıç konumuna sıfırlar.
        /// </summary>
        public void Reset(float startX, float startY)
        {
            X = startX;
            Y = startY;
            VerticalVelocity = 0f;
            WingPhase = 0;
        }
    }
}
