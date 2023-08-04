using Demo.DataAccess.Common;
using Demo.DataAccess.Data;
using Demo.Models;
using Demo.Models.DummyModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PagedList;
using System.Data;


namespace Demo.Controllers
{
    [Area("Management")]
    [Authorize(Roles = "Doctor,ManagementAdmin,User,SuperAdmin")]
    public class ManagementController : Controller
    {

        private readonly ManagementMethods _common;
        private readonly CommonMethods _commonmethods;
        private readonly AppDbContext _db;
        public ManagementController(AppDbContext db, ManagementMethods common, CommonMethods commonmethods)
        {
            _commonmethods = commonmethods;
            _db = db;
            _common = common;
        }


        public IActionResult DisplayDoctor(int hospitalId = 0)
        {
            
            List<DoctorDetails> doctorDetails = _commonmethods.GetDoctorsByManagement(hospitalId);
            List<PatientAppoinmentModel> palist = _commonmethods.GetAppointmentsByManagement(0);
            var viewModel = new AdminInstances
            {
                DoctorDetails = doctorDetails,
                HospitalId = hospitalId,
                patientAppoinmentModels = palist
            };

            return View(viewModel);
        }
        [HttpGet]
        public IActionResult DelateDoctor(int id, int hospitalId = 0)
        {
            if (id <= 0)
            {
                return View();
            }
            _common.RemoveDoctor(id);
            TempData["success"] = "Doctor Deleted Successful";
            if (hospitalId == 0)
            {
                return RedirectToAction("DisplayDoctor");
            }
            else
            {
                return RedirectToAction("DisplayDoctor", "Management", new { area = "Management", hospitalId = hospitalId });
            }
        }

        public IActionResult Appoinments(int hospitalId = 0)
        {

            List<PatientAppoinmentModel> appointments = _commonmethods.GetAppointmentsByManagement(hospitalId);
            var viewModel = new AdminInstances
            {
                Appointments = appointments,
                HospitalId = hospitalId
            };

            return View(viewModel); ;
        }
        [HttpGet]
        public IActionResult AppoinmentsByDoctor(int doctorId, int? page, int hospitalId = 0)
        {
            if (doctorId <= 0)
            {
                return View();
            }
            HttpContext.Session.SetInt32("DoctorIdForCreateApp", doctorId);
            if (User.IsInRole("SuperAdmin") || User.IsInRole("ManagementAdmin"))
            {
                HttpContext.Session.SetInt32("DoctorIdForSAandAdmin", doctorId);
                HttpContext.Session.SetInt32("Did", doctorId);
                HttpContext.Session.SetInt32("FlagforMSA", doctorId);
            }
            ViewBag.Did = doctorId;
            ViewBag.HospitalId = hospitalId;
            List<PatientAppoinmentModel> appointments = _commonmethods.GetAppointmentsByDoctor(doctorId);
            if (appointments.Count <= 0)
            {
                if (User.IsInRole("SuperAdmin"))
                {
                    TempData["NotFound"] = "No Appointment Found";
                    return RedirectToAction("DisplayDoctor", "Management", new { area = "Management" });
                }
                else if (User.IsInRole("ManagementAdmin"))
                {
                    TempData["NotFound"] = "No Appointment Found";
                    return RedirectToAction("DisplayDoctor", "Management", new { area = "Management", hospitalId = hospitalId });
                }
            }

            int pageSize = 5;
            int pageNumber = page ?? 1;
            IPagedList<PatientAppoinmentModel> pagedAppointments = appointments.ToPagedList(pageNumber, pageSize);

            DateTime today = DateTime.Today;
            int hour = DateTime.Now.Hour;

            int data = _db.Patient_Appoinments.Where(x => x.AppointmentDate == today && x.DoctorID == doctorId && x.AppointmentStatus == "Approve").Count();

            List<PatientAppoinmentModel> apps = _commonmethods.GetData(doctorId, today);
            if (apps != null)
            {
                var matchingAppointments = apps.Where(app =>
                app.AppointmentStatus == "Approve" &&
                ((hour >= 7 && hour < 9 && app.AppointmentTime == "9 AM") ||
                 (hour >= 9 && hour < 11 && app.AppointmentTime == "11 AM") ||
                 (hour >= 11 && hour < 14 && app.AppointmentTime == "2 PM") ||
                 (hour >= 14 && hour < 19 && app.AppointmentTime == "5 PM"))).ToList();


                if (matchingAppointments.Count > 0)
                {
                    var selectedAppointment = matchingAppointments.First();
                    ViewBag.time = selectedAppointment.AppointmentTime;
                    ViewBag.id = selectedAppointment.AppointmentID;
                    ViewBag.name = selectedAppointment.UserName;
                }
            }
            HttpContext.Session.SetInt32("total", data);
            return View(pagedAppointments);
        }

        // 26 June - UpdateAppointmentStatus Method

        public JsonResult UpdateAppointmentStatus(int id, string status, int approveId = 0, int rejectId = 0)
        {
            if (id <= 0)
            {
                return null;
            }
            var st = _commonmethods.UpdateAppointmentStatus(id, status, approveId, rejectId);
            return Json(new { success = true, st, id });
        }

        public IActionResult AddEditAppointment(int appId = 0)
        {
            if (appId <= 0)
            {
                var chkHMAccess = HttpContext.Session.GetInt32("Did");
                if (User.IsInRole("Doctor") || ((User.IsInRole("ManagementAdmin") || User.IsInRole("ManagementAdmin")) && chkHMAccess != null))
                {

                    PatientAppoinmentModel datas = new PatientAppoinmentModel();
                    var doctorId = (int)HttpContext.Session.GetInt32("DoctorIdForCreateApp");
                    var doctor = _db.DoctorDetails.Where(x => x.DoctorID == doctorId).FirstOrDefault();
                    if (doctor != null)
                    {
                        datas.DoctorID = doctorId;
                        datas.DoctorTypeId = (int)doctor.DoctorTypeId;
                        datas.HospitalID = (int)doctor.HospitalId;
                    }
                    return View(datas);
                }
                HttpContext.Session.Remove("Did");

                return View();
            }
            PatientAppoinmentModel data = new PatientAppoinmentModel();
            data = _common.GetAppoinmentData(appId);
            return View(data);
        }
        [HttpPost]
        public IActionResult AddEditAppointment(PatientAppoinmentModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var doctorId = HttpContext.Session.GetInt32("DoctorId");
            var adminId = HttpContext.Session.GetInt32("ManagementAdminId");

            int result = 0;
            string successMessage = "";
            string errorMessage = "";

            if (model != null && model.AppointmentID != 0)
            {
                int id = model.AppointmentID;
                result = _common.EditAppoinment(model, id);
                successMessage = "Appointment Edited Successfully";
                errorMessage = "Slot is not Available, Select Different Slot";
            }
            else
            {
                result = _common.CreateNewAppoinment(model);
                successMessage = "Appointment Added Successfully";
                errorMessage = "Slot is not Available, Select Different Slot";
            }

            
            if (result == 1)
            {
                TempData["success"] = successMessage;
            }
            else if(result == 0)
            {
                TempData["error"] = errorMessage;
            }
            else
            {
                TempData["error"] = "Patient Already Booked This Slot for today Select Other Option";
            }

            if (result == 1)
            {
                if (User.IsInRole("User"))
                {
                    return RedirectToAction("BookAppoint", "User", new { area = "User", userId = userId });
                }
                else if (User.IsInRole("Doctor"))
                {
                    return RedirectToAction("AppoinmentsByDoctor", "Management", new { area = "Management", doctorId = doctorId });
                }
                else if (User.IsInRole("ManagementAdmin"))
                {
                    return RedirectToAction("Appoinments", "Management", new { area = "Management", hospitalId = adminId });
                }
                else
                {
                    return RedirectToAction("Appoinments", "Management", new { area = "Management" });
                }
            }
            else
            {
                return RedirectToAction("AddEditAppointment", "Management", new { area = "Management" });
            }
            return View(model);
        }

        public IActionResult DeleteAppoinment(int id, int hospitalId = 0)
        {

            if (id <= 0)
            {
                return View();
            }
            _common.RemoveAppoinment(id);
            TempData["success"] = "Appointment Deleted Successful";
            if (User.IsInRole("Doctor"))
            {
                return RedirectToAction("AppoinmentsByDoctor", "Management", new { area = "Management", doctorId = hospitalId });
            }
            else if (User.IsInRole("SuperAdmin"))
            {
                return RedirectToAction("DisplayDoctor", "Management", new { area = "Management" });
            }
            return RedirectToAction("DisplayDoctor", new { hospitalId = hospitalId });
        }


        public IActionResult AddEditDoctor(int id)
        {
            if (id <= 0)
            {
                return View();
            }
            DoctorDetails doctors = new DoctorDetails();
            doctors = _common.EditDocotrGetData(id);
            return View(doctors);
        }
        [HttpPost]
        public IActionResult AddEditDoctor(DoctorDetails model)
        {
            var adminId = HttpContext.Session.GetInt32("ManagementAdminId");
            var chkPageAccess = HttpContext.Session.GetInt32("UserProfile");
            string successMessage = "";
            int result;
            if (model != null && model.DoctorID != 0)
            {
                int id = model.DoctorID;
                result = _common.EditDoctorById(model, id);
                successMessage = "Doctor Edited Successfully";
            }
            else
            {
                result = _common.CreateNewDoctor(model);
                successMessage = "Doctor Added Successfully";
            }
            if(result == 0)
            {
                TempData["error"] = "Email Already Exists!";
                return View(model);
            }
            else
            {
                TempData["success"] = successMessage;
            }
            

            if (User.IsInRole("ManagementAdmin"))
            {

                if (chkPageAccess != null)
                {
                    return RedirectToAction("UserProfile", "User", new { area = "User", userId = (int)chkPageAccess });
                }
                return RedirectToAction("DisplayDoctor", "Management", new { area = "Management", hospitalId = adminId });
            }
            else
            {
                if (chkPageAccess != null)
                {
                    return RedirectToAction("UserProfile", "User", new { area = "User", userId = (int)chkPageAccess });
                }
                return RedirectToAction("DisplayDoctor", "Management", new { area = "Management" });
            }
            return View();
        }

        [HttpPost]
        public IActionResult GetFilteredAppointments(string status, int doctorId, int pageindex = 1)
        {

            List<PatientAppoinmentModel> appoints = GetAppointmentsByStatus(status, doctorId);
            int pageSize = 5;
            var Doctor = HttpContext.Session.GetInt32("DoctorId"); ;
            if (User.IsInRole("SuperAdmin") || User.IsInRole("ManagementAdmin"))
            {
                Doctor = HttpContext.Session.GetInt32("DoctorIdForSAandAdmin");
            }
            ViewBag.Did = (int)Doctor;
            IPagedList<PatientAppoinmentModel> pagedAppointments = appoints.ToPagedList(pageindex, pageSize);

            return PartialView("_FilterByStatus", pagedAppointments);
        }

        public List<PatientAppoinmentModel> GetAppointmentsByStatus(string status, int doctorId)
        {
            List<PatientAppoinmentModel> appointmentList = new List<PatientAppoinmentModel>();

            using (SqlConnection connection = new SqlConnection(_db.Database.GetConnectionString()))
            {
                SqlCommand command = new SqlCommand("Appointments_FilterByStatus", connection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@status", status);
                command.Parameters.AddWithValue("@doctorId", doctorId);

                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        PatientAppoinmentModel appointment = new PatientAppoinmentModel();
                        appointment.AppointmentID = Convert.ToInt32(reader["AppointmentID"]);
                        appointment.DiseaseDescriptions = reader["DiseaseDescriptions"].ToString();
                        appointment.AppointmentStatus = reader["AppointmentStatus"].ToString();
                        appointment.HospitalName = reader["HospitalName"].ToString();
                        appointment.DoctorType = reader["DoctorType"].ToString();
                        appointment.DoctorName = reader["DoctorName"].ToString();
                        appointment.UserName = reader["UserName"].ToString();
                        appointment.DoctorID = Convert.ToInt32(reader["DoctorID"]);
                        appointment.HospitalID = Convert.ToInt32(reader["HospitalId"]);
                        appointment.PatientId = Convert.ToInt32(reader["PatientId"]);
                        appointment.AppointmentDate = (DateTime)reader["AppointmentDate"];
                        appointment.AppointmentTime = reader["AppoinmentTime"].ToString();

                        appointmentList.Add(appointment);
                    }
                }
            }

            return appointmentList;
        }


        public void ApproveSelectedAppointments(DateTime date1, DateTime date2, int DoctorId = 0, int approveId = 0)
        {


            if (User.IsInRole("SuperAdmin"))
            {
                approveId = (int)HttpContext.Session.GetInt32("SuperAdminId");

            }
            else if (User.IsInRole("ManagementAdmin"))
            {
                approveId = (int)HttpContext.Session.GetInt32("ManagementForStatus");
            }
            else if (User.IsInRole("Doctor"))
            {
                approveId = (int)HttpContext.Session.GetInt32("DoctorForStatus");
            }


            if (User.IsInRole("SuperAdmin") || User.IsInRole("ManagementAdmin"))
            {
                if ((HttpContext.Session.GetInt32("DoctorIdForSAandAdmin") != null))
                {
                    DoctorId = (int)HttpContext.Session.GetInt32("DoctorIdForSAandAdmin");
                }

            }

            DateTime lowerBound = new DateTime(1753, 1, 1, 0, 0, 0);
            DateTime upperBound = new DateTime(9999, 12, 31, 23, 59, 59);
            var d1 = DateTime.MinValue;
            var d2 = DateTime.MaxValue;
            if (!(date1 >= lowerBound && date1 <= upperBound && date2 >= lowerBound && date2 <= upperBound))
            {
                TempData["error"] = "Please Select Valid Date";
                return;
            }
            using (SqlConnection connection = new SqlConnection(_db.Database.GetConnectionString()))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("Appointments_ApproveSelected", connection))
                {
                    var result = new SqlParameter("@result", SqlDbType.VarChar, 50);
                    result.Direction = ParameterDirection.Output;
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@Date1", date1);
                    command.Parameters.AddWithValue("@Date2", date2);
                    command.Parameters.AddWithValue("@DoctorId", DoctorId);
                    command.Parameters.AddWithValue("@approveId", approveId);
                    command.Parameters.Add(result);
                    command.ExecuteNonQuery();
                    if (result.Value.ToString() == "success")
                    {
                        TempData["success"] = "Appointments Approved Successfully";

                    }
                    else
                    {
                        TempData["error"] = "No Appointments Found to be Approve";
                    }
                }
            }
        }
        public void UpdateStatusComplete(int appId)
        {
            if (appId <= 0)
            {
                return;
            }
            using (SqlConnection connection = new SqlConnection(_db.Database.GetConnectionString()))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("UpdateStatus_Complete", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Status", "Completed");
                    command.Parameters.AddWithValue("@AppId", appId);
                    command.ExecuteNonQuery();
                }
            }
        }
        public void UpdateStatusMissed(int hospitalId = 0)
        {
            using (SqlConnection connection = new SqlConnection(_db.Database.GetConnectionString()))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("UpdateStatus_Missed", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@HospitalId", hospitalId);
                    command.ExecuteNonQuery();
                }
            }
        }
        public JsonResult RemoveManagement(int hospitalId)
        {
            if (hospitalId <= 0)
            {
                return null;
            }
            var hospital = _db.Hospital.Find(hospitalId);
            var management = _db.Management_Admin.Where(x => x.HospitalId == hospitalId).FirstOrDefault();
            if (hospital == null || management == null)
            {
                return null;
            }
            hospital.blnActive = false;
            management.blnActive = false;
            _db.SaveChanges();
            return Json(new { redirectUrl = Url.Action("Login", "Login", new { area = "Login" }) });

        }
        public JsonResult UpdateModule(string doctorName, bool updatedStatus)
        {
            var data = _db.DoctorDetails.Where(x => x.DoctorName == doctorName).FirstOrDefault();
            if (data != null)
            {
                data.blnActive = updatedStatus;
                _db.SaveChanges();
            }
            return Json("true");
        }
        public JsonResult UpdateManagementModule(string hospitalname, bool updatedStatus)
        {
            var hospital = _db.Hospital.Where(x => x.HospitalName == hospitalname).FirstOrDefault();
            var data = _db.Management_Admin.Where(x => x.HospitalId == hospital.HospitalId).FirstOrDefault();
            if (data != null)
            {
                data.blnActive = updatedStatus;
                _db.SaveChanges();
            }
            return Json("true");
        }
        public JsonResult UpdateHospitalModule(string hospitalname, bool updatedStatus)
        {
            var data = _db.Hospital.Where(x => x.HospitalName == hospitalname).FirstOrDefault();

            var management = _db.Management_Admin.Where(x => x.HospitalId == data.HospitalId).FirstOrDefault();
            if (data != null)
            {
                data.blnActive = updatedStatus;
                if (management != null)
                    management.blnActive = updatedStatus;
                _db.SaveChanges();
            }
            return Json("true");
        }
    }
}


