<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ReportViewer.aspx.cs" Inherits="GARMCO.AMS.TAS.UI.Views.Reports.ReportViewer" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register Assembly="Telerik.ReportViewer.WebForms" Namespace="Telerik.ReportViewer.WebForms" TagPrefix="telerik" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <link href="../../Styles/main.css" rel="stylesheet" type="text/css" />
    <script src="../../Scripts/main.js"></script>
     
    <title>TAS Report Viewer</title>
</head>
<body style="background-color: gainsboro;">
    <form id="form1" runat="server">
        <asp:ScriptManager ID="scriptMngr" runat="server" />
		<telerik:RadFormDecorator ID="formDecor" runat="server" Skin="Windows7" />
        <div>
            <asp:Panel ID="panMain" runat="server" style="width: 100%; margin-top: 0px; background-color: gainsboro;">
                <table border="0" style="width: 100%; margin-top: 0px; padding-top: 0px;">
		            <tr>
			            <td style="vertical-align: top; padding-right: 5px; padding-left: 5px;">
                            <telerik:ReportViewer ID="repViewer" runat="server" BorderColor="#E0E0E0" 
                                BorderStyle="None" BorderWidth="1px" 
                                Height="900px" Skin="WebBlue" Width="100%" ZoomPercent="100"></telerik:ReportViewer>
			            </td>
		            </tr>
		            <tr>
			            <td style="padding-left: 15px; padding-top: 10px; padding-bottom: 20px; text-align: left; background-color: gainsboro">
                            <telerik:RadButton ID="btnClose" runat="server" Text="Close Report" ToolTip="Close report and go back to previous page" 
                                Skin="Glow" Font-Bold="False" Font-Size="9pt" 
                                CausesValidation="false" Visible="true" Width="110px" 
                                CssClass="RadButtonStyle" onclick="btnClose_Click">                       
                            </telerik:RadButton>   
			            </td>
		            </tr>
	            </table>
            </asp:Panel>
        </div>
    </form>
</body>
</html>
