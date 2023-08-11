using Demo.DataAccess.Common;
using Demo.DataAccess.Data;
using Demo.Models;
using Demo.Models.DummyModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using static Demo.Controllers.ErrorController;

namespace Demo.Controllers
{
    [Area("User")]
    [Authorize(Roles = "Doctor,ManagementAdmin,User,SuperAdmin")]
    [CustomExceptionFilter]
    public class UserController : Controller
    {
        private readonly ManagementMethods _common;
        private readonly CommonMethods _commonmethods;
        private readonly AppDbContext _db;
        public UserController(AppDbContext db, ManagementMethods common, CommonMethods commonmethods)
        {
            _db = db;
            _common = common;
            _commonmethods = commonmethods;
        }


        public IActionResult CreateAppointments(int id)
        {
            if (id <= 0)
            {
                return View();
            }
            PatientAppoinmentModel model = new PatientAppoinmentModel();
            model.PatientId = id;
            return View(model);
        }
        [HttpPost]
        public IActionResult CreateAppointments(PatientAppoinmentModel model)
        {
            if (model == null)
            {
                return View();
            }
            int result = _common.CreateNewAppoinment(model);
            if (User.IsInRole("User") && result == 1)
            {
                TempData["success"] = "Appoinment Added Successfully";
                return RedirectToAction("BookAppoint", "User", new { area = "User", userId = model.PatientId });
            }
            else if (User.IsInRole("User") && result == 0)
            {
                TempData["error"] = "Slot is not Available, Select Different Slot";
                return RedirectToAction("CreateAppointments", "User", new { area = "User", id = model.PatientId });
            }
            return View();
        }
        //28 June - Create BookApoinment Backend
        public IActionResult BookAppoint(int userId)
        {
            if (userId == 0)
            {
                return View();
            }
            try
            {
                List<PatientAppoinmentModel> model = new List<PatientAppoinmentModel>();
                model = _common.GetUserAppoinmentData(userId);
                ViewBag.UserId = userId;

                return View(model);
            }
            catch (Exception ex)
            {
                return View(ex.Message);
            }
        }
        public IActionResult AppointmentHistory(int userId)
        {
            List<PatientAppoinmentModel> model= GetCompletedAppointments(userId);
            return View(model);
        }

        public IActionResult Prescription(int appId)
        {
           
                if (appId <= 0)
                {
                    return View();
                }
               
                PatientDetails appointmentById = new PatientDetails();
                appointmentById = _commonmethods.GetPatientDetailsByAppointmentId(appId);
               
                var docs = _db.Documents.Where(x => x.AppointmentId == appId && x.DeletedAt == null).ToList();
                //var fileNames = Directory.GetFiles(wwwrootDirectory, "*").Select(Path.GetFileName).ToList();
                var doc = new List<Documents>();
                foreach (var d in docs)
                {
                    var doctype = new Documents();
                    doctype.Document_name = d.Document_name;
                    string extension = Path.GetExtension(d.Document_name);
                    doctype.Document_type = extension;
                    doctype.Document_path = d.Document_path;
                    doctype.AppointmentId = d.AppointmentId;

                    doc.Add(doctype);
                }
                appointmentById.Documents = doc;

                return View(appointmentById);
          
        }

        // 29 June - Delete Appoinment
        public IActionResult DeleteAppoinment(int id, int userId)
        {
            if (id <= 0)
            {
                return View();
            }
            _common.RemoveAppoinment(id);
            TempData["success"] = "Appointment Deleted Successfully";
            return RedirectToAction("BookAppoint", new { userId = userId });
        }



        public JsonResult GetHospitals()
        {
            List<Hospital> hospitals = new List<Hospital>();
            if (User.IsInRole("User"))
            {

                var hospital = _db.Hospital.Where(x => x.blnActive == true && _db.Management_Admin.Any(m => m.HospitalId == x.HospitalId && m.blnActive == true)).ToList();

                return Json(hospital);
            }
            hospitals = _common.GetHospitalList();
            return Json(hospitals);
        }


        public JsonResult GetFilterHospitals()
        {
            List<Hospital> hospitals = new List<Hospital>();
            hospitals = _common.GetHospitalListFiltered();
            return Json(hospitals);
        }
        public JsonResult GetUsers()
        {
            var users = _db.User.Where(m => m.RoleID == 1).ToList();
            return Json(users);
        }

        public JsonResult GetAdmins()
        {
            //var users = _db.User
            //.Where(u => u.RoleID == 3 && !_db.Management_Admin.Any(m => m.UserId == u.UserId && m.blnActive == true))
            //.ToList();
            var users = _db.User
            .Where(u => u.RoleID == 3)
            .ToList();
            return Json(users);
        }

        public JsonResult GetEmail(string userId)
        {
            var users = _db.User.Where(m => m.UserName == userId).Select(m => m.Email).FirstOrDefault();
            return Json(users);
        }

        public JsonResult GetDoctorTypes(int hospitalId)
        {

            List<DoctorType> doctorTypes = new List<DoctorType>();
            doctorTypes = _common.GetDoctorTypeList(hospitalId);
            return Json(doctorTypes);
        }

        public JsonResult GetDoctors(int doctorTypeId, int hospitalId)
        {
            List<DoctorDetails> doctors = new List<DoctorDetails>();
            doctors = _common.GetDoctorList(doctorTypeId, hospitalId);
            return Json(doctors);
        }
        public JsonResult GetDoctorTypeListAll()
        {
            List<DoctorType> dt = new List<DoctorType>();
            dt = _common.GetDoctorTypeListAll();

            return Json(dt);
        }

        public IActionResult UserProfile(int userId)
        {
            var userData = _db.User.Where(x => x.UserId == userId).Select(u => new UserDetails
            {
                UserId = u.UserId,
                UserName = u.UserName,
                RoleID = u.RoleID,
                Age = u.Age,
                DateOfBirth = u.DateOfBirth,
                Email = u.Email,
                PhoneNo = u.PhoneNo
            }).FirstOrDefault();
            if (userData != null)
            {
                if (userData.RoleID == 1)
                {
                    userData.RoleType = "Normal user";
                }

                else if (userData.RoleID == 2)
                {
                    userData.RoleType = "Doctor";
                    var doctorData = _db.DoctorDetails.Where(x => x.UserId == userId).FirstOrDefault();
                    if (doctorData != null)
                    {
                        var hospitalData = _db.Hospital.Where(x => x.HospitalId == doctorData.HospitalId).FirstOrDefault();

                        var managementData = _db.Management_Admin.Where(x => x.HospitalId == doctorData.HospitalId).FirstOrDefault();
                        if (managementData != null)
                        {
                            userData.ManagementId = managementData.ManagementId;
                        }
                        if (hospitalData != null)
                        {
                            userData.HospitalId = hospitalData.HospitalId;
                            userData.HospitalName = hospitalData.HospitalName;
                            userData.HospitalAddress = hospitalData.Address;
                        }
                    }
                }
                else if (userData.RoleID == 3)
                {

                    var managementData = _db.Management_Admin.Where(x => x.UserId == userId).FirstOrDefault();

                    if (managementData != null)
                    {
                        var hospitalData = _db.Hospital.Where(x => x.HospitalId == managementData.HospitalId).FirstOrDefault();
                        userData.HospitalId = managementData.HospitalId;
                        userData.ManagementId = managementData.ManagementId;
                        if (hospitalData != null)
                        {
                            userData.HospitalName = hospitalData.HospitalName;
                            userData.HospitalAddress = hospitalData.Address;
                            userData.RoleType = "Management" + "(" + hospitalData.HospitalName + ")";
                        }
                        List<DoctorDetails> doctorDetails = _commonmethods.GetDoctorsByManagement(managementData.HospitalId);
                        userData.DoctorDetails = doctorDetails;
                    }
                }
                else if (userData.RoleID == 4)
                {
                    userData.RoleType = "Super Admin";
                    List<DoctorDetails> doctorDetails = _commonmethods.GetDoctorsByManagement(0);
                    userData.DoctorDetails = doctorDetails;
                    List<Management_Admin> managements = _common.GetManagementsAll();
                    userData.Managements = managements;
                    List<Hospital> hospitals = _common.GetHospitalList();
                    userData.Hospital = hospitals;
                }
            }
            HttpContext.Session.SetInt32("UserProfile", userId);
            return View(userData);
        }
        [HttpPost]
        public IActionResult UserProfile(UserDetails model)
        {
            var hospitalId = HttpContext.Session.GetInt32("ManagementAdminId");
            int hospitalID = 0;
            if (hospitalId != null)
            {
                hospitalID = (int)hospitalId;
            }
            if (model != null)
            {
                int result = UpdateUserDetails(model, hospitalID);

                if (result == 0)
                {
                    TempData["error"] = "Email Already Exists!";
                    if (User.IsInRole("ManagementAdmin"))
                    {
                        List<DoctorDetails> doctorDetails = _commonmethods.GetDoctorsByManagement(hospitalID);
                        model.DoctorDetails = doctorDetails;
                        return View(model);
                    }
                    else if (User.IsInRole("SuperAdmin"))
                    {
                        List<DoctorDetails> doctorDetails = _commonmethods.GetDoctorsByManagement(0);
                        model.DoctorDetails = doctorDetails;
                        List<Management_Admin> managements = _common.GetManagementsAll();
                        model.Managements = managements;
                        List<Hospital> hospitals = _db.Hospital.OrderByDescending(h => h.HospitalId).ToList();
                        model.Hospital = hospitals;
                        return View(model);
                    }
                    return View(model);
                }
                else
                {
                    TempData["profileUpdate"] = "User Profile Updated Successfully";
                    if (User.IsInRole("Doctor"))
                    {
                        if (model.UserName != null)
                        {
                            HttpContext.Session.SetString("DoctorName", model.UserName);
                        }
                    }
                    else if (User.IsInRole("ManagementAdmin"))
                    {
                        HttpContext.Session.SetString("ManagementAdminName", model.UserName);
                        List<DoctorDetails> doctorDetails = _commonmethods.GetDoctorsByManagement(hospitalID);
                        model.DoctorDetails = doctorDetails;
                        return View(model);

                    }
                    else if (User.IsInRole("SuperAdmin"))
                    {
                        HttpContext.Session.SetString("SuperAdminName", model.UserName);
                        List<DoctorDetails> doctorDetails = _commonmethods.GetDoctorsByManagement(0);
                        model.DoctorDetails = doctorDetails;
                        List<Management_Admin> managements = _common.GetManagementsAll();
                        model.Managements = managements;
                        List<Hospital> hospitals = _db.Hospital.OrderByDescending(h => h.HospitalId).ToList();
                        model.Hospital = hospitals;
                        return View(model);
                    }
                    else
                    {
                        HttpContext.Session.SetString("UserName", model.UserName);
                    }
                }
            }
            return View(model);
        }

        public int UpdateUserDetails(UserDetails model, int hospitalId = 0)
        {
            using (SqlConnection connection = new SqlConnection(_db.Database.GetConnectionString()))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("UserDetails_UpdateByUserId", connection))
                {
                    var result = new SqlParameter("@result", SqlDbType.VarChar, 50);
                    result.Direction = ParameterDirection.Output;
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UserId", model.UserId);
                    command.Parameters.AddWithValue("@UserName", model.UserName);
                    command.Parameters.AddWithValue("@Age", model.Age);
                    command.Parameters.AddWithValue("@PhoneNo", model.PhoneNo);
                    command.Parameters.AddWithValue("@DateOfBirth", model.DateOfBirth);
                    command.Parameters.AddWithValue("@Email", model.Email);
                    command.Parameters.AddWithValue("@HospitalId", hospitalId);
                    command.Parameters.AddWithValue("@HospitalName", model.HospitalName);
                    command.Parameters.AddWithValue("@HosapitalAddress", model.HospitalAddress);
                    command.Parameters.Add(result);
                    command.ExecuteNonQuery();
                    if (result.Value.ToString() == "success")
                    {

                        return 1;

                    }
                    else
                    {

                        return 0;
                    }
                }
            }
        }
        public List<PatientAppoinmentModel> GetCompletedAppointments(int userId)
        {
            List<PatientAppoinmentModel> model = new List<PatientAppoinmentModel>();

            using (SqlConnection connection = new SqlConnection(_db.Database.GetConnectionString()))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("Appointments_ByUserANDCompleted", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UserId", userId);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            PatientAppoinmentModel appoinments = new PatientAppoinmentModel
                            {
                                AppointmentID = (int)reader["AppointmentID"],
                                HospitalID = (int)reader["HospitalID"],
                                PatientId = (int)reader["PatientId"],
                                DoctorName = reader["DoctorName"].ToString(),
                                AppointmentStatus = reader["AppointmentStatus"].ToString(),
                                HospitalName = reader["HospitalName"].ToString(),
                                DoctorType = reader["DoctorType"].ToString(),
                                AppointmentDate = (DateTime)reader["AppointmentDate"],
                                AppointmentTime = reader["AppoinmentTime"].ToString(),
                            };

                            model.Add(appoinments);
                        }

                        reader.Close();
                    }
                }

            }

            return model;
        }

    }
}






