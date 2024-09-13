using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace CoreOrderConfirmationEmailToPatient
{
    class JobCenterHistoryLogger
    {
        string conn = ConfigurationManager.ConnectionStrings["arxConnection"].ToString();

        public enum StatusEnum
        {
            Success,
            Info,
            Running,
            Warning,
            Error
        }

        private int _HistoryId;
        private int _JobId;
        private string _JobTitle = String.Empty;
        private DateTime _StartTime;
        private DateTime _EndTime;
        private StatusEnum _status = StatusEnum.Success;
        private string _machineName;
        private string _message;
        private string _exeName;

        public int HistoryId
        {
            get { return _HistoryId; }
            set { _HistoryId = value; }
        }

        public int JobId
        {
            get { return _JobId; }
            set { _JobId = value; }
        }

        public string JobTitle
        {
            get { return _JobTitle; }
            set { _JobTitle = value; }
        }

        public DateTime StartTime
        {
            get { return _StartTime; }
            set { _StartTime = value; }
        }

        public DateTime EndTime
        {
            get { return _EndTime; }
            set { _EndTime = value; }
        }

        public StatusEnum Status
        {
            get { return _status; }
            set { _status = value; }
        }

        public string MachineName
        {
            get { return Environment.MachineName; }
        }

        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }

        public string EXEName
        {
            get
            {
                string exePath = Process.GetCurrentProcess().MainModule.FileName;
                _exeName = Path.GetFileName(exePath);
                return _exeName;
            }
        }

        public JobCenterHistoryLogger()
        {

        }

        public JobCenterHistoryLogger(DateTime StartTime, DateTime EndTime, StatusEnum Status, string Message)
        {
            try
            {
                _JobId = GetJobId();
                _JobTitle = GetJobTitle(_JobId);
                _StartTime = StartTime;
                _EndTime = EndTime;
                _status = Status;
                _machineName = MachineName;
                _message = Message;
                _exeName = EXEName;

                if (_JobId > 0)
                {
                    SaveLogHeader();
                    UpdateJobLastStatus(_JobId, GetStatusValue(), 1);
                }
                else
                {
                    Log.write("JobCenterHistoryLogger - SaveLogHeader() bypassed. JobId = " + _JobId);
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                //throw new Exception(msg);
                // Do not throw anything. 
            }
        }

        private void SaveLogHeader()
        {
            Decimal newHistoryId = 0;
            using (SqlConnection sql = new SqlConnection(conn))
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("INSERT INTO tblJobHistory ([JobId], [JobTitle], StartTime, EndTime, Status, MachineName, message) ");
                sb.Append("values (@jobid, @jobtitle, @starttime, @endtime, @status, @machinename, @message); ");
                sb.Append("SELECT SCOPE_IDENTITY(); ");

                using (SqlCommand cmd = new SqlCommand(sb.ToString(), sql))
                {
                    cmd.Parameters.Add("@jobid", SqlDbType.Int).Value = _JobId;
                    cmd.Parameters.Add("@jobtitle", SqlDbType.VarChar, 100).Value = _JobTitle;
                    cmd.Parameters.Add("@starttime", SqlDbType.DateTime).Value = _StartTime;
                    cmd.Parameters.Add("@endtime", SqlDbType.DateTime).Value = _EndTime;
                    cmd.Parameters.Add("@status", SqlDbType.VarChar, -1).Value = GetStatusValue();
                    cmd.Parameters.Add("@machinename", SqlDbType.VarChar, 100).Value = _machineName;
                    cmd.Parameters.Add("@message", SqlDbType.VarChar, 200).Value = _message;
                    sql.Open();
                    var res = cmd.ExecuteScalar();
                    newHistoryId = (decimal)res;
                    sql.Close();
                }
            }
            _HistoryId = (int)newHistoryId;
        }

        public void Update()
        {
            UpdateLogHeader();
            UpdateJobLastStatus(_JobId, GetStatusValue(), 0);
            RunNowComplete(_JobId);
        }

        public void UpdateLogHeader()
        {
            using var con = new SqlConnection(conn);
            var command = new SqlCommand("UPDATE tblJobHistory SET EndTime = @endtime, Status = @status, Message = @message where HistoryId = @historyid", con);

            command.Parameters.Add("@endtime", SqlDbType.DateTime).Value = DateTime.Now;
            command.Parameters.Add("@status", SqlDbType.VarChar).Value = GetStatusValue();
            command.Parameters.Add("@message", SqlDbType.VarChar).Value = _message;
            command.Parameters.Add("@historyid", SqlDbType.VarChar).Value = _HistoryId;

            con.OpenAsync();
            command.ExecuteNonQuery();
        }

        public void UpdateJobLastStatus(int jobId, string status, int isJobRunning)
        {
            using var con = new SqlConnection(conn);
            var command = new SqlCommand("UPDATE tblJobs SET LastStatus = @status, JobType = @isrunning where JobId = @jobid", con);
            command.Parameters.Add("@status", SqlDbType.VarChar).Value = status;
            command.Parameters.Add("@jobid", SqlDbType.Int).Value = jobId;
            command.Parameters.Add("@isrunning", SqlDbType.Int).Value = isJobRunning;
            con.OpenAsync();
            command.ExecuteNonQuery();
        }

        public void RunNowComplete(int jobId)
        {
            using var con = new SqlConnection(conn);
            var command = new SqlCommand("UPDATE tblJobs SET RunNow = 0 where JobId = @jobid", con);
            command.Parameters.Add("@jobid", SqlDbType.Int).Value = jobId;
            con.OpenAsync();
            command.ExecuteNonQuery();
        }

        public string GetStatusValue()
        {
            string rtnVal = string.Empty;
            switch (_status)
            {
                case StatusEnum.Success:
                    rtnVal = "SUCCESS";
                    break;
                case StatusEnum.Info:
                    rtnVal = "INFO";
                    break;
                case StatusEnum.Running:
                    rtnVal = "RUNNING";
                    break;
                case StatusEnum.Warning:
                    rtnVal = "WARNING";
                    break;
                case StatusEnum.Error:
                    rtnVal = "ERROR";
                    break;
            }
            return rtnVal;
        }

        public int GetJobId()
        {
            try
            {
                int newJobId = 0;
                string exeName = GetExecutableName();
                if (exeName == null)
                {
                    Log.write("Executable name is null.");
                    return -1;
                }

                string sql = "SELECT JobId FROM tblJobs WHERE ExeName LIKE '%" + exeName + "';";
                using (SqlConnection con = new SqlConnection(conn))
                {
                    if (con == null)
                    {
                        Log.write("Connection object is null.");
                        return -1;
                    }

                    SqlCommand cmd = new SqlCommand(sql, con);
                    con.Open();
                    object result = cmd.ExecuteScalar();
                    if (result == null || result == DBNull.Value)
                    {
                        Log.write("Query returned null.");
                        return -1;
                    }
                    else
                    {
                        newJobId = Convert.ToInt32(result);
                        return newJobId;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.write(ex.Message);
                return -1;
            }
        }


        public string GetJobTitle(int jobId)
        {
            try
            {
                string newJobName = string.Empty;
                string sql = "SELECT JobName FROM tblJobs WHERE JobId = @JobId;";

                using (SqlConnection con = new SqlConnection(conn))
                {
                    SqlCommand cmd = new SqlCommand(sql, con);
                    cmd.Parameters.AddWithValue("@JobId", jobId);

                    con.Open();
                    object result = cmd.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        newJobName = result.ToString();
                    }
                    else
                    {
                        Log.write("Job title not found for JobId: " + jobId);
                    }
                }

                return newJobName;
            }
            catch (Exception ex)
            {
                Log.write("Error retrieving job title: " + ex.Message);
                return string.Empty;
            }
        }


        public string GetExecutableName()
        {
            try
            {
                // Get the current process
                Process currentProcess = Process.GetCurrentProcess();

                // Get the main module (executable) of the current process
                ProcessModule mainModule = currentProcess.MainModule;

                // Extract and display the name of the executable
                string executableName = mainModule.ModuleName;

                return executableName;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }
    }

    class JobCenterHistoryLoggerDetails
    {
        string conn = ConfigurationManager.ConnectionStrings["arxConnection"].ToString();

        private int _DetailId;
        private int _HistoryId;
        private string _message;
        private DateTime _CreatedDate;

        public int DetailId
        {
            get { return _DetailId; }
            set { _DetailId = value; }
        }

        public int HistoryId
        {
            get { return _HistoryId; }
            set { _HistoryId = value; }
        }

        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }

        public DateTime CreatedDate
        {
            get { return _CreatedDate; }
            set { _CreatedDate = value; }
        }

        public JobCenterHistoryLoggerDetails()
        {

        }

        public JobCenterHistoryLoggerDetails(int HistoryId, string Message)
        {
            _HistoryId = HistoryId;
            _message = Message;
            try
            {
                if (_HistoryId > 0)
                {
                    SaveLogDetails();
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                Log.write("EXCEPTION JobCenterHistoryLoggerDetails Save() = " + msg);
            }
        }

        public void SaveLogDetails()
        {
            using (SqlConnection sql = new SqlConnection(conn))
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("INSERT INTO tblJobHistoryDetails ([HistoryId], [Message], CreatedDate) ");
                sb.Append("values (@historyid, @message, @createddate); ");

                using (SqlCommand cmd = new SqlCommand(sb.ToString(), sql))
                {
                    cmd.Parameters.Add("@historyid", SqlDbType.Int).Value = _HistoryId;
                    cmd.Parameters.Add("@message", SqlDbType.VarChar, 1000).Value = _message;
                    cmd.Parameters.Add("@createddate", SqlDbType.DateTime).Value = DateTime.Now;
                    sql.Open();
                    var res = cmd.ExecuteScalar();
                    sql.Close();
                }
            }
        }
    }
}