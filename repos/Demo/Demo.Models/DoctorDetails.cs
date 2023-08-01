using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Models
{
    public class DoctorDetails
    {
        [Key]
        [Column("DoctorID")]
        public int DoctorID { get; set; }

        [Required(ErrorMessage = "Enter Doctor Name")]
        [Column("DoctorName")]
        public string? DoctorName { get; set; }

        [Column("HospitalId")] // Remove the [ForeignKey] attribute
        public int HospitalId { get; set; }

        [ForeignKey("HospitalId")] // Add the [ForeignKey] attribute to link with Hospital model
        public Hospital? Hospital { get; set; }

        [Required(ErrorMessage = "Select Status")]
        [Column("blnActive")]
        public bool Status { get; set; }

        [Required(ErrorMessage = "Select Hospital Name")]
        [NotMapped] // Exclude from mapping as it is not a column in the database
        public string? HospitalName { get; set; }

        public List<Hospital>? HospitalList { get; set; }
        public List<DoctorType>? DoctorTypeList { get; set; }

        [Column("DoctorTypeID")] // Remove the [ForeignKey] attribute
        public int? DoctorTypeID { get; set; }
        public string? DoctorType { get; set; } = null;
        public int? UserId { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please Enter Email")]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,3}$", ErrorMessage = "Email is not valid")]
        public string? Email { get; set; }
        [Required]
        [MinLength(8, ErrorMessage = "Password Must be atleast 8 character")]
        [RegularExpression("^((?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])|(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[^a-zA-Z0-9])|(?=.*?[A-Z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])|(?=.*?[a-z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])).{8,}", ErrorMessage = "Please Enter Valid Password")]
        public string? Password { get; set; }
        [NotMapped]
        [Required(ErrorMessage = "Confirm Passowrd is required")]
        [Compare("Password", ErrorMessage = "ConfirmPassword Does Not Match")]
        public string? ConfirmPassWord { get; set; }
        [Required(ErrorMessage = "Please enter Age")]
        public int? Age { get; set; }
        [Required(ErrorMessage = "Please enter Date Of Birth")]
        public DateTime? DateOfBirth { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please Enter Phone Number")]
        [RegularExpression("[0-9]{10}$", ErrorMessage = "Invalid Mobile Number")]
        public string? PhoneNo { get; set; }

    }
}
