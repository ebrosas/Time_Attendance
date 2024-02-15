<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/TASMaster.Master" AutoEventWireup="true" CodeBehind="TimesheetProcessAnalysis.aspx.cs" 
    Inherits="GARMCO.AMS.TAS.UI.Views.AdminFunctions.TimesheetProcessAnalysis" StylesheetTheme="Standard" %>

<%@ MasterType VirtualPath="~/Views/Shared/TASMaster.Master" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <title>Resigned But Swiped Inquiry</title>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="mainContent" runat="server">
    <telerik:RadFormDecorator ID="formDecor" runat="server" DecoratedControls="Buttons" Skin="Office2010Silver" />

    <table border="0" style="width: 100%; text-align: left; margin-top: 5px; margin-left: 5px;">
        <tr>
            <td colspan="2" style="padding-left: 10px;">
                <table border="0" style="width: 100%; text-align: left;">
                    <tr>
                        <td style="width: 50px; text-align: right; padding-right: 5px;" rowspan="2">
                            <img alt="" src="../../Images/empployee_search.jpg" />
                        </td>
                        <td class="PageTitleLabel" style="vertical-align: bottom; padding-left: 5px; width: 900px; font-size: 11pt;">
                            Timesheet Process / SPU Service Analysis
                        </td>
                        <td style="width: 50px;">
                            <asp:LinkButton ID="lnkMoveUp" runat="server" />
                        </td>
                        <td />
                    </tr>
                    <tr>
                        <td class="PageDescriptionHeader" style="text-align: left; vertical-align: top; padding-left: 5px; margin: 0px;">
                            Enable System Administrators to troubleshoot the Timesheet Process and SPU service execution by analyzing the log details
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
                    <td class="LabelBold" style="width: 140px;">
                        Last SPU Run
                    </td>
                    <td class="TextNormal" style="width: 200px;">
                        <asp:Literal ID="litLastSPURun" runat="server" Text="Not defined" />  
                    </td>   
                    <td />
                </tr>    
                <tr style="height: 23px;">
                    <td class="LabelBold">
                        Swipe Last Processed
                    </td>
                    <td class="TextNormal">
                        <asp:Literal ID="litSwipeLastProcess" runat="server" Text="Not defined" />          
                    </td>   
                    <td />
                </tr>   
                <tr>
                    <td class="LabelBold">
                        <asp:CustomValidator ID="cusValProcessDate" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                        Attendance Date
                    </td>
                    <td>
                         <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                            <tr>
                                <td style="width: 120px; padding-left: 0px; margin-left: 0px;">
                                    <telerik:RadDatePicker ID="dtpProcessDate" runat="server"
                                        Width="100%" Skin="Windows7" Culture="en-US" ToolTip="(Note: The maximum date that can be specified is today's date.)">
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
                                <td>
                                    <telerik:RadButton ID="btnSearch" runat="server" Text="Search" ToolTip="Search matching database records" Width="70px" 
                                        CssClass="RadButtonStyle" ValidationGroup="valPrimary" Font-Size="9pt" OnClick="btnSearch_Click" Skin="Office2010Silver" />
                                </td>
                            </tr>
                        </table>                                    
                    </td>
                    <td style="text-align: left;">
                        <asp:CustomValidator ID="cusValButton" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                    </td>
                </tr>                          
            </table>
        </asp:Panel>

        <asp:Panel ID="panLogDetail" runat="server" style="width: 100%; text-align: left; table-layout: fixed; margin-top: 10px; margin-left: 5px; margin-right: 5px;">
            <telerik:RadTabStrip ID="tabLogDetail" runat="server" SelectedIndex="0"   
                MultiPageID="multiPageLogDetail" ReorderTabsOnSelect="True" 
                CausesValidation="False" ontabclick="tabLogDetail_TabClick" 
                style="padding-top: 0px; padding-left: 0px; padding-right: 0px;" 
                Skin="Silk">
                <Tabs>
                    <telerik:RadTab Text="SPU Log Details" Font-Size="9pt" Font-Bold="True" Selected="True" Value="valSPUTab" />
                    <telerik:RadTab Text="Timesheet Process Log Details" Font-Size="9pt" Font-Bold="True" Value="valTimesheetProcessTab" />
                    <telerik:RadTab Text="Shift Pointer Count" Font-Size="9pt" Font-Bold="True" Value="valShiftPointerTab" />
                </Tabs>
            </telerik:RadTabStrip>                                                                                

            <telerik:RadMultiPage ID="multiPageLogDetail" runat="server" SelectedIndex="0" Width="99%" BorderColor="Silver" BorderStyle="None" BorderWidth="1px"                              
                style="margin-top: 0px; margin-left: 5px; padding-right: 0px; margin-right: 0px;">

                <telerik:RadPageView ID="SPULogView" runat="server">
                    <asp:Panel ID="panSPULog" runat="server" Visible="true" style="width: 100%; text-align: left; table-layout: fixed; margin-top: 5px;">
                        <table border="0" style="width: 100%; text-align: left; table-layout: fixed;">
                            <tr>
                                <td style="padding-left: 0px; padding-right: 0px;">
                                    <telerik:RadGrid ID="gridSPULog" runat="server"
                                        AllowSorting="true" AllowMultiRowSelection="true"
                                        PageSize="10" AutoGenerateColumns="false" Skin="Silk" 
                                        GridLines="None" Width="100%" Height="" CellSpacing="0"
                                        onpageindexchanged="gridSPULog_PageIndexChanged" 
                                        onpagesizechanged="gridSPULog_PageSizeChanged" 
                                        onsortcommand="gridSPULog_SortCommand" 
                                        onitemcommand="gridSPULog_ItemCommand" 
                                        onitemdatabound="gridSPULog_ItemDataBound" 
                                        Font-Names="Tahoma" Font-Size="9pt" Font-Bold="False" 
                                        AllowPaging = "true" BorderStyle="Outset" BorderWidth="1px">
                                        <ExportSettings ExportOnlyData="true" IgnorePaging="true" OpenInNewWindow="true" FileName="WorkingCostCenterList" HideStructureColumns="true">
                                            <Pdf PageHeight="210mm" PageWidth="310mm" PageTitle="Working Cost Center List" DefaultFontFamily="Arial Unicode MS"
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
                                                <telerik:GridBoundColumn DataField="AutoID" DataType="System.Int32" HeaderText="Transaction ID" 
                                                    ReadOnly="True" SortExpression="AutoID" UniqueName="AutoID">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Width="120px" Font-Bold="True" />
                                                </telerik:GridBoundColumn> 
                                                <telerik:GridBoundColumn DataField="LogDate" HeaderText="Log Date"
                                                    DataFormatString="{0:dd-MMM-yyyy}" DataType="System.DateTime" 
                                                    ReadOnly="True" SortExpression="LogDate" UniqueName="LogDate">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Font-Bold="True" Font-Size="8pt" Width="110px" Font-Names="Tahoma"></HeaderStyle>
                                                </telerik:GridBoundColumn>   
                                                <telerik:GridBoundColumn DataField="SPUDate" HeaderText="SPU Date"
                                                    DataFormatString="{0:dd-MMM-yyyy}" DataType="System.DateTime" 
                                                    ReadOnly="True" SortExpression="SPUDate" UniqueName="SPUDate">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Font-Bold="True" Font-Size="8pt" Width="110px" Font-Names="Tahoma"></HeaderStyle>
                                                </telerik:GridBoundColumn> 
                                                <telerik:GridBoundColumn DataField="ProcessID" DataType="System.Int32" HeaderText="Process ID" 
                                                    ReadOnly="True" SortExpression="ProcessID" UniqueName="ProcessID">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Width="90px" Font-Bold="True" />
                                                </telerik:GridBoundColumn> 
                                                <telerik:GridTemplateColumn DataField="LogDescription" HeaderText="Log Description" 
                                                    SortExpression="LogDescription" UniqueName="LogDescription">
								                    <HeaderStyle Width="500px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								                    <ItemTemplate>
									                    <div class="columnEllipsis" style="width: 490px; text-align: left;">
										                    <asp:Literal ID="litLogDescription" runat="server" Text='<%# Eval("LogDescription") %>' />
									                    </div>
								                    </ItemTemplate>
							                    </telerik:GridTemplateColumn> 
                                                <telerik:GridBoundColumn DataField="LogErrorDesc" DataType="System.String" HeaderText="Error Description" 
                                                    ReadOnly="True" SortExpression="LogErrorDesc" UniqueName="LogErrorDesc">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Font-Bold="True" />
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

                <telerik:RadPageView ID="TimesheetProcessLogView" runat="server">
                    <asp:Panel ID="panTimesheetProcessLog" runat="server" Visible="true" style="width: 100%; text-align: left; table-layout: fixed; margin-top: 5px;">
                        <table border="0" style="width: 100%; text-align: left; table-layout: fixed;">
                            <tr>
                                <td style="padding-left: 0px; padding-right: 0px;">
                                    <telerik:RadGrid ID="gridTimesheetLog" runat="server"
                                        AllowSorting="true" AllowMultiRowSelection="true"
                                        PageSize="10" AutoGenerateColumns="false" Skin="Silk" 
                                        GridLines="None" Width="100%" Height="" CellSpacing="0"
                                        onpageindexchanged="gridTimesheetLog_PageIndexChanged" 
                                        onpagesizechanged="gridTimesheetLog_PageSizeChanged" 
                                        onsortcommand="gridTimesheetLog_SortCommand" 
                                        onitemcommand="gridTimesheetLog_ItemCommand" 
                                        onitemdatabound="gridTimesheetLog_ItemDataBound" 
                                        Font-Names="Tahoma" Font-Size="9pt" Font-Bold="False" 
                                        AllowPaging = "true" BorderStyle="Outset" BorderWidth="1px">
                                        <ExportSettings ExportOnlyData="true" IgnorePaging="true" OpenInNewWindow="true" FileName="WorkingCostCenterList" HideStructureColumns="true">
                                            <Pdf PageHeight="210mm" PageWidth="310mm" PageTitle="Working Cost Center List" DefaultFontFamily="Arial Unicode MS"
                                            PageBottomMargin="20mm" PageTopMargin="20mm" PageLeftMargin="20mm" PageRightMargin="20mm" PaperSize="A4" AllowPrinting="true" />
                                        </ExportSettings>
                                        <AlternatingItemStyle Font-Names="Tahoma" Font-Size="9pt" Wrap="True" />
                                        <MasterTableView DataKeyNames="MessageID" ClientDataKeyNames="MessageID" 
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
                                                <telerik:GridBoundColumn DataField="MessageID" DataType="System.Int32" HeaderText="Transaction ID" 
                                                    ReadOnly="True" SortExpression="MessageID" UniqueName="MessageID">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Width="120px" Font-Bold="True" />
                                                </telerik:GridBoundColumn> 
                                                <telerik:GridBoundColumn DataField="ProcessDate" HeaderText="Process Date"
                                                    DataFormatString="{0:dd-MMM-yyyy}" DataType="System.DateTime" 
                                                    ReadOnly="True" SortExpression="ProcessDate" UniqueName="ProcessDate">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Font-Bold="True" Font-Size="8pt" Width="110px" Font-Names="Tahoma"></HeaderStyle>
                                                </telerik:GridBoundColumn>   
                                                <telerik:GridBoundColumn DataField="ProcessID" DataType="System.Int32" HeaderText="Process ID" 
                                                    ReadOnly="True" SortExpression="ProcessID" UniqueName="ProcessID">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Width="90px" Font-Bold="True" />
                                                </telerik:GridBoundColumn> 
                                                <telerik:GridBoundColumn DataField="Message" DataType="System.String" HeaderText="Log Description" 
                                                    ReadOnly="True" SortExpression="Message" UniqueName="Message">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Font-Bold="True" />
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
                </telerik:RadPageView>

                <telerik:RadPageView ID="ShiftPointerView" runat="server">
                    <asp:Panel ID="panShiftPointer" runat="server" Visible="true" style="width: 100%; text-align: left; table-layout: fixed; margin-top: 5px;">
                        <table border="0" style="width: 100%; text-align: left; table-layout: fixed;">
                            <tr>
                                <td style="padding-left: 0px; padding-right: 0px;">
                                    <telerik:RadGrid ID="gridShiftPointer" runat="server"
                                        AllowSorting="true" AllowMultiRowSelection="true"
                                        PageSize="10" AutoGenerateColumns="false" Skin="Silk" 
                                        GridLines="None" Width="100%" Height="" CellSpacing="0"
                                        onpageindexchanged="gridShiftPointer_PageIndexChanged" 
                                        onpagesizechanged="gridShiftPointer_PageSizeChanged" 
                                        onsortcommand="gridShiftPointer_SortCommand" 
                                        onitemcommand="gridShiftPointer_ItemCommand" 
                                        onitemdatabound="gridShiftPointer_ItemDataBound" 
                                        Font-Names="Tahoma" Font-Size="9pt" Font-Bold="False" 
                                        AllowPaging = "true" BorderStyle="Outset" BorderWidth="1px">
                                        <ExportSettings ExportOnlyData="true" IgnorePaging="true" OpenInNewWindow="true" FileName="WorkingCostCenterList" HideStructureColumns="true">
                                            <Pdf PageHeight="210mm" PageWidth="310mm" PageTitle="Working Cost Center List" DefaultFontFamily="Arial Unicode MS"
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
                                                <telerik:GridBoundColumn DataField="ShiftPatCode" DataType="System.String" HeaderText="Shift Pattern" 
                                                    ReadOnly="True" SortExpression="ShiftPatCode" UniqueName="ShiftPatCode">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Font-Bold="True" Width="110px" />
                                                    <ItemStyle Font-Bold="true" ForeColor="Purple" />
                                                </telerik:GridBoundColumn> 
                                                <telerik:GridBoundColumn DataField="ShiftCode" DataType="System.String" HeaderText="Shift Code" 
                                                    ReadOnly="True" SortExpression="ShiftCode" UniqueName="ShiftCode">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Font-Bold="True" Width="100px" />
                                                </telerik:GridBoundColumn> 
                                                <telerik:GridBoundColumn DataField="ShiftPointer" DataType="System.Int32" HeaderText="Shift Pointer" 
                                                    ReadOnly="True" SortExpression="ShiftPointer" UniqueName="ShiftPointer">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Width="100px" Font-Bold="True" />                                                    
                                                </telerik:GridBoundColumn> 
                                                <telerik:GridBoundColumn DataField="EmpCount" DataType="System.Int32" HeaderText="Employee Count" 
                                                    ReadOnly="True" SortExpression="EmpCount" UniqueName="EmpCount">
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
                <telerik:AjaxSetting AjaxControlID="gridSPULog">
				    <UpdatedControls>
                        <telerik:AjaxUpdatedControl ControlID="gridSPULog" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>   
                <telerik:AjaxSetting AjaxControlID="gridTimesheetLog">
				    <UpdatedControls>
                        <telerik:AjaxUpdatedControl ControlID="gridTimesheetLog" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>  
                <telerik:AjaxSetting AjaxControlID="gridShiftPointer">
				    <UpdatedControls>
                        <telerik:AjaxUpdatedControl ControlID="gridShiftPointer" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>  
                <telerik:AjaxSetting AjaxControlID="tabLogDetail">
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
