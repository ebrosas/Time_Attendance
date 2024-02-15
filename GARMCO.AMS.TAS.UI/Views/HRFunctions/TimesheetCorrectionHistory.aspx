<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/TASMaster.Master" AutoEventWireup="true" CodeBehind="TimesheetCorrectionHistory.aspx.cs" 
    Inherits="GARMCO.AMS.TAS.UI.Views.HRFunctions.TimesheetCorrectionHistory" StylesheetTheme="Standard" %>

<%@ MasterType VirtualPath="~/Views/Shared/TASMaster.Master" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <title>Attendance History</title>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="mainContent" runat="server">
    <table border="0" style="width: 100%; text-align: left; margin-top: 5px; margin-left: 5px;">
        <tr>
            <td colspan="2" style="padding-left: 5px;">
                <table border="0" style="width: 100%; text-align: left;">
                    <tr>
                        <td style="width: 50px; text-align: right; padding-right: 5px;" rowspan="2">
                            <img alt="" src="../../Images/attendance_history_icon.png" />
                        </td>
                        <td id="tdPageTitle" runat="server" class="PageTitleLabel" style="vertical-align: bottom; padding-left: 5px; width: 900px; font-size: 11pt;">
                            Attendance History
                        </td>
                        <td style="width: 50px;">
                            <asp:LinkButton ID="lnkMoveUp" runat="server" />
                        </td>
                        <td />
                    </tr>
                    <tr>
                        <td class="PageDescriptionHeader" style="text-align: left; vertical-align: top; padding-left: 5px; margin: 0px;">
                            View the Timesheet Corrections, Shift Pattern Changes, Absence and Leave History records
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
                    <td class="LabelBold" style="width: 120px;">
                         <asp:CustomValidator ID="cusValEmpNo" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" />
                        Emp. No. 
                    </td>
                    <td style="width: 200px;">
                        <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                            <tr style="vertical-align: top; margin: 0px; padding: 0px;">
                                <td style="width: 110px; text-align: left;">
                                    <telerik:RadNumericTextBox ID="txtEmpNo" runat="server" width="100%" 
                                        MinValue="0" ToolTip="(Note: Employee No. must start with 1000. Example: 10003632)" 
                                        Skin="Office2010Silver" DataType="System.Int32" MaxLength="8" MaxValue="99999999" 
                                        EmptyMessage="1000xxxx" BackColor="Yellow" >
                                        <NumberFormat ZeroPattern="n" DecimalDigits="0" GroupSeparator="" />
                                    </telerik:RadNumericTextBox> 
                                </td>
                                <td style="width: 40px; text-align: left; padding-left: 3px; padding-top: 0px; vertical-align: top;">
                                    <telerik:RadButton ID="btnGet" runat="server" Skin="Office2010Silver" Width="100%" 
                                        Text="Get" ToolTip="Get employee info based on entered Employee No." Enabled="true" 
                                        Font-Bold="False" Font-Size="9pt" CausesValidation="false"
                                        onclick="btnGet_Click">
                                    </telerik:RadButton>
                                </td> 
                                <td style="text-align: left; width: auto; padding-left: 3px; padding-top: 0px; vertical-align: top;">
                                    <telerik:RadButton ID="btnFindEmployee" runat="server" Skin="Office2010Silver" 
                                        Text="..." ToolTip="Click here to search for an employee." Enabled="true" 
                                        Width="30px" Font-Bold="False" Font-Size="9pt" CausesValidation="false"
                                        onclick="btnFindEmployee_Click">
                                    </telerik:RadButton>
                                </td> 
                            </tr>
                        </table>
                    </td>
                    <td class="LabelBold" style="width: 110px;">
                        Position
                    </td>
                    <td class="TextNormal" style="width: 300px;">
                        <asp:Literal ID="litPosition" runat="server" Text="Not defined" />                               
                    </td>
                    <td />
                </tr>
                <tr style="height: 23px;">
                    <td class="LabelBold">
                        Emp. Name
                    </td>
                    <td class="TextNormal" style="padding-right: 0px;">
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
                        Attendance Date
                    </td>
                    <td class="TextNormal" style="padding-right: 0px;">
                        <asp:Literal ID="litAttendanceDate" runat="server" Text="Not defined" />                               
                    </td>
                    <td class="LabelBold">
                        
                    </td>
                    <td class="TextNormal">
                        
                    </td>
                    <td />
                </tr>
                <tr id="trButtons" runat="server" style="height: 23px;">
                    <td class="LabelBold">
                        <asp:CustomValidator ID="cusValButton" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                    </td>
                    <td colspan="3" style="padding-left: 2px; padding-top: 3px;">
                        <telerik:RadButton ID="btnSearch" runat="server" Text="Search" ToolTip="Search matching database records" Width="70px" 
                            CssClass="RadButtonStyle" ValidationGroup="valPrimary" Font-Size="9pt" OnClick="btnSearch_Click" Skin="Office2010Silver" />
                        <telerik:RadButton ID="btnReset" runat="server" Text="Reset" ToolTip="Reset filter criterias" Width="70px" 
                            CssClass="RadButtonStyle" ValidationGroup="valPrimary" Font-Size="9pt" OnClick="btnReset_Click" Skin="Office2010Silver" />                                                
                        <telerik:RadButton ID="btnBack" runat="server" Text="<< Back" ToolTip="Go back to previous page" Width="70px" 
                            CssClass="RadButtonStyle" ValidationGroup="valPrimary" Font-Size="9pt" OnClick="btnBack_Click" Skin="Office2010Silver" />                                                
                    </td>
                    <td />
                </tr>                
            </table>
        </asp:Panel>

        <asp:Panel ID="panBody" runat="server" CssClass="PanelNoIcon" style="margin-top: 10px; margin-right: 10px; margin-left: 10px; padding-bottom: 10px;">        
            <telerik:RadTabStrip ID="tabMain" runat="server" SelectedIndex="0"   
                MultiPageID="MyMultiPage" ReorderTabsOnSelect="True" 
                CausesValidation="False" ontabclick="tabMain_TabClick" 
                style="padding-top: 0px; padding-left: 0px; padding-right: 0px;" 
                Skin="Silk">
                <Tabs>
                    <telerik:RadTab Text="Timesheet Correction History" Font-Size="9pt" Font-Bold="True" Selected="True" Value="valTimesheetHistory">
                    </telerik:RadTab>                                     
                    <telerik:RadTab Text="Shift Pattern History" Font-Size="9pt" Font-Bold="True" Value="valShiftPatternHistory">
                    </telerik:RadTab> 
                    <telerik:RadTab Text="Absence History" Font-Size="9pt" Font-Bold="True" Value="valAbsenceHistory">
                    </telerik:RadTab> 
                    <telerik:RadTab Text="Leave History" Font-Size="9pt" Font-Bold="True" Value="valLeaveHistory">
                    </telerik:RadTab> 
                </Tabs>
            </telerik:RadTabStrip>

            <telerik:RadMultiPage ID="MyMultiPage" runat="server" SelectedIndex="0" Width="100%" style="padding-top: 5px; padding-left: 0px; padding-right: 10px;">
                <telerik:RadPageView ID="TimesheetHistoryView" runat="server">
                    <asp:Panel ID="panTimesheetHistory" runat="server" BorderStyle="None" style="padding-left: 10px; padding-right: 10px; padding-top: 5px; margin: 0px;">
                        <table border="0" style="width: 100%; text-align: left; margin-top: 0px; table-layout: fixed; display: none;">
                            <tr>
                                <td style="text-align: left; color: Purple; font-weight: bold; font-size: 9pt; padding-left: 5px;">
                                    <asp:Label ID="lblRecordCount" runat="server" Text="0 record found" Width="100%" />                         
                                </td>
                            </tr>
                        </table>
                        <table id="tblGrid" runat="server" border="0" style="width: 100%; text-align: left; margin-top: 0px; table-layout: fixed;">
                            <tr>
                                <td>
                                    <telerik:RadGrid ID="gridTimesheetCorrection" runat="server"
                                        AllowSorting="true" AllowMultiRowSelection="true"
                                        PageSize="10" AutoGenerateColumns="false" Skin="Silk" 
                                        GridLines="None" Width="100%" Height="" CellSpacing="0"
                                        onpageindexchanged="gridTimesheetCorrection_PageIndexChanged" 
                                        onpagesizechanged="gridTimesheetCorrection_PageSizeChanged" 
                                        onsortcommand="gridTimesheetCorrection_SortCommand" 
                                        onitemcommand="gridTimesheetCorrection_ItemCommand" 
                                        onitemdatabound="gridTimesheetCorrection_ItemDataBound" 
                                        Font-Names="Tahoma" Font-Size="9pt" Font-Bold="False" 
                                        AllowPaging = "true" BorderStyle="Outset" BorderWidth="1px" AllowCustomPaging="True" VirtualItemCount="1">
                                        <ExportSettings ExportOnlyData="true" IgnorePaging="true" OpenInNewWindow="true" FileName="AttendanceList" HideStructureColumns="true">
                                            <Pdf PageHeight="210mm" PageWidth="310mm" PageTitle="Attendance List" DefaultFontFamily="Arial Unicode MS"
                                            PageBottomMargin="20mm" PageTopMargin="20mm" PageLeftMargin="20mm" PageRightMargin="20mm" PaperSize="A4" AllowPrinting="true" />
                                        </ExportSettings>
                                        <AlternatingItemStyle Font-Names="Tahoma" Font-Size="9pt" Wrap="True" />
                                        <MasterTableView DataKeyNames="AutoID" ClientDataKeyNames="AutoID" 
                                            NoMasterRecordsText="No changes found in the Timesheet." 
                                            TableLayout="Fixed" PagerStyle-AlwaysVisible="True" Font-Names="Tahoma" 
                                            Font-Size="9pt">
                                            <PagerStyle AlwaysVisible="True" Mode="NextPrevAndNumeric" />
                                            <CommandItemSettings ExportToPdfText="Export to PDF"></CommandItemSettings>
			                                <RowIndicatorColumn>
				                                <HeaderStyle Width="20px" />
			                                </RowIndicatorColumn>
			                                <ExpandCollapseColumn>
				                                <HeaderStyle Width="20px" />
			                                </ExpandCollapseColumn>
                                            <Columns>   
                                                <telerik:GridBoundColumn DataField="AutoID" DataType="System.Int32" HeaderText="Auto ID" 
                                                    ReadOnly="True" SortExpression="AutoID" UniqueName="AutoID">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Width="80px" Font-Bold="True" />                                                    
                                                </telerik:GridBoundColumn>       
                                                <telerik:GridBoundColumn DataField="XID_AutoID" DataType="System.Int32" HeaderText="Ref. ID" 
                                                    ReadOnly="True" SortExpression="XID_AutoID" UniqueName="XID_AutoID">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Width="80px" Font-Bold="True" />
                                                    <ItemStyle Font-Bold="true" ForeColor="Purple" />
                                                </telerik:GridBoundColumn> 
                                                <telerik:GridBoundColumn DataField="CorrectionCode" DataType="System.String" HeaderText="Correction Code" 
                                                    ReadOnly="True" SortExpression="CorrectionCode" UniqueName="CorrectionCode">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Width="120px" Font-Bold="True" />
                                                </telerik:GridBoundColumn>   
                                                <telerik:GridTemplateColumn DataField="CorrectionDesc" HeaderText="Correction Description" 
                                                    SortExpression="CorrectionDesc" UniqueName="CorrectionDesc">
								                    <HeaderStyle Width="170px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								                    <ItemTemplate>
									                    <div class="columnEllipsis" style="width: 160px; text-align: left;">
										                    <asp:Literal ID="litCorrectionDesc" runat="server" Text='<%# Eval("CorrectionDesc") %>' />
									                    </div>
								                    </ItemTemplate>
							                    </telerik:GridTemplateColumn> 
                                                <telerik:GridTemplateColumn DataField="LastUpdateUser" HeaderText="Last Update User" 
                                                    SortExpression="LastUpdateUser" UniqueName="LastUpdateUser">
								                    <HeaderStyle Width="130px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								                    <ItemTemplate>
									                    <div class="columnEllipsis" style="width: 120px; text-align: left;">
										                    <asp:Literal ID="litLastUpdateUser" runat="server" Text='<%# Eval("LastUpdateUser") %>' />
									                    </div>
								                    </ItemTemplate>
							                    </telerik:GridTemplateColumn> 
                                                <telerik:GridBoundColumn DataField="EmpNo" DataType="System.Int32" HeaderText="Emp. No." 
                                                    ReadOnly="True" SortExpression="EmpNo" UniqueName="EmpNo">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Width="80px" Font-Bold="True" />
                                                </telerik:GridBoundColumn> 
                                                <telerik:GridBoundColumn DataField="DT" HeaderText="Date"
                                                    DataFormatString="{0:dd-MMM-yyyy}" DataType="System.DateTime" 
                                                    ReadOnly="True" SortExpression="DT" UniqueName="DT">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Width="90px" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma"></HeaderStyle>
                                                </telerik:GridBoundColumn>   
                                                <telerik:GridBoundColumn DataField="dtIN" HeaderText="Time In"
                                                    DataFormatString="{0:HH:mm}" DataType="System.DateTime" 
                                                    FilterControlAltText="Filter Time In column" ReadOnly="True" SortExpression="dtIN" UniqueName="dtIN">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" Width="80px"></HeaderStyle>
                                                </telerik:GridBoundColumn> 
                                                <telerik:GridBoundColumn DataField="dtOUT" HeaderText="Time Out"
                                                    DataFormatString="{0:HH:mm}" DataType="System.DateTime" 
                                                    FilterControlAltText="Filter Time Out column" ReadOnly="True" SortExpression="dtOUT" UniqueName="dtOUT">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" Width="80px"></HeaderStyle>
                                                </telerik:GridBoundColumn> 
                                                <telerik:GridBoundColumn DataField="ShiftAllowanceDesc" DataType="System.String" HeaderText="Shift Allowance" 
                                                    ReadOnly="True" SortExpression="ShiftAllowanceDesc" UniqueName="ShiftAllowanceDesc">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Width="120px" Font-Bold="True" />
                                                </telerik:GridBoundColumn> 
                                                <telerik:GridBoundColumn DataField="OTType" DataType="System.String" HeaderText="OT Type" 
                                                    ReadOnly="True" SortExpression="OTType" UniqueName="OTType">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Width="80px" Font-Bold="True" />
                                                </telerik:GridBoundColumn> 
                                                <telerik:GridBoundColumn DataField="OTStartTime" HeaderText="OT From"
                                                    DataFormatString="{0:HH:mm}" DataType="System.DateTime" 
                                                    ReadOnly="True" SortExpression="OTStartTime" UniqueName="OTStartTime">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" Width="70px"></HeaderStyle>
                                                </telerik:GridBoundColumn> 
                                                <telerik:GridBoundColumn DataField="OTEndTime" HeaderText="OT To"
                                                    DataFormatString="{0:HH:mm}" DataType="System.DateTime" 
                                                    ReadOnly="True" SortExpression="OTEndTime" UniqueName="OTEndTime">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" Width="70px"></HeaderStyle>
                                                </telerik:GridBoundColumn> 
                                                 <telerik:GridBoundColumn DataField="NoPayHours" DataType="System.Int32" HeaderText="NPH" 
                                                    ReadOnly="True" SortExpression="NoPayHours" UniqueName="NoPayHours">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Width="70px" Font-Bold="True" />
                                                </telerik:GridBoundColumn> 
                                                <telerik:GridBoundColumn DataField="AbsenceReasonCode" DataType="System.String" HeaderText="ROA" 
                                                    ReadOnly="True" SortExpression="AbsenceReasonCode" UniqueName="AbsenceReasonCode">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Width="70px" Font-Bold="True" />
                                                </telerik:GridBoundColumn> 
                                                <telerik:GridBoundColumn DataField="LeaveType" DataType="System.String" HeaderText="Leave Type" 
                                                    ReadOnly="True" SortExpression="LeaveType" UniqueName="LeaveType">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Width="90px" Font-Bold="True" />
                                                </telerik:GridBoundColumn> 
                                                <telerik:GridBoundColumn DataField="RemarkCode" DataType="System.String" HeaderText="Absent" 
                                                    ReadOnly="True" SortExpression="RemarkCode" UniqueName="RemarkCode">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Width="70px" Font-Bold="True" />
                                                </telerik:GridBoundColumn> 
                                                <telerik:GridBoundColumn DataField="DILEntitlement" DataType="System.String" HeaderText="DIL" 
                                                    ReadOnly="True" SortExpression="DILEntitlement" UniqueName="DILEntitlement">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Width="50px" Font-Bold="True" />
                                                </telerik:GridBoundColumn> 
                                                <telerik:GridTemplateColumn DataField="Processed" HeaderText="Processed in Payroll?" 
                                                    SortExpression="Processed" UniqueName="Processed">
								                    <HeaderStyle Width="160px" HorizontalAlign="Center" />
								                    <ItemTemplate>
									                    <div style="width: 100px; text-align: center;">
										                    <asp:Label ID="lblProcessed" runat="server" 
                                                                Text='<%# Convert.ToBoolean(Eval("Processed")) == true ? "Yes" : "No" %>'>
										                    </asp:Label>  
									                    </div>
								                    </ItemTemplate>
                                                    <ItemStyle HorizontalAlign="Center" />
							                    </telerik:GridTemplateColumn>                                                  
                                                <telerik:GridBoundColumn DataField="LastUpdateTime" HeaderText="Last Updated Time"
                                                    DataFormatString="{0:dd-MMM-yyyy h:mm tt}" DataType="System.DateTime" 
                                                    FilterControlAltText="Filter Modified Date column" ReadOnly="True" SortExpression="LastUpdateTime" UniqueName="LastUpdateTime">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Font-Bold="True" Font-Size="8pt" Width="160px" Font-Names="Tahoma"></HeaderStyle>
                                                </telerik:GridBoundColumn>     
                                            </Columns>
                                        </MasterTableView>
                                        <ClientSettings AllowColumnsReorder="False" EnableRowHoverStyle="true">
                                            <Selecting AllowRowSelect="True" UseClientSelectColumnOnly="True" />
                                            <Scrolling AllowScroll="true" UseStaticHeaders="true" SaveScrollPosition="true" ScrollHeight="" FrozenColumnsCount="3" />
			                                <Resizing AllowColumnResize="true" />   
                                        </ClientSettings>
                                        <HeaderStyle Font-Bold="True" Font-Size="8pt" />
                                        <ActiveItemStyle Font-Names="Tahoma" Font-Size="9pt" />
                                        <ItemStyle Font-Names="Tahoma" Font-Size="9pt" Wrap="True" />
                                        <SelectedItemStyle Font-Names="Tahoma" Font-Size="9pt" />
                                        <FilterMenu EnableImageSprites="False">
                                            <WebServiceSettings>
                                                <ODataSettings InitialContainerName="">
                                                </ODataSettings>
                                            </WebServiceSettings>
                                        </FilterMenu>
                                        <HeaderContextMenu CssClass="GridContextMenu GridContextMenu_Windows7">
                                            <WebServiceSettings>
                                                <ODataSettings InitialContainerName="">
                                                </ODataSettings>
                                            </WebServiceSettings>
                                        </HeaderContextMenu>
                                    </telerik:RadGrid>
                                </td>
                            </tr>
                        </table>
                    </asp:Panel>   
                </telerik:RadPageView>

                <telerik:RadPageView ID="ShiftPatternHistoryView" runat="server">
                    <asp:Panel ID="panShiftPatternHistory" runat="server" BorderStyle="None" style="padding-left: 10px; padding-right: 10px; padding-top: 5px; margin: 0px;">
                        <table border="0" style="width: 100%; text-align: left; margin-top: 0px; table-layout: fixed;">
                            <tr>
                                <td>
                                    <telerik:RadGrid ID="gridShiftPatternHistory" runat="server"
                                        AllowSorting="true" AllowMultiRowSelection="true"
                                        PageSize="10" AutoGenerateColumns="false" Skin="Silk" 
                                        GridLines="None" Width="100%" Height="" CellSpacing="0"
                                        onpageindexchanged="gridShiftPatternHistory_PageIndexChanged" 
                                        onpagesizechanged="gridShiftPatternHistory_PageSizeChanged" 
                                        onsortcommand="gridShiftPatternHistory_SortCommand" 
                                        onitemcommand="gridShiftPatternHistory_ItemCommand" 
                                        onitemdatabound="gridShiftPatternHistory_ItemDataBound" 
                                        Font-Names="Tahoma" Font-Size="9pt" Font-Bold="False" 
                                        AllowPaging = "true" BorderStyle="Outset" BorderWidth="1px" AllowCustomPaging="True" VirtualItemCount="1">
                                        <ExportSettings ExportOnlyData="true" IgnorePaging="true" OpenInNewWindow="true" FileName="AttendanceList" HideStructureColumns="true">
                                            <Pdf PageHeight="210mm" PageWidth="310mm" PageTitle="Attendance List" DefaultFontFamily="Arial Unicode MS"
                                            PageBottomMargin="20mm" PageTopMargin="20mm" PageLeftMargin="20mm" PageRightMargin="20mm" PaperSize="A4" AllowPrinting="true" />
                                        </ExportSettings>
                                        <AlternatingItemStyle Font-Names="Tahoma" Font-Size="9pt" Wrap="True" />
                                        <MasterTableView DataKeyNames="XID_AutoID" ClientDataKeyNames="XID_AutoID" 
                                            NoMasterRecordsText="No changes found for Shift Pattern." 
                                            TableLayout="Fixed" PagerStyle-AlwaysVisible="True" Font-Names="Tahoma" 
                                            Font-Size="9pt">
                                            <PagerStyle AlwaysVisible="True" Mode="NextPrevAndNumeric" />
                                            <CommandItemSettings ExportToPdfText="Export to PDF"></CommandItemSettings>
			                                <RowIndicatorColumn>
				                                <HeaderStyle Width="20px" />
			                                </RowIndicatorColumn>
			                                <ExpandCollapseColumn>
				                                <HeaderStyle Width="20px" />
			                                </ExpandCollapseColumn>
                                            <Columns>   
                                                <telerik:GridBoundColumn DataField="AutoID" DataType="System.Int32" HeaderText="Auto ID" 
                                                    ReadOnly="True" SortExpression="AutoID" UniqueName="AutoID">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Width="80px" Font-Bold="True" />
                                                </telerik:GridBoundColumn> 
                                                <telerik:GridBoundColumn DataField="XID_AutoID" DataType="System.Int32" HeaderText="Ref. ID" 
                                                    ReadOnly="True" SortExpression="XID_AutoID" UniqueName="XID_AutoID">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Width="80px" Font-Bold="True" />
                                                    <ItemStyle Font-Bold="true" ForeColor="Purple" />
                                                </telerik:GridBoundColumn> 
                                                <telerik:GridBoundColumn DataField="EmpNo" DataType="System.Int32" HeaderText="Emp. No." 
                                                    ReadOnly="True" SortExpression="EmpNo" UniqueName="EmpNo">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Width="80px" Font-Bold="True" />
                                                </telerik:GridBoundColumn> 
                                                <telerik:GridBoundColumn DataField="EffectiveDate" HeaderText="Effective Date"
                                                    DataFormatString="{0:dd-MMM-yyyy}" DataType="System.DateTime" 
                                                    ReadOnly="True" SortExpression="EffectiveDate" UniqueName="EffectiveDate">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Width="110px" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma"></HeaderStyle>
                                                </telerik:GridBoundColumn>  
                                                <telerik:GridBoundColumn DataField="EndingDate" HeaderText="Ending Date"
                                                    DataFormatString="{0:dd-MMM-yyyy}" DataType="System.DateTime" 
                                                    ReadOnly="True" SortExpression="EndingDate" UniqueName="EndingDate">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Width="90px" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma"></HeaderStyle>
                                                </telerik:GridBoundColumn> 
                                                 <telerik:GridBoundColumn DataField="ShiftPatCode" DataType="System.String" HeaderText="Shift Pat. Code" 
                                                    ReadOnly="True" SortExpression="ShiftPatCode" UniqueName="ShiftPatCode">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Width="110px" Font-Bold="True" />
                                                </telerik:GridBoundColumn> 
                                                <telerik:GridBoundColumn DataField="ShiftPointer" DataType="System.Int32" HeaderText="Shift Pointer" 
                                                    ReadOnly="True" SortExpression="ShiftPointer" UniqueName="ShiftPointer">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Width="100px" Font-Bold="True" />
                                                </telerik:GridBoundColumn> 
                                                <telerik:GridBoundColumn DataField="ChangeTypeDesc" DataType="System.String" HeaderText="Change Type" 
                                                    ReadOnly="True" SortExpression="ChangeTypeDesc" UniqueName="ChangeTypeDesc">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Width="100px" Font-Bold="True" />
                                                </telerik:GridBoundColumn> 
                                                <telerik:GridTemplateColumn DataField="LastUpdateUser" HeaderText="Last Update User" 
                                                    SortExpression="LastUpdateUser" UniqueName="LastUpdateUser">
								                    <HeaderStyle Width="130px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								                    <ItemTemplate>
									                    <div class="columnEllipsis" style="width: 120px; text-align: left;">
										                    <asp:Literal ID="litLastUpdateUser" runat="server" Text='<%# Eval("LastUpdateUser") %>' />
									                    </div>
								                    </ItemTemplate>
							                    </telerik:GridTemplateColumn> 
                                                <telerik:GridBoundColumn DataField="LastUpdateTime" HeaderText="Last Updated Date"
                                                    DataFormatString="{0:dd-MMM-yyyy h:mm tt}" DataType="System.DateTime" 
                                                    FilterControlAltText="Filter Modified Date column" ReadOnly="True" SortExpression="LastUpdateTime" UniqueName="LastUpdateTime">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Font-Bold="True" Font-Size="8pt" Width="160px" Font-Names="Tahoma"></HeaderStyle>
                                                </telerik:GridBoundColumn>    
                                                <telerik:GridBoundColumn DataField="ActionTypeDesc" DataType="System.String" HeaderText="DB Action Type" 
                                                    ReadOnly="True" SortExpression="ActionTypeDesc" UniqueName="ActionTypeDesc">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Width="110px" Font-Bold="True" />
                                                </telerik:GridBoundColumn> 
                                                <telerik:GridBoundColumn DataField="ActionMachineName" DataType="System.String" HeaderText="DB Action Machine Name" 
                                                    ReadOnly="True" SortExpression="ActionMachineName" UniqueName="ActionMachineName">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Width="170px" Font-Bold="True" />
                                                </telerik:GridBoundColumn> 
                                                <telerik:GridBoundColumn DataField="ActionDateTime" HeaderText="DB Action Date/Time"
                                                    DataFormatString="{0:dd-MMM-yyyy h:mm tt}" DataType="System.DateTime" 
                                                    ReadOnly="True" SortExpression="ActionDateTime" UniqueName="ActionDateTime">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma"></HeaderStyle>
                                                </telerik:GridBoundColumn>                                                    
                                            </Columns>
                                        </MasterTableView>
                                        <ClientSettings AllowColumnsReorder="False" EnableRowHoverStyle="true">
                                            <Selecting AllowRowSelect="True" UseClientSelectColumnOnly="True" />
                                            <Scrolling AllowScroll="true" UseStaticHeaders="true" SaveScrollPosition="true" ScrollHeight="" />
			                                <Resizing AllowColumnResize="true" />   
                                        </ClientSettings>
                                        <HeaderStyle Font-Bold="True" Font-Size="8pt" />
                                        <ActiveItemStyle Font-Names="Tahoma" Font-Size="9pt" />
                                        <ItemStyle Font-Names="Tahoma" Font-Size="9pt" Wrap="True" />
                                        <SelectedItemStyle Font-Names="Tahoma" Font-Size="9pt" />
                                        <FilterMenu EnableImageSprites="False">
                                            <WebServiceSettings>
                                                <ODataSettings InitialContainerName="">
                                                </ODataSettings>
                                            </WebServiceSettings>
                                        </FilterMenu>
                                        <HeaderContextMenu CssClass="GridContextMenu GridContextMenu_Windows7">
                                            <WebServiceSettings>
                                                <ODataSettings InitialContainerName="">
                                                </ODataSettings>
                                            </WebServiceSettings>
                                        </HeaderContextMenu>
                                    </telerik:RadGrid>
                                </td>
                            </tr>
                        </table>
                    </asp:Panel>   
                </telerik:RadPageView>

                <telerik:RadPageView ID="AbsenceHistoryView" runat="server">
                    <asp:Panel ID="panAbsenceHistory" runat="server" BorderStyle="None" style="padding-left: 10px; padding-right: 10px; padding-top: 5px; margin: 0px;">
                        <table border="0" style="width: 100%; text-align: left; margin-top: 0px; table-layout: fixed;">
                            <tr>
                                <td>
                                    <telerik:RadGrid ID="gridAbsenceHistory" runat="server"
                                        AllowSorting="true" AllowMultiRowSelection="true"
                                        PageSize="10" AutoGenerateColumns="false" Skin="Silk" 
                                        GridLines="None" Width="100%" Height="" CellSpacing="0"
                                        onpageindexchanged="gridAbsenceHistory_PageIndexChanged" 
                                        onpagesizechanged="gridAbsenceHistory_PageSizeChanged" 
                                        onsortcommand="gridAbsenceHistory_SortCommand" 
                                        onitemcommand="gridAbsenceHistory_ItemCommand" 
                                        onitemdatabound="gridAbsenceHistory_ItemDataBound" 
                                        Font-Names="Tahoma" Font-Size="9pt" Font-Bold="False" 
                                        AllowPaging = "true" BorderStyle="Outset" BorderWidth="1px" AllowCustomPaging="True" VirtualItemCount="1">
                                        <ExportSettings ExportOnlyData="true" IgnorePaging="true" OpenInNewWindow="true" FileName="AbsenceHistoryList" HideStructureColumns="true">
                                            <Pdf PageHeight="210mm" PageWidth="310mm" PageTitle="Absence History List" DefaultFontFamily="Arial Unicode MS"
                                            PageBottomMargin="20mm" PageTopMargin="20mm" PageLeftMargin="20mm" PageRightMargin="20mm" PaperSize="A4" AllowPrinting="true" />
                                        </ExportSettings>
                                        <AlternatingItemStyle Font-Names="Tahoma" Font-Size="9pt" Wrap="True" />
                                        <MasterTableView DataKeyNames="AutoID" ClientDataKeyNames="AutoID" 
                                            NoMasterRecordsText="No changes found for Absence." 
                                            TableLayout="Fixed" PagerStyle-AlwaysVisible="True" Font-Names="Tahoma" 
                                            Font-Size="9pt">
                                            <PagerStyle AlwaysVisible="True" Mode="NextPrevAndNumeric" />
                                            <CommandItemSettings ExportToPdfText="Export to PDF"></CommandItemSettings>
			                                <RowIndicatorColumn>
				                                <HeaderStyle Width="20px" />
			                                </RowIndicatorColumn>
			                                <ExpandCollapseColumn>
				                                <HeaderStyle Width="20px" />
			                                </ExpandCollapseColumn>
                                            <Columns>   
                                                <telerik:GridBoundColumn DataField="AutoID" DataType="System.Int32" HeaderText="Auto ID" 
                                                    ReadOnly="True" SortExpression="AutoID" UniqueName="AutoID">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Width="80px" Font-Bold="True" />
                                                    <ItemStyle Font-Bold="true" ForeColor="Purple" />
                                                </telerik:GridBoundColumn> 
                                                <telerik:GridBoundColumn DataField="EmpNo" DataType="System.Int32" HeaderText="Emp. No." 
                                                    ReadOnly="True" SortExpression="EmpNo" UniqueName="EmpNo">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Width="90px" Font-Bold="True" />
                                                </telerik:GridBoundColumn> 
                                                <telerik:GridBoundColumn DataField="EffectiveDate" HeaderText="Effective Date"
                                                    DataFormatString="{0:dd-MMM-yyyy}" DataType="System.DateTime" 
                                                    ReadOnly="True" SortExpression="EffectiveDate" UniqueName="EffectiveDate">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Width="105px" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma"></HeaderStyle>
                                                </telerik:GridBoundColumn>   
                                                <telerik:GridBoundColumn DataField="EndingDate" HeaderText="Ending Date"
                                                    DataFormatString="{0:dd-MMM-yyyy}" DataType="System.DateTime" 
                                                    ReadOnly="True" SortExpression="EndingDate" UniqueName="EndingDate">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Width="105px" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma"></HeaderStyle>
                                                </telerik:GridBoundColumn>
                                                <telerik:GridBoundColumn DataField="StartTime" HeaderText="Start Time"
                                                    DataFormatString="{0:HH:mm tt}" DataType="System.DateTime" 
                                                    FilterControlAltText="Filter Start Time column" ReadOnly="True" SortExpression="StartTime" UniqueName="StartTime">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" Width="85px"></HeaderStyle>
                                                </telerik:GridBoundColumn> 
                                                <telerik:GridBoundColumn DataField="EndTime" HeaderText="End Time"
                                                    DataFormatString="{0:HH:mm tt}" DataType="System.DateTime" 
                                                    FilterControlAltText="Filter End Time column" ReadOnly="True" SortExpression="EndTime" UniqueName="EndTime">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" Width="85px"></HeaderStyle>
                                                </telerik:GridBoundColumn> 
                                                <telerik:GridBoundColumn DataField="DayOfWeek" DataType="System.String" HeaderText="Day of Week" 
                                                    ReadOnly="True" SortExpression="DayOfWeek" UniqueName="DayOfWeek">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Width="90px" Font-Bold="True" />
                                                </telerik:GridBoundColumn> 
                                                <telerik:GridBoundColumn DataField="AbsenceReasonCode" DataType="System.String" HeaderText="Absence Reason Code" Visible="false" 
                                                    ReadOnly="True" SortExpression="AbsenceReasonCode" UniqueName="AbsenceReasonCode">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Width="150px" Font-Bold="True" />
                                                </telerik:GridBoundColumn> 
                                                <telerik:GridBoundColumn DataField="AbsenceReasonFullName" HeaderText="Absence Reason Code" 
                                                    SortExpression="AbsenceReasonFullName" UniqueName="AbsenceReasonFullName">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
								                    <HeaderStyle Width="200px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
							                    </telerik:GridBoundColumn> 
                                                <telerik:GridTemplateColumn DataField="LastUpdateUser" HeaderText="Last Update User" 
                                                    SortExpression="LastUpdateUser" UniqueName="LastUpdateUser">
								                    <HeaderStyle Width="170px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								                    <ItemTemplate>
									                    <div class="columnEllipsis" style="width: 160px; text-align: left;">
										                    <asp:Literal ID="litLastUpdateUser" runat="server" Text='<%# Eval("LastUpdateUser") %>' />
									                    </div>
								                    </ItemTemplate>
							                    </telerik:GridTemplateColumn> 
                                                <telerik:GridBoundColumn DataField="LastUpdateTime" HeaderText="Last Update Time"
                                                    DataFormatString="{0:dd-MMM-yyyy h:mm tt}" DataType="System.DateTime" 
                                                    ReadOnly="True" SortExpression="LastUpdateTime" UniqueName="LastUpdateTime">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma"></HeaderStyle>
                                                </telerik:GridBoundColumn>                                                  
                                            </Columns>
                                        </MasterTableView>
                                        <ClientSettings AllowColumnsReorder="False" EnableRowHoverStyle="true">
                                            <Selecting AllowRowSelect="True" UseClientSelectColumnOnly="True" />
                                            <Scrolling AllowScroll="true" UseStaticHeaders="true" SaveScrollPosition="true" ScrollHeight="" />
			                                <Resizing AllowColumnResize="true" />   
                                        </ClientSettings>
                                        <HeaderStyle Font-Bold="True" Font-Size="8pt" />
                                        <ActiveItemStyle Font-Names="Tahoma" Font-Size="9pt" />
                                        <ItemStyle Font-Names="Tahoma" Font-Size="9pt" Wrap="True" />
                                        <SelectedItemStyle Font-Names="Tahoma" Font-Size="9pt" />
                                        <FilterMenu EnableImageSprites="False">
                                            <WebServiceSettings>
                                                <ODataSettings InitialContainerName="">
                                                </ODataSettings>
                                            </WebServiceSettings>
                                        </FilterMenu>
                                        <HeaderContextMenu CssClass="GridContextMenu GridContextMenu_Windows7">
                                            <WebServiceSettings>
                                                <ODataSettings InitialContainerName="">
                                                </ODataSettings>
                                            </WebServiceSettings>
                                        </HeaderContextMenu>
                                    </telerik:RadGrid>
                                </td>
                            </tr>
                        </table>
                    </asp:Panel>   
                </telerik:RadPageView>

                <telerik:RadPageView ID="LeaveHistoryView" runat="server">
                    <asp:Panel ID="panLeaveHistory" runat="server" BorderStyle="None" style="padding-left: 10px; padding-right: 10px; padding-top: 5px; margin: 0px;">
                        <table border="0" style="width: 100%; text-align: left; margin-top: 0px; table-layout: fixed;">
                            <tr>
                                <td>
                                    <telerik:RadGrid ID="gridLeaveHistory" runat="server"
                                        AllowSorting="true" AllowMultiRowSelection="true"
                                        PageSize="10" AutoGenerateColumns="false" Skin="Silk" 
                                        GridLines="None" Width="100%" Height="" CellSpacing="0"
                                        onpageindexchanged="gridLeaveHistory_PageIndexChanged" 
                                        onpagesizechanged="gridLeaveHistory_PageSizeChanged" 
                                        onsortcommand="gridLeaveHistory_SortCommand" 
                                        onitemcommand="gridLeaveHistory_ItemCommand" 
                                        onitemdatabound="gridLeaveHistory_ItemDataBound" 
                                        Font-Names="Tahoma" Font-Size="9pt" Font-Bold="False" 
                                        AllowPaging = "true" BorderStyle="Outset" BorderWidth="1px" AllowCustomPaging="True" VirtualItemCount="1">
                                        <ExportSettings ExportOnlyData="true" IgnorePaging="true" OpenInNewWindow="true" FileName="LeaveHistoryList" HideStructureColumns="true">
                                            <Pdf PageHeight="210mm" PageWidth="310mm" PageTitle="Leave History List" DefaultFontFamily="Arial Unicode MS"
                                            PageBottomMargin="20mm" PageTopMargin="20mm" PageLeftMargin="20mm" PageRightMargin="20mm" PaperSize="A4" AllowPrinting="true" />
                                        </ExportSettings>
                                        <AlternatingItemStyle Font-Names="Tahoma" Font-Size="9pt" Wrap="True" />
                                        <MasterTableView DataKeyNames="AutoID" ClientDataKeyNames="AutoID" 
                                            NoMasterRecordsText="No changes for Leave." 
                                            TableLayout="Fixed" PagerStyle-AlwaysVisible="True" Font-Names="Tahoma" 
                                            Font-Size="9pt">
                                            <PagerStyle AlwaysVisible="True" Mode="NextPrevAndNumeric" />
                                            <CommandItemSettings ExportToPdfText="Export to PDF"></CommandItemSettings>
			                                <RowIndicatorColumn>
				                                <HeaderStyle Width="20px" />
			                                </RowIndicatorColumn>
			                                <ExpandCollapseColumn>
				                                <HeaderStyle Width="20px" />
			                                </ExpandCollapseColumn>
                                            <Columns>   
                                                <telerik:GridBoundColumn DataField="AutoID" DataType="System.Int32" HeaderText="Ref. ID" 
                                                    ReadOnly="True" SortExpression="AutoID" UniqueName="AutoID">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Width="80px" Font-Bold="True" />
                                                    <ItemStyle Font-Bold="true" ForeColor="Purple" />
                                                </telerik:GridBoundColumn> 
                                                <telerik:GridBoundColumn DataField="LeaveEmpNo" DataType="System.Int32" HeaderText="Emp. No." 
                                                    ReadOnly="True" SortExpression="LeaveEmpNo" UniqueName="LeaveEmpNo">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Width="90px" Font-Bold="True" />
                                                </telerik:GridBoundColumn> 
                                                <telerik:GridBoundColumn DataField="FromDate" HeaderText="From Date"
                                                    DataFormatString="{0:dd-MMM-yyyy}" DataType="System.DateTime" 
                                                    ReadOnly="True" SortExpression="FromDate" UniqueName="FromDate">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Width="90px" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma"></HeaderStyle>
                                                </telerik:GridBoundColumn>   
                                                <telerik:GridBoundColumn DataField="ToDate" HeaderText="To Date"
                                                    DataFormatString="{0:dd-MMM-yyyy}" DataType="System.DateTime" 
                                                    ReadOnly="True" SortExpression="ToDate" UniqueName="ToDate">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Width="90px" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma"></HeaderStyle>
                                                </telerik:GridBoundColumn>
                                                <telerik:GridBoundColumn DataField="LeaveFullName" DataType="System.String" HeaderText="Leave Code" 
                                                    ReadOnly="True" SortExpression="LeaveFullName" UniqueName="LeaveFullName">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Font-Bold="True" />
                                                </telerik:GridBoundColumn> 
                                            </Columns>
                                        </MasterTableView>
                                        <ClientSettings AllowColumnsReorder="False" EnableRowHoverStyle="true">
                                            <Selecting AllowRowSelect="True" UseClientSelectColumnOnly="True" />
                                            <Scrolling AllowScroll="true" UseStaticHeaders="true" SaveScrollPosition="true" ScrollHeight="" />
			                                <Resizing AllowColumnResize="true" />   
                                        </ClientSettings>
                                        <HeaderStyle Font-Bold="True" Font-Size="8pt" />
                                        <ActiveItemStyle Font-Names="Tahoma" Font-Size="9pt" />
                                        <ItemStyle Font-Names="Tahoma" Font-Size="9pt" Wrap="True" />
                                        <SelectedItemStyle Font-Names="Tahoma" Font-Size="9pt" />
                                        <FilterMenu EnableImageSprites="False">
                                            <WebServiceSettings>
                                                <ODataSettings InitialContainerName="">
                                                </ODataSettings>
                                            </WebServiceSettings>
                                        </FilterMenu>
                                        <HeaderContextMenu CssClass="GridContextMenu GridContextMenu_Windows7">
                                            <WebServiceSettings>
                                                <ODataSettings InitialContainerName="">
                                                </ODataSettings>
                                            </WebServiceSettings>
                                        </HeaderContextMenu>
                                    </telerik:RadGrid>
                                </td>
                            </tr>
                        </table>
                    </asp:Panel>   
                </telerik:RadPageView>
            </telerik:RadMultiPage>
        </asp:Panel>

        
    </asp:Panel>        

    <asp:Panel ID="panHidden" runat="server" style="display: none;">
        <input type="hidden" id="hidFormAccess" runat="server" value="" />
        <input type="hidden" id="hidFormCode" runat="server" value="" />
        <input type="hidden" id="hidForm" runat="server" value="" />
        <input type="hidden" id="hidSearchUrl" runat="server" value="" />
        <input type="hidden" id="hidRequestFlag" runat="server" value="0" />     
        <asp:TextBox ID="txtGeneric" runat="server" Width="100%" Visible="false" />    
        <telerik:RadButton ID="btnRebind" runat="server" Text="" Skin="Office2010Silver" CausesValidation="false" onclick="btnRebind_Click" />
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
                <telerik:AjaxSetting AjaxControlID="btnGet">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting> 
                <telerik:AjaxSetting AjaxControlID="btnSearch">
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
                <telerik:AjaxSetting AjaxControlID="btnRebind">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>    
                <telerik:AjaxSetting AjaxControlID="gridTimesheetCorrection">
				    <UpdatedControls>
                        <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>   
                <telerik:AjaxSetting AjaxControlID="gridShiftPatternHistory">
				    <UpdatedControls>
                        <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>                   
                <telerik:AjaxSetting AjaxControlID="gridAbsenceHistory">
				    <UpdatedControls>
                        <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>                   
                <telerik:AjaxSetting AjaxControlID="gridLeaveHistory">
				    <UpdatedControls>
                        <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>                   
                <telerik:AjaxSetting AjaxControlID="tabMain">
				    <UpdatedControls>
                        <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panBody" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>                                     
            </AjaxSettings>
        </telerik:RadAjaxManager>
        <telerik:RadAjaxLoadingPanel ID="loadingPanel" runat="server" Skin="Office2010Silver"></telerik:RadAjaxLoadingPanel>
    </asp:Panel>
</asp:Content>
