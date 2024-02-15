<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/TASMaster.Master" AutoEventWireup="true" CodeBehind="ReassignmentForm.aspx.cs" 
    Inherits="GARMCO.AMS.TAS.UI.Views.Shared.ReassignmentForm" StylesheetTheme="Standard" %>

<%@ MasterType VirtualPath="~/Views/Shared/TASMaster.Master" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <title>Swipe Correction Request Reassignment</title>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="mainContent" runat="server">
    <telerik:RadFormDecorator ID="formDecor" runat="server" DecoratedControls="Buttons" Skin="Office2010Silver" />
    
    <telerik:RadToolTipManager ID="tooltipMan" runat="server" 
        RelativeTo="Element" AnimationDuration="1"
        Width="190px" Height="70px" Position="MiddleRight" 
        OnAjaxUpdate="tooltipMan_AjaxUpdate" ManualClose="True" 
        Animation="Slide" BackColor="#CCFFFF" BorderStyle="Solid" BorderWidth="2px" 
        EnableShadow="True" Font-Bold="True" Font-Names="Tahoma" 
        HideEvent="LeaveTargetAndToolTip" Skin="Sunset" ShowDelay="200">
        <TargetControls>        
            <telerik:ToolTipTargetControl TargetControlID="txtReassignEmpNo" Value="Note: You can enter directly the Employee Number here or search for an employee by clicking the elipsis button." />
            <telerik:ToolTipTargetControl TargetControlID="txtReassignReason" Value="Note: Type here the reason of re-assignment. You can enter up to 500 text characters." />
            <telerik:ToolTipTargetControl TargetControlID="chkReassignSendBack" Value="Note: Tick this checkbox if you want the request to re-assign back to you" />
        </TargetControls>
    </telerik:RadToolTipManager> 

    <table border="0" style="width: 100%; text-align: left; margin-top: 5px; margin-left: 5px;">
        <tr>
            <td colspan="2" style="padding-left: 0px;">
                <table border="0" style="width: 100%; text-align: left;">
                    <tr>
                        <td style="width: 45px; text-align: right; padding-right: 0px;" rowspan="2">
                            <img alt="" src="../../Images/reassign_icon.png" />
                        </td>
                        <td class="PageTitleLabel" style="vertical-align: bottom; padding-left: 5px; width: 500px; font-size: 13pt;">
                            Overtime Approval Reassignment
                        </td>
                        <td />
                    </tr>
                    <tr>
                        <td colspan="2" class="PageDescriptionHeader" style="text-align: left; vertical-align: top; padding-left: 5px; margin: 0px;">
                            Allows the currently assigned person or the System Administrator to reassign an overtime request to other approver
                        </td>
                    </tr>
                </table>
            </td>                
        </tr>
    </table>

    <asp:Panel ID="panValidator" runat="server" BorderStyle="None" Direction="LeftToRight" style="padding-left: 0px; margin-left: 0px;">
        <asp:ValidationSummary ID="valSummaryPrimary" runat="server" CssClass="ValidationError" HeaderText="Please enter or correct the values on the following field(s):" ValidationGroup="valPrimary" />
    </asp:Panel>

    <asp:Panel ID="panReassign" runat="server" Width="100%" style="margin-top: 10px; padding-left: 10px; padding: 0px;">
		<table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
			<tr>
				<td class="LabelBold" style="width: 100px;">
                    <asp:RequiredFieldValidator ID="reqReassignEmpNo" runat="server" ControlToValidate="txtReassignEmpNo" 
                        CssClass="LabelValidationError" Display="Dynamic" ValidationGroup="valPrimary" 
                        ErrorMessage="Re-assign to cannot be empty" 
                        Text="*" ToolTip="Re-assign To cannot be empty" />
                    <asp:CustomValidator ID="cusReassignEmpNo" runat="server" ControlToValidate="txtGeneric" 
                        CssClass="LabelValidationError" Display="Dynamic" 
                        ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                        ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" />
					Reassign To
				</td>
				<td style="width: 350px; padding-right: 0px; padding-left: 5px;">
                    <telerik:RadTextBox ID="txtReassignEmpNo" runat="server" Width="100%"  
                        EmptyMessage="Enter Employee No. here or search by clicking the button beside" 
                        Skin="Windows7" MaxLength="50">
                    </telerik:RadTextBox>
				</td>
				<td style="width: 40px; padding-left: 0px; text-align: left;">
					<asp:Button ID="btnReassignEmpNo" runat="server" CausesValidation="false" 
                        Text="..." ToolTip="Open the Employee Search Page" Width="30px" 
                        onclick="btnReassignEmpNo_Click" />
				</td>
				<td style="text-align: left; width: 200px;">
					<asp:Literal ID="litReassignEmpName" runat="server" Text="- Employee not yet selected -" Visible="false" />
				</td>
                <td />
			</tr>
			<tr>
				<td class="LabelBold" style="vertical-align: top; padding-top: 5px;">
                    <asp:CustomValidator ID="cusReason" runat="server" ControlToValidate="txtGeneric" 
                        CssClass="LabelValidationError" Display="Dynamic" 
                        ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                        ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" />
					Justification
				</td>
				<td style="vertical-align: top; padding-top: 5px; padding-left: 5px; padding-right: 5px;">
                    <asp:TextBox ID="txtReassignReason" runat="server" width="100%" SkinID="TextLeft" MaxLength="500" TextMode="MultiLine" Rows="8" />
				</td>
				<td colspan="2" style="vertical-align: top;">					
                    <asp:TextBox ID="txtGeneric" runat="server" Visible="false" Width="10px"></asp:TextBox> 
				</td>
                <td />
			</tr>
            <tr style="display: none;">
				<td class="LabelBold" style="vertical-align: top; padding-top: 5px;">
                    
				</td>
				<td class="TextNormal" style="vertical-align: top; padding-top: 5px;">
                    <asp:CheckBox ID="chkReassignSendBack" runat="server" Text="Reassign the request to me after?" Visible="true" />
				</td>
				<td />
                <td />
			</tr>
			<tr>
				<td style="text-align: right; vertical-align: middle;">				
                     <asp:CustomValidator ID="cusValButton" runat="server" 
                        ControlToValidate="txtGeneric" CssClass="LabelValidationError" 
                        Display="Dynamic" SetFocusOnError="true" Text="*"                         
                        ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" />
                </td>
				<td colspan="3">
                    <telerik:RadButton ID="btnReassign" runat="server" ToolTip="Re-assign the request to the specified employee"
                        Text="Re-assign" Skin="Office2010Silver" Font-Bold="False" Font-Size="9pt" Width="80px" 
                        CssClass="RadButtonStyle" ValidationGroup="valPrimary" Visible="true" 
                        OnClick="btnReassign_Click">
                    </telerik:RadButton>
                    <telerik:RadButton ID="btnReset" runat="server" ToolTip="Reset the values of the data entry form"
                        Text="Reset" Skin="Office2010Silver" Font-Bold="False" Font-Size="9pt" Width="70px" 
                        CssClass="RadButtonStyle" CausesValidation="false" Visible="true" OnClick="btnReset_Click">
                    </telerik:RadButton>
					<telerik:RadButton ID="btnBack" runat="server" ToolTip="Return to previous page"
                        Text="<< Back" Skin="Office2010Silver" Font-Bold="False" Font-Size="9pt" Width="70px" 
                        CssClass="RadButtonStyle" CausesValidation="false" Visible="true" onclick="btnBack_Click">
                    </telerik:RadButton>
				</td>
                <td />
			</tr>
            <tr style="height: 5px;">
                <td colspan="4" />
                <td />
            </tr>
		</table>
	</asp:Panel>

    <div>
        <asp:HiddenField ID="hdnErrorFlag" runat="server" Value="0" />
        <input type="hidden" id="hidFormAccess" runat="server" value="" />
	    <input type="hidden" id="hidSearchUrl" runat="server" value="" />
        <input type="hidden" id="hidForm" runat="server" value="Service Request Re-assignment" />
	    <input type="hidden" id="hidFormCode" runat="server" value="SMSREASSGN" />
    </div>

    <div>
        <asp:ObjectDataSource ID="objUserFormAccess" runat="server" OldValuesParameterFormatString="" 
            OnSelected="objUserFormAccess_Selected" SelectMethod="GetUserFormAccess" TypeName="GARMCO.Common.DAL.WebCommonSetup.UserFormAccessBLL">
			<SelectParameters>
				<asp:Parameter DefaultValue="1" Name="mode" Type="Int32" />
				<asp:Parameter DefaultValue="273" Name="userFrmFormAppID" Type="Int32" />
				<asp:Parameter DefaultValue="" Name="userFrmFormCode" Type="String" />
				<asp:Parameter Name="userFrmCostCenter" Type="String" />
				<asp:SessionParameter DefaultValue="0" Name="userFrmEmpNo" SessionField="GARMCO_UserID" Type="Int32" />
				<asp:Parameter Name="userFrmEmpName" Type="String" />
				<asp:Parameter Name="sort" Type="String" />
			</SelectParameters>
		</asp:ObjectDataSource>
    </div>

    <div>
        <telerik:RadAjaxManager ID="MyAjaxManager" runat="server">
            <AjaxSettings>
                <telerik:AjaxSetting AjaxControlID="btnReassign">
				    <UpdatedControls>
                        <telerik:AjaxUpdatedControl ControlID="panReassign" LoadingPanelID="loadingPanel" />
					    <telerik:AjaxUpdatedControl ControlID="panValidator" />
				    </UpdatedControls>
			    </telerik:AjaxSetting>
                <telerik:AjaxSetting AjaxControlID="btnReset">
				    <UpdatedControls>
                        <telerik:AjaxUpdatedControl ControlID="panReassign" LoadingPanelID="loadingPanel" />
					    <telerik:AjaxUpdatedControl ControlID="panValidator" />                                            
				    </UpdatedControls>
			    </telerik:AjaxSetting>
                <telerik:AjaxSetting AjaxControlID="btnReassignEmpNo">
				    <UpdatedControls>
                        <telerik:AjaxUpdatedControl ControlID="panReassign" LoadingPanelID="loadingPanel" />
					    <telerik:AjaxUpdatedControl ControlID="panValidator" />                                            
				    </UpdatedControls>
			    </telerik:AjaxSetting>
            </AjaxSettings>
        </telerik:RadAjaxManager>
        <telerik:RadAjaxLoadingPanel ID="loadingPanel" runat="server" Skin="Vista" MinDisplayTime="0" />
    </div>
</asp:Content>
