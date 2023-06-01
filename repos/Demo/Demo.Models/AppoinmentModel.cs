using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Models
{
    public class AppoinmentModel
    {
        [Key]
        public int AppoinmentId { get; set; }
        [Required(ErrorMessage = "Patient Name is required.")]
        public string PatientName { get; set; }
        [Required(ErrorMessage = "Doctor Name is required.")]
        public string DoctorName { get; set;}
        public string? status { get; set; }
        public string? Description { get; set; }
        public DateTime AppointmentDate { get; set; }
        [Required(ErrorMessage = "Phone no is required")]
        [RegularExpression("[0-9]{10}$", ErrorMessage = "Invalid Mobile.no")]
        public long phoneno { get; set; }

      //  [Required(AllowEmptyStrings = false, ErrorMessage = "Please Enter Email")]
     //   [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,3}$", ErrorMessage = "Please Provide Valid Email")]
        public string Email { get; set; }
    }
}
