using System;
using System.Collections.Generic;

namespace VisitControl
{
    [Serializable]
    public class Visitor
    {
        public string Dni { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
        public string PhotoPath { get; set; }
        public bool IsDenied { get; set; }
        public string DenialReason { get; set; }
        public DateTime? DenialFrom { get; set; }
        public DateTime? DenialUntil { get; set; }
        public bool Active { get; set; }

        public Visitor()
        {
            Active = true;
            Role = "Visitante";
            DenialReason = String.Empty;
            PhotoPath = String.Empty;
        }

        public bool IsDeniedOn(DateTime date)
        {
            if (!IsDenied) return false;
            DateTime day = date.Date;
            if (DenialFrom.HasValue && day < DenialFrom.Value.Date) return false;
            if (DenialUntil.HasValue && day > DenialUntil.Value.Date) return false;
            return true;
        }
    }

    [Serializable]
    public class AttendanceRecord
    {
        public string Dni { get; set; }
        public DateTime Time { get; set; }
        public string Type { get; set; }
        public string Method { get; set; }
        public string Result { get; set; }
        public string Note { get; set; }
    }

    [Serializable]
    public class LocalData
    {
        public List<Visitor> Visitors { get; set; }
        public List<AttendanceRecord> Records { get; set; }

        public LocalData()
        {
            Visitors = new List<Visitor>();
            Records = new List<AttendanceRecord>();
        }
    }
}
