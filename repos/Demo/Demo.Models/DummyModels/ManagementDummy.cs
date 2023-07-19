using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Models.DummyModels
{
    public  class ManagementDummy
    {
        public int ManagementId { get; set; }
        public string UserName { get; set; }
        public string HospitalName { get; set; }
        public string Email{ get; set; }
        public Hospital Hospital { get; set; }
        public UserModel User { get; set; }
    }
}
