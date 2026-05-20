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
        private const string AllClubs = "All clubs";

        private readonly Func<League> _getLeague;
        private readonly Action _onChanged;

        private ListView _grid = null!;
        private Label _countLabel = null!;
        private ComboBox _clubFilter = null!;

        private bool _suppressFilterEvent;

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

            // Title row
            var titleRow = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = UiTheme.Background };
            titleRow.Controls.Add(new Label
            {
                Text = "Fencers",
                Font = UiTheme.TitleMedium,
                ForeColor = UiTheme.Accent,
                AutoSize = true,
                Location = new Point(0, 10)
            });
            titleRow.Controls.Add(new Label
            {
                Text = "Roster of competitors registered for this league",
                Font = UiTheme.Small,
                ForeColor = UiTheme.TextMuted,
                AutoSize = true,
                Location = new Point(2, 34)
            });

            // Filter bar
            var filterBar = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = UiTheme.Background };
            filterBar.Controls.Add(new Label
            {
                Text = "Filter by club:",
                AutoSize = true,
                ForeColor = UiTheme.TextMuted,
                Location = new Point(2, 14)
            });
            _clubFilter = new ComboBox
            {
                Left = 110, Top = 10, Width = 260,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = UiTheme.Body
            };
            _clubFilter.SelectedIndexChanged += OnClubFilterChanged;
            filterBar.Controls.Add(_clubFilter);

            // Bottom toolbar
            var bottom = new Panel { Dock = DockStyle.Bottom, Height = 64, BackColor = UiTheme.Background };
            _countLabel = new Label
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

            var btnAdd = new FlatButton { Text = "+  Add Fencer", Width = 150, IsPrimary = true };
            btnAdd.Click += (s, e) => Add();

            buttons.Controls.Add(btnRemove);
            buttons.Controls.Add(btnEdit);
            buttons.Controls.Add(btnAdd);

            bottom.Controls.Add(buttons);
            bottom.Controls.Add(_countLabel);

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
                MultiSelect = false,
                BackColor = UiTheme.SurfaceAlt,
                ForeColor = UiTheme.TextPrimary,
                Font = UiTheme.Body,
                BorderStyle = BorderStyle.None,
                HeaderStyle = ColumnHeaderStyle.Nonclickable
            };
            _grid.Columns.Add("Name", 220);
            _grid.Columns.Add("Sex", 70);
            _grid.Columns.Add("Club", 100);
            _grid.Columns.Add("Tournaments", 100);
            _grid.Columns.Add("Total Points", 100);
            _grid.DoubleClick += (s, e) => Edit();

            card.Controls.Add(_grid);

            var spacer = new Panel { Dock = DockStyle.Bottom, Height = 8, BackColor = UiTheme.Background };

            Controls.Add(card);
            Controls.Add(spacer);
            Controls.Add(bottom);
            Controls.Add(filterBar);
            Controls.Add(titleRow);
        }

        private void OnClubFilterChanged(object? sender, EventArgs e)
        {
            if (_suppressFilterEvent) return;
            RenderGrid();
        }

        public new void Refresh()
        {
            _suppressFilterEvent = true;
            try
            {
                var prevFilter = _clubFilter.SelectedItem?.ToString() ?? AllClubs;
                _clubFilter.BeginUpdate();
                _clubFilter.Items.Clear();
                _clubFilter.Items.Add(AllClubs);
                // Show short names in the filter dropdown; map back to full name on use.
                foreach (var c in ClubRegistry.All.OrderBy(c => c.Name))
                    _clubFilter.Items.Add(ClubRegistry.GetShortName(c.Name));
                _clubFilter.SelectedItem = _clubFilter.Items.Contains(prevFilter) ? prevFilter : AllClubs;
                _clubFilter.EndUpdate();
            }
            finally { _suppressFilterEvent = false; }

            RenderGrid();
        }

        private void RenderGrid()
        {
            var league = _getLeague();
            var filterShort = _clubFilter.SelectedItem?.ToString() ?? AllClubs;

            // Translate the short-name filter back to a full club name.
            string? filterFull = null;
            if (filterShort != AllClubs)
            {
                var match = ClubRegistry.All.FirstOrDefault(c =>
                    ClubRegistry.GetShortName(c.Name).Equals(filterShort, StringComparison.OrdinalIgnoreCase));
                filterFull = match?.Name ?? filterShort;
            }

            _grid.BeginUpdate();
            _grid.Items.Clear();

            var query = league.Fencers.AsEnumerable();
            if (filterFull != null)
                query = query.Where(f => f.ClubName.Equals(filterFull, StringComparison.OrdinalIgnoreCase));

            var rows = query
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
                item.SubItems.Add(ClubRegistry.GetShortName(r.Fencer.ClubName));
                item.SubItems.Add(r.Tournaments.ToString());
                item.SubItems.Add(r.Points.ToString());
                if (r.Points > 0)
                {
                    item.UseItemStyleForSubItems = false;
                    item.SubItems[4].ForeColor = UiTheme.Accent;
                }
                _grid.Items.Add(item);
            }
            _grid.EndUpdate();

            int total = league.Fencers.Count;
            int shown = _grid.Items.Count;
            _countLabel.Text = filterFull == null
                ? $"{total} fencers   •   {league.Tournaments.Count} tournaments" +
                  (league.IsClosed ? "   •   League closed" : "")
                : $"Showing {shown} of {total} fencers in club '{filterShort}'";
        }

        private void Add()
        {
            var league = _getLeague();
            if (league.IsClosed) { MessageBox.Show("League is closed."); return; }

            if (ClubRegistry.All.Count == 0)
            {
                MessageBox.Show("Create at least one club first (Clubs tab).");
                return;
            }

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

        private void Edit()
        {
            if (_grid.SelectedItems.Count == 0) return;
            var league = _getLeague();
            if (league.IsClosed) { MessageBox.Show("League is closed."); return; }

            var fencer = (Fencer)_grid.SelectedItems[0].Tag!;
            var originalName = fencer.Name;

            using var dlg = new FencerInputDialog(fencer);
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            // Name conflict check (case-insensitive, allowing the same fencer's own name).
            if (!dlg.Result.Name.Equals(originalName, StringComparison.OrdinalIgnoreCase) &&
                league.Fencers.Any(x => x.Name.Equals(dlg.Result.Name, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("Another fencer with this name already exists.");
                return;
            }

            // Apply edits in place so all references (placements) keep working
            // once we propagate the rename below.
            fencer.Name = dlg.Result.Name;
            fencer.Sex = dlg.Result.Sex;
            fencer.ClubName = dlg.Result.ClubName;

            // Propagate rename into every tournament's placements.
            if (!originalName.Equals(fencer.Name, StringComparison.Ordinal))
            {
                foreach (var t in league.Tournaments)
                {
                    for (int i = 0; i < t.Placements.Count; i++)
                        if (t.Placements[i].Equals(originalName, StringComparison.OrdinalIgnoreCase))
                            t.Placements[i] = fencer.Name;
                }
            }

            _onChanged();
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