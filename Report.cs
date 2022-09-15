﻿//
// Copyright (c) 2022 by Acentus
// Developed by: Luis Cabrera
// gigocabrera@outlook.com
//

using System;
using System.Data;

namespace OrderConfirmationEmailToPatient
{
    class Report
    {
        private DataAccess db = new DataAccess();

        public void SendEmail()
        {
            try
            {                
                // Get HTML template for report
                string htmlPatient = Utils.GetTemplate("EmailTemplate.html");
                string htmlList = Utils.GetTemplate("EmailList.html");

                // Get data
                DataTable dt = db.GetData();


                // Populate template
                PopulateTemplate(dt, ref htmlList);

               
            }
            catch (Exception ex)
            {
                App.Log(string.Format("EXCEPTION: " + ex.Message));
            }
        }

        private void PopulateTemplate(DataTable listItems, ref string htmlList)
        {
            int iTotal = 0;
            string tr = string.Empty;
            string reportDate = DateTime.Today.ToShortDateString();
            string strDetails = GetDetailsTemplate(ref htmlList);
            string strThisDetails = strDetails;
            string list1 = string.Empty;
            string thisHtml = htmlList;

            if (listItems != null && listItems.Rows.Count > 0)
            {
                App.Log("Records returned: " + listItems.Rows.Count.ToString());

                int prevPatientId = 0;
                string prevPONUmber = string.Empty;
                DateTime prevServiceDate = DateTime.MinValue;

                foreach (DataRow item in listItems.Rows)
                {
                    iTotal++;

                    int thisPatientId = item.Field<int>("PATIENTID");
                    string FullName = item.Field<string>("FULLNAME");
                    string thisPONumber = item.Field<string>("INVOICENO");
                    DateTime thisAddedDate = item.Field<DateTime>("ADDEDONDATE");
                    DateTime thisServiceDate = item.Field<DateTime>("SVCDATE");
                    string thisEmail = item.Field<string>("EMAIL");

                    // Logic when there are multiple service dates for the same patient, invoice number, and added on date
                    string strServiceDates = thisServiceDate.ToShortDateString();
                    string strComma = ", ";
                    if (thisPatientId == prevPatientId)
                    {
                        if (thisPONumber != prevPONUmber)
                        {
                            if (thisServiceDate != prevServiceDate)
                            {
                                strServiceDates += strComma + prevServiceDate.ToShortDateString();
                            }
                        }
                    }

                    string emailTemplateHtml = Utils.GetTemplate("emailTemplate.html");
                    emailTemplateHtml = emailTemplateHtml.Replace("[PATIENT_NAME]", FullName);
                    emailTemplateHtml = emailTemplateHtml.Replace("[ADDED_DATE]", thisAddedDate.ToShortDateString());
                    emailTemplateHtml = emailTemplateHtml.Replace("[SERVICE_DATE]", strServiceDates);
                    
                    if (!string.IsNullOrEmpty(thisEmail))
                    {
                        try
                        {
                            // Send confirmation email to customer
                            SendEmailToPatient(thisPatientId.ToString(), emailTemplateHtml, thisServiceDate.ToShortDateString(), thisEmail);
                            InsertContactNoteToPatient(thisPatientId.ToString(), strServiceDates);
                        }
                        catch
                        {
                            // Let the application continue
                        }
                    }
                    
                    // Populate the main report
                    strThisDetails = strDetails;
                    strThisDetails = strThisDetails.Replace("[PATIENT_ID]", thisPatientId.ToString());
                    strThisDetails = strThisDetails.Replace("[PATIENT_NAME]", FullName);
                    strThisDetails = strThisDetails.Replace("[EMAIL]", thisEmail);
                    strThisDetails = strThisDetails.Replace("[INVOICE_NUMBER]", thisPONumber);
                    strThisDetails = strThisDetails.Replace("[ADDED_ON_DATE]", thisAddedDate.ToShortDateString());
                    strThisDetails = strThisDetails.Replace("[SERVICE_DATE]", strServiceDates);
                    tr += strThisDetails;

                    prevPatientId = thisPatientId;
                    prevPONUmber = thisPONumber;
                    prevServiceDate = thisServiceDate;
                }

                //
                // Prepare report details
                //
                thisHtml = thisHtml.Replace("[TOTAL_PATIENTS]", iTotal.ToString());
                thisHtml = thisHtml.Replace("[GRID_HERE]", tr);
                thisHtml = thisHtml.Replace("[REPORT_DATE]", App.DateSpan);
                thisHtml = thisHtml.Replace("[DETAILS1_HERE]", tr);

                // only send report if there is data
                if (iTotal > 0)
                {
                    Utils.SendEmailWithModernAuthentication("0000", thisHtml, "Confirmation Email to Patients Report", "");
                }
                else
                {
                    App.Log("Report not sent - NO DATA");
                }
            }
            else
            {
                App.Log("No data found for the report that matched the criteria specified");
            }
        }

        private string GetDetailsTemplate(ref string html)
        {
            int iFirstIndex = html.IndexOf("[DETAIL1_START]");
            int iFirstIndexDetails = iFirstIndex + 15;
            int iLastIndexDetails = html.IndexOf("[/DETAIL1_END]");
            int iLastIndex = iLastIndexDetails + 14;
            int iTotalLenght = html.Length;
            string strTemplate = html.Substring(iFirstIndexDetails, iLastIndexDetails - iFirstIndexDetails);
            html = html.Substring(0, iFirstIndex) + "[DETAILS1_HERE]" + html.Substring(iLastIndex, iTotalLenght - iLastIndex);
            return strTemplate;
        }

        private void SendEmailToPatient(string id, string html, string serviceDate, string emailTo)
        {
            DateTime d = Convert.ToDateTime(serviceDate);
            d = d.AddHours(24);
            string countdownDate = d.ToString("s");
            html = html.Replace("[COUNTDOWN_DATE]", countdownDate);
            Utils.SendEmailWithModernAuthentication(id, html, "Acentus Order Confirmation", emailTo);
        }

        private void InsertContactNoteToPatient(string id, string serviceDate)
        {
            string note = "ORDER CONFIRMATION EMAIL SENT CONFIRMING PATIENT’S AUTHORIZATION TO SHIP FOR NEXT DOS OF " + serviceDate;
            db.InsertNote(id, note);
            App.Log("Contact note added successfully to : " + id + " - " + id);
        }
    }
}