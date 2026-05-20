using System;
using System.Drawing;
using System.Windows.Forms;
using HemaLeagueManager.Models;

namespace HemaLeagueManager.Forms
{
    /// <summary>Themed input dialog for creating a new league.</summary>
    public class LeagueNameDialog : Form
    {
        public string LeagueName { get; private set; } = string.Empty;
        public LeagueGender Gender { get; private set; } = LeagueGender.Open;

        private TextBox _nameBox = null!;
        private ComboBox _genderBox = null!;

        public LeagueNameDialog(string defaultName = "")
        {
            Text = "New League";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MinimizeBox = false;
            MaximizeBox = false;
            BackColor = UiTheme.Background;
            ForeColor = UiTheme.TextPrimary;
            Font = UiTheme.Body;
            ShowInTaskbar = false;
            ClientSize = new Size(540, 340);

            // Header
            var titleBar = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = UiTheme.Header, Padding = new Padding(24, 14, 24, 0) };
            titleBar.Controls.Add(new Label
            {
                Text = "⚔  Create New League",
                Font = UiTheme.TitleMedium, ForeColor = UiTheme.Accent,
                AutoSize = true, Location = new Point(0, 10)
            });
            var stripe = new Panel { Dock = DockStyle.Top, Height = 2, BackColor = UiTheme.Accent };

            // Buttons
            var bottom = new Panel { Dock = DockStyle.Bottom, Height = 64, BackColor = UiTheme.Header, Padding = new Padding(20, 14, 20, 14) };
            var btnCancel = new FlatButton { Text = "Cancel", Width = 130, Height = 36, DialogResult = DialogResult.Cancel };
            var btnOk = new FlatButton { Text = "Create", Width = 130, Height = 36, IsPrimary = true, DialogResult = DialogResult.OK };
            btnOk.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(_nameBox.Text))
                {
                    DialogResult = DialogResult.None;
                    MessageBox.Show("Please enter a league name.");
                    return;
                }
                LeagueName = _nameBox.Text.Trim();
                Gender = (LeagueGender)(_genderBox.SelectedItem ?? LeagueGender.Open);
            };
            var row = new FlowLayoutPanel
            {
                Dock = DockStyle.Right, FlowDirection = FlowDirection.RightToLeft,
                Width = 300, BackColor = UiTheme.Header
            };
            row.Controls.Add(btnOk);
            row.Controls.Add(btnCancel);
            bottom.Controls.Add(row);

            // Body
            var body = new Panel { Dock = DockStyle.Fill, Padding = new Padding(28, 24, 28, 16), BackColor = UiTheme.Background };

            var nameLabel = new Label
            {
                Text = "League name:",
                Font = UiTheme.Subtitle, ForeColor = UiTheme.TextMuted,
                AutoSize = true, Location = new Point(0, 0)
            };
            _nameBox = new TextBox
            {
                Text = defaultName,
                Font = new Font(UiTheme.Body.FontFamily, 12F),
                BackColor = UiTheme.SurfaceAlt, ForeColor = UiTheme.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(0, 32),
                Width = body.ClientSize.Width - body.Padding.Horizontal,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            _nameBox.SelectAll();

            var genderLabel = new Label
            {
                Text = "League gender:",
                Font = UiTheme.Subtitle, ForeColor = UiTheme.TextMuted,
                AutoSize = true, Location = new Point(0, 80)
            };
            _genderBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = UiTheme.Body,
                Location = new Point(0, 112),
                Width = 260,
                BackColor = UiTheme.SurfaceAlt,
                ForeColor = UiTheme.TextPrimary,
                FlatStyle = FlatStyle.Flat
            };
            _genderBox.Items.Add(LeagueGender.Open);
            _genderBox.Items.Add(LeagueGender.Male);
            _genderBox.Items.Add(LeagueGender.Female);
            _genderBox.SelectedIndex = 0;

            var hint = new Label
            {
                Text = "Female-only leagues only allow Female fencers.\n" +
                       "Male-only leagues allow Male and unspecified fencers.\n" +
                       "Open leagues allow everyone.",
                Font = UiTheme.Small, ForeColor = UiTheme.TextMuted,
                AutoSize = true, Location = new Point(0, 152)
            };

            body.Resize += (s, e) => _nameBox.Width = body.ClientSize.Width - body.Padding.Horizontal;

            body.Controls.Add(nameLabel);
            body.Controls.Add(_nameBox);
            body.Controls.Add(genderLabel);
            body.Controls.Add(_genderBox);
            body.Controls.Add(hint);

            Controls.Add(body);
            Controls.Add(bottom);
            Controls.Add(stripe);
            Controls.Add(titleBar);

            AcceptButton = btnOk;
            CancelButton = btnCancel;
        }

        protected override void OnShown(EventArgs e) { base.OnShown(e); _nameBox.Focus(); }
    }
}