using Demo.DataAccess.Common;
using Demo.DataAccess.Data;
using Demo.Models;
using Demo.Models.DummyModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Text.RegularExpressions;
using static Demo.Controllers.ErrorController;

namespace Demo.Controllers
{
    [Area("Doctor")]
    [Authorize(Roles = "Doctor,ManagementAdmin,SuperAdmin")]
    [CustomExceptionFilter]
    public class DoctorController : Controller
    {
        private readonly string wwwrootDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        private readonly CommonMethods _commonmethods;
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _hostEnvironment;
        public DoctorController(AppDbContext db, CommonMethods commonmethods, IWebHostEnvironment hostEnvironment)
        {
            _db = db;
            _commonmethods = commonmethods;
            _hostEnvironment = hostEnvironment;
        }
        public IActionResult DoctorPanel(int hospitalId = 0)
        {
            List<PatientAppoinmentModel> appointments = _commonmethods.GetAppointmentsByManagement(hospitalId);
            return View(appointments);
        }

        public IActionResult PatientDetails(int id, string ongoing = null)
        {
            if (id <= 0)
            {
                return View();
            }
            if (ongoing != null)
            {
                HttpContext.Session.SetInt32("flag", 1);
            }
            PatientDetails appointmentById = new PatientDetails();
            appointmentById = _commonmethods.GetPatientDetailsByAppointmentId(id);
            var appointmentList = GetAppointmentListByUserId(appointmentById.PatientId);
            if (appointmentList != null)
            {
                appointmentById.aps = appointmentList;
            }
            var docs = _db.Documents.Where(x => x.AppointmentId == id && x.DeletedAt == null ).ToList();
            //var fileNames = Directory.GetFiles(wwwrootDirectory, "*").Select(Path.GetFileName).ToList();
            var doc = new List<Documents>();
            foreach (var d in docs)
            {
                var doctype = new Documents();
                doctype.DocumentId = d.DocumentId;
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
        [HttpPost]
        public IActionResult PatientDetails(PatientDetails model)
        {
            var doctorIdStatus = HttpContext.Session.GetInt32("DoctorForStatus");
            var adminIdStatus = HttpContext.Session.GetInt32("ManagementForStatus");
            var superAdminId = HttpContext.Session.GetInt32("SuperAdminId");
            var doctorId = HttpContext.Session.GetInt32("DoctorId");
            var adminId = HttpContext.Session.GetInt32("ManagementAdminId");
            var doctorIdforAdmin = HttpContext.Session.GetInt32("DoctorIdForSAandAdmin");

            int statusUpdateId = 0;
            if (User.IsInRole("Doctor"))
            {
                statusUpdateId = (int)doctorIdStatus;
            }
            else if (User.IsInRole("ManagementAdmin"))
            {
                statusUpdateId = (int)adminIdStatus;
            }
            else
            {
                statusUpdateId = (int)superAdminId;
            }
            int result = 0;
            string successMessage = "";
            string errorMessage = "";

            if (model != null && model.AppointmentId != 0)
            {
                int id = model.AppointmentId;
                result = EditAppoinment(model, id, statusUpdateId);
                successMessage = "Appointment Updated Successfully";
                errorMessage = "Slot is not Available, Select Different Slot";
            }

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

                if (User.IsInRole("Doctor"))
                {
                    return RedirectToAction("AppoinmentsByDoctor", "Management", new { area = "Management", doctorId = doctorId });
                }
                else if (User.IsInRole("ManagementAdmin"))
                {
                    return RedirectToAction("AppoinmentsByDoctor", "Management", new { area = "Management", doctorId = (int)doctorIdforAdmin, hospitalId = adminId });
                }
                else
                {
                    return RedirectToAction("AppoinmentsByDoctor", "Management", new { area = "Management", doctorId = (int)doctorIdforAdmin });
                }
            }
            else
            {
                return RedirectToAction("PatientDetails", new { id = model.AppointmentId });
            }
            return View(model);
        }
        public int EditAppoinment(PatientDetails model, int id, int statusUpdateId)
        {
            if (id <= 0)
            {
                return 0;
            }

            using (SqlConnection connection = new SqlConnection(_db.Database.GetConnectionString()))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("Appointment_ChkEditInPatientDetails", connection))
                {
                    var result = new SqlParameter("@result", SqlDbType.VarChar, 50);
                    result.Direction = ParameterDirection.Output;
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@ID", id);
                    command.Parameters.AddWithValue("@AppointmentDate", model.AppointmentDate);
                    command.Parameters.AddWithValue("@HospitalID", model.HospitalId);
                    command.Parameters.AddWithValue("@PatientId", model.PatientId);
                    command.Parameters.AddWithValue("@DoctorID", model.DoctorId);
                    command.Parameters.AddWithValue("@DoctorTypeId", model.DoctorTypeId);
                    command.Parameters.AddWithValue("@AppoinmentTime", model.AppointmentTime);
                    command.Parameters.AddWithValue("@AppointmentStatus", model.Status);
                    command.Parameters.AddWithValue("@StatusUpdaterId", statusUpdateId);
                    command.Parameters.AddWithValue("@Prescription", model.Prescription);
                    command.Parameters.AddWithValue("@Suggestions", model.Suggestions);
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

        public List<PatientAppoinmentModel> GetAppointmentListByUserId(int doctorId)
        {
            List<PatientAppoinmentModel> appointments = new List<PatientAppoinmentModel>();

            using (SqlConnection connection = new SqlConnection(_db.Database.GetConnectionString()))
            {
                SqlCommand command = new SqlCommand("Appointments_ByUserANDCompleted", connection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@UserId", doctorId);

                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        PatientAppoinmentModel appointment = new PatientAppoinmentModel();
                        appointment.AppointmentDate = (DateTime)reader["AppointmentDate"];
                        appointment.DiseaseDescriptions = reader["DiseaseDescriptions"].ToString();
                        appointment.DoctorName = reader["DoctorName"].ToString();
                        appointment.HospitalName = reader["HospitalName"].ToString();
                        appointments.Add(appointment);
                    }
                }
            }

            return appointments;
        }





        public JsonResult RemoveDoctor(int doctorId)
        {
            if (doctorId <= 0)
            {
                return null;
            }
            var doctor = _db.DoctorDetails.Find(doctorId);
            if (doctor == null)
            {
                return null;
            }

            doctor.blnActive = false;
            _db.SaveChanges();
            return Json(new { redirectUrl = Url.Action("Login", "Login", new { area = "Login" }) });

        }


        public async Task<IActionResult> UploadDocuments(IEnumerable<IFormFile> files, int userId, string userName, string doctorName,int appId)
        {
            foreach (var file in files)
            {
                if (file != null)
                {
                    var fileName = appId.ToString() + "_" + userName + "_" + doctorName + "_" + Path.GetFileName(file.FileName);
                    var data = _db.Documents.Where(x => x.Document_name == fileName).FirstOrDefault();
                    if (data != null)
                    {
                        TempData["error"] = "File Already Exists.";
                        return Content("File Already Exists.");
                    }

                    //var uploads = Path.Combine(_hostEnvironment.WebRootPath, "documents", fileName);
                    var extension = Path.GetExtension(file.FileName);
                    var path = Path.Combine(wwwrootDirectory, fileName);
                    using (var fileStream = new FileStream(path, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    Documents docModel = new Documents()
                    {
                        AppointmentId = appId,
                        Document_name = fileName,
                        Document_path = @"\wwwroot\",
                        Document_type = extension
                    };
                    _db.Documents.Add(docModel);
                    _db.SaveChanges();
                }
            }

            return Content("Success uploading file.");
        }
        [HttpPost]
        public void RemoveFile(int documentId)
        {

            var file = _db.Documents.Where(x => x.DocumentId == documentId).FirstOrDefault();
            if (file != null)
            {
                if (file.Document_name != null)
                {
                    string documentName = file.Document_name;
                    var filePath = Path.Combine(wwwrootDirectory, documentName);

                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                    if (file != null)
                    {
                        file.DeletedAt = DateTime.Now;
                        _db.SaveChanges();
                    }
                }

            }

        }
    }
}
