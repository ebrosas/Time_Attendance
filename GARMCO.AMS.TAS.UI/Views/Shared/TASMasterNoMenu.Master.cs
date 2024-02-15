using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using GARMCO.AMS.GAP.Utility;
using GARMCO.AMS.TAS.UI.Helpers;
//using GARMCO.AMS.TAS.UI.TASWCFProxy;
using GARMCO.Common.DAL.WebCommonSetup;
using Telerik.Web.UI;

namespace GARMCO.AMS.TAS.UI.Views.Shared
{
    public partial class TASMasterNoMenu : System.Web.UI.MasterPage
    {
        #region Properties
        public ObjectDataSource UserFormDataAccess
        {
            get { return this.objUserFormAccess; }
        }

        public string DefaultButton
        {
            set
            {
                this.form1.DefaultButton = value;
            }
        }

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
                return !Path.GetFileName(Request.Path).Equals(UIHelper.PAGE_ERROR);
            }
        }

        public bool IsRecordModified
        {
            get
            {
                bool isModified = false;
                if (ViewState["IsRecordModified"] != null)
                    isModified = Convert.ToBoolean(ViewState["IsRecordModified"]);

                return isModified;
            }

            set
            {
                ViewState["IsRecordModified"] = value;
            }
        }

        public string FormAccess
        {
            get
            {
                string userFormAccess = GAPConstants.FORM_ACCESS_DEFAULT;
                if (!String.IsNullOrEmpty(this.HiddenFormAccess))
                    userFormAccess = this.HiddenFormAccess;

                return userFormAccess;
            }

            set
            {
                this.HiddenFormAccess = value;
            }
        }

        public bool IsSessionExpired
        {
            get
            {
                return (Session[GAPConstants.GARMCO_USERID] == null);
            }
        }

        public string FormTitle
        {
            get
            {
                return this.litPageTitle.Text.Trim();
            }

            set
            {
                this.litPageTitle.Text = value;
            }
        }

        public string SearchUrl
        {
            get
            {
                return this.hidSearchUrl.Value;
            }

            set
            {
                this.hidSearchUrl.Value = value;
            }
        }

        public bool IsRetrieveAllowed
        {
            get
            {
                #region Common Admin security
                return GAPFunction.CheckFormAccess(this.FormAccess, GAPConstants.FormAccessIndex.Retrieve);
                #endregion
            }
        }

        public bool IsCreateAllowed
        {
            get
            {
                #region Common Admin security
                return GAPFunction.CheckFormAccess(this.FormAccess, GAPConstants.FormAccessIndex.Create);
                #endregion
            }
        }

        public bool IsEditAllowed
        {
            get
            {
                #region Common Admin security
                return GAPFunction.CheckFormAccess(this.FormAccess, GAPConstants.FormAccessIndex.Update);
                #endregion
            }
        }

        public bool IsDeleteAllowed
        {
            get
            {
                #region Common Admin Security
                return GAPFunction.CheckFormAccess(this.FormAccess, GAPConstants.FormAccessIndex.Delete);
                #endregion
            }
        }

        public bool IsPrintAllowed
        {
            get
            {
                return GAPFunction.CheckFormAccess(this.FormAccess, GAPConstants.FormAccessIndex.Print);
            }
        }

        public string LogOnUser
        {
            get { return litUser.Text.Trim(); }
            set { litUser.Text = value; }
        }

        public string LogOnUserInfo
        {
            get { return litUserInfo.Text.Trim(); }
            set { litUserInfo.Text = value; }
        }

        public string HiddenFormAccess
        {
            get { return this.hidFormAccess.Value.Trim(); }
            set { this.hidFormAccess.Value = value; }
        }

        //private TASServiceClient WCFProxy
        //{
        //    get
        //    {
        //        TASServiceClient proxy;
        //        if (Session[UIHelper.PAGE_WCF_SESSION] == null)
        //        {
        //            string DynamicEndpointAddress = ConfigurationManager.AppSettings["WCFServiceURL"];
        //            BasicHttpBinding customBinding = ServiceHelper.GetCustomBinding();
        //            EndpointAddress endpointAddress = new EndpointAddress(DynamicEndpointAddress);

        //            proxy = new TASServiceClient(customBinding, endpointAddress);

        //            #region Set the value of MaxItemsInObjectGraph to maximum so that the service can receive large files
        //            try
        //            {
        //                foreach (OperationDescription op in proxy.ChannelFactory.Endpoint.Contract.Operations)
        //                {
        //                    var dataContractBehavior = op.Behaviors.Find<DataContractSerializerOperationBehavior>();
        //                    if (dataContractBehavior != null)
        //                    {
        //                        dataContractBehavior.MaxItemsInObjectGraph = int.MaxValue;
        //                    }
        //                }
        //            }
        //            catch (Exception)
        //            {
        //            }
        //            #endregion

        //            Session[UIHelper.PAGE_WCF_SESSION] = proxy;
        //        }
        //        else
        //            proxy = Session[UIHelper.PAGE_WCF_SESSION] as TASServiceClient;

        //        return proxy;
        //    }
        //}

        public string ApplicationEnvironment
        {
            set
            {
                this.litEnvironment.Text = value;
            }
        }

        public List<string> AllowedCostCenterList
        {
            get
            {
                List<string> list = Session[UIHelper.CONST_ALLOWED_COSTCENTER] as List<string>;
                if (list == null)
                    Session[UIHelper.CONST_ALLOWED_COSTCENTER] = list = new List<string>();

                return list;
            }
        }
        #endregion

        #region Page Events
        protected void Page_Init(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                #region Get Allowed Cost Center list
                //if (Session[UIHelper.CONST_ALLOWED_COSTCENTER] == null)
                //{
                    int empNo = UIHelper.ConvertObjectToInt(Session[GAPConstants.GARMCO_USERID]);
                    Session[UIHelper.CONST_ALLOWED_COSTCENTER] = empNo > 0 ? UIHelper.GetAllowedCostCenterByApp(UIHelper.ApplicationCodes.TAS3.ToString(), empNo) : null;
                //}
                #endregion
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack && !this.IsSessionExpired)
            {
                #region Checks if user has permission to view the page
                //if (!GAPFunction.CheckFormAccess(this.FormAccess, GAPConstants.FormAccessIndex.Retrieve))
                //    Response.Redirect(String.Format("{0}?error={1}", UIHelper.PAGE_ERROR, Convert.ToInt32(UIHelper.PageErrorCodes.NoAccessOnPage)), false);
                #endregion

                this.Page.Title = UIHelper.MASTER_PAGE_TITLE;

                #region Set the application environment
                try
                {
                    bool isLiveDB = false;
                    string dbConnectionString = UIHelper.ConvertObjectToString(ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString);
                    string sqlProdServerName = UIHelper.ConvertObjectToString(ConfigurationManager.AppSettings["SQLProductionServer"]);

                    if (!string.IsNullOrEmpty(dbConnectionString))
                    {
                        string[] connectionArray = dbConnectionString.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        if (connectionArray.Length > 0)
                        {
                            foreach (string item in connectionArray)
                            {
                                int idx = item.IndexOf("=");
                                string searchKey = item.Substring(0, idx);
                                if (searchKey.ToUpper() == "DATA SOURCE")
                                {
                                    string searchValue = item.Substring(idx + 1);
                                    if (searchValue.Trim().ToUpper() == sqlProdServerName.ToUpper())
                                    {
                                        isLiveDB = true;
                                    }
                                    break;
                                }
                            }
                        }
                    }

                    if (isLiveDB)
                        this.ApplicationEnvironment = "PRODUCTION";
                    else
                        this.ApplicationEnvironment = "TEST";
                }
                catch (Exception)
                {

                }
                #endregion

                #region Determine the homepage to use
                string homePage = UIHelper.PAGE_HOME;

                // Find the Home menu item
                RadMenuItem homeMenuItem = this.mainMenu.FindItemByValue("Home");
                if (homeMenuItem != null)
                {
                    homeMenuItem.NavigateUrl = homePage;
                }
                #endregion
            }
        }
        #endregion

        #region Database Access
        public void SetPageForm(string formCode)
        {
            #region Retrieves user's access to the form
            this.objUserFormAccess.SelectParameters["userFrmFormCode"].DefaultValue = formCode;
            this.objUserFormAccess.Select();
            #endregion
        }

        protected void objUserFormAccess_Selected(object sender, ObjectDataSourceStatusEventArgs e)
        {
            UserFormAccessDAL.UserFormAccessDataTable dataTable = e.ReturnValue as
                UserFormAccessDAL.UserFormAccessDataTable;
            if (dataTable != null && dataTable.Rows.Count > 0)
                this.FormAccess = (dataTable.Rows[0] as UserFormAccessDAL.UserFormAccessRow).UserFrmCRUDP;
        }
        #endregion
    }
}