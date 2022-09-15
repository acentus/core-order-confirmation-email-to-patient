//
// Copyright (c) 2020 by Acentus
// Developed by: Luis Cabrera
// gigocabrera@outlook.com
//

using System;
using System.IO;
using System.Configuration;

namespace OrderConfirmationEmailToPatient
{
    public static class App
    {
        private static string logPath = ConfigurationManager.AppSettings["rootlocation"];
        private static string testDateStart = ConfigurationManager.AppSettings["TestDateStart"];
        private static string testDateEnd = ConfigurationManager.AppSettings["TestDateEnd"];
        private static DateTime todayDate = DateTime.Today;
        private static DateTime tomorrowDate = DateTime.Today.AddDays(1);

        static App()
        {
            if (!Directory.Exists(logPath))
            {
                DirectoryInfo di = Directory.CreateDirectory(logPath);
            }

            string ForceReportDate = System.Configuration.ConfigurationManager.AppSettings["ForceReportDate"];
            if (ForceReportDate == "True")
            {
                StartDate = Convert.ToDateTime(System.Configuration.ConfigurationManager.AppSettings["TestDateStart"]);
                EndDate = Convert.ToDateTime(System.Configuration.ConfigurationManager.AppSettings["TestDateEnd"]);
            }
            else
            {
                StartDate = todayDate;
                EndDate = tomorrowDate;
            }
            DateSpan = StartDate.ToShortDateString() + " - " + EndDate.ToShortDateString();
            App.Log("Start span date: " + DateSpan);
        }

        public static string DateSpan { get; private set; } = string.Empty;
        public static DateTime StartDate { get; private set; }
        public static DateTime EndDate { get; private set; }

        public static void Log(string str)
        {
            string logfile = "";
            string filename = "OrderConfirmationEmailToPatient_Log_" + DateTime.Now.Year.ToString() + "_" + DateTime.Now.Month.ToString() + "_" + DateTime.Now.Day.ToString() + ".txt";
            logfile = Path.Combine(logPath, filename);

            try
            {
                using (System.IO.StreamWriter outfile = new System.IO.StreamWriter(logfile, true))
                {
                    outfile.WriteLine(DateTime.Now + " : " + str);
                    outfile.Close();
                    outfile.Dispose();
                }
            }
            catch
            {
            }
        }
    }
}
