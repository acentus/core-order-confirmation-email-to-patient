﻿//
// Copyright (c) 2022 by Acentus
// Developed by: Luis Cabrera
// gigocabrera@outlook.com
//

using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace CoreOrderConfirmationEmailToPatient
{
    class DataAccess
    {
        string conn = ConfigurationManager.ConnectionStrings["arxConnection"].ToString();
        DateTime todayDate = DateTime.Today;
        DateTime tomorrowDate = DateTime.Today.AddDays(1);

        public DataTable GetDataTable(string sql)
        {
            using (SqlConnection cn = new SqlConnection(conn))
            {
                cn.Open();
                using (SqlDataAdapter da = new SqlDataAdapter(sql, cn))
                {
                    da.SelectCommand.CommandTimeout = 120;
                    DataSet ds = new DataSet();
                    da.Fill(ds);
                    return ds.Tables[0];
                }
            }
        }

        public SqlDataReader GetDataReader(string sql)
        {
            using (SqlConnection cn = new SqlConnection(conn))
            {
                SqlCommand cmd = new SqlCommand(sql, cn);
                cn.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                return dr;
            }
        }

        public DataTable GetData()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT ");
            sb.Append("    WO.PATIENTID, ");
            sb.Append("    CONCAT(PAT.FIRSTNAME, ' ' , PAT.LASTNAME) AS FULLNAME, ");
            sb.Append("    WO.INVOICENO, ");
            sb.Append("    CONVERT(date, WO.ENTRYDATE) AS ADDEDONDATE, ");
            sb.Append("    WO.SVCDATE, ");
            sb.Append("    PAT.EMAIL ");
            sb.Append("FROM AR1ORDW WO ");
            sb.Append("INNER JOIN AR1PAT PAT ON PAT.ID = WO.PATIENTID ");
            sb.Append("WHERE PAT.PATIENTSTATUS = 'A' ");
            sb.Append("AND WO.BILLTYPE = 'P' ");
            sb.Append("AND WO.CONFIRM = 'Y' ");
            sb.Append("AND DATEDIFF(day, WO.SVCDATE, WO.CMNEXPIRE) > 0 ");
            sb.Append("AND WO.ITEMID NOT IN(726,1950)");
            sb.Append("AND WO.ENTRYDATE BETWEEN '").Append(App.StartDate).Append("' AND '").Append(App.EndDate).Append("' ");
            sb.Append("GROUP BY WO.PATIENTID, PAT.FIRSTNAME, PAT.LASTNAME, WO.INVOICENO, CONVERT(date, WO.ENTRYDATE), WO.SVCDATE, PAT.EMAIL ");
            sb.Append("ORDER BY WO.PATIENTID, WO.SVCDATE DESC; ");
            return GetDataTable(sb.ToString());
        }

        public void InsertNote(string id, string note, string html)
        {
            using (SqlConnection sql = new SqlConnection(conn))
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("INSERT INTO AR4CONTACTS (ENTITYTYPE, ENTITYID, CONTACTTYPE, NOTE, ADDUSERID, ADDDATE, MESSAGE) ");
                sb.Append("VALUES('PATIENT', @id, 'APATIENT',  @note, 'SYSTEM', GetDate(), @message); ");

                using (SqlCommand cmd = new SqlCommand(sb.ToString(), sql))
                {
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    cmd.Parameters.Add(new SqlParameter("@note", note));
                    cmd.Parameters.Add(new SqlParameter("@message", html));
                    sql.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}