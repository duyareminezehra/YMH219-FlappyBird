using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Media;
using YMH219_FlappyBird.Models;

namespace YMH219_FlappyBird.Services
{
    /// <summary>
    /// Oyun döngüsünü, skor/seviye yönetimini, çarpışma kontrolünü ve yüksek skor dosya işlemlerini
    /// nesne tabanlı olarak yöneten ana kontrol sınıfı.
    /// </summary>
    public class GameManager
    {
        private readonly List<PipePair> _pipes = new List<PipePair>();
        private readonly Random _random = new Random();
        private readonly string _highScoreFilePath;

        public Bird PlayerBird { get; }
        public IReadOnlyList<PipePair> Pipes => _pipes;

        public int CanvasWidth { get; private set; }
        public int CanvasHeight { get; private set; }
        public int GroundHeight { get; } = 60;
        public int GroundTopY => CanvasHeight - GroundHeight;
        public int GroundScrollOffset { get; private set; }

        public int Score { get; private set; }
        public int HighScore { get; private set; }
        public int CurrentLevel { get; private set; } = 1;
        public float CurrentPipeSpeed { get; private set; }
        public GameState State { get; private set; } = GameState.Ready;
        public DifficultyLevel Difficulty { get; private set; } = DifficultyLevel.Normal;
        public BackgroundTheme SelectedTheme { get; set; } = BackgroundTheme.Gunduz;
        public BirdSkin SelectedBirdSkin { get; set; } = BirdSkin.SariKanarya;
        public bool SoundEnabled { get; set; } = true;

        private const int PipeCount = 3;
        private const int PipeWidth = 64;
        private const int PipeSpacing = 270;

        public event EventHandler? ScoreUpdated;
        public event EventHandler? StateChanged;
        public event EventHandler? GameOverOccurred;

        public GameManager(int canvasWidth, int canvasHeight)
        {
            CanvasWidth = Math.Max(600, canvasWidth);
            CanvasHeight = Math.Max(450, canvasHeight);
            _highScoreFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "highscore.txt");

            PlayerBird = new Bird(110f, (GroundTopY / 2f) - 20f);
            LoadHighScore();
            ApplyDifficulty(DifficultyLevel.Normal);
            InitializePipes();
        }

        /// <summary>
        /// Zorluk seviyesini değiştirir ve fizik parametrelerini günceller.
        /// </summary>
        public void ApplyDifficulty(DifficultyLevel level)
        {
            Difficulty = level;
            DifficultySettings settings = DifficultySettings.FromLevel(level);
            PlayerBird.Gravity = settings.Gravity;
            PlayerBird.JumpStrength = settings.JumpStrength;
            CurrentPipeSpeed = settings.InitialPipeSpeed;
        }

        /// <summary>
        /// Oyunu sıfırlayarak yeni bir oyun başlatır.
        /// </summary>
        public void StartNewGame()
        {
            Score = 0;
            CurrentLevel = 1;
            ApplyDifficulty(Difficulty);
            PlayerBird.Reset(110f, (GroundTopY / 2f) - 20f);
            InitializePipes();
            SetState(GameState.Playing);
            ScoreUpdated?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Oyun bittikten sonra kullanıcı 'Hayır / Ana Menü' seçtiğinde başlangıç ekranına döner.
        /// </summary>
        public void ReturnToReadyScreen()
        {
            Score = 0;
            CurrentLevel = 1;
            ApplyDifficulty(Difficulty);
            PlayerBird.Reset(110f, (GroundTopY / 2f) - 20f);
            InitializePipes();
            SetState(GameState.Ready);
            ScoreUpdated?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Oyunu duraklatır veya devam ettirir.
        /// </summary>
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

        /// <summary>
        /// Kullanıcı zıplama tuşuna bastığında çağrılır.
        /// </summary>
        public void HandleJumpInput()
        {
            if (State == GameState.Ready)
            {
                StartNewGame();
                PlayerBird.Jump();
            }
            else if (State == GameState.Playing)
            {
                PlayerBird.Jump();
            }
        }

        /// <summary>
        /// Timer her tetiklendiğinde (oyun döngüsü) kuşu, engelleri ve çarpışmaları günceller.
        /// </summary>
        public void UpdateFrame()
        {
            if (State != GameState.Playing)
            {
                return;
            }

            PlayerBird.Update();
            GroundScrollOffset = (GroundScrollOffset + (int)Math.Round(CurrentPipeSpeed)) % 32;

            DifficultySettings settings = DifficultySettings.FromLevel(Difficulty);

            // Engelleri sola hareket ettir ve ekrandan çıkanları yeniden konumlandır
            float rightmostX = GetRightmostPipeX();
            foreach (PipePair pipe in _pipes)
            {
                pipe.MoveLeft(CurrentPipeSpeed);

                // Kuş engeli başarıyla geçtiyse skoru artır
                if (!pipe.IsScored && (pipe.X + pipe.Width) < PlayerBird.X)
                {
                    pipe.IsScored = true;
                    IncrementScore();
                }

                // Ekranın solundan çıkan engeli sağa yeniden yerleştir
                if (pipe.X + pipe.Width < 0)
                {
                    float nextX = Math.Max(CanvasWidth + 30, rightmostX + PipeSpacing);
                    int nextGapY = GenerateRandomGapTopY(settings.PipeGapHeight);
                    pipe.Reposition(nextX, nextGapY, settings.PipeGapHeight);
                    rightmostX = nextX;
                }
            }

            // Çarpışma kontrolü
            if (CheckCollision())
            {
                TriggerGameOver();
            }
        }

        public void ResizeCanvas(int width, int height)
        {
            if (width > 300 && height > 300)
            {
                CanvasWidth = width;
                CanvasHeight = height;
            }
        }

        private void InitializePipes()
        {
            _pipes.Clear();
            DifficultySettings settings = DifficultySettings.FromLevel(Difficulty);
            float startX = 360f;

            for (int i = 0; i < PipeCount; i++)
            {
                int gapTopY = GenerateRandomGapTopY(settings.PipeGapHeight);
                _pipes.Add(new PipePair(startX + (i * PipeSpacing), PipeWidth, gapTopY, settings.PipeGapHeight));
            }
        }

        private int GenerateRandomGapTopY(int gapHeight)
        {
            int minTop = 55;
            int maxTop = Math.Max(minTop + 40, GroundTopY - gapHeight - 55);
            return _random.Next(minTop, maxTop);
        }

        private float GetRightmostPipeX()
        {
            float max = 0f;
            foreach (PipePair pipe in _pipes)
            {
                if (pipe.X > max)
                {
                    max = pipe.X;
                }
            }
            return max;
        }

        private void IncrementScore()
        {
            Score++;

            // Seviye ilerledikçe hızın artması (İsteğe bağlı geliştirme)
            int calculatedLevel = (Score / 5) + 1;
            if (calculatedLevel > CurrentLevel)
            {
                CurrentLevel = calculatedLevel;
                CurrentPipeSpeed += 0.45f;
            }

            if (Score > HighScore)
            {
                HighScore = Score;
                SaveHighScore();
            }

            if (SoundEnabled)
            {
                try { SystemSounds.Asterisk.Play(); } catch { /* Ses hatası oyunu durdurmamalı */ }
            }

            ScoreUpdated?.Invoke(this, EventArgs.Empty);
        }

        private bool CheckCollision()
        {
            Rectangle birdRect = PlayerBird.Bounds;

            // Zemine çarpma kontrolü
            if (birdRect.Bottom >= GroundTopY)
            {
                return true;
            }

            // Borulara çarpma kontrolü
            foreach (PipePair pipe in _pipes)
            {
                if (birdRect.IntersectsWith(pipe.GetTopPipeBounds()) ||
                    birdRect.IntersectsWith(pipe.GetBottomPipeBounds(GroundTopY)))
                {
                    return true;
                }
            }

            return false;
        }

        private void TriggerGameOver()
        {
            if (Score > HighScore)
            {
                HighScore = Score;
            }
            SaveHighScore();

            if (SoundEnabled)
            {
                try { SystemSounds.Hand.Play(); } catch { /* Ses hatası yoksayılır */ }
            }

            SetState(GameState.GameOver);
            GameOverOccurred?.Invoke(this, EventArgs.Empty);
        }

        private void SetState(GameState newState)
        {
            State = newState;
            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// En yüksek skoru yerel dosyadan güvenli bir şekilde okur.
        /// </summary>
        public void LoadHighScore()
        {
            try
            {
                if (File.Exists(_highScoreFilePath))
                {
                    string content = File.ReadAllText(_highScoreFilePath).Trim();
                    if (int.TryParse(content, out int savedHigh))
                    {
                        HighScore = Math.Max(0, savedHigh);
                    }
                }
            }
            catch
            {
                HighScore = 0;
            }
        }

        /// <summary>
        /// En yüksek skoru yerel dosyaya kaydeder.
        /// </summary>
        public void SaveHighScore()
        {
            try
            {
                File.WriteAllText(_highScoreFilePath, HighScore.ToString());
            }
            catch
            {
                // Dosya yazma izni hatası durumunda uygulamanın çökmesi engellenir
            }
        }

        /// <summary>
        /// Görsel doğrulama ve ekran görüntüsü alma amacıyla örnek oyun sahnesi hazırlar.
        /// </summary>
        public void ConfigureSampleScene(bool gameOverState)
        {
            StartNewGame();
            Score = gameOverState ? 14 : 7;
            CurrentLevel = (Score / 5) + 1;
            HighScore = Math.Max(HighScore, 18);
            CurrentPipeSpeed = DifficultySettings.FromLevel(Difficulty).InitialPipeSpeed + ((CurrentLevel - 1) * 0.45f);

            _pipes.Clear();
            int gap = DifficultySettings.FromLevel(Difficulty).PipeGapHeight;
            if (!gameOverState)
            {
                PlayerBird.Reset(115f, 185f);
                _pipes.Add(new PipePair(235f, PipeWidth, 125, gap));
                _pipes.Add(new PipePair(505f, PipeWidth, 195, gap));
                _pipes.Add(new PipePair(775f, PipeWidth, 95, gap));
                SetState(GameState.Playing);
            }
            else
            {
                PlayerBird.Reset(175f, 115f);
                _pipes.Add(new PipePair(165f, PipeWidth, 130, gap));
                _pipes.Add(new PipePair(435f, PipeWidth, 175, gap));
                _pipes.Add(new PipePair(705f, PipeWidth, 105, gap));
                SetState(GameState.GameOver);
            }
            ScoreUpdated?.Invoke(this, EventArgs.Empty);
        }
    }
}
