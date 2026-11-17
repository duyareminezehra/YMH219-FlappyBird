using System;
using System.Collections.Generic;
using System.Drawing;
using YMH219_FlappyBird.Models;

namespace YMH219_FlappyBird.Services
{
    /// <summary>
    /// Flappy Bird tam oyun döngüsünü, altın toplama, güçlendiriciler (Kalkan, Mıknatıs, Ağır Çekim, 2x),
    /// parçacık efektlerini ve cüzdan/liderlik tablosu entegrasyonunu yöneten ana OOP kontrol sınıfı.
    /// </summary>
    public class GameManager
    {
        private readonly List<PipePair> _pipes = new();
        private readonly List<Coin> _coins = new();
        private readonly List<PowerUp> _powerUps = new();
        private readonly List<Particle> _particles = new();
        private readonly Random _random = new();

        public Bird PlayerBird { get; }
        public IReadOnlyList<PipePair> Pipes => _pipes;
        public IReadOnlyList<Coin> Coins => _coins;
        public IReadOnlyList<PowerUp> PowerUps => _powerUps;
        public IReadOnlyList<Particle> Particles => _particles;

        public int CanvasWidth { get; private set; }
        public int CanvasHeight { get; private set; }
        public int GroundHeight { get; } = 60;
        public int GroundTopY => CanvasHeight - GroundHeight;
        public int GroundScrollOffset { get; private set; }

        public int Score { get; private set; }
        public int HighScore => ProfileManager.Instance.HighScore;
        public int TotalCoins => ProfileManager.Instance.CoinsBalance;
        public int SessionCoins { get; private set; }
        public int CurrentLevel { get; private set; } = 1;
        public float BasePipeSpeed { get; private set; }
        public float CurrentPipeSpeed => IsSlowMoActive ? BasePipeSpeed * 0.55f : BasePipeSpeed;

        public GameState State { get; private set; } = GameState.Ready;
        public DifficultyLevel Difficulty { get; private set; } = DifficultyLevel.Normal;
        public BackgroundTheme SelectedTheme { get; set; } = BackgroundTheme.Gunduz;
        public BirdSkin SelectedBirdSkin { get; set; } = BirdSkin.SariKanarya;
        public bool SoundEnabled
        {
            get => SoundEffects.Enabled;
            set => SoundEffects.Enabled = value;
        }

        // --- Güçlendirici Durumları ---
        public bool HasShield { get; private set; }
        public int InvulnerabilityFrames { get; private set; }
        public float MagnetTimer { get; private set; }
        public float SlowMoTimer { get; private set; }
        public float DoubleScoreTimer { get; private set; }

        public bool IsMagnetActive => MagnetTimer > 0f;
        public bool IsSlowMoActive => SlowMoTimer > 0f;
        public bool IsDoubleScoreActive => DoubleScoreTimer > 0f;

        public bool IsNewRecordAchieved { get; private set; }

        private const int PipeCount = 3;
        private const int PipeWidth = 64;
        private const int PipeSpacing = 280;

        public event EventHandler? ScoreUpdated;
        public event EventHandler? StateChanged;
        public event EventHandler? GameOverOccurred;
        public event EventHandler? PowerUpChanged;

        public GameManager(int canvasWidth, int canvasHeight)
        {
            CanvasWidth = Math.Max(600, canvasWidth);
            CanvasHeight = Math.Max(450, canvasHeight);

            PlayerBird = new Bird(110f, (GroundTopY / 2f) - 20f);
            ApplyDifficulty(DifficultyLevel.Normal);
            InitializePipes();
        }

        public void ApplyDifficulty(DifficultyLevel level)
        {
            Difficulty = level;
            DifficultySettings settings = DifficultySettings.FromLevel(level);
            PlayerBird.Gravity = settings.Gravity;
            PlayerBird.JumpStrength = settings.JumpStrength;
            BasePipeSpeed = settings.InitialPipeSpeed;
        }

        public void StartNewGame()
        {
            Score = 0;
            SessionCoins = 0;
            CurrentLevel = 1;
            HasShield = false;
            InvulnerabilityFrames = 0;
            MagnetTimer = 0f;
            SlowMoTimer = 0f;
            DoubleScoreTimer = 0f;
            IsNewRecordAchieved = false;

            ApplyDifficulty(Difficulty);
            PlayerBird.Reset(110f, (GroundTopY / 2f) - 20f);
            _particles.Clear();
            InitializePipes();
            SetState(GameState.Playing);
            ScoreUpdated?.Invoke(this, EventArgs.Empty);
            PowerUpChanged?.Invoke(this, EventArgs.Empty);
        }

        public void ReturnToReadyScreen()
        {
            Score = 0;
            SessionCoins = 0;
            CurrentLevel = 1;
            HasShield = false;
            InvulnerabilityFrames = 0;
            MagnetTimer = 0f;
            SlowMoTimer = 0f;
            DoubleScoreTimer = 0f;
            IsNewRecordAchieved = false;

            ApplyDifficulty(Difficulty);
            PlayerBird.Reset(110f, (GroundTopY / 2f) - 20f);
            _particles.Clear();
            InitializePipes();
            SetState(GameState.Ready);
            ScoreUpdated?.Invoke(this, EventArgs.Empty);
            PowerUpChanged?.Invoke(this, EventArgs.Empty);
        }

        public void TogglePause()
        {
            if (State == GameState.Playing)
            {
                SetState(GameState.Paused);
            }
            else if (State == GameState.Paused)
            {
                SetState(GameState.Playing);
            }
        }

        public void HandleJumpInput()
        {
            if (State == GameState.Ready)
            {
                StartNewGame();
                PerformJump();
            }
            else if (State == GameState.Playing)
            {
                PerformJump();
            }
        }

        private void PerformJump()
        {
            PlayerBird.Jump();
            SoundEffects.PlayFlap();

            // Zıplama tüyleri üret
            var birdColors = ThemePalette.GetBirdColors(SelectedBirdSkin);
            for (int i = 0; i < 4; i++)
            {
                float vx = (float)(_random.NextDouble() * -2.5 - 0.5);
                float vy = (float)(_random.NextDouble() * 2.0 - 1.0);
                _particles.Add(new Particle(
                    PlayerBird.X + 6,
                    PlayerBird.Y + PlayerBird.Height - 6,
                    vx, vy, 18f,
                    birdColors.WingColor,
                    ParticleType.Feather,
                    size: 4.5f));
            }
        }

        public void UpdateFrame()
        {
            if (State != GameState.Playing)
            {
                return;
            }

            // Güçlendirici zamanlayıcılarını güncelle (~60 FPS)
            UpdatePowerUpTimers();

            // Kuşu güncelle (Ağır çekimde yerçekimi hafifletilir)
            if (IsSlowMoActive)
            {
                PlayerBird.Gravity = DifficultySettings.FromLevel(Difficulty).Gravity * 0.65f;
            }
            else
            {
                PlayerBird.Gravity = DifficultySettings.FromLevel(Difficulty).Gravity;
            }
            PlayerBird.Update();

            GroundScrollOffset = (GroundScrollOffset + (int)Math.Round(CurrentPipeSpeed)) % 32;

            if (InvulnerabilityFrames > 0)
            {
                InvulnerabilityFrames--;
            }

            DifficultySettings settings = DifficultySettings.FromLevel(Difficulty);

            // Engelleri ve nesneleri kaydır
            float rightmostX = GetRightmostPipeX();
            foreach (PipePair pipe in _pipes)
            {
                pipe.MoveLeft(CurrentPipeSpeed);

                // Kuş engeli başarıyla geçtiyse skor
                if (!pipe.IsScored && (pipe.X + pipe.Width) < PlayerBird.X)
                {
                    pipe.IsScored = true;
                    int added = IsDoubleScoreActive ? 2 : 1;
                    IncrementScore(added);
                }

                // Ekranın solundan çıkan engeli sağa yeniden yerleştir
                if (pipe.X + pipe.Width < 0)
                {
                    float nextX = Math.Max(CanvasWidth + 30, rightmostX + PipeSpacing);
                    int nextGapY = GenerateRandomGapTopY(settings.PipeGapHeight);
                    pipe.Reposition(nextX, nextGapY, settings.PipeGapHeight);
                    rightmostX = nextX;

                    // Bu boru aralığına yeni altın ekle
                    SpawnCoinInGap(nextX + (PipeWidth / 2f), nextGapY + (settings.PipeGapHeight / 2f));

                    // %25 olasılıkla sürpriz güçlendirici küresi ekle
                    if (_random.NextDouble() < 0.25)
                    {
                        SpawnRandomPowerUp(nextX + (PipeWidth / 2f) + 120, nextGapY + (settings.PipeGapHeight / 2f));
                    }
                }
            }

            // Altınları güncelle ve topla
            UpdateCoins();

            // Güçlendiricileri güncelle ve topla
            UpdatePowerUps();

            // Parçacıkları güncelle
            UpdateParticles();

            // Çarpışma kontrolü
            if (CheckCollision())
            {
                TriggerGameOver();
            }
        }

        private void UpdatePowerUpTimers()
        {
            const float dt = 1f / 60f;
            bool changed = false;

            if (MagnetTimer > 0f)
            {
                MagnetTimer -= dt;
                if (MagnetTimer <= 0f) { MagnetTimer = 0f; changed = true; }
            }
            if (SlowMoTimer > 0f)
            {
                SlowMoTimer -= dt;
                if (SlowMoTimer <= 0f) { SlowMoTimer = 0f; changed = true; }
            }
            if (DoubleScoreTimer > 0f)
            {
                DoubleScoreTimer -= dt;
                if (DoubleScoreTimer <= 0f) { DoubleScoreTimer = 0f; changed = true; }
            }

            if (changed)
            {
                PowerUpChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void UpdateCoins()
        {
            Rectangle birdBounds = PlayerBird.Bounds;

            for (int i = _coins.Count - 1; i >= 0; i--)
            {
                Coin coin = _coins[i];
                coin.Update(CurrentPipeSpeed);

                // Mıknatıs çekimi
                if (IsMagnetActive && !coin.IsCollected)
                {
                    float dist = GetDistance(PlayerBird.X + 20, PlayerBird.Y + 15, coin.X, coin.Y);
                    if (dist < 280f)
                    {
                        coin.AttractTowards(PlayerBird.X + 20, PlayerBird.Y + 15, 8.5f);
                    }
                }

                // Kuş altını topladı mı?
                if (!coin.IsCollected && birdBounds.IntersectsWith(coin.Bounds))
                {
                    coin.IsCollected = true;
                    int earned = IsDoubleScoreActive ? coin.Value * 2 : coin.Value;
                    SessionCoins += earned;
                    ProfileManager.Instance.AddCoins(earned);
                    SoundEffects.PlayCoin();

                    // Pırıltı partikülleri ve havada süzülen yazı
                    SpawnSparkles(coin.X, coin.Y, Color.FromArgb(250, 204, 21), 8);
                    _particles.Add(new Particle(
                        coin.X, coin.Y - 10,
                        0f, -1.8f, 32f,
                        Color.FromArgb(250, 204, 21),
                        ParticleType.FloatingText,
                        text: $"+{earned} Altın"));

                    ScoreUpdated?.Invoke(this, EventArgs.Empty);
                }

                if (coin.IsCollected || coin.X < -50)
                {
                    _coins.RemoveAt(i);
                }
            }
        }

        private void UpdatePowerUps()
        {
            Rectangle birdBounds = PlayerBird.Bounds;

            for (int i = _powerUps.Count - 1; i >= 0; i--)
            {
                PowerUp p = _powerUps[i];
                p.Update(CurrentPipeSpeed);

                if (!p.IsCollected && birdBounds.IntersectsWith(p.Bounds))
                {
                    p.IsCollected = true;
                    ActivatePowerUp(p.Type, p.X, p.Y);
                    _powerUps.RemoveAt(i);
                }
                else if (p.X < -50)
                {
                    _powerUps.RemoveAt(i);
                }
            }
        }

        private void ActivatePowerUp(PowerUpType type, float x, float y)
        {
            SoundEffects.PlayPowerUp();
            string label;
            Color auraColor;

            switch (type)
            {
                case PowerUpType.Shield:
                    HasShield = true;
                    label = "🛡️ KALKAN AKTİF!";
                    auraColor = Color.FromArgb(56, 189, 248);
                    break;
                case PowerUpType.Magnet:
                    MagnetTimer = 8.0f;
                    label = "🧲 MIKNATIS (8s)!";
                    auraColor = Color.FromArgb(244, 63, 94);
                    break;
                case PowerUpType.SlowMo:
                    SlowMoTimer = 5.0f;
                    label = "⏱️ AĞIR ÇEKİM (5s)!";
                    auraColor = Color.FromArgb(168, 85, 247);
                    break;
                case PowerUpType.DoubleScore:
                    DoubleScoreTimer = 8.0f;
                    label = "🌟 2X ÇARPAN (8s)!";
                    auraColor = Color.FromArgb(250, 204, 21);
                    break;
                default:
                    label = "GÜÇLENDİRİCİ!";
                    auraColor = Color.White;
                    break;
            }

            SpawnSparkles(x, y, auraColor, 14);
            _particles.Add(new Particle(x, y - 12, 0f, -2.0f, 38f, auraColor, ParticleType.FloatingText, text: label));
            PowerUpChanged?.Invoke(this, EventArgs.Empty);
        }

        private void UpdateParticles()
        {
            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                _particles[i].Update();
                if (_particles[i].IsDead)
                {
                    _particles.RemoveAt(i);
                }
            }
        }

        public void SpawnSparkles(float x, float y, Color color, int count)
        {
            for (int i = 0; i < count; i++)
            {
                double angle = _random.NextDouble() * Math.PI * 2;
                float speed = (float)(_random.NextDouble() * 3.5 + 1.0);
                float vx = (float)Math.Cos(angle) * speed;
                float vy = (float)Math.Sin(angle) * speed;
                _particles.Add(new Particle(x, y, vx, vy, 24f, color, ParticleType.Sparkle, size: 5f));
            }
        }

        public void SpawnConfetti()
        {
            Color[] colors = { Color.Gold, Color.Crimson, Color.DeepSkyBlue, Color.LimeGreen, Color.Magenta, Color.Orange };
            for (int i = 0; i < 45; i++)
            {
                float x = _random.Next(50, CanvasWidth - 50);
                float y = _random.Next(-30, 40);
                float vx = (float)(_random.NextDouble() * 4.0 - 2.0);
                float vy = (float)(_random.NextDouble() * 3.0 + 2.0);
                Color c = colors[_random.Next(colors.Length)];
                _particles.Add(new Particle(x, y, vx, vy, 55f, c, ParticleType.Confetti, size: 6.5f));
            }
        }

        private void SpawnCoinInGap(float x, float y)
        {
            _coins.Add(new Coin(x, y));
        }

        private void SpawnRandomPowerUp(float x, float y)
        {
            Array values = Enum.GetValues(typeof(PowerUpType));
            PowerUpType randomType = (PowerUpType)values.GetValue(_random.Next(values.Length))!;
            _powerUps.Add(new PowerUp(x, y, randomType));
        }

        private void IncrementScore(int amount = 1)
        {
            Score += amount;

            int calculatedLevel = (Score / 5) + 1;
            if (calculatedLevel > CurrentLevel)
            {
                CurrentLevel = calculatedLevel;
                BasePipeSpeed += 0.45f;
            }

            if (Score > HighScore && !IsNewRecordAchieved && HighScore > 0)
            {
                IsNewRecordAchieved = true;
                SoundEffects.PlayNewRecord();
                SpawnConfetti();
                _particles.Add(new Particle(
                    PlayerBird.X, PlayerBird.Y - 24,
                    0f, -2.2f, 45f,
                    Color.FromArgb(250, 204, 21),
                    ParticleType.FloatingText,
                    text: "🏆 YENİ REKOR!"));
            }

            ScoreUpdated?.Invoke(this, EventArgs.Empty);
        }

        private bool CheckCollision()
        {
            Rectangle birdRect = PlayerBird.Bounds;

            // Zemine çarpma (Kalkan zemine çarpmayı engellemez, kuş düşmüştür)
            if (birdRect.Bottom >= GroundTopY)
            {
                return true;
            }

            if (InvulnerabilityFrames > 0)
            {
                return false;
            }

            // Borulara çarpma kontrolü
            foreach (PipePair pipe in _pipes)
            {
                bool hitTop = birdRect.IntersectsWith(pipe.GetTopPipeBounds());
                bool hitBottom = birdRect.IntersectsWith(pipe.GetBottomPipeBounds(GroundTopY));

                if (hitTop || hitBottom)
                {
                    // 🛡️ EĞER KALKAN AKTİFSE: BORUYA ÇARPMADAN KURTAR!
                    if (HasShield)
                    {
                        HasShield = false;
                        InvulnerabilityFrames = 45; // ~0.75 saniye hasar almazlık
                        SoundEffects.PlayShieldBreak();

                        // Kalkan kırılma efekti
                        SpawnSparkles(PlayerBird.X + 20, PlayerBird.Y + 15, Color.FromArgb(56, 189, 248), 24);
                        _particles.Add(new Particle(
                            PlayerBird.X, PlayerBird.Y - 20,
                            0f, -2.2f, 40f,
                            Color.FromArgb(56, 189, 248),
                            ParticleType.FloatingText,
                            text: "🛡️ KALKAN SENİ KORUDU!"));

                        // Kuşu hafifçe geriye/ortaya doğru sektir
                        PlayerBird.Jump();
                        PowerUpChanged?.Invoke(this, EventArgs.Empty);
                        return false; // Ölmedin!
                    }

                    return true; // Kalkan yoksa oyun bitti
                }
            }

            return false;
        }

        private void TriggerGameOver()
        {
            bool isRecord = ProfileManager.Instance.RecordGameResult(Score, SessionCoins, SelectedBirdSkin);
            if (isRecord && !IsNewRecordAchieved)
            {
                IsNewRecordAchieved = true;
                SoundEffects.PlayNewRecord();
                SpawnConfetti();
            }
            else
            {
                SoundEffects.PlayCrash();
            }

            SetState(GameState.GameOver);
            GameOverOccurred?.Invoke(this, EventArgs.Empty);
        }

        private void SetState(GameState newState)
        {
            State = newState;
            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        private void InitializePipes()
        {
            _pipes.Clear();
            _coins.Clear();
            _powerUps.Clear();

            DifficultySettings settings = DifficultySettings.FromLevel(Difficulty);
            float startX = 380f;

            for (int i = 0; i < PipeCount; i++)
            {
                float px = startX + (i * PipeSpacing);
                int gapTopY = GenerateRandomGapTopY(settings.PipeGapHeight);
                _pipes.Add(new PipePair(px, PipeWidth, gapTopY, settings.PipeGapHeight));

                // İlk borulardan itibaren altın yerleştir
                SpawnCoinInGap(px + (PipeWidth / 2f), gapTopY + (settings.PipeGapHeight / 2f));
            }
        }

        private int GenerateRandomGapTopY(int gapHeight)
        {
            int minTop = 60;
            int maxTop = Math.Max(minTop + 40, GroundTopY - gapHeight - 60);
            return _random.Next(minTop, maxTop);
        }

        private float GetRightmostPipeX()
        {
            float max = 0f;
            foreach (PipePair pipe in _pipes)
            {
                if (pipe.X > max) max = pipe.X;
            }
            return max;
        }

        private static float GetDistance(float x1, float y1, float x2, float y2)
        {
            float dx = x1 - x2;
            float dy = y1 - y2;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        public void ResizeCanvas(int width, int height)
        {
            if (width > 300 && height > 300)
            {
                CanvasWidth = width;
                CanvasHeight = height;
            }
        }

        public void ConfigureSampleScene(bool gameOverState)
        {
            StartNewGame();
            Score = gameOverState ? 16 : 9;
            SessionCoins = gameOverState ? 6 : 3;
            CurrentLevel = (Score / 5) + 1;
            BasePipeSpeed = DifficultySettings.FromLevel(Difficulty).InitialPipeSpeed + ((CurrentLevel - 1) * 0.45f);

            _pipes.Clear();
            _coins.Clear();
            _powerUps.Clear();
            int gap = DifficultySettings.FromLevel(Difficulty).PipeGapHeight;

            if (!gameOverState)
            {
                PlayerBird.Reset(115f, 185f);
                HasShield = true;
                _pipes.Add(new PipePair(245f, PipeWidth, 125, gap));
                _pipes.Add(new PipePair(525f, PipeWidth, 195, gap));
                _pipes.Add(new PipePair(805f, PipeWidth, 95, gap));
                _coins.Add(new Coin(245f + 32, 125 + (gap / 2f)));
                _coins.Add(new Coin(525f + 32, 195 + (gap / 2f)));
                _powerUps.Add(new PowerUp(380f, 160f, PowerUpType.Magnet));
                SetState(GameState.Playing);
            }
            else
            {
                PlayerBird.Reset(175f, 120f);
                _pipes.Add(new PipePair(165f, PipeWidth, 130, gap));
                _pipes.Add(new PipePair(445f, PipeWidth, 175, gap));
                _pipes.Add(new PipePair(725f, PipeWidth, 105, gap));
                SetState(GameState.GameOver);
            }
            ScoreUpdated?.Invoke(this, EventArgs.Empty);
            PowerUpChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
