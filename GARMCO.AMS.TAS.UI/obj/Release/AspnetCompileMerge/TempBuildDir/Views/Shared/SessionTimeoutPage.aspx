<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/TASMasterNoMenu.Master" AutoEventWireup="true" CodeBehind="SessionTimeoutPage.aspx.cs" 
    Inherits="GARMCO.AMS.TAS.UI.Views.Shared.SessionTimeoutPage" StylesheetTheme="Standard" %>

<%@ MasterType VirtualPath="~/Views/Shared/TASMaster.Master" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <title>Expired Page Sesssion</title>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="mainContent" runat="server">
    <asp:Panel ID="panError" runat="server" style="margin-left: auto; margin-right: auto; vertical-align: middle; width: 900px;">
		<br /><br /><br /><br /><br /><br /> 
		<table border="0" style="vertical-align: top; width: 100%">
			<tr>
                <td style="vertical-align: top; text-align: left; width: 100px;">
					<asp:Image ID="imgError" runat="server" ImageUrl="~/Images/expired_session_icon.png" />
				</td>
				<td style="width: 800px; text-align: left;">
					<asp:Panel ID="panStackError" runat="server" Width="100%">
						<table border="0" style="width: 100%;">
							<tr>
								<td style="width: auto; text-align: left; color: black; font-size: 9pt; font-family: Verdana;">
									Sorry, your session has already expired. As part of the system's security, the session automatically ends after 60 minutes of inactivity. <br /> <br /> Please click <a href="../UserFunctions/EmployeeSelfService.aspx">here</a> to go to the Homepage. <br />
								</td>
							</tr>
						</table>
					</asp:Panel>
				</td>
			</tr>
		</table>
	</asp:Panel>
</asp:Content>
