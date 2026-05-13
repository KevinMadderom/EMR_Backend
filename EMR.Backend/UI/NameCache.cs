using System;
using System.Collections.Generic;
using System.Linq;
using EMR.Backend.Repositories;

namespace EMR.Backend.UI
{
    /// <summary>
    /// Resolves PatientID / StaffID foreign keys to human-readable names for grids.
    /// Loaded lazily on first use; falls back to a one-off lookup on cache miss so
    /// rows created mid-session resolve without an explicit refresh.
    /// </summary>
    internal static class NameCache
    {
        private static readonly object _lock = new();
        private static Dictionary<int, string> _patients;
        private static Dictionary<int, string> _staff;

        private static void EnsureLoaded()
        {
            if (_patients != null && _staff != null) return;
            lock (_lock)
            {
                if (_patients == null)
                {
                    try
                    {
                        _patients = new PatientRepository().GetAllPatients()
                            .ToDictionary(p => p.PatientID, p => $"{p.LastName}, {p.FirstName}");
                    }
                    catch { _patients = new Dictionary<int, string>(); }
                }
                if (_staff == null)
                {
                    try
                    {
                        _staff = new StaffRepository().GetAllStaff()
                            .ToDictionary(s => s.StaffID, s => $"{s.LastName}, {s.FirstName}");
                    }
                    catch { _staff = new Dictionary<int, string>(); }
                }
            }
        }

        public static string PatientName(int id)
        {
            EnsureLoaded();
            if (_patients.TryGetValue(id, out var n)) return n;
            try
            {
                var p = new PatientRepository().GetPatientById(id);
                if (p != null)
                {
                    n = $"{p.LastName}, {p.FirstName}";
                    _patients[id] = n;
                    return n;
                }
            }
            catch { }
            return $"#{id}";
        }

        public static string StaffName(int id)
        {
            EnsureLoaded();
            if (_staff.TryGetValue(id, out var n)) return n;
            try
            {
                var s = new StaffRepository().GetStaffById(id);
                if (s != null)
                {
                    n = $"{s.LastName}, {s.FirstName}";
                    _staff[id] = n;
                    return n;
                }
            }
            catch { }
            return $"#{id}";
        }

        public static void Invalidate()
        {
            lock (_lock)
            {
                _patients = null;
                _staff = null;
            }
        }
    }
}
