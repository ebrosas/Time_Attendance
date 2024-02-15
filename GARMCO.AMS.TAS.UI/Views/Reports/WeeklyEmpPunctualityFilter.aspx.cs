using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using GARMCO.AMS.GAP.Utility;
using GARMCO.AMS.TAS.BL.Entities;
using GARMCO.AMS.TAS.UI.Helpers;
using GARMCO.AMS.TAS.UI.Views.Shared;
using GARMCO.Common.DAL.Employee;
using Telerik.Web.UI;
using System.Configuration;

namespace GARMCO.AMS.TAS.UI.Views.Reports
{
    public partial class WeeklyEmpPunctualityFilter : BaseWebForm, IFormExtension
    {
        #region Enumeration
        private enum ValidationErrorType
        {
            NoError,
            CustomFormError,
            NoCostCenter,
            NoDateFrom
        }
        #endregion

        #region Properties
        public string FormAccess
        {
            get
            {
                string userFormAccess = GAPConstants.FORM_ACCESS_DEFAULT;
                if (!String.IsNullOrEmpty(this.hidFormAccess.Value))
                    userFormAccess = this.hidFormAccess.Value;

                return userFormAccess;
            }

            set
            {
                this.hidFormAccess.Value = value;
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

        private Dictionary<string, object> WeeklyPunctualityStorage
        {
            get
            {
                Dictionary<string, object> list = Session["WeeklyPunctualityStorage"] as Dictionary<string, object>;
                if (list == null)
                    Session["WeeklyPunctualityStorage"] = list = new Dictionary<string, object>();

                return list;
            }
            set
            {
                Session["WeeklyPunctualityStorage"] = value;
            }
        }

        private string CallerForm
        {
            get
            {
                return UIHelper.ConvertObjectToString(ViewState["CallerForm"]);
            }
            set
            {
                ViewState["CallerForm"] = value;
            }
        }

        private ValidationErrorType ErrorType
        {
            get
            {
                ValidationErrorType result = ValidationErrorType.NoError;
                if (ViewState["ErrorType"] != null)
                {
                    try
                    {
                        result = (ValidationErrorType)Enum.Parse(typeof(ValidationErrorType), UIHelper.ConvertObjectToString(ViewState["ErrorType"]));
                    }
                    catch (Exception)
                    {
                    }
                }
                return result;
            }
            set
            {
                ViewState["ErrorType"] = value;
            }
        }

        private string CustomErrorMsg
        {
            get
            {
                return UIHelper.ConvertObjectToString(ViewState["CustomErrorMsg"]);
            }
            set
            {
                ViewState["CustomErrorMsg"] = value;
            }
        }

        private List<string> AllowedCostCenterList
        {
            get
            {
                List<string> list = Session[UIHelper.CONST_ALLOWED_COSTCENTER] as List<string>;
                if (list == null)
                    Session[UIHelper.CONST_ALLOWED_COSTCENTER] = list = new List<string>();

                return list;
            }
        }

        private List<CostCenterEntity> CostCenterList
        {
            get
            {
                List<CostCenterEntity> list = ViewState["CostCenterList"] as List<CostCenterEntity>;
                if (list == null)
                    ViewState["CostCenterList"] = list = new List<CostCenterEntity>();

                return list;
            }
            set
            {
                ViewState["CostCenterList"] = value;
            }
        }

        private List<PunctualityEntity> ReportDataList
        {
            get
            {
                List<PunctualityEntity> list = ViewState["ReportDataList"] as List<PunctualityEntity>;
                if (list == null)
                    ViewState["ReportDataList"] = list = new List<PunctualityEntity>();

                return list;
            }
            set
            {
                ViewState["ReportDataList"] = value;
            }
        }
        #endregion

        #region Page Events
        protected override void OnInit(EventArgs e)
        {
            base.IsRetrieveUserInfo = true;
            base.OnInit(e);

            if (!this.IsPostBack)
            {
                if (this.Master.IsSessionExpired)
                    Response.Redirect(UIHelper.PAGE_SESSION_TIMEOUT_PAGE, false);

                this.Master.SetPageForm(UIHelper.FormAccessCodes.PUNCTLYRPT.ToString());

                // Checks if search url is specified
                int index = Request.QueryString.ToString().IndexOf("searchUrl=");
                if (index > -1)
                    this.SearchUrl = Server.UrlDecode(Request.QueryString.ToString().Substring(index + 10));
            }

            #region Check culture info
            if (System.Threading.Thread.CurrentThread.CurrentCulture.Name.Trim() != "en-GB")
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-GB");
            }
            #endregion
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                //FillCostCenterCombo();
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                #region Set Page Title and Display Login user
                StringBuilder sb = new StringBuilder();
                string position = UIHelper.ConvertStringToTitleCase(UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USER_POSITION_DESC]));
                string costCenter = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USER_COST_CENTER]);
                string costCenterDesc = UIHelper.ConvertStringToTitleCase(UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USER_COST_CENTER_NAME]));
                string extension = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USER_EXT]);
                string userID = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]);

                if (!string.IsNullOrEmpty(userID))
                {
                    sb.Append(string.Format(@"User ID: GARMCO\{0} <br />", userID));
                }

                //if (!string.IsNullOrEmpty(position))
                //{
                //    sb.Append(string.Format("Position: {0} <br />", position));
                //}

                if (!string.IsNullOrEmpty(costCenter))
                {
                    sb.Append(string.Format("Cost Center: {0} <br />", costCenter));
                }

                this.Master.LogOnUser = string.Format("Welcome {0}",
                   UIHelper.GetUserFirstName(UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_FULLNAME]), UIHelper.ConvertObjectToBolean(Session[UIHelper.GARMCO_USER_IS_SPECIAL])));

                this.Master.LogOnUserInfo = sb.ToString().Trim();
                this.Master.FormTitle = UIHelper.PAGE_WEEKLY_PUNCTUALITY_REPORT_TITLE;
                #endregion

                #region Check if user has permission to access the page
                if (!GAPFunction.CheckFormAccess(this.Master.FormAccess, GAPConstants.FormAccessIndex.Retrieve))
                {
                    if (!UIHelper.ConvertObjectToBolean(Session[UIHelper.GARMCO_USER_IS_SPECIAL]))
                    {
                        Response.Redirect(String.Format("{0}?error={1}&pageName={2}", UIHelper.PAGE_ERROR, Convert.ToInt32(UIHelper.PageErrorCodes.NoAccessOnPage), UIHelper.PAGE_WEEKLY_PUNCTUALITY_REPORT_TITLE), true);
                    }
                }
                #endregion

                #region Checks if current user has permission to create, update, delete and print
                this.btnShowReport.Enabled = this.Master.IsPrintAllowed;
                #endregion

                #region Set default button
                this.Master.DefaultButton = this.btnShowReport.UniqueID;
                #endregion

                #region Restore saved data from local storage
                string formFlag = string.Empty;
                if (this.WeeklyPunctualityStorage.Count > 0)
                {
                    if (this.WeeklyPunctualityStorage.ContainsKey("FormFlag"))
                        formFlag = UIHelper.ConvertObjectToString(this.WeeklyPunctualityStorage["FormFlag"]);
                }
                #endregion

                if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.ShowReport.ToString())
                {
                    #region Show last inquiry data
                    RestoreDataFromCollection();

                    // Clear data storage
                    this.WeeklyPunctualityStorage.Clear();
                    #endregion
                }
                else
                {
                    ClearForm();
                    ProcessQueryString();
                    FillComboData();

                    #region Initialize controls
                    this.dtpDateFrom.MaxDate = DateTime.Now.AddDays(-1);
                    this.dtpDateFrom.SelectedDate = this.dtpDateFrom.MaxDate;

                    //string userCostCenter = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USER_COST_CENTER]);
                    //if (!string.IsNullOrEmpty(userCostCenter) &&
                    //    this.cboCostCenter.Items.Count > 0)
                    //{
                    //    this.cboCostCenter.SelectedValue = userCostCenter;
                    //}
                    #endregion
                }                               
            }

            ScriptManager.RegisterStartupScript(this, this.GetType(), "HideScrollbar", "HideScrollbar();", true);
            AddControlsAttribute();
        }
        #endregion

        #region Action Buttons
        protected void btnReset_Click(object sender, EventArgs e)
        {
            #region Clear the form
            this.dtpDateFrom.SelectedDate = null;
            this.cboCostCenter.Text = string.Empty;
            this.cboCostCenter.SelectedIndex = -1;

            // Clear sessions
            ViewState["CustomErrorMsg"] = null;
            #endregion
        }

        protected void btnShowReport_Click(object sender, EventArgs e)
        {
            #region Perform Data Validation
            int errorCount = 0;

            // Check Date From
            if (this.dtpDateFrom.SelectedDate == null)
            {
                this.txtGeneric.Text = ValidationErrorType.NoDateFrom.ToString();
                this.ErrorType = ValidationErrorType.NoDateFrom;
                this.cusValDateFrom.Validate();
                errorCount++;
            }

            if (errorCount > 0)
            {
                // Set focus to the top panel
                Page.SetFocus(this.lnkMoveUp.ClientID);
                return;
            }
            #endregion

            GetReportData(true);

            #region Display in the report all data in the grid
            if (this.ReportDataList != null &&
                this.ReportDataList.Count > 0)
            {
                // Save report data to session
                Session["WeeklyPunctualityReportSource"] = this.ReportDataList;

                // Determine the date range
                string startDate = this.dtpDateFrom.SelectedDate.Value.ToString();
                string endDate = this.dtpDateFrom.SelectedDate.Value.AddDays(6).ToString();
                string costCenter = string.Format("Cost Center: {0}",
                    this.cboCostCenter.SelectedValue == UIHelper.CONST_COMBO_EMTYITEM_ID ? "All" : this.cboCostCenter.Text);

                StoreDataToCollection(UIHelper.PagePostBackFlags.ShowReport);

                // Show the report
                Response.Redirect
                (
                    String.Format(UIHelper.PAGE_REPORT_VIEWER + "?{0}={1}&{2}={3}&{4}={5}&{6}={7}&{8}={9}",
                    UIHelper.QUERY_STRING_REPORT_TYPE_KEY,
                    UIHelper.ReportTypes.PunctualitySummaryReport.ToString(),
                    UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                    UIHelper.PAGE_WEEKLY_PUNCTUALITY_REPORT,
                    UIHelper.QUERY_STRING_STARTDATE_KEY,
                    startDate,
                    UIHelper.QUERY_STRING_ENDDATE_KEY,
                    endDate,
                    UIHelper.QUERY_STRING_COSTCENTER_KEY,
                    costCenter
                ),
                false);
            }
            else
                DisplayFormLevelError("No matching record was found in the database. Please modify the search filter criterias then try to view the report again!");
            #endregion
        }
        #endregion

        #region Page Control Events
        protected void cusGenericValidator_ServerValidate(object source, ServerValidateEventArgs args)
        {
            CustomValidator validator = source as CustomValidator;

            try
            {
                if (this.ErrorType == ValidationErrorType.CustomFormError)
                {
                    validator.ErrorMessage = this.CustomErrorMsg != string.Empty ? this.CustomErrorMsg : "Unhandled Error Occured";
                    validator.ToolTip = this.CustomErrorMsg != string.Empty ? this.CustomErrorMsg : "Unhandled Error Occured";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoCostCenter)
                {
                    validator.ErrorMessage = "Cost Center is required.";
                    validator.ToolTip = "Cost Center is required.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoDateFrom)
                {
                    validator.ErrorMessage = "Date From is required.";
                    validator.ToolTip = "Date From is required.";
                    args.IsValid = false;
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                this.txtGeneric.Text = string.Empty;
                this.ErrorType = ValidationErrorType.NoError;
            }
        }
        #endregion

        #region IFormExtension Interface Implementation
        public void ClearForm()
        {
            #region Reset controls
            this.dtpDateFrom.SelectedDate = null;
            this.cboCostCenter.Text = string.Empty;
            this.cboCostCenter.SelectedIndex = -1;
            #endregion

            // Clear collections
            KillSessions();
        }

        public void AddControlsAttribute()
        {

        }

        public void ProcessQueryString()
        {
            this.CallerForm = UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_CALLER_FORM_KEY]);
        }

        public void KillSessions()
        {
            // Cler collections
            this.CostCenterList.Clear();
            this.ReportDataList.Clear();

            // Clear sessions
            ViewState["CustomErrorMsg"] = null;
            ViewState["CallerForm"] = null;

            // Clear all viewstates
            ViewState.Clear();
        }
        #endregion

        #region Private Methods
        private void RestoreDataFromCollection()
        {
            if (this.WeeklyPunctualityStorage.Count == 0)
                return;

            #region Restore Query String values
            if (this.WeeklyPunctualityStorage.ContainsKey("CallerForm"))
                this.CallerForm = UIHelper.ConvertObjectToString(this.WeeklyPunctualityStorage["CallerForm"]);
            else
                this.CallerForm = string.Empty;
            #endregion

            #region Restore session values
            if (this.WeeklyPunctualityStorage.ContainsKey("CostCenterList"))
                this.CostCenterList = this.WeeklyPunctualityStorage["CostCenterList"] as List<CostCenterEntity>;
            else
                this.CostCenterList = null;

            if (this.WeeklyPunctualityStorage.ContainsKey("ReportDataList"))
                this.ReportDataList = this.WeeklyPunctualityStorage["ReportDataList"] as List<PunctualityEntity>;
            else
                this.ReportDataList = null;

            FillComboData(false);
            #endregion

            #region Restore control values            
            if (this.WeeklyPunctualityStorage.ContainsKey("cboCostCenter"))
                this.cboCostCenter.SelectedValue = UIHelper.ConvertObjectToString(this.WeeklyPunctualityStorage["cboCostCenter"]);
            else
                this.cboCostCenter.SelectedValue = string.Empty;

            if (this.WeeklyPunctualityStorage.ContainsKey("dtpDateFrom"))
                this.dtpDateFrom.SelectedDate = UIHelper.ConvertObjectToDate(this.WeeklyPunctualityStorage["dtpDateFrom"]);
            else
                this.dtpDateFrom.SelectedDate = null;
            #endregion
        }

        private void StoreDataToCollection(UIHelper.PagePostBackFlags formFlag)
        {
            this.WeeklyPunctualityStorage.Clear();
            this.WeeklyPunctualityStorage.Add("FormFlag", formFlag.ToString());

            #region Save control values to session
            this.WeeklyPunctualityStorage.Add("cboCostCenter", this.cboCostCenter.SelectedValue);
            this.WeeklyPunctualityStorage.Add("dtpDateFrom", this.dtpDateFrom.SelectedDate);
            #endregion

            #region Save Query String values to collection
            this.WeeklyPunctualityStorage.Add("CallerForm", this.CallerForm);
            #endregion

            #region Store session data to collection
            this.WeeklyPunctualityStorage.Add("CostCenterList", this.CostCenterList);
            this.WeeklyPunctualityStorage.Add("ReportDataList", this.ReportDataList);
            #endregion
        }

        private void DisplayFormLevelError(string errorMsg)
        {
            if (string.IsNullOrEmpty(errorMsg))
                return;

            this.CustomErrorMsg = errorMsg;
            this.txtGeneric.Text = ValidationErrorType.CustomFormError.ToString();
            this.ErrorType = ValidationErrorType.CustomFormError;
            this.cusValButton.Validate();
        }

        private void FillComboData(bool reloadFromDB = true)
        {
            FillCostCenterCombo(true);
        }
        #endregion

        #region Database Access
        private void GetReportData(bool reloadDataFromDB = false)
        {
            try
            {
                #region Initialize variables                               
                DateTime? startDate = this.dtpDateFrom.SelectedDate;
                DateTime? endDate = this.dtpDateFrom.SelectedDate.Value.AddDays(6);
                string costCenter = this.cboCostCenter.SelectedValue == UIHelper.CONST_COMBO_EMTYITEM_ID ? string.Empty : this.cboCostCenter.SelectedValue;
                bool hideDayOffHoliday = false;
                int punctualityOccurence = UIHelper.ConvertObjectToInt(ConfigurationManager.AppSettings["PunctualityOccurence"]);
                int lateAttendanceThreshold = UIHelper.ConvertObjectToInt(ConfigurationManager.AppSettings["LateAttendanceThreshold"]);
                int earlyLeavingThreshold = UIHelper.ConvertObjectToInt(ConfigurationManager.AppSettings["EarlyLeavingThreshold"]);
                #endregion

                #region Fill data to the collection
                List<PunctualityEntity> gridSource = new List<PunctualityEntity>();
                if (!reloadDataFromDB)
                {
                    gridSource = this.ReportDataList;
                }
                else
                {
                    string error = string.Empty;
                    string innerError = string.Empty;

                    DALProxy proxy = new DALProxy();
                    var source = proxy.GetPunctualitySummaryReport(1, startDate, endDate, costCenter, punctualityOccurence, lateAttendanceThreshold, earlyLeavingThreshold, hideDayOffHoliday, ref error, ref innerError);
                    if (!string.IsNullOrEmpty(error) || !string.IsNullOrEmpty(innerError))
                    {
                        if (!string.IsNullOrEmpty(innerError))
                            throw new Exception(error, new Exception(innerError));
                        else
                            throw new Exception(error);
                    }
                    else
                    {
                        if (source != null && source.Count() > 0)
                        {
                            gridSource.AddRange(source);
                        }
                    }
                }

                // Store collection to session
                this.ReportDataList = gridSource;
                #endregion
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        private void FillCostCenterCombo(bool reloadFromDB = true)
        {
            try
            {
                // Initialize combobox
                this.cboCostCenter.Items.Clear();
                this.cboCostCenter.Text = string.Empty;

                List<CostCenterEntity> comboSource = new List<CostCenterEntity>();
                if (this.CostCenterList.Count > 0 && !reloadFromDB)
                {
                    comboSource = this.CostCenterList;
                }
                else
                {
                    string error = string.Empty;
                    string innerError = string.Empty;

                    DALProxy proxy = new DALProxy();
                    var source = proxy.GetCostCenterList(1, ref error, ref innerError);
                    if (source != null && source.Count() > 0)
                    {
                        //comboSource.AddRange(source.ToList());

                        #region Check for Allowed Cost Center
                        if (this.AllowedCostCenterList.Count > 0)
                        {
                            #region Filter list based on allowed cost center
                            foreach (string filter in this.AllowedCostCenterList)
                            {
                                foreach (CostCenterEntity item in source)
                                {
                                    if (item.CostCenter == filter)
                                    {
                                        comboSource.Add(item);
                                    }
                                }
                            }
                            #endregion
                        }
                        else if (this.AllowedCostCenterList.Count == 0 && UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USER_COST_CENTER]) != string.Empty)
                        {
                            #region Filter list based on user's cost center
                            this.AllowedCostCenterList.Add(UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USER_COST_CENTER]));

                            foreach (string filter in this.AllowedCostCenterList)
                            {
                                foreach (CostCenterEntity item in source)
                                {
                                    if (item.CostCenter == filter)
                                    {
                                        comboSource.Add(item);
                                    }
                                }
                            }
                            #endregion
                        }
                        #endregion
                    }
                }

                if (comboSource.Count > 0)
                {
                    #region Add blank item
                    comboSource.Insert(0, new CostCenterEntity()
                    {
                        CostCenter = UIHelper.CONST_COMBO_EMTYITEM_ID,
                        CostCenterName = string.Empty,
                        CostCenterFullName = string.Empty
                    });
                    #endregion
                }

                // Store to session
                this.CostCenterList = comboSource;

                #region Bind data to combobox
                this.cboCostCenter.DataSource = this.CostCenterList;
                this.cboCostCenter.DataTextField = "CostCenterFullName";
                this.cboCostCenter.DataValueField = "CostCenter";
                this.cboCostCenter.DataBind();

                //if (this.cboCostCenter.Items.Count > 0
                //    && !string.IsNullOrEmpty(userCostCenter))
                //{
                //    this.cboCostCenter.SelectedValue = userCostCenter;
                //}
                #endregion
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex);
            }
        }
        #endregion                
    }
}