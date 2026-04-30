using System;
using System.Drawing;
using System.Windows.Forms;
using EMR.Backend.Models;
using EMR.Backend.Repositories;
using EMR.Backend.Services;

namespace EMR.Backend.UI
{
    /// <summary>
    /// FR-17 .. FR-19 -- admin manages users, assigns permissions, and
    /// performs simple DB-level maintenance views (audit log, schema check).
    /// </summary>
    public class AdminDashboardForm : Form
    {
        private readonly Staff _me = Session.CurrentStaff;
        private readonly StaffRepository      _staffRepo = new StaffRepository();
        private readonly DepartmentRepository _deptRepo  = new DepartmentRepository();
        private readonly AuditLogRepository   _auditRepo = new AuditLogRepository();
        private readonly AuthService          _auth      = new AuthService();
        private readonly AuditLogger          _audit     = new AuditLogger();

        public AdminDashboardForm()
        {
            Text = $"EMRKS - Admin Console ({Session.DisplayName})";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1200, 760);
            MinimumSize = new Size(1000, 640);

            var top = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.FromArgb(245, 247, 250) };
            top.Controls.Add(new Label
            {
                Text = "Admin Console",
                Location = new Point(16, 16),
                Size = new Size(700, 30),
                Font = UiHelpers.Title,
            });
            var btnLogout = UiHelpers.MakeButton("Logout", 0, 14, 110, (_, __) => Close());
            btnLogout.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnLogout.Location = new Point(top.Width - 130, 14);
            top.Resize += (s, e) => btnLogout.Location = new Point(top.Width - 130, 14);
            top.Controls.Add(btnLogout);
            Controls.Add(top);

            var tabs = UiHelpers.MakeTabControl();
            tabs.TabPages.Add(BuildUsersTab());
            tabs.TabPages.Add(BuildAddUserTab());
            tabs.TabPages.Add(BuildAuditTab());
            tabs.TabPages.Add(BuildMaintenanceTab());
            Controls.Add(tabs);

            _audit.LogStaff(_me.StaffID, "Opened admin console");
        }

        // FR-17 + FR-18: list staff, change permissions, delete.
        private TabPage BuildUsersTab()
        {
            var tab = new TabPage("Users (FR-17 / FR-18)") { Padding = new Padding(10) };

            // Bottom action row -- Dock.Bottom so it's always visible
            var actions = new Panel { Dock = DockStyle.Bottom, Height = 56, Padding = new Padding(0, 10, 0, 0) };

            var lblId = UiHelpers.MakeLabel("Selected Staff ID", 0, 14, 130);
            actions.Controls.Add(lblId);

            var idBox = new TextBox
            {
                Location = new Point(140, 12),
                Size = new Size(70, 24),
                ReadOnly = true,
                Font = UiHelpers.Body,
            };
            actions.Controls.Add(idBox);

            actions.Controls.Add(UiHelpers.MakeLabel("New permission", 230, 14, 110));
            var cboLevel = new ComboBox
            {
                Location = new Point(340, 12),
                Size = new Size(170, 24),
                Font = UiHelpers.Body,
                DropDownStyle = ComboBoxStyle.DropDownList,
            };
            cboLevel.Items.AddRange(new object[] { "Admin", "Doctor", "Staff", "Read-only" });
            cboLevel.SelectedIndex = 1;
            actions.Controls.Add(cboLevel);

            var btnUpdate = UiHelpers.MakeButton("Update permission (FR-18)", 530, 8, 220);
            actions.Controls.Add(btnUpdate);

            var btnDelete = UiHelpers.MakeButton("Delete staff (FR-17)", 760, 8, 180);
            actions.Controls.Add(btnDelete);

            // Grid fills the rest
            var grid = UiHelpers.MakeGrid(0, 0, 0, 0);
            grid.Dock = DockStyle.Fill;
            grid.DataSource = _staffRepo.GetAllStaff();

            grid.SelectionChanged += (s, e) =>
            {
                if (grid.CurrentRow?.DataBoundItem is Staff st)
                    idBox.Text = st.StaffID.ToString();
            };

            btnUpdate.Click += (s, e) =>
            {
                if (!int.TryParse(idBox.Text, out int sid)) { UiHelpers.Error("Pick a staff row."); return; }
                if (_staffRepo.UpdateAuthorizationLevel(sid, (string)cboLevel.SelectedItem))
                {
                    _audit.LogStaff(_me.StaffID, $"Set permission of staff {sid} to {cboLevel.SelectedItem} (FR-18)");
                    UiHelpers.Info("Permission updated.");
                    grid.DataSource = _staffRepo.GetAllStaff();
                }
                else UiHelpers.Error("Could not update permission.");
            };

            btnDelete.Click += (s, e) =>
            {
                if (!int.TryParse(idBox.Text, out int sid)) { UiHelpers.Error("Pick a staff row."); return; }
                if (sid == _me.StaffID) { UiHelpers.Error("You cannot delete your own admin account."); return; }
                if (!UiHelpers.Confirm("Permanently delete this staff member?")) return;
                if (_staffRepo.DeleteStaff(sid))
                {
                    _audit.LogStaff(_me.StaffID, $"Deleted staff {sid} (FR-17)");
                    UiHelpers.Info("Staff deleted.");
                    grid.DataSource = _staffRepo.GetAllStaff();
                }
                else UiHelpers.Error("Could not delete (may have related records).");
            };

            // Order matters: Bottom first, then Fill, so Fill takes the leftover space.
            tab.Controls.Add(grid);
            tab.Controls.Add(actions);
            return tab;
        }

        // FR-17: add a new staff user.
        private TabPage BuildAddUserTab()
        {
            var tab = new TabPage("Add Staff User (FR-17)") { Padding = new Padding(20) };

            int y = 16;
            TextBox firstName = null, lastName = null, role = null, pin = null, card = null,
                    username = null, password = null;
            ComboBox auth = null, dept = null;

            void Row(string label, Control input)
            {
                tab.Controls.Add(UiHelpers.MakeLabel(label, 0, y));
                input.Location = new Point(180, y - 2);
                input.Size = new Size(360, 24);
                input.Font = UiHelpers.Body;
                tab.Controls.Add(input);
                y += 36;
            }

            Row("First name *",       firstName = new TextBox());
            Row("Last name *",        lastName  = new TextBox());
            Row("Role *",             role      = new TextBox { Text = "Doctor" });
            auth = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            auth.Items.AddRange(new object[] { "Admin", "Doctor", "Staff", "Read-only" });
            auth.SelectedIndex = 1;
            Row("Authorization *",    auth);
            Row("PIN",                pin       = new TextBox());
            Row("Access card #",      card      = new TextBox());

            dept = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            foreach (var d in _deptRepo.GetAll())
                dept.Items.Add(new StaffDashboardForm.IdItem(d.DepartmentID, d.DepartmentName));
            if (dept.Items.Count > 0) dept.SelectedIndex = 0;
            Row("Department *",       dept);

            Row("Username *",         username  = new TextBox());
            password = new TextBox { UseSystemPasswordChar = true };
            Row("Initial password *", password);

            tab.Controls.Add(UiHelpers.MakeButton("Create user (FR-17)", 180, y + 10, 200, (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(firstName.Text) || string.IsNullOrWhiteSpace(lastName.Text) ||
                    string.IsNullOrWhiteSpace(username.Text)  || string.IsNullOrWhiteSpace(password.Text) ||
                    string.IsNullOrWhiteSpace(role.Text)      ||
                    auth.SelectedItem == null || dept.SelectedItem == null)
                {
                    UiHelpers.Error("Required fields are missing.");
                    return;
                }
                try
                {
                    _auth.RegisterStaff(new Staff
                    {
                        FirstName          = firstName.Text.Trim(),
                        LastName           = lastName.Text.Trim(),
                        Role               = role.Text.Trim(),
                        PIN                = pin.Text.Trim(),
                        AccessCardNumber   = card.Text.Trim(),
                        AuthorizationLevel = (string)auth.SelectedItem,
                        DepartmentID       = ((StaffDashboardForm.IdItem)dept.SelectedItem).Id,
                        Username           = username.Text.Trim(),
                    }, password.Text);

                    UiHelpers.Info("Staff user created.");
                    firstName.Clear(); lastName.Clear(); username.Clear(); password.Clear();
                    pin.Clear(); card.Clear();
                }
                catch (Exception ex)
                {
                    UiHelpers.Error("Could not create user: " + ex.Message);
                }
            }));
            return tab;
        }

        private TabPage BuildAuditTab()
        {
            var tab = new TabPage("Audit Log") { Padding = new Padding(10) };
            var grid = UiHelpers.MakeGrid(0, 0, 0, 0);
            grid.Dock = DockStyle.Fill;
            grid.DataSource = _auditRepo.GetRecent(500);
            tab.Controls.Add(grid);
            return tab;
        }

        // FR-19: a placeholder maintenance view.
        private TabPage BuildMaintenanceTab()
        {
            var tab = new TabPage("DB Maintenance (FR-19)") { Padding = new Padding(10) };
            var info = new TextBox
            {
                Multiline = true, ReadOnly = true, Dock = DockStyle.Fill, Font = UiHelpers.Body,
                ScrollBars = ScrollBars.Vertical,
                Text = string.Join(Environment.NewLine, new[]
                {
                    "Database maintenance",
                    "====================",
                    "",
                    "  - Connection: see appsettings.json",
                    "  - Schema: see SQL/00_full_schema.sql",
                    "  - Migrations: SQL/01_migration_add_auth.sql",
                    "",
                    "Common tasks (run in MySQL Workbench):",
                    "  USE EMRKS;",
                    "  SHOW TABLES;",
                    "  SELECT COUNT(*) FROM Patient;",
                    "  SELECT COUNT(*) FROM Staff;",
                    "  SELECT * FROM AuditLog ORDER BY Time_Stamp DESC LIMIT 50;",
                    "",
                    "Per FR-19, the admin is responsible for backups, indexes, and",
                    "permission grants directly on the database server.",
                }),
            };
            tab.Controls.Add(info);
            return tab;
        }
    }
}
