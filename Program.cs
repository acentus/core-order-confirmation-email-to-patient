//
// Copyright (c) 2022 by Acentus
// Developed by: Luis Cabrera
// gigocabrera@outlook.com
//

using System;

namespace CoreOrderConfirmationEmailToPatient
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Log.write("************************ JOB STARTED ******************************");
                Report report = new Report();
                report.SendEmail();
            }
            catch (Exception ex)
            {
                Log.write("EXCEPTION ******************************");
                Log.write(ex.Message);
            }
        }
    }
}