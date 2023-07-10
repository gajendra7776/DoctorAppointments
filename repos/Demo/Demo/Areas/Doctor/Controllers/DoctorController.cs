using Demo.DataAccess.Common;
using Demo.DataAccess.Data;
using Demo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
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

    }
}
