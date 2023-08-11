using Demo.DataAccess.Data;
using Demo.Models;
using Demo.Models.DummyModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Demo.DataAccess.Common
{
    public class CommonMethods
    {
        private readonly AppDbContext _db;
        public CommonMethods(AppDbContext db)
        {
            _db = db;
        }

        public List<DoctorDetails> GetDoctorsByManagement(int hospitalId)
        {
            List<DoctorDetails> managementList = new List<DoctorDetails>();

            using (SqlConnection connection = new SqlConnection(_db.Database.GetConnectionString()))
            {
                SqlCommand command = new SqlCommand("Doctor_SelectByManagement", connection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@HospitalId", hospitalId);

                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DoctorDetails management = new DoctorDetails();
                        management.DoctorID = (int)reader["DoctorID"];
                        management.DoctorName = reader["DoctorName"].ToString();
                        management.HospitalNameForDoctor = reader["HospitalName"].ToString();
                        management.DoctorType = reader["DoctorType"].ToString();
                        management.Status = Convert.ToBoolean(reader["blnActive"]);
                        management.HospitalId = (int)reader["HospitalId"];
                        managementList.Add(management);
                    }
                }
            }
            return managementList;
        }
        public List<PatientAppoinmentModel> GetAppointmentsByManagement(int hospitalId)
        {
            List<PatientAppoinmentModel> appointmentList = new List<PatientAppoinmentModel>();

            using (SqlConnection connection = new SqlConnection(_db.Database.GetConnectionString()))
            {
                SqlCommand command = new SqlCommand("Appointments_SelectByManagement", connection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@HospitalId", hospitalId);

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
                        appointment.AppointmentDate = (DateTime)reader["AppointmentDate"];
                        appointment.AppointmentTime = reader["AppoinmentTime"].ToString();
                        appointmentList.Add(appointment);
                    }
                }
            }

            return appointmentList;
        }

        public List<PatientAppoinmentModel> GetAppointmentsByDoctor(int doctorId)
        {
            List<PatientAppoinmentModel> appointmentList = new List<PatientAppoinmentModel>();

            using (SqlConnection connection = new SqlConnection(_db.Database.GetConnectionString()))
            {
                SqlCommand command = new SqlCommand("Appoinments_ByDoctorId", connection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@DoctorId", doctorId);

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
        public string UpdateAppointmentStatus(int id, string status,int approveId,int rejectId)
        {

            using (SqlConnection connection = new SqlConnection(_db.Database.GetConnectionString()))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("Appointment_UpdateStatus", connection))
                {

                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@id", id);
                    command.Parameters.AddWithValue("@status", status);
                    command.Parameters.AddWithValue("@approveId", approveId);
                    command.Parameters.AddWithValue("@rejectId", rejectId);
                    command.ExecuteNonQuery();
                }
            }

            return status;
            
        }
        public List<PatientAppoinmentModel> GetData(int doctorId, DateTime date)
        {
            if (doctorId <= 0)
            {
                return null;
            }
            List<PatientAppoinmentModel> managementList = new List<PatientAppoinmentModel>();

            using (SqlConnection connection = new SqlConnection(_db.Database.GetConnectionString()))
            {
                SqlCommand command = new SqlCommand("Appointments_ByDoctorANDdate", connection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@DoctorId", doctorId);
                command.Parameters.AddWithValue("@date", date);

                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        PatientAppoinmentModel management = new PatientAppoinmentModel();
                        management.AppointmentID = (int)reader["AppointmentID"];
                        management.UserName = reader["UserName"].ToString();
                        management.AppointmentTime = reader["AppoinmentTime"].ToString();
                        management.AppointmentStatus = reader["AppointmentStatus"].ToString();
                        management.PatientId = (int)reader["PatientId"];
                        managementList.Add(management);
                    }
                }
            }
            return managementList;
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
                        appointment.Prescription = reader["Prescription"].ToString();
                        appointment.Suggestions = reader["Suggestions"].ToString();
                        appointment.AppointmentDate = (DateTime)reader["AppointmentDate"];
                        appointment.DateOfBirth = (DateTime)reader["DateOfBirth"];
                        if (reader["Approved_Date"] != DBNull.Value)
                        {
                            appointment.ApproveDate = (DateTime)reader["Approved_Date"];
                        }
                        appointment.AppointmentTime = reader["AppoinmentTime"].ToString();

                    }
                }
            }

            return appointment;
        }
    }
}
