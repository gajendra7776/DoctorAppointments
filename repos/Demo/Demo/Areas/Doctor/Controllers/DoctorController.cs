using Demo.DataAccess.Common;
using Demo.DataAccess.Data;
using Demo.Models;
using Demo.Models.DummyModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Text.RegularExpressions;
using static Demo.Controllers.ErrorController;

namespace Demo.Controllers
{
     [Area("Doctor")]
     [Authorize(Roles ="Doctor,ManagementAdmin,SuperAdmin")]
     [CustomExceptionFilter]
    public class DoctorController : Controller
    {

        private readonly CommonMethods _commonmethods;

        private readonly AppDbContext _db;

        public DoctorController(AppDbContext db, CommonMethods commonmethods)
        {
            _db = db;
            _commonmethods = commonmethods;
        }
        public IActionResult DoctorPanel(int hospitalId = 0)
        {
            List<PatientAppoinmentModel> appointments = _commonmethods.GetAppointmentsByManagement(hospitalId);
            return View(appointments);
        }

        public IActionResult PatientDetails(int id)
        {
            if(id <= 0)
            {
                return View();
            }
            PatientDetails appointmentById = new PatientDetails();
            appointmentById = GetPatientDetailsByAppointmentId(id);
            var appointmentList = GetAppointmentListByUserId(appointmentById.PatientId);
            if (appointmentList != null)
            {
                appointmentById.aps = appointmentList;
            }

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

            int statusUpdateId =0;
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
                    return RedirectToAction("AppoinmentsByDoctor", "Management", new { area = "Management", doctorId =(int)doctorIdforAdmin, hospitalId = adminId });
                }
                else
                {
                    return RedirectToAction("AppoinmentsByDoctor", "Management", new { area = "Management" });
                }
            }
            else
            {
                return RedirectToAction("PatientDetails",  new {  id = model.AppointmentId });
            }
            return View(model);
        }
        public int EditAppoinment(PatientDetails model, int id,int statusUpdateId)
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
        public PatientDetails GetPatientDetailsByAppointmentId(int id)
        {
            PatientDetails appointment = new PatientDetails();

            using (SqlConnection connection = new SqlConnection(_db.Database.GetConnectionString()))
            {
                SqlCommand command = new SqlCommand("Patient_Details", connection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@appId", id);

                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                       
                        appointment.AppointmentId = Convert.ToInt32(reader["AppointmentID"]);
                        appointment.PatientId = Convert.ToInt32(reader["PatientId"]);
                        appointment.HospitalId = Convert.ToInt32(reader["HospitalID"]);
                        appointment.DoctorTypeId = Convert.ToInt32(reader["DoctorTypeId"]);
                        appointment.DoctorId = Convert.ToInt32(reader["DoctorID"]);
                        appointment.Age = Convert.ToInt32(reader["Age"]);
                        appointment.Description = reader["DiseaseDescriptions"].ToString();
                        appointment.Status = reader["AppointmentStatus"].ToString();
                        appointment.HospitalName = reader["HospitalName"].ToString();
                        appointment.DoctorType = reader["DoctorType"].ToString();
                        appointment.DoctorName = reader["DoctorName"].ToString();
                        appointment.PatientName = reader["UserName"].ToString();
                        appointment.PatientEmail = reader["Email"].ToString();
                        appointment.DoctorEmail = reader["DEmail"].ToString();
                        appointment.ManagementEmail = reader["MaEmail"].ToString();
                        appointment.Approve_by = reader["ApproveBy"].ToString();
                        appointment.Status = reader["AppointmentStatus"].ToString();
                        appointment.AppointmentDate = (DateTime)reader["AppointmentDate"];
                        appointment.DateOfBirth = (DateTime)reader["DateOfBirth"];
                        if(reader["Approved_Date"] != DBNull.Value)
                        {
                            appointment.ApproveDate = (DateTime)reader["Approved_Date"];
                        }
                        appointment.AppointmentTime = reader["AppoinmentTime"].ToString();
                        
                    }
                }
            }

            return appointment;
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
    }
}
