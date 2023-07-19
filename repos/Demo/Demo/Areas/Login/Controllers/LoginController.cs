using Demo.DataAccess.Common;
using Demo.DataAccess.Data;
using Demo.Models;
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
        public IActionResult Login()
        {
            ClaimsPrincipal claimUser = HttpContext.User;
            if (claimUser.Identity.IsAuthenticated)
                return RedirectToAction("DoctorPanel", "Doctor", new { area = "Doctor" });
            return View();
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
            var chkManage = _db.Management_Admin.FirstOrDefault(m => m.UserId == user.UserId);
            var chkDoc = _db.DoctorDetails.Where(m => m.UserId == user.UserId).FirstOrDefault();

            if (user.RoleID == 3 && (_db.Management_Admin.Any(m => m.UserId == user.UserId)))
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
                TempData["LoginSuccess"] = "Login Successful";
                return RedirectToAction("DisplayDoctor", "Management", new { area = "Management", hospitalId = chkManage.HospitalId });
            }

            if (user != null)
            {
                if (user.RoleID == 1)
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
                    TempData["LoginSuccess"] = "Login Successful";
                    return RedirectToAction("BookAppoint", "User", new { area = "User", userId = user.UserId });
                }
                else if (user.RoleID == 2)
                {
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

                        DateTime today = DateTime.Today;
                        int data = _db.Patient_Appoinments.Where(x => x.AppointmentDate == today).Count();
                        HttpContext.Session.SetInt32("total", data);

                        TempData["LoginSuccess"] = "Login Successful";
                        return RedirectToAction("DisplayDoctor", "Management", new { area = "Management" });
                    }
                }
            }

            TempData["Invalid"] = "Invalid Credentials";
            return View(model);
        }
        public async Task<IActionResult> Logout()
        {
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
