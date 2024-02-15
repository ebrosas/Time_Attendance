using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using System.Web;

namespace GARMCO.AMS.TAS.UI.Helpers
{
    public class ServiceHelper
    {
        #region Public Methods
        public static BasicHttpBinding GetCustomBinding()
        {
            BasicHttpBinding bTHttpBinding = new BasicHttpBinding("BasicHttpEndpoint");

            #region Code commented for future use
            //BasicHttpBinding bTHttpBinding = new BasicHttpBinding(BasicHttpSecurityMode.None);
            //bTHttpBinding = new BasicHttpBinding(BasicHttpSecurityMode.None);
            //bTHttpBinding.MaxReceivedMessageSize = int.MaxValue;
            //bTHttpBinding.MaxBufferSize = int.MaxValue;
            //bTHttpBinding.CloseTimeout = TimeSpan.FromMinutes(15);
            //bTHttpBinding.OpenTimeout = TimeSpan.FromMinutes(15);
            //bTHttpBinding.ReceiveTimeout = TimeSpan.FromMinutes(15);
            //bTHttpBinding.SendTimeout = TimeSpan.FromMinutes(15);
            #endregion

            return bTHttpBinding;
        }

        public static string GetDynamicEndpointOld(Uri address)
        {
            string Scheme = address.Scheme;
            string Host = address.DnsSafeHost;
            string Port = address.Port.ToString();
            string[] VirtualDir = address.AbsolutePath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            string endpointString = string.Empty;
            if (VirtualDir.Length > 0)
            {
                string VD = VirtualDir[0];
                if (!VD.Contains(".aspx"))
                {
                    endpointString = Scheme + "://" + Host + ":" + Port + "/" + VD + "/";
                }
                else
                {
                    endpointString = Scheme + "://" + Host + ":" + Port + "/";
                }
            }
            else
            {
                endpointString = Scheme + "://" + Host + ":" + Port + "/";
            }

            return endpointString;
        }

        public static string GetDynamicEndpoint(Uri address)
        {
            string Scheme = address.Scheme;
            string Host = address.DnsSafeHost;
            string Port = address.Port.ToString();
            string[] VirtualDir = null;

            #region Check if URI contains the aspx file extension
            string urlPath = address.AbsolutePath;
            if (!urlPath.Contains("aspx"))
                urlPath = string.Concat(urlPath, ".aspx");

            VirtualDir = urlPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            #endregion

            string endpointString = string.Empty;
            if (VirtualDir.Length > 0)
            {
                string VD = VirtualDir[0];
                if (VD != "Views")
                {
                    if (Port != string.Empty)
                    {
                        if (Port != "80")
                        {
                            if (!VD.Contains(".aspx"))
                                endpointString = string.Concat(Scheme, "://", Host, ":", Port, "/", VD);
                            else
                                endpointString = string.Concat(Scheme, "://", Host, ":", Port);
                        }
                        else
                        {
                            if (!VD.Contains(".aspx"))
                                endpointString = string.Concat(Scheme, "://", Host, "/", VD);
                            else
                                endpointString = string.Concat(Scheme, "://", Host);
                        }
                    }
                    else
                        endpointString = string.Concat(Scheme, "://", Host);
                }
                else
                {
                    if (Port != string.Empty && Port != "80")
                        endpointString = string.Concat(Scheme, "://", Host, ":", Port);
                    else
                        endpointString = string.Concat(Scheme, "://", Host);
                }
            }
            else
            {
                if (Port != string.Empty && Port != "80")
                    endpointString = Scheme + "://" + Host + ":" + Port + "/";
                else
                    endpointString = string.Concat(Scheme, "://", Host);
            }

            return endpointString;
        }

        public static string GetDynamicEndpointWithDefaultPage(Uri address)
        {
            string Scheme = address.Scheme;
            string Host = address.DnsSafeHost;
            string Port = address.Port.ToString();
            string[] VirtualDir = address.AbsolutePath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            string endpointString = string.Empty;
            if (VirtualDir.Length > 0)
            {
                string VD = VirtualDir[0];
                if (VD != "Views")
                {
                    if (Port != string.Empty)
                    {
                        if (Port != "80")
                        {
                            if (!VD.Contains(".aspx"))
                                endpointString = string.Concat(Scheme, "://", Host, ":", Port, "/", VD);
                            else
                                endpointString = string.Concat(Scheme, "://", Host, ":", Port);
                        }
                        else
                        {
                            if (!VD.Contains(".aspx"))
                                endpointString = string.Concat(Scheme, "://", Host, "/", VD);
                            else
                                endpointString = string.Concat(Scheme, "://", Host);
                        }
                    }
                    else
                        endpointString = string.Concat(Scheme, "://", Host);
                }
                else
                {
                    if (Port != string.Empty && Port != "80")
                        endpointString = string.Concat(Scheme, "://", Host, ":", Port);
                    else
                        endpointString = string.Concat(Scheme, "://", Host);
                }
            }
            else
            {
                if (Port != string.Empty && Port != "80")
                    endpointString = Scheme + "://" + Host + ":" + Port + "/";
                else
                    endpointString = string.Concat(Scheme, "://", Host);
            }

            return string.Concat(endpointString, @"/Default.aspx?url=");
        }

        public static string GetSQLConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString.Trim();
        }
        #endregion
    }
}