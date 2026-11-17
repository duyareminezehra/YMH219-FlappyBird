using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using YMH219_FlappyBird.Models;
using YMH219_FlappyBird.Services;

namespace YMH219_FlappyBird.Forms
{
    /// <summary>
    /// Flappy Bird masaüstü oyununun zenginleştirilmiş ana Windows Forms penceresi.
    /// Altın toplama, mağaza, güçlendiriciler, parçacık efektleri, liderlik tablosu ve
    /// akıcı fizik döngüsünü nesne tabanlı olarak yönetir.
    /// </summary>
    public class MainForm : Form
    {
        private readonly GameManager _gameManager;
        private readonly System.Windows.Forms.Timer _gameTimer;

        private Panel _topBarPanel = null!;
        private DoubleBufferedPanel _canvasPanel = null!;
        private Button _btnStartRestart = null!;
        private Button _btnPause = null!;
        private Button _btnShop = null!;
        private Button _btnLeaderboard = null!;
        private ComboBox _cmbDifficulty = null!;
        private ComboBox _cmbTheme = null!;
        private ComboBox _cmbBirdSkin = null!;
        private CheckBox _chkSound = null!;
        private Label _lblCoins = null!;
        private Label _lblScore = null!;
        private Label _lblHighScore = null!;
        private Label _lblStatus = null!;
        private Label _lblPowerUpStatus = null!;

        // Game Over Onay Butonları
        private Button _btnRetryYes = null!;
        private Button _btnRetryNo = null!;
        private Button _btnGoToShop = null!;

        public MainForm()
        {
            Text = "YMH219 - Flappy Bird Deluxe Oyunu (Altın, Dükkan, Güçlendiriciler)";
            Size = new Size(1020, 680);
            MinimumSize = new Size(920, 600);
            StartPosition = FormStartPosition.CenterScreen;
            KeyPreview = true;
            BackColor = Color.FromArgb(15, 23, 42);

            InitializeComponents();

            _gameManager = new GameManager(_canvasPanel.ClientSize.Width, _canvasPanel.ClientSize.Height);
            _gameManager.ScoreUpdated += (_, _) => UpdateHudLabels();
            _gameManager.PowerUpChanged += (_, _) => UpdateHudLabels();
            _gameManager.StateChanged += (_, _) =>
            {
                UpdateHudLabels();
                UpdateRetryButtonsVisibility();
            };
            _gameManager.GameOverOccurred += (_, _) =>
            {
                _gameTimer?.Stop();
                UpdateHudLabels();
                UpdateRetryButtonsVisibility();
                _canvasPanel.Invalidate();
            };

            _gameTimer = new System.Windows.Forms.Timer
            {
                Interval = 16 // ~60 FPS akıcı oyun döngüsü
            };
            _gameTimer.Tick += GameTimer_Tick;

            ProfileManager.Instance.BalanceChanged += (_, _) => UpdateHudLabels();

            UpdateHudLabels();
            UpdateRetryButtonsVisibility();
        }

        private void InitializeComponents()
        {
            _topBarPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 72,
                BackColor = Color.FromArgb(30, 41, 59),
                Padding = new Padding(10, 8, 10, 8)
            };

            _btnStartRestart = new Button
            {
                Text = "Oyunu Başlat",
                Location = new Point(10, 16),
                Size = new Size(110, 38),
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                TabStop = false
            };
            _btnStartRestart.FlatAppearance.BorderSize = 0;
            _btnStartRestart.Click += (_, _) => StartOrRestartGame();

            _btnPause = new Button
            {
                Text = "Duraklat (P)",
                Location = new Point(126, 16),
                Size = new Size(95, 38),
                BackColor = Color.FromArgb(59, 130, 246),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.8f, FontStyle.Bold),
                TabStop = false
            };
            _btnPause.FlatAppearance.BorderSize = 0;
            _btnPause.Click += (_, _) => TogglePauseGame();

            _btnShop = new Button
            {
                Text = "Dükkan 🛍️",
                Location = new Point(227, 16),
                Size = new Size(90, 38),
                BackColor = Color.FromArgb(217, 119, 6),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                TabStop = false
            };
            _btnShop.FlatAppearance.BorderSize = 0;
            _btnShop.Click += (_, _) => OpenShopDialog();

            _btnLeaderboard = new Button
            {
                Text = "Sıralama 🏆",
                Location = new Point(323, 16),
                Size = new Size(95, 38),
                BackColor = Color.FromArgb(147, 51, 234),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                TabStop = false
            };
            _btnLeaderboard.FlatAppearance.BorderSize = 0;
            _btnLeaderboard.Click += (_, _) => OpenLeaderboardDialog();

            // Zorluk Seçimi
            var lblDiffTitle = new Label
            {
                Text = "Zorluk:",
                Location = new Point(426, 10),
                AutoSize = true,
                ForeColor = Color.FromArgb(203, 213, 225),
                Font = new Font("Segoe UI", 8.2f, FontStyle.Bold)
            };

            _cmbDifficulty = new ComboBox
            {
                Location = new Point(426, 30),
                Size = new Size(82, 26),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 8.8f),
                TabStop = false
            };
            _cmbDifficulty.Items.AddRange(new object[] { "Kolay", "Normal", "Zor" });
            _cmbDifficulty.SelectedIndex = 1;
            _cmbDifficulty.SelectedIndexChanged += CmbDifficulty_SelectedIndexChanged;

            // Arka Plan Teması Seçimi
            var lblThemeTitle = new Label
            {
                Text = "Tema:",
                Location = new Point(514, 10),
                AutoSize = true,
                ForeColor = Color.FromArgb(203, 213, 225),
                Font = new Font("Segoe UI", 8.2f, FontStyle.Bold)
            };

            _cmbTheme = new ComboBox
            {
                Location = new Point(514, 30),
                Size = new Size(120, 26),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 8.8f),
                TabStop = false
            };
            _cmbTheme.Items.AddRange(new object[]
            {
                "Gündüz (Klasik)",
                "Gün Batımı",
                "Yıldızlı Gece",
                "Karlı Kış",
                "Neon Siber Şehir"
            });
            _cmbTheme.SelectedIndex = 0;
            _cmbTheme.SelectedIndexChanged += CmbTheme_SelectedIndexChanged;

            // Kuş Karakteri Seçimi
            var lblBirdTitle = new Label
            {
                Text = "Kuş:",
                Location = new Point(640, 10),
                AutoSize = true,
                ForeColor = Color.FromArgb(203, 213, 225),
                Font = new Font("Segoe UI", 8.2f, FontStyle.Bold)
            };

            _cmbBirdSkin = new ComboBox
            {
                Location = new Point(640, 30),
                Size = new Size(130, 26),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 8.8f),
                TabStop = false
            };
            _cmbBirdSkin.Items.AddRange(new object[]
            {
                "Sarı Kanarya",
                "Kızıl Anka",
                "Mavi Fırtına",
                "Zümrüt Kuşu",
                "Mor Gölge",
                "Polis Kuşu 👮",
                "Kral Kuş 👑",
                "Siber Kuş 🚀"
            });
            _cmbBirdSkin.SelectedIndex = 0;
            _cmbBirdSkin.SelectedIndexChanged += CmbBirdSkin_SelectedIndexChanged;

            _chkSound = new CheckBox
            {
                Text = "Ses",
                Checked = true,
                Location = new Point(776, 30),
                AutoSize = true,
                ForeColor = Color.FromArgb(203, 213, 225),
                Font = new Font("Segoe UI", 9f),
                TabStop = false
            };
            _chkSound.CheckedChanged += (_, _) =>
            {
                if (_gameManager != null) _gameManager.SoundEnabled = _chkSound.Checked;
                ActiveControl = null;
            };

            _lblCoins = new Label
            {
                Text = "💰 0",
                Location = new Point(830, 10),
                Size = new Size(85, 24),
                ForeColor = Color.FromArgb(250, 204, 21),
                Font = new Font("Segoe UI", 11f, FontStyle.Bold)
            };

            _lblScore = new Label
            {
                Text = "Skor: 0",
                Location = new Point(830, 35),
                Size = new Size(85, 20),
                ForeColor = Color.FromArgb(56, 189, 248),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };

            _lblHighScore = new Label
            {
                Text = "En Yüksek: 0",
                Location = new Point(915, 10),
                Size = new Size(95, 20),
                ForeColor = Color.FromArgb(148, 163, 184),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
            };

            _lblStatus = new Label
            {
                Text = "Hazır",
                Location = new Point(915, 33),
                Size = new Size(95, 18),
                ForeColor = Color.FromArgb(134, 239, 172),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular)
            };

            _lblPowerUpStatus = new Label
            {
                Text = "",
                Location = new Point(426, 52),
                Size = new Size(340, 18),
                ForeColor = Color.FromArgb(250, 204, 21),
                Font = new Font("Segoe UI", 8.2f, FontStyle.Bold),
                Visible = false
            };

            _topBarPanel.Controls.AddRange(new Control[]
            {
                _btnStartRestart,
                _btnPause,
                _btnShop,
                _btnLeaderboard,
                lblDiffTitle,
                _cmbDifficulty,
                lblThemeTitle,
                _cmbTheme,
                lblBirdTitle,
                _cmbBirdSkin,
                _chkSound,
                _lblCoins,
                _lblScore,
                _lblHighScore,
                _lblStatus,
                _lblPowerUpStatus
            });

            _canvasPanel = new DoubleBufferedPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(56, 189, 248)
            };
            _canvasPanel.Paint += CanvasPanel_Paint;
            _canvasPanel.MouseDown += (_, _) => HandleJumpAction();
            _canvasPanel.Resize += (_, _) =>
            {
                _gameManager?.ResizeCanvas(_canvasPanel.ClientSize.Width, _canvasPanel.ClientSize.Height);
                RepositionRetryButtons();
            };

            // Game Over Seçenek Butonları
            _btnRetryYes = new Button
            {
                Text = "✓ Evet, Tekrar Oyna",
                Size = new Size(150, 42),
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Visible = false
            };
            _btnRetryYes.FlatAppearance.BorderSize = 0;
            _btnRetryYes.Click += (_, _) => StartOrRestartGame();

            _btnRetryNo = new Button
            {
                Text = "Ana Ekran",
                Size = new Size(120, 42),
                BackColor = Color.FromArgb(71, 85, 105),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Visible = false
            };
            _btnRetryNo.FlatAppearance.BorderSize = 0;
            _btnRetryNo.Click += (_, _) => ReturnToReadyState();

            _btnGoToShop = new Button
            {
                Text = "Dükkan 🛍️",
                Size = new Size(120, 42),
                BackColor = Color.FromArgb(217, 119, 6),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Visible = false
            };
            _btnGoToShop.FlatAppearance.BorderSize = 0;
            _btnGoToShop.Click += (_, _) => OpenShopDialog();

            _canvasPanel.Controls.Add(_btnRetryYes);
            _canvasPanel.Controls.Add(_btnRetryNo);
            _canvasPanel.Controls.Add(_btnGoToShop);

            Controls.Add(_canvasPanel);
            Controls.Add(_topBarPanel);

            KeyDown += MainForm_KeyDown;
        }

        private void OpenShopDialog()
        {
            bool wasPlaying = _gameManager.State == GameState.Playing;
            if (wasPlaying)
            {
                _gameManager.TogglePause();
                _gameTimer.Stop();
            }

            using var shop = new ShopDialog(_gameManager);
            shop.ShowDialog(this);

            SyncCombosFromGameManager();
            UpdateHudLabels();
            _canvasPanel.Invalidate();
        }

        private void OpenLeaderboardDialog()
        {
            using var lb = new LeaderboardDialog();
            lb.ShowDialog(this);
        }

        private void SyncCombosFromGameManager()
        {
            _cmbBirdSkin.SelectedIndex = _gameManager.SelectedBirdSkin switch
            {
                BirdSkin.KizilAnka => 1,
                BirdSkin.MaviFirtina => 2,
                BirdSkin.ZumrutKusu => 3,
                BirdSkin.MorGolge => 4,
                BirdSkin.PolisKusu => 5,
                BirdSkin.AltinKral => 6,
                BirdSkin.SiberRoket => 7,
                _ => 0
            };

            _cmbTheme.SelectedIndex = _gameManager.SelectedTheme switch
            {
                BackgroundTheme.GunBatimi => 1,
                BackgroundTheme.Gece => 2,
                BackgroundTheme.KarliKis => 3,
                BackgroundTheme.NeonSiber => 4,
                _ => 0
            };
        }

        private void GameTimer_Tick(object? sender, EventArgs e)
        {
            _gameManager.UpdateFrame();
            _canvasPanel.Invalidate();
        }

        private void MainForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Up)
            {
                HandleJumpAction();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.P)
            {
                TogglePauseGame();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.M)
            {
                _chkSound.Checked = !_chkSound.Checked;
                e.Handled = true;
            }
        }

        private void CmbDifficulty_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_gameManager == null) return;
            DifficultyLevel level = _cmbDifficulty.SelectedIndex switch
            {
                0 => DifficultyLevel.Kolay,
                2 => DifficultyLevel.Zor,
                _ => DifficultyLevel.Normal
            };
            _gameManager.ApplyDifficulty(level);
            UpdateHudLabels();
            ActiveControl = null;
        }

        private void CmbTheme_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_gameManager == null) return;
            BackgroundTheme chosen = _cmbTheme.SelectedIndex switch
            {
                1 => BackgroundTheme.GunBatimi,
                2 => BackgroundTheme.Gece,
                3 => BackgroundTheme.KarliKis,
                4 => BackgroundTheme.NeonSiber,
                _ => BackgroundTheme.Gunduz
            };

            if (!ProfileManager.Instance.IsThemeUnlocked(chosen))
            {
                var result = MessageBox.Show($"Bu tema kilitli! Dükkandan satın almak ister misiniz?", "Kilitli Tema", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    OpenShopDialog();
                }
                else
                {
                    SyncCombosFromGameManager();
                }
                return;
            }

            _gameManager.SelectedTheme = chosen;
            _canvasPanel.Invalidate();
            ActiveControl = null;
        }

        private void CmbBirdSkin_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_gameManager == null) return;
            BirdSkin chosen = _cmbBirdSkin.SelectedIndex switch
            {
                1 => BirdSkin.KizilAnka,
                2 => BirdSkin.MaviFirtina,
                3 => BirdSkin.ZumrutKusu,
                4 => BirdSkin.MorGolge,
                5 => BirdSkin.PolisKusu,
                6 => BirdSkin.AltinKral,
                7 => BirdSkin.SiberRoket,
                _ => BirdSkin.SariKanarya
            };

            if (!ProfileManager.Instance.IsSkinUnlocked(chosen))
            {
                var result = MessageBox.Show($"Bu kuş kostümü kilitli! Dükkandan satın almak ister misiniz?", "Kilitli Kostüm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    OpenShopDialog();
                }
                else
                {
                    SyncCombosFromGameManager();
                }
                return;
            }

            _gameManager.SelectedBirdSkin = chosen;
            _canvasPanel.Invalidate();
            ActiveControl = null;
        }

        private void StartOrRestartGame()
        {
            _gameManager.StartNewGame();
            _gameTimer.Start();
            UpdateHudLabels();
            UpdateRetryButtonsVisibility();
            _canvasPanel.Invalidate();
            ActiveControl = null;
            Focus();
        }

        private void ReturnToReadyState()
        {
            _gameManager.ReturnToReadyScreen();
            _gameTimer.Stop();
            UpdateHudLabels();
            UpdateRetryButtonsVisibility();
            _canvasPanel.Invalidate();
            ActiveControl = null;
            Focus();
        }

        private void TogglePauseGame()
        {
            _gameManager.TogglePause();
            if (_gameManager.State == GameState.Paused)
            {
                _gameTimer.Stop();
            }
            else if (_gameManager.State == GameState.Playing)
            {
                _gameTimer.Start();
            }

            UpdateHudLabels();
            _canvasPanel.Invalidate();
            ActiveControl = null;
            Focus();
        }

        private void HandleJumpAction()
        {
            if (_gameManager.State == GameState.GameOver)
            {
                return;
            }

            bool wasReady = _gameManager.State == GameState.Ready;
            _gameManager.HandleJumpInput();
            if (wasReady && _gameManager.State == GameState.Playing)
            {
                _gameTimer.Start();
            }

            _canvasPanel.Invalidate();
        }

        private void UpdateRetryButtonsVisibility()
        {
            bool isGameOver = _gameManager.State == GameState.GameOver;
            _btnRetryYes.Visible = isGameOver;
            _btnRetryNo.Visible = isGameOver;
            _btnGoToShop.Visible = isGameOver;

            if (isGameOver)
            {
                RepositionRetryButtons();
                _btnRetryYes.BringToFront();
                _btnRetryNo.BringToFront();
                _btnGoToShop.BringToFront();
            }
        }

        private void RepositionRetryButtons()
        {
            if (_canvasPanel == null || _btnRetryYes == null || _btnRetryNo == null || _btnGoToShop == null)
            {
                return;
            }

            int w = _canvasPanel.ClientSize.Width;
            int groundY = _gameManager?.GroundTopY ?? (_canvasPanel.ClientSize.Height - 60);

            int boxWidth = 460;
            int boxHeight = 255;
            int boxX = (w - boxWidth) / 2;
            int boxY = (groundY - boxHeight) / 2;

            int btnY = boxY + 185;
            _btnRetryYes.Location = new Point(boxX + 20, btnY);
            _btnRetryNo.Location = new Point(boxX + 180, btnY);
            _btnGoToShop.Location = new Point(boxX + 310, btnY);
        }

        private void UpdateHudLabels()
        {
            if (_gameManager == null) return;

            _lblCoins.Text = $"💰 {_gameManager.TotalCoins}";
            _lblScore.Text = $"Skor: {_gameManager.Score}";
            _lblHighScore.Text = $"En Yüksek: {_gameManager.HighScore}";

            // Güçlendirici Barı
            string powerText = "";
            if (_gameManager.HasShield) powerText += "🛡️ [KALKAN]";
            if (_gameManager.IsMagnetActive) powerText += $" 🧲 [MIKNATIS: {_gameManager.MagnetTimer:F1}s]";
            if (_gameManager.IsSlowMoActive) powerText += $" ⏱️ [AĞIR ÇEKİM: {_gameManager.SlowMoTimer:F1}s]";
            if (_gameManager.IsDoubleScoreActive) powerText += $" 🌟 [2X ÇARPAN: {_gameManager.DoubleScoreTimer:F1}s]";

            if (!string.IsNullOrEmpty(powerText))
            {
                _lblPowerUpStatus.Text = powerText.Trim();
                _lblPowerUpStatus.Visible = true;
            }
            else
            {
                _lblPowerUpStatus.Visible = false;
            }

            switch (_gameManager.State)
            {
                case GameState.Ready:
                    _lblStatus.Text = "Hazır";
                    _lblStatus.ForeColor = Color.FromArgb(134, 239, 172);
                    _btnStartRestart.Text = "Başlat";
                    _btnPause.Enabled = false;
                    break;
                case GameState.Playing:
                    _lblStatus.Text = $"Sv: {_gameManager.CurrentLevel}";
                    _lblStatus.ForeColor = Color.FromArgb(56, 189, 248);
                    _btnStartRestart.Text = "Yeniden";
                    _btnPause.Text = "Duraklat (P)";
                    _btnPause.Enabled = true;
                    break;
                case GameState.Paused:
                    _lblStatus.Text = "Duraklatıldı";
                    _lblStatus.ForeColor = Color.FromArgb(250, 204, 21);
                    _btnPause.Text = "Devam Et";
                    break;
                case GameState.GameOver:
                    _lblStatus.Text = "OYUN BİTTİ!";
                    _lblStatus.ForeColor = Color.FromArgb(248, 113, 113);
                    _btnStartRestart.Text = "Yeni Oyun";
                    break;
            }
        }

        private void CanvasPanel_Paint(object? sender, PaintEventArgs e)
        {
            RenderGameCanvas(e.Graphics, _canvasPanel.ClientSize.Width, _canvasPanel.ClientSize.Height);
        }

        private void RenderGameCanvas(Graphics g, int width, int height)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int groundTopY = _gameManager.GroundTopY;
            var themeColors = ThemePalette.GetBackgroundColors(_gameManager.SelectedTheme);

            // 1. Gökyüzü Degrade
            using (var skyBrush = new LinearGradientBrush(
                new Rectangle(0, 0, Math.Max(1, width), Math.Max(1, groundTopY)),
                themeColors.SkyTop,
                themeColors.SkyBottom,
                LinearGradientMode.Vertical))
            {
                g.FillRectangle(skyBrush, 0, 0, width, groundTopY);
            }

            // Yıldızlar / Hilal
            if (themeColors.ShowStars)
            {
                DrawNightStarsAndMoon(g, width);
            }

            // Siber Şehir Arka Planı
            if (themeColors.IsNeon)
            {
                DrawCyberCitySkyline(g, width, groundTopY);
            }
            else
            {
                // Bulutlar
                DrawCloud(g, 90, 65, 95, 36, themeColors.ShowStars);
                DrawCloud(g, 360, 48, 115, 40, themeColors.ShowStars);
                DrawCloud(g, 660, 80, 100, 34, themeColors.ShowStars);
            }

            // 2. Engeller (Borular)
            foreach (PipePair pipe in _gameManager.Pipes)
            {
                DrawPipePair(g, pipe, groundTopY, themeColors.PipeLight, themeColors.PipeDark, themeColors.IsNeon);
            }

            // 3. Altın Paralar
            foreach (Coin coin in _gameManager.Coins)
            {
                DrawCoin(g, coin);
            }

            // 4. Güçlendiriciler
            foreach (PowerUp powerUp in _gameManager.PowerUps)
            {
                DrawPowerUp(g, powerUp);
            }

            // 5. Zemin
            DrawGround(g, width, height, groundTopY, themeColors.GroundSoil, themeColors.GroundGrass, themeColors.GroundStripe, themeColors.IsNeon);

            // 6. Kuş Aurası (Kalkan / Mıknatıs)
            DrawBirdAuras(g, _gameManager.PlayerBird, _gameManager);

            // 7. Kuş Karakteri
            DrawBird(g, _gameManager.PlayerBird, _gameManager.SelectedBirdSkin);

            // 8. Parçacıklar (Tüyler, Altın Pırıltıları, Konfeti, Popuplar)
            DrawParticles(g);

            // 9. Canlı Skor Rozeti
            DrawFloatingScore(g, width);

            // 10. Durum Ekranları (Hazır, Duraklatıldı, Game Over)
            if (_gameManager.State != GameState.Playing)
            {
                DrawStateOverlay(g, width, groundTopY);
            }
        }

        private static void DrawNightStarsAndMoon(Graphics g, int width)
        {
            Point[] starCoords =
            {
                new Point(60, 35), new Point(155, 90), new Point(240, 42),
                new Point(320, 115), new Point(480, 38), new Point(560, 105),
                new Point(710, 45), new Point(820, 95), new Point(890, 40)
            };

            using var starBrush = new SolidBrush(Color.FromArgb(240, 254, 240, 138));
            foreach (Point pt in starCoords)
            {
                if (pt.X < width)
                {
                    g.FillEllipse(starBrush, pt.X, pt.Y, 4, 4);
                }
            }

            int moonX = Math.Max(400, width - 140);
            using var moonBrush = new SolidBrush(Color.FromArgb(254, 249, 195));
            g.FillEllipse(moonBrush, moonX, 32, 46, 46);
        }

        private static void DrawCyberCitySkyline(Graphics g, int width, int groundTopY)
        {
            using var bldBrush = new SolidBrush(Color.FromArgb(90, 15, 23, 42));
            using var neonLinePen = new Pen(Color.FromArgb(120, 236, 72, 153), 1.5f);

            int x = 20;
            while (x < width)
            {
                int bldW = 60;
                int bldH = 120 + ((x * 17) % 110);
                int bldY = groundTopY - bldH;
                g.FillRectangle(bldBrush, x, bldY, bldW, bldH);
                g.DrawLine(neonLinePen, x, bldY, x + bldW, bldY);
                x += 75;
            }
        }

        private static void DrawCloud(Graphics g, int x, int y, int width, int height, bool isNight)
        {
            Color c = isNight ? Color.FromArgb(90, 148, 163, 184) : Color.FromArgb(215, 255, 255, 255);
            using var cloudBrush = new SolidBrush(c);
            g.FillEllipse(cloudBrush, x, y + 8, width / 2, height - 8);
            g.FillEllipse(cloudBrush, x + width / 4, y, width / 2, height);
            g.FillEllipse(cloudBrush, x + width / 2, y + 10, width / 2, height - 10);
        }

        private static void DrawPipePair(Graphics g, PipePair pipe, int groundTopY, Color pipeLight, Color pipeDark, bool isNeon)
        {
            int pipeX = (int)Math.Round(pipe.X);
            int pipeW = pipe.Width;
            int gapTop = pipe.GapTopY;
            int gapBottom = gapTop + pipe.GapHeight;
            int bottomHeight = Math.Max(0, groundTopY - gapBottom);

            using var lightBrush = new SolidBrush(pipeLight);
            using var darkBrush = new SolidBrush(pipeDark);
            using var borderPen = new Pen(isNeon ? Color.FromArgb(34, 211, 238) : Color.FromArgb(15, 23, 42), 2.5f);

            // Üst Boru
            if (gapTop > 0)
            {
                g.FillRectangle(lightBrush, pipeX, 0, pipeW, gapTop);
                g.FillRectangle(darkBrush, pipeX + pipeW - 14, 0, 14, gapTop);
                g.DrawRectangle(borderPen, pipeX, 0, pipeW, gapTop);

                // Üst Boru Başlığı
                int capH = 24;
                int capY = Math.Max(0, gapTop - capH);
                g.FillRectangle(lightBrush, pipeX - 4, capY, pipeW + 8, capH);
                g.FillRectangle(darkBrush, pipeX + pipeW - 10, capY, 14, capH);
                g.DrawRectangle(borderPen, pipeX - 4, capY, pipeW + 8, capH);
            }

            // Alt Boru
            if (bottomHeight > 0)
            {
                g.FillRectangle(lightBrush, pipeX, gapBottom, pipeW, bottomHeight);
                g.FillRectangle(darkBrush, pipeX + pipeW - 14, gapBottom, 14, bottomHeight);
                g.DrawRectangle(borderPen, pipeX, gapBottom, pipeW, bottomHeight);

                // Alt Boru Başlığı
                int capH = 24;
                g.FillRectangle(lightBrush, pipeX - 4, gapBottom, pipeW + 8, capH);
                g.FillRectangle(darkBrush, pipeX + pipeW - 10, gapBottom, 14, capH);
                g.DrawRectangle(borderPen, pipeX - 4, gapBottom, pipeW + 8, capH);
            }
        }

        private static void DrawCoin(Graphics g, Coin coin)
        {
            int cx = (int)Math.Round(coin.X);
            int cy = (int)Math.Round(coin.Y);
            int r = coin.Radius;

            // Dış Altın Halkası
            using var goldOuter = new SolidBrush(Color.FromArgb(245, 158, 11));
            using var goldInner = new SolidBrush(Color.FromArgb(254, 240, 138));
            using var goldBorder = new Pen(Color.FromArgb(180, 83, 9), 1.8f);

            g.FillEllipse(goldOuter, cx - r, cy - r, r * 2, r * 2);
            g.FillEllipse(goldInner, cx - r + 3, cy - r + 3, (r - 3) * 2, (r - 3) * 2);
            g.DrawEllipse(goldBorder, cx - r, cy - r, r * 2, r * 2);

            // İç Yıldız/Sembol
            using var starBrush = new SolidBrush(Color.FromArgb(217, 119, 6));
            g.FillEllipse(starBrush, cx - 3, cy - 3, 6, 6);
        }

        private static void DrawPowerUp(Graphics g, PowerUp powerUp)
        {
            int px = (int)Math.Round(powerUp.X);
            int py = (int)Math.Round(powerUp.Y);
            int r = powerUp.Radius;

            Color orbColor = powerUp.Type switch
            {
                PowerUpType.Shield => Color.FromArgb(56, 189, 248),
                PowerUpType.Magnet => Color.FromArgb(244, 63, 94),
                PowerUpType.SlowMo => Color.FromArgb(168, 85, 247),
                PowerUpType.DoubleScore => Color.FromArgb(250, 204, 21),
                _ => Color.White
            };

            // Dış Işıltı
            using var glowBrush = new SolidBrush(Color.FromArgb(90, orbColor));
            g.FillEllipse(glowBrush, px - r - 4, py - r - 4, (r + 4) * 2, (r + 4) * 2);

            // Küre
            using var orbBrush = new SolidBrush(orbColor);
            using var borderPen = new Pen(Color.White, 2f);
            g.FillEllipse(orbBrush, px - r, py - r, r * 2, r * 2);
            g.DrawEllipse(borderPen, px - r, py - r, r * 2, r * 2);

            // Sembol Metni
            string symbol = powerUp.Type switch
            {
                PowerUpType.Shield => "S",
                PowerUpType.Magnet => "M",
                PowerUpType.SlowMo => "T",
                PowerUpType.DoubleScore => "2X",
                _ => "?"
            };

            using var font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            SizeF sz = g.MeasureString(symbol, font);
            g.DrawString(symbol, font, Brushes.White, px - (sz.Width / 2f), py - (sz.Height / 2f));
        }

        private static void DrawGround(Graphics g, int width, int height, int groundTopY, Color soilColor, Color grassColor, Color stripeColor, bool isNeon)
        {
            using var soilBrush = new SolidBrush(soilColor);
            using var grassBrush = new SolidBrush(grassColor);
            using var stripeBrush = new SolidBrush(stripeColor);
            using var borderPen = new Pen(isNeon ? Color.FromArgb(236, 72, 153) : Color.FromArgb(15, 23, 42), 2.5f);

            g.FillRectangle(soilBrush, 0, groundTopY, width, height - groundTopY);
            g.FillRectangle(grassBrush, 0, groundTopY, width, 14);

            for (int x = -32; x < width + 32; x += 32)
            {
                Point[] stripe =
                {
                    new Point(x, groundTopY),
                    new Point(x + 14, groundTopY + 14),
                    new Point(x + 22, groundTopY + 14),
                    new Point(x + 8, groundTopY)
                };
                g.FillPolygon(stripeBrush, stripe);
            }

            g.DrawLine(borderPen, 0, groundTopY, width, groundTopY);
        }

        private static void DrawBirdAuras(Graphics g, Bird bird, GameManager gm)
        {
            int bx = (int)Math.Round(bird.X);
            int by = (int)Math.Round(bird.Y);
            int cx = bx + (bird.Width / 2);
            int cy = by + (bird.Height / 2);

            // 1. Kalkan Enerji Küresi
            if (gm.HasShield)
            {
                using var shieldPen = new Pen(Color.FromArgb(220, 56, 189, 248), 2.5f);
                using var shieldFill = new SolidBrush(Color.FromArgb(45, 56, 189, 248));
                int r = 26;
                g.FillEllipse(shieldFill, cx - r, cy - r, r * 2, r * 2);
                g.DrawEllipse(shieldPen, cx - r, cy - r, r * 2, r * 2);
            }

            // 2. Mıknatıs Manyetik Dalgaları
            if (gm.IsMagnetActive)
            {
                using var magnetPen = new Pen(Color.FromArgb(180, 244, 63, 94), 2f);
                int r = 24 + ((int)(DateTime.Now.Millisecond / 35) % 12);
                g.DrawEllipse(magnetPen, cx - r, cy - r, r * 2, r * 2);
            }

            // 3. Ağır Çekim Dalgası
            if (gm.IsSlowMoActive)
            {
                using var slowPen = new Pen(Color.FromArgb(150, 168, 85, 247), 1.8f);
                g.DrawEllipse(slowPen, cx - 28, cy - 28, 56, 56);
            }
        }

        private static void DrawBird(Graphics g, Bird bird, BirdSkin skin)
        {
            int bx = (int)Math.Round(bird.X);
            int by = (int)Math.Round(bird.Y);
            Rectangle bodyRect = new Rectangle(bx, by, bird.Width, bird.Height);

            var colors = ThemePalette.GetBirdColors(skin);
            using var bodyBrush = new SolidBrush(colors.BodyColor);
            using var wingBrush = new SolidBrush(colors.WingColor);
            using var beakBrush = new SolidBrush(colors.BeakColor);
            using var outlinePen = new Pen(colors.OutlineColor, 2f);

            // Kral Kuş Pelerini
            if (skin == BirdSkin.AltinKral)
            {
                using var capeBrush = new SolidBrush(Color.FromArgb(220, 38, 38));
                Point[] cape =
                {
                    new Point(bx + 4, by + 12),
                    new Point(bx - 12, by + 26),
                    new Point(bx + 2, by + 24)
                };
                g.FillPolygon(capeBrush, cape);
                g.DrawPolygon(outlinePen, cape);
            }

            // Gövde
            g.FillEllipse(bodyBrush, bodyRect);
            g.DrawEllipse(outlinePen, bodyRect);

            // Özel Kostüm Ayrıntıları
            if (skin == BirdSkin.PolisKusu)
            {
                // Beyaz gömlek ve kravat
                Point[] collar = { new Point(bx + 22, by + 18), new Point(bx + 32, by + 18), new Point(bx + 27, by + 27) };
                g.FillPolygon(Brushes.White, collar);
                using var tieBrush = new SolidBrush(Color.FromArgb(220, 38, 38));
                Point[] tie = { new Point(bx + 26, by + 19), new Point(bx + 29, by + 19), new Point(bx + 28, by + 28), new Point(bx + 25, by + 28) };
                g.FillPolygon(tieBrush, tie);

                // Göğüste altın rozet
                using var badgeBrush = new SolidBrush(Color.FromArgb(250, 204, 21));
                g.FillEllipse(badgeBrush, bx + 18, by + 14, 6, 7);
                g.DrawEllipse(Pens.Black, bx + 18, by + 14, 6, 7);
            }
            else if (skin == BirdSkin.AltinKral)
            {
                // Altın Taç
                using var crownBrush = new SolidBrush(Color.FromArgb(250, 204, 21));
                Point[] crown =
                {
                    new Point(bx + 12, by + 2),
                    new Point(bx + 10, by - 12),
                    new Point(bx + 16, by - 6),
                    new Point(bx + 21, by - 14),
                    new Point(bx + 26, by - 6),
                    new Point(bx + 32, by - 12),
                    new Point(bx + 30, by + 2)
                };
                g.FillPolygon(crownBrush, crown);
                g.DrawPolygon(outlinePen, crown);

                // Mücevherler
                g.FillEllipse(Brushes.Red, bx + 15, by - 4, 3, 3);
                g.FillEllipse(Brushes.Blue, bx + 20, by - 7, 3, 3);
                g.FillEllipse(Brushes.Red, bx + 25, by - 4, 3, 3);
            }
            else if (skin == BirdSkin.SiberRoket)
            {
                // Jet Motor Alevi
                using var flameBrush = new SolidBrush(Color.FromArgb(56, 189, 248));
                Point[] flame =
                {
                    new Point(bx + 2, by + 10),
                    new Point(bx - 14, by + 15),
                    new Point(bx + 2, by + 20)
                };
                g.FillPolygon(flameBrush, flame);

                // Kask Vizörü
                using var visorBrush = new SolidBrush(Color.FromArgb(236, 72, 153));
                Rectangle visor = new Rectangle(bx + bird.Width - 16, by + 4, 14, 12);
                g.FillEllipse(visorBrush, visor);
                g.DrawEllipse(outlinePen, visor);
            }
            else if (skin != BirdSkin.SariKanarya)
            {
                // Üst ibik / tüyler
                Point[] crest = { new Point(bx + 14, by + 2), new Point(bx + 20, by - 6), new Point(bx + 26, by + 2) };
                g.FillPolygon(wingBrush, crest);
                g.DrawPolygon(outlinePen, crest);
            }

            // Kanat
            int wingOffsetY = (bird.WingPhase < 6) ? 8 : 12;
            Rectangle wingRect = new Rectangle(bx + 6, by + wingOffsetY, 16, 11);
            g.FillEllipse(wingBrush, wingRect);
            g.DrawEllipse(outlinePen, wingRect);

            // Göz
            if (skin != BirdSkin.SiberRoket)
            {
                Rectangle eyeRect = new Rectangle(bx + bird.Width - 15, by + 5, 10, 10);
                g.FillEllipse(Brushes.White, eyeRect);
                g.DrawEllipse(outlinePen, eyeRect);
                g.FillEllipse(Brushes.Black, bx + bird.Width - 11, by + 8, 4, 4);
            }

            // Polis Şapkası
            if (skin == BirdSkin.PolisKusu)
            {
                using var capBrush = new SolidBrush(Color.FromArgb(23, 37, 84));
                using var visorBrush = new SolidBrush(Color.FromArgb(15, 23, 42));
                using var goldBrush = new SolidBrush(Color.FromArgb(250, 204, 21));

                Point[] capTop =
                {
                    new Point(bx + 6, by + 3),
                    new Point(bx + 10, by - 7),
                    new Point(bx + 32, by - 8),
                    new Point(bx + 33, by + 3)
                };
                g.FillPolygon(capBrush, capTop);
                g.DrawPolygon(outlinePen, capTop);

                g.FillRectangle(Brushes.White, bx + 8, by + 1, 25, 3);

                Point[] visor =
                {
                    new Point(bx + 24, by + 2),
                    new Point(bx + 41, by + 5),
                    new Point(bx + 39, by + 8),
                    new Point(bx + 24, by + 5)
                };
                g.FillPolygon(visorBrush, visor);
                g.FillEllipse(goldBrush, bx + 24, by - 5, 5, 6);
            }

            // Gaga
            Point[] beakPoints =
            {
                new Point(bx + bird.Width - 2, by + 12),
                new Point(bx + bird.Width + 10, by + 16),
                new Point(bx + bird.Width - 2, by + 21)
            };
            g.FillPolygon(beakBrush, beakPoints);
            g.DrawPolygon(outlinePen, beakPoints);
        }

        private void DrawParticles(Graphics g)
        {
            foreach (Particle p in _gameManager.Particles)
            {
                int alpha = (int)(p.AlphaNormalized * 255);
                if (alpha <= 0) continue;

                if (p.Type == ParticleType.FloatingText)
                {
                    using var font = new Font("Segoe UI", 10.5f, FontStyle.Bold);
                    using var brush = new SolidBrush(Color.FromArgb(alpha, p.BaseColor));
                    using var shadow = new SolidBrush(Color.FromArgb(alpha / 2, Color.Black));
                    g.DrawString(p.Text, font, shadow, p.X + 1, p.Y + 1);
                    g.DrawString(p.Text, font, brush, p.X, p.Y);
                }
                else
                {
                    using var brush = new SolidBrush(Color.FromArgb(alpha, p.BaseColor));
                    g.FillEllipse(brush, p.X - (p.Size / 2f), p.Y - (p.Size / 2f), p.Size, p.Size);
                }
            }
        }

        private void DrawFloatingScore(Graphics g, int width)
        {
            string scoreText = _gameManager.Score.ToString();
            using var font = new Font("Segoe UI", 28f, FontStyle.Bold);
            SizeF textSize = g.MeasureString(scoreText, font);
            float x = (width - textSize.Width) / 2f;
            float y = 14f;

            using var shadowBrush = new SolidBrush(Color.FromArgb(130, 0, 0, 0));
            g.DrawString(scoreText, font, shadowBrush, x + 2, y + 2);
            g.DrawString(scoreText, font, Brushes.White, x, y);
        }

        private void DrawStateOverlay(Graphics g, int width, int groundTopY)
        {
            int boxWidth = 460;
            int boxHeight = 255;
            int boxX = (width - boxWidth) / 2;
            int boxY = (groundTopY - boxHeight) / 2;
            Rectangle cardRect = new Rectangle(boxX, boxY, boxWidth, boxHeight);

            using var cardBrush = new SolidBrush(Color.FromArgb(235, 15, 23, 42));
            using var borderPen = new Pen(Color.FromArgb(56, 189, 248), 2f);
            g.FillRectangle(cardBrush, cardRect);
            g.DrawRectangle(borderPen, cardRect);

            string title;
            string subtitle;
            string promptText;
            Color titleColor;

            if (_gameManager.State == GameState.GameOver)
            {
                title = "GAME OVER - OYUN BİTTİ";
                string recordNotice = _gameManager.IsNewRecordAchieved ? "🏆 YENİ REKOR! 🏆\n" : "";
                subtitle = $"{recordNotice}Kazanılan Skor: {_gameManager.Score}   |   Toplanan Altın: +{_gameManager.SessionCoins} 💰\nEn Yüksek Skor: {_gameManager.HighScore}   |   Toplam Altın: {_gameManager.TotalCoins} 💰";
                promptText = "Tekrar başlamak istiyor musunuz?";
                titleColor = Color.FromArgb(248, 113, 113);
            }
            else if (_gameManager.State == GameState.Paused)
            {
                title = "OYUN DURAKLATILDI";
                subtitle = $"Mevcut Skor: {_gameManager.Score}   |   Altın: {_gameManager.TotalCoins} 💰\nSeviye: {_gameManager.CurrentLevel}";
                promptText = "Devam etmek için [P] tuşuna veya 'Duraklat' butonuna basın.";
                titleColor = Color.FromArgb(250, 204, 21);
            }
            else
            {
                title = "FLAPPY BIRD DELUXE";
                subtitle = $"Altın Topla • Dükkandan Kostüm Al • Güçlendiricileri Yakala!\nEn Yüksek Skor: {_gameManager.HighScore}   |   Cüzdan: {_gameManager.TotalCoins} 💰";
                promptText = "Uçmak ve zıplamak için [SPACE], [YUKARI OK] veya Tıklayın.";
                titleColor = Color.FromArgb(74, 222, 128);
            }

            using var titleFont = new Font("Segoe UI", 16f, FontStyle.Bold);
            using var subFont = new Font("Segoe UI", 10.2f, FontStyle.Regular);
            using var promptFont = new Font("Segoe UI", 11f, FontStyle.Bold);
            using var titleBrush = new SolidBrush(titleColor);
            using var subBrush = new SolidBrush(Color.FromArgb(226, 232, 240));
            using var promptBrush = new SolidBrush(Color.FromArgb(250, 204, 21));
            using var centerFormat = new StringFormat { Alignment = StringAlignment.Center };

            g.DrawString(title, titleFont, titleBrush, new RectangleF(boxX + 10, boxY + 16, boxWidth - 20, 32), centerFormat);
            g.DrawString(subtitle, subFont, subBrush, new RectangleF(boxX + 16, boxY + 54, boxWidth - 32, 58), centerFormat);
            g.DrawString(promptText, promptFont, promptBrush, new RectangleF(boxX + 12, boxY + 130, boxWidth - 24, 34), centerFormat);
        }

        public void SaveDocumentationScreenshots(string outputDirectory)
        {
            Directory.CreateDirectory(outputDirectory);
            int w = 980;
            int h = 640;
            _gameManager.ResizeCanvas(w, h - 72);

            // 1. Ekran Görüntüsü: Oyun Aktif Oynanırken (Neon Tema + Kral Kuş)
            _gameManager.SelectedTheme = BackgroundTheme.NeonSiber;
            _gameManager.SelectedBirdSkin = BirdSkin.AltinKral;
            _gameManager.ConfigureSampleScene(gameOverState: false);
            RenderFullSnapshotToFile(Path.Combine(outputDirectory, "flappybird_gameplay.png"), w, h);

            // 2. Ekran Görüntüsü: Game Over Ekranı (Polis Kuşu)
            _gameManager.SelectedTheme = BackgroundTheme.Gunduz;
            _gameManager.SelectedBirdSkin = BirdSkin.PolisKusu;
            _gameManager.ConfigureSampleScene(gameOverState: true);
            RenderFullSnapshotToFile(Path.Combine(outputDirectory, "flappybird_gameover.png"), w, h);
        }

        private void RenderFullSnapshotToFile(string filePath, int totalWidth, int totalHeight)
        {
            using var bmp = new Bitmap(totalWidth, totalHeight);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.FromArgb(30, 41, 59));

                // Üst Bar Çizimi
                using var font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
                using var fontSm = new Font("Segoe UI", 8.5f, FontStyle.Regular);
                using var yellowBrush = new SolidBrush(Color.FromArgb(250, 204, 21));
                using var cyanBrush = new SolidBrush(Color.FromArgb(56, 189, 248));

                g.DrawString($"💰 {_gameManager.TotalCoins} Altın", font, yellowBrush, 20, 24);
                g.DrawString($"Skor: {_gameManager.Score}", font, cyanBrush, 140, 24);
                g.DrawString($"En Yüksek: {_gameManager.HighScore}", font, Brushes.LightGray, 240, 24);
                g.DrawString($"Zorluk: {_gameManager.Difficulty} | Tema: {_gameManager.SelectedTheme}", fontSm, Brushes.White, 360, 26);

                // Oyun Çizim Alanı
                g.TranslateTransform(0, 72);
                RenderGameCanvas(g, totalWidth, totalHeight - 72);
            }
            bmp.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
        }
    }
}
