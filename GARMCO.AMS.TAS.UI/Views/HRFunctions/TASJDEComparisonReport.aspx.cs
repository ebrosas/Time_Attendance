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
using System.IO;
using OfficeOpenXml;

namespace GARMCO.AMS.TAS.UI.Views.HRFunctions
{
    public partial class TASJDEComparisonReport : BaseWebForm, IFormExtension
    {
        #region Enumeration
        private enum ValidationErrorType
        {
            NoError,
            CustomFormError
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

        private Dictionary<string, object> TASJDEComparisonStorage
        {
            get
            {
                Dictionary<string, object> list = Session["TASJDEComparisonStorage"] as Dictionary<string, object>;
                if (list == null)
                    Session["TASJDEComparisonStorage"] = list = new Dictionary<string, object>();

                return list;
            }
            set
            {
                Session["TASJDEComparisonStorage"] = value;
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

        private List<TASJDEComparisonEntity> TASJDEComparisonList
        {
            get
            {
                List<TASJDEComparisonEntity> list = ViewState["TASJDEComparisonList"] as List<TASJDEComparisonEntity>;
                if (list == null)
                    ViewState["TASJDEComparisonList"] = list = new List<TASJDEComparisonEntity>();

                return list;
            }
            set
            {
                ViewState["TASJDEComparisonList"] = value;
            }
        }

        private List<TASJDEComparisonEntity> TASHistoryList
        {
            get
            {
                List<TASJDEComparisonEntity> list = ViewState["TASHistoryList"] as List<TASJDEComparisonEntity>;
                if (list == null)
                    ViewState["TASHistoryList"] = list = new List<TASJDEComparisonEntity>();

                return list;
            }
            set
            {
                ViewState["TASHistoryList"] = value;
            }
        }

        private List<TASJDEComparisonEntity> JDEHistoryList
        {
            get
            {
                List<TASJDEComparisonEntity> list = ViewState["JDEHistoryList"] as List<TASJDEComparisonEntity>;
                if (list == null)
                    ViewState["JDEHistoryList"] = list = new List<TASJDEComparisonEntity>();

                return list;
            }
            set
            {
                ViewState["JDEHistoryList"] = value;
            }
        }

        private string CurrentPDBA
        {
            get
            {
                return UIHelper.ConvertObjectToString(ViewState["CurrentPDBA"]);
            }
            set
            {
                ViewState["CurrentPDBA"] = value;
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

                this.Master.SetPageForm(UIHelper.FormAccessCodes.TASJDECOMP.ToString());

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
                this.Master.FormTitle = UIHelper.PAGE_TASJDECOMPARISON_TITLE;
                #endregion

                #region Check if user has permission to access the page
                if (!GAPFunction.CheckFormAccess(this.Master.FormAccess, GAPConstants.FormAccessIndex.Retrieve))
                {
                    if (!UIHelper.ConvertObjectToBolean(Session[UIHelper.GARMCO_USER_IS_SPECIAL]))
                    {
                        Response.Redirect(String.Format("{0}?error={1}&pageName={2}", UIHelper.PAGE_ERROR, Convert.ToInt32(UIHelper.PageErrorCodes.NoAccessOnPage), UIHelper.PAGE_TASJDECOMPARISON_TITLE), true);
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
                if (this.TASJDEComparisonStorage.Count > 0)
                {
                    if (this.TASJDEComparisonStorage.ContainsKey("FormFlag"))
                        formFlag = UIHelper.ConvertObjectToString(this.TASJDEComparisonStorage["FormFlag"]);
                }
                #endregion

                if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.RedirectToOtherPage.ToString())
                {
                    #region Show the last form data
                    RestoreDataFromCollection();

                    // Clear data storage
                    Session.Remove("TASJDEComparisonStorage");
                    #endregion
                }
                else
                {
                    ClearForm();
                    ProcessQueryString();

                    // Fill data to the grid
                    this.btnSearch_Click(this.btnSearch, new EventArgs());
                }

                this.btnExportToExcel.Enabled = false;
            }

            ScriptManager.RegisterStartupScript(this, this.GetType(), "HideScrollbar", "HideScrollbar();", true);
            AddControlsAttribute();
        }
        #endregion

        #region Grid Events

        #region Main Grid Events                
        protected void gridSearchResult_PageIndexChanged(object sender, Telerik.Web.UI.GridPageChangedEventArgs e)
        {
            RebindDataToGrid();

            // Reset session variables
            this.TASHistoryList.Clear();
            this.JDEHistoryList.Clear();

            // Hide panels
            this.panTASHistory.Style[HtmlTextWriterStyle.Display] = "none";
            this.panJDEHistory.Style[HtmlTextWriterStyle.Display] = "none";
        }

        protected void gridSearchResult_PageSizeChanged(object sender, Telerik.Web.UI.GridPageSizeChangedEventArgs e)
        {
            RebindDataToGrid();

            // Reset session variables
            this.TASHistoryList.Clear();
            this.JDEHistoryList.Clear();

            // Hide panels
            this.panTASHistory.Style[HtmlTextWriterStyle.Display] = "none";
            this.panJDEHistory.Style[HtmlTextWriterStyle.Display] = "none";
        }

        protected void gridSearchResult_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (this.TASJDEComparisonList.Count > 0)
            {
                this.gridSearchResult.DataSource = this.TASJDEComparisonList;
                this.gridSearchResult.DataBind();

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
                        sortExpr.SortOrder = this.gridSearchResult.MasterTableView.AllowNaturalSort ? GridSortOrder.None : GridSortOrder.Descending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;

                    case GridSortOrder.Descending:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = GridSortOrder.Ascending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;
                }

                e.Canceled = true;
                this.gridSearchResult.Rebind();
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
                    #region Show TAS and JDE transaction histories
                    this.panTASHistory.Style[HtmlTextWriterStyle.Display] = string.Empty;
                    this.panJDEHistory.Style[HtmlTextWriterStyle.Display] = string.Empty;

                    // Get data key value
                    this.CurrentPDBA = UIHelper.ConvertObjectToString(this.gridSearchResult.MasterTableView.Items[e.Item.ItemIndex].GetDataKeyValue("PDBA"));
                    if (!string.IsNullOrEmpty(this.CurrentPDBA))
                    {
                        GetTASAndJDEHistory(this.CurrentPDBA);
                    }
                    #endregion
                }
            }
            else if (e.CommandName.Equals(RadGrid.ExportToExcelCommandName) ||
                  e.CommandName.Equals(RadGrid.ExportToWordCommandName) ||
                  e.CommandName.Equals(RadGrid.ExportToCsvCommandName) ||
                  e.CommandName.Equals(RadGrid.ExportToPdfCommandName))
            {
                this.gridSearchResult.AllowPaging = false;
                RebindDataToGrid();

                this.gridSearchResult.ExportSettings.Excel.Format = GridExcelExportFormat.Biff;
                this.gridSearchResult.ExportSettings.IgnorePaging = true;
                this.gridSearchResult.ExportSettings.ExportOnlyData = true;
                this.gridSearchResult.ExportSettings.OpenInNewWindow = true;
                this.gridSearchResult.ExportSettings.UseItemStyles = true;

                this.gridSearchResult.AllowPaging = true;
                this.gridSearchResult.Rebind();
            }
            else if (e.CommandName.Equals(RadGrid.RebindGridCommandName))
            {
                RebindDataToGrid();
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
            if (!IsPostBack && !string.IsNullOrEmpty(this.CurrentPDBA))
            {
                #region Set the currently selected row in the grid                                
                foreach (GridDataItem item in this.gridSearchResult.MasterTableView.Items)
                {
                    if (item["PDBA"].Text == this.CurrentPDBA)
                    {
                        item.Selected = true;
                    }
                }
                #endregion
            }
        }

        private void RebindDataToGrid()
        {
            if (this.TASJDEComparisonList.Count > 0)
            {
                this.gridSearchResult.DataSource = this.TASJDEComparisonList;
                this.gridSearchResult.DataBind();

                //Display the record count
                this.lblRecordCount.Text = string.Format("{0} record(s) found", this.TASJDEComparisonList.Count.ToString("#,###"));
            }
            else
                InitializeGrid();
        }

        private void InitializeGrid()
        {
            this.gridSearchResult.DataSource = new List<TASJDEComparisonEntity>();
            this.gridSearchResult.DataBind();
        }
        #endregion

        #region TAS History Grid Events                
        protected void gridTASHistory_PageIndexChanged(object sender, Telerik.Web.UI.GridPageChangedEventArgs e)
        {
            RebindDataToTASHistoryGrid();
        }

        protected void gridTASHistory_PageSizeChanged(object sender, Telerik.Web.UI.GridPageSizeChangedEventArgs e)
        {
            RebindDataToTASHistoryGrid();
        }

        protected void gridTASHistory_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (this.TASHistoryList.Count > 0)
            {
                this.gridTASHistory.DataSource = this.TASHistoryList;
                this.gridTASHistory.DataBind();

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
                        sortExpr.SortOrder = this.gridTASHistory.MasterTableView.AllowNaturalSort ? GridSortOrder.None : GridSortOrder.Descending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;

                    case GridSortOrder.Descending:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = GridSortOrder.Ascending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;
                }

                e.Canceled = true;
                this.gridTASHistory.Rebind();
            }
            else
                InitializeTASHistoryGrid();
        }

        protected void gridTASHistory_ItemCommand(object sender, GridCommandEventArgs e)
        {
            if (e.CommandName.Equals(RadGrid.SelectCommandName))
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {
                    #region Open the Timesheet History page
                    // Get data key value
                    int autoID = UIHelper.ConvertObjectToInt(this.gridTASHistory.MasterTableView.Items[e.Item.ItemIndex].GetDataKeyValue("AutoID"));
                    if (autoID > 0)
                    {
                        Session["SelectedTimesheetRecord"] = GetTimesheetRecord(autoID).FirstOrDefault();

                        // Save session values
                        StoreDataToCollection(UIHelper.PagePostBackFlags.RedirectToOtherPage);

                        Response.Redirect
                        (
                            String.Format(UIHelper.PAGE_TIMESHEET_CORRECTION_HISTORY + "?{0}={1}&{2}={3}&{4}={5}",
                            UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                            UIHelper.PAGE_TAS_JDE_COMPARISON_REPORT,
                            UIHelper.QUERY_STRING_FORM_LOAD_TYPE,
                            Convert.ToInt32(UIHelper.FormDataLoadType.OpenSpecificTimesheetRecord).ToString(),
                            UIHelper.QUERY_STRING_IDENTITY_FIELD_KEY,
                            autoID.ToString()
                        ),
                        false);
                    }
                    else
                        DisplayFormLevelError("Unable to view the history because Auto ID is not known.");
                    #endregion
                }
            }
        }

        protected void gridTASHistory_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item.ItemType == GridItemType.AlternatingItem || e.Item.ItemType == GridItemType.Item)
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {

                }
            }
        }

        private void RebindDataToTASHistoryGrid()
        {
            if (this.TASHistoryList.Count > 0)
            {
                this.gridTASHistory.DataSource = this.TASHistoryList;
                this.gridTASHistory.DataBind();
            }
            else
                InitializeTASHistoryGrid();
        }

        private void InitializeTASHistoryGrid()
        {
            this.gridTASHistory.DataSource = new List<TASJDEComparisonEntity>();
            this.gridTASHistory.DataBind();
        }
        #endregion

        #region JDE History Grid Events                
        protected void gridJDEHistory_PageIndexChanged(object sender, Telerik.Web.UI.GridPageChangedEventArgs e)
        {
            RebindDataToJDEHistoryGrid();
        }

        protected void gridJDEHistory_PageSizeChanged(object sender, Telerik.Web.UI.GridPageSizeChangedEventArgs e)
        {
            RebindDataToJDEHistoryGrid();
        }

        protected void gridJDEHistory_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (this.JDEHistoryList.Count > 0)
            {
                this.gridJDEHistory.DataSource = this.JDEHistoryList;
                this.gridJDEHistory.DataBind();

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
                        sortExpr.SortOrder = this.gridJDEHistory.MasterTableView.AllowNaturalSort ? GridSortOrder.None : GridSortOrder.Descending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;

                    case GridSortOrder.Descending:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = GridSortOrder.Ascending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;
                }

                e.Canceled = true;
                this.gridJDEHistory.Rebind();
            }
            else
                InitializeJDEHistoryGrid();
        }

        protected void gridJDEHistory_ItemCommand(object sender, GridCommandEventArgs e)
        {
            if (e.CommandName.Equals(RadGrid.SelectCommandName))
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {
                    #region Open the Timesheet History page
                    // Get data key value
                    int tsAutoID = UIHelper.ConvertObjectToInt(this.gridJDEHistory.MasterTableView.Items[e.Item.ItemIndex].GetDataKeyValue("AutoID"));
                    int jdeAutoID = UIHelper.ConvertObjectToInt(item["JAutoID"].Text);
                    int autoID = tsAutoID > 0 ? tsAutoID : jdeAutoID;

                    if (autoID > 0)
                    {
                        Session["SelectedTimesheetRecord"] = GetTimesheetRecord(autoID).FirstOrDefault();

                        // Save session values
                        StoreDataToCollection(UIHelper.PagePostBackFlags.RedirectToOtherPage);

                        Response.Redirect
                        (
                            String.Format(UIHelper.PAGE_TIMESHEET_CORRECTION_HISTORY + "?{0}={1}&{2}={3}&{4}={5}",
                            UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                            UIHelper.PAGE_TAS_JDE_COMPARISON_REPORT,
                            UIHelper.QUERY_STRING_FORM_LOAD_TYPE,
                            Convert.ToInt32(UIHelper.FormDataLoadType.OpenSpecificTimesheetRecord).ToString(),
                            UIHelper.QUERY_STRING_IDENTITY_FIELD_KEY,
                            autoID.ToString()
                        ),
                        false);
                    }
                    else
                        DisplayFormLevelError("Unable to view the history because Auto ID is not known.");
                    #endregion
                }
            }
        }

        protected void gridJDEHistory_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item.ItemType == GridItemType.AlternatingItem || e.Item.ItemType == GridItemType.Item)
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {

                }
            }
        }

        private void RebindDataToJDEHistoryGrid()
        {
            if (this.JDEHistoryList.Count > 0)
            {
                this.gridJDEHistory.DataSource = this.JDEHistoryList;
                this.gridJDEHistory.DataBind();
            }
            else
                InitializeJDEHistoryGrid();
        }

        private void InitializeJDEHistoryGrid()
        {
            this.gridJDEHistory.DataSource = new List<TASJDEComparisonEntity>();
            this.gridJDEHistory.DataBind();
        }
        #endregion

        #endregion

        #region IFormExtension Interface Implementation
        public void ClearForm()
        {
            // Clear sessions and collections
            KillSessions();

            // Reset datagrid and other controls
            InitializeGrid();
            InitializeTASHistoryGrid();
            InitializeJDEHistoryGrid();

            this.gridSearchResult.CurrentPageIndex = 0;
            this.lblRecordCount.Text = "0 record found";

            // Hide panels
            this.panTASHistory.Style[HtmlTextWriterStyle.Display] = "none";
            this.panJDEHistory.Style[HtmlTextWriterStyle.Display] = "none";
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
            this.TASJDEComparisonList.Clear();
            this.TASHistoryList.Clear();
            this.JDEHistoryList.Clear();

            // Clear sessions
            ViewState["CustomErrorMsg"] = null;
            ViewState["CurrentPDBA"] = null;

            // Clear all viewstates
            ViewState.Clear();
        }
        #endregion

        #region Private Methods
        private void RestoreDataFromCollection()
        {
            if (this.TASJDEComparisonStorage.Count == 0)
                return;

            #region Restore Query String values
            if (this.TASJDEComparisonStorage.ContainsKey("CallerForm"))
                this.CallerForm = UIHelper.ConvertObjectToString(this.TASJDEComparisonStorage["CallerForm"]);
            else
                this.CallerForm = string.Empty;
            #endregion

            #region Restore session values
            if (this.TASJDEComparisonStorage.ContainsKey("TASJDEComparisonList"))
                this.TASJDEComparisonList = this.TASJDEComparisonStorage["TASJDEComparisonList"] as List<TASJDEComparisonEntity>;
            else
                this.TASJDEComparisonList = null;

            if (this.TASJDEComparisonStorage.ContainsKey("TASHistoryList"))
                this.TASHistoryList = this.TASJDEComparisonStorage["TASHistoryList"] as List<TASJDEComparisonEntity>;
            else
                this.TASHistoryList = null;

            if (this.TASJDEComparisonStorage.ContainsKey("JDEHistoryList"))
                this.JDEHistoryList = this.TASJDEComparisonStorage["JDEHistoryList"] as List<TASJDEComparisonEntity>;
            else
                this.JDEHistoryList = null;

            if (this.TASJDEComparisonStorage.ContainsKey("CurrentPDBA"))
                this.CurrentPDBA = UIHelper.ConvertObjectToString(this.TASJDEComparisonStorage["CurrentPDBA"]);
            else
                this.CurrentPDBA = string.Empty;
            #endregion

            #region Restore control values            

            #endregion

            // Refresh the grids
            RebindDataToGrid();
            RebindDataToTASHistoryGrid();
            RebindDataToJDEHistoryGrid();

            // Set the grid attributes
            //this.gridSearchResult.CurrentPageIndex = this.CurrentPageIndex;
            //this.gridSearchResult.MasterTableView.CurrentPageIndex = this.CurrentPageIndex;
            //this.gridSearchResult.MasterTableView.PageSize = this.CurrentPageSize;
            //this.gridSearchResult.MasterTableView.DataBind();
        }

        private void StoreDataToCollection(UIHelper.PagePostBackFlags formFlag)
        {
            this.TASJDEComparisonStorage.Clear();
            this.TASJDEComparisonStorage.Add("FormFlag", formFlag.ToString());

            #region Save control values to session
            
            #endregion

            #region Save Query String values to collection
            this.TASJDEComparisonStorage.Add("CallerForm", this.CallerForm);
            #endregion

            #region Store session data to collection
            this.TASJDEComparisonStorage.Add("TASJDEComparisonList", this.TASJDEComparisonList);
            this.TASJDEComparisonStorage.Add("TASHistoryList", this.TASHistoryList);
            this.TASJDEComparisonStorage.Add("JDEHistoryList", this.JDEHistoryList);
            this.TASJDEComparisonStorage.Add("CurrentPDBA", this.CurrentPDBA);
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
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            GetTASJDEComparisonReport();
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

        #region Database Access
        private void GetTASJDEComparisonReport()
        {
            try
            {
                // Initialize session
                this.TASJDEComparisonList = null;
                this.TASHistoryList = null;
                this.JDEHistoryList = null;

                string error = string.Empty;
                string innerError = string.Empty;

                DALProxy proxy = new DALProxy();
                var rawData = proxy.GetTASJDEComparisonReport(ref error, ref innerError);
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
                        this.TASJDEComparisonList.AddRange(rawData.ToList());                                                
                    }
                }

                // Bind data to the grid
                RebindDataToGrid();

                // Hide panels
                this.panTASHistory.Style[HtmlTextWriterStyle.Display] = "none";
                this.panJDEHistory.Style[HtmlTextWriterStyle.Display] = "none";
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        private void GetTASAndJDEHistory(string pdba)
        {
            try
            {
                // Initialize session variables
                this.TASHistoryList.Clear();
                this.JDEHistoryList.Clear();

                string error = string.Empty;
                string innerError = string.Empty;
                List<TASJDEComparisonEntity> tasHistoryList;
                List<TASJDEComparisonEntity> jdeHistoryList;

                DALProxy proxy = new DALProxy();
                proxy.GetTASJDETransactionHistory(pdba, out tasHistoryList, out jdeHistoryList, ref error, ref innerError);
                if (!string.IsNullOrEmpty(error) || !string.IsNullOrEmpty(innerError))
                {
                    if (!string.IsNullOrEmpty(innerError))
                        throw new Exception(error, new Exception(innerError));
                    else
                        throw new Exception(error);
                }
                else
                {
                    if (tasHistoryList != null && tasHistoryList.Count > 0)
                    {
                        this.TASHistoryList.AddRange(tasHistoryList.ToList());
                        this.btnExportToExcel.Enabled = true;
                    }

                    if (jdeHistoryList != null && jdeHistoryList.Count > 0)
                    { 
                        this.JDEHistoryList.AddRange(jdeHistoryList.ToList());
                        this.btnExportToExcel.Enabled = true;
                    }
                }

                // Bind data to the grid
                RebindDataToTASHistoryGrid();
                RebindDataToJDEHistoryGrid();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private List<EmployeeAttendanceEntity> GetTimesheetRecord(int autoID)
        {
            List<EmployeeAttendanceEntity> result = null;

            try
            {
                string error = string.Empty;
                string innerError = string.Empty;

                DALProxy proxy = new DALProxy();
                var source = proxy.GetTimesheetCorrection(string.Empty, 0, null, null, autoID, 1, 10, ref error, ref innerError);
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
                        result = new List<EmployeeAttendanceEntity>();

                        result.AddRange(source);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        protected void btnExportToExcel_Click(object sender, EventArgs e)
        {
            try
            {
                if (TASHistoryList != null && TASHistoryList.Count > 0)
                {
                    DataTable dt = UIHelper.ConvertListToDataTable(TASHistoryList);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        string fileName = string.Empty;
                        #region Set the Excel file name and path
                        // Retrieve the folder where the files will be saved
                        string exportFolder = Server.MapPath(ConfigurationManager.AppSettings["DownloadPath"]);

                        fileName = string.Format(@"{0}\TASJDEComparisonReport_{1}-{2}.xlsx",
                               exportFolder,
                               Convert.ToDateTime(DateTime.Now).ToString("ddMMMyy"),
                               Convert.ToDateTime(DateTime.Now).ToString("ddMMMyy"));
                        #endregion

                        if (!string.IsNullOrEmpty(fileName))
                        {
                            #region Remove existing files
                            if (File.Exists(fileName))
                            {
                                File.Delete(fileName);
                            }
                            #endregion

                            #region Build the Excel Sheet file using EPPlus

                            DataSet ds = new DataSet();
                            string[] fieldTAS =  { "PDBAName", "TASCount", "DiffTAS", "JDECount", "DiffJDE" , "TotalDiff",
                                                    "JPDBA","JEmpNo", "JAutoID", "Jhours","XXXXX"};
                            string[] fieldJDE =  { "PDBA", "PDBAName", "TASCount", "DiffTAS", "JDECount" , "DiffJDE",
                                                    "TotalDiff", "OTFrom","OTTo"};

                            DataTable dtTAS = UIHelper.ConvertListToDataTable(TASHistoryList).Copy();                            
                            // removing the unrequired field from the datatable 
                            foreach (string colName in fieldTAS)
                            {
                                if (dtTAS.Columns.Contains(colName))
                                    dtTAS.Columns.Remove(colName);
                            }
                            dtTAS.TableName = "TAS";
                            ds.Tables.Add(dtTAS);                          

                            DataTable dtJDE = UIHelper.ConvertListToDataTable(JDEHistoryList).Copy();
                            // removing the unrequired field from the datatable 
                            foreach (string colName in fieldJDE)
                            {
                                if (dtJDE.Columns.Contains(colName))
                                    dtJDE.Columns.Remove(colName);
                            }
                            dtJDE.TableName = "JDE";
                            ds.Tables.Add(dtJDE);

                            createExcelFile(ds, fileName);
                            #endregion
                        }
                    }
                }
                
                // Reset session variables
                this.TASHistoryList.Clear();
                this.JDEHistoryList.Clear();
                this.btnExportToExcel.Enabled = false;
                // Hide panels
                this.panTASHistory.Style[HtmlTextWriterStyle.Display] = "none";
                this.panJDEHistory.Style[HtmlTextWriterStyle.Display] = "none";
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());
            }
        }

        public Boolean createExcelFile(DataSet ds, String fileName)
        {
           
            Boolean IsDone = false;
            try
            {
                FileInfo CreatedFile = new FileInfo(fileName);
                Boolean ISNew = false;
                if (!CreatedFile.Exists)
                {
                    ISNew = true;
                }
                
                using (var pck = new ExcelPackage(CreatedFile))
                {
                    ExcelWorksheet ws;
                    foreach (DataTable Table in ds.Tables)
                    {
                        if (ISNew == true)
                        {
                            ws = pck.Workbook.Worksheets.Add(Table.TableName.ToString());

                            if (System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.IsRightToLeft)// Right to Left for Arabic lang
                            {
                                ExcelWorksheetView wv = ws.View;
                                wv.RightToLeft = true;
                                ws.PrinterSettings.Orientation = eOrientation.Landscape;
                            }
                            else
                            {
                                ExcelWorksheetView wv = ws.View;
                                wv.RightToLeft = false;
                                ws.PrinterSettings.Orientation = eOrientation.Landscape;
                            }
                            ws.Cells[1, 1].LoadFromDataTable(Table, ISNew, OfficeOpenXml.Table.TableStyles.Light8);

                            if (ws.Name == "TAS")
                            {
                                ws.Column(4).Width = 15;     //Date
                                ws.Cells["D:D"].Style.Numberformat.Format = "dd-MMM-yyyy";   //SwipeDate
                                ws.Cells["E:E"].Style.Numberformat.Format = "HH:mm";         //SwipeOut
                                ws.Cells["F:F"].Style.Numberformat.Format = "HH:mm";         //WorkHour
                            }
                            else if (ws.Name == "JDE")
                            {
                                ws.Column(3).Width = 15;     //Date
                                ws.Cells["C:C"].Style.Numberformat.Format = "dd-MMM-yyyy";   //SwipeDate
                                ws.Cells["H:H"].Style.Numberformat.Format = "HH:mm";         //SwipeOut
                            }
                        }
                        else
                        {
                            if (Table.TableName.ToString() == "TAS")
                                ws = pck.Workbook.Worksheets["TAS"];
                            else
                                ws = pck.Workbook.Worksheets["JDE"];
                            if (ws.Name == "TAS")
                            {
                                ws.Cells[2, 1].LoadFromDataTable(Table, ISNew);
                                ws.Column(4).Width = 15;     //Date
                                ws.Cells["D:D"].Style.Numberformat.Format = "dd-MMM-yyyy";   //SwipeDate
                                ws.Cells["E:E"].Style.Numberformat.Format = "HH:mm";         //SwipeOut
                                ws.Cells["F:F"].Style.Numberformat.Format = "HH:mm";         //WorkHour
                            }
                            else if (ws.Name == "JDE")
                            {                               
                                ws.Cells[2, 1].LoadFromDataTable(Table, ISNew);
                                ws.Column(3).Width = 15;     //Date
                                ws.Cells["C:C"].Style.Numberformat.Format = "dd-MMM-yyyy";   //SwipeDate
                                ws.Cells["H:H"].Style.Numberformat.Format = "HH:mm";         //SwipeOut
                            }
                        }
                    }

                    // Save the output file
                    pck.SaveAs(CreatedFile);

                    // Open the excel sheet file
                    string urlPath = string.Format("{0}?filename={1}&fileType={2}",
                        UIHelper.PAGE_FILE_HANDLER.Replace("~", string.Empty),
                        CreatedFile.Name,
                        UIHelper.CONST_EXCEL_FILE_TYPE);

                    string script = string.Format("DisplayAttachment('{0}');", urlPath);
                    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "ShowAttachment", script.ToString(), true);
                    
                    IsDone = true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return IsDone;
        }       
    }
}