using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using HemaLeagueManager.Models;
using HemaLeagueManager.Services;

namespace HemaLeagueManager.Forms
{
    public class ClubsPage : UserControl
    {
        private readonly Func<League> _getLeague;
        private readonly Action _onChanged;

        private ListView _grid = null!;
        private Label _summaryLabel = null!;

        public ClubsPage(Func<League> getLeague, Action onChanged)
        {
            _getLeague = getLeague;
            _onChanged = onChanged;
            BuildUi();
        }

        private void BuildUi()
        {
            Dock = DockStyle.Fill;
            BackColor = UiTheme.Background;
            ForeColor = UiTheme.TextPrimary;
            Font = UiTheme.Body;
            Padding = new Padding(20);

            var titleRow = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = UiTheme.Background };
            titleRow.Controls.Add(new Label
            {
                Text = "Clubs",
                Font = UiTheme.TitleMedium,
                ForeColor = UiTheme.Accent,
                AutoSize = true,
                Location = new Point(0, 10)
            });
            titleRow.Controls.Add(new Label
            {
                Text = "Registered fencing clubs and their performance in the current league",
                Font = UiTheme.Small,
                ForeColor = UiTheme.TextMuted,
                AutoSize = true,
                Location = new Point(2, 34)
            });

            var bottom = new Panel { Dock = DockStyle.Bottom, Height = 64, BackColor = UiTheme.Background };
            _summaryLabel = new Label
            {
                Dock = DockStyle.Left,
                Width = 420,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = UiTheme.TextMuted
            };

            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                Width = 480,
                FlowDirection = FlowDirection.RightToLeft,
                BackColor = UiTheme.Background,
                Padding = new Padding(0, 14, 0, 0)
            };
            var btnRemove = new FlatButton { Text = "Remove", Width = 110 };
            btnRemove.SetColors(UiTheme.Danger, UiTheme.DangerHover, Color.White);
            btnRemove.Click += (s, e) => Remove();

            var btnEdit = new FlatButton { Text = "Edit", Width = 100 };
            btnEdit.Click += (s, e) => Edit();

            var btnAdd = new FlatButton { Text = "+  Add Club", Width = 140, IsPrimary = true };
            btnAdd.Click += (s, e) => Add();

            buttons.Controls.Add(btnRemove);
            buttons.Controls.Add(btnEdit);
            buttons.Controls.Add(btnAdd);

            bottom.Controls.Add(buttons);
            bottom.Controls.Add(_summaryLabel);

            var card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UiTheme.Surface,
                Padding = new Padding(1)
            };

            _grid = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = false,
                BackColor = UiTheme.SurfaceAlt,
                ForeColor = UiTheme.TextPrimary,
                Font = UiTheme.Body,
                BorderStyle = BorderStyle.None,
                HeaderStyle = ColumnHeaderStyle.Nonclickable,
                MultiSelect = false
            };
            _grid.Columns.Add("Short", 90);
            _grid.Columns.Add("Full Name", 220);
            _grid.Columns.Add("City", 130);
            _grid.Columns.Add("Fencers", 80);
            _grid.Columns.Add("Total Pts", 90);
            _grid.Columns.Add("Avg Pts", 90);
            _grid.Columns.Add("Top Fencer", 200);
            _grid.Columns.Add("Tournaments", 100);
            _grid.DoubleClick += (s, e) => Edit();

            card.Controls.Add(_grid);

            var spacer = new Panel { Dock = DockStyle.Bottom, Height = 8, BackColor = UiTheme.Background };

            Controls.Add(card);
            Controls.Add(spacer);
            Controls.Add(bottom);
            Controls.Add(titleRow);
        }

        public new void Refresh()
        {
            var league = _getLeague();
            var stats = ClubStatsService.Compute(ClubRegistry.All, league)
                .OrderByDescending(s => s.AveragePoints)
                .ThenByDescending(s => s.TotalPoints)
                .ToList();

            _grid.BeginUpdate();
            _grid.Items.Clear();

            foreach (var s in stats)
            {
                var shortName = ClubRegistry.GetShortName(s.ClubName);
                var item = new ListViewItem(shortName) { Tag = s.ClubName };
                item.SubItems.Add(s.ClubName);
                item.SubItems.Add(s.City);
                item.SubItems.Add(s.FencerCount.ToString());
                item.SubItems.Add(s.TotalPoints.ToString());
                item.SubItems.Add(s.AveragePoints.ToString("0.0"));
                item.SubItems.Add(string.IsNullOrEmpty(s.BestFencerName)
                    ? "—"
                    : $"{s.BestFencerName} ({s.BestFencerPoints})");
                item.SubItems.Add(s.TournamentsParticipated.ToString());
                if (s.TotalPoints > 0)
                {
                    item.UseItemStyleForSubItems = false;
                    item.SubItems[4].ForeColor = UiTheme.Accent;
                    item.SubItems[5].ForeColor = UiTheme.Accent;
                }
                _grid.Items.Add(item);
            }
            _grid.EndUpdate();

            var bestClub = stats.FirstOrDefault();
            _summaryLabel.Text =
                $"{ClubRegistry.All.Count} clubs   •   " +
                (bestClub == null || bestClub.FencerCount == 0
                    ? "No data yet"
                    : $"Best-performing: {ClubRegistry.GetShortName(bestClub.ClubName)} ({bestClub.AveragePoints:0.0} avg pts)");
        }

        private void Add()
        {
            using var dlg = new ClubInputDialog();
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            if (ClubRegistry.Exists(dlg.Result.Name))
            {
                MessageBox.Show("A club with this name already exists.");
                return;
            }
            ClubRegistry.AddIfMissing(dlg.Result.Name, dlg.Result.ShortName, dlg.Result.City);
            ClubRegistry.Save();
            _onChanged();
        }

        private void Edit()
        {
            if (_grid.SelectedItems.Count == 0) return;

            var oldName = (string)_grid.SelectedItems[0].Tag!;
            var club = ClubRegistry.Find(oldName);
            if (club == null) return;

            using var dlg = new ClubInputDialog(club);
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            if (!dlg.Result.Name.Equals(oldName, StringComparison.OrdinalIgnoreCase) &&
                ClubRegistry.Exists(dlg.Result.Name))
            {
                MessageBox.Show("Another club with this name already exists.");
                return;
            }

            club.Name = dlg.Result.Name;
            club.ShortName = dlg.Result.ShortName;
            club.City = dlg.Result.City;

            if (!oldName.Equals(club.Name, StringComparison.Ordinal))
            {
                var league = _getLeague();
                foreach (var f in league.Fencers)
                    if (f.ClubName.Equals(oldName, StringComparison.OrdinalIgnoreCase))
                        f.ClubName = club.Name;
            }

            ClubRegistry.Save();
            _onChanged();
        }

        private void Remove()
        {
            if (_grid.SelectedItems.Count == 0) return;
            var name = (string)_grid.SelectedItems[0].Tag!;
            var league = _getLeague();

            int usedBy = league.Fencers.Count(f => f.ClubName.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (usedBy > 0)
            {
                MessageBox.Show($"Cannot remove '{name}' — it is still assigned to {usedBy} fencer(s).");
                return;
            }

            if (MessageBox.Show($"Remove club '{name}'?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                ClubRegistry.Remove(name);
                ClubRegistry.Save();
                _onChanged();
            }
        }
    }
}