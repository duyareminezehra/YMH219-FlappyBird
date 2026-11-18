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
    /// Flappy Bird masaüstü oyununun ana Windows Forms penceresi.
    /// Zamanlayıcı (Timer), klavye olayları, arka plan/kuş karakteri seçimi ve
    /// oyun bittiğinde 'Tekrar başlamak istiyor musunuz?' onay ekranını yönetir.
    /// </summary>
    public class MainForm : Form
    {
        private readonly GameManager _gameManager;
        private readonly System.Windows.Forms.Timer _gameTimer;

        private Panel _topBarPanel = null!;
        private DoubleBufferedPanel _canvasPanel = null!;
        private Button _btnStartRestart = null!;
        private Button _btnPause = null!;
        private ComboBox _cmbDifficulty = null!;
        private ComboBox _cmbTheme = null!;
        private ComboBox _cmbBirdSkin = null!;
        private CheckBox _chkSound = null!;
        private Label _lblScore = null!;
        private Label _lblHighScore = null!;
        private Label _lblLevelSpeed = null!;
        private Label _lblStatus = null!;

        // Game Over Tekrar Başlama Seçenek Butonları
        private Button _btnRetryYes = null!;
        private Button _btnRetryNo = null!;

        public MainForm()
        {
            Text = "YMH219 - Flappy Bird Masaüstü Oyunu";
            Size = new Size(980, 640);
            MinimumSize = new Size(880, 560);
            StartPosition = FormStartPosition.CenterScreen;
            KeyPreview = true;
            BackColor = Color.FromArgb(24, 32, 44);

            InitializeComponents();

            _gameManager = new GameManager(_canvasPanel.ClientSize.Width, _canvasPanel.ClientSize.Height);
            _gameManager.ScoreUpdated += (_, _) => UpdateHudLabels();
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

            UpdateHudLabels();
            UpdateRetryButtonsVisibility();
        }

        private void InitializeComponents()
        {
            _topBarPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 68,
                BackColor = Color.FromArgb(30, 41, 59),
                Padding = new Padding(10, 8, 10, 8)
            };

            _btnStartRestart = new Button
            {
                Text = "Oyunu Başlat",
                Location = new Point(12, 15),
                Size = new Size(122, 36),
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
                Location = new Point(140, 15),
                Size = new Size(96, 36),
                BackColor = Color.FromArgb(59, 130, 246),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.8f, FontStyle.Bold),
                TabStop = false
            };
            _btnPause.FlatAppearance.BorderSize = 0;
            _btnPause.Click += (_, _) => TogglePauseGame();

            // Zorluk Seçimi
            var lblDiffTitle = new Label
            {
                Text = "Zorluk:",
                Location = new Point(246, 10),
                AutoSize = true,
                ForeColor = Color.FromArgb(203, 213, 225),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
            };

            _cmbDifficulty = new ComboBox
            {
                Location = new Point(246, 30),
                Size = new Size(86, 26),
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
                Text = "Arka Plan:",
                Location = new Point(340, 10),
                AutoSize = true,
                ForeColor = Color.FromArgb(203, 213, 225),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
            };

            _cmbTheme = new ComboBox
            {
                Location = new Point(340, 30),
                Size = new Size(125, 26),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 8.8f),
                TabStop = false
            };
            _cmbTheme.Items.AddRange(new object[]
            {
                "Gündüz (Mavi)",
                "Gün Batımı",
                "Gece (Yıldızlı)",
                "Karlı Kış"
            });
            _cmbTheme.SelectedIndex = 0;
            _cmbTheme.SelectedIndexChanged += CmbTheme_SelectedIndexChanged;

            // Kuş Karakteri / Rengi Seçimi
            var lblBirdTitle = new Label
            {
                Text = "Kuş Seçimi:",
                Location = new Point(473, 10),
                AutoSize = true,
                ForeColor = Color.FromArgb(203, 213, 225),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
            };

            _cmbBirdSkin = new ComboBox
            {
                Location = new Point(473, 30),
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
                "Polis Kuşu"
            });
            _cmbBirdSkin.SelectedIndex = 0;
            _cmbBirdSkin.SelectedIndexChanged += CmbBirdSkin_SelectedIndexChanged;

            _chkSound = new CheckBox
            {
                Text = "Ses",
                Checked = true,
                Location = new Point(612, 28),
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

            _lblScore = new Label
            {
                Text = "Skor: 0",
                Location = new Point(670, 12),
                Size = new Size(115, 22),
                ForeColor = Color.FromArgb(250, 204, 21),
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold)
            };

            _lblHighScore = new Label
            {
                Text = "En Yüksek: 0",
                Location = new Point(670, 35),
                Size = new Size(125, 20),
                ForeColor = Color.FromArgb(148, 163, 184),
                Font = new Font("Segoe UI", 8.8f, FontStyle.Bold)
            };

            _lblLevelSpeed = new Label
            {
                Text = "Seviye: 1 | Hız: 4.8",
                Location = new Point(795, 13),
                Size = new Size(165, 20),
                ForeColor = Color.FromArgb(125, 211, 252),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };

            _lblStatus = new Label
            {
                Text = "Durum: Hazır",
                Location = new Point(795, 35),
                Size = new Size(165, 20),
                ForeColor = Color.FromArgb(134, 239, 172),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular)
            };

            _topBarPanel.Controls.AddRange(new Control[]
            {
                _btnStartRestart,
                _btnPause,
                lblDiffTitle,
                _cmbDifficulty,
                lblThemeTitle,
                _cmbTheme,
                lblBirdTitle,
                _cmbBirdSkin,
                _chkSound,
                _lblScore,
                _lblHighScore,
                _lblLevelSpeed,
                _lblStatus
            });

            _canvasPanel = new DoubleBufferedPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.SkyBlue
            };
            _canvasPanel.Paint += CanvasPanel_Paint;
            _canvasPanel.Resize += (_, _) =>
            {
                _gameManager?.ResizeCanvas(_canvasPanel.ClientSize.Width, _canvasPanel.ClientSize.Height);
                PositionRetryButtons();
                _canvasPanel.Invalidate();
            };
            _canvasPanel.MouseDown += (_, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    HandleJumpAction();
                }
            };

            // Game Over ekranında gösterilecek 'Evet / Hayır' seçim butonları
            _btnRetryYes = new Button
            {
                Text = "✔ Evet, Tekrar Oyna",
                Size = new Size(185, 40),
                BackColor = Color.FromArgb(22, 163, 74),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Visible = false,
                TabStop = false
            };
            _btnRetryYes.FlatAppearance.BorderSize = 0;
            _btnRetryYes.Click += (_, _) => StartOrRestartGame();

            _btnRetryNo = new Button
            {
                Text = "✖ Hayır, Ana Ekran",
                Size = new Size(185, 40),
                BackColor = Color.FromArgb(100, 116, 139),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Visible = false,
                TabStop = false
            };
            _btnRetryNo.FlatAppearance.BorderSize = 0;
            _btnRetryNo.Click += (_, _) =>
            {
                _gameTimer.Stop();
                _gameManager.ReturnToReadyScreen();
                UpdateHudLabels();
                UpdateRetryButtonsVisibility();
                _canvasPanel.Invalidate();
                ActiveControl = null;
                Focus();
            };

            _canvasPanel.Controls.Add(_btnRetryYes);
            _canvasPanel.Controls.Add(_btnRetryNo);

            Controls.Add(_canvasPanel);
            Controls.Add(_topBarPanel);
        }

        private void PositionRetryButtons()
        {
            if (_gameManager == null) return;
            int width = _canvasPanel.ClientSize.Width;
            int groundTopY = _gameManager.GroundTopY;
            int boxHeight = 235;
            int boxY = (groundTopY - boxHeight) / 2;

            int totalButtonsWidth = _btnRetryYes.Width + 16 + _btnRetryNo.Width;
            int startX = (width - totalButtonsWidth) / 2;
            int btnY = boxY + 174;

            _btnRetryYes.Location = new Point(startX, btnY);
            _btnRetryNo.Location = new Point(startX + _btnRetryYes.Width + 16, btnY);
        }

        private void UpdateRetryButtonsVisibility()
        {
            if (_gameManager == null) return;
            bool isGameOver = _gameManager.State == GameState.GameOver;
            PositionRetryButtons();
            _btnRetryYes.Visible = isGameOver;
            _btnRetryNo.Visible = isGameOver;
        }

        private void CmbDifficulty_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_gameManager == null) return;

            DifficultyLevel selected = _cmbDifficulty.SelectedIndex switch
            {
                0 => DifficultyLevel.Kolay,
                2 => DifficultyLevel.Zor,
                _ => DifficultyLevel.Normal
            };

            _gameManager.ApplyDifficulty(selected);
            UpdateHudLabels();
            _canvasPanel.Invalidate();
            ActiveControl = null;
        }

        private void CmbTheme_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_gameManager == null) return;

            _gameManager.SelectedTheme = _cmbTheme.SelectedIndex switch
            {
                1 => BackgroundTheme.GunBatimi,
                2 => BackgroundTheme.Gece,
                3 => BackgroundTheme.KarliKis,
                _ => BackgroundTheme.Gunduz
            };

            _canvasPanel.Invalidate();
            ActiveControl = null;
        }

        private void CmbBirdSkin_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_gameManager == null) return;

            _gameManager.SelectedBirdSkin = _cmbBirdSkin.SelectedIndex switch
            {
                1 => BirdSkin.KizilAnka,
                2 => BirdSkin.MaviFirtina,
                3 => BirdSkin.ZumrutKusu,
                4 => BirdSkin.MorGolge,
                5 => BirdSkin.PolisKusu,
                _ => BirdSkin.SariKanarya
            };

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
            // Oyun bittiğinde (GameOver) yanlışlıkla Space veya tıklama ile hemen yeniden başlamasını engelle!
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

        private void GameTimer_Tick(object? sender, EventArgs e)
        {
            _gameManager.UpdateFrame();
            _canvasPanel.Invalidate();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Space || keyData == Keys.Up || keyData == Keys.W)
            {
                HandleJumpAction();
                return true;
            }

            if (keyData == Keys.P)
            {
                TogglePauseGame();
                return true;
            }

            if (keyData == Keys.R)
            {
                StartOrRestartGame();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void UpdateHudLabels()
        {
            if (_gameManager == null) return;

            _lblScore.Text = $"Skor: {_gameManager.Score}";
            _lblHighScore.Text = $"En Yüksek: {_gameManager.HighScore}";
            _lblLevelSpeed.Text = $"Seviye: {_gameManager.CurrentLevel} | Hız: {_gameManager.CurrentPipeSpeed:F1}";

            switch (_gameManager.State)
            {
                case GameState.Ready:
                    _lblStatus.Text = "Durum: Hazır (Space)";
                    _lblStatus.ForeColor = Color.FromArgb(134, 239, 172);
                    _btnStartRestart.Text = "Oyunu Başlat";
                    break;
                case GameState.Playing:
                    _lblStatus.Text = "Durum: Oynanıyor";
                    _lblStatus.ForeColor = Color.FromArgb(56, 189, 248);
                    _btnStartRestart.Text = "Yeniden Başlat";
                    break;
                case GameState.Paused:
                    _lblStatus.Text = "Durum: Duraklatıldı";
                    _lblStatus.ForeColor = Color.FromArgb(250, 204, 21);
                    break;
                case GameState.GameOver:
                    _lblStatus.Text = "Durum: OYUN BİTTİ!";
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

            // 1. Gökyüzü degrade arka planı (Seçilen Temaya Göre)
            using (var skyBrush = new LinearGradientBrush(
                new Rectangle(0, 0, Math.Max(1, width), Math.Max(1, groundTopY)),
                themeColors.SkyTop,
                themeColors.SkyBottom,
                LinearGradientMode.Vertical))
            {
                g.FillRectangle(skyBrush, 0, 0, width, groundTopY);
            }

            // Gece temasıysa yıldızlar ve hilal çiz
            if (themeColors.ShowStars)
            {
                DrawNightStarsAndMoon(g, width);
            }

            // 2. Dekoratif bulutlar
            DrawCloud(g, 90, 65, 95, 36, themeColors.ShowStars);
            DrawCloud(g, 360, 48, 115, 40, themeColors.ShowStars);
            DrawCloud(g, 660, 80, 100, 34, themeColors.ShowStars);

            // 3. Engeller (Borular)
            foreach (PipePair pipe in _gameManager.Pipes)
            {
                DrawPipePair(g, pipe, groundTopY, themeColors.PipeLight, themeColors.PipeDark);
            }

            // 4. Zemin
            DrawGround(g, width, height, groundTopY, themeColors.GroundSoil, themeColors.GroundGrass, themeColors.GroundStripe);

            // 5. Oyuncu Kuş (Seçilen Kuş Karakterine Göre)
            DrawBird(g, _gameManager.PlayerBird, _gameManager.SelectedBirdSkin);

            // 6. Canlı Skor Rozeti
            DrawFloatingScore(g, width);

            // 7. Durum Katmanları (Hazır / Duraklatıldı / Game Over)
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

            // Ay
            int moonX = Math.Max(400, width - 140);
            using var moonBrush = new SolidBrush(Color.FromArgb(254, 249, 195));
            g.FillEllipse(moonBrush, moonX, 32, 46, 46);
        }

        private static void DrawCloud(Graphics g, int x, int y, int width, int height, bool isNight)
        {
            Color c = isNight ? Color.FromArgb(90, 148, 163, 184) : Color.FromArgb(215, 255, 255, 255);
            using var cloudBrush = new SolidBrush(c);
            g.FillEllipse(cloudBrush, x, y + 8, width / 2, height - 8);
            g.FillEllipse(cloudBrush, x + width / 4, y, width / 2, height);
            g.FillEllipse(cloudBrush, x + width / 2, y + 10, width / 2, height - 10);
        }

        private static void DrawPipePair(Graphics g, PipePair pipe, int groundTopY, Color pipeLight, Color pipeDark)
        {
            Rectangle topRect = pipe.GetTopPipeBounds();
            Rectangle bottomRect = pipe.GetBottomPipeBounds(groundTopY);

            using var pipeBrush = new LinearGradientBrush(
                new Rectangle((int)pipe.X, 0, Math.Max(1, pipe.Width), 10),
                pipeLight,
                pipeDark,
                LinearGradientMode.Horizontal);
            using var borderPen = new Pen(Color.FromArgb(15, 23, 42), 2f);

            if (topRect.Height > 0)
            {
                g.FillRectangle(pipeBrush, topRect);
                g.DrawRectangle(borderPen, topRect);

                Rectangle topCap = new Rectangle(topRect.X - 4, Math.Max(0, topRect.Bottom - 22), topRect.Width + 8, 22);
                g.FillRectangle(pipeBrush, topCap);
                g.DrawRectangle(borderPen, topCap);
            }

            if (bottomRect.Height > 0)
            {
                g.FillRectangle(pipeBrush, bottomRect);
                g.DrawRectangle(borderPen, bottomRect);

                Rectangle bottomCap = new Rectangle(bottomRect.X - 4, bottomRect.Top, bottomRect.Width + 8, Math.Min(22, bottomRect.Height));
                g.FillRectangle(pipeBrush, bottomCap);
                g.DrawRectangle(borderPen, bottomCap);
            }
        }

        private void DrawGround(Graphics g, int width, int height, int groundTopY, Color soilColor, Color grassColor, Color stripeColor)
        {
            Rectangle groundRect = new Rectangle(0, groundTopY, width, height - groundTopY);
            using var soilBrush = new SolidBrush(soilColor);
            using var grassBrush = new SolidBrush(grassColor);
            using var stripeBrush = new SolidBrush(stripeColor);
            using var borderPen = new Pen(Color.FromArgb(30, 41, 59), 2f);

            g.FillRectangle(soilBrush, groundRect);
            g.FillRectangle(grassBrush, 0, groundTopY, width, 16);

            for (int x = -_gameManager.GroundScrollOffset; x < width; x += 32)
            {
                Point[] stripe =
                {
                    new Point(x, groundTopY + 16),
                    new Point(x + 14, groundTopY + 16),
                    new Point(x + 22, groundTopY),
                    new Point(x + 8, groundTopY)
                };
                g.FillPolygon(stripeBrush, stripe);
            }

            g.DrawLine(borderPen, 0, groundTopY, width, groundTopY);
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

            // Gövde
            g.FillEllipse(bodyBrush, bodyRect);
            g.DrawEllipse(outlinePen, bodyRect);

            if (skin == BirdSkin.PolisKusu)
            {
                // Beyaz gömlek yakası ve kırmızı kravat
                Point[] collar =
                {
                    new Point(bx + 22, by + 18),
                    new Point(bx + 32, by + 18),
                    new Point(bx + 27, by + 27)
                };
                g.FillPolygon(Brushes.White, collar);
                using var tieBrush = new SolidBrush(Color.FromArgb(220, 38, 38));
                Point[] tie =
                {
                    new Point(bx + 26, by + 19),
                    new Point(bx + 29, by + 19),
                    new Point(bx + 28, by + 28),
                    new Point(bx + 25, by + 28)
                };
                g.FillPolygon(tieBrush, tie);

                // Göğüste altın polis rozeti
                using var badgeBrush = new SolidBrush(Color.FromArgb(250, 204, 21));
                g.FillEllipse(badgeBrush, bx + 18, by + 14, 6, 7);
                g.DrawEllipse(Pens.Black, bx + 18, by + 14, 6, 7);
            }
            else if (skin != BirdSkin.SariKanarya)
            {
                // Üst ibik / tüy (Diğer özel kuş görünümleri için)
                Point[] crest =
                {
                    new Point(bx + 14, by + 2),
                    new Point(bx + 20, by - 6),
                    new Point(bx + 26, by + 2)
                };
                g.FillPolygon(wingBrush, crest);
                g.DrawPolygon(outlinePen, crest);
            }

            // Kanat (hareketli)
            int wingOffsetY = (bird.WingPhase < 6) ? 8 : 12;
            Rectangle wingRect = new Rectangle(bx + 6, by + wingOffsetY, 16, 11);
            g.FillEllipse(wingBrush, wingRect);
            g.DrawEllipse(outlinePen, wingRect);

            // Göz
            Rectangle eyeRect = new Rectangle(bx + bird.Width - 15, by + 5, 10, 10);
            g.FillEllipse(Brushes.White, eyeRect);
            g.DrawEllipse(outlinePen, eyeRect);
            g.FillEllipse(Brushes.Black, bx + bird.Width - 11, by + 8, 4, 4);

            // Polis Şapkası (Kep, Siperlik ve Altın Kokart)
            if (skin == BirdSkin.PolisKusu)
            {
                using var capBrush = new SolidBrush(Color.FromArgb(23, 37, 84));
                using var visorBrush = new SolidBrush(Color.FromArgb(15, 23, 42));
                using var goldBrush = new SolidBrush(Color.FromArgb(250, 204, 21));

                // Şapka üst tepe kısmı
                Point[] capTop =
                {
                    new Point(bx + 6, by + 3),
                    new Point(bx + 10, by - 7),
                    new Point(bx + 32, by - 8),
                    new Point(bx + 33, by + 3)
                };
                g.FillPolygon(capBrush, capTop);
                g.DrawPolygon(outlinePen, capTop);

                // Beyaz şerit
                g.FillRectangle(Brushes.White, bx + 8, by + 1, 25, 3);

                // Siyah ön siperlik
                Point[] visor =
                {
                    new Point(bx + 24, by + 2),
                    new Point(bx + 41, by + 5),
                    new Point(bx + 39, by + 8),
                    new Point(bx + 24, by + 5)
                };
                g.FillPolygon(visorBrush, visor);

                // Şapka önündeki altın polis arması
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

        private void DrawFloatingScore(Graphics g, int width)
        {
            string scoreText = _gameManager.Score.ToString();
            using var font = new Font("Segoe UI", 26f, FontStyle.Bold);
            SizeF textSize = g.MeasureString(scoreText, font);
            float x = (width - textSize.Width) / 2f;
            float y = 16f;

            using var shadowBrush = new SolidBrush(Color.FromArgb(120, 0, 0, 0));
            g.DrawString(scoreText, font, shadowBrush, x + 2, y + 2);
            g.DrawString(scoreText, font, Brushes.White, x, y);
        }

        private void DrawStateOverlay(Graphics g, int width, int groundTopY)
        {
            int boxWidth = 440;
            int boxHeight = 235;
            int boxX = (width - boxWidth) / 2;
            int boxY = (groundTopY - boxHeight) / 2;
            Rectangle cardRect = new Rectangle(boxX, boxY, boxWidth, boxHeight);

            using var cardBrush = new SolidBrush(Color.FromArgb(232, 15, 23, 42));
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
                subtitle = $"Skorunuz: {_gameManager.Score}   |   En Yüksek Skor: {_gameManager.HighScore}\nUlaşılan Seviye: {_gameManager.CurrentLevel} ({_gameManager.Difficulty})";
                promptText = "Tekrar başlamak istiyor musunuz?";
                titleColor = Color.FromArgb(248, 113, 113);
            }
            else if (_gameManager.State == GameState.Paused)
            {
                title = "OYUN DURAKLATILDI";
                subtitle = $"Mevcut Skor: {_gameManager.Score}   |   Seviye: {_gameManager.CurrentLevel}";
                promptText = "Devam etmek için [P] tuşuna veya 'Duraklat' butonuna basın.";
                titleColor = Color.FromArgb(250, 204, 21);
            }
            else
            {
                title = "FLAPPY BIRD MASAÜSTÜ";
                subtitle = $"Zorluk: {_gameManager.Difficulty}   |   En Yüksek Skor: {_gameManager.HighScore}\nÜst menüden Arka Plan ve Kuş görünümünü seçebilirsiniz!";
                promptText = "Başlamak ve zıplamak için [SPACE], [YUKARI OK] veya Sol Tık kullanın.";
                titleColor = Color.FromArgb(74, 222, 128);
            }

            using var titleFont = new Font("Segoe UI", 16f, FontStyle.Bold);
            using var subFont = new Font("Segoe UI", 10.2f, FontStyle.Regular);
            using var promptFont = new Font("Segoe UI", 11f, FontStyle.Bold);
            using var titleBrush = new SolidBrush(titleColor);
            using var subBrush = new SolidBrush(Color.FromArgb(226, 232, 240));
            using var promptBrush = new SolidBrush(Color.FromArgb(250, 204, 21));
            using var centerFormat = new StringFormat { Alignment = StringAlignment.Center };

            g.DrawString(title, titleFont, titleBrush, new RectangleF(boxX + 10, boxY + 18, boxWidth - 20, 34), centerFormat);
            g.DrawString(subtitle, subFont, subBrush, new RectangleF(boxX + 16, boxY + 62, boxWidth - 32, 54), centerFormat);
            g.DrawString(promptText, promptFont, promptBrush, new RectangleF(boxX + 12, boxY + 130, boxWidth - 24, 34), centerFormat);
        }

        /// <summary>
        /// README.md dokümantasyonu için oyun içi ve Game Over ekran görüntülerini oluşturur.
        /// </summary>
        public void SaveDocumentationScreenshots(string outputDirectory)
        {
            Directory.CreateDirectory(outputDirectory);
            Show();
            PerformLayout();
            _gameManager.ResizeCanvas(_canvasPanel.ClientSize.Width, _canvasPanel.ClientSize.Height);

            // 1. Ekran Görüntüsü: Oyun Aktif Oynanırken (Gece Teması + Polis Kuşu)
            _cmbTheme.SelectedIndex = 2; // Gece (Yıldızlı)
            _cmbBirdSkin.SelectedIndex = 5; // Polis Kuşu
            _gameManager.ConfigureSampleScene(gameOverState: false);
            UpdateHudLabels();
            UpdateRetryButtonsVisibility();
            Refresh();
            SaveFormSnapshot(Path.Combine(outputDirectory, "flappybird_gameplay.png"));

            // 2. Ekran Görüntüsü: Çarpışma Sonrası Game Over ve 'Tekrar başlamak istiyor musunuz?' Seçenek Ekranı
            _cmbTheme.SelectedIndex = 0; // Gündüz
            _cmbBirdSkin.SelectedIndex = 0; // Sarı Kanarya
            _gameManager.ConfigureSampleScene(gameOverState: true);
            UpdateHudLabels();
            UpdateRetryButtonsVisibility();
            Refresh();
            SaveFormSnapshot(Path.Combine(outputDirectory, "flappybird_gameover.png"));
        }

        private void SaveFormSnapshot(string filePath)
        {
            using var bmp = new Bitmap(Width, Height);
            DrawToBitmap(bmp, new Rectangle(0, 0, Width, Height));
            bmp.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
        }
    }
}
