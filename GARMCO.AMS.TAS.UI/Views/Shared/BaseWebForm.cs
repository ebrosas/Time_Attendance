using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Web;
using GARMCO.AMS.TAS.BL.Entities;
using GARMCO.AMS.TAS.UI.Helpers;
//using GARMCO.AMS.TAS.UI.TASWCFProxy;

namespace GARMCO.AMS.TAS.UI.Views.Shared
{
    public class BaseWebForm : System.Web.UI.Page
    {
        #region Enumeration
        public enum UserInfoSourceType
        {
            EmployeeMaster,
            ActiveDirectoryDB
        }
        #endregion

        #region Properties
        public bool IsRetrieveUserInfo
        {
            get
            {
                bool viewPage = false;
                if (ViewState["IsRetrieveUserInfo"] != null)
                    viewPage = Convert.ToBoolean(ViewState["IsRetrieveUserInfo"]);

                return viewPage;
            }

            set
            {
                ViewState["IsRetrieveUserInfo"] = value;
            }
        }

        public bool IsToCheckSession
        {
            get
            {
                return !Path.GetFileName(Request.Path).Equals("ErrorMessage.aspx");
            }
        }

        public bool IsRecreateWorkflow
        {
            get
            {
                return UIHelper.ConvertObjectToBolean(Session[UIHelper.PAGE_RECREATE_WORKFLOW_SESSION]);
            }
            set
            {
                Session[UIHelper.PAGE_RECREATE_WORKFLOW_SESSION] = value;
            }
        }

        //public TASServiceClient WCFProxy
        //{
        //    get
        //    {
        //        try
        //        {
        //            TASServiceClient proxy;
        //            if (Session[UIHelper.PAGE_WCF_SESSION] == null)
        //            {
        //                string DynamicEndpointAddress = ConfigurationManager.AppSettings["WCFServiceURL"];
        //                BasicHttpBinding customBinding = ServiceHelper.GetCustomBinding();
        //                EndpointAddress endpointAddress = new EndpointAddress(DynamicEndpointAddress);

        //                proxy = new TASServiceClient(customBinding, endpointAddress);

        //                #region Set the value of MaxItemsInObjectGraph to maximum so that the service can receive large files
        //                try
        //                {
        //                    foreach (OperationDescription op in proxy.ChannelFactory.Endpoint.Contract.Operations)
        //                    {
        //                        var dataContractBehavior = op.Behaviors.Find<DataContractSerializerOperationBehavior>();
        //                        if (dataContractBehavior != null)
        //                        {
        //                            dataContractBehavior.MaxItemsInObjectGraph = int.MaxValue;
        //                        }
        //                    }
        //                }
        //                catch (Exception)
        //                {
        //                }
        //                #endregion

        //                Session[UIHelper.PAGE_WCF_SESSION] = proxy;
        //            }
        //            else
        //                proxy = Session[UIHelper.PAGE_WCF_SESSION] as TASServiceClient;

        //            return proxy;
        //        }
        //        catch (Exception ex)
        //        {
        //            return null;
        //        }
        //    }
        //}

        public DALProxy dataProxy
        {
            get
            {
                try
                {
                    DALProxy proxy = null;

                    if (Session[UIHelper.PAGE_DAL_SESSION] == null)
                        proxy = new DALProxy();
                    else
                        proxy = Session[UIHelper.PAGE_DAL_SESSION] as DALProxy;

                    return proxy;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        //public CommonWorkflowClient CommonWorkflowProxy
        //{
        //    get
        //    {
        //        CommonWorkflowClient proxy;

        //        if (this.IsRecreateWorkflow)
        //        {
        //            string DynamicEndpointAddress = ConfigurationManager.AppSettings["RecreateWorkflowURL"];
        //            BasicHttpBinding customBinding = ServiceHelper.GetCustomBinding();
        //            EndpointAddress endpointAddress = new EndpointAddress(DynamicEndpointAddress);
        //            proxy = new CommonWorkflowClient(customBinding, endpointAddress);

        //            // Set sessions
        //            Session[UIHelper.PAGE_COMMON_WORKFLOW_SESSION] = null;
        //        }
        //        else
        //        {
        //            if (Session[UIHelper.PAGE_COMMON_WORKFLOW_SESSION] == null)
        //            {
        //                string DynamicEndpointAddress = ConfigurationManager.AppSettings["CommonWorkflowURL"];
        //                BasicHttpBinding customBinding = ServiceHelper.GetCustomBinding();
        //                EndpointAddress endpointAddress = new EndpointAddress(DynamicEndpointAddress);
        //                proxy = new CommonWorkflowClient(customBinding, endpointAddress);
        //                Session[UIHelper.PAGE_COMMON_WORKFLOW_SESSION] = proxy;
        //            }
        //            else
        //                proxy = Session[UIHelper.PAGE_COMMON_WORKFLOW_SESSION] as CommonWorkflowClient;
        //        }

        //        return proxy;
        //    }
        //}

        public List<string> SpecialUserList
        {
            get
            {
                List<string> list = Session["SpecialUserList"] as List<string>;
                if (list == null)
                    Session["SpecialUserList"] = list = new List<string>();

                return list;
            }
            set
            {
                Session["SpecialUserList"] = value;
            }
        }

        public List<string> SecurityUserList
        {
            get
            {
                List<string> list = Session["SecurityUserList"] as List<string>;
                if (list == null)
                    Session["SecurityUserList"] = list = new List<string>();

                return list;
            }
            set
            {
                Session["SecurityUserList"] = value;
            }
        }

        public bool IsUserAuthenticated
        {
            get
            {
                return UIHelper.ConvertObjectToBolean(Session[UIHelper.GARMCO_USER_IS_AUTHENTICATED]);
            }
        }
        #endregion

        #region Override Methods
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // Checks the session
            this.CheckSession();
        }

        protected override void OnError(EventArgs e)
        {
            // Retrieve the last error
            HttpContext ctx = HttpContext.Current;
            Session[UIHelper.EXCEPTION_ERROR] = ctx.Server.GetLastError();

            // Clear the error
            ctx.Server.ClearError();
            
            Response.Redirect(string.Format("{0}?url={1}", UIHelper.PAGE_ERROR, ctx.Request.Url.ToString()), false);

            base.OnError(e);
        }
        #endregion End of overriding methods

        #region Public Methods
        public void RetrieveUserInfo(string currentUser, UserInfoSourceType sourceType = UserInfoSourceType.ActiveDirectoryDB, int empNo = 0)
        {
            #region Check if in test mode
            try
            {
                bool isTestMode = ConfigurationManager.AppSettings["TestMode"].Trim() == "1" ? true : false;
                string impersonatorName = UIHelper.ConvertObjectToString(ConfigurationManager.AppSettings["TestAdminName"]);
                if (isTestMode && !string.IsNullOrEmpty(impersonatorName))
                {
                    currentUser = string.Concat("GARMCO\\", impersonatorName);
                }
            }
            catch (Exception)
            {
            }
            #endregion

            #region Get Employee Information
            EmployeeDetail empInfo = null;

            if (sourceType == UserInfoSourceType.ActiveDirectoryDB)
                empInfo = UIHelper.GetEmployeeInfoAdvanced(0, currentUser, UIHelper.EmployeeInfoSearchType.SearchByUserID, UIHelper.EmployeeInfoSearchMethod.SearchUsingCommonLibrary);
            else
                empInfo = UIHelper.GetEmployeeInfoAdvanced(empNo, string.Empty, UIHelper.EmployeeInfoSearchType.SearchByEmpNo, UIHelper.EmployeeInfoSearchMethod.SearchUsingEmployeeMaster);
            #endregion

            #region Store employee info to session variables
            if (empInfo != null)
            {
                int index = currentUser.LastIndexOf("\\");
                string[] userArray = null;

                #region Store data to session
                Session[UIHelper.GARMCO_USERID] = empInfo.EmpNo;
                Session[UIHelper.GARMCO_USERNAME] = currentUser.Substring(index + 1);
                Session[UIHelper.GARMCO_FULLNAME] = empInfo.EmpName;
                Session[UIHelper.GARMCO_USER_COST_CENTER] = empInfo.CostCenter;
                Session[UIHelper.GARMCO_USER_COST_CENTER_NAME] = empInfo.CostCenterName;
                Session[UIHelper.GARMCO_USER_EMAIL] = empInfo.EmpEmail;
                Session[UIHelper.GARMCO_USER_EXT] = empInfo.PhoneExtension;
                Session[UIHelper.GARMCO_USER_GENDER] = empInfo.Gender;
                Session[UIHelper.GARMCO_USER_DESTINATION] = empInfo.Destination;
                Session[UIHelper.GARMCO_USER_PAY_GRADE] = empInfo.PayGrade.ToString();
                Session[UIHelper.GARMCO_USER_POSITION_ID] = empInfo.PositionID;
                Session[UIHelper.GARMCO_USER_POSITION_DESC] = empInfo.Position;
                Session[UIHelper.GARMCO_USER_EMP_CLASS] = empInfo.EmployeeClass;
                Session[UIHelper.GARMCO_USER_TICKET_CLASS] = empInfo.TicketClass;
                Session[UIHelper.GARMCO_USER_SUPERVISOR_NO] = empInfo.SupervisorEmpNo.ToString();
                Session[UIHelper.GARMCO_USER_SUPERVISOR_NAME] = empInfo.SupervisorEmpName;
                Session[UIHelper.GARMCO_USER_IS_AUTHENTICATED] = true;
                Session[UIHelper.CONST_JS_VERSION] = UIHelper.GetConfigurationValue("JavaScriptFileVersion");

                this.IsRetrieveUserInfo = true;
                #endregion

                #region Check if current user is member of the Administrators group which is defined in the web.config
                bool isAdmin = false;

                try
                {
                    DALProxy proxy = new DALProxy();
                    string error = string.Empty;
                    string innerError = string.Empty;

                    var rawData = proxy.GetWorkflowActionMember(0, UIHelper.DistributionGroupCodes.TASADMIN.ToString(), "ALL", ref error, ref innerError);
                    if (rawData != null)
                    {
                        EmployeeDetail adminEmployee = rawData
                            .Where(a => a.EmpNo == empInfo.EmpNo)
                            .FirstOrDefault();
                        if (adminEmployee != null)
                            isAdmin = true;
                    }

                    //if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["SystemAdministrators"]))
                    //{
                    //    string logonUser = currentUser.Substring(currentUser.LastIndexOf(@"\") + 1);
                    //    string[] userID = ConfigurationManager.AppSettings["SystemAdministrators"].Split(',');
                    //    if (userID != null && userID.Count() > 0)
                    //    {
                    //        List<string> adminUsers = new List<string>();
                    //        adminUsers.AddRange(userID.ToList());
                    //        if (adminUsers.Where(a => a.ToUpper().Trim() == logonUser.ToUpper().Trim()).FirstOrDefault() != null)
                    //        {
                    //            isAdmin = true;
                    //        }
                    //    }
                    //}
                }
                catch (Exception)
                {
                }
                finally
                {
                    Session[UIHelper.GARMCO_USER_IS_ADMIN] = isAdmin;
                }
                #endregion

                #region Check if current user is member of the SpecialUsers group which is defined in the web.config
                bool isSpecialUser = false;

                try
                {
                    if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["SpecialUsers"]))
                    {
                        string logonUser = currentUser.Substring(currentUser.LastIndexOf(@"\") + 1);
                        string[] userID = ConfigurationManager.AppSettings["SpecialUsers"].Split(',');
                        if (userID != null && userID.Count() > 0)
                        {
                            List<string> adminUsers = new List<string>();
                            adminUsers.AddRange(userID.ToList());
                            if (adminUsers.Where(a => a.ToUpper().Trim() == logonUser.ToUpper().Trim()).FirstOrDefault() != null)
                            {
                                isSpecialUser = true;

                                switch (logonUser.Trim())
                                {
                                    case "gatews1":
                                    case "gatews2":
                                        Session[UIHelper.GARMCO_FULLNAME] = "Security User";
                                        break;
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                }
                finally
                {
                    //Session[UIHelper.GARMCO_USER_IS_SPECIAL] = isSpecialUser;
                }
                #endregion

                #region Build the Security Users group
                // Initialize the colllection
                this.SecurityUserList.Clear();

                userArray = ConfigurationManager.AppSettings["SecurityUsers"].Split(',');
                if (userArray != null && 
                    userArray.Count() > 0)
                {
                    this.SecurityUserList.AddRange(userArray.ToList());
                }
                #endregion

                #region Build the Special Users group
                // Initialize the colllection
                this.SpecialUserList.Clear();

                userArray = ConfigurationManager.AppSettings["SpecialUsers"].Split(',');
                if (userArray != null &&
                    userArray.Count() > 0)
                {
                    this.SpecialUserList.AddRange(userArray.ToList());
                }
                #endregion

                #region Determine if user is a group account
                if (sourceType == UserInfoSourceType.ActiveDirectoryDB)
                {
                    string error = string.Empty;
                    string innerError = string.Empty;
                    DALProxy proxy = new DALProxy();

                    EmployeeDetail empMasterRecord = proxy.GetEmployeeDetail(empInfo.EmpNo, ref error, ref innerError);
                    if (empMasterRecord == null)
                        Session[UIHelper.GARMCO_USER_IS_GROUP_ACCOUNT] = true;
                }
                #endregion
            }
            else
            {
                Session[UIHelper.GARMCO_USER_IS_AUTHENTICATED] = false;
            }
            #endregion
        }

        public void CheckSession()
        {
            string currentUser = this.Page.User.Identity.Name;

            if (this.IsToCheckSession)
            {

                // Checks the session
                if ((Session[UIHelper.GARMCO_USERID] == null || Session[UIHelper.GARMCO_USERID].ToString().Equals(String.Empty))
                    && this.IsRetrieveUserInfo)
                {
                    this.RetrieveUserInfo(currentUser);
                }
                //else if (Session[UIHelper.GARMCO_USERID] == null)
                //    Response.Redirect(String.Format("{0}?error={1}", UIHelper.PAGE_ERROR, Convert.ToInt32(UIHelper.PageErrorCodes.SessionExpired)), false);

                // Sets the flag
                else
                    this.IsRetrieveUserInfo = true;
            }
        }

        public void ShowErrorMessage(Exception error)
        {
            try
            {
                //bool isTrainingEnabled = ConfigurationManager.AppSettings["IsTrainingModule"].Trim() == "1" ? true : false;

                HttpContext ctx = HttpContext.Current;
                Session[UIHelper.EXCEPTION_ERROR] = error;
                
                Response.Redirect(string.Format("{0}?url={1}", UIHelper.PAGE_ERROR, ctx.Request.Url.ToString()), false);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion
    }
}