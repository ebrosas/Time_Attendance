<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/TASMaster.Master" AutoEventWireup="true" CodeBehind="VisitorPassInquiry.aspx.cs" 
    Inherits="GARMCO.AMS.TAS.UI.Views.UserFunctions.VisitorPassInquiry" StylesheetTheme="Standard" %>

<%@ MasterType VirtualPath="~/Views/Shared/TASMaster.Master" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <title>Employee's Self Service</title>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="mainContent" runat="server">
    <telerik:RadFormDecorator ID="formDecor" runat="server" DecoratedControls="Buttons" Skin="Office2010Silver" />

    <table border="0" style="width: 100%; text-align: left; margin-top: 5px; margin-left: 5px;">
        <tr>
            <td colspan="2" style="padding-left: 10px;">
                <table border="0" style="width: 100%; text-align: left;">
                    <tr>
                        <td style="width: 50px; text-align: right; padding-right: 5px;" rowspan="2">
                            <img alt="" src="../../Images/visitorpass_inquiry_icon.png" />
                        </td>
                        <td class="PageTitleLabel" style="vertical-align: bottom; padding-left: 5px; width: 900px; font-size: 11pt;">
                            Visitor's Pass Inquiry
                        </td>
                        <td style="width: 50px;">
                            <asp:LinkButton ID="lnkMoveUp" runat="server" />
                        </td>
                        <td />
                    </tr>
                    <tr>
                        <td class="PageDescriptionHeader" style="text-align: left; vertical-align: top; padding-left: 5px; margin: 0px;">
                            Allows Security Personnel to search for the Visitors' visit history in the company
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
                    <td class="LabelBold" style="width: 160px;">
                        Visitor Name                 
                    </td>
                    <td style="width: 300px;">
                        <telerik:RadTextBox ID="txtVisitorName" runat="server" Width="100%" 
                            EmptyMessage="Enter Visitor Name" Skin="Windows7" ToolTip="Maximum text input is 100 chars." 
                            Font-Names="Tahoma" Font-Size="9pt" MaxLength="100" />
                    </td>
                    <td class="LabelBold" style="width: 140px;">
                        <asp:CustomValidator ID="cusValVisitDate" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                        Visit Date
                    </td>
                    <td style="width: 300px;">
                        <table border="0" style="width: 100%; text-align: left; margin: 0px; padding: 0px; table-layout: fixed;">
                            <tr>
                                <td style="width: 120px; padding-left: 0px;">
                                    <telerik:RadDatePicker ID="dtpVisitStartDate" runat="server"
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
                                    <telerik:RadDatePicker ID="dtpVisitEndDate" runat="server"
                                        Width="120px" Skin="Windows7">
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
                <tr style="height: 23px;">
                    <td class="LabelBold">
                        GARMCO Visitor Card No.
                    </td>
                    <td>
                        <telerik:RadNumericTextBox ID="txtVisitorCardNo" runat="server" width="170px" 
                            MinValue="0" ToolTip="(Note: Value entered must be numeric and should not exceed 12 digits)" 
                            Skin="Windows7" DataType="System.Int32" MaxLength="12" MaxValue="999999999999" 
                            EmptyMessage="Enter Visitor Card No.">
                            <NumberFormat ZeroPattern="n" DecimalDigits="0" GroupSeparator="" />
                        </telerik:RadNumericTextBox>                         
                    </td>
                    <td class="LabelBold">
                        Visited Department
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
                    <td />
                </tr>
                <tr style="height: 23px;">
                    <td class="LabelBold">
                        ID Number
                    </td>
                    <td>
                        <telerik:RadTextBox ID="txtIDNumber" runat="server" Width="170px" 
                            EmptyMessage="Enter CPR or other ID" Skin="Windows7" ToolTip="Maximum text input is 50 chars." 
                            Font-Names="Tahoma" Font-Size="9pt" MaxLength="50" />
                    </td>
                    <td class="LabelBold">
                        Visited Emp. No.
                    </td>
                    <td colspan="2" style="padding-left: 0px; margin-left: 0px;">
                        <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                            <tr style="margin: 0px; padding: 0px;">
                                <td style="width: 120px; text-align: left;">
                                    <telerik:RadNumericTextBox ID="txtEmpNo" runat="server" width="100%" 
                                        MinValue="0" ToolTip="Enter Employee No. or Contractor ID" 
                                        Skin="Vista" DataType="System.Int32" MaxLength="12" MaxValue="999999999999" 
                                        EmptyMessage="" >
                                        <NumberFormat ZeroPattern="n" DecimalDigits="0" GroupSeparator="" />
                                    </telerik:RadNumericTextBox> 
                                </td>
                                <td style="width: auto; padding-left: 0px; margin: 0px; text-align: right; vertical-align: top;">
                                    <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                                        <tr style="margin: 0px; padding: 0px;">
                                            <td style="width: 30px; text-align: left;">
                                                <telerik:RadButton ID="btnFindEmployee" runat="server" Skin="Office2010Silver" 
                                                    Text="..." ToolTip="Click here to search for an employee." Enabled="true" 
                                                    Width="100%" Font-Bold="False" Font-Size="9pt" CausesValidation="false"
                                                    onclick="btnFindEmployee_Click">
                                                </telerik:RadButton>
                                            </td>
                                            <td class="TextNormal"  style="padding-left: 10px; text-align: left;">
                                                <asp:Literal ID="litEmpName" runat="server" Text="" />                                  
                                            </td>
                                        </tr>
                                    </table>                                                                        
                                </td> 
                            </tr>
                        </table>
                    </td>
                </tr>
                <tr style="height: 23px;">
                    <td class="LabelBold">
                        Blocked Visitors?
                    </td>
                    <td>
                        <asp:RadioButtonList ID="rblBlockOption" runat="server" RepeatDirection="Horizontal" AutoPostBack="True">                            
                            <asp:ListItem Text="Yes" Value="valYes" />
                            <asp:ListItem Text="No" Value="valNo" />
                            <asp:ListItem Text="All" Value="valAll" Selected="True" />
                        </asp:RadioButtonList>
                    </td>
                    <td class="LabelBold">
                        Created By
                    </td>
                    <td style="padding-left: 4px;">
                        <table border="0" style="width: 100%; text-align: left; margin: 0px; padding: 0px; table-layout: fixed;">
                            <tr>
                                <td style="width: 150px; text-align: left; padding-left: 0px;">
                                    <asp:RadioButtonList ID="rblCreatedBy" runat="server" 
                                        RepeatDirection="Horizontal" AutoPostBack="True" OnSelectedIndexChanged="rblCreatedBy_SelectedIndexChanged">
                                        <asp:ListItem Text="All" Value="0" Selected="True" />
                                        <asp:ListItem Text="Me" Value="1" />
                                        <asp:ListItem Text="Others" Value="2" />
                                    </asp:RadioButtonList>
                                </td>
                                <td style="text-align: left; width: 100px;">
                                    <telerik:RadNumericTextBox ID="txtOtherEmpNo" runat="server" width="100%" 
                                        MinValue="0" ToolTip="(Note: Employee No. must start with 1000. Example: 10003632)" 
                                        Skin="Vista" DataType="System.Int32" MaxLength="8" MaxValue="99999999" 
                                        EmptyMessage="1000xxxx" Visible="false">
                                        <NumberFormat ZeroPattern="n" DecimalDigits="0" GroupSeparator="" />
                                    </telerik:RadNumericTextBox> 
                                </td>
                                <td style="width: 35px; padding-right: 5px;">
                                    <telerik:RadButton ID="btnFindOtherEmp" runat="server" Skin="Office2010Silver" Width="100%" 
                                        Text="..." ToolTip="Click here to search for an employee." Enabled="true" Visible="false" 
                                        Font-Bold="False" Font-Size="9pt" CausesValidation="false"
                                        onclick="btnFindOtherEmp_Click">
                                    </telerik:RadButton>
                                </td> 
                                <td />
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
                    <td colspan="4" style="padding-top: 2px;">
                        <telerik:RadButton ID="btnSearch" runat="server" Text="Search" ToolTip="Search for training records" Width="80px" 
                            CssClass="RadButtonStyle" ValidationGroup="valPrimary" Font-Size="9pt" OnClick="btnSearch_Click" Skin="Office2010Silver" />
                        <telerik:RadButton ID="btnReset" runat="server" Text="Reset Criteria" ToolTip="Reset filter criterias" Width="110px" 
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
                        <asp:Label ID="lblRecordCount" runat="server" Text="0 record(s) found" Width="100%" />                         
                    </td>
                </tr>
            </table>
            <table id="tblGrid" runat="server" border="0" style="width: 100%; text-align: left; margin-top: 0px; table-layout: fixed;">
                <tr>
                    <td>
                        <telerik:RadGrid ID="gridVisitor" runat="server"
                            AllowSorting="true" AllowMultiRowSelection="true"
                            PageSize="10" AutoGenerateColumns="false" Skin="Silk" 
                            GridLines="None" Width="100%" Height="" CellSpacing="0"
                            onpageindexchanged="gridVisitor_PageIndexChanged" 
                            onpagesizechanged="gridVisitor_PageSizeChanged" 
                            onsortcommand="gridVisitor_SortCommand" 
                            onitemcommand="gridVisitor_ItemCommand" 
                            onitemdatabound="gridVisitor_ItemDataBound" 
                            Font-Names="Tahoma" Font-Size="9pt" Font-Bold="False" 
                            AllowPaging = "true" OnPreRender="gridVisitor_PreRender" BorderStyle="Outset" BorderWidth="1px"
                            AllowCustomPaging="True" VirtualItemCount="1">
                            <ExportSettings ExportOnlyData="true" IgnorePaging="true" OpenInNewWindow="true" FileName="VisitorPassList" HideStructureColumns="true">
                                <Pdf PageHeight="210mm" PageWidth="310mm" PageTitle="Visitor Pass History" DefaultFontFamily="Arial Unicode MS"
                                PageBottomMargin="20mm" PageTopMargin="20mm" PageLeftMargin="20mm" PageRightMargin="20mm" PaperSize="A4" AllowPrinting="true" />
                            </ExportSettings>
                            <AlternatingItemStyle Font-Names="Tahoma" Font-Size="9pt" Wrap="True" />
                            <MasterTableView DataKeyNames="LogID" ClientDataKeyNames="LogID" 
                                NoMasterRecordsText="No Visitor Pass record found!" 
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
                                        UniqueName="CheckboxSelectColumn" >                                                                                            
                                        <HeaderStyle Font-Bold="True" Font-Size="9pt" Width="35px" HorizontalAlign="Center" />
                                        <ItemStyle HorizontalAlign="Center" />
                                    </telerik:GridClientSelectColumn>                                    
                                    <telerik:GridButtonColumn ButtonType="ImageButton" CommandName="Select" HeaderText="" CommandArgument="PrintButton" 
                                        UniqueName="PrintButton" ImageUrl="~/Images/printer.png" HeaderTooltip="View and print Employee Training Report">
                                        <HeaderStyle Font-Bold="True" Width="40px"></HeaderStyle>
                                        <ItemStyle Font-Bold="true" ForeColor="Blue" HorizontalAlign="Center" Font-Size="9pt" />
				                    </telerik:GridButtonColumn>       
                                   <%-- <telerik:GridButtonColumn ButtonType="LinkButton" CommandName="Select" Text="Select" UniqueName="SelectLinkButton" Visible="false">
                                        <HeaderStyle Width="50px" HorizontalAlign="Center" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
                                        <ItemStyle HorizontalAlign="Center" Font-Bold="true" Font-Size="9pt" ForeColor="Blue" />
					                </telerik:GridButtonColumn> --%>
                                    <telerik:GridTemplateColumn DataField="VisitorName" HeaderText="Visitor Name" 
                                        SortExpression="VisitorName" UniqueName="VisitorName">
								        <HeaderStyle Width="230px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								        <ItemTemplate>
									        <div class="columnEllipsis" style="width: 220px; text-align: left;">
                                                <asp:LinkButton ID="lnkVisitorName" runat="server" Text='<%# Eval("VisitorName") %>' 
                                                    Font-Bold="true" ForeColor="Blue" OnClick="lnkVisitorName_Click" />
									        </div>
								        </ItemTemplate>
							        </telerik:GridTemplateColumn> 
                                    <telerik:GridBoundColumn DataField="VisitorCardNo" DataType="System.Int32" HeaderText="Visitor Card No." SortExpression="VisitorCardNo" UniqueName="VisitorCardNo">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="120px" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma"></HeaderStyle>
                                    </telerik:GridBoundColumn>
                                    <telerik:GridBoundColumn DataField="IDNumber" DataType="System.String" HeaderText="ID Number" SortExpression="IDNumber" UniqueName="IDNumber">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="100px" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma"></HeaderStyle>
                                    </telerik:GridBoundColumn>                                    
                                    <telerik:GridBoundColumn DataField="VisitDate" HeaderText="Visit Date"
                                        DataFormatString="{0:dd-MMM-yyyy}" DataType="System.DateTime" 
                                        ReadOnly="True" SortExpression="VisitDate" UniqueName="VisitDate">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="90px" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma"></HeaderStyle>
                                    </telerik:GridBoundColumn>   
                                    <telerik:GridBoundColumn DataField="VisitTimeIn" HeaderText="Time In"
                                        DataFormatString="{0:HH:mm:ss}" DataType="System.DateTime" Display="false" 
                                        ReadOnly="True" SortExpression="VisitTimeIn" UniqueName="VisitTimeIn">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="70px" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" HorizontalAlign="Center"></HeaderStyle>
                                        <ItemStyle HorizontalAlign="Center" />
                                    </telerik:GridBoundColumn>  
                                    <telerik:GridBoundColumn DataField="VisitTimeOut" HeaderText="Time Out"
                                        DataFormatString="{0:HH:mm:ss}" DataType="System.DateTime" Display="false" 
                                        ReadOnly="True" SortExpression="VisitTimeOut" UniqueName="VisitTimeOut">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="70px" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" HorizontalAlign="Center"></HeaderStyle>
                                        <ItemStyle HorizontalAlign="Center" />
                                    </telerik:GridBoundColumn>   
                                    <telerik:GridTemplateColumn DataField="VisitEmpFullName" HeaderText="Visited Employee Name" 
                                        SortExpression="VisitEmpFullName" UniqueName="VisitEmpFullName">
								        <HeaderStyle Width="280px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								        <ItemTemplate>
									        <div class="columnEllipsis" style="width: 270px; text-align: left;">
										        <asp:Literal ID="litVisitEmpFullName" runat="server" Text='<%# Eval("VisitEmpFullName") %>' />
									        </div>
								        </ItemTemplate>
							        </telerik:GridTemplateColumn>  
                                    <telerik:GridTemplateColumn DataField="VisitEmpFullCostCenter" HeaderText="Visited Department" 
                                        SortExpression="VisitEmpFullCostCenter" UniqueName="VisitEmpFullCostCenter">
								        <HeaderStyle Width="280px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								        <ItemTemplate>
									        <div class="columnEllipsis" style="width: 270px; text-align: left;">
										        <asp:Literal ID="litVisitEmpFullCostCenter" runat="server" Text='<%# Eval("VisitEmpFullCostCenter") %>' />
									        </div>
								        </ItemTemplate>
							        </telerik:GridTemplateColumn>  
                                    <telerik:GridTemplateColumn DataField="IsBlock" HeaderText="Is Blocked?" SortExpression="IsBlock" UniqueName="IsBlock">
								        <HeaderStyle Width="90px" HorizontalAlign="Center" />
								        <ItemTemplate>
									        <div style="width: 90px; text-align: center;">
										        <asp:Label ID="lblIsBlock" runat="server" Text='<%# Convert.ToBoolean(Eval("IsBlock")) == true ? "Yes" : "No" %>'></asp:Label>  
									        </div>
								        </ItemTemplate>
							        </telerik:GridTemplateColumn> 
                                    <telerik:GridBoundColumn DataField="CreatedByEmpNo" DataType="System.Int32" Display="false" 
                                        ReadOnly="True" SortExpression="CreatedByEmpNo" UniqueName="CreatedByEmpNo">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                    </telerik:GridBoundColumn>
                                    <telerik:GridTemplateColumn DataField="CreatedByFullName" HeaderText="Created By" SortExpression="CreatedByFullName" UniqueName="CreatedByFullName">
								        <HeaderStyle Width="250px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								        <ItemTemplate>
									        <div class="columnEllipsis" style="width: 240px; text-align: left;">
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
                                        <HeaderStyle Width="150px" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma"></HeaderStyle>
                                    </telerik:GridBoundColumn>                                    
                                    <telerik:GridTemplateColumn DataField="LastUpdateFullName" HeaderText="Last Modified By" SortExpression="LastUpdateFullName" UniqueName="LastUpdateFullName">
								        <HeaderStyle Width="250px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								        <ItemTemplate>
									        <div class="columnEllipsis" style="width: 240px; text-align: left;">
										        <asp:Literal ID="litLastModifiedBy" runat="server" Text='<%# Eval("LastUpdateFullName") %>' />
									        </div>
								        </ItemTemplate>
							        </telerik:GridTemplateColumn>   
                                    <telerik:GridBoundColumn DataField="LastUpdateTime" HeaderText="Last Modified Date"
                                        DataFormatString="{0:dd-MMM-yyyy h:mm tt}" DataType="System.DateTime" 
                                        FilterControlAltText="Filter Modified Date column" ReadOnly="True" SortExpression="LastUpdateTime" UniqueName="LastUpdateTime">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="150px" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma"></HeaderStyle>
                                    </telerik:GridBoundColumn>     
                                </Columns>
                            </MasterTableView>
                            <ClientSettings AllowColumnsReorder="False" EnableRowHoverStyle="true">
                                <Selecting AllowRowSelect="True" UseClientSelectColumnOnly="True" />
                                <Scrolling AllowScroll="true" UseStaticHeaders="true" FrozenColumnsCount="4" SaveScrollPosition="true" ScrollHeight="" />
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
                    <td style="padding-top: 5px; text-align: left;">
                        <asp:CustomValidator ID="cusValApproval" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 

                        <telerik:RadButton ID="btnNewRecord" runat="server" Text="Create New" ToolTip="Create new training record"  Width="90px"
                            CssClass="RadButtonStyle" CausesValidation="false"  Font-Size="9pt" OnClick="btnNewRecord_Click" Skin="Office2010Silver" />
                        <telerik:RadButton ID="btnDelete" runat="server" Text="Delete" ToolTip="Delete selected training records"  Width="90px"
                            CssClass="RadButtonStyle" ValidationGroup="valPrimary" Font-Size="9pt" OnClick="btnDelete_Click" Skin="Office2010Silver" />                        
                        <telerik:RadButton ID="btnPrint" runat="server" Text="Print Report" ToolTip="Show and print the report" Width="100px" 
                            CssClass="RadButtonStyle" ValidationGroup="valPrimary" Font-Size="9pt" OnClick="btnPrint_Click" Skin="Office2010Silver" />                                                
                        <telerik:RadButton ID="btnExportToExcel" runat="server" Text="Export to Excel..." ToolTip="Export trainign records into Excel sheet"  Width="115px" Visible="false"
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
        <telerik:RadButton ID="btnDeleteDummy" runat="server" Text="" Skin="Office2010Silver" ValidationGroup="valPrimary" onclick="btnDeleteDummy_Click" />      
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
                <telerik:AjaxSetting AjaxControlID="btnNewRecord">
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
                <telerik:AjaxSetting AjaxControlID="btnDeleteDummy">
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
                <telerik:AjaxSetting AjaxControlID="btnRebind">
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
                <telerik:AjaxSetting AjaxControlID="btnFindOtherEmp">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting> 
                <telerik:AjaxSetting AjaxControlID="gridVisitor">
				    <UpdatedControls>
                        <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="gridTraining" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>                                       
            </AjaxSettings>
        </telerik:RadAjaxManager>
        <telerik:RadAjaxLoadingPanel ID="loadingPanel" runat="server" Skin="Office2010Silver"></telerik:RadAjaxLoadingPanel>
    </asp:Panel>

    <asp:Panel ID="panelDataSources" runat="server" style="display: none;">
        <asp:ObjectDataSource ID="objCostCenter" runat="server" OnSelected="objCostCenter_Selected" OldValuesParameterFormatString="" SelectMethod="GetCostCenter" TypeName="GARMCO.Common.DAL.Employee.EmployeeBLL">
			<SelectParameters>
				<asp:Parameter Name="costCenter" Type="String" />
				<asp:Parameter Name="costCenterName" Type="String" />
				<asp:Parameter Name="sort" Type="String" />
			</SelectParameters>
		</asp:ObjectDataSource>
    </asp:Panel>
</asp:Content>
