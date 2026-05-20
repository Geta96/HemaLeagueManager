using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using HemaLeagueManager.Services;

namespace HemaLeagueManager.Forms
{
    public class SwitchLeagueDialog : Form
    {
        private ListView _list = null!;

        public string? SelectedPath { get; private set; }

        public SwitchLeagueDialog()
        {
            BuildUi();
            LoadLeagues();
        }

        private void BuildUi()
        {
            Text = "Switch League";
            Size = new Size(620, 460);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = UiTheme.Background;
            ForeColor = UiTheme.TextPrimary;
            Font = UiTheme.Body;
            MinimizeBox = false;
            MaximizeBox = false;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Padding = new Padding(16);

            var title = new Label
            {
                Text = "Choose a league",
                Font = UiTheme.TitleMedium,
                ForeColor = UiTheme.Accent,
                Dock = DockStyle.Top,
                Height = 36
            };
            var sub = new Label
            {
                Text = "Leagues are stored in " + LeagueLibrary.LibraryFolder,
                Font = UiTheme.Small,
                ForeColor = UiTheme.TextMuted,
                Dock = DockStyle.Top,
                Height = 22
            };

            _list = new ListView
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
            _list.Columns.Add("League", 320);
            _list.Columns.Add("Last modified", 200);
            _list.DoubleClick += (s, e) => Accept();

            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 52,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 10, 0, 0),
                BackColor = UiTheme.Background
            };
            var btnCancel = new FlatButton { Text = "Cancel", Width = 100, DialogResult = DialogResult.Cancel };
            var btnOpen = new FlatButton { Text = "Open", Width = 100, IsPrimary = true };
            btnOpen.Click += (s, e) => Accept();
            var btnDelete = new FlatButton { Text = "Delete", Width = 100 };
            btnDelete.SetColors(UiTheme.Danger, UiTheme.DangerHover, Color.White);
            btnDelete.Click += (s, e) => DeleteSelected();

            buttons.Controls.Add(btnCancel);
            buttons.Controls.Add(btnOpen);
            buttons.Controls.Add(btnDelete);

            Controls.Add(_list);
            Controls.Add(buttons);
            Controls.Add(sub);
            Controls.Add(title);

            AcceptButton = btnOpen;
            CancelButton = btnCancel;
        }

        private void LoadLeagues()
        {
            _list.Items.Clear();
            foreach (var file in LeagueLibrary.ListLeagueFiles())
            {
                var item = new ListViewItem(Path.GetFileNameWithoutExtension(file)) { Tag = file };
                item.SubItems.Add(File.GetLastWriteTime(file).ToString("yyyy-MM-dd HH:mm"));
                _list.Items.Add(item);
            }
            if (_list.Items.Count > 0) _list.Items[0].Selected = true;
        }

        private void Accept()
        {
            if (_list.SelectedItems.Count == 0)
            {
                MessageBox.Show("Select a league first.");
                return;
            }
            SelectedPath = (string)_list.SelectedItems[0].Tag!;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void DeleteSelected()
        {
            if (_list.SelectedItems.Count == 0) return;
            var path = (string)_list.SelectedItems[0].Tag!;
            var name = Path.GetFileNameWithoutExtension(path);
            if (MessageBox.Show($"Delete league '{name}'? This cannot be undone.",
                    "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                LeagueLibrary.Delete(path);
                LoadLeagues();
            }
        }
    }
}