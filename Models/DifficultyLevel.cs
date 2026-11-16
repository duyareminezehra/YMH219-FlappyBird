namespace YMH219_FlappyBird.Models
{
    /// <summary>
    /// Oyunun zorluk seviyelerini tanımlar.
    /// </summary>
    public enum DifficultyLevel
    {
        Kolay,
        Normal,
        Zor
    }

    /// <summary>
    /// Seçilen zorluk seviyesine göre oyun parametrelerini (hız, boru aralığı, yer çekimi) döndüren yardımcı sınıf.
    /// </summary>
    public sealed class DifficultySettings
    {
        public DifficultyLevel Level { get; }
        public float InitialPipeSpeed { get; }
        public int PipeGapHeight { get; }
        public float Gravity { get; }
        public float JumpStrength { get; }

        public DifficultySettings(DifficultyLevel level, float initialPipeSpeed, int pipeGapHeight, float gravity, float jumpStrength)
        {
            Level = level;
            InitialPipeSpeed = initialPipeSpeed;
            PipeGapHeight = pipeGapHeight;
            Gravity = gravity;
            JumpStrength = jumpStrength;
        }

        public static DifficultySettings FromLevel(DifficultyLevel level)
        {
            return level switch
            {
                DifficultyLevel.Kolay => new DifficultySettings(level, initialPipeSpeed: 2.3f, pipeGapHeight: 185, gravity: 0.20f, jumpStrength: -5.6f),
                DifficultyLevel.Zor   => new DifficultySettings(level, initialPipeSpeed: 3.4f, pipeGapHeight: 145, gravity: 0.31f, jumpStrength: -6.4f),
                _                     => new DifficultySettings(DifficultyLevel.Normal, initialPipeSpeed: 2.8f, pipeGapHeight: 165, gravity: 0.25f, jumpStrength: -6.0f)
            };
        }
    }
}
