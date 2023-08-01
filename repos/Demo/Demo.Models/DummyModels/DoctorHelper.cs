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
        public int DoctorID { get; set; }
        public string? DoctorName { get; set; }
        public int? HospitalId { get; set; }
        public bool? blnActive { get; set; }
        public int? DoctorTypeId { get; set; }
        public DateTime? Deleted_At { get; set; }
        public int? UserId { get; set; }
        
        
    }
}
