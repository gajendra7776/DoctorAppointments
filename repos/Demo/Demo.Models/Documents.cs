using System.ComponentModel.DataAnnotations;

namespace Demo.Models
{
    public class Documents
    {
        [Key]
        public int DocumentId { get; set; }
        public int AppointmentId { get; set; }

        public string? Document_name { get; set; }  

        public string? Document_type { get; set; }

        public string? Document_path { get; set; }
        public DateTime? DeletedAt { get; set; }


    }
}
