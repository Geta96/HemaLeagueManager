using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using HemaLeagueManager.Models;
using HemaLeagueManager.Services;

namespace HemaLeagueManager.Forms
{
    public class TournamentsPage : UserControl
    {
        private readonly Func<League> _getLeague;
        private readonly Action _onChanged;

        private ListBox _list = null!;
        private ListView _detailView = null!;
        private Label _detailHeader = null!;
        private Label _detailSub = null!;

        public TournamentsPage(Func<League> getLeague, Action onChanged)
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
                Text = "Tournaments",
                Font = UiTheme.TitleMedium,
                ForeColor = UiTheme.Accent,
                AutoSize = true,
                Location = new Point(0, 10)
            });
            titleRow.Controls.Add(new Label
            {
                Text = "All tournaments contributing to the league standings",
                Font = UiTheme.Small,
                ForeColor = UiTheme.TextMuted,
                AutoSize = true,
                Location = new Point(2, 34)
            });

            // Split master/detail — DO NOT set SplitterDistance/MinSizes here:
            // the control has no width yet, which causes InvalidOperationException.
            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterWidth = 8,
                BackColor = UiTheme.Background,
                BorderStyle = BorderStyle.None
            };

            split.Panel1.Controls.Add(BuildMasterPanel());
            split.Panel2.Controls.Add(BuildDetailPanel());

            Controls.Add(split);
            Controls.Add(titleRow);

            // Apply sizes once the splitter actually has a width.
            split.HandleCreated += (s, e) => ApplySplitterSizes(split);
            split.SizeChanged  += (s, e) => ApplySplitterSizes(split);
        }

        private static void ApplySplitterSizes(SplitContainer split)
        {
            if (split.Width <= 0) return;

            // Reset mins so we can safely set the distance, then re-apply them.
            split.Panel1MinSize = 0;
            split.Panel2MinSize = 0;

            int desired = 320;
            int max = System.Math.Max(0, split.Width - split.SplitterWidth - 1);
            split.SplitterDistance = System.Math.Min(desired, max);

            // Re-apply mins only if there's enough room for them.
            if (split.Width >= 280 + 360 + split.SplitterWidth)
            {
                split.Panel1MinSize = 280;
                split.Panel2MinSize = 360;
            }
        }

        private Panel BuildMasterPanel()
        {
            var card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UiTheme.Surface,
                Padding = new Padding(12)
            };

            var label = new Label
            {
                Text = "All tournaments",
                Dock = DockStyle.Top,
                Height = 24,
                Font = UiTheme.Subtitle,
                ForeColor = UiTheme.TextMuted
            };

            _list = new ListBox
            {
                Dock = DockStyle.Fill,
                BackColor = UiTheme.SurfaceAlt,
                ForeColor = UiTheme.TextPrimary,
                Font = UiTheme.Body,
                BorderStyle = BorderStyle.None,
                IntegralHeight = false,
                ItemHeight = 28
            };
            _list.SelectedIndexChanged += (s, e) => ShowDetail();
            _list.DoubleClick += (s, e) => Edit();

            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 52,
                Padding = new Padding(0, 10, 0, 0),
                BackColor = UiTheme.Surface
            };
            var btnAdd = new FlatButton { Text = "+  Add", Width = 90, IsPrimary = true };
            btnAdd.Click += (s, e) => Add();
            var btnEdit = new FlatButton { Text = "Edit", Width = 80 };
            btnEdit.Click += (s, e) => Edit();
            var btnRemove = new FlatButton { Text = "Remove", Width = 95 };
            btnRemove.SetColors(UiTheme.Danger, UiTheme.DangerHover, Color.White);
            btnRemove.Click += (s, e) => Remove();

            buttons.Controls.Add(btnAdd);
            buttons.Controls.Add(btnEdit);
            buttons.Controls.Add(btnRemove);

            card.Controls.Add(_list);
            card.Controls.Add(buttons);
            card.Controls.Add(label);
            return card;
        }

        private Panel BuildDetailPanel()
        {
            var card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UiTheme.Surface,
                Padding = new Padding(16)
            };

            _detailHeader = new Label
            {
                Dock = DockStyle.Top,
                Height = 30,
                Font = UiTheme.TitleMedium,
                ForeColor = UiTheme.Accent,
                Text = "Select a tournament"
            };
            _detailSub = new Label
            {
                Dock = DockStyle.Top,
                Height = 22,
                Font = UiTheme.Small,
                ForeColor = UiTheme.TextMuted,
                Text = " "
            };

            _detailView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = false,
                BackColor = UiTheme.SurfaceAlt,
                ForeColor = UiTheme.TextPrimary,
                Font = UiTheme.Body,
                BorderStyle = BorderStyle.None,
                HeaderStyle = ColumnHeaderStyle.Nonclickable
            };
            _detailView.Columns.Add("Place", 80);
            _detailView.Columns.Add("Fencer", 240);
            _detailView.Columns.Add("Club", 200);
            _detailView.Columns.Add("Points", 90);

            var spacer = new Panel { Dock = DockStyle.Top, Height = 8, BackColor = UiTheme.Surface };

            card.Controls.Add(_detailView);
            card.Controls.Add(spacer);
            card.Controls.Add(_detailSub);
            card.Controls.Add(_detailHeader);
            return card;
        }

        public new void Refresh()
        {
            var league = _getLeague();
            var selected = _list.SelectedItem as Tournament;

            _list.BeginUpdate();
            _list.Items.Clear();
            foreach (var t in league.Tournaments.OrderBy(t => t.Date))
                _list.Items.Add(t);
            _list.EndUpdate();

            if (selected != null)
            {
                int idx = _list.Items.IndexOf(selected);
                if (idx >= 0) _list.SelectedIndex = idx;
            }
            else if (_list.Items.Count > 0)
            {
                _list.SelectedIndex = 0;
            }
            else
            {
                ShowDetail();
            }
        }

        private void ShowDetail()
        {
            _detailView.Items.Clear();

            if (_list.SelectedItem is not Tournament t)
            {
                _detailHeader.Text = "Select a tournament";
                _detailSub.Text = "Add or pick a tournament from the list to view placements.";
                return;
            }

            _detailHeader.Text = t.Name;
            _detailSub.Text = $"{t.Date:dddd, MMMM d, yyyy}   •   {t.Placements.Count} placements";

            var league = _getLeague();
            for (int i = 0; i < t.Placements.Count; i++)
            {
                var name = t.Placements[i];
                var fencer = league.Fencers.FirstOrDefault(f => f.Name == name);
                var pts = ScoringSystem.GetPointsForPlacement(i);

                var item = new ListViewItem(Ordinal(i + 1));
                item.SubItems.Add(name);
                item.SubItems.Add(fencer?.ClubName ?? "");
                item.SubItems.Add(pts.ToString());
                if (i < 3) item.ForeColor = UiTheme.Accent;
                _detailView.Items.Add(item);
            }
        }

        private static string Ordinal(int n) => n switch
        {
            1 => "🥇  1st",
            2 => "🥈  2nd",
            3 => "🥉  3rd",
            _ => "      " + n + "th"
        };

        private void Add()
        {
            var league = _getLeague();
            if (league.IsClosed) { MessageBox.Show("League is closed."); return; }
            if (league.Fencers.Count == 0) { MessageBox.Show("Add fencers first."); return; }

            using var dlg = new TournamentForm(league.Fencers);
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                league.Tournaments.Add(dlg.Tournament);
                _onChanged();
            }
        }

        private void Edit()
        {
            if (_list.SelectedItem is not Tournament t) return;
            var league = _getLeague();
            using var dlg = new TournamentForm(league.Fencers, t);
            if (dlg.ShowDialog(this) == DialogResult.OK) _onChanged();
        }

        private void Remove()
        {
            if (_list.SelectedItem is not Tournament t) return;
            var league = _getLeague();
            if (league.IsClosed) { MessageBox.Show("League is closed."); return; }

            if (MessageBox.Show($"Remove tournament '{t.Name}'?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                league.Tournaments.Remove(t);
                _onChanged();
            }
        }
    }
}