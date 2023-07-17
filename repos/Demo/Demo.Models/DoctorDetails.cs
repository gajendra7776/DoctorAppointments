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
        public string DoctorName { get; set; }

        [Column("HospitalId")] // Remove the [ForeignKey] attribute
        public int HospitalId { get; set; }

        [ForeignKey("HospitalId")] // Add the [ForeignKey] attribute to link with Hospital model
        public Hospital Hospital { get; set; }

        [Required(ErrorMessage = "Select Status")]
        [Column("blnActive")]
        public bool Status { get; set; }

        [Required(ErrorMessage = "Select Hospital Name")]
        [NotMapped] // Exclude from mapping as it is not a column in the database
        public string HospitalName { get; set; }

        public List<Hospital> HospitalList { get; set; }
        public List<DoctorType> DoctorTypeList { get; set; }

        [Column("DoctorTypeID")] // Remove the [ForeignKey] attribute
        public int DoctorTypeID { get; set; }
        public string DoctorType { get; set; } = null;
        public int UserId { get; set; }
            
    }
}
