using GARMCO.AMS.GAP.Utility;
using GARMCO.AMS.TAS.BL.Entities;
using GARMCO.AMS.TAS.UI.Helpers;
using GARMCO.AMS.TAS.UI.Views.Shared;
using GARMCO.Common.DAL.Employee;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

namespace GARMCO.AMS.TAS.UI.Views.SecurityModule
{
    public partial class EmergencyResponseTeam : BaseWebForm, IFormExtension
    {
        #region Enumeration
        private enum ValidationErrorType
        {
            CustomFormError,
            NoError,
            NoGroupType,
            NoDate
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

        private Dictionary<string, object> FireTeamStorage
        {
            get
            {
                Dictionary<string, object> list = Session["FireTeamStorage"] as Dictionary<string, object>;
                if (list == null)
                    Session["FireTeamStorage"] = list = new Dictionary<string, object>();

                return list;
            }
            set
            {
                Session["FireTeamStorage"] = value;
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

        private List<FireTeamMember> FireTeamMemberList
        {
            get
            {
                List<FireTeamMember> list = ViewState["FireTeamMemberList"] as List<FireTeamMember>;
                if (list == null)
                    ViewState["FireTeamMemberList"] = list = new List<FireTeamMember>();

                return list;
            }
            set
            {
                ViewState["FireTeamMemberList"] = value;
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

                this.Master.SetPageForm(UIHelper.FormAccessCodes.FIRETEAM.ToString());

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
                FillCostCenterCombo();
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
                this.Master.FormTitle = UIHelper.PAGE_EMERGENCY_RESPONSE_TEAM_TITLE;
                #endregion

                #region Check if user has permission to access the page
                if (!GAPFunction.CheckFormAccess(this.Master.FormAccess, GAPConstants.FormAccessIndex.Retrieve))
                {
                    if (SecurityUserList.Count > 0 &&
                        SecurityUserList.Where(a => a.Trim() == userID).FirstOrDefault() == null)
                    {
                        Response.Redirect(String.Format("{0}?error={1}&pageName={2}", UIHelper.PAGE_ERROR, Convert.ToInt32(UIHelper.PageErrorCodes.NoAccessOnPage), UIHelper.PAGE_EMERGENCY_RESPONSE_TEAM_TITLE), true);
                    }
                }
                #endregion

                #region Checks if current user has permission to create, update, delete and print
                //this.btnCreate.Visible = this.Master.IsCreateAllowed;
                //this.btnCancel.Visible = this.Master.IsEditAllowed;
                //this.btnDelete.Visible = this.Master.IsDeleteAllowed;
                //this.btnPrint.Visible = this.Master.IsPrintAllowed;
                #endregion

                #region Set default button
                this.Master.DefaultButton = this.btnSearch.UniqueID;
                #endregion

                #region Restore saved data from local storage
                string formFlag = string.Empty;
                if (this.FireTeamStorage.Count > 0)
                {
                    if (this.FireTeamStorage.ContainsKey("FormFlag"))
                        formFlag = UIHelper.ConvertObjectToString(this.FireTeamStorage["FormFlag"]);
                }
                #endregion

                if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.GetEmployeeInfo.ToString())
                {
                    #region Get the employee info
                    RestoreDataFromCollection();

                    if (UIHelper.ConvertObjectToInt(Session[UIHelper.CONST_EMPLOYEE_SEARCH_FLAG]) == 1)
                    {
                        this.txtEmp.Value = UIHelper.ConvertObjectToInt(Request.QueryString[UIHelper.QUERY_STRING_EMPNO_KEY]);
                    }

                    // Clear data storage
                    this.FireTeamStorage.Clear();

                    // Begin searching for records
                    //this.btnSearch_Click(this.btnSearch, new EventArgs());
                    #endregion
                }
                else
                {
                    ClearForm();
                    ProcessQueryString();
                    FillComboData();

                    #region Initialize controls
                    this.cboGroupType.SelectedValue = "valAvailableFireTeam";
                    this.cboShift.SelectedValue = "valAll";
                    this.dtpAttendanceDate.MaxDate = DateTime.Now;
                    this.dtpAttendanceDate.SelectedDate = DateTime.Now;
                    //this.chkAvailableFireTeam.Checked = true;
                    this.rbAvailableFireTeam.Checked = true;

                    // Select all work shifts
                    //foreach (RadComboBoxItem item in this.cboShift.Items)
                    //{
                    //    item.Selected = true;
                    //    item.Checked = true;
                    //}
                    #endregion 

                    // Begin searching for records
                    this.btnSearch_Click(this.btnSearch, new EventArgs());                    
                }
            }

            ScriptManager.RegisterStartupScript(this, this.GetType(), "HideScrollbar", "HideScrollbar();", true);
            AddControlsAttribute();
        }
        #endregion

        #region Action Buttons
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            BeginSearchEmergencyTeam();

            #region Old logic commented
            //try
            //{
            //    #region Perform Data Validation
            //    int errorCount = 0;

            //    // Check Group Type
            //    if (this.cboGroupType.SelectedValue == "valAll")
            //    {
            //        this.txtGeneric.Text = ValidationErrorType.NoGroupType.ToString();
            //        this.ErrorType = ValidationErrorType.NoGroupType;
            //        this.cusValGroupType.Validate();
            //        errorCount++;
            //    }

            //    // Check Date
            //    //if (this.dtpAttendanceDate.SelectedDate == null)
            //    //{
            //    //    this.txtGeneric.Text = ValidationErrorType.NoDate.ToString();
            //    //    this.ErrorType = ValidationErrorType.NoDate;
            //    //    this.cusValDate.Validate();
            //    //    errorCount++;
            //    //}

            //    if (errorCount > 0)
            //        return;
            //    #endregion

            //    DateTime processDate = this.dtpAttendanceDate.SelectedDate.Value;
            //    string costCenter = this.cboCostCenter.Text.Trim();
            //    int empNo = UIHelper.ConvertObjectToInt(this.txtEmp.Text);
            //    if (empNo.ToString().Length == 4)
            //    {
            //        empNo += 10000000;

            //        // Display Emp. No.
            //        this.txtEmp.Text = empNo.ToString();
            //    }
            //    string shiftCode = this.cboShift.SelectedValue;

            //    byte actionType = 0;
            //    if (this.cboGroupType.SelectedValue == "valAvailableFireTeam")
            //        actionType = 1;
            //    else if (this.cboGroupType.SelectedValue == "valAvailableFireWatch")
            //        actionType = 2;
            //    else if (this.cboGroupType.SelectedValue == "valAvailableFireTeamFireWatch")
            //        actionType = 3;
            //    else if (this.cboGroupType.SelectedValue == "valAllFireTeam")
            //        actionType = 4;
            //    else if (this.cboGroupType.SelectedValue == "valAllFireWatch")
            //        actionType = 5;

            //    //string shiftCodeArray = string.Empty;
            //    //if (this.cboShift.CheckedItems.Count > 0)
            //    //{
            //    //    foreach (var item in this.cboShift.CheckedItems)
            //    //    {
            //    //        if (shiftCodeArray.Length == 0)
            //    //            shiftCodeArray = item.Value;
            //    //        else
            //    //            shiftCodeArray = string.Format("{0}, {1}", shiftCodeArray, item.Value);
            //    //    }
            //    //}

            //    GetFireTeamAndFireWatch(actionType, processDate, shiftCode, empNo, costCenter, true);
            //}
            //catch (Exception ex)
            //{
            //    DisplayFormLevelError(ex.Message.ToString());
            //}
            #endregion
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            ClearForm();

            this.cboGroupType.SelectedValue = "valAvailableFireTeam";
            this.cboShift.SelectedValue = "valAll";
            this.dtpAttendanceDate.SelectedDate = DateTime.Now;
            //this.chkAvailableFireTeam.Checked = true;
            this.rbAvailableFireTeam.Checked = true;

            this.btnSearch_Click(this.btnSearch, new EventArgs());
        }

        protected void btnPrint_Click(object sender, EventArgs e)
        {
            //int errorCount = 0;

            //#region Perform Validation
            //// Check Calendar Year
            //if (this.FireTeamMemberList.Count == 0)
            //{
            //    this.txtGeneric.Text = ValidationErrorType.NoRecord.ToString();
            //    this.ErrorType = ValidationErrorType.NoRecord;
            //    this.cusValButton.Validate();
            //    errorCount++;
            //}
            //#endregion

            //if (errorCount > 0)
            //    return;

            //#region Display report
            //StoreDataToCollection(UIHelper.PagePostBackFlags.ShowReport);

            //// Pass data to the session
            //Session["EmployeeSwipeSummarySource"] = this.FireTeamMemberList;

            //Response.Redirect
            //(
            //    String.Format(UIHelper.PAGE_REPORT_VIEWER + "?{0}={1}&{2}={3}&{4}={5}&{6}={7}",
            //    UIHelper.QUERY_STRING_REPORT_TYPE_KEY,
            //    UIHelper.ReportTypes.EmployeeSwipeSummary.ToString(),
            //    UIHelper.QUERY_STRING_CALLER_FORM_KEY,
            //    UIHelper.PAGE_EMERGENCY_RESPONSE_TEAM,
            //    UIHelper.QUERY_STRING_COSTCENTER_NAME_KEY,
            //    this.cboCostCenter.Text,
            //    "DateDuration",
            //    string.Format("{0} to {1}", 
            //        this.dtpStartDate.SelectedDate.Value.ToString("dd-MMM-yyyy"), 
            //        this.dtpEndDate.SelectedDate.Value.ToString("dd-MMM-yyyy"))
            //),
            //false);
            //#endregion
        }

        protected void btnExportToExcelDummy_Click(object sender, EventArgs e)
        {

        }

        protected void btnFindEmployee_Click(object sender, EventArgs e)
        {
            StoreDataToCollection(UIHelper.PagePostBackFlags.GetEmployeeInfo);

            Response.Redirect
            (
                String.Format(UIHelper.PAGE_EMPLOYEE_SEARCH + "?{0}={1}",
                UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                UIHelper.PAGE_EMERGENCY_RESPONSE_TEAM
            ),
            false);
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
                else if (this.ErrorType == ValidationErrorType.NoGroupType)
                {
                    validator.ErrorMessage = "Group Type is mandatory and should not be left blank or unspecified.";
                    validator.ToolTip = "Group Type is mandatory and should not be left blank or unspecified.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoDate)
                {
                    validator.ErrorMessage = "Date is mandatory and should not be left blank or unspecified.";
                    validator.ToolTip = "Date is mandatory and should not be left blank or unspecified.";
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

        protected void rblFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.btnSearch_Click(this.btnSearch, new EventArgs());
        }

        protected void cboGroupType_SelectedIndexChanged(object sender, RadComboBoxSelectedIndexChangedEventArgs e)
        {
            this.btnSearch_Click(this.btnSearch, new EventArgs());
        }

        protected void chkAvailableFireTeam_Click(object sender, EventArgs e)
        {
            //if (this.chkAllFireTeam.Checked == true)
            //    this.chkAllFireTeam.Checked = false;

            //if (this.chkAllFireWatch.Checked == true)
            //    this.chkAllFireWatch.Checked = false;

            //this.btnSearch_Click(this.btnSearch, new EventArgs());
        }

        protected void chkAvailableFireWatch_Click(object sender, EventArgs e)
        {
            //if (this.chkAllFireTeam.Checked == true)
            //    this.chkAllFireTeam.Checked = false;

            //if (this.chkAllFireWatch.Checked == true)
            //    this.chkAllFireWatch.Checked = false;

            //this.btnSearch_Click(this.btnSearch, new EventArgs());
        }

        protected void chkAllFireTeam_Click(object sender, EventArgs e)
        {
            //if (this.chkAvailableFireTeam.Checked == true)
            //    this.chkAvailableFireTeam.Checked = false;

            //if (this.chkAvailableFireWatch.Checked == true)
            //    this.chkAvailableFireWatch.Checked = false;

            //this.btnSearch_Click(this.btnSearch, new EventArgs());
        }

        protected void chkAllFireWatch_Click(object sender, EventArgs e)
        {
            //if (this.chkAvailableFireTeam.Checked == true)
            //    this.chkAvailableFireTeam.Checked = false;

            //if (this.chkAvailableFireWatch.Checked == true)
            //    this.chkAvailableFireWatch.Checked = false;

            //this.btnSearch_Click(this.btnSearch, new EventArgs());
        }

        protected void rbAvailableFireTeam_Click(object sender, EventArgs e)
        {
            this.rbAvailableFireWatch.Checked = !this.rbAvailableFireTeam.Checked;
            this.rbAllFireTeam.Checked = !this.rbAvailableFireTeam.Checked;
            this.rbAllFireWatch.Checked = !this.rbAvailableFireTeam.Checked;

            this.btnSearch_Click(this.btnSearch, new EventArgs());
        }

        protected void rbAvailableFireWatch_Click(object sender, EventArgs e)
        {
            this.rbAvailableFireTeam.Checked = !this.rbAvailableFireWatch.Checked;
            this.rbAllFireTeam.Checked = !this.rbAvailableFireWatch.Checked;
            this.rbAllFireWatch.Checked = !this.rbAvailableFireWatch.Checked;

            this.btnSearch_Click(this.btnSearch, new EventArgs());
        }

        protected void rbAllFireTeam_Click(object sender, EventArgs e)
        {
            this.rbAvailableFireTeam.Checked = !this.rbAllFireTeam.Checked;
            this.rbAvailableFireWatch.Checked = !this.rbAllFireTeam.Checked;
            this.rbAllFireWatch.Checked = !this.rbAllFireTeam.Checked;

            this.btnSearch_Click(this.btnSearch, new EventArgs());
        }

        protected void rbAllFireWatch_Click(object sender, EventArgs e)
        {
            this.rbAvailableFireTeam.Checked = !this.rbAllFireWatch.Checked;
            this.rbAvailableFireWatch.Checked = !this.rbAllFireWatch.Checked;
            this.rbAllFireTeam.Checked = !this.rbAllFireWatch.Checked;

            this.btnSearch_Click(this.btnSearch, new EventArgs());
        }

        protected void tooltipMan_AjaxUpdate(object sender, ToolTipUpdateEventArgs e)
        {
            e.UpdatePanel.ContentTemplateContainer.Controls.Add(new LiteralControl(e.Value));
        }
        #endregion

        #region Grid Events and Methods
        protected void gridSearchResult_PageIndexChanged(object sender, Telerik.Web.UI.GridPageChangedEventArgs e)
        {
            try
            {
                #region Code for grid with pagination
                //int startRowIndex = 0;
                //int maximumRows = this.gridSearchResult.PageSize;

                //if (e.NewPageIndex >= 1)
                //    startRowIndex = (this.gridSearchResult.PageSize * e.NewPageIndex) + 1;

                //// Save to session
                //this.CurrentStartRowIndex = startRowIndex;
                //this.CurrentMaximumRows = maximumRows;

                //GetFireTeamAndFireWatch(startRowIndex, maximumRows);
                #endregion

                // Store page index to session
                this.CurrentPageIndex = e.NewPageIndex;

                RebindDataToGrid();
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void gridSearchResult_PageSizeChanged(object sender, Telerik.Web.UI.GridPageSizeChangedEventArgs e)
        {
            try
            {
                #region Code for grid with pagination
                //int startRowIndex = 0;
                //int maximumRows = e.NewPageSize;

                //GetFireTeamAndFireWatch(startRowIndex, maximumRows);
                #endregion

                RebindDataToGrid();
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void gridSearchResult_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (this.FireTeamMemberList.Count > 0)
            {
                gridSearchResult.DataSource = this.FireTeamMemberList;
                gridSearchResult.DataBind();

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
                        sortExpr.SortOrder = gridSearchResult.MasterTableView.AllowNaturalSort ? GridSortOrder.None : GridSortOrder.Descending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;

                    case GridSortOrder.Descending:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = GridSortOrder.Ascending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;
                }

                e.Canceled = true;
                gridSearchResult.Rebind();
            }
            else
                InitializeGrid();
        }

        protected void gridSearchResult_ItemCommand(object sender, GridCommandEventArgs e)
        {
            if (e.CommandName.Equals(RadGrid.SelectCommandName))
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {
                    int empNo = UIHelper.ConvertObjectToInt(item["EmpNo"].Text);
                    DateTime? swipeDate = UIHelper.ConvertObjectToDate(item["SwipeDate"].Text);

                    #region Redirect to Employee Swipe History page
                    //StoreDataToCollection(UIHelper.PagePostBackFlags.RedirectToOtherPage);

                    //Response.Redirect
                    //(
                    //    String.Format(UIHelper.PAGE_INDIVIDUAL_SWIPES_INQUIRY + "?{0}={1}&{2}={3}&{4}={5}&{6}={7}",
                    //    UIHelper.QUERY_STRING_EMPNO_KEY,
                    //    empNo,
                    //    UIHelper.QUERY_STRING_SWIPEDATE_KEY,
                    //    Convert.ToDateTime(swipeDate).ToString(),
                    //    UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                    //    UIHelper.PAGE_EMERGENCY_RESPONSE_TEAM,
                    //    UIHelper.QUERY_STRING_SHIFTCODE_KEY,
                    //    UIHelper.ConvertObjectToString(item["ShiftCode"].Text)
                    //),
                    //false);
                    #endregion
                }
            }
        }

        protected void gridSearchResult_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item.ItemType == GridItemType.AlternatingItem || e.Item.ItemType == GridItemType.Item)
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {

                }
            }
        }

        protected void gridSearchResult_PreRender(object sender, EventArgs e)
        {
            GridColumn dynamicColumn = this.gridSearchResult.MasterTableView.RenderColumns.Where(a => a.UniqueName == "EmpNo").FirstOrDefault();
            if (dynamicColumn != null)
            {
                dynamicColumn.ItemStyle.Font.Bold = true;
                dynamicColumn.ItemStyle.ForeColor = System.Drawing.Color.Purple;
            }

            dynamicColumn = this.gridSearchResult.MasterTableView.RenderColumns.Where(a => a.UniqueName == "MobileNo").FirstOrDefault();
            if (dynamicColumn != null)
            {
                dynamicColumn.ItemStyle.Font.Bold = true;
                dynamicColumn.ItemStyle.ForeColor = System.Drawing.Color.Red;
            }

            #region Show/Hide "GroupType" field 
            dynamicColumn = this.gridSearchResult.MasterTableView.RenderColumns.Where(a => a.UniqueName == "GroupType").FirstOrDefault();
            if (dynamicColumn != null)
            {
                dynamicColumn.Visible = this.cboGroupType.SelectedValue == "valAvailableFireTeamFireWatch";
            }
            #endregion
        }

        private void RebindDataToGrid()
        {
            if (this.FireTeamMemberList.Count > 0)
            {
                this.gridSearchResult.DataSource = this.FireTeamMemberList;
                this.gridSearchResult.DataBind();
            }
            else
                InitializeGrid();
        }

        private void InitializeGrid()
        {
            this.gridSearchResult.DataSource = new List<FireTeamMember>();
            this.gridSearchResult.DataBind();
        }
        #endregion

        #region Private Methods
        private void RestoreDataFromCollection()
        {
            if (this.FireTeamStorage.Count == 0)
                return;

            #region Restore session values
            if (this.FireTeamStorage.ContainsKey("FireTeamMemberList"))
                this.FireTeamMemberList = this.FireTeamStorage["FireTeamMemberList"] as List<FireTeamMember>;
            else
                this.FireTeamMemberList = null;

            if (this.FireTeamStorage.ContainsKey("CurrentStartRowIndex"))
                this.CurrentStartRowIndex = UIHelper.ConvertObjectToInt(this.FireTeamStorage["CurrentStartRowIndex"]);
            else
                this.CurrentStartRowIndex = 0;

            if (this.FireTeamStorage.ContainsKey("CurrentMaximumRows"))
                this.CurrentMaximumRows = UIHelper.ConvertObjectToInt(this.FireTeamStorage["CurrentMaximumRows"]);
            else
                this.CurrentMaximumRows = 0;

            if (this.FireTeamStorage.ContainsKey("CurrentPageIndex"))
                this.CurrentPageIndex = UIHelper.ConvertObjectToInt(this.FireTeamStorage["CurrentPageIndex"]);
            else
                this.CurrentPageIndex = 0;
            #endregion

            #region Restore control values
            if (this.FireTeamStorage.ContainsKey("cboGroupType"))
                this.cboGroupType.SelectedValue = UIHelper.ConvertObjectToString(this.FireTeamStorage["cboGroupType"]);
            else
            {
                this.cboGroupType.SelectedIndex = -1;
                this.cboGroupType.Text = string.Empty;
            }

            if (this.FireTeamStorage.ContainsKey("cboShift"))
                this.cboShift.SelectedValue = UIHelper.ConvertObjectToString(this.FireTeamStorage["cboShift"]);
            else
            {
                this.cboShift.SelectedIndex = -1;
                this.cboShift.Text = string.Empty;
            }

            if (this.FireTeamStorage.ContainsKey("cboCostCenter"))
                this.cboCostCenter.Text = UIHelper.ConvertObjectToString(this.FireTeamStorage["cboCostCenter"]);
            else
                this.cboCostCenter.Text = string.Empty;

            if (this.FireTeamStorage.ContainsKey("txtEmp"))
                this.txtEmp.Text = UIHelper.ConvertObjectToString(this.FireTeamStorage["txtEmp"]);
            else
                this.txtEmp.Text = string.Empty;

            if (this.FireTeamStorage.ContainsKey("dtpAttendanceDate"))
                this.dtpAttendanceDate.SelectedDate = UIHelper.ConvertObjectToDate(this.FireTeamStorage["dtpAttendanceDate"]);
            else
                this.dtpAttendanceDate.SelectedDate = null;

            //if (this.FireTeamStorage.ContainsKey("chkAvailableFireTeam"))
            //    this.chkAvailableFireTeam.Checked = UIHelper.ConvertObjectToBolean(this.FireTeamStorage["chkAvailableFireTeam"]);
            //else
            //    this.chkAvailableFireTeam.Checked = false;

            //if (this.FireTeamStorage.ContainsKey("chkAvailableFireWatch"))
            //    this.chkAvailableFireWatch.Checked = UIHelper.ConvertObjectToBolean(this.FireTeamStorage["chkAvailableFireWatch"]);
            //else
            //    this.chkAvailableFireWatch.Checked = false;

            //if (this.FireTeamStorage.ContainsKey("chkAllFireTeam"))
            //    this.chkAllFireTeam.Checked = UIHelper.ConvertObjectToBolean(this.FireTeamStorage["chkAllFireTeam"]);
            //else
            //    this.chkAllFireTeam.Checked = false;

            //if (this.FireTeamStorage.ContainsKey("chkAllFireWatch"))
            //    this.chkAllFireWatch.Checked = UIHelper.ConvertObjectToBolean(this.FireTeamStorage["chkAllFireWatch"]);
            //else
            //    this.chkAllFireWatch.Checked = false;

            if (this.FireTeamStorage.ContainsKey("rbAvailableFireTeam"))
                this.rbAvailableFireTeam.Checked = UIHelper.ConvertObjectToBolean(this.FireTeamStorage["rbAvailableFireTeam"]);
            else
                this.rbAvailableFireTeam.Checked = false;

            if (this.FireTeamStorage.ContainsKey("rbAvailableFireWatch"))
                this.rbAvailableFireWatch.Checked = UIHelper.ConvertObjectToBolean(this.FireTeamStorage["rbAvailableFireWatch"]);
            else
                this.rbAvailableFireWatch.Checked = false;

            if (this.FireTeamStorage.ContainsKey("rbAllFireTeam"))
                this.rbAllFireTeam.Checked = UIHelper.ConvertObjectToBolean(this.FireTeamStorage["rbAllFireTeam"]);
            else
                this.rbAllFireTeam.Checked = false;

            if (this.FireTeamStorage.ContainsKey("rbAllFireWatch"))
                this.rbAllFireWatch.Checked = UIHelper.ConvertObjectToBolean(this.FireTeamStorage["rbAllFireWatch"]);
            else
                this.rbAllFireWatch.Checked = false;
            #endregion

            // Refresh the grid
            RebindDataToGrid();

            // Set the grid page index
            this.gridSearchResult.CurrentPageIndex = this.CurrentPageIndex;
            this.gridSearchResult.MasterTableView.CurrentPageIndex = this.CurrentPageIndex;
            this.gridSearchResult.MasterTableView.DataBind();
        }

        private void StoreDataToCollection(UIHelper.PagePostBackFlags formFlag)
        {
            this.FireTeamStorage.Clear();
            this.FireTeamStorage.Add("FormFlag", formFlag.ToString());

            #region Store control values to the collection
            this.FireTeamStorage.Add("cboCostCenter", this.cboCostCenter.Text);
            this.FireTeamStorage.Add("cboGroupType", this.cboGroupType.SelectedValue);
            this.FireTeamStorage.Add("cboShift", this.cboShift.SelectedValue);
            this.FireTeamStorage.Add("txtEmp", this.txtEmp.Text);
            this.FireTeamStorage.Add("dtpAttendanceDate", this.dtpAttendanceDate.SelectedDate);
            //this.FireTeamStorage.Add("chkAvailableFireTeam", this.chkAvailableFireTeam.Checked);
            //this.FireTeamStorage.Add("chkAvailableFireWatch", this.chkAvailableFireWatch.Checked);
            //this.FireTeamStorage.Add("chkAllFireTeam", this.chkAllFireTeam.Checked);
            //this.FireTeamStorage.Add("chkAllFireWatch", this.chkAllFireWatch.Checked);
            this.FireTeamStorage.Add("rbAvailableFireTeam", this.rbAvailableFireTeam.Checked);
            this.FireTeamStorage.Add("rbAvailableFireWatch", this.rbAvailableFireWatch.Checked);
            this.FireTeamStorage.Add("rbAllFireTeam", this.rbAllFireTeam.Checked);
            this.FireTeamStorage.Add("rbAllFireWatch", this.rbAllFireWatch.Checked);
            #endregion

            // Store session data to the collection
            this.FireTeamStorage.Add("FireTeamMemberList", this.FireTeamMemberList);
            this.FireTeamStorage.Add("CurrentStartRowIndex", this.CurrentStartRowIndex);
            this.FireTeamStorage.Add("CurrentMaximumRows", this.CurrentMaximumRows);
            this.FireTeamStorage.Add("CurrentPageIndex", this.CurrentPageIndex);
        }

        private void FillCostCenterCombo()
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
            filteredDT.Rows.Add(defaultRow);
            #endregion

            //if (this.AllowedCostCenterList.Count > 0)
            //{
            //    #region Filter list based on allowed cost center
            //    foreach (string filter in this.AllowedCostCenterList)
            //    {
            //        DataRow[] rows = source.Where(d => UIHelper.ConvertObjectToString(d["CostCenter"]) == filter).ToArray();
            //        foreach (DataRow rw in rows)
            //        {
            //            EmployeeDAL.CostCenterRow row = filteredDT.NewCostCenterRow();
            //            row.CostCenter = UIHelper.ConvertObjectToString(rw["CostCenter"]);
            //            row.CostCenterName = UIHelper.ConvertObjectToString(rw["CostCenterName"]);
            //            row.Company = UIHelper.ConvertObjectToString(rw["Company"]);
            //            row.SuperintendentNo = UIHelper.ConvertObjectToInt(rw["Company"]);
            //            row.SuperintendentName = UIHelper.ConvertObjectToString(rw["Company"]);
            //            row.ManagerNo = UIHelper.ConvertObjectToInt(rw["Company"]);
            //            row.ManagerName = UIHelper.ConvertObjectToString(rw["Company"]);
            //            filteredDT.Rows.Add(row);
            //        }
            //    }
            //    #endregion
            //}
            //else if (this.AllowedCostCenterList.Count == 0 && UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USER_COST_CENTER]) != string.Empty)
            //{
            //    this.AllowedCostCenterList.Add(UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USER_COST_CENTER]));

            //    #region Filter list based on allowed cost center
            //    foreach (string filter in this.AllowedCostCenterList)
            //    {
            //        DataRow[] rows = source.Where(d => UIHelper.ConvertObjectToString(d["CostCenter"]) == filter).ToArray();
            //        foreach (DataRow rw in rows)
            //        {
            //            EmployeeDAL.CostCenterRow row = filteredDT.NewCostCenterRow();
            //            row.CostCenter = UIHelper.ConvertObjectToString(rw["CostCenter"]);
            //            row.CostCenterName = UIHelper.ConvertObjectToString(rw["CostCenterName"]);
            //            row.Company = UIHelper.ConvertObjectToString(rw["Company"]);
            //            row.SuperintendentNo = UIHelper.ConvertObjectToInt(rw["Company"]);
            //            row.SuperintendentName = UIHelper.ConvertObjectToString(rw["Company"]);
            //            row.ManagerNo = UIHelper.ConvertObjectToInt(rw["Company"]);
            //            row.ManagerName = UIHelper.ConvertObjectToString(rw["Company"]);
            //            filteredDT.Rows.Add(row);
            //        }
            //    }
            //    #endregion
            //}
            //else
            //{
            #region No filtering for cost center
            foreach (DataRow rw in source)
            {
                EmployeeDAL.CostCenterRow row = filteredDT.NewCostCenterRow();
                row.CostCenter = UIHelper.ConvertObjectToString(rw["CostCenter"]);
                row.CostCenterName = UIHelper.ConvertObjectToString(rw["CostCenterName"]);
                row.Company = UIHelper.ConvertObjectToString(rw["Company"]);
                row.SuperintendentNo = UIHelper.ConvertObjectToInt(rw["Company"]);
                row.SuperintendentName = UIHelper.ConvertObjectToString(rw["Company"]);
                row.ManagerNo = UIHelper.ConvertObjectToInt(rw["Company"]);
                row.ManagerName = UIHelper.ConvertObjectToString(rw["Company"]);
                filteredDT.Rows.Add(row);
            }
            #endregion
            //}

            if (filteredDT.Rows.Count > 0)
            {
                this.cboCostCenter.DataTextField = "CostCenter";
                this.cboCostCenter.DataValueField = "CostCenter";
                this.cboCostCenter.DataSource = filteredDT;
                this.cboCostCenter.DataBind();
            }
        }

        private void FillComboData(bool reloadFromDB = true)
        {

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

        private void BeginSearchEmergencyTeam()
        {
            try
            {
                #region Perform Data Validation
                int errorCount = 0;

                // Check Group Type
                //if (this.chkAvailableFireTeam.Checked == false &&
                //    this.chkAvailableFireWatch.Checked == false &&
                //    this.chkAllFireTeam.Checked == false &&
                //    this.chkAllFireWatch.Checked == false)
                if (this.rbAvailableFireTeam.Checked == false &&
                    this.rbAvailableFireWatch.Checked == false &&
                    this.rbAllFireTeam.Checked == false &&
                    this.rbAllFireWatch.Checked == false)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoGroupType.ToString();
                    this.ErrorType = ValidationErrorType.NoGroupType;
                    this.cusValGroupType.Validate();
                    errorCount++;
                }

                if (errorCount > 0)
                    return;
                #endregion

                DateTime processDate = DateTime.Now.Date;
                string costCenter = this.cboCostCenter.Text.Trim();
                int empNo = UIHelper.ConvertObjectToInt(this.txtEmp.Text);
                if (empNo.ToString().Length == 4)
                {
                    empNo += 10000000;

                    // Display Emp. No.
                    this.txtEmp.Text = empNo.ToString();
                }
                string shiftCode = this.cboShift.SelectedValue;
               
                byte actionType = 0;
                if (this.rbAvailableFireTeam.Checked)
                    actionType = 1;     // Load currently available fire team
                else if (this.rbAvailableFireWatch.Checked)
                    actionType = 2;     // Load currently available fire watch
                else if (this.rbAllFireTeam.Checked)
                    actionType = 4;     // All fire team members
                else if (this.rbAllFireWatch.Checked)
                    actionType = 5;     // All fire watch members

                GetFireTeamAndFireWatch(actionType, processDate, shiftCode, empNo, costCenter, true);
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }
        #endregion

        #region IFormExtension Interface Implementation
        public void ClearForm()
        {
            #region Reset Controls
            this.cboCostCenter.Text = string.Empty;
            this.cboCostCenter.SelectedIndex = -1;
            this.cboGroupType.SelectedIndex = -1;
            this.cboGroupType.Text = string.Empty;
            //this.cboShift.ClearCheckedItems();
            //this.cboShift.ClearSelection();
            this.cboShift.Text = string.Empty;
            this.cboShift.SelectedIndex = -1;
            this.txtEmp.Text = string.Empty;
            this.dtpAttendanceDate.SelectedDate = null;
            //this.chkAvailableFireTeam.Checked = false;
            //this.chkAvailableFireWatch.Checked = false;
            //this.chkAllFireTeam.Checked = false;
            //this.chkAllFireWatch.Checked = false;
            this.rbAvailableFireTeam.Checked = false;
            this.rbAvailableFireWatch.Checked = false;
            this.rbAllFireTeam.Checked = false;
            this.rbAllFireWatch.Checked = false;
            #endregion

            // Clear collections
            this.FireTeamMemberList.Clear();

            KillSessions();
            InitializeGrid();

            this.gridSearchResult.CurrentPageIndex = 0;
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
            ViewState["CurrentStartRowIndex"] = null;
            ViewState["CurrentMaximumRows"] = null;
            ViewState["CurrentPageIndex"] = null;
            ViewState["CustomErrorMsg"] = null;
        }
        #endregion

        #region Database Access
        protected void objCostCenter_Selected(object sender, ObjectDataSourceStatusEventArgs e)
        {
            EmployeeDAL.CostCenterDataTable dataTable = e.ReturnValue as
                EmployeeDAL.CostCenterDataTable;

            // Checks if found
            if (dataTable != null)
            {
                #region Create a new record
                EmployeeDAL.CostCenterRow row = dataTable.NewCostCenterRow();

                row.CostCenter = String.Empty;
                row.CostCenterName = "Please select a Cost Center...";
                row.Company = String.Empty;
                row.SuperintendentNo = 0;
                row.SuperintendentName = String.Empty;
                row.ManagerNo = 0;
                row.ManagerName = String.Empty;

                dataTable.Rows.InsertAt(row, 0);
                #endregion
            }
        }

        private void FillDataToGridOld(UIHelper.FireTeamLoadTypes actionType, DateTime processDate, int empNo = 0, string costCenter = "", bool isDirty = false)
        {
            try
            {
                #region Fill data to the collection
                List<FireTeamMember> gridSource = new List<FireTeamMember>();
                if (this.FireTeamMemberList.Count > 0 && !isDirty)
                {
                    gridSource = this.FireTeamMemberList;
                }
                else
                {
                    // Get WCF Instance
                    if (dataProxy == null)
                        return;

                    string error = string.Empty;
                    string innerError = string.Empty;
                    string imageRootPath = UIHelper.ConvertObjectToString(ConfigurationManager.AppSettings["ImageRootPath"]);

                    // Fetch data from the database
                    var source = dataProxy.GetEmergencyResponseTeam(Convert.ToInt32(actionType), processDate, empNo, costCenter, imageRootPath, ref error, ref innerError);
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
                this.FireTeamMemberList = gridSource;
                #endregion

                #region Bind data to the grid
                if (gridSource.Count > 0)
                {
                    this.gridSearchResult.DataSource = gridSource;
                    this.gridSearchResult.DataBind();

                    //Display the record count
                    this.lblRecordCount.Text = string.Format("{0} record(s) found", gridSource.Count.ToString("#,###"));
                }
                else
                    InitializeGrid();
                #endregion
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex);
            }
        }

        private void GetFireTeamAndFireWatch(byte loadType, DateTime processDate, string shiftCodeArray = "", int empNo = 0, string costCenter = "", bool reloadFromDB = false)
        {
            try
            {
                #region Fill data to the collection
                List<FireTeamMember> gridSource = new List<FireTeamMember>();
                if (this.FireTeamMemberList.Count > 0 && !reloadFromDB)
                {
                    gridSource = this.FireTeamMemberList;
                }
                else
                {
                    // Get WCF Instance
                    if (dataProxy == null)
                        return;

                    string error = string.Empty;
                    string innerError = string.Empty;

                    //string imageRootPath = UIHelper.ConvertObjectToString(ConfigurationManager.AppSettings["ImageRootPath"]);
                    string imageRootPath = UIHelper.ConvertObjectToString(ConfigurationManager.AppSettings["EmpPhotoVirtualFolder"]);

                    // Fetch data from the database
                    var source = dataProxy.GetFireTeamAndFireWatch(loadType, processDate, shiftCodeArray, empNo, costCenter, imageRootPath, ref error, ref innerError);
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
                this.FireTeamMemberList = gridSource;
                #endregion

                #region Bind data to the grid
                if (gridSource.Count > 0)
                {
                    this.gridSearchResult.DataSource = gridSource;
                    this.gridSearchResult.DataBind();

                    //Display the record count
                    this.lblRecordCount.Text = string.Format("{0} record(s) found", gridSource.Count.ToString("#,###"));
                }
                else
                    InitializeGrid();
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion                
    }
}