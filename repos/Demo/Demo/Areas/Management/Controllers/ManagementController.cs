﻿using Demo.DataAccess.Common;
using Demo.DataAccess.Data;
using Demo.Models;
using Demo.Models.DummyModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NonFactors.Mvc.Grid;
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
            if (id == 0)
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
            var viewModel = new AdminInstances
            {
                DoctorDetails = doctorDetails,
                HospitalId = hospitalId
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
            if (id == 0)
            {
                return View();
            }
            PatientAppoinmentModel data = new PatientAppoinmentModel();
            data = _common.GetAppoinmentData(id);
            return View(data);
        }

        // 26 June - UpdateAppointmentStatus Method

        public JsonResult UpdateAppointmentStatus(int id, string status)
        {
            var st = _commonmethods.UpdateAppointmentStatus(id, status);
            return Json(new { success = true, st, id });
        }

        public IActionResult DeleteAppoinment(int id, int hospitalId = 0)
        {
            if (id <= 0)
            {
                return View();
            }
            _common.RemoveAppoinment(id);
            if (User.IsInRole("Doctor") || User.IsInRole("SuperAdmin"))
            {
                return RedirectToAction("AppoinmentsByDoctor", "Management", new { area = "Management", doctorId = hospitalId });
            }
            return RedirectToAction("Appoinments", new { hospitalId = hospitalId });
        }
        [HttpPost]
        public IActionResult GetFilteredAppointments(string status, int doctorId,int pageindex=1)
        {
            
            List<PatientAppoinmentModel> appoints = GetAppointmentsByStatus(status, doctorId);
            int pageSize = 5;
            var Doctor = HttpContext.Session.GetInt32("DoctorId");
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
                        appointmentList.Add(appointment);
                    }
                }
            }

            return appointmentList;
        }



    }
}


