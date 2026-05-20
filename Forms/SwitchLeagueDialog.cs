using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using HemaLeagueManager.Models;

namespace HemaLeagueManager.Forms
{
    public class SwitchLeagueDialog : Form
    {
        private readonly List<League> _leagues;
        private ListBox _list = null!;

        public League? SelectedLeague { get; private set; }

        public SwitchLeagueDialog(IEnumerable<League> leagues, string? activeLeagueName)
        {
            _leagues = leagues.OrderBy(l => l.Name).ToList();

            Text = "Switch League";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MinimizeBox = false;
            MaximizeBox = false;
            ShowInTaskbar = false;
            BackColor = UiTheme.Background;
            ForeColor = UiTheme.TextPrimary;
            Font = UiTheme.Body;
            ClientSize = new Size(520, 420);

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

            var bottom = new Panel
            {
                Dock = DockStyle.Bottom, Height = 64, BackColor = UiTheme.Header,
                Padding = new Padding(20, 14, 20, 14)
            };
            var btnCancel = new FlatButton { Text = "Cancel", Width = 110, Height = 36, DialogResult = DialogResult.Cancel };
            var btnOk = new FlatButton { Text = "Open", Width = 110, Height = 36, IsPrimary = true, DialogResult = DialogResult.OK };
            btnOk.Click += (s, e) =>
            {
                if (_list.SelectedItem is League l) SelectedLeague = l;
                else DialogResult = DialogResult.None;
            };

            var row = new FlowLayoutPanel
            {
                Dock = DockStyle.Right, FlowDirection = FlowDirection.RightToLeft,
                Width = 260, BackColor = UiTheme.Header
            };
            row.Controls.Add(btnOk);
            row.Controls.Add(btnCancel);
            bottom.Controls.Add(row);

            var body = new Panel
            {
                Dock = DockStyle.Fill, Padding = new Padding(24, 16, 24, 16), BackColor = UiTheme.Background
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
            _list.DoubleClick += (s, e) => { btnOk.PerformClick(); };
            foreach (var l in _leagues)
                _list.Items.Add(l);

            if (!string.IsNullOrWhiteSpace(activeLeagueName))
            {
                var active = _leagues.FirstOrDefault(l =>
                    l.Name.Equals(activeLeagueName, System.StringComparison.OrdinalIgnoreCase));
                if (active != null) _list.SelectedItem = active;
            }
            if (_list.SelectedIndex < 0 && _list.Items.Count > 0) _list.SelectedIndex = 0;

            body.Controls.Add(_list);

            Controls.Add(body);
            Controls.Add(bottom);
            Controls.Add(stripe);
            Controls.Add(titleBar);

            AcceptButton = btnOk;
            CancelButton = btnCancel;
        }
    }
}