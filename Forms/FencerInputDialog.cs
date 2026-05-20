using System.Drawing;
using System.Windows.Forms;
using HemaLeagueManager.Models;
using Microsoft.VisualBasic;

namespace HemaLeagueManager.Forms
{
    public class FencerInputDialog : Form
    {
        public Fencer Result { get; private set; } = new Fencer();

        private TextBox _nameBox = null!;
        private ComboBox _sexBox = null!;
        private TextBox _clubBox = null!;

        public FencerInputDialog()
        {
            Text = "New Fencer";
            Size = new Size(360, 220);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(45, 35, 25);
            ForeColor = Color.Wheat;
            Font = new Font("Garamond", 10F);

            Controls.Add(new Label { Text = "Name:", Left = 12, Top = 15, Width = 80 });
            _nameBox = new TextBox { Left = 100, Top = 12, Width = 220 };
            Controls.Add(_nameBox);

            Controls.Add(new Label { Text = "Sex:", Left = 12, Top = 50, Width = 80 });
            _sexBox = new ComboBox { Left = 100, Top = 47, Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
            _sexBox.Items.AddRange(new object[] { "Male", "Female", "Other" });
            Controls.Add(_sexBox);

            Controls.Add(new Label { Text = "Club:", Left = 12, Top = 85, Width = 80 });
            _clubBox = new TextBox { Left = 100, Top = 82, Width = 220 };
            Controls.Add(_clubBox);

            var ok = new Button { Text = "OK", Left = 150, Top = 130, Width = 80, DialogResult = DialogResult.OK };
            ok.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(_nameBox.Text) || _sexBox.SelectedItem == null)
                {
                    MessageBox.Show("Name and sex are required.");
                    DialogResult = DialogResult.None;
                    return;
                }
                Result = new Fencer
                {
                    Name = _nameBox.Text.Trim(),
                    Sex = _sexBox.SelectedItem!.ToString()!,
                    ClubName = _clubBox.Text.Trim()
                };
            };
            var cancel = new Button { Text = "Cancel", Left = 240, Top = 130, Width = 80, DialogResult = DialogResult.Cancel };
            Controls.Add(ok);
            Controls.Add(cancel);

            AcceptButton = ok;
            CancelButton = cancel;
        }
    }
}