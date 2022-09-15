﻿//
// Copyright (c) 2022 by Acentus
// Developed by: Luis Cabrera
// gigocabrera@outlook.com
//

using System;

namespace OrderConfirmationEmailToPatient
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                App.Log("************************ JOB STARTED ******************************");
                Report report = new Report();
                report.SendEmail();        
            }
            catch (Exception ex)
            {
                App.Log("EXCEPTION ******************************");
                App.Log(ex.Message);
            }
        }
    }
}