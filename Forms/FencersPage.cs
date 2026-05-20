using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using HemaLeagueManager.Models;
using HemaLeagueManager.Services;

namespace HemaLeagueManager.Forms
{
    public class FencersPage : UserControl
    {
        private readonly Func<League> _getLeague;
        private readonly Action _onChanged;

        private ListView _grid = null!;
        private Label _countLabel = null!;

        public FencersPage(Func<League> getLeague, Action onChanged)
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

            // Page title row
            var titleRow = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = UiTheme.Background };
            var title = new Label
            {
                Text = "Fencers",
                Font = UiTheme.TitleMedium,
                ForeColor = UiTheme.Accent,
                AutoSize = true,
                Location = new Point(0, 10)
            };
            var subtitle = new Label
            {
                Text = "Roster of competitors registered for this league",
                Font = UiTheme.Small,
                ForeColor = UiTheme.TextMuted,
                AutoSize = true,
                Location = new Point(2, 34)
            };
            titleRow.Controls.Add(subtitle);
            titleRow.Controls.Add(title);

            // Bottom toolbar
            var bottom = new Panel { Dock = DockStyle.Bottom, Height = 64, BackColor = UiTheme.Background };
            _countLabel = new Label
            {
                Dock = DockStyle.Left,
                Width = 360,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = UiTheme.TextMuted,
                Font = UiTheme.Body
            };

            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                Width = 360,
                FlowDirection = FlowDirection.RightToLeft,
                BackColor = UiTheme.Background,
                Padding = new Padding(0, 14, 0, 0)
            };
            var btnRemove = new FlatButton { Text = "Remove", Width = 110 };
            btnRemove.SetColors(UiTheme.Danger, UiTheme.DangerHover, Color.White);
            btnRemove.Click += (s, e) => Remove();

            var btnAdd = new FlatButton { Text = "+  Add Fencer", Width = 150, IsPrimary = true };
            btnAdd.Click += (s, e) => Add();

            buttons.Controls.Add(btnRemove);
            buttons.Controls.Add(btnAdd);

            bottom.Controls.Add(buttons);
            bottom.Controls.Add(_countLabel);

            // Card-style container for the grid
            var card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UiTheme.Surface,
                Padding = new Padding(1),
                Margin = new Padding(0, 8, 0, 8)
            };

            _grid = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = false,
                MultiSelect = false,
                BackColor = UiTheme.SurfaceAlt,
                ForeColor = UiTheme.TextPrimary,
                Font = UiTheme.Body,
                BorderStyle = BorderStyle.None,
                HeaderStyle = ColumnHeaderStyle.Nonclickable
            };
            _grid.Columns.Add("Name", 260);
            _grid.Columns.Add("Sex", 90);
            _grid.Columns.Add("Club", 260);
            _grid.Columns.Add("Tournaments", 120);
            _grid.Columns.Add("Total Points", 120);

            card.Controls.Add(_grid);

            // Spacer between card and toolbar
            var spacer = new Panel { Dock = DockStyle.Bottom, Height = 8, BackColor = UiTheme.Background };

            Controls.Add(card);
            Controls.Add(spacer);
            Controls.Add(bottom);
            Controls.Add(titleRow);
        }

        public new void Refresh()
        {
            var league = _getLeague();

            _grid.BeginUpdate();
            _grid.Items.Clear();

            var rows = league.Fencers
                .Select(f => new
                {
                    Fencer = f,
                    Tournaments = league.Tournaments.Count(t => t.Placements.Contains(f.Name)),
                    Points = ScoringSystem.GetTotalPointsForFencer(league, f.Name)
                })
                .OrderByDescending(r => r.Points)
                .ThenBy(r => r.Fencer.Name);

            foreach (var r in rows)
            {
                var item = new ListViewItem(r.Fencer.Name) { Tag = r.Fencer };
                item.SubItems.Add(r.Fencer.Sex);
                item.SubItems.Add(r.Fencer.ClubName);
                item.SubItems.Add(r.Tournaments.ToString());
                item.SubItems.Add(r.Points.ToString());
                if (r.Points > 0) item.UseItemStyleForSubItems = false;
                if (r.Points > 0) item.SubItems[4].ForeColor = UiTheme.Accent;
                _grid.Items.Add(item);
            }
            _grid.EndUpdate();

            _countLabel.Text =
                $"{league.Fencers.Count} fencers   •   {league.Tournaments.Count} tournaments" +
                (league.IsClosed ? "   •   League closed" : "");
        }

        private void Add()
        {
            var league = _getLeague();
            if (league.IsClosed) { MessageBox.Show("League is closed."); return; }

            using var dlg = new FencerInputDialog();
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                if (league.Fencers.Any(x => x.Name.Equals(dlg.Result.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show("A fencer with this name already exists.");
                    return;
                }
                league.Fencers.Add(dlg.Result);
                _onChanged();
            }
        }

        private void Remove()
        {
            if (_grid.SelectedItems.Count == 0) return;
            var league = _getLeague();
            if (league.IsClosed) { MessageBox.Show("League is closed."); return; }

            var f = (Fencer)_grid.SelectedItems[0].Tag!;
            if (MessageBox.Show($"Remove '{f.Name}'?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                league.Fencers.Remove(f);
                _onChanged();
            }
        }
    }
}