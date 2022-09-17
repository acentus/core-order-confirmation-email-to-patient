//
// Copyright (c) 2022 by Acentus
// Developed by: Luis Cabrera
// gigocabrera@outlook.com
//

using System;
using System.Threading.Tasks;

namespace CoreOrderConfirmationEmailToPatient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                Log.write("************************ JOB STARTED ******************************");
                Report report = new Report();
                await report.RunReport();
            }
            catch (Exception ex)
            {
                Log.write("EXCEPTION ******************************");
                Log.write(ex.Message);
            }
        }
    }
}