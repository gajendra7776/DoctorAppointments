using Demo.Models;
using Demo.Models.DummyModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.DataAccess.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<ManagementModel> Management_Admin { get; set; }
        public DbSet<UserModel> User { get; set; }
        public DbSet<AppointmentHelper> Patient_Appoinments { get; set; }
        public DbSet<Documents> Documents { get; set; }
      
        
        public DbSet<RoleModel> RoleType { get; set; }
       // public DbSet<DoctorDetails> DoctorDetails { get; set; }  
        public DbSet<DoctorHelper> DoctorDetails { get; set; }  
        public DbSet<Hospital> Hospital { get; set; }
        public DbSet<DoctorTypeModel> DoctorType { get; set; }  

    }
}
