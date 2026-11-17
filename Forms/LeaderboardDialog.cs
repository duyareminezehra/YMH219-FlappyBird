using System;
using System.Drawing;
using System.Windows.Forms;
using YMH219_FlappyBird.Services;

namespace YMH219_FlappyBird.Forms
{
    /// <summary>
    /// En iyi 5 oyun skorunu, kazanılan altınları ve kullanılan kuş kostümlerini
    /// listeleyen Liderlik Tablosu iletişim penceresi.
    /// </summary>
    public class LeaderboardDialog : Form
    {
        public LeaderboardDialog()
        {
            Text = "Liderlik Tablosu - En Yüksek Skorlar 🏆";
            Size = new Size(540, 420);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(15, 23, 42);
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 9.5f);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(30, 41, 59),
                Padding = new Padding(16, 12, 16, 12)
            };

            var lblTitle = new Label
            {
                Text = "🏆 EN İYİ 5 SKOR SIRALAMASI",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.FromArgb(250, 204, 21),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            topPanel.Controls.Add(lblTitle);

            var listView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                BackColor = Color.FromArgb(15, 23, 42),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f),
                BorderStyle = BorderStyle.None
            };

            listView.Columns.Add("Sıra", 55, HorizontalAlignment.Center);
            listView.Columns.Add("Skor", 75, HorizontalAlignment.Center);
            listView.Columns.Add("Altın", 75, HorizontalAlignment.Center);
            listView.Columns.Add("Kuş Karakteri", 140, HorizontalAlignment.Left);
            listView.Columns.Add("Tarih", 160, HorizontalAlignment.Left);

            var records = ProfileManager.Instance.Leaderboard;
            if (records.Count == 0)
            {
                var emptyItem = new ListViewItem(new[] { "-", "0", "0", "Henüz kayıt yok", "-" });
                listView.Items.Add(emptyItem);
            }
            else
            {
                for (int i = 0; i < records.Count; i++)
                {
                    var r = records[i];
                    string medal = i switch
                    {
                        0 => "🥇 1",
                        1 => "🥈 2",
                        2 => "🥉 3",
                        _ => $"   {i + 1}"
                    };
                    var item = new ListViewItem(new[]
                    {
                        medal,
                        r.Score.ToString(),
                        $"{r.Coins} 💰",
                        r.BirdSkinName,
                        r.Date.ToString("dd.MM.yyyy HH:mm")
                    });
                    listView.Items.Add(item);
                }
            }

            var bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.FromArgb(30, 41, 59)
            };

            var btnClose = new Button
            {
                Text = "Kapat",
                Size = new Size(110, 32),
                Location = new Point(215, 9),
                BackColor = Color.FromArgb(59, 130, 246),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (_, _) => Close();

            bottomPanel.Controls.Add(btnClose);

            Controls.Add(listView);
            Controls.Add(bottomPanel);
            Controls.Add(topPanel);
        }
    }
}
