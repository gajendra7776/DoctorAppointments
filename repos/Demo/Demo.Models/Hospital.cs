using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Models
{
    public class Hospital  
    {
        [Key]
        public int HospitalId { get; set; }
        [Required(ErrorMessage = "Hospital Name is required")]
        public string HospitalName { get; set;}
        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }
        public bool blnActive { get; set;}
        
         ICollection<DoctorDetails> DoctorDetails { get; set; }
        
    }
}
