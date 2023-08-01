using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Models.DummyModels
{
    public class AppointmentHelper
    {
        [Key]
        public int AppointmentID { get; set; }
        public string? DiseaseDescriptions { get; set; }
        public int DoctorID { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string? AppointmentStatus { get; set; }
        public int HospitalID { get; set; }
        public int DoctorTypeId { get; set; }
        public int PatientId { get; set; }
        public string? AppoinmentTime { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int? Approved_By { get; set; }
        public int? Rejected_By { get; set; }
        public DateTime? Approved_Date { get; set;}

    }
}
