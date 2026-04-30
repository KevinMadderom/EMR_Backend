using System;
using System.Windows.Forms;
using EMR.Backend.Data;
using EMR.Backend.UI;

namespace EMR.Backend
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // Smoke-test the database connection on startup.
            if (!DatabaseHelper.TestConnection())
            {
                MessageBox.Show(
                    "Could not connect to the EMRKS database.\n\n" +
                    "Make sure MySQL is running and that appsettings.json " +
                    "has the correct connection string.",
                    "EMRKS - Database Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            Application.Run(new LoginForm());
        }
    }
}
