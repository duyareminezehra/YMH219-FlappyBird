using System;
using System.Drawing;

namespace YMH219_FlappyBird.Models
{
    /// <summary>
    /// Oyuncunun toplayabileceği altın paraları temsil eden nesne sınıfı.
    /// Yüzen animasyon (bobbing) ve kuş ile çarpışma/toplanma kontrolünü sağlar.
    /// </summary>
    public class Coin
    {
        public float X { get; set; }
        public float BaseY { get; set; }
        public float Y { get; set; }
        public int Radius { get; } = 12;
        public bool IsCollected { get; set; } = false;
        public float BobbingAngle { get; set; }
        public int Value { get; set; } = 1;

        public Rectangle Bounds => new Rectangle(
            (int)Math.Round(X - Radius),
            (int)Math.Round(Y - Radius),
            Radius * 2,
            Radius * 2);

        public Coin(float x, float y, int value = 1)
        {
            X = x;
            BaseY = y;
            Y = y;
            Value = value;
            BobbingAngle = (float)(new Random().NextDouble() * Math.PI * 2);
        }

        /// <summary>
        /// Altını borularla senkronize olarak sola hareket ettirir ve yukarı-aşağı hafif süzülme animasyonu uygular.
        /// </summary>
        public void Update(float speed)
        {
            X -= speed;
            BobbingAngle += 0.08f;
            Y = BaseY + (float)(Math.Sin(BobbingAngle) * 5.0);
        }

        /// <summary>
        /// Mıknatıs aktifken altını kuşa doğru çeker.
        /// </summary>
        public void AttractTowards(float targetX, float targetY, float pullSpeed)
        {
            float dx = targetX - X;
            float dy = targetY - Y;
            float distance = (float)Math.Sqrt(dx * dx + dy * dy);

            if (distance > 1f)
            {
                X += (dx / distance) * pullSpeed;
                Y += (dy / distance) * pullSpeed;
                BaseY = Y;
            }
        }
    }
}
