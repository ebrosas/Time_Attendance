using GARMCO.AMS.GAP.Utility;
using GARMCO.AMS.TAS.BL.Entities;
using GARMCO.AMS.TAS.UI.Helpers;
using GARMCO.AMS.TAS.UI.Views.Shared;
using GARMCO.Common.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

namespace GARMCO.AMS.TAS.UI.Views.UserFunctions
{
    public partial class OvertimeApprovalHistory : BaseWebForm, IFormExtension
    {
        #region Enumeration
        private enum ValidationErrorType
        {
            NoError,
            CustomFormError,
            NoEmployeeNo
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

        private Dictionary<string, object> ApprovalHistoryStorage
        {
            get
            {
                Dictionary<string, object> list = Session["ApprovalHistoryStorage"] as Dictionary<string, object>;
                if (list == null)
                    Session["ApprovalHistoryStorage"] = list = new Dictionary<string, object>();

                return list;
            }
            set
            {
                Session["ApprovalHistoryStorage"] = value;
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

        private List<RoutineHistoryEntity> RoutineHistoryList
        {
            get
            {
                List<RoutineHistoryEntity> list = ViewState["RoutineHistoryList"] as List<RoutineHistoryEntity>;
                if (list == null)
                    ViewState["RoutineHistoryList"] = list = new List<RoutineHistoryEntity>();

                return list;
            }
            set
            {
                ViewState["RoutineHistoryList"] = value;
            }
        }

        private List<ApprovalEntity> ApprovalList
        {
            get
            {
                List<ApprovalEntity> list = ViewState["ApprovalList"] as List<ApprovalEntity>;
                if (list == null)
                    ViewState["ApprovalList"] = list = new List<ApprovalEntity>();

                return list;
            }
            set
            {
                ViewState["ApprovalList"] = value;
            }
        }

        private List<WFTransActivityEntity> WorkflowActivityList
        {
            get
            {
                List<WFTransActivityEntity> list = ViewState["WorkflowActivityList"] as List<WFTransActivityEntity>;
                if (list == null)
                    ViewState["WorkflowActivityList"] = list = new List<WFTransActivityEntity>();

                return list;
            }
            set
            {
                ViewState["WorkflowActivityList"] = value;
            }
        }

        private bool IsValidEmployee
        {
            get
            {
                return UIHelper.ConvertObjectToBolean(ViewState["IsValidEmployee"]);
            }
            set
            {
                ViewState["IsValidEmployee"] = value;
            }
        }

        private EmployeeAttendanceEntity CurrentOvertimeRequest
        {
            get
            {
                EmployeeAttendanceEntity swipeData = Session["CurrentOvertimeRequest"] as EmployeeAttendanceEntity;
                return swipeData;
            }
        }

        private bool IsLoadRequest
        {
            get
            {
                return UIHelper.ConvertObjectToBolean(ViewState["IsLoadRequest"]);
            }
            set
            {
                ViewState["IsLoadRequest"] = value;
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

                this.Master.SetPageForm(UIHelper.FormAccessCodes.OTWFHISTRY.ToString());

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

                if (!string.IsNullOrEmpty(position))
                {
                    sb.Append(string.Format("Position: {0} <br />", position));
                }
                if (!string.IsNullOrEmpty(costCenter))
                {
                    sb.Append(string.Format("Cost Center: {0} <br />", costCenter));
                }

                this.Master.LogOnUser = string.Concat("Welcome, ",
                    UIHelper.GetUserFirstName(UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_FULLNAME]),
                    UIHelper.ConvertObjectToBolean(Session[UIHelper.GARMCO_USER_IS_SPECIAL])));
                this.Master.LogOnUserInfo = sb.ToString().Trim();
                this.Master.FormTitle = UIHelper.PAGE_OVERTIME_APPROVAL_HISTORY_TITLE;
                #endregion

                #region Check if user has permission to access the page
                if (!GAPFunction.CheckFormAccess(this.Master.FormAccess, GAPConstants.FormAccessIndex.Retrieve))
                {
                    if (!UIHelper.ConvertObjectToBolean(Session[UIHelper.GARMCO_USER_IS_SPECIAL]))
                    {
                        Response.Redirect(String.Format("{0}?error={1}&pageName={2}", UIHelper.PAGE_ERROR, Convert.ToInt32(UIHelper.PageErrorCodes.NoAccessOnPage), UIHelper.PAGE_OVERTIME_APPROVAL_HISTORY_TITLE), true);
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
                this.Master.DefaultButton = this.btnBack.UniqueID;
                #endregion

                #region Restore saved data from local storage
                string formFlag = string.Empty;
                if (this.ApprovalHistoryStorage.Count > 0)
                {
                    if (this.ApprovalHistoryStorage.ContainsKey("FormFlag"))
                        formFlag = UIHelper.ConvertObjectToString(this.ApprovalHistoryStorage["FormFlag"]);
                }
                #endregion

                ClearForm();
                ProcessQueryString();
                FillComboData();
                LoadOvertimeRequest();
            }

            ScriptManager.RegisterStartupScript(this, this.GetType(), "HideScrollbar", "HideScrollbar();", true);
            AddControlsAttribute();
        }
        #endregion

        #region Grid Events and Methods

        #region Routine History Grid
        protected void gridHistory_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item.ItemType == GridItemType.AlternatingItem || e.Item.ItemType == GridItemType.Item)
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {

                    #region Format Display
                    // Last Modified By
                    //if (item["HistCreatedName"].Text.Replace("&nbsp;", String.Empty).Length > 0)
                    //{
                    //    if (UIHelper.ConvertObjectToString(item["HistCreatedName"].Text).ToLower() == "system")
                    //        item["HistCreatedName"].Text = "System";
                    //    else
                    //        item["HistCreatedName"].Text = String.Format("({0}) {1}", item["HistCreatedBy"].Text, item["HistCreatedName"].Text);
                    //}
                    #endregion
                }
            }
        }

        protected void gridHistory_PageIndexChanged(object sender, GridPageChangedEventArgs e)
        {
            RebindRoutineHistoryGrid();
        }

        protected void gridHistory_PageSizeChanged(object sender, GridPageSizeChangedEventArgs e)
        {
            RebindRoutineHistoryGrid();
        }

        protected void gridHistory_SortCommand(object sender, GridSortCommandEventArgs e)
        {
            if (this.RoutineHistoryList.Count > 0)
            {
                this.gridHistory.DataSource = this.RoutineHistoryList;
                this.gridHistory.DataBind();

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
                        sortExpr.SortOrder = this.gridHistory.MasterTableView.AllowNaturalSort ? GridSortOrder.None : GridSortOrder.Descending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;

                    case GridSortOrder.Descending:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = GridSortOrder.Ascending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;
                }

                e.Canceled = true;
                this.gridHistory.Rebind();
            }
            else
                InitializeRoutineHistoryGrid();
        }

        private void RebindRoutineHistoryGrid()
        {
            if (this.RoutineHistoryList.Count > 0)
            {
                this.gridHistory.DataSource = this.RoutineHistoryList;
                this.gridHistory.DataBind();
            }
            else
                InitializeRoutineHistoryGrid();
        }

        private void InitializeRoutineHistoryGrid()
        {
            this.gridHistory.DataSource = new List<RoutineHistoryEntity>();
            this.gridHistory.DataBind();
        }
        #endregion

        #region Approval Grid
        protected void gridApproval_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item.ItemType == GridItemType.AlternatingItem || e.Item.ItemType == GridItemType.Item)
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {
                    bool isApproved = UIHelper.ConvertObjectToBolean(item["AppApproved"].Text);

                    #region Format Display
                    //// Last Modified By
                    //if (item["AppCreatedName"].Text.Replace("&nbsp;", String.Empty).Length > 0)
                    //    item["AppCreatedName"].Text = String.Format("({0}) {1}", item["AppCreatedBy"].Text, item["AppCreatedName"].Text);

                    // Approved
                    item["AppApproved"].Text = isApproved ? "Yes" : "No";     //Convert.ToBoolean(item["AppApproved"].Text) ? "Yes" : "No";
                    #endregion

                   
                    if (!isApproved)
                    {
                        item.BackColor = System.Drawing.Color.Red;
                        item.Font.Bold = true;
                        item.ForeColor = System.Drawing.Color.Yellow;
                    }
                }
            }
        }

        protected void gridApproval_PageIndexChanged(object sender, GridPageChangedEventArgs e)
        {
            RebindApprovalGrid();
        }

        protected void gridApproval_PageSizeChanged(object sender, GridPageSizeChangedEventArgs e)
        {
            RebindApprovalGrid();
        }

        protected void gridApproval_SortCommand(object sender, GridSortCommandEventArgs e)
        {
            if (this.ApprovalList.Count > 0)
            {
                this.gridApproval.DataSource = this.ApprovalList;
                this.gridApproval.DataBind();

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
                        sortExpr.SortOrder = this.gridApproval.MasterTableView.AllowNaturalSort ? GridSortOrder.None : GridSortOrder.Descending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;

                    case GridSortOrder.Descending:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = GridSortOrder.Ascending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;
                }

                e.Canceled = true;
                this.gridApproval.Rebind();
            }
            else
                InitializeApprovalGrid();
        }

        private void RebindApprovalGrid()
        {
            if (this.ApprovalList.Count > 0)
            {
                this.gridApproval.DataSource = this.ApprovalList;
                this.gridApproval.DataBind();
            }
            else
                InitializeApprovalGrid();
        }

        private void InitializeApprovalGrid()
        {
            this.gridApproval.DataSource = new List<ApprovalEntity>();
            this.gridApproval.DataBind();
        }
        #endregion

        #region Workflow History Grid
        protected void gridWorkflow_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item.ItemType == GridItemType.AlternatingItem || e.Item.ItemType == GridItemType.Item)
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {
                    int statusID = UIHelper.ConvertObjectToInt(item["ActStatusID"].Text);
                    if (statusID == 107)
                    {
                        item.BackColor = System.Drawing.Color.LightGreen;
                        item.Font.Bold = true;
                        item.ForeColor = System.Drawing.Color.Red;
                    }
                }
            }
        }

        protected void gridWorkflow_PageIndexChanged(object sender, GridPageChangedEventArgs e)
        {
            RebindWorkflowActivityGrid();
        }

        protected void gridWorkflow_PageSizeChanged(object sender, GridPageSizeChangedEventArgs e)
        {
            RebindWorkflowActivityGrid();
        }

        protected void gridWorkflow_SortCommand(object sender, GridSortCommandEventArgs e)
        {
            if (this.WorkflowActivityList.Count > 0)
            {
                this.gridWorkflow.DataSource = this.WorkflowActivityList;
                this.gridWorkflow.DataBind();

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
                        sortExpr.SortOrder = this.gridWorkflow.MasterTableView.AllowNaturalSort ? GridSortOrder.None : GridSortOrder.Descending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;

                    case GridSortOrder.Descending:
                        sortExpr.FieldName = e.SortExpression;
                        sortExpr.SortOrder = GridSortOrder.Ascending;
                        e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                        break;
                }

                e.Canceled = true;
                this.gridWorkflow.Rebind();
            }
            else
                InitializeWorkflowActivityGrid();
        }

        private void RebindWorkflowActivityGrid()
        {
            if (this.WorkflowActivityList.Count > 0)
            {
                this.gridWorkflow.DataSource = this.WorkflowActivityList;
                this.gridWorkflow.DataBind();
            }
            else
                InitializeWorkflowActivityGrid();
        }

        private void InitializeWorkflowActivityGrid()
        {
            this.gridWorkflow.DataSource = new List<WFTransActivityEntity>();
            this.gridWorkflow.DataBind();
        }
        #endregion

        #endregion

        #region Action Buttons
        protected void btnGet_Click(object sender, EventArgs e)
        {
            try
            {
                #region Perform data validation
                // Check Employee No.
                if (UIHelper.ConvertObjectToInt(this.txtEmpNo.Text) == 0)
                {
                    this.txtGeneric.Text = ValidationErrorType.NoEmployeeNo.ToString();
                    this.ErrorType = ValidationErrorType.NoEmployeeNo;
                    this.cusValEmpNo.Validate();
                    return;
                }
                #endregion

                #region Initialize control values and variables
                this.litEmpName.Text = "Not defined";
                this.litPosition.Text = "Not defined";
                this.litPayGrade.Text = "Not defined";
                this.litCostCenter.Text = "Not defined";
                this.litShiftPatCode.Text = "Not defined";
                this.litShiftCode.Text = "Not defined";

                this.IsValidEmployee = false;
                #endregion

                int empNo = UIHelper.ConvertObjectToInt(this.txtEmpNo.Text);
                if (empNo.ToString().Length == 4)
                {
                    empNo += 10000000;
                }

                string error = string.Empty;
                string innerError = string.Empty;

                EmployeeInfo empInfo = UIHelper.GetEmployeeInfo(empNo);
                if (empInfo != null)
                {
                    if (UIHelper.ConvertObjectToInt(empInfo.EmployeeNo) > 0)
                    {
                        #region Get employee info from the webservice
                        this.litEmpName.Text = UIHelper.ConvertObjectToString(empInfo.FullName);
                        this.litPosition.Text = UIHelper.ConvertObjectToString(empInfo.PositionDesc);
                        this.litPayGrade.Text = empInfo.PayGrade.ToString();
                        this.litCostCenter.Text = string.Format("{0} - {1}", empInfo.CostCenter, UIHelper.ConvertObjectToString(empInfo.CostCenterName));
                        #endregion
                    }
                    else
                    {
                        #region Get employee info from the employee master
                        //if (WCFProxy != null)
                        //{
                        //    var rawData = WCFProxy.GetEmployeeDetail(empNo, ref error, ref innerError);
                        //    if (rawData != null)
                        //    {
                        //        this.litEmpName.Text = UIHelper.ConvertObjectToString(rawData.EmpName);
                        //        this.litPosition.Text = UIHelper.ConvertObjectToString(rawData.Position);
                        //        this.litPayGrade.Text = UIHelper.ConvertObjectToString(rawData.PayGrade);
                        //        this.litCostCenter.Text = string.Format("{0} - {1}", rawData.CostCenter, UIHelper.ConvertObjectToString(rawData.CostCenterName));
                        //        this.litSupervisor.Text = rawData.SupervisorFullName;
                        //        this.litCCManager.Text = rawData.ManagerFullName;
                        //    }
                        //}
                        #endregion
                    }

                    // Display Emp. No.
                    this.txtEmpNo.Text = empNo.ToString();

                    // Set session variable
                    this.IsValidEmployee = empNo > 0;
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex);
            }
        }

        protected void btnFindIssuer_Click(object sender, EventArgs e)
        {
            StoreDataToCollection(UIHelper.PagePostBackFlags.GetEmployeeInfo);

            Response.Redirect
            (
                String.Format(UIHelper.PAGE_EMPLOYEE_SEARCH + "?{0}={1}",
                UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                UIHelper.PAGE_OVERTIME_APPROVAL_HISTORY
            ),
            false);
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.CallerForm))
            {
                Response.Redirect
                (
                    String.Format(this.CallerForm + "?IsLoadRequest={0}",
                    this.IsLoadRequest.ToString()
                ),
                false);
            }
            else
                Response.Redirect(UIHelper.PAGE_OVERTIME_ENTRY, false);
        }

        protected void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadOvertimeRequest();
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

        protected void tabMain_TabClick(object sender, RadTabStripEventArgs e)
        {

        }
        #endregion

        #region Private Methods
        private void RestoreDataFromCollection()
        {
            if (this.ApprovalHistoryStorage.Count == 0)
                return;

            #region Restore session values
            if (this.ApprovalHistoryStorage.ContainsKey("RoutineHistoryList"))
                this.RoutineHistoryList = this.ApprovalHistoryStorage["RoutineHistoryList"] as List<RoutineHistoryEntity>;
            else
                this.RoutineHistoryList = null;

            if (this.ApprovalHistoryStorage.ContainsKey("ApprovalList"))
                this.ApprovalList = this.ApprovalHistoryStorage["ApprovalList"] as List<ApprovalEntity>;
            else
                this.ApprovalList = null;

            if (this.ApprovalHistoryStorage.ContainsKey("WorkflowActivityList"))
                this.WorkflowActivityList = this.ApprovalHistoryStorage["WorkflowActivityList"] as List<WFTransActivityEntity>;
            else
                this.WorkflowActivityList = null;

            if (this.ApprovalHistoryStorage.ContainsKey("IsValidEmployee"))
                this.IsValidEmployee = UIHelper.ConvertObjectToBolean(this.ApprovalHistoryStorage["IsValidEmployee"]);
            else
                this.IsValidEmployee = false;
            #endregion

            #region Details controls
            if (this.ApprovalHistoryStorage.ContainsKey("txtEmpNo"))
                this.txtEmpNo.Text = UIHelper.ConvertObjectToString(this.ApprovalHistoryStorage["txtEmpNo"]);
            else
                this.txtEmpNo.Text = string.Empty;

            if (this.ApprovalHistoryStorage.ContainsKey("litEmpName"))
                this.litEmpName.Text = UIHelper.ConvertObjectToString(this.ApprovalHistoryStorage["litEmpName"]);
            else
                this.litEmpName.Text = string.Empty;

            if (this.ApprovalHistoryStorage.ContainsKey("litPosition"))
                this.litPosition.Text = UIHelper.ConvertObjectToString(this.ApprovalHistoryStorage["litPosition"]);
            else
                this.litPosition.Text = string.Empty;

            if (this.ApprovalHistoryStorage.ContainsKey("litPayGrade"))
                this.litPayGrade.Text = UIHelper.ConvertObjectToString(this.ApprovalHistoryStorage["litPayGrade"]);
            else
                this.litPayGrade.Text = string.Empty;

            if (this.ApprovalHistoryStorage.ContainsKey("litCostCenter"))
                this.litCostCenter.Text = UIHelper.ConvertObjectToString(this.ApprovalHistoryStorage["litCostCenter"]);
            else
                this.litCostCenter.Text = string.Empty;                        

            if (this.ApprovalHistoryStorage.ContainsKey("litShiftPatCode"))
                this.litShiftPatCode.Text = UIHelper.ConvertObjectToString(this.ApprovalHistoryStorage["litShiftPatCode"]);
            else
                this.litShiftPatCode.Text = string.Empty;

            if (this.ApprovalHistoryStorage.ContainsKey("litShiftCode"))
                this.litShiftCode.Text = UIHelper.ConvertObjectToString(this.ApprovalHistoryStorage["litShiftCode"]);
            else
                this.litShiftCode.Text = string.Empty;

            if (this.ApprovalHistoryStorage.ContainsKey("litRequisitionNo"))
                this.litRequisitionNo.Text = UIHelper.ConvertObjectToString(this.ApprovalHistoryStorage["litRequisitionNo"]);
            else
                this.litRequisitionNo.Text = string.Empty;

            if (this.ApprovalHistoryStorage.ContainsKey("litSubmittedDate"))
                this.litSubmittedDate.Text = UIHelper.ConvertObjectToString(this.ApprovalHistoryStorage["litSubmittedDate"]);
            else
                this.litSubmittedDate.Text = string.Empty;

            if (this.ApprovalHistoryStorage.ContainsKey("litDate"))
                this.litDate.Text = UIHelper.ConvertObjectToString(this.ApprovalHistoryStorage["litDate"]);
            else
                this.litDate.Text = string.Empty;

            if (this.ApprovalHistoryStorage.ContainsKey("litOTStartTime"))
                this.litOTStartTime.Text = UIHelper.ConvertObjectToString(this.ApprovalHistoryStorage["litOTStartTime"]);
            else
                this.litOTStartTime.Text = string.Empty;

            if (this.ApprovalHistoryStorage.ContainsKey("litOTEndTime"))
                this.litOTEndTime.Text = UIHelper.ConvertObjectToString(this.ApprovalHistoryStorage["litOTEndTime"]);
            else
                this.litOTEndTime.Text = string.Empty;

            if (this.ApprovalHistoryStorage.ContainsKey("litOTDuration"))
                this.litOTDuration.Text = UIHelper.ConvertObjectToString(this.ApprovalHistoryStorage["litOTDuration"]);
            else
                this.litOTDuration.Text = string.Empty;

            if (this.ApprovalHistoryStorage.ContainsKey("litStatus"))
                this.litStatus.Text = UIHelper.ConvertObjectToString(this.ApprovalHistoryStorage["litStatus"]);
            else
                this.litStatus.Text = string.Empty;
            #endregion

            // Refresh the grid
            RebindRoutineHistoryGrid();
            RebindApprovalGrid();
            RebindWorkflowActivityGrid();
        }

        private void StoreDataToCollection(UIHelper.PagePostBackFlags formFlag)
        {
            this.ApprovalHistoryStorage.Clear();
            this.ApprovalHistoryStorage.Add("FormFlag", formFlag.ToString());

            #region Details controls
            this.ApprovalHistoryStorage.Add("txtEmpNo", this.txtEmpNo.Text);
            this.ApprovalHistoryStorage.Add("litEmpName", this.litEmpName.Text.Trim());
            this.ApprovalHistoryStorage.Add("litPosition", this.litPosition.Text.Trim());
            this.ApprovalHistoryStorage.Add("litPayGrade", this.litPayGrade.Text.Trim());
            this.ApprovalHistoryStorage.Add("litCostCenter", this.litCostCenter.Text.Trim());            
            this.ApprovalHistoryStorage.Add("litShiftPatCode", this.litShiftPatCode.Text.Trim());
            this.ApprovalHistoryStorage.Add("litShiftCode", this.litShiftCode.Text.Trim());
            this.ApprovalHistoryStorage.Add("litRequisitionNo", this.litRequisitionNo.Text.Trim());
            this.ApprovalHistoryStorage.Add("litSubmittedDate", this.litSubmittedDate.Text.Trim());
            this.ApprovalHistoryStorage.Add("litDate", this.litDate.Text.Trim());
            this.ApprovalHistoryStorage.Add("litOTStartTime", this.litOTStartTime.Text.Trim());
            this.ApprovalHistoryStorage.Add("litOTEndTime", this.litOTEndTime.Text.Trim());
            this.ApprovalHistoryStorage.Add("litOTDuration", this.litOTDuration.Text.Trim());
            this.ApprovalHistoryStorage.Add("litStatus", this.litStatus.Text.Trim());
            #endregion

            #region Store session data to collection
            this.ApprovalHistoryStorage.Add("CallerForm", this.CallerForm);
            this.ApprovalHistoryStorage.Add("RoutineHistoryList", this.RoutineHistoryList);
            this.ApprovalHistoryStorage.Add("ApprovalList", this.ApprovalList);
            this.ApprovalHistoryStorage.Add("WorkflowActivityList", this.WorkflowActivityList);
            this.ApprovalHistoryStorage.Add("IsValidEmployee", this.IsValidEmployee);
            #endregion
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

        private void LoadOvertimeRequest()
        {
            if (this.CurrentOvertimeRequest == null)
                return;

            try
            {
                #region Get details of the request
                this.txtEmpNo.Text = this.CurrentOvertimeRequest.EmpNo.ToString();
                this.litEmpName.Text = this.CurrentOvertimeRequest.EmpName;
                this.litPosition.Text = this.CurrentOvertimeRequest.Position;
                this.litPayGrade.Text = UIHelper.ConvertObjectToString(this.CurrentOvertimeRequest.GradeCode);
                this.litCostCenter.Text = this.CurrentOvertimeRequest.CostCenterFullName;
                this.litShiftPatCode.Text = this.CurrentOvertimeRequest.ShiftPatCode;
                this.litShiftCode.Text = string.Format("{0} / {1}",
                    this.CurrentOvertimeRequest.ShiftCode,
                    this.CurrentOvertimeRequest.ActualShiftCode);

                this.litRequisitionNo.Text = this.CurrentOvertimeRequest.OTRequestNo.ToString();
                this.litSubmittedDate.Text = this.CurrentOvertimeRequest.RequestSubmissionDate.HasValue
                    ? Convert.ToDateTime(this.CurrentOvertimeRequest.RequestSubmissionDate).ToString("dd-MMM-yyyy")
                    : "Not defined";
                this.litDate.Text = this.CurrentOvertimeRequest.DT.HasValue
                    ? Convert.ToDateTime(this.CurrentOvertimeRequest.DT).ToString("dd-MMM-yyyy")
                    : "Not defined";

                this.litOTStartTime.Text = this.CurrentOvertimeRequest.OTStartTime.HasValue
                   ? Convert.ToDateTime(this.CurrentOvertimeRequest.OTStartTime).ToString("HH:mm:ss")
                   : "Not defined";
                //this.litOTStartTime.Text = this.CurrentOvertimeRequest.OTApprovalCode == "Y"
                //    ? Convert.ToDateTime(this.CurrentOvertimeRequest.OTStartTime).ToString("HH:mm:ss")
                //    : @"N/A";

                this.litOTEndTime.Text = this.CurrentOvertimeRequest.OTEndTime.HasValue
                    ? Convert.ToDateTime(this.CurrentOvertimeRequest.OTEndTime).ToString("HH:mm:ss")
                    : "Not defined";
                //this.litOTEndTime.Text = this.CurrentOvertimeRequest.OTApprovalCode == "Y"
                //    ? Convert.ToDateTime(this.CurrentOvertimeRequest.OTEndTime).ToString("HH:mm:ss")
                //    : @"N/A";

                this.litOTDuration.Text = this.CurrentOvertimeRequest.OTApprovalCode == "Y"
                    ? this.CurrentOvertimeRequest.OTDurationText
                    : "0";
                this.litOTApproved.Text = this.CurrentOvertimeRequest.OTApprovalDesc;
                this.litMealVoucherApproved.Text = this.CurrentOvertimeRequest.MealVoucherEligibility;

                //this.litStatus.Text = !string.IsNullOrEmpty(this.CurrentOvertimeRequest.CurrentlyAssignedFullName)
                //    ? string.Concat(this.CurrentOvertimeRequest.StatusDesc, " - ", this.CurrentOvertimeRequest.CurrentlyAssignedFullName)
                //    : this.CurrentOvertimeRequest.StatusDesc;
                this.litStatus.Text = string.Format("{0} - {1}",
                    this.CurrentOvertimeRequest.StatusHandlingCode,
                    this.CurrentOvertimeRequest.StatusDesc);
                #endregion

                FillDataToRoutineHistoryGrid(this.CurrentOvertimeRequest.OTRequestNo, this.CurrentOvertimeRequest.AutoID, this.CurrentOvertimeRequest.SubmittedDate);
                FillDataToApprovalGrid(this.CurrentOvertimeRequest.OTRequestNo, this.CurrentOvertimeRequest.AutoID, this.CurrentOvertimeRequest.SubmittedDate);
                FillDataToWorkflowGrid(this.CurrentOvertimeRequest.OTRequestNo, this.CurrentOvertimeRequest.AutoID, this.CurrentOvertimeRequest.SubmittedDate);
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
            #region Reset Details section           
            this.txtEmpNo.Text = string.Empty;
            this.litEmpName.Text = "Not defined";
            this.litPosition.Text = "Not defined";
            this.litPayGrade.Text = "Not defined";
            this.litCostCenter.Text = "Not defined";
            this.litShiftPatCode.Text = "Not defined";
            this.litShiftCode.Text = "Not defined";
            this.litRequisitionNo.Text = "Not defined";
            this.litSubmittedDate.Text = "Not defined";
            this.litDate.Text = "Not defined";
            this.litOTStartTime.Text = "Not defined";
            this.litOTEndTime.Text = "Not defined";
            this.litOTDuration.Text = "Not defined";
            this.litStatus.Text = "Not defined";
            this.litOTApproved.Text = "Not defined";
            this.litMealVoucherApproved.Text = "Not defined";
            #endregion

            // Clear collections
            this.RoutineHistoryList.Clear();
            this.ApprovalList.Clear();
            this.WorkflowActivityList.Clear();

            KillSessions();

            InitializeRoutineHistoryGrid();
            InitializeApprovalGrid();
            InitializeWorkflowActivityGrid();
        }

        public void AddControlsAttribute()
        {

        }

        public void ProcessQueryString()
        {
            this.CallerForm = UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_CALLER_FORM_KEY]);
            this.IsLoadRequest = UIHelper.ConvertObjectToBolean(Request.QueryString["IsLoadRequest"]);
        }

        public void KillSessions()
        {
            ViewState["CurrentStartRowIndex"] = null;
            ViewState["CurrentMaximumRows"] = null;
            ViewState["CustomErrorMsg"] = null;
            ViewState["IsValidEmployee"] = null;
        }
        #endregion

        #region Database Access       
        private void FillDataToRoutineHistoryGrid(long otRequestNo, int tsAutoID, DateTime? reqSubmissionDate, bool reloadFromDB = true)
        {           
            try
            {
                List<RoutineHistoryEntity> gridSource = new List<RoutineHistoryEntity>();
                if (this.RoutineHistoryList.Count > 0 && !reloadFromDB)
                {
                    gridSource = this.RoutineHistoryList;
                }
                else
                {
                    DALProxy proxy = new DALProxy();
                    string error = string.Empty;
                    string innerError = string.Empty;

                    var rawData = proxy.GetRoutineHistory(otRequestNo, tsAutoID, reqSubmissionDate, ref error, ref innerError);
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
                            gridSource.AddRange(rawData.OrderByDescending(a => a.AutoID).ToList());
                        }
                    }
                }

                // Save to session
                this.RoutineHistoryList = gridSource;

                // Bind data to the grid
                RebindRoutineHistoryGrid();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void FillDataToApprovalGrid(long otRequestNo, int tsAutoID, DateTime? reqSubmissionDate, bool reloadFromDB = true)
        {
            try
            {
                List<ApprovalEntity> gridSource = new List<ApprovalEntity>();
                if (this.ApprovalList.Count > 0 && !reloadFromDB)
                {
                    gridSource = this.ApprovalList;
                }
                else
                {
                    DALProxy proxy = new DALProxy();
                    string error = string.Empty;
                    string innerError = string.Empty;

                    var rawData = proxy.GetApprovalHistory(otRequestNo, tsAutoID, reqSubmissionDate, ref error, ref innerError);
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
                            gridSource.AddRange(rawData.OrderByDescending(a => a.AutoID).ToList());
                        }
                    }
                }

                // Save to session
                this.ApprovalList = gridSource;

                // Bind data to the grid
                RebindApprovalGrid();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void FillDataToWorkflowGrid(long otRequestNo, int tsAutoID, DateTime? reqSubmissionDate, bool reloadFromDB = true)
        {
            try
            {
                List<WFTransActivityEntity> gridSource = new List<WFTransActivityEntity>();
                if (this.WorkflowActivityList.Count > 0 && !reloadFromDB)
                {
                    gridSource = this.WorkflowActivityList;
                }
                else
                {
                    DALProxy proxy = new DALProxy();
                    string error = string.Empty;
                    string innerError = string.Empty;

                    var rawData = proxy.GetWorkflowHistory(otRequestNo, tsAutoID, reqSubmissionDate, ref error, ref innerError);
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
                            gridSource.AddRange(rawData.OrderBy(a => a.SequenceNo).ToList());
                        }
                    }
                }

                // Save to session
                this.WorkflowActivityList = gridSource;

                // Bind data to the grid
                RebindWorkflowActivityGrid();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion                
    }
}