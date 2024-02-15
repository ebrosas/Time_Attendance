using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using GARMCO.AMS.TAS.UI.Helpers;

namespace GARMCO.AMS.TAS.UI.Views.Shared
{
    public partial class ErrorMessage : BaseWebForm
    {
        #region Page Events
        protected override void OnInit(EventArgs e)
        {
            base.IsRetrieveUserInfo = true;
            base.OnInit(e);
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
                this.Master.FormTitle = UIHelper.PAGE_ERROR_TITLE;
                #endregion

                #region Retrieve the last error
                Exception exception;
                //if (Application.Contents.Count > 0)
                //    exception = Application.Contents[UIHelper.EXCEPTION_ERROR] as Exception;
                //else
                exception = Session[UIHelper.EXCEPTION_ERROR] as Exception;
                #endregion

                #region Display the exception thrown by the application
                if (exception != null)
                {
                    // Show the stack panel
                    this.panStackError.Visible = true;
                    this.imgError.ImageUrl = "~/Images/error.png";

                    this.litURL.Text = Request.QueryString["url"];
                    this.litSource.Text = exception.Source;
                    this.litMessage.Text = exception.Message;
                    this.litInnerMsg.Text = exception.InnerException != null ? exception.InnerException.Message : String.Empty;
                    this.litStackTrace.Text = exception.StackTrace;

                }
                #endregion

                #region Checks the error code
                else if (!String.IsNullOrEmpty(Request.QueryString["error"]))
                {
                    this.imgError.ImageUrl = "~/Images/accessdenied.gif";
                    int errorCode = Convert.ToInt32(Request.QueryString["error"]);
                    string pageName = UIHelper.ConvertObjectToString(Request.QueryString["pageName"]);
                    if (pageName == string.Empty)
                        pageName = "this page";
                    else
                        pageName += " page";

                    if (errorCode == Convert.ToInt32(UIHelper.PageErrorCodes.NoAccessOnPage))
                    {
                        this.Master.FormTitle = UIHelper.PAGE_SECURITY_ERROR_TITLE;
                        this.lblError.Text = string.Format("Sorry, you don't have the required permission to access the {0}.<br />Please contact HR Department for assistance or create a Helpdesk - Grant Access Request.", pageName);
                    }

                    else if (errorCode == Convert.ToInt32(UIHelper.PageErrorCodes.NotAllowedToCreate))
                    {
                        this.Master.FormTitle = UIHelper.PAGE_SECURITY_ERROR_TITLE;
                        this.lblError.Text = "Sorry, you don't have permission to create new record using this form.<br />Please contact HR Department for assistance or create a Helpdesk - Grant Access Request.";
                    }

                    else if (errorCode == Convert.ToInt32(UIHelper.PageErrorCodes.SessionExpired))
                    {
                        this.imgError.ImageUrl = "~/Images/expired_session_icon.png";
                        this.lnkHome.Visible = true;
                        this.lblError.Text = "Sorry, your session has already expired.<br />Please click Home in the main menu or click the link below to go to the default page!";
                    }
                    else if (errorCode == 404)
                        this.lblError.Text = "The browser is able to connect to the website, but the webpage is not found. This error is sometimes caused by the webpage which becomes temporarily unavailable or because the webpage has been deleted.";

                    else if (errorCode == 500)
                        this.lblError.Text = "The website you are visiting had a server problem that prevented the webpage from displaying. It often occurs as a result of website maintenance or because of a programming error on interactive websites that use scripting.";

                }
                #endregion
            }
        }
        #endregion

        protected void lnkHome_Click(object sender, EventArgs e)
        {
            Response.Redirect(UIHelper.PAGE_HOME, false);
        }
    }
}