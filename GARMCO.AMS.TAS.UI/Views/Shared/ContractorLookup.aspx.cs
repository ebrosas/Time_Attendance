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

namespace GARMCO.AMS.TAS.UI.Views.Shared
{
    public partial class ContractorLookup : BaseWebForm, IFormExtension
    {
        #region Enumeration
        private enum ValidationErrorType
        {
            NoError,
            CustomFormError,
            InvalidContractorNo
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

        private Dictionary<string, object> ContractorLookupStorage
        {
            get
            {
                Dictionary<string, object> list = Session["ContractorLookupStorage"] as Dictionary<string, object>;
                if (list == null)
                    Session["ContractorLookupStorage"] = list = new Dictionary<string, object>();

                return list;
            }
            set
            {
                Session["ContractorLookupStorage"] = value;
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

        private List<EmployeeDetail> ContractorList
        {
            get
            {
                List<EmployeeDetail> list = ViewState["ContractorList"] as List<EmployeeDetail>;
                if (list == null)
                    ViewState["ContractorList"] = list = new List<EmployeeDetail>();

                return list;
            }
            set
            {
                ViewState["ContractorList"] = value;
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

                this.Master.SetPageForm(UIHelper.FormAccessCodes.CONTCRLKUP.ToString());

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
                this.Master.FormTitle = UIHelper.PAGE_CONTRACT_EMPLOYEE_SEARCH_TITLE;
                #endregion

                #region Check if user has permission to access the page
                if (!GAPFunction.CheckFormAccess(this.Master.FormAccess, GAPConstants.FormAccessIndex.Retrieve))
                {
                    if (!UIHelper.ConvertObjectToBolean(Session[UIHelper.GARMCO_USER_IS_SPECIAL]))
                    {
                        Response.Redirect(String.Format("{0}?error={1}&pageName={2}", UIHelper.PAGE_ERROR, Convert.ToInt32(UIHelper.PageErrorCodes.NoAccessOnPage), UIHelper.PAGE_CONTRACT_EMPLOYEE_SEARCH_TITLE), true);
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
                if (this.ContractorLookupStorage.Count > 0)
                {
                    if (this.ContractorLookupStorage.ContainsKey("FormFlag"))
                        formFlag = UIHelper.ConvertObjectToString(this.ContractorLookupStorage["FormFlag"]);
                }
                #endregion

                if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.ShowReport.ToString())
                {
                    #region Show the last form data
                    RestoreDataFromCollection();

                    // Clear data storage
                    Session.Remove("ContractorLookupStorage");
                    #endregion
                }
                else
                {
                    ClearForm();
                    ProcessQueryString();

                    // Fill data to the grid
                    this.btnSearch_Click(this.btnSearch, new EventArgs());
                }
            }

            ScriptManager.RegisterStartupScript(this, this.GetType(), "HideScrollbar", "HideScrollbar();", true);
            AddControlsAttribute();
        }
        #endregion

        #region Grid Events
        protected void gridSearchResult_PageIndexChanged(object sender, Telerik.Web.UI.GridPageChangedEventArgs e)
        {
            RebindDataToGrid();
        }

        protected void gridSearchResult_PageSizeChanged(object sender, Telerik.Web.UI.GridPageSizeChangedEventArgs e)
        {
            RebindDataToGrid();
        }

        protected void gridSearchResult_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
        {
            if (this.ContractorList.Count > 0)
            {
                this.gridSearchResult.DataSource = this.ContractorList;
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
                InitializeDataGrid();
        }

        protected void gridSearchResult_ItemCommand(object sender, GridCommandEventArgs e)
        {
            if (e.CommandName.Equals(RadGrid.SelectCommandName))
            {
                GridDataItem item = e.Item as GridDataItem;
                if (item != null)
                {
                    Session[UIHelper.CONST_EMPLOYEE_SEARCH_FLAG] = 1;

                    // Get Contractor No
                    int empNo = UIHelper.ConvertObjectToInt(item["EmpNo"].Text);

                    // Get Contractor Name
                    string empName = string.Empty;
                    System.Web.UI.WebControls.Literal litEmpName = item["ContractorEmpName"].Controls[1] as System.Web.UI.WebControls.Literal;
                    if (litEmpName != null)
                        empName = litEmpName.Text.Trim();

                    #region Go back to the caller form
                    Response.Redirect
                        (
                            String.Format(this.CallerForm + "?{0}={1}&{2}={3}",
                            UIHelper.QUERY_STRING_EMPNO_KEY,
                            empNo,
                            //UIHelper.ConvertObjectToString(item["EmpNo"].Text),
                            UIHelper.QUERY_STRING_EMPNAME_KEY,
                            empName
                        //UIHelper.ConvertObjectToString(item["ContractorEmpName"].Text)
                        ),
                        false);
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

        private void RebindDataToGrid()
        {
            if (this.ContractorList.Count > 0)
            {
                this.gridSearchResult.DataSource = this.ContractorList;
                this.gridSearchResult.DataBind();
            }
            else
                InitializeDataGrid();
        }

        private void InitializeDataGrid()
        {
            this.gridSearchResult.DataSource = new List<EmployeeDetail>();
            this.gridSearchResult.DataBind();
        }
        #endregion

        #region IFormExtension Interface Implementation
        public void ClearForm()
        {
            #region Reset controls
            this.txtContractorNo.Text = string.Empty;
            this.txtContractorName.Text = string.Empty;
            #endregion

            // Clear sessions and collections
            KillSessions();

            // Reset datagrid and other controls
            InitializeDataGrid();
            this.gridSearchResult.CurrentPageIndex = 0;
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
            this.ContractorList.Clear(); ;

            // Clear sessions
            ViewState["CustomErrorMsg"] = null;

            // Clear all viewstates
            ViewState.Clear();
        }
        #endregion

        #region Private Methods
        private void RestoreDataFromCollection()
        {
            if (this.ContractorLookupStorage.Count == 0)
                return;

            #region Restore Query String values
            if (this.ContractorLookupStorage.ContainsKey("CallerForm"))
                this.CallerForm = UIHelper.ConvertObjectToString(this.ContractorLookupStorage["CallerForm"]);
            else
                this.CallerForm = string.Empty;
            #endregion

            #region Restore session values
            if (this.ContractorLookupStorage.ContainsKey("ContractorList"))
                this.ContractorList = this.ContractorLookupStorage["ContractorList"] as List<EmployeeDetail>;
            else
                this.ContractorList = null;
            #endregion

            #region Restore control values            
            if (this.ContractorLookupStorage.ContainsKey("txtContractorNo"))
                this.txtContractorNo.Text = UIHelper.ConvertObjectToString(this.ContractorLookupStorage["txtContractorNo"]);
            else
                this.txtContractorNo.Text = string.Empty;

            if (this.ContractorLookupStorage.ContainsKey("txtContractorName"))
                this.txtContractorName.Text = UIHelper.ConvertObjectToString(this.ContractorLookupStorage["txtContractorName"]);
            else
                this.txtContractorName.Text = string.Empty;
            #endregion
        }

        private void StoreDataToCollection(UIHelper.PagePostBackFlags formFlag)
        {
            this.ContractorLookupStorage.Clear();
            this.ContractorLookupStorage.Add("FormFlag", formFlag.ToString());

            #region Save control values to session
            this.ContractorLookupStorage.Add("txtContractorNo", this.txtContractorNo.Text.Trim());
            this.ContractorLookupStorage.Add("txtContractorName", this.txtContractorName.Text.Trim());
            #endregion

            #region Save Query String values to collection
            this.ContractorLookupStorage.Add("CallerForm", this.CallerForm);
            #endregion

            #region Store session data to collection
            this.ContractorLookupStorage.Add("ContractorList", this.ContractorList);
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

            this.txtContractorNo.Focus();
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                #region Perform Data Validation
                int errorCount = 0;
                string error = string.Empty;
                string contractorName = this.txtContractorName.Text.Trim();
                int empNo = 0;

                #region Check specified contractor no.
                if (this.txtContractorNo.Text != string.Empty &&
                    UIHelper.ConvertObjectToInt(this.txtContractorNo.Text) == 0)
                {
                    this.txtGeneric.Text = ValidationErrorType.InvalidContractorNo.ToString();
                    this.ErrorType = ValidationErrorType.InvalidContractorNo;
                    this.cusValContractorNo.Validate();
                    errorCount++;
                }
                else
                {
                    empNo = UIHelper.ConvertObjectToInt(this.txtContractorNo.Text);
                    if (empNo.ToString().Length == 4)
                    {
                        empNo += 10000000;

                        // Display the formatted Emp. No.
                        this.txtContractorNo.Text = empNo.ToString();
                    }
                }
                #endregion

                #endregion

                GetContractors(empNo, contractorName);
            }
            catch (Exception ex)
            {
                DisplayFormLevelError(ex.Message.ToString());                
            }
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
                else if (this.ErrorType == ValidationErrorType.InvalidContractorNo)
                {
                    validator.ErrorMessage = "The specified Contractor No. is invalid. Please enter numeric value only!";
                    validator.ToolTip = "The specified Contractor No. is invalid. Please enter numeric value only!";
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
        private void GetContractors(int empNo, string empName)
        {
            try
            {
                // Initialize session
                this.ContractorList = null;

                string error = string.Empty;
                string innerError = string.Empty;

                DALProxy proxy = new DALProxy();
                var rawData = proxy.GetContractors(empNo, empName, ref error, ref innerError);
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
                        this.ContractorList.AddRange(rawData.ToList());

                        //Display the record count
                        this.lblRecordCount.Text = string.Format("{0} record(s) found", this.ContractorList.Count.ToString("#,###"));
                    }
                }

                // Bind data to the grid
                RebindDataToGrid();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion                
    }
}