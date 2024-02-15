using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using GARMCO.AMS.GAP.Utility;
using GARMCO.AMS.TAS.BL.Entities;
using GARMCO.AMS.TAS.UI.Helpers;
using GARMCO.AMS.TAS.UI.Views.Shared;
using Telerik.Web.UI;

namespace GARMCO.AMS.TAS.UI.Views.HRFunctions
{
    public partial class GARMCOCalendar : BaseWebForm, IFormExtension
    {
        #region Enumeration
        private enum ValidationErrorType
        {
            NoError,
            CustomFormError,
            NoDateRange,
            InvalidDateRange
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

        private Dictionary<string, object> CalendarStorage
        {
            get
            {
                Dictionary<string, object> list = Session["CalendarStorage"] as Dictionary<string, object>;
                if (list == null)
                    Session["CalendarStorage"] = list = new Dictionary<string, object>();

                return list;
            }
            set
            {
                Session["CalendarStorage"] = value;
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

        private List<LeaveEntity> HolidayList
        {
            get
            {
                List<LeaveEntity> list = ViewState["HolidayList"] as List<LeaveEntity>;
                if (list == null)
                    ViewState["HolidayList"] = list = new List<LeaveEntity>();

                return list;
            }
            set
            {
                ViewState["HolidayList"] = value;
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

                this.Master.SetPageForm(UIHelper.FormAccessCodes.GARMCOCAL.ToString());

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
                this.Master.FormTitle = UIHelper.PAGE_GARMCO_CALENDAR_TITLE;
                #endregion

                #region Check if user has permission to access the page
                if (!GAPFunction.CheckFormAccess(this.Master.FormAccess, GAPConstants.FormAccessIndex.Retrieve))
                {
                    if (!UIHelper.ConvertObjectToBolean(Session[UIHelper.GARMCO_USER_IS_SPECIAL]))
                    {
                        Response.Redirect(String.Format("{0}?error={1}&pageName={2}", UIHelper.PAGE_ERROR, Convert.ToInt32(UIHelper.PageErrorCodes.NoAccessOnPage), UIHelper.PAGE_GARMCO_CALENDAR_TITLE), true);
                    }
                }
                #endregion

                #region Checks if current user has permission to create, update, delete and print
                //this.btnSearch.Enabled = this.Master.IsRetrieveAllowed;
                #endregion

                #region Set default button
                this.Master.DefaultButton = this.btnSearch.UniqueID;
                #endregion

                #region Restore saved data from local storage
                string formFlag = string.Empty;
                if (this.CalendarStorage.Count > 0)
                {
                    if (this.CalendarStorage.ContainsKey("FormFlag"))
                        formFlag = UIHelper.ConvertObjectToString(this.CalendarStorage["FormFlag"]);
                }
                #endregion

                if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.ShowReport.ToString())
                {
                    #region Show the last form data
                    RestoreDataFromCollection();

                    // Clear data storage
                    Session.Remove("CalendarStorage");
                    #endregion
                }
                else
                {
                    ClearForm();
                    ProcessQueryString();

                    // Initialize controls
                    this.txtYear.Text = DateTime.Now.Year.ToString();

                    // Fill data to the grid
                    this.btnSearch_Click(this.btnSearch, new EventArgs());
                }
            }

            ScriptManager.RegisterStartupScript(this, this.GetType(), "HideScrollbar", "HideScrollbar();", true);
            AddControlsAttribute();
        }
        #endregion

        #region Grid Events
        protected void gridHoliday_PageIndexChanged(object sender, Telerik.Web.UI.GridPageChangedEventArgs e)
        {
            RebindHolidayGrid();
        }

        protected void gridHoliday_PageSizeChanged(object sender, Telerik.Web.UI.GridPageSizeChangedEventArgs e)
        {
            RebindHolidayGrid();
        }

        protected void gridHoliday_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (this.HolidayList.Count > 0)
            {
                this.gridHoliday.DataSource = this.HolidayList;
                this.gridHoliday.DataBind();

                GridSortExpression sortExpr = new GridSortExpression();
                switch (e.OldSortOrder)
                {
                    case GridSortOrder.None:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = GridSortOrder.Descending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;

                    case GridSortOrder.Ascending:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = this.gridHoliday.MasterTableView.AllowNaturalSort ? GridSortOrder.None : GridSortOrder.Descending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;

                    case GridSortOrder.Descending:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = GridSortOrder.Ascending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;
                }

                e.Canceled = true;
                this.gridHoliday.Rebind();
            }
            else
                InitializeHolidayGrid();
        }

        protected void gridHoliday_ItemCommand(object sender, GridCommandEventArgs e)
        {
            if (e.CommandName.Equals(RadGrid.SelectCommandName))
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {

                }
            }
        }

        protected void gridHoliday_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item.ItemType == GridItemType.AlternatingItem || e.Item.ItemType == GridItemType.Item)
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {

                }
            }
        }

        private void RebindHolidayGrid()
        {
            if (this.HolidayList.Count > 0)
            {
                this.gridHoliday.DataSource = this.HolidayList;
                this.gridHoliday.DataBind();

                //Display the record count
                this.lblRecordCount.Text = string.Format("{0} record(s) found", this.HolidayList.Count.ToString("#,###"));
            }
            else
                InitializeHolidayGrid();
        }

        private void InitializeHolidayGrid()
        {
            this.gridHoliday.DataSource = new List<LeaveEntity>();
            this.gridHoliday.DataBind();

            //Display the record count
            this.lblRecordCount.Text = "0 record(s) found";
        }
        #endregion

        #region IFormExtension Interface Implementation
        public void ClearForm()
        {
            #region Reset controls
            this.txtYear.Text = string.Empty;
            this.chkDateRange.Checked = false;
            this.chkDateRange_CheckedChanged(this.chkDateRange, new EventArgs());
            #endregion

            // Clear collections
            KillSessions();

            // Reset datagrid and other controls
            InitializeHolidayGrid();
            this.gridHoliday.CurrentPageIndex = 0;
            this.lblRecordCount.Text = "0 record found";
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
            this.HolidayList.Clear(); ;
            
            // Clear sessions
            ViewState["CustomErrorMsg"] = null;

            // Clear all viewstates
            ViewState.Clear();
        }
        #endregion

        #region Private Methods
        private void RestoreDataFromCollection()
        {
            if (this.CalendarStorage.Count == 0)
                return;

            #region Restore Query String values
            if (this.CalendarStorage.ContainsKey("CallerForm"))
                this.CallerForm = UIHelper.ConvertObjectToString(this.CalendarStorage["CallerForm"]);
            else
                this.CallerForm = string.Empty;
            #endregion

            #region Restore session values
            if (this.CalendarStorage.ContainsKey("HolidayList"))
                this.HolidayList = this.CalendarStorage["HolidayList"] as List<LeaveEntity>;
            else
                this.HolidayList = null;
            #endregion

            #region Restore control values            
            if (this.CalendarStorage.ContainsKey("txtYear"))
                this.txtYear.Text = UIHelper.ConvertObjectToString(this.CalendarStorage["txtYear"]);
            else
                this.txtYear.Text = string.Empty;

            if (this.CalendarStorage.ContainsKey("dtpStartDate"))
                this.dtpStartDate.SelectedDate = UIHelper.ConvertObjectToDate(this.CalendarStorage["dtpStartDate"]);
            else
                this.dtpStartDate.SelectedDate = null;

            if (this.CalendarStorage.ContainsKey("dtpEndDate"))
                this.dtpEndDate.SelectedDate = UIHelper.ConvertObjectToDate(this.CalendarStorage["dtpEndDate"]);
            else
                this.dtpEndDate.SelectedDate = null;
            #endregion
        }

        private void StoreDataToCollection(UIHelper.PagePostBackFlags formFlag)
        {
            this.CalendarStorage.Clear();
            this.CalendarStorage.Add("FormFlag", formFlag.ToString());

            #region Save control values to session
            this.CalendarStorage.Add("txtYear", this.txtYear.Text.Trim());
            this.CalendarStorage.Add("dtpStartDate", this.dtpStartDate.SelectedDate);
            this.CalendarStorage.Add("dtpEndDate", this.dtpEndDate.SelectedDate);
            #endregion

            #region Save Query String values to collection
            this.CalendarStorage.Add("CallerForm", this.CallerForm);
            #endregion

            #region Store session data to collection
            this.CalendarStorage.Add("HolidayList", this.HolidayList);
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
        #endregion

        #region Action Buttons
        protected void btnReset_Click(object sender, EventArgs e)
        {
            ClearForm();

            this.txtYear.Focus();
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            int errorCount = 0;

            try
            {
                int year = UIHelper.ConvertObjectToInt(this.txtYear.Text);
                DateTime? startDate = this.dtpStartDate.SelectedDate;
                DateTime? endDate = this.dtpEndDate.SelectedDate;

                #region Perform data validation
                // Check if date range is specified
                if (this.chkDateRange.Checked)
                {
                    if (startDate == null && endDate == null)
                    {
                        this.txtGeneric.Text = ValidationErrorType.NoDateRange.ToString();
                        this.ErrorType = ValidationErrorType.NoDateRange;
                        this.cusValDateRange.Validate();
                        errorCount++;
                    }
                    else
                    {
                        // Check Date Range
                        if (startDate == null && endDate != null ||
                            startDate != null && endDate == null ||
                            (startDate != null && endDate != null && startDate > endDate))
                        {
                            this.txtGeneric.Text = ValidationErrorType.InvalidDateRange.ToString();
                            this.ErrorType = ValidationErrorType.InvalidDateRange;
                            this.cusValDateRange.Validate();
                            errorCount++;
                        }
                    }
                }

                if (errorCount > 0)
                    return;
                #endregion

                GetHolidays(year, startDate, endDate);
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());                
            }
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
                else if (this.ErrorType == ValidationErrorType.NoDateRange)
                {
                    validator.ErrorMessage = "Please specify Start Date and End Date!";
                    validator.ToolTip = "Please specify Start Date and End Date!";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.InvalidDateRange)
                {
                    validator.ErrorMessage = "The specified date range is invalid. Make sure that Start Date is less than End Date.";
                    validator.ToolTip = "The specified date range is invalid. Make sure that Start Date is less than End Date.";
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

        protected void chkDateRange_CheckedChanged(object sender, EventArgs e)
        {
            if (this.chkDateRange.Checked)
            {
                this.dtpStartDate.Enabled = true;
                this.dtpEndDate.Enabled = true;
                this.txtYear.Text = string.Empty;
                this.txtYear.Enabled = false;
                this.dtpStartDate.Focus();
            }
            else
            {
                this.dtpStartDate.Enabled = false;
                this.dtpEndDate.Enabled = false;
                this.dtpStartDate.SelectedDate = null;
                this.dtpEndDate.SelectedDate = null;
                this.txtYear.Text = DateTime.Now.Year.ToString();
                this.txtYear.Enabled = true;
                this.txtYear.Focus();                
            }
        }
        #endregion

        #region Database Access
        private void GetHolidays(int year, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                // Initialize session
                this.HolidayList = null;

                string error = string.Empty;
                string innerError = string.Empty;

                DALProxy proxy = new DALProxy();
                var rawData = proxy.GetGARMCOCalendar(year, startDate, endDate, ref error, ref innerError);
                if (!string.IsNullOrEmpty(error) || !string.IsNullOrEmpty(innerError))
                {
                    if (!string.IsNullOrEmpty(innerError))
                        throw new Exception(error, new Exception(innerError));
                    else
                        throw new Exception(error);
                }
                else
                {
                    if (rawData != null && rawData.Count() > 0)
                    {
                        // Save to session
                        this.HolidayList.AddRange(rawData.ToList());                                                
                    }
                }

                // Bind data to the grid
                RebindHolidayGrid();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        
    }
}