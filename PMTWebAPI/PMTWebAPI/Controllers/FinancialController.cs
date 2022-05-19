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

namespace PMTWebAPI.Controllers
{
    public class FinancialController : ApiController
    {
        DBGetData db = new DBGetData();
        Invoice invoice = new Invoice();
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

        //[Authorize]
        //[HttpPost]
        //[Route("api/Financial/AddEditRABills")]
        //public IHttpActionResult AddEditRABills()
        //{
        //    bool sError = false;
        //    string ErrorText = "";
        //    string rabillUid = "";
        //    try
        //    {
        //        var httpRequest = HttpContext.Current.Request;
        //        if (String.IsNullOrEmpty(httpRequest.Params["ProjectName"]) || String.IsNullOrEmpty(httpRequest.Params["RAbillno"]) || String.IsNullOrEmpty(httpRequest.Params["Date"]) ||
        //            String.IsNullOrEmpty(httpRequest.Params["numberofJIRs"]) || String.IsNullOrEmpty(httpRequest.Params["JIR"]))
        //        {
        //            return Json(new
        //            {
        //                Status = "Failure",
        //                Message = "Error:Mandatory fields are missing"
        //            }); ;
        //        }
        //        //Insert into WebAPITransctions table
        //        var BaseURL = HttpContext.Current.Request.Url.ToString();
        //        string postData = "ProjectName=" + httpRequest.Params["ProjectName"] + "&RAbillno=" + httpRequest.Params["RAbillno"] + "&Date=" + httpRequest.Params["Date"] + "&numberofJIRs=" + httpRequest.Params["numberofJIRs"] + "&JIR=" + httpRequest.Params["JIR"];
        //        db.WebAPITransctionInsert(Guid.NewGuid(), BaseURL, postData, "");

        //        var identity = (ClaimsIdentity)User.Identity;
        //        if (db.CheckGetWebApiSettings(identity.Name, GetIp()) > 0)
        //        {

        //            DataSet dsWorkPackages = new DataSet();
        //            var projectName = httpRequest.Params["ProjectName"];
        //            var RAbillno = httpRequest.Params["RAbillno"];
        //            int cnt = 0;
        //            if (DateTime.TryParse(httpRequest.Params["Date"], out DateTime sDate))
        //            {
        //                var NoOfJIR = httpRequest.Params["NoOfJIR"];
        //                var InspectionUID = httpRequest.Params["JIR"];//InspectionUID
        //                DataTable dtjoininspection = db.getJointInspection_by_inspectionUid(InspectionUID);
        //                if(dtjoininspection.Rows.Count==0)
        //                {
        //                    return Json(new
        //                    {
        //                        Status = "Failure",
        //                        Message = "Error:InspectionUid is not exists"
        //                    }) ;
        //                }
        //                DataTable dtWorkPackages = db.GetWorkPackages_ProjectName(projectName);
        //                if (dtWorkPackages.Rows.Count > 0)
        //                {
        //                    rabillUid = db.AddRABillNumber(RAbillno, new Guid(dtWorkPackages.Rows[0]["WorkpackageUID"].ToString()), sDate);
        //                    if (rabillUid == "Exists")
        //                    {
        //                        sError = true;
        //                        ErrorText = "RA Bill Number already exists.";
        //                    }
        //                    else if (rabillUid == "Error1")
        //                    {
        //                        sError = true;
        //                        ErrorText = "There is a problem with this feature. Please contact system admin.";
        //                    }
        //                    else
        //                    {
        //                        int ErrorCount = 0;
        //                        int ItemCount = 0;
        //                        double totamount = 0;
        //                        DataSet ds = db.GetBOQDetails_by_projectuid(new Guid(dtWorkPackages.Rows[0]["ProjectUID"].ToString()));
        //                        if (ds.Tables[0].Rows.Count > 0)
        //                        {

        //                            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
        //                            {
        //                                string sDate2 = "";
        //                                DateTime CDate2 = DateTime.Now;

        //                                sDate2 = DateTime.Now.ToString("dd/MM/yyyy");
        //                                //sDate1 = sDate1.Split('/')[1] + "/" + sDate1.Split('/')[0] + "/" + sDate1.Split('/')[2];
        //                                sDate2 = db.ConvertDateFormat(sDate2);
        //                                CDate2 = Convert.ToDateTime(sDate);

        //                                cnt = db.InsertRABillsItems(rabillUid, ds.Tables[0].Rows[i]["Item_Number"].ToString(), ds.Tables[0].Rows[i]["Description"].ToString(), CDate2.ToString(), "0", new Guid(dtWorkPackages.Rows[0]["ProjectUID"].ToString()), new Guid(dtWorkPackages.Rows[0]["WorkpackageUID"].ToString()), new Guid(ds.Tables[0].Rows[i]["BOQDetailsUID"].ToString()));
        //                                if (cnt <= 0)
        //                                {
        //                                    ErrorCount += 1;
        //                                }
        //                                else
        //                                {
        //                                    ItemCount += 1;
        //                                    totamount += ds.Tables[0].Rows[i]["INR-Amount"].ToString() == "" ? 0 : Convert.ToDouble(ds.Tables[0].Rows[i]["INR-Amount"].ToString());
        //                                }
        //                            }
        //                        }

        //                        if (ErrorCount > 0)
        //                        {
        //                            sError = true;
        //                            ErrorText = "There is a problem linking BOQ details to this RABill. Please contact system admin";

        //                        }
        //                        else
        //                        {
        //                            Guid AssignJointInspectionUID = Guid.NewGuid();
        //                            cnt = db.InsertJointInspectiontoRAbill(AssignJointInspectionUID, new Guid(rabillUid), new Guid(ds.Tables[0].Rows[0]["BOQDetailsUid"].ToString()), new Guid(InspectionUID));
        //                            if (cnt == 0)
        //                            {
        //                                sError = true;
        //                                ErrorText = "Join Inspection to RABill is not inserted";
        //                            }
        //                        }

        //                    }

        //                }

        //                else
        //                {
        //                    sError = true;
        //                    ErrorText = "No Workpackage available for select project";
        //                }
        //            }
        //            else
        //            {
        //                sError = true;
        //                ErrorText = "Date is not correct format";
        //            }

        //        }
        //        else
        //        {
        //            sError = true;
        //            ErrorText = "Not Authorized IP address";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        sError = true;
        //        ErrorText = ex.Message;
        //    }


        //    if (sError)
        //    {
        //        return Json(new
        //        {
        //            Status = "Failure",
        //            Message = "Error:" + ErrorText
        //        });
        //    }
        //    else
        //    {
        //        return Json(new
        //        {
        //            Status = "Success",
        //            RABillUId = rabillUid,
        //            Message = "Successfully Updated Inspection to RAbill"
        //        });
        //    }
        //}


        //-----------------------------------------------------------
        // added on 14/02/2022 for venkat
        [Authorize]
        [HttpPost]
        [Route("api/Financial/AddEditRABills")]
        public IHttpActionResult AddEditRABills()
        {
            bool sError = false;
            string ErrorText = "";
            string rabillUid = "";
            Guid transactionUid = Guid.NewGuid();
            try
            {
                var httpRequest = HttpContext.Current.Request;
                //Insert into WebAPITransctions table
                var BaseURL = HttpContext.Current.Request.Url.ToString();
                string postData = "ProjectName=" + httpRequest.Params["ProjectName"] + "&RAbillno=" + httpRequest.Params["RAbillno"] + "&Date=" + httpRequest.Params["Date"] + "&numberofJIRs=" + httpRequest.Params["NoOfJIR"] + "&JIR=" + httpRequest.Params["JIR"];
                db.WebAPITransctionInsert(transactionUid, BaseURL, postData, "");

                if (String.IsNullOrEmpty(httpRequest.Params["ProjectName"]) || String.IsNullOrEmpty(httpRequest.Params["RAbillno"]) || String.IsNullOrEmpty(httpRequest.Params["Date"]) ||
                    String.IsNullOrEmpty(httpRequest.Params["NoOfJIR"]) || String.IsNullOrEmpty(httpRequest.Params["JIR"]))
                {
                    //   db.WebAPITransctionUpdate(transactionUid, "Failure", "Error:Project Name is Manadatory");
                    return Json(new
                    {
                        Status = "Failure",
                        Message = "Error:Mandatory fields are missing"
                    }); ;
                }

                var identity = (ClaimsIdentity)User.Identity;
                if (db.CheckGetWebApiSettings(identity.Name, GetIp()) > 0)
                {

                    DataSet dsWorkPackages = new DataSet();
                    var projectName = httpRequest.Params["ProjectName"];
                    var RAbillno = httpRequest.Params["RAbillno"];
                    int cnt = 0;
                    DateTime sDate=DateTime.Now;
                   if (!string.IsNullOrEmpty(httpRequest.Params["Date"]))
                   {
                        string cdate = db.ConvertDateFormat(httpRequest.Params["Date"]);
                      
                           sDate = Convert.ToDateTime(cdate);
                    }
                    //if (DateTime.TryParse(httpRequest.Params["Date"], out DateTime sDate))
                    //{

                        var NoOfJIR = httpRequest.Params["NoOfJIR"];
                        var InspectionUID = httpRequest.Params["JIR"];//InspectionUID
                        string[] InspectionUIDList = InspectionUID.Split('$');
                        DataTable dtjoininspection ;
                        //DataTable dtjoininspection = db.getJointInspection_by_inspectionUid(InspectionUID);
                        //if (dtjoininspection.Rows.Count == 0)
                        //{
                        //    //  db.WebAPITransctionUpdate(transactionUid, "Failure", "Error:InspectionUid is not exists");
                        //    return Json(new
                        //    {
                        //        Status = "Failure",
                        //        Message = "Error:InspectionUid is not exists"
                        //    });
                        //}
                        // added by zuber on 17/02/2022

                        if (InspectionUID.Contains("$"))
                        {
                            for (int i = 0; i < InspectionUIDList.Length; i++)
                            {
                                dtjoininspection = new DataTable();
                                dtjoininspection = db.getJointInspection_by_inspectionUid(InspectionUIDList[i]);
                                if (dtjoininspection.Rows.Count == 0)
                                {
                                    //  db.WebAPITransctionUpdate(transactionUid, "Failure", "Error:InspectionUid is not exists");
                                    return Json(new
                                    {
                                        Status = "Failure",
                                        Message = "Error:InspectionUid does not exists"
                                    });
                                }
                            }
                        }
                        else
                        {
                            dtjoininspection = new DataTable();
                            dtjoininspection = db.getJointInspection_by_inspectionUid(InspectionUID);
                            if (dtjoininspection.Rows.Count == 0)
                            {
                                //  db.WebAPITransctionUpdate(transactionUid, "Failure", "Error:InspectionUid is not exists");
                                return Json(new
                                {
                                    Status = "Failure",
                                    Message = "Error:InspectionUid is not exists"
                                });
                            }
                        }
                        //-----------------------------------------------------------

                        DataTable dtWorkPackages = db.GetWorkPackages_ProjectName(projectName);
                        if (dtWorkPackages.Rows.Count > 0)
                        {
                            rabillUid = db.AddRABillNumber(RAbillno, new Guid(dtWorkPackages.Rows[0]["WorkpackageUID"].ToString()), sDate);
                            if (rabillUid == "Exists")
                            {
                                sError = true;
                                ErrorText = "RA Bill Number already exists.";
                            }
                            else if (rabillUid == "Error1")
                            {
                                sError = true;
                                ErrorText = "There is a problem with this feature. Please contact system admin.";
                            }
                            else
                            {
                                int ErrorCount = 0;
                                int ItemCount = 0;
                                double totamount = 0;
                                DataSet ds = db.GetBOQDetails_by_projectuid(new Guid(dtWorkPackages.Rows[0]["ProjectUID"].ToString()));
                                if (ds.Tables[0].Rows.Count > 0)
                                {

                                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                                    {
                                        string sDate2 = "";
                                        DateTime CDate2 = DateTime.Now;

                                        sDate2 = DateTime.Now.ToString("dd/MM/yyyy");
                                        //sDate1 = sDate1.Split('/')[1] + "/" + sDate1.Split('/')[0] + "/" + sDate1.Split('/')[2];
                                        sDate2 = db.ConvertDateFormat(sDate2);
                                        CDate2 = Convert.ToDateTime(sDate);

                                        cnt = db.InsertRABillsItems(rabillUid, ds.Tables[0].Rows[i]["Item_Number"].ToString(), ds.Tables[0].Rows[i]["Description"].ToString(), CDate2.ToString(), "0", new Guid(dtWorkPackages.Rows[0]["ProjectUID"].ToString()), new Guid(dtWorkPackages.Rows[0]["WorkpackageUID"].ToString()), new Guid(ds.Tables[0].Rows[i]["BOQDetailsUID"].ToString()));
                                        if (cnt <= 0)
                                        {
                                            ErrorCount += 1;
                                        }
                                        else
                                        {
                                            ItemCount += 1;
                                            totamount += ds.Tables[0].Rows[i]["INR-Amount"].ToString() == "" ? 0 : Convert.ToDouble(ds.Tables[0].Rows[i]["INR-Amount"].ToString());
                                        }
                                    }
                                }

                                if (ErrorCount > 0)
                                {
                                    sError = true;
                                    ErrorText = "There is a problem linking BOQ details to this RABill. Please contact system admin";

                                }
                                else
                                {
                                    Guid AssignJointInspectionUID = Guid.NewGuid();
                                    //Updated by Venkat on 11 Feb 2022
                                    //string boqDetailsUid = db.gbGetBoqDetailsUId_InspectionUId(new Guid(InspectionUID));
                                    //if (boqDetailsUid == "")
                                    //{
                                    //    sError = true;
                                    //    ErrorText = "BOQDetails Uid not available for selcted inspectionuid";
                                    //}
                                    //else
                                    //{
                                    //    cnt = db.InsertJointInspectiontoRAbill(AssignJointInspectionUID, new Guid(rabillUid), new Guid(boqDetailsUid), new Guid(InspectionUID));
                                    //    if (cnt == 0)
                                    //    {
                                    //        sError = true;
                                    //        ErrorText = "Join Inspection to RABill is not inserted";
                                    //    }
                                    //}
                                    //

                                    // added by zuber on 17/02/2022
                                    if (InspectionUID.Contains("$"))
                                    {
                                        for (int i = 0; i < InspectionUIDList.Length; i++)
                                        {
                                            AssignJointInspectionUID = Guid.NewGuid();
                                            string boqDetailsUid = db.gbGetBoqDetailsUId_InspectionUId(new Guid(InspectionUIDList[i]));
                                            if (boqDetailsUid == "")
                                            {
                                                sError = true;
                                                ErrorText += "BOQDetails Uid not available for inspectionuid : " + InspectionUIDList[i];
                                            }
                                            else
                                            {
                                                cnt = db.InsertJointInspectiontoRAbill(AssignJointInspectionUID, new Guid(rabillUid), new Guid(boqDetailsUid), new Guid(InspectionUIDList[i]));
                                                if (cnt == 0)
                                                {
                                                    sError = true;
                                                    ErrorText += "Join Inspection to RABill is not inserted for inspectionuid :" + InspectionUIDList[i];
                                                }
                                            }

                                        }
                                    }
                                    else
                                    {
                                        string boqDetailsUid = db.gbGetBoqDetailsUId_InspectionUId(new Guid(InspectionUID));
                                        if (boqDetailsUid == "")
                                        {
                                            sError = true;
                                            ErrorText = "BOQDetails Uid not available for selcted inspectionuid";
                                        }
                                        else
                                        {
                                            cnt = db.InsertJointInspectiontoRAbill(AssignJointInspectionUID, new Guid(rabillUid), new Guid(boqDetailsUid), new Guid(InspectionUID));
                                            if (cnt == 0)
                                            {
                                                sError = true;
                                                ErrorText = "Join Inspection to RABill is not inserted";
                                            }
                                        }
                                    }
                                    //--------------------------------------------------
                                }

                            }

                        }

                        else
                        {
                            sError = true;
                            ErrorText = "No Workpackage available for select project";
                        }
                    //}
                    //else
                    //{
                    //    sError = true;
                    //    ErrorText = "Date is not correct format";
                    //}

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
                // db.WebAPITransctionUpdate(transactionUid, "Failure", "Error:" + ErrorText);
                return Json(new
                {
                    Status = "Failure",
                    Message = "Error:" + ErrorText
                });
            }
            else
            {

                // db.WebAPITransctionUpdate(transactionUid, "Success", " RABillUId = "+rabillUid+",Message = Successfully Updated Inspection to RAbill");
                return Json(new
                {
                    Status = "Success",
                    RABillUId = rabillUid,
                    Message = "Successfully Updated Inspection to RAbill"
                });
            }
        }


        [Authorize]
        [HttpPost]
        [Route("api/Financial/AddJIRtoRABills")]
        public IHttpActionResult AddJIRtoRABills()
        {
            bool sError = false;
            string ErrorText = "";
          
            Guid transactionUid = Guid.NewGuid();
            try
            {
                var httpRequest = HttpContext.Current.Request;
                //Insert into WebAPITransctions table
                var BaseURL = HttpContext.Current.Request.Url.ToString();
                string postData = "ProjectName=" + httpRequest.Params["ProjectName"] + "&rabillUID=" + httpRequest.Params["RAbillUID"] +   "&JIR=" + httpRequest.Params["JIR"];
                db.WebAPITransctionInsert(transactionUid, BaseURL, postData, "");

                if (String.IsNullOrEmpty(httpRequest.Params["ProjectName"]) || String.IsNullOrEmpty(httpRequest.Params["RAbillUID"]) || String.IsNullOrEmpty(httpRequest.Params["JIR"]))
                {
                    //   db.WebAPITransctionUpdate(transactionUid, "Failure", "Error:Project Name is Manadatory");
                    return Json(new
                    {
                        Status = "Failure",
                        Message = "Error:Mandatory fields are missing"
                    }); ;
                }

                var identity = (ClaimsIdentity)User.Identity;
                if (db.CheckGetWebApiSettings(identity.Name, GetIp()) > 0)
                {

                    DataSet dsWorkPackages = new DataSet();
                    var projectName = httpRequest.Params["ProjectName"];
                    var rabillUID = httpRequest.Params["RAbillUID"];
                    int cnt = 0;
                   
                        var InspectionUID = httpRequest.Params["JIR"];//InspectionUID
                        string[] InspectionUIDList = InspectionUID.Split('$');
                        DataTable dtjoininspection;
                      
                      
                        if (InspectionUID.Contains("$"))
                        {
                            for (int i = 0; i < InspectionUIDList.Length; i++)
                            {
                                dtjoininspection = new DataTable();
                                dtjoininspection = db.getJointInspection_by_inspectionUid(InspectionUIDList[i]);
                                if (dtjoininspection.Rows.Count == 0)
                                {
                                //  db.WebAPITransctionUpdate(transactionUid, "Failure", "Error:InspectionUid is not exists");
                                return Json(new
                                {
                                    Status = "Failure",
                                    Message = "Error:InspectionUid does not exists for " + InspectionUIDList[i]
                                 });
                                }
                            }
                        }
                        else
                        {
                            dtjoininspection = new DataTable();
                            dtjoininspection = db.getJointInspection_by_inspectionUid(InspectionUID);
                            if (dtjoininspection.Rows.Count == 0)
                            {
                                //  db.WebAPITransctionUpdate(transactionUid, "Failure", "Error:InspectionUid is not exists");
                                return Json(new
                                {
                                    Status = "Failure",
                                    Message = "Error:InspectionUid is not exists"
                                });
                            }
                        }
                        //-----------------------------------------------------------

                        DataTable dtWorkPackages = db.GetWorkPackages_ProjectName(projectName);
                        if (dtWorkPackages.Rows.Count > 0)
                        {
                            
                                    Guid AssignJointInspectionUID = Guid.NewGuid();
                                    
                                    if (InspectionUID.Contains("$"))
                                    {
                                        for (int i = 0; i < InspectionUIDList.Length; i++)
                                        {
                                            AssignJointInspectionUID = Guid.NewGuid();
                                            string boqDetailsUid = db.gbGetBoqDetailsUId_InspectionUId(new Guid(InspectionUIDList[i]));
                                            if (boqDetailsUid == "")
                                            {
                                                sError = true;
                                                ErrorText += "BOQDetails Uid not available for inspectionuid : " + InspectionUIDList[i];
                                            }
                                            else
                                            {
                                                cnt = db.InsertJointInspectiontoRAbill(AssignJointInspectionUID, new Guid(rabillUID), new Guid(boqDetailsUid), new Guid(InspectionUIDList[i]));
                                                if (cnt == 0)
                                                {
                                                    sError = true;
                                                    ErrorText += "Join Inspection to RABill is not inserted for inspectionuid :" + InspectionUIDList[i];
                                                }
                                            }

                                        }
                                    }
                                    else
                                    {
                                        string boqDetailsUid = db.gbGetBoqDetailsUId_InspectionUId(new Guid(InspectionUID));
                                        if (boqDetailsUid == "")
                                        {
                                            sError = true;
                                            ErrorText = "BOQDetails Uid not available for selcted inspectionuid";
                                        }
                                        else
                                        {
                                            cnt = db.InsertJointInspectiontoRAbill(AssignJointInspectionUID, new Guid(rabillUID), new Guid(boqDetailsUid), new Guid(InspectionUID));
                                            if (cnt == 0)
                                            {
                                                sError = true;
                                                ErrorText = "Join Inspection to RABill is not inserted";
                                            }
                                        }
                                    }
                                    //--------------------------------------------------
                                

                         }

                        

                        else
                        {
                            sError = true;
                            ErrorText = "No Workpackage available for select project";
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
                // db.WebAPITransctionUpdate(transactionUid, "Failure", "Error:" + ErrorText);
                return Json(new
                {
                    Status = "Failure",
                    Message = "Error:" + ErrorText
                });
            }
            else
            {

                // db.WebAPITransctionUpdate(transactionUid, "Success", " RABillUId = "+rabillUid+",Message = Successfully Updated Inspection to RAbill");
                return Json(new
                {
                    Status = "Success",
                    Message = "Successfully Updated Inspection to RAbill"
                });
            }
        }

        [Authorize]
        [HttpPost]
        [Route("api/Financial/GetDedudctionList")]
        public IHttpActionResult GetDedudctionList()
        {
            bool sError = false;
            string ErrorText = "";
            string deductionJson = "";
            int NoOflistitems = 0;
            try
            {
                var httpRequest = HttpContext.Current.Request;
                if (String.IsNullOrEmpty(httpRequest.Params["ProjectName"]))
                {
                    return Json(new
                    {
                        Status = "Failure",
                        Message = "Error:Project Name is Manadatory"
                    }); ;
                }
                //Insert into WebAPITransctions table
                var BaseURL = HttpContext.Current.Request.Url.ToString();
                string postData = "ProjectName=" + httpRequest.Params["ProjectName"] ;
                db.WebAPITransctionInsert(Guid.NewGuid(), BaseURL, postData, "");

                var identity = (ClaimsIdentity)User.Identity;
                if (db.CheckGetWebApiSettings(identity.Name, GetIp()) > 0)
                {
                    var projectName = httpRequest.Params["ProjectName"];
                   string projectExists = db.ProjectNameExists(projectName);
                    if (!string.IsNullOrEmpty(projectExists))
                    {
                        DataTable dtDeductionList = db.GetDeductionDesc();
                        deductionJson = JsonConvert.SerializeObject(dtDeductionList);
                        NoOflistitems = dtDeductionList.Rows.Count;
                    }
                    else
                    {
                        sError = true;
                        ErrorText = "Project Name not exists.";
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
                return Json(new
                {
                    Status = "Failure",
                    Message = "Error:" + ErrorText
                });
            }
            else if (NoOflistitems == 0)
            {
                return Json(new
                {
                    NoOflistitems = NoOflistitems,
                    Message = "No description found"

                });
            }
            else
            {
                return Json(new
                {
                    NoOflistitems = NoOflistitems,
                    DeductionDescription = deductionJson,

                });
            }
        }

        [Authorize]
        [HttpPost]
        [Route("api/Financial/AddEditInvoices")]
        public IHttpActionResult AddEditInvoices()
        {
            bool sError = false;
            string ErrorText = "";
            Guid InvoiceMaster_UID = Guid.NewGuid();
            var httpRequest = HttpContext.Current.Request;
            try
            {
                if (String.IsNullOrEmpty(httpRequest.Params["ProjectName"]) || String.IsNullOrEmpty(httpRequest.Params["Invoice Date"]) || String.IsNullOrEmpty(httpRequest.Params["Invoice Date"]) ||
                     String.IsNullOrEmpty(httpRequest.Params["Currency"]) || String.IsNullOrEmpty(httpRequest.Params["Rabill number"]) || String.IsNullOrEmpty(httpRequest.Params["Deductiondetails"]))
                {
                    return Json(new
                    {
                        Status = "Failure",
                        Message = "Error:Mandatory fields are missing"
                    }); ;
                }

                //Insert into WebAPITransctions table
                var BaseURL = HttpContext.Current.Request.Url.ToString();
                string postData = "ProjectName=" + httpRequest.Params["ProjectName"] + ";Invoice number=" + httpRequest.Params["Invoice Date"] + ";Invoice Date=" + httpRequest.Params["Invoice Date"] +
                    ";Currency=" + httpRequest.Params["Currency"] + ";Rabill number=" + httpRequest.Params["Rabill number"] + ";Deductiondetails=" + httpRequest.Params["Deductiondetails"];
                db.WebAPITransctionInsert(Guid.NewGuid(), BaseURL, postData, "");

                var identity = (ClaimsIdentity)User.Identity;
                if (db.CheckGetWebApiSettings(identity.Name, GetIp()) > 0)
                {
                    var projectName = httpRequest.Params["ProjectName"];
                    DataTable dtWorkPackages = db.GetWorkPackages_ProjectName(projectName);
                    if (dtWorkPackages.Rows.Count > 0)
                    {
                        var invoiceNumber = httpRequest.Params["Invoice number"];
                        var invoiceDate= httpRequest.Params["Invoice Date"]; ;
                        if (!string.IsNullOrEmpty(invoiceDate))
                        {
                            invoiceDate = db.ConvertDateFormat(invoiceDate);
                           // invoiceDate = Convert.ToDateTime(invoiceDate);
                        }
                       // var invoiceDate = httpRequest.Params["Invoice Date"];
                        var currency = httpRequest.Params["Currency"];
                        var raBillNumber = httpRequest.Params["Rabill number"];
                        //var numberofdeductions = httpRequest.Params["Number of deductions"];
                        var Deductiondetails = httpRequest.Params["Deductiondetails"];
                        decimal invoiceAmount = 0;
                        decimal invoiceforRaBill = 0;
                        JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                        deductionListClass deductionObj = jsonSerializer.Deserialize<deductionListClass>(Deductiondetails);
                        if (deductionObj.deductionList != null)
                        {
                            for (int i = 0; i < deductionObj.deductionList.Count; i++)
                            {

                                DataSet deductionDs = db.GetDeductionFromDesc(deductionObj.deductionList[i].deductionuid);

                                if (deductionDs.Tables[0].Rows.Count == 0)
                                {
                                    return Json(new
                                    {
                                        Status = "Failure",
                                        Message = "Error:  deducion id " + deductionObj.deductionList[i].deductionuid + " is invalid"
                                    });
                                }
                            }
                        }
                        for (int t = 0; t < raBillNumber.ToString().Split(',').Length; t++)
                        {
                            invoiceforRaBill = db.GetRAbillPresentTotalAmount_by_RABill_UID(new Guid(raBillNumber.ToString().Split(',')[t].ToString()));
                            if (invoiceforRaBill == -123)
                            {
                                return Json(new
                                {
                                    Status = "Failure",
                                    Message = "Error:Rabilluid-" + raBillNumber.ToString().Split(',')[t].ToString() + " not exists"
                                });
                            }
                            invoiceAmount += db.GetRAbillPresentTotalAmount_by_RABill_UID(new Guid(raBillNumber.ToString().Split(',')[t].ToString()));
                        }
                        
                        string sDate1 = "";
                        DateTime CDate1 = DateTime.Now;
                        string percentage = "0";

                        if (DateTime.TryParse(invoiceDate, out DateTime sDate))
                        {
                            sDate1 = sDate.ToString("dd/MM/yyyy");
                            sDate1 = db.ConvertDateFormat(sDate1);
                            CDate1 = Convert.ToDateTime(sDate1);

                            string Currecncy_CultureInfo = "";
                            if (currency == "INR")
                            {
                                Currecncy_CultureInfo = "en-IN";
                            }
                            else if (currency == "USD")
                            {
                                Currecncy_CultureInfo = "en-US";
                            }
                            else
                            {
                                Currecncy_CultureInfo = "ja-JP";
                            }
                            int cnt = db.InvoiceMaster_InsertorUpdate(InvoiceMaster_UID, new Guid(dtWorkPackages.Rows[0]["ProjectUID"].ToString()), new Guid(dtWorkPackages.Rows[0]["WorkpackageUID"].ToString()), invoiceNumber, "", CDate1, (currency == "INR") ? "&#x20B9;" : (currency == "USD") ? "&#36;" : "&#165;", Currecncy_CultureInfo);
                            if (cnt > 0)
                            {
                                string[] raBillsList = raBillNumber.ToString().Split(',');
                                for (int p = 0; p < raBillsList.Length; p++)
                                {
                                    int cntRAbill = invoice.Invoice_RABills_Insert(Guid.NewGuid(), InvoiceMaster_UID, new Guid(raBillsList[p].ToString()), CDate1);
                                    //int cnt= dbObj.AddRABillNumber_Invoice(hidInvoiceUId.Value,ddlRabillNumber.SelectedValue.ToString());
                                    if (cntRAbill > 0)
                                    {
                                        DataSet ds = invoice.GetInvoiceDeduction_by_InvoiceMaster_UID_With_Name(InvoiceMaster_UID);
                                        if (ds.Tables[0].Rows.Count > 0)
                                        {
                                            float Mobilization = 0;
                                            DataSet dsInvoice = invoice.GetInvoiceMaster_by_InvoiceMaster_UID(InvoiceMaster_UID);
                                            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                                            {
                                                float Percent = float.Parse(ds.Tables[0].Rows[i]["Percentage"].ToString());
                                                float InvoiceAmount = float.Parse(dsInvoice.Tables[0].Rows[0]["Invoice_TotalAmount"].ToString());
                                                if (i == 0)
                                                {
                                                    if (Percent > 0)
                                                    {
                                                        float finalamount = (InvoiceAmount * Percent) / 100;
                                                        Mobilization = finalamount;
                                                    }
                                                    else
                                                    {
                                                        Mobilization = InvoiceAmount;
                                                    }

                                                    int cnt1 = invoice.InvoiceDeduction_Amount_Update(new Guid(ds.Tables[0].Rows[i]["Invoice_DeductionUID"].ToString()), new Guid(ds.Tables[0].Rows[i]["InvoiceMaster_UID"].ToString()), Mobilization);
                                                    if (cnt1 <= 0)
                                                    {
                                                        sError = true;
                                                        ErrorText = "There is a problem with this feature. Please contact system admin.";
                                                        //  Page.ClientScript.RegisterStartupScript(Page.GetType(), "CLOSE", "<script language='javascript'>alert('There is a problem with this feature. Please contact system admin.');</script>");
                                                    }

                                                }
                                                else
                                                {
                                                    float finalamount = 0;
                                                    if (ds.Tables[0].Rows[i]["DeductionsDescription"].ToString() == "SGST" || ds.Tables[0].Rows[i]["DeductionsDescription"].ToString() == "CGST" || ds.Tables[0].Rows[i]["DeductionsDescription"].ToString() == "GST")
                                                    {
                                                        string sVal = invoice.GetGST_Calculation_Value("GST Calculation");
                                                        if (sVal != "" && !sVal.StartsWith("Error"))
                                                        {
                                                            finalamount = (Mobilization / float.Parse(sVal));
                                                            finalamount = (finalamount * Percent) / 100;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        finalamount = (Mobilization * Percent) / 100;
                                                    }

                                                    int cnt1 = invoice.InvoiceDeduction_Amount_Update(new Guid(ds.Tables[0].Rows[i]["Invoice_DeductionUID"].ToString()), new Guid(ds.Tables[0].Rows[i]["InvoiceMaster_UID"].ToString()), finalamount);
                                                    if (cnt1 <= 0)
                                                    {
                                                        sError = true;
                                                        ErrorText = "There is a problem with this feature. Please contact system admin.";
                                                        // Page.ClientScript.RegisterStartupScript(Page.GetType(), "CLOSE", "<script language='javascript'>alert('There is a problem with this feature. Please contact system admin.');</script>");
                                                    }
                                                }
                                            }
                                        }
                                        //Page.ClientScript.RegisterStartupScript(Page.GetType(), "CLOSE", "<script language='javascript'>parent.location.href=parent.location.href;</script>");
                                    }
                                    else if (cntRAbill == -1)
                                    {
                                        sError = true;
                                        ErrorText = "'RA Bill already exists for the invoice. Try with different RA Bill No.";
                                        // Page.ClientScript.RegisterStartupScript(Page.GetType(), "CLOSE", "<script language='javascript'>alert('RA Bill already exists for the invoice. Try with different RA Bill No.');</script>");
                                    }
                                }
                                if (deductionObj.deductionList != null)
                                {
                                    for (int i = 0; i < deductionObj.deductionList.Count; i++)
                                    {
                                        if (deductionObj.deductionList[i].deductionMode.ToLower() == "percentage")
                                        {
                                            percentage = "0";
                                            deductionObj.deductionList[i].Value = ((invoiceAmount / 100) * Convert.ToDecimal(deductionObj.deductionList[i].Value)).ToString();
                                        }
                                        else
                                        {
                                            percentage = deductionObj.deductionList[i].Value;
                                        }
                                        DataSet deductionDs = db.GetDeductionFromDesc(deductionObj.deductionList[i].deductionuid);

                                        if (deductionDs.Tables[0].Rows.Count > 0)
                                        {                                           
                                            db.InvoiceDeduction_InsertorUpdate(Guid.NewGuid(), new Guid(dtWorkPackages.Rows[0]["WorkpackageUID"].ToString()), InvoiceMaster_UID, new Guid(deductionDs.Tables[0].Rows[0]["UID"].ToString()), float.Parse(deductionObj.deductionList[i].Value), float.Parse(percentage), (currency == "INR") ? "&#x20B9;" : (currency == "USD") ? "&#36;" : "&#165;", Currecncy_CultureInfo, deductionObj.deductionList[i].deductionMode);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                sError = true;
                                ErrorText = "Invoice already exists";
                            }
                        }
                        else
                        {
                            sError = true;
                            ErrorText = "Date is not correct format.";
                        }
                    }
                    else
                    {
                        sError = true;
                        ErrorText = "Project Name not exists.";
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
                return Json(new
                {
                    Status = "Failure",
                    Message = "Error:" + ErrorText
                });
            }
            return Json(new
            {
                Status = "Success",
                InvouceUid = InvoiceMaster_UID,

            });
        }

        [Authorize]
        [HttpPost]
        [Route("api/Financial/AddPaymentDetails")]
        public IHttpActionResult AddPaymentDetails()
        {
            //Insert into WebAPITransctions table
            try
            {
                var httpRequest = HttpContext.Current.Request;
                var BaseURL = HttpContext.Current.Request.Url.ToString();
                string postData = "ProjectName=" + httpRequest.Params["ProjectName"] + ";WorkPackageName=" + httpRequest.Params["WorkpackageName"] + "; Month=" + httpRequest.Params["Month"] + ";Year=" + httpRequest.Params["Year"] +
                    ";InvoiceNo=" + httpRequest.Params["InvoiceNo"] + ";InvoiceAmount=" + httpRequest.Params["InvoiceAmount"] + ";NetAmount=" + httpRequest.Params["NetAmount"] + ";PaymentDate=" + httpRequest.Params["PaymentDate"];
                db.WebAPITransctionInsert(Guid.NewGuid(), BaseURL, postData, "");

                DataSet ds = new DataSet();
                DataSet dsInvDeduc = new DataSet();
                var identity = (ClaimsIdentity)User.Identity;
                var ProjectName = httpRequest.Params["ProjectName"];
                var WorkpackageName = httpRequest.Params["WorkpackageName"];
                var InvoiceNo = httpRequest.Params["InvoiceNo"];
                var Month = httpRequest.Params["Month"];
                int Year =int.Parse(httpRequest.Params["Year"]);
                Guid FinMonthUID = Guid.NewGuid();
                Guid InvoiceUID = Guid.NewGuid();
                Guid DeductionUID = new Guid();
                Guid PaymentUID = Guid.NewGuid();
                float TotalDeductions = 0.0f;
                float DeducAmnt = 0.0f;
                float Deducper = 0.0f;
                float Amount = float.Parse(httpRequest.Params["InvoiceAmount"]);
                float NetAmnt = float.Parse(httpRequest.Params["NetAmount"]);
                if (db.CheckGetWebApiSettings(identity.Name, GetIp()) == 0)
                {
                    return Json(new
                    {
                        Success = false,
                        Message = "Not Authorized IP address"
                    });
                }
                else
                {
                    var pExists = db.ProjectNameExists(ProjectName);
                    if (pExists != "")
                    {
                        var wExists = db.WorkpackageNameExists(WorkpackageName, pExists);
                        if (wExists != "")
                        {
                           

                            if(db.GetFinMonthUID(new Guid(wExists),Month,Year) != "00000000-0000-0000-0000-000000000000")
                            {
                                FinMonthUID =new Guid(db.GetFinMonthUID(new Guid(wExists), Month, Year));

                                if (db.GetInvoiceUIDFromInvoiceNo(new Guid(wExists),InvoiceNo) != "00000000-0000-0000-0000-000000000000")
                                {
                                    InvoiceUID = new Guid(db.GetInvoiceUIDFromInvoiceNo(new Guid(wExists), InvoiceNo));

                                    if(db.checkInvoicePaid(InvoiceUID) > 0)
                                    {
                                        var resultmain1 = new
                                        {
                                            Status = "Failure",
                                            Message = "Payment already done for the Invoice No !",
                                        };
                                        return Json(resultmain1);
                                    }

                                    dsInvDeduc = db.GetInvoiceDeductions(InvoiceUID);

                 
                                    foreach (DataRow dr in dsInvDeduc.Tables[0].Rows)
                                    {
                                        DeducAmnt = float.Parse(dr["Amount"].ToString());
                                        Deducper = float.Parse(dr["Percentage"].ToString());
                                        ds = db.GetDeductionFromDesc(dr["DeductionsDescription"].ToString());
                                        if (ds.Tables[0].Rows.Count > 0)
                                        {
                                            DeductionUID = new Guid(ds.Tables[0].Rows[0]["UID"].ToString());
                                        }
                                        TotalDeductions = TotalDeductions + DeducAmnt;
                                        db.InsertRABillsDeductions(Guid.NewGuid(), PaymentUID, DeductionUID, DeducAmnt, Deducper);
                                    }
                                    string sDate2 = httpRequest.Params["PaymentDate"];
                                   //
                                    sDate2 = db.ConvertDateFormat(sDate2);
                                    DateTime CDate2 = Convert.ToDateTime(sDate2);
                                    db.InsertRABillPayments(PaymentUID, InvoiceUID, "Invoice Amount", Amount, TotalDeductions, NetAmnt, FinMonthUID, CDate2);
                                    var resultmain = new
                                    {
                                        Status = "Success",
                                        PaymentUID = PaymentUID,
                                        Message = "Successfully Updated Payment details !",
                                    };
                                    return Json(resultmain);
                                }
                                else
                                {

                                    var resultmain = new
                                    {
                                        Status = "Failure",
                                        Message = "Invoice Number does not Exists !",
                                    };
                                    return Json(resultmain);
                                }
                            }
                            else
                            {
                                var resultmain = new
                                {
                                    Status = "Failure",
                                    Message = "Month and Year does not Exists !",
                                };
                                return Json(resultmain);
                            }

                            

                        }
                        else
                        {
                            var resultmain = new
                            {
                                Status = "Failure",
                                Message = "Workpackage does not Exists !",
                            };
                            return Json(resultmain);
                        }
                    }
                    else
                    {
                        var resultmain = new
                        {
                            Status = "Failure",
                            Message = "Project Name does not Exists !",
                        };
                        return Json(resultmain);

                    }
                }
            }
            catch(Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = "Error" + ex.Message
                });
            }
        }

        // added for  venkat on 19/02/2022
        /// <summary>
        /// Updated by venkat on 19 Feb 2022
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("api/Financial/AddIssues")]
        public IHttpActionResult AddIssues()
        {
            bool sError = false;
            string ErrorText = "";
            Guid InvoiceMaster_UID = Guid.NewGuid();
            var httpRequest = HttpContext.Current.Request;
            var transactionUid = Guid.NewGuid();
            var issue_uid = Guid.NewGuid();
            try
            {
                //Insert into WebAPITransctions table
                var BaseURL = HttpContext.Current.Request.Url.ToString();
                string postData = "ProjectName=" + httpRequest.Params["project name"] + ";Issue description=" + httpRequest.Params["Issue description"] + ";assigned user=" + httpRequest.Params["assigned user"] +
                    ";assigned date=" + httpRequest.Params["assigned date"] + ";reporting user=" + httpRequest.Params["reporting user"] + ";reporting date=" + httpRequest.Params["reporting date"] +
                    ";approving user=" + httpRequest.Params["approving user"] + ";issue proposed  close date=" + httpRequest.Params["issue proposed  close date"] + ";remarks=" + httpRequest.Params["remarks"] +
                    "issue document=" + httpRequest.Params["issue document"];
                db.WebAPITransctionInsert(transactionUid, BaseURL, postData, "");

                if (String.IsNullOrEmpty(httpRequest.Params["project name"]) || String.IsNullOrEmpty(httpRequest.Params["Issue description"]) || 
                      String.IsNullOrEmpty(httpRequest.Params["reporting user"]) || String.IsNullOrEmpty(httpRequest.Params["reporting date"]) )
                {
                   // db.WebAPITransctionUpdate(transactionUid, "Failure", "Error:Mandatory fields are missing");
                    return Json(new
                    {
                        Status = "Failure",
                        Message = "Error:Mandatory fields are missing"
                    });
                }


                var identity = (ClaimsIdentity)User.Identity;

                if (db.CheckGetWebApiSettings(identity.Name, GetIp()) > 0)
                {
                    var projectName = httpRequest.Params["project name"];
                    DataTable dtWorkPackages = db.GetWorkPackages_ProjectName(projectName);
                    if (dtWorkPackages.Rows.Count > 0)
                    {
                        DataTable dtIssuesExist = db.getIssuesByDescription( new Guid(dtWorkPackages.Rows[0]["WorkpackageUid"].ToString()), httpRequest.Params["Issue description"].ToString());
                        if(dtIssuesExist.Rows.Count>0)
                        {
                            return Json(new
                            {
                                Status = "Failure",
                                Message = "Issue already Exists"

                            });
                        }
                        //DataSet dsUserDetails = db.getUserDetails_by_EmailID(httpRequest.Params["approving user"].ToString());
                        //if (dsUserDetails.Tables[0].Rows.Count == 0)
                        //{
                        //   // db.WebAPITransctionUpdate(transactionUid, "Failure", "Approve user is not " +
                        //        //"available");
                        //    return Json(new
                        //    {
                        //        Status = "Failure",
                        //        Message = "Approve user is not available"

                        //    });

                        //}
                       // string approveUserUid = dsUserDetails.Tables[0].Rows[0]["UserUID"].ToString();

                        DataSet dsUserDetails = db.getUserDetails_by_EmailID(httpRequest.Params["reporting user"].ToString());

                        if (dsUserDetails.Tables[0].Rows.Count == 0)
                        {
                          //  db.WebAPITransctionUpdate(transactionUid, "Failure", "Reporting user is not  available");
                            return Json(new
                            {
                                Status = "Failure",
                                Message = "Reporting user is not available"

                            });

                        }
                        if(dsUserDetails.Tables[0].Rows[0]["IsContractor"].ToString() != "Y")
                        {
                            return Json(new
                            {
                                Status = "Failure",
                                Message = "Not Authorized to add issue"

                            });
                        }
                        string reportUserUid = dsUserDetails.Tables[0].Rows[0]["UserUID"].ToString();
                        //dsUserDetails = db.getUserDetails_by_EmailID(httpRequest.Params["assigned user"].ToString());
                        //if (dsUserDetails.Tables[0].Rows.Count == 0)
                        //{
                        //   // db.WebAPITransctionUpdate(transactionUid, "Failure", "Assigned user is not available");
                        //    return Json(new
                        //    {
                        //        Status = "Failure",
                        //        Message = "Assigned user is not available"

                        //    });

                        //}
                        //string assignedUserUid = dsUserDetails.Tables[0].Rows[0]["UserUID"].ToString();
                        DateTime reportDate = DateTime.Now;
                        if (!string.IsNullOrEmpty(httpRequest.Params["reporting date"]))
                        {
                            string tmpDate = db.ConvertDateFormat(httpRequest.Params["reporting date"]);
                            reportDate = Convert.ToDateTime(tmpDate);
                        }
                       
                        if (DateTime.TryParse(reportDate.ToString(), out  reportDate))// &&
                           // DateTime.TryParse(httpRequest.Params["assigned date"], out DateTime assignedDate) &&
                           // DateTime.TryParse(httpRequest.Params["issue proposed  close date"], out DateTime proposedClosedDate))
                        {

                            //AddDays need to removed when upload in indian server
                            if (reportDate.Date > DateTime.Now.Date.AddDays(1))
                            {
                              //  db.WebAPITransctionUpdate(transactionUid, "Failure", "Reporting Date should be less then assigned date");
                                return Json(new
                                {
                                    Status = "Failure",
                                    Message = "Reporting Date should not be greater then today date"

                                });
                            }
                            //if (reportDate > assignedDate)
                            //{
                            //   // db.WebAPITransctionUpdate(transactionUid, "Failure", "Reporting Date should be less then assigned date");
                            //    return Json(new
                            //    {
                            //        Status = "Failure",
                            //        Message = "Reporting Date should be less then assigned date"

                            //    });
                            //}
                            string DecryptPagePath = "";
                            for (int i = 0; i < httpRequest.Files.Count; i++)
                            {
                                HttpPostedFile httpPostedFile = httpRequest.Files[i];

                                if (httpPostedFile != null)
                                {
                                    string sDocumentPath = ConfigurationManager.AppSettings["DocumentsPath"] + "/Documents/Issues/";
                                    string FileDirectory = "~/Documents/Issues/";
                                    if (!Directory.Exists(sDocumentPath))
                                    {
                                        Directory.CreateDirectory(sDocumentPath);
                                    }

                                    string sFileName = Path.GetFileNameWithoutExtension(httpPostedFile.FileName);
                                    string Extn = Path.GetExtension(httpPostedFile.FileName);
                                    httpPostedFile.SaveAs(sDocumentPath + "/" + sFileName + Extn);
                                    //FileUploadDoc.SaveAs(Server.MapPath("~/Documents/Encrypted/" + sDocumentUID + "_" + txtDocName.Text + "_1"  + "_enp" + InputFile));
                                    string savedPath = sDocumentPath + "/" + sFileName + Extn;
                                    DecryptPagePath = sDocumentPath + "/" + sFileName + "_DE" + Extn;
                                    db.EncryptFile(savedPath, DecryptPagePath);
                                    DecryptPagePath = FileDirectory + "/" + sFileName + "_DE" + Extn;
                                }
                            }
                            string remarks = "";
                            //int Cnt = db.InsertorUpdateIssues(issue_uid, Guid.Empty, httpRequest.Params["Issue description"].ToString(), reportDate, new Guid(reportUserUid), new Guid(assignedUserUid), assignedDate, proposedClosedDate, new Guid(approveUserUid), proposedClosedDate, "Open", httpRequest.Params["remarks"].ToString(), new Guid(dtWorkPackages.Rows[0]["WorkpackageUid"].ToString()), new Guid(dtWorkPackages.Rows[0]["ProjectUid"].ToString()), DecryptPagePath);
                            if(!string.IsNullOrEmpty( httpRequest.Params["remarks"]))
                            {
                                remarks = httpRequest.Params["remarks"].ToString();
                            }
                            int Cnt = 0;
                            dsUserDetails = db.getUserDetails_by_EmailID("bm.srinivasamurthy@njsei.com");
                            if (projectName.ToUpper() == "CP-10" && dsUserDetails.Tables[0].Rows.Count>0)
                            {
                                 Cnt = db.InsertorUpdateIssues(issue_uid, Guid.Empty, httpRequest.Params["Issue description"].ToString(), reportDate, new Guid(reportUserUid), new Guid(dsUserDetails.Tables[0].Rows[0]["UserUID"].ToString()), DateTime.Now, DateTime.Now.AddDays(10), new Guid(dsUserDetails.Tables[0].Rows[0]["UserUID"].ToString()), DateTime.Now.AddDays(10), "Open", httpRequest.Params["remarks"].ToString(), new Guid(dtWorkPackages.Rows[0]["WorkpackageUid"].ToString()), new Guid(dtWorkPackages.Rows[0]["ProjectUid"].ToString()), DecryptPagePath);
                            }
                            else
                            {
                                 Cnt = db.InsertorUpdateIssues(issue_uid, Guid.Empty, httpRequest.Params["Issue description"].ToString(), reportDate, new Guid(reportUserUid), "Open", remarks, new Guid(dtWorkPackages.Rows[0]["WorkpackageUid"].ToString()), new Guid(dtWorkPackages.Rows[0]["ProjectUid"].ToString()), DecryptPagePath);
                            }
                            if (Cnt > 0)
                            {
                                sError = false;
                                ErrorText = "Inserted successfully";
                            }
                            else
                            {
                                sError = false;
                                ErrorText = "Status is not inserted,Please contact administrator";
                            }
                        }
                        else
                        {
                            sError = true;
                            ErrorText = "Date are not correct format.";
                        }

                    }
                    else
                    {
                      //  db.WebAPITransctionUpdate(transactionUid, "Failure", "Not Authorized IP address");

                        sError = true;
                        ErrorText = "ProjectName not exists";

                    }
                }
                else
                {
                  //  db.WebAPITransctionUpdate(transactionUid, "Failure", "Not Authorized IP address");
                    return Json(new
                    {
                        Status = "Failure",
                        Message = "Not Authorized IP address"
                    });

                }
            }
            catch (Exception ex)
            {
                sError = true;
                ErrorText = ex.Message;
            }
            if (sError)
            {
              //  db.WebAPITransctionUpdate(transactionUid, "Failure", "Error:" + ErrorText);
                return Json(new
                {
                    Status = "Failure",
                    Message = "Error:" + ErrorText
                });
            }
           // db.WebAPITransctionUpdate(transactionUid, "Success", ErrorText);
            return Json(new
            {
                Status = "Success",
                IssueUid = issue_uid,
                Message = ErrorText

            });
        }

        [Authorize]
        [HttpPost]
        [Route("api/Financial/UpdateIssueStatus")]
        public IHttpActionResult UpdateIssueStatus()
        {
            bool sError = false;
            string ErrorText = "";
            Guid InvoiceMaster_UID = Guid.NewGuid();
            var httpRequest = HttpContext.Current.Request;
            var transactionUid = Guid.NewGuid();
            bool transactionStatus = false;
            DateTime updatingDate = DateTime.MinValue;
            Guid issue_remarks_uid = Guid.NewGuid(); 
            try
            {
                //Insert into WebAPITransctions table
                var BaseURL = HttpContext.Current.Request.Url.ToString();
                string postData = "ProjectName=" + httpRequest.Params["project name"] + ";issue uid=" + httpRequest.Params["issue uid"] + ";issue status=" + httpRequest.Params["issue status"] + ";issue status update date=" + httpRequest.Params["updating date"] +
                    ";Issue status update user id=" + httpRequest.Params["updating user id"] + ";issue status document=" + httpRequest.Params["issue status document"] + ";remarks=" + httpRequest.Params["remarks"];
                db.WebAPITransctionInsert(transactionUid, BaseURL, postData, "");

                if (String.IsNullOrEmpty(httpRequest.Params["project name"]) || String.IsNullOrEmpty(httpRequest.Params["issue uid"]) || String.IsNullOrEmpty(httpRequest.Params["issue status"]) || String.IsNullOrEmpty(httpRequest.Params["updating user id"])
                    || string.IsNullOrEmpty(httpRequest.Params["updating date"]))
                {
                   // db.WebAPITransctionUpdate(transactionUid, "Failure", "Error:Mandatory fields are missing");
                    return Json(new
                    {
                        Status = "Failure",
                        Message = "Error:Mandatory fields are missing"
                    });
                }


                var identity = (ClaimsIdentity)User.Identity;

                if (db.CheckGetWebApiSettings(identity.Name, GetIp()) > 0)
                {
                    var projectName = httpRequest.Params["project name"];
                    DataTable dtWorkPackages = db.GetWorkPackages_ProjectName(projectName);
                    if (dtWorkPackages.Rows.Count == 0)
                    {
                        sError = true;
                        ErrorText = "ProjectName not exists";
                    }
                    else
                    {
                        DataSet dsIssues = db.getIssuesList_by_UID(new Guid(httpRequest.Params["issue uid"]));
                        if (dsIssues.Tables[0].Rows.Count == 0)
                        {
                            sError = true;
                            ErrorText = "Issue uid is not available";
                        }
                        else
                        {

                            DataSet dsUserDetails = db.getUserDetails_by_EmailID(httpRequest.Params["updating user id"].ToString());
                            if (dsUserDetails.Tables[0].Rows.Count == 0)
                            {
                                sError = true;
                                ErrorText = "User is not available";
                            }
                            else
                            {
                                DateTime reportDate = DateTime.Now;
                                if (!string.IsNullOrEmpty(httpRequest.Params["updating date"]))
                                {
                                    string tmpDate = db.ConvertDateFormat(httpRequest.Params["updating date"]);
                                    reportDate = Convert.ToDateTime(tmpDate);
                                }

                                if (DateTime.TryParse(reportDate.ToString(), out updatingDate))
                                {
                                    string userUid = dsUserDetails.Tables[0].Rows[0]["UserUID"].ToString();
                                    if (userUid.ToString().ToUpper() == dsIssues.Tables[0].Rows[0]["Assigned_User"].ToString().ToUpper() || userUid.ToString().ToUpper() == dsIssues.Tables[0].Rows[0]["Approving_User"].ToString().ToUpper())
                                    {
                                        if (dsIssues.Tables[0].Rows[0]["Assigned_User"].ToString() == dsIssues.Tables[0].Rows[0]["Approving_User"].ToString())
                                        {
                                            if (dsIssues.Tables[0].Rows[0]["Issue_Status"].ToString() == "Open" && (httpRequest.Params["issue status"].ToString() == "In-Progress" ||
                                                       httpRequest.Params["issue status"].ToString() == "Close" || httpRequest.Params["issue status"].ToString() == "Reject"))
                                            {
                                                transactionStatus = true;
                                            }
                                            else if (dsIssues.Tables[0].Rows[0]["Issue_Status"].ToString() == "In-Progress" && (httpRequest.Params["issue status"].ToString() == "In-Progress" ||
                                                      httpRequest.Params["issue status"].ToString() == "Close"))
                                            {

                                                transactionStatus = true;
                                            }
                                            else
                                            {
                                                sError = true;
                                                ErrorText = "User not allowed to update the status";
                                            }

                                        }
                                        else
                                        {
                                            if (dsIssues.Tables[0].Rows[0]["Assigned_User"].ToString().ToUpper() == userUid.ToString().ToUpper())
                                            {
                                                if (dsIssues.Tables[0].Rows[0]["Issue_Status"].ToString() == "Open" && httpRequest.Params["issue status"].ToString() == "In-Progress")
                                                {
                                                    transactionStatus = true;
                                                }
                                                else if (dsIssues.Tables[0].Rows[0]["Issue_Status"].ToString() == "In-Progress" && httpRequest.Params["issue status"].ToString() == "In-Progress")
                                                {
                                                    transactionStatus = true;
                                                }
                                                else
                                                {
                                                    sError = true;
                                                    ErrorText = "User not allowed to update the status";
                                                }
                                            }
                                            else
                                            {
                                                if (dsIssues.Tables[0].Rows[0]["Issue_Status"].ToString() == "In-Progress" && (httpRequest.Params["issue status"].ToString() == "Close" ||
                                                    httpRequest.Params["issue status"].ToString() == "Reject"))
                                                {
                                                    transactionStatus = true;
                                                }
                                                else
                                                {
                                                    sError = true;
                                                    ErrorText = "User not allowed to update the status";
                                                }

                                            }
                                        }

                                    }

                                    else
                                    {
                                        sError = true;
                                        ErrorText = "Issue uid and userid doen't match";
                                    }
                                }
                                else
                                {
                                    sError = true;
                                    ErrorText = "Update date is not correct format";
                                }

                            }
                            if (transactionStatus)
                            {
                                string DecryptPagePath = "";
                                for (int i = 0; i < httpRequest.Files.Count; i++)
                                {
                                    HttpPostedFile httpPostedFile = httpRequest.Files[i];

                                    if (httpPostedFile != null)
                                    {
                                        string sDocumentPath = ConfigurationManager.AppSettings["DocumentsPath"] + "/Documents/Issues/";
                                        string FileDirectory = "/Documents/Issues/";
                                        if (!Directory.Exists(sDocumentPath))
                                        {
                                            Directory.CreateDirectory(sDocumentPath);
                                        }
                                        string fileName = Path.GetFileName(httpPostedFile.FileName);
                                        string sFileName = Path.GetFileNameWithoutExtension(httpPostedFile.FileName);
                                        string Extn = Path.GetExtension(httpPostedFile.FileName);
                                        httpPostedFile.SaveAs(sDocumentPath + "/" + sFileName + Extn);
                                        //FileUploadDoc.SaveAs(Server.MapPath("~/Documents/Encrypted/" + sDocumentUID + "_" + txtDocName.Text + "_1"  + "_enp" + InputFile));
                                        string savedPath = sDocumentPath + "/" + sFileName + Extn;
                                        DecryptPagePath = sDocumentPath + "/" + sFileName + "_DE" + Extn;
                                        db.EncryptFile(savedPath, DecryptPagePath);
                                        DecryptPagePath = FileDirectory + "/" + sFileName + "_DE" + Extn;
                                        //added on 05/05/2022
                                        db.InsertUploadedDocument(sFileName + "_DE" + Extn, FileDirectory, issue_remarks_uid.ToString());
                                    }
                                }

                                int cnt = db.Issues_Status_Remarks_Insert(issue_remarks_uid, new Guid(httpRequest.Params["issue uid"]), httpRequest.Params["issue status"], httpRequest.Params["Remarks"], DecryptPagePath, updatingDate);
                                if (cnt > 0)
                                {
                                    sError = false;
                                    ErrorText = "Updated Successfully";
                                }
                                else
                                {
                                    sError = true;
                                    ErrorText = "Status is not Updated,Please contact administrator";
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
               // db.WebAPITransctionUpdate(transactionUid, "Failure", "Error:" + ErrorText);
                return Json(new
                {
                    Status = "Failure",
                    Message = "Error:" + ErrorText
                });
            }
           // db.WebAPITransctionUpdate(transactionUid, "Success", ErrorText);
            return Json(new
            {
                Status = "Success",
                Message = ErrorText

            });

        }


        /// <summary>
        /// Updated by venkat on 19 Feb 2022
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("api/Financial/GetIssueStatus")]
        public IHttpActionResult GetIssueStatus()
        {
            bool sError = false;
            string ErrorText = "";
            Guid InvoiceMaster_UID = Guid.NewGuid();
            var httpRequest = HttpContext.Current.Request;
            var transactionUid = Guid.NewGuid();
            DataTable dtIssueList = new DataTable();
            DataTable dtResponse = new DataTable();
            try
            {
                //Insert into WebAPITransctions table
                var BaseURL = HttpContext.Current.Request.Url.ToString();
                string postData = "ProjectName=" + httpRequest.Params["project name"] + ";IssueUid=" + httpRequest.Params["IssueUid"];
                db.WebAPITransctionInsert(transactionUid, BaseURL, postData, "");

                if (String.IsNullOrEmpty(httpRequest.Params["project name"]) || String.IsNullOrEmpty(httpRequest.Params["IssueUid"]))
                {
                    //db.WebAPITransctionUpdate(transactionUid, "Failure", "Error:Mandatory fields are missing");
                    return Json(new
                    {
                        Status = "Failure",
                        Message = "Error:Mandatory fields are missing"
                    });
                }

                var identity = (ClaimsIdentity)User.Identity;
                
                if (db.CheckGetWebApiSettings(identity.Name, GetIp()) > 0)
                {
                    var projectName = httpRequest.Params["project name"];
                    DataTable dtWorkPackages = db.GetWorkPackages_ProjectName(projectName);
                    if (dtWorkPackages.Rows.Count > 0)
                    {
                        DataSet dsIssuesList = db.GetIssueStatus_by_Issue_Uid(new Guid(httpRequest.Params["IssueUid"]));
                        if (dsIssuesList.Tables[0].Rows.Count > 0)
                        {
                            dtIssueList.Columns.Add("Status");
                            dtIssueList.Columns.Add("Updated Date");
                            dtIssueList.Columns.Add("FileName");
                            dtIssueList.Columns.Add("Remarks");
                            dtIssueList.Columns.Add("base64");

                            for (int i = 0; i < dsIssuesList.Tables[0].Rows.Count; i++)
                            {
                                DataRow dtNewRow = dtIssueList.NewRow();
                                dtNewRow["Status"] = dsIssuesList.Tables[0].Rows[i]["Issue_Status"].ToString();
                                dtNewRow["Remarks"] = dsIssuesList.Tables[0].Rows[i]["Issue_Remarks"].ToString();
                                dtNewRow["Updated Date"] = Convert.ToDateTime(dsIssuesList.Tables[0].Rows[i]["IssueRemark_Date"]).ToString("dd MMM yyyy");
                                dtNewRow["base64"] = "";
                                if (!string.IsNullOrEmpty(dsIssuesList.Tables[0].Rows[i]["Issue_Document"].ToString()))
                                {
                                    try
                                    {
                                        //  string path = System.Web.Hosting.HostingEnvironment.MapPath(dsIssuesList.Tables[0].Rows[i]["Issue_Document"].ToString());
                                        //  string fileExtension = System.IO.Path.GetExtension(dsIssuesList.Tables[0].Rows[i]["Issue_Document"].ToString());
                                        // string outPath = dsIssuesList.Tables[0].Rows[i]["Issue_Document"].ToString() + "_download" + fileExtension;
                                        // changed by zuber on 19/02/2022
                                        string path = ConfigurationManager.AppSettings["DocumentsPath"] + dsIssuesList.Tables[0].Rows[i]["Issue_Document"].ToString().Remove(0, 2);
                                        string fileName = System.IO.Path.GetFileName(ConfigurationManager.AppSettings["DocumentsPath"] + dsIssuesList.Tables[0].Rows[i]["Issue_Document"].ToString().Remove(0, 2));
                                        dtNewRow["FileName"] = fileName;
                                        string fileExtension = System.IO.Path.GetExtension(ConfigurationManager.AppSettings["DocumentsPath"] + dsIssuesList.Tables[0].Rows[i]["Issue_Document"].ToString().Remove(0, 2));
                                        string outPath = ConfigurationManager.AppSettings["DocumentsPath"] + dsIssuesList.Tables[0].Rows[i]["Issue_Document"].ToString().Remove(0, 2) + "_download" + fileExtension;
                                        //------------------------------------------------------------------------
                                        db.DecryptFile(path, outPath);

                                        Byte[] bytes = File.ReadAllBytes(outPath);
                                        dtNewRow["base64"] = Convert.ToBase64String(bytes);
                                        
                                    }
                                    catch (Exception ex)
                                    { }
                                    //getExtension = System.IO.Path.GetExtension(dtLastestStatus.Tables[0].Rows[0]["filePath"].ToString());
                                    //fileName = System.IO.Path.GetFileName(dtLastestStatus.Tables[0].Rows[0]["filePath"].ToString());
                                }

                                dtIssueList.Rows.Add(dtNewRow);
                            }
                        }
                            DataSet dsIssues=  db.getIssuesList_by_UID(new Guid(httpRequest.Params["IssueUid"]));
                            dtResponse.Columns.Add("issue name");
                            dtResponse.Columns.Add("issue uid");
                            dtResponse.Columns.Add("reporting user");
                            dtResponse.Columns.Add("reporting Date");
                            dtResponse.Columns.Add("Assigned User");
                            dtResponse.Columns.Add("Assigned Date");
                            dtResponse.Columns.Add("Proposed closure Date");
                            dtResponse.Columns.Add("Status", typeof(DataTable));
                           for (int i = 0; i < dsIssues.Tables[0].Rows.Count; i++)
                            {
                                DataRow dtNewRow = dtResponse.NewRow();
                                dtNewRow["issue uid"] = dsIssues.Tables[0].Rows[i]["Issue_Uid"].ToString();
                                dtNewRow["issue name"] = dsIssues.Tables[0].Rows[i]["Issue_Description"].ToString();
                                DataSet dsusers = db.getUserDetails_Contractor(new Guid(dsIssues.Tables[0].Rows[i]["Issued_User"].ToString()));
                                if (dsusers.Tables[0].Rows.Count > 0)
                                {
                                    dtNewRow["reporting user"] = dsusers.Tables[0].Rows[0]["UserName"].ToString();
                                }
                                if(!string.IsNullOrEmpty(dsIssues.Tables[0].Rows[i]["Issue_Date"].ToString()))
                                {
                                    dtNewRow["reporting date"] = Convert.ToDateTime(dsIssues.Tables[0].Rows[i]["Issue_Date"]).ToString("dd MMM yyyy");
                                }
                                dsusers=  db.getUserDetails_Contractor(new Guid(dsIssues.Tables[0].Rows[i]["Assigned_User"].ToString()));
                                if (dsusers.Tables[0].Rows.Count > 0)
                                {
                                    if (dsusers.Tables[0].Rows[0]["IsContractor"].ToString() == "Y")
                                    {
                                        dtNewRow["assigned user"] = dsusers.Tables[0].Rows[0]["UserName"].ToString();
                                    }
                                    else
                                    {
                                       dtResponse.Columns.Remove("assigned user");
                                    }
                                }
                                if (!string.IsNullOrEmpty(dsIssues.Tables[0].Rows[i]["Assigned_Date"].ToString()))
                                {
                                    dtNewRow["assigned date"] = Convert.ToDateTime(dsIssues.Tables[0].Rows[i]["Assigned_Date"]).ToString("dd MMM yyyy");
                                }

                                if (!string.IsNullOrEmpty(dsIssues.Tables[0].Rows[i]["Issue_ProposedCloser_Date"].ToString()))
                                {
                                    dtNewRow["proposed closure date"] = Convert.ToDateTime(dsIssues.Tables[0].Rows[i]["Issue_ProposedCloser_Date"]).ToString("dd MMM yyyy");
                                }
                                dtNewRow["status"] = dtIssueList;
                            dtResponse.Rows.Add(dtNewRow);
                            }
                            
                        }
                    else
                    {
                        sError = true;
                        ErrorText = "ProjectName not exists";
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
               // db.WebAPITransctionUpdate(transactionUid, "Failure", "Error:" + ErrorText);
                return Json(new
                {
                    Status = "Failure",
                    Message = "Error:" + ErrorText
                });
            }
          //  db.WebAPITransctionUpdate(transactionUid, "Success", ErrorText);
            return Json(new
            {
                Status = "Success",
                Message = JsonConvert.SerializeObject(dtResponse)

            });
        }

        /// <summary>
        /// Updated by venkat on 19 Feb 2022
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("api/Financial/GetDocumentStatusCodes")]
        public IHttpActionResult GetDocumentStatusCodes()
        {
            bool sError = false;
            string ErrorText = "";
            var httpRequest = HttpContext.Current.Request;
            DataTable dtDcoumentStatus = new DataTable();
            var transactionUid = Guid.NewGuid();
            try
            {
                //Insert into WebAPITransctions table
                var BaseURL = HttpContext.Current.Request.Url.ToString();
                string postData = "ProjectName=" + httpRequest.Params["ProjectName"];
                db.WebAPITransctionInsert(transactionUid, BaseURL, postData, "");

                if (String.IsNullOrEmpty(httpRequest.Params["ProjectName"]))
                {
                    //db.WebAPITransctionUpdate(transactionUid, "Failure", "ErPor:Mandatory fields are missing");
                    return Json(new
                    {
                        Status = "Failure",
                        Message = "Error:Mandatory fields are missing"
                    });
                }

                var identity = (ClaimsIdentity)User.Identity;

                if (db.CheckGetWebApiSettings(identity.Name, GetIp()) > 0)
                {
                    var projectName = httpRequest.Params["ProjectName"];
                    DataTable dtWorkPackages = db.GetWorkPackages_ProjectName(projectName);
                    if (dtWorkPackages.Rows.Count > 0)
                    {
                        dtDcoumentStatus = db.getDocumentStatusCodes(new Guid(dtWorkPackages.Rows[0]["ProjectUid"].ToString()));
                        
                    }
                    else
                    {
                        sError = true;
                        ErrorText = "ProjectName not exists";
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
              db.WebAPITransctionUpdate(transactionUid, "Success", ErrorText);
            return Json(new
            {
                Status = "Success",
                Message = JsonConvert.SerializeObject(dtDcoumentStatus)

            });
        }


    }
}