using Demo.DataAccess.Common;
using Demo.DataAccess.Data;
using Demo.Models;
using Demo.Models.DummyModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Demo.Controllers.ErrorController;

namespace Demo.Controllers
{
    [Area("User")]
    [Authorize(Roles = "Doctor,ManagementAdmin,User,SuperAdmin")]
    [CustomExceptionFilter]
    public class UserController : Controller
    {
        private readonly ManagementMethods _common;
        private readonly AppDbContext _db;
        public UserController(AppDbContext db, ManagementMethods common)
        {
            _db = db;
            _common = common;
        }

        public IActionResult CreateAppointments(int id)
        {
            if(id <= 0)
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
            if(userId == 0)
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
        // 29 June - Delete Appoinment
        public IActionResult DeleteAppoinment(int id,int userId)
        {
            if (id <= 0)
            {
                return View();
            }
            _common.RemoveAppoinment(id);
            return RedirectToAction("BookAppoint", new { userId = userId});
        }
       
        

        public JsonResult GetHospitals()
            {
            List<Hospital> hospitals = new List<Hospital>();
            hospitals = _common.GetHospitalList();
            return Json(hospitals);
        }
        public JsonResult GetUsers()
        {
            var users = _db.User.Where(m => m.RoleID == 1).ToList();
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

    }
}






