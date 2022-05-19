using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using PMTWebAPI.DAL;
using PMTWebAPI.Models;

namespace PMTWebAPI.Controllers
{
    public class AndriodController : ApiController
    {

        DBGetData db = new DBGetData();

        public string GetIp()
        {
            return GetClientIp();
        }

        private string GetClientIp(HttpRequestMessage request = null)
        {
            request = request ?? Request;

            if (request.Properties.ContainsKey("MS_HttpContext"))
            {
                return ((HttpContextWrapper)request.Properties["MS_HttpContext"]).Request.UserHostAddress;
            }
            else
            if (HttpContext.Current != null)
            {
                return HttpContext.Current.Request.UserHostAddress;
            }
            else
            {
                return null;
            }
        }

        [Authorize]
        [HttpPost]
        [Route("api/Andriod/GetPhysicalProgressChart")]
        public IHttpActionResult GetPhysicalProgressChart([FromBody] ProjectDetails projectObj)
        {
            bool sError = false;
            string ErrorText = "";
            DataSet dsResponse = new DataSet();
            Guid transactionUid = Guid.NewGuid();
            try
            {
                var httpRequest = HttpContext.Current.Request;
                //Insert into WebAPITransctions table
                var BaseURL = HttpContext.Current.Request.Url.ToString();
                string postData = "ProjectName=" + projectObj.ProjectName;

                var identity = (ClaimsIdentity)User.Identity;
                if (db.CheckGetWebApiSettings(identity.Name, GetIp()) > 0)
                {
                    db.WebAPITransctionInsert(transactionUid, BaseURL, postData, "");
                    DataTable dtProject = db.GetWorkPackages_ProjectName(projectObj.ProjectName);
                    DataSet dsvalues = new DataSet();

                    if (dtProject.Rows.Count > 0)
                    {
                        dsResponse = db.GetTaskScheduleDatesforGraph(new Guid(dtProject.Rows[0]["workPackageUid"].ToString()));
                        if (dsResponse.Tables[0].Rows.Count > 0)
                        {
                            dsResponse.Tables[0].Columns.Add("Monthly Plan");
                            dsResponse.Tables[0].Columns.Add("Monthly Actual");
                            dsResponse.Tables[0].Columns.Add("Cumulative Plan");
                            dsResponse.Tables[0].Columns.Add("Cumulative Actual");
                            decimal planvalue = 0;
                            decimal actualvalue = 0;
                            decimal cumplanvalue = 0;
                            decimal cumactualvalue = 0;
                            foreach (DataRow dr in dsResponse.Tables[0].Rows)
                            {
                                //get the actual and planned values....
                                dsvalues.Clear();
                                dsvalues = db.GetTaskScheduleValuesForGraph(new Guid(dtProject.Rows[0]["workPackageUid"].ToString()), Convert.ToDateTime(dr["StartDate"].ToString()), Convert.ToDateTime(dr["StartDate"].ToString()).AddMonths(1));
                                if (dsvalues.Tables[0].Rows.Count > 0)
                                {
                                    planvalue = decimal.Parse(dsvalues.Tables[0].Rows[0]["TotalSchValue"].ToString());
                                    actualvalue = decimal.Parse(dsvalues.Tables[0].Rows[0]["TotalAchValue"].ToString());
                                    cumplanvalue += planvalue;
                                    cumactualvalue += actualvalue;
                                    dr["Monthly Plan"] = decimal.Round(planvalue, 2);
                                    dr["Monthly Actual"] = decimal.Round(actualvalue, 2);
                                    dr["Cumulative Plan"] = decimal.Round(cumplanvalue, 2);
                                    dr["Cumulative Actual"] = decimal.Round(cumactualvalue, 2);
                                }
                            }
                        }
                    }

                }
                else
                {
                    sError = true;
                    ErrorText = "Not Authorized IP address";
                }

            }
            catch (Exception ex)
            {
                sError = true;
                ErrorText = ex.Message;
            }
            if (sError)
            {
                db.WebAPITransctionUpdate(transactionUid, "Failure", "Error:" + ErrorText);
                return Json(new
                {
                    Status = "Failure",
                    Message = "Error:" + ErrorText
                });
            }
            db.WebAPITransctionUpdate(transactionUid, "Success", JsonConvert.SerializeObject(dsResponse.Tables[0]));
            return Json(new { response = JsonConvert.SerializeObject(dsResponse.Tables[0]) });
        }

        [Authorize]
        [HttpPost]
        [Route("api/Andriod/GetReportDocumentSummary")]
        public IHttpActionResult GetReportDocumentSummary([FromBody] ProjectDetails projectObj)
        {
            bool sError = false;
            string ErrorText = "";
            DataTable dt = new DataTable();
            Guid transactionUid = Guid.NewGuid();
            try
            {
                var httpRequest = HttpContext.Current.Request;
                //Insert into WebAPITransctions table
                var BaseURL = HttpContext.Current.Request.Url.ToString();
                string postData = "ProjectName=" + projectObj.ProjectName;

                var identity = (ClaimsIdentity)User.Identity;
                if (db.CheckGetWebApiSettings(identity.Name, GetIp()) > 0)
                {
                    db.WebAPITransctionInsert(transactionUid, BaseURL, postData, "");
                    DataTable dtProject = db.GetWorkPackages_ProjectName(projectObj.ProjectName);
                    DataSet dsvalues = new DataSet();

                    if (dtProject.Rows.Count > 0)
                    {

                        dt.Columns.Add("Sl_No");
                        dt.Columns.Add("Documents");
                        dt.Columns.Add("Status");
                        dt.Columns.Add("Number_Of_Documents");
                        //dt.Columns.Add("Submitted_by_the_Contractor");
                        //dt.Columns.Add("Recommended_Returned_by_PMC");
                        //dt.Columns.Add("Approved_by_BWSSB");
                        int TotalReturnedbyPMC = 0;
                        int TotalSubmitted = 0;
                        int TotalApproved = 0;
                        DataSet ds = db.GetDocumentSummary_by_WorkpackgeUID(new Guid(dtProject.Rows[0]["ProjectUid"].ToString()), new Guid(dtProject.Rows[0]["workPackageUid"].ToString()));
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < ds.Tables[0].Columns.Count; i++)
                            {
                                DataRow dr = dt.NewRow();
                                if (i == 0)
                                {
                                    dr["Sl_No"] = (i + 1);
                                    dr["Documents"] = "No. of Documemts submitted by the Contractor";
                                    dr["Status"] = "Submitted";
                                    dr["Number_Of_Documents"] = ds.Tables[0].Rows[0]["SubmittedDocuments"].ToString();
                                    //dr["Submitted_by_the_Contractor"] = ds.Tables[0].Rows[0]["SubmittedDocuments"].ToString();
                                    //dr["Recommended_Returned_by_PMC"] = "";
                                    //dr["Approved_by_BWSSB"] = "-";
                                    TotalSubmitted += ds.Tables[0].Rows[0]["SubmittedDocuments"].ToString() != "" ? Convert.ToInt32(ds.Tables[0].Rows[0]["SubmittedDocuments"].ToString()) : 0;
                                }
                                else if (i == 1)
                                {
                                    dr["Sl_No"] = (i + 1);
                                    dr["Documents"] = "No. of Documemts under category A";
                                    dr["Status"] = "Code A";
                                    dr["Number_Of_Documents"] = ds.Tables[0].Rows[0]["CodeA"].ToString();
                                    //dr["Submitted_by_the_Contractor"] = "-";
                                    //dr["Recommended_Returned_by_PMC"] = ds.Tables[0].Rows[0]["CodeA"].ToString();
                                    //dr["Approved_by_BWSSB"] = "-";
                                    TotalReturnedbyPMC += ds.Tables[0].Rows[0]["CodeA"].ToString() != "" ? Convert.ToInt32(ds.Tables[0].Rows[0]["CodeA"].ToString()) : 0;
                                }
                                else if (i == 2)
                                {
                                    dr["Sl_No"] = (i + 1);
                                    dr["Documents"] = "No. of Documemts under category B";
                                    dr["Status"] = "Code B";
                                    dr["Number_Of_Documents"] = ds.Tables[0].Rows[0]["CodeB"].ToString();
                                    //dr["Submitted_by_the_Contractor"] = "-";
                                    //dr["Recommended_Returned_by_PMC"] = ds.Tables[0].Rows[0]["CodeB"].ToString();
                                    //dr["Approved_by_BWSSB"] = "-";
                                    TotalReturnedbyPMC += ds.Tables[0].Rows[0]["CodeB"].ToString() != "" ? Convert.ToInt32(ds.Tables[0].Rows[0]["CodeB"].ToString()) : 0;
                                }
                                else if (i == 3)
                                {
                                    dr["Sl_No"] = (i + 1);
                                    dr["Documents"] = "No. of Documemts under category C";
                                    dr["Status"] = "Code C";
                                    dr["Number_Of_Documents"] = ds.Tables[0].Rows[0]["CodeC"].ToString();
                                    //dr["Submitted_by_the_Contractor"] = "-";
                                    //dr["Recommended_Returned_by_PMC"] = ds.Tables[0].Rows[0]["CodeC"].ToString();
                                    //dr["Approved_by_BWSSB"] = "-";
                                    TotalReturnedbyPMC += ds.Tables[0].Rows[0]["CodeC"].ToString() != "" ? Convert.ToInt32(ds.Tables[0].Rows[0]["CodeC"].ToString()) : 0;
                                }
                                else if (i == 4)
                                {
                                    dr["Sl_No"] = (i + 1);
                                    dr["Documents"] = "No. of Documemts under category D";
                                    dr["Status"] = "Code D";
                                    dr["Number_Of_Documents"] = ds.Tables[0].Rows[0]["CodeD"].ToString();
                                    //dr["Submitted_by_the_Contractor"] = "-";
                                    //dr["Recommended_Returned_by_PMC"] = ds.Tables[0].Rows[0]["CodeD"].ToString();
                                    //dr["Approved_by_BWSSB"] = "-";
                                    TotalReturnedbyPMC += ds.Tables[0].Rows[0]["CodeD"].ToString() != "" ? Convert.ToInt32(ds.Tables[0].Rows[0]["CodeD"].ToString()) : 0;
                                }
                                else if (i == 5)
                                {
                                    dr["Sl_No"] = (i + 1);
                                    dr["Documents"] = "No. of Documemts under category E";
                                    dr["Status"] = "Code E";
                                    dr["Number_Of_Documents"] = ds.Tables[0].Rows[0]["CodeE"].ToString();
                                    //dr["Submitted_by_the_Contractor"] = "-";
                                    //dr["Recommended_Returned_by_PMC"] = ds.Tables[0].Rows[0]["CodeE"].ToString();
                                    //dr["Approved_by_BWSSB"] = "-";
                                    TotalReturnedbyPMC += ds.Tables[0].Rows[0]["CodeE"].ToString() != "" ? Convert.ToInt32(ds.Tables[0].Rows[0]["CodeE"].ToString()) : 0;
                                }
                                else if (i == 6)
                                {
                                    dr["Sl_No"] = (i + 1);
                                    dr["Documents"] = "No. of Documemts under category F";
                                    dr["Status"] = "Code F";
                                    dr["Number_Of_Documents"] = ds.Tables[0].Rows[0]["CodeF"].ToString();
                                    //dr["Submitted_by_the_Contractor"] = "-";
                                    //dr["Recommended_Returned_by_PMC"] = ds.Tables[0].Rows[0]["CodeF"].ToString();
                                    //dr["Approved_by_BWSSB"] = "-";
                                    TotalReturnedbyPMC += ds.Tables[0].Rows[0]["CodeF"].ToString() != "" ? Convert.ToInt32(ds.Tables[0].Rows[0]["CodeF"].ToString()) : 0;
                                }
                                else if (i == 7)
                                {
                                    dr["Sl_No"] = (i + 1);
                                    dr["Documents"] = "No. of Documemts under category G";
                                    dr["Status"] = "Code G";
                                    dr["Number_Of_Documents"] = ds.Tables[0].Rows[0]["CodeG"].ToString();
                                    //dr["Submitted_by_the_Contractor"] = "-";
                                    //dr["Recommended_Returned_by_PMC"] = ds.Tables[0].Rows[0]["CodeG"].ToString();
                                    //dr["Approved_by_BWSSB"] = "-";
                                    TotalReturnedbyPMC += ds.Tables[0].Rows[0]["CodeG"].ToString() != "" ? Convert.ToInt32(ds.Tables[0].Rows[0]["CodeG"].ToString()) : 0;
                                }
                                else if (i == 8)
                                {
                                    dr["Sl_No"] = (i + 1);
                                    dr["Documents"] = "No. of Documemts under category H";
                                    dr["Status"] = "Code H";
                                    dr["Number_Of_Documents"] = ds.Tables[0].Rows[0]["CodeH"].ToString();
                                    //dr["Submitted_by_the_Contractor"] = "-";
                                    //dr["Recommended_Returned_by_PMC"] = ds.Tables[0].Rows[0]["CodeH"].ToString();
                                    //dr["Approved_by_BWSSB"] = "-";
                                    TotalReturnedbyPMC += ds.Tables[0].Rows[0]["CodeH"].ToString() != "" ? Convert.ToInt32(ds.Tables[0].Rows[0]["CodeH"].ToString()) : 0;
                                }
                                else
                                {
                                    dr["Sl_No"] = (i + 1);
                                    dr["Documents"] = "Client Approved Documents";
                                    dr["Status"] = "Client Approved";
                                    //dr["Submitted_by_the_Contractor"] = "-";
                                    //dr["Recommended_Returned_by_PMC"] = "-";
                                    //dr["Approved_by_BWSSB"] = ds.Tables[0].Rows[0]["ClientApproved"].ToString();
                                    dr["Number_Of_Documents"] = ds.Tables[0].Rows[0]["ClientApproved"].ToString();
                                    TotalApproved += ds.Tables[0].Rows[0]["ClientApproved"].ToString() != "" ? Convert.ToInt32(ds.Tables[0].Rows[0]["ClientApproved"].ToString()) : 0;
                                }
                                dt.Rows.Add(dr);
                            }
                            DataRow drtot = dt.NewRow();
                            drtot["Sl_No"] = "";
                            drtot["Documents"] = "Total No. of Documents";
                            drtot["Number_Of_Documents"] = TotalSubmitted+ TotalReturnedbyPMC+ TotalApproved;
                            //drtot["Submitted_by_the_Contractor"] = TotalSubmitted;
                            //drtot["Recommended_Returned_by_PMC"] = TotalReturnedbyPMC;
                            //drtot["Approved_by_BWSSB"] = TotalApproved;
                            dt.Rows.Add(drtot);
                        }

                    }
                    else
                    {
                        sError = true;
                        ErrorText = "Invalid Project Name";
                    }
                }

                else
                {
                    sError = true;
                    ErrorText = "Not Authorized IP address";
                }

            }
            catch (Exception ex)
            {
                sError = true;
                ErrorText = ex.Message;
            }
            if (sError)
            {
                db.WebAPITransctionUpdate(transactionUid, "Failure", "Error:" + ErrorText);
                return Json(new
                {
                    Status = "Failure",
                    Message = "Error:" + ErrorText
                });
            }
            if (dt.Rows.Count == 0)
            {
                db.WebAPITransctionUpdate(transactionUid, "Failure", "Error:" + ErrorText);
                return Json(new
                {
                    Status = "Failure",
                    Message = "No Data Available"
                }); ;
            }
            db.WebAPITransctionUpdate(transactionUid, "Success", JsonConvert.SerializeObject(dt));
            return Json(new { Status = "Success", response = JsonConvert.SerializeObject(dt) });
        }


        [Authorize]
        [HttpPost]
        [Route("api/Andriod/GetDocumentByStatus")]
        public IHttpActionResult GetDocumentByStatus([FromBody] DocumentCategory docObj)
        {
            bool sError = false;
            string ErrorText = "";
            DataTable dtRespone = new DataTable();
            Guid transactionUid = Guid.NewGuid();
            try
            {
                var httpRequest = HttpContext.Current.Request;
                //Insert into WebAPITransctions table
                var BaseURL = HttpContext.Current.Request.Url.ToString();
                string postData = "ProjectName=" + docObj.ProjectName + ";Status=" + docObj.status;

                var identity = (ClaimsIdentity)User.Identity;
                if (db.CheckGetWebApiSettings(identity.Name, GetIp()) > 0)
                {
                    db.WebAPITransctionInsert(transactionUid, BaseURL, postData, "");
                    DataTable dtProject = db.GetWorkPackages_ProjectName(docObj.ProjectName);
                    if (dtProject.Rows.Count > 0)
                    {
                        DataSet dsDocumentInfo = db.ActualDocuments_SelectBy_WorkpackageUID_Search(new Guid(dtProject.Rows[0]["ProjectUid"].ToString()), new Guid(dtProject.Rows[0]["WorkPackageUid"].ToString()), "", "All", "", docObj.status, DateTime.Now, DateTime.Now, DateTime.Now, DateTime.Now, 4);
                       // db.LogWrite("Documents Count:" + dsDocumentInfo.Tables[0].Rows.Count);
                        if (dsDocumentInfo.Tables[0].Rows.Count > 0)
                        {
                            dtRespone.Columns.Add("Submittal Name");
                            dtRespone.Columns.Add("Document Name");
                            dtRespone.Columns.Add("Document Type");
                            dtRespone.Columns.Add("Current Status");
                            dtRespone.Columns.Add("Incoming Recv. Date");
                            dtRespone.Columns.Add("Document Date");
                            dtRespone.Columns.Add("Document UID");
                            for (int cnt = 0; cnt < dsDocumentInfo.Tables[0].Rows.Count; cnt++)
                            {
                                try
                                {
                                    DataRow drtot = dtRespone.NewRow();
                                    drtot["Submittal Name"] = db.getDocumentName_by_DocumentUID(new Guid(dsDocumentInfo.Tables[0].Rows[cnt]["DocumentUID"].ToString()));
                                    drtot["Document Name"] = dsDocumentInfo.Tables[0].Rows[cnt]["ActualDocument_Name"].ToString();
                                    drtot["Document Type"] = dsDocumentInfo.Tables[0].Rows[cnt]["ActualDocument_Type"].ToString();
                                    drtot["Current Status"] = dsDocumentInfo.Tables[0].Rows[cnt]["ActualDocument_CurrentStatus"].ToString();
                                    drtot["Incoming Recv. Date"] = Convert.ToDateTime(dsDocumentInfo.Tables[0].Rows[cnt]["IncomingRec_Date"]).ToString("dd/MM/yyyy");
                                    drtot["Document Date"] = Convert.ToDateTime(dsDocumentInfo.Tables[0].Rows[cnt]["Document_Date"]).ToString("dd/MM/yyyy");
                                    drtot["Document UID"] = dsDocumentInfo.Tables[0].Rows[cnt]["DocumentUID"].ToString();
                                    dtRespone.Rows.Add(drtot);
                                }
                                catch (Exception ex)
                                {

                                }
                            }
                        }
                    }
                    else
                    {
                        sError = true;
                        ErrorText = "Invalid Project Name";
                    }
                }
                else
                {
                    sError = true;
                    ErrorText = "Not Authorized IP address";
                }
            }
            catch (Exception ex)
            {
                sError = true;
                ErrorText = ex.Message;
            }
            if (sError)
            {
                db.WebAPITransctionUpdate(transactionUid, "Failure", "Error:" + ErrorText);
                return Json(new
                {
                    Status = "Failure",
                    Message = "Error:" + ErrorText
                });
            }
            if (dtRespone.Rows.Count == 0)
            {
                db.WebAPITransctionUpdate(transactionUid, "Failure", "Error:" + ErrorText);
                return Json(new
                {
                    Status = "Failure",
                    Message = "No Documents Available"
                }); ;
            }
            db.WebAPITransctionUpdate(transactionUid, "Success", JsonConvert.SerializeObject(dtRespone));
            return Json(new { Status = "Success", response = JsonConvert.SerializeObject(dtRespone) });
        }

        [Authorize]
        [HttpPost]
        [Route("api/Andriod/GetTaskByProjectName")]
        public IHttpActionResult GetTaskByProjectName([FromBody] ProjectDetails projectObj)
        {
            var BaseURL = HttpContext.Current.Request.Url.ToString();

            bool sError = false;
            string ErrorText = "";
            DataTable dtRespone = new DataTable();
            Guid transactionUid = Guid.NewGuid();
            try
            {
                var httpRequest = HttpContext.Current.Request;
                //Insert into WebAPITransctions table

                string postData = "ProjectName=" + projectObj.ProjectName;

                var identity = (ClaimsIdentity)User.Identity;
                if (db.CheckGetWebApiSettings(identity.Name, GetIp()) > 0)
                {
                    db.WebAPITransctionInsert(transactionUid, BaseURL, postData, "");
                    DataTable dtProject = db.GetWorkPackages_ProjectName(projectObj.ProjectName);
                    if (dtProject.Rows.Count > 0)
                    {
                        dtRespone = db.GetTasks_by_ProjectName(projectObj.ProjectName);
                    }
                    else
                    {
                        sError = true;
                        ErrorText = "Invalid Project Name";
                    }
                }
                else
                {
                    sError = true;
                    ErrorText = "Not Authorized IP address";
                }
            }
            catch (Exception ex)
            {
                sError = true;
                ErrorText = ex.Message;
            }
            if (sError)
            {
                db.WebAPITransctionUpdate(transactionUid, "Failure", "Error:" + ErrorText);
                return Json(new
                {
                    Status = "Failure",
                    Message = "Error:" + ErrorText
                });
            }
            if (dtRespone.Rows.Count == 0)
            {
                db.WebAPITransctionUpdate(transactionUid, "Failure", "Error:No Tasks Available");
                return Json(new
                {
                    Status = "Failure",
                    Message = "No Tasks Available"
                }); ;
            }
            db.WebAPITransctionUpdate(transactionUid, "Success", JsonConvert.SerializeObject(dtRespone));
            return Json(new { Status = "Success", response = JsonConvert.SerializeObject(dtRespone) });
        }

        [Authorize]
        [HttpPost]
        [Route("api/Andriod/SendDeviceId")]
        public IHttpActionResult SendDeviceId([FromBody] UserAppTokens userAppTokens)
        {
            var BaseURL = HttpContext.Current.Request.Url.ToString();

            bool sError = false;
            string ErrorText = "";
            DataTable dtRespone = new DataTable();
            Guid transactionUid = Guid.NewGuid();
            try
            {
                var httpRequest = HttpContext.Current.Request;
                //Insert into WebAPITransctions table

                string postData = "username=" + userAppTokens.username + ";deviceid=" + userAppTokens.deviceid;

                var identity = (ClaimsIdentity)User.Identity;
                if (db.CheckGetWebApiSettings(identity.Name, GetIp()) > 0)
                {
                    db.WebAPITransctionInsert(transactionUid, BaseURL, postData, "");
                    DataSet dsUsers = db.getUserDetails_by_EmailID(userAppTokens.username);
                    if (dsUsers.Tables[0].Rows.Count > 0)
                    {
                        
                        int cnt = db.InserUserAppTokenDetails(dsUsers.Tables[0].Rows[0]["UserUID"].ToString(), userAppTokens.deviceid);
                        if (cnt > 0)
                        {
                            sError = false;
                            ErrorText = "Inserted Successfully";
                        }
                    }
                    else
                    {
                        sError = true;
                        ErrorText = "Invalid UserName";
                    }
                }
            }
            catch (Exception ex)
            {
                sError = true;
                ErrorText = ex.Message;
            }
            if (sError)
            {
                db.WebAPITransctionUpdate(transactionUid, "Failure", "Error:" + ErrorText);
                return Json(new
                {
                    Status = "Failure",
                    Message = "Error:" + ErrorText
                });
            }
            db.WebAPITransctionUpdate(transactionUid, "Failure", "Error:No Tasks Available");
            return Json(new
            {
                Status = "Success",
                Message = ErrorText
            });


        }

        [Authorize]
        [HttpPost]
        [Route("api/Andriod/gettaskdetailsfromtaskuid")]
        public IHttpActionResult gettaskdetailswfromtaskuid([FromBody] Tasks taskObj)
        {
            var BaseURL = HttpContext.Current.Request.Url.ToString();

            bool sError = false;
            string ErrorText = "";
            DataTable dtRespone = new DataTable();
            Guid transactionUid = Guid.NewGuid();
            try
            {
                var httpRequest = HttpContext.Current.Request;
                //Insert into WebAPITransctions table

                string postData = "taskuid=" + taskObj.TaskUID ;

                var identity = (ClaimsIdentity)User.Identity;
                if (db.CheckGetWebApiSettings(identity.Name, GetIp()) > 0)
                {
                    db.WebAPITransctionInsert(transactionUid, BaseURL, postData, "");
                    dtRespone = db.getNextLevelTaskDetails(taskObj.TaskUID);
                }
                else
                {
                    sError = true;
                    ErrorText = "Not Authorized IP address";
                }
            }
            catch (Exception ex)
            {
                sError = true;
                ErrorText = ex.Message;
            }
            if (sError)
            {
                db.WebAPITransctionUpdate(transactionUid, "Failure", "Error:" + ErrorText);
                return Json(new
                {
                    Status = "Failure",
                    Message = "Error:" + ErrorText
                });
            }
            if (dtRespone.Rows.Count == 0)
            {
                db.WebAPITransctionUpdate(transactionUid, "Failure", "Error:No Tasks Available");
                return Json(new
                {
                    Status = "Failure",
                    Message = "No Tasks Available"
                }); ;
            }
            db.WebAPITransctionUpdate(transactionUid, "Success", JsonConvert.SerializeObject(dtRespone));
            return Json(new { Status = "Success", response = JsonConvert.SerializeObject(dtRespone) });

        }

        [Authorize]
        [HttpPost]
        [Route("api/Andriod/getProjectListByUserName")]
        public IHttpActionResult getProjectListByUserName([FromBody] UserAppTokens users)
        {
            var BaseURL = HttpContext.Current.Request.Url.ToString();

            bool sError = false;
            string ErrorText = "";
            DataSet dtRespone = new DataSet();
            Guid transactionUid = Guid.NewGuid();
            try
            {
                var httpRequest = HttpContext.Current.Request;
                //Insert into WebAPITransctions table

                string postData = "username=" + users.username ;

                var identity = (ClaimsIdentity)User.Identity;
                if (db.CheckGetWebApiSettings(identity.Name, GetIp()) > 0)
                {
                    db.WebAPITransctionInsert(transactionUid, BaseURL, postData, "");
                    DataSet dsUsers = db.getUserDetails_by_EmailID(users.username);
                    if (dsUsers.Tables[0].Rows.Count > 0)
                    {
                        dtRespone = db.GetAssignedProjects_by_UserUID(new Guid(dsUsers.Tables[0].Rows[0]["UserUId"].ToString()));
                    }
                    else
                    {
                        sError = true;
                        ErrorText = "Invalid UserName";
                    }
                }
            }
            catch (Exception ex)
            {
                sError = true;
                ErrorText = ex.Message;
            }
            if (sError)
            {
                db.WebAPITransctionUpdate(transactionUid, "Failure", "Error:" + ErrorText);
                return Json(new
                {
                    Status = "Failure",
                    Message = "Error:" + ErrorText
                });
            }
            
            if (dtRespone.Tables[0].Rows.Count == 0)
            {
                db.WebAPITransctionUpdate(transactionUid, "Failure", "Error:No Projects Available");
                return Json(new
                {
                    Status = "Failure",
                    Message = "No Projects Available"
                }); ;
            }
            db.WebAPITransctionUpdate(transactionUid, "Success", JsonConvert.SerializeObject(dtRespone.Tables[0]));
            return Json(new { Status = "Success", response = JsonConvert.SerializeObject(dtRespone.Tables[0]) });
        }
        [Authorize]
        [HttpPost]
        [Route("api/Andriod/GetProjectUserdocuments")]
        public IHttpActionResult GetProjectUserdocuments([FromBody] UserProjectDocuments projectObj)
        {
            var BaseURL = HttpContext.Current.Request.Url.ToString();
            bool sError = false;
            string ErrorText = "";
            DataTable dtRespone = new DataTable();
            Guid transactionUid = Guid.NewGuid();
            try
            {
                var httpRequest = HttpContext.Current.Request;
                //Insert into WebAPITransctions table

                string postData = "ProjectName=" + projectObj.ProjectName+";UserName="+projectObj.Username;

                var identity = (ClaimsIdentity)User.Identity;
                if (db.CheckGetWebApiSettings(identity.Name, GetIp()) > 0)
                {
                    db.WebAPITransctionInsert(transactionUid, BaseURL, postData, "");
                    DataTable dtProject = db.GetWorkPackages_ProjectName(projectObj.ProjectName);
                    if (dtProject.Rows.Count > 0)
                    {
                        dtRespone = db.GetNextUserDocuments(new Guid(dtProject.Rows[0]["ProjectUID"].ToString()), new Guid(dtProject.Rows[0]["WorkpackageUID"].ToString()));
                    }
                    else
                    {
                        sError = true;
                        ErrorText = "Invalid Project Name";
                    }
                }
              
            }
            catch (Exception ex)
            {
                sError = true;
                ErrorText = ex.Message;
            }
            if (sError)
            {
                db.WebAPITransctionUpdate(transactionUid, "Failure", "Error:" + ErrorText);
                return Json(new
                {
                    Status = "Failure",
                    Message = "Error:" + ErrorText
                });
            }
            
            if (dtRespone.Rows.Count == 0)
            {
                db.WebAPITransctionUpdate(transactionUid, "Failure", "Error:No Projects Available");
                return Json(new
                {
                    Status = "Failure",
                    Message = "No Projects Available"
                }); ;
            }
            db.WebAPITransctionUpdate(transactionUid, "Success", JsonConvert.SerializeObject(dtRespone));
            return Json(new { Status = "Success", response = JsonConvert.SerializeObject(dtRespone) });

        }
    }
}
