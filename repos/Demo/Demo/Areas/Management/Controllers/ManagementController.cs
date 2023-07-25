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


        public IActionResult CreateDoctor(int id = 0)
        {
            DoctorDetails model = new DoctorDetails();
            model.HospitalId = id;
            return View(model);
        }

        [HttpPost, ActionName("CreateDoctor")]
        public IActionResult CreateDoctor(DoctorDetails doctorDetailModel)
        {
            if (doctorDetailModel == null)
            {
                return View();
            }
            _common.CreateNewDoctor(doctorDetailModel);
            if (User.IsInRole("Doctor"))
            {
                TempData["success"] = "Doctor Added Successfully";
                return RedirectToAction("DisplayDoctor", "Management", new { area = "Management" });
            }
            else
            {
                TempData["success"] = "Doctor Added Successfully";
                return RedirectToAction("DisplayDoctor", "Management", new { area = "Management", hospitalId = doctorDetailModel.HospitalId });
            }
            return View();
        }

        [HttpGet]
        public IActionResult EditDoctor(int id)
        {
            if (id <= null)
            {
                return View();
            }
            DoctorDetails doctors = new DoctorDetails();
            doctors = _common.EditDocotrGetData(id);
            return View(doctors);
        }

        [HttpPost, ActionName("EditDoctor")]
        public IActionResult EditDoctor(DoctorDetails doctorDetailModel, int id)
        {
            if (id <= 0)
            {
                return View();
            }
            _common.EditDoctorById(doctorDetailModel, id);
            if (User.IsInRole("Doctor"))
            {
                TempData["success"] = "Doctor Edited Successfully";
                return RedirectToAction("DisplayDoctor", "Management", new { area = "Management" });
            }
            else
            {
                TempData["success"] = "Doctor Edited Successfully";
                return RedirectToAction("DisplayDoctor", "Management", new { area = "Management", hospitalId = doctorDetailModel.HospitalId });
            }
            return View();
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
            if(User.IsInRole("SuperAdmin") || User.IsInRole("ManagementAdmin"))
            {
                HttpContext.Session.SetInt32("DoctorIdForSAandAdmin", doctorId);
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
                else
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

        // 26 June - Create New Appoinment Method
        public IActionResult CreateAppoinment(int id = 0, int doctorId = 0)
        {
            var model = new PatientAppoinmentModel();
            model.HospitalID = id;
            model.DoctorID = doctorId;
            return View(model);
        }
        [HttpPost]
        public IActionResult CreateAppoinment(PatientAppoinmentModel model)
        {
            if (model == null)
            {
                return View();
            }

            int result = _common.CreateNewAppoinment(model);
            string successMessage = "Appoinment Added Successfully";
            string errorMessage = "Slot is not Available, Select Different Slot";

            if (User.IsInRole("Doctor"))
            {
                if (result == 1)
                {
                    TempData["success"] = successMessage;
                    return RedirectToAction("Appoinments", "Management", new { area = "Management"/*, hospitalId = model.HospitalID*/ });
                }
                else if (result == 0)
                {
                    TempData["error"] = errorMessage;
                    return RedirectToAction("CreateAppoinment", "Management", new { area = "Management"/*, id = model.HospitalID */});
                }
            }
            else if (User.IsInRole("ManagementAdmin"))
            {
                if (result == 1)
                {
                    TempData["success"] = successMessage;
                    return RedirectToAction("Appoinments", "Management", new { area = "Management", hospitalId = model.HospitalID });
                }
                else if (result == 0)
                {
                    TempData["error"] = errorMessage;
                    return RedirectToAction("CreateAppoinment", "Management", new { area = "Management", id = model.HospitalID });
                }
            }

            return View();
        }

        //27 June - Complete Edit Method 
        [HttpPost]
        public IActionResult EditAppoinment(PatientAppoinmentModel model, int id)
        {
            if (id == 0)
            {
                return View();
            }
            int result = _common.EditAppoinment(model, id);
            string success = "Appoinment Edited Successfully";
            string errorMessage = "Slot is not Available, Select Different Slot";

            if (User.IsInRole("User"))
            {
                if (result == 1)
                {
                    TempData["success"] = success;
                    return RedirectToAction("BookAppoint", "User", new { area = "User", userId = model.PatientId });
                }
                else if (result == 0)
                {
                    TempData["error"] = errorMessage;
                    return RedirectToAction("CreateAppointments", "User", new { area = "User", userId = model.PatientId });
                }
            }
            else if (User.IsInRole("ManagementAdmin"))
            {
                if (result == 1)
                {
                    TempData["success"] = success;
                    return RedirectToAction("Appoinments", "Management", new { area = "Management", hospitalId = model.HospitalID });
                }
                else if (result == 0)
                {
                    TempData["error"] = errorMessage;
                    return RedirectToAction("CreateAppoinment", "Management", new { area = "Management", id = model.HospitalID });
                }
            }
            else if (User.IsInRole("Doctor"))
            {
                if (result == 1)
                {
                    TempData["success"] = success;
                    return RedirectToAction("AppoinmentsByDoctor", "Management", new { area = "Management", doctorId = model.DoctorID });
                }
                else if (result == 0)
                {
                    TempData["error"] = errorMessage;
                    return RedirectToAction("CreateAppoinment", "Management", new { area = "Management"/*, id = model.HospitalID */});
                }
            }
            else
            {
                TempData["success"] = success;
                return RedirectToAction("Appoinments", "Management", new { area = "Management" });
            }
            return View();

        }
        [HttpGet, ActionName("EditAppoinment")]
        public IActionResult EditAppoinment(int id)
        {
            if (id <= 0)
            {
                return View();
            }
            PatientAppoinmentModel data = new PatientAppoinmentModel();
            data = _common.GetAppoinmentData(id);
            return View(data);
        }

        // 26 June - UpdateAppointmentStatus Method

        public JsonResult UpdateAppointmentStatus(int id, string status,int approveId=0,int rejectId=0)
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

            TempData["success"] = (result == 1) ? successMessage : errorMessage;
            if (result == 1)
            {
                TempData["success"] = successMessage;
            }
            else
            {
                TempData["error"] = errorMessage;
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
            string successMessage = "";
            if (model != null && model.DoctorID != 0)
            {
                int id = model.DoctorID;
                _common.EditDoctorById(model, id);
                successMessage = "Doctor Edited Successfully";
            }
            else
            {
                _common.CreateNewDoctor(model);
                successMessage = "Doctor Added Successfully";
            }
            TempData["success"] = successMessage;

            if (User.IsInRole("ManagementAdmin"))
            {
                return RedirectToAction("DisplayDoctor", "Management", new { area = "Management", hospitalId = adminId });
            }
            else
            {
                return RedirectToAction("DisplayDoctor", "Management", new { area = "Management" });
            }
            return View();
        }

        [HttpPost]
        public IActionResult GetFilteredAppointments(string status, int doctorId, int pageindex = 1)
        {

            List<PatientAppoinmentModel> appoints = GetAppointmentsByStatus(status, doctorId);
            int pageSize = 5;
            var Doctor= HttpContext.Session.GetInt32("DoctorId"); ;
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
                if((HttpContext.Session.GetInt32("DoctorIdForSAandAdmin") != null))
                {
                    DoctorId = (int)HttpContext.Session.GetInt32("DoctorIdForSAandAdmin");
                }
                
            }




            DateTime lowerBound = new DateTime(1753, 1, 1, 0, 0, 0);
            DateTime upperBound = new DateTime(9999, 12, 31, 23, 59, 59);
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
    }
}


