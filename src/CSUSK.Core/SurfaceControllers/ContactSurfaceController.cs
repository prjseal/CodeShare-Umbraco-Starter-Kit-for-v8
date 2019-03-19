using CSUSK.Core.ViewModels;
using System.Net.Mail;
using System.Reflection;
using System.Web.Mvc;
using Umbraco.Web.Mvc;

namespace CSUSK.Core.SurfaceControllers
{
    public class ContactController : SurfaceController
    {
        public string GetViewPath(string name)
        {
            return string.Format("/Views/Partials/Contact/{0}.cshtml", name);
        }

        [HttpGet]
        public ActionResult RenderForm()
        {
            ContactViewModel model = new ContactViewModel();
            return PartialView(GetViewPath("_ContactForm"), model);
        }

        [HttpPost]
        public ActionResult RenderForm(ContactViewModel model)
        {
            return PartialView(GetViewPath("_ContactForm"), model);
        }

        [HttpPost]
        public ActionResult SubmitForm(ContactViewModel model)
        {
            bool success = false;
            if (ModelState.IsValid)
            {
                success = SendEmail(model);
            }
            return PartialView(GetViewPath(success ? "_Success" : "_Error"));
        }

        public bool SendEmail(ContactViewModel model)
        {
            try
            {
                MailMessage message = new MailMessage();
                SmtpClient client = new SmtpClient();

                string toAddress = System.Web.Configuration.WebConfigurationManager.AppSettings["ContactEmailTo"];
                string fromAddress = System.Web.Configuration.WebConfigurationManager.AppSettings["ContactEmailFrom"];
                message.Subject = string.Format("Enquiry from: {0} - {1}", model.Name, model.Email);
                message.Body = model.Message;
                message.To.Add(new MailAddress(toAddress, toAddress));
                message.From = new MailAddress(fromAddress, fromAddress);

                client.Send(message);
                return true;
            }
            catch (System.Exception ex)
            {
                Logger.Error(MethodBase.GetCurrentMethod().DeclaringType, "Contact Form Error", ex);
                return false;
            }
        }
    }
}
