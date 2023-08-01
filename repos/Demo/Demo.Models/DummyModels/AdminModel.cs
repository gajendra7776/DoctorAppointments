using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Models.DummyModels
{
    public class AdminModel
    {
        public int ManagementId { get; set; }
        public int HospitalId { get; set; }
        public int UserId { get; set; }
        public string? DoctorName { get; set; }
        public string? HospitalName { get; set; }
        public string? DoctorType { get; set; }
        public bool Status { get; set; }
        public int DoctorId { get; set; }
        public int AppointmentID { get; set; }
        public string? DiseaseDescriptions { get; set; }
        public string? AppointmentStatus { get; set; }
    }
}
