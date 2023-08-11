using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Models.DummyModels
{
    public class PatientDetails
    {
        public int PatientId { get; set; }
        public int DoctorId{ get; set; }
        public int DoctorTypeId { get; set; }
        public int HospitalId { get; set; }
        public int AppointmentId { get; set; }
        [Required(ErrorMessage = "Appointment Date is Required")]
        public DateTime AppointmentDate { get; set; }
        [Required(ErrorMessage ="Appointment Time is Required")]
        public string? AppointmentTime { get; set; }
        public string? Description { get; set; }
        public string? PatientName { get; set; }
        public string? Approve_by { get; set; }
        public string? PatientEmail { get; set; }
        public string? DoctorName { get; set; }
        public string? DoctorEmail { get; set; }
        public string? DoctorType { get; set; }
        public string? HospitalName { get; set; }
        public string? ManagementEmail { get; set; }
        [Required(ErrorMessage = "Appointment Status is Required")]
        public string? Status { get; set; }
        public DateTime? ApproveDate { get; set; }
        public List<PatientAppoinmentModel> aps { get; set; }  
        public DateTime? DateOfBirth { get; set; }
        public int Age { get; set; }
        public List<Documents?> Documents { get; set; }
        [Required]
        public string? Prescription { get; set; }
        public string? Suggestions { get; set; }

    }
}
