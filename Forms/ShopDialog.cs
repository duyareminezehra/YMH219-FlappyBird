using System;
using System.Drawing;
using System.Windows.Forms;
using YMH219_FlappyBird.Models;
using YMH219_FlappyBird.Services;

namespace YMH219_FlappyBird.Forms
{
    /// <summary>
    /// Oyuncunun topladığı altınlarla yeni kuş kostümleri ve arka plan temaları
    /// satın alabileceği ve seçebileceği modern mağaza iletişim penceresi.
    /// </summary>
    public class ShopDialog : Form
    {
        private readonly GameManager _gameManager;
        private Label _lblBalance = null!;
        private TabControl _tabControl = null!;
        private FlowLayoutPanel _pnlSkins = null!;
        private FlowLayoutPanel _pnlThemes = null!;

        public ShopDialog(GameManager gameManager)
        {
            _gameManager = gameManager;

            Text = "Flappy Bird - Karakter & Tema Dükkanı 🛍️";
            Size = new Size(720, 520);
            MinimumSize = new Size(680, 480);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(15, 23, 42);
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 9.5f);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            InitializeComponents();
            RefreshStoreItems();
        }

        private void InitializeComponents()
        {
            // Üst Başlık & Bakiye Paneli
            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 65,
                BackColor = Color.FromArgb(30, 41, 59),
                Padding = new Padding(16, 12, 16, 12)
            };

            var lblTitle = new Label
            {
                Text = "KUŞ & TEMA MAĞAZASI",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.FromArgb(56, 189, 248),
                Location = new Point(16, 18),
                AutoSize = true
            };

            _lblBalance = new Label
            {
                Text = $"💰 Altın Bakiyeniz: {ProfileManager.Instance.CoinsBalance}",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.FromArgb(250, 204, 21),
                Location = new Point(440, 18),
                AutoSize = true
            };

            topPanel.Controls.Add(lblTitle);
            topPanel.Controls.Add(_lblBalance);

            // Sekmeler (Kuşlar & Temalar)
            _tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Padding = new Point(14, 8)
            };

            var tabSkins = new TabPage("Kuş Kostümleri 🐦")
            {
                BackColor = Color.FromArgb(15, 23, 42)
            };

            var tabThemes = new TabPage("Arka Plan Temaları 🎨")
            {
                BackColor = Color.FromArgb(15, 23, 42)
            };

            _pnlSkins = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(12),
                BackColor = Color.FromArgb(15, 23, 42)
            };

            _pnlThemes = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(12),
                BackColor = Color.FromArgb(15, 23, 42)
            };

            tabSkins.Controls.Add(_pnlSkins);
            tabThemes.Controls.Add(_pnlThemes);

            _tabControl.TabPages.Add(tabSkins);
            _tabControl.TabPages.Add(tabThemes);

            // Alt Kapat Butonu
            var bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.FromArgb(30, 41, 59),
                Padding = new Padding(10)
            };

            var btnClose = new Button
            {
                Text = "Tamam / Oyuna Dön",
                Size = new Size(160, 32),
                Location = new Point(530, 9),
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (_, _) => Close();

            bottomPanel.Controls.Add(btnClose);

            Controls.Add(_tabControl);
            Controls.Add(bottomPanel);
            Controls.Add(topPanel);
        }

        private void RefreshStoreItems()
        {
            _lblBalance.Text = $"💰 Altın Bakiyeniz: {ProfileManager.Instance.CoinsBalance}";
            _pnlSkins.Controls.Clear();
            _pnlThemes.Controls.Clear();

            // 1. Kuş Kostümleri Kartları
            foreach (var kvp in ThemePalette.SkinStoreCatalog)
            {
                BirdSkin skin = kvp.Key;
                var (name, price, desc) = kvp.Value;
                bool isUnlocked = ProfileManager.Instance.IsSkinUnlocked(skin);
                bool isSelected = _gameManager.SelectedBirdSkin == skin;

                var card = CreateItemCard(name, price, desc, isUnlocked, isSelected,
                    onBuy: () =>
                    {
                        if (ProfileManager.Instance.TryBuySkin(skin))
                        {
                            _gameManager.SelectedBirdSkin = skin;
                            RefreshStoreItems();
                        }
                        else
                        {
                            MessageBox.Show("Bu kostümü almak için yeterli altınınız yok! Oyun oynayarak daha fazla altın toplayabilirsiniz.", "Yetersiz Bakiye", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    },
                    onSelect: () =>
                    {
                        _gameManager.SelectedBirdSkin = skin;
                        RefreshStoreItems();
                    });

                _pnlSkins.Controls.Add(card);
            }

            // 2. Arka Plan Temaları Kartları
            foreach (var kvp in ThemePalette.ThemeStoreCatalog)
            {
                BackgroundTheme theme = kvp.Key;
                var (name, price, desc) = kvp.Value;
                bool isUnlocked = ProfileManager.Instance.IsThemeUnlocked(theme);
                bool isSelected = _gameManager.SelectedTheme == theme;

                var card = CreateItemCard(name, price, desc, isUnlocked, isSelected,
                    onBuy: () =>
                    {
                        if (ProfileManager.Instance.TryBuyTheme(theme))
                        {
                            _gameManager.SelectedTheme = theme;
                            RefreshStoreItems();
                        }
                        else
                        {
                            MessageBox.Show("Bu temayı almak için yeterli altınınız yok!", "Yetersiz Bakiye", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    },
                    onSelect: () =>
                    {
                        _gameManager.SelectedTheme = theme;
                        RefreshStoreItems();
                    });

                _pnlThemes.Controls.Add(card);
            }
        }

        private static Panel CreateItemCard(string name, int price, string desc, bool isUnlocked, bool isSelected, Action onBuy, Action onSelect)
        {
            var pnl = new Panel
            {
                Size = new Size(310, 115),
                Margin = new Padding(8),
                BackColor = isSelected ? Color.FromArgb(30, 58, 138) : Color.FromArgb(30, 41, 59),
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblName = new Label
            {
                Text = name,
                Location = new Point(12, 10),
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = isSelected ? Color.FromArgb(250, 204, 21) : Color.White,
                AutoSize = true
            };

            var lblDesc = new Label
            {
                Text = desc,
                Location = new Point(12, 34),
                Size = new Size(285, 34),
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(203, 213, 225)
            };

            var btnAction = new Button
            {
                Size = new Size(130, 30),
                Location = new Point(165, 74),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnAction.FlatAppearance.BorderSize = 0;

            if (isSelected)
            {
                btnAction.Text = "✓ Seçildi";
                btnAction.BackColor = Color.FromArgb(16, 185, 129);
                btnAction.ForeColor = Color.White;
                btnAction.Enabled = false;
            }
            else if (isUnlocked)
            {
                btnAction.Text = "Kullan / Seç";
                btnAction.BackColor = Color.FromArgb(59, 130, 246);
                btnAction.ForeColor = Color.White;
                btnAction.Click += (_, _) => onSelect();
            }
            else
            {
                btnAction.Text = $"Satın Al ({price} 💰)";
                btnAction.BackColor = Color.FromArgb(217, 119, 6);
                btnAction.ForeColor = Color.White;
                btnAction.Click += (_, _) => onBuy();
            }

            pnl.Controls.Add(lblName);
            pnl.Controls.Add(lblDesc);
            pnl.Controls.Add(btnAction);

            return pnl;
        }
    }
}
