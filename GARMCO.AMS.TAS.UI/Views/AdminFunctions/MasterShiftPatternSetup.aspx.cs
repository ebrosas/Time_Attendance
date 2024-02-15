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

namespace GARMCO.AMS.TAS.UI.Views.AdminFunctions
{
    public partial class MasterShiftPatternSetup : BaseWebForm, IFormExtension
    {
        #region Enumeration
        private enum ValidationErrorType
        {
            NoError,
            CustomFormError,
            NoRecordToDelete,
            NoShiftPattern,
            NoShiftPatternCode,
            NoShiftCode,
            NoShiftTimingSchedule,
            NoShiftTimingSequence,
            ShiftCodeAlreadyExist,
            InvalidArrivalTime,
            InvalidDepartureTime,
            InvalidRArrivalTime,
            InvalidRDepartureTime,
            CannotDeleteOneShiftTiming,
            CannotDeleteOneShiftPointer
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

        private Dictionary<string, object> MasterShiftPatternStorage
        {
            get
            {
                Dictionary<string, object> list = Session["MasterShiftPatternStorage"] as Dictionary<string, object>;
                if (list == null)
                    Session["MasterShiftPatternStorage"] = list = new Dictionary<string, object>();

                return list;
            }
            set
            {
                Session["MasterShiftPatternStorage"] = value;
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
                    pageSize = this.gridShiftTiming.MasterTableView.PageSize;

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

        private List<MasterShiftPatternEntity> ShiftTimingScheduleList
        {
            get
            {
                List<MasterShiftPatternEntity> list = ViewState["ShiftTimingScheduleList"] as List<MasterShiftPatternEntity>;
                if (list == null)
                    ViewState["ShiftTimingScheduleList"] = list = new List<MasterShiftPatternEntity>();

                return list;
            }
            set
            {
                ViewState["ShiftTimingScheduleList"] = value;
            }
        }

        private List<MasterShiftPatternEntity> WorkShiftList
        {
            get
            {
                List<MasterShiftPatternEntity> list = ViewState["WorkShiftList"] as List<MasterShiftPatternEntity>;
                if (list == null)
                    ViewState["WorkShiftList"] = list = new List<MasterShiftPatternEntity>();

                return list;
            }
            set
            {
                ViewState["WorkShiftList"] = value;
            }
        }

        private List<MasterShiftPatternEntity> ShiftPointerSequenceList
        {
            get
            {
                List<MasterShiftPatternEntity> list = ViewState["ShiftPointerSequenceList"] as List<MasterShiftPatternEntity>;
                if (list == null)
                    ViewState["ShiftPointerSequenceList"] = list = new List<MasterShiftPatternEntity>();

                return list;
            }
            set
            {
                ViewState["ShiftPointerSequenceList"] = value;
            }
        }

        private List<MasterShiftPatternEntity> ShiftPatternList
        {
            get
            {
                List<MasterShiftPatternEntity> list = ViewState["ShiftPatternList"] as List<MasterShiftPatternEntity>;
                if (list == null)
                    ViewState["ShiftPatternList"] = list = new List<MasterShiftPatternEntity>();

                return list;
            }
            set
            {
                ViewState["ShiftPatternList"] = value;
            }
        }

        private List<MasterShiftPatternEntity> ShiftCodeList
        {
            get
            {
                List<MasterShiftPatternEntity> list = ViewState["ShiftCodeList"] as List<MasterShiftPatternEntity>;
                if (list == null)
                    ViewState["ShiftCodeList"] = list = new List<MasterShiftPatternEntity>();

                return list;
            }
            set
            {
                ViewState["ShiftCodeList"] = value;
            }
        }

        private MasterShiftPatternEntity SelectedShiftPattenCode
        {
            get
            {
                return ViewState["SelectedShiftPattenCode"] as MasterShiftPatternEntity;
            }
            set
            {
                ViewState["SelectedShiftPattenCode"] = value;
            }
        }

        private MasterShiftPatternEntity SelectedShiftTimingRecord
        {
            get
            {
                return ViewState["SelectedShiftTimingRecord"] as MasterShiftPatternEntity;
            }
            set
            {
                ViewState["SelectedShiftTimingRecord"] = value;
            }
        }

        private MasterShiftPatternEntity SelectedShiftPointerRecord
        {
            get
            {
                return ViewState["SelectedShiftPointerRecord"] as MasterShiftPatternEntity;
            }
            set
            {
                ViewState["SelectedShiftPointerRecord"] = value;
            }
        }

        private bool IsReadonlyView
        {
            get
            {
                return UIHelper.ConvertObjectToBolean(ViewState["IsReadonlyView"]);
            }
            set
            {
                ViewState["IsReadonlyView"] = value;
            }
        }

        private string ShiftPatCode
        {
            get
            {
                return UIHelper.ConvertObjectToString(ViewState["ShiftPatCode"]);
            }
            set
            {
                ViewState["ShiftPatCode"] = value;
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

                this.Master.SetPageForm(UIHelper.FormAccessCodes.SHIFTPATRN.ToString());

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

                if (!string.IsNullOrEmpty(costCenter))
                {
                    sb.Append(string.Format("Cost Center: {0} <br />", costCenter));
                }

                this.Master.LogOnUser = string.Format("Welcome {0}",
                   UIHelper.GetUserFirstName(UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_FULLNAME]), UIHelper.ConvertObjectToBolean(Session[UIHelper.GARMCO_USER_IS_SPECIAL])));

                this.Master.LogOnUserInfo = sb.ToString().Trim();
                this.Master.FormTitle = UIHelper.PAGE_MASTER_SHIFT_PATTERN_SETUP_TITLE;
                #endregion

                #region Check if user has permission to access the page
                this.IsReadonlyView = UIHelper.ConvertObjectToBolean(Request.QueryString["IsReadonlyView"]);
                if (!GAPFunction.CheckFormAccess(this.Master.FormAccess, GAPConstants.FormAccessIndex.Retrieve))
                {                    
                    if (!UIHelper.ConvertObjectToBolean(Session[UIHelper.GARMCO_USER_IS_SPECIAL]) &&
                        !this.IsReadonlyView)
                    {
                        Response.Redirect(String.Format("{0}?error={1}&pageName={2}", UIHelper.PAGE_ERROR, Convert.ToInt32(UIHelper.PageErrorCodes.NoAccessOnPage), UIHelper.PAGE_MASTER_SHIFT_PATTERN_SETUP_TITLE), true);
                    }
                }
                #endregion

                #region Checks if current user has permission to create, update, delete and print
                this.btnNew.Enabled = this.Master.IsCreateAllowed;
                this.btnSave.Visible = this.Master.IsEditAllowed;
                //this.btnDelete.Visible = this.Master.IsDeleteAllowed;
                this.btnDeleteShiftPattern.Visible = this.Master.IsDeleteAllowed;
                #endregion

                #region Set default button
                this.Master.DefaultButton = this.btnSearch.UniqueID;
                #endregion

                #region Restore saved data from local storage
                string formFlag = string.Empty;
                if (this.MasterShiftPatternStorage.Count > 0)
                {
                    if (this.MasterShiftPatternStorage.ContainsKey("FormFlag"))
                        formFlag = UIHelper.ConvertObjectToString(this.MasterShiftPatternStorage["FormFlag"]);
                }
                #endregion

                if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.RedirectToOtherPage.ToString())
                {
                    #region Show the last form data
                    RestoreDataFromCollection();
                    ProcessQueryString();

                    // Clear data storage
                    Session.Remove("MasterShiftPatternStorage");

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
                    if (!string.IsNullOrEmpty(this.ShiftPatCode))
                    {
                        this.cboShiftPattern.SelectedValue = this.ShiftPatCode;
                        this.cboShiftPattern_SelectedIndexChanged(this.cboShiftPattern, new RadComboBoxSelectedIndexChangedEventArgs(this.cboShiftPattern.Text, string.Empty, this.cboShiftPattern.SelectedValue, string.Empty));
                    }
                    else
                    {
                        // Fill data to the grid
                        this.btnSearch_Click(this.btnSearch, new EventArgs());
                    }

                    if (this.IsReadonlyView)
                    {
                        this.panButton.Style[HtmlTextWriterStyle.Display] = "none";
                        this.tblShiftTimingFilter.Style[HtmlTextWriterStyle.Display] = "none";
                        this.tblShiftSequenceFilter.Style[HtmlTextWriterStyle.Display] = "none";

                        this.cboShiftPattern.Enabled = false;
                        this.rblDayShift.Enabled = false;
                        this.rblFlexitime.Enabled = false;
                        this.btnShiftPatternDetail.Enabled = false;
                        this.btnNew.Visible = false;
                        this.btnDeleteShiftPattern.Visible = false;
                        this.btnSearch.Visible = false;
                        this.btnBack.Visible = true;
                        this.gridShiftTiming.Enabled = false;
                        this.gridShiftSequence.Enabled = false;
                        this.gridShiftSequence.PageSize = 50;
                        this.gridShiftSequence.Rebind();
                    }
                    #endregion
                }
            }

            ScriptManager.RegisterStartupScript(this, this.GetType(), "HideScrollbar", "HideScrollbar();", true);
            AddControlsAttribute();
        }
        #endregion

        #region Grid Events

        #region Shift Timing Schedule Grid                
        protected void gridShiftTiming_PageIndexChanged(object sender, Telerik.Web.UI.GridPageChangedEventArgs e)
        {
            // Store page index to session
            this.CurrentPageIndex = e.NewPageIndex + 1;

            RebindDataToShiftTimingGrid();
        }

        protected void gridShiftTiming_PageSizeChanged(object sender, Telerik.Web.UI.GridPageSizeChangedEventArgs e)
        {
            // Store page size to session
            this.CurrentPageSize = e.NewPageSize;
            this.CurrentPageIndex = 1;

            RebindDataToShiftTimingGrid();
        }

        protected void gridShiftTiming_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (this.ShiftTimingScheduleList.Count > 0)
            {
                this.gridShiftTiming.DataSource = this.ShiftTimingScheduleList;
                this.gridShiftTiming.DataBind();

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
                        sortExpr.SortOrder = this.gridShiftTiming.MasterTableView.AllowNaturalSort ? GridSortOrder.None : GridSortOrder.Descending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;

                    case GridSortOrder.Descending:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = GridSortOrder.Ascending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;
                }

                e.Canceled = true;
                this.gridShiftTiming.Rebind();
            }
            else
                InitializeDataToShiftTimingGrid();
        }

        protected void gridShiftTiming_ItemCommand(object sender, GridCommandEventArgs e)
        {
            if (e.CommandName.Equals(RadGrid.SelectCommandName))
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {
                    // Get data key value
                    int autoID = UIHelper.ConvertObjectToInt(this.gridShiftTiming.MasterTableView.Items[e.Item.ItemIndex].GetDataKeyValue("AutoID"));
                    if (autoID > 0 &&
                        this.ShiftTimingScheduleList.Count > 0)
                    {
                        if (this.ShiftTimingScheduleList.Count == 1)
                        {
                            this.txtGeneric.Text = ValidationErrorType.CannotDeleteOneShiftTiming.ToString();
                            this.ErrorType = ValidationErrorType.CannotDeleteOneShiftTiming;
                            this.cusValButton.Validate();
                            return;
                        }
                        else
                        {
                            this.SelectedShiftTimingRecord = this.ShiftTimingScheduleList
                               .Where(a => a.AutoID == autoID)
                               .FirstOrDefault();
                        }
                    }

                    if (UIHelper.ConvertObjectToString(e.CommandArgument) == "DeleteButton")
                    {
                        #region Delete button
                        StringBuilder script = new StringBuilder();
                        script.Append("ConfirmButtonAction('");
                        script.Append(string.Concat(this.btnRemoveShiftTiming.ClientID, "','"));
                        script.Append(string.Concat(this.btnRebind.ClientID, "','"));
                        script.Append(UIHelper.CONST_DELETE_CONFIRMATION + "');");

                        ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "Action Confirmation", script.ToString(), true);
                        #endregion
                    }
                }
            }
        }

        protected void gridShiftTiming_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item.ItemType == GridItemType.AlternatingItem || e.Item.ItemType == GridItemType.Item)
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {
                    #region Set Image Button's Tooltip
                    System.Web.UI.WebControls.ImageButton imgDelete = item["DeleteButton"].Controls[0] as System.Web.UI.WebControls.ImageButton;
                    if (imgDelete != null)
                        imgDelete.ToolTip = "Delete this row";
                    #endregion
                }
            }
        }

        private void RebindDataToShiftTimingGrid()
        {
            if (this.ShiftTimingScheduleList.Count > 0)
            {
                this.gridShiftTiming.DataSource = this.ShiftTimingScheduleList;
                this.gridShiftTiming.DataBind();

                #region Bind data collection for shift pointer combobox
                this.WorkShiftList.Clear();
                this.WorkShiftList.AddRange(this.ShiftTimingScheduleList);

                // Add Day-off item
                this.WorkShiftList.Insert(0, new MasterShiftPatternEntity() { ShiftCode = "O", ShiftFullDescription = "O - Weekend", ShiftDescription = "Weekend" });

                this.cboWorkShift2.DataSource = this.WorkShiftList.OrderBy(a => a.ArrivalFrom);
                this.cboWorkShift2.DataTextField = "ShiftFullDescription";
                this.cboWorkShift2.DataValueField = "ShiftCode";
                this.cboWorkShift2.DataBind();
                #endregion
            }
            else
                InitializeDataToShiftTimingGrid();
        }

        private void InitializeDataToShiftTimingGrid()
        {
            this.gridShiftTiming.DataSource = new List<MasterShiftPatternEntity>();
            this.gridShiftTiming.DataBind();

            this.cboWorkShift2.DataSource = null;
            this.cboWorkShift2.Items.Clear();
            this.cboWorkShift2.SelectedIndex = -1;
            this.cboWorkShift2.Text = string.Empty;
        }
        #endregion

        #region Shift Pointer Sequence Grid
        protected void gridShiftSequence_ItemCommand(object sender, GridCommandEventArgs e)
        {
            if (e.CommandName.Equals(RadGrid.SelectCommandName))
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {
                    // Get data key value
                    int autoID = UIHelper.ConvertObjectToInt(this.gridShiftSequence.MasterTableView.Items[e.Item.ItemIndex].GetDataKeyValue("AutoID"));
                    if (autoID > 0 &&
                        this.ShiftPointerSequenceList.Count > 0)
                    {
                        if (this.ShiftPointerSequenceList.Count == 1)
                        {
                            this.txtGeneric.Text = ValidationErrorType.CannotDeleteOneShiftPointer.ToString();
                            this.ErrorType = ValidationErrorType.CannotDeleteOneShiftPointer;
                            this.cusValButton.Validate();
                            return;
                        }
                        else
                        {
                            this.SelectedShiftPointerRecord = this.ShiftPointerSequenceList
                                .Where(a => a.AutoID == autoID)
                                .FirstOrDefault();
                        }
                    }

                    if (UIHelper.ConvertObjectToString(e.CommandArgument) == "DeleteButton")
                    {
                        #region Delete button
                        StringBuilder script = new StringBuilder();
                        script.Append("ConfirmButtonAction('");
                        script.Append(string.Concat(this.btnRemoveShiftSequence.ClientID, "','"));
                        script.Append(string.Concat(this.btnRebindShiftPointer.ClientID, "','"));
                        script.Append(UIHelper.CONST_DELETE_CONFIRMATION + "');");

                        ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "Action Confirmation", script.ToString(), true);
                        #endregion
                    }
                }
            }
        }

        protected void gridShiftSequence_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item.ItemType == GridItemType.AlternatingItem || e.Item.ItemType == GridItemType.Item)
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {
                    #region Set Image Button's Tooltip
                    System.Web.UI.WebControls.ImageButton imgDelete = item["DeleteButton"].Controls[0] as System.Web.UI.WebControls.ImageButton;
                    if (imgDelete != null)
                        imgDelete.ToolTip = "Delete this row";
                    #endregion
                }
            }
        }

        protected void gridShiftSequence_PageIndexChanged(object sender, GridPageChangedEventArgs e)
        {
            RebindDataToShiftSequenceGrid();
        }

        protected void gridShiftSequence_PageSizeChanged(object sender, GridPageSizeChangedEventArgs e)
        {
            RebindDataToShiftSequenceGrid();
        }

        protected void gridShiftSequence_SortCommand(object sender, GridSortCommandEventArgs e)
        {
            if (this.ShiftPointerSequenceList.Count > 0)
            {
                this.gridShiftSequence.DataSource = this.ShiftPointerSequenceList;
                this.gridShiftSequence.DataBind();

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
                        sortExpr.SortOrder = this.gridShiftSequence.MasterTableView.AllowNaturalSort ? GridSortOrder.None : GridSortOrder.Descending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;

                    case GridSortOrder.Descending:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = GridSortOrder.Ascending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;
                }

                e.Canceled = true;
                this.gridShiftSequence.Rebind();
            }
            else
                InitializeDataToShiftSequenceGrid();
        }

        private void RebindDataToShiftSequenceGrid()
        {
            if (this.ShiftPointerSequenceList.Count > 0)
            {
                this.gridShiftSequence.DataSource = this.ShiftPointerSequenceList;
                this.gridShiftSequence.DataBind();
            }
            else
                InitializeDataToShiftSequenceGrid();
        }

        private void InitializeDataToShiftSequenceGrid()
        {
            this.gridShiftSequence.DataSource = new List<MasterShiftPatternEntity>();
            this.gridShiftSequence.DataBind();
        }
        #endregion

        #endregion

        #region Action Buttons
        protected void btnReset_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.cboShiftPattern.SelectedValue) ||
                this.cboShiftPattern.SelectedValue == UIHelper.CONST_COMBO_EMTYITEM_ID)
            {
                #region Reset controls
                this.cboWorkShift.SelectedIndex = -1;
                this.cboWorkShift.Text = string.Empty;
                this.cboWorkShift2.SelectedIndex = -1;
                this.cboWorkShift2.Text = string.Empty;
                this.cboShiftPattern.Text = string.Empty;
                this.cboShiftPattern.SelectedIndex = -1;
                this.cboShiftPattern_SelectedIndexChanged(this.cboShiftPattern, new RadComboBoxSelectedIndexChangedEventArgs(this.cboShiftPattern.Text, string.Empty, this.cboShiftPattern.SelectedValue, string.Empty));

                this.txtShiftPatAutoID.Text = string.Empty;
                this.txtShiftPatCode.Text = string.Empty;
                this.txtShiftPatDesc.Text = string.Empty;
                this.rblIsDayShift.ClearSelection();

                // Initialize panels and tables
                this.tblShiftTimingFilter.Style[HtmlTextWriterStyle.Display] = "none";
                this.panGridShiftSequence.Style[HtmlTextWriterStyle.Display] = "none";
                this.panShiftPatternDetail.Style[HtmlTextWriterStyle.Display] = "none";
                #endregion

                // Cler collections
                this.ShiftTimingScheduleList.Clear();
                this.ShiftPointerSequenceList.Clear();
                this.ShiftPatternList.Clear();
                this.WorkShiftList.Clear();

                // Clear sessions
                ViewState["CustomErrorMsg"] = null;
                ViewState["CurrentStartRowIndex"] = null;
                ViewState["CurrentMaximumRows"] = null;
                ViewState["CurrentPageIndex"] = null;
                ViewState["CurrentPageSize"] = null;

                // Reset the grid
                this.gridShiftTiming.VirtualItemCount = 1;
                this.gridShiftTiming.CurrentPageIndex = 0;
                this.CurrentPageIndex = this.gridShiftTiming.CurrentPageIndex + 1;
                this.CurrentPageSize = this.gridShiftTiming.PageSize;

                InitializeDataToShiftTimingGrid();
                InitializeDataToShiftSequenceGrid();
            }

            // Set form into edit mode
            ToggleButton(false);


            // Reload the data
            this.btnSearch_Click(this.btnSearch, new EventArgs());
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                #region Perform Data Validation
                int errorCount = 0;
                string shiftPatCode = this.cboShiftPattern.SelectedValue;

                //if (this.cboShiftPattern.Text == string.Empty)
                //{
                //    this.txtGeneric.Text = ValidationErrorType.NoShiftPattern.ToString();
                //    this.ErrorType = ValidationErrorType.NoShiftPattern;
                //    this.cusValShiftPattern.Validate();
                //    errorCount++;
                //}

                if (errorCount > 0)
                {
                    // Set focus to the top panel
                    Page.SetFocus(this.lnkMoveUp.ClientID);
                    return;
                }
                #endregion

                // Reset page index
                this.gridShiftTiming.CurrentPageIndex = 0;
                this.CurrentPageIndex = this.gridShiftTiming.CurrentPageIndex + 1;
                this.CurrentPageSize = this.gridShiftTiming.PageSize;                                

                // Get shift timing schedules
                GetShiftTimingSchedule(shiftPatCode);

                // Get shift timing sequences
                if (!string.IsNullOrEmpty(shiftPatCode) &&
                    shiftPatCode != UIHelper.CONST_COMBO_EMTYITEM_ID)
                {
                    GetShiftTimingSequence(shiftPatCode);
                }
            }
            catch(Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }               

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            //// Reset collection
            //this.CheckedCostCenterList.Clear();

            //#region Loop through each record in the grid
            //GridDataItemCollection gridData = this.gridShiftTiming.MasterTableView.Items;
            //if (gridData.Count > 0)
            //{
            //    foreach (GridDataItem item in gridData)
            //    {
            //        System.Web.UI.WebControls.CheckBox chkSelectColumn = item["CheckboxSelectColumn"].Controls[0] as System.Web.UI.WebControls.CheckBox;
            //        int empNo = UIHelper.ConvertObjectToInt(this.gridShiftTiming.MasterTableView.Items[item.ItemIndex].GetDataKeyValue("EmpNo"));

            //        if (chkSelectColumn != null)
            //        {
            //            if (chkSelectColumn.Checked)
            //            {
            //                if (this.ShiftTimingScheduleList.Count > 0 && empNo > 0)
            //                {
            //                    MasterShiftPatternEntity selectedRecord = this.ShiftTimingScheduleList
            //                        .Where(a => a.EmpNo == empNo)
            //                        .FirstOrDefault();
            //                    if (selectedRecord != null)
            //                    {
            //                        // Check if item already exist in the collection
            //                        if (this.CheckedCostCenterList.Count == 0)
            //                        {
            //                            this.CheckedCostCenterList.Add(selectedRecord);
            //                        }
            //                        else if (this.CheckedCostCenterList.Count > 0 &&
            //                            this.CheckedCostCenterList.Where(a => a.EmpNo == selectedRecord.EmpNo).FirstOrDefault() == null)
            //                        {
            //                            this.CheckedCostCenterList.Add(selectedRecord);
            //                        }
            //                    }
            //                }
            //            }
            //            else
            //            {
            //                #region Check if record exist in the selected item collection
            //                if (empNo > 0)
            //                {
            //                    MasterShiftPatternEntity selectedRecord = this.ShiftTimingScheduleList
            //                        .Where(a => a.EmpNo == empNo)
            //                        .FirstOrDefault();
            //                    if (selectedRecord != null)
            //                    {
            //                        if (this.CheckedCostCenterList.Count > 0
            //                            && this.CheckedCostCenterList.Where(a => a.EmpNo == selectedRecord.EmpNo).FirstOrDefault() != null)
            //                        {
            //                            MasterShiftPatternEntity itemToDelete = this.CheckedCostCenterList
            //                                .Where(a => a.EmpNo == selectedRecord.EmpNo)
            //                                .FirstOrDefault();
            //                            if (itemToDelete != null)
            //                            {
            //                                this.CheckedCostCenterList.Remove(itemToDelete);
            //                            }
            //                        }
            //                    }
            //                }
            //                #endregion
            //            }
            //        }
            //    }
            //}
            //#endregion

            //#region Display confirmation message
            //// Check for selected swipe records to submit for approval
            //if (this.CheckedCostCenterList.Count == 0)
            //{
            //    this.txtGeneric.Text = ValidationErrorType.NoRecordToDelete.ToString();
            //    this.ErrorType = ValidationErrorType.NoRecordToDelete;
            //    this.cusValButton.Validate();

            //    // Refresh the grid
            //    RebindDataToShiftTimingGrid();
            //}
            //else
            //{
            //    StringBuilder script = new StringBuilder();
            //    script.Append("ConfirmButtonAction('");
            //    script.Append(string.Concat(this.btnDeleteDummy.ClientID, "','"));
            //    script.Append(string.Concat(this.btnRebind.ClientID, "','"));
            //    script.Append(UIHelper.CONST_DELETE_CONFIRMATION + "');");
            //    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "Delete Confirmation", script.ToString(), true);
            //}
            //#endregion
        }                

        protected void btnRebind_Click(object sender, EventArgs e)
        {
            RebindDataToShiftTimingGrid();
        }                

        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                #region Perform Validation                        
                int errorCount = 0;
                string shiftPatCode = this.cboShiftPattern.SelectedValue;
                if (shiftPatCode == UIHelper.CONST_COMBO_EMTYITEM_ID)
                    shiftPatCode = string.Empty;

                if (shiftPatCode == string.Empty)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoShiftPattern.ToString();
                    this.ErrorType = ValidationErrorType.NoShiftPattern;
                    this.cusValShiftPattern.Validate();
                    errorCount++;
                }

                if (this.ShiftTimingScheduleList.Count == 0)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoShiftTimingSchedule.ToString();
                    this.ErrorType = ValidationErrorType.NoShiftTimingSchedule;
                    this.cusValWorkShift.Validate();
                    errorCount++;
                }

                if (this.ShiftPointerSequenceList.Count == 0)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoShiftTimingSequence.ToString();
                    this.ErrorType = ValidationErrorType.NoShiftTimingSequence;
                    this.cusValWorkShift2.Validate();
                    errorCount++;
                }

                #region Loop through each Shift Timing Schedule record to validate the data entries
                //foreach (MasterShiftPatternEntity item in this.ShiftTimingScheduleList)
                //{
                //    #region Validate input time
                //    if (item.ArrivalFrom > item.ArrivalTo)
                //    {
                //        this.txtGeneric.Text = ValidationErrorType.InvalidArrivalTime.ToString();
                //        this.ErrorType = ValidationErrorType.InvalidArrivalTime;
                //        this.cusValButton.Validate();
                //        errorCount++;
                //    }
                //    #endregion
                //}
                #endregion

                if (errorCount > 0)
                {
                    // Set focus to the top panel
                    Page.SetFocus(this.lnkMoveUp.ClientID);
                    return;
                }
                #endregion

                SaveChanges(shiftPatCode);
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void btnAddShift_Click(object sender, EventArgs e)
        {
            try
            {
                #region Perform Data Validation
                int errorCount = 0;

                if (this.cboWorkShift.Text == string.Empty)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoShiftCode.ToString();
                    this.ErrorType = ValidationErrorType.NoShiftCode;
                    this.cusValWorkShift.Validate();
                    errorCount++;
                }
                else
                {
                    // Check if selected shift already exist
                    if (this.ShiftTimingScheduleList.Count > 0)
                    {
                        MasterShiftPatternEntity selectedShift = this.ShiftTimingScheduleList
                            .Where(a => a.ShiftPatCode == this.cboShiftPattern.SelectedValue && a.ShiftCode == this.cboWorkShift.SelectedValue)
                            .FirstOrDefault();
                        if (selectedShift != null)
                        {
                            this.txtGeneric.Text = ValidationErrorType.ShiftCodeAlreadyExist.ToString();
                            this.ErrorType = ValidationErrorType.ShiftCodeAlreadyExist;
                            this.cusValWorkShift.Validate();
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

                #region Add new shift timing schedule in the grid
                MasterShiftPatternEntity selectedShiftPattern = this.ShiftPatternList
                    .Where(a => a.ShiftPatCode == this.cboShiftPattern.SelectedValue)
                    .FirstOrDefault();

                MasterShiftPatternEntity selectedShifCode = this.ShiftCodeList
                    .Where(a => a.ShiftCode == this.cboWorkShift.SelectedValue)
                    .FirstOrDefault();

                MasterShiftPatternEntity newShiftTiming = new MasterShiftPatternEntity()
                {
                    ShiftPatCode = this.cboShiftPattern.SelectedValue,
                    ShiftCode = this.cboWorkShift.SelectedValue,
                    ShiftFullDescription = this.cboWorkShift.Text,
                    IsDayShift = selectedShiftPattern != null ? selectedShiftPattern.IsDayShift : false,
                    CreatedByEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]),
                    CreatedByEmpName = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_FULLNAME]),
                    CreatedByFullName = string.Format("({0}) {1}",
                        UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]),
                        UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_FULLNAME])),
                    CreatedByUserID = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]),
                    CreatedDate = DateTime.Now
                };

                if (selectedShifCode != null)
                {
                    newShiftTiming.ArrivalFrom = selectedShifCode.ArrivalFrom;
                    newShiftTiming.ArrivalTo = selectedShifCode.ArrivalTo;
                    newShiftTiming.DepartFrom = selectedShifCode.DepartFrom;
                    newShiftTiming.DepartTo = selectedShifCode.DepartTo;
                    newShiftTiming.DurationNormalDayString = selectedShifCode.DurationNormalDayString;
                    newShiftTiming.RArrivalFrom = selectedShifCode.RArrivalFrom;
                    newShiftTiming.RArrivalTo = selectedShifCode.RArrivalTo;
                    newShiftTiming.RDepartFrom = selectedShifCode.RDepartFrom;
                    newShiftTiming.RDepartTo = selectedShifCode.RDepartTo;
                    newShiftTiming.DurationRamadanDayString = selectedShifCode.DurationRamadanDayString;
                }

                // Add item to the collection
                this.ShiftTimingScheduleList.Add(newShiftTiming);

                // Refresh the grid
                RebindDataToShiftTimingGrid();

                // Set the form into edit mode
                ToggleButton(true);
                #endregion

                // Reset the work shift combobox
                this.cboWorkShift.SelectedIndex = -1;
                this.cboWorkShift.Text = string.Empty;
            }
            catch(Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void btnAddShift2_Click(object sender, EventArgs e)
        {
            try
            {
                #region Perform Data Validation
                int errorCount = 0;

                if (this.cboWorkShift2.Text == string.Empty)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoShiftCode.ToString();
                    this.ErrorType = ValidationErrorType.NoShiftCode;
                    this.cusValWorkShift2.Validate();
                    errorCount++;
                }

                if (errorCount > 0)
                {
                    // Set focus to the top panel
                    Page.SetFocus(this.lnkMoveUp.ClientID);
                    return;
                }
                #endregion

                #region Add new shift timing sequence in the grid
                MasterShiftPatternEntity selectedShifCode = this.ShiftCodeList
                    .Where(a => a.ShiftCode == this.cboWorkShift2.SelectedValue)
                    .FirstOrDefault();

                MasterShiftPatternEntity newShiftSequence = new MasterShiftPatternEntity()
                {
                    ShiftPatCode = this.cboShiftPattern.SelectedValue,
                    ShiftCode = this.cboWorkShift2.SelectedValue,
                    ShiftFullDescription = this.cboWorkShift2.Text
                };

                if (this.ShiftPointerSequenceList.Count > 0)
                    newShiftSequence.ShiftPointer = this.ShiftPointerSequenceList.OrderByDescending(a => a.ShiftPointer).FirstOrDefault().ShiftPointer + 1;
                else
                    newShiftSequence.ShiftPointer = 1;

                // Add item to the collection
                this.ShiftPointerSequenceList.Add(newShiftSequence);

                // Refresh the grid
                RebindDataToShiftSequenceGrid();

                // Set the form into edit mode
                ToggleButton(true);

                // Go to the last page index
                int currentPageIndex = this.gridShiftSequence.PageCount - 1;
                if (currentPageIndex < 0)
                    currentPageIndex = 0;
                
                this.gridShiftSequence.CurrentPageIndex = currentPageIndex;
                this.gridShiftSequence.MasterTableView.CurrentPageIndex = currentPageIndex;
                #endregion
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void btnRemoveShiftTiming_Click(object sender, EventArgs e)
        {
            if (this.ShiftTimingScheduleList.Count > 0 &&
                this.SelectedShiftTimingRecord != null)
            {
                MasterShiftPatternEntity itemToRemove = this.ShiftTimingScheduleList
                    .Where(a => a.AutoID == this.SelectedShiftTimingRecord.AutoID)
                    .FirstOrDefault();
                if (itemToRemove != null)
                {
                    this.ShiftTimingScheduleList.Remove(itemToRemove);

                    // Refresh the grid
                    RebindDataToShiftTimingGrid();

                    // Set the form into edit mode
                    ToggleButton(true);
                }
            }
        }

        protected void btnRemoveShiftSequence_Click(object sender, EventArgs e)
        {
            if (this.ShiftPointerSequenceList.Count > 0 &&
                this.SelectedShiftPointerRecord != null)
            {
                MasterShiftPatternEntity itemToRemove = this.ShiftPointerSequenceList
                    .Where(a => a.AutoID == this.SelectedShiftPointerRecord.AutoID)
                    .FirstOrDefault();
                if (itemToRemove != null)
                {
                    this.ShiftPointerSequenceList.Remove(itemToRemove);

                    // Re-arrange the pointer
                    int counter = 1;
                    foreach (MasterShiftPatternEntity item in ShiftPointerSequenceList)
                    {
                        item.ShiftPointer = counter;
                        counter++;
                    }

                    // Refresh the grid
                    RebindDataToShiftSequenceGrid();

                    // Set the form into edit mode
                    ToggleButton(true);
                }
            }
        }

        protected void btnRebindShiftPointer_Click(object sender, EventArgs e)
        {
            RebindDataToShiftSequenceGrid();
        }

        protected void btnSaveShiftPattern_Click(object sender, EventArgs e)
        {
            try
            {
                #region Perform Validation                        
                int errorCount = 0;
                string shiftPatCode = this.txtShiftPatCode.Text.Trim();

                if (shiftPatCode == string.Empty)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoShiftPatternCode.ToString();
                    this.ErrorType = ValidationErrorType.NoShiftPatternCode;
                    this.cusValShiftPatCode.Validate();
                    errorCount++;
                }

                if (errorCount > 0)
                {
                    // Set focus to the top panel
                    Page.SetFocus(this.lnkMoveUp.ClientID);
                    return;
                }
                #endregion

                UIHelper.SaveType saveType = UIHelper.SaveType.NotDefined;
                if (this.SelectedShiftPattenCode != null)
                {
                    this.SelectedShiftPattenCode.ShiftPatDescription = this.txtShiftPatDesc.Text.Trim();
                    this.SelectedShiftPattenCode.IsDayShift = UIHelper.ConvertNumberToBolean(this.rblIsDayShift.SelectedValue);
                    this.SelectedShiftPattenCode.LastUpdateUserID = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]);
                    this.SelectedShiftPattenCode.LastUpdateTime = DateTime.Now;

                    SaveShiftPatternDetails(Convert.ToInt32(UIHelper.SaveType.Update), this.SelectedShiftPattenCode);
                }
                else
                {
                    MasterShiftPatternEntity shiftPatternInfo = new MasterShiftPatternEntity()
                    {
                        ShiftPatCode = shiftPatCode,
                        ShiftPatDescription = this.txtShiftPatDesc.Text.Trim(),
                        IsDayShift = UIHelper.ConvertNumberToBolean(this.rblIsDayShift.SelectedValue)
                    };

                    if (saveType == UIHelper.SaveType.Insert)
                    {
                        shiftPatternInfo.CreatedByUserID = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]);
                        shiftPatternInfo.CreatedDate = DateTime.Now;
                    }
                    else
                    {
                        shiftPatternInfo.LastUpdateUserID = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]);
                        shiftPatternInfo.LastUpdateTime = DateTime.Now;
                    }

                    SaveShiftPatternDetails(Convert.ToInt32(UIHelper.SaveType.Insert), shiftPatternInfo);
                }
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }            
        }

        protected void btnDeleteShiftPattern_Click(object sender, EventArgs e)
        {
            #region Perform Validation                        
            int errorCount = 0;
            string shiftPatCode = this.cboShiftPattern.SelectedValue; //this.txtShiftPatCode.Text.Trim();

            if (shiftPatCode == string.Empty ||
                this.cboShiftPattern.SelectedValue == UIHelper.CONST_COMBO_EMTYITEM_ID)
            {
                this.txtGeneric.Text = ValidationErrorType.NoShiftPatternCode.ToString();
                this.ErrorType = ValidationErrorType.NoShiftPatternCode;
                this.cusValShiftPatCode.Validate();
                errorCount++;
            }

            if (errorCount > 0)
            {
                // Set focus to the top panel
                Page.SetFocus(this.lnkMoveUp.ClientID);
                return;
            }
            #endregion

            StringBuilder script = new StringBuilder();
            script.Append("ConfirmButtonActionNoPostback('");
            script.Append(string.Concat(this.btnDeleteDummy.ClientID, "','"));
            script.Append(UIHelper.CONST_DELETE_SHIFTPATTERN_CONFIRMATION + "');");
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "Delete Confirmation", script.ToString(), true);
        }

        protected void btnDeleteDummy_Click(object sender, EventArgs e)
        {
            try
            {
                MasterShiftPatternEntity shiftPatternInfo = new MasterShiftPatternEntity()
                {
                    AutoID = UIHelper.ConvertObjectToInt(this.txtShiftPatAutoID.Text),
                    ShiftPatCode = this.cboShiftPattern.SelectedValue,  //this.txtShiftPatCode.Text.Trim(),
                    ShiftPatDescription = this.txtShiftPatDesc.Text.Trim(),
                    IsDayShift = UIHelper.ConvertNumberToBolean(this.rblIsDayShift.SelectedValue),
                    LastUpdateUserID = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]),
                    LastUpdateTime = DateTime.Now
                };

                SaveShiftPatternDetails(Convert.ToInt32(UIHelper.SaveType.Delete), shiftPatternInfo);
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void btnShiftPatternDetail_Click(object sender, EventArgs e)
        {
            #region Initialize the form
            // Initialize buttons
            this.btnShiftPatternDetail.Enabled = false;
            this.btnSearch.Enabled = false;
            this.btnNew.Enabled = false;
            //this.btnDeleteShiftPattern.Enabled = true;

            // Initialize panels
            this.panShiftPatternDetail.Style[HtmlTextWriterStyle.Display] = string.Empty;
            this.panButton.Style[HtmlTextWriterStyle.Display] = "none";
            this.panGridShiftTiming.Enabled = false;
            this.panGridShiftSequence.Enabled = false;

            // Initialize controls
            this.cboShiftPattern.Enabled = false;
            this.rblDayShift.Enabled = false;
            this.rblFlexitime.Enabled = false;
            this.txtShiftPatCode.Enabled = false;
            #endregion

            #region Fill data to the Shift Pattern Details                         
            if (this.SelectedShiftPattenCode != null)
            {
                this.txtShiftPatAutoID.Text = this.SelectedShiftPattenCode.AutoID.ToString();
                this.txtShiftPatCode.Text = this.SelectedShiftPattenCode.ShiftPatCode;
                this.txtShiftPatDesc.Text = this.SelectedShiftPattenCode.ShiftPatDescription;
                this.rblIsDayShift.SelectedValue = this.SelectedShiftPattenCode.IsDayShift ? "1" : "0";
            }
            else
            {
                this.txtShiftPatAutoID.Text = string.Empty;
                this.txtShiftPatCode.Text = string.Empty;
                this.txtShiftPatDesc.Text = string.Empty;
                this.rblIsDayShift.ClearSelection();
            }
            #endregion
        }

        protected void btnNew_Click(object sender, EventArgs e)
        {
            // Clear the selection in the Shift Pattern combobox
            this.cboShiftPattern.SelectedValue = UIHelper.CONST_COMBO_EMTYITEM_ID;
            this.cboShiftPattern.Text = string.Empty;
            this.cboShiftPattern_SelectedIndexChanged(this.cboShiftPattern, new RadComboBoxSelectedIndexChangedEventArgs(this.cboShiftPattern.Text, string.Empty, this.cboShiftPattern.SelectedValue, string.Empty));

            // Clear the Shift Pattern Details
            this.txtShiftPatAutoID.Text = string.Empty;
            this.txtShiftPatCode.Text = string.Empty;
            this.txtShiftPatCode.Enabled = true;
            this.txtShiftPatDesc.Text = string.Empty;
            this.rblIsDayShift.SelectedValue = "0";
            this.txtShiftPatCode.Focus();

            // Initialize buttons
            this.btnShiftPatternDetail.Enabled = false;
            this.btnSearch.Enabled = false;
            this.btnNew.Enabled = false;
            //this.btnDeleteShiftPattern.Enabled = false;

            // Initialize panels
            this.panShiftPatternDetail.Style[HtmlTextWriterStyle.Display] = string.Empty;
            this.panButton.Style[HtmlTextWriterStyle.Display] = "none";
            this.panGridShiftTiming.Enabled = false;
            this.panGridShiftSequence.Enabled = false;

            // Initialize controls
            this.cboShiftPattern.Enabled = false;
            this.rblDayShift.Enabled = false;
            this.rblFlexitime.Enabled = false;

            // Reset session variables
            this.SelectedShiftPattenCode = null;                               
        }

        protected void btnCancelShiftPattern_Click(object sender, EventArgs e)
        {
            // Initialize buttons
            this.btnShiftPatternDetail.Enabled = true;
            this.btnSearch.Enabled = true;
            this.btnNew.Enabled = true;

            // Initialize panels
            this.panShiftPatternDetail.Style[HtmlTextWriterStyle.Display] = "none";
            this.panButton.Style[HtmlTextWriterStyle.Display] = string.Empty;
            this.panGridShiftTiming.Enabled = true;
            this.panGridShiftSequence.Enabled = true;

            // Initialize controls
            this.cboShiftPattern.Enabled = true;
            this.rblDayShift.Enabled = true;
            this.rblFlexitime.Enabled = true;
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.CallerForm))
                Response.Redirect(this.CallerForm, false);
            else
                Response.Redirect(UIHelper.PAGE_SHIFT_PATTERN_CHANGE_ENTRY, false);
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
                else if (this.ErrorType == ValidationErrorType.NoRecordToDelete)
                {
                    validator.ErrorMessage = "Please select the record(s) you wish to delete in the grid!";
                    validator.ToolTip = "Please select the record(s)you wish to delete in the grid!";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoShiftPattern)
                {
                    validator.ErrorMessage = "Shift Pattern is required!";
                    validator.ToolTip = "Shift Pattern is required!";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoShiftCode)
                {
                    validator.ErrorMessage = "Shift Timing is required!";
                    validator.ToolTip = "Shift Timing is required!";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.ShiftCodeAlreadyExist)
                {
                    validator.ErrorMessage = "The selected work shift already exist in the Shift Timing Schedule.";
                    validator.ToolTip = "The selected work shift already exist in the Shift Timing Schedule.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoShiftTimingSchedule)
                {
                    validator.ErrorMessage = "Unable to save shift pattern details because there are no shift timing schedule defined.";
                    validator.ToolTip = "Unable to save shift pattern details because there are no shift timing schedule defined.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoShiftTimingSequence)
                {
                    validator.ErrorMessage = "Unable to save shift pattern details because there are no shift timing sequence defined.";
                    validator.ToolTip = "Unable to save shift pattern details because there are no shift timing sequence defined.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.InvalidArrivalTime)
                {
                    validator.ErrorMessage = "Invalid arrival timing during normal days. Take note that Arrival From should be less than Arrival To.";
                    validator.ToolTip = "Invalid arrival timing during normal days. Take note that Arrival From should be less than Arrival To.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.InvalidRArrivalTime)
                {
                    validator.ErrorMessage = "Invalid arrival timing during Ramadan. Take note that Arrival From should be less than Arrival To.";
                    validator.ToolTip = "Invalid arrival timing during Ramadan. Take note that Arrival From should be less than Arrival To.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.InvalidDepartureTime)
                {
                    validator.ErrorMessage = "Invalid departure timing during normal days. Take note that Depart From should be less than Depart To.";
                    validator.ToolTip = "Invalid departure timing during normal days. Take note that Depart From should be less than Depart To.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.InvalidRDepartureTime)
                {
                    validator.ErrorMessage = "Invalid departure timing during Ramadan. Take note that Depart From should be less than Depart To.";
                    validator.ToolTip = "Invalid departure timing during Ramadan. Take note that Depart From should be less than Depart To.";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.CannotDeleteOneShiftTiming)
                {
                    validator.ErrorMessage = "Sorry, you cannot remove all shift timing schedules. At least one record should exist!";
                    validator.ToolTip = "Sorry, you cannot remove all shift timing schedules. At least one record should exist!";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.CannotDeleteOneShiftPointer)
                {
                    validator.ErrorMessage = "Sorry, you cannot remove all shift timing sequence. At least one record should exist!";
                    validator.ToolTip = "Sorry, you cannot remove all shift timing sequence. At least one record should exist!";
                    args.IsValid = false;
                }
                else if (this.ErrorType == ValidationErrorType.NoShiftPatternCode)
                {
                    validator.ErrorMessage = "Shift Pattern Code is required!";
                    validator.ToolTip = "Shift Pattern Code is required!";
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

        protected void cboShiftPattern_SelectedIndexChanged(object sender, RadComboBoxSelectedIndexChangedEventArgs e)
        {
            if (this.cboShiftPattern.SelectedValue != string.Empty &&
                this.cboShiftPattern.SelectedValue != UIHelper.CONST_COMBO_EMTYITEM_ID)
            {
                // Show panels and tables
                this.tblShiftTimingFilter.Style[HtmlTextWriterStyle.Display] = string.Empty;
                this.panGridShiftSequence.Style[HtmlTextWriterStyle.Display] = string.Empty;
                this.panButton.Style[HtmlTextWriterStyle.Display] = string.Empty;

                // Initialize controls
                this.btnDeleteShiftPattern.Enabled = true;
                this.btnShiftPatternDetail.Enabled = true;
            }
            else
            {
                // Hide panels and tables
                this.tblShiftTimingFilter.Style[HtmlTextWriterStyle.Display] = "none";
                this.panGridShiftSequence.Style[HtmlTextWriterStyle.Display] = "none";
                this.panButton.Style[HtmlTextWriterStyle.Display] = "none";

                // Initialize controls
                this.btnDeleteShiftPattern.Enabled = false;
                this.btnShiftPatternDetail.Enabled = false;
            }

            #region Fill data to Shift Pattern Details
            if (!string.IsNullOrEmpty(this.cboShiftPattern.SelectedValue) &&
                this.cboShiftPattern.SelectedValue != UIHelper.CONST_COMBO_EMTYITEM_ID)
            {
                this.SelectedShiftPattenCode = this.ShiftPatternList
                    .Where(a => a.ShiftPatCode == this.cboShiftPattern.SelectedValue)
                    .FirstOrDefault();
            }
            #endregion

            // Reload data in the grid
            this.btnSearch_Click(this.btnSearch, new EventArgs());
        }
                
        protected void lnkAddNewShift_Click(object sender, EventArgs e)
        {

        }

        protected void lnkUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                LinkButton lnkUpdate = sender as LinkButton;
                GridDataItem item = lnkUpdate.NamingContainer as GridDataItem;
                if (item != null)
                {
                    //// Get data key value
                    //int empNo = UIHelper.ConvertObjectToInt(this.gridUserFormAccess.MasterTableView.Items[item.ItemIndex].GetDataKeyValue("EmpNo"));
                    //if (empNo > 0)
                    //{
                    //    // Show the details panel
                    //    this.panDetails.Style[HtmlTextWriterStyle.Display] = string.Empty;

                    //    // Fetch details from the database
                    //    GetPermittedCostCenterList(empNo);
                    //}
                }
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void cboShiftCode_ItemsRequested(object sender, RadComboBoxItemsRequestedEventArgs e)
        {
            try
            {
                RadComboBox cboShiftCode = (RadComboBox)sender;
                GridDataItem gridItem = cboShiftCode.Parent.Parent as GridDataItem;
                if (gridItem != null)
                {
                    FillDataToShiftCodeCombo();
                }

                if (this.ShiftCodeList != null)
                {
                    // Clear combobox items
                    cboShiftCode.Items.Clear();

                    foreach (MasterShiftPatternEntity item in this.ShiftCodeList)
                    {
                        RadComboBoxItem cboItem = new RadComboBoxItem();
                        cboItem.Text = item.ShiftFullDescription;
                        cboItem.Value = item.ShiftCode;
                        cboItem.Attributes.Add(item.ShiftCode, item.ShiftDescription);

                        // Add item to combobox
                        cboShiftCode.Items.Add(cboItem);
                        cboItem.DataBind();
                    }

                    if (this.SelectedShiftTimingRecord != null &&
                        !string.IsNullOrEmpty(this.SelectedShiftTimingRecord.ShiftCode))
                    {
                        cboShiftCode.SelectedValue = this.SelectedShiftTimingRecord.ShiftCode;
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void cboShiftCode_SelectedIndexChanged(object sender, RadComboBoxSelectedIndexChangedEventArgs e)
        {

        }

        protected void cboShiftTiming_ItemsRequested(object sender, RadComboBoxItemsRequestedEventArgs e)
        {
            try
            {
                RadComboBox cboShiftTiming = (RadComboBox)sender;
                //GridDataItem gridItem = cboShiftTiming.Parent.Parent as GridDataItem;
                //if (gridItem != null)
                //{
                //    FillDataToShiftCodeCombo(true, null, false);
                //}

                if (this.WorkShiftList != null)
                {
                    // Clear combobox items
                    cboShiftTiming.Items.Clear();

                    foreach (MasterShiftPatternEntity item in this.WorkShiftList)
                    {
                        RadComboBoxItem cboItem = new RadComboBoxItem();
                        cboItem.Text = item.ShiftFullDescription;
                        cboItem.Value = item.ShiftCode;
                        cboItem.Attributes.Add(item.ShiftCode, item.ShiftDescription);

                        // Add item to combobox
                        cboShiftTiming.Items.Add(cboItem);
                        cboItem.DataBind();
                    }

                    if (this.SelectedShiftPointerRecord != null &&
                        !string.IsNullOrEmpty(this.SelectedShiftPointerRecord.ShiftCode))
                    {
                        cboShiftTiming.SelectedValue = this.SelectedShiftPointerRecord.ShiftCode;
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void cboShiftTiming_SelectedIndexChanged(object sender, RadComboBoxSelectedIndexChangedEventArgs e)
        {
            RadComboBox cboShiftTiming = sender as RadComboBox;
            GridDataItem item = cboShiftTiming.NamingContainer as GridDataItem;
            if (item != null)
            {
                // Get data key value
                int autoID = UIHelper.ConvertObjectToInt(this.gridShiftSequence.MasterTableView.Items[item.ItemIndex].GetDataKeyValue("AutoID"));
                if (autoID > 0)
                {
                    this.SelectedShiftPointerRecord = this.ShiftPointerSequenceList
                        .Where(a => a.AutoID == autoID)
                        .FirstOrDefault();
                    if (this.SelectedShiftPointerRecord != null)
                    {
                        if (!this.SelectedShiftPointerRecord.IsDirty)
                            this.SelectedShiftPointerRecord.IsDirty = true;

                        this.SelectedShiftPointerRecord.ShiftCode = cboShiftTiming.SelectedValue;

                        // Set form into edit mode
                        ToggleButton(true);
                    }
                }
            }
        }

        protected void cboWorkShift2_ItemsRequested(object sender, RadComboBoxItemsRequestedEventArgs e)
        {
            try
            {
                if (this.ShiftTimingScheduleList != null)
                {
                    // Clear combobox items
                    this.cboWorkShift2.Items.Clear();

                    foreach (MasterShiftPatternEntity item in this.ShiftTimingScheduleList)
                    {
                        RadComboBoxItem cboItem = new RadComboBoxItem();
                        cboItem.Text = item.ShiftFullDescription;
                        cboItem.Value = item.ShiftCode;
                        cboItem.Attributes.Add(item.ShiftCode, item.ShiftDescription);

                        // Add item to combobox
                        this.cboWorkShift2.Items.Add(cboItem);
                        cboItem.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        protected void cboWorkShift2_SelectedIndexChanged(object sender, RadComboBoxSelectedIndexChangedEventArgs e)
        {

        }

        protected void dtpArrivalFrom_TextChanged(object sender, EventArgs e)
        {
            RadDateInput dtpArrivalFrom = sender as RadDateInput;
            GridDataItem item = dtpArrivalFrom.NamingContainer as GridDataItem;
            if (item != null)
            {                
                // Get data key value
                int autoID = UIHelper.ConvertObjectToInt(this.gridShiftTiming.MasterTableView.Items[item.ItemIndex].GetDataKeyValue("AutoID"));
                if (autoID > 0)
                {
                    this.SelectedShiftTimingRecord = this.ShiftTimingScheduleList
                        .Where(a => a.AutoID == autoID)
                        .FirstOrDefault();
                    if (this.SelectedShiftTimingRecord != null)
                    {
                        #region Validate input time
                        if (dtpArrivalFrom.SelectedDate >= this.SelectedShiftTimingRecord.ArrivalTo ||
                            dtpArrivalFrom.SelectedDate == null)
                        {
                            dtpArrivalFrom.SelectedDate = this.SelectedShiftTimingRecord.ArrivalFrom;

                            //this.txtGeneric.Text = ValidationErrorType.InvalidArrivalTime.ToString();
                            //this.ErrorType = ValidationErrorType.InvalidArrivalTime;
                            //this.cusValButton.Validate();
                            //return;
                        }
                        #endregion

                        if (!this.SelectedShiftTimingRecord.IsDirty)
                            this.SelectedShiftTimingRecord.IsDirty = true;

                        this.SelectedShiftTimingRecord.ArrivalFrom = dtpArrivalFrom.SelectedDate;

                        // Set form into edit mode
                        ToggleButton(true);
                    }
                }
            }
        }

        protected void dtpArrivalTo_TextChanged(object sender, EventArgs e)
        {
            RadDateInput dtpArrivalTo = sender as RadDateInput;
            GridDataItem item = dtpArrivalTo.NamingContainer as GridDataItem;
            if (item != null)
            {
                // Get data key value
                int autoID = UIHelper.ConvertObjectToInt(this.gridShiftTiming.MasterTableView.Items[item.ItemIndex].GetDataKeyValue("AutoID"));
                if (autoID > 0)
                {
                    this.SelectedShiftTimingRecord = this.ShiftTimingScheduleList
                        .Where(a => a.AutoID == autoID)
                        .FirstOrDefault();
                    if (this.SelectedShiftTimingRecord != null)
                    {
                        #region Validate input time
                        if (dtpArrivalTo.SelectedDate <= this.SelectedShiftTimingRecord.ArrivalFrom ||
                            dtpArrivalTo.SelectedDate == null)
                        {
                            // Get the old value from the database
                            dtpArrivalTo.SelectedDate = this.SelectedShiftTimingRecord.ArrivalTo;
                        }
                        #endregion

                        if (!this.SelectedShiftTimingRecord.IsDirty)
                            this.SelectedShiftTimingRecord.IsDirty = true;

                        this.SelectedShiftTimingRecord.ArrivalTo = dtpArrivalTo.SelectedDate;

                        // Set form into edit mode
                        ToggleButton(true);
                    }
                }
            }
        }

        protected void dtpDepartFrom_TextChanged(object sender, EventArgs e)
        {
            RadDateInput dtpDepartFrom = sender as RadDateInput;
            GridDataItem item = dtpDepartFrom.NamingContainer as GridDataItem;
            if (item != null)
            {
                // Get data key value
                int autoID = UIHelper.ConvertObjectToInt(this.gridShiftTiming.MasterTableView.Items[item.ItemIndex].GetDataKeyValue("AutoID"));
                if (autoID > 0)
                {
                    this.SelectedShiftTimingRecord = this.ShiftTimingScheduleList
                        .Where(a => a.AutoID == autoID)
                        .FirstOrDefault();
                    if (this.SelectedShiftTimingRecord != null)
                    {
                        #region Validate input time
                        if (dtpDepartFrom.SelectedDate >= this.SelectedShiftTimingRecord.DepartTo ||
                            dtpDepartFrom.SelectedDate == null)
                        {
                            // Get the old value from the database
                            dtpDepartFrom.SelectedDate = this.SelectedShiftTimingRecord.DepartFrom;
                        }
                        #endregion

                        if (!this.SelectedShiftTimingRecord.IsDirty)
                            this.SelectedShiftTimingRecord.IsDirty = true;

                        this.SelectedShiftTimingRecord.DepartFrom = dtpDepartFrom.SelectedDate;

                        // Set form into edit mode
                        ToggleButton(true);
                    }
                }
            }
        }

        protected void dtpDepartTo_TextChanged(object sender, EventArgs e)
        {
            RadDateInput dtpDepartTo = sender as RadDateInput;
            GridDataItem item = dtpDepartTo.NamingContainer as GridDataItem;
            if (item != null)
            {
                // Get data key value
                int autoID = UIHelper.ConvertObjectToInt(this.gridShiftTiming.MasterTableView.Items[item.ItemIndex].GetDataKeyValue("AutoID"));
                if (autoID > 0)
                {
                    this.SelectedShiftTimingRecord = this.ShiftTimingScheduleList
                        .Where(a => a.AutoID == autoID)
                        .FirstOrDefault();
                    if (this.SelectedShiftTimingRecord != null)
                    {
                        #region Validate input time
                        if (dtpDepartTo.SelectedDate <= this.SelectedShiftTimingRecord.DepartFrom ||
                            dtpDepartTo.SelectedDate == null)
                        {
                            // Get the old value from the database
                            dtpDepartTo.SelectedDate = this.SelectedShiftTimingRecord.DepartTo;
                        }
                        #endregion

                        if (!this.SelectedShiftTimingRecord.IsDirty)
                            this.SelectedShiftTimingRecord.IsDirty = true;

                        this.SelectedShiftTimingRecord.DepartTo = dtpDepartTo.SelectedDate;

                        // Set form into edit mode
                        ToggleButton(true);
                    }
                }
            }
        }

        protected void dtpRArrivalFrom_TextChanged(object sender, EventArgs e)
        {
            RadDateInput dtpRArrivalFrom = sender as RadDateInput;
            GridDataItem item = dtpRArrivalFrom.NamingContainer as GridDataItem;
            if (item != null)
            {
                // Get data key value
                int autoID = UIHelper.ConvertObjectToInt(this.gridShiftTiming.MasterTableView.Items[item.ItemIndex].GetDataKeyValue("AutoID"));
                if (autoID > 0)
                {
                    this.SelectedShiftTimingRecord = this.ShiftTimingScheduleList
                        .Where(a => a.AutoID == autoID)
                        .FirstOrDefault();
                    if (this.SelectedShiftTimingRecord != null)
                    {
                        #region Validate input time
                        if (dtpRArrivalFrom.SelectedDate >= this.SelectedShiftTimingRecord.RArrivalTo ||
                            dtpRArrivalFrom.SelectedDate == null)
                        {
                            dtpRArrivalFrom.SelectedDate = this.SelectedShiftTimingRecord.RArrivalFrom;
                        }
                        #endregion

                        if (!this.SelectedShiftTimingRecord.IsDirty)
                            this.SelectedShiftTimingRecord.IsDirty = true;

                        this.SelectedShiftTimingRecord.RArrivalFrom = dtpRArrivalFrom.SelectedDate;

                        // Set form into edit mode
                        ToggleButton(true);
                    }
                }
            }
        }

        protected void dtpRArrivalTo_TextChanged(object sender, EventArgs e)
        {
            RadDateInput dtpRArrivalTo = sender as RadDateInput;
            GridDataItem item = dtpRArrivalTo.NamingContainer as GridDataItem;
            if (item != null)
            {
                // Get data key value
                int autoID = UIHelper.ConvertObjectToInt(this.gridShiftTiming.MasterTableView.Items[item.ItemIndex].GetDataKeyValue("AutoID"));
                if (autoID > 0)
                {
                    this.SelectedShiftTimingRecord = this.ShiftTimingScheduleList
                        .Where(a => a.AutoID == autoID)
                        .FirstOrDefault();
                    if (this.SelectedShiftTimingRecord != null)
                    {
                        #region Validate input time
                        if (dtpRArrivalTo.SelectedDate <= this.SelectedShiftTimingRecord.RArrivalFrom ||
                            dtpRArrivalTo.SelectedDate == null)
                        {
                            // Get the old value from the database
                            dtpRArrivalTo.SelectedDate = this.SelectedShiftTimingRecord.RArrivalTo;
                        }
                        #endregion

                        if (!this.SelectedShiftTimingRecord.IsDirty)
                            this.SelectedShiftTimingRecord.IsDirty = true;

                        this.SelectedShiftTimingRecord.RArrivalTo = dtpRArrivalTo.SelectedDate;

                        // Set form into edit mode
                        ToggleButton(true);
                    }
                }
            }
        }

        protected void dtpRDepartFrom_TextChanged(object sender, EventArgs e)
        {
            RadDateInput dtpRDepartFrom = sender as RadDateInput;
            GridDataItem item = dtpRDepartFrom.NamingContainer as GridDataItem;
            if (item != null)
            {
                // Get data key value
                int autoID = UIHelper.ConvertObjectToInt(this.gridShiftTiming.MasterTableView.Items[item.ItemIndex].GetDataKeyValue("AutoID"));
                if (autoID > 0)
                {
                    this.SelectedShiftTimingRecord = this.ShiftTimingScheduleList
                        .Where(a => a.AutoID == autoID)
                        .FirstOrDefault();
                    if (this.SelectedShiftTimingRecord != null)
                    {
                        #region Validate input time
                        if (dtpRDepartFrom.SelectedDate >= this.SelectedShiftTimingRecord.RDepartTo ||
                            dtpRDepartFrom.SelectedDate == null)
                        {
                            // Get the old value from the database
                            dtpRDepartFrom.SelectedDate = this.SelectedShiftTimingRecord.RDepartFrom;
                        }
                        #endregion

                        if (!this.SelectedShiftTimingRecord.IsDirty)
                            this.SelectedShiftTimingRecord.IsDirty = true;

                        this.SelectedShiftTimingRecord.RDepartFrom = dtpRDepartFrom.SelectedDate;

                        // Set form into edit mode
                        ToggleButton(true);
                    }
                }
            }
        }

        protected void dtpRDepartTo_TextChanged(object sender, EventArgs e)
        {
            RadDateInput dtpRDepartTo = sender as RadDateInput;
            GridDataItem item = dtpRDepartTo.NamingContainer as GridDataItem;
            if (item != null)
            {
                // Get data key value
                int autoID = UIHelper.ConvertObjectToInt(this.gridShiftTiming.MasterTableView.Items[item.ItemIndex].GetDataKeyValue("AutoID"));
                if (autoID > 0)
                {
                    this.SelectedShiftTimingRecord = this.ShiftTimingScheduleList
                        .Where(a => a.AutoID == autoID)
                        .FirstOrDefault();
                    if (this.SelectedShiftTimingRecord != null)
                    {
                        #region Validate input time
                        if (dtpRDepartTo.SelectedDate <= this.SelectedShiftTimingRecord.RDepartFrom ||
                            dtpRDepartTo.SelectedDate == null)
                        {
                            // Get the old value from the database
                            dtpRDepartTo.SelectedDate = this.SelectedShiftTimingRecord.RDepartTo;
                        }
                        #endregion

                        if (!this.SelectedShiftTimingRecord.IsDirty)
                            this.SelectedShiftTimingRecord.IsDirty = true;

                        this.SelectedShiftTimingRecord.RDepartTo = dtpRDepartTo.SelectedDate;

                        // Set form into edit mode
                        ToggleButton(true);
                    }
                }
            }
        }
        #endregion

        #region IFormExtension Interface Implementation
        public void ClearForm()
        {
            #region Reset controls
            this.cboWorkShift.SelectedIndex = -1;
            this.cboWorkShift.Text = string.Empty;
            this.cboWorkShift2.SelectedIndex = -1;
            this.cboWorkShift2.Text = string.Empty;
            this.cboShiftPattern.Text = string.Empty;
            this.cboShiftPattern.SelectedIndex = -1;
            this.cboShiftPattern_SelectedIndexChanged(this.cboShiftPattern, new RadComboBoxSelectedIndexChangedEventArgs(this.cboShiftPattern.Text, string.Empty, this.cboShiftPattern.SelectedValue, string.Empty));
            this.txtShiftPatAutoID.Text = string.Empty;
            this.txtShiftPatCode.Text = string.Empty;
            this.txtShiftPatDesc.Text = string.Empty;
            this.rblIsDayShift.ClearSelection();

            // Initialize panels and tables
            this.tblShiftTimingFilter.Style[HtmlTextWriterStyle.Display] = "none";
            this.panGridShiftSequence.Style[HtmlTextWriterStyle.Display] = "none";
            this.panShiftPatternDetail.Style[HtmlTextWriterStyle.Display] = "none";
            this.panButton.Style[HtmlTextWriterStyle.Display] = "none";
            #endregion

            // Clear collections
            this.ShiftTimingScheduleList.Clear();
            this.ShiftPointerSequenceList.Clear();
            this.ShiftPatternList.Clear();
            this.ShiftCodeList.Clear();
            this.WorkShiftList.Clear();

            KillSessions();

            // Reset the grid
            this.gridShiftTiming.VirtualItemCount = 1;
            this.gridShiftTiming.CurrentPageIndex = 0;
            this.CurrentPageIndex = this.gridShiftTiming.CurrentPageIndex + 1;
            this.CurrentPageSize = this.gridShiftTiming.PageSize;

            InitializeDataToShiftTimingGrid();
            InitializeDataToShiftSequenceGrid();
        }

        public void AddControlsAttribute()
        {

        }

        public void ProcessQueryString()
        {
            this.CallerForm = UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_CALLER_FORM_KEY]);
            this.ReloadGridData = UIHelper.ConvertObjectToBolean(Request.QueryString[UIHelper.QUERY_STRING_RELOAD_DATA_KEY]);
            this.IsReadonlyView = UIHelper.ConvertObjectToBolean(Request.QueryString["IsReadonlyView"]);
            this.ShiftPatCode = UIHelper.ConvertObjectToString(Request.QueryString["ShiftPatCode"]);
        }

        public void KillSessions()
        {
            // Cler collections

            // Clear sessions
            ViewState["CustomErrorMsg"] = null;
            ViewState["CurrentStartRowIndex"] = null;
            ViewState["CurrentMaximumRows"] = null;
            ViewState["CurrentPageIndex"] = null;
            ViewState["CurrentPageSize"] = null;
            ViewState["SelectedShiftTimingRecord"] = null;
            ViewState["SelectedShiftPointerRecord"] = null;
            ViewState["SelectedShiftPattenCode"] = null;
            ViewState["IsReadonlyView"] = null;
            ViewState["ShiftPatCode"] = null;

            // Clear all viewstates
            ViewState.Clear();
        }
        #endregion

        #region Private Methods
        private void RestoreDataFromCollection()
        {
            if (this.MasterShiftPatternStorage.Count == 0)
                return;

            #region Restore Query String values
            if (this.MasterShiftPatternStorage.ContainsKey("CallerForm"))
                this.CallerForm = UIHelper.ConvertObjectToString(this.MasterShiftPatternStorage["CallerForm"]);
            else
                this.CallerForm = string.Empty;

            if (this.MasterShiftPatternStorage.ContainsKey("ReloadGridData"))
                this.ReloadGridData = UIHelper.ConvertObjectToBolean(this.MasterShiftPatternStorage["ReloadGridData"]);
            else
                this.ReloadGridData = false;

            if (this.MasterShiftPatternStorage.ContainsKey("IsReadonlyView"))
                this.IsReadonlyView = UIHelper.ConvertObjectToBolean(this.MasterShiftPatternStorage["IsReadonlyView"]);
            else
                this.IsReadonlyView = false;

            if (this.MasterShiftPatternStorage.ContainsKey("ShiftPatCode"))
                this.ShiftPatCode = UIHelper.ConvertObjectToString(this.MasterShiftPatternStorage["ShiftPatCode"]);
            else
                this.ShiftPatCode = string.Empty;
            #endregion

            #region Restore session values
            if (this.MasterShiftPatternStorage.ContainsKey("CurrentStartRowIndex"))
                this.CurrentStartRowIndex = UIHelper.ConvertObjectToInt(this.MasterShiftPatternStorage["CurrentStartRowIndex"]);
            else
                this.CurrentStartRowIndex = 0;

            if (this.MasterShiftPatternStorage.ContainsKey("CurrentMaximumRows"))
                this.CurrentMaximumRows = UIHelper.ConvertObjectToInt(this.MasterShiftPatternStorage["CurrentMaximumRows"]);
            else
                this.CurrentMaximumRows = 0;

            if (this.MasterShiftPatternStorage.ContainsKey("CurrentPageIndex"))
                this.CurrentPageIndex = UIHelper.ConvertObjectToInt(this.MasterShiftPatternStorage["CurrentPageIndex"]);
            else
                this.CurrentPageIndex = 0;

            if (this.MasterShiftPatternStorage.ContainsKey("CurrentPageSize"))
                this.CurrentPageSize = UIHelper.ConvertObjectToInt(this.MasterShiftPatternStorage["CurrentPageSize"]);
            else
                this.CurrentPageSize = 0;

            if (this.MasterShiftPatternStorage.ContainsKey("ShiftTimingScheduleList"))
                this.ShiftTimingScheduleList = this.MasterShiftPatternStorage["ShiftTimingScheduleList"] as List<MasterShiftPatternEntity>;
            else
                this.ShiftTimingScheduleList = null;

            if (this.MasterShiftPatternStorage.ContainsKey("ShiftPointerSequenceList"))
                this.ShiftPointerSequenceList = this.MasterShiftPatternStorage["ShiftPointerSequenceList"] as List<MasterShiftPatternEntity>;
            else
                this.ShiftPointerSequenceList = null;

            if (this.MasterShiftPatternStorage.ContainsKey("ShiftPatternList"))
                this.ShiftPatternList = this.MasterShiftPatternStorage["ShiftPatternList"] as List<MasterShiftPatternEntity>;
            else
                this.ShiftPatternList = null;

            if (this.MasterShiftPatternStorage.ContainsKey("ShiftCodeList"))
                this.ShiftCodeList = this.MasterShiftPatternStorage["ShiftCodeList"] as List<MasterShiftPatternEntity>;
            else
                this.ShiftCodeList = null;

            if (this.MasterShiftPatternStorage.ContainsKey("WorkShiftList"))
                this.WorkShiftList = this.MasterShiftPatternStorage["WorkShiftList"] as List<MasterShiftPatternEntity>;
            else
                this.WorkShiftList = null;

            if (this.MasterShiftPatternStorage.ContainsKey("SelectedShiftTimingRecord"))
                this.SelectedShiftTimingRecord = this.MasterShiftPatternStorage["SelectedShiftTimingRecord"] as MasterShiftPatternEntity;
            else
                this.SelectedShiftTimingRecord = null;

            if (this.MasterShiftPatternStorage.ContainsKey("SelectedShiftPointerRecord"))
                this.SelectedShiftPointerRecord = this.MasterShiftPatternStorage["SelectedShiftPointerRecord"] as MasterShiftPatternEntity;
            else
                this.SelectedShiftPointerRecord = null;

            if (this.MasterShiftPatternStorage.ContainsKey("SelectedShiftPattenCode"))
                this.SelectedShiftPattenCode = this.MasterShiftPatternStorage["SelectedShiftPattenCode"] as MasterShiftPatternEntity;
            else
                this.SelectedShiftPattenCode = null;

            FillComboData(false);
            #endregion

            #region Restore control values            
            if (this.MasterShiftPatternStorage.ContainsKey("cboShiftPattern"))
                this.cboShiftPattern.SelectedValue = UIHelper.ConvertObjectToString(this.MasterShiftPatternStorage["cboShiftPattern"]);
            else
            {
                this.cboShiftPattern.Text = string.Empty;
                this.cboShiftPattern.SelectedIndex = -1;
            }

            if (this.MasterShiftPatternStorage.ContainsKey("cboWorkShift"))
                this.cboWorkShift.SelectedValue = UIHelper.ConvertObjectToString(this.MasterShiftPatternStorage["cboWorkShift"]);
            else
            {
                this.cboWorkShift.Text = string.Empty;
                this.cboWorkShift.SelectedIndex = -1;
            }

            if (this.MasterShiftPatternStorage.ContainsKey("cboWorkShift2"))
                this.cboWorkShift2.SelectedValue = UIHelper.ConvertObjectToString(this.MasterShiftPatternStorage["cboWorkShift2"]);
            else
            {
                this.cboWorkShift2.Text = string.Empty;
                this.cboWorkShift2.SelectedIndex = -1;
            }
            #endregion

            // Refresh the grid
            RebindDataToShiftTimingGrid();

            // Set the grid attributes
            this.gridShiftTiming.CurrentPageIndex = this.CurrentPageIndex;
            this.gridShiftTiming.MasterTableView.CurrentPageIndex = this.CurrentPageIndex;
            this.gridShiftTiming.MasterTableView.PageSize = this.CurrentPageSize;
            this.gridShiftTiming.MasterTableView.DataBind();
        }

        private void StoreDataToCollection(UIHelper.PagePostBackFlags formFlag)
        {
            this.MasterShiftPatternStorage.Clear();
            this.MasterShiftPatternStorage.Add("FormFlag", formFlag.ToString());

            #region Save control values to session            
            this.MasterShiftPatternStorage.Add("cboShiftPattern", this.cboShiftPattern.SelectedValue);
            this.MasterShiftPatternStorage.Add("cboWorkShift", this.cboWorkShift.SelectedValue);
            this.MasterShiftPatternStorage.Add("cboWorkShift2", this.cboWorkShift2.SelectedValue);
            #endregion

            #region Save Query String values to collection
            this.MasterShiftPatternStorage.Add("CallerForm", this.CallerForm);
            this.MasterShiftPatternStorage.Add("ReloadGridData", this.ReloadGridData);
            this.MasterShiftPatternStorage.Add("IsReadonlyView", this.IsReadonlyView);
            this.MasterShiftPatternStorage.Add("ShiftPatCode", this.ShiftPatCode);
            #endregion

            #region Store session data to collection
            this.MasterShiftPatternStorage.Add("CurrentStartRowIndex", this.CurrentStartRowIndex);
            this.MasterShiftPatternStorage.Add("CurrentMaximumRows", this.CurrentMaximumRows);
            this.MasterShiftPatternStorage.Add("CurrentPageIndex", this.CurrentPageIndex);
            this.MasterShiftPatternStorage.Add("CurrentPageSize", this.CurrentPageSize);
            this.MasterShiftPatternStorage.Add("ShiftTimingScheduleList", this.ShiftTimingScheduleList);
            this.MasterShiftPatternStorage.Add("ShiftPointerSequenceList", this.ShiftPointerSequenceList);
            this.MasterShiftPatternStorage.Add("ShiftPatternList", this.ShiftPatternList);
            this.MasterShiftPatternStorage.Add("ShiftCodeList", this.ShiftCodeList);
            this.MasterShiftPatternStorage.Add("WorkShiftList", this.WorkShiftList);
            this.MasterShiftPatternStorage.Add("SelectedShiftTimingRecord", this.SelectedShiftTimingRecord);
            this.MasterShiftPatternStorage.Add("SelectedShiftPointerRecord", this.SelectedShiftPointerRecord);
            this.MasterShiftPatternStorage.Add("SelectedShiftPattenCode", this.SelectedShiftPattenCode);
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
            FillDataToShiftPatternCombo(reloadFromDB);
            FillDataToShiftCodeCombo(reloadFromDB, this.cboWorkShift);            
        }

        private void ToggleButton(bool isEditMode)
        {
            if (isEditMode)
            {
                this.btnNew.Enabled = false;
                this.btnDelete.Enabled = false;
                this.btnDeleteShiftPattern.Enabled = false;
                this.btnSearch.Enabled = false;
                this.btnShiftPatternDetail.Enabled = false;
                this.btnSave.Enabled = true;
            }
            else
            {
                this.btnNew.Enabled = true;
                this.btnDelete.Enabled = true;
                this.btnDeleteShiftPattern.Enabled = true;
                this.btnSearch.Enabled = true;
                this.btnShiftPatternDetail.Enabled = true;
                this.btnSave.Enabled = false;
            }
        }
        #endregion

        #region Database Access
        private void GetShiftTimingSchedule(string shiftPatCode)
        {
            try
            {
                #region Initialize variables and objects            
                string error = string.Empty;
                string innerError = string.Empty;

                if (shiftPatCode == UIHelper.CONST_COMBO_EMTYITEM_ID)
                    shiftPatCode = string.Empty;
                
                byte isDayShift = Convert.ToByte(this.rblDayShift.SelectedValue);
                byte isFlexitime = Convert.ToByte(this.rblFlexitime.SelectedValue);

                // Reset session
                this.ShiftTimingScheduleList.Clear();
                #endregion

                #region Fill data to the collection
                DALProxy proxy = new DALProxy();
                var source = proxy.GetShiftPatternDetail(1, shiftPatCode, isDayShift, isFlexitime, ref error, ref innerError);
                if (!string.IsNullOrEmpty(error) || 
                    !string.IsNullOrEmpty(innerError))
                {
                    if (!string.IsNullOrEmpty(innerError))
                        throw new Exception(innerError);
                    else
                        throw new Exception(error);
                }
                else
                {
                    if (source != null)
                    {
                        this.ShiftTimingScheduleList.AddRange(source);
                    }
                }
                #endregion

                // Fill data in the grid
                RebindDataToShiftTimingGrid();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void GetShiftTimingSequence(string shiftPatCode)
        {
            try
            {
                #region Initialize variables and objects            
                string error = string.Empty;
                string innerError = string.Empty;                                

                // Reset session
                this.ShiftPointerSequenceList.Clear();
                #endregion

                #region Fill data to the collection
                DALProxy proxy = new DALProxy();
                var source = proxy.GetShiftPatternDetail(2, shiftPatCode, 0, 0, ref error, ref innerError);
                if (!string.IsNullOrEmpty(error) ||
                    !string.IsNullOrEmpty(innerError))
                {
                    if (!string.IsNullOrEmpty(innerError))
                        throw new Exception(innerError);
                    else
                        throw new Exception(error);
                }
                else
                {
                    if (source != null)
                    {
                        this.ShiftPointerSequenceList.AddRange(source);
                    }
                }
                #endregion

                // Fill data in the grid
                RebindDataToShiftSequenceGrid();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool DeleteShiftPattern(List<MasterShiftPatternEntity> recordToDeleteList)
        {
            if (recordToDeleteList == null || recordToDeleteList.Count == 0)
                return false;
                        
            try
            {
                string error = string.Empty;
                string innerError = string.Empty;
                int userEmpNo = UIHelper.ConvertObjectToInt(Session[UIHelper.GARMCO_USERID]);

                //foreach (MasterShiftPatternEntity item in recordToDeleteList)
                //{
                //    error = string.Empty;
                //    innerError = string.Empty;

                //    DALProxy proxy = new DALProxy();
                //    proxy.InsertUpdateDeleteShiftPattern(4, 0, item.EmpNo, string.Empty, userEmpNo, ref error, ref innerError);
                //    if (!string.IsNullOrEmpty(error) || !string.IsNullOrEmpty(innerError))
                //    {
                //        if (!string.IsNullOrEmpty(innerError))
                //            throw new Exception(error, new Exception(innerError));
                //        else
                //            throw new Exception(error);
                //    }

                //}

                return true;
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
                return false;
            }
        }

        private void FillDataToShiftPatternCombo(bool reloadFromDB)
        {
            try
            {
                List<MasterShiftPatternEntity> comboSource = new List<MasterShiftPatternEntity>();

                if (this.ShiftPatternList.Count > 0 && !reloadFromDB)
                {
                    comboSource = this.ShiftPatternList;
                }
                else
                {
                    string error = string.Empty;
                    string innerError = string.Empty;

                    DALProxy proxy = new DALProxy();
                    var rawData = proxy.GetShiftPatternList(ref error, ref innerError);
                    if (!string.IsNullOrEmpty(error) ||
                        !string.IsNullOrEmpty(innerError))
                    {
                        if (!string.IsNullOrEmpty(innerError))
                            throw new Exception(innerError);
                        else
                            throw new Exception(error);
                    }
                    else
                    {
                        if (rawData != null && 
                            rawData.Count() > 0)
                        {
                            comboSource.AddRange(rawData.ToList());

                            // Add blank item
                            comboSource.Insert(0, new MasterShiftPatternEntity() { ShiftCode = UIHelper.CONST_COMBO_EMTYITEM_ID, ShiftPatDescription = string.Empty });
                        }
                    }
                }

                // Store to session
                this.ShiftPatternList = comboSource;

                #region Bind data to combobox
                this.cboShiftPattern.DataSource = this.ShiftPatternList;
                this.cboShiftPattern.DataTextField = "ShiftPatternFullName";
                this.cboShiftPattern.DataValueField = "ShiftPatCode";
                this.cboShiftPattern.DataBind();
                #endregion
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        private void FillDataToShiftCodeCombo(bool reloadFromDB = true, RadComboBox cbo = null, bool removeDayOff = true)
        {
            try
            {
                List<MasterShiftPatternEntity> comboSource = new List<MasterShiftPatternEntity>();
                if (this.ShiftCodeList.Count > 0 && 
                    !reloadFromDB)
                {
                    comboSource = this.ShiftCodeList;
                }
                else
                {
                    string error = string.Empty;
                    string innerError = string.Empty;

                    DALProxy proxy = new DALProxy();
                    var rawData = proxy.GetShiftCodeList(string.Empty, ref error, ref innerError);
                    if (!string.IsNullOrEmpty(error) ||
                        !string.IsNullOrEmpty(innerError))
                    {
                        if (!string.IsNullOrEmpty(innerError))
                            throw new Exception(innerError);
                        else
                            throw new Exception(error);
                    }
                    else
                    {
                        if (rawData != null &&
                            rawData.Count() > 0)
                        {
                            if (removeDayOff)
                                comboSource.AddRange(rawData.Where(a => a.ShiftCode != "O").ToList());
                            else
                                comboSource.AddRange(rawData.ToList());

                            // Add blank item
                            comboSource.Insert(0, new MasterShiftPatternEntity() { ShiftCode = UIHelper.CONST_COMBO_EMTYITEM_ID, ShiftFullDescription = string.Empty, ShiftDescription = string.Empty });
                        }
                    }
                }

                // Store to session
                this.ShiftCodeList = comboSource;

                if (cbo != null)
                {
                    cbo.DataSource = this.ShiftCodeList;
                    cbo.DataTextField = "ShiftFullDescription";
                    cbo.DataValueField = "ShiftCode";
                    cbo.DataBind();
                }
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        private void SaveChanges(string shiftPatCode)
        {
            string error = string.Empty;
            string innerError = string.Empty;

            try
            {
                DALProxy proxy = new DALProxy();
                proxy.SaveMasterShiftPattern(shiftPatCode, this.ShiftTimingScheduleList, this.ShiftPointerSequenceList, ref error, ref innerError);
                if (!string.IsNullOrEmpty(error) || 
                    !string.IsNullOrEmpty(innerError))
                {
                    if (!string.IsNullOrEmpty(innerError))
                        throw new Exception(innerError);
                    else
                        throw new Exception(error);
                }
                else
                {
                    ToggleButton(false);
                    //UIHelper.DisplayJavaScriptMessage(this, "Changes have been saved successfully!");
                    this.btnSearch_Click(this.btnSearch, new EventArgs());
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString());
            }
        }

        private void SaveShiftPatternDetails(int saveTypeID, MasterShiftPatternEntity shiftPatternInfo)
        {
            string error = string.Empty;
            string innerError = string.Empty;

            try
            {
                DALProxy proxy = new DALProxy();
                proxy.InsertUpdateDeleteShiftPattern(saveTypeID, shiftPatternInfo, ref error, ref innerError);
                if (!string.IsNullOrEmpty(error) ||
                    !string.IsNullOrEmpty(innerError))
                {
                    if (!string.IsNullOrEmpty(innerError))
                        throw new Exception(innerError);
                    else
                        throw new Exception(error);
                }
                else
                {
                    // Reload the combobox
                    FillDataToShiftPatternCombo(true);

                    // Set the selected shift pattern
                    this.cboShiftPattern.SelectedValue = shiftPatternInfo.ShiftPatCode;

                    if (saveTypeID == Convert.ToInt32(UIHelper.SaveType.Insert))
                        this.cboShiftPattern_SelectedIndexChanged(this.cboShiftPattern, new RadComboBoxSelectedIndexChangedEventArgs(this.cboShiftPattern.Text, string.Empty, this.cboShiftPattern.SelectedValue, string.Empty));
                    else if (saveTypeID == Convert.ToInt32(UIHelper.SaveType.Delete))
                    {
                        UIHelper.DisplayJavaScriptMessage(this, "The selected shift pattern and all associated records have been deleted successfully!");
                        this.SelectedShiftPattenCode = null;
                        this.cboShiftPattern.SelectedIndex = -1;
                        this.cboShiftPattern.Text = string.Empty;
                        this.cboShiftPattern_SelectedIndexChanged(this.cboShiftPattern, new RadComboBoxSelectedIndexChangedEventArgs(this.cboShiftPattern.Text, string.Empty, this.cboShiftPattern.SelectedValue, string.Empty));
                    }

                    // Initialize buttons
                    this.btnShiftPatternDetail.Enabled = true;
                    this.btnSearch.Enabled = true;
                    this.btnNew.Enabled = true;

                    // Initialize panels
                    this.panShiftPatternDetail.Style[HtmlTextWriterStyle.Display] = "none";
                    this.panButton.Style[HtmlTextWriterStyle.Display] = string.Empty;
                    this.panGridShiftTiming.Enabled = true;
                    this.panGridShiftSequence.Enabled = true;

                    // Initialize controls
                    this.cboShiftPattern.Enabled = true;
                    this.rblDayShift.Enabled = true;
                    this.rblFlexitime.Enabled = true;
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