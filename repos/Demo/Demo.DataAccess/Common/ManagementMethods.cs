using Demo.DataAccess.Data;
using Demo.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Demo.DataAccess.Common
{
    public class ManagementMethods
    {
        private readonly AppDbContext _db;
        public ManagementMethods(AppDbContext db)
        {
            _db = db;
        }
        public List<Hospital> GetHospitalList()
        {
            List<Hospital> hospitals = new List<Hospital>();
            using (SqlConnection connection = new SqlConnection(_db.Database.GetConnectionString()))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("Hospital_SelectAll", connection);
                command.CommandType = CommandType.StoredProcedure;
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Hospital hospital = new Hospital()
                    {
                        HospitalId = (int)(reader["HospitalId"]),
                        blnActive = (bool)(reader["blnActive"]),
                        Description = reader["Description"].ToString(),
                        HospitalName = reader["HospitalName"].ToString()
                    };

                    hospitals.Add(hospital);
                }
            }
            return hospitals;
        }
        public List<Hospital> GetHospitalListFiltered()
        {
            List<Hospital> hospitals = new List<Hospital>();
            using (SqlConnection connection = new SqlConnection(_db.Database.GetConnectionString()))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("Hospital_FilterByAdmin", connection);
                command.CommandType = CommandType.StoredProcedure;
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Hospital hospital = new Hospital()
                    {
                        HospitalId = (int)(reader["HospitalId"]),
                        HospitalName = reader["HospitalName"].ToString()
                    };

                    hospitals.Add(hospital);
                }
            }
            return hospitals;
        }
        public List<Management_Admin> GetManagementsAll()
        {
            List<Management_Admin> managements = new List<Management_Admin>();
            using (SqlConnection connection = new SqlConnection(_db.Database.GetConnectionString()))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("Managements_SelectAll", connection);
                command.CommandType = CommandType.StoredProcedure;
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Management_Admin hospital = new Management_Admin()
                    {

                        ManagementName = reader["HospitalName"].ToString(),
                        HospitalName = reader["HospitalName"].ToString(),
                        HospitalId= (int)(reader["HospitalId"]),
                        ManagementId = (int)(reader["ManagementId"]),
                        blnActive = (bool)reader["blnActive"]
                    };

                    managements.Add(hospital);
                }
            }
            return managements;
        }
        public List<DoctorType> GetDoctorTypeList(int hospitalId)
        {
            List<DoctorType> doctorTypes = new List<DoctorType>();

            using (SqlConnection connection = new SqlConnection(_db.Database.GetConnectionString()))
            {
                SqlCommand command = new SqlCommand("GetDoctorTypesByHospital", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@HospitalId", hospitalId);

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    DoctorType doctorType = new DoctorType
                    {
                        DoctorTypeId = Convert.ToInt32(reader["DoctorTypeId"]),
                        DoctorTypeName = reader["DoctorType"].ToString()
                    };

                    doctorTypes.Add(doctorType);
                }

                reader.Close();
            }
            return doctorTypes;
        }
        public List<DoctorDetails> GetDoctorList(int DoctorId, int hospitalId)
        {
            List<DoctorDetails> doctors = new List<DoctorDetails>();
            using (SqlConnection connection = new SqlConnection(_db.Database.GetConnectionString()))
            {
                SqlCommand command = new SqlCommand("Doctor_ByDocType", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@DoctorTypeId", DoctorId);
                command.Parameters.AddWithValue("@HospitalId", hospitalId);

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    DoctorDetails doctor = new DoctorDetails
                    {
                        DoctorID = Convert.ToInt32(reader["DoctorID"]),
                        DoctorName = reader["DoctorName"].ToString(),

                    };

                    doctors.Add(doctor);
                }

                reader.Close();
            }
            return doctors;
        }

        public int CreateNewDoctor(DoctorDetails doctorDetailModel)
        {

            using (SqlConnection connection = new SqlConnection(_db.Database.GetConnectionString()))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("Doctor_CreateNewDoctor", connection))
                {
                    var result = new SqlParameter("@result", SqlDbType.VarChar, 50);
                    result.Direction = ParameterDirection.Output;
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@DoctorName", doctorDetailModel.DoctorName);
                    command.Parameters.AddWithValue("@HospitalId", doctorDetailModel.HospitalId);
                    command.Parameters.AddWithValue("@Status", doctorDetailModel.Status);
                    command.Parameters.AddWithValue("@DoctorTypeId", doctorDetailModel.DoctorTypeID);
                    command.Parameters.AddWithValue("@Email", doctorDetailModel.Email);
                    command.Parameters.AddWithValue("@Password", doctorDetailModel.Password);
                    command.Parameters.AddWithValue("@Age", doctorDetailModel.Age);
                    command.Parameters.AddWithValue("@DateOfBirth", doctorDetailModel.DateOfBirth);
                    command.Parameters.AddWithValue("@PhoneNo", doctorDetailModel.PhoneNo);
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

        public int EditDoctorById(DoctorDetails doctorDetailModel, int id)
        {
            using (SqlConnection connection = new SqlConnection(_db.Database.GetConnectionString()))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("Doctor_UpdateDoctor", connection))
                {
                    var result = new SqlParameter("@result", SqlDbType.VarChar, 50);
                    result.Direction = ParameterDirection.Output;
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@DoctorName", doctorDetailModel.DoctorName);
                    command.Parameters.AddWithValue("@HospitalId", doctorDetailModel.HospitalId);
                    command.Parameters.AddWithValue("@Status", doctorDetailModel.Status);
                    command.Parameters.AddWithValue("@DoctorTypeId", doctorDetailModel.DoctorTypeID);
                    command.Parameters.AddWithValue("@Email", doctorDetailModel.Email);
                    command.Parameters.AddWithValue("@Age", doctorDetailModel.Age);
                    command.Parameters.AddWithValue("@DateOfBirth", doctorDetailModel.DateOfBirth);
                    command.Parameters.AddWithValue("@PhoneNo", doctorDetailModel.PhoneNo);
                    command.Parameters.AddWithValue("@UserId", doctorDetailModel.UserId);
                    command.Parameters.AddWithValue("@Id", id);
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
        public DoctorDetails GetHospitalandDoctorTypeList(int id)
        {
            DoctorDetails model = new DoctorDetails();
            using (SqlConnection connection = new SqlConnection(_db.Database.GetConnectionString()))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("Hospital_SelectAll", connection))

                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@HospitalId", id);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<Hospital> modell = new List<Hospital>();
                        while (reader.Read())
                        {
                            Hospital h = new Hospital();
                            h.HospitalName = reader["HospitalName"].ToString();
                            h.HospitalId = (int)reader["HospitalId"];
                            modell.Add(h);
                        }
                        model.HospitalList = modell;
                    }
                }
                using (SqlCommand command = new SqlCommand("DoctorType_GetAllDoctor", connection))

                {
                    command.CommandType = CommandType.StoredProcedure;

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<DoctorType> doctorType = new List<DoctorType>();
                        while (reader.Read())
                        {
                            DoctorType d = new DoctorType();
                            d.DoctorTypeId = (int)reader["DoctorTypeId"];
                            d.DoctorTypeName = reader["DoctorType"].ToString();
                            doctorType.Add(d);
                        }
                        model.DoctorTypeList = doctorType;
                    }
                }
            }
            return model;
        }

        public DoctorDetails EditDocotrGetData(int id)
        {
            DoctorDetails doctors = new DoctorDetails();
            using (SqlConnection connection = new SqlConnection(_db.Database.GetConnectionString()))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("Doctor_SelectById", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Id", id);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            doctors.DoctorID = (int)reader["DoctorID"];
                            doctors.DoctorName = reader["DoctorName"].ToString();
                            doctors.HospitalId = (int)reader["HospitalId"];
                            doctors.Status = (bool)reader["blnActive"];
                            doctors.HospitalName = reader["HospitalName"].ToString();
                            doctors.DoctorType = reader["DoctorType"].ToString();
                            doctors.DoctorTypeID = (int)reader["DoctorTypeId"];
                            doctors.Email = reader["Email"].ToString();
                            doctors.PhoneNo = reader["PhoneNo"].ToString();
                            doctors.DateOfBirth = (DateTime)reader["DateOfBirth"];
                            doctors.Age = (int)reader["Age"];
                            doctors.UserId = (int)reader["UserId"];
                        }
                    }
                }
            }
            return doctors;
        }

        public void RemoveDoctor(int id) //Appoinment_DeleteByUser
        {
            using (SqlConnection connection = new SqlConnection(_db.Database.GetConnectionString()))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("Doctor_Delete", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@DoctorID", id);

                    command.ExecuteNonQuery();
                }
            }
        }

        // 26 June - Create New Appoinment Method
        public int CreateNewAppoinment(PatientAppoinmentModel model)
        {
            using (SqlConnection connection = new SqlConnection(_db.Database.GetConnectionString()))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("Appointment_ChkAndInsert", connection))
                {
                    var result = new SqlParameter("@result", SqlDbType.VarChar, 50);
                    result.Direction = ParameterDirection.Output;
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@AppointmentDate", model.AppointmentDate);
                    command.Parameters.AddWithValue("@DiseaseDescriptions", model.DiseaseDescriptions);
                    command.Parameters.AddWithValue("@HospitalID", model.HospitalID);
                    command.Parameters.AddWithValue("@PatientId", model.PatientId);
                    command.Parameters.AddWithValue("@DoctorID", model.DoctorID);
                    command.Parameters.AddWithValue("@DoctorTypeId", model.DoctorTypeId);
                    command.Parameters.AddWithValue("@AppoinmentTime", model.AppointmentTime);
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
        // 27 June - Edit Methods

        public PatientAppoinmentModel GetAppoinmentData(int id)
        {
            if (id <= 0)
            {
                return null;
            }

            var appData = new PatientAppoinmentModel();
            using (SqlConnection connection = new SqlConnection(_db.Database.GetConnectionString()))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("Appointment_ByAppId", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@ID", id);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            appData.AppointmentID = (int)reader["AppointmentID"];
                            appData.HospitalID = (int)reader["HospitalId"];
                            appData.DoctorID = (int)reader["DoctorID"];
                            appData.DoctorTypeId = (int)reader["DoctorTypeId"];
                            appData.AppointmentDate = (DateTime)reader["AppointmentDate"];
                            appData.AppointmentTime = reader["AppoinmentTime"].ToString();
                            appData.DiseaseDescriptions = reader["DiseaseDescriptions"].ToString();
                            appData.PatientId = (int)reader["PatientId"];
                        }
                    }
                }
            }
            return appData;
        }
        public int EditAppoinment(PatientAppoinmentModel model, int id)
        {
            if (id <= 0)
            {
                return 0;
            }

            using (SqlConnection connection = new SqlConnection(_db.Database.GetConnectionString()))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("Appointment_ChkAndEdit", connection))
                {
                    var result = new SqlParameter("@result", SqlDbType.VarChar, 50);
                    result.Direction = ParameterDirection.Output;
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@ID", id);
                    command.Parameters.AddWithValue("@AppointmentDate", model.AppointmentDate);
                    command.Parameters.AddWithValue("@DiseaseDescriptions", model.DiseaseDescriptions);
                    command.Parameters.AddWithValue("@HospitalID", model.HospitalID);
                    command.Parameters.AddWithValue("@PatientId", model.PatientId);
                    command.Parameters.AddWithValue("@DoctorID", model.DoctorID);
                    command.Parameters.AddWithValue("@DoctorTypeId", model.DoctorTypeId);
                    command.Parameters.AddWithValue("@AppoinmentTime", model.AppointmentTime);
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
        // 28 June - Fetch Appoinments For User Module
        public List<PatientAppoinmentModel> GetUserAppoinmentData(int userId)
        {
            List<PatientAppoinmentModel> model = new List<PatientAppoinmentModel>();

            using (SqlConnection connection = new SqlConnection(_db.Database.GetConnectionString()))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("Appoinments_ByUserId", connection))
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
                                PatientId = (int)reader["PatientId"],
                                DoctorName = reader["DoctorName"].ToString(),
                                AppointmentStatus = reader["AppointmentStatus"].ToString(),
                                HospitalName = reader["HospitalName"].ToString(),
                                DoctorType = reader["DoctorType"].ToString(),
                                UserName = reader["UserName"].ToString(),
                                AppointmentDate = (DateTime)reader["AppointmentDate"],
                                AppointmentTime = reader["AppoinmentTime"].ToString()
                            };

                            model.Add(appoinments);
                        }

                        reader.Close();
                    }
                }

            }

            return model;
        }
        public void RemoveAppoinment(int id) 
        {
            using (SqlConnection connection = new SqlConnection(_db.Database.GetConnectionString()))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("Appoinment_DeleteByUser", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@AppId", id);

                    command.ExecuteNonQuery();
                }   
            }
        }
        
        public List<DoctorType> GetDoctorTypeListAll()
        {
            List<DoctorType> dt = new List<DoctorType>();

            using (SqlConnection connection = new SqlConnection(_db.Database.GetConnectionString()))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("DoctorType_GetAllDoctor", connection))
                {

                    command.CommandType = CommandType.StoredProcedure;
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DoctorType data = new DoctorType
                            {
                                DoctorTypeName = reader["DoctorType"].ToString(),
                                DoctorTypeId = (int)reader["DoctorTypeID"]
                            };

                            dt.Add(data);
                        }

                        reader.Close();
                    }
                }

            }
            return dt;
        }
    }
}



