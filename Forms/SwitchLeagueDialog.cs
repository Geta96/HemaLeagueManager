using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using HemaLeagueManager.Models;

namespace HemaLeagueManager.Forms
{
    public class SwitchLeagueDialog : Form
    {
        // Live reference to the project's league list — additions/removals
        // performed here are reflected in the caller automatically.
        private readonly List<League> _leagues;
        private ListBox _list = null!;
        private FlatButton _btnOk = null!;
        private FlatButton _btnDelete = null!;

        public League? SelectedLeague { get; private set; }

        public SwitchLeagueDialog(List<League> leagues, string? activeLeagueName)
        {
            _leagues = leagues;

            Text = "Switch League";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MinimizeBox = false;
            MaximizeBox = false;
            ShowInTaskbar = false;
            BackColor = UiTheme.Background;
            ForeColor = UiTheme.TextPrimary;
            Font = UiTheme.Body;
            ClientSize = new Size(560, 460);

            // ---- Header ----
            var titleBar = new Panel
            {
                Dock = DockStyle.Top, Height = 60, BackColor = UiTheme.Header,
                Padding = new Padding(24, 14, 24, 0)
            };
            titleBar.Controls.Add(new Label
            {
                Text = "⚔  Switch League",
                Font = UiTheme.TitleMedium,
                ForeColor = UiTheme.Accent,
                AutoSize = true,
                Location = new Point(0, 10)
            });
            var stripe = new Panel { Dock = DockStyle.Top, Height = 2, BackColor = UiTheme.Accent };

            // ---- Bottom action row ----
            var bottom = new Panel
            {
                Dock = DockStyle.Bottom, Height = 64, BackColor = UiTheme.Header,
                Padding = new Padding(20, 14, 20, 14)
            };

            var btnCancel = new FlatButton { Text = "Cancel", Width = 110, Height = 36, DialogResult = DialogResult.Cancel };
            _btnOk = new FlatButton { Text = "Open", Width = 110, Height = 36, IsPrimary = true, DialogResult = DialogResult.OK };
            _btnOk.Click += (s, e) =>
            {
                if (_list.SelectedItem is League l) SelectedLeague = l;
                else DialogResult = DialogResult.None;
            };

            var btnNew = new FlatButton { Text = "+ New", Width = 100, Height = 36 };
            btnNew.Click += (s, e) => CreateNewLeague();

            _btnDelete = new FlatButton { Text = "Delete", Width = 100, Height = 36 };
            _btnDelete.SetColors(UiTheme.Danger, UiTheme.DangerHover, Color.White);
            _btnDelete.Click += (s, e) => DeleteSelectedLeague();

            // Right-aligned: Open / Cancel
            var rightRow = new FlowLayoutPanel
            {
                Dock = DockStyle.Right, FlowDirection = FlowDirection.RightToLeft,
                Width = 260, BackColor = UiTheme.Header
            };
            rightRow.Controls.Add(_btnOk);
            rightRow.Controls.Add(btnCancel);

            // Left-aligned: + New / Delete
            var leftRow = new FlowLayoutPanel
            {
                Dock = DockStyle.Left, FlowDirection = FlowDirection.LeftToRight,
                Width = 230, BackColor = UiTheme.Header
            };
            leftRow.Controls.Add(btnNew);
            leftRow.Controls.Add(_btnDelete);

            bottom.Controls.Add(rightRow);
            bottom.Controls.Add(leftRow);

            // ---- Body ----
            var body = new Panel
            {
                Dock = DockStyle.Fill, Padding = new Padding(24, 16, 24, 16), BackColor = UiTheme.Background
            };
            var hint = new Label
            {
                Text = "Double-click a league to open it.",
                Dock = DockStyle.Top,
                Height = 20,
                Font = UiTheme.Small,
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
            _list.DoubleClick += (s, e) => { if (_list.SelectedItem is League) _btnOk.PerformClick(); };
            _list.SelectedIndexChanged += (s, e) => UpdateButtonStates();

            body.Controls.Add(_list);
            body.Controls.Add(hint);

            Controls.Add(body);
            Controls.Add(bottom);
            Controls.Add(stripe);
            Controls.Add(titleBar);

            AcceptButton = _btnOk;
            CancelButton = btnCancel;

            ReloadList(activeLeagueName);
            UpdateButtonStates();
        }

        private void ReloadList(string? selectName = null)
        {
            _list.BeginUpdate();
            _list.Items.Clear();
            foreach (var l in _leagues.OrderBy(l => l.Name))
                _list.Items.Add(l);
            _list.EndUpdate();

            if (!string.IsNullOrWhiteSpace(selectName))
            {
                var match = _leagues.FirstOrDefault(l =>
                    l.Name.Equals(selectName, StringComparison.OrdinalIgnoreCase));
                if (match != null) _list.SelectedItem = match;
            }
            if (_list.SelectedIndex < 0 && _list.Items.Count > 0)
                _list.SelectedIndex = 0;
        }

        private void UpdateButtonStates()
        {
            bool any = _list.SelectedItem is League;
            _btnOk.Enabled = any;
            _btnDelete.Enabled = any;
        }

        private void CreateNewLeague()
        {
            using var dlg = new LeagueNameDialog("Season " + DateTime.Now.Year);
            if (dlg.ShowDialog(this) != DialogResult.OK) return;
            if (string.IsNullOrWhiteSpace(dlg.LeagueName)) return;

            if (_leagues.Any(l => l.Name.Equals(dlg.LeagueName, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("A league with this name already exists.");
                return;
            }

            // Fencers list will be re-bound to FencerRegistry.All by MainForm
            // when the dialog closes — leaving it empty here is fine.
            var league = new League
            {
                Name = dlg.LeagueName,
                Gender = dlg.Gender
            };
            _leagues.Add(league);
            ReloadList(selectName: league.Name);
            UpdateButtonStates();
        }

        private void DeleteSelectedLeague()
        {
            if (_list.SelectedItem is not League l) return;

            var yes = ConfirmDialog.Ask(
                this,
                "Delete League",
                $"Permanently delete the league '{l.Name}'?\n\n" +
                $"This removes its {l.Tournaments.Count} tournament(s) from the project. " +
                "Fencers and clubs are kept.",
                yesText: "Delete",
                noText: "Cancel",
                primaryYes: false);
            if (!yes) return;

            _leagues.Remove(l);
            ReloadList();
            UpdateButtonStates();
        }
    }
}