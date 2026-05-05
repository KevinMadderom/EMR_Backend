using System;
using System.Drawing;
using System.Windows.Forms;
using EMR.Backend.Models;
using EMR.Backend.Repositories;
using EMR.Backend.Services;

namespace EMR.Backend.UI
{
    /// <summary>
    /// FR-10 .. FR-14 -- doctor portal. Pick a patient on the left, then act
    /// on them in the right-hand panel.
    /// </summary>
    public class DoctorDashboardForm : Form
    {
        private readonly Staff _me = Session.CurrentStaff;
        private readonly PatientRepository       _patientRepo = new PatientRepository();
        private readonly MedicalRecordRepository _recordsRepo = new MedicalRecordRepository();
        private readonly AppointmentRepository   _aptRepo     = new AppointmentRepository();
        private readonly PrescriptionRepository  _rxRepo      = new PrescriptionRepository();
        private readonly LabResultRepository     _labRepo     = new LabResultRepository();
        private readonly MedicationRepository    _medRepo     = new MedicationRepository();
        private readonly AllergyRepository       _allergyRepo = new AllergyRepository();
        private readonly ImmunizationRepository  _immunRepo   = new ImmunizationRepository();
        private readonly AuditLogger             _audit       = new AuditLogger();

        private ListBox _patientList;
        private TabControl _tabs;
        private Label _selectedLabel;
        private Patient _selected;

        public DoctorDashboardForm()
        {
            Text = $"EMRKS - Doctor Portal ({Session.DisplayName})";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1200, 720);
            MinimumSize = new Size(1000, 600);

            // Top bar
            var top = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.FromArgb(245, 247, 250) };
            top.Controls.Add(new Label
            {
                Text = $"Dr. {_me.LastName} -- Doctor Portal",
                Location = new Point(16, 16),
                Size = new Size(700, 30),
                Font = UiHelpers.Title,
            });
            top.Controls.Add(UiHelpers.MakeButton("My Appointments", 720, 14, 200, (_, __) => ShowMyAppointments()));
            top.Controls.Add(UiHelpers.MakeButton("Logout", 1060, 14, 110, (_, __) => Close()));

            // Left: patient search/list.
            // WinForms docks children in REVERSE Z-order: the LAST control
            // added is docked FIRST and claims its slice first. So inside the
            // left panel we add the Fill (list) first, then the Top docks
            // in visual bottom-to-top order (search, then header).
            var left = new Panel { Dock = DockStyle.Left, Width = 300, Padding = new Padding(10) };
            _patientList = new ListBox { Dock = DockStyle.Fill, Font = UiHelpers.Body };
            var search = new TextBox { Dock = DockStyle.Top, Font = UiHelpers.Body };
            search.PlaceholderText = "Search by name or email...";
            var patientsHeader = new Label { Text = "Patients", Font = UiHelpers.Header, Dock = DockStyle.Top, Height = 28 };
            left.Controls.Add(_patientList);   // Fill first
            left.Controls.Add(search);         // Top, sits above the list
            left.Controls.Add(patientsHeader); // Top, added last so it sits at the very top

            ReloadPatients(null);
            search.TextChanged += (s, e) => ReloadPatients(search.Text);
            _patientList.SelectedIndexChanged += (s, e) => OnPatientChanged();

            // Right: tabs (will be (re)populated when a patient is picked)
            _selectedLabel = new Label
            {
                Text = "Select a patient on the left to begin.",
                Dock = DockStyle.Top,
                Height = 30,
                Font = UiHelpers.Header,
                Padding = new Padding(10, 6, 0, 0),
            };
            _tabs = UiHelpers.MakeTabControl();

            // Same rule at the form level: Fill first, then Top/Left docks in
            // visual bottom-to-top order. The last-added control (top bar) is
            // docked first and carves out its 60 px before `left` claims the
            // left edge -- without this ordering the patient list gets clipped
            // behind the top bar.
            Controls.Add(_tabs);            // Fill -- claims leftover
            Controls.Add(_selectedLabel);   // Top, 30 px, sits above _tabs in the right column
            Controls.Add(left);             // Left, 300 px, claimed before top
            Controls.Add(top);              // Top, 60 px -- added last so it's docked first
        }

        private void ReloadPatients(string filter)
        {
            _patientList.Items.Clear();
            var rows = string.IsNullOrWhiteSpace(filter)
                ? _patientRepo.GetAllPatients()
                : _patientRepo.SearchPatients(filter);
            foreach (var p in rows)
                _patientList.Items.Add(new PatientItem(p));
        }

        private void OnPatientChanged()
        {
            if (_patientList.SelectedItem is PatientItem item)
            {
                _selected = item.Patient;
                _selectedLabel.Text = $"Patient: {_selected.FirstName} {_selected.LastName}  (ID #{_selected.PatientID})";
                _audit.LogStaff(_me.StaffID, $"Doctor opened patient {_selected.PatientID}");
                BuildPatientTabs();
            }
        }

        private void BuildPatientTabs()
        {
            _tabs.TabPages.Clear();
            _tabs.TabPages.Add(BuildPrescriptionsTab());
            _tabs.TabPages.Add(BuildLabsTab());
            _tabs.TabPages.Add(BuildChronicTab());
            _tabs.TabPages.Add(BuildAllergiesTab());
            _tabs.TabPages.Add(BuildImmunizationsTab());
        }

        // FR-10
        // NOT IN USE
        private TabPage BuildRecordsTab()
        {
            var tab = new TabPage("Records");
            var grid = UiHelpers.MakeGrid(0, 0, 0, 0); grid.Dock = DockStyle.Fill;
            grid.DataSource = _recordsRepo.GetByPatient(_selected.PatientID);
            tab.Controls.Add(grid);
            return tab;
        }

        // §4.2: Doctor adds a new medical record.
        // NOT IN USE
        private TabPage BuildAddRecordTab()
        {
            var tab = new TabPage("Add Record");
            int y = 16;
            tab.Controls.Add(UiHelpers.MakeLabel("Diagnosis", 20, y));
            var diag = new TextBox { Location = new Point(160, y - 2), Size = new Size(700, 24), Font = UiHelpers.Body };
            tab.Controls.Add(diag);
            y += 40;

            tab.Controls.Add(UiHelpers.MakeLabel("Record type", 20, y));
            var type = new ComboBox { Location = new Point(160, y - 2), Size = new Size(300, 24), Font = UiHelpers.Body, DropDownStyle = ComboBoxStyle.DropDownList };
            type.Items.AddRange(new object[] { "Consultation", "Lab Result", "Imaging", "Prescription", "Chronic - Diabetes", "Chronic - Hypertension", "Chronic - Other" });
            type.SelectedIndex = 0;
            tab.Controls.Add(type);
            y += 40;

            tab.Controls.Add(UiHelpers.MakeLabel("Notes", 20, y));
            var notes = new TextBox
            {
                Location = new Point(160, y - 2),
                Size = new Size(800, 220),
                Font = UiHelpers.Body,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
            };
            tab.Controls.Add(notes);
            y += 240;

            tab.Controls.Add(UiHelpers.MakeButton("Save Record", 160, y, 160, (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(diag.Text)) { UiHelpers.Error("Diagnosis is required."); return; }
                int id = _recordsRepo.AddRecord(new MedicalRecord
                {
                    PatientID    = _selected.PatientID,
                    StaffID      = _me.StaffID,
                    DateCreated  = DateTime.Today,
                    DateModified = DateTime.Today,
                    Diagnosis    = diag.Text.Trim(),
                    Notes        = notes.Text.Trim(),
                    RecordType   = (string)type.SelectedItem,
                });
                if (id > 0)
                {
                    _audit.LogStaff(_me.StaffID, $"Added MedicalRecord #{id} for patient {_selected.PatientID}");
                    UiHelpers.Info("Record saved.");
                    BuildPatientTabs();
                }
                else UiHelpers.Error("Could not save record.");
            }));
            return tab;
        }

        // FR-12
        private TabPage BuildPrescriptionsTab()
        {
            var tab = new TabPage("Prescriptions");

            var grid = UiHelpers.MakeGrid(10, 10, 0, 240);
            grid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grid.DataSource = _rxRepo.GetByPatient(_selected.PatientID);
            tab.Controls.Add(grid);

            int y = 260;
            tab.Controls.Add(UiHelpers.MakeLabel("Write a Prescription", 10, y, 300, true)); y += 28;

            tab.Controls.Add(UiHelpers.MakeLabel("Medication", 10, y));
            var cboMed = new ComboBox
            {
                Location = new Point(140, y - 2), Size = new Size(200, 24),
                Font = UiHelpers.Body, DropDownStyle = ComboBoxStyle.DropDownList,
            };
            foreach (var m in _medRepo.GetAll())
                cboMed.Items.Add(new MedItem(m));
            if (cboMed.Items.Count > 0) cboMed.SelectedIndex = 0;
            tab.Controls.Add(cboMed); y += 34;

            tab.Controls.Add(UiHelpers.MakeLabel("Dosage", 10, y));
            var dosage = new TextBox { Location = new Point(140, y - 2), Size = new Size(200, 24), Font = UiHelpers.Body };
            tab.Controls.Add(dosage); y += 34;

            tab.Controls.Add(UiHelpers.MakeLabel("Refills", 10, y));
            var refills = new NumericUpDown
            {
                Location = new Point(140, y - 2), Size = new Size(80, 24),
                Font = UiHelpers.Body, Minimum = 0, Maximum = 12, Value = 0,
            };
            tab.Controls.Add(refills); y += 34;

            var chkPharmacy = new CheckBox
            {
                Text = "Send to pharmacy immediately",
                Location = new Point(140, y), Size = new Size(280, 24), Font = UiHelpers.Body,
            };
            tab.Controls.Add(chkPharmacy); y += 34;

            tab.Controls.Add(UiHelpers.MakeButton("Prescribe", 140, y, 180, (s, e) =>
            {
                if (cboMed.SelectedItem is not MedItem med) { UiHelpers.Error("Select a medication."); return; }
                if (string.IsNullOrWhiteSpace(dosage.Text)) { UiHelpers.Error("Dosage is required."); return; }
                int id = _rxRepo.AddPrescription(new Prescription
                {
                    PatientID        = _selected.PatientID,
                    StaffID          = _me.StaffID,
                    MedicationID     = med.Id,
                    DateIssued       = DateTime.Today,
                    Dosage           = dosage.Text.Trim(),
                    RefillsRemaining = (int)refills.Value,
                    SentToPharmacy   = chkPharmacy.Checked,
                });
                if (id > 0)
                {
                    _audit.LogStaff(_me.StaffID, $"Prescribed medication {med.Id} for patient {_selected.PatientID}");
                    UiHelpers.Info("Prescription written.");
                    BuildPatientTabs();
                }
                else UiHelpers.Error("Could not save prescription.");
            }));
            return tab;
        }

        private class MedItem
        {
            public int Id { get; }
            private readonly string _name;
            public MedItem(Medication m) { Id = m.MedicationID; _name = m.MedicationName; }
            public override string ToString() => _name;
        }

        // FR-14
        private TabPage BuildLabsTab()
        {
            var tab = new TabPage("Lab Results");

            var grid = UiHelpers.MakeGrid(10, 10, 0, 0);
            grid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grid.Size = new Size(840, 280);
            grid.DataSource = _labRepo.GetByPatient(_selected.PatientID);
            tab.Controls.Add(grid);

            int y = 310;
            tab.Controls.Add(UiHelpers.MakeLabel("Submit a new lab result", 10, y, 300, true)); y += 30;
            tab.Controls.Add(UiHelpers.MakeLabel("Test name", 10, y));
            var name = new TextBox { Location = new Point(140, y - 2), Size = new Size(300, 24), Font = UiHelpers.Body };
            tab.Controls.Add(name); y += 35;
            tab.Controls.Add(UiHelpers.MakeLabel("Result", 10, y));
            var result = new TextBox { Location = new Point(140, y - 2), Size = new Size(500, 24), Font = UiHelpers.Body };
            tab.Controls.Add(result); y += 35;
            tab.Controls.Add(UiHelpers.MakeLabel("Status", 10, y));
            var status = new ComboBox
            {
                Location = new Point(140, y - 2), Size = new Size(220, 24), Font = UiHelpers.Body,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            status.Items.AddRange(new object[] { "Final", "Preliminary", "Critical", "Normal", "Abnormal" });
            status.SelectedIndex = 0;
            tab.Controls.Add(status); y += 40;

            tab.Controls.Add(UiHelpers.MakeButton("Submit", 140, y, 160, (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(name.Text)) { UiHelpers.Error("Test name required."); return; }
                int id = _labRepo.AddLabResult(new LabResult
                {
                    PatientID = _selected.PatientID,
                    StaffID   = _me.StaffID,
                    TestName  = name.Text.Trim(),
                    TestDate  = DateTime.Today,
                    Result    = result.Text.Trim(),
                    Status    = (string)status.SelectedItem,
                });
                if (id > 0)
                {
                    _audit.LogStaff(_me.StaffID, $"Submitted LabResult #{id} for patient {_selected.PatientID}");
                    UiHelpers.Info("Lab result submitted.");
                    BuildPatientTabs();
                }
                else UiHelpers.Error("Could not submit lab result.");
            }));
            return tab;
        }

        // FR-13
        private TabPage BuildChronicTab()
        {
            var tab = new TabPage("Chronic Tracking");
            var grid = UiHelpers.MakeGrid(0, 0, 0, 0); grid.Dock = DockStyle.Fill;
            grid.DataSource = _recordsRepo.GetChronicByPatient(_selected.PatientID);
            tab.Controls.Add(grid);
            return tab;
        }

        // FR-11
        private void ShowMyAppointments()
        {
            using (var f = new SimpleGridForm(
                "My Upcoming & Past Appointments",
                _aptRepo.GetByStaff(_me.StaffID)))
            {
                _audit.LogStaff(_me.StaffID, "Viewed my appointments");
                f.ShowDialog(this);
            }
        }

        private TabPage BuildAllergiesTab()
        {
            var tab = new TabPage("Allergies");

            DataGridView grid = null;
            void Reload() { grid.DataSource = _allergyRepo.GetByPatient(_selected.PatientID); }

            grid = UiHelpers.MakeGrid(10, 10, 0, 200);
            grid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            Reload();
            tab.Controls.Add(grid);

            int y = 220;
            tab.Controls.Add(UiHelpers.MakeLabel("Add Allergy", 10, y, 200, true)); y += 28;

            tab.Controls.Add(UiHelpers.MakeLabel("Allergen", 10, y, 100, false));
            var name = new TextBox { Location = new Point(120, y - 2), Size = new Size(180, 24), Font = UiHelpers.Body };
            tab.Controls.Add(name); y += 34;

            tab.Controls.Add(UiHelpers.MakeLabel("Severity", 10, y, 100, false));
            var sev = new ComboBox
            {
                Location = new Point(120, y - 2), Size = new Size(120, 24),
                Font = UiHelpers.Body, DropDownStyle = ComboBoxStyle.DropDownList,
            };
            sev.Items.AddRange(new object[] { "Mild", "Moderate", "Severe" });
            sev.SelectedIndex = 0;
            tab.Controls.Add(sev); y += 34;

            tab.Controls.Add(UiHelpers.MakeButton("Add Allergy", 120, y, 140, (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(name.Text)) { UiHelpers.Error("Allergen name required."); return; }
                int id = _allergyRepo.AddAllergy(new Allergy
                {
                    PatientID    = _selected.PatientID,
                    AllergenName = name.Text.Trim(),
                    Severity     = (string)sev.SelectedItem,
                });
                if (id > 0) { name.Clear(); Reload(); }
                else UiHelpers.Error("Could not add allergy.");
            }));

            tab.Controls.Add(UiHelpers.MakeButton("Delete Selected", 280, y, 160, (s, e) =>
            {
                if (grid.CurrentRow?.DataBoundItem is Allergy a)
                {
                    if (!UiHelpers.Confirm($"Delete allergy '{a.AllergenName}'?")) return;
                    if (_allergyRepo.DeleteAllergy(a.AllergyID)) Reload();
                    else UiHelpers.Error("Could not delete allergy.");
                }
                else UiHelpers.Error("Select an allergy row first.");
            }));
            return tab;
        }

        private TabPage BuildImmunizationsTab()
        {
            var tab = new TabPage("Immunizations");

            DataGridView grid = null;
            void Reload() { grid.DataSource = _immunRepo.GetByPatient(_selected.PatientID); }

            grid = UiHelpers.MakeGrid(10, 10, 0, 200);
            grid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            Reload();
            tab.Controls.Add(grid);

            int y = 220;
            tab.Controls.Add(UiHelpers.MakeLabel("Record Immunization", 10, y, 260, true)); y += 28;

            tab.Controls.Add(UiHelpers.MakeLabel("Vaccine", 10, y, 100, false));
            var vaccine = new TextBox { Location = new Point(120, y - 2), Size = new Size(300, 24), Font = UiHelpers.Body };
            tab.Controls.Add(vaccine); y += 34;

            tab.Controls.Add(UiHelpers.MakeLabel("Date given", 10, y, 100, false));
            var datePicker = new DateTimePicker
            {
                Location = new Point(120, y - 2), Size = new Size(200, 24),
                Font = UiHelpers.Body, Format = DateTimePickerFormat.Short, Value = DateTime.Today,
            };
            tab.Controls.Add(datePicker); y += 34;

            tab.Controls.Add(UiHelpers.MakeButton("Record", 120, y, 160, (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(vaccine.Text)) { UiHelpers.Error("Vaccine name required."); return; }
                int id = _immunRepo.AddImmunization(new Immunization
                {
                    PatientID        = _selected.PatientID,
                    VaccineName      = vaccine.Text.Trim(),
                    DateAdministered = datePicker.Value.Date,
                    AdminStaffID     = _me.StaffID,
                });
                if (id > 0)
                {
                    _audit.LogStaff(_me.StaffID, $"Recorded immunization for patient {_selected.PatientID}");
                    vaccine.Clear();
                    Reload();
                }
                else UiHelpers.Error("Could not record immunization.");
            }));
            return tab;
        }

        private class PatientItem
        {
            public Patient Patient { get; }
            public PatientItem(Patient p) { Patient = p; }
            public override string ToString() => $"{Patient.LastName}, {Patient.FirstName}  (#{Patient.PatientID})";
        }
    }
}
