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
    public partial class FireTeamFireWatch : BaseWebForm, IFormExtension
    {
        #region Enumeration
        private enum ValidationErrorType
        {
            CustomFormError,
            NoError,
            NoGroupType
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

        private int CurrentPageSize
        {
            get
            {
                int pageSize = UIHelper.ConvertObjectToInt(ViewState["CurrentPageSize"]);
                if (pageSize == 0)
                    pageSize = this.gridSearchResult.MasterTableView.PageSize;

                return pageSize;
            }
            set
            {
                ViewState["CurrentPageSize"] = value;
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

                    // Select all work shifts
                    foreach (RadComboBoxItem item in this.cboShift.Items)
                    {
                        item.Selected = true;
                        item.Checked = true;
                    }
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
            try
            {
                #region Perform Data Validation
                int errorCount = 0;
                if (this.cboGroupType.SelectedValue == "valAll")
                {
                    this.txtGeneric.Text = ValidationErrorType.NoGroupType.ToString();
                    this.ErrorType = ValidationErrorType.NoGroupType;
                    this.cusValGroupType.Validate();
                    errorCount++;
                }

                if (errorCount > 0)
                    return;
                #endregion

                // Reset page index
                this.gridSearchResult.CurrentPageIndex = 0;
                this.CurrentPageIndex = this.gridSearchResult.CurrentPageIndex + 1;
                this.CurrentPageSize = this.gridSearchResult.PageSize;

                GetFireTeamAndFireWatch(true);
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            ClearForm();
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
        #endregion

        #region Grid Events and Methods
        protected void gridSearchResult_PageIndexChanged(object sender, Telerik.Web.UI.GridPageChangedEventArgs e)
        {
            // Store page index to session
            this.CurrentPageIndex = e.NewPageIndex + 1;

            // Fill data to the grid
            GetFireTeamAndFireWatch(true);

            // Set focus to the top panel
            Page.SetFocus(this.lnkMoveUp.ClientID);
        }

        protected void gridSearchResult_PageSizeChanged(object sender, Telerik.Web.UI.GridPageSizeChangedEventArgs e)
        {
            // Store page size to session
            this.CurrentPageSize = e.NewPageSize;
            this.CurrentPageIndex = 1;

            // Fill data to the grid            
            GetFireTeamAndFireWatch(true);

            // Set focus to the top panel
            Page.SetFocus(this.lnkMoveUp.ClientID);
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
            try
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
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex);
            }
        }

        private void RebindDataToGrid()
        {
            if (this.FireTeamMemberList.Count > 0)
            {
                int totalRecords = this.FireTeamMemberList.FirstOrDefault().TotalRecords;
                if (totalRecords > 0)
                    this.gridSearchResult.VirtualItemCount = totalRecords;
                else
                    this.gridSearchResult.VirtualItemCount = 1;

                this.gridSearchResult.DataSource = this.FireTeamMemberList;
                this.gridSearchResult.DataBind();

                //Display the record count
                this.lblRecordCount.Text = string.Format("{0} record(s) found", totalRecords.ToString("#,###"));
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

            if (this.FireTeamStorage.ContainsKey("CurrentPageSize"))
                this.CurrentPageSize = UIHelper.ConvertObjectToInt(this.FireTeamStorage["CurrentPageSize"]);
            else
                this.CurrentPageSize = 0;
            #endregion

            #region Restore control values
            if (this.FireTeamStorage.ContainsKey("cboCostCenter"))
                this.cboGroupType.SelectedValue = UIHelper.ConvertObjectToString(this.FireTeamStorage["cboCostCenter"]);
            else
            {
                this.cboGroupType.SelectedIndex = -1;
                this.cboGroupType.Text = string.Empty;
            }

            if (this.FireTeamStorage.ContainsKey("cboCostCenter"))
                this.cboCostCenter.Text = UIHelper.ConvertObjectToString(this.FireTeamStorage["cboCostCenter"]);
            else
                this.cboCostCenter.Text = string.Empty;

            if (this.FireTeamStorage.ContainsKey("txtEmp"))
                this.txtEmp.Text = UIHelper.ConvertObjectToString(this.FireTeamStorage["txtEmp"]);
            else
                this.txtEmp.Text = string.Empty;
            #endregion

            // Refresh the grid
            RebindDataToGrid();
            // Set the grid attributes
            this.gridSearchResult.CurrentPageIndex = this.CurrentPageIndex;
            this.gridSearchResult.MasterTableView.CurrentPageIndex = this.CurrentPageIndex;
            this.gridSearchResult.MasterTableView.PageSize = this.CurrentPageSize;
            this.gridSearchResult.MasterTableView.DataBind();
        }

        private void StoreDataToCollection(UIHelper.PagePostBackFlags formFlag)
        {
            this.FireTeamStorage.Clear();
            this.FireTeamStorage.Add("FormFlag", formFlag.ToString());

            #region Store control values to the collection
            this.FireTeamStorage.Add("cboCostCenter", this.cboCostCenter.Text);
            this.FireTeamStorage.Add("cboGroupType", this.cboGroupType.SelectedValue);
            this.FireTeamStorage.Add("txtEmp", this.txtEmp.Text);
            #endregion

            // Store session data to the collection
            this.FireTeamStorage.Add("FireTeamMemberList", this.FireTeamMemberList);
            this.FireTeamStorage.Add("CurrentStartRowIndex", this.CurrentStartRowIndex);
            this.FireTeamStorage.Add("CurrentMaximumRows", this.CurrentMaximumRows);
            this.FireTeamStorage.Add("CurrentPageIndex", this.CurrentPageIndex);
            this.FireTeamStorage.Add("CurrentPageSize", this.CurrentPageSize);
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
            //EmployeeDAL.CostCenterRow defaultRow = filteredDT.NewCostCenterRow();
            //defaultRow.CostCenter = String.Empty;
            //defaultRow.CostCenterName = "Please select a Cost Center...";
            //defaultRow.Company = String.Empty;
            //defaultRow.SuperintendentNo = 0;
            //defaultRow.SuperintendentName = String.Empty;
            //defaultRow.ManagerNo = 0;
            //defaultRow.ManagerName = String.Empty;
            //filteredDT.Rows.Add(defaultRow);
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
        #endregion

        #region IFormExtension Interface Implementation
        public void ClearForm()
        {
            #region Reset Controls
            this.cboCostCenter.Text = string.Empty;
            this.cboCostCenter.SelectedIndex = -1;
            this.cboGroupType.SelectedIndex = -1;
            this.cboGroupType.Text = string.Empty;
            this.cboShift.ClearCheckedItems();
            this.cboShift.ClearSelection();
            this.cboShift.Text = string.Empty;
            this.txtEmp.Text = string.Empty;                        
            #endregion

            // Clear collections
            this.FireTeamMemberList.Clear();

            KillSessions();

            // Reset the grid
            this.gridSearchResult.VirtualItemCount = 1;
            this.gridSearchResult.CurrentPageIndex = 0;
            this.CurrentPageIndex = this.gridSearchResult.CurrentPageIndex + 1;
            this.CurrentPageSize = this.gridSearchResult.PageSize;
            InitializeGrid();
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
            ViewState["CurrentPageSize"] = null;
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

        private void GetFireTeamAndFireWatch(bool reloadFromDB = false)
        {
            try
            {
                #region Initialize variables                                
                DateTime processDate = DateTime.Now.Date;
                string costCenter = this.cboCostCenter.Text.Trim();
                int empNo = UIHelper.ConvertObjectToInt(this.txtEmp.Text);
                if (empNo.ToString().Length == 4)
                {
                    empNo += 10000000;

                    // Display Emp. No.
                    this.txtEmp.Text = empNo.ToString();
                }

                byte loadType = 0;
                if (this.cboGroupType.SelectedValue == "valAvailableFireTeam")
                    loadType = 1;
                else if (this.cboGroupType.SelectedValue == "valAvailableFireWatch")
                    loadType = 2;
                else if (this.cboGroupType.SelectedValue == "valAvailableFireTeamFireWatch")
                    loadType = 3;
                else if (this.cboGroupType.SelectedValue == "valAllFireTeam")
                    loadType = 4;
                else if (this.cboGroupType.SelectedValue == "valAllFireWatch")
                    loadType = 5;

                string shiftCodeArray = string.Empty;
                if (this.cboShift.CheckedItems.Count > 0)
                {
                    foreach (var item in this.cboShift.CheckedItems)
                    {
                        if (shiftCodeArray.Length == 0)
                            shiftCodeArray = item.Value;
                        else
                            shiftCodeArray = string.Format("{0}, {1}", shiftCodeArray, item.Value);
                    }
                }

                // Initialize record count
                this.lblRecordCount.Text = "0 record found";
                this.gridSearchResult.VirtualItemCount = 1;
                #endregion

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
                    string imageRootPath = UIHelper.ConvertObjectToString(ConfigurationManager.AppSettings["ImageRootPath"]);

                    // Fetch data from the database
                    var source = dataProxy.GetFireTeamAndFireWatchWithPaging(loadType, processDate, shiftCodeArray, empNo, costCenter, this.CurrentPageIndex, this.CurrentPageSize, imageRootPath, ref error, ref innerError);
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
                if (this.FireTeamMemberList.Count > 0)
                {
                    int totalRecords = this.FireTeamMemberList.FirstOrDefault().TotalRecords;
                    if (totalRecords > 0)
                        this.gridSearchResult.VirtualItemCount = totalRecords;
                    else
                        this.gridSearchResult.VirtualItemCount = 1;

                    this.gridSearchResult.DataSource = this.FireTeamMemberList;
                    this.gridSearchResult.DataBind();

                    //Display the record count
                    this.lblRecordCount.Text = string.Format("{0} record(s) found", totalRecords.ToString("#,###"));
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