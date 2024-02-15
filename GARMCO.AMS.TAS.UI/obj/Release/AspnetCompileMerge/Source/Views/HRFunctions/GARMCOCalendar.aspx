<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/TASMaster.Master" AutoEventWireup="true" CodeBehind="GARMCOCalendar.aspx.cs" 
    Inherits="GARMCO.AMS.TAS.UI.Views.HRFunctions.GARMCOCalendar" StylesheetTheme="Standard" %>

<%@ MasterType VirtualPath="~/Views/Shared/TASMaster.Master" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <title>GARMCO Calendar</title>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="mainContent" runat="server">
    <telerik:RadFormDecorator ID="formDecor" runat="server" DecoratedControls="Buttons" Skin="Office2010Silver" />

    <table border="0" style="width: 100%; text-align: left; margin-top: 5px; margin-left: 5px;">
        <tr>
            <td colspan="2" style="padding-left: 10px;">
                <table border="0" style="width: 100%; text-align: left;">
                    <tr>
                        <td style="width: 50px; text-align: right; padding-right: 5px;" rowspan="2">
                            <img alt="" src="../../Images/calendar_icon.jpg" />
                        </td>
                        <td class="PageTitleLabel" style="vertical-align: bottom; padding-left: 5px; width: 900px; font-size: 11pt;">
                            GARMCO Calendar
                        </td>
                        <td style="width: 50px;">
                            <asp:LinkButton ID="lnkMoveUp" runat="server" />
                        </td>
                        <td />
                    </tr>
                    <tr>
                        <td class="PageDescriptionHeader" style="text-align: left; vertical-align: top; padding-left: 5px; margin: 0px;">
                            Allows an authorized person to view the public holidays and special days declared in the company
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
                        Calendar Year                 
                    </td>
                    <td style="width: 330px; padding-left: 2px;">
                        <telerik:RadNumericTextBox ID="txtYear" runat="server" width="70px" 
                            MinValue="0" ToolTip="Enter calendar year" 
                            Skin="Vista" DataType="System.Int32" MaxLength="4" MaxValue="9999" 
                            EmptyMessage="" >
                            <NumberFormat ZeroPattern="n" DecimalDigits="0" GroupSeparator="" />
                        </telerik:RadNumericTextBox>
                    </td>
                    <td />
                </tr>
                <tr style="height: 23px;">
                    <td class="LabelBold">
                        <asp:CustomValidator ID="cusValDateRange" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                        <asp:CheckBox ID="chkDateRange" runat="server" Text="Date Duration" TextAlign="Left" SkinID="CheckBold" 
                            style="padding-right: 0px;" AutoPostBack="True" OnCheckedChanged="chkDateRange_CheckedChanged" />      
                    </td>
                    <td style="padding-left: 0px;">
                        <table border="0" style="width: 100%; text-align: left; margin: 0px; padding: 0px; table-layout: fixed;">
                            <tr>
                                <td style="width: 120px; padding-left: 0px;">
                                    <telerik:RadDatePicker ID="dtpStartDate" runat="server"
                                        Width="100%" Skin="Windows7" Enabled="False">
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
                                <td class="LabelBold" style="width: 20px; text-align: center; padding: 0px;">
                                    ~
                                </td>
                                <td style="width: auto; text-align: left;">
                                    <telerik:RadDatePicker ID="dtpEndDate" runat="server"
                                        Width="120px" Skin="Windows7" Enabled="False">
                                        <Calendar ID="Calendar4" runat="server" Skin="Windows7" UseColumnHeadersAsSelectors="False" 
                                            UseRowHeadersAsSelectors="False" ViewSelectorText="x">
                                        </Calendar>
                                        <DateInput ID="DateInput4" runat="server" DateFormat="d/M/yyyy" DisplayDateFormat="d/M/yyyy">
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
                <tr>
                    <td class="LabelBold">
                        <asp:CustomValidator ID="cusValButton" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                    </td>
                    <td style="padding-top: 2px;">
                        <telerik:RadButton ID="btnSearch" runat="server" Text="Search" ToolTip="Search matching database records" Width="70px" 
                            CssClass="RadButtonStyle" ValidationGroup="valPrimary" Font-Size="9pt" OnClick="btnSearch_Click" Skin="Office2010Silver" />
                        <telerik:RadButton ID="btnReset" runat="server" Text="Reset" ToolTip="Reset filter criterias" Width="70px" 
                            CssClass="RadButtonStyle" ValidationGroup="valPrimary" Font-Size="9pt" OnClick="btnReset_Click" Skin="Office2010Silver" />                                                
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
                        <telerik:RadGrid ID="gridHoliday" runat="server"
                            AllowSorting="true" AllowMultiRowSelection="true"
                            PageSize="10" AutoGenerateColumns="false" Skin="Silk" 
                            GridLines="None" Width="100%" Height="" CellSpacing="0"
                            onpageindexchanged="gridHoliday_PageIndexChanged" 
                            onpagesizechanged="gridHoliday_PageSizeChanged" 
                            onsortcommand="gridHoliday_SortCommand" 
                            onitemcommand="gridHoliday_ItemCommand" 
                            onitemdatabound="gridHoliday_ItemDataBound" 
                            Font-Names="Tahoma" Font-Size="9pt" Font-Bold="False" 
                            AllowPaging = "true" BorderStyle="Outset" BorderWidth="1px">
                            <ExportSettings ExportOnlyData="true" IgnorePaging="true" OpenInNewWindow="true" FileName="HolidayList" HideStructureColumns="true">
                                <Pdf PageHeight="210mm" PageWidth="310mm" PageTitle="Holidays List" DefaultFontFamily="Arial Unicode MS"
                                PageBottomMargin="20mm" PageTopMargin="20mm" PageLeftMargin="20mm" PageRightMargin="20mm" PaperSize="A4" AllowPrinting="true" />
                            </ExportSettings>
                            <AlternatingItemStyle Font-Names="Tahoma" Font-Size="9pt" Wrap="True" />
                            <MasterTableView DataKeyNames="HolidayDate" ClientDataKeyNames="HolidayDate" 
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
                                    <telerik:GridBoundColumn DataField="HolidayDate" HeaderText="Holiday Date"
                                        DataFormatString="{0:dd-MMM-yyyy}" DataType="System.DateTime" 
                                        ReadOnly="True" SortExpression="HolidayDate" UniqueName="HolidayDate">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="100px" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma"></HeaderStyle>
                                        <ItemStyle Font-Bold="true" ForeColor="Purple" />
                                    </telerik:GridBoundColumn>   
                                    <telerik:GridBoundColumn DataField="DOW" DataType="System.String" HeaderText="Day of Week" 
                                        ReadOnly="True" SortExpression="DOW" UniqueName="DOW">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Font-Bold="True" Width="90px" />
                                    </telerik:GridBoundColumn> 
                                    <telerik:GridBoundColumn DataField="HolidayName" DataType="System.String" HeaderText="Holiday Name" 
                                        ReadOnly="True" SortExpression="HolidayName" UniqueName="HolidayName">
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
                <telerik:AjaxSetting AjaxControlID="btnReset">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting> 
                <telerik:AjaxSetting AjaxControlID="gridHoliday">
				    <UpdatedControls>
                        <telerik:AjaxUpdatedControl ControlID="gridHoliday" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>                                       
            </AjaxSettings>
        </telerik:RadAjaxManager>
        <telerik:RadAjaxLoadingPanel ID="loadingPanel" runat="server" Skin="Office2010Silver"></telerik:RadAjaxLoadingPanel>
    </asp:Panel>
</asp:Content>
