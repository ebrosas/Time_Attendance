<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/TASMaster.Master" AutoEventWireup="true" CodeBehind="ErrorMessage.aspx.cs" 
    Inherits="GARMCO.AMS.TAS.UI.Views.Shared.ErrorMessage" StylesheetTheme="Standard" %>

<%@ MasterType VirtualPath="~/Views/Shared/TASMaster.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <title>Error Page</title>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="mainContent" runat="server">
    <asp:Panel ID="panError" runat="server" style="margin-left: auto; margin-right: auto; vertical-align: middle; width: 900px;">
		<br /><br /><br /><br /><br /><br /> 
		<table border="0" style="vertical-align: top; width: 100%">
			<tr>
                <td style="vertical-align: top; text-align: left; width: 100px;">
					<asp:Image ID="imgError" runat="server" ImageUrl="~/Images/error.png" />
				</td>
				<td style="width: 800px; text-align: left;">
					<asp:Panel ID="panStackError" runat="server" Visible="false" Width="100%">
						<br />We apologize, an error occurred while processing your request.<br /><br />
						<table border="0" style="width: 100%;">
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
                    <br /> 
                    <br />
                    <asp:LinkButton ID="lnkHome" runat="server" Text="Click here to open the Homepage." Font-Bold="true" ForeColor="Blue"
                        style="padding-left: 0px; padding-top: 0px;" OnClick="lnkHome_Click" Visible="false" />
				</td>
			</tr>
		</table>
	</asp:Panel>
</asp:Content>

