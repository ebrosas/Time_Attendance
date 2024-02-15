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

namespace GARMCO.AMS.TAS.UI.Views.UserFunctions
{
    public partial class AttendanceDashboard : BaseWebForm, IFormExtension
    {
        #region Enumeration
        private enum ValidationErrorType
        {
            NoError,
            CustomFormError,
            NoAttendanceDate,
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

        private Dictionary<string, object> AttendanceDashboardStorage
        {
            get
            {
                Dictionary<string, object> list = Session["AttendanceDashboardStorage"] as Dictionary<string, object>;
                if (list == null)
                    Session["AttendanceDashboardStorage"] = list = new Dictionary<string, object>();

                return list;
            }
            set
            {
                Session["AttendanceDashboardStorage"] = value;
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

        private List<EmployeeAttendanceEntity> AttendanceList
        {
            get
            {
                List<EmployeeAttendanceEntity> list = ViewState["AttendanceList"] as List<EmployeeAttendanceEntity>;
                if (list == null)
                    ViewState["AttendanceList"] = list = new List<EmployeeAttendanceEntity>();

                return list;
            }
            set
            {
                ViewState["AttendanceList"] = value;
            }
        }

        private List<EmployeeAttendanceEntity> SwipeDetailList
        {
            get
            {
                List<EmployeeAttendanceEntity> list = ViewState["SwipeDetailList"] as List<EmployeeAttendanceEntity>;
                if (list == null)
                    ViewState["SwipeDetailList"] = list = new List<EmployeeAttendanceEntity>();

                return list;
            }
            set
            {
                ViewState["SwipeDetailList"] = value;
            }
        }

        private int CurrentStartRowIndex
        {
            get
            {
                return UIHelper.ConvertObjectToInt(ViewState["CurrentStartRowIndex"]);
            }
            set
            {
                ViewState["CurrentStartRowIndex"] = value;
            }
        }

        private int CurrentMaximumRows
        {
            get
            {
                return UIHelper.ConvertObjectToInt(ViewState["CurrentMaximumRows"]);
            }
            set
            {
                ViewState["CurrentMaximumRows"] = value;
            }
        }

        private int CurrentPageIndex
        {
            get
            {
                return UIHelper.ConvertObjectToInt(ViewState["CurrentPageIndex"]);
            }
            set
            {
                ViewState["CurrentPageIndex"] = value;
            }
        }

        private int CurrentPageSize
        {
            get
            {
                int pageSize = UIHelper.ConvertObjectToInt(ViewState["CurrentPageSize"]);
                if (pageSize == 0)
                    pageSize = this.gridSearchResults.MasterTableView.PageSize;

                return pageSize;
            }
            set
            {
                ViewState["CurrentPageSize"] = value;
            }
        }

        private bool ReloadGridData
        {
            get
            {
                return UIHelper.ConvertObjectToBolean(ViewState["ReloadGridData"]);
            }
            set
            {
                ViewState["ReloadGridData"] = value;
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

        private List<CostCenterEntity> ManagedCostCenterList
        {
            get
            {
                List<CostCenterEntity> list = ViewState["ManagedCostCenterList"] as List<CostCenterEntity>;
                if (list == null)
                    ViewState["ManagedCostCenterList"] = list = new List<CostCenterEntity>();

                return list;
            }
            set
            {
                ViewState["ManagedCostCenterList"] = value;
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
                {
                    Response.Redirect(UIHelper.PAGE_SESSION_TIMEOUT_PAGE, false);                    
                }

                this.Master.SetPageForm(UIHelper.FormAccessCodes.ATTENDDASH.ToString());

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
                this.Master.FormTitle = UIHelper.PAGE_ATTENDANCE_DASHBOARD_TITLE;
                #endregion

                #region Check if user has permission to access the page
                if (!GAPFunction.CheckFormAccess(this.Master.FormAccess, GAPConstants.FormAccessIndex.Retrieve))
                {
                    if (!UIHelper.ConvertObjectToBolean(Session[UIHelper.GARMCO_USER_IS_SPECIAL]))
                    {
                        Response.Redirect(String.Format("{0}?error={1}&pageName={2}", UIHelper.PAGE_ERROR, Convert.ToInt32(UIHelper.PageErrorCodes.NoAccessOnPage), UIHelper.PAGE_ATTENDANCE_DASHBOARD_TITLE), true);
                    }
                }
                #endregion

                #region Checks if current user has permission to create, update, delete and print
                //this.btnNew.Enabled = this.Master.IsCreateAllowed;
                //this.btnDelete.Enabled = this.Master.IsDeleteAllowed;
                #endregion

                #region Set default button
                this.Master.DefaultButton = this.btnSearch.UniqueID;
                #endregion

                #region Restore saved data from local storage
                string formFlag = string.Empty;
                if (this.AttendanceDashboardStorage.Count > 0)
                {
                    if (this.AttendanceDashboardStorage.ContainsKey("FormFlag"))
                        formFlag = UIHelper.ConvertObjectToString(this.AttendanceDashboardStorage["FormFlag"]);
                }
                #endregion

                if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.RedirectToOtherPage.ToString())
                {
                    #region Show the last form data
                    RestoreDataFromCollection();
                    ProcessQueryString();

                    // Clear data storage
                    Session.Remove("AttendanceDashboardStorage");

                    // Check if need to refresh data in the grid
                    if (this.ReloadGridData)
                        this.btnSearch_Click(this.btnSearch, new EventArgs());
                    #endregion
                }
                else
                {
                    ClearForm();
                    ProcessQueryString();
                    FillComboData();
                    GetEmployeeManagedCostCenter();

                    #region Check if user has permission to display employee photos
                    bool showPhoto = false;
                    try
                    {
                        #region Check if current user is member of the allowed cost center who can view employee photos
                        string[] costCenterArray = ConfigurationManager.AppSettings["EnablePhotoCostCenters"].Split(',');
                        if (costCenterArray != null)
                        {
                            foreach (string item in costCenterArray)
                            {
                                if (item == costCenter)
                                {
                                    showPhoto = true;
                                    break;
                                }
                            }
                        }
                        #endregion

                        #region Check if current user is member of the System Administrators group
                        if (!showPhoto)
                        {
                            string[] adminArray = ConfigurationManager.AppSettings["SystemAdministrators"].Split(',');
                            if (adminArray != null)
                            {
                                foreach (string item in adminArray)
                                {
                                    if (item == userID)
                                    {
                                        showPhoto = true;
                                        break;
                                    }
                                }
                            }
                        }
                        #endregion
                    }
                    catch (Exception)
                    {
                    }

                    if (!showPhoto)
                    {
                        this.chkShowPhoto.Visible = false;
                        this.tdShowPhotoTitle.Style[HtmlTextWriterStyle.Display] = "none";
                        this.tdShowPhotoCheckbox.Style[HtmlTextWriterStyle.Display] = "none";
                    }
                    else
                    {
                        this.chkShowPhoto.Visible = true;
                        this.tdShowPhotoTitle.Style[HtmlTextWriterStyle.Display] = string.Empty;
                        this.tdShowPhotoCheckbox.Style[HtmlTextWriterStyle.Display] = string.Empty;
                    }
                    #endregion

                    #region Initialize controls
                    this.dtpAttendanceDate.MaxDate = DateTime.Now;
                    this.dtpAttendanceDate.SelectedDate = DateTime.Now;
                    #endregion

                    // Fill data to the grid
                    this.btnSearch_Click(this.btnSearch, new EventArgs());
                }
            }

            ScriptManager.RegisterStartupScript(this, this.GetType(), "HideScrollbar", "HideScrollbar();", true);
            AddControlsAttribute();
        }
        #endregion

        #region Grid Events

        #region Attendance Grid                 
        protected void gridSearchResults_PageIndexChanged(object sender, Telerik.Web.UI.GridPageChangedEventArgs e)
        {
            RebindDataToAttendanceGrid();
        }

        protected void gridSearchResults_PageSizeChanged(object sender, Telerik.Web.UI.GridPageSizeChangedEventArgs e)
        {
            RebindDataToAttendanceGrid();
        }

        protected void gridSearchResults_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (this.AttendanceList.Count > 0)
            {
                this.gridSearchResults.DataSource = this.AttendanceList;
                this.gridSearchResults.DataBind();

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
                        sortExpr.SortOrder = this.gridSearchResults.MasterTableView.AllowNaturalSort ? GridSortOrder.None : GridSortOrder.Descending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;

                    case GridSortOrder.Descending:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = GridSortOrder.Ascending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;
                }

                e.Canceled = true;
                this.gridSearchResults.Rebind();
            }
            else
                InitializeDataToGrid();
        }

        protected void gridSearchResults_ItemCommand(object sender, GridCommandEventArgs e)
        {
            if (e.CommandName.Equals(RadGrid.SelectCommandName))
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {
                }
            }
        }

        protected void gridSearchResults_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item.ItemType == GridItemType.AlternatingItem || e.Item.ItemType == GridItemType.Item)
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {
                    #region Set attendance remarks                                        
                    int empNo = UIHelper.ConvertObjectToInt(item["EmpNo"].Text);
                    string status = UIHelper.ConvertObjectToString(item["InOutStatus"].Text);

                    if (status == UIHelper.CONST_ARRIVAL_LATE)
                    {
                        #region Process late arrival     
                        item["AttendanceRemarks"].ForeColor = System.Drawing.Color.Red;
                        item["AttendanceRemarks"].Font.Bold = true;

                        #region Show the arrival time in the tooltip
                        if (empNo > 0)
                        {
                            EmployeeAttendanceEntity attendanceRecord = this.AttendanceList
                                .Where(a => a.EmpNo == empNo)
                                .FirstOrDefault();
                            if (attendanceRecord != null)
                            {
                                item["AttendanceRemarks"].ToolTip = attendanceRecord.FirstTimeIn.HasValue
                                    ? string.Format("Arrived at {0}", Convert.ToDateTime(attendanceRecord.FirstTimeIn).ToString("h:mm:ss tt"))
                                    : string.Empty;
                            }
                        }
                        #endregion
                        #endregion
                    }
                    else if (status == UIHelper.CONST_LEFT_EARLY)
                    {
                        #region Process early leaving 
                        item["AttendanceRemarks"].ForeColor = System.Drawing.Color.Red;
                        item["AttendanceRemarks"].Font.Bold = true;

                        #region Show the arrival time in the tooltip
                        if (empNo > 0)
                        {
                            EmployeeAttendanceEntity attendanceRecord = this.AttendanceList
                                .Where(a => a.EmpNo == empNo)
                                .FirstOrDefault();
                            if (attendanceRecord != null)
                            {
                                item["AttendanceRemarks"].ToolTip = attendanceRecord.LastTimeOut.HasValue
                                    ? string.Format("Left at {0}", Convert.ToDateTime(attendanceRecord.LastTimeOut).ToString("h:mm:ss tt"))
                                    : string.Empty;
                            }
                        }
                        #endregion
                        #endregion
                    }
                    #endregion

                    #region Enable or disable the Employee Name link
                    LinkButton lnkEmpName = (LinkButton)item["EmpName"].FindControl("lnkEmpName");   
                    if (lnkEmpName != null)
                    {
                        int currentUserEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);
                        if (currentUserEmpNo == empNo)
                            lnkEmpName.Enabled = true;
                        else
                        {
                            if (this.AllowedCostCenterList.Count > 0)
                            {
                                EmployeeAttendanceEntity attendanceRow = this.AttendanceList.Where(a => a.EmpNo == empNo).FirstOrDefault();
                                if (attendanceRow != null)
                                {
                                    string searchKey = Master.AllowedCostCenterList.Find(x => x.Contains(attendanceRow.CostCenter));
                                    lnkEmpName.Enabled = !string.IsNullOrEmpty(searchKey);
                                }
                            }
                            else
                            {
                                if (this.ManagedCostCenterList.Count > 0)
                                {
                                    #region Check if user is a Superintendent or Cost Center Manager
                                    EmployeeAttendanceEntity attendanceRow = this.AttendanceList.Where(a => a.EmpNo == empNo).FirstOrDefault();
                                    if (attendanceRow != null)
                                    {
                                        CostCenterEntity managedCostCenter = this.ManagedCostCenterList.Where(x => x.CostCenter == attendanceRow.CostCenter).FirstOrDefault();
                                        lnkEmpName.Enabled = managedCostCenter != null;
                                    }
                                    #endregion
                                }
                                else
                                {
                                    #region Check if current user is the Supervisor of some employees
                                    int supervisorEmpNo = UIHelper.ConvertObjectToInt(item["SupervisorEmpNo"].Text);
                                    lnkEmpName.Enabled = currentUserEmpNo == supervisorEmpNo;
                                    #endregion
                                }
                            }
                        }

                        if (lnkEmpName.Enabled)
                            lnkEmpName.ForeColor = System.Drawing.Color.Blue;
                        else
                            lnkEmpName.ForeColor = System.Drawing.Color.Gray;
                    }
                    #endregion

                    #region Set Employee Photo tooltip
                    //LinkButton imgPhoto = (LinkButton)item["EmpName"].FindControl("lnkEmpName");
                    //if (lnkEmpName != null)
                    //{
                    //}
                    #endregion
                }
            }
        }

        private void RebindDataToAttendanceGrid()
        {
            if (this.AttendanceList.Count > 0)
            {
                this.gridSearchResults.DataSource = this.AttendanceList;
                this.gridSearchResults.DataBind();

                //Display the record count
                this.lblRecordCount.Text = string.Format("Found {0} attendance records:", this.AttendanceList.Count.ToString("#,###"));
            }
            else
                InitializeDataToGrid();
        }

        private void InitializeDataToGrid()
        {
            this.gridSearchResults.DataSource = new List<EmployeeAttendanceEntity>();
            this.gridSearchResults.DataBind();

            this.lblRecordCount.Text = "Found 0 attendance record:";
        }
        #endregion

        #region Swipe Details Grid                 
        protected void gridSwipeDetail_PageIndexChanged(object sender, Telerik.Web.UI.GridPageChangedEventArgs e)
        {
            RebindDataToSwipeDetailsGrid();
        }

        protected void gridSwipeDetail_PageSizeChanged(object sender, Telerik.Web.UI.GridPageSizeChangedEventArgs e)
        {
            RebindDataToSwipeDetailsGrid();
        }

        protected void gridSwipeDetail_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (this.SwipeDetailList.Count > 0)
            {
                this.gridSwipeDetail.DataSource = this.SwipeDetailList;
                this.gridSwipeDetail.DataBind();

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
                        sortExpr.SortOrder = this.gridSwipeDetail.MasterTableView.AllowNaturalSort ? GridSortOrder.None : GridSortOrder.Descending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;

                    case GridSortOrder.Descending:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = GridSortOrder.Ascending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;
                }

                e.Canceled = true;
                this.gridSwipeDetail.Rebind();
            }
            else
                InitializeSwipeDetailsGrid();
        }

        protected void gridSwipeDetail_ItemCommand(object sender, GridCommandEventArgs e)
        {
            if (e.CommandName.Equals(RadGrid.SelectCommandName))
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {
                }
            }
        }

        protected void gridSwipeDetail_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item.ItemType == GridItemType.AlternatingItem || e.Item.ItemType == GridItemType.Item)
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {

                }
            }
        }

        private void RebindDataToSwipeDetailsGrid()
        {
            if (this.SwipeDetailList.Count > 0)
            {
                this.gridSwipeDetail.DataSource = this.SwipeDetailList;
                this.gridSwipeDetail.DataBind();                                
            }
            else
                InitializeSwipeDetailsGrid();

            // Show the grid
            this.gridSwipeDetail.Visible = true;
        }

        private void InitializeSwipeDetailsGrid()
        {
            this.gridSwipeDetail.DataSource = new List<EmployeeAttendanceEntity>();
            this.gridSwipeDetail.DataBind();                        
        }
        #endregion

        #endregion

        #region Action Buttons
        protected void btnReset_Click(object sender, EventArgs e)
        {
            #region Clear the form
            this.dtpAttendanceDate.SelectedDate = DateTime.Now;
            this.txtEmpName.Text = string.Empty;
            //this.cboCostCenter.Text = string.Empty;
            //this.cboCostCenter.SelectedIndex = -1;
            this.chkShowPhoto.Checked = false;
            this.chkShowPhoto_CheckedChanged(this.chkShowPhoto, new EventArgs());

            this.chkEnablePaging.Checked = false;
            this.chkEnablePaging_CheckedChanged(this.chkEnablePaging, new EventArgs());

            // Select the employee's cost center
            if (this.cboCostCenter.Items.Count > 0)
                this.cboCostCenter.SelectedValue = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USER_COST_CENTER]);

            // Cler collections
            this.AttendanceList.Clear();
            this.SwipeDetailList.Clear();

            // Clear sessions
            ViewState["CustomErrorMsg"] = null;
            ViewState["CurrentStartRowIndex"] = null;
            ViewState["CurrentMaximumRows"] = null;
            ViewState["CurrentPageIndex"] = null;
            ViewState["CurrentPageSize"] = null;

            // Reset datagrid and other controls
            InitializeDataToGrid();
            InitializeSwipeDetailsGrid();

            this.gridSearchResults.CurrentPageIndex = 0;
            this.lblRecordCount.Text = "Found 0 attendance record:";

            // Hide the swipe details grid
            this.gridSwipeDetail.Visible = false;
            #endregion

            // Reload the data
            this.btnSearch_Click(this.btnSearch, new EventArgs());
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            #region Perform Data Validation
            int errorCount = 0;

            if (errorCount > 0)
            {
                // Set focus to the top panel
                Page.SetFocus(this.lnkMoveUp.ClientID);
                return;
            }

            #endregion

            // Reset page index
            this.gridSearchResults.CurrentPageIndex = 0;
            this.CurrentPageIndex = this.gridSearchResults.CurrentPageIndex + 1;
            this.CurrentPageSize = this.gridSearchResults.PageSize;

            // Reset swipe details grid
            this.gridSwipeDetail.Visible = false;
            InitializeSwipeDetailsGrid();

            GetEmployeeAttendance(true);
        }

        protected void btnRebind_Click(object sender, EventArgs e)
        {
            RebindDataToAttendanceGrid();
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
                else if (this.ErrorType == ValidationErrorType.NoAttendanceDate)
                {
                    validator.ErrorMessage = "Date is required.";
                    validator.ToolTip = "Date is required.";
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

        protected void cboCostCenter_SelectedIndexChanged(object sender, RadComboBoxSelectedIndexChangedEventArgs e)
        {
            this.btnSearch_Click(this.btnSearch, new EventArgs());
        }

        protected void lnkEmpName_Click(object sender, EventArgs e)
        {
            try
            {
                LinkButton lnkEmpName = sender as LinkButton;
                GridDataItem item = lnkEmpName.NamingContainer as GridDataItem;

                if (item != null)
                {
                    // Get data key value
                    int empNo = UIHelper.ConvertObjectToInt(this.gridSearchResults.MasterTableView.Items[item.ItemIndex].GetDataKeyValue("EmpNo"));
                    DateTime? attendanceDate = UIHelper.ConvertObjectToDate(item["AttendanceDate"].Text);

                    // Get the swipe details
                    GetEmployeeSwipeDetails(empNo, attendanceDate);
                }
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void chkShowPhoto_CheckedChanged(object sender, EventArgs e)
        {
            GridColumn imageColumn = this.gridSearchResults.MasterTableView.RenderColumns.Where(a => a.UniqueName == "EmployeeImagePath").FirstOrDefault();
            if (imageColumn != null)
            {
                if (!this.chkShowPhoto.Checked)
                    imageColumn.Visible = false;
                else
                {
                    RebindDataToAttendanceGrid();
                    imageColumn.Visible = true;
                }
            }
        }

        protected void chkEnablePaging_CheckedChanged(object sender, EventArgs e)
        {            
            if (this.chkEnablePaging.Checked)
            {
                this.gridSearchResults.AllowPaging = true;
                this.gridSearchResults.Height = Unit.Empty;
            }
            else
            {
                this.gridSearchResults.AllowPaging = false;
                this.gridSearchResults.Height = Unit.Pixel(500);
            }

            RebindDataToAttendanceGrid();
        }
        #endregion

        #region IFormExtension Interface Implementation
        public void ClearForm()
        {
            #region Reset controls
            this.txtEmpName.Text = string.Empty;
            this.dtpAttendanceDate.SelectedDate = null;
            this.cboCostCenter.Text = string.Empty;
            this.cboCostCenter.SelectedIndex = -1;

            this.chkShowPhoto.Checked = false;
            this.chkShowPhoto_CheckedChanged(this.chkShowPhoto, new EventArgs());

            this.chkEnablePaging.Checked = false;
            this.chkEnablePaging_CheckedChanged(this.chkEnablePaging, new EventArgs());
            #endregion

            // Clear collections
            KillSessions();

            // Reset the grid
            this.gridSearchResults.VirtualItemCount = 1;
            this.gridSearchResults.CurrentPageIndex = 0;
            this.CurrentPageIndex = this.gridSearchResults.CurrentPageIndex + 1;
            this.CurrentPageSize = this.gridSearchResults.PageSize;

            InitializeDataToGrid();
            InitializeSwipeDetailsGrid();

            // Hide the swipe details grid
            this.gridSwipeDetail.Visible = false;
        }

        public void AddControlsAttribute()
        {

        }

        public void ProcessQueryString()
        {
            this.CallerForm = UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_CALLER_FORM_KEY]);
            this.ReloadGridData = UIHelper.ConvertObjectToBolean(Request.QueryString[UIHelper.QUERY_STRING_RELOAD_DATA_KEY]);
        }

        public void KillSessions()
        {
            // Cler collections
            this.AttendanceList.Clear();
            this.SwipeDetailList.Clear();
            this.CostCenterList.Clear();
            this.ManagedCostCenterList.Clear();

            // Clear sessions
            ViewState["CustomErrorMsg"] = null;
            ViewState["CurrentStartRowIndex"] = null;
            ViewState["CurrentMaximumRows"] = null;
            ViewState["CurrentPageIndex"] = null;
            ViewState["CurrentPageSize"] = null;
            ViewState["ReloadGridData"] = null;
            ViewState["CallerForm"] = null;
            ViewState["ReloadGridData"] = null;

            // Clear all viewstates
            ViewState.Clear();

            Session["SelectedReasonOfAbsence"] = null;
            Session.Remove("SelectedReasonOfAbsence");
        }
        #endregion

        #region Private Methods
        private void RestoreDataFromCollection()
        {
            if (this.AttendanceDashboardStorage.Count == 0)
                return;

            #region Restore Query String values
            if (this.AttendanceDashboardStorage.ContainsKey("CallerForm"))
                this.CallerForm = UIHelper.ConvertObjectToString(this.AttendanceDashboardStorage["CallerForm"]);
            else
                this.CallerForm = string.Empty;

            if (this.AttendanceDashboardStorage.ContainsKey("ReloadGridData"))
                this.ReloadGridData = UIHelper.ConvertObjectToBolean(this.AttendanceDashboardStorage["ReloadGridData"]);
            else
                this.ReloadGridData = false;
            #endregion

            #region Restore session values
            if (this.AttendanceDashboardStorage.ContainsKey("CurrentStartRowIndex"))
                this.CurrentStartRowIndex = UIHelper.ConvertObjectToInt(this.AttendanceDashboardStorage["CurrentStartRowIndex"]);
            else
                this.CurrentStartRowIndex = 0;

            if (this.AttendanceDashboardStorage.ContainsKey("CurrentMaximumRows"))
                this.CurrentMaximumRows = UIHelper.ConvertObjectToInt(this.AttendanceDashboardStorage["CurrentMaximumRows"]);
            else
                this.CurrentMaximumRows = 0;

            if (this.AttendanceDashboardStorage.ContainsKey("CurrentPageIndex"))
                this.CurrentPageIndex = UIHelper.ConvertObjectToInt(this.AttendanceDashboardStorage["CurrentPageIndex"]);
            else
                this.CurrentPageIndex = 0;

            if (this.AttendanceDashboardStorage.ContainsKey("CurrentPageSize"))
                this.CurrentPageSize = UIHelper.ConvertObjectToInt(this.AttendanceDashboardStorage["CurrentPageSize"]);
            else
                this.CurrentPageSize = 0;

            if (this.AttendanceDashboardStorage.ContainsKey("AttendanceList"))
                this.AttendanceList = this.AttendanceDashboardStorage["AttendanceList"] as List<EmployeeAttendanceEntity>;
            else
                this.AttendanceList = null;

            if (this.AttendanceDashboardStorage.ContainsKey("SwipeDetailList"))
                this.SwipeDetailList = this.AttendanceDashboardStorage["SwipeDetailList"] as List<EmployeeAttendanceEntity>;
            else
                this.SwipeDetailList = null;

            if (this.AttendanceDashboardStorage.ContainsKey("CostCenterList"))
                this.CostCenterList = this.AttendanceDashboardStorage["CostCenterList"] as List<CostCenterEntity>;
            else
                this.CostCenterList = null;

            if (this.AttendanceDashboardStorage.ContainsKey("ManagedCostCenterList"))
                this.ManagedCostCenterList = this.AttendanceDashboardStorage["ManagedCostCenterList"] as List<CostCenterEntity>;
            else
                this.ManagedCostCenterList = null;

            FillComboData(false);
            #endregion

            #region Restore control values            
            if (this.AttendanceDashboardStorage.ContainsKey("txtEmpName"))
                this.txtEmpName.Text = UIHelper.ConvertObjectToString(this.AttendanceDashboardStorage["txtEmpName"]);
            else
                this.txtEmpName.Text = string.Empty;

            if (this.AttendanceDashboardStorage.ContainsKey("cboCostCenter"))
                this.cboCostCenter.SelectedValue = UIHelper.ConvertObjectToString(this.AttendanceDashboardStorage["cboCostCenter"]);
            else
                this.cboCostCenter.SelectedValue = string.Empty;

            if (this.AttendanceDashboardStorage.ContainsKey("dtpAttendanceDate"))
                this.dtpAttendanceDate.SelectedDate = UIHelper.ConvertObjectToDate(this.AttendanceDashboardStorage["dtpAttendanceDate"]);
            else
                this.dtpAttendanceDate.SelectedDate = null;

            if (this.AttendanceDashboardStorage.ContainsKey("chkShowPhoto"))
                this.chkShowPhoto.Checked = UIHelper.ConvertObjectToBolean(this.AttendanceDashboardStorage["chkShowPhoto"]);
            else
                this.chkShowPhoto.Checked = false;

            this.chkShowPhoto_CheckedChanged(this.chkShowPhoto, new EventArgs());

            if (this.AttendanceDashboardStorage.ContainsKey("chkEnablePaging"))
                this.chkEnablePaging.Checked = UIHelper.ConvertObjectToBolean(this.AttendanceDashboardStorage["chkEnablePaging"]);
            else
                this.chkEnablePaging.Checked = false;

            this.chkEnablePaging_CheckedChanged(this.chkEnablePaging, new EventArgs());
            #endregion

            // Refresh the grid
            RebindDataToAttendanceGrid();
            RebindDataToSwipeDetailsGrid();

            // Set the grid attributes
            this.gridSearchResults.CurrentPageIndex = this.CurrentPageIndex;
            this.gridSearchResults.MasterTableView.CurrentPageIndex = this.CurrentPageIndex;
            this.gridSearchResults.MasterTableView.PageSize = this.CurrentPageSize;
            this.gridSearchResults.MasterTableView.DataBind();
        }

        private void StoreDataToCollection(UIHelper.PagePostBackFlags formFlag)
        {
            this.AttendanceDashboardStorage.Clear();
            this.AttendanceDashboardStorage.Add("FormFlag", formFlag.ToString());

            #region Save control values to session
            this.AttendanceDashboardStorage.Add("txtEmpName", this.txtEmpName.Text.Trim());
            this.AttendanceDashboardStorage.Add("cboCostCenter", this.cboCostCenter.SelectedValue);
            this.AttendanceDashboardStorage.Add("dtpAttendanceDate", this.dtpAttendanceDate.SelectedDate);
            this.AttendanceDashboardStorage.Add("chkShowPhoto", this.chkShowPhoto.Checked);
            this.AttendanceDashboardStorage.Add("chkEnablePaging", this.chkEnablePaging.Checked);
            #endregion

            #region Save Query String values to collection
            this.AttendanceDashboardStorage.Add("CallerForm", this.CallerForm);
            this.AttendanceDashboardStorage.Add("ReloadGridData", this.ReloadGridData);
            #endregion

            #region Store session data to collection
            this.AttendanceDashboardStorage.Add("CurrentStartRowIndex", this.CurrentStartRowIndex);
            this.AttendanceDashboardStorage.Add("CurrentMaximumRows", this.CurrentMaximumRows);
            this.AttendanceDashboardStorage.Add("CurrentPageIndex", this.CurrentPageIndex);
            this.AttendanceDashboardStorage.Add("CurrentPageSize", this.CurrentPageSize);
            this.AttendanceDashboardStorage.Add("AttendanceList", this.AttendanceList);
            this.AttendanceDashboardStorage.Add("SwipeDetailList", this.SwipeDetailList);
            this.AttendanceDashboardStorage.Add("CostCenterList", this.CostCenterList);
            this.AttendanceDashboardStorage.Add("ManagedCostCenterList", this.ManagedCostCenterList);
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
        private void GetEmployeeAttendance(bool reloadDataFromDB = false)
        {
            try
            {
                #region Initialize variables         
                string empName = this.txtEmpName.Text.Trim();      
                string costCenter = this.cboCostCenter.SelectedValue;
                DateTime? attendanceDate = this.dtpAttendanceDate.SelectedDate;

                // Initialize record count
                this.lblRecordCount.Text = "Found 0 attendance record:";
                this.gridSearchResults.VirtualItemCount = 1;
                #endregion

                #region Fill data to the collection
                List<EmployeeAttendanceEntity> gridSource = new List<EmployeeAttendanceEntity>();
                if (!reloadDataFromDB)
                {
                    gridSource = this.AttendanceList;
                }
                else
                {
                    string error = string.Empty;
                    string innerError = string.Empty;
                    //string imageRootPath = UIHelper.ConvertObjectToString(ConfigurationManager.AppSettings["ImageRootPath"]);
                    string imageRootPath = UIHelper.ConvertObjectToString(ConfigurationManager.AppSettings["EmpPhotoVirtualFolder"]);

                    DALProxy proxy = new DALProxy();
                    var source = proxy.GetEmployeeAttendance(empName, costCenter, attendanceDate, imageRootPath, ref error, ref innerError);
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

                            #region Filter records based on the allowed cost center
                            if (this.AllowedCostCenterList.Count > 0)
                            {
                                foreach (EmployeeAttendanceEntity item in gridSource)
                                {
                                    string searchKey = this.AllowedCostCenterList.Where(a => a.Contains(item.CostCenter)).FirstOrDefault();
                                    if (string.IsNullOrEmpty(searchKey))
                                    {
                                        if (item.InOutStatus == "l")
                                        //|| attendance.StatusCode == "im")
                                        {
                                            // Change icon to "Arrival - Normal" for late and manual swipe
                                            item.StatusIconPath = UIHelper.CONST_ARRIVAL_NORMAL_ICON;
                                            item.StatusIconNotes = UIHelper.CONST_ARRIVAL_NORMAL_NOTES;

                                            // Remove the remarks
                                            item.AttendanceRemarks = string.Empty;
                                        }
                                        else if (item.InOutStatus == "e")
                                        {
                                            // Change icon to "Left - Normal" 
                                            item.StatusIconPath = UIHelper.CONST_LEFT_NORMAL_ICON;
                                            item.StatusIconNotes = UIHelper.CONST_LEFT_NORMAL_NOTES;

                                            // Remove the remarks
                                            item.AttendanceRemarks = string.Empty;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (this.ManagedCostCenterList.Count > 0)
                                {
                                    #region Check if user is a Superintendent or Cost Center Manager
                                    foreach (EmployeeAttendanceEntity item in gridSource)
                                    {
                                        CostCenterEntity managedCostCenter = this.ManagedCostCenterList.Where(a => a.CostCenter == item.CostCenter).FirstOrDefault();
                                        if (managedCostCenter == null)
                                        {
                                            if (item.InOutStatus == "l")
                                            //|| attendance.StatusCode == "im")
                                            {
                                                // Change icon to "Arrival - Normal" for late and manual swipe
                                                item.StatusIconPath = UIHelper.CONST_ARRIVAL_NORMAL_ICON;
                                                item.StatusIconNotes = UIHelper.CONST_ARRIVAL_NORMAL_NOTES;

                                                // Remove the remarks
                                                item.AttendanceRemarks = string.Empty;
                                            }
                                            else if (item.InOutStatus == "e")
                                            {
                                                // Change icon to "Left - Normal" 
                                                item.StatusIconPath = UIHelper.CONST_LEFT_NORMAL_ICON;
                                                item.StatusIconNotes = UIHelper.CONST_LEFT_NORMAL_NOTES;

                                                // Remove the remarks
                                                item.AttendanceRemarks = string.Empty;
                                            }
                                        }
                                    }
                                    #endregion
                                }
                                else
                                {
                                    #region Check if current user is the Supervisor of some employees
                                    int currentUserEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);
                                    foreach (EmployeeAttendanceEntity item in gridSource)
                                    {
                                        // Skip security for current user
                                        if (item.EmpNo == currentUserEmpNo)
                                            continue;

                                        if (item.SupervisorEmpNo != currentUserEmpNo)
                                        {
                                            if (item.InOutStatus == "l")
                                            //|| attendance.StatusCode == "im")
                                            {
                                                // Change icon to "Arrival - Normal" for late and manual swipe
                                                item.StatusIconPath = UIHelper.CONST_ARRIVAL_NORMAL_ICON;
                                                item.StatusIconNotes = UIHelper.CONST_ARRIVAL_NORMAL_NOTES;

                                                // Remove the remarks
                                                item.AttendanceRemarks = string.Empty;
                                            }
                                            else if (item.InOutStatus == "e")
                                            {
                                                // Change icon to "Left - Normal" 
                                                item.StatusIconPath = UIHelper.CONST_LEFT_NORMAL_ICON;
                                                item.StatusIconNotes = UIHelper.CONST_LEFT_NORMAL_NOTES;

                                                // Remove the remarks
                                                item.AttendanceRemarks = string.Empty;
                                            }
                                        }
                                    }
                                    #endregion
                                }
                            }
                            #endregion
                        }
                    }
                }

                // Store collection to session
                this.AttendanceList = gridSource;
                #endregion

                // Show data in the grid
                RebindDataToAttendanceGrid();
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        private void GetEmployeeSwipeDetails(int empNo, DateTime? attendanceDate)
        {
            try
            {
                string error = string.Empty;
                string innerError = string.Empty;
                List<EmployeeAttendanceEntity> gridSource = new List<EmployeeAttendanceEntity>();

                // Initialize grid
                this.gridSwipeDetail.VirtualItemCount = 1;

                #region Fill data to the collection
                DALProxy proxy = new DALProxy();
                var source = proxy.GetSwipeDetails(empNo, attendanceDate , ref error, ref innerError);
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

                // Store collection to session
                this.SwipeDetailList = gridSource;
                #endregion

                // Show data to the grid
                RebindDataToSwipeDetailsGrid();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void FillCostCenterComboOld()
        {
            DataView dv = this.objCostCenter.Select() as DataView;
            if (dv == null || dv.Count == 0)
                return;

            DataRow[] source = new DataRow[dv.Count];
            dv.Table.Rows.CopyTo(source, 0);
            EmployeeDAL.CostCenterDataTable filteredDT = new EmployeeDAL.CostCenterDataTable();

            #region Add default selection item
            EmployeeDAL.CostCenterRow defaultRow = filteredDT.NewCostCenterRow();
            defaultRow.CostCenter = String.Empty;
            defaultRow.CostCenterName = "Please select a Cost Center...";
            defaultRow.Company = String.Empty;
            defaultRow.SuperintendentNo = 0;
            defaultRow.SuperintendentName = String.Empty;
            defaultRow.ManagerNo = 0;
            defaultRow.ManagerName = String.Empty;

            // Add record to the collection
            filteredDT.Rows.Add(defaultRow);
            #endregion

            if (this.AllowedCostCenterList.Count > 0)
            {
                #region Filter list based on allowed cost center
                foreach (string filter in this.AllowedCostCenterList)
                {
                    DataRow[] rows = source.Where(d => UIHelper.ConvertObjectToString(d["CostCenter"]) == filter).ToArray();
                    foreach (DataRow rw in rows)
                    {
                        EmployeeDAL.CostCenterRow row = filteredDT.NewCostCenterRow();
                        row.CostCenter = UIHelper.ConvertObjectToString(rw["CostCenter"]);
                        row.CostCenterName = UIHelper.ConvertObjectToString(rw["CostCenterName"]);
                        row.Company = UIHelper.ConvertObjectToString(rw["Company"]);
                        row.SuperintendentNo = UIHelper.ConvertObjectToInt(rw["Company"]);
                        row.SuperintendentName = UIHelper.ConvertObjectToString(rw["Company"]);
                        row.ManagerNo = UIHelper.ConvertObjectToInt(rw["Company"]);
                        row.ManagerName = UIHelper.ConvertObjectToString(rw["Company"]);

                        // Add record to the collection
                        filteredDT.Rows.Add(row);
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
                    DataRow[] rows = source.Where(d => UIHelper.ConvertObjectToString(d["CostCenter"]) == filter).ToArray();
                    foreach (DataRow rw in rows)
                    {
                        EmployeeDAL.CostCenterRow row = filteredDT.NewCostCenterRow();
                        row.CostCenter = UIHelper.ConvertObjectToString(rw["CostCenter"]);
                        row.CostCenterName = UIHelper.ConvertObjectToString(rw["CostCenterName"]);
                        row.Company = UIHelper.ConvertObjectToString(rw["Company"]);
                        row.SuperintendentNo = UIHelper.ConvertObjectToInt(rw["Company"]);
                        row.SuperintendentName = UIHelper.ConvertObjectToString(rw["Company"]);
                        row.ManagerNo = UIHelper.ConvertObjectToInt(rw["Company"]);
                        row.ManagerName = UIHelper.ConvertObjectToString(rw["Company"]);

                        // Add record to the collection
                        filteredDT.Rows.Add(row);
                    }
                }                
                #endregion
            }
            else
            {
                #region No filtering for cost center
                //foreach (DataRow rw in source)
                //{
                //    EmployeeDAL.CostCenterRow row = filteredDT.NewCostCenterRow();
                //    row.CostCenter = UIHelper.ConvertObjectToString(rw["CostCenter"]);
                //    row.CostCenterName = UIHelper.ConvertObjectToString(rw["CostCenterName"]);
                //    row.Company = UIHelper.ConvertObjectToString(rw["Company"]);
                //    row.SuperintendentNo = UIHelper.ConvertObjectToInt(rw["Company"]);
                //    row.SuperintendentName = UIHelper.ConvertObjectToString(rw["Company"]);
                //    row.ManagerNo = UIHelper.ConvertObjectToInt(rw["Company"]);
                //    row.ManagerName = UIHelper.ConvertObjectToString(rw["Company"]);

                //    // Add record to the collection
                //    filteredDT.Rows.Add(row);
                //}

                ////Set the flag
                //enableEmpSearch = true;
                #endregion
            }

            if (filteredDT.Rows.Count > 0)
            {
                this.cboCostCenter.DataTextField = "CostCenter";
                this.cboCostCenter.DataValueField = "CostCenter";
                this.cboCostCenter.DataSource = filteredDT;
                this.cboCostCenter.DataBind();
            }
        }

        private void FillCostCenterCombo(bool reloadFromDB = true)
        {
            try
            {
                string userCostCenter = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USER_COST_CENTER]);
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
                        comboSource.AddRange(source.ToList());
                    }
                }

                #region Add blank item
                comboSource.Insert(0, new CostCenterEntity()
                {
                    CostCenter = UIHelper.CONST_COMBO_EMTYITEM_ID,
                    CostCenterName = string.Empty,
                    CostCenterFullName = string.Empty
                });
                #endregion

                // Store to session
                this.CostCenterList = comboSource;

                #region Bind data to combobox
                this.cboCostCenter.DataSource = comboSource;
                this.cboCostCenter.DataTextField = "CostCenterFullName";
                this.cboCostCenter.DataValueField = "CostCenter";
                this.cboCostCenter.DataBind();

                if (this.cboCostCenter.Items.Count > 0
                    && !string.IsNullOrEmpty(userCostCenter))
                {
                    this.cboCostCenter.SelectedValue = userCostCenter;
                }
                #endregion
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex);
            }
        }

        private void GetEmployeeManagedCostCenter()
        {
            try
            {
                // Initialize collection
                this.ManagedCostCenterList.Clear();

                int empNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);
                string error = string.Empty;
                string innerError = string.Empty;
                DALProxy proxy = new DALProxy();

                var source = proxy.GetManagedCostCenter(empNo, ref error, ref innerError);
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
                        this.ManagedCostCenterList.AddRange(source);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion                
    }
}