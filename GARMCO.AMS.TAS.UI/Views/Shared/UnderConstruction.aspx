<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/TASMaster.Master" AutoEventWireup="true" CodeBehind="UnderConstruction.aspx.cs" 
    Inherits="GARMCO.AMS.TAS.UI.Views.Shared.UnderConstruction" StylesheetTheme="Standard" %>

<%@ MasterType VirtualPath="~/Views/Shared/TASMaster.Master" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <title>Page Under Construction</title>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="mainContent" runat="server">
    <asp:Panel ID="panError" runat="server" style="margin-left: auto; margin-right: auto; vertical-align: middle; width: 900px;">
		<br /><br /><br /><br /> 
		<table border="0" style="vertical-align: top; width: 100%">
			<tr>
                <td style="vertical-align: top; text-align: left; width: 100px;">
					<asp:Image ID="imgError" runat="server" ImageUrl="~/Images/under_maintenance.png" />
				</td>
				<td style="width: 800px; text-align: left;">
					<asp:Panel ID="panStackError" runat="server" Visible="true" Width="100%">
						<br style="font: Verdana; font-size: 10pt; font-weight: bold;" />We apologize, this web page is under construction.<br /><br />
						<table border="0" style="width: 100%; display: none;">
							<tr>
								<td class="LabelBold" style="width: 100px; vertical-align: top;">
									Offending URL
								</td>
								<td class="RowStyle">
									<asp:Literal ID="litURL" runat="server" />
								</td>
							</tr>
							<tr>
								<td class="LabelBold" style="vertical-align: top;">
									Source
								</td>
								<td class="RowStyle">
									<asp:Literal ID="litSource" runat="server" />
								</td>
							</tr>
							<tr>
								<td class="LabelBold" style="vertical-align: top;">
									Message
								</td>
								<td class="RowStyle">
									<asp:Literal ID="litMessage" runat="server" />
								</td>
							</tr>
							<tr>
								<td class="LabelBold" style="vertical-align: top;">
									Inner Message
								</td>
								<td class="RowStyle">
									<asp:Literal ID="litInnerMsg" runat="server" />
								</td>
							</tr>
							<tr>
								<td class="LabelBold" style="vertical-align: top;">
									Stack Trace
								</td>
								<td class="RowStyle">
									<asp:Literal ID="litStackTrace" runat="server" />
								</td>
							</tr>
						</table>
					</asp:Panel>
                    <asp:Label ID="lblError" runat="server" SkinID="TextNormal" />
				</td>
			</tr>
		</table>
	</asp:Panel>
</asp:Content>
