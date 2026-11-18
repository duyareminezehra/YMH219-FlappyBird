using System.Windows.Forms;

namespace YMH219_FlappyBird.Forms
{
    /// <summary>
    /// Oyun çizimlerinde ekran titremesini (flickering) önlemek için çift tamponlama (Double Buffering)
    /// özelliğini aktif eden özel Panel bileşeni.
    /// </summary>
    public class DoubleBufferedPanel : Panel
    {
        public DoubleBufferedPanel()
        {
            DoubleBuffered = true;
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);
            UpdateStyles();
        }
    }
}
