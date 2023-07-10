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
        public string DiseaseDescriptions { get; set; }
        [ForeignKey("DoctorId")]
        public int DoctorID { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string AppointmentStatus { get; set; }
        [ForeignKey("HospitalId")]
        public int HospitalID { get; set; }
        public int DoctorTypeId { get; set; }
        public int PatientId { get; set; }
        [Required(ErrorMessage = "Appointment Time is required")]
        public string AppoinmentTime { get; set; }
        public DateTime DeletedAt { get; set; }
    }
}
