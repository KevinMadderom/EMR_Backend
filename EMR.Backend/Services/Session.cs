using EMR.Backend.Models;

namespace EMR.Backend.Services
{
    /// <summary>
    /// Holds the currently logged-in user for the lifetime of the WinForms
    /// application. Supports NFR-06 (role-based access control) -- forms read
    /// the session role to decide which actions to expose.
    /// </summary>
    public static class Session
    {
        public static Patient CurrentPatient { get; private set; }
        public static Staff   CurrentStaff   { get; private set; }

        public static bool IsPatient => CurrentPatient != null;
        public static bool IsStaff   => CurrentStaff   != null;

        public static string Role
        {
            get
            {
                if (CurrentStaff   != null) return CurrentStaff.AuthorizationLevel ?? CurrentStaff.Role;
                if (CurrentPatient != null) return "Patient";
                return null;
            }
        }

        public static string DisplayName
        {
            get
            {
                if (CurrentStaff   != null) return $"{CurrentStaff.FirstName} {CurrentStaff.LastName} ({CurrentStaff.Role})";
                if (CurrentPatient != null) return $"{CurrentPatient.FirstName} {CurrentPatient.LastName}";
                return "Guest";
            }
        }

        public static void SetPatient(Patient p)
        {
            CurrentPatient = p;
            CurrentStaff   = null;
        }

        public static void SetStaff(Staff s)
        {
            CurrentStaff   = s;
            CurrentPatient = null;
        }

        public static void Clear()
        {
            CurrentPatient = null;
            CurrentStaff   = null;
        }

        public static bool HasRole(params string[] roles)
        {
            var r = Role;
            if (string.IsNullOrEmpty(r)) return false;
            foreach (var x in roles)
                if (string.Equals(x, r, System.StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }
    }
}
