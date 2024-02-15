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
using GARMCO.Common.Object;
using Telerik.Web.UI;

namespace GARMCO.AMS.TAS.UI.Views.HRFunctions
{
    public partial class TimesheetCorrectionHistory : BaseWebForm, IFormExtension
    {
        #region Enumeration
        private enum ValidationErrorType
        {
            NoError,
            CustomFormError,
            NoSpecifiedEmpNo,
            NoEmpNo,
            NoRecordToDelete
        }

        private enum TabSelection
        {
            valTimesheetHistory,
            valShiftPatternHistory,
            valAbsenceHistory,
            valLeaveHistory
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

        private Dictionary<string, object> TimesheetCorrectionStorage
        {
            get
            {
                Dictionary<string, object> list = Session["TimesheetCorrectionStorage"] as Dictionary<string, object>;
                if (list == null)
                    Session["TimesheetCorrectionStorage"] = list = new Dictionary<string, object>();

                return list;
            }
            set
            {
                Session["TimesheetCorrectionStorage"] = value;
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

        private List<EmployeeAttendanceEntity> TimesheetCorrectionDataList
        {
            get
            {
                List<EmployeeAttendanceEntity> list = ViewState["TimesheetCorrectionDataList"] as List<EmployeeAttendanceEntity>;
                if (list == null)
                    ViewState["TimesheetCorrectionDataList"] = list = new List<EmployeeAttendanceEntity>();

                return list;
            }
            set
            {
                ViewState["TimesheetCorrectionDataList"] = value;
            }
        }

        private List<ShiftPatternEntity> ShiftPatternHistoryDataList
        {
            get
            {
                List<ShiftPatternEntity> list = ViewState["ShiftPatternHistoryDataList"] as List<ShiftPatternEntity>;
                if (list == null)
                    ViewState["ShiftPatternHistoryDataList"] = list = new List<ShiftPatternEntity>();

                return list;
            }
            set
            {
                ViewState["ShiftPatternHistoryDataList"] = value;
            }
        }

        private List<ReasonOfAbsenceEntity> AbsenceHistoryDataList
        {
            get
            {
                List<ReasonOfAbsenceEntity> list = ViewState["AbsenceHistoryDataList"] as List<ReasonOfAbsenceEntity>;
                if (list == null)
                    ViewState["AbsenceHistoryDataList"] = list = new List<ReasonOfAbsenceEntity>();

                return list;
            }
            set
            {
                ViewState["AbsenceHistoryDataList"] = value;
            }
        }

        private List<LeaveEntity> LeaveHistoryDataList
        {
            get
            {
                List<LeaveEntity> list = ViewState["LeaveHistoryDataList"] as List<LeaveEntity>;
                if (list == null)
                    ViewState["LeaveHistoryDataList"] = list = new List<LeaveEntity>();

                return list;
            }
            set
            {
                ViewState["LeaveHistoryDataList"] = value;
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
                    pageSize = this.gridTimesheetCorrection.MasterTableView.PageSize;

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

        private int EmployeeNo
        {
            get
            {
                return UIHelper.ConvertObjectToInt(ViewState["EmployeeNo"]);
            }
            set
            {
                ViewState["EmployeeNo"] = value;
            }
        }

        private UIHelper.FormDataLoadType CurrentFormLoadType
        {
            get
            {
                UIHelper.FormDataLoadType result = UIHelper.FormDataLoadType.OpenSpecificTimesheetRecord;
                if (ViewState["CurrentFormLoadType"] != null)
                {
                    try
                    {
                        result = (UIHelper.FormDataLoadType)Enum.Parse(typeof(UIHelper.FormDataLoadType), UIHelper.ConvertObjectToString(ViewState["CurrentFormLoadType"]));
                    }
                    catch (Exception)
                    {
                    }
                }
                return result;
            }
            set
            {
                ViewState["CurrentFormLoadType"] = value;
            }
        }

        private EmployeeAttendanceEntity CurrentTimesheetRecord
        {
            get
            {
                return Session["SelectedTimesheetRecord"] as EmployeeAttendanceEntity;
            }
            set
            {
                Session["SelectedTimesheetRecord"] = value;
            }
        }

        private int ParamAutoID
        {
            get
            {
                return UIHelper.ConvertObjectToInt(ViewState["ParamAutoID"]);
            }
            set
            {
                ViewState["ParamAutoID"] = value;
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

                this.Master.SetPageForm(UIHelper.FormAccessCodes.TSCORRECTN.ToString());

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
                this.Master.FormTitle = UIHelper.PAGE_TIMESHEET_CORRECTION_HISTORY_TITLE;
                #endregion

                #region Check if user has permission to access the page
                if (!GAPFunction.CheckFormAccess(this.Master.FormAccess, GAPConstants.FormAccessIndex.Retrieve))
                {
                    if (!UIHelper.ConvertObjectToBolean(Session[UIHelper.GARMCO_USER_IS_SPECIAL]))
                    {
                        Response.Redirect(String.Format("{0}?error={1}&pageName={2}", UIHelper.PAGE_ERROR, Convert.ToInt32(UIHelper.PageErrorCodes.NoAccessOnPage), UIHelper.PAGE_TIMESHEET_CORRECTION_HISTORY_TITLE), true);
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
                if (this.TimesheetCorrectionStorage.Count > 0)
                {
                    if (this.TimesheetCorrectionStorage.ContainsKey("FormFlag"))
                        formFlag = UIHelper.ConvertObjectToString(this.TimesheetCorrectionStorage["FormFlag"]);
                }
                #endregion

                if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.GetEmployeeInfo.ToString())
                {
                    #region Get the employee info
                    RestoreDataFromCollection();

                    if (UIHelper.ConvertObjectToInt(Session[UIHelper.CONST_EMPLOYEE_SEARCH_FLAG]) == 1)
                    {
                        this.txtEmpNo.Value = UIHelper.ConvertObjectToInt(Request.QueryString[UIHelper.QUERY_STRING_EMPNO_KEY]);
                        this.litEmpName.Text = UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_EMPNAME_KEY]);
                        this.litPosition.Text = UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_POSITION_KEY]);
                        this.litCostCenter.Text = string.Format("{0} - {1}",
                            UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_COSTCENTER_KEY]),
                            UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_DEPARTMENT_KEY]));

                        // Save Employee No. to session
                        this.EmployeeNo = UIHelper.ConvertObjectToInt(Request.QueryString[UIHelper.QUERY_STRING_EMPNO_KEY]);
                    }

                    // Clear data storage
                    Session.Remove("TimesheetCorrectionStorage");
                    #endregion
                }
                else if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.RedirectToOtherPage.ToString())
                {
                    #region Show the last form data
                    RestoreDataFromCollection();
                    ProcessQueryString();

                    // Clear data storage
                    Session.Remove("TimesheetCorrectionStorage");

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

                    #region Initialize controls
                    if (this.CurrentFormLoadType == UIHelper.FormDataLoadType.OpenSpecificTimesheetRecord)
                    {
                        //this.trButtons.Style[HtmlTextWriterStyle.Display] = "none";
                        this.btnGet.Enabled = false;
                        this.btnFindEmployee.Enabled = false;
                        this.btnSearch.Visible = false;
                        this.txtEmpNo.ReadOnly = true;
                    }
                    else
                    {
                        //this.trButtons.Style[HtmlTextWriterStyle.Display] = string.Empty;
                        this.btnGet.Enabled = true;
                        this.btnFindEmployee.Enabled = true;
                        this.btnSearch.Visible = true;
                        this.txtEmpNo.ReadOnly = false;
                    }
                    #endregion

                    if (this.CurrentTimesheetRecord != null)
                    {
                        // Get employee details
                        this.txtEmpNo.Value = this.CurrentTimesheetRecord.EmpNo;
                        this.litEmpName.Text = this.CurrentTimesheetRecord.EmpName;
                        this.litPosition.Text = !string.IsNullOrEmpty(this.CurrentTimesheetRecord.Position) ? this.CurrentTimesheetRecord.Position : "Not defined";
                        this.litCostCenter.Text = this.CurrentTimesheetRecord.CostCenterFullName;
                        this.litAttendanceDate.Text = this.CurrentTimesheetRecord.DT.HasValue ? Convert.ToDateTime(this.CurrentTimesheetRecord.DT).ToString("dd-MMM-yyyy") : string.Empty;

                        // Fill data in the grid
                        GetTimesheetCorrectionHistory(true);
                    }
                    else
                        GetTimesheetCorrectionHistory(true, this.ParamAutoID);
                }
            }

            ScriptManager.RegisterStartupScript(this, this.GetType(), "HideScrollbar", "HideScrollbar();", true);
            AddControlsAttribute();
        }
        #endregion

        #region Timesheet Correction History Grid Events
        protected void gridTimesheetCorrection_PageIndexChanged(object sender, Telerik.Web.UI.GridPageChangedEventArgs e)
        {
            // Store page index to session
            this.CurrentPageIndex = e.NewPageIndex + 1;

            // Fill data to the grid
            GetTimesheetCorrectionHistory(true);

            // Set focus to the top panel
            Page.SetFocus(this.lnkMoveUp.ClientID);
        }

        protected void gridTimesheetCorrection_PageSizeChanged(object sender, Telerik.Web.UI.GridPageSizeChangedEventArgs e)
        {
            // Store page size to session
            this.CurrentPageSize = e.NewPageSize;
            this.CurrentPageIndex = 1;

            // Fill data to the grid
            GetTimesheetCorrectionHistory(true);

            // Set focus to the top panel
            Page.SetFocus(this.lnkMoveUp.ClientID);
        }

        protected void gridTimesheetCorrection_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (this.TimesheetCorrectionDataList.Count > 0)
            {
                this.gridTimesheetCorrection.DataSource = this.TimesheetCorrectionDataList;
                this.gridTimesheetCorrection.DataBind();

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
                        sortExpr.SortOrder = this.gridTimesheetCorrection.MasterTableView.AllowNaturalSort ? GridSortOrder.None : GridSortOrder.Descending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;

                    case GridSortOrder.Descending:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = GridSortOrder.Ascending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;
                }

                e.Canceled = true;
                this.gridTimesheetCorrection.Rebind();
            }
            else
                InitializeTimesheetCorrectionGrid();
        }

        protected void gridTimesheetCorrection_ItemCommand(object sender, GridCommandEventArgs e)
        {
            if (e.CommandName.Equals(RadGrid.SelectCommandName))
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {
                }
            }
        }

        protected void gridTimesheetCorrection_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item.ItemType == GridItemType.AlternatingItem || e.Item.ItemType == GridItemType.Item)
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {
                    #region Set font color to red if CorrectionCode is not null
                    string correctionCode = UIHelper.ConvertObjectToString(item["CorrectionCode"].Text.Replace("&nbsp;", string.Empty));
                    if (!string.IsNullOrEmpty(correctionCode))
                    {
                        //item["CorrectionCode"].BackColor = System.Drawing.Color.Red;
                        item["CorrectionCode"].ForeColor = System.Drawing.Color.Red;
                        item["CorrectionCode"].Font.Bold = true;
                        //item["CorrectionCode"].ToolTip = UIHelper.ConvertObjectToString(item["CorrectionCodeDesc"].Text);
                    }
                    #endregion
                }
            }
        }

        private void RebindDataToTimesheetCorrectionGrid()
        {
            if (this.TimesheetCorrectionDataList.Count > 0)
            {
                int totalRecords = this.TimesheetCorrectionDataList.FirstOrDefault().TotalRecords;
                if (totalRecords > 0)
                    this.gridTimesheetCorrection.VirtualItemCount = totalRecords;
                else
                    this.gridTimesheetCorrection.VirtualItemCount = 1;

                this.gridTimesheetCorrection.DataSource = this.TimesheetCorrectionDataList;
                this.gridTimesheetCorrection.DataBind();

                //Display the record count
                this.lblRecordCount.Text = string.Format("{0} record(s) found", totalRecords.ToString("#,###"));
            }
            else
                InitializeTimesheetCorrectionGrid();
        }

        private void InitializeTimesheetCorrectionGrid()
        {
            this.gridTimesheetCorrection.DataSource = new List<EmployeeAttendanceEntity>();
            this.gridTimesheetCorrection.DataBind();

            //this.lblRecordCount.Text = "0 record found";
        }
        #endregion

        #region Shift Pattern Change Grid Events
        protected void gridShiftPatternHistory_PageIndexChanged(object sender, Telerik.Web.UI.GridPageChangedEventArgs e)
        {
            // Store page index to session
            this.CurrentPageIndex = e.NewPageIndex + 1;

            // Fill data to the grid
            GetShiftPatternChangeHistory(true);

            // Set focus to the top panel
            Page.SetFocus(this.lnkMoveUp.ClientID);
        }

        protected void gridShiftPatternHistory_PageSizeChanged(object sender, Telerik.Web.UI.GridPageSizeChangedEventArgs e)
        {
            // Store page size to session
            this.CurrentPageSize = e.NewPageSize;
            this.CurrentPageIndex = 1;

            // Fill data to the grid
            GetShiftPatternChangeHistory(true);

            // Set focus to the top panel
            Page.SetFocus(this.lnkMoveUp.ClientID);
        }

        protected void gridShiftPatternHistory_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (this.ShiftPatternHistoryDataList.Count > 0)
            {
                this.gridShiftPatternHistory.DataSource = this.ShiftPatternHistoryDataList;
                this.gridShiftPatternHistory.DataBind();

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
                        sortExpr.SortOrder = this.gridShiftPatternHistory.MasterTableView.AllowNaturalSort ? GridSortOrder.None : GridSortOrder.Descending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;

                    case GridSortOrder.Descending:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = GridSortOrder.Ascending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;
                }

                e.Canceled = true;
                this.gridShiftPatternHistory.Rebind();
            }
            else
                InitializeShiftPatternChangeGrid();
        }

        protected void gridShiftPatternHistory_ItemCommand(object sender, GridCommandEventArgs e)
        {
            if (e.CommandName.Equals(RadGrid.SelectCommandName))
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {
                }
            }
        }

        protected void gridShiftPatternHistory_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item.ItemType == GridItemType.AlternatingItem || e.Item.ItemType == GridItemType.Item)
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {
                    #region Set font color to red if CorrectionCode is not null
                    //string correctionCode = UIHelper.ConvertObjectToString(item["CorrectionCode"].Text.Replace("&nbsp;", string.Empty));
                    //if (!string.IsNullOrEmpty(correctionCode))
                    //{
                    //    //item["CorrectionCode"].BackColor = System.Drawing.Color.Red;
                    //    item["CorrectionCode"].ForeColor = System.Drawing.Color.Red;
                    //    item["CorrectionCode"].Font.Bold = true;
                    //    //item["CorrectionCode"].ToolTip = UIHelper.ConvertObjectToString(item["CorrectionCodeDesc"].Text);
                    //}
                    #endregion
                }
            }
        }

        private void RebindDataToShiftPatternChangeGrid()
        {
            if (this.ShiftPatternHistoryDataList.Count > 0)
            {
                int totalRecords = this.ShiftPatternHistoryDataList.FirstOrDefault().TotalRecords;
                if (totalRecords > 0)
                    this.gridShiftPatternHistory.VirtualItemCount = totalRecords;
                else
                    this.gridShiftPatternHistory.VirtualItemCount = 1;

                this.gridShiftPatternHistory.DataSource = this.ShiftPatternHistoryDataList;
                this.gridShiftPatternHistory.DataBind();

                //Display the record count
                this.lblRecordCount.Text = string.Format("{0} record(s) found", totalRecords.ToString("#,###"));
            }
            else
                InitializeShiftPatternChangeGrid();
        }

        private void InitializeShiftPatternChangeGrid()
        {
            this.gridShiftPatternHistory.DataSource = new List<ShiftPatternEntity>();
            this.gridShiftPatternHistory.DataBind();

            this.lblRecordCount.Text = "0 record found";
        }
        #endregion

        #region Absence History Grid Events
        protected void gridAbsenceHistory_PageIndexChanged(object sender, Telerik.Web.UI.GridPageChangedEventArgs e)
        {
            // Store page index to session
            this.CurrentPageIndex = e.NewPageIndex + 1;

            // Fill data to the grid
            GetAbsencesHistory(true);

            // Set focus to the top panel
            Page.SetFocus(this.lnkMoveUp.ClientID);
        }

        protected void gridAbsenceHistory_PageSizeChanged(object sender, Telerik.Web.UI.GridPageSizeChangedEventArgs e)
        {
            // Store page size to session
            this.CurrentPageSize = e.NewPageSize;
            this.CurrentPageIndex = 1;

            // Fill data to the grid
            GetAbsencesHistory(true);

            // Set focus to the top panel
            Page.SetFocus(this.lnkMoveUp.ClientID);
        }

        protected void gridAbsenceHistory_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (this.AbsenceHistoryDataList.Count > 0)
            {
                this.gridAbsenceHistory.DataSource = this.AbsenceHistoryDataList;
                this.gridAbsenceHistory.DataBind();

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
                        sortExpr.SortOrder = this.gridAbsenceHistory.MasterTableView.AllowNaturalSort ? GridSortOrder.None : GridSortOrder.Descending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;

                    case GridSortOrder.Descending:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = GridSortOrder.Ascending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;
                }

                e.Canceled = true;
                this.gridAbsenceHistory.Rebind();
            }
            else
                InitializeAbsenceHistoryGrid();
        }

        protected void gridAbsenceHistory_ItemCommand(object sender, GridCommandEventArgs e)
        {
            if (e.CommandName.Equals(RadGrid.SelectCommandName))
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {
                }
            }
        }

        protected void gridAbsenceHistory_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item.ItemType == GridItemType.AlternatingItem || e.Item.ItemType == GridItemType.Item)
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {
                   
                }
            }
        }

        private void RebindDataToAbsenceHistoryGrid()
        {
            if (this.AbsenceHistoryDataList.Count > 0)
            {
                int totalRecords = this.AbsenceHistoryDataList.FirstOrDefault().TotalRecords;
                if (totalRecords > 0)
                    this.gridAbsenceHistory.VirtualItemCount = totalRecords;
                else
                    this.gridAbsenceHistory.VirtualItemCount = 1;

                this.gridAbsenceHistory.DataSource = this.AbsenceHistoryDataList;
                this.gridAbsenceHistory.DataBind();
            }
            else
                InitializeAbsenceHistoryGrid();
        }

        private void InitializeAbsenceHistoryGrid()
        {
            this.gridAbsenceHistory.DataSource = new List<ReasonOfAbsenceEntity>();
            this.gridAbsenceHistory.DataBind();
        }
        #endregion

        #region Leave History Grid Events
        protected void gridLeaveHistory_PageIndexChanged(object sender, Telerik.Web.UI.GridPageChangedEventArgs e)
        {
            // Store page index to session
            this.CurrentPageIndex = e.NewPageIndex + 1;

            // Fill data to the grid
            GetLeaveHistory(true);

            // Set focus to the top panel
            Page.SetFocus(this.lnkMoveUp.ClientID);
        }

        protected void gridLeaveHistory_PageSizeChanged(object sender, Telerik.Web.UI.GridPageSizeChangedEventArgs e)
        {
            // Store page size to session
            this.CurrentPageSize = e.NewPageSize;
            this.CurrentPageIndex = 1;

            // Fill data to the grid
            GetLeaveHistory(true);

            // Set focus to the top panel
            Page.SetFocus(this.lnkMoveUp.ClientID);
        }

        protected void gridLeaveHistory_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (this.LeaveHistoryDataList.Count > 0)
            {
                this.gridLeaveHistory.DataSource = this.LeaveHistoryDataList;
                this.gridLeaveHistory.DataBind();

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
                        sortExpr.SortOrder = this.gridLeaveHistory.MasterTableView.AllowNaturalSort ? GridSortOrder.None : GridSortOrder.Descending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;

                    case GridSortOrder.Descending:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = GridSortOrder.Ascending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;
                }

                e.Canceled = true;
                this.gridLeaveHistory.Rebind();
            }
            else
                InitializeLeaveHistoryGrid();
        }

        protected void gridLeaveHistory_ItemCommand(object sender, GridCommandEventArgs e)
        {
            if (e.CommandName.Equals(RadGrid.SelectCommandName))
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {
                }
            }
        }

        protected void gridLeaveHistory_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item.ItemType == GridItemType.AlternatingItem || e.Item.ItemType == GridItemType.Item)
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {

                }
            }
        }

        private void RebindDataToLeaveHistoryGrid()
        {
            if (this.LeaveHistoryDataList.Count > 0)
            {
                int totalRecords = this.LeaveHistoryDataList.FirstOrDefault().TotalRecords;
                if (totalRecords > 0)
                    this.gridLeaveHistory.VirtualItemCount = totalRecords;
                else
                    this.gridLeaveHistory.VirtualItemCount = 1;

                this.gridLeaveHistory.DataSource = this.LeaveHistoryDataList;
                this.gridLeaveHistory.DataBind();
            }
            else
                InitializeLeaveHistoryGrid();
        }

        private void InitializeLeaveHistoryGrid()
        {
            this.gridLeaveHistory.DataSource = new List<LeaveEntity>();
            this.gridLeaveHistory.DataBind();
        }
        #endregion

        #region Action Buttons
        protected void btnReset_Click(object sender, EventArgs e)
        {
            if (this.CurrentFormLoadType == UIHelper.FormDataLoadType.OpenSpecificTimesheetRecord)
            {
                // Clear collections
                this.TimesheetCorrectionDataList.Clear();
                this.ShiftPatternHistoryDataList.Clear();
                this.AbsenceHistoryDataList.Clear();
                this.LeaveHistoryDataList.Clear();

                // Select the default tab
                RadTab defaultTab = this.tabMain.Tabs.Where(a => a.Value == TabSelection.valTimesheetHistory.ToString()).FirstOrDefault();
                if (defaultTab != null)
                {
                    this.tabMain.SelectedIndex = this.tabMain.Tabs.IndexOf(defaultTab);
                    this.MyMultiPage.SelectedIndex = this.tabMain.Tabs.IndexOf(defaultTab);

                    this.tabMain_TabClick(this.tabMain, new RadTabStripEventArgs(defaultTab));
                }
            }
            else
            {
                #region Clear the form
                this.txtEmpNo.Text = string.Empty;
                this.litEmpName.Text = "Not defined";
                this.litPosition.Text = "Not defined";
                this.litCostCenter.Text = "Not defined";
                this.litAttendanceDate.Text = "Not defined";

                // Cler collections
                this.TimesheetCorrectionDataList.Clear();
                this.ShiftPatternHistoryDataList.Clear();
                this.AbsenceHistoryDataList.Clear();
                this.LeaveHistoryDataList.Clear();

                // Clear sessions
                ViewState["CustomErrorMsg"] = null;
                ViewState["CurrentStartRowIndex"] = null;
                ViewState["CurrentMaximumRows"] = null;
                ViewState["CurrentPageIndex"] = null;
                ViewState["CurrentPageSize"] = null;
                ViewState["EmployeeNo"] = null;
                ViewState["ParamAutoID"] = null;

                // Reset the grid
                this.gridTimesheetCorrection.VirtualItemCount = 1;
                this.gridShiftPatternHistory.VirtualItemCount = 1;
                this.gridAbsenceHistory.VirtualItemCount = 1;
                this.gridLeaveHistory.VirtualItemCount = 1;

                this.gridTimesheetCorrection.CurrentPageIndex = 0;
                this.gridShiftPatternHistory.CurrentPageIndex = 0;
                this.gridAbsenceHistory.CurrentPageIndex = 0;
                this.gridLeaveHistory.CurrentPageIndex = 0;

                this.CurrentPageIndex = this.gridTimesheetCorrection.CurrentPageIndex + 1;
                this.CurrentPageSize = this.gridTimesheetCorrection.PageSize;

                InitializeTimesheetCorrectionGrid();
                InitializeShiftPatternChangeGrid();
                InitializeAbsenceHistoryGrid();
                InitializeLeaveHistoryGrid();
                #endregion
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            #region Perform Data Validation
            int errorCount = 0;

            #region Check selected employee 
            int empNo = UIHelper.ConvertObjectToInt(this.txtEmpNo.Text);
            if (empNo.ToString().Length == 4)
            {
                empNo += 10000000;

                // Display the formatted Emp. No.
                this.txtEmpNo.Text = empNo.ToString();

                if (this.EmployeeNo == 0)
                    this.btnGet_Click(this.btnGet, new EventArgs());
            }

            if (empNo == 0)
            {
                this.txtGeneric.Text = ValidationErrorType.NoEmpNo.ToString();
                this.ErrorType = ValidationErrorType.NoEmpNo;
                this.cusValEmpNo.Validate();
                errorCount++;

                this.litEmpName.Text = "Not defined";
                this.litPosition.Text = "Not defined";
                this.litCostCenter.Text = "Not defined";
            }
            #endregion

            if (errorCount > 0)
            {
                InitializeTimesheetCorrectionGrid();
                InitializeShiftPatternChangeGrid();
                InitializeAbsenceHistoryGrid();
                InitializeLeaveHistoryGrid();

                // Set focus to the top panel
                Page.SetFocus(this.lnkMoveUp.ClientID);
                return;
            }

            #endregion

            // Reset page index
            this.gridTimesheetCorrection.CurrentPageIndex = 0;
            this.CurrentPageIndex = this.gridTimesheetCorrection.CurrentPageIndex + 1;
            this.CurrentPageSize = this.gridTimesheetCorrection.PageSize;

            GetTimesheetCorrectionHistory(true);
        }

        protected void btnGet_Click(object sender, EventArgs e)
        {
            try
            {
                #region Perform data validation
                // Check Employee No.
                if (UIHelper.ConvertObjectToInt(this.txtEmpNo.Text) == 0)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoSpecifiedEmpNo.ToString();
                    this.ErrorType = ValidationErrorType.NoSpecifiedEmpNo;
                    this.cusValEmpNo.Validate();
                    return;
                }
                #endregion

                #region Initialize control values and variables
                this.litEmpName.Text = "Not defined";
                this.litPosition.Text = "Not defined";
                this.litCostCenter.Text = "Not defined";
                #endregion

                int empNo = UIHelper.ConvertObjectToInt(this.txtEmpNo.Text);
                if (empNo.ToString().Length == 4)
                {
                    empNo += 10000000;

                    // Display the formatted Emp. No.
                    this.txtEmpNo.Text = empNo.ToString();
                }

                string error = string.Empty;
                string innerError = string.Empty;

                EmployeeInfo empInfo = UIHelper.GetEmployeeInfo(empNo);
                if (empInfo != null)
                {
                    if (UIHelper.ConvertObjectToInt(empInfo.EmployeeNo) > 0)
                    {
                        #region Check if cost center exist in the allowed cost center list
                        //if (this.Master.AllowedCostCenterList.Count > 0)
                        //{
                        //    string allowedCC = this.Master.AllowedCostCenterList
                        //        .Where(a => a == UIHelper.ConvertObjectToString(empInfo.CostCenter))
                        //        .FirstOrDefault();
                        //    if (!string.IsNullOrEmpty(allowedCC))
                        //    {
                        this.litEmpName.Text = UIHelper.ConvertObjectToString(empInfo.FullName);
                        this.litPosition.Text = UIHelper.ConvertObjectToString(empInfo.PositionDesc);
                        this.litCostCenter.Text = string.Format("{0} - {1}",
                            empInfo.CostCenter,
                            empInfo.CostCenterName);
                        //    }
                        //    else
                        //    {
                        //        DisplayFormLevelError("Sorry, you don't have access permission to view the information of the specified Employee No. Please check with ICT or create a Helpdesk Request!");
                        //    }
                        //}
                        #endregion
                    }
                    else
                    {
                        #region Get employee info from the employee master
                        DALProxy proxy = new DALProxy();
                        var rawData = proxy.GetEmployeeDetail(empNo, ref error, ref innerError);
                        if (rawData != null)
                        {
                            //if (this.Master.AllowedCostCenterList.Count > 0)
                            //{
                            //    string allowedCC = this.Master.AllowedCostCenterList
                            //        .Where(a => a == UIHelper.ConvertObjectToString(rawData.CostCenter))
                            //        .FirstOrDefault();
                            //    if (!string.IsNullOrEmpty(allowedCC))
                            //    {
                            this.litEmpName.Text = UIHelper.ConvertObjectToString(rawData.EmpName);
                            this.litPosition.Text = UIHelper.ConvertObjectToString(rawData.Position);
                            this.litCostCenter.Text = string.Format("{0} - {1}",
                               rawData.CostCenter,
                               rawData.CostCenterName);
                            //    }
                            //    else
                            //    {
                            //        DisplayFormLevelError("Sorry, you don't have access permission to view the information of the specified Employee No. Please check with ICT or create a Helpdesk Request!");
                            //    }
                            //}
                        }
                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex);
            }
        }

        protected void btnFindEmployee_Click(object sender, EventArgs e)
        {
            StoreDataToCollection(UIHelper.PagePostBackFlags.GetEmployeeInfo);

            Response.Redirect
            (
                String.Format(UIHelper.PAGE_EMPLOYEE_SEARCH + "?{0}={1}",
                UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                UIHelper.PAGE_TIMESHEET_CORRECTION_HISTORY
            ),
            false);
        }

        protected void btnRebind_Click(object sender, EventArgs e)
        {
            RebindDataToTimesheetCorrectionGrid();
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.CallerForm))
                Response.Redirect(this.CallerForm, false);
            else
                Response.Redirect(UIHelper.PAGE_HOME, false);
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
                else if (this.ErrorType == ValidationErrorType.NoSpecifiedEmpNo)
                {
                    validator.ErrorMessage = "Please specify the Employee No. (Note: Make sure that the specified employee is active and exists in the Employee Master.)";
                    validator.ToolTip = "Please specify the Employee No. (Note: Make sure that the specified employee is active and exists in the Employee Master.)";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoEmpNo)
                {
                    validator.ErrorMessage = "Employee No. is required.";
                    validator.ToolTip = "Employee No. is required.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoRecordToDelete)
                {
                    validator.ErrorMessage = "Please select the record to delete in the grid.";
                    validator.ToolTip = "Please select the record to delete in the grid.";
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

        protected void tabMain_TabClick(object sender, RadTabStripEventArgs e)
        {
            RadTab selected = e.Tab;
            if (selected.Value == TabSelection.valTimesheetHistory.ToString())
            {
                #region Fill data to Timesheet Correction History grid
                GetTimesheetCorrectionHistory(this.TimesheetCorrectionDataList.Count == 0);
                #endregion
            }
            else if (selected.Value == TabSelection.valShiftPatternHistory.ToString())
            {
                #region Fill data to Shift Pattern Change History grid
                GetShiftPatternChangeHistory(this.ShiftPatternHistoryDataList.Count == 0);
                #endregion
            }
            else if (selected.Value == TabSelection.valAbsenceHistory.ToString())
            {
                #region Fill data to Absences History grid
                GetAbsencesHistory(this.AbsenceHistoryDataList.Count == 0);
                #endregion
            }
            else if (selected.Value == TabSelection.valLeaveHistory.ToString())
            {
                #region Fill data to Leave History grid
                GetLeaveHistory(this.LeaveHistoryDataList.Count == 0);
                #endregion
            }
        }
        #endregion

        #region IFormExtension Interface Implementation
        public void ClearForm()
        {
            #region Reset controls
            this.txtEmpNo.Text = string.Empty;
            this.litEmpName.Text = "Not defined";
            this.litPosition.Text = "Not defined";
            this.litCostCenter.Text = "Not defined";
            this.litAttendanceDate.Text = "Not defined";
            #endregion

            // Clear collections
            KillSessions();

            // Reset the grid
            this.gridTimesheetCorrection.VirtualItemCount = 1;
            this.gridShiftPatternHistory.VirtualItemCount = 1;
            this.gridAbsenceHistory.VirtualItemCount = 1;
            this.gridLeaveHistory.VirtualItemCount = 1;

            this.gridTimesheetCorrection.CurrentPageIndex = 0;
            this.gridShiftPatternHistory.CurrentPageIndex = 0;
            this.gridAbsenceHistory.CurrentPageIndex = 0;
            this.gridLeaveHistory.CurrentPageIndex = 0;

            this.CurrentPageIndex = this.gridTimesheetCorrection.CurrentPageIndex + 1;
            this.CurrentPageSize = this.gridTimesheetCorrection.PageSize;

            InitializeTimesheetCorrectionGrid();
            InitializeShiftPatternChangeGrid();
            InitializeAbsenceHistoryGrid();
            InitializeLeaveHistoryGrid();
        }

        public void AddControlsAttribute()
        {

        }

        public void ProcessQueryString()
        {
            this.CallerForm = UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_CALLER_FORM_KEY]);
            this.ReloadGridData = UIHelper.ConvertObjectToBolean(Request.QueryString[UIHelper.QUERY_STRING_RELOAD_DATA_KEY]);
            this.ParamAutoID = UIHelper.ConvertObjectToInt(Request.QueryString[UIHelper.QUERY_STRING_IDENTITY_FIELD_KEY]);

            #region Determine the Form Data Load Type
            string formLoadType = UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_FORM_LOAD_TYPE]);
            if (formLoadType != string.Empty)
            {
                UIHelper.FormDataLoadType loadType = UIHelper.FormDataLoadType.OpenSpecificTimesheetRecord;
                try
                {
                    loadType = (UIHelper.FormDataLoadType)Enum.Parse(typeof(UIHelper.FormDataLoadType), formLoadType);
                }
                catch (Exception)
                {
                }
                this.CurrentFormLoadType = loadType;
            }
            #endregion
        }

        public void KillSessions()
        {
            // Cler collections
            this.TimesheetCorrectionDataList.Clear();
            this.ShiftPatternHistoryDataList.Clear();
            this.AbsenceHistoryDataList.Clear();
            this.LeaveHistoryDataList.Clear();

            // Clear sessions
            ViewState["CustomErrorMsg"] = null;
            ViewState["CurrentStartRowIndex"] = null;
            ViewState["CurrentMaximumRows"] = null;
            ViewState["CurrentPageIndex"] = null;
            ViewState["CurrentPageSize"] = null;
            ViewState["EmployeeNo"] = null;
            ViewState["CallerForm"] = null;
            ViewState["ParamAutoID"] = null;

            // Clear all viewstates
            ViewState.Clear();
        }
        #endregion

        #region Private Methods
        private void RestoreDataFromCollection()
        {
            if (this.TimesheetCorrectionStorage.Count == 0)
                return;

            #region Restore Query String values
            if (this.TimesheetCorrectionStorage.ContainsKey("CallerForm"))
                this.CallerForm = UIHelper.ConvertObjectToString(this.TimesheetCorrectionStorage["CallerForm"]);
            else
                this.CallerForm = string.Empty;

            if (this.TimesheetCorrectionStorage.ContainsKey("ReloadGridData"))
                this.ReloadGridData = UIHelper.ConvertObjectToBolean(this.TimesheetCorrectionStorage["ReloadGridData"]);
            else
                this.ReloadGridData = false;

            if (this.TimesheetCorrectionStorage.ContainsKey("ParamAutoID"))
                this.ParamAutoID = UIHelper.ConvertObjectToInt(this.TimesheetCorrectionStorage["ParamAutoID"]);
            else
                this.ParamAutoID = 0;

            // Determine the Form Load Type
            if (this.TimesheetCorrectionStorage.ContainsKey("CurrentFormLoadType"))
            {
                string formLoadType = UIHelper.ConvertObjectToString(this.TimesheetCorrectionStorage["CurrentFormLoadType"]);
                if (formLoadType != string.Empty)
                {
                    UIHelper.FormDataLoadType loadType = UIHelper.FormDataLoadType.OpenSpecificTimesheetRecord;
                    try
                    {
                        loadType = (UIHelper.FormDataLoadType)Enum.Parse(typeof(UIHelper.FormDataLoadType), formLoadType);
                    }
                    catch (Exception)
                    {
                    }
                    this.CurrentFormLoadType = loadType;
                }
            }
            #endregion

            #region Restore session values
            if (this.TimesheetCorrectionStorage.ContainsKey("CurrentStartRowIndex"))
                this.CurrentStartRowIndex = UIHelper.ConvertObjectToInt(this.TimesheetCorrectionStorage["CurrentStartRowIndex"]);
            else
                this.CurrentStartRowIndex = 0;

            if (this.TimesheetCorrectionStorage.ContainsKey("CurrentMaximumRows"))
                this.CurrentMaximumRows = UIHelper.ConvertObjectToInt(this.TimesheetCorrectionStorage["CurrentMaximumRows"]);
            else
                this.CurrentMaximumRows = 0;

            if (this.TimesheetCorrectionStorage.ContainsKey("CurrentPageIndex"))
                this.CurrentPageIndex = UIHelper.ConvertObjectToInt(this.TimesheetCorrectionStorage["CurrentPageIndex"]);
            else
                this.CurrentPageIndex = 0;

            if (this.TimesheetCorrectionStorage.ContainsKey("CurrentPageSize"))
                this.CurrentPageSize = UIHelper.ConvertObjectToInt(this.TimesheetCorrectionStorage["CurrentPageSize"]);
            else
                this.CurrentPageSize = 0;

            if (this.TimesheetCorrectionStorage.ContainsKey("EmployeeNo"))
                this.EmployeeNo = UIHelper.ConvertObjectToInt(this.TimesheetCorrectionStorage["EmployeeNo"]);
            else
                this.EmployeeNo = 0;

            if (this.TimesheetCorrectionStorage.ContainsKey("TimesheetCorrectionDataList"))
                this.TimesheetCorrectionDataList = this.TimesheetCorrectionStorage["TimesheetCorrectionDataList"] as List<EmployeeAttendanceEntity>;
            else
                this.TimesheetCorrectionDataList = null;

            if (this.TimesheetCorrectionStorage.ContainsKey("ShiftPatternHistoryDataList"))
                this.ShiftPatternHistoryDataList = this.TimesheetCorrectionStorage["ShiftPatternHistoryDataList"] as List<ShiftPatternEntity>;
            else
                this.ShiftPatternHistoryDataList = null;

            if (this.TimesheetCorrectionStorage.ContainsKey("AbsenceHistoryDataList"))
                this.AbsenceHistoryDataList = this.TimesheetCorrectionStorage["AbsenceHistoryDataList"] as List<ReasonOfAbsenceEntity>;
            else
                this.AbsenceHistoryDataList = null;

            if (this.TimesheetCorrectionStorage.ContainsKey("LeaveHistoryDataList"))
                this.LeaveHistoryDataList = this.TimesheetCorrectionStorage["LeaveHistoryDataList"] as List<LeaveEntity>;
            else
                this.LeaveHistoryDataList = null;

            FillComboData(false);
            #endregion

            #region Restore control values            
            if (this.TimesheetCorrectionStorage.ContainsKey("txtEmpNo"))
                this.txtEmpNo.Text = UIHelper.ConvertObjectToString(this.TimesheetCorrectionStorage["txtEmpNo"]);
            else
                this.txtEmpNo.Text = string.Empty;

            if (this.TimesheetCorrectionStorage.ContainsKey("litEmpName"))
                this.litEmpName.Text = UIHelper.ConvertObjectToString(this.TimesheetCorrectionStorage["litEmpName"]);
            else
                this.litEmpName.Text = string.Empty;

            if (this.TimesheetCorrectionStorage.ContainsKey("litPosition"))
                this.litPosition.Text = UIHelper.ConvertObjectToString(this.TimesheetCorrectionStorage["litPosition"]);
            else
                this.litPosition.Text = string.Empty;

            if (this.TimesheetCorrectionStorage.ContainsKey("litCostCenter"))
                this.litCostCenter.Text = UIHelper.ConvertObjectToString(this.TimesheetCorrectionStorage["litCostCenter"]);
            else
                this.litCostCenter.Text = string.Empty;

            if (this.TimesheetCorrectionStorage.ContainsKey("litAttendanceDate"))
                this.litAttendanceDate.Text = UIHelper.ConvertObjectToString(this.TimesheetCorrectionStorage["litAttendanceDate"]);
            else
                this.litAttendanceDate.Text = string.Empty;
            #endregion

            // Refresh the grid
            RebindDataToTimesheetCorrectionGrid();

            // Set the grid attributes
            this.gridTimesheetCorrection.CurrentPageIndex = this.CurrentPageIndex;
            this.gridTimesheetCorrection.MasterTableView.CurrentPageIndex = this.CurrentPageIndex;
            this.gridTimesheetCorrection.MasterTableView.PageSize = this.CurrentPageSize;
            this.gridTimesheetCorrection.MasterTableView.DataBind();
        }

        private void StoreDataToCollection(UIHelper.PagePostBackFlags formFlag)
        {
            this.TimesheetCorrectionStorage.Clear();
            this.TimesheetCorrectionStorage.Add("FormFlag", formFlag.ToString());

            #region Save control values to session
            this.TimesheetCorrectionStorage.Add("txtEmpNo", this.txtEmpNo.Text.Trim());
            this.TimesheetCorrectionStorage.Add("litEmpName", this.litEmpName.Text.Trim());
            this.TimesheetCorrectionStorage.Add("litPosition", this.litPosition.Text.Trim());
            this.TimesheetCorrectionStorage.Add("litCostCenter", this.litCostCenter.Text.Trim());
            this.TimesheetCorrectionStorage.Add("litAttendanceDate", this.litAttendanceDate.Text.Trim());
            #endregion

            #region Save Query String values to collection
            this.TimesheetCorrectionStorage.Add("CallerForm", this.CallerForm);
            this.TimesheetCorrectionStorage.Add("ReloadGridData", this.ReloadGridData);
            this.TimesheetCorrectionStorage.Add("ParamAutoID", this.ParamAutoID);
            #endregion

            #region Store session data to collection
            this.TimesheetCorrectionStorage.Add("CurrentStartRowIndex", this.CurrentStartRowIndex);
            this.TimesheetCorrectionStorage.Add("CurrentMaximumRows", this.CurrentMaximumRows);
            this.TimesheetCorrectionStorage.Add("CurrentPageIndex", this.CurrentPageIndex);
            this.TimesheetCorrectionStorage.Add("CurrentPageSize", this.CurrentPageSize);
            this.TimesheetCorrectionStorage.Add("EmployeeNo", this.EmployeeNo);
            this.TimesheetCorrectionStorage.Add("TimesheetCorrectionDataList", this.TimesheetCorrectionDataList);
            this.TimesheetCorrectionStorage.Add("ShiftPatternHistoryDataList", this.ShiftPatternHistoryDataList);
            this.TimesheetCorrectionStorage.Add("AbsenceHistoryDataList", this.AbsenceHistoryDataList);
            this.TimesheetCorrectionStorage.Add("LeaveHistoryDataList", this.LeaveHistoryDataList);
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
            
        }
        #endregion

        #region Database Access
        private void GetTimesheetCorrectionHistory(bool reloadDataFromDB = false, int paramAutoID = 0)
        {
            try
            {
                #region Initialize variables  
                // Initialize record count
                this.lblRecordCount.Text = "0 record found";
                this.gridTimesheetCorrection.VirtualItemCount = 1;
                #endregion

                #region Fill data in the grid
                List<EmployeeAttendanceEntity> gridSource = new List<EmployeeAttendanceEntity>();
                if (!reloadDataFromDB)
                {
                    gridSource = this.TimesheetCorrectionDataList;
                }
                else
                {
                    #region Fetch data from database
                    int autoID = 0;

                    if (this.CurrentTimesheetRecord != null)
                        autoID = this.CurrentTimesheetRecord.AutoID;

                    if (paramAutoID > 0)
                        autoID = paramAutoID;

                    if (autoID > 0)
                    {
                        string error = string.Empty;
                        string innerError = string.Empty;

                        DALProxy proxy = new DALProxy();
                        var source = proxy.GetTimesheetHistory(autoID, this.CurrentPageIndex, this.CurrentPageSize, ref error, ref innerError);
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
                    #endregion
                }

                // Store collection to session
                this.TimesheetCorrectionDataList = gridSource;
                #endregion

                #region Bind data to the grid
                if (this.TimesheetCorrectionDataList.Count > 0)
                {
                    int totalRecords = this.TimesheetCorrectionDataList.FirstOrDefault().TotalRecords;
                    if (totalRecords > 0)
                        this.gridTimesheetCorrection.VirtualItemCount = totalRecords;
                    else
                        this.gridTimesheetCorrection.VirtualItemCount = 1;

                    this.gridTimesheetCorrection.DataSource = this.TimesheetCorrectionDataList;
                    this.gridTimesheetCorrection.DataBind();

                    //Display the record count
                    this.lblRecordCount.Text = string.Format("{0} record(s) found", totalRecords.ToString("#,###"));
                }
                else
                    InitializeTimesheetCorrectionGrid();
                #endregion
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        private void GetShiftPatternChangeHistory(bool reloadDataFromDB = false)
        {
            try
            {
                #region Initialize variables  
                // Initialize record count
                this.lblRecordCount.Text = "0 record found";
                this.gridShiftPatternHistory.VirtualItemCount = 1;                                
                #endregion

                #region Fill data in the grid                
                List<ShiftPatternEntity> gridSource = new List<ShiftPatternEntity>();
                if (!reloadDataFromDB)
                {
                    gridSource = this.ShiftPatternHistoryDataList;
                }
                else
                {
                    #region Fetch data from database
                    if (this.CurrentTimesheetRecord != null)
                    {
                        string error = string.Empty;
                        string innerError = string.Empty;
                        int empNo = this.CurrentTimesheetRecord.EmpNo;
                        DateTime? DT = this.CurrentTimesheetRecord.DT;

                        DALProxy proxy = new DALProxy();
                        var source = proxy.GetShiftPatternChangeHistory(empNo, DT, this.CurrentPageIndex, this.CurrentPageSize, ref error, ref innerError);
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
                    #endregion
                }

                // Store collection to session
                this.ShiftPatternHistoryDataList = gridSource;
                #endregion

                #region Bind data to the grid
                if (this.ShiftPatternHistoryDataList.Count > 0)
                {
                    int totalRecords = this.ShiftPatternHistoryDataList.FirstOrDefault().TotalRecords;
                    if (totalRecords > 0)
                        this.gridShiftPatternHistory.VirtualItemCount = totalRecords;
                    else
                        this.gridShiftPatternHistory.VirtualItemCount = 1;

                    this.gridShiftPatternHistory.DataSource = this.ShiftPatternHistoryDataList;
                    this.gridShiftPatternHistory.DataBind();

                    //Display the record count
                    //this.lblRecordCount.Text = string.Format("{0} record(s) found", totalRecords.ToString("#,###"));
                }
                else
                    InitializeShiftPatternChangeGrid();
                #endregion
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        private void GetAbsencesHistory(bool reloadDataFromDB = false)
        {
            try
            {
                #region Initialize variables  
                // Initialize record count
                this.lblRecordCount.Text = "0 record found";
                this.gridAbsenceHistory.VirtualItemCount = 1;
                #endregion

                #region Fill data in the grid
                List<ReasonOfAbsenceEntity> gridSource = new List<ReasonOfAbsenceEntity>();
                if (!reloadDataFromDB)
                {
                    gridSource = this.AbsenceHistoryDataList;
                }
                else
                {
                    #region Fetch data from database
                    if (this.CurrentTimesheetRecord != null)
                    {
                        string error = string.Empty;
                        string innerError = string.Empty;
                        int empNo = this.CurrentTimesheetRecord.EmpNo;
                        DateTime? DT = this.CurrentTimesheetRecord.DT;

                        DALProxy proxy = new DALProxy();
                        var source = proxy.GetEmployeeAbsenceHistory(empNo, DT, this.CurrentPageIndex, this.CurrentPageSize, ref error, ref innerError);
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
                    #endregion
                }

                // Store collection to session
                this.AbsenceHistoryDataList = gridSource;
                #endregion

                #region Bind data to the grid
                if (this.AbsenceHistoryDataList.Count > 0)
                {
                    int totalRecords = this.AbsenceHistoryDataList.FirstOrDefault().TotalRecords;
                    if (totalRecords > 0)
                        this.gridAbsenceHistory.VirtualItemCount = totalRecords;
                    else
                        this.gridAbsenceHistory.VirtualItemCount = 1;

                    this.gridAbsenceHistory.DataSource = this.AbsenceHistoryDataList;
                    this.gridAbsenceHistory.DataBind();

                    //Display the record count
                    //this.lblRecordCount.Text = string.Format("{0} record(s) found", totalRecords.ToString("#,###"));
                }
                else
                    InitializeAbsenceHistoryGrid();
                #endregion
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        private void GetLeaveHistory(bool reloadDataFromDB = false)
        {
            try
            {
                #region Initialize variables  
                // Initialize record count
                this.lblRecordCount.Text = "0 record found";
                this.gridLeaveHistory.VirtualItemCount = 1;
                #endregion

                #region Fill data in the grid
                List<LeaveEntity> gridSource = new List<LeaveEntity>();
                if (!reloadDataFromDB)
                {
                    gridSource = this.LeaveHistoryDataList;
                }
                else
                {
                    #region Fetch data from database
                    if (this.CurrentTimesheetRecord != null)
                    {
                        string error = string.Empty;
                        string innerError = string.Empty;
                        int empNo = this.CurrentTimesheetRecord.EmpNo;
                        DateTime? DT = this.CurrentTimesheetRecord.DT;

                        DALProxy proxy = new DALProxy();
                        var source = proxy.GetEmployeeLeaveHistory(empNo, DT, this.CurrentPageIndex, this.CurrentPageSize, ref error, ref innerError);
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
                    #endregion
                }

                // Store collection to session
                this.LeaveHistoryDataList = gridSource;
                #endregion

                #region Bind data to the grid
                if (this.LeaveHistoryDataList.Count > 0)
                {
                    int totalRecords = this.LeaveHistoryDataList.FirstOrDefault().TotalRecords;
                    if (totalRecords > 0)
                        this.gridLeaveHistory.VirtualItemCount = totalRecords;
                    else
                        this.gridLeaveHistory.VirtualItemCount = 1;

                    this.gridLeaveHistory.DataSource = this.LeaveHistoryDataList;
                    this.gridLeaveHistory.DataBind();
                }
                else
                    InitializeLeaveHistoryGrid();
                #endregion
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }
        #endregion
    }
}