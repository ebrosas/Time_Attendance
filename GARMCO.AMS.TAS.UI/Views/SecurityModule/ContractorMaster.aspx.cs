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
    public partial class ContractorMaster : BaseWebForm
    {
        #region Fields
        private const string CONST_CONTRACT_REGISTRATION = "register";
        private const string CONST_CONTRACT_INQUIRY = "inquiry";
        private const string CONST_IDCARD_GENERATOR = "idcard";
        #endregion

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

                this.hidFormName.Value = this.GetQueryStringValue("formName");
                if (this.hidFormName.Value == CONST_CONTRACT_REGISTRATION)
                    this.Master.SetPageForm(UIHelper.FormAccessCodes.CONTREGSTR.ToString());
                else if (this.hidFormName.Value == CONST_CONTRACT_INQUIRY)
                    this.Master.SetPageForm(UIHelper.FormAccessCodes.CONTRCTINQ.ToString());
                if (this.hidFormName.Value == CONST_IDCARD_GENERATOR)
                    this.Master.SetPageForm(UIHelper.FormAccessCodes.CONTIDCARD.ToString());

                this.Master.ShowHideDateTime(true);
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
                //ClientScript.RegisterStartupScript(Page.GetType(), string.Format("script_{0}", this.ClientID), "window.onload = function() { showLoadingPanel(); };", true);
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
                
                if (this.hidFormName.Value == CONST_CONTRACT_REGISTRATION)
                    this.Master.FormTitle = UIHelper.PAGE_CONTRACTOR_REGISTRATION_TITLE;
                else if (this.hidFormName.Value == CONST_CONTRACT_INQUIRY)
                    this.Master.FormTitle = UIHelper.PAGE_CONTRACTOR_INQUIRY_TITLE;
                if (this.hidFormName.Value == CONST_IDCARD_GENERATOR)
                    this.Master.FormTitle = UIHelper.PAGE_ID_CARD_GENERATOR_TITLE;
                #endregion

                #region Check if user has permission to access the page
                if (!GAPFunction.CheckFormAccess(this.Master.FormAccess, GAPConstants.FormAccessIndex.Retrieve))
                {
                    if (SecurityUserList.Count > 0 &&
                        SecurityUserList.Where(a => a.Trim() == userID).FirstOrDefault() == null)
                    {
                        Response.Redirect(String.Format("{0}?error={1}&pageName={2}", UIHelper.PAGE_ERROR, Convert.ToInt32(UIHelper.PageErrorCodes.NoAccessOnPage), this.Master.FormTitle), true);
                    }
                }
                #endregion
            }

            ScriptManager.RegisterStartupScript(this, this.GetType(), "HideScrollbar", "HideScrollbar();", true);
            Page.ClientScript.RegisterHiddenField("hidMasterFrameClientID", this.ifInnerFrame.ClientID);

            // Show the loading panel
            //ScriptManager.RegisterStartupScript(Page, Page.GetType(), string.Format("script_{0}", this.ClientID), "showLoadingPanel();", true);
            //ClientScript.RegisterStartupScript(Page.GetType(), string.Format("script_{0}", this.ClientID), "showLoadingPanel();", true);
        }
        #endregion

        #region Private Functions
        private string GetQueryStringValue(string key)
        {
            return string.IsNullOrWhiteSpace(Request.QueryString[key]) ? string.Empty : Request.QueryString[key];
        }
        #endregion
    }
}