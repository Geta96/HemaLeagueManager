using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using HemaLeagueManager.Models;
using HemaLeagueManager.Services;

namespace HemaLeagueManager.Forms
{
    public class TournamentForm : Form
    {
        private readonly List<Fencer> _availableFencers;
        public Tournament Tournament { get; private set; }

        private TextBox _nameBox = null!;
        private DateTimePicker _datePicker = null!;
        private CheckBox _grandPrixBox = null!;
        private TextBox _filterBox = null!;
        private ListBox _availableList = null!;
        private ListView _placementView = null!;
        private Label _placementCount = null!;

        public TournamentForm(List<Fencer> fencers, Tournament? existing = null)
        {
            _availableFencers = fencers;
            Tournament = existing ?? new Tournament();
            BuildUi();
            LoadData();
        }

        private void BuildUi()
        {
            Text = string.IsNullOrEmpty(Tournament.Name) ? "New Tournament" : "Edit Tournament";
            Size = new Size(960, 680);
            MinimumSize = new Size(820, 560);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = UiTheme.Background;
            ForeColor = UiTheme.TextPrimary;
            Font = UiTheme.Body;
            DoubleBuffered = true;

            // ---- Top: title bar ----
            var titleBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 64,
                BackColor = UiTheme.Header,
                Padding = new Padding(24, 12, 24, 0)
            };
            titleBar.Controls.Add(new Label
            {
                Text = string.IsNullOrEmpty(Tournament.Name) ? "⚔  New Tournament" : "⚔  Edit Tournament",
                Font = UiTheme.TitleLarge,
                ForeColor = UiTheme.Accent,
                AutoSize = true,
                Location = new Point(0, 8)
            });

            var accentStripe = new Panel { Dock = DockStyle.Top, Height = 2, BackColor = UiTheme.Accent };

            // ---- Bottom: Save / Cancel bar ----
            var bottomBar = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 64,
                BackColor = UiTheme.Header,
                Padding = new Padding(24, 12, 24, 12)
            };
            var btnSave = new FlatButton
            {
                Text = "Save Tournament",
                Width = 180,
                Height = 40,
                IsPrimary = true,
                DialogResult = DialogResult.OK
            };
            btnSave.Click += (s, e) => SaveAndClose();
            var btnCancel = new FlatButton
            {
                Text = "Cancel",
                Width = 100,
                Height = 40,
                DialogResult = DialogResult.Cancel,
                Margin = new Padding(10, 0, 0, 0)
            };

            var btnRow = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                FlowDirection = FlowDirection.RightToLeft,
                Width = 320,
                BackColor = UiTheme.Header
            };
            btnRow.Controls.Add(btnSave);
            btnRow.Controls.Add(btnCancel);
            bottomBar.Controls.Add(btnRow);

            AcceptButton = btnSave;
            CancelButton = btnCancel;

            // ---- Middle: metadata + split work area ----
            var content = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UiTheme.Background,
                Padding = new Padding(20)
            };
            content.Controls.Add(BuildSplitArea());      // fill
            content.Controls.Add(BuildMetadataPanel());  // top

            Controls.Add(content);
            Controls.Add(bottomBar);
            Controls.Add(accentStripe);
            Controls.Add(titleBar);
        }

        private Panel BuildMetadataPanel()
        {
            var card = new Panel
            {
                Dock = DockStyle.Top,
                Height = 110,
                BackColor = UiTheme.Surface,
                Padding = new Padding(20, 14, 20, 14)
            };

            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 2,
                BackColor = UiTheme.Surface
            };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            grid.Controls.Add(MakeLabel("Tournament:"), 0, 0);
            _nameBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = UiTheme.Body,
                BackColor = UiTheme.SurfaceAlt,
                ForeColor = UiTheme.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 6, 14, 6)
            };
            grid.Controls.Add(_nameBox, 1, 0);

            grid.Controls.Add(MakeLabel("Date:"), 2, 0);
            _datePicker = new DateTimePicker
            {
                Dock = DockStyle.Fill,
                Font = UiTheme.Body,
                Format = DateTimePickerFormat.Short,
                CalendarMonthBackground = UiTheme.Surface,
                Margin = new Padding(0, 6, 0, 6)
            };
            grid.Controls.Add(_datePicker, 3, 0);

            _grandPrixBox = new CheckBox
            {
                Text = "★  Grand Prix  —  points awarded in this tournament are doubled",
                AutoSize = true,
                ForeColor = UiTheme.Accent,
                BackColor = UiTheme.Surface,
                Font = UiTheme.BodyBold,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 6, 0, 0)
            };
            _grandPrixBox.CheckedChanged += (s, e) => RenumberPlacements();
            grid.Controls.Add(_grandPrixBox, 1, 1);
            grid.SetColumnSpan(_grandPrixBox, 3);

            card.Controls.Add(grid);
            return card;
        }

        private static Label MakeLabel(string text) => new()
        {
            Text = text,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = UiTheme.TextMuted,
            Font = UiTheme.Subtitle
        };

        private SplitContainer BuildSplitArea()
        {
            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterWidth = 10,
                BackColor = UiTheme.Background,
                BorderStyle = BorderStyle.None
            };
            split.Panel1.Controls.Add(BuildAvailablePanel());
            split.Panel2.Controls.Add(BuildPlacementsPanel());

            split.HandleCreated += (s, e) => ApplySplitterSizes(split);
            split.SizeChanged  += (s, e) => ApplySplitterSizes(split);
            return split;
        }

        private static void ApplySplitterSizes(SplitContainer split)
        {
            if (split.Width <= 0) return;
            split.Panel1MinSize = 0;
            split.Panel2MinSize = 0;
            int desired = split.Width / 2;
            int max = Math.Max(0, split.Width - split.SplitterWidth - 1);
            split.SplitterDistance = Math.Min(desired, max);
            if (split.Width >= 300 + 380 + split.SplitterWidth)
            {
                split.Panel1MinSize = 280;
                split.Panel2MinSize = 360;
            }
        }

        private Panel BuildAvailablePanel()
        {
            var card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UiTheme.Surface,
                Padding = new Padding(16)
            };
            var header = new Label
            {
                Text = "Available fencers",
                Dock = DockStyle.Top,
                Height = 24,
                Font = UiTheme.Subtitle,
                ForeColor = UiTheme.Accent
            };
            var hint = new Label
            {
                Text = "Double-click a fencer to add them to placements",
                Dock = DockStyle.Top,
                Height = 18,
                Font = UiTheme.Small,
                ForeColor = UiTheme.TextMuted
            };
            _filterBox = new TextBox
            {
                Dock = DockStyle.Top,
                Font = UiTheme.Body,
                BackColor = UiTheme.SurfaceAlt,
                ForeColor = UiTheme.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle,
                PlaceholderText = "Filter by name or club…",
                Margin = new Padding(0, 8, 0, 0)
            };
            _filterBox.TextChanged += (s, e) => RefreshAvailable();

            var filterSpacer = new Panel { Dock = DockStyle.Top, Height = 8, BackColor = UiTheme.Surface };

            _availableList = new ListBox
            {
                Dock = DockStyle.Fill,
                BackColor = UiTheme.SurfaceAlt,
                ForeColor = UiTheme.TextPrimary,
                Font = UiTheme.Body,
                BorderStyle = BorderStyle.None,
                IntegralHeight = false,
                ItemHeight = 26
            };
            _availableList.DoubleClick += (s, e) => AddSelected();

            var btnRow = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 44,
                Padding = new Padding(0, 8, 0, 0),
                BackColor = UiTheme.Surface
            };
            var btnAdd = new FlatButton { Text = "Add to placements  →", Width = 200, Height = 34, IsPrimary = true };
            btnAdd.Click += (s, e) => AddSelected();
            btnRow.Controls.Add(btnAdd);

            card.Controls.Add(_availableList);
            card.Controls.Add(filterSpacer);
            card.Controls.Add(_filterBox);
            card.Controls.Add(hint);
            card.Controls.Add(header);
            card.Controls.Add(btnRow);
            return card;
        }

        private Panel BuildPlacementsPanel()
        {
            var card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UiTheme.Surface,
                Padding = new Padding(16)
            };
            var header = new Label
            {
                Text = "Placements",
                Dock = DockStyle.Top,
                Height = 24,
                Font = UiTheme.Subtitle,
                ForeColor = UiTheme.Accent
            };
            var hint = new Label
            {
                Text = "1st on top — points are assigned automatically. Reorder with ▲ / ▼.",
                Dock = DockStyle.Top,
                Height = 18,
                Font = UiTheme.Small,
                ForeColor = UiTheme.TextMuted
            };

            _placementView = new ListView
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
                HideSelection = false
            };
            _placementView.Columns.Add("Place", 80);
            _placementView.Columns.Add("Fencer", 220);
            _placementView.Columns.Add("Club", 160);
            _placementView.Columns.Add("Points", 70);
            _placementView.DoubleClick += (s, e) => RemoveSelected();

            var btnRow = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 44,
                Padding = new Padding(0, 8, 0, 0),
                BackColor = UiTheme.Surface
            };
            var btnUp = new FlatButton { Text = "▲  Up", Width = 80, Height = 34 };
            btnUp.Click += (s, e) => Move(-1);
            var btnDown = new FlatButton { Text = "▼  Down", Width = 90, Height = 34 };
            btnDown.Click += (s, e) => Move(1);
            var btnRemove = new FlatButton { Text = "Remove", Width = 100, Height = 34 };
            btnRemove.SetColors(UiTheme.Danger, UiTheme.DangerHover, Color.White);
            btnRemove.Click += (s, e) => RemoveSelected();

            btnRow.Controls.Add(btnUp);
            btnRow.Controls.Add(btnDown);
            btnRow.Controls.Add(btnRemove);

            _placementCount = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 22,
                Font = UiTheme.Small,
                ForeColor = UiTheme.TextMuted,
                TextAlign = ContentAlignment.MiddleLeft
            };

            card.Controls.Add(_placementView);
            card.Controls.Add(btnRow);
            card.Controls.Add(_placementCount);
            card.Controls.Add(hint);
            card.Controls.Add(header);
            return card;
        }

        // ---- Data + interactions ----

        private void LoadData()
        {
            _nameBox.Text = Tournament.Name;
            _datePicker.Value = Tournament.Date == default ? DateTime.Today : Tournament.Date;
            _grandPrixBox.Checked = Tournament.IsGrandPrix;

            foreach (var name in Tournament.Placements)
            {
                var f = _availableFencers.FirstOrDefault(x => x.Name == name);
                if (f != null) AppendPlacement(f);
            }
            RefreshAvailable();
            RenumberPlacements();
        }

        private void RefreshAvailable()
        {
            var placedNames = _placementView.Items
                .Cast<ListViewItem>()
                .Select(i => ((Fencer)i.Tag!).Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var filter = _filterBox.Text?.Trim() ?? "";

            _availableList.BeginUpdate();
            _availableList.Items.Clear();
            foreach (var f in _availableFencers
                .Where(f => !placedNames.Contains(f.Name))
                .Where(f => string.IsNullOrEmpty(filter)
                            || f.Name.Contains(filter, StringComparison.OrdinalIgnoreCase)
                            || f.ClubName.Contains(filter, StringComparison.OrdinalIgnoreCase))
                .OrderBy(f => f.Name))
            {
                _availableList.Items.Add(f);
            }
            _availableList.EndUpdate();
        }

        private void AddSelected()
        {
            if (_availableList.SelectedItem is not Fencer f) return;
            AppendPlacement(f);
            RefreshAvailable();
            RenumberPlacements();
        }

        private void AppendPlacement(Fencer f)
        {
            var item = new ListViewItem("") { Tag = f };
            item.SubItems.Add(f.Name);
            item.SubItems.Add(ClubRegistry.GetShortName(f.ClubName));
            item.SubItems.Add("");
            _placementView.Items.Add(item);
        }

        private void RemoveSelected()
        {
            if (_placementView.SelectedItems.Count == 0) return;
            foreach (var item in _placementView.SelectedItems.Cast<ListViewItem>().ToList())
                _placementView.Items.Remove(item);
            RefreshAvailable();
            RenumberPlacements();
        }

        private void Move(int delta)
        {
            if (_placementView.SelectedItems.Count == 0) return;
            int i = _placementView.SelectedIndices[0];
            int j = i + delta;
            if (j < 0 || j >= _placementView.Items.Count) return;

            var item = _placementView.Items[i];
            _placementView.Items.RemoveAt(i);
            _placementView.Items.Insert(j, item);
            item.Selected = true;
            item.EnsureVisible();
            RenumberPlacements();
        }

        private void RenumberPlacements()
        {
            bool gp = _grandPrixBox?.Checked ?? false;
            for (int i = 0; i < _placementView.Items.Count; i++)
            {
                var item = _placementView.Items[i];
                item.SubItems[0].Text = Ordinal(i + 1);
                var basePts = ScoringSystem.GetPointsForPlacement(i);
                var pts = gp ? basePts * ScoringSystem.GrandPrixMultiplier : basePts;
                item.SubItems[3].Text = pts.ToString();

                item.UseItemStyleForSubItems = false;
                item.ForeColor = i < 3 ? UiTheme.Accent : UiTheme.TextPrimary;
                item.SubItems[3].ForeColor = pts > 0 ? UiTheme.Accent : UiTheme.TextMuted;
            }
            _placementCount.Text =
                $"{_placementView.Items.Count} placed" +
                (gp ? "   •   Grand Prix (×2 points)" : "");
        }

        private static string Ordinal(int n) => n switch
        {
            1 => "🥇  1st",
            2 => "🥈  2nd",
            3 => "🥉  3rd",
            _ => "      " + n + "th"
        };

        private void SaveAndClose()
        {
            if (string.IsNullOrWhiteSpace(_nameBox.Text))
            {
                MessageBox.Show("Please enter a tournament name.");
                DialogResult = DialogResult.None;
                return;
            }

            Tournament.Name = _nameBox.Text.Trim();
            Tournament.Date = _datePicker.Value.Date;
            Tournament.IsGrandPrix = _grandPrixBox.Checked;
            Tournament.Placements = _placementView.Items
                .Cast<ListViewItem>()
                .Select(i => ((Fencer)i.Tag!).Name)
                .ToList();
        }
    }
}