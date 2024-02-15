using GARMCO.AMS.GAP.Utility;
using GARMCO.AMS.TAS.UI.Helpers;
using GARMCO.AMS.TAS.UI.Views.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GARMCO.AMS.TAS.UI.Views.SecurityModule
{
    public partial class ContractorRegistration : BaseWebForm
    {
        #region Properties
        public string JSVersion { get { return Session[UIHelper.CONST_JS_VERSION].ToString(); } }
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

                this.Master.SetPageForm(UIHelper.FormAccessCodes.MANUALSWIP.ToString());
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

                if (!string.IsNullOrEmpty(costCenter))
                {
                    sb.Append(string.Format("Cost Center: {0} <br />", costCenter));
                }

                this.Master.LogOnUser = string.Format("Welcome {0}",
                   UIHelper.GetUserFirstName(UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_FULLNAME]), UIHelper.ConvertObjectToBolean(Session[UIHelper.GARMCO_USER_IS_SPECIAL])));

                this.Master.LogOnUserInfo = sb.ToString().Trim();
                this.Master.FormTitle = UIHelper.PAGE_MANUAL_ATTENDANCE_TITLE;
                #endregion

                #region Check if user has permission to access the page
                if (!GAPFunction.CheckFormAccess(this.Master.FormAccess, GAPConstants.FormAccessIndex.Retrieve))
                {
                    if (SecurityUserList.Count > 0 &&
                        SecurityUserList.Where(a => a.Trim() == userID).FirstOrDefault() == null)
                    {
                        Response.Redirect(String.Format("{0}?error={1}&pageName={2}", UIHelper.PAGE_ERROR, Convert.ToInt32(UIHelper.PageErrorCodes.NoAccessOnPage), UIHelper.PAGE_MANUAL_ATTENDANCE_TITLE), true);
                    }
                }
                #endregion

                #region Checks if current user has permission to create, update, delete and print
                //this.btnSwipeIn.Enabled = this.btnSwipeOut.Enabled = this.Master.IsCreateAllowed;
                #endregion

                #region Set default button
                //this.Master.DefaultButton = this.btnGet.UniqueID;
                #endregion

                #region Restore saved data from local storage
                string formFlag = string.Empty;
                //if (this.ManualAttendanceStorage.Count > 0)
                //{
                //    if (this.ManualAttendanceStorage.ContainsKey("FormFlag"))
                //        formFlag = UIHelper.ConvertObjectToString(this.ManualAttendanceStorage["FormFlag"]);
                //}
                #endregion

                //if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.GetEmployeeInfo.ToString())
                //{
                //    #region Get the employee info
                //    RestoreDataFromCollection();

                //    string callerControlName = this.ManualAttendanceStorage.ContainsKey("CallerControlName")
                //        ? UIHelper.ConvertObjectToString(this.ManualAttendanceStorage["CallerControlName"]) : string.Empty;

                //    if (UIHelper.ConvertObjectToInt(Session[UIHelper.CONST_EMPLOYEE_SEARCH_FLAG]) == 1)
                //    {
                //        switch (callerControlName)
                //        {
                //            case "btnFindEmployee":
                //                this.txtEmpNo.Value = UIHelper.ConvertObjectToInt(Request.QueryString[UIHelper.QUERY_STRING_EMPNO_KEY]);
                //                this.litEmpName.Text = UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_EMPNAME_KEY]);
                //                this.litPosition.Text = UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_POSITION_KEY]);
                //                this.litCostCenter.Text = string.Format("{0} - {1}",
                //                    UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_WORKINGCOSTCENTER_KEY]) != string.Empty ? UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_WORKINGCOSTCENTER_KEY]) : UIHelper.ConvertObjectToString(Request.QueryString[UIHelper.QUERY_STRING_COSTCENTER_KEY]),
                //                    UIHelper.ConvertObjectToString(Server.UrlDecode(Request.QueryString[UIHelper.QUERY_STRING_DEPARTMENT_KEY])));
                //                break;

                //            case "btnFindEmpHistory":
                //                this.txtEmpNoHistory.Value = UIHelper.ConvertObjectToInt(Request.QueryString[UIHelper.QUERY_STRING_EMPNO_KEY]);
                //                break;
                //        }
                //    }

                //    // Clear data storage
                //    Session.Remove("ManualAttendanceStorage");
                //    #endregion
                //}
                //else if (formFlag != string.Empty && formFlag == UIHelper.PagePostBackFlags.RedirectToOtherPage.ToString())
                //{
                //    #region Show the last form data
                //    RestoreDataFromCollection();
                //    ProcessQueryString();

                //    // Clear data storage
                //    Session.Remove("ManualAttendanceStorage");

                //    // Check if need to refresh data in the grid
                //    if (this.ReloadGridData)
                //        this.btnSearch_Click(this.btnSearch, new EventArgs());
                //    #endregion
                //}
                //else
                //{
                //    ClearForm();
                //    ProcessQueryString();
                //    FillComboData();

                //    // Fill data to the grid
                //    this.btnSearch_Click(this.btnSearch, new EventArgs());
                //}
            }

            ScriptManager.RegisterStartupScript(this, this.GetType(), "HideScrollbar", "HideScrollbar();", true);
            //AddControlsAttribute();
        }
        #endregion


    }
}