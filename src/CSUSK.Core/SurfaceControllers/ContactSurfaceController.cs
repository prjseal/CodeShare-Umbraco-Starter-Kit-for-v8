using CSUSK.Core.Models;
using CSUSK.Core.ViewModels;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Web.Mvc;
using System.Web.Script.Serialization;
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
            ContactViewModel model = new ContactViewModel() { SiteKey = CaptchaSiteKey };
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
                if(!string.IsNullOrEmpty(CaptchaSecretKey))
                {
                    string captchaResponse = Request["g-recaptcha-response"];
                    if(!CaptchaIsValid(captchaResponse))
                    {
                        return PartialView(GetViewPath("_CaptchaError"));
                    }
                }

                success = SendEmail(model);
            }
            return PartialView(GetViewPath(success ? "_Success" : "_Error"));
        }

        private bool CaptchaIsValid(string captchaResponse)
        {
            string Response = Request["g-recaptcha-response"];//Getting Response String Append to Post Method
            bool Valid = false;
            //Request to Google Server

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://www.google.com/recaptcha/api/siteverify");

            string postData = $"secret={CaptchaSecretKey}&response={captchaResponse}";

            byte[] send = Encoding.Default.GetBytes(postData);
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.ContentLength = send.Length;

            Stream sout = req.GetRequestStream();
            sout.Write(send, 0, send.Length);
            sout.Flush();
            sout.Close();
                        
            try
            {
                //Google recaptcha Response
                using (WebResponse wResponse = req.GetResponse())
                {
                    using (StreamReader readStream = new StreamReader(wResponse.GetResponseStream()))
                    {
                        string jsonResponse = readStream.ReadToEnd();

                        JavaScriptSerializer js = new JavaScriptSerializer();
                        RecaptchaVerifyModel model = js.Deserialize<RecaptchaVerifyModel>(jsonResponse);// Deserialize Json

                        Valid = Convert.ToBoolean(model.success);
                    }
                }

                return Valid;
            }
            catch (WebException ex)
            {
                throw ex;
            }
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

        public string CaptchaSiteKey
        {
            get
            {
                return System.Web.Configuration.WebConfigurationManager.AppSettings["ContactCaptchaSiteKey"];
            }
        }

        public string CaptchaSecretKey
        {
            get
            {
                return System.Web.Configuration.WebConfigurationManager.AppSettings["ContactCaptchaSecretKey"];
            }
        }

    }
}
