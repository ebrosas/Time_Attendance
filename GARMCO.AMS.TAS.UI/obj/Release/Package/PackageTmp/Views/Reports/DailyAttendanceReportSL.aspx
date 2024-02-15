<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/TASMaster.Master" AutoEventWireup="true" CodeBehind="DailyAttendanceReportSL.aspx.cs" 
    Inherits="GARMCO.AMS.TAS.UI.Views.Reports.DailyAttendanceReportSL" StylesheetTheme="Standard" %>

<%@ MasterType VirtualPath="~/Views/Shared/TASMaster.Master" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <title>Daily Attendance Report</title>
    <style type="text/css">
        .auto-style1 {
            font-weight: bolder;
            font-size: 8pt;
            color: #333333;
            font-family: Tahoma;
            text-align: right;
            padding-right: 10px;
            height: 23px;
        }
        .auto-style2 {
            height: 23px;
        }
    </style>
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
                            Salary Staff Daily Attendance Report
                        </td>
                        <td style="width: 50px;">
                            <asp:LinkButton ID="lnkMoveUp" runat="server" />
                        </td>
                        <td />
                    </tr>
                    <tr>
                        <td class="PageDescriptionHeader" style="text-align: left; vertical-align: top; padding-left: 0px; margin: 0px;">
                            View the daily attendance report for Salary Staff employees
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
        <asp:Panel ID="panBody" runat="server" BorderStyle="None" style="padding: 0px; margin: 0px;" CssClass="GroupPanelHeader">
            <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                <tr style="height: 23px;">
                    <td class="LabelBold" style="width: 120px; padding-right: 2px;">        
                        <asp:RadioButtonList ID="rblDateOption" runat="server" Width="100%" TextAlign="Left" AutoPostBack="True" OnSelectedIndexChanged="rblDateOption_SelectedIndexChanged" RepeatLayout="Flow">
                            <asp:ListItem Text="Specific Date" Value="valSpecificDate" Selected="True" />
                            <asp:ListItem Text="Date Range" Value="valDateRange" />
                        </asp:RadioButtonList>
                    </td>
                    <td style="width: 300px; vertical-align: top;">
                        <table border="0" style="width: 100%; text-align: left; margin: 0px; padding: 0px; table-layout: fixed;">
                            <tr style="vertical-align: top;">
                                <td style="width: 120px; padding-left: 0px;">
                                     <telerik:RadDatePicker ID="dtpStartDate" runat="server"
                                        Width="120px" Skin="Windows7" TabIndex="2">
                                        <Calendar ID="Calendar3" runat="server" Skin="Windows7" UseColumnHeadersAsSelectors="False" 
                                            UseRowHeadersAsSelectors="False" ViewSelectorText="x">
                                        </Calendar>
                                        <DateInput ID="DateInput3" runat="server" DateFormat="d/M/yyyy" DisplayDateFormat="d/M/yyyy" TabIndex="2">
                                            <EmptyMessageStyle Resize="None" />
                                            <ReadOnlyStyle Resize="None" />
                                            <FocusedStyle Resize="None" />
                                            <DisabledStyle Resize="None" />
                                            <InvalidStyle Resize="None" />
                                            <HoveredStyle Resize="None" />
                                            <EnabledStyle Resize="None" />
                                        </DateInput>
                                        <DatePopupButton HoverImageUrl="" ImageUrl="" TabIndex="2" />
                                    </telerik:RadDatePicker>       
                                </td>
                                <td class="LabelBold" style="width: 5px; text-align: center; padding: 0px;">
                                    
                                </td>
                                <td style="width: 200px;">
                                    <telerik:RadDatePicker ID="dtpEndDate" runat="server"
                                        Width="120px" Skin="Windows7" TabIndex="2">
                                        <Calendar ID="Calendar1" runat="server" Skin="Windows7" UseColumnHeadersAsSelectors="False" 
                                            UseRowHeadersAsSelectors="False" ViewSelectorText="x">
                                        </Calendar>
                                        <DateInput ID="DateInput1" runat="server" DateFormat="d/M/yyyy" DisplayDateFormat="d/M/yyyy" TabIndex="2">
                                            <EmptyMessageStyle Resize="None" />
                                            <ReadOnlyStyle Resize="None" />
                                            <FocusedStyle Resize="None" />
                                            <DisabledStyle Resize="None" />
                                            <InvalidStyle Resize="None" />
                                            <HoveredStyle Resize="None" />
                                            <EnabledStyle Resize="None" />
                                        </DateInput>
                                        <DatePopupButton HoverImageUrl="" ImageUrl="" TabIndex="2" />
                                    </telerik:RadDatePicker>       
                                </td>
                            </tr>
                        </table>                          
                    </td>
                    <td class="LabelBold" style="width: 100px; text-align: left;">
                       <asp:CustomValidator ID="cusValDate" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" />      
                    </td>
                    <td style="width: 150px; padding-left: 0px;">
                                                                       
                    </td>                    
                    <td />
                </tr>    
                <tr style="height: 23px; display: none;">
                    <td class="LabelBold">
                        Employee Type
                    </td>
                    <td style="padding-left: 0px; margin-left: 0px; text-align: left;">
                        <asp:RadioButtonList ID="rblEmployeeType" runat="server" RepeatDirection="Horizontal" Width="250px" Font-Bold="False" Font-Names="Tahoma" Font-Size="9pt"
                            style="padding-left: 0px;" TextAlign="Left">
                            <asp:ListItem Text="All" Value="valBoth" Selected="True" />
                            <asp:ListItem Text="Non Salary Staff" Value="valNonSalaryStaff" />
                            <asp:ListItem Text="Salary Staff" Value="valSalaryStaff" />                                                        
                        </asp:RadioButtonList>
                    </td>
                    <td class="LabelBold">
                       
                    </td>
                    <td>
                         
                    </td>                    
                    <td />
                </tr> 
                <tr style="vertical-align: top;">
                    <td class="LabelBold" style="padding-top: 4px;">
                        <asp:CustomValidator ID="cusValCostCenter" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                        Cost Center
                    </td>
                    <td style="padding-left: 0px;">
                        <telerik:RadListBox ID="lbCostCenter" runat="server" 
                            Height="180px" Width="100%" Skin="Office2010Silver" style="top: 0px; left: 0px"                            
                            EmptyMessage="Cost Center List" 
                            AutoPostBackOnReorder="True" EnableDragAndDrop="True" 
                            CheckBoxes="True" TabIndex="4">
                            <ButtonSettings TransferButtons="All" />
                            <EmptyMessageTemplate>
                                No allowed Cost Center was found!
                            </EmptyMessageTemplate>
                        </telerik:RadListBox>
                    </td>
                    <td class="LabelBold" style="padding-top: 4px;">
                        
                    </td>
                    <td>
                                                
                    </td>                    
                    <td />
                </tr>    
                <tr id="trSelectAll" runat="server">
                    <td>
                        
                    </td>
                    <td style="padding-left: 4px;" class="auto-style2">
                        <asp:CheckBox ID="chkSelectAll" runat="server" Text="Select All Cost Centers" 
                            AutoPostBack="true" style="padding-left: 0px;"
                            oncheckedchanged="chkSelectAll_CheckedChanged" TabIndex="5" />
                    </td>
                    <td class="auto-style1">
                       
                    </td>
                    <td class="auto-style2">
                         
                    </td>                    
                    <td class="auto-style2" />
                </tr>                 
                <tr style="height: 23px;">
                    <td class="LabelBold">
                        <asp:CustomValidator ID="cusValButton" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                    </td>
                    <td colspan="3" style="padding-left: 2px; padding-top: 5px;">
                        <telerik:RadButton ID="btnShowReport" runat="server" Text="Show Report" ToolTip="View and print the report" Width="100px" 
                            CssClass="RadButtonStyle" ValidationGroup="valPrimary" Font-Size="9pt" OnClick="btnShowReport_Click" Skin="Office2010Silver" TabIndex="6" />
                        <telerik:RadButton ID="btnReset" runat="server" Text="Reset" ToolTip="Clear the form" Width="70px" 
                            CssClass="RadButtonStyle" ValidationGroup="valPrimary" Font-Size="9pt" OnClick="btnReset_Click" Skin="Office2010Silver" TabIndex="7" />                                                
                    </td>
                    <td />
                </tr>                
            </table>

            <table border="0" style="width: 100%; text-align: left; margin-top: 0px; margin-left: 0px; table-layout: fixed;">
                <tr>
                    <td class="LabelBold" style="text-align: left; padding-left: 20px; color: silver;">
                        NOTES:
                    </td>
                </tr>
                <tr>
                    <td class="LabelNotes" style="text-align: left; color: silver; font-style: normal; padding-left: 20px;">
                        - Date, Employee Type and Cost Center are mandatory fields.
                    </td>
                </tr>
                <tr>
                    <td class="LabelNotes" style="text-align: left; color: silver; font-style: normal; padding-left: 20px;">
                        - Multiple Cost Center can be selected if date filter is set to "<b>Specific Date</b>"
                    </td>
                </tr>
                <tr>
                    <td class="LabelNotes" style="text-align: left; color: silver; font-style: normal; padding-left: 20px;">
                        - Only one cost center can be selected if date filter is set to "<b>Date Range</b>"
                    </td>
                </tr>
                <tr>
                    <td class="LabelNotes" style="text-align: left; color: silver; font-style: normal; padding-left: 20px;">
                        - The speed of data retrieval is dependent on the specified filter criterias
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
                <telerik:AjaxSetting AjaxControlID="rblDateOption">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panBody" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>     
            </AjaxSettings>
        </telerik:RadAjaxManager>
        <telerik:RadAjaxLoadingPanel ID="loadingPanel" runat="server" Skin="Office2010Silver"></telerik:RadAjaxLoadingPanel>
    </asp:Panel>

    <asp:Panel ID="panelDataSources" runat="server" style="display: none;">
        <asp:ObjectDataSource ID="objCostCenter" runat="server" OldValuesParameterFormatString="" SelectMethod="GetCostCenter" TypeName="GARMCO.Common.DAL.Employee.EmployeeBLL">
			<SelectParameters>
				<asp:Parameter Name="costCenter" Type="String" />
				<asp:Parameter Name="costCenterName" Type="String" />
				<asp:Parameter Name="sort" Type="String" />
			</SelectParameters>
		</asp:ObjectDataSource>
    </asp:Panel>
</asp:Content>
