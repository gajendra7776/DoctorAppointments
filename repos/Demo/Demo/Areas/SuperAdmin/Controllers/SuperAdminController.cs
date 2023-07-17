using Demo.DataAccess.Data;
using Demo.Models;
using Demo.Models.DummyModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq;
using static Demo.Controllers.ErrorController;

namespace Demo.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")]
    [Authorize(Roles = "SuperAdmin")]
    [CustomExceptionFilter]
    public class SuperAdminController : Controller
    {
        private readonly AppDbContext _db;
        public SuperAdminController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Managements()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Managements(ManagementDummy model)
        {
            if(model != null)
            {
               int result =  CreateNewAdmin(model);
                if (result == 1)
                {
                    TempData["warning"] = "Selected Hospital Already Have Management";
                }
                else if (result ==2)
                {
                    TempData["warning"] = "Selected User is Management Of Other Hospital";
                }
                else if (result == 3)
                {
                    TempData["success"] = "Management Created Successfully for Selected Hospital";
                }
                else
                {
                    TempData["warning"] = "Selected User is Not Management User";
                }
            }
            return View();
        }

        public int CreateNewAdmin(ManagementDummy model)
        {
            using (SqlConnection connection = new SqlConnection(_db.Database.GetConnectionString()))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("Admin_CreateForHospital", connection))
                {
                    var result = new SqlParameter("@result", SqlDbType.VarChar, 50);
                    result.Direction = ParameterDirection.Output;
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@HospitalName", model.HospitalName);
                    command.Parameters.AddWithValue("@UserName", model.UserName);
                    command.Parameters.Add(result);
                    command.ExecuteNonQuery();
                    if (result.Value.ToString() == "haveAdmin")
                    {
                        return 1;
                    }
                    else if(result.Value.ToString() == "otherHAdmin")
                    {
                        return 2;
                    }
                    else if(result.Value.ToString() == "created")
                    {
                        return 3;
                    }
                    else
                    {
                        return 4;
                    }
                }
            }
        }

        public IActionResult AddEditHospital()
        {
            return View();
        }
        [HttpPost]
        public IActionResult AddEditHospital(Hospital model)
        {
            if (model != null)
            {
                _db.Hospital.Add(model);
                _db.SaveChanges();
                TempData["success"] = "Hospital Added Successfully";
            }
            return RedirectToAction("Managements");
        }
        public IActionResult AddUser()
        {
            return View();
        }
        [HttpPost]
        public IActionResult AddUser(UserModel model)
        {
            if (model != null)
            {

                _db.User.Add(model);
                _db.SaveChanges();
                TempData["success"] = "User Added Successfully";
            }
            return RedirectToAction("Managements");
        }
    }
}
