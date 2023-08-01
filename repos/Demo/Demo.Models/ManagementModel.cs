﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Models
{
    public class ManagementModel
    {
        [Key]
        public int ManagementId { get; set; }
        public int HospitalId { get; set; }
        public int UserId { get; set; }
        public bool? blnActive { get; set; } 
      

    }
}
