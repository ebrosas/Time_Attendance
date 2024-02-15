<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/TASMaster.Master" AutoEventWireup="true" CodeBehind="ContractorAttendanceInq.aspx.cs" 
    Inherits="GARMCO.AMS.TAS.UI.Views.Reports.ContractorAttendanceInq" StylesheetTheme="Standard" %>

<%@ MasterType VirtualPath="~/Views/Shared/TASMaster.Master" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <title>Contractor Attendance Report</title>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="mainContent" runat="server">
    <telerik:RadFormDecorator ID="formDecor" runat="server" DecoratedControls="Buttons" Skin="Office2010Silver" />

    <table border="0" style="width: 100%; text-align: left; margin-top: 5px; margin-left: 5px;">
        <tr>
            <td colspan="2" style="padding-left: 10px;">
                <table border="0" style="width: 100%; text-align: left;">
                    <tr>
                        <td style="width: 50px; text-align: right; padding-right: 5px;" rowspan="2">
                            <img alt="" src="../../Images/print_report.jpg" />
                        </td>
                        <td id="tdPageTitle" runat="server" class="PageTitleLabel" style="vertical-align: bottom; padding-left: 0px; width: 900px; font-size: 11pt;">
                            Contractor Attendance Report
                        </td>
                        <td style="width: 50px;">
                            <asp:LinkButton ID="lnkMoveUp" runat="server" />
                        </td>
                        <td />
                    </tr>
                    <tr>
                        <td class="PageDescriptionHeader" style="text-align: left; vertical-align: top; padding-left: 0px; margin: 0px;">
                            View the attendance history of all contractors
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
                    <td id="tdEmployeeTitle" runat="server" class="LabelBold" style="width: 120px;">
                        Contractor ID      
                    </td>
                    <td style="width: 250px; padding-left: 0px; margin-left: 0px;">
                        <telerik:RadNumericTextBox ID="txtContractorNo" runat="server" width="130px" 
                            MinValue="0" ToolTip="(Note: Enter the number written in the ID badge)" 
                            Skin="Office2010Silver" DataType="System.Int32" MaxLength="8" MaxValue="99999999" 
                            EmptyMessage="Ex. 50123">
                            <NumberFormat ZeroPattern="n" DecimalDigits="0" GroupSeparator="" />
                        </telerik:RadNumericTextBox> 
                    </td>
                    <td style="width: 150px;">
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
                    <td style="width: 170px;">
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
                <tr style="height: 23px;">
                    <td class="LabelBold">
                        Contractor Name             
                    </td>
                    <td style="padding-left: 0px; margin-left: 0px;">
                        <telerik:RadTextBox ID="txtContractorName" runat="server" Width="100%" 
                            EmptyMessage="Enter Contractor Name" Skin="Office2010Silver" ToolTip="Maximum text input is 100 chars." 
                            Font-Names="Tahoma" Font-Size="9pt" MaxLength="100" />
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
                <tr style="height: 23px;">
                    <td class="LabelBold">
                        Cost Center
                    </td>
                    <td style="padding-left: 0px; margin-left: 0px;">
                         <telerik:RadComboBox ID="cboCostCenter" runat="server" 
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
					    </telerik:RadComboBox>   
                    </td>
                    <td class="LabelBold">
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
                <tr style="height: 23px;">
                    <td class="LabelBold">
                        
                    </td>
                    <td colspan="3" style="padding-left: 0px; padding-top: 5px; margin-left: 0px;">
                        <telerik:RadButton ID="btnSearch" runat="server" Text="Search" ToolTip="Search matching database records" Width="70px" 
                            CssClass="RadButtonStyle" ValidationGroup="valPrimary" Font-Size="9pt" OnClick="btnSearch_Click" Skin="Office2010Silver" />
                        <telerik:RadButton ID="btnReset" runat="server" Text="Reset Criteria" ToolTip="Reset filter criterias" Width="100px" 
                            CssClass="RadButtonStyle" ValidationGroup="valPrimary" Font-Size="9pt" OnClick="btnReset_Click" Skin="Office2010Silver" />   
                         <telerik:RadButton ID="btnPrint" runat="server" Text="Show Report" ToolTip="View and print the report" Width="100px" 
                            CssClass="RadButtonStyle" ValidationGroup="valPrimary" Font-Size="9pt" OnClick="btnPrint_Click" Skin="Office2010Silver" />                                                                       
                    </td>
                    <td />
                </tr>                
            </table>
        </asp:Panel>

        <asp:Panel ID="panGrid" runat="server" BorderStyle="None" style="padding-left: 15px; padding-right: 20px; margin: 0px;">
             <table border="0" style="width: 100%; text-align: left; margin-top: 5px; table-layout: fixed;">
                <tr>
                    <td style="text-align: left; color: Purple; font-weight: bold; font-size: 9pt; padding-left: 5px;">
                        <asp:Label ID="lblRecordCount" runat="server" Text="0 record found" Width="100%" />                         
                    </td>
                </tr>
            </table>
            <table id="tblGrid" runat="server" border="0" style="width: 100%; text-align: left; margin-top: 0px; table-layout: fixed;">
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
                            Font-Names="Tahoma" Font-Size="9pt" Font-Bold="False" 
                            AllowPaging = "true" BorderStyle="Outset" BorderWidth="1px"> 
                            <%--AllowCustomPaging="True" VirtualItemCount="1">--%>
                            <ExportSettings ExportOnlyData="true" IgnorePaging="true" OpenInNewWindow="true" FileName="ShiftPatternChangeList" HideStructureColumns="true">
                                <Pdf PageHeight="210mm" PageWidth="310mm" PageTitle="Shift Pattern Changes List" DefaultFontFamily="Arial Unicode MS"
                                PageBottomMargin="20mm" PageTopMargin="20mm" PageLeftMargin="20mm" PageRightMargin="20mm" PaperSize="A4" AllowPrinting="true" />
                            </ExportSettings>
                            <AlternatingItemStyle Font-Names="Tahoma" Font-Size="9pt" Wrap="True" />
                            <MasterTableView DataKeyNames="EmpNo" ClientDataKeyNames="EmpNo" 
                                NoMasterRecordsText="No attendance record found." 
                                TableLayout="Fixed" PagerStyle-AlwaysVisible="True" Font-Names="Tahoma" 
                                Font-Size="9pt" CommandItemDisplay="Top">
                                <PagerStyle AlwaysVisible="True" Mode="NextPrevAndNumeric" />
                                <CommandItemSettings ShowRefreshButton="true" ShowExportToExcelButton="true" ShowAddNewRecordButton="false" ExportToPdfText="Export to PDF"></CommandItemSettings>
			                    <RowIndicatorColumn>
				                    <HeaderStyle Width="20px" />
			                    </RowIndicatorColumn>
			                    <ExpandCollapseColumn>
				                    <HeaderStyle Width="20px" />
			                    </ExpandCollapseColumn>
                                <Columns>         
                                    <telerik:GridBoundColumn DataField="EmpNo" DataType="System.Int32" HeaderText="Contractor ID" 
                                        ReadOnly="True" SortExpression="EmpNo" UniqueName="EmpNo">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="110px" Font-Bold="True" />
                                        <ItemStyle Font-Bold="true" ForeColor="Purple" />
                                    </telerik:GridBoundColumn> 

                                    <%--<telerik:GridTemplateColumn DataField="EmpName" HeaderText="Contractor Name" 
                                        SortExpression="EmpName" UniqueName="EmpName">
								        <HeaderStyle Width="210px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								        <ItemTemplate>
									        <div class="columnEllipsis" style="width: 200px; text-align: left;">
										        <asp:Literal ID="litEmpName" runat="server" Text='<%# Eval("EmpName") %>' />
									        </div>
								        </ItemTemplate>
							        </telerik:GridTemplateColumn>--%> 
                                    <telerik:GridBoundColumn DataField="EmpName" DataType="System.String" HeaderText="Contractor Name" 
                                        ReadOnly="True" SortExpression="EmpName" UniqueName="EmpName">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="210px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
                                    </telerik:GridBoundColumn>
                                                                             
                                    <telerik:GridBoundColumn DataField="CPRNo" DataType="System.Int32" HeaderText="CPR No." 
                                        ReadOnly="True" SortExpression="CPRNo" UniqueName="CPRNo">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="90px" Font-Bold="True" />
                                    </telerik:GridBoundColumn> 

                                    <%--<telerik:GridTemplateColumn DataField="JobTitle" HeaderText="Job Title" 
                                        SortExpression="JobTitle" UniqueName="JobTitle">
								        <HeaderStyle Width="160px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								        <ItemTemplate>
									        <div class="columnEllipsis" style="width: 150px; text-align: left;">
										        <asp:Literal ID="litJobTitle" runat="server" Text='<%# Eval("JobTitle") %>' />
									        </div>
								        </ItemTemplate>
							        </telerik:GridTemplateColumn> --%>
                                    <telerik:GridBoundColumn DataField="JobTitle" DataType="System.String" HeaderText="Job Title" 
                                        ReadOnly="True" SortExpression="JobTitle" UniqueName="JobTitle">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="160px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
                                    </telerik:GridBoundColumn>

                                    <%--<telerik:GridTemplateColumn DataField="EmployerName" HeaderText="Employer Name" 
                                        SortExpression="EmployerName" UniqueName="EmployerName">
								        <HeaderStyle Width="200px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								        <ItemTemplate>
									        <div class="columnEllipsis" style="width: 190px; text-align: left;">
										        <asp:Literal ID="litEmployerName" runat="server" Text='<%# Eval("EmployerName") %>' />
									        </div>
								        </ItemTemplate>
							        </telerik:GridTemplateColumn>--%> 
                                    <telerik:GridBoundColumn DataField="EmployerName" DataType="System.String" HeaderText="Employer Name" 
                                        ReadOnly="True" SortExpression="EmployerName" UniqueName="EmployerName">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="200px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
                                    </telerik:GridBoundColumn>

                                    <telerik:GridBoundColumn DataField="CostCenter" DataType="System.String" HeaderText="Cost Center" 
                                        ReadOnly="True" SortExpression="CostCenter" UniqueName="CostCenter" Display="false">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="90px" Font-Bold="True" />
                                    </telerik:GridBoundColumn> 

                                    <%--<telerik:GridTemplateColumn DataField="CostCenterName" HeaderText="Department" 
                                        SortExpression="CostCenterName" UniqueName="CostCenterName">
								        <HeaderStyle Width="220px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								        <ItemTemplate>
									        <div class="columnEllipsis" style="width: 210px; text-align: left;">
										        <asp:Literal ID="litCostCenterName" runat="server" Text='<%# Eval("CostCenterName") %>' />
									        </div>
								        </ItemTemplate>
							        </telerik:GridTemplateColumn>--%> 
                                    <telerik:GridBoundColumn DataField="CostCenterName" DataType="System.String" HeaderText="Department" 
                                        ReadOnly="True" SortExpression="CostCenterName" UniqueName="CostCenterName">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="220px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
                                    </telerik:GridBoundColumn>

                                    <%--<telerik:GridTemplateColumn DataField="LocationName" HeaderText="Gate" 
                                        SortExpression="LocationName" UniqueName="LocationName" Display="false">
								        <HeaderStyle Width="110px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								        <ItemTemplate>
									        <div class="columnEllipsis" style="width: 100px; text-align: left;">
										        <asp:Literal ID="litLocationName" runat="server" Text='<%# Eval("LocationName") %>' />
									        </div>
								        </ItemTemplate>
							        </telerik:GridTemplateColumn>--%>   
                                    <telerik:GridBoundColumn DataField="LocationName" DataType="System.String" HeaderText="Gate" 
                                        ReadOnly="True" SortExpression="LocationName" UniqueName="LocationName" Display="false">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="110px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
                                    </telerik:GridBoundColumn> 

                                    <%--<telerik:GridTemplateColumn DataField="ReaderName" HeaderText="Gate" 
                                        SortExpression="ReaderName" UniqueName="ReaderName">
								        <HeaderStyle Width="150px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								        <ItemTemplate>
									        <div class="columnEllipsis" style="width: 140px; text-align: left;">
										        <asp:Literal ID="litReaderName" runat="server" Text='<%# Eval("ReaderName") %>' />
									        </div>
								        </ItemTemplate>
							        </telerik:GridTemplateColumn>--%>  
                                    <telerik:GridBoundColumn DataField="ReaderName" DataType="System.String" HeaderText="Gate" 
                                        ReadOnly="True" SortExpression="ReaderName" UniqueName="ReaderName">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="150px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
                                    </telerik:GridBoundColumn> 

                                    <telerik:GridBoundColumn DataField="SwipeDate" HeaderText="Date"
                                        DataFormatString="{0:dd-MMM-yyyy}" DataType="System.DateTime" 
                                        ReadOnly="True" SortExpression="SwipeDate" UniqueName="SwipeDate">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Font-Bold="True" Font-Size="8pt" Width="90px" Font-Names="Tahoma"></HeaderStyle>
                                    </telerik:GridBoundColumn>   

                                    <telerik:GridBoundColumn DataField="SwipeIn" HeaderText="Time In"
                                        DataFormatString="{0:HH:mm tt}" DataType="System.DateTime" 
                                        ReadOnly="True" SortExpression="SwipeIn" UniqueName="SwipeIn">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" Width="80px"></HeaderStyle>
                                    </telerik:GridBoundColumn> 
                                    <telerik:GridBoundColumn DataField="SwipeIn" HeaderText="Time In"
                                        DataFormatString="{0:HH:mm}" DataType="System.DateTime" 
                                        ReadOnly="True" SortExpression="SwipeIn" UniqueName="SwipeInExcel" Visible="false">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" Width="80px"></HeaderStyle>
                                    </telerik:GridBoundColumn> 

                                    <telerik:GridBoundColumn DataField="SwipeOut" HeaderText="Time Out"
                                        DataFormatString="{0:HH:mm tt}" DataType="System.DateTime" 
                                        ReadOnly="True" SortExpression="SwipeOut" UniqueName="SwipeOut">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" Width="80px"></HeaderStyle>
                                    </telerik:GridBoundColumn>                                                                 
                                    <telerik:GridBoundColumn DataField="SwipeOut" HeaderText="Time Out"
                                        DataFormatString="{0:HH:mm}" DataType="System.DateTime" 
                                        ReadOnly="True" SortExpression="SwipeOut" UniqueName="SwipeOutExcel" Visible="false">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" Width="80px"></HeaderStyle>
                                    </telerik:GridBoundColumn>    

                                    <telerik:GridBoundColumn DataField="RequiredWorkDuration" DataType="System.Double" HeaderText="Work Hour" 
                                        ReadOnly="True" SortExpression="RequiredWorkDuration" UniqueName="RequiredWorkDuration" DataFormatString="{0:N2}">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="85px" Font-Bold="True" HorizontalAlign="Center" />
                                        <ItemStyle HorizontalAlign="Center" />
                                    </telerik:GridBoundColumn> 
                                    <telerik:GridBoundColumn DataField="NetHour" DataType="System.Double" HeaderText="Net Hour" 
                                        ReadOnly="True" SortExpression="NetHour" UniqueName="NetHour" DataFormatString="{0:N2}">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="80px" Font-Bold="True" HorizontalAlign="Center" />
                                        <ItemStyle HorizontalAlign="Center" />
                                    </telerik:GridBoundColumn> 
                                    <telerik:GridBoundColumn DataField="OvertimeHour" DataType="System.Double" HeaderText="Overtime" 
                                        ReadOnly="True" SortExpression="OvertimeHour" UniqueName="OvertimeHour" DataFormatString="{0:N2}">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="80px" Font-Bold="True" HorizontalAlign="Center" />
                                        <ItemStyle HorizontalAlign="Center" />
                                    </telerik:GridBoundColumn> 
                                    <telerik:GridBoundColumn DataField="StatusDesc" DataType="System.String" HeaderText="Status" 
                                        ReadOnly="True" SortExpression="StatusDesc" UniqueName="StatusDesc">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="80px" Font-Bold="True" HorizontalAlign="Center" />
                                        <ItemStyle HorizontalAlign="Center" />
                                    </telerik:GridBoundColumn> 
                                    <telerik:GridBoundColumn DataField="ContractStartDate" HeaderText="Start Date"
                                        DataFormatString="{0:dd-MMM-yyyy}" DataType="System.DateTime" 
                                        ReadOnly="True" SortExpression="ContractStartDate" UniqueName="ContractStartDate">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="100px" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma"></HeaderStyle>
                                    </telerik:GridBoundColumn>  
                                    <telerik:GridBoundColumn DataField="ContractEndDate" HeaderText="Expiry Date"
                                        DataFormatString="{0:dd-MMM-yyyy}" DataType="System.DateTime" 
                                        ReadOnly="True" SortExpression="ContractEndDate" UniqueName="ContractEndDate">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="100px" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma"></HeaderStyle>
                                    </telerik:GridBoundColumn>  
                                    <telerik:GridBoundColumn DataField="ContractorTypeDesc" DataType="System.String" HeaderText="Contractor Type" Display="false" 
                                        ReadOnly="True" SortExpression="ContractorTypeDesc" UniqueName="ContractorTypeDesc">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="130px" Font-Bold="True" />
                                    </telerik:GridBoundColumn> 
                                    <telerik:GridBoundColumn DataField="CreatedDate" HeaderText="Printed Date"
                                        DataFormatString="{0:dd-MMM-yyyy}" DataType="System.DateTime" 
                                        ReadOnly="True" SortExpression="CreatedDate" UniqueName="CreatedDate">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Font-Bold="True" Font-Size="8pt" Width="100px" Font-Names="Tahoma"></HeaderStyle>
                                    </telerik:GridBoundColumn>  
                                     
                                    <%--<telerik:GridTemplateColumn DataField="CreatedByFullName" HeaderText="Created By" Display="false" 
                                        SortExpression="CreatedByFullName" UniqueName="CreatedByFullName">
								        <HeaderStyle Width="210px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								        <ItemTemplate>
									        <div class="columnEllipsis" style="width: 200px; text-align: left;">
										        <asp:Literal ID="litCreatedByFullName" runat="server" Text='<%# Eval("CreatedByFullName") %>' />
									        </div>
								        </ItemTemplate>
							        </telerik:GridTemplateColumn>--%>                                        
                                    <telerik:GridBoundColumn DataField="CreatedByFullName" DataType="System.String" HeaderText="Created By" 
                                        ReadOnly="True" SortExpression="CreatedByFullName" UniqueName="CreatedByFullName" Display="false">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="150px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
                                    </telerik:GridBoundColumn> 
                                </Columns>
                            </MasterTableView>
                            <ClientSettings AllowColumnsReorder="False" EnableRowHoverStyle="true">
                                <Selecting AllowRowSelect="True" UseClientSelectColumnOnly="True" />
                                <Scrolling AllowScroll="true" UseStaticHeaders="true" SaveScrollPosition="true" ScrollHeight="" FrozenColumnsCount="2" />
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

        <asp:Panel ID="panButton" runat="server" BorderStyle="None" style="padding-left: 15px; margin: 0px; display: none;">
            <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                <tr>                    
                    <td style="padding-top: 3px; text-align: left;">
                        <asp:CustomValidator ID="cusValButton" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" />                                                                                            
                        <telerik:RadButton ID="btnExportToExcel" runat="server" Text="Download Data to Excel" ToolTip="Export data to Excel" Width="150px" 
                            CssClass="RadButtonStyle" ValidationGroup="valPrimary" Font-Size="9pt" OnClick="btnExportToExcel_Click" Skin="Office2010Silver" />                                                                      
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
        <telerik:RadButton ID="btnRebind" runat="server" Text="" Skin="Office2010Silver" CausesValidation="false" onclick="btnRebind_Click" />
    </asp:Panel>

    <asp:Panel ID="panAjaxManager" runat="server">
        <telerik:RadAjaxManager ID="MyAjaxManager" runat="server">
            <AjaxSettings>
                <telerik:AjaxSetting AjaxControlID="btnSearch">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="gridSearchResults" />   
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>   
                <telerik:AjaxSetting AjaxControlID="btnReset">
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
                <telerik:AjaxSetting AjaxControlID="btnPrint">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>   
                <telerik:AjaxSetting AjaxControlID="btnExportToExcel">
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
					    <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>  
                <telerik:AjaxSetting AjaxControlID="txtYear">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panMain" />  
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
