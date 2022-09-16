//
// Copyright (c) 2022 by Acentus
// Developed by: Luis Cabrera
// gigocabrera@outlook.com
//

using System;
using System.IO;

namespace CoreOrderConfirmationEmailToPatient
{
    internal class Log
    {
        public static void write(string str)
        {
            string logfile = "";
            string filename = "CoreOrderConfirmationEmailToPatient_Log_" + DateTime.Now.Year.ToString() + "_" + DateTime.Now.Month.ToString() + "_" + DateTime.Now.Day.ToString() + ".txt";
            string appPath = AppDomain.CurrentDomain.BaseDirectory + "logs";
            logfile = Path.Combine(appPath, filename);

            try
            {
                if (!Directory.Exists(logfile))
                {
                    //App.writetoLog("Path does not exist. Creating directory.");
                    DirectoryInfo di = Directory.CreateDirectory(appPath);
                }

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
