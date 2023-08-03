using System.ComponentModel.DataAnnotations;

namespace Demo.Models.DummyModels
{
    public class DoctorTypeModel
    {
        [Key]
        public int DoctorTypeId { get; set; }
        [Required(ErrorMessage = "Doctor Type is required")]
        public string? DoctorType { get; set; }
    }
}
