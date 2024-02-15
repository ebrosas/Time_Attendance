<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/TASMaster.Master" AutoEventWireup="true" CodeBehind="OvertimeMealVoucherEntry.aspx.cs" 
    Inherits="GARMCO.AMS.TAS.UI.Views.UserFunctions.OvertimeMealVoucherEntry" StylesheetTheme="Standard" %>

<%@ MasterType VirtualPath="~/Views/Shared/TASMaster.Master" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <title>Employee Overtime Entry</title>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="mainContent" runat="server">
   <%-- <style type="text/css">
        .RadGrid_Silk .rgCommandRow
        {
            color: Transparent !important;
        }
    </style>--%>
    <telerik:RadFormDecorator ID="formDecor" runat="server" DecoratedControls="Buttons" Skin="Office2010Silver" />
    <table border="0" style="width: 100%; text-align: left; margin-top: 5px; margin-left: 5px;">
        <tr>
            <td colspan="2" style="padding-left: 5px;">
                <table border="0" style="width: 100%; text-align: left;">
                    <tr>
                        <td style="width: 50px; text-align: right; padding-right: 5px;" rowspan="2">
                            <img alt="" src="../../Images/overtime_icon.png" />
                        </td>
                        <td id="tdPageTitle" runat="server" class="PageTitleLabel" style="vertical-align: bottom; padding-left: 5px; width: 900px; font-size: 11pt;">
                            Employee Overtime Entry
                        </td>
                        <td style="width: 50px;">
                            <asp:LinkButton ID="lnkMoveUp" runat="server" />
                        </td>
                        <td />
                    </tr>
                    <tr>
                        <td class="PageDescriptionHeader" style="text-align: left; vertical-align: top; padding-left: 5px; margin: 0px;">
                            Allows a Department Secretary or Clerk to submit an overtime request
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

    <asp:Panel ID="panMain" runat="server" style="margin-top: 0px; padding-bottom: 40px;"> 
        <asp:Panel ID="panSearchCriteria" runat="server" BorderStyle="None" style="padding: 0px; margin: 0px;">
            <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                <tr style="height: 23px;">
                    <td style="width: 120px;">
                        <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                            <tr style="margin: 0px; padding: 0px;">                                
                                <td class="LabelBold" style="width: auto; padding-right: 0px;">
                                    <asp:CustomValidator ID="cusValPayrollYear" runat="server" ControlToValidate="txtGeneric" 
                                        CssClass="LabelValidationError" Display="Dynamic" 
                                        ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                                        ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                                    Payroll Period
                                </td>
                                <td style="width: 20px; text-align: left; ">
                                     <asp:CheckBox ID="chkPayPeriod" runat="server" Text="" AutoPostBack="True" 
                                        OnCheckedChanged="chkPayPeriod_CheckedChanged" />
                                </td>
                            </tr>
                        </table>
                    </td>
                    <td style="width: 170px; padding-left: 0px;">
                        <table border="0" style="width: 100%; text-align: left; margin: 0px; padding: 0px; table-layout: fixed;">
                            <tr>
                                <td style="width: 93px; padding-left: 0px;">                                    
                                    <telerik:RadComboBox ID="cboMonth" runat="server"
                                        DropDownWidth="105px" 
                                        HighlightTemplatedItems="True" 
                                        Skin="Office2010Silver" 
                                        Width="93px" 
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
                    <td class="LabelBold" style="width: 120px;">
                        Employee No.
                    </td>
                    <td style="width: 230px; padding-left: 0px;">
                        <table id="tdEmployee" runat="server" border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed; border-spacing: 0px;">
                            <tr style="vertical-align: top; margin: 0px; padding: 0px;">
                                <td style="width: 110px; text-align: left;">
                                    <telerik:RadNumericTextBox ID="txtEmpNo" runat="server" width="100%" 
                                        MinValue="0" ToolTip="(Note: Employee No. must start with 1000. Example: 10003632)" 
                                        Skin="Office2010Silver" DataType="System.Int32" MaxLength="8" MaxValue="99999999" 
                                        EmptyMessage="1000xxxx">
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
                    <td />
                </tr>    
                <tr style="height: 23px;">
                    <td class="LabelBold">
                        <asp:CustomValidator ID="cusValStartDate" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                        Start Date            
                    </td>
                    <td style="padding-left: 0px;">
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
                    <td class="LabelBold">
                        Cost Center
                    </td>
                    <td style="padding-left: 1px;">
                        <telerik:RadComboBox ID="cboCostCenter" runat="server"
							DropDownWidth="300px" 
							HighlightTemplatedItems="True" 
							MarkFirstMatch="false" 
							Skin="Office2010Silver" 
							Width="100%"     
                            Height="200px"                        
							EmptyMessage="Select Cost Center"
							EnableLoadOnDemand="false"
							EnableVirtualScrolling="true" 
                            AutoPostBack="False" Font-Names="Tahoma" Font-Size="9pt" />   

                        <%--<telerik:RadComboBox ID="cboCostCenter" runat="server" 
                            DropDownWidth="330px"    
                            Width="100%" Height="200px"                                
                            Filter="Contains" Skin="Office2010Silver" 
                            EmptyMessage="Select Cost Center"                               
                            HighlightTemplatedItems="True" 
                            MarkFirstMatch="True" EnableVirtualScrolling="true">
						    <HeaderTemplate>
							    <table border="0" style="width: 100%">
								    <tr>
									    <td style="width: 70px;">
										    Cost Center
									    </td>
									    <td>
										    Cost Center Name
									    </td>
								    </tr>
							    </table>
						    </HeaderTemplate>
						    <ItemTemplate>
							    <table border="0" style="width: 100%">
								    <tr>
									    <td style="width: 70px;">
										    <%# DataBinder.Eval(Container.DataItem, "CostCenter")%>
									    </td>
									    <td>
										    <%# DataBinder.Eval(Container.DataItem, "CostCenterName")%>
									    </td>
								    </tr>
							    </table>
						    </ItemTemplate>
					    </telerik:RadComboBox>   --%>
                    </td>                    
                    <td />
                </tr>    
                <tr style="height: 23px;">
                    <td class="LabelBold">
                        End Date
                    </td>
                    <td style="padding-left: 0px;">
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
                    <td class="LabelBold">
                        <asp:CustomValidator ID="cusValFilterOption" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                        Display Option
                    </td>
                    <td style="padding-left: 1px;">
                        <telerik:RadComboBox ID="cboFilterOption" runat="server"
                            DropDownWidth="220px" 
                            HighlightTemplatedItems="True" 
                            EmptyMessage="Select Option"
                            Skin="Office2010Silver" 
                            Width="100%"
                            EnableVirtualScrolling="True" AutoPostBack="True" 
                            OnSelectedIndexChanged="cboFilterOption_SelectedIndexChanged" />
                    </td>                    
                    <td />
                </tr>    
                <tr style="height: 23px;">
                    <td class="LabelBold" style="padding-right: 5px;">
                        <asp:CustomValidator ID="cusValButton" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                    </td>
                    <td colspan="3" style="padding-left: 0px; padding-top: 7px;">                        
                        <telerik:RadButton ID="btnSearch" runat="server" Text="Search" ToolTip="Search matching database records" Width="75px" 
                            CssClass="RadButtonStyle" ValidationGroup="valPrimary" Font-Size="9pt" OnClick="btnSearch_Click" Skin="Office2010Silver" />
                        <telerik:RadButton ID="btnReset" runat="server" Text="Reset" ToolTip="Reset filter criterias" Width="75px" 
                            CssClass="RadButtonStyle" ValidationGroup="valPrimary" Font-Size="9pt" OnClick="btnReset_Click" Skin="Office2010Silver" />                                                
                        <telerik:RadButton ID="btnSave" runat="server" Text="Submit" ToolTip="Submit request for approval" Width="75px" 
                            CssClass="RadButtonStyle" ValidationGroup="valPrimary" Font-Size="9pt" OnClick="btnSave_Click" Skin="Office2010Silver" />                                                
                    </td>
                    <td />
                </tr>                
            </table>
        </asp:Panel>

        <asp:Panel ID="panGrid" runat="server" BorderStyle="None" style="padding-left: 15px; padding-right: 20px; margin: 0px;">
             <table border="0" style="width: 100%; text-align: left; margin-top: 5px; table-layout: fixed; display: none;">
                <tr>
                    <td style="text-align: left; color: Purple; font-weight: bold; font-size: 9pt; padding-left: 5px;">
                        <asp:Label ID="lblRecordCount" runat="server" Text="0 record found" Width="100%" />                         
                    </td>
                </tr>
            </table>
            <table id="tblGrid" runat="server" border="0" style="width: 100%; text-align: left; margin-top: 2px; table-layout: fixed;">
                <tr>
                    <td>
                        <telerik:RadGrid ID="gridSearchResults" runat="server"
                            AllowSorting="true" AllowMultiRowSelection="true"
                            PageSize="10" AutoGenerateColumns="false" Skin="Silk" 
                            GridLines="None" Width="100%" Height="" CellSpacing="0"
                            onpageindexchanged="gridSearchResults_PageIndexChanged" 
                            onpagesizechanged="gridSearchResults_PageSizeChanged" 
                            onsortcommand="gridSearchResults_SortCommand" 
                            onitemcommand="gridSearchResults_ItemCommand" 
                            onitemdatabound="gridSearchResults_ItemDataBound" 
                            OnPreRender="gridSearchResults_PreRender"
                            Font-Names="Tahoma" Font-Size="9pt" Font-Bold="False" 
                            AllowPaging = "true" BorderStyle="Outset" BorderWidth="1px" AllowCustomPaging="True" VirtualItemCount="1">
                            <ExportSettings ExportOnlyData="true" IgnorePaging="true" OpenInNewWindow="true" FileName="OvertimeRecords" HideStructureColumns="true" Excel-DefaultCellAlignment="Left">
                                <Pdf PageHeight="210mm" PageWidth="310mm" PageTitle="Employee Attendance List" DefaultFontFamily="Arial Unicode MS"
                                PageBottomMargin="20mm" PageTopMargin="20mm" PageLeftMargin="20mm" PageRightMargin="20mm" PaperSize="A4" AllowPrinting="true" />
                            </ExportSettings>
                            <AlternatingItemStyle Font-Names="Tahoma" Font-Size="9pt" Wrap="True" />
                            <MasterTableView DataKeyNames="AutoID" ClientDataKeyNames="AutoID" 
                                NoMasterRecordsText="No overtime record found." 
                                TableLayout="Fixed" PagerStyle-AlwaysVisible="True" Font-Names="Tahoma" 
                                Font-Size="9pt" CommandItemDisplay="Top">
                                <PagerStyle AlwaysVisible="True" Mode="NextPrevAndNumeric" />
                                <CommandItemSettings ExportToExcelText="Download data to Excel" ShowRefreshButton="true" ShowExportToExcelButton="true" ShowAddNewRecordButton="false" ExportToPdfText="Export to PDF"></CommandItemSettings>
			                    <RowIndicatorColumn>
				                    <HeaderStyle Width="20px" />
			                    </RowIndicatorColumn>
			                    <ExpandCollapseColumn>
				                    <HeaderStyle Width="20px" />
			                    </ExpandCollapseColumn>
                                <Columns>         
                                    <telerik:GridTemplateColumn UniqueName="CancelButton" ItemStyle-HorizontalAlign="Center" HeaderText="">
                                        <HeaderStyle Width="25px" HorizontalAlign="Center" />
                                        <ItemStyle HorizontalAlign="Center" VerticalAlign="Middle" />
                                        <ItemTemplate>
                                            <asp:ImageButton ID="imgCancelOT" runat="server" ImageUrl="~/Images/delete_enabled_icon.png" ToolTip="Cancel overtime request" OnClick="imgCancelOT_Click" />
                                        </ItemTemplate>
                                    </telerik:GridTemplateColumn>                                    
                                    <telerik:GridButtonColumn ButtonType="LinkButton" CommandName="Select" Text="View History" UniqueName="ViewHistoryLinkButton" 
                                        HeaderTooltip="View approval history" Visible="false">
                                        <HeaderStyle Width="93px" HorizontalAlign="Center" Font-Bold="true" Font-Size="9pt" />
                                        <ItemStyle HorizontalAlign="Center" Font-Bold="true" Font-Size="9pt" ForeColor="Blue" />
					                </telerik:GridButtonColumn>                                                                        
                                    <telerik:GridBoundColumn DataField="DT" HeaderText="Date"
                                        DataFormatString="{0:dd-MMM-yyyy}" DataType="System.DateTime" 
                                        ReadOnly="True" SortExpression="DT" UniqueName="DT">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="90px" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma"></HeaderStyle>
                                    </telerik:GridBoundColumn>                                    
                                    <telerik:GridBoundColumn DataField="EmpNo" DataType="System.Int32" HeaderText="Emp. No." 
                                        ReadOnly="True" SortExpression="EmpNo" UniqueName="EmpNo">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="80px" Font-Bold="True" />
                                        <ItemStyle Font-Bold="true" ForeColor="Purple" HorizontalAlign="Left" />
                                    </telerik:GridBoundColumn> 
                                                                        
                                    <telerik:GridTemplateColumn DataField="EmpName" HeaderText="Employee Name" 
                                        SortExpression="EmpName" UniqueName="EmpName">
								        <HeaderStyle Width="230px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								        <ItemTemplate>
									        <div class="columnEllipsis" style="width: 220px; text-align: left;">
										        <asp:Literal ID="litEmpName" runat="server" Text='<%# Eval("EmpName") %>' />
									        </div>
								        </ItemTemplate>
							        </telerik:GridTemplateColumn> 
                                    <telerik:GridBoundColumn DataField="EmpName" DataType="System.String" HeaderText="Employee Name" Visible="false" 
                                        ReadOnly="True" SortExpression="EmpName" UniqueName="EmpNameExport">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="260px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
                                    </telerik:GridBoundColumn>

                                    <telerik:GridBoundColumn DataField="GradeCode" DataType="System.Int32" HeaderText="Pay Grade" 
                                        ReadOnly="True" SortExpression="GradeCode" UniqueName="GradeCode">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="50px" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" HorizontalAlign="Center" />
                                        <ItemStyle HorizontalAlign="Center" />
                                    </telerik:GridBoundColumn> 
                                    <telerik:GridBoundColumn DataField="ShiftPatCode" DataType="System.String" HeaderText="Shift Pat." 
                                        ReadOnly="True" SortExpression="ShiftPatCode" UniqueName="ShiftPatCode">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="55px" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" HorizontalAlign="Center" />
                                        <ItemStyle HorizontalAlign="Center" />
                                    </telerik:GridBoundColumn> 
                                    <telerik:GridBoundColumn DataField="ShiftCode" DataType="System.String" HeaderText="Schd. Shift" 
                                        ReadOnly="True" SortExpression="ShiftCode" UniqueName="ShiftCode">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="55px" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" HorizontalAlign="Center" />
                                        <ItemStyle HorizontalAlign="Center" />
                                    </telerik:GridBoundColumn> 
                                    <telerik:GridBoundColumn DataField="ActualShiftCode" DataType="System.String" HeaderText="Act. Shift" 
                                        ReadOnly="True" SortExpression="ActualShiftCode" UniqueName="ActualShiftCode">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="55px" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" HorizontalAlign="Center" />
                                        <ItemStyle HorizontalAlign="Center" ForeColor="Green" Font-Bold="true" />
                                    </telerik:GridBoundColumn> 

                                    <telerik:GridBoundColumn DataField="dtINLastRow" HeaderText="Time In"
                                        DataFormatString="{0:HH:mm tt}" DataType="System.DateTime" 
                                        ReadOnly="True" SortExpression="dtINLastRow" UniqueName="dtINLastRow">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" Width="70px"></HeaderStyle>
                                    </telerik:GridBoundColumn>
                                    <telerik:GridBoundColumn DataField="dtIN" HeaderText="Time In" Visible="false"
                                        DataFormatString="{0:HH:mm tt}" DataType="System.DateTime" 
                                        ReadOnly="True" SortExpression="dtIN" UniqueName="dtIN">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" Width="70px"></HeaderStyle>
                                    </telerik:GridBoundColumn> 
                                    <telerik:GridBoundColumn DataField="dtIN" HeaderText="Time In"
                                        DataFormatString="{0:HH:mm}" DataType="System.DateTime" Visible="false" 
                                        ReadOnly="True" SortExpression="dtIN" UniqueName="dtINExport">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" Width="70px"></HeaderStyle>
                                    </telerik:GridBoundColumn> 

                                    <telerik:GridBoundColumn DataField="dtOUT" HeaderText="Time Out"
                                        DataFormatString="{0:HH:mm tt}" DataType="System.DateTime" 
                                        ReadOnly="True" SortExpression="dtOUT" UniqueName="dtOUT">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" Width="70px"></HeaderStyle>
                                    </telerik:GridBoundColumn> 
                                    <telerik:GridBoundColumn DataField="dtOUT" HeaderText="Time Out"
                                        DataFormatString="{0:HH:mm}" DataType="System.DateTime" Visible="false" 
                                        ReadOnly="True" SortExpression="dtOUT" UniqueName="dtOUTExport">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" Width="70px"></HeaderStyle>
                                    </telerik:GridBoundColumn>                                                                         

                                    <telerik:GridBoundColumn DataField="OTStartTime" HeaderText="OT Start Time"
                                        DataFormatString="{0:HH:mm tt}" DataType="System.DateTime" 
                                        ReadOnly="True" SortExpression="OTStartTime" UniqueName="OTStartTime">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" Width="75px" HorizontalAlign="Center"></HeaderStyle>
                                        <ItemStyle HorizontalAlign="Center" ForeColor="Red" />
                                    </telerik:GridBoundColumn> 
                                    <telerik:GridBoundColumn DataField="OTStartTime" HeaderText="OT Start Time"
                                        DataFormatString="{0:HH:mm}" DataType="System.DateTime" Visible="false" 
                                        ReadOnly="True" SortExpression="OTStartTime" UniqueName="OTStartTimeExport">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" Width="90px" HorizontalAlign="Center"></HeaderStyle>
                                        <ItemStyle HorizontalAlign="Center" ForeColor="Red" />
                                    </telerik:GridBoundColumn> 

                                    <telerik:GridBoundColumn DataField="OTEndTime" HeaderText="OT End Time"
                                        DataFormatString="{0:HH:mm tt}" DataType="System.DateTime" 
                                        ReadOnly="True" SortExpression="OTEndTime" UniqueName="OTEndTime">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" Width="75px" HorizontalAlign="Center"></HeaderStyle>
                                        <ItemStyle HorizontalAlign="Center" ForeColor="Red" />
                                    </telerik:GridBoundColumn>
                                    <telerik:GridBoundColumn DataField="OTEndTime" HeaderText="OT End Time"
                                        DataFormatString="{0:HH:mm}" DataType="System.DateTime" Visible="false"  
                                        ReadOnly="True" SortExpression="OTEndTime" UniqueName="OTEndTimeExport">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" Width="90px" HorizontalAlign="Center"></HeaderStyle>
                                        <ItemStyle HorizontalAlign="Center" ForeColor="Red" />
                                    </telerik:GridBoundColumn>

                                    <telerik:GridBoundColumn DataField="OTType" DataType="System.String" HeaderText="OT Type" 
                                        ReadOnly="True" SortExpression="OTType" UniqueName="OTType">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" Width="50px" HorizontalAlign="Center"></HeaderStyle>
                                        <ItemStyle HorizontalAlign="Center" />
                                    </telerik:GridBoundColumn> 

                                    <telerik:GridTemplateColumn DataField="" HeaderText="Grant Overtime?" 
                                        SortExpression="AllowOvertime" UniqueName="AllowOvertime" Visible="false">
								        <HeaderStyle Width="90px" HorizontalAlign="Center" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
                                        <ItemStyle HorizontalAlign="Center" />
                                        <HeaderTemplate>
                                            <asp:CheckBox ID="chkAllowOTAll" runat="server" AutoPostBack="true" 
                                                Checked="False" Text="" Font-Names="Tahoma" Font-Size="8pt" Font-Bold="true"
                                                OnCheckedChanged="chkOTApprove_CheckedChanged" />
                                        </HeaderTemplate>
								        <ItemTemplate>
                                            <asp:CheckBox ID="chkAllowOT" runat="server" AutoPostBack="true" OnCheckedChanged="chkAllowOT_CheckedChanged" />
								        </ItemTemplate>
							        </telerik:GridTemplateColumn>     

                                    <telerik:GridTemplateColumn DataField="OTApprovalDesc" HeaderText="OT Approved?" 
                                        SortExpression="OTApprovalDesc" UniqueName="OTApprovalDesc">
								        <HeaderStyle Width="80px" HorizontalAlign="Center" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
                                        <HeaderTemplate>
                                            <asp:CheckBox ID="chkOTApprove" runat="server" AutoPostBack="true" 
                                                Checked="False" Text="" Font-Names="Tahoma" Font-Size="8pt" Font-Bold="true"
                                                OnCheckedChanged="chkOTApprove_CheckedChanged" />
                                        </HeaderTemplate>
								        <ItemTemplate>     
                                            <telerik:RadComboBox ID="cboOTApprovalType" runat="server"
							                    DropDownWidth="60px" 
							                    HighlightTemplatedItems="True" 
							                    MarkFirstMatch="false" 
							                    Skin="Black" 
							                    Width="100%"     
							                    EmptyMessage=""
							                    EnableLoadOnDemand="false"
							                    EnableVirtualScrolling="true" 
                                                Text='<%# Eval("OTApprovalDesc")%>'
                                                AutoPostBack="true" OnSelectedIndexChanged="cboOTApprovalType_SelectedIndexChanged" 
                                                Font-Names="Verdana" Font-Size="9pt" ForeColor="WhiteSmoke" Font-Bold="true">
                                                <Items>                                                    
                                                    <telerik:RadComboBoxItem runat="server" Text="Yes" Value="Y" Font-Names="Verdana" ForeColor="YellowGreen" Font-Bold="true" />
                                                    <telerik:RadComboBoxItem runat="server" Text="No" Value="N" Font-Names="Verdana" ForeColor="Red" Font-Bold="true" />
                                                    <telerik:RadComboBoxItem runat="server" Text="Hold" Value="0" Font-Names="Verdana" ForeColor="Orange" Font-Bold="true" />
                                                </Items>
                                            </telerik:RadComboBox>                                       
								        </ItemTemplate>
                                        <ItemStyle HorizontalAlign="Center" />
							        </telerik:GridTemplateColumn> 
                                    <telerik:GridBoundColumn DataField="OTApprovalDesc" DataType="System.String" HeaderText="OT Approved?" Visible="false" 
                                        ReadOnly="True" SortExpression="OTApprovalDesc" UniqueName="OTApprovalDescExport">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="80px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
                                    </telerik:GridBoundColumn>

                                    <telerik:GridBoundColumn DataField="OTApprovalCode" DataType="System.String" HeaderText="OTApprovalCode" 
                                        ReadOnly="True" SortExpression="OTApprovalCode" UniqueName="OTApprovalCode" Display="false">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="80px" Font-Bold="True" />
                                    </telerik:GridBoundColumn> 
                                    <telerik:GridTemplateColumn DataField="MealVoucherEligibility" HeaderText="Meal Voucher Approved?" 
                                        SortExpression="MealVoucherEligibility" UniqueName="MealVoucherEligibility" Visible="false">
								        <HeaderStyle Width="110px" HorizontalAlign="Center" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								        <ItemTemplate> 
                                            <telerik:RadComboBox ID="cboMealVoucherEligibility" runat="server"
							                    DropDownWidth="100px" 
							                    HighlightTemplatedItems="True" 
							                    MarkFirstMatch="false" 
							                    Skin="Office2010Silver" 
							                    Width="70px"     
							                    EmptyMessage=""
							                    EnableLoadOnDemand="false"
							                    EnableVirtualScrolling="true" 
                                                Text='<%# Eval("MealVoucherEligibility")%>'
                                                AutoPostBack="true"  OnSelectedIndexChanged="cboMealVoucherEligibility_SelectedIndexChanged"
                                                Font-Names="Tahoma" Font-Size="9pt" Enabled="false">
                                                <Items>
                                                    <telerik:RadComboBoxItem runat="server" Text="-" Value="Y" />
                                                    <telerik:RadComboBoxItem runat="server" Text="Yes" Value="YA" />
                                                    <telerik:RadComboBoxItem runat="server" Text="No" Value="N" />
                                                </Items>
                                            </telerik:RadComboBox>                                       
								        </ItemTemplate>
                                        <ItemStyle HorizontalAlign="Center" />
							        </telerik:GridTemplateColumn> 
                                    <telerik:GridBoundColumn DataField="MealVoucherEligibilityCode" DataType="System.String" HeaderText="MealVoucherEligibilityCode" 
                                        ReadOnly="True" SortExpression="MealVoucherEligibilityCode" UniqueName="MealVoucherEligibilityCode" Display="false">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" Width="110px" HorizontalAlign="Center"></HeaderStyle>
                                    </telerik:GridBoundColumn> 

                                    <telerik:GridTemplateColumn DataField="OTDurationHour" DataType="System.String" HeaderText="OT Duration <br/> (hh:mm)" 
                                        ReadOnly="True" SortExpression="OTDurationHour" UniqueName="OTDurationHour">
                                        <HeaderStyle Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" Width="95px" HorizontalAlign="Center"></HeaderStyle>
                                        <ItemTemplate>
                                            <telerik:RadLabel ID="lblDuration" runat="server" Text="12:00" Width="70px" style="text-align: center; color: red;" Visible="false" />
                                            <telerik:RadNumericTextBox ID="txtDuration" runat="server" width="70px" style="text-align: center;" 
                                                MinValue="0" Skin="Office2010Silver" DataType="System.Int32" MaxLength="4" Enabled="false" AutoPostBack="true" OnTextChanged="txtDuration_TextChanged" 
                                                EmptyMessage="HH:mm" Text='<%# Eval("OTDurationHour") %>' ToolTip="(Note: The maximum OT duration than can be entered is equal to the Total Work Duration.)"> 
                                                <NumberFormat AllowRounding="False" DecimalDigits="0" GroupSeparator=":" GroupSizes="2" />
                                            </telerik:RadNumericTextBox> 
								        </ItemTemplate>
                                        <ItemStyle HorizontalAlign="Center" />
                                    </telerik:GridTemplateColumn> 
                                    <telerik:GridBoundColumn DataField="OTDurationText" DataType="System.String" HeaderText="OT Duration" Visible="false" 
                                        ReadOnly="True" SortExpression="OTDurationText" UniqueName="OTDurationHourExport">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="95px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
                                    </telerik:GridBoundColumn>

                                    <telerik:GridBoundColumn DataField="OTDurationHourClone" DataType="System.Int32" HeaderText="OT Duration Clone" Display="false" 
                                        ReadOnly="True" SortExpression="OTDurationHourClone" UniqueName="OTDurationHourClone">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="80px" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" HorizontalAlign="Center" />
                                        <ItemStyle HorizontalAlign="Center" />
                                    </telerik:GridBoundColumn> 

                                    <telerik:GridTemplateColumn DataField="OTReason" HeaderText="OT Reason" DataType="System.String" 
                                        SortExpression="OTReason" UniqueName="OTReason">
								        <HeaderStyle Width="200px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								        <ItemTemplate>     
                                            <telerik:RadComboBox ID="cboOTReason" runat="server"
							                    DropDownWidth="230px" 
							                    HighlightTemplatedItems="True" 
							                    MarkFirstMatch="true" 
							                    Skin="Office2010Silver" 
							                    Width="100%"  
							                    EmptyMessage="Select OT Reason"
							                    EnableLoadOnDemand="true"
							                    EnableVirtualScrolling="true" 
                                                Text='<%# Eval("OTReason")%>'
                                                Font-Names="Tahoma" 
                                                Font-Size="9pt"
                                                AutoPostBack="true"
                                                Filter="None"
                                                BackColor="Yellow"
                                                Enabled="false"
                                                ToolTip="(Note: OT Reason is mandatory if OT or Meal Voucher is approved or rejected.)"
                                                OnItemsRequested="cboOTReason_ItemsRequested"
                                                OnSelectedIndexChanged="cboOTReason_SelectedIndexChanged">
                                            </telerik:RadComboBox>                                       
								        </ItemTemplate>
							        </telerik:GridTemplateColumn> 
                                    <telerik:GridBoundColumn DataField="OTReason" DataType="System.String" HeaderText="OT Reason" Visible="false" 
                                        ReadOnly="True" SortExpression="OTReason" UniqueName="OTReasonExport">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="200px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
                                    </telerik:GridBoundColumn>
                                    <telerik:GridBoundColumn DataField="OTReasonCode" HeaderText="OTReasonCode" DataType="System.String" 
                                        SortExpression="OTReasonCode" UniqueName="OTReasonCode" Display="false">
								        <HeaderStyle Width="100px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
							        </telerik:GridBoundColumn> 
                                    <telerik:GridTemplateColumn DataField="AttendanceRemarks" HeaderText="Remarks" DataType="System.String" 
                                        SortExpression="AttendanceRemarks" UniqueName="AttendanceRemarks">
								        <HeaderStyle Width="200px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								        <ItemTemplate>
                                            <asp:TextBox ID="txtRemarks" runat="server" MaxLength="1000" TextMode="MultiLine" Rows="2" Enabled="false" BackColor="Yellow" 
                                                Text='<%# Eval("AttendanceRemarks") %>' ToolTip="(Note: Maximum text input is 1000 chars.)" />
								        </ItemTemplate>
							        </telerik:GridTemplateColumn> 
                                    <telerik:GridBoundColumn DataField="OTRequestNo" DataType="System.Int32" HeaderText="Req. No." HeaderTooltip="Overtime requisition number" 
                                        ReadOnly="True" SortExpression="OTRequestNo" UniqueName="OTRequestNo">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="70px" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" HorizontalAlign="Center" />
                                        <ItemStyle Font-Bold="true" ForeColor="Blue" HorizontalAlign="Center" />
                                    </telerik:GridBoundColumn> 
                                    <telerik:GridBoundColumn DataField="CostCenter" DataType="System.String" HeaderText="Cost Center" 
                                        ReadOnly="True" SortExpression="CostCenter" UniqueName="CostCenter">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="55px" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
                                    </telerik:GridBoundColumn> 
                                    <telerik:GridBoundColumn DataField="TotalWorkDuration" DataType="System.String" HeaderText="Total Work <br /> Duration" 
                                        ReadOnly="True" SortExpression="TotalWorkDuration" UniqueName="TotalWorkDuration">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="88px" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" HorizontalAlign="Center" />
                                        <ItemStyle HorizontalAlign="Center" />
                                    </telerik:GridBoundColumn> 
                                    <telerik:GridBoundColumn DataField="TotalWorkDuration" DataType="System.String" HeaderText="Total Work Duration" Visible="false" 
                                        ReadOnly="True" SortExpression="TotalWorkDuration" UniqueName="TotalWorkDurationExport">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="130px" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" HorizontalAlign="Center" />
                                        <ItemStyle HorizontalAlign="Center" />
                                    </telerik:GridBoundColumn> 
                                    
                                    <telerik:GridBoundColumn DataField="RequiredWorkDuration" DataType="System.String" HeaderText="Req. Work <br /> Duration" 
                                        ReadOnly="True" SortExpression="RequiredWorkDuration" UniqueName="RequiredWorkDuration">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="78px" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" HorizontalAlign="Center" />
                                        <ItemStyle HorizontalAlign="Center" />
                                    </telerik:GridBoundColumn> 
                                    <telerik:GridBoundColumn DataField="RequiredWorkDuration" DataType="System.String" HeaderText="Req. Work Duration" Visible="false" 
                                        ReadOnly="True" SortExpression="RequiredWorkDuration" UniqueName="RequiredWorkDurationExport">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="120px" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" HorizontalAlign="Center" />
                                        <ItemStyle HorizontalAlign="Center" />
                                    </telerik:GridBoundColumn> 

                                    <telerik:GridTemplateColumn DataField="IsOTDueToShiftSpan" HeaderText="Is Shift <br /> Span?" 
                                        SortExpression="IsOTDueToShiftSpan" UniqueName="IsOTDueToShiftSpan" Visible="false">
								        <HeaderStyle Width="70px" HorizontalAlign="Center" />
								        <ItemTemplate>
									        <div style="width: 70px; text-align: center;">
										        <asp:Label ID="lblShiftSpan" runat="server" 
                                                    Text='<%# Convert.ToBoolean(Eval("IsOTDueToShiftSpan")) == true ? "Yes" : "No" %>'>
										        </asp:Label>  
									        </div>
								        </ItemTemplate>
                                        <ItemStyle HorizontalAlign="Center" />
							        </telerik:GridTemplateColumn>    
                                    
                                    <telerik:GridBoundColumn DataField="ArrivalSchedule" DataType="System.String" HeaderText="Arrival Sched." 
                                        ReadOnly="True" SortExpression="ArrivalSchedule" UniqueName="ArrivalSchedule" Visible="false">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="130px" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
                                        <ItemStyle ForeColor="Blue" />
                                    </telerik:GridBoundColumn> 
                                                                        
                                    <telerik:GridTemplateColumn DataField="StatusDesc" FilterControlAltText="Filter Status column" HeaderText="Status" 
                                        SortExpression="StatusDesc" UniqueName="StatusDesc" Visible="false">
								        <HeaderStyle Width="180px" HorizontalAlign="Left" />
								        <ItemTemplate>
									        <div class="columnEllipsis" style="width: 170px; text-align: left;">
										        <asp:Literal ID="litStatusDesc" runat="server" Text='<%# Eval("StatusDesc") %>' />
									        </div>
								        </ItemTemplate>
							        </telerik:GridTemplateColumn>   
                                    <telerik:GridBoundColumn DataField="StatusDesc" DataType="System.String" HeaderText="Status" Visible="false" 
                                        ReadOnly="True" SortExpression="StatusDesc" UniqueName="StatusDescExport">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="180px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
                                    </telerik:GridBoundColumn>

                                    <telerik:GridBoundColumn DataField="StatusCode" DataType="System.String" HeaderText="StatusCode" Visible="false"
                                        FilterControlAltText="Filter StatusCode column" ReadOnly="True" SortExpression="StatusCode" UniqueName="StatusCode">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                    </telerik:GridBoundColumn>
                                    <telerik:GridBoundColumn DataField="StatusHandlingCode" DataType="System.String" Visible="false" 
                                        ReadOnly="True" SortExpression="StatusHandlingCode" UniqueName="StatusHandlingCode">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                    </telerik:GridBoundColumn>
                                                                        
                                   <%-- <telerik:GridTemplateColumn DataField="CurrentlyAssignedFullName" FilterControlAltText="Filter Currently Assigned To column" HeaderText="Currently Assigned To" 
                                        SortExpression="CurrentlyAssignedFullName" UniqueName="CurrentlyAssignedFullName" Visible="false">
								        <HeaderStyle Width="300px" HorizontalAlign="Left" />
								        <ItemTemplate>
									        <div class="columnEllipsis" style="width: 290px; text-align: left;">
										        <asp:Literal ID="litCurrentlyAssignedTo" runat="server" Text='<%# Eval("CurrentlyAssignedFullName") %>' />
									        </div>
								        </ItemTemplate>
							        </telerik:GridTemplateColumn>--%>  
                                    <telerik:GridBoundColumn DataField="CurrentlyAssignedFullName" DataType="System.String" HeaderText="Currently Assigned To" 
                                        ReadOnly="True" SortExpression="CurrentlyAssignedFullName" UniqueName="CurrentlyAssignedFullName">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="300px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
                                    </telerik:GridBoundColumn>
                                                                        
                                    <telerik:GridTemplateColumn DataField="DistListDesc" HeaderText="Approval Level" 
                                        SortExpression="DistListDesc" UniqueName="DistListDesc">
								        <HeaderStyle Width="250px" HorizontalAlign="Left" />
								        <ItemTemplate>
									        <div class="columnEllipsis" style="width: 240px; text-align: left;">
										        <asp:Literal ID="litApprovalLevel" runat="server" Text='<%# Eval("DistListDesc") %>' />
									        </div>
								        </ItemTemplate>
							        </telerik:GridTemplateColumn> 
                                    <telerik:GridBoundColumn DataField="DistListDesc" DataType="System.String" HeaderText="Approval Level" Visible="false" 
                                        ReadOnly="True" SortExpression="DistListDesc" UniqueName="DistListDescExport">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="250px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
                                    </telerik:GridBoundColumn>
                                                                        
                                    <telerik:GridTemplateColumn DataField="LastUpdateFullName" HeaderText="Last Updated By" DataType="System.String" 
                                        SortExpression="LastUpdateFullName" UniqueName="LastUpdateFullName">
								        <HeaderStyle Width="230px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								        <ItemTemplate>
									        <div class="columnEllipsis" style="width: 220px; text-align: left;">
										        <asp:Literal ID="litLastUpdateUser" runat="server" Text='<%# Eval("LastUpdateFullName") %>' />
									        </div>
								        </ItemTemplate>
							        </telerik:GridTemplateColumn> 
                                    <telerik:GridBoundColumn DataField="LastUpdateFullName" DataType="System.String" HeaderText="Last Updated By" Visible="false" 
                                        ReadOnly="True" SortExpression="LastUpdateFullName" UniqueName="LastUpdateFullNameExport">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="250px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
                                    </telerik:GridBoundColumn>

                                    <telerik:GridBoundColumn DataField="LastUpdateTime" HeaderText="Last Updated Time"
                                        DataFormatString="{0:dd-MMM-yyyy h:mm tt}" DataType="System.DateTime" 
                                        FilterControlAltText="Filter Modified Date column" ReadOnly="True" SortExpression="LastUpdateTime" UniqueName="LastUpdateTime">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Font-Bold="True" Font-Size="8pt" Width="140px" Font-Names="Tahoma"></HeaderStyle>
                                    </telerik:GridBoundColumn>   
                                    <telerik:GridBoundColumn DataField="LastUpdateTime" HeaderText="Last Updated Time"
                                        DataFormatString="{0:dd-MMM-yyyy hh:mm:ss}" DataType="System.DateTime" Visible="false" 
                                        FilterControlAltText="Filter Modified Date column" ReadOnly="True" SortExpression="LastUpdateTime" UniqueName="LastUpdateTimeExport">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Font-Bold="True" Font-Size="8pt" Width="140px" Font-Names="Tahoma"></HeaderStyle>
                                    </telerik:GridBoundColumn>   
                                    <telerik:GridTemplateColumn UniqueName="HistoryButton" ItemStyle-HorizontalAlign="Center" HeaderText="">
                                        <HeaderStyle Width="40px" HorizontalAlign="Center" />
                                        <ItemStyle HorizontalAlign="Center" VerticalAlign="Middle" />
                                        <ItemTemplate>
                                            <asp:ImageButton ID="imgViewHistory" runat="server" ImageUrl="~/Images/view_history_icon.jpg" ToolTip="View approval history" OnClick="imgViewHistory_Click" />
                                        </ItemTemplate>
                                    </telerik:GridTemplateColumn>

                                    <telerik:GridBoundColumn DataField="IsOTAlreadyProcessed" DataType="System.Boolean" HeaderText="IsOTAlreadyProcessed" 
                                        ReadOnly="True" SortExpression="IsOTAlreadyProcessed" UniqueName="IsOTAlreadyProcessed" Display="false">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="80px" Font-Bold="True" />
                                    </telerik:GridBoundColumn>  
                                    <telerik:GridBoundColumn DataField="AutoID" DataType="System.Int32" HeaderText="AutoID" 
                                        ReadOnly="True" SortExpression="AutoID" UniqueName="AutoID" Display="false">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="50px" Font-Bold="True" />
                                    </telerik:GridBoundColumn>  
                                    <telerik:GridBoundColumn DataField="OTRequestNo" DataType="System.Int64" HeaderText="OTRequestNo" 
                                        ReadOnly="True" SortExpression="OTRequestNo" UniqueName="OTRequestNo" Visible="false">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="50px" Font-Bold="True" />
                                    </telerik:GridBoundColumn>  
                                    <telerik:GridBoundColumn DataField="CreatedByEmpNo" DataType="System.Int32" HeaderText="CreatedByEmpNo" 
                                        ReadOnly="True" SortExpression="CreatedByEmpNo" UniqueName="CreatedByEmpNo" Visible="false">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="50px" Font-Bold="True" />
                                    </telerik:GridBoundColumn> 
                                    <telerik:GridBoundColumn DataField="IsArrivedEarly" DataType="System.Boolean" HeaderText="IsArrivedEarly" 
                                        ReadOnly="True" SortExpression="IsArrivedEarly" UniqueName="IsArrivedEarly" Visible="false">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="80px" Font-Bold="True" />
                                    </telerik:GridBoundColumn>  
                                    <telerik:GridBoundColumn DataField="IsOTExceedOrig" DataType="System.Boolean" HeaderText="IsOTExceedOrig" 
                                        ReadOnly="True" SortExpression="IsOTExceedOrig" UniqueName="IsOTExceedOrig" Visible="false">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="80px" Font-Bold="True" />
                                    </telerik:GridBoundColumn>  
                                    <telerik:GridBoundColumn DataField="OTDurationHourOrig" DataType="System.Int64" HeaderText="OTDurationHourOrig" 
                                        ReadOnly="True" SortExpression="OTDurationHourOrig" UniqueName="OTDurationHourOrig" Visible="false">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="50px" Font-Bold="True" />
                                    </telerik:GridBoundColumn> 
                                    <telerik:GridBoundColumn DataField="IsPublicHoliday" DataType="System.Boolean" HeaderText="IsPublicHoliday" 
                                        ReadOnly="True" SortExpression="IsPublicHoliday" UniqueName="IsPublicHoliday" Visible="false">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="80px" Font-Bold="True" />
                                    </telerik:GridBoundColumn> 
                                    <telerik:GridBoundColumn DataField="IsRamadan" DataType="System.Boolean" HeaderText="IsRamadan" 
                                        ReadOnly="True" SortExpression="IsRamadan" UniqueName="IsRamadan" Visible="false">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="80px" Font-Bold="True" />
                                    </telerik:GridBoundColumn> 
                                    <telerik:GridBoundColumn DataField="IsOTRamadanExceedLimit" DataType="System.Boolean" HeaderText="IsOTRamadanExceedLimit" 
                                        ReadOnly="True" SortExpression="IsOTRamadanExceedLimit" UniqueName="IsOTRamadanExceedLimit" Visible="false">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                    </telerik:GridBoundColumn> 
                                </Columns>
                            </MasterTableView>
                            <ClientSettings AllowColumnsReorder="False" EnableRowHoverStyle="true" EnablePostBackOnRowClick="false">
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

        <asp:Panel ID="panButton" runat="server" BorderStyle="None" style="padding-left: 15px; margin: 0px;">
            <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                <tr>                    
                    <td style="padding-top: 3px; text-align: left;">
                        
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
        <telerik:RadButton ID="btnCancelDummy" runat="server" Text="" Skin="Office2010Silver" ValidationGroup="valPrimary" onclick="btnCancelDummy_Click" />      
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
                <telerik:AjaxSetting AjaxControlID="btnSave">
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
                <telerik:AjaxSetting AjaxControlID="btnCancelDummy">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>    
                <telerik:AjaxSetting AjaxControlID="gridSearchResults">
				    <UpdatedControls>
                        <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>   
                <telerik:AjaxSetting AjaxControlID="cboMonth">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panSearchCriteria" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting> 
                <telerik:AjaxSetting AjaxControlID="cboFilterOption">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting> 
                <telerik:AjaxSetting AjaxControlID="txtYear">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panSearchCriteria" />  
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
