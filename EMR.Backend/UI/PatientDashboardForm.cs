using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using EMR.Backend.Models;
using EMR.Backend.Repositories;
using EMR.Backend.Services;

namespace EMR.Backend.UI
{
    /// <summary>
    /// FR-03 .. FR-08 -- patient self-service dashboard.
    /// Tabs: medical history, profile, appointments, prescriptions,
    /// chronic conditions, billing.
    /// </summary>
    public class PatientDashboardForm : Form
    {
        private readonly Patient _me = Session.CurrentPatient;
        private readonly MedicalRecordRepository    _recordsRepo  = new MedicalRecordRepository();
        private readonly AppointmentRepository      _aptRepo      = new AppointmentRepository();
        private readonly PrescriptionRepository     _rxRepo       = new PrescriptionRepository();
        private readonly LabResultRepository        _labRepo      = new LabResultRepository();
        private readonly BillingRepository          _billRepo     = new BillingRepository();
        private readonly PaymentRepository          _payRepo      = new PaymentRepository();
        private readonly PatientRepository          _patientRepo  = new PatientRepository();
        private readonly NotificationRepository     _notifRepo    = new NotificationRepository();
        private readonly AllergyRepository          _allergyRepo  = new AllergyRepository();
        private readonly ImmunizationRepository     _immunRepo    = new ImmunizationRepository();
        private readonly InsuranceRepository        _insurRepo    = new InsuranceRepository();
        private readonly AuditLogger                _audit        = new AuditLogger();

        public PatientDashboardForm()
        {
            Text = $"EMRKS - Patient Portal ({Session.DisplayName})";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1000, 680);
            MinimumSize = new Size(900, 600);

            var top = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.FromArgb(245, 247, 250) };
            top.Controls.Add(new Label
            {
                Text = $"Welcome, {_me.FirstName} {_me.LastName}",
                Location = new Point(16, 16),
                Size = new Size(500, 30),
                Font = UiHelpers.Title,
            });
            var btnLogout = UiHelpers.MakeButton("Logout", 0, 14, 110, (_, __) => Close());
            btnLogout.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnLogout.Left = top.Width - btnLogout.Width - 20; // adjust for form resizing
            top.Controls.Add(btnLogout);

            var tabs = UiHelpers.MakeTabControl(verticalSidebar: true);
            tabs.TabPages.Add(BuildHistoryTab());
            tabs.TabPages.Add(BuildProfileTab());
            tabs.TabPages.Add(BuildAppointmentsTab());
            tabs.TabPages.Add(BuildPrescriptionsTab());
            tabs.TabPages.Add(BuildChronicTab());
            tabs.TabPages.Add(BuildBillingTab());
            tabs.TabPages.Add(BuildLabResultsTab());
            tabs.TabPages.Add(BuildNotificationsTab());
            tabs.TabPages.Add(BuildAllergiesTab());
            tabs.TabPages.Add(BuildImmunizationsTab());
            tabs.TabPages.Add(BuildInsuranceTab());
            Controls.Add(tabs);
            Controls.Add(top);

            _audit.LogPatient(_me.PatientID, "Opened patient dashboard");
        }

        // ---------- FR-03: complete medical history ----------
        private TabPage BuildHistoryTab()
        {
            var tab = new TabPage("Medical History");
            var grid = UiHelpers.MakeGrid(10, 10, 950, 550);
            grid.Dock = DockStyle.Fill;
            tab.Controls.Add(grid);
            grid.DataSource = _recordsRepo.GetByPatient(_me.PatientID);
            _audit.LogPatient(_me.PatientID, "Viewed medical history");
            return tab;
        }

        // ---------- FR-04: update personal info ----------
        private TabPage BuildProfileTab()
        {
            var tab = new TabPage("Profile");
            tab.AutoScroll = true;

            int y = 20, gap = 44;
            TextBox first = null, last = null, gender = null, addr = null, phone = null, email = null, emergency = null;
            DateTimePicker dob = null;

            void Row(string label, Control input)
            {
                tab.Controls.Add(UiHelpers.MakeLabel(label, 30, y));
                input.Location = new Point(180, y - 2);
                input.Size = new Size(360, 24);
                input.Font = UiHelpers.Body;
                tab.Controls.Add(input);
                y += gap;
            }

            Row("First name",        first     = new TextBox { Text = _me.FirstName });
            Row("Last name",         last      = new TextBox { Text = _me.LastName });
            Row("Date of birth",     dob       = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = _me.DateOfBirth });
            Row("Gender",            gender    = new TextBox { Text = _me.Gender });
            Row("Address",           addr      = new TextBox { Text = _me.Address });
            Row("Phone",             phone     = new TextBox { Text = _me.Phone });
            Row("Email",             email     = new TextBox { Text = _me.Email });
            Row("Emergency contact", emergency = new TextBox { Text = _me.EmergencyContact });

            tab.Controls.Add(UiHelpers.MakeButton("Save changes", 180, y + 10, 160, (s, e) =>
            {
                _me.FirstName        = first.Text.Trim();
                _me.LastName         = last.Text.Trim();
                _me.DateOfBirth      = dob.Value.Date;
                _me.Gender           = gender.Text.Trim();
                _me.Address          = addr.Text.Trim();
                _me.Phone            = phone.Text.Trim();
                _me.Email            = email.Text.Trim();
                _me.EmergencyContact = emergency.Text.Trim();
                if (_patientRepo.UpdatePatient(_me))
                {
                    _audit.LogPatient(_me.PatientID, "Updated profile");
                    UiHelpers.Info("Profile saved.");
                }
                else UiHelpers.Error("Could not save profile.");
            }));
            return tab;
        }

        // ---------- FR-05: appointments ----------
        private TabPage BuildAppointmentsTab()
        {
            var tab = new TabPage("Appointments");
            var grid = UiHelpers.MakeGrid(0, 0, 0, 0);
            grid.Dock = DockStyle.Fill;
            tab.Controls.Add(grid);
            grid.DataSource = _aptRepo.GetByPatient(_me.PatientID);
            _audit.LogPatient(_me.PatientID, "Viewed appointments");
            return tab;
        }

        // ---------- FR-06: prescriptions ----------
        private TabPage BuildPrescriptionsTab()
        {
            var tab = new TabPage("Prescriptions");
            var grid = UiHelpers.MakeGrid(0, 0, 0, 0);
            grid.Dock = DockStyle.Fill;
            tab.Controls.Add(grid);
            grid.DataSource = _rxRepo.GetByPatient(_me.PatientID);
            _audit.LogPatient(_me.PatientID, "Viewed prescriptions");
            return tab;
        }

        // ---------- FR-07: chronic-condition tracking ----------
        private TabPage BuildChronicTab()
        {
            var tab = new TabPage("Chronic Conditions");
            var grid = UiHelpers.MakeGrid(0, 0, 0, 0);
            grid.Dock = DockStyle.Fill;
            tab.Controls.Add(grid);
            var rows = _recordsRepo.GetChronicByPatient(_me.PatientID);
            if (rows.Count == 0)
            {
                tab.Controls.Clear();
                tab.Controls.Add(new Label
                {
                    Text = "No chronic-condition records yet.\n\n" +
                           "Records with RecordType containing 'Chronic' will appear here so you can track progress over time.",
                    Dock = DockStyle.Fill,
                    Font = UiHelpers.Body,
                    TextAlign = ContentAlignment.MiddleCenter,
                });
            }
            else grid.DataSource = rows;
            _audit.LogPatient(_me.PatientID, "Viewed chronic conditions");
            return tab;
        }

        // ---------- FR-08: billing ----------
        private TabPage BuildBillingTab()
        {
            var tab = new TabPage("Billing");

            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(10),
            };

            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var balance = _billRepo.GetBalance(_me.PatientID);
            var lblBalance = new Label
            {
                Text = $"Current balance: ${balance:F2}",
                Dock = DockStyle.Fill,
                Font = UiHelpers.Header,
                TextAlign = ContentAlignment.MiddleCenter,
            };

            table.Controls.Add(lblBalance, 0, 0);
            table.SetColumnSpan(lblBalance, 2);

            table.Controls.Add(new Label 
            { 
                Text = "Outstanding Bills",
                Font = UiHelpers.Header,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.BottomLeft 
            }, 0, 1);

            table.Controls.Add(new Label 
            { 
                Text = "Payment History",
                Font = UiHelpers.Header,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.BottomLeft 
            }, 1, 1);

            // grid 1
            var billGrid = UiHelpers.MakeGrid(0, 0, 100, 100);
            billGrid.Dock = DockStyle.Fill;
            billGrid.DataSource = _billRepo.GetByPatient(_me.PatientID);
            table.Controls.Add(billGrid, 0, 2);

            // grid 2
            var payGrid = UiHelpers.MakeGrid(0, 0, 100, 100);
            payGrid.Dock = DockStyle.Fill;
            payGrid.DataSource = _payRepo.GetByPatient(_me.PatientID);
            table.Controls.Add(payGrid, 1, 2);

            tab.Controls.Add(table);

            _audit.LogPatient(_me.PatientID, "Viewed billing");
            return tab;
        }

        // Lab results -- supports use-case scenarios 2 (download) and is referenced by §4.1.
        private TabPage BuildLabResultsTab()
        {
            var tab = new TabPage("Lab Results");
            var grid = UiHelpers.MakeGrid(0, 0, 0, 0);
            grid.Dock = DockStyle.Fill;
            tab.Controls.Add(grid);
            grid.DataSource = _labRepo.GetByPatient(_me.PatientID);
            _audit.LogPatient(_me.PatientID, "Viewed lab results");
            return tab;
        }

        private TabPage BuildNotificationsTab()
        {
            var tab = new TabPage("Notifications");
            var grid = UiHelpers.MakeGrid(0, 0, 0, 0);
            grid.Dock = DockStyle.Fill;
            tab.Controls.Add(grid);
            grid.DataSource = _notifRepo.GetByPatient(_me.PatientID);
            return tab;
        }

        private TabPage BuildAllergiesTab()
        {
            var tab = new TabPage("Allergies");
            var grid = UiHelpers.MakeGrid(0, 0, 0, 0);
            grid.Dock = DockStyle.Fill;
            tab.Controls.Add(grid);
            grid.DataSource = _allergyRepo.GetByPatient(_me.PatientID);
            _audit.LogPatient(_me.PatientID, "Viewed allergies");
            return tab;
        }

        private TabPage BuildImmunizationsTab()
        {
            var tab = new TabPage("Immunizations");
            var grid = UiHelpers.MakeGrid(0, 0, 0, 0);
            grid.Dock = DockStyle.Fill;
            tab.Controls.Add(grid);
            grid.DataSource = _immunRepo.GetByPatient(_me.PatientID);
            _audit.LogPatient(_me.PatientID, "Viewed immunizations");
            return tab;
        }

        private TabPage BuildInsuranceTab()
        {
            var tab = new TabPage("Insurance");
            tab.AutoScroll = true;

            DataGridView grid = null;
            void Reload() { grid.DataSource = _insurRepo.GetByPatient(_me.PatientID); }

            grid = UiHelpers.MakeGrid(10, 10, 0, 200);
            grid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            Reload();
            tab.Controls.Add(grid);

            int y = 220;
            tab.Controls.Add(UiHelpers.MakeLabel("Add Insurance", 10, y, 200, true)); y += 28;

            tab.Controls.Add(UiHelpers.MakeLabel("Provider", 10, y));
            var provider = new TextBox { Location = new Point(160, y - 2), Size = new Size(300, 24), Font = UiHelpers.Body };
            tab.Controls.Add(provider); y += 34;

            tab.Controls.Add(UiHelpers.MakeLabel("Policy number", 10, y));
            var policy = new TextBox { Location = new Point(160, y - 2), Size = new Size(200, 24), Font = UiHelpers.Body };
            tab.Controls.Add(policy); y += 34;

            tab.Controls.Add(UiHelpers.MakeLabel("Coverage details", 10, y));
            var coverage = new TextBox
            {
                Location = new Point(160, y - 2), Size = new Size(500, 60),
                Font = UiHelpers.Body, Multiline = true,
            };
            tab.Controls.Add(coverage); y += 74;

            tab.Controls.Add(UiHelpers.MakeButton("Add Insurance", 160, y, 160, (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(provider.Text) || string.IsNullOrWhiteSpace(policy.Text))
                {
                    UiHelpers.Error("Provider and policy number are required.");
                    return;
                }
                int id = _insurRepo.AddInsurance(new Insurance
                {
                    PatientID       = _me.PatientID,
                    ProviderName    = provider.Text.Trim(),
                    PolicyNumber    = policy.Text.Trim(),
                    CoverageDetails = coverage.Text.Trim(),
                });
                if (id > 0)
                {
                    _audit.LogPatient(_me.PatientID, "Added insurance record");
                    UiHelpers.Info("Insurance added.");
                    provider.Clear(); policy.Clear(); coverage.Clear();
                    Reload();
                }
                else UiHelpers.Error("Could not save insurance.");
            }));

            _audit.LogPatient(_me.PatientID, "Viewed insurance");
            return tab;
        }
    }
}
