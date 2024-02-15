using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using GARMCO.AMS.TAS.UI.Helpers;
using GARMCO.AMS.TAS.UI.Views.Shared;

namespace GARMCO.AMS.TAS.UI
{
    public partial class Default : BaseWebForm
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
                bool isUnderMaintenance = ConfigurationManager.AppSettings["UnderMaintenance"].Trim() == "1" ? true : false;
                string homePage = UIHelper.PAGE_HOME;

                if (isUnderMaintenance)
                    Response.Redirect(UIHelper.PAGE_UNDER_MAINTENANCE, false);
                else
                {
                    if (IsUserAuthenticated)
                    {
                        string userID = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]);
                        if (SecurityUserList.Count > 0 &&
                            SecurityUserList.Where(a => a.Trim() == userID).FirstOrDefault() != null)
                        {
                            // Current user is member of the Security Personnel group, so set the "Manual Attendance" to be the homepage 
                            Response.Redirect(UIHelper.PAGE_MANUAL_ATTENDANCE, false);
                        }
                        else if (SpecialUserList.Count > 0 &&
                            SpecialUserList.Where(a => a.Trim() == userID).FirstOrDefault() != null)
                        {
                            // Current user is a special user, so set the "Employee Attendance Dashboard" to be the homepage
                            Response.Redirect(UIHelper.PAGE_EMPLOYEE_ATTENDANCE_DASHBOARD, false);
                        }
                        else if (UIHelper.ConvertNumberToBolean(Session[UIHelper.GARMCO_USER_IS_GROUP_ACCOUNT]))
                            Response.Redirect(UIHelper.PAGE_LOGIN, false);
                        else
                            Response.Redirect(homePage, false);
                    }
                    else
                        Response.Redirect(UIHelper.PAGE_LOGIN, false);
                }
            }
        }
        #endregion
    }
}