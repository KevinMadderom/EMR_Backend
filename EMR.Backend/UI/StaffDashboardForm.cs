using System;
using System.Drawing;
using System.Windows.Forms;
using EMR.Backend.Models;
using EMR.Backend.Repositories;
using EMR.Backend.Services;

namespace EMR.Backend.UI
{
    /// <summary>
    /// FR-15 (staff schedules tests/appointments) and FR-16 (staff views patient
    /// billing). Reused for nurses/receptionists.
    /// </summary>
    public class StaffDashboardForm : Form
    {
        private readonly Staff _me = Session.CurrentStaff;
        private readonly PatientRepository     _patientRepo = new PatientRepository();
        private readonly StaffRepository       _staffRepo   = new StaffRepository();
        private readonly AppointmentRepository _aptRepo     = new AppointmentRepository();
        private readonly BillingRepository     _billRepo    = new BillingRepository();
        private readonly PaymentRepository     _payRepo     = new PaymentRepository();
        private readonly AuditLogger           _audit       = new AuditLogger();

        public StaffDashboardForm()
        {
            Text = $"EMRKS - Staff Portal ({Session.DisplayName})";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1100, 680);
            MinimumSize = new Size(900, 600);

            var top = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.FromArgb(245, 247, 250) };
            top.Controls.Add(new Label
            {
                Text = $"Staff Portal -- {_me.FirstName} {_me.LastName} ({_me.Role})",
                Location = new Point(16, 16),
                Size = new Size(800, 30),
                Font = UiHelpers.Title,
            });
            top.Controls.Add(UiHelpers.MakeButton("Logout", 960, 14, 110, (_, __) => Close()));
            Controls.Add(top);

            var tabs = UiHelpers.MakeTabControl();
            tabs.TabPages.Add(BuildScheduleTab());
            tabs.TabPages.Add(BuildBillingTab());
            tabs.TabPages.Add(BuildAllAppointmentsTab());
            Controls.Add(tabs);

            _audit.LogStaff(_me.StaffID, "Opened staff dashboard");
        }

        // FR-15
        private TabPage BuildScheduleTab()
        {
            var tab = new TabPage("Schedule Appointment / Test (FR-15)");

            int y = 20;
            tab.Controls.Add(UiHelpers.MakeLabel("Patient", 20, y));
            var cboPatient = new ComboBox
            {
                Location = new Point(160, y - 2), Size = new Size(400, 24),
                Font = UiHelpers.Body, DropDownStyle = ComboBoxStyle.DropDownList,
            };
            foreach (var p in _patientRepo.GetAllPatients())
                cboPatient.Items.Add(new IdItem(p.PatientID, $"{p.LastName}, {p.FirstName} (#{p.PatientID})"));
            tab.Controls.Add(cboPatient); y += 44;

            tab.Controls.Add(UiHelpers.MakeLabel("Doctor / Provider", 20, y));
            var cboStaff = new ComboBox
            {
                Location = new Point(160, y - 2), Size = new Size(400, 24),
                Font = UiHelpers.Body, DropDownStyle = ComboBoxStyle.DropDownList,
            };
            foreach (var s in _staffRepo.GetAllStaff())
                cboStaff.Items.Add(new IdItem(s.StaffID, $"{s.LastName}, {s.FirstName} ({s.Role})"));
            tab.Controls.Add(cboStaff); y += 44;

            tab.Controls.Add(UiHelpers.MakeLabel("Date", 20, y));
            var dt = new DateTimePicker { Location = new Point(160, y - 2), Size = new Size(200, 24), Font = UiHelpers.Body, Format = DateTimePickerFormat.Short };
            tab.Controls.Add(dt); y += 44;

            tab.Controls.Add(UiHelpers.MakeLabel("Time", 20, y));
            var tp = new DateTimePicker { Location = new Point(160, y - 2), Size = new Size(200, 24), Font = UiHelpers.Body, Format = DateTimePickerFormat.Time, ShowUpDown = true };
            tab.Controls.Add(tp); y += 44;

            tab.Controls.Add(UiHelpers.MakeLabel("Type", 20, y));
            var cboType = new ComboBox
            {
                Location = new Point(160, y - 2), Size = new Size(220, 24),
                Font = UiHelpers.Body, DropDownStyle = ComboBoxStyle.DropDownList,
            };
            cboType.Items.AddRange(new object[] { "Consultation", "Follow-up", "Procedure", "Lab Test", "Imaging" });
            cboType.SelectedIndex = 0;
            tab.Controls.Add(cboType); y += 54;

            tab.Controls.Add(UiHelpers.MakeButton("Schedule (FR-15)", 160, y, 200, (s, e) =>
            {
                if (cboPatient.SelectedItem is not IdItem p || cboStaff.SelectedItem is not IdItem st)
                {
                    UiHelpers.Error("Pick both a patient and a provider.");
                    return;
                }
                var newId = _aptRepo.AddAppointment(new Appointment
                {
                    PatientID = p.Id,
                    StaffID = st.Id,
                    AppointmentDate = dt.Value.Date,
                    AppointmentTime = tp.Value.TimeOfDay,
                    Status = "Scheduled",
                    AppointmentType = (string)cboType.SelectedItem,
                });
                if (newId > 0)
                {
                    _audit.LogStaff(_me.StaffID, $"Scheduled appointment #{newId} for patient {p.Id} (FR-15)");
                    UiHelpers.Info($"Appointment #{newId} scheduled.");
                }
                else UiHelpers.Error("Could not schedule appointment.");
            }));
            return tab;
        }

        // FR-16
        private TabPage BuildBillingTab()
        {
            var tab = new TabPage("Patient Billing (FR-16)");

            tab.Controls.Add(new Label { Text = "Pick a patient:", Location = new Point(10, 10), Size = new Size(130, 24), Font = UiHelpers.Body });
            var cbo = new ComboBox
            {
                Location = new Point(140, 8), Size = new Size(380, 24),
                Font = UiHelpers.Body, DropDownStyle = ComboBoxStyle.DropDownList,
            };
            foreach (var p in _patientRepo.GetAllPatients())
                cbo.Items.Add(new IdItem(p.PatientID, $"{p.LastName}, {p.FirstName} (#{p.PatientID})"));

            var balance = new Label { Location = new Point(540, 10), Size = new Size(380, 24), Font = UiHelpers.Header };

            // Grids — top half of available area
            var bills = UiHelpers.MakeGrid(10, 40, 510, 220);
            bills.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            var pays = UiHelpers.MakeGrid(530, 40, 500, 220);
            pays.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            tab.Controls.Add(new Label { Text = "Bills", Location = new Point(10, 40), Size = new Size(200, 18), Font = UiHelpers.Body });
            tab.Controls.Add(new Label { Text = "Payments", Location = new Point(530, 40), Size = new Size(200, 18), Font = UiHelpers.Body });
            tab.Controls.Add(bills);
            tab.Controls.Add(pays);

            void RefreshGrids()
            {
                if (cbo.SelectedItem is IdItem item)
                {
                    bills.DataSource = _billRepo.GetByPatient(item.Id);
                    pays.DataSource  = _payRepo.GetByPatient(item.Id);
                    balance.Text     = $"Balance: ${_billRepo.GetBalance(item.Id):F2}";
                }
            }

            cbo.SelectedIndexChanged += (s, e) =>
            {
                if (cbo.SelectedItem is IdItem item)
                {
                    RefreshGrids();
                    _audit.LogStaff(_me.StaffID, $"Staff viewed billing for patient {item.Id} (FR-16)");
                }
            };

            // ---------- Create Bill ----------
            int y = 270;
            tab.Controls.Add(UiHelpers.MakeLabel("Create Bill", 10, y, 160, true)); y += 26;

            tab.Controls.Add(UiHelpers.MakeLabel("Total amount ($)", 10, y));
            var amount = new NumericUpDown
            {
                Location = new Point(160, y - 2), Size = new Size(120, 24),
                Font = UiHelpers.Body, Minimum = 0, Maximum = 999999, DecimalPlaces = 2, Value = 0,
            };
            tab.Controls.Add(amount);

            tab.Controls.Add(UiHelpers.MakeLabel("Status", 300, y));
            var billStatus = new ComboBox
            {
                Location = new Point(360, y - 2), Size = new Size(140, 24),
                Font = UiHelpers.Body, DropDownStyle = ComboBoxStyle.DropDownList,
            };
            billStatus.Items.AddRange(new object[] { "Unpaid", "Partial", "Paid" });
            billStatus.SelectedIndex = 0;
            tab.Controls.Add(billStatus); y += 34;

            tab.Controls.Add(UiHelpers.MakeButton("Save Bill (FR-16)", 160, y, 180, (s, e) =>
            {
                if (cbo.SelectedItem is not IdItem patient) { UiHelpers.Error("Select a patient first."); return; }
                int id = _billRepo.AddBilling(new Billing
                {
                    PatientID     = patient.Id,
                    TotalAmount   = (double)amount.Value,
                    PaymentStatus = (string)billStatus.SelectedItem,
                });
                if (id > 0)
                {
                    _audit.LogStaff(_me.StaffID, $"Created bill #{id} for patient {patient.Id} (FR-16)");
                    UiHelpers.Info($"Bill #{id} created.");
                    amount.Value = 0;
                    RefreshGrids();
                }
                else UiHelpers.Error("Could not create bill.");
            }));

            // ---------- Record Payment ----------
            y += 44;
            tab.Controls.Add(UiHelpers.MakeLabel("Record Payment", 10, y, 200, true)); y += 26;

            tab.Controls.Add(UiHelpers.MakeLabel("Billing ID", 10, y));
            var billingId = new NumericUpDown
            {
                Location = new Point(120, y - 2), Size = new Size(90, 24),
                Font = UiHelpers.Body, Minimum = 1, Maximum = 999999, Value = 1,
            };
            tab.Controls.Add(billingId);

            tab.Controls.Add(UiHelpers.MakeLabel("Amount ($)", 230, y));
            var payAmount = new NumericUpDown
            {
                Location = new Point(320, y - 2), Size = new Size(120, 24),
                Font = UiHelpers.Body, Minimum = 0, Maximum = 999999, DecimalPlaces = 2, Value = 0,
            };
            tab.Controls.Add(payAmount);

            tab.Controls.Add(UiHelpers.MakeLabel("Method", 460, y));
            var method = new ComboBox
            {
                Location = new Point(520, y - 2), Size = new Size(140, 24),
                Font = UiHelpers.Body, DropDownStyle = ComboBoxStyle.DropDownList,
            };
            method.Items.AddRange(new object[] { "Card", "Cash", "Insurance", "Check" });
            method.SelectedIndex = 0;
            tab.Controls.Add(method); y += 34;

            // Wire up bill grid row selection to auto-fill billing ID
            bills.SelectionChanged += (s, e) =>
            {
                if (bills.CurrentRow?.DataBoundItem is Billing b)
                    billingId.Value = b.BillingID;
            };

            tab.Controls.Add(UiHelpers.MakeButton("Record Payment (FR-16)", 120, y, 200, (s, e) =>
            {
                if (cbo.SelectedItem is not IdItem patient) { UiHelpers.Error("Select a patient first."); return; }
                if (payAmount.Value <= 0) { UiHelpers.Error("Payment amount must be greater than zero."); return; }
                int id = _payRepo.AddPayment(new Payment
                {
                    BillingID     = (int)billingId.Value,
                    PatientID     = patient.Id,
                    AmountPaid    = (double)payAmount.Value,
                    PaymentDate   = DateTime.Today,
                    PaymentMethod = (string)method.SelectedItem,
                });
                if (id > 0)
                {
                    _audit.LogStaff(_me.StaffID, $"Recorded payment #{id} for patient {patient.Id} (FR-16)");
                    UiHelpers.Info($"Payment #{id} recorded.");
                    payAmount.Value = 0;
                    RefreshGrids();
                }
                else UiHelpers.Error("Could not record payment — check that the Billing ID is valid.");
            }));

            tab.Controls.Add(cbo);
            tab.Controls.Add(balance);
            return tab;
        }

        private TabPage BuildAllAppointmentsTab()
        {
            var tab = new TabPage("All Appointments");
            var grid = UiHelpers.MakeGrid(0, 0, 0, 0); grid.Dock = DockStyle.Fill;
            grid.DataSource = _aptRepo.GetAll();
            tab.Controls.Add(grid);
            return tab;
        }

        public class IdItem
        {
            public int Id { get; }
            private readonly string _label;
            public IdItem(int id, string label) { Id = id; _label = label; }
            public override string ToString() => _label;
        }
    }
}
