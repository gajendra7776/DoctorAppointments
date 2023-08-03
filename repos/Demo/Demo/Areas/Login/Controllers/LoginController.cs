using Demo.DataAccess.Common;
using Demo.DataAccess.Data;
using Demo.Models;
using Demo.Models.DummyModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Net.Mail;
using System.Security.Claims;

namespace Demo.Controllers
{
    [Area("Login")]
    public class LoginController : Controller
    {
        private readonly AppDbContext _db;
        private readonly CommonMethods _common;
        public LoginController(AppDbContext db, CommonMethods common)
        {
            _db = db;
            _common = common;
        }
        
        public IActionResult PageNotFound()
        {
            return View();
        }
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost, ActionName("Register")]
        public IActionResult Register(UserModel model)
        {
           
            var data = _db.User.Where(x => x.Email == model.Email).FirstOrDefault();
            if (data != null)
            {
                ViewData["Message"] = "User Already Exists!";
                return View(model);
            }
            else
            {
                using (SqlConnection connection = new SqlConnection(_db.Database.GetConnectionString()))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("User_RegisterNewUser", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@UserName", model.UserName);
                        command.Parameters.AddWithValue("@Email", model.Email);
                        command.Parameters.AddWithValue("@Password", model.Password);
                        command.ExecuteNonQuery();
                    }
                }
                return RedirectToAction("Login");
            }
            return View(model);
        }
        [HttpPost, ActionName("Login")]
        public async Task<IActionResult> Login(UserModel model)
        {
            var user = _db.User.FirstOrDefault(x => x.Email == model.Email && x.Password == model.Password);
            if(user == null)
            {
                TempData["Invalid"] = "Invalid Credentials";
                return View(model);
            }
            var chkManage = _db.Management_Admin.FirstOrDefault(m => m.UserId == user.UserId && m.blnActive == true);
            var chkDoc = _db.DoctorDetails.Where(m => m.UserId == user.UserId && m.blnActive == true).FirstOrDefault();
            var chkHospital=new Hospital();
            chkHospital = null;
            if (chkManage != null)
            {
                 chkHospital = _db.Hospital.Where(h => h.HospitalId == chkManage.HospitalId && h.blnActive == true).FirstOrDefault();
            }

            if (user != null)
            {
                if (user.RoleID == 3 && chkHospital != null )
                {

                    List<Claim> claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, "ManagementAdmin")
                };
                    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    AuthenticationProperties authenticationProperties = new AuthenticationProperties()
                    {
                        AllowRefresh = true,
                    };
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity), authenticationProperties);
                    int id = chkManage.HospitalId;
                    HttpContext.Session.SetInt32("ManagementAdminId", chkManage.HospitalId);
                    HttpContext.Session.SetInt32("ManagementForStatus", user.UserId);
                    HttpContext.Session.SetString("ManagementAdminName", user.UserName);
                    HttpContext.Session.SetString("HospitalName", chkHospital.HospitalName);
                    TempData["LoginSuccess"] = "Login Successful";
                    return RedirectToAction("DisplayDoctor", "Management", new { area = "Management", hospitalId = chkManage.HospitalId });
                }
                else if (user.RoleID == 1)
                {

                    List<Claim> claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, "User")
                };
                    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    AuthenticationProperties authenticationProperties = new AuthenticationProperties()
                    {
                        AllowRefresh = true,

                    };
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity), authenticationProperties);
                    HttpContext.Session.SetInt32("UserId", user.UserId);
                    HttpContext.Session.SetString("UserName", user.UserName);
                    TempData["LoginSuccess"] = "Login Successful";
                    return RedirectToAction("BookAppoint", "User", new { area = "User", userId = user.UserId });
                }
                else if (user.RoleID == 2 && chkDoc != null)
                {
                    var hospital = _db.Hospital.Where(x => x.HospitalId == chkDoc.HospitalId && x.blnActive==true).FirstOrDefault();
                    var management = _db.Management_Admin.Where(x => x.HospitalId == chkDoc.HospitalId && x.blnActive == true).FirstOrDefault();

                    if (hospital == null || management == null)
                    {
                        HttpContext.Session.SetInt32("HospitalFlag3", 3);
                    }
                    List<Claim> claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, "Doctor")
                };
                    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    AuthenticationProperties authenticationProperties = new AuthenticationProperties()
                    {
                        AllowRefresh = true,

                    };
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity), authenticationProperties);
                    HttpContext.Session.SetInt32("DoctorId", chkDoc.DoctorID);
                    HttpContext.Session.SetInt32("DoctorForStatus", user.UserId);
                    HttpContext.Session.SetString("DoctorName", user.UserName);
                    TempData["LoginSuccess"] = "Login Successful";
                    return RedirectToAction("AppoinmentsByDoctor", "Management", new { area = "Management", doctorId = chkDoc.DoctorID });
                }
                else if (user.RoleID == 4)
                {
                    {
                        List<Claim> claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, "SuperAdmin")
                };
                        ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        AuthenticationProperties authenticationProperties = new AuthenticationProperties()
                        {
                            AllowRefresh = true,
                        };
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity), authenticationProperties);
                        HttpContext.Session.SetInt32("SuperAdminId", user.UserId);
                        DateTime today = DateTime.Today;
                        int data = _db.Patient_Appoinments.Where(x => x.AppointmentDate == today).Count();
                        HttpContext.Session.SetInt32("total", data);
                        HttpContext.Session.SetString("SuperAdminName", user.UserName);
                        TempData["LoginSuccess"] = "Login Successful";
                        return RedirectToAction("DisplayDoctor", "Management", new { area = "Management" });
                    }
                }
                if(user.RoleID == 2)
                {
                    TempData["Invalid"] = "Doctor is  Inactive";
                    return View(model);
                }
            
                else if(user.RoleID == 3 && chkManage == null)
                {
                    TempData["Invalid"] = "Management is  Inactive";
                    return View(model);
                }
                else if(user.RoleID == 3 && chkHospital == null)
                {
                    TempData["Invalid"] = "Hospital is  Inactive";
                    return View(model);
                }
            }

            TempData["Invalid"] = "Invalid Credentials";
            return View(model);
        }
        public IActionResult Login()
        {
            //ClaimsPrincipal claimUser = HttpContext.User;
            //if (claimUser.Identity.IsAuthenticated)
            //    return RedirectToAction("DoctorPanel", "Doctor", new { area = "Doctor" });
            return View();
        }
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Remove("ManagementAdminId");
            HttpContext.Session.Remove("DoctorForStatus");
            HttpContext.Session.Remove("ManagementForStatus");
            HttpContext.Session.Remove("DoctorIdForSAandAdmin");
            HttpContext.Session.Remove("DoctorId");
            HttpContext.Session.Remove("UserName");
            HttpContext.Session.Remove("DoctorName");
            HttpContext.Session.Remove("UserId");
            HttpContext.Session.Remove("HospitalFlag3");
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Login", new { area = "Login" });
        }

        public void SendNotification()
        {
            var id = int.Parse(HttpContext.Session.GetString("sessionUserid"));
            var user = _db.User.FirstOrDefault(x => x.UserId == id);
            var from_email = new MailAddress("gajjulabana799@gmail.com", "Gajendra");
            var to_mail = new MailAddress(user.Email);
            var subject = user.UserName + "You Put Null Value in this Column";
            var body = "Please Fill Valid Data";
            var message = new MailMessage(from_email, to_mail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            var smtpClient = new SmtpClient("smtp.gmail.com", 587);
        }
       

    }
}
