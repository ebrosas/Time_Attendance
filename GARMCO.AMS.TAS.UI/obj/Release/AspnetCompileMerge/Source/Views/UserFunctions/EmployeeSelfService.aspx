<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/TASMaster.Master" AutoEventWireup="true" CodeBehind="EmployeeSelfService.aspx.cs" 
    Inherits="GARMCO.AMS.TAS.UI.Views.UserFunctions.EmployeeSelfService" StylesheetTheme="Standard" %>

<%@ MasterType VirtualPath="~/Views/Shared/TASMaster.Master" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <title>Employee's Self Service</title>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="mainContent" runat="server">
    <telerik:RadFormDecorator ID="formDecor" runat="server" DecoratedControls="Buttons" Skin="Office2010Silver" />

    <table border="0" style="width: 100%; text-align: left; margin-top: 5px; margin-left: 0px;">
        <tr>
            <td colspan="2" style="padding-left: 10px;">
                <table border="0" style="width: 100%; text-align: left;">
                    <tr>
                        <%--<td style="width: 50px; text-align: right; padding-right: 5px;" rowspan="2">
                            <img alt="" src="../../Images/shift_pattern_icon.jpg" />
                        </td>--%>
                        <td id="tdPageTitle" runat="server"  class="PageTitleLabel" style="vertical-align: bottom; padding-left: 5px; width: 900px; font-size: 11pt;">
                            Employee's Self Service
                        </td>
                        <td style="width: 50px;">
                            <asp:LinkButton ID="lnkMoveUp" runat="server" />
                        </td>
                        <td />
                    </tr>
                    <tr>
                        <td class="PageDescriptionHeader" style="text-align: left; vertical-align: top; padding-left: 5px; margin: 0px;">
                            Allows users to view their swipes history, daily attendance, leave balances, day-in-lieu entitlements and other attendance related information.
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
        <asp:Panel ID="panEmployeeInfo" runat="server" BorderStyle="None" style="padding-left: 20px; margin-top: 0px;">
            <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                <tr style="height: 23px;">
                    <td id="tdEmpPhoto" runat="server" style="width: 120px; padding-left: 0px; padding-right: 0px; padding-top: 2px; vertical-align: top;">
                        <asp:Image ID="imgPhoto" runat="server" ImageAlign="Middle" ImageUrl="~/Images/no_picture_icon.png" 
                            Width="100%" Height="140px"  BorderStyle="None" AlternateText="Employee Photo" />
                        <%--<img id="imgEmp" class=" img-thumbnail empPic" alt="Employee image not available for display" src="../../Images/3632.jpg" style="width: 120px; height: 140px;"  />--%>
                    </td>
                    <td style="width: auto;">
                        <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                            <tr style="height: 23px;">
                                <td class="LabelBold" style="width: 105px;">
                                    <asp:CustomValidator ID="cusValEmpNo" runat="server" ControlToValidate="txtGeneric" 
                                        CssClass="LabelValidationError" Display="Dynamic" 
                                        ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                                        ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                                    Employee No.
                                </td>
                                <td class="TextNormal" style="width: 400px; font-weight: bold; color: purple;">
                                    <table border="0" style="width: 100%; text-align: left; padding: 0px; margin: 0px; table-layout: fixed;">
                                        <tr style="margin: 0px; padding: 0px;">
                                            <td style="width: 110px; text-align: left; padding-left: 0px;">
                                                <telerik:RadNumericTextBox ID="txtEmpNo" runat="server" width="100%" 
                                                    MinValue="0" ToolTip="(Note: Employee No. must start with 1000. Example: 10003632)" 
                                                    Skin="Vista" DataType="System.Int32" MaxLength="8" MaxValue="99999999" 
                                                    EmptyMessage="1000xxxx" >
                                                    <NumberFormat ZeroPattern="n" DecimalDigits="0" GroupSeparator="" />
                                                </telerik:RadNumericTextBox> 
                                            </td>
                                            <td style="width: auto; text-align: left; padding-left: 2px;">
                                                <telerik:RadButton ID="btnFindEmp" runat="server" Skin="Office2010Silver" 
                                                    Text="..." ToolTip="Click to open the Employee Search page" Enabled="true" 
                                                    Font-Bold="False" Font-Size="9pt" CausesValidation="false" Width="35px"
                                                    onclick="btnFindEmp_Click">
                                                </telerik:RadButton>
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
                                <td class="TextNormal" style="padding-left: 3px;">
                                    <asp:Literal ID="litEmployeeName" runat="server" Text="Not defined" /> 
                                </td>
                                <td />
                            </tr>
                            <tr style="height: 20px;">
                                <td class="LabelBold">
                                    Position
                                </td>
                                <td class="TextNormal" style="padding-left: 3px;">
                                    <asp:Literal ID="litPosition" runat="server" Text="Not defined" /> 
                                </td>
                                <td />
                            </tr>
                            <tr style="height: 20px;">
                                <td class="LabelBold">
                                    Cost Center
                                </td>
                                <td class="TextNormal" style="padding-left: 3px;">
                                    <asp:Literal ID="litCostCenter" runat="server" Text="Not defined" /> 
                                </td>
                                <td>
                                    <asp:Literal ID="litCostCenterCode" runat="server" Visible="false" /> 
                                </td>
                            </tr>                            
                            <tr style="height: 20px;">
                                <td class="LabelBold">
                                    Joining Date
                                </td>
                                <td class="TextNormal" style="padding-left: 0px;">
                                    <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                                        <tr style="margin: 0px; padding: 0px; vertical-align: top;">                                
                                            <td class="TextNormal" style="width: 90px;">
                                                <asp:Literal ID="litJoiningDate" runat="server" Text="Not defined" /> 
                                            </td>
                                            <td class="LabelBold" style="width: 100px; padding-right: 5px;">
                                               Years of Service
                                            </td>
                                            <td class="TextNormal" style="width: auto; text-align: left;">
                                                <asp:Literal ID="litServiceYear" runat="server" Text="Not defined" /> 
                                            </td>
                                        </tr>
                                    </table> 
                                    
                                </td>
                                <td>
                                    <asp:LinkButton ID="lnkReset" runat="server" Text="Reset Form" Font-Bold="true" ForeColor="Blue" Visible="false" 
                                        style="padding-left: 5px; padding-top: 0px;" OnClick="lnkReset_Click" />
                                </td>
                            </tr>
                            <tr style="height: 20px; display: none;">
                                <td class="LabelBold">
                                    Years of Service
                                </td>
                                <td class="TextNormal" style="padding-left: 3px;">
                                    
                                </td>
                                <td />
                            </tr>
                            <%--<tr style="height: 20px; vertical-align: top;">
                                <td colspan="2">
                                    <table border="0" style="width: 100%; text-align: left; margin: 0px; padding: 0px; table-layout: fixed;">
                                        <tr style="margin: 0px; padding: 0px; vertical-align: top;">                                
                                            <td class="LabelBold" style="width: 100px; color: DarkGoldenrod; padding-top: 3px;">
                                                Show Photo
                                            </td>
                                            <td style="width: auto; text-align: left; padding-left: 0px; margin: 0px; padding-top: 0px;">
                                                
                                                
                                            </td>
                                        </tr>
                                    </table>                                    
                                </td>
                                <td />
                            </tr>--%>
                            <tr style="height: 20px;">
                                <td>
                                    <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                                        <tr style="margin: 0px; padding: 0px;">                                
                                            <td style="width: 30px; text-align: right; padding-right: 0px;">
                                                <asp:CheckBox ID="chkShowPhoto" runat="server" Text="" AutoPostBack="True" 
                                                    OnCheckedChanged="chkShowPhoto_CheckedChanged" Checked="True" />
                                            </td>
                                            <td id="tdShowPhoto" runat="server" class="LabelBold" style="width: auto; padding-right: 10px; padding-left: 0px; margin-left: 0px; color: darkgoldenrod; text-align: left;">
                                                Show Photo
                                            </td>                                            
                                        </tr>
                                    </table>                                    
                                </td>
                                <td style="padding-left: 0px;">
                                    <telerik:RadButton ID="btnGet" runat="server" Skin="Office2010Silver" CssClass="RadButtonStyle" 
                                        Text="Search" ToolTip="Get the employee attendance information" Enabled="true" 
                                        Font-Bold="False" Font-Size="9pt" CausesValidation="false" Width="70px"
                                        onclick="btnGet_Click">
                                    </telerik:RadButton>
                                    <telerik:RadButton ID="btnReset" runat="server" Text="Reset" ToolTip="Reset filter criterias" Width="70px" 
                                        CssClass="RadButtonStyle" ValidationGroup="valPrimary" Font-Size="9pt" OnClick="btnReset_Click" Skin="Office2010Silver" />                                                
                                </td>
                                <td />
                            </tr>      
                        </table>
                    </td>
                </tr>
            </table>
        </asp:Panel>

         <asp:Panel ID="panQuickLinks" runat="server" BorderStyle="None" CssClass="GroupPanelHeader" GroupingText="" 
            style="padding-left: 10px; margin-top: 0px; padding-bottom: 10px; padding-top: 0px;">
            <table border="0" style="width: 100%; text-align: left; margin: 0px; padding-top: 0px; table-layout: fixed;">
                <tr style="height: 23px; vertical-align: top;">
                    <td style="padding-left: 0px; padding-top: 0px; width: 250px;">
                        <table border="0" style="width: 100%; text-align: left; margin: 0px; padding-top: 0px; table-layout: fixed;">                            
                            <tr>
                                <td>
                                    <telerik:RadPanelBar RenderMode="Lightweight" runat="server" ID="panBarMain" Height="430px" Width="100%" ExpandMode="FullExpandedItem" Skin="Silk" OnItemClick="panBarMain_ItemClick">
                                        <Items>
                                            <telerik:RadPanelItem Text="Personal Information" Expanded="True" Font-Bold="true" Font-Size="10pt">
                                                <Items>
                                                    <telerik:RadPanelItem ImageUrl="../../Images/menu_bullet_icon.png" Text="Swipes History" Value="SwipeHistory" />                                                    
                                                    <telerik:RadPanelItem ImageUrl="../../Images/menu_bullet_icon.png" Text="Absences History" Value="AbsenceHistory" />
                                                    <telerik:RadPanelItem ImageUrl="../../Images/menu_bullet_icon.png" Text="Leave History" Value="LeaveHistory" />
                                                    <telerik:RadPanelItem ImageUrl="../../Images/menu_bullet_icon.png" Text="Leave Balance" Value="LeaveBalance" Visible="false" />
                                                    <telerik:RadPanelItem ImageUrl="../../Images/menu_bullet_icon.png" Text="Attendance History" Value="AttendanceHistory" />
                                                    <telerik:RadPanelItem ImageUrl="../../Images/menu_bullet_icon.png" Text="Training History" Value="TrainingHistory" />
                                                    <telerik:RadPanelItem ImageUrl="../../Images/menu_bullet_icon.png" Text="DIL Entitlements" Value="DILEntitlement" />
                                                    <telerik:RadPanelItem ImageUrl="../../Images/menu_bullet_icon.png" Text="Shift Pattern Information" Value="ShiftPatternInfo" />
                                                    <telerik:RadPanelItem ImageUrl="../../Images/menu_bullet_icon.png" Text="Dependent Information" Value="DependentsInfo" />
                                                    <telerik:RadPanelItem ImageUrl="../../Images/menu_bullet_icon.png" Text="Personal Legal Documents" Value="PersonalLegalDocument" Visible="false" />                                                    
                                                    <%--<telerik:RadPanelItem ImageUrl="../../Images/menu_bullet_icon.png" Text="View E-Payslip" Value="EPayslip" NavigateUrl="https://selfservice.garmco.com/" Target="_blank" />--%>
                                                    <telerik:RadPanelItem ImageUrl="../../Images/menu_bullet_icon.png" Text="View Workplace Swipes" Value="PlantSwipe" NavigateUrl="http://tasweb/Views/UserFunctions/MissingSwipesInquiry.aspx" Target="_blank" />
                                                    <telerik:RadPanelItem ImageUrl="../../Images/menu_bullet_icon.png" Text="View Leave Planner" Value="EmployeeLeavePlanner" NavigateUrl="http://leaveplanner/EmpOnLeaveView.aspx" Target="_blank" />
                                                    <telerik:RadPanelItem ImageUrl="../../Images/menu_bullet_icon.png" Text="Apply for Annual Leave" Value="LeaveRequisition" NavigateUrl="http://gap/Default.aspx?appType=leave&apploc=Leave/Default.aspx?leaveType=AL" Target="_blank" />
                                                    <telerik:RadPanelItem ImageUrl="../../Images/menu_bullet_icon.png" Text="Apply for Day in Lieu" Value="DILRequisition" NavigateUrl="http://dil/Views/DIL/DILUsage.aspx" Target="_blank" />                                                    
                                                </Items>
                                            </telerik:RadPanelItem>
                                            <telerik:RadPanelItem Text="Other System Links" Expanded="True" Font-Bold="true" Font-Size="10pt" Visible="false">
                                                <Items>
                                                    <telerik:RadPanelItem ImageUrl="../../Images/menu_bullet_icon.png" Text="Employee Leave Planner" Value="EmployeeLeavePlanner" NavigateUrl="http://leaveplanner" Target="_blank" />
                                                    <telerik:RadPanelItem ImageUrl="../../Images/menu_bullet_icon.png" Text="Leave Requisition System" Value="LeaveRequisition" NavigateUrl="http://gap/Index.aspx?url=Default.aspx?appType=leave&apploc=Leave/Default.aspx?leaveType=AL" Target="_blank" />
                                                    <telerik:RadPanelItem ImageUrl="../../Images/menu_bullet_icon.png" Text="DIL Requisition System" Value="DILRequisition" NavigateUrl="http://dil" Target="_blank" />
                                                    <telerik:RadPanelItem ImageUrl="../../Images/menu_bullet_icon.png" Text="Plant Swipe Access System" Value="PlantSwipeAccessSystem" NavigateUrl="http://tasweb" Target="_blank" />
                                                </Items>
                                            </telerik:RadPanelItem>
                                        </Items>
                                    </telerik:RadPanelBar>
                                </td>
                            </tr>
                        </table>
                    </td>
                    <td style="width: auto; vertical-align: top; padding-top: 0px; text-align: left;">
                        <asp:Panel ID="panBody" runat="server" CssClass="PanelNoIcon" style="margin-top: 0px; margin-right: 20px; margin-left: 5px; padding-bottom: 10px;">        
                            <telerik:RadTabStrip ID="tabMain" runat="server" SelectedIndex="0"   
                                MultiPageID="MyMultiPage" ReorderTabsOnSelect="True" 
                                CausesValidation="False" ontabclick="tabMain_TabClick" 
                                style="padding-top: 0px; padding-left: 0px; padding-right: 0px;" 
                                Skin="Silk" Align="Right">
                                <Tabs>
                                    <telerik:RadTab Text="Swipes History" Font-Size="9pt" Font-Bold="True" Selected="True" Value="SwipeHistory">
                                    </telerik:RadTab>                                     
                                    <telerik:RadTab Text="Absences History" Font-Size="9pt" Font-Bold="True" Visible="false" Value="AbsenceHistory">
                                    </telerik:RadTab> 
                                    <telerik:RadTab Text="Leave History" Font-Size="9pt" Font-Bold="True" Visible="false" Value="LeaveHistory">
                                    </telerik:RadTab> 
                                    <telerik:RadTab Text="Leave Balance History" Font-Size="9pt" Font-Bold="True" Visible="false" Value="LeaveBalance">
                                    </telerik:RadTab> 
                                    <telerik:RadTab Text="Attendance History" Font-Size="9pt" Font-Bold="True" Visible="false" Value="AttendanceHistory">
                                    </telerik:RadTab>
                                    <telerik:RadTab Text="Training History" Font-Size="9pt" Font-Bold="True" Visible="false" Value="TrainingHistory">
                                    </telerik:RadTab> 
                                    <telerik:RadTab Text="DIL Entitlements" Font-Size="9pt" Font-Bold="True" Visible="false" Value="DILEntitlement">
                                    </telerik:RadTab> 
                                    <telerik:RadTab Text="Shift Pattern Information" Font-Size="9pt" Font-Bold="True" Visible="false" Value="ShiftPatternInfo">
                                    </telerik:RadTab> 
                                    <telerik:RadTab Text="Dependents Information" Font-Size="9pt" Font-Bold="True" Visible="false" Value="DependentsInfo">
                                    </telerik:RadTab> 
                                    <telerik:RadTab Text="Personal Legal Documents" Font-Size="9pt" Font-Bold="True" Visible="false" Value="PersonalLegalDocument">
                                    </telerik:RadTab>                                     
                                </Tabs>
                            </telerik:RadTabStrip>

                            <telerik:RadMultiPage ID="MyMultiPage" runat="server" SelectedIndex="0" Width="100%" style="padding-top: 5px; padding-left: 0px; padding-right: 10px;">
                                <telerik:RadPageView ID="SwipeHistoryView" runat="server">
                                    <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                                        <tr>
                                            <td class="LabelBold" style="width: 133px; padding-right: 2px; color: blue; font-size: 9pt;">
                                                Show Filter Criterias
                                            </td>
                                            <td style="width: 100px; text-align: left;">
                                                <asp:CheckBox ID="chkSwipeHistoryFilter" runat="server" Text="" TextAlign="Right" SkinID="CheckBold" style="padding-left: 0px;" AutoPostBack="True" OnCheckedChanged="chkSwipeHistoryFilter_CheckedChanged" />
                                            </td>
                                            <td style="text-align: right; color: Purple; font-weight: bold; font-size: 9pt; padding-right: 10px;">
                                                <asp:Label ID="lblSwipeHistorySearchString" runat="server" Text="" Width="100%" />                         
                                            </td>
                                        </tr>
                                    </table>

                                    <asp:Panel ID="panSwipeHistoryFilter" runat="server" CssClass="GroupPanelHeader" GroupingText=""  BorderStyle="None" style="padding: 0px; margin-top: 5px;">
                                        <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                                            <tr style="height: 23px;">
                                                <td style="width: 130px; padding-right: 0px;">
                                                    <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                                                        <tr style="margin: 0px; padding: 0px; vertical-align: top;">                                
                                                            <td class="LabelBold" style="width: auto; padding-top: 3px; padding-right: 0px;">
                                                                Select Pay Period
                                                            </td>
                                                            <td style="width: 20px; text-align: right; padding-left: 0px; padding-top: 0px;">
                                                                <asp:CheckBox ID="chkPayPeriodSwipeHistory" runat="server" Text="" style="padding-left: 0px;" 
                                                                    AutoPostBack="True" OnCheckedChanged="chkPayPeriodSwipeHistory_CheckedChanged" />
                                                            </td>
                                                        </tr>
                                                    </table>                                                          
                                                </td>
                                                <td style="width: 300px; padding-right: 0px;">
                                                    <table border="0" style="width: 100%; text-align: left; margin: 0px; padding: 0px; table-layout: fixed;">
                                                        <tr>
                                                            <td style="width: 100px; padding-left: 5px;">                                    
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
                                                                    Width="70px" AutoPostBack="True" OnTextChanged="txtYear_TextChanged">
                                                                    <NumberFormat DecimalDigits="0" ZeroPattern="n" GroupSeparator="" />
                                                                </telerik:RadNumericTextBox>
                                                            </td>
                                                        </tr>
                                                    </table>                                               
                                                </td>
                                                <td class="LabelBold" style="width: 70px;">
                                                    Location
                                                </td>
                                                <td class="TextNormal" style="width: 300px;">
                                                    <telerik:RadComboBox ID="cboLocation" runat="server"
                                                        DropDownWidth="295px" 
                                                        HighlightTemplatedItems="True" 
                                                        EmptyMessage="Select Location"
                                                        Skin="Office2010Silver" 
                                                        Width="100%"
                                                        Height="150px"
                                                        EnableVirtualScrolling="True" />
                                                </td>
                                                <td />
                                            </tr>
                                            <tr style="height: 23px;">
                                                <td class="LabelBold" style="padding-right: 5px;">
                                                    <asp:CustomValidator ID="cusValSwipeDuration" runat="server" ControlToValidate="txtGeneric" 
                                                        CssClass="LabelValidationError" Display="Dynamic" 
                                                        ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                                                        ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                                                    Date Duration
                                                </td>
                                                <td style="padding-right: 0px;">
                                                    <table border="0" style="width: 100%; text-align: left; margin: 0px; padding: 0px; table-layout: fixed;">
                                                        <tr>
                                                            <td style="width: 120px; padding-left: 3px;">
                                                                <telerik:RadDatePicker ID="dtpSwipeHistorySDate" runat="server"
                                                                    Width="100%" Skin="Vista">
                                                                    <Calendar ID="Calendar1" runat="server" Skin="Vista" UseColumnHeadersAsSelectors="False" 
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
                                                            <td class="LabelBold" style="width: 15px; text-align: center; padding: 0px;">
                                                                ~
                                                            </td>
                                                            <td style="width: 120px;">
                                                                <telerik:RadDatePicker ID="dtpSwipeHistoryEDate" runat="server"
                                                                    Width="100%" Skin="Vista">
                                                                    <Calendar ID="Calendar2" runat="server" Skin="Vista" UseColumnHeadersAsSelectors="False" 
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
                                                            <td />
                                                        </tr>
                                                    </table>
                                                </td>
                                                <td class="LabelBold">
                                                    
                                                </td>
                                                <td class="TextNormal">
                                                    
                                                </td>
                                                <td />
                                            </tr>
                                            <tr>
                                                <td class="LabelBold">
                                                    <asp:CustomValidator ID="cusValButtonSwipeHistory" runat="server" ControlToValidate="txtGeneric" 
                                                        CssClass="LabelValidationError" Display="Dynamic" 
                                                        ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                                                        ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                                                </td>
                                                <td colspan="3" style="padding-top: 5px; padding-left: 5px;">
                                                    <telerik:RadButton ID="btnSearchSwipeHistory" runat="server" Text="Search" ToolTip="Begin searching for records" 
                                                        CssClass="RadButtonStyle" ValidationGroup="valPrimary" Font-Size="9pt" OnClick="btnSearchSwipeHistory_Click" 
                                                        Skin="Office2010Silver" Width="70px" />
                                                    <telerik:RadButton ID="btnResetSwipeHistory" runat="server" Text="Reset" ToolTip="Reset filter criterias" 
                                                        CssClass="RadButtonStyle" ValidationGroup="valPrimary" Font-Size="9pt" OnClick="btnResetSwipeHistory_Click" 
                                                        Skin="Office2010Silver" Width="70px" />    
                                                </td>
                                                <td />
                                            </tr>
                                        </table>
                                    </asp:Panel>

                                    <asp:Panel ID="panSwipeHistoryGrid" runat="server" style="width: 100%; text-align: left; table-layout: fixed; margin-top: 5px;">
                                        <table border="0" style="width: 100%; text-align: left; table-layout: fixed;">
                                            <tr>
                                                <td style="padding-left: 10px; padding-right: 0px;">
                                                    <telerik:RadGrid ID="gridSwipeHistory" runat="server"
                                                        AllowSorting="true" AllowMultiRowSelection="true"
                                                        PageSize="10" AutoGenerateColumns="false" Skin="Silk" 
                                                        GridLines="None" Width="100%" Height="" CellSpacing="0"
                                                        onpageindexchanged="gridSwipeHistory_PageIndexChanged" 
                                                        onpagesizechanged="gridSwipeHistory_PageSizeChanged" 
                                                        onsortcommand="gridSwipeHistory_SortCommand" 
                                                        onitemcommand="gridSwipeHistory_ItemCommand" 
                                                        onitemdatabound="gridSwipeHistory_ItemDataBound" 
                                                        Font-Names="Tahoma" Font-Size="9pt" Font-Bold="False" 
                                                        AllowPaging = "true" BorderStyle="Outset" BorderWidth="1px">
                                                        <ExportSettings ExportOnlyData="true" IgnorePaging="true" OpenInNewWindow="true" FileName="SwipeHistory" HideStructureColumns="true">
                                                            <Pdf PageHeight="210mm" PageWidth="310mm" PageTitle="Employee Swipe History" DefaultFontFamily="Arial Unicode MS"
                                                            PageBottomMargin="20mm" PageTopMargin="20mm" PageLeftMargin="20mm" PageRightMargin="20mm" PaperSize="A4" AllowPrinting="true" />
                                                        </ExportSettings>
                                                        <AlternatingItemStyle Font-Names="Tahoma" Font-Size="9pt" Wrap="True" />
                                                        <MasterTableView DataKeyNames="SwipeDate" ClientDataKeyNames="SwipeDate" 
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
                                                               <telerik:GridBoundColumn DataField="SwipeDate" HeaderText="Date"
                                                                    DataFormatString="{0:dd-MMM-yyyy}" DataType="System.DateTime" 
                                                                    ReadOnly="True" SortExpression="SwipeDate" UniqueName="SwipeDate">
                                                                    <ColumnValidationSettings>
                                                                        <ModelErrorMessage Text="" />
                                                                    </ColumnValidationSettings>
                                                                    <HeaderStyle Width="100px" Font-Bold="True"></HeaderStyle>
                                                                    <ItemStyle Font-Bold="true" ForeColor="Purple" />
                                                                </telerik:GridBoundColumn>    
                                                                <telerik:GridBoundColumn DataField="SwipeTime" HeaderText="Swipe Time"
                                                                    DataFormatString="{0:HH:mm:ss}" DataType="System.DateTime" 
                                                                    ReadOnly="True" SortExpression="SwipeTime" UniqueName="SwipeTime">
                                                                    <ColumnValidationSettings>
                                                                        <ModelErrorMessage Text="" />
                                                                    </ColumnValidationSettings>
                                                                    <HeaderStyle Width="100px" Font-Bold="True"></HeaderStyle>
                                                                </telerik:GridBoundColumn>                                                                                                                                                                                                      
                                                                <%--<telerik:GridBoundColumn DataField="EmpNo" DataType="System.Int32" HeaderText="Emp. No." 
                                                                    ReadOnly="True" SortExpression="EmpNo" UniqueName="EmpNo" Display="false">
                                                                    <ColumnValidationSettings>
                                                                        <ModelErrorMessage Text="" />
                                                                    </ColumnValidationSettings>
                                                                    <HeaderStyle Width="80px" Font-Bold="True"></HeaderStyle>
                                                                </telerik:GridBoundColumn>     
                                                                <telerik:GridTemplateColumn DataField="EmpName" FilterControlAltText="Filter Employee Name column" HeaderText="Employe Name" 
                                                                    SortExpression="EmpName" UniqueName="EmpName" Display="false">
								                                    <HeaderStyle Width="250px" HorizontalAlign="Left" Font-Size="8pt" />
								                                    <ItemTemplate>
									                                    <div class="columnEllipsis" style="width: 250px; text-align: left;">
										                                    <asp:Literal ID="litEmpName" runat="server" Text='<%# Eval("EmpName") %>' />
									                                    </div>
								                                    </ItemTemplate>
							                                    </telerik:GridTemplateColumn>  
                                                                <telerik:GridTemplateColumn DataField="Position" FilterControlAltText="Filter Position column" HeaderText="Position" 
                                                                    SortExpression="Position" UniqueName="Position" Display="false">
								                                    <HeaderStyle Width="250px" HorizontalAlign="Left" Font-Size="8pt" />
								                                    <ItemTemplate>
									                                    <div class="columnEllipsis" style="width: 250px; text-align: left;">
										                                    <asp:Literal ID="litPosition" runat="server" Text='<%# Eval("Position") %>' />
									                                    </div>
								                                    </ItemTemplate>
							                                    </telerik:GridTemplateColumn>    
                                                                <telerik:GridTemplateColumn DataField="CostCenterFullName" FilterControlAltText="Filter Cost Center column" HeaderText="Cost Center" 
                                                                    SortExpression="CostCenterFullName" UniqueName="CostCenterFullName" Display="false">
								                                    <HeaderStyle Width="250px" HorizontalAlign="Left" />
								                                    <ItemTemplate>
									                                    <div class="columnEllipsis" style="width: 250px; text-align: left;">
										                                    <asp:Literal ID="litCostCenter" runat="server" Text='<%# Eval("CostCenterFullName") %>' />
									                                    </div>
								                                    </ItemTemplate>
							                                    </telerik:GridTemplateColumn>--%>
                                                                <telerik:GridTemplateColumn DataField="SwipeLocation" FilterControlAltText="Filter Location column" HeaderText="Location" 
                                                                    SortExpression="SwipeLocation" UniqueName="SwipeLocation">
								                                    <HeaderStyle Width="200px" HorizontalAlign="Left" />
								                                    <ItemTemplate>
									                                    <div class="columnEllipsis" style="width: 200px; text-align: left;">
										                                    <asp:Literal ID="litSwipeLocation" runat="server" Text='<%# Eval("SwipeLocation") %>' />
									                                    </div>
								                                    </ItemTemplate>
							                                    </telerik:GridTemplateColumn>                                     
                                                                <telerik:GridBoundColumn DataField="SwipeType" DataType="System.String" HeaderText="Swipe Type"
                                                                    ReadOnly="True" SortExpression="SwipeType" UniqueName="SwipeType">
                                                                    <ColumnValidationSettings>
                                                                        <ModelErrorMessage Text="" />
                                                                    </ColumnValidationSettings>
                                                                    <HeaderStyle Width="90px" Font-Bold="True"></HeaderStyle>
                                                                </telerik:GridBoundColumn>                                                                                                 
                                                                <telerik:GridBoundColumn DataField="ShiftPatCode" DataType="System.String" HeaderText="Shift Pat. Code" 
                                                                    ReadOnly="True" SortExpression="ShiftPatCode" UniqueName="ShiftPatCode">
                                                                    <ColumnValidationSettings>
                                                                        <ModelErrorMessage Text="" />
                                                                    </ColumnValidationSettings>
                                                                    <HeaderStyle Width="115px" Font-Bold="True"></HeaderStyle>
                                                                </telerik:GridBoundColumn>   
                                                                <telerik:GridBoundColumn DataField="ShiftCode" DataType="System.String" HeaderText="Shift Code" 
                                                                    ReadOnly="True" SortExpression="ShiftCode" UniqueName="ShiftCode">
                                                                    <ColumnValidationSettings>
                                                                        <ModelErrorMessage Text="" />
                                                                    </ColumnValidationSettings>
                                                                    <HeaderStyle Width="90px" Font-Bold="True"></HeaderStyle>
                                                                </telerik:GridBoundColumn>   
                                                                 <telerik:GridBoundColumn DataField="ShiftPointer" DataType="System.Int32" HeaderText="Shift Pointer"
                                                                    ReadOnly="True" SortExpression="ShiftPointer" UniqueName="ShiftPointer">
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
                                                        <HeaderStyle Font-Bold="True" Font-Size="7.5pt" />
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
                                    <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                                        <tr>
                                            <td class="LabelBold" style="width: 133px; padding-right: 2px; color: blue; font-size: 9pt;">
                                                Show Filter Criterias
                                            </td>
                                            <td style="width: 100px; text-align: left;">
                                                <asp:CheckBox ID="chkAbsenceHistoryFilter" runat="server" Text="" TextAlign="Right" SkinID="CheckBold" style="padding-left: 0px;" AutoPostBack="True" OnCheckedChanged="chkAbsenceHistoryFilter_CheckedChanged" />
                                            </td>
                                            <td style="text-align: right; color: Purple; font-weight: bold; font-size: 9pt; padding-right: 10px;">
                                                <asp:Label ID="lblAbsenceHistorySearchString" runat="server" Text="" Width="100%" />                         
                                            </td>
                                        </tr>
                                    </table>

                                    <asp:Panel ID="panAbsenceHistoryFilter" runat="server" CssClass="GroupPanelHeader" GroupingText=""  BorderStyle="None" style="padding: 0px; margin-top: 5px;">
                                        <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                                            <tr style="height: 23px;">
                                                <td style="width: 130px; padding-right: 0px;">
                                                    <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                                                        <tr style="margin: 0px; padding: 0px; vertical-align: top;">                                
                                                            <td class="LabelBold" style="width: auto; padding-top: 3px; padding-right: 0px;">
                                                                Select Pay Period
                                                            </td>
                                                            <td style="width: 20px; text-align: right; padding-left: 0px; padding-top: 0px;">
                                                                <asp:CheckBox ID="chkPayPeriodAbsence" runat="server" Text="" style="padding-left: 0px;" 
                                                                    AutoPostBack="True" OnCheckedChanged="chkPayPeriodAbsence_CheckedChanged" />
                                                            </td>
                                                        </tr>
                                                    </table>                                                    
                                                </td>
                                                <td style="width: 300px; padding-right: 0px;">
                                                    <table border="0" style="width: 100%; text-align: left; margin: 0px; padding: 0px; table-layout: fixed;">
                                                        <tr>
                                                            <td style="width: 100px; padding-left: 5px;">                                    
                                                                <telerik:RadComboBox ID="cboMonthAbsence" runat="server"
                                                                    DropDownWidth="140px" 
                                                                    HighlightTemplatedItems="True" 
                                                                    Skin="Office2010Silver" 
                                                                    Width="100%" 
                                                                    EmptyMessage="Select Month" ToolTip="Payroll month"
                                                                    EnableVirtualScrolling="True" AutoPostBack="True" 
                                                                    onselectedindexchanged="cboMonthAbsence_SelectedIndexChanged" >
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
                                                                 <telerik:RadNumericTextBox ID="txtYearAbsence" runat="server" ToolTip="Payroll year" 
                                                                    DataType="System.UInt32" MaxLength="4" MaxValue="2099" MinValue="0" 
                                                                    Width="70px" AutoPostBack="True" OnTextChanged="txtYearAbsence_TextChanged">
                                                                    <NumberFormat DecimalDigits="0" ZeroPattern="n" GroupSeparator="" />
                                                                </telerik:RadNumericTextBox>
                                                            </td>
                                                        </tr>
                                                    </table>                                               
                                                </td>
                                                <td class="LabelBold" style="width: 70px;">
                                                    
                                                </td>
                                                <td class="TextNormal" style="width: 300px;">
                                                    
                                                </td>
                                                <td />
                                            </tr>
                                            <tr style="height: 23px;">
                                                <td class="LabelBold" style="padding-right: 5px;">
                                                    <asp:CustomValidator ID="cusValDurationAbsence" runat="server" ControlToValidate="txtGeneric" 
                                                        CssClass="LabelValidationError" Display="Dynamic" 
                                                        ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                                                        ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                                                    Date Duration
                                                </td>
                                                <td style="padding-right: 0px;">
                                                    <table border="0" style="width: 100%; text-align: left; margin: 0px; padding: 0px; table-layout: fixed;">
                                                        <tr>
                                                            <td style="width: 120px; padding-left: 3px;">
                                                                <telerik:RadDatePicker ID="dtpStartDateAbsence" runat="server"
                                                                    Width="100%" Skin="Vista">
                                                                    <Calendar ID="Calendar3" runat="server" Skin="Vista" UseColumnHeadersAsSelectors="False" 
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
                                                            <td style="width: 120px;">
                                                                <telerik:RadDatePicker ID="dtpEndDateAbsence" runat="server"
                                                                    Width="100%" Skin="Vista">
                                                                    <Calendar ID="Calendar4" runat="server" Skin="Vista" UseColumnHeadersAsSelectors="False" 
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
                                                            <td />
                                                        </tr>
                                                    </table>
                                                </td>
                                                <td class="LabelBold">
                                                    
                                                </td>
                                                <td class="TextNormal">
                                                    
                                                </td>
                                                <td />
                                            </tr>
                                            <tr>
                                                <td class="LabelBold">
                                                    <asp:CustomValidator ID="CustomValidator3" runat="server" ControlToValidate="txtGeneric" 
                                                        CssClass="LabelValidationError" Display="Dynamic" 
                                                        ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                                                        ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                                                </td>
                                                <td colspan="3" style="padding-top: 5px; padding-left: 5px;">
                                                    <telerik:RadButton ID="btnSearchAbsence" runat="server" Text="Search" ToolTip="Begin searching for records" 
                                                        CssClass="RadButtonStyle" ValidationGroup="valPrimary" Font-Size="9pt" OnClick="btnSearchAbsence_Click" 
                                                        Skin="Office2010Silver" Width="70px" />
                                                    <telerik:RadButton ID="btnResetAbsence" runat="server" Text="Reset" ToolTip="Reset filter criterias" 
                                                        CssClass="RadButtonStyle" ValidationGroup="valPrimary" Font-Size="9pt" OnClick="btnResetAbsence_Click" 
                                                        Skin="Office2010Silver" Width="70px" />    
                                                </td>
                                                <td />
                                            </tr>
                                        </table>
                                    </asp:Panel>

                                    <asp:Panel ID="panAbsenceHistory" runat="server" style="width: 100%; text-align: left; table-layout: fixed; margin-top: 5px;">
                                        <table border="0" style="width: 100%; text-align: left; table-layout: fixed;">
                                            <tr>
                                                <td style="padding-left: 10px; padding-right: 0px;">
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
                                                        AllowPaging = "true" BorderStyle="Outset" BorderWidth="1px"
                                                        AllowCustomPaging="True" VirtualItemCount="1">
                                                        <ExportSettings ExportOnlyData="true" IgnorePaging="true" OpenInNewWindow="true" FileName="AbsenceHistory" HideStructureColumns="true">
                                                            <Pdf PageHeight="210mm" PageWidth="310mm" PageTitle="Absences History" DefaultFontFamily="Arial Unicode MS"
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
                                                               <telerik:GridBoundColumn DataField="DT" HeaderText="Date"
                                                                    DataFormatString="{0:dd-MMM-yyyy}" DataType="System.DateTime" 
                                                                    ReadOnly="True" SortExpression="DT" UniqueName="DT">
                                                                    <ColumnValidationSettings>
                                                                        <ModelErrorMessage Text="" />
                                                                    </ColumnValidationSettings>
                                                                    <HeaderStyle Width="100px" Font-Bold="True"></HeaderStyle>
                                                                   <ItemStyle Font-Bold="true" ForeColor="Purple" />
                                                                </telerik:GridBoundColumn>    
                                                                <%--<telerik:GridBoundColumn DataField="EmpNo" DataType="System.Int32" HeaderText="Emp. No." 
                                                                    ReadOnly="True" SortExpression="EmpNo" UniqueName="EmpNo" Display="false">
                                                                    <ColumnValidationSettings>
                                                                        <ModelErrorMessage Text="" />
                                                                    </ColumnValidationSettings>
                                                                    <HeaderStyle Width="80px" Font-Bold="True"></HeaderStyle>
                                                                </telerik:GridBoundColumn>     
                                                                <telerik:GridTemplateColumn DataField="EmpName" FilterControlAltText="Filter Employee Name column" HeaderText="Employe Name" 
                                                                    SortExpression="EmpName" UniqueName="EmpName" Display="false">
								                                    <HeaderStyle Width="250px" HorizontalAlign="Left" />
								                                    <ItemTemplate>
									                                    <div class="columnEllipsis" style="width: 250px; text-align: left;">
										                                    <asp:Literal ID="litEmpName" runat="server" Text='<%# Eval("EmpName") %>' />
									                                    </div>
								                                    </ItemTemplate>
							                                    </telerik:GridTemplateColumn> --%> 
                                                                <telerik:GridBoundColumn DataField="RemarkCode" DataType="System.String" HeaderText="Leave Code"
                                                                    ReadOnly="True" SortExpression="RemarkCode" UniqueName="RemarkCode">
                                                                    <ColumnValidationSettings>
                                                                        <ModelErrorMessage Text="" />
                                                                    </ColumnValidationSettings>
                                                                    <HeaderStyle Width="90px" Font-Bold="True"></HeaderStyle>
                                                                </telerik:GridBoundColumn>                                                                                                 
                                                                <telerik:GridBoundColumn DataField="AttendanceRemarks" DataType="System.String" HeaderText="Description" 
                                                                    ReadOnly="True" SortExpression="AttendanceRemarks" UniqueName="AttendanceRemarks">
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
                                                        <HeaderStyle Font-Bold="True" Font-Size="7.5pt" />
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
                                    <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                                        <tr>
                                            <td class="LabelBold" style="width: 133px; padding-right: 2px; color: blue; font-size: 9pt;">
                                                Show Filter Criterias
                                            </td>
                                            <td style="width: 100px; text-align: left;">
                                                <asp:CheckBox ID="chkLeaveHistoryFilter" runat="server" Text="" TextAlign="Right" SkinID="CheckBold" style="padding-left: 0px;" 
                                                    AutoPostBack="True" OnCheckedChanged="chkLeaveHistoryFilter_CheckedChanged" />
                                            </td>
                                            <td style="text-align: right; color: Purple; font-weight: bold; font-size: 9pt; padding-right: 10px;">
                                                <asp:Label ID="lblLeaveHistorySearchString" runat="server" Text="" Width="100%" />                         
                                            </td>
                                        </tr>
                                    </table>

                                    <asp:Panel ID="panLeaveHistoryFilter" runat="server" CssClass="GroupPanelHeader" GroupingText=""  BorderStyle="None" style="padding: 0px; margin-top: 5px;">
                                        <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                                            <tr style="height: 23px;">
                                                <td style="width: 130px; padding-right: 0px;">
                                                    <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                                                        <tr style="margin: 0px; padding: 0px; vertical-align: top;">                                
                                                            <td class="LabelBold" style="width: auto; padding-top: 3px; padding-right: 0px;">
                                                                Select Pay Period
                                                            </td>
                                                            <td style="width: 20px; text-align: right; padding-left: 0px; padding-top: 0px;">
                                                                <asp:CheckBox ID="chkPayPeriodLeaveHistory" runat="server" Text="" style="padding-left: 0px;" 
                                                                    AutoPostBack="True" OnCheckedChanged="chkPayPeriodLeaveHistory_CheckedChanged" />
                                                            </td>
                                                        </tr>
                                                    </table>                                                        
                                                </td>
                                                <td style="width: 300px; padding-right: 0px;">
                                                    <table border="0" style="width: 100%; text-align: left; margin: 0px; padding: 0px; table-layout: fixed;">
                                                        <tr>
                                                            <td style="width: 100px; padding-left: 5px;">                                    
                                                                <telerik:RadComboBox ID="cboMonthLeaveHistory" runat="server"
                                                                    DropDownWidth="140px" 
                                                                    HighlightTemplatedItems="True" 
                                                                    Skin="Office2010Silver" 
                                                                    Width="100%" 
                                                                    EmptyMessage="Select Month" ToolTip="Payroll month"
                                                                    EnableVirtualScrolling="True" AutoPostBack="True" 
                                                                    onselectedindexchanged="cboMonthLeaveHistory_SelectedIndexChanged" >
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
                                                                 <telerik:RadNumericTextBox ID="txtYearLeaveHistory" runat="server" ToolTip="Payroll year" 
                                                                    DataType="System.UInt32" MaxLength="4" MaxValue="2099" MinValue="0" 
                                                                    Width="70px" AutoPostBack="True" OnTextChanged="txtYearLeaveHistory_TextChanged">
                                                                    <NumberFormat DecimalDigits="0" ZeroPattern="n" GroupSeparator="" />
                                                                </telerik:RadNumericTextBox>
                                                            </td>
                                                        </tr>
                                                    </table>                                               
                                                </td>
                                                <td class="LabelBold" style="width: 70px;">
                                                    
                                                </td>
                                                <td class="TextNormal" style="width: 300px;">
                                                    
                                                </td>
                                                <td />
                                            </tr>
                                            <tr style="height: 23px;">
                                                <td class="LabelBold" style="padding-right: 5px;">
                                                    <asp:CustomValidator ID="cusValDurationLeaveHistory" runat="server" ControlToValidate="txtGeneric" 
                                                        CssClass="LabelValidationError" Display="Dynamic" 
                                                        ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                                                        ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                                                    Date Duration
                                                </td>
                                                <td style="padding-right: 0px;">
                                                    <table border="0" style="width: 100%; text-align: left; margin: 0px; padding: 0px; table-layout: fixed;">
                                                        <tr>
                                                            <td style="width: 120px; padding-left: 3px;">
                                                                <telerik:RadDatePicker ID="dtpStartDateLeaveHistory" runat="server"
                                                                    Width="100%" Skin="Vista">
                                                                    <Calendar ID="Calendar5" runat="server" Skin="Vista" UseColumnHeadersAsSelectors="False" 
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
                                                            <td class="LabelBold" style="width: 15px; text-align: center; padding: 0px;">
                                                                ~
                                                            </td>
                                                            <td style="width: 120px;">
                                                                <telerik:RadDatePicker ID="dtpEndDateLeaveHistory" runat="server"
                                                                    Width="100%" Skin="Vista">
                                                                    <Calendar ID="Calendar6" runat="server" Skin="Vista" UseColumnHeadersAsSelectors="False" 
                                                                        UseRowHeadersAsSelectors="False" ViewSelectorText="x">
                                                                    </Calendar>
                                                                    <DateInput ID="DateInput6" runat="server" DateFormat="d/M/yyyy" DisplayDateFormat="d/M/yyyy">
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
                                                    </table>
                                                </td>
                                                <td class="LabelBold">
                                                    
                                                </td>
                                                <td class="TextNormal">
                                                    
                                                </td>
                                                <td />
                                            </tr>
                                            <tr>
                                                <td class="LabelBold">
                                                    
                                                </td>
                                                <td colspan="3" style="padding-top: 5px; padding-left: 5px;">
                                                    <telerik:RadButton ID="btnSearchLeaveHistory" runat="server" Text="Search" ToolTip="Initiate search for Leave History records" 
                                                        CssClass="RadButtonStyle" ValidationGroup="valPrimary" Font-Size="9pt" OnClick="btnSearchLeaveHistory_Click" 
                                                        Skin="Office2010Silver" Width="70px" />
                                                    <telerik:RadButton ID="btnResetLeaveHistory" runat="server" Text="Reset" ToolTip="Reset filter criterias" 
                                                        CssClass="RadButtonStyle" ValidationGroup="valPrimary" Font-Size="9pt" OnClick="btnResetLeaveHistory_Click" 
                                                        Skin="Office2010Silver" Width="70px" />    
                                                </td>
                                                <td />
                                            </tr>
                                        </table>
                                    </asp:Panel>

                                    <asp:Panel ID="panLeaveDetailsGrid" runat="server" style="width: 100%; text-align: left; table-layout: fixed; margin-top: 5px;">
                                        <table border="0" style="width: 100%; text-align: left; table-layout: fixed;">
                                            <tr>
                                                <td style="padding-left: 10px; padding-right: 0px;">
                                                    <telerik:RadGrid ID="gridLeaveDetails" runat="server"
                                                        AllowSorting="true" AllowMultiRowSelection="true"
                                                        PageSize="10" AutoGenerateColumns="false" Skin="Silk" 
                                                        GridLines="None" Width="100%" Height="" CellSpacing="0"
                                                        onpageindexchanged="gridLeaveDetails_PageIndexChanged" 
                                                        onpagesizechanged="gridLeaveDetails_PageSizeChanged" 
                                                        onsortcommand="gridLeaveDetails_SortCommand" 
                                                        onitemcommand="gridLeaveDetails_ItemCommand" 
                                                        onitemdatabound="gridLeaveDetails_ItemDataBound" 
                                                        Font-Names="Tahoma" Font-Size="9pt" Font-Bold="False" 
                                                        AllowPaging = "true" BorderStyle="Outset" BorderWidth="1px">
                                                        <ExportSettings ExportOnlyData="true" IgnorePaging="true" OpenInNewWindow="true" FileName="LeaveDetails" HideStructureColumns="true">
                                                            <Pdf PageHeight="210mm" PageWidth="310mm" PageTitle="Leave Details" DefaultFontFamily="Arial Unicode MS"
                                                            PageBottomMargin="20mm" PageTopMargin="20mm" PageLeftMargin="20mm" PageRightMargin="20mm" PaperSize="A4" AllowPrinting="true" />
                                                        </ExportSettings>
                                                        <AlternatingItemStyle Font-Names="Tahoma" Font-Size="9pt" Wrap="True" />
                                                        <MasterTableView DataKeyNames="LeaveEmpNo" ClientDataKeyNames="LeaveEmpNo" 
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
                                                                <telerik:GridBoundColumn DataField="LeaveEntitlement" DataType="System.String" HeaderText="Leave Entitlement" 
                                                                    ReadOnly="True" SortExpression="LeaveEntitlement" UniqueName="LeaveEntitlement">
                                                                    <ColumnValidationSettings>
                                                                        <ModelErrorMessage Text="" />
                                                                    </ColumnValidationSettings>
                                                                    <HeaderStyle Width="140px" Font-Bold="True" />
                                                                </telerik:GridBoundColumn> 
                                                                <telerik:GridBoundColumn DataField="LeaveTakenAsOfDate" DataType="System.String" HeaderText="Leaves Taken to Date" 
                                                                    ReadOnly="True" SortExpression="LeaveTakenAsOfDate" UniqueName="LeaveTakenAsOfDate">
                                                                    <ColumnValidationSettings>
                                                                        <ModelErrorMessage Text="" />
                                                                    </ColumnValidationSettings>
                                                                    <HeaderStyle Width="170px" Font-Bold="True" />
                                                                </telerik:GridBoundColumn> 
                                                                <telerik:GridBoundColumn DataField="LeaveTakenCurrentYear" DataType="System.String" HeaderText="Leaves Taken (Current Year)" 
                                                                    ReadOnly="True" SortExpression="LeaveTakenCurrentYear" UniqueName="LeaveTakenCurrentYear">
                                                                    <ColumnValidationSettings>
                                                                        <ModelErrorMessage Text="" />
                                                                    </ColumnValidationSettings>
                                                                    <HeaderStyle Width="200px" Font-Bold="True" />
                                                                </telerik:GridBoundColumn>
                                                                <telerik:GridBoundColumn DataField="LeaveCurrentBal" DataType="System.String" HeaderText="Leave Balance to Date" 
                                                                    ReadOnly="True" SortExpression="LeaveCurrentBal" UniqueName="LeaveCurrentBal">
                                                                    <ColumnValidationSettings>
                                                                        <ModelErrorMessage Text="" />
                                                                    </ColumnValidationSettings>
                                                                    <HeaderStyle Font-Bold="True" />
                                                                    <ItemStyle Font-Bold="true" ForeColor="Green" />
                                                                </telerik:GridBoundColumn>
                                                            </Columns>
                                                        </MasterTableView>
                                                        <ClientSettings AllowColumnsReorder="False" EnableRowHoverStyle="true">
                                                            <Selecting AllowRowSelect="True" UseClientSelectColumnOnly="True" />
                                                            <Scrolling AllowScroll="true" UseStaticHeaders="true" SaveScrollPosition="true" ScrollHeight="" />
			                                                <Resizing AllowColumnResize="true" />   
                                                        </ClientSettings>
                                                        <HeaderStyle Font-Bold="True" Font-Size="7.5pt" />
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

                                    <asp:Panel ID="panLeaveHistoryGrid" runat="server" style="width: 100%; text-align: left; table-layout: fixed; margin-top: 5px;">
                                        <table border="0" style="width: 100%; text-align: left; table-layout: fixed;">
                                            <tr>
                                                <td style="padding-left: 10px; padding-right: 0px;">
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
                                                        AllowPaging = "true" BorderStyle="Outset" BorderWidth="1px"
                                                        AllowCustomPaging="True" VirtualItemCount="1">
                                                        <ExportSettings ExportOnlyData="true" IgnorePaging="true" OpenInNewWindow="true" FileName="LeaveHistory" HideStructureColumns="true">
                                                            <Pdf PageHeight="210mm" PageWidth="310mm" PageTitle="Leave History" DefaultFontFamily="Arial Unicode MS"
                                                            PageBottomMargin="20mm" PageTopMargin="20mm" PageLeftMargin="20mm" PageRightMargin="20mm" PaperSize="A4" AllowPrinting="true" />
                                                        </ExportSettings>
                                                        <AlternatingItemStyle Font-Names="Tahoma" Font-Size="9pt" Wrap="True" />
                                                        <MasterTableView DataKeyNames="LeaveNo" ClientDataKeyNames="LeaveNo" 
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
                                                               <telerik:GridBoundColumn DataField="LeaveStartDate" HeaderText="From Date"
                                                                    DataFormatString="{0:dd-MMM-yyyy}" DataType="System.DateTime" 
                                                                    ReadOnly="True" SortExpression="LeaveStartDate" UniqueName="LeaveStartDate">
                                                                    <ColumnValidationSettings>
                                                                        <ModelErrorMessage Text="" />
                                                                    </ColumnValidationSettings>
                                                                    <HeaderStyle Width="100px" Font-Bold="True"></HeaderStyle>
                                                                </telerik:GridBoundColumn>    
                                                                <telerik:GridBoundColumn DataField="LeaveEndDate" HeaderText="To Date"
                                                                    DataFormatString="{0:dd-MMM-yyyy}" DataType="System.DateTime" 
                                                                    ReadOnly="True" SortExpression="LeaveEndDate" UniqueName="LeaveEndDate">
                                                                    <ColumnValidationSettings>
                                                                        <ModelErrorMessage Text="" />
                                                                    </ColumnValidationSettings>
                                                                    <HeaderStyle Width="100px" Font-Bold="True"></HeaderStyle>
                                                                </telerik:GridBoundColumn>   
                                                                <%--<telerik:GridBoundColumn DataField="EmpNo" DataType="System.Int32" HeaderText="Emp. No." 
                                                                    ReadOnly="True" SortExpression="EmpNo" UniqueName="EmpNo" Display="false">
                                                                    <ColumnValidationSettings>
                                                                        <ModelErrorMessage Text="" />
                                                                    </ColumnValidationSettings>
                                                                    <HeaderStyle Width="80px" Font-Bold="True"></HeaderStyle>
                                                                </telerik:GridBoundColumn>--%>     
                                                                <telerik:GridBoundColumn DataField="LeaveType" DataType="System.String" HeaderText="Leave Code"
                                                                    ReadOnly="True" SortExpression="LeaveType" UniqueName="LeaveType">
                                                                    <ColumnValidationSettings>
                                                                        <ModelErrorMessage Text="" />
                                                                    </ColumnValidationSettings>
                                                                    <HeaderStyle Width="90px" Font-Bold="True"></HeaderStyle>
                                                                </telerik:GridBoundColumn>                                                                                                 
                                                                <telerik:GridBoundColumn DataField="LeaveTypeDesc" DataType="System.String" HeaderText="Description" 
                                                                    ReadOnly="True" SortExpression="LeaveTypeDesc" UniqueName="LeaveTypeDesc">
                                                                    <ColumnValidationSettings>
                                                                        <ModelErrorMessage Text="" />
                                                                    </ColumnValidationSettings>
                                                                    <HeaderStyle Width="200px" Font-Bold="True"></HeaderStyle>
                                                                </telerik:GridBoundColumn>   
                                                                <telerik:GridBoundColumn DataField="LeaveDuration" DataType="System.Double" HeaderText="Duration (Days)" 
                                                                    ReadOnly="True" SortExpression="LeaveDuration" UniqueName="LeaveDuration" DataFormatString="{0:N2}">
                                                                    <ColumnValidationSettings>
                                                                        <ModelErrorMessage Text="" />
                                                                    </ColumnValidationSettings>
                                                                    <HeaderStyle Font-Bold="True"></HeaderStyle>
                                                                    <ItemStyle Font-Bold="true" ForeColor="Purple" />
                                                                </telerik:GridBoundColumn>  
                                                            </Columns>
                                                        </MasterTableView>
                                                        <ClientSettings AllowColumnsReorder="False" EnableRowHoverStyle="true">
                                                            <Selecting AllowRowSelect="True" UseClientSelectColumnOnly="True" />
                                                            <Scrolling AllowScroll="true" UseStaticHeaders="true" SaveScrollPosition="true" ScrollHeight="" />
			                                                <Resizing AllowColumnResize="true" />   
                                                        </ClientSettings>
                                                        <HeaderStyle Font-Bold="True" Font-Size="7.5pt" />
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

                                <telerik:RadPageView ID="LeaveBalanceView" runat="server">
                                    
                                </telerik:RadPageView>

                                <telerik:RadPageView ID="AttendanceHistoryView" runat="server">
                                    <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                                        <tr>
                                            <td class="LabelBold" style="width: 133px; padding-right: 2px; color: blue; font-size: 9pt;">
                                                Show Filter Criterias
                                            </td>
                                            <td style="width: 100px; text-align: left;">
                                                <asp:CheckBox ID="chkAttendanceHistoryFilter" runat="server" Text="" TextAlign="Right" SkinID="CheckBold" style="padding-left: 0px;" 
                                                    AutoPostBack="True" OnCheckedChanged="chkAttendanceHistoryFilter_CheckedChanged" />
                                            </td>
                                            <td style="text-align: right; color: Purple; font-weight: bold; font-size: 9pt; padding-right: 10px;">
                                                <asp:Label ID="lblAttendanceHistorySearchString" runat="server" Text="" Width="100%" />                         
                                            </td>
                                        </tr>
                                    </table>

                                    <asp:Panel ID="panAttendanceHistoryFilter" runat="server" CssClass="GroupPanelHeader" GroupingText=""  BorderStyle="None" style="padding: 0px; margin-top: 5px;">
                                        <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                                            <tr style="height: 23px;">
                                                <td style="width: 130px; padding-right: 0px;">
                                                    <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                                                        <tr style="margin: 0px; padding: 0px; vertical-align: top;">                                
                                                            <td class="LabelBold" style="width: auto; padding-top: 3px; padding-right: 0px;">
                                                                Select Pay Period
                                                            </td>
                                                            <td style="width: 20px; text-align: right; padding-left: 0px; padding-top: 0px;">
                                                                <asp:CheckBox ID="chkPayPeriodAttendanceHistory" runat="server" Text="" style="padding-left: 0px;" 
                                                                    AutoPostBack="True" OnCheckedChanged="chkPayPeriodAttendanceHistory_CheckedChanged" />
                                                            </td>
                                                        </tr>
                                                    </table>                                                    
                                                </td>
                                                <td style="width: 300px; padding-right: 0px;">
                                                    <table border="0" style="width: 100%; text-align: left; margin: 0px; padding: 0px; table-layout: fixed;">
                                                        <tr>
                                                            <td style="width: 100px; padding-left: 5px;">                                    
                                                                <telerik:RadComboBox ID="cboMonthAttendanceHistory" runat="server"
                                                                    DropDownWidth="140px" 
                                                                    HighlightTemplatedItems="True" 
                                                                    Skin="Office2010Silver" 
                                                                    Width="100%" 
                                                                    EmptyMessage="Select Month" ToolTip="Payroll month"
                                                                    EnableVirtualScrolling="True" AutoPostBack="True" 
                                                                    onselectedindexchanged="cboMonthAttendanceHistory_SelectedIndexChanged" >
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
                                                                 <telerik:RadNumericTextBox ID="txtYearAttendanceHistory" runat="server" ToolTip="Payroll year" 
                                                                    DataType="System.UInt32" MaxLength="4" MaxValue="2099" MinValue="0" 
                                                                    Width="70px" AutoPostBack="True" OnTextChanged="txtYearAttendanceHistory_TextChanged">
                                                                    <NumberFormat DecimalDigits="0" ZeroPattern="n" GroupSeparator="" />
                                                                </telerik:RadNumericTextBox>
                                                            </td>
                                                        </tr>
                                                    </table>                                               
                                                </td>
                                                <td class="LabelBold" style="width: 70px;">
                                                    
                                                </td>
                                                <td class="TextNormal" style="width: 300px;">
                                                    
                                                </td>
                                                <td />
                                            </tr>
                                            <tr style="height: 23px;">
                                                <td class="LabelBold" style="padding-right: 5px;">
                                                    <asp:CustomValidator ID="cusValDurationAttendanceHistory" runat="server" ControlToValidate="txtGeneric" 
                                                        CssClass="LabelValidationError" Display="Dynamic" 
                                                        ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                                                        ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                                                    Date Duration
                                                </td>
                                                <td style="padding-right: 0px;">
                                                    <table border="0" style="width: 100%; text-align: left; margin: 0px; padding: 0px; table-layout: fixed;">
                                                        <tr>
                                                            <td style="width: 120px; padding-left: 3px;">
                                                                <telerik:RadDatePicker ID="dtpStartDateAttendanceHistory" runat="server"
                                                                    Width="100%" Skin="Vista">
                                                                    <Calendar ID="Calendar9" runat="server" Skin="Vista" UseColumnHeadersAsSelectors="False" 
                                                                        UseRowHeadersAsSelectors="False" ViewSelectorText="x">
                                                                    </Calendar>
                                                                    <DateInput ID="DateInput9" runat="server" DateFormat="d/M/yyyy" DisplayDateFormat="d/M/yyyy">
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
                                                            <td style="width: 120px;">
                                                                <telerik:RadDatePicker ID="dtpEndDateAttendanceHistory" runat="server"
                                                                    Width="100%" Skin="Vista">
                                                                    <Calendar ID="Calendar10" runat="server" Skin="Vista" UseColumnHeadersAsSelectors="False" 
                                                                        UseRowHeadersAsSelectors="False" ViewSelectorText="x">
                                                                    </Calendar>
                                                                    <DateInput ID="DateInput10" runat="server" DateFormat="d/M/yyyy" DisplayDateFormat="d/M/yyyy">
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
                                                    </table>
                                                </td>
                                                <td class="LabelBold">
                                                    
                                                </td>
                                                <td class="TextNormal">
                                                    
                                                </td>
                                                <td />
                                            </tr>
                                            <tr>
                                                <td class="LabelBold">
                                                    
                                                </td>
                                                <td colspan="3" style="padding-top: 5px; padding-left: 5px;">
                                                    <telerik:RadButton ID="btnSearchAttendanceHistory" runat="server" Text="Search" ToolTip="Initiate search for Leave History records" 
                                                        CssClass="RadButtonStyle" ValidationGroup="valPrimary" Font-Size="9pt" OnClick="btnSearchAttendanceHistory_Click" 
                                                        Skin="Office2010Silver" Width="70px" />
                                                    <telerik:RadButton ID="btnResetAttendanceHistory" runat="server" Text="Reset" ToolTip="Reset filter criterias" 
                                                        CssClass="RadButtonStyle" ValidationGroup="valPrimary" Font-Size="9pt" OnClick="btnResetAttendanceHistory_Click" 
                                                        Skin="Office2010Silver" Width="70px" />    
                                                </td>
                                                <td />
                                            </tr>
                                        </table>
                                    </asp:Panel>

                                    <asp:Panel ID="panAttendanceHistoryGrid" runat="server" style="width: 100%; text-align: left; table-layout: fixed; margin-top: 5px;">
                                        <table border="0" style="width: 100%; text-align: left; table-layout: fixed;">
                                            <tr>
                                                <td style="padding-left: 10px; padding-right: 0px;">
                                                    <telerik:RadGrid ID="gridAttendanceHistory" runat="server"
                                                        AllowSorting="true" AllowMultiRowSelection="true"
                                                        PageSize="10" AutoGenerateColumns="false" Skin="Silk" 
                                                        GridLines="None" Width="100%" Height="" CellSpacing="0" 
                                                        onpageindexchanged="gridAttendanceHistory_PageIndexChanged" 
                                                        onpagesizechanged="gridAttendanceHistory_PageSizeChanged" 
                                                        onsortcommand="gridAttendanceHistory_SortCommand" 
                                                        onitemcommand="gridAttendanceHistory_ItemCommand" 
                                                        onitemdatabound="gridAttendanceHistory_ItemDataBound" 
                                                        Font-Names="Tahoma" Font-Size="9pt" Font-Bold="False" 
                                                        AllowPaging = "true" BorderStyle="Outset" BorderWidth="1px"
                                                        AllowCustomPaging="True" VirtualItemCount="1">
                                                        <ExportSettings ExportOnlyData="true" IgnorePaging="true" OpenInNewWindow="true" FileName="AttendanceHistory" HideStructureColumns="true">
                                                            <Pdf PageHeight="210mm" PageWidth="310mm" PageTitle="Attendance History" DefaultFontFamily="Arial Unicode MS"
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
                                                                <telerik:GridBoundColumn DataField="AutoID" DataType="System.Int32" HeaderText="Auto ID" 
                                                                    ReadOnly="True" SortExpression="AutoID" UniqueName="AutoID">
                                                                    <ColumnValidationSettings>
                                                                        <ModelErrorMessage Text="" />
                                                                    </ColumnValidationSettings>
                                                                    <HeaderStyle Width="80px" Font-Bold="True"></HeaderStyle>
                                                                </telerik:GridBoundColumn>   
                                                                <telerik:GridBoundColumn DataField="DT" HeaderText="Date"
                                                                    DataFormatString="{0:dd-MMM-yyyy}" DataType="System.DateTime" 
                                                                    ReadOnly="True" SortExpression="DT" UniqueName="DT">
                                                                    <ColumnValidationSettings>
                                                                        <ModelErrorMessage Text="" />
                                                                    </ColumnValidationSettings>
                                                                    <HeaderStyle Width="100px" Font-Bold="True"></HeaderStyle>
                                                                    <ItemStyle Font-Bold="true" ForeColor="Purple" />
                                                                </telerik:GridBoundColumn>    
                                                                <telerik:GridBoundColumn DataField="dtIN" HeaderText="Time In"
                                                                    DataFormatString="{0:HH:mm}" DataType="System.DateTime" 
                                                                    ReadOnly="True" SortExpression="dtIN" UniqueName="dtIN">
                                                                    <ColumnValidationSettings>
                                                                        <ModelErrorMessage Text="" />
                                                                    </ColumnValidationSettings>
                                                                    <HeaderStyle Width="70px" Font-Bold="True" HorizontalAlign="Center"></HeaderStyle>
                                                                    <ItemStyle HorizontalAlign="Center" />
                                                                </telerik:GridBoundColumn>   
                                                                <telerik:GridBoundColumn DataField="dtOUT" HeaderText="Time Out"
                                                                    DataFormatString="{0:HH:mm}" DataType="System.DateTime" 
                                                                    ReadOnly="True" SortExpression="dtOUT" UniqueName="dtOUT">
                                                                    <ColumnValidationSettings>
                                                                        <ModelErrorMessage Text="" />
                                                                    </ColumnValidationSettings>
                                                                    <HeaderStyle Width="70px" Font-Bold="True" HorizontalAlign="Center"></HeaderStyle>
                                                                    <ItemStyle HorizontalAlign="Center" />
                                                                </telerik:GridBoundColumn>   
                                                                <telerik:GridBoundColumn DataField="DurationHourString" DataType="System.String" HeaderText="Duration <br /> (HH:mm)"
                                                                    ReadOnly="True" SortExpression="DurationHourString" UniqueName="DurationHourString">
                                                                    <ColumnValidationSettings>
                                                                        <ModelErrorMessage Text="" />
                                                                    </ColumnValidationSettings>
                                                                    <HeaderStyle Width="65px" Font-Bold="True" HorizontalAlign="Center"></HeaderStyle>
                                                                    <ItemStyle HorizontalAlign="Center" />
                                                                </telerik:GridBoundColumn>  

                                                                <telerik:GridBoundColumn DataField="ShiftPatCode" DataType="System.String" HeaderText="Shift <br /> Pattern"
                                                                    ReadOnly="True" SortExpression="ShiftPatCode" UniqueName="ShiftPatCode">
                                                                    <ColumnValidationSettings>
                                                                        <ModelErrorMessage Text="" />
                                                                    </ColumnValidationSettings>
                                                                    <HeaderStyle Width="70px" Font-Bold="True" HorizontalAlign="Center"></HeaderStyle>
                                                                    <ItemStyle HorizontalAlign="Center" />
                                                                </telerik:GridBoundColumn>  
                                                                <telerik:GridBoundColumn DataField="ShiftCode" DataType="System.String" HeaderText="Scheduled <br /> Shift"
                                                                    ReadOnly="True" SortExpression="ShiftCode" UniqueName="ShiftCode">
                                                                    <ColumnValidationSettings>
                                                                        <ModelErrorMessage Text="" />
                                                                    </ColumnValidationSettings>
                                                                    <HeaderStyle Width="75px" Font-Bold="True" HorizontalAlign="Center"></HeaderStyle>
                                                                    <ItemStyle HorizontalAlign="Center" />
                                                                </telerik:GridBoundColumn>  
                                                                <telerik:GridBoundColumn DataField="ActualShiftCode" DataType="System.String" HeaderText="Actual <br /> Shift"
                                                                    ReadOnly="True" SortExpression="ActualShiftCode" UniqueName="ActualShiftCode">
                                                                    <ColumnValidationSettings>
                                                                        <ModelErrorMessage Text="" />
                                                                    </ColumnValidationSettings>
                                                                    <HeaderStyle Width="70px" Font-Bold="True" HorizontalAlign="Center"></HeaderStyle>
                                                                    <ItemStyle HorizontalAlign="Center" />
                                                                </telerik:GridBoundColumn> 
                                                                <telerik:GridBoundColumn DataField="OTType" DataType="System.String" HeaderText="OT Type" 
                                                                    ReadOnly="True" SortExpression="OTType" UniqueName="OTType">
                                                                    <ColumnValidationSettings>
                                                                        <ModelErrorMessage Text="" />
                                                                    </ColumnValidationSettings>
                                                                    <HeaderStyle Width="70px" Font-Bold="True" HorizontalAlign="Center"></HeaderStyle>
                                                                    <ItemStyle HorizontalAlign="Center" />
                                                                </telerik:GridBoundColumn>   
                                                                <telerik:GridBoundColumn DataField="OTStartTime" HeaderText="OT Start <br /> Time"
                                                                    DataFormatString="{0:HH:mm}" DataType="System.DateTime" 
                                                                    ReadOnly="True" SortExpression="OTStartTime" UniqueName="OTStartTime">
                                                                    <ColumnValidationSettings>
                                                                        <ModelErrorMessage Text="" />
                                                                    </ColumnValidationSettings>
                                                                    <HeaderStyle Width="70px" Font-Bold="True" HorizontalAlign="Center"></HeaderStyle>
                                                                    <ItemStyle HorizontalAlign="Center" />
                                                                </telerik:GridBoundColumn>   
                                                                <telerik:GridBoundColumn DataField="OTEndTime" HeaderText="OT End <br /> Time"
                                                                    DataFormatString="{0:HH:mm}" DataType="System.DateTime" 
                                                                    ReadOnly="True" SortExpression="OTEndTime" UniqueName="OTEndTime">
                                                                    <ColumnValidationSettings>
                                                                        <ModelErrorMessage Text="" />
                                                                    </ColumnValidationSettings>
                                                                    <HeaderStyle Width="70px" Font-Bold="True" HorizontalAlign="Center"></HeaderStyle>
                                                                    <ItemStyle HorizontalAlign="Center" />
                                                                </telerik:GridBoundColumn> 
                                                                <telerik:GridBoundColumn DataField="NoPayHours" DataType="System.Int32" HeaderText="No Pay <br /> Hours" 
                                                                    ReadOnly="True" SortExpression="NoPayHours" UniqueName="NoPayHours">
                                                                    <ColumnValidationSettings>
                                                                        <ModelErrorMessage Text="" />
                                                                    </ColumnValidationSettings>
                                                                    <HeaderStyle Width="70px" Font-Bold="True" HorizontalAlign="Center"></HeaderStyle>
                                                                    <ItemStyle HorizontalAlign="Center" />
                                                                </telerik:GridBoundColumn> 
                                                                <telerik:GridBoundColumn DataField="CorrectionCode" DataType="System.String" HeaderText="Correction <br /> Code"
                                                                    ReadOnly="True" SortExpression="CorrectionCode" UniqueName="CorrectionCode">
                                                                    <ColumnValidationSettings>
                                                                        <ModelErrorMessage Text="" />
                                                                    </ColumnValidationSettings>
                                                                    <HeaderStyle Width="80px" Font-Bold="True" HorizontalAlign="Center"></HeaderStyle>
                                                                    <ItemStyle HorizontalAlign="Center" />
                                                                </telerik:GridBoundColumn>
                                                                <telerik:GridBoundColumn DataField="AbsenceReasonCode" DataType="System.String" HeaderText="Absence Reason <br/> Code" 
                                                                    ReadOnly="True" SortExpression="AbsenceReasonCode" UniqueName="AbsenceReasonCode">
                                                                    <ColumnValidationSettings>
                                                                        <ModelErrorMessage Text="" />
                                                                    </ColumnValidationSettings>
                                                                    <HeaderStyle Width="110px" Font-Bold="True" HorizontalAlign="Center"></HeaderStyle>
                                                                    <ItemStyle HorizontalAlign="Center" />
                                                                </telerik:GridBoundColumn>  
                                                                <telerik:GridBoundColumn DataField="LeaveType" DataType="System.String" HeaderText="Leave Type <br/> Code" 
                                                                    ReadOnly="True" SortExpression="LeaveType" UniqueName="LeaveType">
                                                                    <ColumnValidationSettings>
                                                                        <ModelErrorMessage Text="" />
                                                                    </ColumnValidationSettings>
                                                                    <HeaderStyle Width="80px" Font-Bold="True" HorizontalAlign="Center"></HeaderStyle>
                                                                    <ItemStyle HorizontalAlign="Center" />
                                                                </telerik:GridBoundColumn>
                                                                <telerik:GridBoundColumn DataField="DILEntitlement" DataType="System.String" HeaderText="DIL" 
                                                                    ReadOnly="True" SortExpression="DILEntitlement" UniqueName="DILEntitlement">
                                                                    <ColumnValidationSettings>
                                                                        <ModelErrorMessage Text="" />
                                                                    </ColumnValidationSettings>
                                                                    <HeaderStyle Width="70px" Font-Bold="True" HorizontalAlign="Center"></HeaderStyle>
                                                                    <ItemStyle HorizontalAlign="Center" />
                                                                </telerik:GridBoundColumn>
                                                                <telerik:GridBoundColumn DataField="RemarkCode" DataType="System.String" HeaderText="Absent" 
                                                                    ReadOnly="True" SortExpression="RemarkCode" UniqueName="RemarkCode">
                                                                    <ColumnValidationSettings>
                                                                        <ModelErrorMessage Text="" />
                                                                    </ColumnValidationSettings>
                                                                    <HeaderStyle Width="60px" Font-Bold="True" HorizontalAlign="Center"></HeaderStyle>
                                                                    <ItemStyle HorizontalAlign="Center" />
                                                                </telerik:GridBoundColumn>
                                                                <telerik:GridTemplateColumn DataField="IsLastRow" HeaderText="Is Last Row?" 
                                                                    SortExpression="IsLastRow" UniqueName="IsLastRow">
								                                    <HeaderStyle Width="95px" HorizontalAlign="Center" />
								                                    <ItemTemplate>
									                                    <div style="width: 95px; text-align: center;">
										                                    <asp:Label ID="lblIsLastRow" runat="server" Text='<%# Convert.ToBoolean(Eval("IsLastRow")) == true ? "Yes" : "No" %>'></asp:Label>  
									                                    </div>
								                                    </ItemTemplate>
							                                    </telerik:GridTemplateColumn> 
                                                                <telerik:GridBoundColumn DataField="LastUpdateUser" DataType="System.String" HeaderText="Updated By" 
                                                                    ReadOnly="True" SortExpression="LastUpdateUser" UniqueName="LastUpdateUser">
                                                                    <ColumnValidationSettings>
                                                                        <ModelErrorMessage Text="" />
                                                                    </ColumnValidationSettings>
                                                                    <HeaderStyle Width="130px" Font-Bold="True"></HeaderStyle>
                                                                </telerik:GridBoundColumn>
                                                                <telerik:GridBoundColumn DataField="LastUpdateTime" SortExpression="LastUpdateTime" UniqueName="LastUpdateTime"
                                                                    HeaderText="Updated Date" DataFormatString="{0:dd-MMM-yyyy h:mm tt}" DataType="System.DateTime" 
                                                                    FilterControlAltText="Filter Modified Date column" ReadOnly="True">
                                                                    <HeaderStyle Width="150px" Font-Bold="True" Font-Names="Tahoma"></HeaderStyle>
                                                                </telerik:GridBoundColumn>
                                                            </Columns>
                                                        </MasterTableView>
                                                        <ClientSettings AllowColumnsReorder="False" EnableRowHoverStyle="true">
                                                            <Selecting AllowRowSelect="True" UseClientSelectColumnOnly="True" />
                                                            <Scrolling AllowScroll="true" UseStaticHeaders="true" SaveScrollPosition="true" ScrollHeight="" FrozenColumnsCount="2" />
			                                                <Resizing AllowColumnResize="true" />   
                                                        </ClientSettings>
                                                        <HeaderStyle Font-Bold="True" Font-Size="7.5pt" />
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

                                <telerik:RadPageView ID="TrainingHistoryView" runat="server">
                                    <asp:Panel ID="panTrainingHistory" runat="server" style="width: 100%; text-align: left; table-layout: fixed; margin-top: 5px;">
                                        <table border="0" style="width: 100%; text-align: left; table-layout: fixed;">
                                            <tr>
                                                <td style="padding-left: 10px; padding-right: 0px;">
                                                    <telerik:RadGrid ID="gridTraining" runat="server"
                                                        AllowSorting="true" AllowMultiRowSelection="false"
                                                        PageSize="10" AutoGenerateColumns="false" Skin="Silk" 
                                                        GridLines="None" Width="100%" Height="" CellSpacing="0"
                                                        onpageindexchanged="gridTraining_PageIndexChanged" 
                                                        onpagesizechanged="gridTraining_PageSizeChanged" 
                                                        onsortcommand="gridTraining_SortCommand" 
                                                        onitemcommand="gridTraining_ItemCommand" 
                                                        onitemdatabound="gridTraining_ItemDataBound" 
                                                        Font-Names="Tahoma" Font-Size="9pt" Font-Bold="False" BorderStyle="Outset" BorderWidth="1px" 
                                                        AllowCustomPaging="True" VirtualItemCount="1" AllowPaging="True">
                                                        <ExportSettings ExportOnlyData="true" IgnorePaging="true" OpenInNewWindow="true" FileName="Training_History" HideStructureColumns="true">
                                                            <Pdf PageHeight="210mm" PageWidth="310mm" PageTitle="Training History" DefaultFontFamily="Arial Unicode MS"
                                                            PageBottomMargin="20mm" PageTopMargin="20mm" PageLeftMargin="20mm" PageRightMargin="20mm" PaperSize="A4" AllowPrinting="true" />
                                                        </ExportSettings>
                                                        <AlternatingItemStyle Font-Names="Tahoma" Font-Size="9pt" Wrap="True" />
                                                        <MasterTableView DataKeyNames="TrainingRecordID" ClientDataKeyNames="TrainingRecordID" 
                                                            NoMasterRecordsText="No training record found." 
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
                                                                <telerik:GridButtonColumn ButtonType="ImageButton" CommandName="Select" HeaderText="" CommandArgument="PrintButton" Visible="false" 
                                                                    UniqueName="PrintButton" ImageUrl="~/Images/printer.png" HeaderTooltip="View and print Employee Training Report">
                                                                    <HeaderStyle Font-Bold="True" Width="40px"></HeaderStyle>
                                                                    <ItemStyle Font-Bold="true" ForeColor="Blue" HorizontalAlign="Center" Font-Size="9pt" />
				                                                </telerik:GridButtonColumn>       
                                                                <telerik:GridButtonColumn ButtonType="LinkButton" CommandName="Select" Text="Select" UniqueName="SelectLinkButton" Visible="false">
                                                                    <HeaderStyle Width="50px" HorizontalAlign="Center" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
                                                                    <ItemStyle HorizontalAlign="Center" Font-Bold="true" Font-Size="9pt" ForeColor="Blue" />
					                                            </telerik:GridButtonColumn> 
                                                                <telerik:GridBoundColumn DataField="TraineeID" DataType="System.Int32" HeaderText="Emp. No./Contr. ID" Visible="false"
                                                                    ReadOnly="True" SortExpression="TraineeID" UniqueName="TraineeID">
                                                                    <ColumnValidationSettings>
                                                                        <ModelErrorMessage Text="" />
                                                                    </ColumnValidationSettings>
                                                                    <HeaderStyle Width="127px" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma"></HeaderStyle>
                                                                    <ItemStyle HorizontalAlign="Right" />
                                                                </telerik:GridBoundColumn>     
                                                                <telerik:GridTemplateColumn DataField="TraineeName" HeaderText="Name" Visible="false" 
                                                                    SortExpression="TraineeName" UniqueName="TraineeName">
								                                    <HeaderStyle Width="230px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								                                    <ItemTemplate>
									                                    <div class="columnEllipsis" style="width: 220px; text-align: left;">
										                                    <asp:Literal ID="litEmpName" runat="server" Text='<%# Eval("TraineeName") %>' />
									                                    </div>
								                                    </ItemTemplate>
							                                    </telerik:GridTemplateColumn>  
                                                                <telerik:GridTemplateColumn DataField="CostCenterFullName" HeaderText="Cost Center" Visible="false" 
                                                                    SortExpression="CostCenterFullName" UniqueName="CostCenterFullName">
								                                    <HeaderStyle Width="250px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								                                    <ItemTemplate>
									                                    <div class="columnEllipsis" style="width: 240px; text-align: left;">
										                                    <asp:Literal ID="litCostCenterFullName" runat="server" Text='<%# Eval("CostCenterFullName") %>' />
									                                    </div>
								                                    </ItemTemplate>
							                                    </telerik:GridTemplateColumn>
                                                                <telerik:GridBoundColumn DataField="CostCenter" DataType="System.String" HeaderText="Cost Center" SortExpression="CostCenter" 
                                                                    UniqueName="CostCenter" Visible="false">
                                                                    <ColumnValidationSettings>
                                                                        <ModelErrorMessage Text="" />
                                                                    </ColumnValidationSettings>
                                                                    <HeaderStyle Width="100px" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma"></HeaderStyle>
                                                                </telerik:GridBoundColumn>
                                                                <telerik:GridTemplateColumn DataField="CostCenterName" HeaderText="Cost Center Name" 
                                                                    SortExpression="CostCenterName" UniqueName="CostCenterName" Visible="false">
								                                    <HeaderStyle Width="250px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								                                    <ItemTemplate>
									                                    <div class="columnEllipsis" style="width: 250px; text-align: left;">                                                
										                                    <asp:Literal ID="litCostCenterName" runat="server" Text='<%# Eval("CostCenterName") %>' />
									                                    </div>
								                                    </ItemTemplate>
							                                    </telerik:GridTemplateColumn>	
                                                                <telerik:GridTemplateColumn DataField="CourseTitle" HeaderText="Course Title" 
                                                                    SortExpression="CourseTitle" UniqueName="CourseTitle">
								                                    <HeaderStyle Width="350px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								                                    <ItemTemplate>
									                                    <div class="columnEllipsis" style="width: 340px; text-align: left;">                                                
										                                    <asp:Literal ID="litCourseTitle" runat="server" Text='<%# Eval("CourseTitle") %>' />
									                                    </div>
								                                    </ItemTemplate>
                                                                    <ItemStyle Font-Bold="true" ForeColor="Purple" />
							                                    </telerik:GridTemplateColumn>
                                                                <telerik:GridBoundColumn DataField="FromDate" HeaderText="From"
                                                                    DataFormatString="{0:dd-MMM-yyyy}" DataType="System.DateTime" 
                                                                    ReadOnly="True" SortExpression="FromDate" UniqueName="FromDate">
                                                                    <ColumnValidationSettings>
                                                                        <ModelErrorMessage Text="" />
                                                                    </ColumnValidationSettings>
                                                                    <HeaderStyle Width="90px" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma"></HeaderStyle>
                                                                </telerik:GridBoundColumn>    
                                                                <telerik:GridBoundColumn DataField="ToDate" HeaderText="To"
                                                                    DataFormatString="{0:dd-MMM-yyyy}" DataType="System.DateTime" 
                                                                    ReadOnly="True" SortExpression="ToDate" UniqueName="ToDate">
                                                                    <ColumnValidationSettings>
                                                                        <ModelErrorMessage Text="" />
                                                                    </ColumnValidationSettings>
                                                                    <HeaderStyle Width="90px" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma"></HeaderStyle>
                                                                </telerik:GridBoundColumn> 
                                                                <telerik:GridTemplateColumn DataField="DurationDetails" HeaderText="Duration" 
                                                                    SortExpression="DurationDetails" UniqueName="DurationDetails">
								                                    <HeaderStyle Width="90px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								                                    <ItemTemplate>
									                                    <div class="columnEllipsis" style="width: 80px; text-align: left;">
										                                    <asp:Literal ID="litDurationDetails" runat="server" Text='<%# Eval("DurationDetails") %>' />
									                                    </div>
								                                    </ItemTemplate>
							                                    </telerik:GridTemplateColumn> 

                                                                <telerik:GridTemplateColumn DataField="TrainingProviderName" HeaderText="Training Provider" 
                                                                    SortExpression="TrainingProviderName" UniqueName="TrainingProviderName">
								                                    <HeaderStyle Width="150px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								                                    <ItemTemplate>
									                                    <div class="columnEllipsis" style="width: 140px; text-align: left;">
										                                    <asp:Literal ID="litTrainingProviderName" runat="server" Text='<%# Eval("TrainingProviderName") %>' />
									                                    </div>
								                                    </ItemTemplate>
							                                    </telerik:GridTemplateColumn> 
                                                                <telerik:GridTemplateColumn DataField="QualificationDesc" HeaderText="Qualification" 
                                                                    SortExpression="QualificationDesc" UniqueName="QualificationDesc">
								                                    <HeaderStyle Width="150px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								                                    <ItemTemplate>
									                                    <div class="columnEllipsis" style="width: 140px; text-align: left;">
										                                    <asp:Literal ID="litQualificationDesc" runat="server" Text='<%# Eval("QualificationDesc") %>' />
									                                    </div>
								                                    </ItemTemplate>
							                                    </telerik:GridTemplateColumn>  
                                                                <telerik:GridTemplateColumn DataField="TypeOfTrainingDesc" HeaderText="Type of Training" 
                                                                    SortExpression="TypeOfTrainingDesc" UniqueName="TypeOfTrainingDesc">
								                                    <HeaderStyle Width="120px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								                                    <ItemTemplate>
									                                    <div class="columnEllipsis" style="width: 110px; text-align: left;">
										                                    <asp:Literal ID="litTypeOfTrainingDesc" runat="server" Text='<%# Eval("TypeOfTrainingDesc") %>' />
									                                    </div>
								                                    </ItemTemplate>
							                                    </telerik:GridTemplateColumn>  
                                                                <telerik:GridBoundColumn DataField="Cost" DataType="System.Double" ReadOnly="True" SortExpression="Cost" Visible="false" 
                                                                    UniqueName="Cost" HeaderText="Cost (BD)" DataFormatString="{0:F3}">
                                                                    <ColumnValidationSettings>
                                                                        <ModelErrorMessage Text="" />
                                                                    </ColumnValidationSettings>
                                                                    <HeaderStyle Width="90px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
                                                                    <ItemStyle HorizontalAlign="Left" Font-Bold="true" ForeColor="OrangeRed" />
                                                                </telerik:GridBoundColumn>
                                                                <telerik:GridBoundColumn DataField="StatusCode" 
                                                                    DataType="System.String" Visible="false" 
                                                                    ReadOnly="True" SortExpression="StatusCode" UniqueName="StatusCode">
                                                                    <ColumnValidationSettings>
                                                                        <ModelErrorMessage Text="" />
                                                                    </ColumnValidationSettings>
                                                                </telerik:GridBoundColumn>
                                                                <telerik:GridTemplateColumn DataField="StatusDesc" HeaderText="Status" 
                                                                    SortExpression="StatusDesc" UniqueName="StatusDesc">
								                                    <HeaderStyle Width="110px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								                                    <ItemTemplate>
									                                    <div class="columnEllipsis" style="width: 100px; text-align: left;">
										                                    <asp:Literal ID="litStatusDesc" runat="server" Text='<%# Eval("StatusDesc") %>' />
									                                    </div>
								                                    </ItemTemplate>
							                                    </telerik:GridTemplateColumn> 
                                                                <telerik:GridBoundColumn DataField="CreatedByEmpNo" DataType="System.Int32" Visible="false" 
                                                                    ReadOnly="True" SortExpression="CreatedByEmpNo" UniqueName="CreatedByEmpNo">
                                                                    <ColumnValidationSettings>
                                                                        <ModelErrorMessage Text="" />
                                                                    </ColumnValidationSettings>
                                                                </telerik:GridBoundColumn>
                                                                <telerik:GridTemplateColumn DataField="CreatedByFullName" HeaderText="Created By" SortExpression="CreatedByFullName" UniqueName="CreatedByFullName">
								                                    <HeaderStyle Width="300px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								                                    <ItemTemplate>
									                                    <div class="columnEllipsis" style="width: 290px; text-align: left;">
										                                    <asp:Literal ID="litCreatedByFullName" runat="server" Text='<%# Eval("CreatedByFullName") %>' />
									                                    </div>
								                                    </ItemTemplate>
							                                    </telerik:GridTemplateColumn>   
                                                                <telerik:GridBoundColumn DataField="CreatedDate" HeaderText="Created Date"
                                                                    DataFormatString="{0:dd-MMM-yyyy h:mm tt}" DataType="System.DateTime" 
                                                                    ReadOnly="True" SortExpression="CreatedDate" UniqueName="CreatedDate">
                                                                    <ColumnValidationSettings>
                                                                        <ModelErrorMessage Text="" />
                                                                    </ColumnValidationSettings>
                                                                    <HeaderStyle Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" Width="150px"></HeaderStyle>
                                                                </telerik:GridBoundColumn>                                    
                                                                <telerik:GridTemplateColumn DataField="LastUpdateFullName" HeaderText="Last Modified By" SortExpression="LastUpdateFullName" 
                                                                    UniqueName="LastUpdateFullName" Visible="false">
								                                    <HeaderStyle Width="250px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								                                    <ItemTemplate>
									                                    <div class="columnEllipsis" style="width: 240px; text-align: left;">
										                                    <asp:Literal ID="litLastModifiedBy" runat="server" Text='<%# Eval("LastUpdateFullName") %>' />
									                                    </div>
								                                    </ItemTemplate>
							                                    </telerik:GridTemplateColumn>   
                                                                <telerik:GridBoundColumn DataField="LastUpdateTime" HeaderText="Last Modified Date"
                                                                    DataFormatString="{0:dd-MMM-yyyy h:mm tt}" DataType="System.DateTime" Visible="false" 
                                                                    FilterControlAltText="Filter Modified Date column" ReadOnly="True" SortExpression="LastUpdateTime" UniqueName="LastUpdateTime">
                                                                    <ColumnValidationSettings>
                                                                        <ModelErrorMessage Text="" />
                                                                    </ColumnValidationSettings>
                                                                    <HeaderStyle Width="150px" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma"></HeaderStyle>
                                                                </telerik:GridBoundColumn>     
                                                            </Columns>
                                                        </MasterTableView>
                                                        <ClientSettings AllowColumnsReorder="False" EnableRowHoverStyle="true" EnablePostBackOnRowClick="false">
                                                            <Selecting AllowRowSelect="True" UseClientSelectColumnOnly="true" EnableDragToSelectRows="true" />
                                                            <Scrolling AllowScroll="true" UseStaticHeaders="true" SaveScrollPosition="true" ScrollHeight="" FrozenColumnsCount="1" />
			                                                <Resizing AllowColumnResize="true" />   
                                                        </ClientSettings>
                                                        <HeaderStyle Font-Bold="True" Font-Size="7.5pt" />
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

                                <telerik:RadPageView ID="DILEntitlementView" runat="server">
                                    <asp:Panel ID="panDILEntitlement" runat="server" style="width: 100%; text-align: left; table-layout: fixed; margin-top: 5px; margin-left: 5px; margin-right: 5px;">
                                        <telerik:RadTabStrip ID="tabDIL" runat="server" SelectedIndex="0"   
                                            MultiPageID="multiPageDIL" ReorderTabsOnSelect="True" 
                                            CausesValidation="False" ontabclick="tabDIL_TabClick" 
                                            style="padding-top: 0px; padding-left: 0px; padding-right: 0px;" 
                                            Skin="Office2010Silver">
                                            <Tabs>
                                                <telerik:RadTab Text="Approved DIL" Font-Size="9pt" Font-Bold="True" Selected="True" Value="ApprovedDIL">
                                                </telerik:RadTab>                                     
                                                <telerik:RadTab Text="Entitled to DIL - Inactive" Font-Size="9pt" Font-Bold="True" Value="InactiveDIL">
                                                </telerik:RadTab> 
                                            </Tabs>
                                        </telerik:RadTabStrip>                                                                                
                                    </asp:Panel>    
                                </telerik:RadPageView>

                                <telerik:RadPageView ID="ShiftPatternView" runat="server">
                                    <asp:Panel ID="patShiftPatternGrid" runat="server" style="width: 100%; text-align: left; table-layout: fixed; margin-top: 5px;">
                                        <table border="0" style="width: 100%; text-align: left; table-layout: fixed;">
                                            <tr>
                                                <td style="padding-left: 10px; padding-right: 0px;">
                                                    <telerik:RadGrid ID="gridShiftPattern" runat="server"
                                                        AllowSorting="true" AllowMultiRowSelection="true"
                                                        PageSize="10" AutoGenerateColumns="false" Skin="Silk" 
                                                        GridLines="None" Width="100%" Height="" CellSpacing="0"
                                                        onpageindexchanged="gridShiftPattern_PageIndexChanged" 
                                                        onpagesizechanged="gridShiftPattern_PageSizeChanged" 
                                                        onsortcommand="gridShiftPattern_SortCommand" 
                                                        onitemcommand="gridShiftPattern_ItemCommand" 
                                                        onitemdatabound="gridShiftPattern_ItemDataBound" 
                                                        Font-Names="Tahoma" Font-Size="9pt" Font-Bold="False" 
                                                        AllowPaging = "true" BorderStyle="Outset" BorderWidth="1px">
                                                        <ExportSettings ExportOnlyData="true" IgnorePaging="true" OpenInNewWindow="true" FileName="ShiftPatternInfo" HideStructureColumns="true">
                                                            <Pdf PageHeight="210mm" PageWidth="310mm" PageTitle="Shift Pattern Information" DefaultFontFamily="Arial Unicode MS"
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
                                                                <telerik:GridBoundColumn DataField="ShiftPatCode" DataType="System.String" HeaderText="Shift Pat. Code" 
                                                                    ReadOnly="True" SortExpression="ShiftPatCode" UniqueName="ShiftPatCode">
                                                                    <ColumnValidationSettings>
                                                                        <ModelErrorMessage Text="" />
                                                                    </ColumnValidationSettings>
                                                                    <HeaderStyle Width="110px" Font-Bold="True"></HeaderStyle>
                                                                    <ItemStyle ForeColor="Purple" Font-Bold="true" />
                                                                </telerik:GridBoundColumn>
                                                                <telerik:GridTemplateColumn DataField="ShiftCodeArray" HeaderText="Shift Pattern Detail" 
                                                                    SortExpression="ShiftCodeArray" UniqueName="ShiftCodeArray">
								                                    <HeaderStyle Width="200px" HorizontalAlign="Left" />
								                                    <ItemTemplate>
									                                    <div class="columnEllipsis" style="width: 230px; text-align: left;">
										                                    <asp:Literal ID="litShiftCodeArray" runat="server" Text='<%# Eval("ShiftCodeArray") %>' />
									                                    </div>
								                                    </ItemTemplate>
							                                    </telerik:GridTemplateColumn>   
                                                                <telerik:GridBoundColumn DataField="ShiftPointer" DataType="System.Int32" HeaderText="Shift Pointer" 
                                                                    ReadOnly="True" SortExpression="ShiftPointer" UniqueName="ShiftPointer">
                                                                    <ColumnValidationSettings>
                                                                        <ModelErrorMessage Text="" />
                                                                    </ColumnValidationSettings>
                                                                    <HeaderStyle Width="120px" Font-Bold="True" HorizontalAlign="Center" />
                                                                    <ItemStyle HorizontalAlign="Center" />
                                                                </telerik:GridBoundColumn> 
                                                                <telerik:GridTemplateColumn DataField="WorkingCostCenterFullName" HeaderText="Working Cost Center" 
                                                                    SortExpression="WorkingCostCenterFullName" UniqueName="WorkingCostCenterFullName">
								                                    <HeaderStyle Width="220px" HorizontalAlign="Left" />
								                                    <ItemTemplate>
									                                    <div class="columnEllipsis" style="width: 250px; text-align: left;">
										                                    <asp:Literal ID="litWorkingCostCenterFullName" runat="server" Text='<%# Eval("WorkingCostCenterFullName") %>' />
									                                    </div>
								                                    </ItemTemplate>
							                                    </telerik:GridTemplateColumn> 
                                                                <telerik:GridBoundColumn DataField="LastUpdateUser" DataType="System.String" HeaderText="Last Updated By" 
                                                                    ReadOnly="True" SortExpression="LastUpdateUser" UniqueName="LastUpdateUser">
                                                                    <ColumnValidationSettings>
                                                                        <ModelErrorMessage Text="" />
                                                                    </ColumnValidationSettings>
                                                                    <HeaderStyle Width="150px" Font-Bold="True"></HeaderStyle>
                                                                </telerik:GridBoundColumn>
                                                                <telerik:GridBoundColumn DataField="LastUpdateTime" SortExpression="LastUpdateTime" UniqueName="LastUpdateTime"
                                                                    HeaderText="Last Updated Date" DataFormatString="{0:dd-MMM-yyyy h:mm tt}" DataType="System.DateTime" 
                                                                    FilterControlAltText="Filter Modified Date column" ReadOnly="True">
                                                                    <HeaderStyle Font-Bold="True" Font-Names="Tahoma"></HeaderStyle>
                                                                </telerik:GridBoundColumn>                                                                
                                                            </Columns>
                                                        </MasterTableView>
                                                        <ClientSettings AllowColumnsReorder="False" EnableRowHoverStyle="true">
                                                            <Selecting AllowRowSelect="True" UseClientSelectColumnOnly="True" />
                                                            <Scrolling AllowScroll="true" UseStaticHeaders="true" SaveScrollPosition="true" ScrollHeight="" />
			                                                <Resizing AllowColumnResize="true" />   
                                                        </ClientSettings>
                                                        <HeaderStyle Font-Bold="True" Font-Size="7.5pt" />
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

                                <telerik:RadPageView ID="DependentInfoView" runat="server">
                                    <asp:Panel ID="panDependentInfo" runat="server" style="width: 100%; text-align: left; table-layout: fixed; margin-top: 5px;">
                                        <table border="0" style="width: 100%; text-align: left; table-layout: fixed;">
                                            <tr>
                                                <td style="padding-left: 10px; padding-right: 0px;">
                                                    <telerik:RadGrid ID="gridDependentInfo" runat="server"
                                                        AllowSorting="true" AllowMultiRowSelection="true"
                                                        PageSize="10" AutoGenerateColumns="false" Skin="Silk" 
                                                        GridLines="None" Width="100%" Height="" CellSpacing="0"
                                                        onpageindexchanged="gridDependentInfo_PageIndexChanged" 
                                                        onpagesizechanged="gridDependentInfo_PageSizeChanged" 
                                                        onsortcommand="gridDependentInfo_SortCommand" 
                                                        onitemcommand="gridDependentInfo_ItemCommand" 
                                                        onitemdatabound="gridDependentInfo_ItemDataBound" 
                                                        Font-Names="Tahoma" Font-Size="9pt" Font-Bold="False" 
                                                        AllowPaging = "true" BorderStyle="Outset" BorderWidth="1px">
                                                        <ExportSettings ExportOnlyData="true" IgnorePaging="true" OpenInNewWindow="true" FileName="DependentInfo" HideStructureColumns="true">
                                                            <Pdf PageHeight="210mm" PageWidth="310mm" PageTitle="Dependent Information" DefaultFontFamily="Arial Unicode MS"
                                                            PageBottomMargin="20mm" PageTopMargin="20mm" PageLeftMargin="20mm" PageRightMargin="20mm" PaperSize="A4" AllowPrinting="true" />
                                                        </ExportSettings>
                                                        <AlternatingItemStyle Font-Names="Tahoma" Font-Size="9pt" Wrap="True" />
                                                        <MasterTableView DataKeyNames="DependentNo" ClientDataKeyNames="DependentNo" 
                                                            NoMasterRecordsText="No dependent records found or you do not have acccess to view the data." 
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
                                                                <telerik:GridBoundColumn DataField="DependentNo" DataType="System.Double" HeaderText="Dependent No." 
                                                                    ReadOnly="True" SortExpression="DependentNo" UniqueName="DependentNo">
                                                                    <ColumnValidationSettings>
                                                                        <ModelErrorMessage Text="" />
                                                                    </ColumnValidationSettings>
                                                                    <HeaderStyle Width="100px" Font-Bold="True" />
                                                                </telerik:GridBoundColumn> 
                                                                <telerik:GridTemplateColumn DataField="DependentName" HeaderText="Dependent Name" 
                                                                    SortExpression="DependentName" UniqueName="DependentName">
								                                    <HeaderStyle Width="250px" HorizontalAlign="Left" Font-Bold="true" />
                                                                    <ItemStyle ForeColor="Purple" Font-Bold="true" />
								                                    <ItemTemplate>
									                                    <div class="columnEllipsis" style="width: 240px; text-align: left;">
										                                    <asp:Literal ID="litDependentName" runat="server" Text='<%# Eval("DependentName") %>' />
									                                    </div>
								                                    </ItemTemplate>
							                                    </telerik:GridTemplateColumn> 
                                                                <telerik:GridBoundColumn DataField="Relationship" DataType="System.String" HeaderText="Relationship" 
                                                                    ReadOnly="True" SortExpression="Relationship" UniqueName="Relationship">
                                                                    <ColumnValidationSettings>
                                                                        <ModelErrorMessage Text="" />
                                                                    </ColumnValidationSettings>
                                                                    <HeaderStyle Width="140px" Font-Bold="True"></HeaderStyle>
                                                                </telerik:GridBoundColumn>
                                                                <telerik:GridBoundColumn DataField="Sex" DataType="System.String" HeaderText="Sex" 
                                                                    ReadOnly="True" SortExpression="Sex" UniqueName="Sex">
                                                                    <ColumnValidationSettings>
                                                                        <ModelErrorMessage Text="" />
                                                                    </ColumnValidationSettings>
                                                                    <HeaderStyle Width="50px" Font-Bold="True" HorizontalAlign="Center" />
                                                                    <ItemStyle HorizontalAlign="Center" />
                                                                </telerik:GridBoundColumn>
                                                                <telerik:GridBoundColumn DataField="DOB" SortExpression="DOB" UniqueName="DOB"
                                                                    HeaderText="Birth Date" DataFormatString="{0:dd-MMM-yyyy}" DataType="System.DateTime" 
                                                                    FilterControlAltText="Filter Birth Date column" ReadOnly="True">
                                                                    <HeaderStyle Width="90" Font-Bold="True" Font-Names="Tahoma"></HeaderStyle>
                                                                </telerik:GridBoundColumn> 
                                                                <telerik:GridBoundColumn DataField="CPRNo" DataType="System.String" HeaderText="CPR No." 
                                                                    ReadOnly="True" SortExpression="CPRNo" UniqueName="CPRNo">
                                                                    <ColumnValidationSettings>
                                                                        <ModelErrorMessage Text="" />
                                                                    </ColumnValidationSettings>
                                                                    <HeaderStyle Width="90px" Font-Bold="True" />
                                                                </telerik:GridBoundColumn>     
                                                                <telerik:GridBoundColumn DataField="CPRExpDate" SortExpression="CPRExpDate" UniqueName="CPRExpDate"
                                                                    HeaderText="CPR Expiry" DataFormatString="{0:dd-MMM-yyyy}" DataType="System.DateTime" 
                                                                    FilterControlAltText="Filter CPR Expiry Date column" ReadOnly="True">
                                                                    <HeaderStyle Width="90" Font-Bold="True" Font-Names="Tahoma"></HeaderStyle>
                                                                </telerik:GridBoundColumn>                                                    
                                                                <telerik:GridBoundColumn DataField="ResPermitExpDate" SortExpression="ResPermitExpDate" UniqueName="ResPermitExpDate"
                                                                    HeaderText="Resident Permit Expiry" DataFormatString="{0:dd-MMM-yyyy}" DataType="System.DateTime" 
                                                                    FilterControlAltText="Filter Resident Permit Expiry Date column" ReadOnly="True">
                                                                    <HeaderStyle Font-Bold="True" Font-Names="Tahoma"></HeaderStyle>
                                                                </telerik:GridBoundColumn>   
                                                            </Columns>
                                                        </MasterTableView>
                                                        <ClientSettings AllowColumnsReorder="False" EnableRowHoverStyle="true">
                                                            <Selecting AllowRowSelect="True" UseClientSelectColumnOnly="True" />
                                                            <Scrolling AllowScroll="true" UseStaticHeaders="true" SaveScrollPosition="true" ScrollHeight="" />
			                                                <Resizing AllowColumnResize="true" />   
                                                        </ClientSettings>
                                                        <HeaderStyle Font-Bold="True" Font-Size="7.5pt" />
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

                            <telerik:RadMultiPage ID="multiPageDIL" runat="server" SelectedIndex="0" Width="99%" Visible="false" BorderColor="Silver" BorderStyle="None" BorderWidth="1px"                              
                                style="margin-top: 0px; margin-left: 5px; padding-right: 0px; margin-right: 0px;">
                                <telerik:RadPageView ID="ApprovedDILView" runat="server">
                                    <asp:Panel ID="panApprovedDIL" runat="server" Visible="true" style="width: 100%; text-align: left; table-layout: fixed; margin-top: 5px;">
                                        <table border="0" style="width: 100%; text-align: left; table-layout: fixed;">
                                            <tr>
                                                <td style="padding-left: 0px; padding-right: 0px;">
                                                    <telerik:RadGrid ID="gridApprovedDIL" runat="server"
                                                        AllowSorting="true" AllowMultiRowSelection="true"
                                                        PageSize="10" AutoGenerateColumns="false" Skin="Silk" 
                                                        GridLines="None" Width="100%" Height="" CellSpacing="0"
                                                        onpageindexchanged="gridApprovedDIL_PageIndexChanged" 
                                                        onpagesizechanged="gridApprovedDIL_PageSizeChanged" 
                                                        onsortcommand="gridApprovedDIL_SortCommand" 
                                                        onitemcommand="gridApprovedDIL_ItemCommand" 
                                                        onitemdatabound="gridApprovedDIL_ItemDataBound" 
                                                        Font-Names="Tahoma" Font-Size="9pt" Font-Bold="False" 
                                                        AllowPaging = "true" BorderStyle="Outset" BorderWidth="1px">
                                                        <ExportSettings ExportOnlyData="true" IgnorePaging="true" OpenInNewWindow="true" FileName="ApprovedDILEntitlements" HideStructureColumns="true">
                                                            <Pdf PageHeight="210mm" PageWidth="310mm" PageTitle="Approved DIL Entitlements" DefaultFontFamily="Arial Unicode MS"
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
                                                                <telerik:GridBoundColumn DataField="EntitlementDate" HeaderText="Entitlement Date"
                                                                    DataFormatString="{0:dd-MMM-yyyy}" DataType="System.DateTime" 
                                                                    ReadOnly="True" SortExpression="EntitlementDate" UniqueName="EntitlementDate">
                                                                    <ColumnValidationSettings>
                                                                        <ModelErrorMessage Text="" />
                                                                    </ColumnValidationSettings>
                                                                    <HeaderStyle Width="140px" Font-Bold="True"></HeaderStyle>
                                                                    <ItemStyle Font-Bold="true" ForeColor="Purple" />
                                                                </telerik:GridBoundColumn>   
                                                                <telerik:GridBoundColumn DataField="DILCode" DataType="System.String" HeaderText="DIL Code" 
                                                                    ReadOnly="True" SortExpression="DILCode" UniqueName="DILCode">
                                                                    <ColumnValidationSettings>
                                                                        <ModelErrorMessage Text="" />
                                                                    </ColumnValidationSettings>
                                                                    <HeaderStyle Width="100px" Font-Bold="True" />
                                                                </telerik:GridBoundColumn> 
                                                                <telerik:GridBoundColumn DataField="Remarks" DataType="System.String" HeaderText="Remarks" 
                                                                    ReadOnly="True" SortExpression="Remarks" UniqueName="Remarks">
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
                                                        <HeaderStyle Font-Bold="True" Font-Size="7.5pt" />
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

                                <telerik:RadPageView ID="InactiveDILView" runat="server">
                                    <asp:Panel ID="panInactiveDIL" runat="server" Visible="false" style="width: 100%; text-align: left; table-layout: fixed; margin-top: 5px;">
                                        <table border="0" style="width: 100%; text-align: left; table-layout: fixed;">
                                            <tr>
                                                <td style="padding-left: 0px; padding-right: 0px;">
                                                    <telerik:RadGrid ID="gridInactiveDIL" runat="server"
                                                        AllowSorting="true" AllowMultiRowSelection="true"
                                                        PageSize="10" AutoGenerateColumns="false" Skin="Silk" 
                                                        GridLines="None" Width="100%" Height="" CellSpacing="0"
                                                        onpageindexchanged="gridInactiveDIL_PageIndexChanged" 
                                                        onpagesizechanged="gridInactiveDIL_PageSizeChanged" 
                                                        onsortcommand="gridInactiveDIL_SortCommand" 
                                                        onitemcommand="gridInactiveDIL_ItemCommand" 
                                                        onitemdatabound="gridInactiveDIL_ItemDataBound" 
                                                        Font-Names="Tahoma" Font-Size="9pt" Font-Bold="False" 
                                                        AllowPaging = "true" BorderStyle="Outset" BorderWidth="1px">
                                                        <ExportSettings ExportOnlyData="true" IgnorePaging="true" OpenInNewWindow="true" FileName="InactiveDIL" HideStructureColumns="true">
                                                            <Pdf PageHeight="210mm" PageWidth="310mm" PageTitle="Inactive DIL Entitlements" DefaultFontFamily="Arial Unicode MS"
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
                                                                <telerik:GridBoundColumn DataField="EntitlementDate" HeaderText="Entitlement Date"
                                                                    DataFormatString="{0:dd-MMM-yyyy}" DataType="System.DateTime" 
                                                                    ReadOnly="True" SortExpression="EntitlementDate" UniqueName="EntitlementDate">
                                                                    <ColumnValidationSettings>
                                                                        <ModelErrorMessage Text="" />
                                                                    </ColumnValidationSettings>
                                                                    <HeaderStyle Width="140px" Font-Bold="True"></HeaderStyle>
                                                                    <ItemStyle Font-Bold="true" ForeColor="Purple" />
                                                                </telerik:GridBoundColumn>   
                                                                <telerik:GridBoundColumn DataField="DILCode" DataType="System.String" HeaderText="DIL Code" 
                                                                    ReadOnly="True" SortExpression="DILCode" UniqueName="DILCode">
                                                                    <ColumnValidationSettings>
                                                                        <ModelErrorMessage Text="" />
                                                                    </ColumnValidationSettings>
                                                                    <HeaderStyle Width="100px" Font-Bold="True" />
                                                                </telerik:GridBoundColumn> 
                                                                <telerik:GridBoundColumn DataField="Remarks" DataType="System.String" HeaderText="Remarks" 
                                                                    ReadOnly="True" SortExpression="Remarks" UniqueName="Remarks">
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
                                                        <HeaderStyle Font-Bold="True" Font-Size="7.5pt" />
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
                <telerik:AjaxSetting AjaxControlID="btnSearchSwipeHistory">
				    <UpdatedControls>                        
                        <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
					    <%--<telerik:AjaxUpdatedControl ControlID="panSwipeHistoryGrid" LoadingPanelID="loadingPanel" />  --%>
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>   
                <telerik:AjaxSetting AjaxControlID="btnSearchAbsence">
				    <UpdatedControls>  
                        <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />                        
					    <%--<telerik:AjaxUpdatedControl ControlID="panAbsenceHistory" LoadingPanelID="loadingPanel" />  --%>
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>  
                <telerik:AjaxSetting AjaxControlID="btnSearchLeaveHistory">
				    <UpdatedControls>  
                        <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />                        
					    <%--<telerik:AjaxUpdatedControl ControlID="panLeaveHistoryGrid" LoadingPanelID="loadingPanel" />  --%>
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>  
                <telerik:AjaxSetting AjaxControlID="btnSearchAttendanceHistory">
				    <UpdatedControls>  
                        <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />                        
					    <%--<telerik:AjaxUpdatedControl ControlID="panAttendanceHistoryGrid" LoadingPanelID="loadingPanel" />  --%>
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>  
                <telerik:AjaxSetting AjaxControlID="btnResetSwipeHistory">
				    <UpdatedControls>
                        <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />                          
					    <%--<telerik:AjaxUpdatedControl ControlID="panSwipeHistoryFilter" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panSwipeHistoryGrid" />     --%>
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>  
                <telerik:AjaxSetting AjaxControlID="btnResetAbsence">
				    <UpdatedControls>        
                        <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />                  
					    <%--<telerik:AjaxUpdatedControl ControlID="panAbsenceHistoryFilter" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panAbsenceHistory" />       --%>
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting> 
                <telerik:AjaxSetting AjaxControlID="btnResetLeaveHistory">
				    <UpdatedControls>    
                        <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />                      
					    <%--<telerik:AjaxUpdatedControl ControlID="panLeaveHistoryFilter" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panLeaveHistoryGrid" />       --%>
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting> 
                <telerik:AjaxSetting AjaxControlID="btnResetAttendanceHistory">
				    <UpdatedControls>    
                        <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />                      
					    <%--<telerik:AjaxUpdatedControl ControlID="panAttendanceHistoryFilter" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panAttendanceHistoryGrid" />       --%>
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting> 
                <telerik:AjaxSetting AjaxControlID="btnGet">
				    <UpdatedControls>    
                        <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />                      
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting> 
                <telerik:AjaxSetting AjaxControlID="btnFindEmp">
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
                <telerik:AjaxSetting AjaxControlID="lnkReset">
				    <UpdatedControls>    
                        <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />                      
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting> 
                <telerik:AjaxSetting AjaxControlID="cboMonth">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panSwipeHistoryFilter" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting> 
                <telerik:AjaxSetting AjaxControlID="txtYear">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panSwipeHistoryFilter" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting> 
                <telerik:AjaxSetting AjaxControlID="cboMonthAbsence">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panAbsenceHistoryFilter" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting> 
                <telerik:AjaxSetting AjaxControlID="txtYearAbsence">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panAbsenceHistoryFilter" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>
                <telerik:AjaxSetting AjaxControlID="cboMonthLeaveHistory">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panLeaveHistoryFilter" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>                
                <telerik:AjaxSetting AjaxControlID="txtYearLeaveHistory">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panLeaveHistoryFilter" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting> 
                <telerik:AjaxSetting AjaxControlID="cboMonthAttendanceHistory">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panAttendanceHistoryFilter" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>                
                <telerik:AjaxSetting AjaxControlID="txtYearAttendanceHistory">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panAttendanceHistoryFilter" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting> 
                <telerik:AjaxSetting AjaxControlID="panBarMain">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting> 
                <telerik:AjaxSetting AjaxControlID="gridSwipeHistory">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="gridSwipeHistory" LoadingPanelID="loadingPanel" />  
				    </UpdatedControls>
			    </telerik:AjaxSetting>     
                <telerik:AjaxSetting AjaxControlID="gridAbsenceHistory">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="gridAbsenceHistory" LoadingPanelID="loadingPanel" />  
				    </UpdatedControls>
			    </telerik:AjaxSetting>
                <telerik:AjaxSetting AjaxControlID="gridLeaveHistory">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="gridLeaveHistory" LoadingPanelID="loadingPanel" />  
				    </UpdatedControls>
			    </telerik:AjaxSetting>                             
                <telerik:AjaxSetting AjaxControlID="gridAttendanceHistory">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="gridAttendanceHistory" LoadingPanelID="loadingPanel" />  
				    </UpdatedControls>
			    </telerik:AjaxSetting>       
                <telerik:AjaxSetting AjaxControlID="gridLeaveDetails">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="gridLeaveDetails" LoadingPanelID="loadingPanel" />  
				    </UpdatedControls>
			    </telerik:AjaxSetting>
                <telerik:AjaxSetting AjaxControlID="gridApprovedDIL">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="gridApprovedDIL" LoadingPanelID="loadingPanel" />  
				    </UpdatedControls>
			    </telerik:AjaxSetting>
                <telerik:AjaxSetting AjaxControlID="gridInactiveDIL">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="gridInactiveDIL" LoadingPanelID="loadingPanel" />  
				    </UpdatedControls>
			    </telerik:AjaxSetting>      
                <telerik:AjaxSetting AjaxControlID="gridDependentInfo">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="gridDependentInfo" LoadingPanelID="loadingPanel" />  
				    </UpdatedControls>
			    </telerik:AjaxSetting>        
                <telerik:AjaxSetting AjaxControlID="gridTraining">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="gridTraining" LoadingPanelID="loadingPanel" />  
				    </UpdatedControls>
			    </telerik:AjaxSetting>   
                <telerik:AjaxSetting AjaxControlID="tabDIL">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>  
                <telerik:AjaxSetting AjaxControlID="chkShowPhoto">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panEmployeeInfo" />  
				    </UpdatedControls>
			    </telerik:AjaxSetting>                   
            </AjaxSettings>
        </telerik:RadAjaxManager>
        <telerik:RadAjaxLoadingPanel ID="loadingPanel" runat="server" Skin="Office2010Silver"></telerik:RadAjaxLoadingPanel>
    </asp:Panel>
</asp:Content>
