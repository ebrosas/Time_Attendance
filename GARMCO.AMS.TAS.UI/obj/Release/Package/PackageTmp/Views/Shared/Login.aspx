<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/TASMasterNoMenu.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" 
    Inherits="GARMCO.AMS.TAS.UI.Views.Shared.Login" StylesheetTheme="Standard" %>

<%@ MasterType VirtualPath="~/Views/Shared/TASMasterNoMenu.Master" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <title>Login Page</title>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="mainContent" runat="server">
    <telerik:RadFormDecorator ID="formDecor" runat="server" Skin="Office2010Silver" />

    <table border="0" style="width: 100%; text-align: left; margin-top: 5px; margin-left: 0px;">
        <tr>
            <td colspan="2" style="padding-left: 10px;">
                <table border="0" style="width: 100%; text-align: left;">
                    <tr>
                        <td style="width: 50px; text-align: right; padding-right: 5px;" rowspan="2">
                            <img alt="" src="../../Images/login_icon.png" />
                        </td>
                        <td class="PageTitleLabel" style="vertical-align: bottom; padding-left: 5px; width: 550px; font-size: 12pt;">
                            System Login
                        </td>
                        <td />
                    </tr>
                    <tr>
                        <td class="ServiceDescriptionHeader" style="text-align: left; vertical-align: top; padding-left: 5px; margin: 0px; font-size: 9pt">
                            Allows user to login to the system using their Windows account credentials
                        </td>
                        <td />
                    </tr>
                </table>
            </td>                
        </tr>
    </table>

    <asp:Panel ID="panMain" runat="server" style="width: 100%; margin-top: 0px;">
        <asp:Panel ID="panValidators" runat="server" BorderStyle="None" Direction="LeftToRight">
            <asp:ValidationSummary ID="valSummaryPrimary" runat="server" CssClass="ValidationError" HeaderText="Please enter or correct the values on the following field(s):" ValidationGroup="valPrimary" />
        </asp:Panel>

        <asp:Panel ID="panBody" runat="server" CssClass="GroupPanelWorkflowActivity" GroupingText="Please provide your login information:" style="width: 100%;">
            <table border="0" style="width: 100%; table-layout: fixed;">
                <tr style="height: 28px;">
                    <td class="LabelBold" style="width: 100px;">
                        Login Options
                    </td>
                    <td style="width: 250px;">
                        <asp:RadioButtonList ID="rblLoginOption" runat="server" Width="100%" 
                            RepeatDirection="Horizontal" AutoPostBack="True" 
                            onselectedindexchanged="rblLoginOption_SelectedIndexChanged">
                            <asp:ListItem Value="valUsername" Selected="True">By Username</asp:ListItem>
                            <asp:ListItem Value="valEmployeeNo">By Employee No.</asp:ListItem>
                        </asp:RadioButtonList> 
                    </td>
                    <td style="width: 30px;" />
                    <td />
                </tr>
                <tr style="height: 28px;">
                    <td class="LabelBold" id="tdUsername" runat="server">                                               
                        Username
                    </td>
                    <td>
                        <telerik:RadTextBox ID="txtUsername" runat="server" Width="250px" 
                            EmptyMessage="Enter Username (Ex. ervin)" Skin="Windows7" 
                            Font-Size="9pt" Font-Names="Tahoma" MaxLength="50" TabIndex="1" />
                    </td>
                    <td>
                        <asp:CustomValidator ID="cusValUser" runat="server" 
                            ControlToValidate="txtGeneric" CssClass="LabelValidationError" 
                            Display="Dynamic" SetFocusOnError="true" Text="*" 
                            ValidationGroup="valPrimary" 
                            onservervalidate="cusGeneric_ServerValidate" />
                    </td>
                    <td />
                </tr>
                <tr style="height: 28px;">
                    <td id="tdPassword" runat="server" class="LabelBold" style="vertical-align: top; padding-top: 5px;">                        
                        Password
                    </td>
                    <td>
                        <telerik:RadTextBox ID="txtPassword" runat="server" Width="250px" 
                            Skin="Windows7" Font-Size="9pt" Font-Names="Tahoma" MaxLength="50" 
                            TextMode="Password" TabIndex="2" />
                    </td>
                    <td>
                        <asp:CustomValidator ID="cusValPwd" runat="server" 
                            ControlToValidate="txtGeneric" CssClass="LabelValidationError" 
                            Display="Dynamic" SetFocusOnError="true" Text="*" 
                            ValidationGroup="valPrimary" 
                            onservervalidate="cusGeneric_ServerValidate" />
                    </td>
                    <td />
                </tr>
            </table>

            <table border="0" style="width: 100%; table-layout: fixed;">
                <tr style="height: 28px;">
                    <td style="width: 100px;" />
                    <td style="padding-left: 10px; width: 390px;">
                        <asp:Panel ID="panButtons" runat="server" style="width: 100%; margin-left: 0px; margin-top: 0px; margin-bottom: 0px;">
                            <asp:CustomValidator ID="cusValButtons" runat="server" 
                                ControlToValidate="txtGeneric" CssClass="LabelValidationError" 
                                Display="Dynamic" SetFocusOnError="true" Text="*" 
                                ValidationGroup="valPrimary" onservervalidate="cusGeneric_ServerValidate" />
                            <telerik:RadButton ID="btnLogin" runat="server" ToolTip="Validate the credentials supplied"
                                Text="Login" Skin="Office2010Silver" Font-Bold="False" Font-Size="9pt" Width="70px" 
                                CssClass="RadButtonStyle" ValidationGroup="valPrimary" Enabled="true"
                                OnClick="btnLogin_Click" TabIndex="3">
                            </telerik:RadButton>
                            <telerik:RadButton ID="btnReset" runat="server" ToolTip="Reset the values of the data entry form"
                                Text="Reset" Skin="Office2010Silver" Font-Bold="False" Font-Size="9pt" Width="70px" 
                                CssClass="RadButtonStyle" CausesValidation="false" Enabled="true" 
                                OnClick="btnReset_Click" TabIndex="4">
                            </telerik:RadButton> 
                            <telerik:RadButton ID="btnBack" runat="server" Text="Cancel" ToolTip="Go back to previous page" 
                                Skin="Office2010Silver" Font-Bold="False" Font-Size="9pt" Width="70px" 
                                CausesValidation="false" CssClass="RadButtonStyle" onclick="btnBack_Click" 
                                TabIndex="5">                       
                            </telerik:RadButton>                               
                        </asp:Panel>
                    </td>
                    <td />
                </tr>
            </table>            
        </asp:Panel>
    </asp:Panel>

    <div style="display: none;">
        <asp:TextBox ID="txtGeneric" runat="server" Width="10px"></asp:TextBox>
    </div>

    <asp:Panel ID="panAjaxManager" runat="server">
        <telerik:RadAjaxManager ID="MainAjaxManager" runat="server">
            <AjaxSettings>
                <telerik:AjaxSetting AjaxControlID="btnLogin">
				    <UpdatedControls>
					    <telerik:AjaxUpdatedControl ControlID="panBody" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidators" />   
				    </UpdatedControls>
			    </telerik:AjaxSetting>
                <telerik:AjaxSetting AjaxControlID="btnReset">
				    <UpdatedControls>
					    <telerik:AjaxUpdatedControl ControlID="panBody" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidators" />   
				    </UpdatedControls>
			    </telerik:AjaxSetting>
                <telerik:AjaxSetting AjaxControlID="btnBack">
				    <UpdatedControls>
					    <telerik:AjaxUpdatedControl ControlID="panBody" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidators" />   
				    </UpdatedControls>
			    </telerik:AjaxSetting>     
                <telerik:AjaxSetting AjaxControlID="rblLoginOption">
				    <UpdatedControls>
					    <telerik:AjaxUpdatedControl ControlID="panBody" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidators" />   
				    </UpdatedControls>
			    </telerik:AjaxSetting>                                 
            </AjaxSettings>
        </telerik:RadAjaxManager>
        <telerik:RadAjaxLoadingPanel ID="loadingPanel" runat="server" Skin="Office2010Silver" MinDisplayTime="0" />
    </asp:Panel>
</asp:Content>
