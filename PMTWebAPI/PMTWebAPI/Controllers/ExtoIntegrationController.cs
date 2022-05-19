using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Http;
using PMTWebAPI.DAL;

namespace PMTWebAPI.Controllers
{
    public class ExtoIntegrationController : ApiController
    {
        DBGetData db = new DBGetData();
        DBSyncData dbsync = new DBSyncData();
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
        [HttpGet]
        [Route("api/ExtoIntegration/authenticate")]
        public IHttpActionResult GetForAuthenticate()
        {
            var identity = (ClaimsIdentity)User.Identity;
            if (db.CheckGetWebApiSettings(identity.Name, GetIp()) > 0)
            {
                return Ok("Hello " + identity.Name);
            }
            else
            {
                return Json(new
                {
                    Success = false,
                    Message = "Not Authorized IP address"
                });
            }

        }


        [Authorize]
        [HttpPost] // for getting Submittal Document flows
        [Route("api/ExtoIntegration/GetIpAddress")]
        public IHttpActionResult GetClientIpAddress()
        {
            try
            {
                var identity = (ClaimsIdentity)User.Identity;
                var httpRequest = HttpContext.Current.Request;
                var UserName = httpRequest.Params["UserName"];
                var Password = Security.Encrypt(httpRequest.Params["Password"]);
                if (db.CheckGetWebApiSettings(identity.Name, GetIp()) > 0)
                {
                    DataSet ds = new DataSet();
                    ds = db.GetDocumentFlows();
                    return Json(new
                    {
                        Success = true,
                        Ipaddress = GetIp()

                    });
                }
                else
                {
                    return Json(new
                    {
                        Success = false,
                        Message = "Not Authorized IP address"
                    });
                }
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

        [Authorize]
        [HttpPost] // this is for adding submittals
        [Route("api/ExtoIntegration/AddSubmittals")]
        public IHttpActionResult AddSubmittals()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                var identity = (ClaimsIdentity)User.Identity;
                string TaskUID = string.Empty;
                string SubCategory = string.Empty;
               var FlowUID = string.Empty;
                var SubmitterUID = string.Empty;
                var ReviewerUID = string.Empty;
                var ApproverUID = string.Empty;
                var tmpData = string.Empty;
                //var transactionUid = Guid.NewGuid();
                //var BaseURL = HttpContext.Current.Request.Url.ToString();
                //for (int p=0;p<httpRequest.Params.Count;p++)
                //{
                //    tmpData += httpRequest.Params[p];
                //}
                //db.WebAPITransctionInsert(transactionUid, BaseURL, tmpData, "");
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
                    if (httpRequest.Params["SubmittalName"] == null)
                    {
                        var resultmain = new
                        {
                            Status = "Failure",
                            Message = "Please Enter Submittal Name !",
                        };
                        return Json(resultmain);
                    }

                    if (httpRequest.Params["SubmittalName"] != null)
                    {
                        if (string.IsNullOrWhiteSpace((httpRequest.Params["SubmittalName"].ToString())))
                        {
                            var resultmain = new
                            {
                                Status = "Failure",
                                Message = "Submittal Name cannot be Empty !",
                            };
                            return Json(resultmain);
                        }
                    }

                    if (httpRequest.Params["FlowUID"] == null)
                    {
                        FlowUID = db.CheckSubmittalFlowExists(Guid.NewGuid(), httpRequest.Params["FlowName"], "Name");
                        if (String.IsNullOrEmpty(FlowUID))
                        {
                            var resultmain = new
                            {
                                Status = "Failure",
                                Message = "Flow Name does not Exists !",
                            };
                            return Json(resultmain);
                        }
                    }
                    else
                    {
                        //check the flowUID in db
                        
                        if (String.IsNullOrEmpty(db.CheckSubmittalFlowExists(new Guid(httpRequest.Params["FlowUID"]), "", "UID")))
                        {
                            var resultmain = new
                            {
                                Status = "Failure",
                                Message = "FlowUID does not Exists !",
                            };
                            return Json(resultmain);
                        }
                    }
                    if (httpRequest.Params["Submitter"] == null)
                    {
                        var resultmain = new
                        {
                            Status = "Failure",
                            Message = "Please Enter Submitter !",
                        };
                        return Json(resultmain);
                    }
                    else
                    {
                        if (String.IsNullOrEmpty(db.CheckUserExists(Guid.NewGuid(), httpRequest.Params["Submitter"], "Name")))
                        {
                            var resultmain = new
                            {
                                Status = "Failure",
                                Message = "Submitter does not Exists !",
                            };
                            return Json(resultmain);
                        }
                        else
                        {
                            SubmitterUID = db.CheckUserExists(Guid.NewGuid(), httpRequest.Params["Submitter"], "Name");
                        }
                    }
                    if (httpRequest.Params["SubmitterTargetDate"] == null)
                    {
                        var resultmain = new
                        {
                            Status = "Failure",
                            Message = "Please Enter SubmitterTargetDate !",
                        };
                        return Json(resultmain);
                    }


                    if (httpRequest.Params["taskUID"] == null) //if null then only go for tasls,tasks level,prjname,wrkpkgname checking
                    {
                        var TaskParam = httpRequest.Params["Tasks"];
                        var tLevel = Convert.ToInt32(httpRequest.Params["TLevel"]);
                        var ProjectName = httpRequest.Params["ProjectName"];
                        var WorkpackageName = httpRequest.Params["WorkpackageName"];

                        var pExists = db.ProjectNameExists(ProjectName);
                        if (pExists != "")
                        {
                            string[] TaskList = TaskParam.Split('$');
                            //var objects = JArray.Parse(TaskParam); // parse as array 
                            //var result = JsonConvert.DeserializeObject<RootObject>(TaskParam);

                            var wExists = db.WorkpackageNameExists(WorkpackageName, pExists);
                            if (wExists != "")
                            {

                                if (tLevel > 0)
                                {
                                  
                                    string ParentTaskUID = "";
                                    for (int i = 0; i < tLevel; i++)
                                    {
                                        if (i == 0)
                                        {
                                            TaskUID = db.TaskNameExists(new Guid(wExists), TaskList[i], "Workpackage");
                                            ParentTaskUID = TaskUID;
                                        }
                                        else
                                        {
                                            TaskUID = db.TaskNameExists(new Guid(ParentTaskUID), TaskList[i], "Task");
                                            ParentTaskUID = TaskUID;
                                        }

                                        if (TaskUID == null || TaskUID == "")
                                        {
                                            var resultmain = new
                                            {
                                                Status = "Failure",
                                                Message = "Invalid Task Name !",
                                            };
                                            return Json(resultmain);

                                        }
                                    }
                                    if (TaskUID != "")
                                    {

                                    }
                                    else
                                    {
                                        var resultmain = new
                                        {
                                            Status = "Failure",
                                            Message = "Invalid Task Name !",
                                        };
                                        return Json(resultmain);

                                    }
                                }
                                else
                                {
                                    var resultmain = new
                                    {
                                        Status = "Failure",
                                        Message = "Invalid Task Level !",
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
                    else
                    {
                        TaskUID = httpRequest.Params["taskUID"];
                    }



                    var taskUID = TaskUID;
                    var SubmittalName = httpRequest.Params["SubmittalName"];
                    var SubmittalCategory = SubCategory;
                    
                    var sDocumentUID = Guid.NewGuid();
                   
                    var SubmitterTargetDate = httpRequest.Params["SubmitterTargetDate"];
                    
                    var RevieweTargetDate = httpRequest.Params["ReviewerTargetDate"];
                   
                    var ApproverTargetDate = httpRequest.Params["ApproverTargetDate"];
                    var EstimatedDocuments = httpRequest.Params["EstimatedDocuments"];
                    var Remarks = httpRequest.Params["Remarks"];
                    var DocumentSearchType = httpRequest.Params["SubmittalDocType"];
                    Guid projectId = Guid.Empty;
                    Guid workpackageid = Guid.Empty;
                    string sDate1 = "", sDate2 = "", sDate3 = "", sDate4 = "", sDate5 = "", DocStartString = "";
                    DateTime CDate1 = DateTime.Now, CDate2 = DateTime.Now, CDate3 = DateTime.Now, CDate4 = DateTime.Now, CDate5 = DateTime.Now, DocStartDate = DateTime.Now;
                    DataTable dttasks = db.GetTaskDetails_TaskUID(taskUID);
                    if (dttasks.Rows.Count > 0)
                    {
                        projectId = new Guid(dttasks.Rows[0]["ProjectUID"].ToString());
                        workpackageid = new Guid(dttasks.Rows[0]["workPackageUID"].ToString());

                        if (httpRequest.Params["SubmittalCategory"] == null)
                        {
                            var resultmain = new
                            {
                                Status = "Failure",
                                Message = "Please Enter Submittal Category !",
                            };
                            return Json(resultmain);
                        }
                        else
                        {
                            //check 
                            SubCategory = db.CheckSubmittalCategoryExists(Guid.NewGuid(), httpRequest.Params["SubmittalCategory"], "Name", workpackageid);
                            SubmittalCategory = SubCategory;
                            if (String.IsNullOrEmpty(SubCategory))
                            {
                                var resultmain = new
                                {
                                    Status = "Failure",
                                    Message = "SubmittalCategory does not Exists !",
                                };
                                return Json(resultmain);
                            }
                        }
                    }
                    else
                    {
                        var resultmain = new
                        {
                            Status = "Failure",
                            Message = "TaskUID does not Exists !",
                        };
                        return Json(resultmain);
                    }
                    DataSet dsFlow = db.GetDocumentFlows_by_UID(new Guid(FlowUID));
                    int result = 0;
                    if (dsFlow.Tables[0].Rows.Count > 0)
                    {
                        DocStartString = DateTime.Now.ToString("dd/MM/yyyy");
                        DocStartString = DocStartString.Split('/')[1] + "/" + DocStartString.Split('/')[0] + "/" + DocStartString.Split('/')[2];
                        DocStartDate = Convert.ToDateTime(DocStartString);


                        //
                        //sDate1 = dtSubTargetDate.Text;
                        //sDate1 = sDate1.Split('/')[1] + "/" + sDate1.Split('/')[0] + "/" + sDate1.Split('/')[2];
                        //CDate1 = Convert.ToDateTime(sDate1);
                        ////

                        //sDate2 = dtQualTargetDate.Text;
                        //sDate2 = sDate2.Split('/')[1] + "/" + sDate2.Split('/')[0] + "/" + sDate2.Split('/')[2];
                        //CDate2 = Convert.ToDateTime(sDate2);

                        //sDate3 = dtRev_B_TargetDate.Text;
                        //sDate3 = sDate3.Split('/')[1] + "/" + sDate3.Split('/')[0] + "/" + sDate3.Split('/')[2];
                        //CDate3 = Convert.ToDateTime(sDate3);

                        //result = getdata.DoumentMaster_Insert_or_Update_Flow_2(sDocumentUID, workpackageid, projectId, TaskUID, txtDocumentName.Text, new Guid(DDLDocumentCategory.SelectedValue),
                        //"", "Submittle Document", 0.0, new Guid(DDLDocumentFlow.SelectedValue), DocStartDate, new Guid(ddlSubmissionUSer.SelectedValue), CDate1,
                        //new Guid(ddlQualityEngg.SelectedValue), CDate2, new Guid(ddlReviewer_B.SelectedValue), CDate3);

                        if (dsFlow.Tables[0].Rows[0]["Steps_Count"].ToString() == "1")
                        {
                            //


                            sDate1 = SubmitterTargetDate;
                            sDate1 = sDate1.Split('/')[1] + "/" + sDate1.Split('/')[0] + "/" + sDate1.Split('/')[2];
                            CDate1 = Convert.ToDateTime(sDate1);
                            result = db.DoumentMaster_Insert_or_Update_Flow_0(sDocumentUID, workpackageid, projectId, new Guid(taskUID), SubmittalName, new Guid(SubmittalCategory),
                            "", "Submittle Document", 0.0, new Guid(FlowUID), DocStartDate, new Guid(SubmitterUID), CDate1, Convert.ToInt16(EstimatedDocuments), Remarks, DocumentSearchType);

                        }
                        else if (dsFlow.Tables[0].Rows[0]["Steps_Count"].ToString() == "2")
                        {
                            //

                            if (httpRequest.Params["Approver"] == null)
                            {
                                var resultmain = new
                                {
                                    Status = "Failure",
                                    Message = "Please Enter Approver !",
                                };
                                return Json(resultmain);
                            }
                            else
                            {
                                //if (String.IsNullOrEmpty(db.CheckUserExists(Guid.NewGuid(), httpRequest.Params["Approver"], "Name")))
                                //{
                                //    var resultmain = new
                                //    {
                                //        Status = "Failure",
                                //        Message = "Approver does not Exists !",
                                //    };
                                //    return Json(resultmain);
                                //}
                                //else
                                //{
                                DataTable dtApiSubmittalUsers= db.getapi_submittal_users(httpRequest.Params["ProjectName"]);
                                if(dtApiSubmittalUsers.Rows.Count==0 )
                                {
                                    var resultmain = new
                                    {
                                        Status = "Failure",
                                        Message = "Apporver is not exists",
                                    };
                                    return Json(resultmain);
                                }
                                string approverName = dtApiSubmittalUsers.Rows[0]["approver"].ToString();
                                    ApproverUID = db.CheckUserExists(Guid.NewGuid(), approverName, "Name");
                                //}
                            }
                            if (httpRequest.Params["ApproverTargetDate"] == null)
                            {
                                var resultmain = new
                                {
                                    Status = "Failure",
                                    Message = "Please Enter Approver TargetDate !",
                                };
                                return Json(resultmain);
                            }
                            sDate1 = SubmitterTargetDate;
                            sDate1 = sDate1.Split('/')[1] + "/" + sDate1.Split('/')[0] + "/" + sDate1.Split('/')[2];
                            CDate1 = Convert.ToDateTime(sDate1);
                            //
                            sDate2 = ApproverTargetDate;
                            sDate2 = sDate2.Split('/')[1] + "/" + sDate2.Split('/')[0] + "/" + sDate2.Split('/')[2];
                            CDate2 = Convert.ToDateTime(sDate2);

                            CDate2 = Convert.ToDateTime(sDate2);
                            result = db.DoumentMaster_Insert_or_Update_Flow_1(sDocumentUID, workpackageid, projectId, new Guid(taskUID), SubmittalName, new Guid(SubmittalCategory),
                            "", "Submittle Document", 0.0, new Guid(FlowUID), DocStartDate, new Guid(SubmitterUID), CDate1,
                            new Guid(ApproverUID), CDate2, Convert.ToInt16(EstimatedDocuments), Remarks, DocumentSearchType);
                        }
                        else if (dsFlow.Tables[0].Rows[0]["Steps_Count"].ToString() == "3")
                        {
                            //
                            if (httpRequest.Params["Reviewer"] == null)
                            {
                                var resultmain = new
                                {
                                    Status = "Failure",
                                    Message = "Please Enter Reviewer !",
                                };
                                return Json(resultmain);
                            }
                            else
                            {
                                //if (String.IsNullOrEmpty(db.CheckUserExists(Guid.NewGuid(), httpRequest.Params["Reviewer"], "Name")))
                                //{
                                //    var resultmain = new
                                //    {
                                //        Status = "Failure",
                                //        Message = "Reviewer does not Exists !",
                                //    };
                                //    return Json(resultmain);
                                //}
                                //else
                                //{
                                DataTable dtApiSubmittalUsers = db.getapi_submittal_users(httpRequest.Params["ProjectName"]);
                                if (dtApiSubmittalUsers.Rows.Count == 0)
                                {
                                    var resultmain = new
                                    {
                                        Status = "Failure",
                                        Message = "Reviewer is not exists",
                                    };
                                    return Json(resultmain);
                                }
                                string reviewerName = dtApiSubmittalUsers.Rows[0]["reviewer"].ToString();
                                ReviewerUID = db.CheckUserExists(Guid.NewGuid(), reviewerName, "Name");
                                //}
                            }
                            if (httpRequest.Params["ReviewerTargetDate"] == null)
                            {
                                var resultmain = new
                                {
                                    Status = "Failure",
                                    Message = "Please Enter Reviewer TargetDate !",
                                };
                                return Json(resultmain);
                            }
                            if (httpRequest.Params["Approver"] == null)
                            {
                                var resultmain = new
                                {
                                    Status = "Failure",
                                    Message = "Please Enter Approver !",
                                };
                                return Json(resultmain);
                            }
                            else
                            {
                                //if (String.IsNullOrEmpty(db.CheckUserExists(Guid.NewGuid(), httpRequest.Params["Approver"], "Name")))
                                //{
                                //    var resultmain = new
                                //    {
                                //        Status = "Failure",
                                //        Message = "Approver does not Exists !",
                                //    };
                                //    return Json(resultmain);
                                //}
                                //else
                                //{

                                //    ApproverUID = db.CheckUserExists(Guid.NewGuid(), httpRequest.Params["Approver"], "Name");
                                DataTable dtApiSubmittalUsers = db.getapi_submittal_users(httpRequest.Params["ProjectName"]);
                                if (dtApiSubmittalUsers.Rows.Count == 0)
                                {
                                    var resultmain = new
                                    {
                                        Status = "Failure",
                                        Message = "Apporver is not exists",
                                    };
                                    return Json(resultmain);
                                }
                                string approverName = dtApiSubmittalUsers.Rows[0]["approver"].ToString();
                                ApproverUID = db.CheckUserExists(Guid.NewGuid(), approverName, "Name");

                                //}
                            }
                            if (httpRequest.Params["ApproverTargetDate"] == null)
                            {
                                var resultmain = new
                                {
                                    Status = "Failure",
                                    Message = "Please Enter Approver TargetDate !",
                                };
                                return Json(resultmain);
                            }
                            //
                            sDate1 = SubmitterTargetDate;
                            sDate1 = sDate1.Split('/')[1] + "/" + sDate1.Split('/')[0] + "/" + sDate1.Split('/')[2];
                            CDate1 = Convert.ToDateTime(sDate1);
                            //

                            sDate2 = RevieweTargetDate;
                            sDate2 = sDate2.Split('/')[1] + "/" + sDate2.Split('/')[0] + "/" + sDate2.Split('/')[2];
                            CDate2 = Convert.ToDateTime(sDate2);

                            sDate3 = ApproverTargetDate;
                            sDate3 = sDate3.Split('/')[1] + "/" + sDate3.Split('/')[0] + "/" + sDate3.Split('/')[2];
                            CDate3 = Convert.ToDateTime(sDate3);

                            result = db.DoumentMaster_Insert_or_Update_Flow_2(sDocumentUID, workpackageid, projectId, new Guid(taskUID), SubmittalName, new Guid(SubmittalCategory),
                            "", "Submittle Document", 0.0, new Guid(FlowUID), DocStartDate, new Guid(SubmitterUID), CDate1,
                            new Guid(ReviewerUID), CDate2, new Guid(ApproverUID), CDate3, Convert.ToInt16(EstimatedDocuments), Remarks, DocumentSearchType);
                        }

                    }

                    if(result == -1)
                    {
                        var result_a = new
                        {
                            Status = "Failure",
                            Message = "Submittal already Exists.Try using diferent name !",
                        };
                        return Json(result_a);
                    }

                    var result_1 = new
                    {
                        Status = "Success",
                        Message = "Submittal Added Sucessfully !",
                        SubmittalUID = sDocumentUID
                    };
                    return Json(result_1);
                
                }
            }
            catch (Exception ex)
            {
                var result = new
                {
                    Status = "Failure",
                    Message = ex.Message,
                };
                return Json(result);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("api/ExtoIntegration/AddDocument")]
        public IHttpActionResult AddDocument()
        {
            bool sError = false;
            string ErrorText = "";
            string CoverUID = string.Empty;
            string DocUID = string.Empty;
            try
            {
                var httpRequest = HttpContext.Current.Request;
                var identity = (ClaimsIdentity)User.Identity;
                if (db.CheckGetWebApiSettings(identity.Name, GetIp()) > 0)
                {
                    var ProjectName = httpRequest.Params["ProjectName"];
                    var WorkpackageName = httpRequest.Params["WorkpackageName"];
                    var SubmittalUID = httpRequest.Params["SubmittalUID"];
                    var ReferenceNumber = httpRequest.Params["ReferenceNumber"];
                    var Description = httpRequest.Params["Description"];
                    var DocumentType = httpRequest.Params["DocumentType"];
                    var DocMedia_HardCopy = httpRequest.Params["DocMedia_HardCopy"] != string.Empty ? Convert.ToBoolean(httpRequest.Params["DocMedia_HardCopy"]) : false;
                    var DocMedia_SoftCopy_PDF = httpRequest.Params["DocMedia_SoftCopy_PDF"] != string.Empty ? Convert.ToBoolean(httpRequest.Params["DocMedia_SoftCopy_PDF"]) : false;
                    var DocMedia_SoftCopy_Editable = httpRequest.Params["DocMedia_SoftCopy_Editable"] != string.Empty ? Convert.ToBoolean(httpRequest.Params["DocMedia_SoftCopy_Editable"]) : false;
                    var DocMedia_SoftCopy_Ref = httpRequest.Params["DocMedia_SoftCopy_Ref"] != string.Empty ? Convert.ToBoolean(httpRequest.Params["DocMedia_SoftCopy_Ref"]) : false;
                    var DocMedia_HardCopy_Ref = httpRequest.Params["DocMedia_HardCopy_Ref"] != string.Empty ? Convert.ToBoolean(httpRequest.Params["DocMedia_HardCopy_Ref"]) : false;
                    var DocMedia_NoMedia = httpRequest.Params["DocMedia_NoMedia"] != string.Empty ? Convert.ToBoolean(httpRequest.Params["DocMedia_NoMedia"]) : false;
                    var Originator = httpRequest.Params["Originator"];
                    var OriginatorRefNo = httpRequest.Params["OriginatorRefNo"];
                    var IncomingReciveDate = httpRequest.Params["IncomingReciveDate"];
                    //var CoverLetterDate = httpRequest.Params["CoverLetterDate"];
                    var DocumentDate = httpRequest.Params["DocumentDate"];
                    var FileReferenceNumber = httpRequest.Params["FileReferenceNumber"];
                    var Remarks = httpRequest.Params["Remarks"];

                    bool CoverLetterUpload = false, DocumentsUpload = false;
                    var wExists = string.Empty;
                    var pExists = db.ProjectNameExists(ProjectName);
                    string ActualDocumentUID = string.Empty;

                    if (!string.IsNullOrEmpty(pExists))
                    {
                        wExists = db.WorkpackageNameExists(WorkpackageName, pExists);
                        if (!string.IsNullOrEmpty(wExists))
                        {
                            if (Guid.TryParse(SubmittalUID, out Guid submittaluid))
                            {
                                if (!string.IsNullOrEmpty(Description))
                                {
                                    if (!string.IsNullOrEmpty(DocumentType))
                                    {
                                        string[] formats = { "dd/MM/yyyy" };
                                        if (DateTime.TryParseExact(DocumentDate, formats, System.Globalization.CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime documentDate))
                                        {
                                            if (DocumentType == "Cover Letter")
                                            {
                                                if (string.IsNullOrEmpty(Originator))
                                                {
                                                    sError = true;
                                                    ErrorText = "Originator cannot be empty.";
                                                }
                                                if (string.IsNullOrEmpty(OriginatorRefNo))
                                                {
                                                    sError = true;
                                                    ErrorText = "Originator Reference number cannot be empty.";
                                                }
                                                if (!DateTime.TryParseExact(IncomingReciveDate, formats, System.Globalization.CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime incomingReceiveDate))
                                                {
                                                    sError = true;
                                                    ErrorText = "Invalid Incoming Receive Date.";
                                                }
                                                if (httpRequest.Files.Count == 0)
                                                {
                                                    sError = true;
                                                    ErrorText = "Please upload a document.";
                                                }
                                                if (documentDate > incomingReceiveDate)
                                                {
                                                    sError = true;
                                                    ErrorText = "Document Date connot be greater than Incoming Receive date.";
                                                }
                                                for (int i = 0; i < httpRequest.Files.Count; i++)
                                                {
                                                    HttpPostedFile httpPostedFile = httpRequest.Files[i];

                                                    if (httpPostedFile != null)
                                                    {
                                                        if (httpPostedFile.FileName.StartsWith("Cover") || httpPostedFile.FileName.StartsWith("Cover_Letter") || httpPostedFile.FileName.StartsWith("Cover Letter"))
                                                        {
                                                            CoverLetterUpload = true;
                                                        }
                                                        else
                                                        {
                                                            DocumentsUpload = true;
                                                        }
                                                    }
                                                }
                                                if (!CoverLetterUpload)
                                                {
                                                    sError = true;
                                                    ErrorText = "Please upload a cover letter file.";
                                                }
                                                if (!DocumentsUpload)
                                                {
                                                    sError = true;
                                                    ErrorText = "Please upload document file.";
                                                }
                                            }
                                            else
                                            {
                                                if (httpRequest.Files.Count == 0)
                                                {
                                                    sError = true;
                                                    ErrorText = "Please upload a document.";
                                                }
                                            }
                                        }
                                        else
                                        {
                                            sError = true;
                                            ErrorText = "Invalid Document Date.";
                                        }

                                    }
                                    else
                                    {
                                        sError = true;
                                        ErrorText = "Document Type cannot be empty.";
                                    }
                                }
                                else
                                {
                                    sError = true;
                                    ErrorText = "Description cannot be empty.";
                                }

                            }
                            else
                            {
                                sError = true;
                                ErrorText = "Invalid Submittal UID.";
                            }
                        }
                        else
                        {
                            sError = true;
                            ErrorText = "Workpackage name not found";
                        }
                    }
                    else
                    {
                        sError = true;
                        ErrorText = "Project name not found.";
                    }

                    if (!sError)
                    {
                        DataSet ds = db.getDocumentsbyDocID(new Guid(SubmittalUID));
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            string cStatus = "Submitted", FileDatetime = DateTime.Now.ToString("dd MMM yyyy hh-mm-ss tt"), sDocumentPath = string.Empty,
                                DocumentFor = string.Empty, IncomingRec_Date_String = string.Empty, CoverPagePath = string.Empty, CoverLetterUID = ""; ;

                            if (!string.IsNullOrEmpty(IncomingReciveDate))
                            {
                                IncomingRec_Date_String = IncomingReciveDate;
                            }
                            else
                            {
                                IncomingRec_Date_String = DocumentDate;
                            }
                            IncomingRec_Date_String = db.ConvertDateFormat(IncomingRec_Date_String);
                            DateTime IncomingRec_Date = Convert.ToDateTime(IncomingRec_Date_String);

                            if (string.IsNullOrEmpty(DocumentDate))
                            {
                                DocumentDate = DateTime.MinValue.ToString("dd/MM/yyyy");
                            }
                            DocumentDate = db.ConvertDateFormat(DocumentDate);
                            DateTime Document_Date = Convert.ToDateTime(DocumentDate);

                            //Cover Letter Insert
                            for (int i = 0; i < httpRequest.Files.Count; i++)
                            {
                                HttpPostedFile httpPostedFile = httpRequest.Files[i];
                                if (httpPostedFile != null)
                                {
                                    string Extn = System.IO.Path.GetExtension(httpPostedFile.FileName);
                                    if (Extn.ToLower() != ".exe" && Extn.ToLower() != ".msi" && Extn.ToLower() != ".db")
                                    {
                                        if ((httpPostedFile.FileName.StartsWith("Cover") || httpPostedFile.FileName.StartsWith("Cover_Letter")) && DocumentType == "Cover Letter")
                                        {
                                            DocumentFor = "Cover Letter";
                                            sDocumentPath = ConfigurationManager.AppSettings["DocumentsPath"] + pExists + "//" + wExists + "//CoverLetter";

                                            if (!Directory.Exists(sDocumentPath))
                                            {
                                                Directory.CreateDirectory(sDocumentPath);
                                            }
                                            string sFileName = Path.GetFileNameWithoutExtension(httpPostedFile.FileName);
                                            httpPostedFile.SaveAs(sDocumentPath + "/" + sFileName + "_1_copy" + Extn);
                                            string savedPath = sDocumentPath + "/" + sFileName + "_1_copy" + Extn;
                                            CoverPagePath = sDocumentPath + "/" + sFileName + "_1" + Extn;
                                            EncryptFile(savedPath, CoverPagePath);

                                            //

                                            savedPath = "~/" + pExists + "/" + wExists + "/CoverLetter" + "/" + sFileName + "_1_copy" + Extn;
                                            CoverPagePath = "~/" + pExists + "/" + wExists + "/CoverLetter" + "/" + sFileName + "_1" + Extn;
                                            //

                                            CoverLetterUID = Guid.NewGuid().ToString();

                                            int RetCount = db.DocumentCoverLetter_Insert_or_Update(new Guid(CoverLetterUID), new Guid(pExists), new Guid(wExists), new Guid(SubmittalUID), ReferenceNumber, "",
                                            DocumentFor, IncomingRec_Date, new Guid(ds.Tables[0].Rows[0]["FlowUID"].ToString()), sFileName, Description, 1, Extn, DocMedia_HardCopy ? "true" : "false",
                                            DocMedia_SoftCopy_PDF == true ? "true" : "false", DocMedia_SoftCopy_Editable == true ? "true" : "false", DocMedia_SoftCopy_Ref == true ? "true" : "false",
                                            DocMedia_HardCopy_Ref == true ? "true" : "false", DocMedia_NoMedia == true ? "true" : "false", CoverPagePath, Remarks ?? "", FileReferenceNumber ?? "", cStatus, Originator ?? "", Document_Date);
                                            if (RetCount <= 0)
                                            {
                                                sError = true;
                                                ErrorText = "Error occured while inserting cover letter. Please contact system admin.";
                                            }
                                            CoverUID = CoverLetterUID;
                                            try
                                            {
                                                if (File.Exists(savedPath))
                                                {
                                                    File.Delete(savedPath);
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                //throw
                                            }
                                        }
                                    }
                                }
                            }
                            DocUID = string.Empty;
                            //ActualDocument Insert
                            for (int i = 0; i < httpRequest.Files.Count; i++)
                            {
                                HttpPostedFile httpPostedFile = httpRequest.Files[i];
                                if (httpPostedFile != null)
                                {
                                    string Extn = System.IO.Path.GetExtension(httpPostedFile.FileName);
                                    if (Extn.ToLower() != ".exe" && Extn.ToLower() != ".msi" && Extn.ToLower() != ".db")
                                    {                                                       //Added code by Venkat on 13 May 2022
                                        if (!httpPostedFile.FileName.StartsWith("Cover") && httpPostedFile.FileName != "")
                                        {

                                            string sDate1 = "", sDate2 = "", sDate3 = "", sDate4 = "", sDate5 = "";
                                            DateTime CDate1 = DateTime.Now, CDate2 = DateTime.Now, CDate3 = DateTime.Now, CDate4 = DateTime.Now, CDate5 = DateTime.Now;
                                            if (!checkDocumentExists(Path.GetFileNameWithoutExtension(httpPostedFile.FileName), SubmittalUID))
                                            {
                                                if (DocumentType == "General Document")
                                                {
                                                    DocumentFor = "General Document";
                                                }
                                                else if (DocumentType == "Photographs")
                                                {
                                                    DocumentFor = "Photographs";
                                                }
                                                else
                                                {
                                                    DocumentFor = "Document";
                                                }

                                                sDocumentPath = ConfigurationManager.AppSettings["DocumentsPath"] + pExists + "//" + wExists + "//Documents";

                                                if (!Directory.Exists(sDocumentPath))
                                                {
                                                    Directory.CreateDirectory(sDocumentPath);
                                                }

                                                string sFileName = Path.GetFileNameWithoutExtension(httpPostedFile.FileName);

                                                string savedPath = string.Empty;
                                                if (DocumentType != "Photographs")
                                                {
                                                    savedPath = sDocumentPath + "/" + sFileName + "_1_copy" + Extn;
                                                    httpPostedFile.SaveAs(savedPath);

                                                    CoverPagePath = sDocumentPath + "/" + sFileName + "_1" + Extn;
                                                    EncryptFile(savedPath, CoverPagePath);
                                                }
                                                else
                                                {
                                                    savedPath = sDocumentPath + "/" + DateTime.Now.Ticks.ToString() + Path.GetFileName(httpPostedFile.FileName);
                                                    httpPostedFile.SaveAs(savedPath);

                                                }
                                                ActualDocumentUID = Guid.NewGuid().ToString();
                                                string UploadFilePhysicalpath = CoverPagePath;
                                                //
                                                if (DocumentType != "Photographs")
                                                {
                                                    savedPath = "~/" + pExists + "/" + wExists + "/Documents" + "/" + sFileName + "_1_copy" + Extn;
                                                    CoverPagePath = "~/" + pExists + "/" + wExists + "/Documents" + "/" + sFileName + "_1" + Extn;
                                                }
                                                //
                                                string Flow1DisplayName = "", Flow2DisplayName = "", Flow3DisplayName = "", Flow4DisplayName = "", Flow5DisplayName = "";
                                                DataSet dsFlow = db.GetDocumentFlows_by_UID(new Guid(ds.Tables[0].Rows[0]["FlowUID"].ToString()));
                                                if (dsFlow.Tables[0].Rows.Count > 0)
                                                {
                                                    if (dsFlow.Tables[0].Rows[0]["Steps_Count"].ToString() == "-1")
                                                    {
                                                        Flow1DisplayName = dsFlow.Tables[0].Rows[0]["FlowStep1_DisplayName"].ToString();

                                                        sDate1 = Convert.ToDateTime(ds.Tables[0].Rows[0]["FlowStep1_TargetDate"].ToString()).ToString("dd/MM/yyyy");
                                                        sDate1 = db.ConvertDateFormat(sDate1);
                                                        CDate1 = Convert.ToDateTime(sDate1);

                                                        int cnt = db.Document_Insert_or_Update_with_RelativePath_Flow1(new Guid(ActualDocumentUID), new Guid(pExists), new Guid(wExists), new Guid(SubmittalUID), ReferenceNumber, OriginatorRefNo ?? "",
                                                        DocumentFor, IncomingRec_Date, new Guid(ds.Tables[0].Rows[0]["FlowUID"].ToString()), sFileName, Description, 1, Extn,
                                                        DocMedia_HardCopy ? "true" : "false", DocMedia_SoftCopy_PDF == true ? "true" : "false", DocMedia_SoftCopy_Editable == true ? "true" : "false", DocMedia_SoftCopy_Ref == true ? "true" : "false",
                                                        DocMedia_HardCopy_Ref == true ? "true" : "false", DocMedia_NoMedia == true ? "true" : "false", savedPath, Remarks ?? "", FileReferenceNumber ?? "", cStatus,
                                                        new Guid(ds.Tables[0].Rows[0]["FlowStep1_UserUID"].ToString()), CDate1, Flow1DisplayName, Originator ?? "", Document_Date, "", "", UploadFilePhysicalpath, CoverLetterUID, "Submission");
                                                        if (cnt <= 0)
                                                        {
                                                            sError = true;
                                                            ErrorText = "Error occured while inserting document. Please contact system admin.";
                                                        }
                                                    }
                                                    else if (dsFlow.Tables[0].Rows[0]["Steps_Count"].ToString() == "1")
                                                    {
                                                        Flow1DisplayName = dsFlow.Tables[0].Rows[0]["FlowStep1_DisplayName"].ToString();

                                                        sDate1 = Convert.ToDateTime(ds.Tables[0].Rows[0]["FlowStep1_TargetDate"].ToString()).ToString("dd/MM/yyyy");
                                                        sDate1 = db.ConvertDateFormat(sDate1);
                                                        CDate1 = Convert.ToDateTime(sDate1);

                                                        int cnt = db.Document_Insert_or_Update_with_RelativePath_Flow1(new Guid(ActualDocumentUID), new Guid(pExists), new Guid(wExists), new Guid(SubmittalUID), ReferenceNumber, OriginatorRefNo ?? "",
                                                        DocumentFor, IncomingRec_Date, new Guid(ds.Tables[0].Rows[0]["FlowUID"].ToString()), sFileName, Description, 1, Extn,
                                                        DocMedia_HardCopy ? "true" : "false", DocMedia_SoftCopy_PDF == true ? "true" : "false", DocMedia_SoftCopy_Editable == true ? "true" : "false", DocMedia_SoftCopy_Ref == true ? "true" : "false",
                                                        DocMedia_HardCopy_Ref == true ? "true" : "false", DocMedia_NoMedia == true ? "true" : "false", CoverPagePath, Remarks ?? "", FileReferenceNumber ?? "", cStatus,
                                                        new Guid(ds.Tables[0].Rows[0]["FlowStep1_UserUID"].ToString()), CDate1, Flow1DisplayName, Originator ?? "", Document_Date, "", "", UploadFilePhysicalpath, CoverLetterUID, "Submission");
                                                        if (cnt <= 0)
                                                        {
                                                            sError = true;
                                                            ErrorText = "Error occured while inserting document. Please contact system admin.";
                                                        }
                                                    }
                                                    else if (dsFlow.Tables[0].Rows[0]["Steps_Count"].ToString() == "2")
                                                    {
                                                        Flow1DisplayName = dsFlow.Tables[0].Rows[0]["FlowStep1_DisplayName"].ToString();
                                                        Flow2DisplayName = dsFlow.Tables[0].Rows[0]["FlowStep2_DisplayName"].ToString();

                                                        sDate1 = Convert.ToDateTime(ds.Tables[0].Rows[0]["FlowStep1_TargetDate"].ToString()).ToString("dd/MM/yyyy");
                                                        sDate1 = db.ConvertDateFormat(sDate1);
                                                        CDate1 = Convert.ToDateTime(sDate1);

                                                        sDate2 = Convert.ToDateTime(ds.Tables[0].Rows[0]["FlowStep2_TargetDate"].ToString()).ToString("dd/MM/yyyy");
                                                        sDate2 = db.ConvertDateFormat(sDate2);
                                                        CDate2 = Convert.ToDateTime(sDate2);

                                                        int cnt = db.Document_Insert_or_Update_with_RelativePath_Flow2(new Guid(ActualDocumentUID), new Guid(pExists), new Guid(wExists), new Guid(SubmittalUID), ReferenceNumber, OriginatorRefNo,
                                                          DocumentFor, IncomingRec_Date, new Guid(ds.Tables[0].Rows[0]["FlowUID"].ToString()), sFileName, Description, 1, Extn,
                                                          DocMedia_HardCopy ? "true" : "false", DocMedia_SoftCopy_PDF == true ? "true" : "false", DocMedia_SoftCopy_Editable == true ? "true" : "false", DocMedia_SoftCopy_Ref == true ? "true" : "false",
                                                          DocMedia_HardCopy_Ref == true ? "true" : "false", DocMedia_NoMedia == true ? "true" : "false", CoverPagePath, Remarks ?? "", FileReferenceNumber ?? "", cStatus,
                                                          new Guid(ds.Tables[0].Rows[0]["FlowStep1_UserUID"].ToString()), CDate1, new Guid(ds.Tables[0].Rows[0]["FlowStep2_UserUID"].ToString()), CDate2, Flow1DisplayName, Flow2DisplayName, Originator, Document_Date, "", "", UploadFilePhysicalpath, CoverLetterUID, "Submission");
                                                        if (cnt <= 0)
                                                        {
                                                            sError = true;
                                                            ErrorText = "Error occured while inserting document. Please contact system admin.";
                                                        }

                                                    }
                                                    else if (dsFlow.Tables[0].Rows[0]["Steps_Count"].ToString() == "3")
                                                    {
                                                        Flow1DisplayName = dsFlow.Tables[0].Rows[0]["FlowStep1_DisplayName"].ToString();
                                                        Flow2DisplayName = dsFlow.Tables[0].Rows[0]["FlowStep2_DisplayName"].ToString();
                                                        Flow3DisplayName = dsFlow.Tables[0].Rows[0]["FlowStep3_DisplayName"].ToString();

                                                        sDate1 = Convert.ToDateTime(ds.Tables[0].Rows[0]["FlowStep1_TargetDate"].ToString()).ToString("dd/MM/yyyy");
                                                        sDate1 = db.ConvertDateFormat(sDate1);
                                                        CDate1 = Convert.ToDateTime(sDate1);


                                                        sDate2 = Convert.ToDateTime(ds.Tables[0].Rows[0]["FlowStep2_TargetDate"].ToString()).ToString("dd/MM/yyyy");
                                                        sDate2 = db.ConvertDateFormat(sDate2);
                                                        CDate2 = Convert.ToDateTime(sDate2);

                                                        sDate3 = Convert.ToDateTime(ds.Tables[0].Rows[0]["FlowStep3_TargetDate"].ToString()).ToString("dd/MM/yyyy");
                                                        sDate3 = db.ConvertDateFormat(sDate3);
                                                        CDate3 = Convert.ToDateTime(sDate3);

                                                        sDate4 = DateTime.Now.ToString("dd/MM/yyyy");
                                                        sDate4 = db.ConvertDateFormat(sDate4);
                                                        CDate4 = Convert.ToDateTime(CDate4);

                                                        int cnt = db.Document_Insert_or_Update(new Guid(ActualDocumentUID), new Guid(pExists), new Guid(wExists), new Guid(SubmittalUID), ReferenceNumber, OriginatorRefNo,
                                                       DocumentFor, IncomingRec_Date, new Guid(ds.Tables[0].Rows[0]["FlowUID"].ToString()), sFileName, Description, 1, Extn,
                                                       DocMedia_HardCopy ? "true" : "false", DocMedia_SoftCopy_PDF == true ? "true" : "false", DocMedia_SoftCopy_Editable == true ? "true" : "false", DocMedia_SoftCopy_Ref == true ? "true" : "false",
                                                       DocMedia_HardCopy_Ref == true ? "true" : "false", DocMedia_NoMedia == true ? "true" : "false", CoverPagePath, Remarks ?? "", FileReferenceNumber ?? "", cStatus,
                                                       new Guid(ds.Tables[0].Rows[0]["FlowStep1_UserUID"].ToString()), CDate1, new Guid(ds.Tables[0].Rows[0]["FlowStep2_UserUID"].ToString()), CDate2, new Guid(ds.Tables[0].Rows[0]["FlowStep3_UserUID"].ToString()), CDate3,
                                                       Flow1DisplayName, Flow2DisplayName, Flow3DisplayName, Originator, Document_Date, UploadFilePhysicalpath, CoverLetterUID, "Submission");
                                                        if (cnt <= 0)
                                                        {
                                                            sError = true;
                                                            ErrorText = "Error occured while inserting document. Please contact system admin.";
                                                        }

                                                    }
                                                    else if (dsFlow.Tables[0].Rows[0]["Steps_Count"].ToString() == "4")
                                                    {
                                                        Flow1DisplayName = dsFlow.Tables[0].Rows[0]["FlowStep1_DisplayName"].ToString();
                                                        Flow2DisplayName = dsFlow.Tables[0].Rows[0]["FlowStep2_DisplayName"].ToString();
                                                        Flow3DisplayName = dsFlow.Tables[0].Rows[0]["FlowStep3_DisplayName"].ToString();
                                                        Flow4DisplayName = dsFlow.Tables[0].Rows[0]["FlowStep4_DisplayName"].ToString();

                                                        sDate1 = Convert.ToDateTime(ds.Tables[0].Rows[0]["FlowStep1_TargetDate"].ToString()).ToString("dd/MM/yyyy");
                                                        sDate1 = db.ConvertDateFormat(sDate1);
                                                        CDate1 = Convert.ToDateTime(sDate1);


                                                        sDate2 = Convert.ToDateTime(ds.Tables[0].Rows[0]["FlowStep2_TargetDate"].ToString()).ToString("dd/MM/yyyy");
                                                        sDate2 = db.ConvertDateFormat(sDate2);
                                                        CDate2 = Convert.ToDateTime(sDate2);

                                                        sDate3 = Convert.ToDateTime(ds.Tables[0].Rows[0]["FlowStep3_TargetDate"].ToString()).ToString("dd/MM/yyyy");
                                                        sDate3 = db.ConvertDateFormat(sDate3);
                                                        CDate3 = Convert.ToDateTime(sDate3);


                                                        sDate4 = Convert.ToDateTime(ds.Tables[0].Rows[0]["FlowStep4_TargetDate"].ToString()).ToString("dd/MM/yyyy");
                                                        sDate4 = db.ConvertDateFormat(sDate4);
                                                        CDate4 = Convert.ToDateTime(sDate4);

                                                        int cnt = db.Document_Insert_or_Update_with_RelativePath_Flow4(new Guid(ActualDocumentUID), new Guid(pExists), new Guid(wExists), new Guid(SubmittalUID), ReferenceNumber, OriginatorRefNo,
                                                          DocumentFor, IncomingRec_Date, new Guid(ds.Tables[0].Rows[0]["FlowUID"].ToString()), sFileName, Description, 1, Extn,
                                                          DocMedia_HardCopy ? "true" : "false", DocMedia_SoftCopy_PDF == true ? "true" : "false", DocMedia_SoftCopy_Editable == true ? "true" : "false", DocMedia_SoftCopy_Ref == true ? "true" : "false",
                                                          DocMedia_HardCopy_Ref == true ? "true" : "false", DocMedia_NoMedia == true ? "true" : "false", CoverPagePath, Remarks ?? "", FileReferenceNumber ?? "", cStatus,
                                                          new Guid(ds.Tables[0].Rows[0]["FlowStep1_UserUID"].ToString()), CDate1, new Guid(ds.Tables[0].Rows[0]["FlowStep2_UserUID"].ToString()), CDate2, new Guid(ds.Tables[0].Rows[0]["FlowStep3_UserUID"].ToString()), CDate3, new Guid(ds.Tables[0].Rows[0]["FlowStep4_UserUID"].ToString()), CDate4,
                                                          Flow1DisplayName, Flow2DisplayName, Flow3DisplayName, Flow4DisplayName, Originator, Document_Date, "", "", UploadFilePhysicalpath, CoverLetterUID, "Submission");
                                                        if (cnt <= 0)
                                                        {
                                                            sError = true;
                                                            ErrorText = "Error occured while inserting document. Please contact system admin.";
                                                        }

                                                    }
                                                    else
                                                    {
                                                        Flow1DisplayName = dsFlow.Tables[0].Rows[0]["FlowStep1_DisplayName"].ToString();
                                                        Flow2DisplayName = dsFlow.Tables[0].Rows[0]["FlowStep2_DisplayName"].ToString();
                                                        Flow3DisplayName = dsFlow.Tables[0].Rows[0]["FlowStep3_DisplayName"].ToString();
                                                        Flow4DisplayName = dsFlow.Tables[0].Rows[0]["FlowStep4_DisplayName"].ToString();
                                                        Flow5DisplayName = dsFlow.Tables[0].Rows[0]["FlowStep5_DisplayName"].ToString();

                                                        sDate1 = Convert.ToDateTime(ds.Tables[0].Rows[0]["FlowStep1_TargetDate"].ToString()).ToString("dd/MM/yyyy");
                                                        sDate1 = db.ConvertDateFormat(sDate1);
                                                        CDate1 = Convert.ToDateTime(sDate1);

                                                        sDate2 = Convert.ToDateTime(ds.Tables[0].Rows[0]["FlowStep2_TargetDate"].ToString()).ToString("dd/MM/yyyy");
                                                        sDate2 = db.ConvertDateFormat(sDate2);
                                                        CDate2 = Convert.ToDateTime(sDate2);

                                                        sDate3 = Convert.ToDateTime(ds.Tables[0].Rows[0]["FlowStep3_TargetDate"].ToString()).ToString("dd/MM/yyyy");
                                                        sDate3 = db.ConvertDateFormat(sDate3);
                                                        CDate3 = Convert.ToDateTime(sDate3);


                                                        sDate4 = Convert.ToDateTime(ds.Tables[0].Rows[0]["FlowStep4_TargetDate"].ToString()).ToString("dd/MM/yyyy");
                                                        sDate4 = db.ConvertDateFormat(sDate4);
                                                        CDate4 = Convert.ToDateTime(sDate4);

                                                        sDate5 = Convert.ToDateTime(ds.Tables[0].Rows[0]["FlowStep4_TargetDate"].ToString()).ToString("dd/MM/yyyy");
                                                        sDate5 = db.ConvertDateFormat(sDate5);
                                                        CDate5 = Convert.ToDateTime(sDate5);

                                                        int cnt = db.Document_Insert_or_Update_with_RelativePath_Flow5(new Guid(ActualDocumentUID), new Guid(pExists), new Guid(wExists), new Guid(SubmittalUID), ReferenceNumber, OriginatorRefNo,
                                                          DocumentFor, IncomingRec_Date, new Guid(ds.Tables[0].Rows[0]["FlowUID"].ToString()), sFileName, Description, 1, Extn,
                                                          DocMedia_HardCopy ? "true" : "false", DocMedia_SoftCopy_PDF == true ? "true" : "false", DocMedia_SoftCopy_Editable == true ? "true" : "false", DocMedia_SoftCopy_Ref == true ? "true" : "false",
                                                          DocMedia_HardCopy_Ref == true ? "true" : "false", DocMedia_NoMedia == true ? "true" : "false", CoverPagePath, Remarks ?? "", FileReferenceNumber ?? "", cStatus,
                                                          new Guid(ds.Tables[0].Rows[0]["FlowStep1_UserUID"].ToString()), CDate1, new Guid(ds.Tables[0].Rows[0]["FlowStep2_UserUID"].ToString()), CDate2, new Guid(ds.Tables[0].Rows[0]["FlowStep3_UserUID"].ToString()), CDate3, new Guid(ds.Tables[0].Rows[0]["FlowStep4_UserUID"].ToString()), CDate4, new Guid(ds.Tables[0].Rows[0]["FlowStep5_UserUID"].ToString()), CDate5,
                                                          Flow1DisplayName, Flow2DisplayName, Flow3DisplayName, Flow4DisplayName, Flow5DisplayName, Originator, Document_Date, "", "", UploadFilePhysicalpath, CoverLetterUID, "Submission");
                                                        if (cnt <= 0)
                                                        {
                                                            sError = true;
                                                            ErrorText = "Error occured while inserting document. Please contact system admin.";
                                                        }
                                                    }

                                                    DocUID += ActualDocumentUID + ",";
                                                }

                                                try
                                                {
                                                    if (DocumentType != "Photographs")
                                                    {
                                                        if (File.Exists(savedPath))
                                                        {
                                                            File.Delete(savedPath);
                                                        }
                                                    }

                                                }
                                                catch (Exception ex)
                                                {
                                                    //throw
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    sError = true;
                    ErrorText = "Not Authorized IP address.";
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
            else
            {
                //Added code by Venkat on 13 May 2022
                if(DocUID == "")
                {
                    return Json(new
                    {
                        Status = "Failure",
                        Message = "No Document was Attached."
                    });
                }
               else if (!string.IsNullOrEmpty(CoverUID))
                {
                    return Json(new
                    {
                        Status = "Success",
                        CoverLetterUID = CoverUID,
                        ActualDocumentUID = DocUID.TrimEnd(','),
                        Message = "Documents Uploaded Successfully."
                    });
                }
                else
                {
                    return Json(new
                    {
                        Status = "Success",
                        ActualDocumentUID = DocUID.TrimEnd(','),
                        Message = "Documents Uploaded Successfully."
                    });
                }

            }
        }

        [Authorize]
        [HttpPost]
        [Route("api/ExtoIntegration/ReSubmitDocument")]
        public IHttpActionResult ReSubmitDocument()
        {
            bool sError = false;
            string ErrorText = "";
            string CoverUID = string.Empty;
            string DocumentUid = string.Empty;
            try
            {
                var httpRequest = HttpContext.Current.Request;
                var identity = (ClaimsIdentity)User.Identity;
                if (db.CheckGetWebApiSettings(identity.Name, GetIp()) > 0)
                {
                    var ProjectName = httpRequest.Params["ProjectName"];
                    var WorkpackageName = httpRequest.Params["WorkpackageName"];
                    var SubmittalUID = httpRequest.Params["SubmittalUID"];
                     DocumentUid = httpRequest.Params["DocumentUid"];
                   // var CoverLetterDate = httpRequest.Params["CoverLetterDate"];
                    var CoverLetterFile = httpRequest.Params["CoverLetterFile"];
                    var DocumentFile = httpRequest.Params["DocumentFile"];
                    var Comments = httpRequest.Params["Comments"];
                
                    bool CoverLetterUpload = false, DocumentsUpload = false;
                    var wExists = string.Empty;
                    var pExists = db.ProjectNameExists(ProjectName);
                    string ActualDocumentUID = string.Empty;

                    if (!string.IsNullOrEmpty(pExists))
                    {
                        wExists = db.WorkpackageNameExists(WorkpackageName, pExists);
                        if (!string.IsNullOrEmpty(wExists))
                        {
                            if (Guid.TryParse(SubmittalUID, out Guid submittaluid))
                            {
                              
                                if (httpRequest.Files.Count == 0)
                                {
                                    sError = true;
                                    ErrorText = "Please upload a document.";
                                }
                                                
                                for (int i = 0; i < httpRequest.Files.Count; i++)
                                {
                                    HttpPostedFile httpPostedFile = httpRequest.Files[i];

                                    if (httpPostedFile.FileName != null && httpPostedFile.FileName != string.Empty)
                                    {
                                        if (httpPostedFile.FileName.StartsWith("Cover") || httpPostedFile.FileName.StartsWith("Cover_Letter") || httpPostedFile.FileName.StartsWith("Cover Letter"))
                                        {
                                            CoverLetterUpload = true;
                                        }
                                        else
                                        {
                                            DocumentsUpload = true;
                                        }
                                    }
                                    else
                                    {
                                            sError = true;
                                            ErrorText = "Please upload a document.";
                                        
                                    }
                                }
                                //if (!CoverLetterUpload)
                                //{
                                //    sError = true;
                                //    ErrorText = "Please upload a cover letter file.";
                                //}
                                if (!DocumentsUpload)
                                {
                                    sError = true;
                                    ErrorText = "Please upload document file.";
                                }   
                            }
                            else
                            {
                                sError = true;
                                ErrorText = "Invalid Submittal UID.";
                            }
                        }
                        else
                        {
                            sError = true;
                            ErrorText = "Workpackage name not found";
                        }
                    }
                    else
                    {
                        sError = true;
                        ErrorText = "Project name not found.";
                    }
                    DataTable  dtWorkpackages=  db.GetWorkPackages_ProjectName(ProjectName);
                    if(dtWorkpackages.Rows.Count==0)
                    {
                        sError = true;
                        ErrorText = "Workpackage not available for this project";
                    }
                    if(dtWorkpackages.Rows[0]["Name"].ToString() != WorkpackageName)
                    {
                        sError = true;
                        ErrorText = "Workpackage not matching with project";
                    }

                    DataTable dsDocuments=  db.getActualDocumentStatus(new Guid(dtWorkpackages.Rows[0]["ProjectUid"].ToString()), new Guid(dtWorkpackages.Rows[0]["WorkpackageUid"].ToString()), new Guid(DocumentUid));
                    if(dsDocuments.Rows.Count==0)
                    {
                        sError = true;
                        ErrorText = "Invalid documentuid";
                    }
                    DataSet dsdocs = db.getDocumentsbyDocID(new Guid(SubmittalUID));
                    if (dsdocs.Tables[0].Rows.Count ==0)
                    {
                        sError = true;
                        ErrorText = "Invalid Submitaluid";
                    }

                    string Extn = "";
                    if (!sError)
                    {
                        DataSet ds = db.getDocumentsbyDocID(new Guid(SubmittalUID));
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            string  sDocumentPath = string.Empty,
                               CoverPagePath = string.Empty , documentPagePath=string.Empty;


                            //Cover Letter Insert
                            for (int i = 0; i < httpRequest.Files.Count; i++)
                            {
                                HttpPostedFile httpPostedFile = httpRequest.Files[i];
                                if (httpPostedFile != null)
                                {
                                    Extn = System.IO.Path.GetExtension(httpPostedFile.FileName);
                                    if (Extn.ToLower() != ".exe" && Extn.ToLower() != ".msi" && Extn.ToLower() != ".db")
                                    {
                                        if ((httpPostedFile.FileName.StartsWith("Cover") || httpPostedFile.FileName.StartsWith("Cover_Letter")))
                                        {
                                            //DocumentFor = "Cover Letter";
                                            sDocumentPath = ConfigurationManager.AppSettings["DocumentsPath"] + pExists + "//" + wExists + "//CoverLetter";

                                            if (!Directory.Exists(sDocumentPath))
                                            {
                                                Directory.CreateDirectory(sDocumentPath);
                                            }
                                            string sFileName = Path.GetFileNameWithoutExtension(httpPostedFile.FileName);
                                            httpPostedFile.SaveAs(sDocumentPath + "/" + sFileName + "_1_copy" + Extn);
                                            string savedPath = sDocumentPath + "/" + sFileName + "_1_copy" + Extn;
                                            CoverPagePath = sDocumentPath + "/" + sFileName + "_1" + Extn;
                                            EncryptFile(sDocumentPath + "/" + sFileName + "_1_copy" + Extn, sDocumentPath + "/" + sFileName + "_1" + Extn);
                                            savedPath = "~/" + pExists + "/" + wExists + "/CoverLetter" + "/" + sFileName + "_1_copy" + Extn;
                                            CoverPagePath = "~/" + pExists + "/" + wExists + "/CoverLetter" + "/" + sFileName + "_1" + Extn;
                                            // db.EncryptFile(sDocumentPath + "/" + sFileName + "_1_copy" + Extn, sDocumentPath + "/" + sFileName + "_1" + Extn);

                                        }

                                        if (!httpPostedFile.FileName.StartsWith("Cover"))
                                        {
                                            sDocumentPath = ConfigurationManager.AppSettings["DocumentsPath"] + pExists + "//" + wExists + "//Documents";

                                            if (!Directory.Exists(sDocumentPath))
                                            {
                                                Directory.CreateDirectory(sDocumentPath);
                                            }

                                            string sFileName = Path.GetFileNameWithoutExtension(httpPostedFile.FileName);

                                            string savedPath = string.Empty;

                                            savedPath = sDocumentPath + "/" + sFileName + "_1_copy" + Extn;
                                            httpPostedFile.SaveAs(savedPath);

                                            documentPagePath = sDocumentPath + "/" + sFileName + "_1" + Extn;
                                            EncryptFile(sDocumentPath + "/" + sFileName + "_1_copy" + Extn, sDocumentPath + "/" + sFileName + "_1" + Extn);

                                            // ActualDocumentUID = Guid.NewGuid().ToString();
                                            string UploadFilePhysicalpath = documentPagePath;

                                            savedPath = "~/" + pExists + "/" + wExists + "/Documents" + "/" + sFileName + "_1_copy" + Extn;
                                            documentPagePath = "~/" + pExists + "/" + wExists + "/Documents" + "/" + sFileName + "_1" + Extn;
                                            //db.EncryptFile(savedPath, sDocumentPath + "/" + sFileName + "_1" + Extn);
                                        }
                                    }
                                }
                            }
                            DataSet dsdocStatus= db.getActualDocumentStatusList(new Guid(DocumentUid));
                            if(dsdocStatus.Tables[0].Rows.Count==0)
                            {
                                return Json(new
                                {
                                    Status = "Failure",
                                    Message = "Error:document Status is not available" 
                                });
                            }
                            int DocVersion = db.getDocumentStatusVersion(new Guid(dsdocStatus.Tables[0].Rows[dsdocStatus.Tables[0].Rows.Count-1]["StatusUID"].ToString()), new Guid(DocumentUid));
                            int cnt = db.InsertDocumentVersion(Guid.NewGuid(), new Guid(dsdocStatus.Tables[0].Rows[dsdocStatus.Tables[0].Rows.Count - 1]["StatusUID"].ToString()), new Guid(DocumentUid), Extn, documentPagePath, Comments, CoverPagePath);
                            if (cnt > 0)
                            {
                                DataSet dsUsers = db.getAllUsers();
                                if (ds.Tables[0].Rows.Count > 0)
                                {
                                    string CC = string.Empty;
                                    for (int i = 1; i < ds.Tables[0].Rows.Count; i++)
                                    {
                                        CC += ds.Tables[0].Rows[i]["EmailID"].ToString() + ",";
                                    }
                                    CC = CC.TrimEnd(',');
                                    string sHtmlString = string.Empty;
                                    sHtmlString = "<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>" + "<html xmlns='http://www.w3.org/1999/xhtml'>" +
                                            "<head>" + "<meta http-equiv='Content-Type' content='text/html; charset=utf-8' />" + "</head>" +
                                                "<body style='font-family:Verdana, Arial, sans-serif; font-size:12px; font-style:normal;'>";
                                    sHtmlString += "<div style='float:left; width:100%; height:30px;'>" +
                                                        "Dear, " + "Users" +
                                                        "<br/><br/></div>";
                                    sHtmlString += "<div style='width:100%; float:left;'><div style='width:100%; float:left;'>Below are the Status details. <br/><br/></div>";
                                    sHtmlString += "<div style='width:100%; float:left;'><div style='width:100%; float:left;'>Document Name : " + db.getDocumentName_by_DocumentUID(new Guid(dsDocuments.Rows[0]["DocumentUID"].ToString())) + "<br/></div>";
                                    sHtmlString += "<div style='width:100%; float:left;'><div style='width:100%; float:left;'>Version : " + DocVersion + "<br/></div>";
                                    sHtmlString += "<div style='width:100%; float:left;'><div style='width:100%; float:left;'>Status : " + db.getDocumentStatus_by_StatusUID(new Guid(dsdocStatus.Tables[0].Rows[0]["StatusUID"].ToString())) + "<br/></div>";
                                    sHtmlString += "<div style='width:100%; float:left;'><div style='width:100%; float:left;'>Document Type : " + Extn + "<br/></div>";
                                    sHtmlString += "<div style='width:100%; float:left;'><div style='width:100%; float:left;'>Date : " + DateTime.Now.ToShortDateString() + "<br/></div>";
                                    sHtmlString += "<div style='width:100%; float:left;'><div style='width:100%; float:left;'>Comments : " + Comments + "<br/></div>";
                                    sHtmlString += "<div style='width:100%; float:left;'><br/><br/>Sincerely, <br/> Project Manager Tool.</div></div></body></html>";
                                    //string ret = db.SendMail(ds.Tables[0].Rows[0]["EmailID"].ToString(), Subject, sHtmlString, CC, Server.MapPath(DocPath));
                                }                                                    
                            }

                          
                        }
                    }
                }
                else
                {
                    sError = true;
                    ErrorText = "Not Authorized IP address.";
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
            else
            {
                if (!string.IsNullOrEmpty(CoverUID))
                {
                    return Json(new
                    {
                        Status = "Success",
                        CoverLetterUID = CoverUID,
                        ActualDocumentUID = DocumentUid.TrimEnd(','),
                        Message = "Documents Uploaded Successfully."
                    });
                }
                else
                {
                    return Json(new
                    {
                        Status = "Success",
                        ActualDocumentUID = DocumentUid.TrimEnd(','),
                        Message = "Documents Uploaded Successfully."
                    });
                }

            }
        }

        [Authorize]
        [HttpPost] // this is for adding submittals
        [Route("api/ExtoIntegration/GetUsersforAddSubmittal")]
         // for getting Users for Add Submittal
        public IHttpActionResult GetUsersforAddSubmittal()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                var identity = (ClaimsIdentity)User.Identity;
                DataSet ds = new DataSet();
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
                    var ProjectName = httpRequest.Params["ProjectName"];
                    

                    var pExists = db.ProjectNameExists(ProjectName);
                    if (pExists != "")
                    {
                        ds = db.GetUsers_under_ProjectUID(new Guid(pExists));

                        return Json(new
                        {
                            Status = "Success",
                            Users = ds.Tables[0]
                        });
                    }
                    else
                    {
                        return Json(new
                        {
                            Status = "Failure",
                            Message = "Project Name does not Exists !"
                        });
                    }
                }
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

        private void EncryptFile(string inputFile, string outputFile)
        {

            try
            {
                string password = @"myKey123"; // Your Key Here
                UnicodeEncoding UE = new UnicodeEncoding();
                byte[] key = UE.GetBytes(password);

                string cryptFile = outputFile;
                FileStream fsCrypt = new FileStream(cryptFile, FileMode.Create);

                RijndaelManaged RMCrypto = new RijndaelManaged();

                CryptoStream cs = new CryptoStream(fsCrypt,
                    RMCrypto.CreateEncryptor(key, key),
                    CryptoStreamMode.Write);

                FileStream fsIn = new FileStream(inputFile, FileMode.Open);

                int data;
                while ((data = fsIn.ReadByte()) != -1)
                    cs.WriteByte((byte)data);


                fsIn.Close();
                cs.Close();
                fsCrypt.Close();
            }
            catch (Exception ex)
            {
                string exmsg = ex.Message;
                //MessageBox.Show("Encryption failed!", "Error");
            }
        }

        private bool checkDocumentExists(string filename, string sDocumentUID)
        {
            bool result = false;
            DataSet dsfiles = new DataSet();
            try
            {
                dsfiles = db.ActualDocuments_SelectBy_DocumentUID(new Guid(sDocumentUID));
                foreach (DataRow dr in dsfiles.Tables[0].Rows)
                {
                    if (dr["ActualDocument_Name"].ToString() == filename)
                    {
                        result = true;
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }



        // added for Venkat on 08/02/2022
        [Authorize]
        [HttpPost]
        [Route("api/ExtoIntegration/GetDocumentStatus")]
        public IHttpActionResult GetDocumentStatus()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                var identity = (ClaimsIdentity)User.Identity;
                DataSet dtLastestStatus = new DataSet();
                var BaseURL = HttpContext.Current.Request.Url.ToString();
                string postData = "documentUid=" + httpRequest.Params["document"];
                db.WebAPITransctionInsert(Guid.NewGuid(), BaseURL, postData, "");
                if (db.CheckGetWebApiSettings(identity.Name, GetIp()) > 0)
                {
                    var documentUid = httpRequest.Params["document"];
                    dtLastestStatus = db.getLatesDocumentStatus(new Guid(documentUid));
                    if (dtLastestStatus.Tables[0].Rows.Count == 0)
                    {
                        return Json(new
                        {
                            Status = "Failure",
                            Message = "No data available"
                        });
                    }
                    string base64 = "";
                    string getExtension = "";
                    string fileName = "";
                    if (!string.IsNullOrEmpty(dtLastestStatus.Tables[0].Rows[0]["filePath"].ToString()))
                    {
                        string path = ConfigurationManager.AppSettings["DocumentsPathReviewFile"] + dtLastestStatus.Tables[0].Rows[0]["filePath"].ToString();
                        string fileExtension = System.IO.Path.GetExtension(ConfigurationManager.AppSettings["DocumentsPathReviewFile"] + dtLastestStatus.Tables[0].Rows[0]["filePath"].ToString());
                        string outPath = ConfigurationManager.AppSettings["DocumentsPathReviewFile"] + dtLastestStatus.Tables[0].Rows[0]["filePath"].ToString().Replace(fileExtension, "") + "_download" + fileExtension;
                        db.DecryptFile(path, outPath);

                        Byte[] bytes = File.ReadAllBytes(outPath);
                        base64 = Convert.ToBase64String(bytes);

                        getExtension = System.IO.Path.GetExtension(ConfigurationManager.AppSettings["DocumentsPathReviewFile"] + dtLastestStatus.Tables[0].Rows[0]["filePath"].ToString());
                        fileName = System.IO.Path.GetFileName(ConfigurationManager.AppSettings["DocumentsPathReviewFile"] + dtLastestStatus.Tables[0].Rows[0]["filePath"].ToString());
                    }
                    return Json(new
                    {
                        status = "Success",
                        documentstatus = dtLastestStatus.Tables[0].Rows[0]["Status"].ToString(),
                        statusupadteddate = dtLastestStatus.Tables[0].Rows[0]["statusUpdatedDate"].ToString(),
                        remarks = dtLastestStatus.Tables[0].Rows[0]["remarks"].ToString(),
                        fileContent = base64,
                        fileName = fileName,
                        fileType = getExtension
                    });

                }
                else
                {
                    return Json(new
                    {
                        Success = false,
                        Message = "Not Authorized IP address"
                    });
                }




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




        //added on 06/02/2022  for venkat
        //[Authorize]
        //[HttpPost]
        //[Route("api/ExtoIntegration/GetDocumentStatus")]
        //public IHttpActionResult GetDocumentStatus()
        //{

        //    try
        //    {
        //        var httpRequest = HttpContext.Current.Request;
        //        var identity = (ClaimsIdentity)User.Identity;
        //        DataSet dtLastestStatus = new DataSet();
        //        var BaseURL = HttpContext.Current.Request.Url.ToString();
        //        string postData = "documentUid=" + httpRequest.Params["document"];
        //        db.WebAPITransctionInsert(Guid.NewGuid(), BaseURL, postData, "");
        //        if (db.CheckGetWebApiSettings(identity.Name, GetIp()) > 0)
        //        {
        //            var documentUid = httpRequest.Params["document"];
        //            dtLastestStatus = db.getLatesDocumentStatus(new Guid(documentUid));
        //            if (dtLastestStatus.Tables[0].Rows.Count == 0)
        //            {
        //                return Json(new
        //                {
        //                    Status = "Failure",
        //                    Message = "No data available"
        //                });
        //            }
        //            string base64 = "";
        //            string getExtension = "";
        //            string fileName = "";
        //            if (!string.IsNullOrEmpty(dtLastestStatus.Tables[0].Rows[0]["filePath"].ToString()))
        //            {
        //                Byte[] bytes = File.ReadAllBytes(ConfigurationManager.AppSettings["DocumentsPathReviewFile"] + dtLastestStatus.Tables[0].Rows[0]["filePath"].ToString());
        //                base64 = Convert.ToBase64String(bytes);
        //                getExtension = System.IO.Path.GetExtension(ConfigurationManager.AppSettings["DocumentsPathReviewFile"] + dtLastestStatus.Tables[0].Rows[0]["filePath"].ToString());
        //                fileName = System.IO.Path.GetFileName(ConfigurationManager.AppSettings["DocumentsPathReviewFile"] + dtLastestStatus.Tables[0].Rows[0]["filePath"].ToString());
        //            }
        //            return Json(new
        //            {
        //                status = dtLastestStatus.Tables[0].Rows[0]["Status"].ToString(),
        //                statusupadteddate = dtLastestStatus.Tables[0].Rows[0]["statusUpdatedDate"].ToString(),
        //                remarks = dtLastestStatus.Tables[0].Rows[0]["remarks"].ToString(),
        //                fileContent = base64,
        //                fileName = fileName,
        //                fileType = getExtension
        //            });

        //        }
        //        else
        //        {
        //            return Json(new
        //            {
        //                Success = false,
        //                Message = "Not Authorized IP address"
        //            });
        //        }




        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new
        //        {
        //            Success = false,
        //            Message = "Error" + ex.Message
        //        });
        //    }

        //}








        //[Authorize]
        //[HttpPost]
        //[Route("api/ExtoIntegration/AddDocument")]
        //public IHttpActionResult AddDocument()
        //{
        //    bool sError = false;
        //    string ErrorText = "";
        //    string CoverUID = string.Empty;
        //    string DocUID = string.Empty;
        //    try
        //    {
        //        var httpRequest = HttpContext.Current.Request;
        //        var ProjectName = httpRequest.Params["ProjectName"];
        //        var WorkpackageName = httpRequest.Params["WorkpackageName"];
        //        var SubmittalUID = httpRequest.Params["SubmittalUID"];
        //        var ReferenceNumber = httpRequest.Params["ReferenceNumber"];
        //        var Description = httpRequest.Params["Description"];
        //        var DocumentType = httpRequest.Params["DocumentType"];
        //        var DocMedia_HardCopy = httpRequest.Params["DocMedia_HardCopy"] != string.Empty ? Convert.ToBoolean(httpRequest.Params["DocMedia_HardCopy"]) : false;
        //        var DocMedia_SoftCopy_PDF = httpRequest.Params["DocMedia_SoftCopy_PDF"] != string.Empty ? Convert.ToBoolean(httpRequest.Params["DocMedia_SoftCopy_PDF"]) : false;
        //        var DocMedia_SoftCopy_Editable = httpRequest.Params["DocMedia_SoftCopy_Editable"] != string.Empty ? Convert.ToBoolean(httpRequest.Params["DocMedia_SoftCopy_Editable"]) : false;
        //        var DocMedia_SoftCopy_Ref = httpRequest.Params["DocMedia_SoftCopy_Ref"] != string.Empty ? Convert.ToBoolean(httpRequest.Params["DocMedia_SoftCopy_Ref"]) : false;
        //        var DocMedia_HardCopy_Ref = httpRequest.Params["DocMedia_HardCopy_Ref"] != string.Empty ? Convert.ToBoolean(httpRequest.Params["DocMedia_HardCopy_Ref"]) : false;
        //        var DocMedia_NoMedia = httpRequest.Params["DocMedia_NoMedia"] != string.Empty ? Convert.ToBoolean(httpRequest.Params["DocMedia_NoMedia"]) : false;
        //        var Originator = httpRequest.Params["Originator"];
        //        var OriginatorRefNo = httpRequest.Params["OriginatorRefNo"];
        //        var IncomingReciveDate = httpRequest.Params["IncomingReciveDate"];
        //        //var CoverLetterDate = httpRequest.Params["CoverLetterDate"];
        //        var DocumentDate = httpRequest.Params["DocumentDate"];
        //        var FileReferenceNumber = httpRequest.Params["FileReferenceNumber"];
        //        var Remarks = httpRequest.Params["Remarks"];

        //        bool CoverLetterUpload = false, DocumentsUpload = false;
        //        var wExists = string.Empty;
        //        var pExists = db.ProjectNameExists(ProjectName);
        //        string ActualDocumentUID = string.Empty;

        //        if (!string.IsNullOrEmpty(pExists))
        //        {
        //            wExists = db.WorkpackageNameExists(WorkpackageName, pExists);
        //            if (!string.IsNullOrEmpty(wExists))
        //            {
        //                if (Guid.TryParse(SubmittalUID, out Guid submittaluid))
        //                {
        //                    if (!string.IsNullOrEmpty(Description))
        //                    {
        //                        if (!string.IsNullOrEmpty(DocumentType))
        //                        {
        //                            string[] formats = { "dd/MM/yyyy" };
        //                            if (DateTime.TryParseExact(DocumentDate, formats, System.Globalization.CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime documentDate))
        //                            {
        //                                if (DocumentType == "Cover Letter")
        //                                {
        //                                    if (string.IsNullOrEmpty(Originator))
        //                                    {
        //                                        sError = true;
        //                                        ErrorText = "Originator cannot be empty.";
        //                                    }
        //                                    if (string.IsNullOrEmpty(OriginatorRefNo))
        //                                    {
        //                                        sError = true;
        //                                        ErrorText = "Originator Reference number cannot be empty.";
        //                                    }
        //                                    if (!DateTime.TryParseExact(IncomingReciveDate, formats, System.Globalization.CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime incomingReceiveDate))
        //                                    {
        //                                        sError = true;
        //                                        ErrorText = "Invalid Incoming Receive Date.";
        //                                    }
        //                                    if (httpRequest.Files.Count == 0)
        //                                    {
        //                                        sError = true;
        //                                        ErrorText = "Please upload a document.";
        //                                    }
        //                                    if (documentDate > incomingReceiveDate)
        //                                    {
        //                                        sError = true;
        //                                        ErrorText = "Document Date connot be greater than Incoming Receive date.";
        //                                    }
        //                                    for (int i = 0; i < httpRequest.Files.Count; i++)
        //                                    {
        //                                        HttpPostedFile httpPostedFile = httpRequest.Files[i];

        //                                        if (httpPostedFile != null)
        //                                        {
        //                                            if (httpPostedFile.FileName.StartsWith("Cover") || httpPostedFile.FileName.StartsWith("Cover_Letter") || httpPostedFile.FileName.StartsWith("Cover Letter"))
        //                                            {
        //                                                CoverLetterUpload = true;
        //                                            }
        //                                            else
        //                                            {
        //                                                DocumentsUpload = true;
        //                                            }
        //                                        }
        //                                    }
        //                                    if (!CoverLetterUpload)
        //                                    {
        //                                        sError = true;
        //                                        ErrorText = "Please upload a cover letter file.";
        //                                    }
        //                                    if (!DocumentsUpload)
        //                                    {
        //                                        sError = true;
        //                                        ErrorText = "Please upload document file.";
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    if (httpRequest.Files.Count == 0)
        //                                    {
        //                                        sError = true;
        //                                        ErrorText = "Please upload a document.";
        //                                    }
        //                                }
        //                            }
        //                            else
        //                            {
        //                                sError = true;
        //                                ErrorText = "Invalid Document Date.";
        //                            }

        //                        }
        //                        else
        //                        {
        //                            sError = true;
        //                            ErrorText = "Document Type cannot be empty.";
        //                        }
        //                    }
        //                    else
        //                    {
        //                        sError = true;
        //                        ErrorText = "Description cannot be empty.";
        //                    }

        //                }
        //                else
        //                {
        //                    sError = true;
        //                    ErrorText = "Invalid Submittal UID.";
        //                }
        //            }
        //            else
        //            {
        //                sError = true;
        //                ErrorText = "Workpackage name not found";
        //            }
        //        }
        //        else
        //        {
        //            sError = true;
        //            ErrorText = "Project name not found.";
        //        }

        //        if (!sError)
        //        {
        //            DataSet ds = db.getDocumentsbyDocID(new Guid(SubmittalUID));
        //            if (ds.Tables[0].Rows.Count > 0)
        //            {
        //                string cStatus = "Submitted", FileDatetime = DateTime.Now.ToString("dd MMM yyyy hh-mm-ss tt"), sDocumentPath = string.Empty,
        //                    DocumentFor = string.Empty, IncomingRec_Date_String = string.Empty, CoverPagePath = string.Empty, CoverLetterUID = ""; ;

        //                if (!string.IsNullOrEmpty(IncomingReciveDate))
        //                {
        //                    IncomingRec_Date_String = IncomingReciveDate;
        //                }
        //                else
        //                {
        //                    IncomingRec_Date_String = DocumentDate;
        //                }
        //                IncomingRec_Date_String = db.ConvertDateFormat(IncomingRec_Date_String);
        //                DateTime IncomingRec_Date = Convert.ToDateTime(IncomingRec_Date_String);

        //                if (string.IsNullOrEmpty(DocumentDate))
        //                {
        //                    DocumentDate = DateTime.MinValue.ToString("dd/MM/yyyy");
        //                }
        //                DocumentDate = db.ConvertDateFormat(DocumentDate);
        //                DateTime Document_Date = Convert.ToDateTime(DocumentDate);

        //                //Cover Letter Insert
        //                for (int i = 0; i < httpRequest.Files.Count; i++)
        //                {
        //                    HttpPostedFile httpPostedFile = httpRequest.Files[i];
        //                    if (httpPostedFile != null)
        //                    {
        //                        string Extn = System.IO.Path.GetExtension(httpPostedFile.FileName);
        //                        if (Extn.ToLower() != ".exe" && Extn.ToLower() != ".msi" && Extn.ToLower() != ".db")
        //                        {
        //                            if ((httpPostedFile.FileName.StartsWith("Cover") || httpPostedFile.FileName.StartsWith("Cover_Letter")) && DocumentType == "Cover Letter")
        //                            {
        //                                DocumentFor = "Cover Letter";
        //                                sDocumentPath = ConfigurationManager.AppSettings["DocumentsPath"] + pExists + "//" + wExists + "//CoverLetter";

        //                                if (!Directory.Exists(sDocumentPath))
        //                                {
        //                                    Directory.CreateDirectory(sDocumentPath);
        //                                }
        //                                string sFileName = Path.GetFileNameWithoutExtension(httpPostedFile.FileName);
        //                                httpPostedFile.SaveAs(sDocumentPath + "/" + sFileName + "_1_copy" + Extn);
        //                                string savedPath = sDocumentPath + "/" + sFileName + "_1_copy" + Extn;
        //                                CoverPagePath = sDocumentPath + "/" + sFileName + "_1" + Extn;
        //                                EncryptFile(savedPath, CoverPagePath);

        //                                CoverLetterUID = Guid.NewGuid().ToString();

        //                                int RetCount = db.DocumentCoverLetter_Insert_or_Update(new Guid(CoverLetterUID), new Guid(pExists), new Guid(wExists), new Guid(SubmittalUID), ReferenceNumber, "",
        //                                DocumentFor, IncomingRec_Date, new Guid(ds.Tables[0].Rows[0]["FlowUID"].ToString()), sFileName, Description, 1, Extn, DocMedia_HardCopy ? "true" : "false",
        //                                DocMedia_SoftCopy_PDF == true ? "true" : "false", DocMedia_SoftCopy_Editable == true ? "true" : "false", DocMedia_SoftCopy_Ref == true ? "true" : "false",
        //                                DocMedia_HardCopy_Ref == true ? "true" : "false", DocMedia_NoMedia == true ? "true" : "false", CoverPagePath, Remarks ?? "", FileReferenceNumber ?? "", cStatus, Originator ?? "", Document_Date);
        //                                if (RetCount <= 0)
        //                                {
        //                                    sError = true;
        //                                    ErrorText = "Error occured while inserting cover letter. Please contact system admin.";
        //                                }
        //                                CoverUID = CoverLetterUID;
        //                                try
        //                                {
        //                                    if (File.Exists(savedPath))
        //                                    {
        //                                        File.Delete(savedPath);
        //                                    }
        //                                }
        //                                catch (Exception ex)
        //                                {
        //                                    //throw
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //                DocUID = string.Empty;
        //                //ActualDocument Insert
        //                for (int i = 0; i < httpRequest.Files.Count; i++)
        //                {
        //                    HttpPostedFile httpPostedFile = httpRequest.Files[i];
        //                    if (httpPostedFile != null)
        //                    {
        //                        string Extn = System.IO.Path.GetExtension(httpPostedFile.FileName);
        //                        if (Extn.ToLower() != ".exe" && Extn.ToLower() != ".msi" && Extn.ToLower() != ".db")
        //                        {
        //                            if (!httpPostedFile.FileName.StartsWith("Cover"))
        //                            {

        //                                string sDate1 = "", sDate2 = "", sDate3 = "", sDate4 = "", sDate5 = "";
        //                                DateTime CDate1 = DateTime.Now, CDate2 = DateTime.Now, CDate3 = DateTime.Now, CDate4 = DateTime.Now, CDate5 = DateTime.Now;
        //                                if (!checkDocumentExists(Path.GetFileNameWithoutExtension(httpPostedFile.FileName), SubmittalUID))
        //                                {
        //                                    if (DocumentType == "General Document")
        //                                    {
        //                                        DocumentFor = "General Document";
        //                                    }
        //                                    else if (DocumentType == "Photographs")
        //                                    {
        //                                        DocumentFor = "Photographs";
        //                                    }
        //                                    else
        //                                    {
        //                                        DocumentFor = "Document";
        //                                    }

        //                                    sDocumentPath = ConfigurationManager.AppSettings["DocumentsPath"] + pExists + "//" + wExists + "//Documents";

        //                                    if (!Directory.Exists(sDocumentPath))
        //                                    {
        //                                        Directory.CreateDirectory(sDocumentPath);
        //                                    }

        //                                    string sFileName = Path.GetFileNameWithoutExtension(httpPostedFile.FileName);

        //                                    string savedPath = string.Empty;
        //                                    if (DocumentType != "Photographs")
        //                                    {
        //                                        savedPath = sDocumentPath + "/" + sFileName + "_1_copy" + Extn;
        //                                        httpPostedFile.SaveAs(savedPath);

        //                                        CoverPagePath = sDocumentPath + "/" + sFileName + "_1" + Extn;
        //                                        EncryptFile(savedPath, CoverPagePath);
        //                                    }
        //                                    else
        //                                    {
        //                                        savedPath = sDocumentPath + "/" + DateTime.Now.Ticks.ToString() + Path.GetFileName(httpPostedFile.FileName);
        //                                        httpPostedFile.SaveAs(savedPath);

        //                                    }
        //                                    ActualDocumentUID = Guid.NewGuid().ToString();
        //                                    string UploadFilePhysicalpath = CoverPagePath;
        //                                    string Flow1DisplayName = "", Flow2DisplayName = "", Flow3DisplayName = "", Flow4DisplayName = "", Flow5DisplayName = "";
        //                                    DataSet dsFlow = db.GetDocumentFlows_by_UID(new Guid(ds.Tables[0].Rows[0]["FlowUID"].ToString()));
        //                                    if (dsFlow.Tables[0].Rows.Count > 0)
        //                                    {
        //                                        if (dsFlow.Tables[0].Rows[0]["Steps_Count"].ToString() == "-1")
        //                                        {
        //                                            Flow1DisplayName = dsFlow.Tables[0].Rows[0]["FlowStep1_DisplayName"].ToString();

        //                                            sDate1 = Convert.ToDateTime(ds.Tables[0].Rows[0]["FlowStep1_TargetDate"].ToString()).ToString("dd/MM/yyyy");
        //                                            sDate1 = db.ConvertDateFormat(sDate1);
        //                                            CDate1 = Convert.ToDateTime(sDate1);

        //                                            int cnt = db.Document_Insert_or_Update_with_RelativePath_Flow1(new Guid(ActualDocumentUID), new Guid(pExists), new Guid(wExists), new Guid(SubmittalUID), ReferenceNumber, OriginatorRefNo ?? "",
        //                                            DocumentFor, IncomingRec_Date, new Guid(ds.Tables[0].Rows[0]["FlowUID"].ToString()), sFileName, Description, 1, Extn,
        //                                            DocMedia_HardCopy ? "true" : "false", DocMedia_SoftCopy_PDF == true ? "true" : "false", DocMedia_SoftCopy_Editable == true ? "true" : "false", DocMedia_SoftCopy_Ref == true ? "true" : "false",
        //                                            DocMedia_HardCopy_Ref == true ? "true" : "false", DocMedia_NoMedia == true ? "true" : "false", savedPath, Remarks ?? "", FileReferenceNumber ?? "", cStatus,
        //                                            new Guid(ds.Tables[0].Rows[0]["FlowStep1_UserUID"].ToString()), CDate1, Flow1DisplayName, Originator ?? "", Document_Date, "", "", UploadFilePhysicalpath, CoverLetterUID, "Submission");
        //                                            if (cnt <= 0)
        //                                            {
        //                                                sError = true;
        //                                                ErrorText = "Error occured while inserting document. Please contact system admin.";
        //                                            }
        //                                        }
        //                                        else if (dsFlow.Tables[0].Rows[0]["Steps_Count"].ToString() == "1")
        //                                        {
        //                                            Flow1DisplayName = dsFlow.Tables[0].Rows[0]["FlowStep1_DisplayName"].ToString();

        //                                            sDate1 = Convert.ToDateTime(ds.Tables[0].Rows[0]["FlowStep1_TargetDate"].ToString()).ToString("dd/MM/yyyy");
        //                                            sDate1 = db.ConvertDateFormat(sDate1);
        //                                            CDate1 = Convert.ToDateTime(sDate1);

        //                                            int cnt = db.Document_Insert_or_Update_with_RelativePath_Flow1(new Guid(ActualDocumentUID), new Guid(pExists), new Guid(wExists), new Guid(SubmittalUID), ReferenceNumber, OriginatorRefNo ?? "",
        //                                            DocumentFor, IncomingRec_Date, new Guid(ds.Tables[0].Rows[0]["FlowUID"].ToString()), sFileName, Description, 1, Extn,
        //                                            DocMedia_HardCopy ? "true" : "false", DocMedia_SoftCopy_PDF == true ? "true" : "false", DocMedia_SoftCopy_Editable == true ? "true" : "false", DocMedia_SoftCopy_Ref == true ? "true" : "false",
        //                                            DocMedia_HardCopy_Ref == true ? "true" : "false", DocMedia_NoMedia == true ? "true" : "false", CoverPagePath, Remarks ?? "", FileReferenceNumber ?? "", cStatus,
        //                                            new Guid(ds.Tables[0].Rows[0]["FlowStep1_UserUID"].ToString()), CDate1, Flow1DisplayName, Originator ?? "", Document_Date, "", "", UploadFilePhysicalpath, CoverLetterUID, "Submission");
        //                                            if (cnt <= 0)
        //                                            {
        //                                                sError = true;
        //                                                ErrorText = "Error occured while inserting document. Please contact system admin.";
        //                                            }
        //                                        }
        //                                        else if (dsFlow.Tables[0].Rows[0]["Steps_Count"].ToString() == "2")
        //                                        {
        //                                            Flow1DisplayName = dsFlow.Tables[0].Rows[0]["FlowStep1_DisplayName"].ToString();
        //                                            Flow2DisplayName = dsFlow.Tables[0].Rows[0]["FlowStep2_DisplayName"].ToString();

        //                                            sDate1 = Convert.ToDateTime(ds.Tables[0].Rows[0]["FlowStep1_TargetDate"].ToString()).ToString("dd/MM/yyyy");
        //                                            sDate1 = db.ConvertDateFormat(sDate1);
        //                                            CDate1 = Convert.ToDateTime(sDate1);

        //                                            sDate2 = Convert.ToDateTime(ds.Tables[0].Rows[0]["FlowStep2_TargetDate"].ToString()).ToString("dd/MM/yyyy");
        //                                            sDate2 = db.ConvertDateFormat(sDate2);
        //                                            CDate2 = Convert.ToDateTime(sDate2);

        //                                            int cnt = db.Document_Insert_or_Update_with_RelativePath_Flow2(new Guid(ActualDocumentUID), new Guid(pExists), new Guid(wExists), new Guid(SubmittalUID), ReferenceNumber, OriginatorRefNo,
        //                                              DocumentFor, IncomingRec_Date, new Guid(ds.Tables[0].Rows[0]["FlowUID"].ToString()), sFileName, Description, 1, Extn,
        //                                              DocMedia_HardCopy ? "true" : "false", DocMedia_SoftCopy_PDF == true ? "true" : "false", DocMedia_SoftCopy_Editable == true ? "true" : "false", DocMedia_SoftCopy_Ref == true ? "true" : "false",
        //                                              DocMedia_HardCopy_Ref == true ? "true" : "false", DocMedia_NoMedia == true ? "true" : "false", CoverPagePath, Remarks ?? "", FileReferenceNumber ?? "", cStatus,
        //                                              new Guid(ds.Tables[0].Rows[0]["FlowStep1_UserUID"].ToString()), CDate1, new Guid(ds.Tables[0].Rows[0]["FlowStep2_UserUID"].ToString()), CDate2, Flow1DisplayName, Flow2DisplayName, Originator, Document_Date, "", "", UploadFilePhysicalpath, CoverLetterUID, "Submission");
        //                                            if (cnt <= 0)
        //                                            {
        //                                                sError = true;
        //                                                ErrorText = "Error occured while inserting document. Please contact system admin.";
        //                                            }

        //                                        }
        //                                        else if (dsFlow.Tables[0].Rows[0]["Steps_Count"].ToString() == "3")
        //                                        {
        //                                            Flow1DisplayName = dsFlow.Tables[0].Rows[0]["FlowStep1_DisplayName"].ToString();
        //                                            Flow2DisplayName = dsFlow.Tables[0].Rows[0]["FlowStep2_DisplayName"].ToString();
        //                                            Flow3DisplayName = dsFlow.Tables[0].Rows[0]["FlowStep3_DisplayName"].ToString();

        //                                            sDate1 = Convert.ToDateTime(ds.Tables[0].Rows[0]["FlowStep1_TargetDate"].ToString()).ToString("dd/MM/yyyy");
        //                                            sDate1 = db.ConvertDateFormat(sDate1);
        //                                            CDate1 = Convert.ToDateTime(sDate1);


        //                                            sDate2 = Convert.ToDateTime(ds.Tables[0].Rows[0]["FlowStep2_TargetDate"].ToString()).ToString("dd/MM/yyyy");
        //                                            sDate2 = db.ConvertDateFormat(sDate2);
        //                                            CDate2 = Convert.ToDateTime(sDate2);

        //                                            sDate3 = Convert.ToDateTime(ds.Tables[0].Rows[0]["FlowStep3_TargetDate"].ToString()).ToString("dd/MM/yyyy");
        //                                            sDate3 = db.ConvertDateFormat(sDate3);
        //                                            CDate3 = Convert.ToDateTime(sDate3);

        //                                            sDate4 = DateTime.Now.ToString("dd/MM/yyyy");
        //                                            sDate4 = db.ConvertDateFormat(sDate4);
        //                                            CDate4 = Convert.ToDateTime(CDate4);

        //                                            int cnt = db.Document_Insert_or_Update(new Guid(ActualDocumentUID), new Guid(pExists), new Guid(wExists), new Guid(SubmittalUID), ReferenceNumber, OriginatorRefNo,
        //                                           DocumentFor, IncomingRec_Date, new Guid(ds.Tables[0].Rows[0]["FlowUID"].ToString()), sFileName, Description, 1, Extn,
        //                                           DocMedia_HardCopy ? "true" : "false", DocMedia_SoftCopy_PDF == true ? "true" : "false", DocMedia_SoftCopy_Editable == true ? "true" : "false", DocMedia_SoftCopy_Ref == true ? "true" : "false",
        //                                           DocMedia_HardCopy_Ref == true ? "true" : "false", DocMedia_NoMedia == true ? "true" : "false", CoverPagePath, Remarks ?? "", FileReferenceNumber ?? "", cStatus,
        //                                           new Guid(ds.Tables[0].Rows[0]["FlowStep1_UserUID"].ToString()), CDate1, new Guid(ds.Tables[0].Rows[0]["FlowStep2_UserUID"].ToString()), CDate2, new Guid(ds.Tables[0].Rows[0]["FlowStep3_UserUID"].ToString()), CDate3,
        //                                           Flow1DisplayName, Flow2DisplayName, Flow3DisplayName, Originator, Document_Date, UploadFilePhysicalpath, CoverLetterUID, "Submission");
        //                                            if (cnt <= 0)
        //                                            {
        //                                                sError = true;
        //                                                ErrorText = "Error occured while inserting document. Please contact system admin.";
        //                                            }

        //                                        }
        //                                        else if (dsFlow.Tables[0].Rows[0]["Steps_Count"].ToString() == "4")
        //                                        {
        //                                            Flow1DisplayName = dsFlow.Tables[0].Rows[0]["FlowStep1_DisplayName"].ToString();
        //                                            Flow2DisplayName = dsFlow.Tables[0].Rows[0]["FlowStep2_DisplayName"].ToString();
        //                                            Flow3DisplayName = dsFlow.Tables[0].Rows[0]["FlowStep3_DisplayName"].ToString();
        //                                            Flow4DisplayName = dsFlow.Tables[0].Rows[0]["FlowStep4_DisplayName"].ToString();

        //                                            sDate1 = Convert.ToDateTime(ds.Tables[0].Rows[0]["FlowStep1_TargetDate"].ToString()).ToString("dd/MM/yyyy");
        //                                            sDate1 = db.ConvertDateFormat(sDate1);
        //                                            CDate1 = Convert.ToDateTime(sDate1);


        //                                            sDate2 = Convert.ToDateTime(ds.Tables[0].Rows[0]["FlowStep2_TargetDate"].ToString()).ToString("dd/MM/yyyy");
        //                                            sDate2 = db.ConvertDateFormat(sDate2);
        //                                            CDate2 = Convert.ToDateTime(sDate2);

        //                                            sDate3 = Convert.ToDateTime(ds.Tables[0].Rows[0]["FlowStep3_TargetDate"].ToString()).ToString("dd/MM/yyyy");
        //                                            sDate3 = db.ConvertDateFormat(sDate3);
        //                                            CDate3 = Convert.ToDateTime(sDate3);


        //                                            sDate4 = Convert.ToDateTime(ds.Tables[0].Rows[0]["FlowStep4_TargetDate"].ToString()).ToString("dd/MM/yyyy");
        //                                            sDate4 = db.ConvertDateFormat(sDate4);
        //                                            CDate4 = Convert.ToDateTime(sDate4);

        //                                            int cnt = db.Document_Insert_or_Update_with_RelativePath_Flow4(new Guid(ActualDocumentUID), new Guid(pExists), new Guid(wExists), new Guid(SubmittalUID), ReferenceNumber, OriginatorRefNo,
        //                                              DocumentFor, IncomingRec_Date, new Guid(ds.Tables[0].Rows[0]["FlowUID"].ToString()), sFileName, Description, 1, Extn,
        //                                              DocMedia_HardCopy ? "true" : "false", DocMedia_SoftCopy_PDF == true ? "true" : "false", DocMedia_SoftCopy_Editable == true ? "true" : "false", DocMedia_SoftCopy_Ref == true ? "true" : "false",
        //                                              DocMedia_HardCopy_Ref == true ? "true" : "false", DocMedia_NoMedia == true ? "true" : "false", CoverPagePath, Remarks ?? "", FileReferenceNumber ?? "", cStatus,
        //                                              new Guid(ds.Tables[0].Rows[0]["FlowStep1_UserUID"].ToString()), CDate1, new Guid(ds.Tables[0].Rows[0]["FlowStep2_UserUID"].ToString()), CDate2, new Guid(ds.Tables[0].Rows[0]["FlowStep3_UserUID"].ToString()), CDate3, new Guid(ds.Tables[0].Rows[0]["FlowStep4_UserUID"].ToString()), CDate4,
        //                                              Flow1DisplayName, Flow2DisplayName, Flow3DisplayName, Flow4DisplayName, Originator, Document_Date, "", "", UploadFilePhysicalpath, CoverLetterUID, "Submission");
        //                                            if (cnt <= 0)
        //                                            {
        //                                                sError = true;
        //                                                ErrorText = "Error occured while inserting document. Please contact system admin.";
        //                                            }

        //                                        }
        //                                        else
        //                                        {
        //                                            Flow1DisplayName = dsFlow.Tables[0].Rows[0]["FlowStep1_DisplayName"].ToString();
        //                                            Flow2DisplayName = dsFlow.Tables[0].Rows[0]["FlowStep2_DisplayName"].ToString();
        //                                            Flow3DisplayName = dsFlow.Tables[0].Rows[0]["FlowStep3_DisplayName"].ToString();
        //                                            Flow4DisplayName = dsFlow.Tables[0].Rows[0]["FlowStep4_DisplayName"].ToString();
        //                                            Flow5DisplayName = dsFlow.Tables[0].Rows[0]["FlowStep5_DisplayName"].ToString();

        //                                            sDate1 = Convert.ToDateTime(ds.Tables[0].Rows[0]["FlowStep1_TargetDate"].ToString()).ToString("dd/MM/yyyy");
        //                                            sDate1 = db.ConvertDateFormat(sDate1);
        //                                            CDate1 = Convert.ToDateTime(sDate1);

        //                                            sDate2 = Convert.ToDateTime(ds.Tables[0].Rows[0]["FlowStep2_TargetDate"].ToString()).ToString("dd/MM/yyyy");
        //                                            sDate2 = db.ConvertDateFormat(sDate2);
        //                                            CDate2 = Convert.ToDateTime(sDate2);

        //                                            sDate3 = Convert.ToDateTime(ds.Tables[0].Rows[0]["FlowStep3_TargetDate"].ToString()).ToString("dd/MM/yyyy");
        //                                            sDate3 = db.ConvertDateFormat(sDate3);
        //                                            CDate3 = Convert.ToDateTime(sDate3);


        //                                            sDate4 = Convert.ToDateTime(ds.Tables[0].Rows[0]["FlowStep4_TargetDate"].ToString()).ToString("dd/MM/yyyy");
        //                                            sDate4 = db.ConvertDateFormat(sDate4);
        //                                            CDate4 = Convert.ToDateTime(sDate4);

        //                                            sDate5 = Convert.ToDateTime(ds.Tables[0].Rows[0]["FlowStep4_TargetDate"].ToString()).ToString("dd/MM/yyyy");
        //                                            sDate5 = db.ConvertDateFormat(sDate5);
        //                                            CDate5 = Convert.ToDateTime(sDate5);

        //                                            int cnt = db.Document_Insert_or_Update_with_RelativePath_Flow5(new Guid(ActualDocumentUID), new Guid(pExists), new Guid(wExists), new Guid(SubmittalUID), ReferenceNumber, OriginatorRefNo,
        //                                              DocumentFor, IncomingRec_Date, new Guid(ds.Tables[0].Rows[0]["FlowUID"].ToString()), sFileName, Description, 1, Extn,
        //                                              DocMedia_HardCopy ? "true" : "false", DocMedia_SoftCopy_PDF == true ? "true" : "false", DocMedia_SoftCopy_Editable == true ? "true" : "false", DocMedia_SoftCopy_Ref == true ? "true" : "false",
        //                                              DocMedia_HardCopy_Ref == true ? "true" : "false", DocMedia_NoMedia == true ? "true" : "false", CoverPagePath, Remarks ?? "", FileReferenceNumber ?? "", cStatus,
        //                                              new Guid(ds.Tables[0].Rows[0]["FlowStep1_UserUID"].ToString()), CDate1, new Guid(ds.Tables[0].Rows[0]["FlowStep2_UserUID"].ToString()), CDate2, new Guid(ds.Tables[0].Rows[0]["FlowStep3_UserUID"].ToString()), CDate3, new Guid(ds.Tables[0].Rows[0]["FlowStep4_UserUID"].ToString()), CDate4, new Guid(ds.Tables[0].Rows[0]["FlowStep5_UserUID"].ToString()), CDate5,
        //                                              Flow1DisplayName, Flow2DisplayName, Flow3DisplayName, Flow4DisplayName, Flow5DisplayName, Originator, Document_Date, "", "", UploadFilePhysicalpath, CoverLetterUID, "Submission");
        //                                            if (cnt <= 0)
        //                                            {
        //                                                sError = true;
        //                                                ErrorText = "Error occured while inserting document. Please contact system admin.";
        //                                            }
        //                                        }

        //                                        DocUID += ActualDocumentUID + ",";
        //                                    }

        //                                    try
        //                                    {
        //                                        if (DocumentType != "Photographs")
        //                                        {
        //                                            if (File.Exists(savedPath))
        //                                            {
        //                                                File.Delete(savedPath);
        //                                            }
        //                                        }

        //                                    }
        //                                    catch (Exception ex)
        //                                    {
        //                                        //throw
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
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
        //        if (!string.IsNullOrEmpty(CoverUID))
        //        {
        //            return Json(new
        //            {
        //                Status = "Success",
        //                CoverLetterUID = CoverUID,
        //                ActualDocumentUID = DocUID.TrimEnd(','),
        //                Message = "Documents Uploaded Successfully."
        //            });
        //        }
        //        else
        //        {
        //            return Json(new
        //            {
        //                Status = "Success",
        //                ActualDocumentUID = DocUID.TrimEnd(','),
        //                Message = "Documents Uploaded Successfully."
        //            });
        //        }

        //    }
        //}
    }
}
