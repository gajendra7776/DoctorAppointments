using Demo.DataAccess.Data;
using Demo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Numerics;
using System.Reflection.PortableExecutable;
using System.Text.RegularExpressions;

namespace Demo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _db;

        public HomeController(AppDbContext db ,ILogger<HomeController> logger)
        {
            _logger = logger;
            _db = db;
        }

        [HttpGet]
        public IActionResult Index()
        {
            List<AppoinmentModel> appoinments= new List<AppoinmentModel>();
            using (SqlConnection connection = new SqlConnection(_db.Database.GetConnectionString()))
            {
                connection.Open();
                string queryString = "select * from Appointments";
                using (SqlCommand command = new SqlCommand(queryString, connection))
                {
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        AppoinmentModel appmodel = new AppoinmentModel
                        {
                            AppoinmentId = (int)reader["AppoinmentId"],
                            PatientName = reader["PatientName"].ToString(),
                            DoctorName = reader["DoctorName"].ToString(),
                            status = reader["status"].ToString(),
                            Description = reader["Description"].ToString(),
                            AppointmentDate = Convert.ToDateTime(reader["AppointmentDate"]),
                            phoneno = (long)reader["phoneno"],
                            Email = reader["Email"].ToString()

                        };
                        appoinments.Add(appmodel);
                    }
                    reader.Close();
                }
            }
            return View(appoinments);
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost, ActionName("Create")]
        public IActionResult Create(AppoinmentModel model)

        {
            string pattern = @"^[a-zA-Z0-9_\.-]+@([a-zA-Z0-9-]+\.)+[a-zA-Z]{2,3}$";

            if (model.Email == null)
            {
                ModelState.AddModelError("Email", "Enter Email");
                return View(model);
            }
            else if(!Regex.IsMatch(model.Email, pattern))
            {
                ModelState.AddModelError("Email", "Invalid email address, Please Enter Valid Address");
                return View(model);
            }
            using (SqlConnection connection = new SqlConnection(_db.Database.GetConnectionString()))
            {
                connection.Open();
                string queryString = "INSERT INTO Appointments(PatientName,DoctorName,status,Description,AppointmentDate,phoneno,Email) VALUES (@PatientName,@DoctorName,@status,@Description,@AppointmentDate,@phoneno,@Email)";
                using (SqlCommand command = new SqlCommand(queryString, connection))
                {
                    command.Parameters.AddWithValue("@PatientName", model.PatientName) ;
                    command.Parameters.AddWithValue("@DoctorName", model.DoctorName) ;
                    command.Parameters.AddWithValue("@status", model.status) ;
                    command.Parameters.AddWithValue("@Description", model.Description) ;
                    command.Parameters.AddWithValue("@AppointmentDate", model.AppointmentDate) ;
                    command.Parameters.AddWithValue("@phoneno", model.phoneno) ;
                    command.Parameters.AddWithValue("@Email", model.Email) ;
                    command.ExecuteNonQuery();
                }
            }
            return RedirectToAction("Index");
        }


        public IActionResult Edit(int id)
        {
            AppoinmentModel appmts = null;
            using (SqlConnection connection = new SqlConnection(_db.Database.GetConnectionString()))
            {
                connection.Open();
                string queryString = "SELECT * FROM Appointments WHERE AppoinmentId = @id";
                using (SqlCommand command = new SqlCommand(queryString, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    SqlDataReader reader = command.ExecuteReader();
                    
                        if (reader.Read())
                        {
                            appmts = new AppoinmentModel
                            {
                                AppoinmentId = (int)reader["AppoinmentId"],
                                PatientName = reader["PatientName"].ToString(),
                                DoctorName = reader["DoctorName"].ToString(),
                                status = reader["status"].ToString(),
                                Description = reader["Description"].ToString(),
                                AppointmentDate = Convert.ToDateTime(reader["AppointmentDate"]),
                                phoneno = (long)reader["phoneno"],
                                Email = reader["Email"].ToString()
                            };
                        }
                        reader.Close();
                    }
                }
            if (appmts != null)
            {
                return View(appmts);
            }

            return View();
        }


        [HttpPost, ActionName("Edit")]
        public IActionResult Edit(AppoinmentModel model, int id)
        {
            using (SqlConnection connection = new SqlConnection(_db.Database.GetConnectionString()))
            {
                connection.Open();
                string queryString = "UPDATE Appointments SET PatientName = @PatientName ,DoctorName = @DoctorName,status=@status,Description = @Description,AppointmentDate = @AppointmentDate,phoneno = @phoneno, Email = @Email WHERE AppoinmentId = @id";
                using (SqlCommand command = new SqlCommand(queryString, connection))
                {
                    
                    command.Parameters.AddWithValue("@id", id);
                    command.Parameters.AddWithValue("@PatientName", model.PatientName);
                    command.Parameters.AddWithValue("@DoctorName", model.DoctorName);
                    command.Parameters.AddWithValue("@status", model.status);
                    command.Parameters.AddWithValue("@Description", model.Description);
                    command.Parameters.AddWithValue("@AppointmentDate", model.AppointmentDate);
                    command.Parameters.AddWithValue("@phoneno", model.phoneno);
                    command.Parameters.AddWithValue("@Email", model.Email);
                    command.ExecuteNonQuery();
                }
            }
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            using (SqlConnection connection = new SqlConnection(_db.Database.GetConnectionString()))
            {
                connection.Open();
                string queryString = "DELETE FROM Appointments WHERE AppoinmentId = @id";
                using (SqlCommand command = new SqlCommand(queryString, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                   
                    command.ExecuteNonQuery();
                }
            }
            return RedirectToAction("Index");
        }


    }
}