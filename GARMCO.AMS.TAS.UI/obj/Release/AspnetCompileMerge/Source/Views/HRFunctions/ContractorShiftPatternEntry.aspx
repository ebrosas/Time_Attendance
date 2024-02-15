<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/TASMaster.Master" AutoEventWireup="true" CodeBehind="ContractorShiftPatternEntry.aspx.cs" 
    Inherits="GARMCO.AMS.TAS.UI.Views.HRFunctions.ContractorShiftPatternEntry" StylesheetTheme="Standard" %>

<%@ MasterType VirtualPath="~/Views/Shared/TASMaster.Master" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <title>Contractor's Shift Pattern Entry</title>
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
                        <td id="tdPageTitle" runat="server"  class="PageTitleLabel" style="vertical-align: bottom; padding-left: 5px; width: 900px; font-size: 11pt;">
                            Contractor's Shift Pattern Entry
                        </td>
                        <td style="width: 50px;">
                            <asp:LinkButton ID="lnkMoveUp" runat="server" />
                        </td>
                        <td />
                    </tr>
                    <tr>
                        <td class="PageDescriptionHeader" style="text-align: left; vertical-align: top; padding-left: 5px; margin: 0px;">
                            Create or update the contractor's shift pattern information
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
        <asp:Panel ID="panSearchCriteria" runat="server" BorderStyle="None" style="padding: 0px; margin: 0px;">
            <asp:Panel ID="panEmployee" runat="server" BorderStyle="None" style="padding: 0px; margin: 0px;">
                <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                    <tr style="height: 23px;">
                        <td class="LabelBold" style="width: 130px;">
                            <asp:CustomValidator ID="cusValContractorNo" runat="server" ControlToValidate="txtGeneric" 
                                CssClass="LabelValidationError" Display="Dynamic" 
                                ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                                ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                            Contractor No.                 
                        </td>
                        <td style="width: 250px;">
                            <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                                <tr style="vertical-align: top; margin: 0px; padding: 0px;">
                                    <td style="width: 130px; text-align: left;">
                                        <telerik:RadNumericTextBox ID="txtContractorNo" runat="server" width="130px" 
                                            MinValue="0" ToolTip="(Note: Maximum number input lenght is 8 digits)" 
                                            Skin="Office2010Silver" DataType="System.Int32" MaxLength="8" MaxValue="99999999" 
                                            EmptyMessage="" BackColor="Yellow" >
                                            <NumberFormat ZeroPattern="n" DecimalDigits="0" GroupSeparator="" />
                                        </telerik:RadNumericTextBox> 
                                    </td>
                                    <td style="width: 40px; text-align: left; padding-left: 3px; padding-top: 0px; vertical-align: top; display: none;">
                                        <telerik:RadButton ID="btnGet" runat="server" Skin="Office2010Silver" Width="100%" 
                                            Text="Get" ToolTip="Get employee info based on entered Employee No." Enabled="true" 
                                            Font-Bold="False" Font-Size="9pt" CausesValidation="false"
                                            onclick="btnGet_Click">
                                        </telerik:RadButton>
                                    </td> 
                                    <td style="text-align: left; width: 30px; padding-left: 3px; padding-top: 0px; vertical-align: top; display: none;">
                                        <telerik:RadButton ID="btnFindEmployee" runat="server" Skin="Office2010Silver" 
                                            Text="..." ToolTip="Click here to search for an employee." Enabled="true" 
                                            Width="100%" Font-Bold="False" Font-Size="9pt" CausesValidation="false"
                                            onclick="btnFindEmployee_Click">
                                        </telerik:RadButton>
                                    </td> 
                                    <td />
                                </tr>
                            </table>
                        </td>
                        <td class="LabelBold" style="width: 140px;">
                            Contractor Company
                        </td>
                        <td class="TextNormal" style="width: 300px;">
                            <telerik:RadComboBox ID="cboContractorCompany" runat="server"
                                DropDownWidth="350px" 
                                HighlightTemplatedItems="true" 
                                MarkFirstMatch="True" 
                                AllowCustomText="false"
                                Skin="Office2010Silver" 
                                Width="100%" 
                                Height="150px"
                                EmptyMessage="Select Contractor Company"
                                EnableLoadOnDemand="true"
                                EnableVirtualScrolling="true" />
                        </td>
                        <td />
                    </tr>
                    <tr style="height: 23px;">
                        <td class="LabelBold">
                            <asp:CustomValidator ID="cusValContractorName" runat="server" ControlToValidate="txtGeneric" 
                                CssClass="LabelValidationError" Display="Dynamic" 
                                ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                                ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                            Contractor Name                 
                        </td>
                        <td class="TextNormal">
                            <telerik:RadTextBox ID="txtContractorName" runat="server" Width="100%" 
                                EmptyMessage="Enter Contractor Name" Skin="Office2010Silver" ToolTip="Maximum text input is 40 chars." 
                                Font-Names="Tahoma" Font-Size="9pt" MaxLength="40" />
                        </td>
                        <td class="LabelBold">
                            Group Type
                        </td>
                        <td class="TextNormal">
                            <telerik:RadComboBox ID="cboGroupType" runat="server"
                                DropDownWidth="140px" 
                                HighlightTemplatedItems="True" 
                                Skin="Office2010Silver" 
                                Width="150px" 
                                EmptyMessage="Select Group Type"
                                EnableVirtualScrolling="True" >
                            </telerik:RadComboBox> 
                        </td>
                        <td />
                    </tr>
                    <tr style="height: 23px;">
                        <td class="LabelBold">
                            <asp:CustomValidator ID="cusValDateStarted" runat="server" ControlToValidate="txtGeneric" 
                                CssClass="LabelValidationError" Display="Dynamic" 
                                ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                                ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                            Date Started              
                        </td>
                        <td class="TextNormal">
                             <telerik:RadDatePicker ID="dtpDateStarted" runat="server"
                            Width="130px" Skin="Office2010Silver" Culture="en-US" Enabled="False">
                            <Calendar ID="Calendar1" runat="server" Skin="Office2010Silver" UseColumnHeadersAsSelectors="False" 
                                UseRowHeadersAsSelectors="False" ViewSelectorText="x">
                            </Calendar>
                            <DateInput ID="DateInput1" runat="server" DateFormat="d/M/yyyy" DisplayDateFormat="d/M/yyyy" BackColor="Yellow">
                                <EmptyMessageStyle Resize="None" />
                                <ReadOnlyStyle Resize="None" />
                                <FocusedStyle Resize="None" />
                                <DisabledStyle Resize="None" />
                                <InvalidStyle Resize="None" />
                                <HoveredStyle Resize="None" />
                                <EnabledStyle Resize="None" />
                            </DateInput>
                            <DatePopupButton HoverImageUrl="" ImageUrl="" />
                        </telerik:RadDatePicker>      
                        </td>
                        <td class="LabelBold">
                            Religion
                        </td>
                        <td class="TextNormal">
                            <telerik:RadComboBox ID="cboReligion" runat="server"
                                DropDownWidth="140px" 
                                HighlightTemplatedItems="True" 
                                Skin="Office2010Silver" 
                                Width="150px" 
                                EmptyMessage="Select Religion"
                                EnableVirtualScrolling="True" >
                            </telerik:RadComboBox> 
                        </td>
                        <td />
                    </tr>
                    <tr style="height: 23px;">
                        <td class="LabelBold">
                            <asp:CustomValidator ID="cusValExpirationDate" runat="server" ControlToValidate="txtGeneric" 
                                CssClass="LabelValidationError" Display="Dynamic" 
                                ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                                ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                            Expiration Date
                        </td>
                        <td class="TextNormal">
                             <telerik:RadDatePicker ID="dtpExpirationDate" runat="server"
                            Width="130px" Skin="Office2010Silver" Culture="en-US" Enabled="False">
                            <Calendar ID="Calendar3" runat="server" Skin="Office2010Silver" UseColumnHeadersAsSelectors="False" 
                                UseRowHeadersAsSelectors="False" ViewSelectorText="x">
                            </Calendar>
                            <DateInput ID="DateInput3" runat="server" DateFormat="d/M/yyyy" DisplayDateFormat="d/M/yyyy">
                                <EmptyMessageStyle Resize="None" />
                                <ReadOnlyStyle Resize="None" />
                                <FocusedStyle Resize="None" />
                                <DisabledStyle Resize="None" />
                                <InvalidStyle Resize="None" />
                                <HoveredStyle Resize="None" />
                                <EnabledStyle Resize="None" />
                            </DateInput>
                            <DatePopupButton HoverImageUrl="" ImageUrl="" />
                        </telerik:RadDatePicker>      
                        </td>
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
                        <td />
                    </tr>
                    <tr style="height: 23px;">
                        <td class="LabelBold">
                            Last Update Date
                        </td>
                        <td class="TextNormal">
                            <asp:Literal ID="litLastUpdateTime" runat="server" Text="Not defined" />   
                        </td>
                        <td class="LabelBold">
                            <asp:CustomValidator ID="cusValShiftPointer" runat="server" ControlToValidate="txtGeneric" 
                                CssClass="LabelValidationError" Display="Dynamic" 
                                ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                                ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                            Shift Pointer
                        </td>
                        <td class="TextNormal">
                             <telerik:RadComboBox ID="cboShiftPointer" runat="server"
                                DropDownWidth="140px" 
                                HighlightTemplatedItems="True" 
                                MarkFirstMatch="false" 
                                Skin="Office2010Silver" 
                                Width="150px" 
                                EmptyMessage="Select Shift Pointer"
                                EnableLoadOnDemand="false"
                                EnableVirtualScrolling="true"
                                BackColor="Yellow" />
                        </td>
                        <td />
                    </tr>
                    <tr style="height: 23px;">
                        <td class="LabelBold">
                            Last Update User
                        </td>
                        <td class="TextNormal">
                            <asp:Literal ID="litUpdateUser" runat="server" Text="Not defined" />
                        </td>
                        <td class="LabelBold">
                            
                        </td>
                        <td class="TextNormal">
                            
                        </td>
                        <td />
                    </tr>
                </table>
            </asp:Panel>
        </asp:Panel>

        <asp:Panel ID="panButton" runat="server" BorderStyle="None" style="padding-left: 3px; padding-bottom: 30px; margin-top: 5px;">
            <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                <tr style="height: 23px;">
                    <td class="LabelBold" style="width: 125px;">
                        <asp:CustomValidator ID="cusValButton" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                    </td>
                    <td style="width: 750px;">                        
                        <telerik:RadButton ID="btnSave" runat="server" ToolTip="Save data" Width="80px"
                            Text="Save" Skin="Office2010Silver" Font-Bold="False" Font-Size="9pt" 
                            CssClass="RadButtonStyle" ValidationGroup="valPrimary"
                            OnClick="btnSave_Click" Enabled="False">
                        </telerik:RadButton>
                        <telerik:RadButton ID="btnDelete" runat="server" ToolTip="Delete record"
                            Text="Delete" Skin="Office2010Silver" Font-Bold="False" Font-Size="9pt" 
                            CssClass="RadButtonStyle" ValidationGroup="valPrimary" Width="80px"
                            OnClick="btnDelete_Click" Enabled="False">
                        </telerik:RadButton>                                                 
                        <telerik:RadButton ID="btnReset" runat="server" ToolTip="Clear data entry form"
                            Text="Reset" Skin="Office2010Silver" Font-Bold="False" Font-Size="9pt"
                            CssClass="RadButtonStyle" CausesValidation="false" Width="80px"
                            OnClick="btnReset_Click" Enabled="False">
                        </telerik:RadButton> 
                        <telerik:RadButton ID="btnBack" runat="server" ToolTip="Go back to previous page"
                            Text="<< Back" Skin="Office2010Silver" Font-Bold="False" Font-Size="9pt" Width="80px"
                            CssClass="RadButtonStyle" CausesValidation="false" OnClick="btnBack_Click">
                        </telerik:RadButton>  
                    </td>
                    <td />
                </tr>                                
            </table>    
            
            <table border="0" style="width: 100%; text-align: left; margin-top: 0px; table-layout: fixed;">
                <tr>
                    <td class="LabelBold" style="text-align: left; padding-left: 15px; color: silver;">
                        NOTES:
                    </td>
                </tr>
                <tr>
                    <td class="LabelNotes" style="text-align: left; color: silver; font-style: normal; padding-left: 20px;">
                        - The following fields are mandatory: 1) Contractor No.; 2) Contractor Name; 3) Date Started; 4) Shift Pat. Code; 5) Shift Pointer
                    </td>
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
            </AjaxSettings>
        </telerik:RadAjaxManager>
        <telerik:RadAjaxLoadingPanel ID="loadingPanel" runat="server" Skin="Office2010Silver"></telerik:RadAjaxLoadingPanel>
    </asp:Panel>
</asp:Content>
