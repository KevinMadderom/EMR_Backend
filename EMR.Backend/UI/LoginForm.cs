using System;
using System.Drawing;
using System.Windows.Forms;
using EMR.Backend.Services;
using EMR.Backend.Models;

namespace EMR.Backend.UI
{
    /// <summary>
    /// FR-02 (patient login), FR-09 (staff login), FR-17 (admin login).
    /// Routes the authenticated user to the role-appropriate dashboard.
    /// </summary>
    public class LoginForm : Form
    {
        private readonly AuthService _auth = new AuthService();
        private RadioButton _rbPatient;
        private RadioButton _rbStaff;
        private TextBox _txtUser;
        private TextBox _txtPass;

        public LoginForm()
        {
            Text = "EMRKS - Login";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(460, 420);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            AutoScaleMode = AutoScaleMode.None;

            Controls.Add(new Label
            {
                Text = "Electronic Medical Record Keeping System",
                Location = new Point(20, 18),
                Size = new Size(420, 34),
                Font = UiHelpers.Title,
                TextAlign = ContentAlignment.MiddleCenter,
            });

            // Role selector
            _rbPatient = new RadioButton { Text = "Patient", Location = new Point(60, 72), Size = new Size(130, 28), Checked = true, Font = UiHelpers.Body };
            _rbStaff   = new RadioButton { Text = "Staff / Doctor / Admin", Location = new Point(210, 72), Size = new Size(210, 28), Font = UiHelpers.Body };
            Controls.Add(_rbPatient);
            Controls.Add(_rbStaff);

            // Username / Password
            Controls.Add(UiHelpers.MakeLabel("Username:", 40, 120));
            _txtUser = UiHelpers.MakeText(180, 118, 240);
            Controls.Add(_txtUser);

            Controls.Add(UiHelpers.MakeLabel("Password:", 40, 168));
            _txtPass = UiHelpers.MakeText(180, 166, 240, passwordChar: '*');
            Controls.Add(_txtPass);

            // Buttons
            var btnLogin    = UiHelpers.MakeButton("Login",              40,  220, 160, OnLogin);
            var btnRegister = UiHelpers.MakeButton("Register (Patient)", 240, 220, 170, OnRegister);
            var btnExit     = UiHelpers.MakeButton("Exit",               240, 272, 170, (s, e) => Close());
            Controls.Add(btnLogin);
            Controls.Add(btnRegister);
            Controls.Add(btnExit);

            AcceptButton = btnLogin;

            Controls.Add(new Label
            {
                Text = "Default admin: admin / admin123  (change after first login)",
                Location = new Point(20, 332),
                Size = new Size(420, 22),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 8.25F, FontStyle.Italic),
            });
        }

        private void OnLogin(object sender, EventArgs e)
        {
            string user = _txtUser.Text.Trim();
            string pass = _txtPass.Text;
            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                UiHelpers.Error("Username and password are required.");
                return;
            }

            if (_rbPatient.Checked)
            {
                Patient p = _auth.LoginPatient(user, pass);
                if (p == null) { UiHelpers.Error("Invalid patient credentials."); return; }
                Session.SetPatient(p);
                LaunchAndHide(new PatientDashboardForm());
            }
            else
            {
                Staff s = _auth.LoginStaff(user, pass);
                if (s == null) { UiHelpers.Error("Invalid staff credentials."); return; }
                Session.SetStaff(s);
                Form dash;
                string role = (s.AuthorizationLevel ?? s.Role ?? "").Trim();
                if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
                    dash = new AdminDashboardForm();
                else if (string.Equals(role, "Doctor", StringComparison.OrdinalIgnoreCase))
                    dash = new DoctorDashboardForm();
                else
                    dash = new StaffDashboardForm();
                LaunchAndHide(dash);
            }
        }

        private void LaunchAndHide(Form dashboard)
        {
            Hide();
            dashboard.FormClosed += (_, __) =>
            {
                Session.Clear();
                _txtPass.Clear();
                Show();
            };
            dashboard.Show();
        }

        private void OnRegister(object sender, EventArgs e)
        {
            using (var f = new RegisterPatientForm())
            {
                if (f.ShowDialog(this) == DialogResult.OK)
                {
                    UiHelpers.Info("Registration successful! You can now log in.");
                    _rbPatient.Checked = true;
                    _txtUser.Text = f.RegisteredUsername;
                    _txtPass.Focus();
                }
            }
        }
    }
}
