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

namespace GARMCO.AMS.TAS.UI.Views.HRFunctions
{
    public partial class OvertimeApproval : BaseWebForm, IFormExtension
    {
        #region Enumeration
        private enum ValidationErrorType
        {
            NoError,
            CustomFormError,
            NoStartDate,
            InvalidDateRange,
            InvalidYear
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

        private Dictionary<string, object> OvertimeApprovalStorage
        {
            get
            {
                Dictionary<string, object> list = Session["OvertimeApprovalStorage"] as Dictionary<string, object>;
                if (list == null)
                    Session["OvertimeApprovalStorage"] = list = new Dictionary<string, object>();

                return list;
            }
            set
            {
                Session["OvertimeApprovalStorage"] = value;
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

        private List<UDCEntity> OTReasonList
        {
            get
            {
                List<UDCEntity> list = ViewState["OTReasonList"] as List<UDCEntity>;
                if (list == null)
                    ViewState["OTReasonList"] = list = new List<UDCEntity>();

                return list;
            }
            set
            {
                ViewState["OTReasonList"] = value;
            }
        }

        private EmployeeAttendanceEntity SelectedOvertimeRecord
        {
            get
            {
                return ViewState["SelectedOvertimeRecord"] as EmployeeAttendanceEntity;
            }
            set
            {
                ViewState["SelectedOvertimeRecord"] = value;
            }
        }

        private bool IsOTApprove
        {
            get
            {
                return UIHelper.ConvertObjectToBolean(ViewState["IsOTApprove"]);
            }
            set
            {
                ViewState["IsOTApprove"] = value;
            }
        }

        private bool IsOTApprovalHeaderClicked
        {
            get
            {
                return UIHelper.ConvertObjectToBolean(ViewState["IsOTApprovalHeaderClicked"]);
            }
            set
            {
                ViewState["IsOTApprovalHeaderClicked"] = value;
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

                this.Master.SetPageForm(UIHelper.FormAccessCodes.OTAPPROVAL.ToString());

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
                this.Master.FormTitle = UIHelper.PAGE_OVERTIME_APPROVAL_TITLE;
                #endregion

                #region Check if user has permission to access the page
                if (!GAPFunction.CheckFormAccess(this.Master.FormAccess, GAPConstants.FormAccessIndex.Retrieve))
                {
                    if (!UIHelper.ConvertObjectToBolean(Session[UIHelper.GARMCO_USER_IS_SPECIAL]))
                    {
                        Response.Redirect(String.Format("{0}?error={1}&pageName={2}", UIHelper.PAGE_ERROR, Convert.ToInt32(UIHelper.PageErrorCodes.NoAccessOnPage), UIHelper.PAGE_OVERTIME_APPROVAL_TITLE), true);
                    }
                }
                #endregion

                #region Checks if current user has permission to create, update, delete and print
                this.btnSave.Enabled = this.Master.IsEditAllowed;
                #endregion

                #region Set default button
                this.Master.DefaultButton = this.btnSearch.UniqueID;
                #endregion

                #region Restore saved data from local storage
                string formFlag = string.Empty;
                if (this.OvertimeApprovalStorage.Count > 0)
                {
                    if (this.OvertimeApprovalStorage.ContainsKey("FormFlag"))
                        formFlag = UIHelper.ConvertObjectToString(this.OvertimeApprovalStorage["FormFlag"]);
                }
                #endregion

                if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.GetEmployeeInfo.ToString())
                {
                    #region Get the employee info
                    RestoreDataFromCollection();

                    if (UIHelper.ConvertObjectToInt(Session[UIHelper.CONST_EMPLOYEE_SEARCH_FLAG]) == 1)
                    {
                        this.txtEmpNo.Value = UIHelper.ConvertObjectToInt(Request.QueryString[UIHelper.QUERY_STRING_EMPNO_KEY]);
                    }

                    // Clear data storage
                    Session.Remove("OvertimeApprovalStorage");
                    #endregion
                }
                else if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.RedirectToOtherPage.ToString())
                {
                    #region Show the last form data
                    RestoreDataFromCollection();
                    ProcessQueryString();

                    // Clear data storage
                    Session.Remove("OvertimeApprovalStorage");

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
                    //this.chkPayPeriod.Checked = true;
                    //this.chkPayPeriod_CheckedChanged(this.chkPayPeriod, new EventArgs());

                    //int month = DateTime.Now.Month;
                    //if (DateTime.Now.Day >= 16)
                    //    month = month + 1;

                    //this.txtYear.Text = DateTime.Now.Year.ToString();
                    //this.cboMonth.SelectedValue = month.ToString();
                    //this.cboMonth_SelectedIndexChanged(this.cboMonth, new RadComboBoxSelectedIndexChangedEventArgs(this.cboMonth.Text, string.Empty, this.cboMonth.SelectedValue, string.Empty));
                    //this.cboMonth.Focus();
                    #endregion

                    // Fill data to the grid
                    //this.btnSearch_Click(this.btnSearch, new EventArgs());
                }
            }

            ScriptManager.RegisterStartupScript(this, this.GetType(), "HideScrollbar", "HideScrollbar();", true);
            AddControlsAttribute();
        }
        #endregion

        #region Grid Events
        protected void gridSearchResults_PageIndexChanged(object sender, Telerik.Web.UI.GridPageChangedEventArgs e)
        {
            RebindDataToGrid();
        }

        protected void gridSearchResults_PageSizeChanged(object sender, Telerik.Web.UI.GridPageSizeChangedEventArgs e)
        {
            RebindDataToGrid();
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
                    #region Open the Manual Timesheet data entry form
                    //dynamic itemObj = e.CommandSource;
                    //string itemText = itemObj.Text;

                    //// Get data key value
                    //long autoID = UIHelper.ConvertObjectToLong(this.gridSearchResults.MasterTableView.Items[e.Item.ItemIndex].GetDataKeyValue("AutoID"));
                    //if (autoID > 0 && this.AttendanceList.Count > 0)
                    //{
                    //    EmployeeAttendanceEntity selectedRecord = this.AttendanceList
                    //        .Where(a => a.AutoID == autoID)
                    //        .FirstOrDefault();
                    //    if (selectedRecord != null && autoID > 0)
                    //    {
                    //        // Save to session
                    //        Session["SelectedReasonOfAbsence"] = selectedRecord;
                    //    }
                    //}

                    //if (!string.IsNullOrEmpty(itemText) && itemText.Trim() == (item["EditLinkButton"].Controls[0] as LinkButton).Text.Trim())
                    //{
                    //    #region Edit link is clicked
                    //    // Save session values
                    //    StoreDataToCollection(UIHelper.PagePostBackFlags.RedirectToOtherPage);

                    //    Response.Redirect
                    //   (
                    //       String.Format(UIHelper.PAGE_REASON_ABSENCE_ENTRY + "?{0}={1}&{2}={3}&{4}={5}",
                    //       UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                    //       UIHelper.PAGE_OT_MEALVOUCHER_APPROVAL,
                    //       UIHelper.QUERY_STRING_IDENTITY_FIELD_KEY,
                    //       autoID,
                    //       UIHelper.QUERY_STRING_FORM_LOAD_TYPE,
                    //       Convert.ToInt32(UIHelper.DataLoadTypes.EditExistingRecord).ToString()
                    //   ),
                    //   false);
                    //    #endregion
                    //}
                    //else if (!string.IsNullOrEmpty(itemText) && itemText.Trim() == (item["ViewLinkButton"].Controls[0] as LinkButton).Text.Trim())
                    //{
                    //    #region View link is clicked
                    //    // Save session values
                    //    StoreDataToCollection(UIHelper.PagePostBackFlags.RedirectToOtherPage);

                    //    Response.Redirect
                    //   (
                    //       String.Format(UIHelper.PAGE_REASON_ABSENCE_ENTRY + "?{0}={1}&{2}={3}&{4}={5}",
                    //       UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                    //       UIHelper.PAGE_OT_MEALVOUCHER_APPROVAL,
                    //       UIHelper.QUERY_STRING_IDENTITY_FIELD_KEY,
                    //       autoID,
                    //       UIHelper.QUERY_STRING_FORM_LOAD_TYPE,
                    //       Convert.ToInt32(UIHelper.DataLoadTypes.OpenReadonlyRecord).ToString()
                    //   ),
                    //   false);
                    //    #endregion
                    //}
                    #endregion
                }
            }
        }

        protected void gridSearchResults_ItemDataBound(object sender, GridItemEventArgs e)
        {
            #region Customize the grid pager items                        
            if (e.Item is GridPagerItem)
            {
                RadComboBox myPageSizeCombo = (RadComboBox)e.Item.FindControl("PageSizeComboBox");
                if (myPageSizeCombo != null)
                {
                    // Clear default items
                    myPageSizeCombo.Items.Clear();

                    // Add new items
                    string[] arrayPageSize = { "10", "20", "50", "100", "200" };
                    foreach (string item in arrayPageSize)
                    {
                        RadComboBoxItem cboItem = new RadComboBoxItem(item);
                        cboItem.Attributes.Add("ownerTableViewId", gridSearchResults.MasterTableView.ClientID);

                        // Add to the grid combo
                        myPageSizeCombo.Items.Add(cboItem);
                    }

                    // Get the default size
                    RadComboBoxItem cboItemDefault = myPageSizeCombo.FindItemByText(e.Item.OwnerTableView.PageSize.ToString());
                    if (cboItemDefault != null)
                        cboItemDefault.Selected = true;
                }
            }
            #endregion

            if (e.Item.ItemType == GridItemType.AlternatingItem || e.Item.ItemType == GridItemType.Item)
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {
                    bool isOTProcessed = UIHelper.ConvertObjectToBolean(item["IsOTAlreadyProcessed"].Text);

                    #region Process "OT Approved?" Header
                    foreach (GridHeaderItem headerItem in this.gridSearchResults.MasterTableView.GetItems(GridItemType.Header))
                    {
                        CheckBox chkOTApprove = (CheckBox)headerItem["OTApprovalDesc"].Controls[1]; // Get the header checkbox 
                        if (chkOTApprove != null)
                        {
                            chkOTApprove.Checked = this.IsOTApprove;
                        }
                    }
                    #endregion

                    // Process "OT Approved?"
                    RadComboBox cboOTApprovalType = (RadComboBox)item["OTApprovalDesc"].FindControl("cboOTApprovalType");
                    if (cboOTApprovalType != null && 
                        cboOTApprovalType.Items.Count > 0)
                    {
                        cboOTApprovalType.SelectedValue = UIHelper.ConvertObjectToString(item["OTApprovalCode"].Text).Replace("&nbsp;", "");
                    }

                    // Process "Meal Voucher Approved?"
                    RadComboBox cboMealVoucherEligibility = (RadComboBox)item["MealVoucherEligibility"].FindControl("cboMealVoucherEligibility");
                    if (cboMealVoucherEligibility != null && 
                        cboMealVoucherEligibility.Items.Count > 0)
                    {
                        cboMealVoucherEligibility.SelectedValue = UIHelper.ConvertObjectToString(item["MealVoucherEligibilityCode"].Text).Replace("&nbsp;", "");                                                                        
                    }

                    // Process "OT reason"
                    RadComboBox cboOTReason = (RadComboBox)item["OTReason"].FindControl("cboOTReason");
                    if (cboOTReason != null)
                    {
                        cboOTReason.SelectedValue = UIHelper.ConvertObjectToString(item["OTReasonCode"].Text).Replace("&nbsp;", "");
                    }

                    #region Enable/disable controls based on the value of "OT Approved"
                    if (cboOTApprovalType != null)
                    {
                        // Disable "OT Duration"
                        RadNumericTextBox txtDuration = (RadNumericTextBox)item["OTDurationHour"].FindControl("txtDuration");
                        if (txtDuration != null)
                        {
                            txtDuration.Enabled = false;

                            // Set the maximum input value
                            txtDuration.MaxValue = UIHelper.ConvertObjectToDouble(item["OTDurationHourClone"].Text);
                        }

                        // Disable "OT Reason"
                        if (cboOTReason != null)
                            cboOTReason.Enabled = false;

                        // Disable "Remarks"
                        TextBox txtRemarks = (TextBox)item["AttendanceRemarks"].FindControl("txtRemarks");
                        if (txtRemarks != null)
                            txtRemarks.Enabled = false;

                        if (cboOTApprovalType.SelectedValue == "Y" ||
                            cboOTApprovalType.SelectedValue == "N")
                        {
                            if (!this.IsOTApprovalHeaderClicked || isOTProcessed)
                            {
                                // Disable "OT Approved?"
                                cboOTApprovalType.Enabled = false;
                            }

                            // Enable/disable "Meal Voucher Approved?"
                            if (cboMealVoucherEligibility != null)
                            {
                                if (cboMealVoucherEligibility.SelectedValue == "YA" ||
                                    cboMealVoucherEligibility.SelectedValue == "N")
                                {
                                    cboMealVoucherEligibility.Enabled = false;
                                }
                                else
                                    cboMealVoucherEligibility.Enabled = true;
                            }
                        }
                        else
                        {
                            // Enable "OT Approved?"
                            cboOTApprovalType.Enabled = true;

                            // Dsiable "Meal Voucher Approved?"
                            if (cboMealVoucherEligibility != null)
                                cboMealVoucherEligibility.Enabled = false;
                        }
                    }
                    #endregion

                    #region Enable/disable other controls based on OT approval value
                    if (this.IsOTApprovalHeaderClicked && !isOTProcessed)
                    {
                        // Get the data key value
                        int autoID = UIHelper.ConvertObjectToInt(item["AutoID"].Text);

                        EmployeeAttendanceEntity selectedRecord = null;
                        if (autoID > 0)
                        {
                            selectedRecord = this.AttendanceList
                                .Where(a => a.AutoID == autoID)
                                .FirstOrDefault();
                        }

                        if (cboOTApprovalType.SelectedValue == "Y" ||
                            cboOTApprovalType.SelectedValue == "N")
                        {
                            #region Enable other template controls
                            // Enable "Meal Voucher Approved?"
                            //RadComboBox cboMealVoucherEligibility = (RadComboBox)item["MealVoucherEligibility"].FindControl("cboMealVoucherEligibility");
                            //if (cboMealVoucherEligibility != null)
                            //    cboMealVoucherEligibility.Enabled = true;

                            // Enable "OT Duration"
                            RadNumericTextBox txtDuration = (RadNumericTextBox)item["OTDurationHour"].FindControl("txtDuration");
                            if (txtDuration != null)
                                txtDuration.Enabled = cboOTApprovalType.SelectedValue == "Y";

                            // Enable "OT Reason"
                            //RadComboBox cboOTReason = (RadComboBox)item["OTReason"].FindControl("cboOTReason");
                            if (cboOTReason != null)
                                cboOTReason.Enabled = true;

                            // Enable "Remarks"
                            TextBox txtRemarks = (TextBox)item["AttendanceRemarks"].FindControl("txtRemarks");
                            if (txtRemarks != null)
                                txtRemarks.Enabled = true;
                            #endregion

                            #region Update data in the collection                        
                            if (selectedRecord != null)
                            {
                                // Turn on the flag to save changes in the current row
                                selectedRecord.IsDirty = true;
                            }
                            #endregion

                            #region Reload data to OT Reason combobox
                            if (cboOTApprovalType.SelectedValue == "Y")
                                FillOvertimeReasonCombo(true, 1);
                            else
                                FillOvertimeReasonCombo(true, 2);
                            #endregion
                        }
                        else
                        {
                            #region Disable other template controls
                            // Disable "Meal Voucher Approved?"
                            //RadComboBox cboMealVoucherEligibility = (RadComboBox)item["MealVoucherEligibility"].FindControl("cboMealVoucherEligibility");
                            //if (cboMealVoucherEligibility != null)
                            //    cboMealVoucherEligibility.Enabled = false;

                            // Disable "OT Duration"
                            RadNumericTextBox txtDuration = (RadNumericTextBox)item["OTDurationHour"].FindControl("txtDuration");
                            if (txtDuration != null)
                            {
                                txtDuration.Enabled = false;
                                if (this.SelectedOvertimeRecord != null)
                                    txtDuration.Text = this.SelectedOvertimeRecord.OTDurationHour.ToString();
                            }

                            // Disable "OT Reason"
                            //RadComboBox cboOTReason = (RadComboBox)item["OTReason"].FindControl("cboOTReason");
                            if (cboOTReason != null)
                            {
                                cboOTReason.Enabled = false;
                                cboOTReason.SelectedIndex = -1;
                                cboOTReason.Text = string.Empty;
                            }

                            // Disable "Remarks"
                            TextBox txtRemarks = (TextBox)item["AttendanceRemarks"].FindControl("txtRemarks");
                            if (txtRemarks != null)
                            {
                                txtRemarks.Enabled = false;
                                txtRemarks.Text = string.Empty;
                            }
                            #endregion

                            #region Update data in the collection                        
                            if (selectedRecord != null)
                            {
                                // Turn off the flag to skip saving changes in the current row
                                selectedRecord.IsDirty = false;
                            }
                            #endregion
                        }
                    }
                    #endregion
                }
            }
        }

        protected void gridSearchResults_PreRender(object sender, EventArgs e)
        {
            try
            {
                //if (this.gridSearchResults.MasterTableView.Items.Count > 0)
                //{
                //    GridDataItem item = this.gridSearchResults.MasterTableView.Items[10];
                //    if (item != null)
                //    {
                //        RadComboBox cboOTApprovalType = (RadComboBox)item["OTApprovalDesc"].FindControl("cboOTApprovalType");
                //        if (cboOTApprovalType != null)
                //        {
                //            cboOTApprovalType.SelectedValue = "";
                //        }
                //    }
                //}

                //GridColumn OTReasonColumn = this.gridSearchResults.MasterTableView.RenderColumns.Where(a => a.UniqueName == "OTReason").FirstOrDefault();
                //if (OTReasonColumn != null)
                //{
                //    OTReasonColumn.ItemStyle.Font.Bold = true;
                //    OTReasonColumn.ItemStyle.ForeColor = System.Drawing.Color.Purple;
                //}
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        private void RebindDataToGrid()
        {
            if (this.AttendanceList.Count > 0)
            {
                this.gridSearchResults.DataSource = this.AttendanceList;
                this.gridSearchResults.DataBind();

                //Display the record count
                this.lblRecordCount.Text = string.Format("{0} record(s) found", this.AttendanceList.Count.ToString("#,###"));
            }
            else
                InitializeDataToGrid();
        }

        private void InitializeDataToGrid()
        {
            this.gridSearchResults.DataSource = new List<EmployeeAttendanceEntity>();
            this.gridSearchResults.DataBind();

            this.lblRecordCount.Text = "0 record found";
        }
        #endregion

        #region Action Buttons
        protected void btnReset_Click(object sender, EventArgs e)
        {
            #region Clear the form
            this.cboMonth.Text = string.Empty;
            this.cboMonth.SelectedIndex = -1;
            this.txtYear.Text = string.Empty;
            this.dtpStartDate.SelectedDate = null;
            this.dtpEndDate.SelectedDate = null;
            this.chkPayPeriod.Checked = false;
            this.chkPayPeriod_CheckedChanged(this.chkPayPeriod, new EventArgs());

            this.txtEmpNo.Text = string.Empty;            
            this.cboCostCenter.Text = string.Empty;
            this.cboCostCenter.SelectedIndex = -1;

            // Cler collections
            this.AttendanceList.Clear();

            // Clear sessions
            ViewState["CustomErrorMsg"] = null;
            ViewState["CurrentStartRowIndex"] = null;
            ViewState["CurrentMaximumRows"] = null;
            ViewState["CurrentPageIndex"] = null;
            ViewState["CurrentPageSize"] = null;
            ViewState["ReloadGridData"] = null;
            ViewState["CallerForm"] = null;
            ViewState["SelectedOvertimeRecord"] = null;
            ViewState["IsOTApprove"] = null;
            ViewState["IsOTApprovalHeaderClicked"] = null;

            // Reset the grid
            this.gridSearchResults.CurrentPageIndex = 0;
            this.CurrentPageIndex = this.gridSearchResults.CurrentPageIndex + 1;
            this.CurrentPageSize = this.gridSearchResults.PageSize;

            InitializeDataToGrid();
            #endregion

            // Reload the data
            //this.btnSearch_Click(this.btnSearch, new EventArgs());
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            #region Perform Data Validation
            int errorCount = 0;

            // Check date range
            if (this.dtpStartDate.SelectedDate == null)
            {
                this.txtGeneric.Text = ValidationErrorType.NoStartDate.ToString();
                this.ErrorType = ValidationErrorType.NoStartDate;
                this.cusValStartDate.Validate();
                errorCount++;
            }
            else
            {
                if (this.dtpStartDate.SelectedDate != null &&
                    this.dtpEndDate.SelectedDate != null)
                {
                    if (this.dtpStartDate.SelectedDate > this.dtpEndDate.SelectedDate)
                    {
                        this.txtGeneric.Text = ValidationErrorType.InvalidDateRange.ToString();
                        this.ErrorType = ValidationErrorType.InvalidDateRange;
                        this.cusValStartDate.Validate();
                        errorCount++;
                    }
                }
            }

            if (errorCount > 0)
            {
                // Set focus to the top panel
                Page.SetFocus(this.lnkMoveUp.ClientID);
                return;
            }

            #endregion

            if (!this.ReloadGridData)
            {
                // Reset page index
                this.gridSearchResults.CurrentPageIndex = 0;
                this.CurrentPageIndex = this.gridSearchResults.CurrentPageIndex + 1;
                this.CurrentPageSize = this.gridSearchResults.PageSize;
            }

            GetOvertimeAttendance(true);
        }

        protected void btnFindEmployee_Click(object sender, EventArgs e)
        {
            StoreDataToCollection(UIHelper.PagePostBackFlags.GetEmployeeInfo);

            Response.Redirect
            (
                String.Format(UIHelper.PAGE_EMPLOYEE_SEARCH + "?{0}={1}",
                UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                UIHelper.PAGE_OT_MEALVOUCHER_APPROVAL
            ),
            false);
        }
        
        protected void btnRebind_Click(object sender, EventArgs e)
        {
            RebindDataToGrid();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (this.AttendanceList.Count == 0)
                return;

            try
            {
                int errorCount = 0;
                StringBuilder sb = new StringBuilder();
                List<EmployeeAttendanceEntity> attendanceList = new List<EmployeeAttendanceEntity>();

                #region Build the collection and populate overtime record
                foreach (GridDataItem item in this.gridSearchResults.MasterTableView.GetItems(GridItemType.Item, GridItemType.AlternatingItem))
                {
                    // Get the data key value
                    int autoID = UIHelper.ConvertObjectToInt(this.gridSearchResults.MasterTableView.Items[item.ItemIndex].GetDataKeyValue("AutoID"));
                    if (autoID > 0)
                    {
                        EmployeeAttendanceEntity selectedRecord = this.AttendanceList
                            .Where(a => a.AutoID == autoID)
                            .FirstOrDefault();
                        if (selectedRecord != null)
                        {
                            if (selectedRecord.IsDirty)
                            {
                                EmployeeAttendanceEntity newItem = new EmployeeAttendanceEntity();

                                // Store the identity key
                                newItem.AutoID = autoID;

                                // Set value for "OTApprovalCode", "OTApprovalDesc"
                                RadComboBox cboOTApprovalType = (RadComboBox)item["OTApprovalDesc"].FindControl("cboOTApprovalType");
                                if (cboOTApprovalType != null)
                                {
                                    newItem.OTApprovalCode = cboOTApprovalType.SelectedValue;
                                    newItem.OTApprovalDesc = cboOTApprovalType.Text;
                                }

                                // Set value for "MealVoucherEligibilityCode", "MealVoucherEligibility"
                                RadComboBox cboMealVoucherEligibility = (RadComboBox)item["MealVoucherEligibility"].FindControl("cboMealVoucherEligibility");
                                if (cboMealVoucherEligibility != null)
                                {
                                    newItem.MealVoucherEligibilityCode = cboMealVoucherEligibility.SelectedValue;
                                    newItem.MealVoucherEligibility = cboMealVoucherEligibility.Text;
                                }

                                // Set value for "OTReasonCode", "OTReason"
                                RadComboBox cboOTReason = (RadComboBox)item["OTReason"].FindControl("cboOTReason");
                                if (cboOTReason != null)
                                {
                                    if (string.IsNullOrEmpty(cboOTReason.SelectedValue) ||
                                        cboOTReason.SelectedValue.Replace("&nbsp;", "").Trim() == string.Empty ||
                                        cboOTReason.SelectedValue == "0")
                                    {
                                        errorCount += 1;
                                        sb.AppendLine(string.Format(@"OT Reason for Employee No. {0} is mandatory. Please specify the overtime reason then try to save again!<br />",  selectedRecord.EmpNo));
                                    }
                                    else
                                    {
                                        newItem.OTReasonCode = cboOTReason.SelectedValue;
                                        newItem.OTReason = cboOTReason.Text;
                                    }
                                }
                                   
                                // Set value for "AttendanceRemarks"
                                TextBox txtRemarks = (TextBox)item["AttendanceRemarks"].FindControl("txtRemarks");
                                if (txtRemarks != null)
                                    newItem.AttendanceRemarks = txtRemarks.Text.Trim();

                                // Set value for "OTDurationHour"
                                RadNumericTextBox txtDuration = (RadNumericTextBox)item["OTDurationHour"].FindControl("txtDuration");
                                if (txtDuration != null)
                                {
                                    bool isValidOTDuration = false;
                                    int specifiedOTDurationHour = UIHelper.ConvertObjectToInt(txtDuration.Value);
                                    int specifiedOTDurationMin = 0;
                                    double actualOTDurationMin = 0;
                                    int hours = 0;
                                    int minutes = 0;

                                    // Convert hour duration into minutes
                                    hours = Math.DivRem(specifiedOTDurationHour, 100, out minutes);
                                    specifiedOTDurationMin = (hours * 60) + minutes;

                                    // Validate overtime duration
                                    if (selectedRecord.OTStartTime != null &&
                                        selectedRecord.OTEndTime != null)
                                    {
                                        DateTime otStart = Convert.ToDateTime(selectedRecord.OTStartTime);
                                        DateTime otEnd = Convert.ToDateTime(selectedRecord.OTEndTime);                                        

                                        actualOTDurationMin = (new DateTime(otEnd.Year, otEnd.Month, otEnd.Day, otEnd.Hour, otEnd.Minute, 0) - new DateTime(otStart.Year, otStart.Month, otStart.Day, otStart.Hour, otStart.Minute, 0)).TotalMinutes;
                                        //actualOTDurationMin = (Convert.ToDateTime(selectedRecord.OTEndTime) - Convert.ToDateTime(selectedRecord.OTStartTime)).TotalMinutes;
                                    }

                                    if (specifiedOTDurationMin > 0 && actualOTDurationMin > 0)
                                    {
                                        if (specifiedOTDurationMin > Convert.ToInt32(actualOTDurationMin))
                                        {
                                            #region Check if overtime reason is a callout
                                            string[] callOutArray = ConfigurationManager.AppSettings["OTCallOut"].Split(',');
                                            if (callOutArray != null)
                                            {
                                                if (callOutArray.Where(a => a.Trim() == cboOTReason.SelectedValue.Trim()).FirstOrDefault() != null)
                                                    isValidOTDuration = true;
                                            }
                                            #endregion
                                        }                                        
                                        else
                                            isValidOTDuration = true;
                                    }
                                    else if (specifiedOTDurationMin == 0 && actualOTDurationMin > 0)
                                    {
                                        // Note: Allow zero minutes overtime duration
                                        isValidOTDuration = true;   
                                        
                                        // Disallow zero minutes overtime duration (Note: Code is temporaty commented)
                                        //if (txtDuration.Enabled)
                                        //{
                                        //    isValidOTDuration = false;
                                        //    errorCount += 1;
                                        //    sb.AppendLine(string.Format(@"OT Duration for Employee No. {0} is mandatory if overtime is approved. Take note that duration should be greater than zero.<br />", selectedRecord.EmpNo));
                                        //}
                                    }

                                    if (isValidOTDuration)
                                    {
                                        int maxOTMinutes = UIHelper.ConvertObjectToInt(ConfigurationManager.AppSettings["MaxOTMinutes"]);

                                        newItem.OTDurationHour = specifiedOTDurationHour;
                                        newItem.OTDurationMinute = specifiedOTDurationMin;

                                        #region Check if overtime duration is greater than or equals to the limit set in the config file
                                        if (maxOTMinutes > 0 &&
                                            txtDuration.Enabled)
                                        {
                                            if (newItem.OTDurationMinute >= maxOTMinutes)
                                            {
                                                errorCount += 1;
                                                sb.AppendLine(string.Format(@"The overtime duration of Employee No. {0} on {1} is not allowed. Take note that duration should not be equal or greater than {2} hours.<br />", 
                                                    selectedRecord.EmpNo,
                                                    Convert.ToDateTime(selectedRecord.DT).ToString("dd-MMM-yyyy"),
                                                    maxOTMinutes / 60));
                                            }
                                        }
                                        #endregion
                                    }
                                    else
                                    {
                                        errorCount += 1;
                                        sb.AppendLine(string.Format(@"The specified overtime duration for Employee No. {0} is incorrect. Take note that duration should be equal or less than the actual duration based on the value of OT Start Time and OT End Time.<br />", selectedRecord.EmpNo));
                                    }
                                }

                                // Add item to the collection
                                attendanceList.Add(newItem);
                            }
                        }
                    }
                }
                #endregion

                #region Check for errors
                if (errorCount > 0)
                {
                    DisplayFormLevelError(sb.ToString().Trim());
                    return;
                }
                #endregion

                if (attendanceList.Count > 0)
                    SaveOvertime(attendanceList);
                else
                    DisplayFormLevelError("Data has not been modified hence requires no database update!");
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
                else if (this.ErrorType == ValidationErrorType.NoStartDate)
                {
                    validator.ErrorMessage = "Start Date is required.";
                    validator.ToolTip = "Start Date is required.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.InvalidDateRange)
                {
                    validator.ErrorMessage = "The specified date range is invalid. Make sure that the Effective Date is less than the Ending Date.";
                    validator.ToolTip = "The specified date range is invalid. Make sure that the Effective Date is less than the Ending Date.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.InvalidYear)
                {
                    validator.ErrorMessage = "The specified payroll year should not be greater than the current year.";
                    validator.ToolTip = "The specified payroll year should not be greater than the current year.";
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

        protected void cboMonth_SelectedIndexChanged(object sender, RadComboBoxSelectedIndexChangedEventArgs e)
        {
            // Reset start and end dates
            this.dtpStartDate.SelectedDate = this.dtpEndDate.SelectedDate = null;

            // Check Calendar Year
            if (this.txtYear.Text == string.Empty)
            {
                this.txtYear.Text = DateTime.Now.Year.ToString();
            }
            //else
            //{
            //    // Check if greater than current year
            //    if (this.txtYear.Value > DateTime.Today.Year)
            //    {
            //        this.txtGeneric.Text = ValidationErrorType.InvalidYear.ToString();
            //        this.ErrorType = ValidationErrorType.InvalidYear;
            //        this.cusValPayrollYear.Validate();
            //        this.txtYear.Focus();
            //        return;
            //    }
            //}

            int month = UIHelper.ConvertObjectToInt(this.cboMonth.SelectedValue);
            int year = UIHelper.ConvertObjectToInt(this.txtYear.Text);
            DateTime? startDate = null;
            DateTime? endDate = null;

            GetPayPeriod(year, month, ref startDate, ref endDate);

            this.dtpStartDate.SelectedDate = startDate;
            this.dtpEndDate.SelectedDate = endDate;
        }

        protected void chkPayPeriod_CheckedChanged(object sender, EventArgs e)
        {
            if (this.chkPayPeriod.Checked)
            {
                this.cboMonth.Enabled = true;
                this.txtYear.Enabled = true;
                this.dtpStartDate.Enabled = false;
                this.dtpEndDate.Enabled = false;

                #region Set the current pay period
                int month = DateTime.Now.Month;
                if (DateTime.Now.Day >= 16)
                    month = month + 1;

                this.txtYear.Text = DateTime.Now.Year.ToString();

                if (month > 12)
                {
                    month = 1;
                    this.txtYear.Text = (DateTime.Now.Year + 1).ToString();
                }

                this.cboMonth.SelectedValue = month.ToString();
                this.cboMonth_SelectedIndexChanged(this.cboMonth, new RadComboBoxSelectedIndexChangedEventArgs(this.cboMonth.Text, string.Empty, this.cboMonth.SelectedValue, string.Empty));
                this.cboMonth.Focus();
                #endregion
            }
            else
            {
                this.cboMonth.Enabled = false;
                this.txtYear.Enabled = false;
                this.dtpStartDate.Enabled = true;
                this.dtpEndDate.Enabled = true;
                //this.dtpStartDate.SelectedDate = null;
                //this.dtpEndDate.SelectedDate = null;

                this.cboMonth.SelectedIndex = -1;
                this.cboMonth.Text = string.Empty;
                this.txtYear.Text = string.Empty;
                this.dtpStartDate.Focus();
            }
        }

        protected void cboOTReason_ItemsRequested(object sender, RadComboBoxItemsRequestedEventArgs e)
        {
            try
            {
                RadComboBox cboOTReason = (RadComboBox)sender;
                if (cboOTReason != null &&
                    this.OTReasonList != null)
                {
                    // Clear combobox items
                    cboOTReason.Items.Clear();

                    foreach (UDCEntity item in this.OTReasonList)
                    {
                        RadComboBoxItem cboItem = new RadComboBoxItem();
                        cboItem.Text = item.Description;
                        cboItem.Value = item.Code;
                        cboItem.Attributes.Add(item.Code, item.Description);

                        // Add item to combobox
                        cboOTReason.Items.Add(cboItem);
                        cboItem.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void cboOTReason_SelectedIndexChanged(object sender, RadComboBoxSelectedIndexChangedEventArgs e)
        {
            try
            {
                RadComboBox cboOTReason = sender as RadComboBox;
                if (cboOTReason != null &&
                    !string.IsNullOrEmpty(cboOTReason.SelectedValue))
                {
                    #region Check if selected overtime reason is a callout
                    GridDataItem item = cboOTReason.Parent.Parent as GridDataItem;
                    if (item != null)
                    {
                        int otDurationOrig = UIHelper.ConvertObjectToInt(item["OTDurationHourClone"].Text);
                        int callOutValue = UIHelper.ConvertObjectToInt(ConfigurationManager.AppSettings["OTCallOutValue"]);                        
                        string[] callOutArray = ConfigurationManager.AppSettings["OTCallOut"].Split(',');

                        if (callOutArray != null)
                        {
                            RadNumericTextBox txtDuration = (RadNumericTextBox)item["OTDurationHour"].FindControl("txtDuration");
                            if (txtDuration != null)
                            {
                                if (callOutArray.Where(a => a.Trim() == cboOTReason.SelectedValue.Trim()).FirstOrDefault() != null)
                                {
                                    txtDuration.MaxValue = otDurationOrig + callOutValue;

                                    //if (txtDuration.Value >= 0 &&
                                    //    txtDuration.Value != otDurationOrig)
                                    //{
                                    //    txtDuration.Value = (txtDuration.Value * 100) + callOutValue;
                                    //}
                                    //else
                                    txtDuration.Value = otDurationOrig + callOutValue;
                                }
                                else
                                {
                                    txtDuration.MaxValue = otDurationOrig;

                                    //if (txtDuration.Value >= 0 &&
                                    //    txtDuration.Value != otDurationOrig)
                                    //{
                                    //    txtDuration.Value = txtDuration.Value;
                                    //}
                                    //else
                                        txtDuration.Value = otDurationOrig;
                                }
                            }
                        }
                    }

                    
                    #endregion
                }
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void cboOTApprovalType_SelectedIndexChanged(object sender, RadComboBoxSelectedIndexChangedEventArgs e)
        {
            RadComboBox cboOTApprovalType = (RadComboBox)sender;
            if (cboOTApprovalType != null)
            {
                GridDataItem item = cboOTApprovalType.Parent.Parent as GridDataItem;
                if (item != null)
                {
                    // Reset session
                    this.SelectedOvertimeRecord = null;

                    // Get the data key value
                    int autoID = UIHelper.ConvertObjectToInt(this.gridSearchResults.MasterTableView.Items[item.ItemIndex].GetDataKeyValue("AutoID"));

                    // Save current selected datagrid row
                    if (autoID > 0 && this.AttendanceList.Count > 0)
                    {
                        EmployeeAttendanceEntity selectedRecord = this.AttendanceList
                            .Where(a => a.AutoID == autoID)
                            .FirstOrDefault();
                        if (selectedRecord != null)
                            this.SelectedOvertimeRecord = selectedRecord;
                    }

                    if (cboOTApprovalType.SelectedValue == "Y" ||
                        cboOTApprovalType.SelectedValue == "N")
                    {
                        #region Enable other template controls
                        // Enable "Meal Voucher Approved?"
                        RadComboBox cboMealVoucherEligibility = (RadComboBox)item["MealVoucherEligibility"].FindControl("cboMealVoucherEligibility");
                        if (cboMealVoucherEligibility != null)
                            cboMealVoucherEligibility.Enabled = true;

                        // Enable "OT Duration"
                        RadNumericTextBox txtDuration = (RadNumericTextBox)item["OTDurationHour"].FindControl("txtDuration");
                        if (txtDuration != null)
                        {
                            txtDuration.Enabled = cboOTApprovalType.SelectedValue == "Y";
                            if (!txtDuration.Enabled)
                            {
                                if (this.SelectedOvertimeRecord != null)
                                    txtDuration.Text = this.SelectedOvertimeRecord.OTDurationHour.ToString();
                            }
                        }

                        // Enable "OT Reason"
                        RadComboBox cboOTReason = (RadComboBox)item["OTReason"].FindControl("cboOTReason");
                        if (cboOTReason != null)
                        {
                            cboOTReason.Enabled = true;
                            cboOTReason.SelectedIndex = -1;
                            cboOTReason.Text = string.Empty;
                        }

                        // Enable "Remarks"
                        TextBox txtRemarks = (TextBox)item["AttendanceRemarks"].FindControl("txtRemarks");
                        if (txtRemarks != null)
                            txtRemarks.Enabled = true;
                        #endregion

                        #region Update data in the collection                        
                        if (this.SelectedOvertimeRecord != null)
                        {
                            // Turn on the flag to save changes in the current row
                            this.SelectedOvertimeRecord.IsDirty = true;

                            // Set the value for "OTApprovalCode" and "OTApprovalDesc" fields
                            //this.SelectedOvertimeRecord.OTApprovalCode = cboOTApprovalType.SelectedValue;
                            //this.SelectedOvertimeRecord.OTApprovalDesc = cboOTApprovalType.Text;
                        }
                        #endregion

                        #region Reload data to OT Reason combobox
                        if (cboOTApprovalType.SelectedValue == "Y")
                            FillOvertimeReasonCombo(true, 1);
                        else
                            FillOvertimeReasonCombo(true, 2);
                        #endregion
                    }
                    else
                    {
                        #region Disable other template controls
                        // Disable "Meal Voucher Approved?"
                        RadComboBox cboMealVoucherEligibility = (RadComboBox)item["MealVoucherEligibility"].FindControl("cboMealVoucherEligibility");
                        if (cboMealVoucherEligibility != null)
                            cboMealVoucherEligibility.Enabled = false;

                        // Disable "OT Duration"
                        RadNumericTextBox txtDuration = (RadNumericTextBox)item["OTDurationHour"].FindControl("txtDuration");
                        if (txtDuration != null)
                        {
                            txtDuration.Enabled = false;
                            if (this.SelectedOvertimeRecord != null)
                                txtDuration.Text = this.SelectedOvertimeRecord.OTDurationHour.ToString();
                        }

                        // Disable "OT Reason"
                        RadComboBox cboOTReason = (RadComboBox)item["OTReason"].FindControl("cboOTReason");
                        if (cboOTReason != null)
                        {
                            cboOTReason.Enabled = false;
                            cboOTReason.SelectedIndex = -1;
                            cboOTReason.Text = string.Empty;
                        }

                        // Disable "Remarks"
                        TextBox txtRemarks = (TextBox)item["AttendanceRemarks"].FindControl("txtRemarks");
                        if (txtRemarks != null)
                        {
                            txtRemarks.Enabled = false;
                            txtRemarks.Text = string.Empty;
                        }
                        #endregion

                        #region Update data in the collection                        
                        if (this.SelectedOvertimeRecord != null)
                        {
                            // Turn off the flag to skip saving changes in the current row
                            this.SelectedOvertimeRecord.IsDirty = false;

                            // Set the value for "OTApprovalCode" and "OTApprovalDesc" fields
                            //this.SelectedOvertimeRecord.OTApprovalCode = cboOTApprovalType.SelectedValue;
                            //this.SelectedOvertimeRecord.OTApprovalDesc = cboOTApprovalType.Text;
                        }
                        #endregion
                    }
                }
            }
        }

        protected void cboMealVoucherEligibility_SelectedIndexChanged(object sender, RadComboBoxSelectedIndexChangedEventArgs e)
        {
            try
            {
                RadComboBox cboMealVoucherEligibility = (RadComboBox)sender;
                if (cboMealVoucherEligibility != null)
                {
                    GridDataItem item = cboMealVoucherEligibility.Parent.Parent as GridDataItem;
                    if (item != null)
                    {
                        #region Get the selected record
                        // Reset session
                        this.SelectedOvertimeRecord = null;

                        // Get the data key value
                        int autoID = UIHelper.ConvertObjectToInt(this.gridSearchResults.MasterTableView.Items[item.ItemIndex].GetDataKeyValue("AutoID"));

                        // Save current selected datagrid row
                        if (autoID > 0 && this.AttendanceList.Count > 0)
                        {
                            EmployeeAttendanceEntity selectedRecord = this.AttendanceList
                                .Where(a => a.AutoID == autoID)
                                .FirstOrDefault();
                            if (selectedRecord != null)
                                this.SelectedOvertimeRecord = selectedRecord;
                        }
                        #endregion

                        // Initialize template controls
                        TextBox txtRemarks = (TextBox)item["AttendanceRemarks"].FindControl("txtRemarks");

                        if (cboMealVoucherEligibility.SelectedValue == "YA" ||
                            cboMealVoucherEligibility.SelectedValue == "N")
                        {
                            #region Enable other template controls
                            // Enable "Remarks"
                            if (txtRemarks != null)
                                txtRemarks.Enabled = true;
                            #endregion

                            #region Update data in the collection                        
                            if (this.SelectedOvertimeRecord != null)
                            {
                                // Turn on the flag to save changes in the current row
                                this.SelectedOvertimeRecord.IsDirty = true;

                                // Set the value for "MealVoucherEligibilityCode" and "MealVoucherEligibility" fields
                                //this.SelectedOvertimeRecord.MealVoucherEligibilityCode = cboMealVoucherEligibility.SelectedValue;
                                //this.SelectedOvertimeRecord.MealVoucherEligibility = cboMealVoucherEligibility.Text;
                            }
                            #endregion
                        }
                        else
                        {
                            #region Disable other template controls
                            // Disable "Remarks"
                            if (txtRemarks != null)
                            {
                                txtRemarks.Enabled = false;
                                //txtRemarks.Text = string.Empty;
                            }
                            #endregion

                            #region Update data in the collection                        
                            if (this.SelectedOvertimeRecord != null)
                            {
                                // Turn off the flag to skip saving changes in the current row
                                this.SelectedOvertimeRecord.IsDirty = true;

                                // Set the value for "MealVoucherEligibilityCode" and "MealVoucherEligibility" fields
                                //this.SelectedOvertimeRecord.MealVoucherEligibilityCode = cboMealVoucherEligibility.SelectedValue;
                                //this.SelectedOvertimeRecord.MealVoucherEligibility = cboMealVoucherEligibility.Text;
                            }
                            #endregion
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void chkOTApprove_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                CheckBox chkOTApprove = sender as CheckBox;
                if (chkOTApprove != null)
                {
                    // Save to session
                    this.IsOTApprove = UIHelper.ConvertObjectToBolean(chkOTApprove.Checked);
                    this.IsOTApprovalHeaderClicked = true;

                    if (this.AttendanceList.Count > 0)
                    {
                        foreach (EmployeeAttendanceEntity item in this.AttendanceList)
                        {
                            if (!item.IsOTAlreadyProcessed)
                            {
                                item.OTApprovalDesc = chkOTApprove.Checked == true ? "Yes" : "-";
                                item.OTApprovalCode = chkOTApprove.Checked == true ? "Y" : "-";
                            }
                        }

                        RebindDataToGrid();
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void txtYear_TextChanged(object sender, EventArgs e)
        {
            this.cboMonth_SelectedIndexChanged(this.cboMonth, new RadComboBoxSelectedIndexChangedEventArgs(this.cboMonth.Text, string.Empty, this.cboMonth.SelectedValue, string.Empty));
        }
        #endregion

        #region IFormExtension Interface Implementation
        public void ClearForm()
        {
            #region Reset controls            
            this.cboMonth.Text = string.Empty;
            this.cboMonth.SelectedIndex = -1;
            this.txtYear.Text = string.Empty;
            this.dtpStartDate.SelectedDate = null;
            this.dtpEndDate.SelectedDate = null;
            this.chkPayPeriod.Checked = false;
            this.chkPayPeriod_CheckedChanged(this.chkPayPeriod, new EventArgs());

            this.txtEmpNo.Text = string.Empty;            
            this.cboCostCenter.Text = string.Empty;
            this.cboCostCenter.SelectedIndex = -1;
            #endregion

            // Clear collections
            KillSessions();

            // Reset the grid
            this.gridSearchResults.CurrentPageIndex = 0;
            this.CurrentPageIndex = this.gridSearchResults.CurrentPageIndex + 1;
            this.CurrentPageSize = this.gridSearchResults.PageSize;

            InitializeDataToGrid();
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
            this.OTReasonList.Clear();

            // Clear sessions
            ViewState["CustomErrorMsg"] = null;
            ViewState["CurrentStartRowIndex"] = null;
            ViewState["CurrentMaximumRows"] = null;
            ViewState["CurrentPageIndex"] = null;
            ViewState["CurrentPageSize"] = null;
            ViewState["ReloadGridData"] = null;
            ViewState["CallerForm"] = null;
            ViewState["SelectedOvertimeRecord"] = null;
            ViewState["IsOTApprove"] = null;
            ViewState["IsOTApprovalHeaderClicked"] = null;

            // Clear all viewstates
            ViewState.Clear();
        }
        #endregion

        #region Private Methods
        private void RestoreDataFromCollection()
        {
            if (this.OvertimeApprovalStorage.Count == 0)
                return;

            #region Restore Query String values
            if (this.OvertimeApprovalStorage.ContainsKey("CallerForm"))
                this.CallerForm = UIHelper.ConvertObjectToString(this.OvertimeApprovalStorage["CallerForm"]);
            else
                this.CallerForm = string.Empty;

            if (this.OvertimeApprovalStorage.ContainsKey("ReloadGridData"))
                this.ReloadGridData = UIHelper.ConvertObjectToBolean(this.OvertimeApprovalStorage["ReloadGridData"]);
            else
                this.ReloadGridData = false;
            #endregion

            #region Restore session values
            if (this.OvertimeApprovalStorage.ContainsKey("CurrentStartRowIndex"))
                this.CurrentStartRowIndex = UIHelper.ConvertObjectToInt(this.OvertimeApprovalStorage["CurrentStartRowIndex"]);
            else
                this.CurrentStartRowIndex = 0;

            if (this.OvertimeApprovalStorage.ContainsKey("CurrentMaximumRows"))
                this.CurrentMaximumRows = UIHelper.ConvertObjectToInt(this.OvertimeApprovalStorage["CurrentMaximumRows"]);
            else
                this.CurrentMaximumRows = 0;

            if (this.OvertimeApprovalStorage.ContainsKey("CurrentPageIndex"))
                this.CurrentPageIndex = UIHelper.ConvertObjectToInt(this.OvertimeApprovalStorage["CurrentPageIndex"]);
            else
                this.CurrentPageIndex = 0;

            if (this.OvertimeApprovalStorage.ContainsKey("CurrentPageSize"))
                this.CurrentPageSize = UIHelper.ConvertObjectToInt(this.OvertimeApprovalStorage["CurrentPageSize"]);
            else
                this.CurrentPageSize = 0;

            if (this.OvertimeApprovalStorage.ContainsKey("AttendanceList"))
                this.AttendanceList = this.OvertimeApprovalStorage["AttendanceList"] as List<EmployeeAttendanceEntity>;
            else
                this.AttendanceList = null;

            if (this.OvertimeApprovalStorage.ContainsKey("OTReasonList"))
                this.OTReasonList = this.OvertimeApprovalStorage["OTReasonList"] as List<UDCEntity>;
            else
                this.OTReasonList = null;

            if (this.OvertimeApprovalStorage.ContainsKey("SelectedOvertimeRecord"))
                this.SelectedOvertimeRecord = this.OvertimeApprovalStorage["SelectedOvertimeRecord"] as EmployeeAttendanceEntity;
            else
                this.SelectedOvertimeRecord = null;

            if (this.OvertimeApprovalStorage.ContainsKey("IsOTApprove"))
                this.IsOTApprove = UIHelper.ConvertObjectToBolean(this.OvertimeApprovalStorage["IsOTApprove"]);
            else
                this.IsOTApprove = false;

            if (this.OvertimeApprovalStorage.ContainsKey("IsOTApprovalHeaderClicked"))
                this.IsOTApprovalHeaderClicked = UIHelper.ConvertObjectToBolean(this.OvertimeApprovalStorage["IsOTApprovalHeaderClicked"]);
            else
                this.IsOTApprovalHeaderClicked = false;

            FillComboData(false);
            #endregion

            #region Restore control values  

            if (this.OvertimeApprovalStorage.ContainsKey("txtEmpNo"))
                this.txtEmpNo.Text = UIHelper.ConvertObjectToString(this.OvertimeApprovalStorage["txtEmpNo"]);
            else
                this.txtEmpNo.Text = string.Empty;

            if (this.OvertimeApprovalStorage.ContainsKey("cboCostCenter"))
                this.cboCostCenter.SelectedValue = UIHelper.ConvertObjectToString(this.OvertimeApprovalStorage["cboCostCenter"]);
            else
            {
                this.cboCostCenter.Text = string.Empty;
                this.cboCostCenter.SelectedIndex = -1;
            }

            if (this.OvertimeApprovalStorage.ContainsKey("cboMonth"))
                this.cboMonth.SelectedValue = UIHelper.ConvertObjectToString(this.OvertimeApprovalStorage["cboMonth"]);
            else
            {
                this.cboMonth.Text = string.Empty;
                this.cboMonth.SelectedIndex = -1;
            }

            if (this.OvertimeApprovalStorage.ContainsKey("txtYear"))
                this.txtYear.Text = UIHelper.ConvertObjectToString(this.OvertimeApprovalStorage["txtYear"]);
            else
                this.txtYear.Text = string.Empty;

            if (this.OvertimeApprovalStorage.ContainsKey("dtpStartDate"))
                this.dtpStartDate.SelectedDate = UIHelper.ConvertObjectToDate(this.OvertimeApprovalStorage["dtpStartDate"]);
            else
                this.dtpStartDate.SelectedDate = null;

            if (this.OvertimeApprovalStorage.ContainsKey("dtpEndDate"))
                this.dtpEndDate.SelectedDate = UIHelper.ConvertObjectToDate(this.OvertimeApprovalStorage["dtpEndDate"]);
            else
                this.dtpEndDate.SelectedDate = null;

            if (this.OvertimeApprovalStorage.ContainsKey("chkPayPeriod"))
                this.chkPayPeriod.Checked = UIHelper.ConvertObjectToBolean(this.OvertimeApprovalStorage["chkPayPeriod"]);
            else
                this.chkPayPeriod.Checked = false;

            if (this.chkPayPeriod.Checked)
            {
                this.cboMonth.Enabled = true;
                this.txtYear.Enabled = true;
                this.dtpStartDate.Enabled = false;
                this.dtpEndDate.Enabled = false;
            }
            else
            {
                this.cboMonth.Enabled = false;
                this.txtYear.Enabled = false;
                this.dtpStartDate.Enabled = true;
                this.dtpEndDate.Enabled = true;
            }
            #endregion

            // Refresh the grid
            RebindDataToGrid();

            // Set the grid attributes
            this.gridSearchResults.CurrentPageIndex = this.CurrentPageIndex;
            this.gridSearchResults.MasterTableView.CurrentPageIndex = this.CurrentPageIndex;
            this.gridSearchResults.MasterTableView.PageSize = this.CurrentPageSize;
            this.gridSearchResults.MasterTableView.DataBind();
        }

        private void StoreDataToCollection(UIHelper.PagePostBackFlags formFlag)
        {
            this.OvertimeApprovalStorage.Clear();
            this.OvertimeApprovalStorage.Add("FormFlag", formFlag.ToString());

            #region Save control values to session
            this.OvertimeApprovalStorage.Add("txtEmpNo", this.txtEmpNo.Text.Trim());
            this.OvertimeApprovalStorage.Add("cboCostCenter", this.cboCostCenter.SelectedValue);
            this.OvertimeApprovalStorage.Add("chkPayPeriod", this.chkPayPeriod.Checked);
            this.OvertimeApprovalStorage.Add("cboMonth", this.cboMonth.SelectedValue);
            this.OvertimeApprovalStorage.Add("txtYear", this.txtYear.Text.Trim());
            this.OvertimeApprovalStorage.Add("dtpStartDate", this.dtpStartDate.SelectedDate);
            this.OvertimeApprovalStorage.Add("dtpEndDate", this.dtpEndDate.SelectedDate);
            #endregion

            #region Save Query String values to collection
            this.OvertimeApprovalStorage.Add("CallerForm", this.CallerForm);
            this.OvertimeApprovalStorage.Add("ReloadGridData", this.ReloadGridData);
            #endregion

            #region Store session data to collection
            this.OvertimeApprovalStorage.Add("CurrentStartRowIndex", this.CurrentStartRowIndex);
            this.OvertimeApprovalStorage.Add("CurrentMaximumRows", this.CurrentMaximumRows);
            this.OvertimeApprovalStorage.Add("CurrentPageIndex", this.CurrentPageIndex);
            this.OvertimeApprovalStorage.Add("CurrentPageSize", this.CurrentPageSize);
            this.OvertimeApprovalStorage.Add("AttendanceList", this.AttendanceList);
            this.OvertimeApprovalStorage.Add("OTReasonList", this.OTReasonList);
            this.OvertimeApprovalStorage.Add("SelectedOvertimeRecord", this.SelectedOvertimeRecord);
            this.OvertimeApprovalStorage.Add("IsOTApprove", this.IsOTApprove);
            this.OvertimeApprovalStorage.Add("IsOTApprovalHeaderClicked", this.IsOTApprovalHeaderClicked);
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
            FillOvertimeReasonCombo(reloadFromDB);
        }

        private void GetPayPeriod(int year, int month, ref DateTime? startDate, ref DateTime? endDate)
        {
            try
            {
                switch (month)
                {
                    case 1:     // January
                        startDate = UIHelper.ConvertObjectToDate(string.Format("16/{0}/{1}", month + 11, year - 1));
                        endDate = UIHelper.ConvertObjectToDate(string.Format("15/{0}/{1}", month, year));
                        break;

                    case 2:     // February
                    case 3:     // March
                    case 4:     // April
                    case 5:     // May
                    case 6:     // June
                    case 7:     // July
                    case 8:     // August
                    case 9:     // September
                    case 10:    // October
                    case 11:    // November
                    case 12:    // December
                        startDate = UIHelper.ConvertObjectToDate(string.Format("16/{0}/{1}", month - 1, year));
                        endDate = UIHelper.ConvertObjectToDate(string.Format("15/{0}/{1}", month, year));
                        break;
                }
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }
        #endregion

        #region Database Access
        private void GetOvertimeAttendance(bool reloadDataFromDB = false)
        {
            try
            {
                #region Initialize variables          
                int empNo = UIHelper.ConvertObjectToInt(this.txtEmpNo.Text);
                if (empNo.ToString().Length == 4)
                {
                    empNo += 10000000;

                    // Display Emp. No.
                    this.txtEmpNo.Text = empNo.ToString();
                }

                string costCenter = this.cboCostCenter.SelectedValue;
                DateTime? startDate = this.dtpStartDate.SelectedDate;
                DateTime? endDate = this.dtpEndDate.SelectedDate;

                // Initialize record count
                this.lblRecordCount.Text = "0 record found";

                // Reset session variables
                ViewState["IsOTApprove"] = null;
                ViewState["IsOTApprovalHeaderClicked"] = null;
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

                    DALProxy proxy = new DALProxy();
                    var source = proxy.GetOvertimeAttendance(startDate, endDate, costCenter, empNo, ref error, ref innerError);
                    if (!string.IsNullOrEmpty(error) || !string.IsNullOrEmpty(innerError))
                    {
                        if (!string.IsNullOrEmpty(innerError))
                            throw new Exception(innerError, new Exception(innerError));
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
                this.AttendanceList = gridSource;
                #endregion

                //Bind data to the grid
                RebindDataToGrid();
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        private void FillCostCenterCombo()
        {
            DataView dv = this.objCostCenter.Select() as DataView;
            if (dv == null || dv.Count == 0)
                return;

            DataRow[] source = new DataRow[dv.Count];
            dv.Table.Rows.CopyTo(source, 0);
            EmployeeDAL.CostCenterDataTable filteredDT = new EmployeeDAL.CostCenterDataTable();
            bool enableEmpSearch = false;

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

                // Set the flag
                enableEmpSearch = true;
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

                //// Set the flag
                enableEmpSearch = true;
                #endregion
            }
            else
            {
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

                    // Add record to the collection
                    filteredDT.Rows.Add(row);
                }

                //Set the flag
                enableEmpSearch = true;
                #endregion
            }

            if (filteredDT.Rows.Count > 0)
            {
                this.cboCostCenter.DataTextField = "CostCenter";
                this.cboCostCenter.DataValueField = "CostCenter";
                this.cboCostCenter.DataSource = filteredDT;
                this.cboCostCenter.DataBind();
            }

            // Enable/Disable employee search button 
            this.btnFindEmployee.Enabled = enableEmpSearch;
        }

        private void FillOvertimeReasonCombo(bool reloadFromDB = true, byte loadType = 0)
        {
            try
            {
                List<UDCEntity> comboSource = new List<UDCEntity>();
                if (this.OTReasonList.Count > 0 && !reloadFromDB)
                {
                    comboSource = this.OTReasonList;
                }
                else
                {
                    string error = string.Empty;
                    string innerError = string.Empty;

                    DALProxy proxy = new DALProxy();
                    var source = proxy.GetOvertimeReasons(loadType, ref error, ref innerError);
                    if (source != null && source.Count() > 0)
                    {
                        comboSource.AddRange(source.ToList());
                    }
                }

                // Store to session
                this.OTReasonList = comboSource;
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex);
            }
        }

        private void SaveOvertime(List<EmployeeAttendanceEntity> attendanceList)
        {
            try
            {
                if (attendanceList.Count == 0)
                    return;

                #region Initialize variables
                DALProxy proxy = new DALProxy();
                string error = string.Empty;
                string innerError = string.Empty;
                int autoID = 0;
                string otReasonCode = null;
                string comment = null;
                string userID = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]);
                string otApprovalCode = null;
                string mealVoucherApprovalCode = null;
                int otDuration = 0;
                #endregion

                #region Save to database
                foreach (EmployeeAttendanceEntity item in attendanceList)
                {
                    autoID = item.AutoID;
                    otReasonCode = item.OTReasonCode;
                    comment = item.AttendanceRemarks;
                    otApprovalCode = item.OTApprovalCode;
                    mealVoucherApprovalCode = item.MealVoucherEligibilityCode;
                    otDuration = UIHelper.ConvertObjectToInt(item.OTDurationMinute);

                    DatabaseSaveResult dbResult = proxy.SaveEmployeeOvertime(autoID, otReasonCode, comment, userID, otApprovalCode, mealVoucherApprovalCode, otDuration, ref error, ref innerError);
                    if (!string.IsNullOrEmpty(error) || !string.IsNullOrEmpty(innerError))
                    {
                        if (!string.IsNullOrEmpty(innerError))
                            throw new Exception(innerError, new Exception(innerError));
                        else
                            throw new Exception(error);
                    }
                }
                #endregion

                // Show success notification and refresh the grid
                //UIHelper.DisplayJavaScriptMessage(this, "Record has been saved sucessfully!");

                this.ReloadGridData = true;
                this.btnSearch_Click(this.btnSearch, new EventArgs());
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
            finally
            {
                this.ReloadGridData = false;
            }
        }
        #endregion
                
    }
}