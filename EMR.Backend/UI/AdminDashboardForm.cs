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

        private static readonly Color NavActive   = Color.FromArgb(45, 90, 160);
        private static readonly Color NavInactive = Color.FromArgb(100, 130, 180);

        public AdminDashboardForm()
        {
            Text = $"EMRKS - Admin Console ({Session.DisplayName})";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1200, 760);
            MinimumSize = new Size(1000, 640);

            // ── Content host ───────────────────────────────────────────────
            var host = new Panel { Dock = DockStyle.Fill };
            Controls.Add(host);

            // ── Navigation button bar ──────────────────────────────────────
            var navBar = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = NavInactive };
            Controls.Add(navBar);

            // ── Header bar ────────────────────────────────────────────────
            var header = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.FromArgb(245, 247, 250) };
            header.Controls.Add(new Label
            {
                Text = "Admin Console",
                Location = new Point(16, 16),
                Size = new Size(700, 30),
                Font = UiHelpers.Title,
            });
            var btnLogout = UiHelpers.MakeButton("Logout", 0, 14, 110, (_, __) => Close());
            btnLogout.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnLogout.Location = new Point(header.Width - 130, 14);
            header.Resize += (s, e) => btnLogout.Location = new Point(header.Width - 130, 14);
            header.Controls.Add(btnLogout);
            Controls.Add(header);

            // Build content panels
            var pUsers  = BuildUsersPanel();
            var pAdd    = BuildAddUserPanel();
            var pAudit  = BuildAuditPanel();
            var pMaint  = BuildMaintenancePanel();

            foreach (var p in new[] { pUsers, pAdd, pAudit, pMaint })
            {
                p.Dock = DockStyle.Fill;
                p.Visible = false;
                host.Controls.Add(p);
            }
            pUsers.Visible = true;

            // Nav buttons
            string[] labels = { "Users  (FR-17/18)", "Add Staff  (FR-17)", "Audit Log", "DB Maintenance" };
            Panel[] pages   = { pUsers, pAdd, pAudit, pMaint };
            Button[] navBtns = new Button[4];

            void Activate(int idx)
            {
                for (int i = 0; i < 4; i++)
                {
                    pages[i].Visible   = i == idx;
                    navBtns[i].BackColor = i == idx ? NavActive : NavInactive;
                    navBtns[i].Font    = i == idx ? UiHelpers.Header : UiHelpers.Body;
                }
            }

            int bx = 0;
            for (int i = 0; i < 4; i++)
            {
                int idx = i;
                var btn = new Button
                {
                    Text      = labels[i],
                    Location  = new Point(bx, 0),
                    Size      = new Size(220, 44),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = i == 0 ? NavActive : NavInactive,
                    ForeColor = Color.White,
                    Font      = i == 0 ? UiHelpers.Header : UiHelpers.Body,
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.Click += (s, e) => Activate(idx);
                navBar.Controls.Add(btn);
                navBtns[i] = btn;
                bx += 220;
            }

            _audit.LogStaff(_me.StaffID, "Opened admin console");
        }

        // FR-17 + FR-18: list staff, change permissions, delete.
        private Panel BuildUsersPanel()
        {
            var panel = new Panel { Padding = new Padding(10) };

            var actions = new Panel { Dock = DockStyle.Bottom, Height = 56, Padding = new Padding(0, 10, 0, 0) };

            var idBox = new TextBox { Location = new Point(140, 12), Size = new Size(70, 24), ReadOnly = true, Font = UiHelpers.Body };
            actions.Controls.Add(UiHelpers.MakeLabel("Selected Staff ID", 0, 14, 140));
            actions.Controls.Add(idBox);
            actions.Controls.Add(UiHelpers.MakeLabel("New permission", 230, 14, 110));

            var cboLevel = new ComboBox
            {
                Location = new Point(340, 12), Size = new Size(170, 24),
                Font = UiHelpers.Body, DropDownStyle = ComboBoxStyle.DropDownList,
            };
            cboLevel.Items.AddRange(new object[] { "Admin", "Doctor", "Staff", "Read-only" });
            cboLevel.SelectedIndex = 1;
            actions.Controls.Add(cboLevel);

            var btnUpdate = UiHelpers.MakeButton("Update permission (FR-18)", 530, 8, 220);
            var btnDelete = UiHelpers.MakeButton("Delete staff (FR-17)",      760, 8, 180);
            actions.Controls.Add(btnUpdate);
            actions.Controls.Add(btnDelete);

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

            panel.Controls.Add(grid);
            panel.Controls.Add(actions);
            return panel;
        }

        // FR-17: add a new staff user.
        private Panel BuildAddUserPanel()
        {
            var panel = new Panel { Padding = new Padding(20) };

            int y = 10;
            TextBox firstName = null, lastName = null, role = null, pin = null, card = null,
                    username = null, password = null;
            ComboBox auth = null, dept = null;

            void Row(string label, Control input)
            {
                panel.Controls.Add(UiHelpers.MakeLabel(label, 0, y));
                input.Location = new Point(180, y - 2);
                input.Size = new Size(360, 24);
                input.Font = UiHelpers.Body;
                panel.Controls.Add(input);
                y += 38;
            }

            Row("First name *",       firstName = new TextBox());
            Row("Last name *",        lastName  = new TextBox());
            Row("Role *",             role      = new TextBox { Text = "Doctor" });
            auth = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            auth.Items.AddRange(new object[] { "Admin", "Doctor", "Staff", "Read-only" });
            auth.SelectedIndex = 1;
            Row("Authorization *",    auth);
            Row("PIN",                pin  = new TextBox());
            Row("Access card #",      card = new TextBox());

            dept = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            foreach (var d in _deptRepo.GetAll())
                dept.Items.Add(new StaffDashboardForm.IdItem(d.DepartmentID, d.DepartmentName));
            if (dept.Items.Count > 0) dept.SelectedIndex = 0;
            Row("Department *",       dept);

            Row("Username *",         username = new TextBox());
            password = new TextBox { UseSystemPasswordChar = true };
            Row("Initial password *", password);

            panel.Controls.Add(UiHelpers.MakeButton("Create user (FR-17)", 180, y + 10, 200, (s, e) =>
            {
                var missing = new System.Collections.Generic.List<string>();
                if (string.IsNullOrWhiteSpace(firstName.Text)) missing.Add("First name");
                if (string.IsNullOrWhiteSpace(lastName.Text))  missing.Add("Last name");
                if (string.IsNullOrWhiteSpace(role.Text))      missing.Add("Role");
                if (auth.SelectedItem == null)                 missing.Add("Authorization");
                if (dept.SelectedItem == null)                 missing.Add("Department");
                if (string.IsNullOrWhiteSpace(username.Text))  missing.Add("Username");
                if (string.IsNullOrWhiteSpace(password.Text))  missing.Add("Initial password");
                if (missing.Count > 0)
                {
                    UiHelpers.Error("Required fields are missing:\n• " + string.Join("\n• ", missing));
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
            return panel;
        }

        private Panel BuildAuditPanel()
        {
            var panel = new Panel { Padding = new Padding(10) };
            var grid = UiHelpers.MakeGrid(0, 0, 0, 0);
            grid.Dock = DockStyle.Fill;
            grid.DataSource = _auditRepo.GetRecent(500);
            panel.Controls.Add(grid);
            return panel;
        }

        private Panel BuildMaintenancePanel()
        {
            var panel = new Panel { Padding = new Padding(10) };
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
            panel.Controls.Add(info);
            return panel;
        }
    }
}
