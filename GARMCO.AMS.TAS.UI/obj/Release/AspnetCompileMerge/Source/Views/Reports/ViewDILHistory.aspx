<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/TASMaster.Master" AutoEventWireup="true" CodeBehind="ViewDILHistory.aspx.cs" 
    Inherits="GARMCO.AMS.TAS.UI.Views.Reports.ViewDILHistory" StylesheetTheme="Standard" %>

<%@ MasterType VirtualPath="~/Views/Shared/TASMaster.Master" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <title>View Employe DIL History</title>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="mainContent" runat="server">
    <telerik:RadFormDecorator ID="formDecor" runat="server" DecoratedControls="Buttons" Skin="Office2010Silver" />

    <table border="0" style="width: 100%; text-align: left; margin-top: 10px; margin-left: 5px;">
        <tr>
            <td colspan="2" style="padding-left: 5px;">
                <table border="0" style="width: 100%; text-align: left;">
                    <tr>
                        <td style="width: 50px; text-align: right; padding-right: 5px;" rowspan="2">
                            <img alt="" src="../../Images/print_report.jpg" />
                        </td>
                        <td id="tdPageTitle" runat="server" class="PageTitleLabel" style="vertical-align: bottom; padding-left: 0px; width: 900px; font-size: 11pt;">
                            View Employee DIL History
                        </td>
                        <td style="width: 50px;">
                            <asp:LinkButton ID="lnkMoveUp" runat="server" />
                        </td>
                        <td />
                    </tr>
                    <tr>
                        <td class="PageDescriptionHeader" style="text-align: left; vertical-align: top; padding-left: 0px; margin: 0px;">
                            View the employee's Day In Lieu history records
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

    <asp:Panel ID="panMain" runat="server" style="margin-top: 10px; padding-bottom: 40px;"> 
        <asp:Panel ID="panBody" runat="server" BorderStyle="None" style="padding: 0px; margin: 0px;" CssClass="GroupPanelHeader">
            <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                <tr style="height: 23px;">
                    <td class="LabelBold" style="width: 110px;">
                        <asp:CustomValidator ID="cusValEmpNo" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                        Employee No.
                    </td>
                    <td style="width: 300px;">
                        <table id="tdEmployee" runat="server" border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                            <tr style="vertical-align: top; margin: 0px; padding: 0px;">
                                <td style="width: 110px; text-align: left; padding-left: 0px;">
                                    <telerik:RadNumericTextBox ID="txtEmpNo" runat="server" width="100%" 
                                        MinValue="0" ToolTip="(Note: Employee No. must start with 1000. Example: 10003632)" 
                                        Skin="Office2010Silver" DataType="System.Int32" MaxLength="8" MaxValue="99999999" 
                                        EmptyMessage="1000xxxx" BackColor="Yellow" ReadOnly="True">
                                        <NumberFormat ZeroPattern="n" DecimalDigits="0" GroupSeparator="" />
                                    </telerik:RadNumericTextBox> 
                                </td>
                                <td style="text-align: left; width: 30px; padding-left: 3px;">
                                    <telerik:RadButton ID="btnFindEmployee" runat="server" Skin="Office2010Silver" 
                                        Text="..." ToolTip="Click to open the Employee Search page." Enabled="true" 
                                        Width="100%" Font-Bold="False" Font-Size="9pt" CausesValidation="false"
                                        onclick="btnFindEmployee_Click">
                                    </telerik:RadButton>
                                </td> 
                                <td />
                            </tr>
                        </table>    
                    </td>
                    <td style="width: 140px;">
                        <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                            <tr style="margin: 0px; padding: 0px;">                                
                                <td class="LabelBold" style="width: auto; padding-right: 0px;">
                                    <asp:CustomValidator ID="cusValPayrollYear" runat="server" ControlToValidate="txtGeneric" 
                                        CssClass="LabelValidationError" Display="Dynamic" 
                                        ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                                        ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                                    Select Pay Period
                                </td>
                                <td style="width: 20px; text-align: left; ">
                                     <asp:CheckBox ID="chkPayPeriod" runat="server" Text="" AutoPostBack="True" 
                                        OnCheckedChanged="chkPayPeriod_CheckedChanged" />
                                </td>
                            </tr>
                        </table>
                    </td>
                    <td style="width: 250px; padding-left: 0px;">
                        <table border="0" style="width: 100%; text-align: left; margin: 0px; padding: 0px; table-layout: fixed;">
                            <tr>
                                <td style="width: 100px; padding-left: 0px;">                                    
                                    <telerik:RadComboBox ID="cboMonth" runat="server"
                                        DropDownWidth="140px" 
                                        HighlightTemplatedItems="True" 
                                        Skin="Office2010Silver" 
                                        Width="100%" 
                                        EmptyMessage="Select Month" ToolTip="Payroll month"
                                        EnableVirtualScrolling="True" AutoPostBack="True" 
                                        onselectedindexchanged="cboMonth_SelectedIndexChanged" >
                                        <Items>
                                            <telerik:RadComboBoxItem runat="server" Text="January" Value="1" />
                                            <telerik:RadComboBoxItem runat="server" Text="February" Value="2" />
                                            <telerik:RadComboBoxItem runat="server" Text="March" Value="3" />
                                            <telerik:RadComboBoxItem runat="server" Text="April" Value="4" />
                                            <telerik:RadComboBoxItem runat="server" Text="May" Value="5" />
                                            <telerik:RadComboBoxItem runat="server" Text="June" Value="6" />
                                            <telerik:RadComboBoxItem runat="server" Text="July" Value="7" />
                                            <telerik:RadComboBoxItem runat="server" Text="August" Value="8" />
                                            <telerik:RadComboBoxItem runat="server" Text="September" Value="9" />
                                            <telerik:RadComboBoxItem runat="server" Text="October" Value="10" />
                                            <telerik:RadComboBoxItem runat="server" Text="November" Value="11" />
                                            <telerik:RadComboBoxItem runat="server" Text="December" Value="12" />
                                        </Items>
                                    </telerik:RadComboBox>
                                </td>
                                <td style="width: auto;">
                                        <telerik:RadNumericTextBox ID="txtYear" runat="server" ToolTip="Payroll year" 
                                        DataType="System.UInt32" MaxLength="4" MaxValue="2099" MinValue="0" 
                                        Width="60px" Skin="Office2010Silver" AutoPostBack="True" OnTextChanged="txtYear_TextChanged">
                                        <NumberFormat DecimalDigits="0" ZeroPattern="n" GroupSeparator="" />
                                    </telerik:RadNumericTextBox>
                                </td>
                            </tr>
                        </table>                                                                                
                    </td>                    
                    <td />
                </tr>    
                <tr style="height: 20px;">
                    <td class="LabelBold">
                        Employee Name     
                    </td>
                    <td style="padding-left: 4px;">
                        <asp:Literal ID="litEmpName" runat="server" Text="Not defined" /> 
                    </td>
                    <td class="LabelBold">
                        <asp:CustomValidator ID="cusValStartDate" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                        Start Date   
                    </td>
                    <td>
                        <telerik:RadDatePicker ID="dtpStartDate" runat="server"
                            Width="120px" Skin="Windows7">
                            <Calendar ID="Calendar3" runat="server" Skin="Windows7" UseColumnHeadersAsSelectors="False" 
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
                    <td />
                </tr>    
                <tr style="height: 20px;">
                    <td class="LabelBold">
                        Position
                    </td>
                    <td style="padding-left: 4px;">
                        <asp:Literal ID="litPosition" runat="server" Text="Not defined" />   
                    </td>
                    <td class="LabelBold">
                       <asp:CustomValidator ID="cusValEndDate" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                        End Date
                    </td>
                    <td>
                        <telerik:RadDatePicker ID="dtpEndDate" runat="server"
                            Width="120px" Skin="Windows7">
                            <Calendar ID="Calendar1" runat="server" Skin="Windows7" UseColumnHeadersAsSelectors="False" 
                                UseRowHeadersAsSelectors="False" ViewSelectorText="x">
                            </Calendar>
                            <DateInput ID="DateInput1" runat="server" DateFormat="d/M/yyyy" DisplayDateFormat="d/M/yyyy">
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
                    <td />
                </tr> 
                <tr style="height: 20px;">
                    <td class="LabelBold">
                        Cost Center
                    </td>
                    <td style="padding-left: 4px;">
                        <asp:Literal ID="litCostCenter" runat="server" Text="Not defined" /> 
                    </td>
                    <td class="LabelBold">
                       
                    </td>
                    <td>
                        
                    </td>                    
                    <td />
                </tr> 
                <tr style="height: 20px;">
                    <td class="LabelBold">
                        <asp:CustomValidator ID="cusValButton" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                    </td>
                    <td colspan="3" style="padding-left: 2px; padding-top: 5px;">
                        <telerik:RadButton ID="btnShowReport" runat="server" Text="Show Report" ToolTip="View and print the report" Width="100px" 
                            CssClass="RadButtonStyle" ValidationGroup="valPrimary" Font-Size="9pt" OnClick="btnShowReport_Click" Skin="Office2010Silver" />
                        <telerik:RadButton ID="btnReset" runat="server" Text="Reset" ToolTip="Clear the form" Width="70px" 
                            CssClass="RadButtonStyle" ValidationGroup="valPrimary" Font-Size="9pt" OnClick="btnReset_Click" Skin="Office2010Silver" />                                                
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
                        - Employee No. is mandatory field.
                    </td>
                </tr>
                <tr>
                    <td class="LabelNotes" style="text-align: left; color: silver; font-style: normal; padding-left: 20px;">
                        - The search for other employees button is enabled only if user has permission to view other employee's attendance 
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
    </asp:Panel>

    <asp:Panel ID="panAjaxManager" runat="server">
        <telerik:RadAjaxManager ID="MyAjaxManager" runat="server">
            <AjaxSettings>
                <telerik:AjaxSetting AjaxControlID="btnFindEmployee">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting> 
                <telerik:AjaxSetting AjaxControlID="btnShowReport">
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
                <telerik:AjaxSetting AjaxControlID="cboMonth">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panBody" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>      
                <telerik:AjaxSetting AjaxControlID="txtYear">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panBody" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>                  
                <telerik:AjaxSetting AjaxControlID="chkPayPeriod">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panBody" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>                  
            </AjaxSettings>
        </telerik:RadAjaxManager>
        <telerik:RadAjaxLoadingPanel ID="loadingPanel" runat="server" Skin="Office2010Silver"></telerik:RadAjaxLoadingPanel>
    </asp:Panel>
</asp:Content>
