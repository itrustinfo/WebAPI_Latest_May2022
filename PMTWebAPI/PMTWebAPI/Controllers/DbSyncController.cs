﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;
using PMTWebAPI.DAL;
using PMTWebAPI.Models_Doc;

namespace PMTWebAPI.Controllers
{
    public class DbSyncController : ApiController
    {
        DBGetData db = new DBGetData();
        DBSyncData dbsync = new DBSyncData();
        TableProperties_sub tp = new TableProperties_sub();

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


        [HttpPost] // for getting Submittal Document flows
        public IHttpActionResult GetSubmittalDocumentFlows()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                var UserName = httpRequest.Params["UserName"];
                var Password = Security.Encrypt(httpRequest.Params["Password"]);
                DataSet ds = new DataSet();
                ds = db.GetDocumentFlows();
                return Json(new
                {
                    Success = true,
                    Ipaddress = GetIp()

                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = "Error" + ex.Message
                });
            }
        }

        [HttpPost] // for dbsync submittals
        public IHttpActionResult SubmmitalSync()
        {
            try
            {
                //var rawUrl =WebUtility.UrlDecode(HttpContext.Current.Request.RawUrl);
               // var test = HttpContext.Current.Request.QueryString["DocumentUID"];

               // WebUtility.UrlDecode
                var httpRequest = HttpContext.Current.Request;
                //var DocumentUID = httpRequest.Params["DocumentUID"];
                //var WorkPackageUID = httpRequest.Params["WorkPackageUID"];

                dbsync.Submittal_Insert_or_Update_Flow(new Guid(httpRequest.Params["DocumentUID"]), new Guid(httpRequest.Params["WorkPackageUID"]), new Guid(httpRequest.Params["ProjectUID"]), new Guid(httpRequest.Params["TaskUID"]), httpRequest.Params["DocName"], new Guid(httpRequest.Params["Doc_Category"]), httpRequest.Params["Doc_RefNumber"], httpRequest.Params["Doc_Type"], double.Parse(httpRequest.Params["Doc_Budget"]), new Guid(httpRequest.Params["FlowUID"]), DateTime.Parse(httpRequest.Params["Flow_StartDate"]), new Guid(httpRequest.Params["FlowStep1_UserUID"]), DateTime.Parse(httpRequest.Params["FlowStep1_TargetDate"]), httpRequest.Params["FlowStep2_UserUID"], httpRequest.Params["FlowStep2_TargetDate"], httpRequest.Params["FlowStep3_UserUID"], httpRequest.Params["FlowStep3_TargetDate"], httpRequest.Params["FlowStep4_UserUID"], httpRequest.Params["FlowStep4_TargetDate"], httpRequest.Params["FlowStep5_UserUID"], httpRequest.Params["FlowStep5_TargetDate"], int.Parse(httpRequest.Params["EstimatedDocuments"]), httpRequest.Params["Remarks"], httpRequest.Params["DocumentSearchType"], DateTime.Parse(httpRequest.Params["CreatedDate"]), httpRequest.Params["Delete_Flag"], httpRequest.Params["IsSync"],
                   
                    httpRequest.Params["FlowStep6_UserUID"], httpRequest.Params["FlowStep6_TargetDate"],
                    httpRequest.Params["FlowStep7_UserUID"], httpRequest.Params["FlowStep7_TargetDate"],
                    httpRequest.Params["FlowStep8_UserUID"], httpRequest.Params["FlowStep8_TargetDate"],
                    httpRequest.Params["FlowStep9_UserUID"], httpRequest.Params["FlowStep9_TargetDate"],
                    httpRequest.Params["FlowStep10_UserUID"], httpRequest.Params["FlowStep10_TargetDate"],
                    httpRequest.Params["FlowStep11_UserUID"], httpRequest.Params["FlowStep11_TargetDate"],
                    httpRequest.Params["FlowStep12_UserUID"], httpRequest.Params["FlowStep12_TargetDate"],
                    httpRequest.Params["FlowStep13_UserUID"], httpRequest.Params["FlowStep13_TargetDate"],
                    httpRequest.Params["FlowStep14_UserUID"], httpRequest.Params["FlowStep14_TargetDate"],
                    httpRequest.Params["FlowStep15_UserUID"], httpRequest.Params["FlowStep15_TargetDate"],
                    httpRequest.Params["FlowStep16_UserUID"], httpRequest.Params["FlowStep16_TargetDate"],
                    httpRequest.Params["FlowStep17_UserUID"], httpRequest.Params["FlowStep17_TargetDate"],
                    httpRequest.Params["FlowStep18_UserUID"], httpRequest.Params["FlowStep18_TargetDate"],
                    httpRequest.Params["FlowStep19_UserUID"], httpRequest.Params["FlowStep19_TargetDate"],
                    httpRequest.Params["FlowStep20_UserUID"], httpRequest.Params["FlowStep20_TargetDate"],
                httpRequest.Params["FlowStep1_IsMUser"], httpRequest.Params["FlowStep2_IsMUser"], httpRequest.Params["FlowStep3_IsMUser"],
                httpRequest.Params["FlowStep4_IsMUser"], httpRequest.Params["FlowStep5_IsMUser"], httpRequest.Params["FlowStep6_IsMUser"],
                httpRequest.Params["FlowStep7_IsMUser"], httpRequest.Params["FlowStep8_IsMUser"], httpRequest.Params["FlowStep9_IsMUser"],
                httpRequest.Params["FlowStep10_IsMUser"], httpRequest.Params["FlowStep11_IsMUser"], httpRequest.Params["FlowStep12_IsMUser"],
                httpRequest.Params["FlowStep13_IsMUser"], httpRequest.Params["FlowStep14_IsMUser"], httpRequest.Params["FlowStep15_IsMUser"],
                 httpRequest.Params["FlowStep16_IsMUser"], httpRequest.Params["FlowStep17_IsMUser"], httpRequest.Params["FlowStep18_IsMUser"],
                  httpRequest.Params["FlowStep19_IsMUser"], httpRequest.Params["FlowStep20_IsMUser"]
                );

                return Json(new
                {
                    Success = true
                    
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = "Error" + ex.Message
                });
            }
        }

        [HttpPost] // for dbsync ActualDocuments
        public IHttpActionResult ActualDocumentsSync()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                //var DocumentUID = httpRequest.Params["DocumentUID"];
                //var WorkPackageUID = httpRequest.Params["WorkPackageUID"];

                dbsync.Document_Insert_or_Update_ActualDocuments(new Guid(httpRequest.Params["ActualDocumentUID"]), new Guid(httpRequest.Params["ProjectUID"]), new Guid(httpRequest.Params["WorkPackageUID"]), new Guid(httpRequest.Params["DocumentUID"]), httpRequest.Params["ProjectRef_Number"], httpRequest.Params["Ref_Number"], httpRequest.Params["Doc_Type"],
                   DateTime.Parse(httpRequest.Params["IncomingRec_Date"]), new Guid(httpRequest.Params["FlowUID"].ToString()), httpRequest.Params["ActualDocument_Name"].ToString(), httpRequest.Params["Description"].ToString(), double.Parse(httpRequest.Params["ActualDocument_Version"].ToString()), httpRequest.Params["ActualDocument_Type"].ToString(),
                            httpRequest.Params["Media_HC"].ToString(), httpRequest.Params["Media_SC"].ToString(), httpRequest.Params["Media_SCEF"].ToString(), httpRequest.Params["Media_HCR"].ToString(), httpRequest.Params["Media_SCR"].ToString(), httpRequest.Params["Media_NA"].ToString(), httpRequest.Params["ActualDocument_Path"].ToString(), httpRequest.Params["Remarks"].ToString(), httpRequest.Params["FileRef_Number"].ToString(), httpRequest.Params["ActualDocument_CurrentStatus"].ToString(),
                            httpRequest.Params["FlowStep1_TargetDate"].ToString(), httpRequest.Params["FlowStep2_TargetDate"].ToString(), httpRequest.Params["FlowStep3_TargetDate"].ToString(), httpRequest.Params["FlowStep4_TargetDate"].ToString(), httpRequest.Params["FlowStep5_TargetDate"].ToString(), httpRequest.Params["ActualDocument_Originator"].ToString(), httpRequest.Params["Document_Date"].ToString(), httpRequest.Params["ActualDocument_RelativePath"].ToString(), httpRequest.Params["ActualDocument_DirectoryName"].ToString(), httpRequest.Params["ActualDocument_CreatedDate"].ToString(), httpRequest.Params["Delete_Flag"].ToString(), httpRequest.Params["CoverLetterUID"].ToString(), httpRequest.Params["SubmissionType"].ToString(),
                            httpRequest.Params["FlowStep6_TargetDate"].ToString(), httpRequest.Params["FlowStep7_TargetDate"].ToString(), httpRequest.Params["FlowStep8_TargetDate"].ToString(),
                            httpRequest.Params["FlowStep9_TargetDate"].ToString(), httpRequest.Params["FlowStep10_TargetDate"].ToString(), httpRequest.Params["FlowStep11_TargetDate"].ToString(),
                            httpRequest.Params["FlowStep12_TargetDate"].ToString(), httpRequest.Params["FlowStep13_TargetDate"].ToString(), httpRequest.Params["FlowStep14_TargetDate"].ToString(), httpRequest.Params["FlowStep15_TargetDate"].ToString(),
                            httpRequest.Params["FlowStep16_TargetDate"].ToString(), httpRequest.Params["FlowStep17_TargetDate"].ToString(), httpRequest.Params["FlowStep18_TargetDate"].ToString(), httpRequest.Params["FlowStep19_TargetDate"].ToString(), httpRequest.Params["FlowStep20_TargetDate"].ToString());

               

                return Json(new
                {
                    Success = true

                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = "Error" + ex.Message
                });
            }
        }

        [HttpPost] // for dbsync DocumentStatus
        public IHttpActionResult DocumentStatusSync()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                //var DocumentUID = httpRequest.Params["DocumentUID"];
                //var WorkPackageUID = httpRequest.Params["WorkPackageUID"];

                dbsync.InsertorUpdateDocumentStatus(new Guid(httpRequest.Params["StatusUID"].ToString()), new Guid(httpRequest.Params["DocumentUID"].ToString()), double.Parse(httpRequest.Params["Version"].ToString()), httpRequest.Params["ActivityType"].ToString(), httpRequest.Params["Activity_Budget"].ToString(),
                           DateTime.Parse(httpRequest.Params["ActivityDate"].ToString()), httpRequest.Params["LinkToReviewFile"].ToString(), new Guid(httpRequest.Params["AcivityUserUID"].ToString()), httpRequest.Params["Status_Comments"].ToString(), httpRequest.Params["Current_Status"].ToString(), httpRequest.Params["Ref_Number"].ToString(), httpRequest.Params["DocumentDate"].ToString(), httpRequest.Params["CoverLetterFile"].ToString(), httpRequest.Params["Delete_Flag"].ToString(), httpRequest.Params["Origin"].ToString(), httpRequest.Params["CreatedDate"].ToString(), httpRequest.Params["Forwarded"].ToString());
                return Json(new
                {
                    Success = true

                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = "Error" + ex.Message
                });
            }
        }

        [HttpPost] // for dbsync RefNoHistory added on 08/04/2022
        public IHttpActionResult ReferencNoHistorySync()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                //var DocumentUID = httpRequest.Params["DocumentUID"];
                //var WorkPackageUID = httpRequest.Params["WorkPackageUID"];

                dbsync.InsertorUpdateReferencNoHistory(new Guid(httpRequest.Params["UID"].ToString()), new Guid(httpRequest.Params["ActualDocumentUID"].ToString()),httpRequest.Params["OriginatorReferenceNo"].ToString(), httpRequest.Params["ONTBRefNo"].ToString(), httpRequest.Params["CreatedDate"].ToString());
                return Json(new
                {
                    Success = true

                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = "Error" + ex.Message
                });
            }
        }


        [HttpPost] // for dbsync Submittal_MultipleUsersSync added on 09/04/2022
        public IHttpActionResult Submittal_MultipleUsersSync()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                //var DocumentUID = httpRequest.Params["DocumentUID"];
                //var WorkPackageUID = httpRequest.Params["WorkPackageUID"];

                dbsync.InsertOrUpdateSubmittal_MultipleUsers(new Guid(httpRequest.Params["UID"].ToString()), new Guid(httpRequest.Params["SubmittalUID"].ToString()),Convert.ToInt16(httpRequest.Params["Step"].ToString()), new Guid(httpRequest.Params["UserUID"].ToString()));
                return Json(new
                {
                    Success = true

                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = "Error" + ex.Message
                });
            }
        }

        [HttpPost] // for dbsync Flow_Master_UsersSync added on 11/04/2022
        public IHttpActionResult Flow_Master_UsersSync()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                //var DocumentUID = httpRequest.Params["DocumentUID"];
                //var WorkPackageUID = httpRequest.Params["WorkPackageUID"];

                dbsync.InsertorUpdate_Flow_Master_Users(new Guid(httpRequest.Params["UID"].ToString()), new Guid(httpRequest.Params["FlowUID"].ToString()), new Guid(httpRequest.Params["ProjectUID"].ToString()), new Guid(httpRequest.Params["WorkpackageUID"].ToString()), new Guid(httpRequest.Params["WorkpackagecategoryUID"].ToString()) ,Convert.ToInt16(httpRequest.Params["step"].ToString()), new Guid(httpRequest.Params["UserUID"].ToString()), httpRequest.Params["CreatedDate"].ToString(), httpRequest.Params["DeletedFlag"].ToString());
                return Json(new
                {
                    Success = true

                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = "Error" + ex.Message
                });
            }
        }

        [HttpPost] // for dbsync MeasurementBookSync added on 13/04/2022
        public IHttpActionResult MeasurementBookSync()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                //var DocumentUID = httpRequest.Params["DocumentUID"];
                //var WorkPackageUID = httpRequest.Params["WorkPackageUID"];

                dbsync.InsertorUpdate_MeasurementBook(new Guid(httpRequest.Params["UID"].ToString()), new Guid(httpRequest.Params["TaskUID"].ToString()), httpRequest.Params["UnitforProgress"].ToString(), httpRequest.Params["Quantity"].ToString(), httpRequest.Params["Description"].ToString(), httpRequest.Params["Upload_File"].ToString(), httpRequest.Params["CreatedDate"].ToString(), httpRequest.Params["CreatedByUID"].ToString(), httpRequest.Params["Remarks"].ToString(), httpRequest.Params["Delete_Flag"].ToString(), httpRequest.Params["Achieved_Date"].ToString());
                return Json(new
                {
                    Success = true

                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = "Error" + ex.Message
                });
            }
        }

        [HttpPost] // for dbsync DocumentVersion
        public IHttpActionResult DocumentVersionSync()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                //var DocumentUID = httpRequest.Params["DocumentUID"];
                //var WorkPackageUID = httpRequest.Params["WorkPackageUID"];

                dbsync.InsertDocumentorUpdateVersion(new Guid(httpRequest.Params["DocVersion_UID"].ToString()), new Guid(httpRequest.Params["DocStatus_UID"].ToString()), new Guid(httpRequest.Params["DocumentUID"].ToString()), httpRequest.Params["Doc_Type"].ToString(), httpRequest.Params["Doc_FileName"].ToString(), httpRequest.Params["Doc_Comments"].ToString(), int.Parse(httpRequest.Params["Doc_Version"].ToString()),
                            httpRequest.Params["Doc_Status"].ToString(), httpRequest.Params["Doc_StatusDate"].ToString(), httpRequest.Params["Delete_Flag"].ToString(), httpRequest.Params["Doc_CoverLetter"].ToString());
                return Json(new
                {
                    Success = true

                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = "Error" + ex.Message
                });
            }
        }

        [HttpPost] // for dbsync DocumentFlowData
        public IHttpActionResult DocumentFlowDataSync()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                //var DocumentUID = httpRequest.Params["DocumentUID"];
                //var WorkPackageUID = httpRequest.Params["WorkPackageUID"];

               dbsync.InsertorUpdateDocumentflowdata(new Guid(httpRequest.Params["DocumentFlow_UID"].ToString()), new Guid(httpRequest.Params["FlowMasterUID"].ToString()), new Guid(httpRequest.Params["DocumentUID"].ToString()), new Guid(httpRequest.Params["FlowStep1_UserID"].ToString()), new Guid(httpRequest.Params["FlowStep1_UserRole"].ToString()), DateTime.Parse(httpRequest.Params["FlowStep1_TargetDate"].ToString()), httpRequest.Params["FlowStep1_DisplayName"].ToString(),
                            httpRequest.Params["FlowStep2_UserID"].ToString(), httpRequest.Params["FlowStep2_UserRole"].ToString(), httpRequest.Params["FlowStep2_TargetDate"].ToString(), httpRequest.Params["FlowStep2_DisplayName"].ToString(),
                            httpRequest.Params["FlowStep3_UserID"].ToString(), httpRequest.Params["FlowStep3_UserRole"].ToString(), httpRequest.Params["FlowStep3_TargetDate"].ToString(), httpRequest.Params["FlowStep3_DisplayName"].ToString(),
                            httpRequest.Params["FlowStep4_UserID"].ToString(), httpRequest.Params["FlowStep4_UserRole"].ToString(), httpRequest.Params["FlowStep4_TargetDate"].ToString(), httpRequest.Params["FlowStep4_DisplayName"].ToString(),
                            httpRequest.Params["FlowStep5_UserID"].ToString(), httpRequest.Params["FlowStep5_UserRole"].ToString(), httpRequest.Params["FlowStep5_TargetDate"].ToString(), httpRequest.Params["FlowStep5_DisplayName"].ToString(),
                            httpRequest.Params["FlowStep6_UserID"].ToString(), httpRequest.Params["FlowStep6_UserRole"].ToString(), httpRequest.Params["FlowStep6_TargetDate"].ToString(), httpRequest.Params["FlowStep6_DisplayName"].ToString(), httpRequest.Params["Delete_Flag"].ToString());
                return Json(new
                {
                    Success = true

                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = "Error" + ex.Message
                });
            }
        }

        [HttpPost] // for dbsync UploadDocs
        public IHttpActionResult UploadDocs()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                //var DocumentUID = httpRequest.Params["DocumentUID"];


                var relativepath = httpRequest.Params["relativepath"];
                for (int i = 0; i < httpRequest.Files.Count; i++)
                {
                    HttpPostedFile httpPostedFile = httpRequest.Files[i];

                    if (httpPostedFile != null)
                    {
                        string sDocumentPath = string.Empty;
                        //sDocumentPath = "~/" + Request.QueryString["ProjectUID"] + "/" + Request.QueryString["DocID"] + "/" + StatusUID + "/" + FileDatetime + "/CoverLetter";
                        sDocumentPath = ConfigurationManager.AppSettings["DocumentsPath"] + relativepath;

                        if (!Directory.Exists(sDocumentPath))
                        {
                            Directory.CreateDirectory(sDocumentPath);
                        }

                        string sFileName = Path.GetFileNameWithoutExtension(httpPostedFile.FileName);
                        string Extn = System.IO.Path.GetExtension(httpPostedFile.FileName);
                        httpPostedFile.SaveAs(sDocumentPath + sFileName + Extn);

                    }
                }

                return Json(new
                {
                    Success = true

                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = "Error" + ex.Message
                });
            }
        }

        [HttpPost] // for dbsync UploadDocs
        public IHttpActionResult UploadReviewFile()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                //var DocumentUID = httpRequest.Params["DocumentUID"];


                var relativepath = httpRequest.Params["relativepath"];
                for (int i = 0; i < httpRequest.Files.Count; i++)
                {
                    HttpPostedFile httpPostedFile = httpRequest.Files[i];

                    if (httpPostedFile != null)
                    {
                        string sDocumentPath = string.Empty;
                        //sDocumentPath = "~/" + Request.QueryString["ProjectUID"] + "/" + Request.QueryString["DocID"] + "/" + StatusUID + "/" + FileDatetime + "/CoverLetter";
                        sDocumentPath = ConfigurationManager.AppSettings["DocumentsPathReviewFile"] + relativepath;

                        if (!Directory.Exists(sDocumentPath))
                        {
                            Directory.CreateDirectory(sDocumentPath);
                        }

                        string sFileName = Path.GetFileNameWithoutExtension(httpPostedFile.FileName);
                        string Extn = System.IO.Path.GetExtension(httpPostedFile.FileName);
                        httpPostedFile.SaveAs(sDocumentPath + sFileName + Extn);

                    }
                }

                return Json(new
                {
                    Success = true

                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = "Error" + ex.Message
                });
            }
        }

        [HttpPost] // for dbsync DocumentVersion
        public IHttpActionResult WorddocReadSync()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                //var DocumentUID = httpRequest.Params["DocumentUID"];
                //var WorkPackageUID = httpRequest.Params["WorkPackageUID"];

                dbsync.InsertWordocRead(new Guid(httpRequest.Params["UID"].ToString()), httpRequest.Params["Doc_path"].ToString(), httpRequest.Params["Status"].ToString(), httpRequest.Params["HTML_Text"].ToString(), httpRequest.Params["CreatedDate"].ToString(), new Guid(httpRequest.Params["DocumemtUID"].ToString()), httpRequest.Params["Encrypted"].ToString());
               
                return Json(new
                {
                    Success = true

                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = "Error" + ex.Message
                });
            }
        }


        [HttpPost] // for dbsync DocumentUploadLog
        public IHttpActionResult DocumentsUploadLog()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                //var DocumentUID = httpRequest.Params["DocumentUID"];
                //var WorkPackageUID = httpRequest.Params["WorkPackageUID"];

                dbsync.InsertDocumentsUploadLog(new Guid(httpRequest.Params["ActualDocumentUID"].ToString()), httpRequest.Params["UploadStartDate"].ToString(), httpRequest.Params["UploadEndDate"].ToString(), new Guid(httpRequest.Params["UploadUserUID"].ToString()), httpRequest.Params["Duration"].ToString());

                return Json(new
                {
                    Success = true

                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = "Error" + ex.Message
                });
            }
        }


        [HttpPost] // for dbsync RABill_Abstract
        public IHttpActionResult RABill_AbstractSync()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
               
                dbsync.InsertOrUpdateRABill_Abstract(new Guid(httpRequest.Params["RABillUid"].ToString()), new Guid(httpRequest.Params["WorkpackageUID"].ToString()), httpRequest.Params["RABillNumber"].ToString(), httpRequest.Params["RABill_Date"].ToString(), httpRequest.Params["DeleteFlag"].ToString());
                return Json(new
                {
                    Success = true

                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = "Error" + ex.Message
                });
            }
        }

        [HttpPost] // for dbsync RABills
        public IHttpActionResult RABillsSync()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;

                dbsync.InsertOrUpdateRABills(new Guid(httpRequest.Params["itemUId"].ToString()), new Guid(httpRequest.Params["ProjectUID"].ToString()), new Guid(httpRequest.Params["WorkpackageUID"].ToString()), httpRequest.Params["item_number"].ToString(), httpRequest.Params["item_desc"].ToString(), decimal.Parse(httpRequest.Params["current_cost"].ToString()), decimal.Parse(httpRequest.Params["cumulative_cost"].ToString()), httpRequest.Params["created_date"].ToString(), new Guid(httpRequest.Params["RABillUid"].ToString()), new Guid(httpRequest.Params["UID"].ToString()), httpRequest.Params["DeleteFlag"].ToString());
                return Json(new
                {
                    Success = true

                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = "Error" + ex.Message
                });
            }
        }

        [HttpPost] // for dbsync DeductionsMaster
        public IHttpActionResult DeductionsMasterSync()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                dbsync.InsertOrUpdateDeductionsMaster(new Guid(httpRequest.Params["UID"].ToString()), httpRequest.Params["DeductionsDescription"].ToString(), decimal.Parse(httpRequest.Params["Maxpercentage"].ToString()), Int16.Parse(httpRequest.Params["Order_By"].ToString()));
                return Json(new
                {
                    Success = true

                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = "Error" + ex.Message
                });
            }
        }

        [HttpPost] // for dbsync InvoiceMaster
        public IHttpActionResult InvoiceMasterSync()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                dbsync.InsertorUpdate_InvoiceMaster(new Guid(httpRequest.Params["InvoiceMaster_UID"].ToString()), new Guid(httpRequest.Params["ProjectUID"].ToString()), new Guid(httpRequest.Params["WorkpackageUID"].ToString()), httpRequest.Params["Invoice_Number"].ToString(), httpRequest.Params["Invoice_Desc"].ToString(), httpRequest.Params["Invoice_Date"].ToString(), decimal.Parse(httpRequest.Params["Invoice_TotalAmount"].ToString()), decimal.Parse(httpRequest.Params["Invoice_DeductionAmount"].ToString()), decimal.Parse(httpRequest.Params["Invoice_NetAmount"].ToString()), httpRequest.Params["Currency"].ToString(), httpRequest.Params["Currency_CultureInfo"].ToString(), httpRequest.Params["Delete_Flag"].ToString());
                return Json(new
                {
                    Success = true

                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = "Error" + ex.Message
                });
            }
        }

        [HttpPost] // for dbsync InvoiceRABills
        public IHttpActionResult InvoiceRABillsSync()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                
                dbsync.InsertorUpdate_InvoiceRABills(new Guid(httpRequest.Params["InvoiceRABill_UID"].ToString()), new Guid(httpRequest.Params["InvoiceMaster_UID"].ToString()), new Guid(httpRequest.Params["RABillUid"].ToString()), httpRequest.Params["InvoiceRABill_Date"].ToString(), httpRequest.Params["Delete_Flag"].ToString());
                return Json(new
                {
                    Success = true

                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = "Error" + ex.Message
                });
            }
        }

        [HttpPost] // for dbsync InvoiceDeduction
        public IHttpActionResult InvoiceDeductionSync()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;

                dbsync.InsertorUpdate_InvoiceDeduction(new Guid(httpRequest.Params["Invoice_DeductionUID"].ToString()), new Guid(httpRequest.Params["WorkpackageUID"].ToString()), new Guid(httpRequest.Params["InvoiceMaster_UID"].ToString()), new Guid(httpRequest.Params["Deduction_UID"].ToString()), decimal.Parse(httpRequest.Params["Amount"].ToString()), httpRequest.Params["Currency"].ToString(), httpRequest.Params["Currency_CultureInfo"].ToString(), float.Parse(httpRequest.Params["Percentage"].ToString()), httpRequest.Params["Deduction_Mode"].ToString(),Int16.Parse(httpRequest.Params["Order_By"].ToString()), httpRequest.Params["Delete_Flag"].ToString());
                return Json(new
                {
                    Success = true

                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = "Error" + ex.Message
                });
            }
        }

        [HttpPost] // for dbsync AssignJointInspectiontoRAbill
        public IHttpActionResult AssignJointInspectiontoRAbillSync()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;

                // dbsync.InsertorUpdate_InvoiceDeduction(new Guid(httpRequest.Params["Invoice_DeductionUID"].ToString()), new Guid(httpRequest.Params["WorkpackageUID"].ToString()), new Guid(httpRequest.Params["InvoiceMaster_UID"].ToString()), new Guid(httpRequest.Params["Deduction_UID"].ToString()), decimal.Parse(httpRequest.Params["Amount"].ToString()), httpRequest.Params["Currency"].ToString(), httpRequest.Params["Currency_CultureInfo"].ToString(), float.Parse(httpRequest.Params["Percentage"].ToString()), httpRequest.Params["Deduction_Mode"].ToString(), Int16.Parse(httpRequest.Params["Order_By"].ToString()), httpRequest.Params["Delete_Flag"].ToString());
                dbsync.InsertorUpdate_AssignJointInspectiontoRAbill(new Guid(httpRequest.Params["AssignJointInspectionUID"].ToString()), new Guid(httpRequest.Params["RABill_UID"].ToString()), new Guid(httpRequest.Params["RABill_ItemUID"].ToString()), new Guid(httpRequest.Params["InspectionUID"].ToString()), httpRequest.Params["Assign_Date"].ToString());
                return Json(new
                {
                    Success = true

                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = "Error" + ex.Message
                });
            }
        }

        [HttpPost] // for dbsync get status doc count
        public IHttpActionResult InsertOrUpdateDocCount()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;

                // dbsync.InsertorUpdate_InvoiceDeduction(new Guid(httpRequest.Params["Invoice_DeductionUID"].ToString()), new Guid(httpRequest.Params["WorkpackageUID"].ToString()), new Guid(httpRequest.Params["InvoiceMaster_UID"].ToString()), new Guid(httpRequest.Params["Deduction_UID"].ToString()), decimal.Parse(httpRequest.Params["Amount"].ToString()), httpRequest.Params["Currency"].ToString(), httpRequest.Params["Currency_CultureInfo"].ToString(), float.Parse(httpRequest.Params["Percentage"].ToString()), httpRequest.Params["Deduction_Mode"].ToString(), Int16.Parse(httpRequest.Params["Order_By"].ToString()), httpRequest.Params["Delete_Flag"].ToString());
                Int64 destcount = dbsync.GetStatusDocCount(new Guid(httpRequest.Params["WorkPackageUID"].ToString()));
                dbsync.InsertOrUpdateDocCount(new Guid(httpRequest.Params["ProjectUID"].ToString()), new Guid(httpRequest.Params["WorkPackageUID"].ToString()), Int64.Parse(httpRequest.Params["SourceDocCount"].ToString()), destcount);
                return Json(new
                {
                    Success = true

                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = "Error" + ex.Message
                });
            }
        }


        [HttpPost] // for dbsync USer Details
        public IHttpActionResult UserDetailsSync()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
               
                dbsync.InsertOrUpdate_UserDetails(new Guid(httpRequest.Params["UserUID"].ToString()), httpRequest.Params["FirstName"].ToString(), httpRequest.Params["LastName"].ToString(), httpRequest.Params["EmailID"].ToString(), httpRequest.Params["Phonenumber"].ToString(), httpRequest.Params["Mobilenumber"].ToString(), httpRequest.Params["Address1"].ToString(), httpRequest.Params["Address2"].ToString(), httpRequest.Params["Username"].ToString(), httpRequest.Params["password"].ToString(), httpRequest.Params["CreatedDate"].ToString(), httpRequest.Params["DeletedFlag"].ToString(), httpRequest.Params["DeletedDate"].ToString(), httpRequest.Params["TypeOfUser"].ToString(), new Guid(httpRequest.Params["Admin_Under"].ToString()), httpRequest.Params["Project_Under"].ToString(), httpRequest.Params["Profile_Pic"].ToString());
                return Json(new
                {
                    Success = true

                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = "Error" + ex.Message
                });
            }
        }

        [HttpPost] // for dbsync UserProjects
        public IHttpActionResult UserProjectsSync()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;

                // dbsync.InsertOrUpdate_UserDetails(new Guid(httpRequest.Params["UserUID"].ToString()), httpRequest.Params["FirstName"].ToString(), httpRequest.Params["LastName"].ToString(), httpRequest.Params["EmailID"].ToString(), httpRequest.Params["Phonenumber"].ToString(), httpRequest.Params["Mobilenumber"].ToString(), httpRequest.Params["Address1"].ToString(), httpRequest.Params["Address2"].ToString(), httpRequest.Params["Username"].ToString(), httpRequest.Params["password"].ToString(), httpRequest.Params["CreatedDate"].ToString(), httpRequest.Params["DeletedFlag"].ToString(), httpRequest.Params["DeletedDate"].ToString(), httpRequest.Params["TypeOfUser"].ToString(), new Guid(httpRequest.Params["Admin_Under"].ToString()), httpRequest.Params["Project_Under"].ToString(), httpRequest.Params["Profile_Pic"].ToString());
                dbsync.InsertOrUpdate_UserProjects(new Guid(httpRequest.Params["AssignID"].ToString()), new Guid(httpRequest.Params["UserUID"].ToString()), new Guid(httpRequest.Params["ProjectUID"].ToString()), new Guid(httpRequest.Params["UserRole"].ToString()), httpRequest.Params["AssignDate"].ToString(), httpRequest.Params["Delete_Flag"].ToString());
                return Json(new
                {
                    Success = true

                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = "Error" + ex.Message
                });
            }
        }

        [HttpPost] // for dbsync User WorkPackages
        public IHttpActionResult UserWorkPackagesSync()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;

                // dbsync.InsertOrUpdate_UserDetails(new Guid(httpRequest.Params["UserUID"].ToString()), httpRequest.Params["FirstName"].ToString(), httpRequest.Params["LastName"].ToString(), httpRequest.Params["EmailID"].ToString(), httpRequest.Params["Phonenumber"].ToString(), httpRequest.Params["Mobilenumber"].ToString(), httpRequest.Params["Address1"].ToString(), httpRequest.Params["Address2"].ToString(), httpRequest.Params["Username"].ToString(), httpRequest.Params["password"].ToString(), httpRequest.Params["CreatedDate"].ToString(), httpRequest.Params["DeletedFlag"].ToString(), httpRequest.Params["DeletedDate"].ToString(), httpRequest.Params["TypeOfUser"].ToString(), new Guid(httpRequest.Params["Admin_Under"].ToString()), httpRequest.Params["Project_Under"].ToString(), httpRequest.Params["Profile_Pic"].ToString());
                dbsync.InsertorUpdate_UserWorkPackages(new Guid(httpRequest.Params["UID"].ToString()), new Guid(httpRequest.Params["ProjectUID"].ToString()), new Guid(httpRequest.Params["UserUID"].ToString()), new Guid(httpRequest.Params["WorkPackageUID"].ToString()), httpRequest.Params["Status"].ToString(), httpRequest.Params["CreatedDate"].ToString(), httpRequest.Params["UpdatedDate"].ToString(), httpRequest.Params["Activity_Type"].ToString(), httpRequest.Params["Activity_Id"].ToString(), httpRequest.Params["UserRole_ID"].ToString());
                return Json(new
                {
                    Success = true

                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = "Error" + ex.Message
                });
            }
        }

        [HttpPost] // for dbsync UserRoles Master
        public IHttpActionResult UserRolesMasterSync()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;

                // dbsync.InsertOrUpdate_UserDetails(new Guid(httpRequest.Params["UserUID"].ToString()), httpRequest.Params["FirstName"].ToString(), httpRequest.Params["LastName"].ToString(), httpRequest.Params["EmailID"].ToString(), httpRequest.Params["Phonenumber"].ToString(), httpRequest.Params["Mobilenumber"].ToString(), httpRequest.Params["Address1"].ToString(), httpRequest.Params["Address2"].ToString(), httpRequest.Params["Username"].ToString(), httpRequest.Params["password"].ToString(), httpRequest.Params["CreatedDate"].ToString(), httpRequest.Params["DeletedFlag"].ToString(), httpRequest.Params["DeletedDate"].ToString(), httpRequest.Params["TypeOfUser"].ToString(), new Guid(httpRequest.Params["Admin_Under"].ToString()), httpRequest.Params["Project_Under"].ToString(), httpRequest.Params["Profile_Pic"].ToString());
                dbsync.InsertorUpdate_UserRolesMaster(new Guid(httpRequest.Params["UserRole_ID"].ToString()), httpRequest.Params["UserRole_Desc"].ToString(), httpRequest.Params["UserRole_Name"].ToString());
                return Json(new
                {
                    Success = true

                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = "Error" + ex.Message
                });
            }
        }

        [HttpPost] // for dbsync UserType_Functionality_Master
        public IHttpActionResult UserType_Functionality_MasterSync()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;

                // dbsync.InsertOrUpdate_UserDetails(new Guid(httpRequest.Params["UserUID"].ToString()), httpRequest.Params["FirstName"].ToString(), httpRequest.Params["LastName"].ToString(), httpRequest.Params["EmailID"].ToString(), httpRequest.Params["Phonenumber"].ToString(), httpRequest.Params["Mobilenumber"].ToString(), httpRequest.Params["Address1"].ToString(), httpRequest.Params["Address2"].ToString(), httpRequest.Params["Username"].ToString(), httpRequest.Params["password"].ToString(), httpRequest.Params["CreatedDate"].ToString(), httpRequest.Params["DeletedFlag"].ToString(), httpRequest.Params["DeletedDate"].ToString(), httpRequest.Params["TypeOfUser"].ToString(), new Guid(httpRequest.Params["Admin_Under"].ToString()), httpRequest.Params["Project_Under"].ToString(), httpRequest.Params["Profile_Pic"].ToString());
                dbsync.InsertorUpdate_UserType_Functionality_Master(new Guid(httpRequest.Params["UID"].ToString()), httpRequest.Params["Functionality"].ToString(), httpRequest.Params["Code"].ToString());
                return Json(new
                {
                    Success = true

                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = "Error" + ex.Message
                });
            }
        }

        [HttpPost] // for dbsync UserType_Functionality_Mapping
        public IHttpActionResult UserType_Functionality_MappingSync()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                dbsync.InsertorUpdate_UserType_Functionality_Mapping(new Guid(httpRequest.Params["UID"].ToString()), httpRequest.Params["UserType"].ToString(), new Guid(httpRequest.Params["FunctionalityUID"].ToString()));
                return Json(new
                {
                    Success = true

                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = "Error" + ex.Message
                });
            }
        }

        [HttpPost] // for dbsync Issues
        public IHttpActionResult IssuesSync()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                //dbsync.InsertorUpdate_UserType_Functionality_Mapping(new Guid(httpRequest.Params["UID"].ToString()), httpRequest.Params["UserType"].ToString(), new Guid(httpRequest.Params["FunctionalityUID"].ToString()));
                dbsync.InsertorUpdate_Issues(new Guid(httpRequest.Params["Issue_Uid"].ToString()), new Guid(httpRequest.Params["ProjectUID"].ToString()), new Guid(httpRequest.Params["WorkPackagesUID"].ToString()), new Guid(httpRequest.Params["TaskUID"].ToString()), httpRequest.Params["Issue_Description"].ToString(), httpRequest.Params["Issue_Date"].ToString(), httpRequest.Params["Issued_User"].ToString(), httpRequest.Params["Assigned_User"].ToString(), httpRequest.Params["Assigned_Date"].ToString(), httpRequest.Params["Issue_ProposedCloser_Date"].ToString(), httpRequest.Params["Approving_User"].ToString(), httpRequest.Params["Actual_Closer_Date"].ToString(), httpRequest.Params["Issue_Status"].ToString(), httpRequest.Params["Issue_Remarks"].ToString(), httpRequest.Params["Issue_Document"].ToString(), httpRequest.Params["Delete_Flag"].ToString());
                return Json(new
                {
                    Success = true

                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = "Error" + ex.Message
                });
            }
        }

        [HttpPost] // for dbsync IssueRemarks
        public IHttpActionResult IssueRemarksSync()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                // dbsync.InsertorUpdate_UserType_Functionality_Mapping(new Guid(httpRequest.Params["UID"].ToString()), httpRequest.Params["UserType"].ToString(), new Guid(httpRequest.Params["FunctionalityUID"].ToString()));
                dbsync.InsertorUpdate_IssueRemarks(new Guid(httpRequest.Params["IssueRemarksUID"].ToString()), new Guid(httpRequest.Params["Issue_Uid"].ToString()), httpRequest.Params["Issue_Status"].ToString(), httpRequest.Params["Issue_Remarks"].ToString(), httpRequest.Params["Issue_Document"].ToString(), httpRequest.Params["IssueRemark_Date"].ToString(), httpRequest.Params["Delete_Flag"].ToString());
                return Json(new
                {
                    Success = true

                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = "Error" + ex.Message
                });
            }
        }

        [HttpPost] // for dbsync IssueRemarksDocs
        public IHttpActionResult IssueRemarksDocs()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                // dbsync.InsertorUpdate_UserType_Functionality_Mapping(new Guid(httpRequest.Params["UID"].ToString()), httpRequest.Params["UserType"].ToString(), new Guid(httpRequest.Params["FunctionalityUID"].ToString()));
                dbsync.InsertOrUpdate_IssueDocs(int.Parse(httpRequest.Params["uploaded_doc_id"].ToString()), httpRequest.Params["doc_name"].ToString(), httpRequest.Params["doc_path"].ToString(), httpRequest.Params["issue_remarks_uid"].ToString());
                return Json(new
                {
                    Success = true

                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = "Error" + ex.Message
                });
            }
        }

        [HttpPost] // for dbsync ActivityDeleteLogs
        public IHttpActionResult ActivityDeleteLogsSync()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                // dbsync.InsertorUpdate_UserType_Functionality_Mapping(new Guid(httpRequest.Params["UID"].ToString()), httpRequest.Params["UserType"].ToString(), new Guid(httpRequest.Params["FunctionalityUID"].ToString()));
                dbsync.InsertorUpdate_ActivityDeleteLogs(new Guid(httpRequest.Params["DeleteLog_UID"].ToString()), new Guid(httpRequest.Params["Activity_UID"].ToString()), new Guid(httpRequest.Params["Activity_UserUID"].ToString()), httpRequest.Params["Activity_For"].ToString(), httpRequest.Params["Activity_Date"].ToString());
                return Json(new
                {
                    Success = true

                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = "Error" + ex.Message
                });
            }
        }

        [HttpPost] // for dbsync FinanceMileStones
        public IHttpActionResult FinanceMileStonesSync()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                // dbsync.InsertorUpdate_UserType_Functionality_Mapping(new Guid(httpRequest.Params["UID"].ToString()), httpRequest.Params["UserType"].ToString(), new Guid(httpRequest.Params["FunctionalityUID"].ToString()));
                dbsync.InsertorUpdate_FinanceMileStones(new Guid(httpRequest.Params["Finance_MileStoneUID"].ToString()), new Guid(httpRequest.Params["TaskUID"].ToString()), httpRequest.Params["Finance_MileStoneName"].ToString(), httpRequest.Params["Finance_MileStoneCreatedDate"].ToString(), httpRequest.Params["User_Created"].ToString(), httpRequest.Params["Delete_Flag"].ToString(), httpRequest.Params["IsMonth"].ToString());
                return Json(new
                {
                    Success = true

                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = "Error" + ex.Message
                });
            }
        }

        [HttpPost] // for dbsync FinanceMileStoneMonth
        public IHttpActionResult FinanceMileStoneMonthSync()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                // dbsync.InsertorUpdate_UserType_Functionality_Mapping(new Guid(httpRequest.Params["UID"].ToString()), httpRequest.Params["UserType"].ToString(), new Guid(httpRequest.Params["FunctionalityUID"].ToString()));
                dbsync.InsertorUpdate_FinanceMileStoneMonth(new Guid(httpRequest.Params["FinMileStoneMonthUID"].ToString()), new Guid(httpRequest.Params["Finance_MileStoneUID"].ToString()), decimal.Parse(httpRequest.Params["AllowedPayment"].ToString()), httpRequest.Params["UserCreated"].ToString(), httpRequest.Params["CreatedDate"].ToString(), httpRequest.Params["DeletedFlag"].ToString(), httpRequest.Params["Month"].ToString(), Int16.Parse(httpRequest.Params["Year"].ToString()), Int64.Parse(httpRequest.Params["MultiplyingFactor"].ToString()), new Guid(httpRequest.Params["WorkPackageUID"].ToString()), int.Parse(httpRequest.Params["OrderBy"].ToString()), httpRequest.Params["DeletedBy"].ToString(), httpRequest.Params["DeletedDate"].ToString());
                return Json(new
                {
                    Success = true

                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = "Error" + ex.Message
                });
            }
        }

        [HttpPost] // for dbsync FinanceMileStoneMonth_EditedValues
        public IHttpActionResult FinanceMileStoneMonth_EditedValuesSync()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                // dbsync.InsertorUpdate_UserType_Functionality_Mapping(new Guid(httpRequest.Params["UID"].ToString()), httpRequest.Params["UserType"].ToString(), new Guid(httpRequest.Params["FunctionalityUID"].ToString()));
                dbsync.InsertorUpdate_FinanceMileStoneMonth_EditedValues(new Guid(httpRequest.Params["UID"].ToString()), new Guid(httpRequest.Params["FinMileStoneMonthUID"].ToString()),decimal.Parse(httpRequest.Params["OldPaymentValue"].ToString()),decimal.Parse(httpRequest.Params["NewPaymentValue"].ToString()), httpRequest.Params["CreatedDate"].ToString(),new Guid(httpRequest.Params["EditedBy"].ToString()));
                return Json(new
                {
                    Success = true

                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = "Error" + ex.Message
                });
            }
        }

        [HttpPost] // for dbsync InsertorUpdate_TaskSchedule
        public IHttpActionResult TaskScheduleSync()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                dbsync.InsertorUpdate_TaskSchedule(new Guid(httpRequest.Params["TaskScheduleUID"].ToString()), new Guid(httpRequest.Params["WorkpacageUID"].ToString()), new Guid(httpRequest.Params["TaskUID"].ToString()), httpRequest.Params["StartDate"].ToString(), httpRequest.Params["EndDate"].ToString(), decimal.Parse(httpRequest.Params["Schedule_Value"].ToString()), httpRequest.Params["Schedule_Type"].ToString(), httpRequest.Params["Created_Date"].ToString(), float.Parse(httpRequest.Params["TaskScheduleVersion"].ToString()), httpRequest.Params["TaskSchedule_Approved"].ToString(), httpRequest.Params["Delete_Flag"].ToString(), httpRequest.Params["Achieved_Value"].ToString(), httpRequest.Params["Achieved_Date"].ToString(), httpRequest.Params["Schedule_Per"].ToString(), httpRequest.Params["Achieved_Per"].ToString());
                return Json(new
                {
                    Success = true

                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = "Error" + ex.Message
                });
            }
        }

        [HttpPost] // for dbsync InsertorUpdate_TaskScheduleVersion
        public IHttpActionResult TaskScheduleVersionSync()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                dbsync.InsertorUpdate_TaskScheduleVersion(new Guid(httpRequest.Params["TaskScheduleVersion_UID"].ToString()), new Guid(httpRequest.Params["TaskUID"].ToString()), float.Parse(httpRequest.Params["TaskScheduleVersion"].ToString()), httpRequest.Params["TaskScheduleType"].ToString(), httpRequest.Params["TaskSchedule_Approved"].ToString(), httpRequest.Params["Delete_Flag"].ToString());
                return Json(new
                {
                    Success = true

                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = "Error" + ex.Message
                });
            }
        }

        [HttpPost] // for dbsync InsertorUpdate_TaskScheduleVersion
        public IHttpActionResult TaskSync()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                //dbsync.InsertorUpdate_TaskScheduleVersion(new Guid(httpRequest.Params["TaskScheduleVersion_UID"].ToString()), new Guid(httpRequest.Params["TaskUID"].ToString()), float.Parse(httpRequest.Params["TaskScheduleVersion"].ToString()), httpRequest.Params["TaskScheduleType"].ToString(), httpRequest.Params["TaskSchedule_Approved"].ToString(), httpRequest.Params["Delete_Flag"].ToString());
                dbsync.InsertorUpdate_Task(new Guid(httpRequest.Params["TaskUID"].ToString()), 
                    new Guid(httpRequest.Params["WorkPackageUID"].ToString()), 
                    new Guid(httpRequest.Params["ProjectUID"].ToString()), 
                    new Guid(httpRequest.Params["Workpackage_Option"].ToString()), 
                    httpRequest.Params["Owner"].ToString(), 
                    httpRequest.Params["Task_Section"].ToString(), 
                    httpRequest.Params["Name"].ToString(), 
                    httpRequest.Params["Description"].ToString(), 
                    httpRequest.Params["RFPReference"].ToString(), 
                    httpRequest.Params["POReference"].ToString(), 
                    httpRequest.Params["StartDate"].ToString(), 
                    httpRequest.Params["PlannedEndDate"].ToString(), httpRequest.Params["ProjectedEndDate"].ToString(), 
                    httpRequest.Params["Status"].ToString(), httpRequest.Params["Currency"].ToString(), 
                    httpRequest.Params["Currency_CultureInfo"].ToString(),float.Parse(httpRequest.Params["Basic_Budget"].ToString()),
                    float.Parse(httpRequest.Params["GST"].ToString()),
                    float.Parse(httpRequest.Params["Total_Budget"].ToString()),
                    float.Parse(httpRequest.Params["ActualExpenditure"].ToString()), httpRequest.Params["RFPDocument"].ToString(),
                    httpRequest.Params["NoOfDocuments"].ToString(), int.Parse(httpRequest.Params["TaskLevel"].ToString()),
                    httpRequest.Params["ParentTaskID"].ToString(),
                    httpRequest.Params["UpdatedDate"].ToString(), float.Parse(httpRequest.Params["StatusPer"].ToString()),
                    httpRequest.Params["UnitforProgress"].ToString(), httpRequest.Params["UnitQuantity"].ToString(),
                    httpRequest.Params["PlannedStartDate"].ToString(), httpRequest.Params["ProjectedStartDate"].ToString(),
                    httpRequest.Params["ActualEndDate"].ToString(), httpRequest.Params["Discipline"].ToString(),
                    httpRequest.Params["MileStone"].ToString(), httpRequest.Params["Task_Weightage"].ToString(), 
                    httpRequest.Params["Task_Type"].ToString(), httpRequest.Params["Delete_Flag"].ToString(),
                    httpRequest.Params["Task_Order"].ToString(), httpRequest.Params["BOQDetailsUID"].ToString(),
                    httpRequest.Params["GroupBOQItems"].ToString()
                    ,httpRequest.Params["Task_CulumativePercentage"].ToString()
                    ,httpRequest.Params["CumulativeAchvQuantity"].ToString()
                    , httpRequest.Params["InGraph"].ToString()
                    , httpRequest.Params["Report1"].ToString()
                    , httpRequest.Params["Report2"].ToString()
                    , httpRequest.Params["Report3"].ToString()
                    , httpRequest.Params["Report4"].ToString()
                    , httpRequest.Params["Report5"].ToString()
                   );
                return Json(new
                {
                    Success = true

                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = "Error" + ex.Message
                });
            }
        }

        [HttpPost] // for dbsync DailyProgressReportMasterSync
        public IHttpActionResult DailyProgressReportMasterSync()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                dbsync.InsertOrUpdateDailyProgressReportMaster(new Guid(httpRequest.Params["DPR_UID"].ToString()), httpRequest.Params["Description"].ToString(), httpRequest.Params["CreatedDate"].ToString());
                return Json(new
                {
                    Success = true

                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = "Error" + ex.Message
                });
            }
        }

        [HttpPost] // for dbsync DailyProgressSync
        public IHttpActionResult DailyProgressSync()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                //dbsync.InsertorUpdate_TaskScheduleVersion(new Guid(httpRequest.Params["TaskScheduleVersion_UID"].ToString()), new Guid(httpRequest.Params["TaskUID"].ToString()), float.Parse(httpRequest.Params["TaskScheduleVersion"].ToString()), httpRequest.Params["TaskScheduleType"].ToString(), httpRequest.Params["TaskSchedule_Approved"].ToString(), httpRequest.Params["Delete_Flag"].ToString());
                dbsync.InsertOrUpdateDailyProgress(new Guid(httpRequest.Params["UID"].ToString()),
                    new Guid(httpRequest.Params["DPR_UID"].ToString()),
                    new Guid(httpRequest.Params["ProjectUID"].ToString()),
                    new Guid(httpRequest.Params["WorkPackageUID"].ToString()),
                    httpRequest.Params["VillageName"].ToString(),
                    httpRequest.Params["PipeDia"].ToString(),
                   decimal.Parse(httpRequest.Params["Quantity"].ToString()),
                   decimal.Parse(httpRequest.Params["RevisedQuantity"].ToString()),
                    decimal.Parse(httpRequest.Params["PipesReceived"].ToString()),
                    decimal.Parse(httpRequest.Params["PreviousQuantity"].ToString()),
                    decimal.Parse(httpRequest.Params["TodaysQuantity"].ToString()),
                   decimal.Parse(httpRequest.Params["TotalQuantity"].ToString()), decimal.Parse(httpRequest.Params["Balance"].ToString()),
                     httpRequest.Params["Remarks"].ToString(),
                    httpRequest.Params["CreatedDate"].ToString(), httpRequest.Params["ZoneName"].ToString(),
                    httpRequest.Params["DeletedFlag"].ToString()
                   );
                return Json(new
                {
                    Success = true

                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = "Error" + ex.Message
                });
            }
        }


    }
}
