using System.Collections;
using System.Drawing;
using System.Windows.Forms;

namespace EMR.Backend.UI
{
    /// <summary>
    /// Generic "show me a table" popup used throughout the app for read-only
    /// list views.
    /// </summary>
    public class SimpleGridForm : Form
    {
        public SimpleGridForm(string title, IList data)
        {
            Text = "EMRKS - " + title;
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(900, 520);

            var lbl = new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                Height = 36,
                Font = UiHelpers.Header,
                Padding = new Padding(10, 8, 0, 0),
            };
            Controls.Add(lbl);

            var grid = UiHelpers.MakeGrid(0, 0, 0, 0);
            grid.Dock = DockStyle.Fill;
            grid.DataSource = data;
            Controls.Add(grid);
            grid.BringToFront();
        }
    }
}
