using System;
using System.Drawing;
using System.Windows.Forms;

namespace EMR.Backend.UI
{
    /// <summary>
    /// Small helpers used by every form to keep the UI consistent
    /// (NFR-09 user-friendly interface).
    /// </summary>
    internal static class UiHelpers
    {
        public static readonly Font Title  = new Font("Segoe UI", 16F, FontStyle.Bold);
        public static readonly Font Header = new Font("Segoe UI", 11F, FontStyle.Bold);
        public static readonly Font Body   = new Font("Segoe UI", 10F);

        public static Label MakeLabel(string text, int x, int y, int w = 130, bool bold = false)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(w, 24),
                Font = bold ? Header : Body,
            };
        }

        public static TextBox MakeText(int x, int y, int w = 250, char? passwordChar = null)
        {
            var t = new TextBox
            {
                Location = new Point(x, y),
                Size = new Size(w, 24),
                Font = Body,
            };
            if (passwordChar.HasValue) t.UseSystemPasswordChar = true;
            return t;
        }

        public static Button MakeButton(string text, int x, int y, int w = 140, EventHandler onClick = null)
        {
            var b = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(w, 32),
                Font = Body,
            };
            if (onClick != null) b.Click += onClick;
            return b;
        }

        public static void Error(string msg)
            => MessageBox.Show(msg, "EMRKS", MessageBoxButtons.OK, MessageBoxIcon.Error);

        public static void Info(string msg)
            => MessageBox.Show(msg, "EMRKS", MessageBoxButtons.OK, MessageBoxIcon.Information);

        public static bool Confirm(string msg)
            => MessageBox.Show(msg, "EMRKS", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
               == DialogResult.Yes;

        public static DataGridView MakeGrid(int x, int y, int w, int h)
        {
            var g = new DataGridView
            {
                Location = new Point(x, y),
                Size = new Size(w, h),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false, // required so header styles below take effect
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing,
                ColumnHeadersHeight = 32,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single,
                Font = Body,
            };
            // Visible, high-contrast column headers (Windows 11 themes wash these out by default)
            g.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(45, 90, 160),
                ForeColor = Color.White,
                Font = Header,
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                SelectionBackColor = Color.FromArgb(45, 90, 160),
                SelectionForeColor = Color.White,
            };
            g.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.White,
                ForeColor = Color.Black,
                SelectionBackColor = Color.FromArgb(220, 232, 246),
                SelectionForeColor = Color.Black,
                Font = Body,
            };
            g.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(247, 249, 252),
                ForeColor = Color.Black,
            };
            return g;
        }

        private static readonly Color TabActive   = Color.FromArgb(45, 90, 160);
        private static readonly Color TabInactive = Color.FromArgb(100, 130, 180);

        /// <summary>
        /// TabControl with owner-drawn tabs so the strip is always visible on
        /// Windows 11 (the default visual styles wash it out entirely).
        /// </summary>
        public static TabControl MakeTabControl()
        {
            var tc = new TabControl
            {
                Dock     = DockStyle.Fill,
                Font     = Body,
                SizeMode = TabSizeMode.Fixed,
                ItemSize = new Size(220, 36),
                DrawMode = TabDrawMode.OwnerDrawFixed,
            };

            tc.DrawItem += (sender, e) =>
            {
                var tab  = (TabControl)sender;
                bool sel = e.Index == tab.SelectedIndex;
                var bg   = sel ? TabActive : TabInactive;
                using var brush = new SolidBrush(bg);
                e.Graphics.FillRectangle(brush, e.Bounds);
                var text = tab.TabPages[e.Index].Text;
                var tf   = new System.Drawing.StringFormat
                {
                    Alignment     = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                };
                using var textBrush = new SolidBrush(Color.White);
                var font = sel ? Header : Body;
                e.Graphics.DrawString(text, font, textBrush, e.Bounds, tf);
            };

            return tc;
        }
    }
}
