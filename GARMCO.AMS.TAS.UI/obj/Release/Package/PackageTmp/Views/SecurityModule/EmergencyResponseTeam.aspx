<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/TASMaster.Master" AutoEventWireup="true" CodeBehind="EmergencyResponseTeam.aspx.cs" 
    Inherits="GARMCO.AMS.TAS.UI.Views.SecurityModule.EmergencyResponseTeam" %>

<%@ MasterType VirtualPath="~/Views/Shared/TASMaster.Master" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <title>Emergency Response Team</title>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="mainContent" runat="server">
    <telerik:RadFormDecorator ID="formDecor" runat="server" DecoratedControls="Buttons" Skin="Office2010Silver" />
    <%--<telerik:RadToolTipManager ID="tooltipMan" runat="server" 
        RelativeTo="Element" AnimationDuration="1"
        Width="190px" Height="50px" Position="TopCenter" 
        OnAjaxUpdate="tooltipMan_AjaxUpdate" ManualClose="True" 
        Animation="Resize" BackColor="#CCFFFF" BorderStyle="Solid" BorderWidth="2px" 
        EnableShadow="True" Font-Bold="True" Font-Names="Verdana" 
        HideEvent="LeaveTargetAndToolTip" Skin="Metro" ShowDelay="200" Font-Size="9pt">
        <TargetControls>        
            <telerik:ToolTipTargetControl TargetControlID="rbAvailableFireTeam" Value="Note: Tick this option to view the available Fire Team Members who are currently present in the company." />
            <telerik:ToolTipTargetControl TargetControlID="rbAvailableFireWatch" Value="Note: Tick this option to view the available Fire Watch Members who are currently present in the company." />            
            <telerik:ToolTipTargetControl TargetControlID="rbAllFireTeam" Value="Note: Tick this option to view the list of all Fire Team Members." />                        
            <telerik:ToolTipTargetControl TargetControlID="rbAllFireWatch" Value="Note: Tick this option to view the list of all Fire Watch Members." />                        
        </TargetControls>
    </telerik:RadToolTipManager>--%>  

    <table border="0" style="width: 100%; text-align: left; margin-top: 5px; margin-left: 5px;">
        <tr>
            <td colspan="2" style="padding-left: 10px;">
                <table border="0" style="width: 100%; text-align: left;">
                    <tr>
                        <td style="width: 50px; text-align: right; padding-right: 5px;" rowspan="2">
                            <img alt="" src="../../Images/fireteam_icon.jpg" />
                        </td>
                        <td id="tdPageTitle" runat="server"  class="PageTitleLabel" style="vertical-align: bottom; padding-left: 5px; width: 900px; font-size: 11pt;">
                            Emergency Response Team 
                        </td>
                        <td style="width: 50px;">
                            <asp:LinkButton ID="lnkMoveUp" runat="server" />
                        </td>
                        <td />
                    </tr>
                    <tr>
                        <td class="PageDescriptionHeader" style="text-align: left; vertical-align: top; padding-left: 5px; margin: 0px;">
                            View the Fire Team and Fire Watch member employees
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

    <asp:Panel ID="panMain" runat="server" style="margin-top: 0px; padding-bottom: 30px;"> 
        <asp:Panel ID="panSearchCriteria" runat="server" BorderStyle="None" style="padding: 0px; margin: 0px;">
            <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                <tr style="vertical-align: top;">
                    <td class="LabelBold" style="width: 100px; vertical-align: top; padding-top: 5px;">
                        <asp:CustomValidator ID="cusValGroupType" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                        Group Type
                    </td>
                    <td style="width: 900px; padding-left: 0px;">
                        <table border="0" style="width: 100%; text-align: left; margin: 0px; padding: 0px; table-layout: fixed;">                           
                            <tr style="height: 23px; padding-left: 0px; margin-left: 0px; vertical-align: top;">
                                <td class="LabelBold" style="width: 235px; text-align: left; padding-left: 0px;">
                                    <telerik:RadButton ID="rbAvailableFireTeam" runat="server" Text="Currently Available Fire Team" Checked="True" Width="100%" 
                                        Skin="Office2010Silver" OnClick="rbAvailableFireTeam_Click" ForeColor="Blue" 
                                        ButtonType="ToggleButton" ToggleType="Radio" Font-Names="Verdana">
                                    </telerik:RadButton>
                                </td>
                                <td class="LabelBold" style="width: 245px; text-align: left;">
                                    <telerik:RadButton ID="rbAvailableFireWatch" runat="server" Text="Currently Available Fire Watch" 
                                        Width="100%" Skin="Office2010Silver" OnClick="rbAvailableFireWatch_Click" ForeColor="Blue" 
                                        ButtonType="ToggleButton" ToggleType="Radio" Font-Names="Verdana">
                                    </telerik:RadButton>
                                </td>
                                <td class="LabelBold" style="width: 190px; text-align: left;">
                                    <telerik:RadButton ID="rbAllFireTeam" runat="server" Text="All Fire Team Members" Width="100%" 
                                        Skin="Office2010Silver" OnClick="rbAllFireTeam_Click" ForeColor="Blue" 
                                        ButtonType="ToggleButton" ToggleType="Radio" Font-Names="Verdana">
                                    </telerik:RadButton>
                                </td>
                                <td class="LabelBold" style="width: 195px; text-align: left;">
                                    <telerik:RadButton ID="rbAllFireWatch" runat="server" Text="All Fire Watch Members" Width="100%" 
                                        Skin="Office2010Silver" OnClick="rbAllFireWatch_Click" ForeColor="Blue" 
                                        ButtonType="ToggleButton" ToggleType="Radio" Font-Names="Verdana">
                                    </telerik:RadButton>
                                </td>
                                <td />
                            </tr>
                            <%--<tr style="height: 23px; padding-left: 0px; margin-left: 0px;">
                                <td class="LabelBold" style="text-align: left; padding-left: 0px;">
                                    <telerik:RadCheckBox ID="chkAvailableFireTeam" runat="server" Text="Currently Available Fire Team" Checked="True" Width="100%" Skin="Telerik" OnClick="chkAvailableFireTeam_Click" ForeColor="Blue"></telerik:RadCheckBox>
                                </td>
                                <td class="LabelBold" style="text-align: left;">
                                    <telerik:RadCheckBox ID="chkAvailableFireWatch" runat="server" Text="Currently Available Fire Watch" Width="100%" Skin="Telerik" OnClick="chkAvailableFireWatch_Click" ForeColor="Blue"></telerik:RadCheckBox>
                                </td>
                                <td class="LabelBold" style="text-align: left;">
                                    <telerik:RadCheckBox ID="chkAllFireTeam" runat="server" Text="All Fire Team Members" Width="100%" Skin="Telerik" OnClick="chkAllFireTeam_Click" ForeColor="Blue"></telerik:RadCheckBox>
                                </td>
                                <td class="LabelBold" style="text-align: left;">
                                    <telerik:RadCheckBox ID="chkAllFireWatch" runat="server" Text="All Fire Watch Members" Width="100%" Skin="Telerik" OnClick="chkAllFireWatch_Click" ForeColor="Blue"></telerik:RadCheckBox>
                                </td>
                                <td />
                            </tr>--%>
                        </table>                                                                  
                    </td>
                    <td />
                </tr> 
            </table>
            <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                <tr style="height: 23px; vertical-align: top;">
                    <td class="LabelBold" style="width: 100px; padding-top: 5px;">
                        Cost Center
                    </td>
                    <td style="width: 200px; padding-left: 0px;">
                        <telerik:RadComboBox ID="cboCostCenter" runat="server" 
                            DropDownWidth="330px"    
                            Width="100%" MaxHeight="200px"                                
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
                    <td class="LabelBold" style="width: 100px; padding-top: 5px;">
                        Employee No.
                    </td>
                    <td style="padding-left: 0px; padding-top: 0px; width: 150px;">
                         <table id="tdEmployee" runat="server" border="0" style="width: 100%; text-align: left; margin: 0px; padding-left: 0px; padding-top: 0px; table-layout: fixed;">
                            <tr style="vertical-align: top; margin: 0px; padding-left: 0px;">
                                <td style="width: 105px; text-align: left; padding-left: 0px; margin-left: 0px;">
                                    <telerik:RadNumericTextBox ID="txtEmp" runat="server" width="100%" 
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
                    <td class="LabelBold" style="width: 102px; padding-top: 5px;">
                        Scheduled Shift
                    </td>
                    <td style="width: 140px; padding-left: 0px;">
                         <telerik:RadComboBox ID="cboShift" runat="server"
                            DropDownWidth="135px" 
                            HighlightTemplatedItems="True" 
                            EmptyMessage="Select Work Shift"
                            Skin="Office2010Silver" 
                            Width="100%"
                            EnableVirtualScrolling="True">
                            <Items>
                                <telerik:RadComboBoxItem runat="server" Text="All Shifts" Value="valAll" Selected="true" />
                                <telerik:RadComboBoxItem runat="server" Text="D - Day Shift" Value="valDayShift" />
                                <telerik:RadComboBoxItem runat="server" Text="M - Morning Shift" Value="valMorningShift" />
                                <telerik:RadComboBoxItem runat="server" Text="E - Evening Shift" Value="valEveningShift" />
                                <telerik:RadComboBoxItem runat="server" Text="N - Night Shift" Value="valNightShift" />
                                <telerik:RadComboBoxItem runat="server" Text="O - Day Off" Value="valDayOff" />
                            </Items>
                        </telerik:RadComboBox>
                    </td>                    
                    <td />
                </tr> 
                <tr style="height: 23px; display: none;">
                    <td class="LabelBold">
                        
                    </td>
                    <td style="padding-left: 0px;">
                         <telerik:RadDatePicker ID="dtpAttendanceDate" runat="server" Width="120px" Skin="Windows7" ToolTip="(Note: The maximun date that can be selected is equal to today's date.)" Visible="False">
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
                        
                    </td>
                    <td>
                        <telerik:RadComboBox ID="cboGroupType" runat="server"
                            DropDownWidth="250px" 
                            HighlightTemplatedItems="True" 
                            Skin="Office2010Silver" 
                            Width="100%" Visible="false" 
                            EmptyMessage="Select Group Type"
                            EnableVirtualScrolling="True" AutoPostBack="True" OnSelectedIndexChanged="cboGroupType_SelectedIndexChanged">  
                             <Items>                                 
                                 <telerik:RadComboBoxItem runat="server" Selected="true" Text="Currently Available Fire Team" Value="valAvailableFireTeam" />
                                 <telerik:RadComboBoxItem runat="server" Text="Currently Available Fire Watch" Value="valAvailableFireWatch" />
                                 <telerik:RadComboBoxItem runat="server" Text="Currently Available Fire Team &amp; Fire Watch" Value="valAvailableFireTeamFireWatch" />
                                 <telerik:RadComboBoxItem runat="server" Text="All Fire Team Members" Value="valAllFireTeam" />
                                 <telerik:RadComboBoxItem runat="server" Text="All Fire Watch Members" Value="valAllFireWatch" />
                             </Items>
                         </telerik:RadComboBox>  
                    </td>                    
                    <td>
                        <asp:CustomValidator ID="cusValDate" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" Visible="False" />                                                 
                    </td>
                </tr>  
                <tr style="height: 23px;">
                    <td class="LabelBold">
                        <asp:CustomValidator ID="cusValButton" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                    </td>
                    <td colspan="3" style="padding-left: 0px;">
                        <telerik:RadButton ID="btnSearch" runat="server" Text="Search" ToolTip="Search matching database records" Width="75px" 
                            CssClass="RadButtonStyle" ValidationGroup="valPrimary" Font-Size="9pt" OnClick="btnSearch_Click" Skin="Office2010Silver" />
                        <telerik:RadButton ID="btnReset" runat="server" Text="Reset" ToolTip="Reset filter criterias" Width="75px" 
                            CssClass="RadButtonStyle" ValidationGroup="valPrimary" Font-Size="9pt" OnClick="btnReset_Click" Skin="Office2010Silver" />                                                
                    </td>
                    <td>
                                            
                    </td>
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
            <table id="tblGrid" runat="server" border="0" style="width: 100%; text-align: left; margin-top: 0px; table-layout: fixed;">
                <tr>
                    <td>
                        <telerik:RadGrid ID="gridSearchResult" runat="server"
                            AllowSorting="true" AllowMultiRowSelection="true"
                            PageSize="10" AutoGenerateColumns="false" Skin="Silk" 
                            GridLines="None" Width="100%" Height="" CellSpacing="0"
                            onpageindexchanged="gridSearchResult_PageIndexChanged" 
                            onpagesizechanged="gridSearchResult_PageSizeChanged" 
                            onsortcommand="gridSearchResult_SortCommand" 
                            onitemcommand="gridSearchResult_ItemCommand" 
                            onitemdatabound="gridSearchResult_ItemDataBound" 
                            OnPreRender="gridSearchResult_PreRender"
                            Font-Names="Tahoma" Font-Size="9pt" Font-Bold="False" 
                            AllowPaging = "true" BorderStyle="Outset" BorderWidth="1px">
                            <ExportSettings ExportOnlyData="true" IgnorePaging="true" OpenInNewWindow="true" FileName="EmergencyResponseTeamList" HideStructureColumns="true">
                                <Pdf PageHeight="210mm" PageWidth="310mm" PageTitle="Emergency Response Team List" DefaultFontFamily="Arial Unicode MS"
                                PageBottomMargin="20mm" PageTopMargin="20mm" PageLeftMargin="20mm" PageRightMargin="20mm" PaperSize="A4" AllowPrinting="true" />
                            </ExportSettings>
                            <AlternatingItemStyle Font-Names="Tahoma" Font-Size="9pt" Wrap="True" />
                            <MasterTableView DataKeyNames="EmpNo" ClientDataKeyNames="EmpNo" NoMasterRecordsText="No record found." 
                                TableLayout="Fixed" PagerStyle-AlwaysVisible="True" Font-Names="Tahoma" Font-Size="9pt">
                                <PagerStyle AlwaysVisible="True" Mode="NextPrevAndNumeric" />
                                <CommandItemSettings ExportToPdfText="Export to PDF"></CommandItemSettings>
			                    <RowIndicatorColumn>
				                    <HeaderStyle Width="20px" />
			                    </RowIndicatorColumn>
			                    <ExpandCollapseColumn>
				                    <HeaderStyle Width="20px" />
			                    </ExpandCollapseColumn>
                                 <Columns>      
                                    <telerik:GridTemplateColumn DataField="EmpAttendanceFlag" HeaderText="" 
                                        SortExpression="EmpAttendanceFlag" UniqueName="EmpAttendanceFlag">
								        <HeaderStyle Width="35px" HorizontalAlign="Left" />
                                        <ItemStyle HorizontalAlign="Center" />
								        <ItemTemplate>
									        <div style="width: 35px; text-align: left;">    
                                                <asp:Image ID="imgAvailability" runat="server" 
                                                    ImageUrl='<%# Eval("EmpAttendanceFlag") %>'
                                                    ToolTip='<%# Eval("EmpAttendanceNotes") %>'
                                                    ImageAlign="Left" Height="20px" Width="20px" />   
									        </div>
								        </ItemTemplate>
							        </telerik:GridTemplateColumn> 
                                    <telerik:GridTemplateColumn DataField="EmpPhotoPath" HeaderText="" 
                                        SortExpression="EmpPhotoPath" UniqueName="EmpPhotoPath">
								        <HeaderStyle Width="60px" HorizontalAlign="Left" />
                                        <ItemStyle HorizontalAlign="Center" />
								        <ItemTemplate>
									        <div style="width: 100%; text-align: left;">                                                
                                                <img id="imgPhoto" runat="server" 
                                                    src='<%# Eval("EmpImagePath") %>' 
                                                    alt='<%# Eval("PhotoTooltip") %>' 
                                                    style="height: 60px; width: 60px;" />
									        </div>
								        </ItemTemplate>
							        </telerik:GridTemplateColumn>                                      
                                    <telerik:GridBoundColumn DataField="EmpNo" DataType="System.Int32" HeaderText="Emp. No."
                                        ReadOnly="True" SortExpression="EmpNo" UniqueName="EmpNo">
                                        <HeaderStyle Width="80px" Font-Bold="True" HorizontalAlign="Right"  />
                                        <ItemStyle HorizontalAlign="Right" />
                                    </telerik:GridBoundColumn>     
                                    <telerik:GridTemplateColumn DataField="EmpName" HeaderText="Employee Name" 
                                        SortExpression="EmpName" UniqueName="EmpName">
								        <HeaderStyle Width="250px" HorizontalAlign="Left"  />
								        <ItemTemplate>
									        <div class="columnEllipsis" style="width: 240px; text-align: left;">
										        <asp:Literal ID="litEmpName" runat="server" Text='<%# Eval("EmpName") %>' />
									        </div>
								        </ItemTemplate>
							        </telerik:GridTemplateColumn>	                                        
                                    <telerik:GridTemplateColumn DataField="Position" FilterControlAltText="Filter Position column" HeaderText="Position" 
                                        SortExpression="Position" UniqueName="Position">
								        <HeaderStyle Width="220px" HorizontalAlign="Left"  />
								        <ItemTemplate>
									        <div class="columnEllipsis" style="width: 210px; text-align: left;">
										        <asp:Literal ID="litPosition" runat="server" Text='<%# Eval("Position") %>' />
									        </div>
								        </ItemTemplate>
							        </telerik:GridTemplateColumn>      
                                    <telerik:GridBoundColumn DataField="ShiftPatCode" DataType="System.String" HeaderText="Shift Pat." 
                                        ReadOnly="True" SortExpression="ShiftPatCode" UniqueName="ShiftPatCode">
                                        <HeaderStyle Width="77px" Font-Bold="True" HorizontalAlign="Center"  />
                                        <ItemStyle HorizontalAlign="Center" />
                                    </telerik:GridBoundColumn>   
                                    <telerik:GridBoundColumn DataField="ShiftCode" DataType="System.String" HeaderText="Sch. Shift" 
                                        ReadOnly="True" SortExpression="ShiftCode" UniqueName="ShiftCode">
                                        <HeaderStyle Width="77px" Font-Bold="True" HorizontalAlign="Center" />
                                        <ItemStyle HorizontalAlign="Center" />
                                    </telerik:GridBoundColumn> 
                                    <telerik:GridBoundColumn DataField="ShiftPointer" DataType="System.Int32" HeaderText="Shift Pointer"
                                        ReadOnly="True" SortExpression="ShiftPointer" UniqueName="ShiftPointer" Visible="false">
                                        <HeaderStyle Width="100px" Font-Bold="True"></HeaderStyle>
                                    </telerik:GridBoundColumn>                                          
                                    <telerik:GridBoundColumn DataField="Extension" DataType="System.String" HeaderText="Phone Ext." 
                                        ReadOnly="True" SortExpression="Extension" UniqueName="Extension">
                                        <HeaderStyle Width="80px" Font-Bold="True" ></HeaderStyle>
                                    </telerik:GridBoundColumn>
                                    <telerik:GridBoundColumn DataField="MobileNo" DataType="System.String" HeaderText="Mobile No." 
                                        ReadOnly="True" SortExpression="MobileNo" UniqueName="MobileNo">
                                        <HeaderStyle Width="80px" Font-Bold="True" ></HeaderStyle>
                                    </telerik:GridBoundColumn>                                    
                                    <telerik:GridBoundColumn DataField="SwipeTime" HeaderText="Last Swiped"
                                        DataFormatString="{0:dd-MMM-yyyy hh:mm tt}" DataType="System.DateTime" 
                                        ReadOnly="True" SortExpression="SwipeTime" UniqueName="SwipeTime">
                                        <HeaderStyle Width="140px" Font-Bold="True"></HeaderStyle>
                                    </telerik:GridBoundColumn>  
                                    <telerik:GridBoundColumn DataField="SwipeDate" HeaderText="Date" Visible="false"
                                        DataFormatString="{0:dd-MMM-yyyy}" DataType="System.DateTime" 
                                        ReadOnly="True" SortExpression="SwipeDate" UniqueName="SwipeDate">
                                        <HeaderStyle Width="85px" Font-Bold="True" ></HeaderStyle>
                                    </telerik:GridBoundColumn> 
                                    <telerik:GridTemplateColumn DataField="SwipeSummary" HeaderText="Swipe Location" 
                                        SortExpression="SwipeSummary" UniqueName="SwipeSummary">
								        <HeaderStyle Width="230px" HorizontalAlign="Left"   />
								        <ItemTemplate>
									        <div class="columnEllipsis" style="width: 220px; text-align: left;">
										        <asp:Literal ID="litSwipeSummary" runat="server" Text='<%# Eval("SwipeSummary") %>' />
									        </div>
								        </ItemTemplate>
							        </telerik:GridTemplateColumn>	   
                                    <telerik:GridTemplateColumn DataField="CostCenterFullName" FilterControlAltText="Filter Cost Center column" HeaderText="Cost Center" 
                                        SortExpression="CostCenterFullName" UniqueName="CostCenterFullName">
								        <HeaderStyle Width="250px" HorizontalAlign="Left"   />
								        <ItemTemplate>
									        <div class="columnEllipsis" style="width: 250px; text-align: left;">
										        <asp:Literal ID="litCostCenter" runat="server" Text='<%# Eval("CostCenterFullName") %>' />
									        </div>
								        </ItemTemplate>
							        </telerik:GridTemplateColumn>	
                                    <telerik:GridTemplateColumn DataField="SupervisorFullName" HeaderText="Direct Supervisor" 
                                        SortExpression="SupervisorFullName" UniqueName="SupervisorFullName">
								        <HeaderStyle Width="280px" HorizontalAlign="Left"  />
								        <ItemTemplate>
									        <div class="columnEllipsis" style="width: 270px; text-align: left;">
										        <asp:Literal ID="litSupervisorFullName" runat="server" Text='<%# Eval("SupervisorFullName") %>' />
									        </div>
								        </ItemTemplate>
							        </telerik:GridTemplateColumn>
                                    <telerik:GridTemplateColumn DataField="SuperintendentFullName" HeaderText="Area Manager" 
                                        SortExpression="SuperintendentFullName" UniqueName="SuperintendentFullName" Visible="false">
								        <HeaderStyle Width="280px" HorizontalAlign="Left"  />
								        <ItemTemplate>
									        <div class="columnEllipsis" style="width: 270px; text-align: left;">
										        <asp:Literal ID="litSuperintendentFullName" runat="server" Text='<%# Eval("SuperintendentFullName") %>' />
									        </div>
								        </ItemTemplate>
							        </telerik:GridTemplateColumn>                                      
                                    <telerik:GridBoundColumn DataField="Notes" DataType="System.String" HeaderText="Remarks" 
                                        ReadOnly="True" SortExpression="Notes" UniqueName="Notes">
                                        <HeaderStyle Width="200px" Font-Bold="True" ></HeaderStyle>
                                    </telerik:GridBoundColumn> 
                                    <telerik:GridBoundColumn DataField="GroupType" DataType="System.String" HeaderText="Group Type" 
                                        ReadOnly="True" SortExpression="GroupType" UniqueName="GroupType">
                                        <HeaderStyle Width="100px" Font-Bold="True" ></HeaderStyle>
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
                <telerik:AjaxSetting AjaxControlID="btnPrint">
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
                <telerik:AjaxSetting AjaxControlID="rblFilter">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>                      
                <telerik:AjaxSetting AjaxControlID="gridSearchResult">
				    <UpdatedControls>
                        <telerik:AjaxUpdatedControl ControlID="gridSearchResult" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting> 
                <telerik:AjaxSetting AjaxControlID="cboGroupType">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>    
                <telerik:AjaxSetting AjaxControlID="chkAvailableFireTeam">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>  
                <telerik:AjaxSetting AjaxControlID="chkAvailableFireWatch">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>  
                <telerik:AjaxSetting AjaxControlID="chkAllFireTeam">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>  
                <telerik:AjaxSetting AjaxControlID="chkAllFireWatch">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>  
                <telerik:AjaxSetting AjaxControlID="rbAvailableFireTeam">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>  
                <telerik:AjaxSetting AjaxControlID="rbAvailableFireWatch">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>  
                <telerik:AjaxSetting AjaxControlID="rbAllFireTeam">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>  
                <telerik:AjaxSetting AjaxControlID="rbAllFireWatch">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
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
