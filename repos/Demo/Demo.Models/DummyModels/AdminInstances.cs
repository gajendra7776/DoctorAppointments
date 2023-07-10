using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Models.DummyModels
{
    public class AdminInstances
    {
        public List<DoctorDetails> DoctorDetails { get; set; }
        public List<PatientAppoinmentModel> Appointments { get; set; }
        public int HospitalId { get; set; }
        public int DoctorId { get; set; }   
    }
}
