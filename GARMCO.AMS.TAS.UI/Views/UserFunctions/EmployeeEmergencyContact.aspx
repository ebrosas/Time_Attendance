﻿<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/TASMaster.Master" AutoEventWireup="true" CodeBehind="EmployeeEmergencyContact.aspx.cs" 
    Inherits="GARMCO.AMS.TAS.UI.Views.UserFunctions.EmployeeEmergencyContact" StylesheetTheme="Standard" %>

<%@ MasterType VirtualPath="~/Views/Shared/TASMaster.Master" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <title>Employee Emergency Contact</title>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="mainContent" runat="server">
    <telerik:RadFormDecorator ID="formDecor" runat="server" DecoratedControls="Buttons" Skin="Office2010Silver" />

    <table border="0" style="width: 100%; text-align: left; margin-top: 5px; margin-left: 5px;">
        <tr>
            <td colspan="2" style="padding-left: 5px;">
                <table border="0" style="width: 100%; text-align: left;">
                    <tr>
                        <td style="width: 50px; text-align: right; padding-right: 5px;" rowspan="2">
                            <img alt="" src="../../Images/emergency_contact_icon.jpg" />
                        </td>
                        <td id="tdPageTitle" runat="server" class="PageTitleLabel" style="vertical-align: bottom; padding-left: 0px; width: 900px; font-size: 11pt;">
                            Employee Emergency Contact
                        </td>
                        <td style="width: 50px;">
                            <asp:LinkButton ID="lnkMoveUp" runat="server" />
                        </td>
                        <td />
                    </tr>
                    <tr>
                        <td class="PageDescriptionHeader" style="text-align: left; vertical-align: top; padding-left: 0px; margin: 0px;">
                            View the employee's emergency contact information 
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
                    <td id="tdEmployeeTitle" runat="server" class="LabelBold" style="width: 130px;">
                        Employee No.      
                    </td>
                    <td style="width: 250px;">
                        <table id="tdEmployee" runat="server" border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                            <tr style="vertical-align: top; margin: 0px; padding: 0px;">
                                <td style="width: 110px; text-align: left;">
                                    <telerik:RadNumericTextBox ID="txtEmpNo" runat="server" width="100%" 
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
                    <td style="padding-left: 4px;">
                        <telerik:RadComboBox ID="cboCostCenter" runat="server" 
                            DropDownWidth="330px"    
                            Width="100%" Height="200px"                                
                            Filter="Contains" Skin="Office2010Silver" 
                            EmptyMessage="Select Cost Center"                               
                            HighlightTemplatedItems="True" 
                            MarkFirstMatch="True" EnableVirtualScrolling="true" AutoPostBack="False">
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
                        Enter Search String          
                    </td>
                    <td style="padding-left: 4px;">
                        <telerik:RadTextBox ID="txtSearchString" runat="server" Width="100%" 
                            EmptyMessage="Type search keyword here" Skin="Office2010Silver" ToolTip="Maximum text input is 100 chars." 
                            Font-Names="Tahoma" Font-Size="9pt" MaxLength="100" />
                    </td>
                    <td colspan="3" class="LabelNotes" style="text-align: left; color: silver; font-style: normal; font-size: 9pt; padding-left: 10px;">
                        (Note: The system searches the following fields for matching records: Employee Name and Position) 
                    </td>
                </tr>  
                <tr style="height: 23px;">
                    <td class="LabelBold">
                        <asp:CustomValidator ID="cusValButton" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                    </td>
                    <td colspan="3" style="padding-left: 2px; padding-top: 5px;">
                        <telerik:RadButton ID="btnSearch" runat="server" Text="Search" ToolTip="Search matching database records" Width="70px" 
                            CssClass="RadButtonStyle" ValidationGroup="valPrimary" Font-Size="9pt" OnClick="btnSearch_Click" Skin="Office2010Silver" />
                        <telerik:RadButton ID="btnReset" runat="server" Text="Reset" ToolTip="Reset filter criterias" Width="70px" 
                            CssClass="RadButtonStyle" ValidationGroup="valPrimary" Font-Size="9pt" OnClick="btnReset_Click" Skin="Office2010Silver" />                                                
                        <telerik:RadButton ID="btnExport" runat="server" Text="Export to Excel" ToolTip="Export raw data to Excel" Width="110px" Visible="false"  
                            CssClass="RadButtonStyle" ValidationGroup="valPrimary" Font-Size="9pt" OnClick="btnExport_Click" Skin="Office2010Silver" />                                                
                    </td>
                    <td />
                </tr>                
            </table>
        </asp:Panel>

        <asp:Panel ID="panGrid" runat="server" BorderStyle="None" style="padding-left: 15px; padding-right: 15px; margin: 0px;">
             <table border="0" style="width: 100%; text-align: left; margin-top: 0px; table-layout: fixed;">
                <tr>
                    <td style="text-align: left; color: Purple; font-weight: bold; font-size: 9pt; padding-left: 5px; width: 300px;">
                        <asp:Label ID="lblRecordCount" runat="server" Text="0 record found" Width="100%" />                         
                    </td>
                    <td style="width: auto; text-align: right; padding-right: 0px;">
                        <table border="0" style="width: 100%; margin: 0px; table-layout: fixed;">
                            <tr style="margin: 0px; padding: 0px; vertical-align: top;">       
                                <td style="width: auto;" />                         
                                <td class="LabelBold" style="width: 120px; color: DarkGoldenrod; padding-top: 3px; padding-right: 0px;">
                                    Show Photo
                                </td>
                                <td style="width: 20px; padding-top: 0px; text-align: left; padding-left: 0px; margin-left: 0px;">
                                    <asp:CheckBox ID="chkShowPhoto" runat="server" Text="" AutoPostBack="True" OnCheckedChanged="chkShowPhoto_CheckedChanged" Checked="true" />
                                </td>
                            </tr>
                        </table>    
                    </td>
                </tr>
            </table>
            <table id="tblGrid" runat="server" border="0" style="width: 100%; text-align: left; margin-top: 0px; padding-top: 0px; table-layout: fixed;">
                <tr>
                    <td>
                        <telerik:RadGrid ID="gridEmployeeDetail" runat="server"
                            AllowSorting="true" PageSize="10" AutoGenerateColumns="false" Skin="Silk" 
                            GridLines="None" Width="100%" Height="" CellSpacing="0"
                            onpageindexchanged="gridEmployeeDetail_PageIndexChanged" 
                            onpagesizechanged="gridEmployeeDetail_PageSizeChanged" 
                            onsortcommand="gridEmployeeDetail_SortCommand" 
                            onitemcommand="gridEmployeeDetail_ItemCommand" 
                            onitemdatabound="gridEmployeeDetail_ItemDataBound" 
                            Font-Names="Tahoma" Font-Size="9pt" Font-Bold="False"                             
                            AllowPaging = "true" BorderStyle="Outset" BorderWidth="1px" OnPreRender="gridEmployeeDetail_PreRender">
                            <GroupingSettings CollapseAllTooltip="Collapse all groups" />
                            <ExportSettings ExportOnlyData="true" IgnorePaging="true" OpenInNewWindow="true" FileName="Employee_Shift_Pattern" HideStructureColumns="true">
                                <Pdf PageHeight="210mm" PageWidth="310mm" PageTitle="Shift Pattern List" DefaultFontFamily="Arial Unicode MS"
                                PageBottomMargin="20mm" PageTopMargin="20mm" PageLeftMargin="20mm" PageRightMargin="20mm" PaperSize="A4" AllowPrinting="true" />
                            </ExportSettings>
                            <AlternatingItemStyle Font-Names="Tahoma" Font-Size="9pt" Wrap="True" />
                            <MasterTableView DataKeyNames="EmpNo" ClientDataKeyNames="EmpNo" 
                                NoMasterRecordsText="No employee records found." TableLayout="Fixed" PagerStyle-AlwaysVisible="True" Font-Names="Tahoma" 
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
                                    <telerik:GridTemplateColumn DataField="EmployeeImagePath" HeaderText="" Visible="false" 
                                        SortExpression="EmployeeImagePath" UniqueName="EmployeeImagePath">
								        <HeaderStyle Width="65px" HorizontalAlign="Left" />
								        <ItemTemplate>
									        <div style="width: 100%; text-align: left;">                                                
                                                <img id="imgPhoto" runat="server"  
                                                    src='<%# Eval("EmployeeImagePath") %>' 
                                                    alt='<%# Eval("EmployeeImageTooltip") %>' 
                                                    style="height: 60px; width: 60px;" />
									        </div>
								        </ItemTemplate>
							        </telerik:GridTemplateColumn> 
                                    <telerik:GridBoundColumn DataField="EmpNo" DataType="System.Int32" HeaderText="Emp. No." 
                                        ReadOnly="True" SortExpression="EmpNo" UniqueName="EmpNo">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="90px" Font-Bold="True" />
                                        <ItemStyle Font-Bold="true" ForeColor="Purple" />
                                    </telerik:GridBoundColumn> 
                                    <telerik:GridBoundColumn DataField="EmpName" DataType="System.String" HeaderText="Employee Name" 
                                        ReadOnly="True" SortExpression="EmpName" UniqueName="EmpName">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="260px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
                                    </telerik:GridBoundColumn>
                                    <telerik:GridBoundColumn DataField="Position" DataType="System.String" HeaderText="Position" 
                                        ReadOnly="True" SortExpression="Position" UniqueName="Position">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="240px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
                                    </telerik:GridBoundColumn>
                                    <telerik:GridBoundColumn DataField="PayGrade" DataType="System.Int32" HeaderText="Pay Grade" 
                                        ReadOnly="True" SortExpression="PayGrade" UniqueName="PayGrade">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="80px" Font-Bold="True" Font-Size="8pt" HorizontalAlign="Left" />
                                        <ItemStyle HorizontalAlign="Center" />
                                    </telerik:GridBoundColumn> 
                                    <telerik:GridBoundColumn DataField="CostCenter" DataType="System.String" HeaderText="Cost Center" 
                                        ReadOnly="True" SortExpression="CostCenter" UniqueName="CostCenter">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="90px" Font-Bold="True" />
                                    </telerik:GridBoundColumn> 
                                    <telerik:GridBoundColumn DataField="CostCenterName" DataType="System.String" HeaderText="Cost Center Name" 
                                        ReadOnly="True" SortExpression="CostCenterName" UniqueName="CostCenterName">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="260px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
                                    </telerik:GridBoundColumn>
                                    <telerik:GridBoundColumn DataField="SupervisorFullName" DataType="System.String" HeaderText="Direct Supervisor" 
                                        ReadOnly="True" SortExpression="SupervisorFullName" UniqueName="SupervisorFullName">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="300px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
                                    </telerik:GridBoundColumn>
                                    <telerik:GridBoundColumn DataField="DateJoined" HeaderText="Date of Join"
                                        DataFormatString="{0:dd-MMM-yyyy}" DataType="System.DateTime" 
                                        ReadOnly="True" SortExpression="DateJoined" UniqueName="DateJoined">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="100px" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma"></HeaderStyle>
                                    </telerik:GridBoundColumn>  
                                    <telerik:GridBoundColumn DataField="YearsOfService" DataType="System.Double" HeaderText="Years of Service" 
                                        ReadOnly="True" SortExpression="YearsOfService" UniqueName="YearsOfService">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="120px" Font-Bold="True" Font-Size="8pt" HorizontalAlign="Center" />
                                        <ItemStyle HorizontalAlign="Center" />
                                    </telerik:GridBoundColumn> 
                                </Columns>
                                <NestedViewTemplate>
                                     <table border="0" style="width: 100%; padding-left: 15px; padding-top: 5px; table-layout: fixed;">
                                        <tr style="margin: 0px; padding: 0px; vertical-align: top;">      
                                            <td style="width: 200px; text-align: left;" />    
                                                <img id="imgEmpPhoto" runat="server" 
                                                    src='<%# Eval("EmployeeImagePath") %>' 
                                                    alt='<%# Eval("EmployeeImageTooltip") %>' 
                                                    style="height: 160px; width: 180px;" />

                                                <%--<img id="imgEmpPhoto" runat="server" visible='<%# Eval("IsShowPhoto") %>'
                                                    src='<%# Eval("EmployeeImagePath") %>' 
                                                    alt='<%# Eval("EmployeeImageTooltip") %>' 
                                                    style="height: 160px; width: 180px;" />--%>
                                            </td>    
                                            <td style="width: auto; text-align: left;">
                                                <telerik:RadGrid ID="gridContactPerson" runat="server" Skin="Office2010Blue" Width="730px" CellSpacing="0" GridLines="None" 
                                                    PageSize="5" AllowPaging="true" AllowSorting="true" 
                                                    onitemcommand="gridContactPerson_ItemCommand" onitemdatabound="gridContactPerson_ItemDataBound">
							                        <MasterTableView AutoGenerateColumns="False" NoMasterRecordsText="No emergency contacts found." TableLayout="Fixed">
								                        <CommandItemSettings ExportToPdfText="Export to PDF" />
								                        <RowIndicatorColumn FilterControlAltText="Filter RowIndicator column" Visible="True">
									                        <HeaderStyle Width="20px" />
								                        </RowIndicatorColumn>
								                        <ExpandCollapseColumn FilterControlAltText="Filter ExpandColumn column" Visible="True">
									                        <HeaderStyle Width="20px" />
								                        </ExpandCollapseColumn>
								                        <Columns>
                                                            <telerik:GridBoundColumn DataField="RelatedPersonName" DataType="System.String" HeaderText="Related Person Name" 
                                                                ReadOnly="True" SortExpression="RelatedPersonName" UniqueName="RelatedPersonName">
                                                                <ColumnValidationSettings>
                                                                    <ModelErrorMessage Text="" />
                                                                </ColumnValidationSettings>
                                                                <HeaderStyle Width="300px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
                                                                <ItemStyle HorizontalAlign="Left" Font-Size="9pt" />
                                                            </telerik:GridBoundColumn>
                                                            <telerik:GridBoundColumn DataField="RelationTypeDesc" DataType="System.String" HeaderText="Relationship" 
                                                                ReadOnly="True" SortExpression="RelationTypeDesc" UniqueName="RelationTypeDesc">
                                                                <ColumnValidationSettings>
                                                                    <ModelErrorMessage Text="" />
                                                                </ColumnValidationSettings>
                                                                <HeaderStyle Width="120px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
                                                                <ItemStyle HorizontalAlign="Left" Font-Size="9pt" />
                                                            </telerik:GridBoundColumn>
                                                            <telerik:GridBoundColumn DataField="PhoneNumber" DataType="System.String" HeaderText="Phone No." 
                                                                ReadOnly="True" SortExpression="PhoneNumber" UniqueName="PhoneNumber">
                                                                <ColumnValidationSettings>
                                                                    <ModelErrorMessage Text="" />
                                                                </ColumnValidationSettings>
                                                                <HeaderStyle Width="100px" Font-Bold="True" Font-Size="8pt" HorizontalAlign="Left" />
                                                                <ItemStyle HorizontalAlign="Left" ForeColor="Red" Font-Size="9pt" />
                                                            </telerik:GridBoundColumn> 
                                                            <telerik:GridBoundColumn DataField="PhoneNumberDesc" DataType="System.String" HeaderText="Phone Type" 
                                                                ReadOnly="True" SortExpression="PhoneNumberDesc" UniqueName="PhoneNumberDesc">
                                                                <ColumnValidationSettings>
                                                                    <ModelErrorMessage Text="" />
                                                                </ColumnValidationSettings>
                                                                <HeaderStyle Font-Bold="True" Font-Size="8pt" HorizontalAlign="Left" />
                                                                <ItemStyle HorizontalAlign="Left" Font-Size="9pt" />
                                                            </telerik:GridBoundColumn> 
                                                        </Columns>
                                                        <EditFormSettings>
									                        <EditColumn FilterControlAltText="Filter EditCommandColumn column">
									                        </EditColumn>
								                        </EditFormSettings>
								                        <BatchEditingSettings EditType="Cell" />
								                        <PagerStyle PageSizeControlType="RadComboBox" />
                                                    </MasterTableView>
                                                </telerik:RadGrid>
                                            </td>    
                                                
                                        </tr>
                                    </table>
                                </NestedViewTemplate>
                            </MasterTableView>
                            <ClientSettings EnableRowHoverStyle="true" AllowColumnsReorder="True" ColumnsReorderMethod="Reorder" ReorderColumnsOnClient="True" EnablePostBackOnRowClick="true">
                                <Selecting AllowRowSelect="True" UseClientSelectColumnOnly="True" />
                                <Scrolling AllowScroll="true" UseStaticHeaders="true" SaveScrollPosition="true" ScrollHeight="" FrozenColumnsCount="2" />
			                    <Resizing AllowColumnResize="true" AllowResizeToFit="True" EnableRealTimeResize="True" ResizeGridOnColumnResize="True" />   
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
                            <PagerStyle PageSizeControlType="RadComboBox" />
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
        <telerik:RadButton ID="btnRebind" runat="server" Text="" Skin="Office2010Silver" CausesValidation="false" onclick="btnRebind_Click" />
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
                <telerik:AjaxSetting AjaxControlID="btnRebind">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>    
                <telerik:AjaxSetting AjaxControlID="btnExport">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>  
                <telerik:AjaxSetting AjaxControlID="gridEmployeeDetail">
				    <UpdatedControls>
                        <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>   
                 <telerik:AjaxSetting AjaxControlID="chkShowPhoto">
				    <UpdatedControls>
                        <telerik:AjaxUpdatedControl ControlID="gridEmployeeDetail" LoadingPanelID="loadingPanel" />  
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