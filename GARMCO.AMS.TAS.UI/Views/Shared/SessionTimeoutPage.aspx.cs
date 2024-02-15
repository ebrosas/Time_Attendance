using GARMCO.AMS.TAS.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GARMCO.AMS.TAS.UI.Views.Shared
{
    public partial class SessionTimeoutPage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void lnkHomePage_Click(object sender, EventArgs e)
        {
            Response.Redirect
            (
                String.Format(UIHelper.PAGE_HOME + "?{0}={1}",
                UIHelper.QUERY_STRING_CALLER_FORM_KEY,
                UIHelper.PAGE_SESSION_TIMEOUT_PAGE
            ),
            false);
        }
    }
}