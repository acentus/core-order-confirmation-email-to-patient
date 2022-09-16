//
// Copyright (c) 2022 by Acentus
// Developed by: Luis Cabrera
// gigocabrera@outlook.com
//

using System;
using System.Configuration;

namespace CoreOrderConfirmationEmailToPatient
{
    public static class App
    {
        private static string testDateStart = ConfigurationManager.AppSettings["TestDateStart"];
        private static string testDateEnd = ConfigurationManager.AppSettings["TestDateEnd"];
        private static DateTime todayDate = DateTime.Today;
        private static DateTime tomorrowDate = DateTime.Today.AddDays(1);

        static App()
        {
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
        }

        public static string DateSpan { get; private set; } = string.Empty;
        public static DateTime StartDate { get; private set; }
        public static DateTime EndDate { get; private set; }
    }
}