using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Models.DummyModels
{
    public class UserDetails
    {
        public int UserId { get; set; }
        [Required]
        public string? UserName { get; set; }
        [Required]
        public int? Age { get; set; }
        [Required]
        public string? PhoneNo { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please Enter Email")]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,3}$", ErrorMessage = "Email is not valid")]
        public string? Email { get; set; }   
        public string? RoleType { get; set; }
        public int RoleID { get; set; }
        public int HospitalId { get; set; }
        public int ManagementId { get; set; }
        [Required]
        public string? HospitalName { get; set; }
        [Required]
        public string? HospitalAddress { get; set; }
        [Required]
        public DateTime?  DateOfBirth { get; set; }
        public List<Management_Admin>? Managements { get; set; }
        public List<DoctorDetails>? DoctorDetails { get; set; }  
        public List<Hospital>? Hospital { get; set; }  
    }
}
