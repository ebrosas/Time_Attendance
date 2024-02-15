<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/TASMaster.Master" AutoEventWireup="true" CodeBehind="MasterShiftPatternSetup.aspx.cs" 
    Inherits="GARMCO.AMS.TAS.UI.Views.AdminFunctions.MasterShiftPatternSetup" StylesheetTheme="Standard" %>

<%@ MasterType VirtualPath="~/Views/Shared/TASMaster.Master" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <title>Cost Center Security</title>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="mainContent" runat="server">
    <script type="text/javascript">
    function ConvertToUppercase(sender, args)
    {
        args.set_newValue(args.get_newValue().toUpperCase());
    }
    </script>

    <telerik:RadFormDecorator ID="formDecor" runat="server" DecoratedControls="Buttons" Skin="Office2010Silver" />

    <table border="0" style="width: 100%; text-align: left; margin-top: 5px; margin-left: 0px;">
        <tr>
            <td colspan="2" style="padding-left: 10px;">
                <table border="0" style="width: 100%; text-align: left;">
                    <tr>
                        <td style="width: 50px; text-align: right; padding-right: 5px;" rowspan="2">
                            <img alt="" src="../../Images/cost_center_security_icon.png" />
                        </td>
                        <td id="tdPageTitle" runat="server" class="PageTitleLabel" style="vertical-align: bottom; padding-left: 2px; width: 900px; font-size: 11pt;">
                            Master Shift Pattern Setup
                        </td>
                        <td style="width: 50px;">
                            <asp:LinkButton ID="lnkMoveUp" runat="server" />
                        </td>
                        <td />
                    </tr>
                    <tr>
                        <td class="PageDescriptionHeader" style="text-align: left; vertical-align: top; padding-left: 2px; margin: 0px;">
                            Allows a System Administrator to manage the Shift Pattern, Shift Timing Schedules and Shift Pointer Sequences
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
                    <td id="tdEmployeeTitle" runat="server" class="LabelBold" style="width: 110px;">
                        <asp:CustomValidator ID="cusValShiftPattern" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                        Shift Pattern
                    </td>
                    <td style="width: 200px;">
                        <telerik:RadComboBox ID="cboShiftPattern" runat="server"
                            DropDownWidth="300px" 
                            HighlightTemplatedItems="True" 
                            Skin="Office2010Silver" 
                            Width="100%" 
                            EmptyMessage="Select Shift Pattern"
                            EnableVirtualScrolling="True" AutoPostBack="True" 
                            OnSelectedIndexChanged="cboShiftPattern_SelectedIndexChanged" MaxHeight="250px" >
                        </telerik:RadComboBox> 
                    </td>
                    <td style="text-align: left; width: 200px; padding-left: 1px;">                        
                        <telerik:RadButton ID="btnShiftPatternDetail" runat="server" Skin="Office2010Silver" 
                            Text="..." ToolTip="Click to open the Manage Shift Pattern page." 
                            Width="30px" Font-Bold="False" Font-Size="9pt" CausesValidation="false"
                            onclick="btnShiftPatternDetail_Click" Enabled="False">
                        </telerik:RadButton>
                    </td>
                    <td rowspan="3" style="text-align: right; padding-right: 20px; vertical-align: top;">
                        <asp:Panel ID="panShiftPatternDetail" CssClass="GroupPanelHeader" GroupingText="Shift Pattern Details:"  runat="server" BorderStyle="None" style="padding: 0px; margin: 0px;">
                            <table border="0" style="width: 100%; text-align: left; table-layout: fixed;">
                                <tr style="vertical-align: top;">
                                    <td class="LabelBold" style="width: 130px; padding-top: 5px;">
                                        <asp:CustomValidator ID="cusValShiftPatCode" runat="server" ControlToValidate="txtGeneric" 
                                            CssClass="LabelValidationError" Display="Dynamic" 
                                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                                        Shift Pattern Code
                                    </td>
                                    <td style="width: 70px;">
                                        <telerik:RadTextBox ID="txtShiftPatCode" runat="server" Width="100%" style="text-transform: uppercase;"
                                            EmptyMessage="" Skin="Office2010Silver" ToolTip="(Note: The maximum text input is 2 chars.)" 
                                            Font-Names="Tahoma" Font-Size="9pt" MaxLength="2" BackColor="Yellow" >
                                            <ClientEvents OnValueChanging="ConvertToUppercase" />
                                            <EmptyMessageStyle Resize="None" />
                                            <ReadOnlyStyle Resize="None" />
                                            <FocusedStyle Resize="None" />
                                            <DisabledStyle Resize="None" />
                                            <InvalidStyle Resize="None" />
                                            <HoveredStyle Resize="None" />
                                            <EnabledStyle Resize="None" />
                                        </telerik:RadTextBox>
                                    </td>
                                    <td rowspan="2" class="LabelBold" style="width: 90px; padding-top: 5px;">
                                        Description
                                    </td>
                                    <td style="width: 300px;">
                                        <telerik:RadTextBox ID="txtShiftPatDesc" runat="server" Width="100%" 
                                            EmptyMessage="" Skin="Office2010Silver" ToolTip="(Note: The maximum text input is 50 chars.)" 
                                            Font-Names="Tahoma" Font-Size="9pt" MaxLength="50" Rows="2" TextMode="MultiLine" />
                                    </td>
                                    <td class="LabelBold" style="width: 90px; padding-top: 6px; padding-right: 0px;">
                                        Is Day Shift?
                                    </td>
                                    <td style="width: 120px;">
                                        <asp:RadioButtonList ID="rblIsDayShift" runat="server" RepeatDirection="Horizontal">
                                            <asp:ListItem Text="Yes" Value="1" />
                                            <asp:ListItem Text="No" Value="0" Selected="True" />
                                        </asp:RadioButtonList>
                                    </td>
                                    <td />
                                </tr>      
                                <tr>
                                    <td colspan="5" style="padding-left: 10px;">
                                        <telerik:RadButton ID="btnSaveShiftPattern" runat="server" Text="Save" ToolTip="Click here to save the shift pattern information" Width="70px" 
                                            CssClass="RadButtonStyle" ValidationGroup="valPrimary" Font-Size="9pt" OnClick="btnSaveShiftPattern_Click" Skin="Office2010Silver" />                                                                                                                
                                        <telerik:RadButton ID="btnCancelShiftPattern" runat="server" Text="Cancel" ToolTip="Click here to cancel changes to the shift pattern" Width="70px" 
                                            CssClass="RadButtonStyle" ValidationGroup="valPrimary" Font-Size="9pt" OnClick="btnCancelShiftPattern_Click" Skin="Office2010Silver" />                                                                        
                                    </td>
                                    <td>
                                        <telerik:RadNumericTextBox ID="txtShiftPatAutoID" runat="server" width="100%" Visible="false" 
                                            MinValue="0" Skin="Office2010Silver" DataType="System.Int32" MaxLength="8" MaxValue="99999999">
                                            <NumberFormat ZeroPattern="n" DecimalDigits="0" GroupSeparator="" />
                                        </telerik:RadNumericTextBox>
                                    </td>
                                    <td />
                                </tr>                          
                            </table>
                        </asp:Panel>
                    </td>
                </tr>    
                <tr style="height: 23px;">
                    <td id="td1" runat="server" class="LabelBold" style="width: 110px;">
                       Is Day Shift?
                    </td>
                    <td colspan="2">
                        <table border="0" style="width: 100%; text-align: left; padding-left: 0px; margin-left: 0px; table-layout: fixed;">
                            <tr>
                                <td style="text-align: left; width: 120px; padding-left: 0px; margin-left: 0px;">
                                    <asp:RadioButtonList ID="rblDayShift" runat="server" RepeatDirection="Horizontal">
                                        <asp:ListItem Text="All" Value="0" Selected="True" />
                                        <asp:ListItem Text="Yes" Value="2" />
                                        <asp:ListItem Text="No" Value="1" />
                                    </asp:RadioButtonList>
                                </td>
                                <td class="LabelBold" style="width: 100px; text-align: right; padding-right: 5px;">
                                    Is Flexitime?
                                </td>
                                <td style="padding-left: 0px; margin-left: 0px;">
                                    <asp:RadioButtonList ID="rblFlexitime" runat="server" RepeatDirection="Horizontal">
                                        <asp:ListItem Text="All" Value="0" Selected="True" />
                                        <asp:ListItem Text="Yes" Value="2" />
                                        <asp:ListItem Text="No" Value="1" />
                                    </asp:RadioButtonList>
                                </td>
                            </tr>
                        </table>
                    </td>                    
                </tr>     
                <tr runat="server" id="trCommandButtons">
                    <td>

                    </td>
                    <td colspan="2">
                        <telerik:RadButton ID="btnNew" runat="server" Text="Create New Shift Pattern..." ToolTip="Click here to add new shift pattern"  Width="170px"
                            CssClass="RadButtonStyle" ValidationGroup="valPrimary" Font-Size="9pt" OnClick="btnNew_Click" Skin="Office2010Silver" />      
                        <telerik:RadButton ID="btnDeleteShiftPattern" runat="server" Text="Delete" ToolTip="Click here to delete the selected shift pattern" Width="80px" 
                            CssClass="RadButtonStyle" ValidationGroup="valPrimary" Font-Size="9pt" OnClick="btnDeleteShiftPattern_Click" Skin="Office2010Silver" Enabled="False" />                                                                                          
                        <telerik:RadButton ID="btnSearch" runat="server" Text="Search" ToolTip="Search matching database records" Width="80px" 
                            CssClass="RadButtonStyle" ValidationGroup="valPrimary" Font-Size="9pt" OnClick="btnSearch_Click" Skin="Office2010Silver" />       
                        <telerik:RadButton ID="btnBack" runat="server" ToolTip="Go back to previous page"
                            Text="<< Go back to previous page" Skin="Office2010Silver" Font-Bold="False" Font-Size="9pt" Width="175px"
                            CssClass="RadButtonStyle" CausesValidation="false" OnClick="btnBack_Click" Visible="False">
                        </telerik:RadButton>                                                                   
                    </td>
                </tr>                        
            </table>
        </asp:Panel>

        <asp:Panel ID="panGridShiftTiming" runat="server" BorderStyle="None" CssClass="GroupPanelHeader" GroupingText="Shift Timing Schedule:"  style="margin-top: 10px;">
            <table runat="server" id="tblShiftTimingFilter" border="0" style="width: 100%; text-align: left; margin-top: 0px; table-layout: fixed; padding-top: 0px; margin-top: 0px;">
                <tr>
                    <td class="LabelBold" style="width: 100px;">
                        <asp:CustomValidator ID="cusValWorkShift" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                        Shift Timing
                    </td>
                    <td style="width: 110px;">
                        <telerik:RadComboBox ID="cboWorkShift" runat="server"
                            DropDownWidth="105px" 
                            HighlightTemplatedItems="True" 
                            Skin="Office2010Silver" 
                            Width="100%" 
                            EmptyMessage="Select Shift"
                            EnableVirtualScrolling="True">
                        </telerik:RadComboBox> 
                    </td>
                    <td style="width: 70px;">
                        <telerik:RadButton ID="btnAddShift" runat="server" Text="Add" ToolTip="Add the selected shift timing to the grid" Width="50px" 
                            CssClass="RadButtonStyle" ValidationGroup="valPrimary"  Font-Size="9pt" OnClick="btnAddShift_Click" Skin="Office2010Silver" />  
                    </td>
                    <td />
                </tr>
            </table>

            <table id="tblGrid" runat="server" border="0" style="width: 100%; text-align: left; margin-top: 0px; table-layout: fixed;">
                <tr>
                    <td>
                        <telerik:RadGrid ID="gridShiftTiming" runat="server"
                            AllowSorting="true" AllowMultiRowSelection="true"
                            PageSize="10" AutoGenerateColumns="false" Skin="Silk" 
                            GridLines="None" Width="100%" Height="" CellSpacing="0"
                            onpageindexchanged="gridShiftTiming_PageIndexChanged" 
                            onpagesizechanged="gridShiftTiming_PageSizeChanged" 
                            onsortcommand="gridShiftTiming_SortCommand" 
                            onitemcommand="gridShiftTiming_ItemCommand" 
                            onitemdatabound="gridShiftTiming_ItemDataBound" 
                            Font-Names="Tahoma" Font-Size="9pt" Font-Bold="False" 
                            AllowPaging = "true" BorderStyle="Outset" BorderWidth="1px">
                            <ExportSettings ExportOnlyData="true" IgnorePaging="true" OpenInNewWindow="true" FileName="ShiftPatternChangeList" HideStructureColumns="true">
                                <Pdf PageHeight="210mm" PageWidth="310mm" PageTitle="Shift Pattern Changes List" DefaultFontFamily="Arial Unicode MS"
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
                                    <telerik:GridTemplateColumn HeaderText="" UniqueName="UpdateLink" HeaderTooltip="Update" Visible="false">
								        <HeaderStyle Width="60px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								        <ItemTemplate>
									        <div style="width: 60px; text-align: left;">
                                                <asp:LinkButton ID="lnkUpdate" runat="server" Text='Update' 
                                                    Font-Bold="true" ForeColor="Blue" OnClick="lnkUpdate_Click" />
									        </div>
								        </ItemTemplate>
							        </telerik:GridTemplateColumn>  
                                    <telerik:GridButtonColumn ButtonType="ImageButton" CommandName="Select" HeaderText="" CommandArgument="UpdateButton" Visible="false" 
                                        UniqueName="UpdateButton" ImageUrl="~/Images/save_grid_icon.png" HeaderTooltip="Update changes applied to the selected record">
                                        <HeaderStyle Font-Bold="True" Width="35px"></HeaderStyle>
                                        <ItemStyle Font-Bold="true" ForeColor="Blue" HorizontalAlign="Center" />
				                    </telerik:GridButtonColumn>       
                                    <telerik:GridButtonColumn ButtonType="ImageButton" CommandName="Select" HeaderText="" CommandArgument="DeleteButton" 
                                        UniqueName="DeleteButton" ImageUrl="~/Images/delete_grid_icon.jpg" HeaderTooltip="Delete the selected record">
                                        <HeaderStyle Font-Bold="True" Width="35px"></HeaderStyle>
                                        <ItemStyle Font-Bold="true" ForeColor="Blue" HorizontalAlign="Center" />
				                    </telerik:GridButtonColumn>          
                                    <telerik:GridBoundColumn DataField="ShiftPatCode" DataType="System.String" HeaderText="Shift Pat. <br /> Code" 
                                        ReadOnly="True" SortExpression="ShiftPatCode" UniqueName="ShiftPatCode">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" Width="80px" HorizontalAlign="Center"></HeaderStyle>
                                        <ItemStyle HorizontalAlign="Center" Font-Bold="true" ForeColor="Purple" />
                                    </telerik:GridBoundColumn>    

                                    <telerik:GridTemplateColumn DataField="ShiftFullDescription" HeaderText="Shift Timing" DataType="System.String" 
                                        SortExpression="ShiftFullDescription" UniqueName="ShiftFullDescription">
								        <HeaderStyle Width="120px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								        <ItemTemplate>     
                                            <telerik:RadComboBox ID="cboShiftCode" runat="server"
							                    DropDownWidth="150px" 
							                    HighlightTemplatedItems="True" 
							                    MarkFirstMatch="true" 
							                    Skin="Office2010Silver" 
							                    Width="100%"  
							                    EmptyMessage="Select Shift"
							                    EnableLoadOnDemand="true"
							                    EnableVirtualScrolling="true" 
                                                Text='<%# Eval("ShiftFullDescription")%>'
                                                Value='<%# Eval("ShiftCode")%>'
                                                Font-Names="Tahoma" 
                                                Font-Size="9pt"
                                                AutoPostBack="false"
                                                Filter="None"
                                                Enabled="false"
                                                ToolTip="(Note: Scheduled Shift is a mandatory field which should not be left blank.)"
                                                OnItemsRequested="cboShiftCode_ItemsRequested"
                                                OnSelectedIndexChanged="cboShiftCode_SelectedIndexChanged">
                                            </telerik:RadComboBox>                                       
								        </ItemTemplate>
							        </telerik:GridTemplateColumn> 

                                    <telerik:GridTemplateColumn DataField="IsDayShift" HeaderText="Is Day Shift?" SortExpression="IsDayShift" UniqueName="IsDayShift">
								        <HeaderStyle Width="100px" HorizontalAlign="Center" />
								        <ItemTemplate>
									        <div style="width: 90px; text-align: center;">
										        <asp:Label ID="lblIsDayShift" runat="server" 
                                                    Text='<%# Convert.ToBoolean(Eval("IsDayShift")) == true ? "Yes" : "No" %>'>
										        </asp:Label>  
									        </div>
								        </ItemTemplate>
                                        <ItemStyle HorizontalAlign="Center" />
							        </telerik:GridTemplateColumn>    
                                    <telerik:GridTemplateColumn DataField="IsFlexitime" HeaderText="Is Flexitime?" SortExpression="IsFlexitime" UniqueName="IsFlexitime">
								        <HeaderStyle Width="95px" HorizontalAlign="Center" />
								        <ItemTemplate>
									        <div style="width: 95px; text-align: center;">
										        <asp:Label ID="lblIsFlexitime" runat="server" 
                                                    Text='<%# Convert.ToBoolean(Eval("IsFlexitime")) == true ? "Yes" : "No" %>'>
										        </asp:Label>  
									        </div>
								        </ItemTemplate>
                                        <ItemStyle HorizontalAlign="Center" />
							        </telerik:GridTemplateColumn>

                                    <telerik:GridTemplateColumn DataField="ArrivalFrom" HeaderText="Arrival From <br /> (Normal Day)" SortExpression="ArrivalFrom" UniqueName="ArrivalFrom">
								        <HeaderStyle Width="105px" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" HorizontalAlign="Center" />
								        <ItemTemplate>   
                                            <telerik:RadDateInput ID="dtpArrivalFrom" runat="server" SelectedDate='<%# Eval("ArrivalFrom") %>'  LabelWidth="80px" Width="80px" Culture="en-US" ToolTip="(Note: Arrival From is a mandatory field and should be less than Arrival To.)"  
                                                EmptyMessage="HH:mm" InvalidStyleDuration="100" DateFormat="HH:mm"  Font-Bold="true" ForeColor="Gray" AutoPostBack="true" OnTextChanged="dtpArrivalFrom_TextChanged">
                                            </telerik:RadDateInput>   
								        </ItemTemplate>
                                        <ItemStyle HorizontalAlign="Left" />
							        </telerik:GridTemplateColumn> 
                                    <telerik:GridTemplateColumn DataField="ArrivalTo" HeaderText="Arrival To <br /> (Normal Day)" SortExpression="ArrivalTo" UniqueName="ArrivalTo">
								        <HeaderStyle Width="105px" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" HorizontalAlign="Center" />
								        <ItemTemplate>   
                                            <telerik:RadDateInput ID="dtpArrivalTo" runat="server" SelectedDate='<%# Eval("ArrivalTo") %>'  LabelWidth="80px" Width="80px" Culture="en-US" 
                                                EmptyMessage="HH:mm" InvalidStyleDuration="100" DateFormat="HH:mm"  Font-Bold="true" ForeColor="Green" AutoPostBack="true" OnTextChanged="dtpArrivalTo_TextChanged">
                                            </telerik:RadDateInput>   
								        </ItemTemplate>
                                        <ItemStyle HorizontalAlign="Left" />
							        </telerik:GridTemplateColumn> 
                                    <telerik:GridTemplateColumn DataField="DepartFrom" HeaderText="Depart From <br /> (Normal Day)" SortExpression="DepartFrom" UniqueName="DepartFrom">
								        <HeaderStyle Width="105px" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" HorizontalAlign="Center" />
								        <ItemTemplate>   
                                            <telerik:RadDateInput ID="dtpDepartFrom" runat="server" SelectedDate='<%# Eval("DepartFrom") %>'  LabelWidth="80px" Width="80px" Culture="en-US" 
                                                EmptyMessage="HH:mm" InvalidStyleDuration="100" DateFormat="HH:mm" Font-Bold="true" ForeColor="Green" AutoPostBack="true" OnTextChanged="dtpDepartFrom_TextChanged">
                                            </telerik:RadDateInput>   
								        </ItemTemplate>
                                        <ItemStyle HorizontalAlign="Left" />
							        </telerik:GridTemplateColumn> 
                                    <telerik:GridTemplateColumn DataField="DepartTo" HeaderText="Depart To <br /> (Normal Day)" SortExpression="DepartTo" UniqueName="DepartTo">
								        <HeaderStyle Width="105px" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" HorizontalAlign="Center" />
								        <ItemTemplate>   
                                            <telerik:RadDateInput ID="dtpDepartTo" runat="server" SelectedDate='<%# Eval("DepartTo") %>'  LabelWidth="80px" Width="80px" Culture="en-US" 
                                                EmptyMessage="HH:mm" InvalidStyleDuration="100" DateFormat="HH:mm" Font-Bold="true" ForeColor="Gray" AutoPostBack="true" OnTextChanged="dtpDepartTo_TextChanged">
                                            </telerik:RadDateInput>   
								        </ItemTemplate>
                                        <ItemStyle HorizontalAlign="Left" />
							        </telerik:GridTemplateColumn> 
                                    <telerik:GridBoundColumn DataField="DurationNormalDayString" DataType="System.String" HeaderText="Duration <br /> (Normal Day)" 
                                        ReadOnly="True" SortExpression="DurationNormalDayString" UniqueName="DurationNormalDayString">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="100px" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" HorizontalAlign="Center" />
                                        <ItemStyle HorizontalAlign="Center" ForeColor="Red" />
                                    </telerik:GridBoundColumn> 

                                    <telerik:GridTemplateColumn DataField="RArrivalFrom" HeaderText="Arrival From <br /> (Ramadan)" SortExpression="RArrivalFrom" UniqueName="RArrivalFrom">
								        <HeaderStyle Width="105px" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" HorizontalAlign="Center" />
								        <ItemTemplate>   
                                            <telerik:RadDateInput ID="dtpRArrivalFrom" runat="server" SelectedDate='<%# Eval("RArrivalFrom") %>'  LabelWidth="80px" Width="80px" Culture="en-US" 
                                                EmptyMessage="HH:mm" InvalidStyleDuration="100" DateFormat="HH:mm" Font-Bold="true" ForeColor="Gray" AutoPostBack="true" OnTextChanged="dtpRArrivalFrom_TextChanged">
                                            </telerik:RadDateInput>   
								        </ItemTemplate>
                                        <ItemStyle HorizontalAlign="Left" />
							        </telerik:GridTemplateColumn> 
                                    <telerik:GridTemplateColumn DataField="RArrivalTo" HeaderText="Arrival To <br /> (Ramadan)" SortExpression="RArrivalTo" UniqueName="RArrivalTo">
								        <HeaderStyle Width="105px" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" HorizontalAlign="Center" />
								        <ItemTemplate>   
                                            <telerik:RadDateInput ID="dtpRArrivalTo" runat="server" SelectedDate='<%# Eval("RArrivalTo") %>'  LabelWidth="80px" Width="80px" Culture="en-US" 
                                                EmptyMessage="HH:mm" InvalidStyleDuration="100" DateFormat="HH:mm" Font-Bold="true" ForeColor="Green" AutoPostBack="true" OnTextChanged="dtpRArrivalTo_TextChanged">
                                            </telerik:RadDateInput>   
								        </ItemTemplate>
                                        <ItemStyle HorizontalAlign="Left" />
							        </telerik:GridTemplateColumn> 
                                    <telerik:GridTemplateColumn DataField="RDepartFrom" HeaderText="Depart From <br /> (Ramadan)" SortExpression="RDepartFrom" UniqueName="RDepartFrom">
								        <HeaderStyle Width="105px" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" HorizontalAlign="Center" />
								        <ItemTemplate>   
                                            <telerik:RadDateInput ID="dtpRDepartFrom" runat="server" SelectedDate='<%# Eval("RDepartFrom") %>'  LabelWidth="80px" Width="80px" Culture="en-US" 
                                                EmptyMessage="HH:mm" InvalidStyleDuration="100" DateFormat="HH:mm" Font-Bold="true" ForeColor="Green" AutoPostBack="true" OnTextChanged="dtpRDepartFrom_TextChanged">
                                            </telerik:RadDateInput>   
								        </ItemTemplate>
                                        <ItemStyle HorizontalAlign="Left" />
							        </telerik:GridTemplateColumn> 
                                    <telerik:GridTemplateColumn DataField="RDepartTo" HeaderText="Depart To <br /> (Ramadan)" SortExpression="RDepartTo" UniqueName="RDepartTo">
								        <HeaderStyle Width="105px" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" HorizontalAlign="Center" />
								        <ItemTemplate>   
                                            <telerik:RadDateInput ID="dtpRDepartTo" runat="server" SelectedDate='<%# Eval("RDepartTo") %>'  LabelWidth="80px" Width="80px" Culture="en-US" 
                                                EmptyMessage="HH:mm" InvalidStyleDuration="100" DateFormat="HH:mm" Font-Bold="true" ForeColor="Gray" AutoPostBack="true" OnTextChanged="dtpRDepartTo_TextChanged">
                                            </telerik:RadDateInput>   
								        </ItemTemplate>
                                        <ItemStyle HorizontalAlign="Left" />
							        </telerik:GridTemplateColumn> 
                                    <telerik:GridBoundColumn DataField="DurationRamadanDayString" DataType="System.String" HeaderText="Duration <br /> (Ramadan)" 
                                        ReadOnly="True" SortExpression="DurationRamadanDayString" UniqueName="DurationRamadanDayString">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="90px" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" HorizontalAlign="Center" />
                                        <ItemStyle HorizontalAlign="Center" ForeColor="Red" />
                                    </telerik:GridBoundColumn> 
                                    <telerik:GridTemplateColumn DataField="CreatedByFullName" HeaderText="Created By" DataType="System.String" 
                                        SortExpression="CreatedByFullName" UniqueName="CreatedByFullName">
								        <HeaderStyle Width="200px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								        <ItemTemplate>
									        <div class="columnEllipsis" style="width: 190px; text-align: left;">
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
                                        <HeaderStyle Font-Bold="True" Font-Size="8pt" Width="150px" Font-Names="Tahoma"></HeaderStyle>
                                    </telerik:GridBoundColumn>   
                                    <telerik:GridTemplateColumn DataField="LastUpdateFullName" HeaderText="Last Modified By" DataType="System.String" 
                                        SortExpression="LastUpdateFullName" UniqueName="LastUpdateFullName">
								        <HeaderStyle Width="200px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								        <ItemTemplate>
									        <div class="columnEllipsis" style="width: 190px; text-align: left;">
										        <asp:Literal ID="litLastUpdateFullName" runat="server" Text='<%# Eval("LastUpdateFullName") %>' />
									        </div>
								        </ItemTemplate>
							        </telerik:GridTemplateColumn> 
                                    <telerik:GridBoundColumn DataField="LastUpdateTime" HeaderText="Last Modified Date"
                                        DataFormatString="{0:dd-MMM-yyyy h:mm tt}" DataType="System.DateTime" 
                                        ReadOnly="True" SortExpression="LastUpdateTime" UniqueName="LastUpdateTime">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="150px" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma"></HeaderStyle>
                                    </telerik:GridBoundColumn>   
                                </Columns>
                            </MasterTableView>
                            <ClientSettings AllowColumnsReorder="False" EnableRowHoverStyle="true">
                                <Selecting AllowRowSelect="true" UseClientSelectColumnOnly="True" />
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

        <asp:Panel ID="panGridShiftSequence" runat="server" BorderStyle="None" CssClass="GroupPanelHeader" GroupingText="Shift Timing Sequence:"  style="margin-top: 10px;">
            <table runat="server" id="tblShiftSequenceFilter" border="0" style="width: 100%; text-align: left; margin-top: 0px; table-layout: fixed; padding-top: 0px; margin-top: 0px;">
                <tr>
                    <td class="LabelBold" style="width: 100px;">
                        <asp:CustomValidator ID="cusValWorkShift2" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                        Shift Timing
                    </td>
                    <td style="width: 110px;">
                        <telerik:RadComboBox ID="cboWorkShift2" runat="server"
                            DropDownWidth="105px" 
                            HighlightTemplatedItems="True" 
                            Skin="Office2010Silver" 
                            Width="100%" 
                            EmptyMessage="Select Shift"
                            EnableVirtualScrolling="True">
                        </telerik:RadComboBox> 
                    </td>
                    <td style="width: 70px;">
                        <telerik:RadButton ID="btnAddShift2" runat="server" Text="Add" ToolTip="Add the selected shift timing to the grid" Width="50px" 
                            CssClass="RadButtonStyle" ValidationGroup="valPrimary"  Font-Size="9pt" OnClick="btnAddShift2_Click" Skin="Office2010Silver" />  
                    </td>
                    <td />
                </tr>
            </table>
            <table border="0" style="width: 100%; text-align: left; margin-top: 0px; table-layout: fixed;">
                <tr>
                    <td>
                        <telerik:RadGrid ID="gridShiftSequence" runat="server"
                            AllowSorting="true" AllowMultiRowSelection="true"
                            PageSize="5" AutoGenerateColumns="false" Skin="Silk" 
                            GridLines="None" Width="300px" Height="" CellSpacing="0"
                            onpageindexchanged="gridShiftSequence_PageIndexChanged" 
                            onpagesizechanged="gridShiftSequence_PageSizeChanged" 
                            onsortcommand="gridShiftSequence_SortCommand" 
                            onitemcommand="gridShiftSequence_ItemCommand" 
                            onitemdatabound="gridShiftSequence_ItemDataBound" 
                            Font-Names="Tahoma" Font-Size="9pt" Font-Bold="False" 
                            AllowPaging = "true" BorderStyle="Outset" BorderWidth="1px">
                            <ExportSettings ExportOnlyData="true" IgnorePaging="true" OpenInNewWindow="true" FileName="ShiftPatternChangeList" HideStructureColumns="true">
                                <Pdf PageHeight="210mm" PageWidth="310mm" PageTitle="Shift Pattern Changes List" DefaultFontFamily="Arial Unicode MS"
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
                                        ReadOnly="True" SortExpression="ShiftPatCode" UniqueName="ShiftPatCode" Visible="false">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Font-Bold="True" />
                                    </telerik:GridBoundColumn>    
                                    <telerik:GridButtonColumn ButtonType="ImageButton" CommandName="Select" HeaderText="" CommandArgument="DeleteButton" 
                                        UniqueName="DeleteButton" ImageUrl="~/Images/delete_grid_icon.jpg" HeaderTooltip="Delete the selected record">
                                        <HeaderStyle Font-Bold="True" Width="35px"></HeaderStyle>
                                        <ItemStyle Font-Bold="true" ForeColor="Blue" HorizontalAlign="Center" />
				                    </telerik:GridButtonColumn>  
                                    <telerik:GridBoundColumn DataField="ShiftPointer" DataType="System.Int32" HeaderText="Shift Pointer" 
                                        ReadOnly="True" SortExpression="ShiftPointer" UniqueName="ShiftPointer">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="105px" Font-Bold="True" HorizontalAlign="Center" />
                                        <ItemStyle Font-Bold="true" ForeColor="Purple" HorizontalAlign="Center" />
                                    </telerik:GridBoundColumn>  
                                    <telerik:GridTemplateColumn DataField="ShiftFullDescription" HeaderText="Shift Timing" DataType="System.String" 
                                        SortExpression="ShiftFullDescription" UniqueName="ShiftFullDescription">
								        <HeaderStyle HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								        <ItemTemplate>     
                                            <telerik:RadComboBox ID="cboShiftTiming" runat="server"
							                    DropDownWidth="140px" 
							                    HighlightTemplatedItems="True" 
							                    MarkFirstMatch="true" 
							                    Skin="Office2010Silver" 
							                    Width="100%"  
							                    EmptyMessage="Select Shift"
							                    EnableLoadOnDemand="true"
							                    EnableVirtualScrolling="true" 
                                                Text='<%# Eval("ShiftFullDescription")%>'
                                                Value='<%# Eval("ShiftCode")%>'
                                                Font-Names="Tahoma" 
                                                Font-Size="9pt"
                                                AutoPostBack="true"
                                                Filter="None"
                                                BackColor="Yellow"
                                                ToolTip="(Note: Shift Timing is a mandatory field which should not be left blank.)"
                                                OnItemsRequested="cboShiftTiming_ItemsRequested"
                                                OnSelectedIndexChanged="cboShiftTiming_SelectedIndexChanged">
                                            </telerik:RadComboBox>                                       
								        </ItemTemplate>
							        </telerik:GridTemplateColumn>                                    
                                </Columns>
                            </MasterTableView>
                            <ClientSettings AllowColumnsReorder="False" EnableRowHoverStyle="true">
                                <Selecting AllowRowSelect="true" UseClientSelectColumnOnly="True" />
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

        <asp:Panel ID="panButton" runat="server" BorderStyle="None" style="margin-top: 5px;">
            <table border="0" style="width: 100%; text-align: left; margin-top: 0px; table-layout: fixed; padding-top: 0px; margin-top: 0px;">
                <tr>
                    <td style="width: 500px; padding-left: 20px;">
                        <asp:CustomValidator ID="cusValButton" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" />                         
                        <telerik:RadButton ID="btnSave" runat="server" Text="Save Shift Pattern Details" ToolTip="Save data changes"  Width="160px"
                            CssClass="RadButtonStyle" ValidationGroup="valPrimary" Font-Size="9pt" OnClick="btnSave_Click" Skin="Office2010Silver" Enabled="False" />
                        <telerik:RadButton ID="btnDelete" runat="server" Text="Delete" ToolTip="Delete selected record(s)"  Width="80px" Visible="false"
                            CssClass="RadButtonStyle" ValidationGroup="valPrimary" Font-Size="9pt" OnClick="btnDelete_Click" Skin="Office2010Silver" />
                        <telerik:RadButton ID="btnReset" runat="server" Text="Reset" ToolTip="Reset filter criterias" Width="80px" 
                            CssClass="RadButtonStyle" CausesValidation="false" Font-Size="9pt" OnClick="btnReset_Click" Skin="Office2010Silver" />                                                                        
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
        <telerik:RadButton ID="btnRebindShiftPointer" runat="server" Text="" Skin="Office2010Silver" CausesValidation="false" onclick="btnRebindShiftPointer_Click" />
        <telerik:RadButton ID="btnDeleteDummy" runat="server" Text="" Skin="Office2010Silver" ValidationGroup="valPrimary" onclick="btnDeleteDummy_Click" />      
        <telerik:RadButton ID="btnRemoveShiftTiming" runat="server" Text="" Skin="Office2010Silver" CausesValidation="false" onclick="btnRemoveShiftTiming_Click" />  
        <telerik:RadButton ID="btnRemoveShiftSequence" runat="server" Text="" Skin="Office2010Silver" CausesValidation="false" onclick="btnRemoveShiftSequence_Click" />  
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
                <telerik:AjaxSetting AjaxControlID="btnNew">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>                      
                <telerik:AjaxSetting AjaxControlID="btnDelete">
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
                <telerik:AjaxSetting AjaxControlID="btnDeleteDummy">
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
                <telerik:AjaxSetting AjaxControlID="btnRebindShiftPointer">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>    
                <telerik:AjaxSetting AjaxControlID="btnAddShift">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <%--<telerik:AjaxUpdatedControl ControlID="panGridShiftSequence" />--%>  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>  
                <telerik:AjaxSetting AjaxControlID="btnAddShift2">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>  
                <telerik:AjaxSetting AjaxControlID="btnRemoveShiftTiming">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>                     
                <telerik:AjaxSetting AjaxControlID="btnRemoveShiftSequence">
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
                <telerik:AjaxSetting AjaxControlID="btnSaveShiftPattern">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>
                <telerik:AjaxSetting AjaxControlID="btnDeleteShiftPattern">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>
                <telerik:AjaxSetting AjaxControlID="btnCancelShiftPattern">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>
                <telerik:AjaxSetting AjaxControlID="btnShiftPatternDetail">
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
                <telerik:AjaxSetting AjaxControlID="gridShiftTiming">
				    <UpdatedControls>
                        <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="gridShiftTiming" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>   
                <telerik:AjaxSetting AjaxControlID="gridShiftSequence">
				    <UpdatedControls>
                        <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="gridShiftSequence" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>  
                <telerik:AjaxSetting AjaxControlID="cboShiftPattern">
				    <UpdatedControls>
                        <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>  
                <telerik:AjaxSetting AjaxControlID="cboWorkShift2">
				    <UpdatedControls>
                        <telerik:AjaxUpdatedControl ControlID="panGridShiftSequence" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>                  
            </AjaxSettings>
        </telerik:RadAjaxManager>
        <telerik:RadAjaxLoadingPanel ID="loadingPanel" runat="server" Skin="Office2010Silver"></telerik:RadAjaxLoadingPanel>
    </asp:Panel>
</asp:Content>
