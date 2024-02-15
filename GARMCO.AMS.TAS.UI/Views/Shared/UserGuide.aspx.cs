using GARMCO.AMS.TAS.UI.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GARMCO.AMS.TAS.UI.Views.Shared
{
    public partial class UserGuide : System.Web.UI.Page
    {
        #region Properties
        private int ManualType
        {
            get
            {
                return UIHelper.ConvertObjectToInt(ViewState[UIHelper.CONST_USER_GUIDE_TYPE]);
            }
            set
            {
                ViewState[UIHelper.CONST_USER_GUIDE_TYPE] = value;
            }
        }
        #endregion

        #region Page Methods
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                this.ManualType = UIHelper.ConvertObjectToInt(Request.QueryString[UIHelper.CONST_USER_GUIDE_TYPE]);
                this.Title = "TAS - User Guide Manual";
                ReadPdfFile();
            }
        }
        #endregion

        #region Private Methods
        private void ReadPdfFile()
        {
            try
            {
                string path = string.Empty;
                if (this.ManualType == Convert.ToInt32(UIHelper.UserManualType.OvertimeOnlineApproval))
                    path = Server.MapPath(UIHelper.CONST_USER_GUIDE_OVERTIME);
                else if (this.ManualType == Convert.ToInt32(UIHelper.UserManualType.EmergencyResponseTeam))
                    path = Server.MapPath(UIHelper.CONST_USER_GUIDE_FIRE_TEAM);

                if (!File.Exists(path))
                    return;

                WebClient client = new WebClient();
                Byte[] buffer = client.DownloadData(path);

                if (buffer != null)
                {
                    Response.ContentType = "application/pdf";
                    Response.AddHeader("content-length", buffer.Length.ToString());
                    Response.BinaryWrite(buffer);
                }
            }
            catch (Exception ex)
            {
                //Exception appError = new ApplicationException("The system could not identify the request type of the selected record");
                //appError.Source = "Day-in-Lieu User Guide page";
                //this.ApplicationException = ex;
                //Response.Redirect(DILHelper.PAGE_DIL_ERROR_PAGE, true);
            }
        }
        #endregion
    }
}