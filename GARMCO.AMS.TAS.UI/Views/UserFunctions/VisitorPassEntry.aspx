<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/TASMaster.Master" AutoEventWireup="true" CodeBehind="VisitorPassEntry.aspx.cs" 
    Inherits="GARMCO.AMS.TAS.UI.Views.UserFunctions.VisitorPassEntry" StylesheetTheme="Standard" %>

<%@ MasterType VirtualPath="~/Views/Shared/TASMaster.Master" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <title>Visitor's Pass Entry</title>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="mainContent" runat="server">
    <telerik:RadFormDecorator ID="formDecor" runat="server" DecoratedControls="Buttons" Skin="Office2010Silver" />

    <table border="0" style="width: 100%; text-align: left; margin-top: 5px; margin-left: 5px;">
        <tr>
            <td colspan="2" style="padding-left: 10px;">
                <table border="0" style="width: 100%; text-align: left;">
                    <tr>
                        <td style="width: 50px; text-align: right; padding-right: 5px;" rowspan="2">
                            <img alt="" src="../../Images/visitor_pass_icon.jpg" />
                        </td>
                        <td class="PageTitleLabel" style="vertical-align: bottom; padding-left: 5px; width: 900px; font-size: 11pt;">
                            Visitor's Pass Entry
                        </td>
                        <td style="width: 50px;">
                            <asp:LinkButton ID="lnkMoveUp" runat="server" />
                        </td>
                        <td />
                    </tr>
                    <tr>
                        <td class="PageDescriptionHeader" style="text-align: left; vertical-align: top; padding-left: 5px; margin: 0px;">
                            Allows Security Personnel to record the entry of visitors inside the company
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
        <asp:Panel ID="panVisitorInfo" runat="server" BorderStyle="None" CssClass="GroupPanelHeader" GroupingText="Visitor Information:" 
            style="padding: 0px; margin-top: 10px;">
            <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                <tr style="height: 23px;">
                    <td class="LabelBold" style="width: 110px;">
                        <asp:CustomValidator ID="cusValVisitorName" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                        Visitor Name
                    </td>
                    <td style="width: 300px;">
                        <telerik:RadTextBox ID="txtVisitorName" runat="server" Width="100%" 
                            EmptyMessage="Enter Visitor Name" Skin="Office2010Silver" ToolTip="Maximum text input is 100 chars." 
                            Font-Names="Tahoma" Font-Size="9pt" MaxLength="100" BackColor="Yellow" />
                    </td>
                    <td class="LabelBold" style="width: 160px;">
                        <asp:CustomValidator ID="cusValVisitDate" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                        Visit Date
                    </td>
                    <td style="width: 200px;">
                        <telerik:RadDatePicker ID="dtpVisitDate" runat="server" Width="120px" Skin="Windows7" Culture="en-US" AutoPostBack="True" OnSelectedDateChanged="dtpVisitDate_SelectedDateChanged">
                            <Calendar ID="Calendar3" runat="server" Skin="Windows7" UseColumnHeadersAsSelectors="False" 
                                UseRowHeadersAsSelectors="False" ViewSelectorText="x">
                            </Calendar>
                            <DateInput ID="DateInput3" runat="server" DateFormat="d/M/yyyy" DisplayDateFormat="d/M/yyyy" BackColor="Yellow" AutoPostBack="True">
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
                        <asp:CustomValidator ID="cusValIDNumber" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                        ID Number                        
                    </td>
                    <td>
                        <telerik:RadTextBox ID="txtIDNumber" runat="server" Width="100%" 
                            EmptyMessage="Enter CPR, mobile no., or other unique ID" Skin="Office2010Silver" ToolTip="Maximum text input is 50 chars." 
                            Font-Names="Tahoma" Font-Size="9pt" MaxLength="50" BackColor="Yellow" />
                    </td>
                    <td class="LabelBold">
                        <asp:CustomValidator ID="cusValVisitorCardNo" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                        GARMCO Visitor Card No.
                    </td>
                    <td>
                        <telerik:RadNumericTextBox ID="txtVisitorCardNo" runat="server" width="150px" 
                            MinValue="0" ToolTip="(Note: Value must be numeric and should not exceed 8 digits)" 
                            Skin="Office2010Silver" DataType="System.Int32" MaxLength="8" MaxValue="99999999" 
                            EmptyMessage="" BackColor="Yellow" >
                            <NumberFormat ZeroPattern="n" DecimalDigits="0" GroupSeparator="" />
                        </telerik:RadNumericTextBox>                         
                    </td>
                    <td />
                </tr>
                <tr id="trCheckOffense" runat="server" style="height: 23px; display: none;">
                    <td class="LabelBold">
                        
                    </td>
                    <td colspan="3">
                         <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                            <tr>
                                <td style="width: 110px;">
                                    <telerik:RadButton ID="btnCheckVisit" runat="server" ToolTip="Check if visitor has offense"
                                        Text="Check Offense" Skin="Office2010Silver" Font-Bold="False" Font-Size="9pt" 
                                        CssClass="RadButtonStyle" ValidationGroup="valPrimary" Width="110px"
                                        OnClick="btnCheckVisit_Click">
                                    </telerik:RadButton> 
                                </td>
                                <td colspan="3" style="font-weight: bold; color: red; padding-left: 20px; display: none; ">
                                    No offense found!
                                </td>
                            </tr>
                        </table>
                        
                    </td>                    
                    <td />
                </tr>
            </table>
        </asp:Panel>

        <asp:Panel ID="panPersonToVisit" runat="server" BorderStyle="None" CssClass="GroupPanelHeader" GroupingText="Person to Visit Information:" 
            style="padding: 0px; margin-top: 10px;">
            <table border="0" style="width: 100%; text-align: left; margin-top: 0px; table-layout: fixed;">
                <tr style="height: 23px;">
                    <td class="LabelBold" style="width: 110px;">
                        <asp:CustomValidator ID="cusValEmpNo" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                        Employee No.
                    </td>
                    <td style="width: 300px; padding-right: 5px; padding-left: 0px; margin-left: 0px;">
                        <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                            <tr style="vertical-align: top; margin: 0px; padding: 0px;">
                                <td style="width: 130px; text-align: left;">
                                    <telerik:RadNumericTextBox ID="txtEmpNo" runat="server" width="130px" 
                                        MinValue="0" ToolTip="(Note: Employee No. must start with 1000. Example: 10003632)" 
                                        Skin="Office2010Silver" DataType="System.Int32" MaxLength="8" MaxValue="99999999" 
                                        EmptyMessage="1000xxxx" BackColor="Yellow" OnTextChanged="txtEmpNo_TextChanged" AutoPostBack="True" >
                                        <NumberFormat ZeroPattern="n" DecimalDigits="0" GroupSeparator="" />
                                    </telerik:RadNumericTextBox> 
                                </td>
                                <td style="width: 40px; text-align: left; padding-left: 3px; padding-top: 2px; vertical-align: top; display: none;">
                                    <telerik:RadButton ID="btnGet" runat="server" Skin="Office2010Silver" Width="100%" 
                                        Text="Get" ToolTip="Get employee info based on entered Employee No." Enabled="true" 
                                        Font-Bold="False" Font-Size="9pt" CausesValidation="false"
                                        onclick="btnGet_Click">
                                    </telerik:RadButton>
                                </td> 
                                <td style="text-align: left; width: 30px; padding-left: 3px; padding-top: 2px; vertical-align: top;">
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
                    <td class="LabelBold" style="width: 160px;">
                        Cost Center
                    </td>
                    <td class="TextNormal" style="width: 340px;">
                        <asp:Literal ID="litCostCenter" runat="server" Text="Not defined" /> 
                    </td>
                    <td />
                </tr>
                <tr style="height: 23px;">
                    <td class="LabelBold">
                        Employee Name
                    </td>
                    <td class="TextNormal"  style="padding-left: 0px;">
                        <asp:Literal ID="litEmpName" runat="server" Text="Not defined" />         
                    </td>
                    <td class="LabelBold">
                        Immediate Supervisor
                    </td>
                    <td class="TextNormal">
                        <asp:Literal ID="litSupervisor" runat="server" Text="Not defined" />
                    </td>
                    <td>
                        
                    </td>
                </tr>
                <tr style="height: 23px;">
                    <td class="LabelBold">
                        Position
                    </td>
                    <td class="TextNormal"  style="padding-left: 0px;">
                        <asp:Literal ID="litPosition" runat="server" Text="Not defined" />                                  
                    </td>
                    <td class="LabelBold">
                        Cost Center Manager
                    </td>
                    <td class="TextNormal">
                        <asp:Literal ID="litCCManager" runat="server" Text="Not defined" />
                    </td>
                    <td />
                </tr>
                <tr style="height: 23px;">
                    <td class="LabelBold">
                        Phone Ext. No.
                    </td>
                    <td class="TextNormal"  style="padding-left: 0px;">
                        <asp:Literal ID="litExtNo" runat="server" Text="Not defined" />                                  
                    </td>
                    <td class="LabelBold" style="display: none;">
                        
                    </td>
                    <td class="TextNormal" style="display: none;">
                        
                    </td>
                    <td />
                </tr>                   
            </table>
        </asp:Panel>

        <asp:Panel ID="panSwipeInfo" runat="server" BorderStyle="None" CssClass="GroupPanelHeader" GroupingText="Log Information:" 
            style="padding: 0px; margin-top: 10px;">
            <table border="0" style="width: 100%; text-align: left; margin: 0px; padding: 0px; table-layout: fixed;">
                <tr id="trSwipeHistoryTimeEntry" runat="server" style="height: 23px;">
                    <td style="width: 115px;">
                        <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                            <tr style="margin: 0px; padding: 0px;">                                
                                <td class="LabelBold" style="width: auto; padding-right: 5px;">
                                    <asp:CustomValidator ID="cusValSwipeDate" runat="server" ControlToValidate="txtGeneric" 
                                        CssClass="LabelValidationError" Display="Dynamic" 
                                        ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                                        ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                                    Swipe Time
                                </td>
                                <td style="width: 20px; text-align: left; display: none;">
                                     <asp:CheckBox ID="chkSwipeIn" runat="server" Text="" AutoPostBack="True" 
                                        OnCheckedChanged="chkSwipeIn_CheckedChanged" />
                                </td>
                            </tr>
                        </table>
                    </td>
                    <td style="width: 250px; padding-left: 0px; margin-left: 0px;">
                        <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                            <tr style="height: 23px;">
                                <td style="width: 130px; text-align: left;">
                                    <telerik:RadDatePicker ID="dtpSwipeDate" runat="server" ToolTip="Date Format: dd/mm/yyyy"
                                        Width="100%" Skin="Windows7" Culture="en-US">
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
                                <td style="width: 80px; text-align: left;">
                                    <telerik:RadDateInput ID="dtpSwipeTime" runat="server" LabelWidth="50px" Width="100%" Culture="en-US" EmptyMessage="HH:mm:ss" ToolTip="Time Format: HH:mm:ss"
                                        InvalidStyleDuration="100" DateFormat="HH:mm:ss" Skin="Windows7">
                                    </telerik:RadDateInput>   
                                </td> 
                                <td style="width: auto; text-align: left;">
                                    <asp:CustomValidator ID="cusValSwipeTime" runat="server" ControlToValidate="txtGeneric" 
                                        CssClass="LabelValidationError" Display="Dynamic" 
                                        ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                                        ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                                </td>
                            </tr>
                        </table>  
                    </td>
                    <td class="LabelBold" style="width: 85px;">
                        <asp:CustomValidator ID="cusValSwipeType" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                        Swipe Type
                    </td>
                    <td style="width: 60px;">
                        <telerik:RadComboBox ID="cboSwipeType" runat="server"
                            DropDownWidth="55px" 
                            HighlightTemplatedItems="True" 
                            Skin="Office2010Silver" 
                            Width="100%"
                            EnableVirtualScrolling="True" >                                                                  
                            <Items>
                                <telerik:RadComboBoxItem runat="server" Selected="True" Text=" " Value=" " />
                                <telerik:RadComboBoxItem runat="server" Text="In" Value="valIN" />
                                <telerik:RadComboBoxItem runat="server" Text="Out" Value="valOUT" />
                            </Items>
                        </telerik:RadComboBox>
                    </td>
                    <td style="width: 120px; text-align: left;">
                        <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed; display: none;">
                            <tr style="margin: 0px; padding: 0px;">                                
                                <td class="LabelBold" style="width: auto; padding-right: 0px;">
                                    <asp:CustomValidator ID="cusValTimeOut" runat="server" ControlToValidate="txtGeneric" 
                                        CssClass="LabelValidationError" Display="Dynamic" 
                                        ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                                        ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                                    Time Out
                                </td>
                                <td style="width: 20px; text-align: left; ">
                                     <asp:CheckBox ID="chkSwipeOut" runat="server" Text="" AutoPostBack="True" 
                                        OnCheckedChanged="chkSwipeOut_CheckedChanged" />
                                </td>
                            </tr>
                        </table>
                        <%--(Note: Time In and Time Out can be entered manually or fetch automatically from the Swipe Access System) --%>
                    </td>
                    <td style="text-align: left;">
                        <table border="0" style="width: 300px; text-align: left; margin: 0px; table-layout: fixed; display: none">
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
                                    <telerik:RadDateInput ID="dtpTimeOut" runat="server" LabelWidth="50px" Width="80px" Culture="en-US" EmptyMessage="HH:mm:ss" ToolTip="Time Format: HH:mm:ss"
                                        InvalidStyleDuration="100" DateFormat="HH:mm:ss" Enabled="False">
                                    </telerik:RadDateInput>  
                                </td> 
                            </tr>
                        </table>  
                    </td>
                </tr>
                <tr id="trSwipeHistoryButton" runat="server" style="height: 23px;">
                    <td class="LabelBold">
                        <asp:CustomValidator ID="CustomValidator1" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                    </td>
                    <td colspan="3" style="padding-left: 3px;">                        
                        <telerik:RadButton ID="btnAddSwipe" runat="server" ToolTip="Add swipe record" Width="80px"
                            Text="Add Swipe" Skin="Office2010Silver" Font-Bold="False" Font-Size="9pt" 
                            CssClass="RadButtonStyle" ValidationGroup="valPrimary"
                            OnClick="btnAddSwipe_Click">
                        </telerik:RadButton>
                        <telerik:RadButton ID="btnUpdateSwipe" runat="server" ToolTip="Update swipe record"
                            Text="Update" Skin="Office2010Silver" Font-Bold="False" Font-Size="9pt" 
                            CssClass="RadButtonStyle" ValidationGroup="valPrimary" Width="70px"
                            OnClick="btnUpdateSwipe_Click">
                        </telerik:RadButton>                                                                       
                        <telerik:RadButton ID="btnResetSwipe" runat="server" ToolTip="Clear data entry fields"
                            Text="Clear" Skin="Office2010Silver" Font-Bold="False" Font-Size="9pt"
                            CssClass="RadButtonStyle" CausesValidation="false" Width="70px"
                            OnClick="btnResetSwipe_Click">
                        </telerik:RadButton> 
                        <telerik:RadButton ID="btnDeleteSwipe" runat="server" ToolTip="Delete swipe record"
                            Text="Remove Swipe" Skin="Office2010Silver" Font-Bold="False" Font-Size="9pt" Visible="false" 
                            CssClass="RadButtonStyle" ValidationGroup="valPrimary" Width="100px"
                            OnClick="btnDeleteSwipe_Click" Enabled="False">
                        </telerik:RadButton>   
                    </td>
                    <td>
                        <telerik:RadNumericTextBox ID="txtSwipeID" runat="server" width="100px" Visible="false" 
                            MinValue="0" Skin="Office2010Silver" DataType="System.Int32">
                            <NumberFormat ZeroPattern="n" DecimalDigits="0" GroupSeparator="" />
                        </telerik:RadNumericTextBox> 
                    </td>
                    <td />
                </tr>   
            </table>

            <table id="tblGrid" runat="server" border="0" style="width: 100%; text-align: left; margin-top: 0px; table-layout: fixed;">
                <tr>
                    <td>
                        <telerik:RadGrid ID="gridSwipeHistory" runat="server"
                            AllowSorting="true" AllowMultiRowSelection="true"
                            PageSize="5" AutoGenerateColumns="false" Skin="Silk" 
                            GridLines="None" Width="100%" Height="" CellSpacing="0"
                            onpageindexchanged="gridSwipeHistory_PageIndexChanged" 
                            onpagesizechanged="gridSwipeHistory_PageSizeChanged" 
                            onsortcommand="gridSwipeHistory_SortCommand" 
                            onitemcommand="gridSwipeHistory_ItemCommand" 
                            onitemdatabound="gridSwipeHistory_ItemDataBound" 
                            Font-Names="Tahoma" Font-Size="9pt" Font-Bold="False" 
                            AllowPaging = "true" BorderStyle="Outset" BorderWidth="1px" AllowCustomPaging="True" VirtualItemCount="1">
                            <ExportSettings ExportOnlyData="true" IgnorePaging="true" OpenInNewWindow="true" FileName="AttendanceList" HideStructureColumns="true">
                                <Pdf PageHeight="210mm" PageWidth="310mm" PageTitle="Attendance List" DefaultFontFamily="Arial Unicode MS"
                                PageBottomMargin="20mm" PageTopMargin="20mm" PageLeftMargin="20mm" PageRightMargin="20mm" PaperSize="A4" AllowPrinting="true" />
                            </ExportSettings>
                            <AlternatingItemStyle Font-Names="Tahoma" Font-Size="9pt" Wrap="True" />
                            <MasterTableView DataKeyNames="SwipeTime" ClientDataKeyNames="SwipeTime" 
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
                                    <telerik:GridBoundColumn DataField="LogID" DataType="System.Int64" HeaderText="Log ID" 
                                        ReadOnly="True" SortExpression="LogID" UniqueName="LogID" Display="false">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="80px" Font-Bold="True"></HeaderStyle>
                                    </telerik:GridBoundColumn>   
                                    <telerik:GridBoundColumn DataField="SwipeID" DataType="System.Int64" HeaderText="Swipe ID" 
                                        ReadOnly="True" SortExpression="SwipeID" UniqueName="SwipeID" Display="false">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="80px" Font-Bold="True"></HeaderStyle>
                                    </telerik:GridBoundColumn>   
                                    <telerik:GridBoundColumn DataField="LocationCode" DataType="System.Int32" HeaderText="LocationCode" 
                                        ReadOnly="True" SortExpression="LocationCode" UniqueName="LocationCode" Display="false">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="80px" Font-Bold="True"></HeaderStyle>
                                    </telerik:GridBoundColumn>   
                                    <telerik:GridBoundColumn DataField="ReaderNo" DataType="System.Int32" HeaderText="ReaderNo" 
                                        ReadOnly="True" SortExpression="ReaderNo" UniqueName="ReaderNo" Display="false">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="80px" Font-Bold="True"></HeaderStyle>
                                    </telerik:GridBoundColumn>  
                                    <telerik:GridBoundColumn DataField="CreatedByEmpNo" DataType="System.Int32" HeaderText="CreatedByEmpNo" 
                                        ReadOnly="True" SortExpression="CreatedByEmpNo" UniqueName="CreatedByEmpNo" Display="false">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="80px" Font-Bold="True"></HeaderStyle>
                                    </telerik:GridBoundColumn> 
                                    <%--<telerik:GridClientSelectColumn HeaderText="Select" HeaderStyle-Width="50px" 
                                        HeaderStyle-Font-Bold="true" HeaderStyle-Font-Size = "9pt" 
                                        UniqueName="CheckboxSelectColumn" >                                                                                            
                                        <HeaderStyle Font-Bold="True" Font-Size="9pt" Width="35px" HorizontalAlign="Center" />
                                        <ItemStyle HorizontalAlign="Center" />
                                    </telerik:GridClientSelectColumn> --%> 

                                    <telerik:GridButtonColumn ButtonType="ImageButton" CommandName="Select" HeaderText="" CommandArgument="DeleteButton" 
                                        UniqueName="DeleteButton" ImageUrl="~/Images/delete_enabled_icon.png" HeaderTooltip="Delete selected record">
                                        <HeaderStyle Font-Bold="True" Width="35px"></HeaderStyle>
                                        <ItemStyle Font-Bold="true" ForeColor="Blue" HorizontalAlign="Center" Font-Size="9pt" />
				                    </telerik:GridButtonColumn> 
                                    <telerik:GridButtonColumn ButtonType="LinkButton" CommandName="Select" Text="Edit" UniqueName="EditLinkButton" HeaderTooltip="Edit selected record">
                                        <HeaderStyle Width="40px" HorizontalAlign="Center" Font-Bold="true" Font-Size="9pt" />
                                        <ItemStyle HorizontalAlign="Center" Font-Bold="true" Font-Size="9pt" ForeColor="Blue" />
					                </telerik:GridButtonColumn>   
                                    <telerik:GridBoundColumn DataField="SwipeDate" HeaderText="Date"
                                        DataFormatString="{0:dd-MMM-yyyy}" DataType="System.DateTime" 
                                        ReadOnly="True" SortExpression="SwipeDate" UniqueName="SwipeDate">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="90px" Font-Bold="True"></HeaderStyle>
                                    </telerik:GridBoundColumn>    
                                    <telerik:GridTemplateColumn DataField="SwipeLocation" FilterControlAltText="Filter Location column" HeaderText="Location" 
                                        SortExpression="SwipeLocation" UniqueName="SwipeLocation">
								        <HeaderStyle Width="210px" HorizontalAlign="Left" />
								        <ItemTemplate>
									        <div class="columnEllipsis" style="width: 200px; text-align: left;">
										        <asp:Literal ID="litSwipeLocation" runat="server" Text='<%# Eval("SwipeLocation") %>' />
									        </div>
								        </ItemTemplate>
							        </telerik:GridTemplateColumn>                                     
                                    <telerik:GridBoundColumn DataField="SwipeTypeDesc" DataType="System.String" HeaderText="Swipe Type"
                                        ReadOnly="True" SortExpression="SwipeTypeDesc" UniqueName="SwipeTypeDesc">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="90px" Font-Bold="True"></HeaderStyle>
                                    </telerik:GridBoundColumn>                                 
                                    <telerik:GridBoundColumn DataField="SwipeTime" HeaderText="Swipe Time"
                                        DataFormatString="{0:dd/MM/yyyy HH:mm:ss}" DataType="System.DateTime" 
                                        ReadOnly="True" SortExpression="SwipeTime" UniqueName="SwipeTime">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Font-Bold="True"></HeaderStyle>
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

            <table border="0" style="width: 100%; text-align: left; margin-top: 5px; padding: 0px; table-layout: fixed;">
                <tr style="height: 23px; vertical-align: top;">
                    <td class="LabelBold" style="padding-top: 5px; width: 110px;">
                        <asp:CustomValidator ID="cusValRemarks" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                        Remarks
                    </td>
                    <td class="TextNormal" style="width: 900px;">
                        <asp:TextBox ID="txtRemarks" runat="server" width="890px" 
                            SkinID="TextLeft" MaxLength="3000" TextMode="MultiLine" Rows="7" 
                            ToolTip="(Note: Maximum text input is 3000 chars.)" />  
                    </td>
                </tr>  
                <tr style="height: 23px;">
                    <td class="LabelBold" style="color: red; font-size: 8pt;">
                        BLOCK VISITOR?
                    </td>
                    <td style="padding-left: 0px; margin-left: 0px;">
                        <asp:CheckBox ID="chkBlockVisitor" runat="server" Text="" TextAlign="Right" />
                    </td>
                    <td />
                </tr>  
            </table>
        </asp:Panel>

        <asp:Panel ID="panButtons" runat="server" style="margin-top: 5px; margin-right: 15px; margin-left: 10px; padding-bottom: 10px;">        
            <table border="0" style="width: 100%; table-layout: fixed;">            
                <tr style="height: 20px;">
                    <td style="text-align: left; width: 10px;">
                        <asp:CustomValidator ID="cusValButton" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                    </td>
                    <td class="LabelBold" style="width: 800px; text-align: left; padding-left: 0px;">
                        <telerik:RadButton ID="btnSave" runat="server" ToolTip="Save record" Width="80px"
                            Text="Save" Skin="Office2010Silver" Font-Bold="False" Font-Size="9pt" 
                            CssClass="RadButtonStyle" ValidationGroup="valPrimary"
                            OnClick="btnSave_Click" Enabled="False">
                        </telerik:RadButton>
                        <telerik:RadButton ID="btnDelete" runat="server" ToolTip="Delete current record"
                            Text="Delete" Skin="Office2010Silver" Font-Bold="False" Font-Size="9pt" 
                            CssClass="RadButtonStyle" ValidationGroup="valPrimary" Width="80px"
                            OnClick="btnDelete_Click" Enabled="False">
                        </telerik:RadButton>                                                 
                        <telerik:RadButton ID="btnReset" runat="server" ToolTip="Reset form"
                            Text="Reset" Skin="Office2010Silver" Font-Bold="False" Font-Size="9pt"
                            CssClass="RadButtonStyle" CausesValidation="false" Width="80px"
                            OnClick="btnReset_Click">
                        </telerik:RadButton>                         
                        <telerik:RadButton ID="btnViewReport" runat="server" Text="Print Report" ToolTip="View and print report" 
                            Skin="Office2010Silver" Font-Bold="False" Font-Size="9pt" ValidationGroup="valPrimary" Width="100px"
                            CssClass="RadButtonStyle" onclick="btnViewReport_Click" Enabled="False">                       
                        </telerik:RadButton>                                                                         
                        <telerik:RadButton ID="btnBack" runat="server" ToolTip="Go back to previous page"
                            Text="<< Back" Skin="Office2010Silver" Font-Bold="False" Font-Size="9pt" Width="80px"
                            CssClass="RadButtonStyle" CausesValidation="false" OnClick="btnBack_Click">
                        </telerik:RadButton>  
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
        <telerik:RadButton ID="btnDeleteDummy" runat="server" Text="" Skin="Office2010Silver" 
            CssClass="HideButton" ValidationGroup="valPrimary" onclick="btnDeleteDummy_Click" />   
        <telerik:RadButton ID="btnRebind" runat="server" Text="" Skin="Office2010Silver" CausesValidation="false" onclick="btnRebind_Click" />    
        <telerik:RadButton ID="btnSave2" runat="server" Text="" Skin="Office2010Silver" CausesValidation="false" onclick="btnSave2_Click" />    
        <telerik:RadButton ID="btnDeleteSwipeDummy" runat="server" Text="" Skin="Office2010Silver" 
            CssClass="HideButton" ValidationGroup="valPrimary" onclick="btnDeleteSwipeDummy_Click" />       
        <telerik:RadButton ID="btnRemoveGridItem" runat="server" Text="" Skin="Office2010Silver" 
            CausesValidation="false" onclick="btnRemoveGridItem_Click" />  
    </asp:Panel>

    <asp:Panel ID="panAjaxManager" runat="server">
        <telerik:RadAjaxManager ID="MyAjaxManager" runat="server">
            <AjaxSettings>                
                 <telerik:AjaxSetting AjaxControlID="btnCheckVisit">
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
                <telerik:AjaxSetting AjaxControlID="btnSave">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>  
                <telerik:AjaxSetting AjaxControlID="btnDelete">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panMain" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>   
                <telerik:AjaxSetting AjaxControlID="btnDeleteDummy">
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
                <telerik:AjaxSetting AjaxControlID="btnViewReport">
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
                <telerik:AjaxSetting AjaxControlID="btnAddSwipe">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panSwipeInfo" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>  
                 <telerik:AjaxSetting AjaxControlID="btnUpdateSwipe">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panSwipeInfo" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting> 
                <telerik:AjaxSetting AjaxControlID="btnDeleteSwipe">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panSwipeInfo" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>  
                <telerik:AjaxSetting AjaxControlID="btnResetSwipe">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panSwipeInfo" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>  
                 <telerik:AjaxSetting AjaxControlID="btnRemoveGridItem">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>  
                <telerik:AjaxSetting AjaxControlID="gridSwipeHistory">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>  
                <telerik:AjaxSetting AjaxControlID="chkSwipeIn">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panSwipeInfo" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>  
                <telerik:AjaxSetting AjaxControlID="chkSwipeOut">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panSwipeInfo" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>  
                <telerik:AjaxSetting AjaxControlID="dtpVisitDate">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panMain" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>    
                <telerik:AjaxSetting AjaxControlID="txtEmpNo">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panPersonToVisit" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting> 
            </AjaxSettings>
        </telerik:RadAjaxManager>
        <telerik:RadAjaxLoadingPanel ID="loadingPanel" runat="server" Skin="Office2010Silver"></telerik:RadAjaxLoadingPanel>
    </asp:Panel>
</asp:Content>
