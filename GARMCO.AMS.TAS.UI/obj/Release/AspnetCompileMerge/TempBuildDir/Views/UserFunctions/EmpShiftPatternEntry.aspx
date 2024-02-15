<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/TASMaster.Master" AutoEventWireup="true" CodeBehind="EmpShiftPatternEntry.aspx.cs" 
    Inherits="GARMCO.AMS.TAS.UI.Views.UserFunctions.EmpShiftPatternEntry" StylesheetTheme="Standard" %>

<%@ MasterType VirtualPath="~/Views/Shared/TASMaster.Master" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <title>View Current Shift Pattern (Employee)</title>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="mainContent" runat="server">
    <telerik:RadFormDecorator ID="formDecor" runat="server" DecoratedControls="Buttons" Skin="Office2010Silver" />

    <table border="0" style="width: 100%; text-align: left; margin-top: 5px; margin-left: 5px;">
        <tr>
            <td colspan="2" style="padding-left: 10px;">
                <table border="0" style="width: 100%; text-align: left;">
                    <tr>
                        <td style="width: 50px; text-align: right; padding-right: 5px;" rowspan="2">
                            <img alt="" src="../../Images/shift_pattern_icon.jpg" />
                        </td>
                        <td id="tdPageTitle" runat="server" class="PageTitleLabel" style="vertical-align: bottom; padding-left: 5px; width: 900px; font-size: 11pt;">
                            View Current Shift Pattern (Employee)
                        </td>
                        <td style="width: 50px;">
                            <asp:LinkButton ID="lnkMoveUp" runat="server" />
                        </td>
                        <td />
                    </tr>
                    <tr>
                        <td class="PageDescriptionHeader" style="text-align: left; vertical-align: top; padding-left: 5px; margin: 0px;">
                            View the current shift pattern of an employee
                        </td>
                        <td />
                        <td />
                    </tr>
                </table>
            </td>                
        </tr>
    </table>

    <asp:Panel ID="panValidator" runat="server" BorderStyle="None" Direction="LeftToRight">
        <asp:ValidationSummary ID="valSummaryPrimary" runat="server" CssClass="ValidationError" HeaderText="Please enter or correct the values on the following field(s):" ValidationGroup="valPrimary" />
    </asp:Panel>

    <asp:Panel ID="panMain" runat="server" style="margin-top: 5px; padding-bottom: 40px;"> 
        <asp:Panel ID="panBody" runat="server" BorderStyle="None" style="padding: 0px; margin: 0px;">
            <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                <tr style="height: 23px;">
                    <td class="LabelBold" style="width: 135px;">
                        Employee Name                 
                    </td>
                    <td class="TextNormal" style="width: 300px;">
                        <asp:Literal ID="litEmpName" runat="server" Text="Not defined" />         
                    </td>
                    <td class="LabelBold" style="width: 130px;">
                        Position
                    </td>
                    <td class="TextNormal" style="width: 300px;">
                        <asp:Literal ID="litPosition" runat="server" Text="Not defined" />                                  
                    </td>
                    <td />
                </tr>
                <tr style="height: 23px;">
                    <td class="LabelBold">
                        <asp:CustomValidator ID="cusValShiftPatCode" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                        Shift Pat. Code               
                    </td>
                    <td class="TextNormal">
                        <telerik:RadComboBox ID="cboShiftPatCode" runat="server"
                            DropDownWidth="350px" 
                            HighlightTemplatedItems="True" 
                            MarkFirstMatch="false" 
                            Skin="Office2010Silver" 
                            Width="100%" 
                            EmptyMessage="Select Shift Pattern"
                            EnableLoadOnDemand="false"
                            EnableVirtualScrolling="true"
                            AutoPostBack="True" 
                            OnSelectedIndexChanged="cboShiftPatCode_SelectedIndexChanged"
                            BackColor="Yellow"
                            Height="180px" />
                    </td>
                    <td class="LabelBold">
                        Last Update User
                    </td>
                    <td class="TextNormal">
                        <asp:Literal ID="litUpdateUser" runat="server" Text="Not defined" />
                    </td>
                    <td />
                </tr>
                <tr style="height: 23px;">
                    <td class="LabelBold" >
                        <asp:CustomValidator ID="cusValShiftPointer" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                        Shift Pointer
                    </td>
                    <td class="TextNormal"  style="padding-left: 0px;">
                        <telerik:RadComboBox ID="cboShiftPointer" runat="server"
                            DropDownWidth="125px" 
                            HighlightTemplatedItems="True" 
                            MarkFirstMatch="false" 
                            Skin="Office2010Silver" 
                            Width="130px" 
                            EmptyMessage="Select Shift Pointer"
                            EnableLoadOnDemand="false"
                            EnableVirtualScrolling="true"
                            BackColor="Yellow" />
                    </td>
                    <td class="LabelBold">
                        Last Update Time
                    </td>
                    <td class="TextNormal">
                        <asp:Literal ID="litLastUpdateTime" runat="server" Text="Not defined" />
                    </td>
                    <td />
                </tr>
                <tr style="height: 23px;">
                    <td class="LabelBold">
                        Working Cost Center
                    </td>
                    <td class="TextNormal"  style="padding-left: 0px;">
                        <asp:Literal ID="litCostCenter" runat="server" Text="Not defined" />                                  
                    </td>
                    <td class="LabelBold">
                        
                    </td>
                    <td class="TextNormal">
                        
                    </td>
                    <td />
                </tr>
            </table>
        </asp:Panel>  

        <asp:Panel ID="panButton" runat="server" BorderStyle="None" style="padding-left: 0px; padding-bottom: 30px; margin-top: 0px;">
            <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                <tr style="height: 23px;">
                    <td class="LabelBold" style="width: 132px;">
                        <asp:CustomValidator ID="cusValButton" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                    </td>
                    <td style="width: 750px;">                        
                        <telerik:RadButton ID="btnSave" runat="server" ToolTip="Save data" Width="70px"
                            Text="Save" Skin="Office2010Silver" Font-Bold="False" Font-Size="9pt" 
                            CssClass="RadButtonStyle" ValidationGroup="valPrimary"
                            OnClick="btnSave_Click" Enabled="False" Visible="False">
                        </telerik:RadButton>
                        <telerik:RadButton ID="btnDelete" runat="server" ToolTip="Delete record"
                            Text="Delete" Skin="Office2010Silver" Font-Bold="False" Font-Size="9pt" 
                            CssClass="RadButtonStyle" ValidationGroup="valPrimary" Width="70px"
                            OnClick="btnDelete_Click" Enabled="False" Visible="False">
                        </telerik:RadButton>                                                 
                        <telerik:RadButton ID="btnReset" runat="server" ToolTip="Clear data entry form"
                            Text="Reset" Skin="Office2010Silver" Font-Bold="False" Font-Size="9pt"
                            CssClass="RadButtonStyle" CausesValidation="false" Width="70px"
                            OnClick="btnReset_Click" Enabled="False" Visible="False">
                        </telerik:RadButton> 
                        <telerik:RadButton ID="btnBack" runat="server" ToolTip="Go back to previous page"
                            Text="<< Back" Skin="Office2010Silver" Font-Bold="False" Font-Size="9pt" Width="70px"
                            CssClass="RadButtonStyle" CausesValidation="false" OnClick="btnBack_Click">
                        </telerik:RadButton>  
                    </td>
                    <td />
                </tr>                                
            </table>    
        </asp:Panel>  
    </asp:Panel>        

    <asp:Panel ID="panHidden" runat="server" style="display: none;">
        <input type="hidden" id="hidFormAccess" runat="server" value="" />
        <input type="hidden" id="hidFormCode" runat="server" value="" />
        <input type="hidden" id="hidForm" runat="server" value="" />
        <input type="hidden" id="hidSearchUrl" runat="server" value="" />
        <input type="hidden" id="hidRequestFlag" runat="server" value="0" />     
        <asp:TextBox ID="txtGeneric" runat="server" Width="100%" Visible="false" />    
        <telerik:RadButton ID="btnDeleteDummy" runat="server" Text="" Skin="Office2010Silver" CssClass="HideButton" ValidationGroup="valPrimary" onclick="btnDeleteDummy_Click" />   
        <telerik:RadButton ID="btnRebind" runat="server" Text="" Skin="Office2010Silver" CausesValidation="false" onclick="btnRebind_Click" />
    </asp:Panel>

    <asp:Panel ID="panAjaxManager" runat="server">
        <telerik:RadAjaxManager ID="MyAjaxManager" runat="server">
            <AjaxSettings>
                <telerik:AjaxSetting AjaxControlID="btnSave">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>     
                <telerik:AjaxSetting AjaxControlID="btnDelete">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>  
                <telerik:AjaxSetting AjaxControlID="btnReset">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting> 
                <telerik:AjaxSetting AjaxControlID="btnBack">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting> 
                <telerik:AjaxSetting AjaxControlID="btnGet">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting> 
                <telerik:AjaxSetting AjaxControlID="btnFindEmployee">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>                 
                <telerik:AjaxSetting AjaxControlID="btnDeleteDummy">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>     
                <telerik:AjaxSetting AjaxControlID="btnRebind">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>    
                <telerik:AjaxSetting AjaxControlID="cboShiftPatCode">
				    <UpdatedControls>
                        <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>     
                <telerik:AjaxSetting AjaxControlID="cboChangeType">
				    <UpdatedControls>
                        <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>
            </AjaxSettings>
        </telerik:RadAjaxManager>
        <telerik:RadAjaxLoadingPanel ID="loadingPanel" runat="server" Skin="Office2010Silver"></telerik:RadAjaxLoadingPanel>
    </asp:Panel>
</asp:Content>
