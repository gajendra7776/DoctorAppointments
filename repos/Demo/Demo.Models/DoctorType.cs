using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Models
{
    public class DoctorType
    {
        [Key]
        public int DoctorTypeId { get; set; }
        [Required(ErrorMessage = "Doctor Type is required")]
        public string? DoctorTypeName { get; set; }
    }
}
