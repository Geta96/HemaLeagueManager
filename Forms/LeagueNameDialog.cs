using System;
using System.Drawing;
using System.Windows.Forms;

namespace HemaLeagueManager.Forms
{
    /// <summary>Themed input dialog for creating a new league.</summary>
    public class LeagueNameDialog : Form
    {
        public string LeagueName { get; private set; } = string.Empty;

        private TextBox _nameBox = null!;

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
            ClientSize = new Size(520, 280);

            // Header
            var titleBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = UiTheme.Header,
                Padding = new Padding(24, 14, 24, 0)
            };
            titleBar.Controls.Add(new Label
            {
                Text = "⚔  Create New League",
                Font = UiTheme.TitleMedium,
                ForeColor = UiTheme.Accent,
                AutoSize = true,
                Location = new Point(0, 10)
            });

            var stripe = new Panel { Dock = DockStyle.Top, Height = 2, BackColor = UiTheme.Accent };

            // Buttons
            var bottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 64,
                BackColor = UiTheme.Header,
                Padding = new Padding(20, 14, 20, 14)
            };
            var btnCancel = new FlatButton
            {
                Text = "Cancel",
                Width = 130,
                Height = 36,
                DialogResult = DialogResult.Cancel
            };
            var btnOk = new FlatButton
            {
                Text = "Create",
                Width = 130,
                Height = 36,
                IsPrimary = true,
                DialogResult = DialogResult.OK
            };
            btnOk.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(_nameBox.Text))
                {
                    DialogResult = DialogResult.None;
                    MessageBox.Show("Please enter a league name.");
                    return;
                }
                LeagueName = _nameBox.Text.Trim();
            };

            var row = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                FlowDirection = FlowDirection.RightToLeft,
                Width = 300,
                BackColor = UiTheme.Header
            };
            row.Controls.Add(btnOk);
            row.Controls.Add(btnCancel);
            bottom.Controls.Add(row);

            // Body — label + input
            var body = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(28, 24, 28, 16),
                BackColor = UiTheme.Background
            };

            var label = new Label
            {
                Text = "League name:",
                Font = UiTheme.Subtitle,
                ForeColor = UiTheme.TextMuted,
                AutoSize = true,
                Location = new Point(0, 0)
            };

            var hint = new Label
            {
                Text = "This name identifies the league across all tabs and reports.",
                Font = UiTheme.Small,
                ForeColor = UiTheme.TextMuted,
                AutoSize = true,
                Location = new Point(0, 70)
            };

            _nameBox = new TextBox
            {
                Text = defaultName,
                Font = new Font(UiTheme.Body.FontFamily, 12F),
                BackColor = UiTheme.SurfaceAlt,
                ForeColor = UiTheme.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(0, 32),
                Width = body.ClientSize.Width - body.Padding.Horizontal,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            body.Resize += (s, e) =>
                _nameBox.Width = body.ClientSize.Width - body.Padding.Horizontal;
            _nameBox.SelectAll();

            body.Controls.Add(label);
            body.Controls.Add(_nameBox);
            body.Controls.Add(hint);

            Controls.Add(body);
            Controls.Add(bottom);
            Controls.Add(stripe);
            Controls.Add(titleBar);

            AcceptButton = btnOk;
            CancelButton = btnCancel;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            _nameBox.Focus();
        }
    }
}