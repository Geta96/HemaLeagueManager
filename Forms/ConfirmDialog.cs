using System.Drawing;
using System.Windows.Forms;

namespace HemaLeagueManager.Forms
{
    /// <summary>
    /// Themed Yes/No dialog matching the rest of the app.
    /// Returns DialogResult.Yes / DialogResult.No.
    /// </summary>
    public class ConfirmDialog : Form
    {
        public ConfirmDialog(string title, string message,
                             string yesText = "Yes", string noText = "No",
                             bool primaryYes = true)
        {
            Text = title;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MinimizeBox = false;
            MaximizeBox = false;
            BackColor = UiTheme.Background;
            ForeColor = UiTheme.TextPrimary;
            Font = UiTheme.Body;
            ShowInTaskbar = false;

            const int dialogWidth = 560;

            // Header bar (gold title)
            var titleBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = UiTheme.Header,
                Padding = new Padding(24, 14, 24, 0)
            };
            titleBar.Controls.Add(new Label
            {
                Text = "⚔  " + title,
                Font = UiTheme.TitleMedium,
                ForeColor = UiTheme.Accent,
                AutoSize = true,
                Location = new Point(0, 10)
            });

            var stripe = new Panel { Dock = DockStyle.Top, Height = 2, BackColor = UiTheme.Accent };

            // Buttons row
            var bottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 64,
                BackColor = UiTheme.Header,
                Padding = new Padding(20, 14, 20, 14)
            };
            var btnNo = new FlatButton
            {
                Text = noText,
                Width = 130,
                Height = 36,
                DialogResult = DialogResult.No
            };
            var btnYes = new FlatButton
            {
                Text = yesText,
                Width = 130,
                Height = 36,
                IsPrimary = primaryYes,
                DialogResult = DialogResult.Yes
            };

            var row = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                FlowDirection = FlowDirection.RightToLeft,
                Width = 300,
                BackColor = UiTheme.Header
            };
            row.Controls.Add(btnYes);
            row.Controls.Add(btnNo);
            bottom.Controls.Add(row);

            // Message — auto-sizes to required height for the wrapped text.
            var body = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(28, 24, 28, 20),
                BackColor = UiTheme.Background
            };
            var msg = new Label
            {
                Text = message,
                AutoSize = true,
                MaximumSize = new Size(dialogWidth - 56, 0),
                Font = UiTheme.Body,
                ForeColor = UiTheme.TextPrimary,
                TextAlign = ContentAlignment.TopLeft,
                Location = new Point(0, 0)
            };
            body.Controls.Add(msg);

            Controls.Add(body);
            Controls.Add(bottom);
            Controls.Add(stripe);
            Controls.Add(titleBar);

            AcceptButton = btnYes;
            CancelButton = btnNo;

            // Size the dialog to fit the message height comfortably.
            int neededBodyHeight = msg.PreferredHeight + body.Padding.Vertical;
            int totalHeight = titleBar.Height + stripe.Height + neededBodyHeight + bottom.Height + 40;
            ClientSize = new Size(dialogWidth, System.Math.Max(220, totalHeight));
        }

        public static bool Ask(IWin32Window? owner, string title, string message,
                               string yesText = "Yes", string noText = "No",
                               bool primaryYes = true)
        {
            using var dlg = new ConfirmDialog(title, message, yesText, noText, primaryYes);
            return dlg.ShowDialog(owner) == DialogResult.Yes;
        }
    }
}