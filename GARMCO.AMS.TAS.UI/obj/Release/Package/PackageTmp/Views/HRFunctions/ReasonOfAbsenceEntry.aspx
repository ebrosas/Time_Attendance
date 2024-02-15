<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/TASMaster.Master" AutoEventWireup="true" CodeBehind="ReasonOfAbsenceEntry.aspx.cs" 
    Inherits="GARMCO.AMS.TAS.UI.Views.HRFunctions.ReasonOfAbsenceEntry" StylesheetTheme="Standard" %>

<%@ MasterType VirtualPath="~/Views/Shared/TASMaster.Master" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <title>Reason of Absence Entry</title>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="mainContent" runat="server">
    <telerik:RadFormDecorator ID="formDecor" runat="server" DecoratedControls="Buttons" Skin="Office2010Silver" />

    <table border="0" style="width: 100%; text-align: left; margin-top: 5px; margin-left: 5px;">
        <tr>
            <td colspan="2" style="padding-left: 10px;">
                <table border="0" style="width: 100%; text-align: left;">
                    <tr>
                        <td style="width: 50px; text-align: right; padding-right: 5px;" rowspan="2">
                            <img alt="" src="../../Images/manual_timesheet_icon.jpg" />
                        </td>
                        <td id="tdPageTitle" runat="server"  class="PageTitleLabel" style="vertical-align: bottom; padding-left: 5px; width: 900px; font-size: 11pt;">
                            Reason of Absence Entry
                        </td>
                        <td style="width: 50px;">
                            <asp:LinkButton ID="lnkMoveUp" runat="server" />
                        </td>
                        <td />
                    </tr>
                    <tr>
                        <td class="PageDescriptionHeader" style="text-align: left; vertical-align: top; padding-left: 5px; margin: 0px;">
                            Setup the reason of absence for an employee who goes on training, business trip, sick leave and other reasons
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
                    <td class="LabelBold" style="width: 150px;">
                        <asp:CustomValidator ID="cusValEmpNo" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                        Employee No.                 
                    </td>
                    <td style="width: 230px;">
                        <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                            <tr style="vertical-align: top; margin: 0px; padding: 0px;">
                                <td style="width: 110px; text-align: left;">
                                    <telerik:RadNumericTextBox ID="txtEmpNo" runat="server" width="100%" 
                                        MinValue="0" ToolTip="(Note: Employee No. must start with 1000. Example: 10003632)" 
                                        Skin="Office2010Silver" DataType="System.Int32" MaxLength="8" MaxValue="99999999" 
                                        EmptyMessage="1000xxxx" BackColor="Yellow" AutoPostBack="True" OnTextChanged="txtEmpNo_TextChanged" >
                                        <NumberFormat ZeroPattern="n" DecimalDigits="0" GroupSeparator="" />
                                    </telerik:RadNumericTextBox> 
                                </td>
                                <td style="width: 40px; text-align: left; padding-left: 0px; display: none;">
                                    <telerik:RadButton ID="btnGet" runat="server" Skin="Office2010Silver" Width="100%" 
                                        Text="Get" ToolTip="Get employee info based on entered Employee No." Enabled="true" 
                                        Font-Bold="False" Font-Size="9pt" CausesValidation="false"
                                        onclick="btnGet_Click">
                                    </telerik:RadButton>
                                </td> 
                                <td style="text-align: left; width: 30px; padding-left: 5px;">
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
                    <td class="LabelBold" style="width: 120px;">
                        Position
                    </td>
                    <td class="TextNormal" style="width: 300px;">
                        <asp:Literal ID="litPosition" runat="server" Text="Not defined" />                                  
                    </td>
                    <td />
                </tr>
                <tr style="height: 23px;">
                    <td class="LabelBold">
                        Employee Name                 
                    </td>
                    <td class="TextNormal">
                        <asp:Literal ID="litEmpName" runat="server" Text="Not defined" />         
                    </td>
                    <td class="LabelBold">
                        Cost Center
                    </td>
                    <td class="TextNormal">
                        <asp:Literal ID="litCostCenter" runat="server" Text="Not defined" />                                  
                    </td>
                    <td />
                </tr>
                <tr style="height: 23px;">
                    <td class="LabelBold">
                        <asp:CustomValidator ID="cusValEffectiveDate" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                        Effective Date
                    </td>
                    <td class="TextNormal">
                        <telerik:RadDatePicker ID="dtpEffectiveDate" runat="server" ToolTip="Date Format: dd/mm/yyyy"
                            Width="120px" Skin="Office2010Silver" Culture="en-US" Enabled="False">
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
                        <asp:CustomValidator ID="cusValStartTime" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                        Start Time
                    </td>
                    <td class="TextNormal">
                        <telerik:RadDateInput ID="dtpStartTime" runat="server" LabelWidth="50px" Width="80px" Culture="en-US" EmptyMessage="HH:mm:ss" ToolTip="Time Format: HH:mm:ss"
                            InvalidStyleDuration="100" DateFormat="HH:mm:ss" Enabled="False">
                        </telerik:RadDateInput> 
                    </td>
                    <td />
                </tr>
                <tr style="height: 23px;">
                    <td class="LabelBold">
                        <asp:CustomValidator ID="cusValEndingDate" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                        Ending Date
                    </td>
                    <td class="TextNormal">
                        <telerik:RadDatePicker ID="dtpEndingDate" runat="server" ToolTip="Date Format: dd/mm/yyyy"
                            Width="120px" Skin="Office2010Silver" Culture="en-US" Enabled="False">
                            <Calendar ID="Calendar3" runat="server" Skin="Office2010Silver" UseColumnHeadersAsSelectors="False" 
                                UseRowHeadersAsSelectors="False" ViewSelectorText="x">
                            </Calendar>
                            <DateInput ID="DateInput3" runat="server" DateFormat="d/M/yyyy" DisplayDateFormat="d/M/yyyy" BackColor="Yellow">
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
                        <asp:CustomValidator ID="cusValEndTime" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                        End Time
                    </td>
                    <td class="TextNormal">
                        <telerik:RadDateInput ID="dtpEndTime" runat="server" LabelWidth="50px" Width="80px" Culture="en-US" EmptyMessage="HH:mm:ss" ToolTip="Time Format: HH:mm:ss"
                            InvalidStyleDuration="100" DateFormat="HH:mm:ss" Enabled="False">
                        </telerik:RadDateInput> 
                    </td>
                    <td />
                </tr>
                <tr style="height: 23px;">
                    <td class="LabelBold">
                        <asp:CustomValidator ID="cusValAbsenceReasonCode" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                        Absence Reason Code
                    </td>
                    <td class="TextNormal"  style="padding-left: 0px;">
                        <telerik:RadComboBox ID="cboAbsenceReason" runat="server"
                            DropDownWidth="300px" 
                            HighlightTemplatedItems="True" 
                            Skin="Office2010Silver" 
                            Width="100%" 
                            Height="150px"
                            EmptyMessage="Select Absence Reason"
                            EnableVirtualScrolling="True" BackColor="Yellow" >
                        </telerik:RadComboBox>
                    </td>
                    <td class="LabelBold">
                        Day of Week
                    </td>
                    <td class="TextNormal">
                        <telerik:RadComboBox ID="cboDayOfWeek" runat="server"
                            DropDownWidth="120px" 
                            HighlightTemplatedItems="True" 
                            Skin="Office2010Silver" 
                            Width="130px" 
                            EmptyMessage="Select Day of Week"
                            EnableVirtualScrolling="True" >
                            <Items>
                                <telerik:RadComboBoxItem runat="server" Text="Sunday" Value="SUN" />
                                <telerik:RadComboBoxItem runat="server" Text="Monday" Value="MON" />
                                <telerik:RadComboBoxItem runat="server" Text="Tuesday" Value="TUE" />
                                <telerik:RadComboBoxItem runat="server" Text="Wednesday" Value="WED" />
                                <telerik:RadComboBoxItem runat="server" Text="Thursday" Value="THU" />
                                <telerik:RadComboBoxItem runat="server" Text="Friday" Value="FRI" />
                                <telerik:RadComboBoxItem runat="server" Text="Saturday" Value="SAT" />
                            </Items>
                        </telerik:RadComboBox>
                    </td>
                    <td />
                </tr>
                <tr style="height: 23px;">
                    <td class="LabelBold">
                       XID DIL Entitled
                    </td>
                    <td class="TextNormal"  style="padding-left: 0px;">
                        <asp:Literal ID="litXIDDILEntitled" runat="server" Text="Not defined" />
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
                    <td class="LabelBold">
                        XID DIL Used                        
                    </td>
                    <td class="TextNormal"  style="padding-left: 0px;">
                        <asp:Literal ID="litXIDDILUsed" runat="server" Text="Not defined" />                        
                    </td>
                    <td class="LabelBold">
                        Last Update Time
                    </td>
                    <td class="TextNormal">
                        <asp:Literal ID="litLastUpdateTime" runat="server" Text="Not defined" />
                    </td>
                    <td />
                </tr>
            </table>                                  
        </asp:Panel>

        <asp:Panel ID="panButton" runat="server" BorderStyle="None" style="padding-left: 0px; padding-bottom: 30px; margin-top: 0px;">
            <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                <tr style="height: 23px;">
                    <td class="LabelBold" style="width: 145px;">
                        <asp:CustomValidator ID="cusValButton" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                    </td>
                    <td style="width: 750px;">                        
                        <telerik:RadButton ID="btnSave" runat="server" ToolTip="Save data" Width="70px"
                            Text="Save" Skin="Office2010Silver" Font-Bold="False" Font-Size="9pt" 
                            CssClass="RadButtonStyle" ValidationGroup="valPrimary"
                            OnClick="btnSave_Click" Enabled="False">
                        </telerik:RadButton>
                        <telerik:RadButton ID="btnDelete" runat="server" ToolTip="Delete record"
                            Text="Delete" Skin="Office2010Silver" Font-Bold="False" Font-Size="9pt" 
                            CssClass="RadButtonStyle" ValidationGroup="valPrimary" Width="70px"
                            OnClick="btnDelete_Click" Enabled="False">
                        </telerik:RadButton>                                                 
                        <telerik:RadButton ID="btnReset" runat="server" ToolTip="Clear data entry form"
                            Text="Reset" Skin="Office2010Silver" Font-Bold="False" Font-Size="9pt"
                            CssClass="RadButtonStyle" CausesValidation="false" Width="70px"
                            OnClick="btnReset_Click" Enabled="False">
                        </telerik:RadButton> 
                        <telerik:RadButton ID="btnBack" runat="server" ToolTip="Go back to previous page"
                            Text="<< Back" Skin="Office2010Silver" Font-Bold="False" Font-Size="9pt" Width="70px"
                            CssClass="RadButtonStyle" CausesValidation="false" OnClick="btnBack_Click">
                        </telerik:RadButton>  
                    </td>
                    <td />
                </tr>                                
            </table>    
            
            <table border="0" style="width: 100%; text-align: left; margin-top: 0px; table-layout: fixed;">
                <tr>
                    <td class="LabelBold" style="text-align: left; padding-left: 20px; color: silver;">
                        NOTES:
                    </td>
                </tr>
                <tr>
                    <td class="LabelNotes" style="text-align: left; color: silver; font-style: normal; padding-left: 20px;">
                        - Employee No., Effective Date, Ending Date, and Absence Reason Code are mandatory fields.
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
                <telerik:AjaxSetting AjaxControlID="txtEmpNo">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panBody" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting> 
            </AjaxSettings>
        </telerik:RadAjaxManager>
        <telerik:RadAjaxLoadingPanel ID="loadingPanel" runat="server" Skin="Office2010Silver"></telerik:RadAjaxLoadingPanel>
    </asp:Panel>
</asp:Content>
