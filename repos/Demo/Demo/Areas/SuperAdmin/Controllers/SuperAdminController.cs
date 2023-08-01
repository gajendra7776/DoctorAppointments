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
    [Authorize(Roles = "SuperAdmin,ManagementAdmin")]
    [CustomExceptionFilter]
    public class SuperAdminController : Controller
    {
        private readonly AppDbContext _db;
        public SuperAdminController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Managements(int id)
        {
            if (id <= 0)
            {
                return View();
            }
            var data = _db.Management_Admin.Where(model=> model.HospitalId == id).FirstOrDefault();
            var hospital = _db.Hospital.Where(x=> x.HospitalId == id).FirstOrDefault();
            var user = _db.User.Where(x=> x.UserId == data.UserId).FirstOrDefault();
            var model = new ManagementDummy();
            model.HospitalName = hospital.HospitalName;
            model.UserName = user.UserName;
            model.Email = user.Email;
            return View(model);
        }
        [HttpPost]
        public IActionResult Managements(ManagementDummy model)
        {

            var chkPageAccess = HttpContext.Session.GetInt32("UserProfile");
            if (model != null)
            {


                int result = CreateNewAdmin(model);
                if (result == 1)
                {
                    TempData["warning"] = "Selected User is Other Hospital's Admin";
                    return View(model);
                }
                else if (result == 2)
                {
                    TempData["warning"] = "Selected Hospital is InActive! ";
                    return View(model);
                }
                else if (result == 3)
                {
                    TempData["adminCreated"] = "Management Created Successfully for Selected Hospital";
                    if (chkPageAccess != null)
                    {
                        return RedirectToAction("UserProfile", "User", new { area = "User", userId = (int)chkPageAccess });
                    }
                    return RedirectToAction("ManagementDetails");
                }
                else
                {
                    if (chkPageAccess != null)
                    {
                        TempData["adminCreated"] = "Management Updated For Selected Hospital";
                        return RedirectToAction("UserProfile", "User", new { area = "User", userId = (int)chkPageAccess });
                    }
                    TempData["success"] = "Management Updated For Selected Hospital";
                }
            }
            return View(model);
        }

        public int CreateNewAdmin(ManagementDummy model)
        {
            using (SqlConnection connection = new SqlConnection(_db.Database.GetConnectionString()))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("AddEdit_Management", connection))
                {
                    var result = new SqlParameter("@result", SqlDbType.VarChar, 50);
                    result.Direction = ParameterDirection.Output;
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@HospitalName", model.HospitalName);
                    command.Parameters.AddWithValue("@UserName", model.UserName);
                    command.Parameters.Add(result);
                    command.ExecuteNonQuery();
                    if (result.Value.ToString() == "otherHAdmin")
                    {
                        return 1;
                    }
                    else if (result.Value.ToString() == "hInactive")
                    {
                        return 2;
                    }
                    else if (result.Value.ToString() == "created")
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

        public IActionResult AddEditHospital(int id)
        {
            if(id <= 0)
            {
                return View();
            }
            var hospital = _db.Hospital.Where(x=> x.HospitalId == id).FirstOrDefault();
            return View(hospital);
        }
        [HttpPost]
        public IActionResult AddEditHospital(Hospital model)
        {

            var chkPageAccess = HttpContext.Session.GetInt32("UserProfile");
            
            if (model != null && model.HospitalId != 0)
            {
                int id = model.HospitalId;
                var hospital = _db.Hospital.Where(x=> x.HospitalId == id).FirstOrDefault(); 
                if (hospital != null)
                {
                    hospital.HospitalName = model.HospitalName;
                    hospital.blnActive = model.blnActive;
                    hospital.Description = model.Description;
                    hospital.Address = model.Address;
                    _db.SaveChanges();
                    TempData["successful"] = "Hospital Edited Successfully";
                }
                
            }
            else
            {
                model.blnActive = true;
                _db.Hospital.Add(model);
                _db.SaveChanges();
                TempData["successful"] = "Hospital Added Successfully";
            }
            
            if (chkPageAccess != null)
            {
                return RedirectToAction("UserProfile", "User", new { area = "User", userId = (int)chkPageAccess });
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
                model.RoleID = 3;
                _db.User.Add(model);
                _db.SaveChanges();
                TempData["success"] = "User Added Successfully";
            }
            return RedirectToAction("Managements");
        }
        public IActionResult ManagementDetails()
        {
            var model = new List<ManagementDummy>();
            model = GetManagementDetails();
            return View(model);
        }

        public IActionResult AdminDetails(int id)
        {
            var model = new ManagementDummy();
            model = GetManagementDetailsByID(id);
            return View(model);
        }

        public ManagementDummy GetManagementDetailsByID(int id)
        {
            var model = new ManagementDummy();

            using (SqlConnection connection = new SqlConnection(_db.Database.GetConnectionString()))
            {
                SqlCommand command = new SqlCommand("Management_SelectById", connection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@ManagementId", id);

                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        model.Hospital = new Hospital();
                        model.User = new UserModel();

                        model.Hospital.HospitalName = reader["UserName"].ToString();
                        model.Hospital.Description = reader["Description"].ToString();
                        model.User.UserName = reader["UserName"].ToString();
                        model.User.Email = reader["Email"].ToString();
                        model.User.Password = reader["Password"].ToString();
                    }
                }
            }

            return model;
        }


        public List<ManagementDummy> GetManagementDetails()
        {
            List<ManagementDummy> managements = new List<ManagementDummy>();
            using (SqlConnection connection = new SqlConnection(_db.Database.GetConnectionString()))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("ManagementAdmin_SelectAll", connection);
                command.CommandType = CommandType.StoredProcedure;
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    ManagementDummy hospital = new ManagementDummy()
                    {
                        UserName = reader["UserName"].ToString(),
                        HospitalName = reader["HospitalName"].ToString(),
                        Email = reader["Email"].ToString(),
                        ManagementId = (int)reader["ManagementId"]
                    };

                    managements.Add(hospital);
                }
            }
            return managements;
        }

        

    }
}
