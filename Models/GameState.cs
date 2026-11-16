namespace YMH219_FlappyBird.Models
{
    /// <summary>
    /// Oyunun anlık çalışma durumunu temsil eden numaralandırma (Enum).
    /// </summary>
    public enum GameState
    {
        Ready,      // Oyun başlamaya hazır (Başlangıç ekranı)
        Playing,    // Oyun aktif olarak devam ediyor
        Paused,     // Oyun duraklatıldı
        GameOver    // Çarpışma gerçekleşti, oyun bitti
    }
}
