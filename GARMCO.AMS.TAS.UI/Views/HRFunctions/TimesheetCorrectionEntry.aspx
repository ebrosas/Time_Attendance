<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/TASMaster.Master" AutoEventWireup="true" CodeBehind="TimesheetCorrectionEntry.aspx.cs" 
    Inherits="GARMCO.AMS.TAS.UI.Views.HRFunctions.TimesheetCorrectionEntry" StylesheetTheme="Standard" %>

<%@ MasterType VirtualPath="~/Views/Shared/TASMaster.Master" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <title>Timesheet Correction</title>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="mainContent" runat="server">
    <telerik:RadFormDecorator ID="formDecor" runat="server" DecoratedControls="Buttons" Skin="Office2010Silver" />

    <table border="0" style="width: 100%; text-align: left; margin-top: 5px; margin-left: 5px;">
        <tr>
            <td colspan="2" style="padding-left: 10px;">
                <table border="0" style="width: 100%; text-align: left;">
                    <tr>
                        <td style="width: 50px; text-align: right; padding-right: 5px;" rowspan="2">
                            <img alt="" src="../../Images/attendance_correction_icon.png" />
                        </td>
                        <td id="tdPageTitle" runat="server"  class="PageTitleLabel" style="vertical-align: bottom; padding-left: 5px; width: 900px; font-size: 11pt;">
                            Timesheet Correction
                        </td>
                        <td style="width: 50px;">
                            <asp:LinkButton ID="lnkMoveUp" runat="server" />
                        </td>
                        <td />
                    </tr>
                    <tr>
                        <td class="PageDescriptionHeader" style="text-align: left; vertical-align: top; padding-left: 5px; margin: 0px;">
                            View and edit the employee's attendance record
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
            <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                <tr style="height: 23px;">
                    <td class="LabelBold" style="width: 130px;">
                        <asp:CustomValidator ID="cusValEmpNo" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                        Employee No.                 
                    </td>
                    <td style="width: 250px;">
                        <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                            <tr style="vertical-align: top; margin: 0px; padding: 0px;">
                                <td style="width: 110px; text-align: left; padding-left: 3px;">
                                    <telerik:RadNumericTextBox ID="txtEmpNo" runat="server" width="100%" 
                                        MinValue="0" ToolTip="(Note: Employee No. must start with 1000. Example: 10003632)" 
                                        Skin="Office2010Silver" DataType="System.Int32" MaxLength="8" MaxValue="99999999" 
                                        EmptyMessage="1000xxxx" BackColor="Yellow" >
                                        <NumberFormat ZeroPattern="n" DecimalDigits="0" GroupSeparator="" />
                                    </telerik:RadNumericTextBox> 
                                </td>
                                <td style="width: 40px; text-align: left; padding-left: 1px; padding-top: 0px; display: none;">
                                    <telerik:RadButton ID="btnGet" runat="server" Skin="Office2010Silver" Width="100%" 
                                        Text="Get" ToolTip="Get employee info based on entered Employee No." Enabled="true" 
                                        Font-Bold="False" Font-Size="9pt" CausesValidation="false"
                                        onclick="btnGet_Click">
                                    </telerik:RadButton>
                                </td> 
                                <td style="text-align: left; width: 30px; padding-left: 1px; padding-top: 0px;">
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
                        <asp:CustomValidator ID="cusValCorrectionCode" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                        Correction Code
                    </td>
                    <td class="TextNormal" style="width: 250px;">
                        <telerik:RadComboBox ID="cboCorrectionCode" runat="server"
                            DropDownWidth="240px" 
                            HighlightTemplatedItems="True" 
                            Skin="Office2010Silver" 
                            Width="100%" 
                            MaxHeight="200px"
                            EmptyMessage="Select Correction Code"
                            EnableVirtualScrolling="True" BackColor="Yellow" AutoPostBack="True" OnSelectedIndexChanged="cboCorrectionCode_SelectedIndexChanged" >
                        </telerik:RadComboBox>
                    </td>
                    <td />
                </tr>
                <tr id="trRelativeType" runat="server" style="height: 23px; vertical-align: top; display: none;">
                    <td style="padding-top: 5px;">
                         
                    </td>
                    <td class="TextNormal"  style="padding-left: 3px;">
                        
                    </td>
                    <td style="padding-top: 5px;">
                        <table border="0" style="width: 100%; text-align: left; margin: 0px; padding: 0px; table-layout: fixed;">
                            <tr style=" margin: 0px; padding: 0px;">
                                <td style="width: auto; text-align: right;">
                                    <asp:CustomValidator ID="cusValRelativeType" runat="server" ControlToValidate="txtGeneric"                            
                                        CssClass="LabelValidationError" Display="Dynamic" 
                                        ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                                        ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                                </td>
                                <td id="tdRelativeTitle" runat="server" class="LabelBold" style="width: 95px;">
                                    Relative Type
                                </td>
                            </tr>
                        </table>
                    </td>
                    <td class="TextNormal">
                        <table border="0" style="width: 100%; text-align: left; margin: 0px; padding: 0px; table-layout: fixed;">
                            <tr id="trOtherRelative" runat="server" style=" margin: 0px; padding: 0px; display: none;">
                                <td style="margin: 0px; padding: 0px;">
                                    <telerik:RadTextBox ID="txtOtherRelative" runat="server" Width="100%" 
                                        EmptyMessage="Enter other relative type here" Skin="Office2010Silver" ToolTip="(Note: Maximum text input is 200 chars.)" TextMode="MultiLine" Rows="3" 
                                        Font-Names="Tahoma" Font-Size="9pt" MaxLength="200" BackColor="Yellow" />
                                </td>
                            </tr>
                            <tr id="trRelativeTypeCombo" runat="server" style=" margin: 0px; padding: 0px;">
                                <td style="margin: 0px; padding: 0px;">
                                    <telerik:RadComboBox ID="cboRelativeType" runat="server"
                                        DropDownWidth="350px" 
                                        HighlightTemplatedItems="True" 
                                        Skin="Office2010Silver" 
                                        MaxHeight="200px"
                                        Width="100%" 
                                        EmptyMessage="Select Relative Type"
                                        EnableVirtualScrolling="True" BackColor="Yellow">
                                    </telerik:RadComboBox>
                                </td>
                            </tr>
                        </table>
                    </td>
                    <td>
                        <telerik:RadTextBox ID="txtRemarks" runat="server" Width="100px" Visible="false" 
                            EmptyMessage="" Skin="Office2010Silver" ToolTip="(Note: Maximum text input is 200 chars.)" TextMode="MultiLine" Rows="3" 
                            Font-Names="Tahoma" Font-Size="9pt" MaxLength="200" />
                    </td>
                </tr>
                <tr style="height: 23px;">
                    <td class="LabelBold">
                        Employee Name                 
                    </td>
                    <td class="TextNormal">
                        <asp:Literal ID="litEmpName" runat="server" Text="Not defined" />         
                    </td>
                    <td class="LabelBold">
                        <asp:CustomValidator ID="cusValOTType" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                        OT Type
                    </td>
                    <td class="TextNormal">
                        <telerik:RadComboBox ID="cboOTType" runat="server"
                            DropDownWidth="240px" 
                            HighlightTemplatedItems="True" 
                            Skin="Office2010Silver" 
                            Width="100%" 
                            EmptyMessage="Select Overtime Type"
                            EnableVirtualScrolling="True" >
                        </telerik:RadComboBox>
                    </td>
                    <td />
                </tr>
                <tr style="height: 23px;">
                    <td class="LabelBold">
                        Position
                    </td>
                    <td class="TextNormal">
                        <asp:Literal ID="litPosition" runat="server" Text="Not defined" />                                  
                    </td>
                    <td class="LabelBold">
                        <asp:CustomValidator ID="cusValStartTime" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                        OT Start Time
                    </td>
                    <td class="TextNormal">
                        <telerik:RadDateInput ID="dtpStartTime" runat="server" LabelWidth="50px" Width="80px" Culture="en-US" EmptyMessage="HH:mm" ToolTip="Time Format: HH:mm"
                            InvalidStyleDuration="100" DateFormat="HH:mm" Enabled="False" Skin="Office2010Silver">
                        </telerik:RadDateInput>
                        <telerik:RadDateInput ID="dtpStartTimeMirror" runat="server" LabelWidth="50px" Width="80px" Culture="en-US" EmptyMessage="HH:mm" ToolTip="Time Format: HH:mm" Visible="false"
                            InvalidStyleDuration="100" DateFormat="HH:mm" Enabled="False" Skin="Office2010Silver">
                        </telerik:RadDateInput>
                    </td>
                    <td />
                </tr>
                <tr style="height: 23px;">
                    <td class="LabelBold">
                        Cost Center
                    </td>
                    <td class="TextNormal">
                       <asp:Literal ID="litCostCenter" runat="server" Text="Not defined" />                                  
                    </td>
                    <td class="LabelBold">
                        <asp:CustomValidator ID="cusValEndTime" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                        OT End Time  
                    </td>
                    <td class="TextNormal">
                        <telerik:RadDateInput ID="dtpEndTime" runat="server" LabelWidth="50px" Width="80px" Culture="en-US" EmptyMessage="HH:mm" ToolTip="Time Format: HH:mm"
                            InvalidStyleDuration="100" DateFormat="HH:mm" Enabled="False" Skin="Office2010Silver">
                        </telerik:RadDateInput> 
                        <telerik:RadDateInput ID="dtpEndTimeMirror" runat="server" LabelWidth="50px" Width="80px" Culture="en-US" EmptyMessage="HH:mm" ToolTip="Time Format: HH:mm" Visible="false"
                            InvalidStyleDuration="100" DateFormat="HH:mm" Enabled="False" Skin="Office2010Silver">
                        </telerik:RadDateInput> 
                    </td>
                    <td />
                </tr>
                <tr style="height: 23px;">
                    <td class="LabelBold">
                        Attendance Date
                    </td>
                    <td class="TextNormal">
                       <asp:Literal ID="litAttendanceDate" runat="server" Text="Not defined" />                                  
                    </td>
                    <td class="LabelBold">
                        <asp:CustomValidator ID="cusValNPH" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                        No Pay Hours
                    </td>
                    <td class="TextNormal">
                        <telerik:RadDateInput ID="dtpNPH" runat="server" LabelWidth="50px" Width="80px" Culture="en-US" EmptyMessage="HH:mm"
                            InvalidStyleDuration="100" DateFormat="HH:mm">
                        </telerik:RadDateInput>     
                        <telerik:RadNumericTextBox ID="txtNPH" runat="server" width="80px" 
                            MinValue="0" ToolTip="(Note: Enter numeric value in terms of minutes)" 
                            Skin="Office2010Silver" DataType="System.Int32" MaxLength="4" MaxValue="1440" 
                            EmptyMessage="" Visible="false">
                            <NumberFormat ZeroPattern="n" DecimalDigits="0" GroupSeparator="" />
                        </telerik:RadNumericTextBox> 
                    </td>
                    <td />
                </tr>

                <tr style="height: 23px;">
                    <td class="LabelBold">
                        Time In
                    </td>
                    <td class="TextNormal">
                        <asp:Literal ID="litTimeIn" runat="server" Text="Not defined" />         
                    </td>
                    <td class="LabelBold">
                        <asp:CustomValidator ID="cusValShiftCode" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                       Shift Code
                    </td>
                    <td class="TextNormal">
                        <telerik:RadComboBox ID="cboShiftCode" runat="server"
                            DropDownWidth="125px" 
                            HighlightTemplatedItems="True" 
                            Skin="Office2010Silver" 
                            Width="130px" 
                            EmptyMessage=""
                            EnableVirtualScrolling="True">
                        </telerik:RadComboBox>
                    </td>
                    <td style="text-align: left;">
                        <telerik:RadTextBox ID="txtShiftCode" runat="server" Width="80px" 
                            EmptyMessage="" Skin="Office2010Silver" ToolTip="(Note: Maximum text input is 10 chars.)" 
                            Font-Names="Tahoma" Font-Size="9pt" MaxLength="10" Visible="False" />                        
                        <asp:Literal ID="litActualShiftCode" runat="server" Text="" Visible="false" />  
                    </td>
                </tr>
                <tr style="height: 23px;">
                    <td class="LabelBold">
                        Time Out
                    </td>
                    <td class="TextNormal">
                        <asp:Literal ID="litTimeOut" runat="server" Text="Not defined" />         
                    </td>
                    <td class="LabelBold">
                        <asp:CustomValidator ID="cusValShiftAllowance" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                        Shift Allowance
                    </td>
                    <td class="TextNormal">
                        <asp:CheckBox ID="chkShiftAllowance" runat="server" Text="" />
                    </td>
                    <td />
                </tr>
                <tr style="height: 23px;">
                    <td class="LabelBold">
                        Last Update User
                    </td>
                    <td class="TextNormal"  style="padding-left: 0px;">
                        <asp:Literal ID="litLastUpdateUser" runat="server" Text="Not defined" />
                    </td>
                    <td class="LabelBold">
                        <asp:CustomValidator ID="cusvalDILEntitlement" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                        DIL Entitlement
                    </td>
                    <td class="TextNormal">
                        <telerik:RadComboBox ID="cboDILEntitlement" runat="server"
                            DropDownWidth="125px" 
                            HighlightTemplatedItems="True" 
                            Skin="Office2010Silver" 
                            Width="130px" 
                            EmptyMessage=""
                            EnableVirtualScrolling="True">
                        </telerik:RadComboBox>
                    </td>
                    <td />
                </tr>
                <tr style="height: 23px;">
                    <td class="LabelBold">
                        Last Update Time
                    </td>
                    <td class="TextNormal"  style="padding-left: 0px;">
                         <asp:Literal ID="litLastUpdateTime" runat="server" Text="Not defined" />
                    </td>
                    <td class="LabelBold">
                        <asp:CustomValidator ID="cusValRemarkCode" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" />
                        Remark Code
                    </td>
                    <td class="TextNormal">
                        <telerik:RadComboBox ID="cboRemarkCode" runat="server"
                            DropDownWidth="125px" 
                            HighlightTemplatedItems="True" 
                            Skin="Office2010Silver" 
                            Width="130px" 
                            EmptyMessage=""
                            EnableVirtualScrolling="True">
                            <Items>
                                <telerik:RadComboBoxItem runat="server" Text="" Value="" />
                                <telerik:RadComboBoxItem runat="server" Text="A" Value="A" />
                            </Items>
                        </telerik:RadComboBox>
                    </td>
                    <td />
                </tr>
            </table>                                  
        </asp:Panel>

        <asp:Panel ID="panButton" runat="server" BorderStyle="None" style="padding-left: 0px; padding-bottom: 30px; margin-top: 0px;">
            <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                <tr style="height: 23px;">
                    <td class="LabelBold" style="width: 120px;">
                        <asp:CustomValidator ID="cusValButton" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                    </td>
                    <td style="width: 750px; padding-top: 3px;">                        
                        <telerik:RadButton ID="btnSave" runat="server" ToolTip="Save data" Width="80px"
                            Text="Save" Skin="Office2010Silver" Font-Bold="False" Font-Size="9pt" 
                            CssClass="RadButtonStyle" ValidationGroup="valPrimary"
                            OnClick="btnSave_Click" Enabled="False">
                        </telerik:RadButton>
                        <telerik:RadButton ID="btnDelete" runat="server" ToolTip="Delete record"
                            Text="Delete" Skin="Office2010Silver" Font-Bold="False" Font-Size="9pt" 
                            CssClass="RadButtonStyle" ValidationGroup="valPrimary" Width="80px"
                            OnClick="btnDelete_Click" Enabled="False" Visible="False">
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
            
            <table border="0" style="width: 100%; text-align: left; margin-top: 0px; table-layout: fixed; display: none;">
                <tr>
                    <td class="LabelBold" style="text-align: left; padding-left: 20px; color: silver;">
                        NOTES:
                    </td>
                </tr>
                <tr>
                    <td class="LabelNotes" style="text-align: left; color: silver; font-style: normal; padding-left: 20px;">
                        - The Employee No., Effective Date, Ending Date, and Absence Reason Code are mandatory fields.
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
                <telerik:AjaxSetting AjaxControlID="cboCorrectionCode">
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
