using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Models
{
    public class PatientAppoinmentModel
    {
        [Key]
        public int AppointmentID { get; set; }
        [Required(ErrorMessage = "Description is required")]
        public string DiseaseDescriptions { get; set;}

        [Required(ErrorMessage = "Doctor Name is required")]
        public int DoctorID { get; set; }

        [Required(ErrorMessage = "DoctorType is required")]
        public string DoctorType { get; set; }
        [Required(ErrorMessage = "Appointment Date is required")]
        public DateTime AppointmentDate { get; set; }
        [Required(ErrorMessage = "Appointment Time is required")]
        public string AppointmentTime { get; set; }
        public string AppointmentStatus { get; set; }
        [ForeignKey("HospitalId")]
        [Required(ErrorMessage = "Hospital Name is required")]
        public int HospitalID { get; set; }
        [Required(ErrorMessage = "Doctor Name is required")]
        public string DoctorName { get; set; }
        [Required(ErrorMessage = "Hospital Name is required")]
        public string HospitalName { get; set; }   
        public string UserName { get; set; }
        public int DoctorTypeId { get; set; }
        public int PatientId { get; set; }

        public DateTime? DeletedAt { get; set; } 
    }
}
