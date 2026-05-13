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
            // Grow (never shrink) the button so the caller's intended width is a floor, not a cap —
            // long labels stay visible without forcing every call site to recalculate widths.
            var textSize = TextRenderer.MeasureText(text, Body);
            int needed = textSize.Width + 24;
            if (needed > b.Width) b.Width = needed;
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
            // Fill mode otherwise squeezes columns below their header text. Forcing each column's
            // MinimumWidth to its header width keeps labels visible; if the sum exceeds the grid,
            // a horizontal scrollbar appears instead of clipped headers.
            g.DataBindingComplete += (_, __) =>
            {
                // Rename ID-FK columns to friendlier headers BEFORE measuring so width fits the new text.
                foreach (DataGridViewColumn col in g.Columns)
                {
                    if (IsForeignKeyColumn(g, col.DataPropertyName))
                    {
                        switch (col.DataPropertyName)
                        {
                            case "PatientID":     col.HeaderText = "Patient"; break;
                            case "StaffID":       col.HeaderText = "Doctor / Staff"; break;
                            case "AdminStaffID":  col.HeaderText = "Administered by"; break;
                        }
                    }
                }
                foreach (DataGridViewColumn col in g.Columns)
                {
                    var headerSize = TextRenderer.MeasureText(col.HeaderText, Header);
                    col.MinimumWidth = headerSize.Width + 24; // padding for sort glyph + cell margin
                }
            };

            // Swap the displayed cell value (not the underlying data) for FK ID columns so users see names.
            g.CellFormatting += (_, e) =>
            {
                if (e.ColumnIndex < 0 || e.RowIndex < 0) return;
                var prop = g.Columns[e.ColumnIndex].DataPropertyName;
                if (e.Value is not int id) return;
                if (!IsForeignKeyColumn(g, prop)) return;
                switch (prop)
                {
                    case "PatientID":
                        e.Value = NameCache.PatientName(id);
                        e.FormattingApplied = true;
                        break;
                    case "StaffID":
                    case "AdminStaffID":
                        e.Value = NameCache.StaffName(id);
                        e.FormattingApplied = true;
                        break;
                }
            };
            return g;
        }

        // A column is the row's own PK if its DataPropertyName matches "{TypeName}ID" of the bound items;
        // otherwise PatientID / StaffID / AdminStaffID are foreign keys and safe to resolve to a name.
        private static bool IsForeignKeyColumn(DataGridView g, string propName)
        {
            if (propName != "PatientID" && propName != "StaffID" && propName != "AdminStaffID")
                return false;
            if (g.DataSource is System.Collections.IList list && list.Count > 0 && list[0] != null)
            {
                var typeName = list[0].GetType().Name;
                if (propName == typeName + "ID") return false; // own PK
            }
            return true;
        }

        private static readonly Color TabActive   = Color.FromArgb(45, 90, 160);
        private static readonly Color TabInactive = Color.FromArgb(100, 130, 180);

        /// <summary>
        /// TabControl with owner-drawn tabs so the strip is always visible on
        /// Windows 11 (the default visual styles wash it out entirely).
        /// </summary>
        public static TabControl MakeTabControl(bool verticalSidebar = false)
        {
            var tc = new TabControl
            {
                Dock      = DockStyle.Fill,
                Font      = Body,
                SizeMode  = TabSizeMode.Fixed,
                DrawMode  = TabDrawMode.OwnerDrawFixed,
                Alignment = verticalSidebar ? TabAlignment.Left : TabAlignment.Top,
                Multiline = true,
                ItemSize  = verticalSidebar ? new Size(45, 220) : new Size(160, 36),
            };

            // OwnerDrawFixed forces every tab to share one ItemSize. Once the caller has populated
            // TabPages, measure the longest label (in the bold "selected" font) and grow the strip
            // so no label is clipped. For vertical sidebars the long axis is Height, not Width.
            tc.HandleCreated += (_, __) =>
            {
                if (tc.TabPages.Count == 0) return;
                int needed = verticalSidebar ? 180 : 120;
                foreach (TabPage p in tc.TabPages)
                {
                    var sz = TextRenderer.MeasureText(p.Text, Header);
                    int w = sz.Width + 28;
                    if (w > needed) needed = w;
                }
                tc.ItemSize = verticalSidebar ? new Size(45, needed) : new Size(needed, 36);
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
