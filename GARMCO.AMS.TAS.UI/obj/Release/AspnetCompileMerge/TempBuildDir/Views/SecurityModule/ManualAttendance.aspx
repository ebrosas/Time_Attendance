<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/TASMaster.Master" AutoEventWireup="true" CodeBehind="ManualAttendance.aspx.cs" 
    Inherits="GARMCO.AMS.TAS.UI.Views.SecurityModule.ManualAttendance" StylesheetTheme="Standard" %>

<%@ MasterType VirtualPath="~/Views/Shared/TASMaster.Master" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <title>Manual Attendance</title>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="mainContent" runat="server">
    <telerik:RadFormDecorator ID="formDecor" runat="server" DecoratedControls="Buttons" Skin="Office2010Silver" />

    <table border="0" style="width: 100%; text-align: left; margin-top: 5px; margin-left: 5px;">
        <tr>
            <td colspan="2" style="padding-left: 10px;">
                <table border="0" style="width: 100%; text-align: left;">
                    <tr>
                        <td style="width: 50px; text-align: right; padding-right: 5px;" rowspan="2">
                            <img alt="" src="../../Images/manual_attendance_icon.jpg" />
                        </td>
                        <td id="tdPageTitle" runat="server"  class="PageTitleLabel" style="vertical-align: bottom; padding-left: 5px; width: 900px; font-size: 11pt;">
                            Manual Attendance
                        </td>
                        <td style="width: 50px;">
                            <asp:LinkButton ID="lnkMoveUp" runat="server" />
                        </td>
                        <td />
                    </tr>
                    <tr>
                        <td class="PageDescriptionHeader" style="text-align: left; vertical-align: top; padding-left: 5px; margin: 0px;">
                            Allows Security Personnel to manually log the attendance of an employee who forgot to bring his/her ID badge
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

    <asp:Panel ID="panMain" runat="server" Width="100%" style="margin-top: 5px; padding-bottom: 40px;"> 
        <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
            <tr style="height: 23px;">
                <td class="LabelBold" style="width: 670px; vertical-align: top;">
                    <asp:Panel ID="panEmployee" runat="server" BorderStyle="None" Width="100%" style="padding: 0px; margin: 0px;" 
                        CssClass="PanelTitle" GroupingText="Employee Information:">
                        <table border="0" style="width: 100%; text-align: left; margin: 0px; padding-bottom: 5px; table-layout: fixed;">
                            <tr style="height: 23px;">
                                <td class="LabelBold" style="width: 110px;">
                                    <asp:CustomValidator ID="cusValEmpNo" runat="server" ControlToValidate="txtGeneric" 
                                        CssClass="LabelValidationError" Display="Dynamic" 
                                        ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                                        ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                                    Employee No.                 
                                </td>
                                <td style="width: 320px;">
                                    <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                                        <tr style="margin: 0px; padding: 0px;">
                                            <td style="width: 100px; text-align: left;">
                                                <telerik:RadNumericTextBox ID="txtEmpNo" runat="server" width="100%" 
                                                    MinValue="0" ToolTip="(Note: Employee No. must start with 1000. Example: 10003632)" 
                                                    Skin="Office2010Silver" DataType="System.Int32" MaxLength="8" MaxValue="99999999" 
                                                    EmptyMessage="1000xxxx" BackColor="Yellow" >
                                                    <NumberFormat ZeroPattern="n" DecimalDigits="0" GroupSeparator="" />
                                                </telerik:RadNumericTextBox> 
                                            </td>
                                            <td style="width: 40px; text-align: left; padding-left: 3px;">
                                                <telerik:RadButton ID="btnGet" runat="server" Skin="Office2010Silver" Width="100%" 
                                                    Text="Get" ToolTip="Get employee info based on entered Employee No." Enabled="true" 
                                                    Font-Bold="False" Font-Size="9pt" CausesValidation="false"
                                                    onclick="btnGet_Click">
                                                </telerik:RadButton>
                                            </td> 
                                            <td style="text-align: left; width: 35px; padding-left: 3px;">
                                                <telerik:RadButton ID="btnFindEmployee" runat="server" Skin="Office2010Silver" 
                                                    Text="..." ToolTip="Click here to search for an employee." Enabled="true" 
                                                    Width="100%" Font-Bold="False" Font-Size="9pt" CausesValidation="false"
                                                    onclick="btnFindEmployee_Click">
                                                </telerik:RadButton>
                                            </td> 
                                            <td style="text-align: left; width: 65px; padding-left: 3px;">
                                                
                                            </td> 
                                            <td />
                                        </tr>
                                    </table>
                                </td>
                                <td rowspan="4" class="TextNormal" style="width: 140px; vertical-align: top;">
                                    <asp:Image ID="imgPhoto" runat="server" ImageAlign="Middle" ImageUrl="~/Images/employee_photo.png" 
                                        Width="100%" Height="130px"  BorderStyle="None" AlternateText="Employee Photo" />
                                </td>
                                <td />
                            </tr>
                            <tr style="height: 20px;">
                                <td class="LabelBold">
                                    Employee Name                 
                                </td>
                                <td class="TextNormal">
                                    <asp:Literal ID="litEmpName" runat="server" Text="Not defined" />         
                                </td>
                                <td />
                            </tr>
                            <tr style="height: 20px;">
                                <td class="LabelBold">
                                    Job Title                 
                                </td>
                                <td class="TextNormal">
                                    <asp:Literal ID="litPosition" runat="server" Text="Not defined" />                                  
                                </td>
                                <td />
                            </tr>
                            <tr style="height: 20px;">
                                <td class="LabelBold">
                                    Cost Center
                                </td>
                                <td class="TextNormal">
                                    <asp:Literal ID="litCostCenter" runat="server" Text="Not defined" />                                  
                                </td>
                                <td />
                            </tr>
                            <tr style="height: 20px;">
                                <td class="LabelBold">
                                    
                                </td>
                                <td class="TextNormal">
                                    <telerik:RadButton ID="btnClear" runat="server" Skin="Silk" 
                                        Text="Clear Form" ToolTip="Clear the form" Enabled="true" 
                                        Width="100px" Font-Bold="True" Font-Size="9pt" CausesValidation="false"
                                        onclick="btnClear_Click">
                                    </telerik:RadButton>
                                </td>
                                <td />
                            </tr>
                        </table>
                    </asp:Panel>
                    
                    <asp:Panel ID="panAttendanceAction" runat="server" BorderStyle="None" Width="100%" style="padding: 0px; margin-top: 10px;" 
                        CssClass="PanelTitle" GroupingText="Attendance Action:">
                        <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                            <tr style="height: 23px;">
                                <td class="LabelBold" style="width: 110px;">
                                    <asp:CustomValidator ID="cusValSwipeIn" runat="server" ControlToValidate="txtGeneric" 
                                        CssClass="LabelValidationError" Display="Dynamic" 
                                        ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                                        ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                                    <asp:RadioButton ID="rblSwipeIn" runat="server" Text="Swipe In" TextAlign="Left" Checked="True" AutoPostBack="True" OnCheckedChanged="rblSwipeIn_CheckedChanged" />
                                </td>
                                <td style="width: 210px;">
                                    <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                                        <tr style="height: 23px;">
                                            <td style="width: 130px; text-align: left;">
                                                <telerik:RadDatePicker ID="dtpDateIn" runat="server" ToolTip="Date Format: dd/mm/yyyy"
                                                    Width="100%" Skin="Office2010Silver" Culture="en-US" Enabled="False">
                                                    <Calendar ID="Calendar1" runat="server" Skin="Office2010Silver" UseColumnHeadersAsSelectors="False" 
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
                                            <td style="width: auto; text-align: left;">
                                                <telerik:RadDateInput ID="dtpTimeIn" runat="server" LabelWidth="50px" Width="100%" Culture="en-US" EmptyMessage="HH:mm:ss" ToolTip="Time Format: HH:mm:ss"
                                                    InvalidStyleDuration="100" DateFormat="HH:mm:ss" Enabled="False">
                                                </telerik:RadDateInput>   
                                            </td> 
                                        </tr>
                                    </table>                                              
                                </td>
                                <td class="LabelBold" style="width: 100px; text-align: left; padding-left: 3px; margin-left: 0px;">
                                    <telerik:RadButton ID="btnSwipeIn" runat="server" ToolTip="Save swipe in time" Width="80px"
                                        Text="Swipe In" Skin="Silk" Font-Bold="True" Font-Size="9pt" 
                                        CssClass="RadButtonStyle" ValidationGroup="valPrimary"
                                        OnClick="btnSwipeIn_Click">
                                    </telerik:RadButton>
                                </td>
                                <td />
                            </tr>
                            <tr style="height: 23px;">
                                <td class="LabelBold">
                                    <asp:CustomValidator ID="cusValSwipeOut" runat="server" ControlToValidate="txtGeneric" 
                                        CssClass="LabelValidationError" Display="Dynamic" 
                                        ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                                        ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                                    <asp:RadioButton ID="rblSwipeOut" runat="server" Text="Swipe Out" TextAlign="Left" AutoPostBack="True" OnCheckedChanged="rblSwipeOut_CheckedChanged" />
                                </td>
                                <td class="TextNormal"  style="padding-left: 0px;">
                                    <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                                        <tr style="height: 23px;">
                                            <td style="width: 130px; text-align: left;">
                                                <telerik:RadDatePicker ID="dtpDateOut" runat="server" ToolTip="Date Format: dd/mm/yyyy"
                                                    Width="100%" Skin="Office2010Silver" Culture="en-US" Enabled="False">
                                                    <Calendar ID="Calendar2" runat="server" Skin="Office2010Silver" UseColumnHeadersAsSelectors="False" 
                                                        UseRowHeadersAsSelectors="False" ViewSelectorText="x">
                                                    </Calendar>
                                                    <DateInput ID="DateInput2" runat="server" DateFormat="d/M/yyyy" DisplayDateFormat="d/M/yyyy">
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
                                            <td style="width: auto; text-align: left;">
                                                <telerik:RadDateInput ID="dtpTimeOut" runat="server" LabelWidth="50px" Width="100%" Culture="en-US" EmptyMessage="HH:mm:ss" ToolTip="Time Format: HH:mm:ss"
                                                    InvalidStyleDuration="100" DateFormat="HH:mm:ss" Enabled="False">
                                                </telerik:RadDateInput>  
                                            </td> 
                                        </tr>
                                    </table>  
                                </td>
                                <td class="LabelBold" style="text-align: left; padding-left: 3px; margin-left: 0px;">
                                    <telerik:RadButton ID="btnSwipeOut" runat="server" ToolTip="Save swipe out time" Width="80px"
                                        Text="Swipe Out" Skin="Silk" Font-Bold="True" Font-Size="9pt" 
                                        CssClass="RadButtonStyle" ValidationGroup="valPrimary"
                                        OnClick="btnSwipeOut_Click" Visible="False">
                                    </telerik:RadButton>
                                </td>
                                <td />
                            </tr>
                        </table>       
                    </asp:Panel>   
            
                    <asp:Panel ID="panAttendanceHistoryGrid" runat="server" BorderStyle="None" Width="100%" style="padding-left: 3px; padding-bottom: 30px; margin-top: 10px;" 
                        CssClass="PanelTitle" GroupingText="Employee Attendance History:">
                        <table border="0" style="width: 100%; text-align: left; margin-top: 5px; table-layout: fixed;">
                            <tr>
                                <td style="text-align: left; color: Purple; font-weight: bold; font-size: 9pt; padding-left: 5px;">
                                    <asp:Label ID="lblRecordCount" runat="server" Text="0 record found" Width="100%" />                         
                                </td>
                            </tr>
                        </table>
                        <table id="tblGrid" runat="server" border="0" style="width: 100%; text-align: left; margin-top: 0px; table-layout: fixed;">
                            <tr>
                                <td style="padding-left: 5px; padding-right: 10px; padding-bottom: 5px;">
                                    <telerik:RadGrid ID="gridEmpAttendanceHistory" runat="server"
                                        AllowSorting="true" AllowMultiRowSelection="true"
                                        PageSize="5" AutoGenerateColumns="false" Skin="Silk" 
                                        GridLines="None" Width="100%" Height="" CellSpacing="0"
                                        onpageindexchanged="gridEmpAttendanceHistory_PageIndexChanged" 
                                        onpagesizechanged="gridEmpAttendanceHistory_PageSizeChanged" 
                                        onsortcommand="gridEmpAttendanceHistory_SortCommand" 
                                        onitemcommand="gridEmpAttendanceHistory_ItemCommand" 
                                        onitemdatabound="gridEmpAttendanceHistory_ItemDataBound" 
                                        Font-Names="Tahoma" Font-Size="9pt" Font-Bold="False" 
                                        AllowPaging = "true" BorderStyle="Outset" BorderWidth="1px" AllowCustomPaging="True" VirtualItemCount="1">
                                        <ExportSettings ExportOnlyData="true" IgnorePaging="true" OpenInNewWindow="true" FileName="EmpAttendanceHistoryList" HideStructureColumns="true">
                                            <Pdf PageHeight="210mm" PageWidth="310mm" PageTitle="Empployee Attendance History List" DefaultFontFamily="Arial Unicode MS"
                                            PageBottomMargin="20mm" PageTopMargin="20mm" PageLeftMargin="20mm" PageRightMargin="20mm" PaperSize="A4" AllowPrinting="true" />
                                        </ExportSettings>
                                        <AlternatingItemStyle Font-Names="Tahoma" Font-Size="9pt" Wrap="True" />
                                        <MasterTableView DataKeyNames="AutoID" ClientDataKeyNames="AutoID" 
                                            NoMasterRecordsText="No employee attendance record found." 
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
                                                <telerik:GridClientSelectColumn HeaderText="Select" HeaderStyle-Width="50px" Visible="false" 
                                                    HeaderStyle-Font-Bold="true" HeaderStyle-Font-Size = "9pt" 
                                                    UniqueName="CheckboxSelectColumn" >                                                                                            
                                                    <HeaderStyle Font-Bold="True" Font-Size="9pt" Width="35px" HorizontalAlign="Center" />
                                                    <ItemStyle HorizontalAlign="Center" />
                                                </telerik:GridClientSelectColumn>  
                                                <telerik:GridButtonColumn ButtonType="LinkButton" CommandName="Select" Text="Edit" 
                                                    UniqueName="EditLinkButton" HeaderTooltip="Open selected record for editing" Visible="false">
                                                    <HeaderStyle Width="40px" HorizontalAlign="Center" Font-Bold="true" Font-Size="9pt" />
                                                    <ItemStyle HorizontalAlign="Center" Font-Bold="true" Font-Size="9pt" ForeColor="Blue" />
					                            </telerik:GridButtonColumn>    
                                                <telerik:GridButtonColumn ButtonType="LinkButton" CommandName="Select" Text="View" 
                                                    UniqueName="ViewLinkButton" HeaderTooltip="View details about the selected record" Visible="false">
                                                    <HeaderStyle Width="40px" HorizontalAlign="Center" Font-Bold="true" Font-Size="9pt" />
                                                    <ItemStyle HorizontalAlign="Center" Font-Bold="true" Font-Size="9pt" ForeColor="Blue" />
					                            </telerik:GridButtonColumn>      
                                                <telerik:GridBoundColumn DataField="EmpNo" DataType="System.Int32" HeaderText="Emp. No." 
                                                    ReadOnly="True" SortExpression="EmpNo" UniqueName="EmpNo">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Width="90px" Font-Bold="True" />
                                                    <ItemStyle Font-Bold="true" ForeColor="Purple" />
                                                </telerik:GridBoundColumn> 
                                                <telerik:GridTemplateColumn DataField="EmpName" HeaderText="Emp. Name" Visible="false" 
                                                    SortExpression="EmpName" UniqueName="EmpName">
								                    <HeaderStyle Width="300px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								                    <ItemTemplate>
									                    <div class="columnEllipsis" style="width: 290px; text-align: left;">
										                    <asp:Literal ID="litEmpName" runat="server" Text='<%# Eval("EmpName") %>' />
									                    </div>
								                    </ItemTemplate>
							                    </telerik:GridTemplateColumn> 
                                                <telerik:GridTemplateColumn DataField="Position" HeaderText="Position" 
                                                    SortExpression="Position" UniqueName="Position" Visible="false">
								                    <HeaderStyle Width="220px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								                    <ItemTemplate>
									                    <div class="columnEllipsis" style="width: 210px; text-align: left;">
										                    <asp:Literal ID="litPosition" runat="server" Text='<%# Eval("Position") %>' />
									                    </div>
								                    </ItemTemplate>
							                    </telerik:GridTemplateColumn> 
                                                <telerik:GridTemplateColumn DataField="CostCenterFullName" HeaderText="Cost Center" 
                                                    SortExpression="CostCenterFullName" UniqueName="CostCenterFullName" Visible="false">
								                    <HeaderStyle Width="300px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								                    <ItemTemplate>
									                    <div class="columnEllipsis" style="width: 290px; text-align: left;">
										                    <asp:Literal ID="litCostCenterFullName" runat="server" Text='<%# Eval("CostCenterFullName") %>' />
									                    </div>
								                    </ItemTemplate>
							                    </telerik:GridTemplateColumn> 
                                                <telerik:GridBoundColumn DataField="SwipeIn" HeaderText="Time In"
                                                    DataFormatString="{0:dd-MMM-yyyy HH:mm tt}" DataType="System.DateTime" 
                                                    ReadOnly="True" SortExpression="SwipeIn" UniqueName="SwipeIn">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Width="160px" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma"></HeaderStyle>
                                                </telerik:GridBoundColumn>   
                                                <telerik:GridBoundColumn DataField="SwipeOut" HeaderText="Time Out"
                                                    DataFormatString="{0:dd-MMM-yyyy HH:mm tt}" DataType="System.DateTime" 
                                                    ReadOnly="True" SortExpression="SwipeOut" UniqueName="SwipeOut">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Width="160px" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma"></HeaderStyle>
                                                </telerik:GridBoundColumn> 
                                                <telerik:GridTemplateColumn DataField="IsContractor" HeaderText="Contractor?" Display="false" 
                                                    SortExpression="IsContractor" UniqueName="IsContractor">
								                    <HeaderStyle Width="100px" HorizontalAlign="Center" />
								                    <ItemTemplate>
									                    <div style="width: 100px; text-align: center;">
										                    <asp:Label ID="lblIsContractor" runat="server" Text='<%# Convert.ToBoolean(Eval("IsContractor")) == true ? "Yes" : "No" %>'></asp:Label>  
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
                                                <telerik:GridBoundColumn DataField="LastUpdateTime" HeaderText="Last Update Time"
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
                </td>
                <td style="width: auto; vertical-align: top; text-align: left; padding-left: 0px; margin-left: 0px;">
                    <asp:Panel ID="panManualAttendanceHistory" runat="server" style="margin-top: 0px;" CssClass="GroupPanelHeader" GroupingText="Manual Attendance History:"> 
                        <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                            <tr>
                                <td class="LabelBold" style="width: 170px; padding-right: 2px; color: blue; font-size: 9pt;">
                                    Show Search Filter Criteria
                                </td>
                                <td style="width: 100px; text-align: left;">
                                    <asp:CheckBox ID="chkSearchFilter" runat="server" Text="" TextAlign="Right" SkinID="CheckBold" style="padding-left: 0px;" 
                                        AutoPostBack="True" OnCheckedChanged="chkSearchFilter_CheckedChanged" />
                                </td>
                                <td style="text-align: right; color: Purple; font-weight: bold; font-size: 9pt; padding-right: 10px;">
                                    <asp:Label ID="lblSwipeHistorySearchString" runat="server" Text="" Width="100%" />                         
                                </td>
                            </tr>
                        </table>

                        <asp:Panel ID="panManualAttendanceFilter" runat="server" BorderStyle="None" style="padding: 0px; margin: 0px;">
                            <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                                <tr style="height: 23px;">
                                    <td id="tdEmployeeTitle" runat="server" class="LabelBold" style="width: 100px;">
                                        Employee No.      
                                    </td>
                                    <td style="width: 250px;">
                                        <table id="tdEmployee" runat="server" border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                                            <tr style="vertical-align: top; margin: 0px; padding: 0px;">
                                                <td style="width: 110px; text-align: left;">
                                                    <telerik:RadNumericTextBox ID="txtEmpNoHistory" runat="server" width="100%" 
                                                        MinValue="0" ToolTip="(Note: Employee No. must start with 1000. Example: 10003632)" 
                                                        Skin="Office2010Silver" DataType="System.Int32" MaxLength="8" MaxValue="99999999" 
                                                        EmptyMessage="1000xxxx">
                                                        <NumberFormat ZeroPattern="n" DecimalDigits="0" GroupSeparator="" />
                                                    </telerik:RadNumericTextBox> 
                                                </td>
                                                <td style="text-align: left; width: 35px; padding-left: 3px;">
                                                    <telerik:RadButton ID="btnFindEmpHistory" runat="server" Skin="Office2010Silver" 
                                                        Text="..." ToolTip="Click to open the Employee Search page." Enabled="true" 
                                                        Width="100%" Font-Bold="False" Font-Size="9pt" CausesValidation="false"
                                                        onclick="btnFindEmpHistory_Click">
                                                    </telerik:RadButton>
                                                </td> 
                                                <td />
                                            </tr>
                                        </table>
                                    </td>
                                    <td class="LabelBold" style="width: 90px;">
                                        Swipe Date
                                    </td>
                                    <td style="width: 150px;">
                                        <table border="0" style="width: 100%; text-align: left; margin: 0px; padding: 0px; table-layout: fixed;">
                                            <tr>
                                                <td style="width: 120px; padding-left: 0px;">
                                                    <telerik:RadDatePicker ID="dtpSwipeStartDate" runat="server"
                                                        Width="100%" Skin="Windows7">
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
                                                <td class="LabelBold" style="width: 15px; text-align: center; padding: 0px;">
                                                    ~
                                                </td>
                                                <td style="width: 200px;">
                                                    <telerik:RadDatePicker ID="dtpSwipeEndDate" runat="server"
                                                        Width="120px" Skin="Windows7">
                                                        <Calendar ID="Calendar5" runat="server" Skin="Windows7" UseColumnHeadersAsSelectors="False" 
                                                            UseRowHeadersAsSelectors="False" ViewSelectorText="x">
                                                        </Calendar>
                                                        <DateInput ID="DateInput5" runat="server" DateFormat="d/M/yyyy" DisplayDateFormat="d/M/yyyy">
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
                                            </tr>
                                        </table>                  
                                    </td>                    
                                    <td />
                                </tr>    
                                <tr style="height: 23px;">
                                    <td class="LabelBold">
                                        Cost Center             
                                    </td>
                                    <td style="padding-left: 4px;">
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
                                        
                                    </td>
                                    <td>
                                    </td>                    
                                    <td />
                                </tr>    
                                <tr style="height: 23px;">
                                    <td class="LabelBold">
                                        <asp:CustomValidator ID="cusValButton" runat="server" ControlToValidate="txtGeneric" 
                                            CssClass="LabelValidationError" Display="Dynamic" 
                                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                                    </td>
                                    <td colspan="3" style="padding-left: 3px; padding-top: 5px;">
                                        <telerik:RadButton ID="btnSearch" runat="server" Text="Search" ToolTip="Search matching database records" Width="70px" 
                                            CssClass="RadButtonStyle" ValidationGroup="valPrimary" Font-Size="9pt" OnClick="btnSearch_Click" Skin="Office2010Silver" />
                                        <telerik:RadButton ID="btnReset" runat="server" Text="Reset Criteria" ToolTip="Reset filter criterias" Width="100px" 
                                            CssClass="RadButtonStyle" ValidationGroup="valPrimary" Font-Size="9pt" OnClick="btnReset_Click" Skin="Office2010Silver" />                                                
                                    </td>
                                    <td />
                                </tr>                
                            </table>
                        </asp:Panel>

                        <asp:Panel ID="panManualAttendanceGrid" runat="server" BorderStyle="None" style="padding-left: 5px; padding-right: 10px; padding-bottom: 5px; margin: 0px;">
                             <table border="0" style="width: 100%; text-align: left; margin-top: 5px; table-layout: fixed;">
                                <tr>
                                    <td style="text-align: left; color: Purple; font-weight: bold; font-size: 9pt; padding-left: 5px;">
                                        <asp:Label ID="lblRecordCountAll" runat="server" Text="0 record found" Width="100%" />                         
                                    </td>
                                </tr>
                            </table>
                            <table border="0" style="width: 100%; text-align: left; margin-top: 0px; table-layout: fixed;">
                                <tr>
                                    <td>
                                        <telerik:RadGrid ID="gridAttendanceHistoryAll" runat="server"
                                            AllowSorting="true" AllowMultiRowSelection="true"
                                            PageSize="10" AutoGenerateColumns="false" Skin="Silk" 
                                            GridLines="None" Width="100%" Height="" CellSpacing="0"
                                            onpageindexchanged="gridAttendanceHistoryAll_PageIndexChanged" 
                                            onpagesizechanged="gridAttendanceHistoryAll_PageSizeChanged" 
                                            onsortcommand="gridAttendanceHistoryAll_SortCommand" 
                                            onitemcommand="gridAttendanceHistoryAll_ItemCommand" 
                                            onitemdatabound="gridAttendanceHistoryAll_ItemDataBound" 
                                            Font-Names="Tahoma" Font-Size="9pt" Font-Bold="False" 
                                            AllowPaging = "true" BorderStyle="Outset" BorderWidth="1px" 
                                            AllowCustomPaging="True" VirtualItemCount="1">
                                            <ExportSettings ExportOnlyData="true" IgnorePaging="true" OpenInNewWindow="true" FileName="ManualAttendanceList" HideStructureColumns="true">
                                                <Pdf PageHeight="210mm" PageWidth="310mm" PageTitle="Manual Attendance List" DefaultFontFamily="Arial Unicode MS"
                                                PageBottomMargin="20mm" PageTopMargin="20mm" PageLeftMargin="20mm" PageRightMargin="20mm" PaperSize="A4" AllowPrinting="true" />
                                            </ExportSettings>
                                            <AlternatingItemStyle Font-Names="Tahoma" Font-Size="9pt" Wrap="True" />
                                            <MasterTableView DataKeyNames="AutoID" ClientDataKeyNames="AutoID" 
                                                NoMasterRecordsText="No record found." 
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
                                                    <telerik:GridClientSelectColumn HeaderText="Select" HeaderStyle-Width="50px" 
                                                        HeaderStyle-Font-Bold="true" HeaderStyle-Font-Size = "9pt" 
                                                        UniqueName="CheckboxSelectColumn" Visible="false">                                                                                            
                                                        <HeaderStyle Font-Bold="True" Font-Size="9pt" Width="35px" HorizontalAlign="Center" />
                                                        <ItemStyle HorizontalAlign="Center" />
                                                    </telerik:GridClientSelectColumn>  
                                                    <telerik:GridButtonColumn ButtonType="LinkButton" CommandName="Select" Text="Edit" UniqueName="EditLinkButton" HeaderTooltip="Open selected record for editing" Visible="false">
                                                        <HeaderStyle Width="40px" HorizontalAlign="Center" Font-Bold="true" Font-Size="9pt" />
                                                        <ItemStyle HorizontalAlign="Center" Font-Bold="true" Font-Size="9pt" ForeColor="Blue" />
					                                </telerik:GridButtonColumn>    
                                                    <telerik:GridButtonColumn ButtonType="LinkButton" CommandName="Select" Text="View" UniqueName="ViewLinkButton" HeaderTooltip="View details about the selected record" Visible="false">
                                                        <HeaderStyle Width="40px" HorizontalAlign="Center" Font-Bold="true" Font-Size="9pt" />
                                                        <ItemStyle HorizontalAlign="Center" Font-Bold="true" Font-Size="9pt" ForeColor="Blue" />
					                                </telerik:GridButtonColumn>      
                                                    <telerik:GridBoundColumn DataField="EmpNo" DataType="System.Int32" HeaderText="Emp. No." 
                                                        ReadOnly="True" SortExpression="EmpNo" UniqueName="EmpNo">
                                                        <ColumnValidationSettings>
                                                            <ModelErrorMessage Text="" />
                                                        </ColumnValidationSettings>
                                                        <HeaderStyle Width="90px" Font-Bold="True" />
                                                        <ItemStyle Font-Bold="true" ForeColor="Purple" />
                                                    </telerik:GridBoundColumn> 
                                                    <telerik:GridTemplateColumn DataField="EmpName" HeaderText="Emp. Name" 
                                                        SortExpression="EmpName" UniqueName="EmpName">
								                        <HeaderStyle Width="250px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								                        <ItemTemplate>
									                        <div class="columnEllipsis" style="width: 240px; text-align: left;">
										                        <asp:Literal ID="litEmpName" runat="server" Text='<%# Eval("EmpName") %>' />
									                        </div>
								                        </ItemTemplate>
							                        </telerik:GridTemplateColumn> 
                                                    <telerik:GridTemplateColumn DataField="Position" HeaderText="Position" 
                                                        SortExpression="Position" UniqueName="Position" Visible="false">
								                        <HeaderStyle Width="220px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								                        <ItemTemplate>
									                        <div class="columnEllipsis" style="width: 210px; text-align: left;">
										                        <asp:Literal ID="litPosition" runat="server" Text='<%# Eval("Position") %>' />
									                        </div>
								                        </ItemTemplate>
							                        </telerik:GridTemplateColumn> 
                                                    <telerik:GridBoundColumn DataField="CostCenter" DataType="System.String" HeaderText="Cost Center" 
                                                        ReadOnly="True" SortExpression="CostCenter" UniqueName="CostCenter">
                                                        <ColumnValidationSettings>
                                                            <ModelErrorMessage Text="" />
                                                        </ColumnValidationSettings>
                                                        <HeaderStyle Width="90px" Font-Bold="True" />
                                                    </telerik:GridBoundColumn> 
                                                    <telerik:GridTemplateColumn DataField="CostCenterFullName" HeaderText="Cost Center" Visible="false" 
                                                        SortExpression="CostCenterFullName" UniqueName="CostCenterFullName">
								                        <HeaderStyle Width="250px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								                        <ItemTemplate>
									                        <div class="columnEllipsis" style="width: 240px; text-align: left;">
										                        <asp:Literal ID="litCostCenterFullName" runat="server" Text='<%# Eval("CostCenterFullName") %>' />
									                        </div>
								                        </ItemTemplate>
							                        </telerik:GridTemplateColumn> 
                                                    <telerik:GridBoundColumn DataField="SwipeIn" HeaderText="Time In"
                                                        DataFormatString="{0:dd-MMM-yyyy HH:mm tt}" DataType="System.DateTime" 
                                                        ReadOnly="True" SortExpression="SwipeIn" UniqueName="SwipeIn">
                                                        <ColumnValidationSettings>
                                                            <ModelErrorMessage Text="" />
                                                        </ColumnValidationSettings>
                                                        <HeaderStyle Width="160px" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma"></HeaderStyle>
                                                    </telerik:GridBoundColumn>   
                                                    <telerik:GridBoundColumn DataField="SwipeOut" HeaderText="Time Out"
                                                        DataFormatString="{0:dd-MMM-yyyy HH:mm tt}" DataType="System.DateTime" 
                                                        ReadOnly="True" SortExpression="SwipeOut" UniqueName="SwipeOut">
                                                        <ColumnValidationSettings>
                                                            <ModelErrorMessage Text="" />
                                                        </ColumnValidationSettings>
                                                        <HeaderStyle Width="160px" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma"></HeaderStyle>
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
                                                    <telerik:GridBoundColumn DataField="LastUpdateTime" HeaderText="Last Update Time"
                                                        DataFormatString="{0:dd-MMM-yyyy h:mm tt}" DataType="System.DateTime" 
                                                        FilterControlAltText="Filter Modified Date column" ReadOnly="True" SortExpression="LastUpdateTime" UniqueName="LastUpdateTime">
                                                        <ColumnValidationSettings>
                                                            <ModelErrorMessage Text="" />
                                                        </ColumnValidationSettings>
                                                        <HeaderStyle Font-Bold="True" Font-Size="8pt" Width="160px" Font-Names="Tahoma"></HeaderStyle>
                                                    </telerik:GridBoundColumn>     
                                                    <telerik:GridTemplateColumn DataField="IsContractor" HeaderText="Contractor?" Display="false" 
                                                        SortExpression="IsContractor" UniqueName="IsContractor">
								                        <HeaderStyle Width="100px" HorizontalAlign="Center" />
								                        <ItemTemplate>
									                        <div style="width: 100px; text-align: center;">
										                        <asp:Label ID="lblIsContractor" runat="server" Text='<%# Convert.ToBoolean(Eval("IsContractor")) == true ? "Yes" : "No" %>'></asp:Label>  
									                        </div>
								                        </ItemTemplate>
							                        </telerik:GridTemplateColumn>   
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
                    </asp:Panel>                                        
                </td>
            </tr>
        </table>
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
                <telerik:AjaxSetting AjaxControlID="btnSwipeIn">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>     
                <telerik:AjaxSetting AjaxControlID="btnSwipeOut">
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
                <telerik:AjaxSetting AjaxControlID="btnFindEmpHistory">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>   
                <telerik:AjaxSetting AjaxControlID="btnClear">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>   
                <telerik:AjaxSetting AjaxControlID="rblSwipeIn">
				    <UpdatedControls>
                        <telerik:AjaxUpdatedControl ControlID="panAttendanceAction" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>     
                <telerik:AjaxSetting AjaxControlID="rblSwipeOut">
				    <UpdatedControls>
                        <telerik:AjaxUpdatedControl ControlID="panAttendanceAction" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>
                <telerik:AjaxSetting AjaxControlID="gridEmpAttendanceHistory">
				    <UpdatedControls>
                        <telerik:AjaxUpdatedControl ControlID="gridEmpAttendanceHistory" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>
                <telerik:AjaxSetting AjaxControlID="gridAttendanceHistoryAll">
				    <UpdatedControls>
                        <telerik:AjaxUpdatedControl ControlID="gridAttendanceHistoryAll" LoadingPanelID="loadingPanel" />  
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
