using System.ComponentModel.DataAnnotations;

namespace Demo.Models
{
    public class Hospital
    {
        [Key]
        public int HospitalId { get; set; }
        [Required(ErrorMessage = "Hospital Name is required")]
        public string? HospitalName { get; set; }
        [Required(ErrorMessage = "Description is required")]
        public string? Description { get; set; }
        public bool blnActive { get; set; }
        public int? HospitalHostDetails { get; set; }
        public string? Address { get; set; }


    }
}
