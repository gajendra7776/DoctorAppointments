using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
using System.Net.Mail;

namespace Demo.Controllers
{
    
    public class ErrorController : Controller
    {

        public class CustomExceptionFilter : ExceptionFilterAttribute, IExceptionFilter
        {
            public override void OnException(ExceptionContext context)
            {

                SendErrorEmail();
                context.Result = new ViewResult
                {
                    ViewName = "ErrorHandle"
                };

                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                context.ExceptionHandled = true;

            }

            private void SendErrorEmail()
            {
                MailMessage mail = new MailMessage();
                var smtpClient = new SmtpClient("smtp.gmail.com", 587)
                {
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential("gajjulabana799@gmail.com", "kfcuhlzbqbhhsldu"),
                    EnableSsl = true
                };

                mail.From = new MailAddress("gajjulabana799@gmail.com");
                mail.To.Add("patelchintan3171@gmail.com");
                mail.Subject = "Error occurred in the application";
                mail.Body = "An error occurred in the application. Please check the logs.";

                smtpClient.Send(mail);
            }

        }
        

    }
}
