//
// Copyright (c) 2022 by Acentus
// Developed by: Luis Cabrera
// gigocabrera@outlook.com
//

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace CoreOrderConfirmationEmailToPatient
{
    /// <summary>
    /// This abstract class provides commonly used utility methods
    /// </summary>
    public abstract class Utils
    {
        #region GetTemplate

        public static string GetTemplate(string strTemplateName)
        {
            string loc = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string filepath = loc + "\\HtmlTemplates\\" + strTemplateName;
            string html = System.IO.File.ReadAllText(filepath);
            return html;
        }

        #endregion

        #region SendEmail

        public static void SendEmail(string id, string EmailBody, string EmailSubject, string emailPatient)
        {
            System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
            try
            {
                string host = ConfigurationManager.AppSettings["sendEmailHost"];
                string emailTo = ConfigurationManager.AppSettings["emailTo"];
                string emailFrom = ConfigurationManager.AppSettings["emailFrom"];
                string pwd = ConfigurationManager.AppSettings["pwd"];

                if (id != "0000")
                {
                    emailTo = emailPatient;
                }

                string testMode = System.Configuration.ConfigurationManager.AppSettings["TestMode"];
                if (testMode == "True")
                {
                    emailTo = System.Configuration.ConfigurationManager.AppSettings["TestEmailTo"];
                }

                var msg = new System.Net.Mail.MailMessage(emailFrom, emailTo, EmailSubject, EmailBody);
                msg.IsBodyHtml = true;
                var smtpClient = new System.Net.Mail.SmtpClient(host, 587);
                smtpClient.UseDefaultCredentials = true;
                smtpClient.Credentials = new System.Net.NetworkCredential(emailFrom, pwd);
                smtpClient.EnableSsl = true;
                smtpClient.Send(msg);
                Log.write("Reorder email sent successfully to : " + id + " - " + emailTo);
            }
            catch (Exception ex)
            {
                Log.write("EXCEPTION : " + ex.Message);
            }
        }

        public static async Task SendEmailWithModernAuthentication(string id, string EmailBody, string EmailSubject, string emailPatient)
        {            
            try
            {
                string emailTo = ConfigurationManager.AppSettings["emailTo"];
                string emailFrom = ConfigurationManager.AppSettings["emailFrom"];

                if (id != "0000")
                {
                    emailTo = emailPatient.Trim();
                }

                string testMode = System.Configuration.ConfigurationManager.AppSettings["TestMode"];
                if (testMode == "True")
                {
                    emailTo = System.Configuration.ConfigurationManager.AppSettings["TestEmailTo"];
                }
                AppConfig appConfig = new AppConfig
                {
                    AppId = ConfigurationManager.AppSettings["AppId"],
                    AppSecret = ConfigurationManager.AppSettings["AppSecret"],
                    TenantId = ConfigurationManager.AppSettings["TenantId"],
                };
                await MSGraphApiService.GetInstance(appConfig).SendEmail(EmailSubject, EmailBody, emailFrom, new List<string> { emailTo });
                Log.write("Reorder email sent successfully to : " + id + " - " + emailTo);
            }
            catch (Exception ex)
            {
                Log.write("EXCEPTION : " + ex.Message);
            }
        }

        #endregion
    }
}