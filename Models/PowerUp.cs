using System;
using System.Drawing;

namespace YMH219_FlappyBird.Models
{
    /// <summary>
    /// Oyunda beliren özel güçlendirici türleri.
    /// </summary>
    public enum PowerUpType
    {
        Shield,      // 🛡️ Kalkan: 1 çarpışmadan korur ve boruyu kırar
        Magnet,      // 🧲 Mıknatıs: 8 saniye boyunca altınları kuşa çeker
        SlowMo,      // ⏱️ Ağır Çekim: 5 saniye boyunca hızı ve yer çekimini yavaşlatır
        DoubleScore  // 🌟 2x Çarpan: 8 saniye boyunca boru ve altın puanlarını 2 katına çıkarır
    }

    /// <summary>
    /// Boruların arasında seyrek beliren sürpriz güçlendirici küresi.
    /// </summary>
    public class PowerUp
    {
        public float X { get; set; }
        public float BaseY { get; set; }
        public float Y { get; set; }
        public int Radius { get; } = 15;
        public PowerUpType Type { get; }
        public bool IsCollected { get; set; } = false;
        public float BobbingAngle { get; set; }

        public Rectangle Bounds => new Rectangle(
            (int)Math.Round(X - Radius),
            (int)Math.Round(Y - Radius),
            Radius * 2,
            Radius * 2);

        public PowerUp(float x, float y, PowerUpType type)
        {
            X = x;
            BaseY = y;
            Y = y;
            Type = type;
            BobbingAngle = (float)(new Random().NextDouble() * Math.PI * 2);
        }

        public void Update(float speed)
        {
            X -= speed;
            BobbingAngle += 0.09f;
            Y = BaseY + (float)(Math.Sin(BobbingAngle) * 7.0);
        }

        public static string GetName(PowerUpType type) => type switch
        {
            PowerUpType.Shield => "🛡️ Kalkan",
            PowerUpType.Magnet => "🧲 Altın Mıknatısı",
            PowerUpType.SlowMo => "⏱️ Ağır Çekim",
            PowerUpType.DoubleScore => "🌟 2x Çarpan",
            _ => "Güçlendirici"
        };
    }
}
