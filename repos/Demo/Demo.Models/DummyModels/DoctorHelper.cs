using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Models.DummyModels
{
    public class DoctorHelper
    {
        [Key]
        [Column("DoctorID")]
        public int DoctorID { get; set; }

        [Required(ErrorMessage = "Enter Doctor Name")]
        [Column("DoctorName")]
        public string DoctorName { get; set; }

        public int HospitalId { get; set; }

        [Required(ErrorMessage = "Select Status")]
        [Column("blnActive")]
        public bool blnActive { get; set; }

        public int DoctorTypeId { get; set; }
        public int UserId { get; set; }
        
    }
}
