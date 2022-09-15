//
// Copyright (c) 2020 by Acentus
// Developed by: Luis Cabrera
// gigocabrera@outlook.com
//

using EmailSenderService;
using System;
using System.Configuration;
using System.IO;
using System.Net.Mail;
using System.Reflection;

namespace OrderConfirmationEmailToPatient
{
    /// <summary>
    /// This abstract class provides commonly used utility methods
    /// </summary>
    public abstract class Utils
    {
        #region Conversion Methods

        public static int StringToInt(string value)
        {
            try
            {
                return System.Convert.ToInt32(value);
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public static bool IsEmpty(string value)
        {
            return ((value == null) || (value.Trim().Equals(String.Empty)));
        }

        #endregion

        #region FormatDate(DateTime date)

        public static string FormatDate(DateTime date)
        {
            return FormatDate(date, "/");
        }

        #endregion

        #region FormatDate(DateTime date, string separator)

        public static string FormatDate(DateTime date, string separator)
        {
            return string.Format("{0:D4}" + separator + "{1:D2}" + separator + "{2:D2}", date.Year, date.Month, date.Day);
        }

        #endregion

        #region FormatTime(DateTime date)

        public static string FormatTime(DateTime date)
        {
            return FormatTime(date, ":");
        }

        #endregion

        #region FormatTime(DateTime date, string separator)

        public static string FormatTime(DateTime date, string separator)
        {
            return string.Format("{0:D2}" + separator + "{1:D2}" + separator + "{2:D2}", date.Hour, date.Minute, date.Second);
        }

        #endregion

        #region FormatDateTime(DateTime date)

        public static string FormatDateTime(DateTime date)
        {
            return FormatDate(date) + " " + FormatTime(date);
        }

        #endregion

        #region FormatDateTime(DateTime date, string dateSeparator, string timeSeparator)

        public static string FormatDateTime(DateTime date, string dateSeparator, string timeSeparator)
        {
            return FormatDate(date, dateSeparator) + " " + FormatTime(date, timeSeparator);
        }

        #endregion

        #region DateTimeToString(DateTime dt)

        public static string DateTimeToString(DateTime dt)
        {
            return (dt == DateTime.MinValue) ? string.Empty : dt.ToString("MM/dd/yyyy");
        }

        #endregion

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
                App.Log("Reorder email sent successfully to : " + id + " - " + emailTo);
            }
            catch (Exception ex)
            {
                App.Log("EXCEPTION : " + ex.Message);
            }
        }

        public static async void SendEmailWithModernAuthentication(string id, string EmailBody, string EmailSubject, string emailPatient)
        {            
            try
            {
                string emailTo = ConfigurationManager.AppSettings["emailTo"];
                string emailFrom = ConfigurationManager.AppSettings["emailFrom"];

                if (id != "0000")
                {
                    emailTo = emailPatient;
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
                await MSGraphApiService.GetInstance(appConfig).SendEmail(EmailSubject, EmailBody, emailFrom, emailTo);
                App.Log("Reorder email sent successfully to : " + id + " - " + emailTo);
            }
            catch (Exception ex)
            {
                App.Log("EXCEPTION : " + ex.Message);
            }
        }

        #endregion
    }
}
