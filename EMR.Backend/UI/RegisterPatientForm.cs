using System;
using System.Drawing;
using System.Windows.Forms;
using EMR.Backend.Models;
using EMR.Backend.Services;

namespace EMR.Backend.UI
{
    /// <summary>
    /// FR-01: Patients register via kiosk / tablet / web. Captures the
    /// fields on the Patient table plus a username + password.
    /// </summary>
    public class RegisterPatientForm : Form
    {
        private readonly AuthService _auth = new AuthService();

        private TextBox _firstName, _lastName, _gender, _address, _phone, _email, _emergency;
        private TextBox _username, _password, _passwordConfirm;
        private DateTimePicker _dob;

        public string RegisteredUsername { get; private set; }

        public RegisterPatientForm()
        {
            Text = "EMRKS - New Patient Registration";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(540, 560);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            Controls.Add(new Label
            {
                Text = "Register a new patient",
                Location = new Point(20, 16),
                Size = new Size(500, 30),
                Font = UiHelpers.Title,
            });

            int y = 60, gap = 35;
            void AddRow(string label, Control input)
            {
                Controls.Add(UiHelpers.MakeLabel(label, 20, y));
                input.Location = new Point(160, y - 2);
                input.Size = new Size(340, 24);
                input.Font = UiHelpers.Body;
                Controls.Add(input);
                y += gap;
            }

            AddRow("First name *",   _firstName = new TextBox());
            AddRow("Last name *",    _lastName  = new TextBox());
            AddRow("Date of birth *", _dob = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = new DateTime(1990, 1, 1) });
            AddRow("Gender *",       _gender    = new TextBox());
            AddRow("Address *",      _address   = new TextBox());
            AddRow("Phone *",        _phone     = new TextBox());
            AddRow("Email *",        _email     = new TextBox());
            AddRow("Emergency contact *", _emergency = new TextBox());
            AddRow("Username *",     _username  = new TextBox());
            _password = new TextBox { UseSystemPasswordChar = true };
            AddRow("Password *",     _password);
            _passwordConfirm = new TextBox { UseSystemPasswordChar = true };
            AddRow("Confirm password *", _passwordConfirm);

            Controls.Add(UiHelpers.MakeButton("Cancel",   240, y + 10, 120, (s, e) => { DialogResult = DialogResult.Cancel; Close(); }));
            Controls.Add(UiHelpers.MakeButton("Register", 380, y + 10, 120, OnRegister));
        }

        private void OnRegister(object sender, EventArgs e)
        {
            // basic validation
            if (string.IsNullOrWhiteSpace(_firstName.Text) ||
                string.IsNullOrWhiteSpace(_lastName.Text)  ||
                string.IsNullOrWhiteSpace(_gender.Text)    ||
                string.IsNullOrWhiteSpace(_address.Text)   ||
                string.IsNullOrWhiteSpace(_phone.Text)     ||
                string.IsNullOrWhiteSpace(_email.Text)     ||
                string.IsNullOrWhiteSpace(_emergency.Text) ||
                string.IsNullOrWhiteSpace(_username.Text)  ||
                string.IsNullOrWhiteSpace(_password.Text))
            {
                UiHelpers.Error("All fields marked * are required.");
                return;
            }

            if (_password.Text != _passwordConfirm.Text)
            {
                UiHelpers.Error("Passwords do not match.");
                return;
            }

            try
            {
                var patient = new Patient
                {
                    FirstName        = _firstName.Text.Trim(),
                    LastName         = _lastName.Text.Trim(),
                    DateOfBirth      = _dob.Value.Date,
                    Gender           = _gender.Text.Trim(),
                    Address          = _address.Text.Trim(),
                    Phone            = _phone.Text.Trim(),
                    Email            = _email.Text.Trim(),
                    EmergencyContact = _emergency.Text.Trim(),
                    Username         = _username.Text.Trim(),
                };
                _auth.RegisterPatient(patient, _password.Text);
                RegisteredUsername = patient.Username;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                UiHelpers.Error("Registration failed: " + ex.Message);
            }
        }
    }
}
