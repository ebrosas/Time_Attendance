using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using GARMCO.AMS.TAS.UI.Helpers;
using GARMCO.Common.DAL.Employee;
using Telerik.Web.UI;

namespace GARMCO.AMS.TAS.UI.Views.Shared
{
    public partial class EmployeeLookup : BaseWebForm
    {
        #region Enumeration
        private enum ValidationErrorType
        {
            NoError,
            CustomFormError
        }
        #endregion

        #region Properties
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

        private bool IsFetchEmployeeMaster
        {
            get
            {
                return UIHelper.ConvertObjectToBolean(ViewState["IsFetchEmployeeMaster"]);
            }
            set
            {
                ViewState["IsFetchEmployeeMaster"] = value;
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
        #endregion

        #region Page Events
        protected void Page_Init(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                FillCostCenterCombo();
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
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
                this.Master.FormTitle = UIHelper.PAGE_EMPLOYEE_LOOKUP_TITLE;
                #endregion

                #region Set default button
                this.Master.DefaultButton = this.btnSearch.UniqueID;
                #endregion

                // Set the focus to the initial object
                this.txtEmpNo.Focus();

                // Get the query string
                this.CallerForm = UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_CALLER_FORM_KEY]);
                this.IsFetchEmployeeMaster = UIHelper.ConvertObjectToBolean(Request.QueryString[UIHelper.CONST_FETCH_EMPLOYEE_MASTER]);

                #region Store the ajax id and other default values
                this.hidAjaxID.Value = Request.QueryString["ajaxID"];
                this.hidControlID.Value = Request.QueryString["controlID"];
                this.hidControlContent.Value = Server.HtmlDecode(Request.QueryString["controlContent"]);
                #endregion
            }
        }
        #endregion

        #region Page Controls Events
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

        #region Private Methods
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
                this.cmbCostCenter.DataTextField = "CostCenter";
                this.cmbCostCenter.DataValueField = "CostCenter";
                this.cmbCostCenter.DataSource = filteredDT;
                this.cmbCostCenter.DataBind();
            }
        }

        private void FillCostCenterComboOld()
        {
            bool enableFilterByCostCenter = false;

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

            if (this.Master.AllowedCostCenterList.Count > 0 && 
                enableFilterByCostCenter)
            {
                #region Filter list based on allowed cost center
                foreach (string filter in this.Master.AllowedCostCenterList)
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
            else if (this.Master.AllowedCostCenterList.Count == 0 && 
                UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USER_COST_CENTER]) != string.Empty &&
                enableFilterByCostCenter)
            {
                #region Filter list based on user's cost center
                this.Master.AllowedCostCenterList.Add(UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USER_COST_CENTER]));

                foreach (string filter in this.Master.AllowedCostCenterList)
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
                #endregion
            }

            if (filteredDT.Rows.Count > 0)
            {
                this.cmbCostCenter.DataTextField = "CostCenter";
                this.cmbCostCenter.DataValueField = "CostCenter";
                this.cmbCostCenter.DataSource = filteredDT;
                this.cmbCostCenter.DataBind();
            }
        }

        protected void cmbCostCenter_ItemsRequested(object sender, RadComboBoxItemsRequestedEventArgs e)
        {
            if (cmbCostCenter.DataSource != null)
                return;

            DataView dv = this.objCostCenter.Select() as DataView;
            if (dv == null || dv.Count == 0)
                return;

            DataRow[] source = new DataRow[dv.Count];
            dv.Table.Rows.CopyTo(source, 0);
            EmployeeDAL.CostCenterDataTable filteredDT = new EmployeeDAL.CostCenterDataTable();

            if (this.Master.AllowedCostCenterList.Count > 0)
            {
                foreach (string filter in this.Master.AllowedCostCenterList)
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
                        filteredDT.Rows.Add(row);
                    }
                }
            }
            else
            {
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
            }

            if (filteredDT.Rows.Count > 0)
            {
                cmbCostCenter.DataTextField = "CostCenter";
                cmbCostCenter.DataValueField = "CostCenter";
                cmbCostCenter.DataSource = filteredDT;
                cmbCostCenter.DataBind();
            }
        }

        private void DisplayFormLevelError(string errorMsg)
        {
            if (string.IsNullOrEmpty(errorMsg))
                return;

            this.CustomErrorMsg = errorMsg;
            this.txtGeneric.Text = ValidationErrorType.CustomFormError.ToString();
            this.ErrorType = ValidationErrorType.CustomFormError;
            this.cusValEmpNo.Validate();
        }
        #endregion

        #region Action Buttons
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            if (this.cmbCostCenter.Text == string.Empty && this.txtEmpNo.Text == string.Empty && this.txtEmpName.Text == string.Empty)
                return;

            #region Get the Employee No.
            int empNo = UIHelper.ConvertObjectToInt(this.txtEmpNo.Text);
            if (empNo > 0)
            {
                if (empNo.ToString().Length == 4)
                    empNo += 10000000;

                this.txtEmpNo.Text = empNo.ToString();
            }
            #endregion

            #region Set the search criteria
            // Reset the page index
            this.gvList.CurrentPageIndex = 0;

            this.objEmployee.SelectParameters["costCenter"].DefaultValue = this.cmbCostCenter.Text;
            this.objEmployee.SelectParameters["empNo"].DefaultValue = this.txtEmpNo.Text.Trim();
            this.objEmployee.SelectParameters["empName"].DefaultValue = this.txtEmpName.Text.Trim();

            this.objEmployee.Select();
            #endregion
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            // Reset the criteria
            this.cmbCostCenter.Text = String.Empty;
            this.txtEmpNo.Text = String.Empty;
            this.txtEmpName.Text = String.Empty;
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            //Set search flag status
            Session[UIHelper.CONST_EMPLOYEE_SEARCH_FLAG] = 0;

            if (!string.IsNullOrEmpty(this.CallerForm))
                Response.Redirect(this.CallerForm, false);
            else
                Response.Redirect(UIHelper.PAGE_HOME, false);
        }
        #endregion

        #region Data Binding
        protected void gvList_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item.ItemType == GridItemType.AlternatingItem || e.Item.ItemType == GridItemType.Item)
            {

                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {
                    #region Remove items wherein cost center is not allowed (To-do)
                    //List<string> AllowedCostCenterList = Session[UIHelper.CONST_ALLOWED_COSTCENTER] != null ? Session[UIHelper.CONST_ALLOWED_COSTCENTER] as List<string> : null;
                    //if (AllowedCostCenterList != null && AllowedCostCenterList.Count > 0 && cmbCostCenter.Text == string.Empty)
                    //{
                    //    if (item["CostCenter"].Text.Replace("&nbsp;", String.Empty).Length > 0)
                    //    {
                    //        string CostCenter = item["CostCenter"].Text;
                    //        string CCenter = AllowedCostCenterList.Where(c => c.ToString().Trim().Equals(CostCenter)).FirstOrDefault();
                    //        if (string.IsNullOrEmpty(CCenter))
                    //        {
                    //            this.gvList.MasterTableView.PerformDelete(item, true);
                    //            return;
                    //        }
                    //    }
                    //}
                    #endregion

                    #region Format Display
                    // Format Supervisor Name
                    Literal litSupervisorName = item["SupervisorName"].FindControl("litSupervisorName") as Literal;
                    if (litSupervisorName != null)
                    {
                        litSupervisorName.Text = String.Format("({0}) {1}", item["SupervisorNo"].Text, litSupervisorName.Text);
                        item["SupervisorEmpName"].Text = litSupervisorName.Text;
                    }

                    // Format Superintendent
                    Literal litSuperintendentName = item["SuperintendentName"].FindControl("litSuperintendentName") as Literal;
                    if (litSuperintendentName != null)
                    {
                        litSuperintendentName.Text = String.Format("({0}) {1}", item["SuperintendentNo"].Text, litSuperintendentName.Text);
                    }

                    // Format Manager
                    Literal litManagerName = item["ManagerName"].FindControl("litManagerName") as Literal;
                    if (litManagerName != null)
                    {
                        litManagerName.Text = String.Format("({0}) {1}", item["ManagerNo"].Text, litManagerName.Text);
                    }
                    #endregion
                }
            }
        }

        protected void gvList_ItemCommand(object sender, GridCommandEventArgs e)
        {
            if (e.CommandName.Equals(RadGrid.SelectCommandName))
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {
                    Session[UIHelper.CONST_EMPLOYEE_SEARCH_FLAG] = 1;

                    if (this.CallerForm != string.Empty)
                    {
                        string empStatus = item["Status"].Text.Trim().Replace("&nbsp;", string.Empty);

                        // Get Employee Name
                        string empName = string.Empty;
                        System.Web.UI.WebControls.Literal litEmpName = item["EmpName"].Controls[1] as System.Web.UI.WebControls.Literal;
                        if (litEmpName != null)
                            empName = litEmpName.Text.Trim();

                        // Get Cost Center Name
                        string costCenterName = string.Empty;
                        System.Web.UI.WebControls.Literal litCostCenterName = item["CostCenterName"].Controls[1] as System.Web.UI.WebControls.Literal;
                        if (litEmpName != null)
                            costCenterName = Server.UrlEncode(litCostCenterName.Text.Trim());

                        // Get Position
                        string empPosition = string.Empty;
                        System.Web.UI.WebControls.Literal litEmpPositionDesc = item["EmpPositionDesc"].Controls[1] as System.Web.UI.WebControls.Literal;
                        if (litEmpPositionDesc != null)
                            empPosition = litEmpPositionDesc.Text.Trim();

                        // Get Supervisor 
                        string supervisorName = string.Empty;
                        System.Web.UI.WebControls.Literal litSupervisorName = item["SupervisorName"].Controls[1] as System.Web.UI.WebControls.Literal;
                        if (litSupervisorName != null)
                            supervisorName = litSupervisorName.Text.Trim();

                        // Get Superintendent 
                        string superintendentName = string.Empty;
                        System.Web.UI.WebControls.Literal litSuperintendentName = item["SuperintendentName"].Controls[1] as System.Web.UI.WebControls.Literal;
                        if (litSuperintendentName != null)
                            superintendentName = litSuperintendentName.Text.Trim();

                        // Get Cost Center Manager
                        string managerName = string.Empty;
                        System.Web.UI.WebControls.Literal litManagerName = item["ManagerName"].Controls[1] as System.Web.UI.WebControls.Literal;
                        if (litManagerName != null)
                            managerName = litManagerName.Text.Trim();

                        Response.Redirect
                        (
                            String.Format(this.CallerForm + "?{0}={1}&{2}={3}&{4}={5}&{6}={7}&{8}={9}&{10}={11}&{12}={13}&{14}={15}&{16}={17}&{18}={19}&{20}={21}&{22}={23}&{24}={25}",
                            UIHelper.QUERY_STRING_EMPNO_KEY,
                            UIHelper.ConvertObjectToString(item["EmpNo"].Text),

                            UIHelper.QUERY_STRING_EMPNAME_KEY,
                            empName,

                            UIHelper.QUERY_STRING_COSTCENTER_KEY,
                            Server.HtmlEncode(item["CostCenter"].Text),

                            UIHelper.QUERY_STRING_DEPARTMENT_KEY,
                            costCenterName,

                            UIHelper.QUERY_STRING_SUPERVISOR_KEY,
                            supervisorName,

                            UIHelper.QUERY_STRING_COSTCENTER_MANAGER_KEY,
                            managerName,

                            UIHelper.QUERY_STRING_POSITION_KEY,
                            empPosition,

                            UIHelper.QUERY_STRING_EXTENSION_KEY,
                            Server.HtmlEncode(item["TelephoneExt"].Text),
                            UIHelper.QUERY_STRING_SUPERVISOR_NO_KEY,
                            Server.HtmlEncode(item["SupervisorNo"].Text),

                            UIHelper.QUERY_STRING_SUPERVISOR_NAME_KEY,
                            supervisorName,

                            UIHelper.QUERY_STRING_WORKINGCOSTCENTER_KEY,
                            Server.HtmlEncode(item["WorkCostCenter"].Text),
                            UIHelper.QUERY_STRING_PAY_GRADE_KEY,
                            Server.HtmlEncode(item["PayGrade"].Text),
                            UIHelper.QUERY_STRING_EMPLOYEE_STATUS_KEY,
                            empStatus
                        ),
                        false);
                    }
                }
            }
        }
        #endregion

        #region Database Access
        protected void objCostCenter_Selected(object sender, ObjectDataSourceStatusEventArgs e)
        {
            EmployeeDAL.CostCenterDataTable dataTable = e.ReturnValue as
                EmployeeDAL.CostCenterDataTable;

            // Checks if found
            if (dataTable != null && dataTable.Count > 0)
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

        protected void objEmployee_Selected(object sender, ObjectDataSourceStatusEventArgs e)
        {
            EmployeeDAL.EmployeeDataTable dataTable = e.ReturnValue as EmployeeDAL.EmployeeDataTable;

            if (dataTable.Count == 0)
            {
                #region Get employee details from the Employee Master
                int empNo = UIHelper.ConvertObjectToInt(this.txtEmpNo.Text);
                if (empNo > 0 && this.IsFetchEmployeeMaster)
                {
                    string error = string.Empty;
                    string innerError = string.Empty;

                    DALProxy proxy = new DALProxy();
                    var rawData = proxy.GetEmployeeDetail(empNo, ref error, ref innerError);
                    if (rawData != null)
                    {
                        // Check if the employee's cost center exist in the allowed cost center list
                        if (this.Master.AllowedCostCenterList.Count > 0)
                        {
                            string allowedCC = this.Master.AllowedCostCenterList
                                .Where(a => a == UIHelper.ConvertObjectToString(rawData.CostCenter))
                                .FirstOrDefault();
                            if (!string.IsNullOrEmpty(allowedCC))
                            {
                                EmployeeDAL.EmployeeRow row = dataTable.NewEmployeeRow();

                                row.EmpNo = rawData.EmpNo;
                                row.EmpName = UIHelper.ConvertObjectToString(rawData.EmpName);
                                //row.EmpEmail = rawData.EmpEmail;
                                //row.EmpUserID = rawData.EmpUserID;
                                row.CostCenter = UIHelper.ConvertObjectToString(rawData.CostCenter);
                                row.CostCenterName = UIHelper.ConvertObjectToString(rawData.CostCenterName);
                                row.SuperintendentNo = UIHelper.ConvertObjectToInt(rawData.SupervisorEmpNo);
                                row.SupervisorName = UIHelper.ConvertObjectToString(rawData.SupervisorEmpName);
                                row.SuperintendentNo = rawData.SuperintendentEmpNo;
                                row.SuperintendentName = UIHelper.ConvertObjectToString(rawData.SuperintendentEmpName);
                                row.ManagerNo = UIHelper.ConvertObjectToInt(rawData.ManagerEmpNo);
                                row.ManagerName = UIHelper.ConvertObjectToString(rawData.ManagerEmpName);
                                row.EmpPositionDesc = UIHelper.ConvertObjectToString(rawData.Position);
                                row.EmpPositionID = rawData.PositionID;
                                row.PayGrade = UIHelper.ConvertObjectToInt(rawData.PayGrade);
                                row.Gender = UIHelper.ConvertObjectToString(rawData.Gender);
                                row.EmpClass = UIHelper.ConvertObjectToString(rawData.EmployeeClass);
                                row.TicketClass = UIHelper.ConvertObjectToString(rawData.TicketClass);
                                row.Destination = UIHelper.ConvertObjectToString(rawData.Destination);
                                row.Status = UIHelper.ConvertObjectToString(rawData.EmployeeStatus);

                                dataTable.Rows.InsertAt(row, 0);
                            }
                        }
                    }
                }
                #endregion
            }
            else
            {
                #region Check if cost center exist in the allowed cost center list
                if (this.Master.AllowedCostCenterList.Count > 0)
                {
                    List<DataRow> recordToDelete = new List<DataRow>();
                    foreach (DataRow item in dataTable.Rows)
                    {
                        string allowedCC = this.Master.AllowedCostCenterList
                            .Where(a => a == UIHelper.ConvertObjectToString(item["CostCenter"]))
                            .FirstOrDefault();
                        if (string.IsNullOrEmpty(allowedCC))
                        {
                            recordToDelete.Add(item);
                        }
                    }

                    if (recordToDelete.Count > 0)
                    {
                        foreach (DataRow row in recordToDelete)
                        {
                            dataTable.Rows.Remove(row);
                        }

                        DisplayFormLevelError("Sorry, you don't have access permission to view the information of the specified employee. Please contact ICT or create a Helpdesk Request to grant you cost center permission!");
                    }
                }
                #endregion
            }
        }
        #endregion
    }
}