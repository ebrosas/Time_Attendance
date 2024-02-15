using GARMCO.AMS.TAS.UI.Helpers;
using GARMCO.AMS.TAS.UI.Views.Shared;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GARMCO.AMS.TAS.UI
{
    public partial class Index : BaseWebForm
    {
        #region Page Events
        protected override void OnInit(EventArgs e)
        {
            base.IsRetrieveUserInfo = true;
            base.OnInit(e);

            if (!this.IsPostBack)
            {
                if (this.Master.IsSessionExpired)
                    Response.Redirect(UIHelper.PAGE_SESSION_TIMEOUT_PAGE, false);

                this.Master.SetPageForm(UIHelper.FormAccessCodes.EMPSELFSVC.ToString());
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                #region Retrieve the url and show the application in fullscreen
                string url = Request.QueryString["url"];
                bool isUnderMaintenance = ConfigurationManager.AppSettings["UnderMaintenance"].Trim() == "1" ? true : false;
                string homePage = UIHelper.PAGE_HOME;

                if (!isUnderMaintenance)
                {
                    if (IsUserAuthenticated)
                    {
                        string userID = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]);
                        if (SecurityUserList.Count > 0 &&
                            SecurityUserList.Where(a => a.Trim() == userID).FirstOrDefault() != null)
                        {
                            // Current user is member of the Security Personnel group, so set the "Manual Attendance" to be the homepage 
                            url = String.Format("ShowApplicationInFullScreen('{0}');",
                                string.Concat(ServiceHelper.GetDynamicEndpoint(Request.Url), UIHelper.PAGE_MANUAL_ATTENDANCE.Replace("~", string.Empty)));
                        }
                        else if (SpecialUserList.Count > 0 &&
                            SpecialUserList.Where(a => a.Trim() == userID).FirstOrDefault() != null)
                        {
                            // Current user is a special user, so set the "Employee Attendance Dashboard" to be the homepage
                            url = String.Format("ShowApplicationInFullScreen('{0}');",
                                string.Concat(ServiceHelper.GetDynamicEndpoint(Request.Url), UIHelper.PAGE_EMPLOYEE_ATTENDANCE_DASHBOARD.Replace("~", string.Empty)));
                        }
                        else if (UIHelper.ConvertNumberToBolean(Session[UIHelper.GARMCO_USER_IS_GROUP_ACCOUNT]))
                        {
                            url = String.Format("ShowApplicationInFullScreen('{0}');",
                                string.Concat(ServiceHelper.GetDynamicEndpoint(Request.Url), UIHelper.PAGE_LOGIN.Replace("~", string.Empty)));
                        }
                        else
                        {
                            url = String.Format("ShowApplicationInFullScreen('{0}');",
                            string.Concat(ServiceHelper.GetDynamicEndpoint(Request.Url), homePage.Replace("~", string.Empty)));
                        }
                    }
                    else
                    {
                        url = String.Format("ShowApplicationInFullScreen('{0}');",
                            string.Concat(ServiceHelper.GetDynamicEndpoint(Request.Url), homePage.Replace("~", string.Empty)));
                    }
                }
                else
                {
                    url = String.Format("ShowApplicationInFullScreen('{0}');",
                        string.Concat(ServiceHelper.GetDynamicEndpoint(Request.Url), UIHelper.PAGE_UNDER_MAINTENANCE.Replace("~", string.Empty)));
                }

                //ClientScriptManager csm = Page.ClientScript;
                //Type csType = this.GetType();
                //string csName = "CustomScript";
                //csm.RegisterStartupScript(csType, csName, url);

                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "Application", url, true);
                #endregion
            }
        }
        #endregion
    }
}