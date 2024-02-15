<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/TASMaster.Master" AutoEventWireup="true" CodeBehind="AttendanceDashboard.aspx.cs" 
    Inherits="GARMCO.AMS.TAS.UI.Views.UserFunctions.AttendanceDashboard" StylesheetTheme="Standard" %>

<%@ MasterType VirtualPath="~/Views/Shared/TASMaster.Master" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <title>Employee Attendance Dashboard</title>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="mainContent" runat="server">
    <telerik:RadFormDecorator ID="formDecor" runat="server" DecoratedControls="Buttons" Skin="Windows7" />

    <table border="0" style="width: 100%; text-align: left; margin-top: 5px; margin-left: 5px;">
        <tr>
            <td colspan="2" style="padding-left: 10px;">
                <table border="0" style="width: 100%; text-align: left;">
                    <tr>
                        <td style="width: 50px; text-align: right; padding-right: 5px;" rowspan="2">
                            <img alt="" src="../../Images/dashboard_icon.jpg" />
                        </td>
                        <td id="tdPageTitle" runat="server" class="PageTitleLabel" style="vertical-align: bottom; padding-left: 5px; width: 900px; font-size: 11pt;">
                            Employee Attendance Dashboard
                        </td>
                        <td style="width: 50px;">
                            <asp:LinkButton ID="lnkMoveUp" runat="server" />
                        </td>
                        <td />
                    </tr>
                    <tr>
                        <td class="PageDescriptionHeader" style="text-align: left; vertical-align: top; padding-left: 5px; margin: 0px;">
                            View the employee's attendance information on specific date 
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
                    <td id="tdEmployeeTitle" runat="server" class="LabelBold" style="width: 120px;">
                        <asp:CustomValidator ID="cusValAttendanceDate" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                        Date     
                    </td>
                    <td style="width: 320px;">
                        <telerik:RadDatePicker ID="dtpAttendanceDate" runat="server"
                            Width="120px" Skin="Silk" Culture="en-US" ToolTip="Date is requierd">
                            <Calendar ID="Calendar3" runat="server" Skin="Silk" UseColumnHeadersAsSelectors="False" 
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
                    <td class="LabelBold" style="width: 110px;">
                        
                    </td>
                    <td style="width: 150px;">
                                           
                    </td>                    
                    <td />
                </tr>    
                <tr style="height: 23px;">
                    <td class="LabelBold">
                        Cost Center             
                    </td>
                    <td style="padding-left: 0px;">
                        <telerik:RadComboBox ID="cboCostCenter" runat="server"
							DropDownWidth="350px" 
							HighlightTemplatedItems="True" 
							MarkFirstMatch="true" 
							Skin="Office2010Silver" 
							Width="100%"     
                            Height="200px"                        
							EmptyMessage="Select Cost Center"
							EnableLoadOnDemand="false"
							EnableVirtualScrolling="true" 
                             AutoPostBack="True" Font-Names="Tahoma" Font-Size="9pt" OnSelectedIndexChanged="cboCostCenter_SelectedIndexChanged" />

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
					    </telerik:RadComboBox>--%>   
                    </td>
                    <td class="LabelBold">
                        
                    </td>
                    <td>
                        
                    </td>                    
                    <td />
                </tr>    
                <tr style="height: 23px;">
                    <td class="LabelBold">
                        Employee Name             
                    </td>
                    <td style="padding-left: 0px;">
                         <telerik:RadTextBox ID="txtEmpName" runat="server" Width="100%" 
                            EmptyMessage="Enter Employee Name" Skin="Office2010Silver" 
                            Font-Names="Tahoma" Font-Size="9pt" MaxLength="100" />
                    </td>
                    <td class="LabelBold">
                        
                    </td>
                    <td>
                          
                    </td>                    
                    <td />
                </tr>    
                <tr style="height: 23px;">
                    <td class="LabelBold">
                        
                    </td>
                    <td colspan="3" style="padding-left: 0px; padding-top: 2px;">
                        <telerik:RadButton ID="btnSearch" runat="server" Text="Search" ToolTip="Search matching database records" Width="70px" 
                            CssClass="RadButtonStyle" ValidationGroup="valPrimary" Font-Size="9pt" OnClick="btnSearch_Click" Skin="Office2010Silver" />
                        <telerik:RadButton ID="btnReset" runat="server" Text="Reset" ToolTip="Reset filter criterias" Width="70px" 
                            CssClass="RadButtonStyle" ValidationGroup="valPrimary" Font-Size="9pt" OnClick="btnReset_Click" Skin="Office2010Silver" />                                                
                    </td>
                    <td />
                </tr>                
            </table>
        </asp:Panel>

        <asp:Panel ID="panGrid" runat="server" BorderStyle="None" style="padding-left: 15px; padding-right: 15px; margin: 0px;">
             <table border="0" style="width: 100%; text-align: left; margin-top: 5px; table-layout: fixed;">
                <tr style="vertical-align: bottom;">
                    <td style="text-align: left; color: Purple; font-weight: bold; font-size: 9pt; padding-left: 5px;">
                        <asp:Label ID="lblRecordCount" runat="server" Text="0 record found" Width="100%" />                         
                    </td>
                </tr>
            </table>
            <table border="0" style="width: 100%; text-align: left; margin-top: 0px; padding-top: 0px; table-layout: fixed;">
                <tr style="vertical-align: top;">
                    <td style="padding-left: 0px; padding-top: 0px; width: 910px;">
                        <table id="tblGrid" runat="server" border="0" style="width: 100%; text-align: left; margin-top: 0px; table-layout: fixed; 
                            border-color: gray; border-width: 1px; border-style: none;">
                            <tr>
                                <td>
                                    <telerik:RadGrid ID="gridSearchResults" runat="server"
                                        AllowSorting="true" AllowMultiRowSelection="false"
                                        PageSize="10" AutoGenerateColumns="false" Skin="Silk" Width="100%" Height="500px" CellSpacing="0"
                                        onpageindexchanged="gridSearchResults_PageIndexChanged" 
                                        onpagesizechanged="gridSearchResults_PageSizeChanged" 
                                        onsortcommand="gridSearchResults_SortCommand" 
                                        onitemcommand="gridSearchResults_ItemCommand" 
                                        onitemdatabound="gridSearchResults_ItemDataBound" 
                                        Font-Names="Tahoma" Font-Size="9pt" Font-Bold="False" 
                                        AllowPaging = "false" BorderStyle="Outset" BorderWidth="1px">
                                        <ExportSettings ExportOnlyData="true" IgnorePaging="true" OpenInNewWindow="true" FileName="AttendanceList" HideStructureColumns="true">
                                            <Pdf PageHeight="210mm" PageWidth="310mm" PageTitle="Employee Attendance List" DefaultFontFamily="Arial Unicode MS"
                                            PageBottomMargin="20mm" PageTopMargin="20mm" PageLeftMargin="20mm" PageRightMargin="20mm" PaperSize="A4" AllowPrinting="true" />
                                        </ExportSettings>
                                        <AlternatingItemStyle Font-Names="Tahoma" Font-Size="9pt" Wrap="True" />
                                        <MasterTableView DataKeyNames="EmpNo" ClientDataKeyNames="EmpNo" 
                                            NoMasterRecordsText="No attendance record found." 
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
                                                <telerik:GridTemplateColumn DataField="EmployeeImagePath" HeaderText="" 
                                                    SortExpression="EmployeeImagePath" UniqueName="EmployeeImagePath">
								                    <HeaderStyle Width="55px" HorizontalAlign="Left" />
								                    <ItemTemplate>
									                    <div style="width: 100%; text-align: left;">                                                
                                                            <img id="imgPhoto" runat="server" 
                                                                src='<%# Eval("EmployeeImagePath") %>' 
                                                                alt='<%# Eval("EmployeeImageTooltip") %>' 
                                                                style="height: 50px; width: 50px;" />
									                    </div>
								                    </ItemTemplate>
							                    </telerik:GridTemplateColumn>                                                   
                                                <telerik:GridTemplateColumn DataField="EmpName" HeaderText="Employee Name" 
                                                    SortExpression="EmpName" UniqueName="EmpName">
								                    <HeaderStyle Width="280px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								                    <ItemTemplate>
									                    <div class="columnEllipsis" style="width: 270px; text-align: left;">
                                                            <asp:LinkButton ID="lnkEmpName" runat="server" Text='<%# Eval("EmpName") %>' 
                                                                Font-Bold="true" Font-Names="Tahoma" ForeColor="Blue" OnClick="lnkEmpName_Click" />
									                    </div>
								                    </ItemTemplate>
							                    </telerik:GridTemplateColumn> 
                                                <telerik:GridBoundColumn DataField="EmpNo" DataType="System.Int32" HeaderText="Emp. No."
                                                    ReadOnly="True" SortExpression="EmpNo" UniqueName="EmpNo">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Width="75px" Font-Bold="True"></HeaderStyle>
                                                </telerik:GridBoundColumn>      
                                                <telerik:GridBoundColumn DataField="ExtensionNo" DataType="System.String" HeaderText="Ext. No." 
                                                    ReadOnly="True" SortExpression="ExtensionNo" UniqueName="ExtensionNo">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Width="60px" Font-Bold="True" Font-Size="8pt" HorizontalAlign="Center" />
                                                    <ItemStyle HorizontalAlign="Center" />
                                                </telerik:GridBoundColumn> 
                                                <telerik:GridTemplateColumn DataField="StatusIconPath" HeaderText="Status" 
                                                    SortExpression="StatusIconPath" UniqueName="StatusIconPath">
								                    <HeaderStyle Width="60px" HorizontalAlign="Center" Font-Size="8pt" />
								                    <ItemTemplate>
									                    <div style="width: 100%; text-align: center;">    
                                                            <asp:Image ID="imgAvailability" runat="server" 
                                                                ImageUrl='<%# Eval("StatusIconPath") %>'
                                                                ToolTip='<%# Eval("StatusIconNotes") %>'
                                                                ImageAlign="Middle" Height="18px" Width="18px" />   
									                    </div>
								                    </ItemTemplate>
							                    </telerik:GridTemplateColumn>                                      
                                                <telerik:GridTemplateColumn DataField="AttendanceRemarks" HeaderText="Remarks" 
                                                    SortExpression="AttendanceRemarks" UniqueName="AttendanceRemarks">
								                    <HeaderStyle Width="300px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								                    <ItemTemplate>
									                    <div class="columnEllipsis" style="width: 290px; text-align: left;">
										                    <asp:Literal ID="litAttendanceRemarks" runat="server" Text='<%# Eval("AttendanceRemarks") %>' />
									                    </div>
								                    </ItemTemplate>
							                    </telerik:GridTemplateColumn> 
                                                <telerik:GridBoundColumn DataField="SwipeDate" HeaderText="Swipe Date"
                                                    DataFormatString="{0:dd-MMM-yyyy}" DataType="System.DateTime" Display="false" 
                                                    ReadOnly="True" SortExpression="SwipeDate" UniqueName="SwipeDate">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma"></HeaderStyle>
                                                </telerik:GridBoundColumn>   
                                                <telerik:GridBoundColumn DataField="InOutStatus" DataType="System.String" HeaderText="InOutStatus" 
                                                    ReadOnly="True" SortExpression="InOutStatus" UniqueName="InOutStatus" Display="false">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                </telerik:GridBoundColumn>                                                 
                                                <telerik:GridBoundColumn DataField="SupervisorEmpNo" DataType="System.Int32" HeaderText="SupervisorEmpNo"
                                                    ReadOnly="True" SortExpression="SupervisorEmpNo" UniqueName="SupervisorEmpNo" Display="false">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Width="75px" Font-Bold="True"></HeaderStyle>
                                                </telerik:GridBoundColumn>  
                                                <telerik:GridBoundColumn DataField="AttendanceDate" HeaderText="AttendanceDate"
                                                    DataFormatString="{0:dd-MMM-yyyy}" DataType="System.DateTime" Display="false" 
                                                    ReadOnly="True" SortExpression="AttendanceDate" UniqueName="AttendanceDate">
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
                    </td>
                    <td style="padding-left: 0px; padding-top: 3px; padding-left: 5px; padding-right: 0px; width: auto;">
                        <telerik:RadGrid ID="gridSwipeDetail" runat="server"
                            AllowSorting="true" AllowMultiRowSelection="true"
                            PageSize="10" AutoGenerateColumns="false" Skin="Silk" Width="400px" CellSpacing="0"
                            onpageindexchanged="gridSwipeDetail_PageIndexChanged" 
                            onpagesizechanged="gridSwipeDetail_PageSizeChanged" 
                            onsortcommand="gridSwipeDetail_SortCommand" 
                            onitemcommand="gridSwipeDetail_ItemCommand" 
                            onitemdatabound="gridSwipeDetail_ItemDataBound" 
                            Font-Names="Tahoma" Font-Size="9pt" Font-Bold="False" 
                            AllowPaging = "true" BorderStyle="Outset" 
                            BorderWidth="1px" Visible="false">
                            <ExportSettings ExportOnlyData="true" IgnorePaging="true" OpenInNewWindow="true" FileName="AttendanceList" HideStructureColumns="true">
                                <Pdf PageHeight="210mm" PageWidth="310mm" PageTitle="Employee Attendance List" DefaultFontFamily="Arial Unicode MS"
                                PageBottomMargin="20mm" PageTopMargin="20mm" PageLeftMargin="20mm" PageRightMargin="20mm" PaperSize="A4" AllowPrinting="true" />
                            </ExportSettings>
                            <AlternatingItemStyle Font-Names="Tahoma" Font-Size="9pt" Wrap="True" />
                            <MasterTableView DataKeyNames="DT" ClientDataKeyNames="DT" 
                                NoMasterRecordsText="No swipe record found." 
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
                                    <telerik:GridBoundColumn DataField="DT" HeaderText="Swipe Time"
                                        DataFormatString="{0:dd-MMM-yyyy h:mm:ss tt}" DataType="System.DateTime" 
                                        ReadOnly="True" SortExpression="DT" UniqueName="DT">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="160px" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma"></HeaderStyle>
                                    </telerik:GridBoundColumn>    
                                    <telerik:GridBoundColumn DataField="SwipeType" DataType="System.String" HeaderText="Status" 
                                        ReadOnly="True" SortExpression="SwipeType" UniqueName="SwipeType">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="60px" Font-Bold="True" Font-Size="8pt" HorizontalAlign="Center" />
                                        <ItemStyle HorizontalAlign="Center" />
                                    </telerik:GridBoundColumn> 
                                    <telerik:GridBoundColumn DataField="SwipeLocation" DataType="System.String" HeaderText="Location" 
                                        ReadOnly="True" SortExpression="SwipeLocation" UniqueName="SwipeLocation">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Font-Bold="True" Font-Size="8pt" HorizontalAlign="Center" />
                                        <ItemStyle HorizontalAlign="Center" />
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

        <asp:Panel ID="panButton" runat="server" BorderStyle="None" style="padding-left: 15px; margin: 0px; display: none;">
            <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                <tr>                    
                    <td style="padding-top: 3px; text-align: left;">
                        <asp:CustomValidator ID="cusValButton" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                    </td>
                </tr>
            </table>             
        </asp:Panel>  

        <asp:Panel ID="panLegends" runat="server" BorderStyle="None" Direction="LeftToRight" style="margin-left: 15px; padding-top: 0px; padding-bottom: 0px; margin-top: 0px;">
             <table border="0" style="width: 100%; table-layout: fixed; padding-top: 0px; margin-top: 0px;">   
                <tr>
                    <td style="width: 60px; text-align: left; font-style: italic; text-decoration: underline; font-size: 8pt; font-family: Verdana; font-weight: bold; color: gray; vertical-align: bottom; padding-bottom: 3px;">
                        LEGEND:
                    </td>
                    <td style="width: 90px">
                        
                    </td>
                    <td style="width: 20px; text-align: right; padding-top: 5px;">
                        
                    </td>
                    <td style="width: 75px;">
                        
                    </td>
                    <td style="width: 20px; text-align: right; padding-top: 5px;">
                        
                    </td>
                    <td style="width: 80px;">
                        
                    </td>
                    <td style="width: 20px; text-align: right; padding-top: 5px;">
                        
                    </td>
                    <td style="width: 65px;">
                        
                    </td>
                    <td style="width: 20px; text-align: right; padding-top: 5px;">
                        
                    </td>
                    <td style="width: 430px;">
                        <table border="0" style="width: 100%; table-layout: fixed; padding-top: 0px; margin-top: 0px;">   
                            <tr style="vertical-align: top;">
                                <td style="width: auto;">
                                </td>
                                <td class="LabelBold" style="width: 85px; padding-right: 0px; padding-top: 3px; color: purple;">
                                    Enable Paging
                                </td>
                                <td style="width: 20px; padding-top: 0px;">
                                    <asp:CheckBox ID="chkEnablePaging" runat="server" Text="" AutoPostBack="True" OnCheckedChanged="chkEnablePaging_CheckedChanged" />
                                </td>
                                <td id="tdShowPhotoTitle" runat="server" class="LabelBold" style="width: 75px; padding-right: 0px; padding-top: 3px; color: purple;">
                                    Show Photo
                                </td>
                                <td id="tdShowPhotoCheckbox" runat="server" style="width: 20px; padding-top: 0px;">
                                    <asp:CheckBox ID="chkShowPhoto" runat="server" Text="" AutoPostBack="True" OnCheckedChanged="chkShowPhoto_CheckedChanged" />
                                </td>
                            </tr>
                        </table>
                    </td>
                    <td />
                </tr>
                <tr>
                    <td colspan="10">
                        <table border="0" style="width: 100%; table-layout: fixed; padding-top: 0px; margin-top: 0px;">   
                            <tr style="vertical-align: top;">
                                <td style="width: 20px; text-align: right; padding-top: 0px;">
                                    <asp:Image ID="imgLegGreen" runat="server" ImageUrl="~/Images/ArrivalNormal.ICO" />                        
                                </td>
                                <td style="width: 80px;">
                                    Arrival - Normal
                                </td>
                                <td style="width: 23px; text-align: right; padding-top: 0px;">
                                    <asp:Image ID="Image2" runat="server" ImageUrl="~/Images/ArrivalLate.png" /> 
                                </td>
                                <td style="width: 70px;">
                                    Arrival - Late
                                </td>
                                <td style="width: 20px; text-align: right; padding-top: 0px;">
                                    <asp:Image ID="Image3" runat="server" ImageUrl="~/Images/LeftNormal.ico" /> 
                                </td>
                                <td style="width: 70px;">
                                    Left - Normal
                                </td>
                                <td style="width: 20px; text-align: right; padding-top: 0px;">
                                    <asp:Image ID="Image4" runat="server" ImageUrl="~/Images/LeftEarly.png" /> 
                                </td>
                                <td style="width: 60px;">
                                    Left - Early
                                </td>
                                <td style="width: 20px; text-align: right; padding-top: 0px;">
                                    <asp:Image ID="Image5" runat="server" ImageUrl="~/Images/NotComeYet.ICO" /> 
                                </td>
                                <td style="width: 190px;">
                                    Not Come Yet or Day Off or On-leave
                                </td>
                                <td />
                            </tr>
                        </table>
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
        <telerik:RadButton ID="btnRebind" runat="server" Text="" Skin="Office2010Silver" CausesValidation="false" onclick="btnRebind_Click" />
    </asp:Panel>

    <asp:Panel ID="panAjaxManager" runat="server">
        <telerik:RadAjaxManager ID="MyAjaxManager" runat="server">
            <AjaxSettings>
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
                <telerik:AjaxSetting AjaxControlID="gridSearchResults">
				    <UpdatedControls>
                        <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>   
                <telerik:AjaxSetting AjaxControlID="gridSwipeDetail">
				    <UpdatedControls>
                        <telerik:AjaxUpdatedControl ControlID="gridSwipeDetail" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>   
                <telerik:AjaxSetting AjaxControlID="cboCostCenter">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>  
                <telerik:AjaxSetting AjaxControlID="chkShowPhoto">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="gridSearchResults" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>  
                <telerik:AjaxSetting AjaxControlID="chkEnablePaging">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="gridSearchResults" LoadingPanelID="loadingPanel" />  
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
