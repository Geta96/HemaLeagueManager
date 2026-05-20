using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using HemaLeagueManager.Models;
using HemaLeagueManager.Services;

namespace HemaLeagueManager.Forms
{
    public class MainForm : Form
    {
        private Project _project = new Project();
        private League _league = new League();   // currently active league inside _project

        private TabControl _tabs = null!;
        private FencersPage _fencersPage = null!;
        private TournamentsPage _tournamentsPage = null!;
        private ClubsPage _clubsPage = null!;
        private ListView _standingsView = null!;
        private Label _titleLabel = null!;
        private Label _subtitleLabel = null!;
        private Panel _accentStripe = null!;

        private Panel _sidebar = null!;
        private Panel _sidebarOverlay = null!;
        private Label _autosaveStatus = null!;
        private const int SidebarWidth = 280;

        private readonly List<FlatButton> _navButtons = new();

        public MainForm()
        {
            BuildUi();
            LoadAutosaveIfAny();
            BindActiveLeagueFencers();
            RefreshAll();
        }

        // --- Wire the active league's Fencers list to the shared registry. ---
        private void BindActiveLeagueFencers()
        {
            _league.Fencers = FencerRegistry.All;
        }

        private void BuildUi()
        {
            Text = "HEMA League Manager";
            MinimumSize = new Size(1100, 720);
            Size = new Size(1280, 780);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = UiTheme.Background;
            ForeColor = UiTheme.TextPrimary;
            Font = UiTheme.Body;
            DoubleBuffered = true;

            _tabs = new TabControl
            {
                Dock = DockStyle.Fill,
                Appearance = TabAppearance.FlatButtons,
                ItemSize = new Size(0, 1),
                SizeMode = TabSizeMode.Fixed,
                Multiline = true
            };

            _fencersPage = new FencersPage(() => _league, OnDataChanged);
            _tournamentsPage = new TournamentsPage(() => _league, OnDataChanged, EnsureLeagueOrCreate);
            _clubsPage = new ClubsPage(() => _league, OnDataChanged);

            var fencersTab     = new TabPage("Fencers")     { BackColor = UiTheme.Background }; fencersTab.Controls.Add(_fencersPage);
            var clubsTab       = new TabPage("Clubs")       { BackColor = UiTheme.Background }; clubsTab.Controls.Add(_clubsPage);
            var tournamentsTab = new TabPage("Tournaments") { BackColor = UiTheme.Background }; tournamentsTab.Controls.Add(_tournamentsPage);

            _tabs.TabPages.Add(fencersTab);
            _tabs.TabPages.Add(clubsTab);
            _tabs.TabPages.Add(tournamentsTab);
            _tabs.TabPages.Add(BuildStandingsTab());
            _tabs.SelectedIndexChanged += (s, e) => { SyncNavSelection(); RefreshStandings(); };

            var header = BuildHeader();
            _accentStripe = new Panel { Dock = DockStyle.Top, Height = 2, BackColor = UiTheme.Accent };

            _sidebarOverlay = BuildSidebarOverlay();
            _sidebar = BuildSidebar();

            Controls.Add(_tabs);
            Controls.Add(_accentStripe);
            Controls.Add(header);
            Controls.Add(_sidebarOverlay);
            Controls.Add(_sidebar);
            _sidebarOverlay.BringToFront();
            _sidebar.BringToFront();

            Resize += (s, e) => LayoutSidebar();
            LayoutSidebar();
            SyncNavSelection();
        }

        // ---- Header with hamburger toggle ----
        private Panel BuildHeader()
        {
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 116,
                BackColor = UiTheme.Header,
                Padding = new Padding(20, 14, 28, 0)
            };

            var titleRow = new Panel { Dock = DockStyle.Top, Height = 46, BackColor = UiTheme.Header };

            // Hamburger — use Segoe UI Symbol which reliably ships the ☰ glyph,
            // and force a strong foreground colour so it's never invisible.
            var menuBtn = new FlatButton
            {
                Text = "☰",
                Width = 48,
                Height = 40,
                Font = new Font("Segoe UI Symbol", 18F, FontStyle.Bold),
                Location = new Point(0, 2),
                Margin = new Padding(0),
                TextAlign = ContentAlignment.MiddleCenter
            };
            menuBtn.SetColors(UiTheme.ButtonIdle, UiTheme.ButtonHover, UiTheme.Accent);
            menuBtn.UseCompatibleTextRendering = true;
            menuBtn.Click += (s, e) => ToggleSidebar();

            _titleLabel = new Label
            {
                AutoSize = true,
                Text = "⚔  HEMA League Manager",
                Font = UiTheme.TitleLarge,
                ForeColor = UiTheme.Accent,
                Location = new Point(60, 4)
            };

            _subtitleLabel = new Label
            {
                AutoSize = true,
                Text = "No league loaded",
                Font = UiTheme.Subtitle,
                ForeColor = UiTheme.TextMuted
            };
            titleRow.Resize += (s, e) => PositionSubtitle(titleRow);

            titleRow.Controls.Add(menuBtn);
            titleRow.Controls.Add(_titleLabel);
            titleRow.Controls.Add(_subtitleLabel);

            var navRow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = UiTheme.Header,
                Padding = new Padding(0, 6, 0, 6)
            };
            navRow.Controls.Add(CreateNavButton("Fencers", 0));
            navRow.Controls.Add(CreateNavButton("Clubs", 1));
            navRow.Controls.Add(CreateNavButton("Tournaments", 2));
            navRow.Controls.Add(CreateNavButton("Standings", 3));

            header.Controls.Add(navRow);
            header.Controls.Add(titleRow);
            return header;
        }

        private void PositionSubtitle(Panel titleRow)
        {
            _subtitleLabel.Location = new Point(
                titleRow.ClientSize.Width - _subtitleLabel.Width, 14);
        }

        private FlatButton CreateNavButton(string text, int tabIndex)
        {
            var btn = new FlatButton
            {
                Text = text,
                Tag = tabIndex,
                Width = 150,
                Height = 40,
                Font = UiTheme.BodyBold,
                Margin = new Padding(0, 0, 6, 0)
            };
            btn.Click += (s, e) => _tabs.SelectedIndex = tabIndex;
            _navButtons.Add(btn);
            return btn;
        }

        private void SyncNavSelection()
        {
            foreach (var btn in _navButtons)
            {
                bool active = (int)btn.Tag! == _tabs.SelectedIndex;
                if (active)
                    btn.SetColors(UiTheme.ButtonActive, UiTheme.ButtonActive, UiTheme.Accent);
                else
                    btn.SetColors(UiTheme.ButtonIdle, UiTheme.ButtonHover, UiTheme.TextPrimary);
            }
        }

        // ---- Sidebar ----
        private Panel BuildSidebar()
        {
            var sb = new Panel
            {
                Width = SidebarWidth,
                BackColor = UiTheme.Header,
                Visible = false,
                Padding = new Padding(18)
            };

            var header = new Label
            {
                Text = "⚙  League Tools",
                Dock = DockStyle.Top,
                Height = 36,
                Font = UiTheme.TitleMedium,
                ForeColor = UiTheme.Accent,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var divider = new Panel
            {
                Dock = DockStyle.Top,
                Height = 1,
                BackColor = UiTheme.Divider,
                Margin = new Padding(0, 4, 0, 4)
            };

            var stack = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                BackColor = UiTheme.Header,
                Padding = new Padding(0, 12, 0, 0)
            };
            stack.Controls.Add(BuildSidebarSectionLabel("LEAGUE"));
            stack.Controls.Add(BuildSidebarButton("+  New League",        (s, e) => { CloseSidebar(); NewLeague(); }, primary: true));
            stack.Controls.Add(BuildSidebarButton("Switch League",        (s, e) => { CloseSidebar(); SwitchLeague(); }));
            stack.Controls.Add(BuildSidebarButton("Close League",         (s, e) => { CloseSidebar(); CloseLeague(); }));
            stack.Controls.Add(BuildSidebarButton("Start Empty Project",  (s, e) => { CloseSidebar(); StartEmptyProject(); }));

            stack.Controls.Add(BuildSidebarSectionLabel("FILE"));
            stack.Controls.Add(BuildSidebarButton("Save As…",          (s, e) => { CloseSidebar(); SaveAs(); }));
            stack.Controls.Add(BuildSidebarButton("Load From File",    (s, e) => { CloseSidebar(); LoadFromFile(); }));
            stack.Controls.Add(BuildSidebarButton("Export PDF Report", (s, e) => { CloseSidebar(); ExportPdf(); }));

            stack.Controls.Add(BuildSidebarSectionLabel("ABOUT"));
            stack.Controls.Add(BuildSidebarButton("Open Data Folder",  (s, e) =>
            {
                CloseSidebar();
                try { System.Diagnostics.Process.Start("explorer.exe", LeagueLibrary.RootFolder); }
                catch { }
            }));

            _autosaveStatus = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                Text = "Autosave: idle",
                Font = UiTheme.Small,
                ForeColor = UiTheme.TextMuted,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(0, 0, 0, 4)
            };

            sb.Controls.Add(stack);
            sb.Controls.Add(divider);
            sb.Controls.Add(header);
            sb.Controls.Add(_autosaveStatus);
            return sb;
        }

        private Panel BuildSidebarOverlay()
        {
            var p = new Panel { BackColor = Color.Black, Visible = false };
            p.Click += (s, e) => CloseSidebar();
            return p;
        }

        private Label BuildSidebarSectionLabel(string text) => new()
        {
            Text = text,
            AutoSize = false,
            Width = SidebarWidth - 36,
            Height = 28,
            Font = UiTheme.Small,
            ForeColor = UiTheme.TextMuted,
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(0, 10, 0, 4)
        };

        private FlatButton BuildSidebarButton(string text, EventHandler onClick, bool primary = false)
        {
            var btn = new FlatButton
            {
                Text = "   " + text,
                Width = SidebarWidth - 36,
                Height = 40,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 10, 0),
                Margin = new Padding(0, 2, 0, 2),
                IsPrimary = primary
            };
            btn.Click += onClick;
            return btn;
        }

        private void LayoutSidebar()
        {
            if (_sidebar == null || _sidebarOverlay == null) return;
            _sidebar.Height = ClientSize.Height;
            _sidebar.Location = new Point(_sidebar.Visible ? 0 : -SidebarWidth, 0);
            _sidebarOverlay.Bounds = new Rectangle(_sidebar.Width, 0,
                ClientSize.Width - _sidebar.Width, ClientSize.Height);
        }

        private void ToggleSidebar()
        {
            if (_sidebar.Visible) CloseSidebar(); else OpenSidebar();
        }

        private void OpenSidebar()
        {
            _sidebar.Visible = true;
            _sidebarOverlay.Visible = true;
            _sidebar.BringToFront();
            LayoutSidebar();
        }

        private void CloseSidebar()
        {
            _sidebar.Visible = false;
            _sidebarOverlay.Visible = false;
        }

        // ---- Standings Tab ----
        private TabPage BuildStandingsTab()
        {
            var page = new TabPage("Standings") { BackColor = UiTheme.Background };
            var wrapper = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20), BackColor = UiTheme.Background };

            var header = new Label
            {
                Dock = DockStyle.Top,
                Height = 40,
                Text = "League Standings",
                Font = UiTheme.TitleMedium,
                ForeColor = UiTheme.Accent,
                TextAlign = ContentAlignment.MiddleLeft
            };

            _standingsView = new ListView
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
            _standingsView.Columns.Add("Rank", 80);
            _standingsView.Columns.Add("Fencer", 260);
            _standingsView.Columns.Add("Club", 240);
            _standingsView.Columns.Add("Sex", 90);
            _standingsView.Columns.Add("Points", 100);

            wrapper.Controls.Add(_standingsView);
            wrapper.Controls.Add(header);
            page.Controls.Add(wrapper);
            return page;
        }

        // ---- Autosave on every change ----
        private void OnDataChanged()
        {
            AutosaveProject();
            UpdateAutosaveStatus();
            RefreshAll();
        }

        private void AutosaveProject()
        {
            SyncProjectFromMemory();
            try { ProjectStorage.Save(_project, LeagueLibrary.AutosavePath); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex); }
        }

        /// <summary>Pull live registry/active-league state into the Project before saving.</summary>
        private void SyncProjectFromMemory()
        {
            _project.Fencers = FencerRegistry.All.ToList();
            _project.Clubs   = ClubRegistry.All.ToList();
            _project.ActiveLeagueName = _league.Name ?? "";

            // Make sure the active league is in the project's list.
            if (!string.IsNullOrWhiteSpace(_league.Name) && !_project.Leagues.Contains(_league))
                _project.Leagues.Add(_league);
        }

        private void UpdateAutosaveStatus()
        {
            if (_autosaveStatus == null) return;
            _autosaveStatus.Text =
                $"Autosaved at {DateTime.Now:HH:mm:ss}\n" +
                $"Project: {LeagueLibrary.AutosavePath}\n" +
                $"{_project.Leagues.Count} leagues  •  {FencerRegistry.All.Count} fencers";
        }

        private void RefreshAll()
        {
            _subtitleLabel.Text = string.IsNullOrWhiteSpace(_league.Name)
                ? "No league loaded"
                : $"{_league.Name}{(_league.IsClosed ? "  •  Closed" : "")}";
            if (_subtitleLabel.Parent is Panel p) PositionSubtitle(p);

            _fencersPage.Refresh();
            _clubsPage.Refresh();
            _tournamentsPage.Refresh();
            RefreshStandings();
        }

        // ---- League actions ----
        private void NewLeague()
        {
            using var dlg = new LeagueNameDialog("Season " + DateTime.Now.Year);
            if (dlg.ShowDialog(this) != DialogResult.OK) return;
            if (string.IsNullOrWhiteSpace(dlg.LeagueName)) return;

            var league = new League { Name = dlg.LeagueName, Fencers = FencerRegistry.All };
            _project.Leagues.Add(league);
            _league = league;
            _project.ActiveLeagueName = league.Name;

            AutosaveProject();
            UpdateAutosaveStatus();
            RefreshAll();
        }

        private void SwitchLeague()
        {
            if (_project.Leagues.Count == 0)
            {
                ConfirmDialog.Ask(this, "Switch League",
                    "There are no leagues yet. Create one first via 'New League'.",
                    yesText: "OK", noText: "Cancel");
                return;
            }

            using var dlg = new SwitchLeagueDialog(_project.Leagues, _project.ActiveLeagueName);
            if (dlg.ShowDialog(this) == DialogResult.OK && dlg.SelectedLeague != null)
            {
                _league = dlg.SelectedLeague;
                _league.Fencers = FencerRegistry.All;   // re-bind shared list
                _project.ActiveLeagueName = _league.Name;
                AutosaveProject();
                UpdateAutosaveStatus();
                RefreshAll();
            }
        }

        private void CloseLeague()
        {
            if (string.IsNullOrWhiteSpace(_league.Name)) return;

            var yes = ConfirmDialog.Ask(this, "Close League",
                "Close the current league?\n\nIt will become read-only until you create or load another league.",
                yesText: "Close", noText: "Cancel", primaryYes: false);
            if (!yes) return;

            _league.IsClosed = true;
            AutosaveProject();
            UpdateAutosaveStatus();
            RefreshAll();
        }

        private void SaveAs()
        {
            using var dlg = new SaveFileDialog
            {
                Filter = "HEMA project (*.csv)|*.csv",
                FileName = "HemaProject.csv"
            };
            if (dlg.ShowDialog(this) != DialogResult.OK) return;
            SyncProjectFromMemory();
            ProjectStorage.Save(_project, dlg.FileName);
            MessageBox.Show("Project saved to:\n" + dlg.FileName, "Saved");
        }

        private void LoadFromFile()
        {
            using var dlg = new OpenFileDialog { Filter = "HEMA project (*.csv)|*.csv" };
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                var loaded = ProjectStorage.Load(dlg.FileName);
                ApplyLoadedProject(loaded);
                AutosaveProject();          // mirror into autosave
                UpdateAutosaveStatus();
                RefreshAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load project:\n" + ex.Message, "Error");
            }
        }

        private void ExportPdf()
        {
            if (string.IsNullOrWhiteSpace(_league.Name))
            {
                MessageBox.Show("Create or load a league first.");
                return;
            }

            using var dlg = new SaveFileDialog
            {
                Filter = "PDF document (*.pdf)|*.pdf",
                FileName = $"{_league.Name} - Report.pdf"
            };
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                PdfReportService.Generate(_league, dlg.FileName);
                if (ConfirmDialog.Ask(this, "PDF Exported",
                        $"Saved to:\n{dlg.FileName}\n\nOpen now?",
                        yesText: "Open", noText: "Close"))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = dlg.FileName, UseShellExecute = true
                    });
                }
            }
            catch (Exception ex) { MessageBox.Show("PDF export failed:\n" + ex.Message, "Error"); }
        }

        // ---- Startup ----
        private void LoadAutosaveIfAny()
        {
            if (!LeagueLibrary.AutosaveExists())
            {
                _project = new Project();
                _league = new League();
                return;
            }

            try
            {
                var loaded = ProjectStorage.Load(LeagueLibrary.AutosavePath);
                ApplyLoadedProject(loaded);
                UpdateAutosaveStatus();
            }
            catch
            {
                _project = new Project();
                _league = new League();
            }
        }

        /// <summary>Replace in-memory state with a freshly-loaded project.</summary>
        private void ApplyLoadedProject(Project loaded)
        {
            _project = loaded;
            FencerRegistry.Replace(_project.Fencers);
            ClubRegistry.Replace(_project.Clubs);
            ClubRegistry.EnsureFromFencers(FencerRegistry.All);

            // Every league shares the same fencer list (the registry).
            foreach (var l in _project.Leagues)
                l.Fencers = FencerRegistry.All;

            // Restore the active league, defaulting to the first one if available.
            _league = _project.Leagues.FirstOrDefault(l =>
                l.Name.Equals(_project.ActiveLeagueName, StringComparison.OrdinalIgnoreCase))
                ?? _project.Leagues.FirstOrDefault()
                ?? new League { Fencers = FencerRegistry.All };

            // Keep the project list in sync with the (replaced) registry references.
            _project.Fencers = FencerRegistry.All.ToList();
            _project.Clubs   = ClubRegistry.All.ToList();
        }

        private bool EnsureLeagueOrCreate()
        {
            if (!string.IsNullOrWhiteSpace(_league.Name)) return true;

            var yes = ConfirmDialog.Ask(this, "No league loaded",
                "Tournaments must belong to a league, but no league is loaded.\n\nWould you like to create a new league now?",
                yesText: "Create league", noText: "Cancel");
            if (!yes) return false;

            NewLeague();
            return !string.IsNullOrWhiteSpace(_league.Name);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            AutosaveProject();
            base.OnFormClosing(e);
        }

        private void StartEmptyProject()
        {
            using var dlg = new StartEmptyDialog();
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            _project = new Project();
            _league  = new League { Fencers = FencerRegistry.All };

            if (dlg.Result == StartEmptyDialog.Choice.ClearEverything)
            {
                FencerRegistry.Replace(Array.Empty<Fencer>());
                ClubRegistry.Replace(Array.Empty<Club>());
            }

            LeagueLibrary.DeleteAutosave();
            AutosaveProject();
            UpdateAutosaveStatus();
            RefreshAll();
        }

        private void RefreshStandings()
        {
            _standingsView.Items.Clear();
            var standings = ScoringSystem.CalculateStandings(_league)
                .OrderByDescending(kv => kv.Value)
                .ToList();

            int rank = 1;
            foreach (var (name, pts) in standings)
            {
                var f = _league.Fencers.FirstOrDefault(x => x.Name == name);
                var rankText = rank switch
                {
                    1 => "🥇  1",
                    2 => "🥈  2",
                    3 => "🥉  3",
                    _ => "      " + rank
                };

                var item = new ListViewItem(rankText);
                item.SubItems.Add(name);
                item.SubItems.Add(ClubRegistry.GetShortName(f?.ClubName ?? ""));
                item.SubItems.Add(f?.Sex ?? "");
                item.SubItems.Add(pts.ToString());
                if (rank <= 3) item.ForeColor = UiTheme.Accent;
                _standingsView.Items.Add(item);
                rank++;
            }
        }
    }
}