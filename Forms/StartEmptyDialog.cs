using System.Drawing;
using System.Windows.Forms;

namespace HemaLeagueManager.Forms
{
    /// <summary>
    /// Themed three-option dialog asking the user how much to wipe when
    /// starting an empty project.
    /// </summary>
    public class StartEmptyDialog : Form
    {
        public enum Choice { Cancel, ClearLeaguesOnly, ClearEverything }

        public Choice Result { get; private set; } = Choice.Cancel;

        public StartEmptyDialog()
        {
            Text = "Start Empty Project";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MinimizeBox = false;
            MaximizeBox = false;
            BackColor = UiTheme.Background;
            ForeColor = UiTheme.TextPrimary;
            Font = UiTheme.Body;
            ShowInTaskbar = false;

            const int dialogWidth = 620;

            // ---- Header ----
            var titleBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = UiTheme.Header,
                Padding = new Padding(24, 14, 24, 0)
            };
            titleBar.Controls.Add(new Label
            {
                Text = "⚔  Start Empty Project",
                Font = UiTheme.TitleMedium,
                ForeColor = UiTheme.Accent,
                AutoSize = true,
                Location = new Point(0, 10)
            });
            var stripe = new Panel { Dock = DockStyle.Top, Height = 2, BackColor = UiTheme.Accent };

            // ---- Buttons ----
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
                Width = 110,
                Height = 36,
                DialogResult = DialogResult.Cancel
            };
            var btnKeepFencers = new FlatButton
            {
                Text = "Keep fencers",
                Width = 160,
                Height = 36
            };
            btnKeepFencers.Click += (s, e) =>
            {
                Result = Choice.ClearLeaguesOnly;
                DialogResult = DialogResult.OK;
            };
            var btnWipeAll = new FlatButton
            {
                Text = "Delete everything",
                Width = 180,
                Height = 36
            };
            btnWipeAll.SetColors(UiTheme.Danger, UiTheme.DangerHover, Color.White);
            btnWipeAll.Click += (s, e) =>
            {
                Result = Choice.ClearEverything;
                DialogResult = DialogResult.OK;
            };

            var row = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                FlowDirection = FlowDirection.RightToLeft,
                Width = 540,
                BackColor = UiTheme.Header
            };
            row.Controls.Add(btnWipeAll);
            row.Controls.Add(btnKeepFencers);
            row.Controls.Add(btnCancel);
            bottom.Controls.Add(row);

            // ---- Body ----
            var body = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(28, 24, 28, 20),
                BackColor = UiTheme.Background
            };

            var msg = new Label
            {
                Text =
                    "This will clear the current workspace and remove all saved leagues " +
                    "and tournaments.\n\n" +
                    "Do you want to also delete the global fencer and club registries?\n\n" +
                    "• Keep fencers — leagues and tournaments are deleted, fencers and clubs are preserved.\n" +
                    "• Delete everything — leagues, tournaments, fencers and clubs are all wiped.",
                AutoSize = true,
                MaximumSize = new Size(dialogWidth - 56, 0),
                Font = UiTheme.Body,
                ForeColor = UiTheme.TextPrimary,
                Location = new Point(0, 0)
            };
            body.Controls.Add(msg);

            Controls.Add(body);
            Controls.Add(bottom);
            Controls.Add(stripe);
            Controls.Add(titleBar);

            AcceptButton = btnKeepFencers;
            CancelButton = btnCancel;

            int needed = msg.PreferredHeight + body.Padding.Vertical;
            int total = titleBar.Height + stripe.Height + needed + bottom.Height + 30;
            ClientSize = new Size(dialogWidth, System.Math.Max(280, total));
        }
    }
}   