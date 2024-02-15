using GARMCO.AMS.TAS.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace GARMCO.AMS.TAS.UI.Views.Shared
{
    /// <summary>
    /// Summary description for FileHandler
    /// </summary>
    public class FileHandler : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            #region Retrieve the file and display it
            if (!String.IsNullOrEmpty(context.Request.QueryString["filename"]))
            {

                // Set the full path
                string fileName = UIHelper.ConvertObjectToString(context.Request.QueryString["filename"]);
                string fileType = UIHelper.ConvertObjectToString(context.Request.QueryString["fileType"]);

                if (fileName != string.Empty)
                {
                    string fileFolder = ConfigurationManager.AppSettings["DownloadPhysicalPath"];
                    string filePath = string.Concat(fileFolder, @"\", fileName);

                    if (!File.Exists(filePath))
                    {
                        return;
                    }

                    WebClient client = new WebClient();
                    Byte[] buffer = client.DownloadData(filePath);

                    if (buffer != null)
                    {
                        context.Response.ContentType = fileType;
                        context.Response.AddHeader("content-length", buffer.Length.ToString());
                        context.Response.BinaryWrite(buffer);
                    }
                }
            }
            #endregion
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}