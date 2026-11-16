using System;
using System.Drawing;

namespace YMH219_FlappyBird.Models
{
    /// <summary>
    /// Ekranda sağdan sola doğru hareket eden alt ve üst boru çiftini temsil eden sınıf.
    /// </summary>
    public class PipePair
    {
        public float X { get; private set; }
        public int Width { get; }
        public int GapTopY { get; private set; }
        public int GapHeight { get; private set; }
        public bool IsScored { get; set; }

        public PipePair(float startX, int width, int gapTopY, int gapHeight)
        {
            X = startX;
            Width = width;
            GapTopY = gapTopY;
            GapHeight = gapHeight;
            IsScored = false;
        }

        /// <summary>
        /// Engeli belirtilen hız kadar sola kaydırır.
        /// </summary>
        public void MoveLeft(float speed)
        {
            X -= speed;
        }

        /// <summary>
        /// Engel ekranın solundan tamamen çıktığında sağ tarafa yeni bir boşluk yüksekliğiyle yeniden konumlandırılır.
        /// </summary>
        public void Reposition(float newX, int newGapTopY, int newGapHeight)
        {
            X = newX;
            GapTopY = newGapTopY;
            GapHeight = newGapHeight;
            IsScored = false;
        }

        /// <summary>
        /// Üst borunun çarpışma dikdörtgenini döndürür.
        /// </summary>
        public Rectangle GetTopPipeBounds()
        {
            return new Rectangle((int)Math.Round(X), 0, Width, GapTopY);
        }

        /// <summary>
        /// Alt borunun çarpışma dikdörtgenini döndürür.
        /// </summary>
        public Rectangle GetBottomPipeBounds(int groundTopY)
        {
            int bottomPipeTop = GapTopY + GapHeight;
            int height = Math.Max(0, groundTopY - bottomPipeTop);
            return new Rectangle((int)Math.Round(X), bottomPipeTop, Width, height);
        }
    }
}
