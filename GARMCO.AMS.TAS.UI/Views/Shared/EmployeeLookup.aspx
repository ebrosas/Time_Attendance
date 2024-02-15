<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/TASMaster.Master" AutoEventWireup="true" CodeBehind="EmployeeLookup.aspx.cs" 
    Inherits="GARMCO.AMS.TAS.UI.Views.Shared.EmployeeLookup" StylesheetTheme="Standard" %>

<%@ MasterType VirtualPath="~/Views/Shared/TASMaster.Master" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <title>Employee Search Page</title>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="mainContent" runat="server">
    <table border="0" style="width: 100%; text-align: left; margin-top: 5px; margin-left: 5px;">
        <tr>
            <td colspan="2" style="padding-left: 10px;">
                <table border="0" style="width: 100%; text-align: left;">
                    <tr>
                        <td style="width: 50px; text-align: right; padding-right: 5px;" rowspan="2">
                            <img alt="" src="../../Images/employee_lookup.png" />
                        </td>
                        <td id="tdPageTitle" runat="server" class="PageTitleLabel" style="vertical-align: bottom; padding-left: 5px; width: 900px; font-size: 11pt;">
                            Employee Search
                        </td>
                        <td style="width: 50px;">
                            <asp:LinkButton ID="lnkMoveUp" runat="server" />
                        </td>
                        <td />
                    </tr>
                    <tr>
                        <td class="PageDescriptionHeader" style="text-align: left; vertical-align: top; padding-left: 5px; margin: 0px;">
                            Search for specific active employee in the company
                        </td>
                        <td />
                        <td />
                    </tr>
                </table>
            </td>                
        </tr>
    </table>

    <table border="0" style="width: 100%; text-align: left; table-layout:fixed;"">
        <tr>
            <td style="padding-top: 0px; vertical-align: top; padding-top: 0px;">
                <asp:Panel ID="panSearch" runat="server" Width="100%" style="padding-top: 0px; margin-top: 0px; margin-bottom: 5px;">
		            <asp:ValidationSummary ID="valSummary" ValidationGroup="valPrimary" runat="server" CssClass="ValidationError" HeaderText="Please enter values on the following fields:" />
		            <table border="0" style="width: 100%; table-layout: fixed;">
			            <tr style="height: 25px;">
				            <td class="LabelBold" style="width: 120px;">
                                <asp:RegularExpressionValidator ID="regEmpNo" runat="server" ControlToValidate="txtEmpNo" 
                                    CssClass="LabelValidationError" ErrorMessage="Enter a valid employee no." 
                                    SetFocusOnError="true" Text="*" ToolTip="Enter a valid employee no." 
                                    ValidationExpression="(^[ ]*?\d+[ ]*?$)" />
                                <asp:CustomValidator ID="cusValEmpNo" runat="server" 
                                    ControlToValidate="txtGeneric" CssClass="LabelValidationError" 
                                    Display="Dynamic" SetFocusOnError="true" Text="*"                         
                                    ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" />
					            Employee No.
				            </td>
				            <td style="width: 220px;">
                                <telerik:RadNumericTextBox ID="txtEmpNo" runat="server" width="100%" 
                                    MinValue="0" ToolTip="(Note: Employee No. must start with 1000. Example: 10003632)" 
                                    Skin="Vista" DataType="System.Int32" MaxLength="8" MaxValue="99999999" 
                                    EmptyMessage="1000xxxx" >
                                    <NumberFormat ZeroPattern="n" DecimalDigits="0" GroupSeparator="" />
                                </telerik:RadNumericTextBox> 
				            </td>
				            <td class="LabelBold" style="width: 100px;">
					            Cost Center
				            </td>
				            <td style="width: 250px;">
                                <telerik:RadComboBox ID="cmbCostCenter" runat="server" 
                                    DropDownWidth="350px"    
                                    Width="100%" Height="300px"                                
                                    Filter="Contains" Skin="Office2010Silver" 
                                    EmptyMessage="Please select a Cost Center..."                               
                                    HighlightTemplatedItems="True" 
                                    MarkFirstMatch="True" EnableVirtualScrolling="true"
                                    onitemsrequested="cmbCostCenter_ItemsRequested">
						            <HeaderTemplate>
							            <table border="0" style="width: 100%">
								            <tr>
									            <td style="width: 70px;">
										            Cost Center
									            </td>
									            <td>
										            Department Name
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
			            <tr style="height: 25px;">
				            <td class="LabelBold">
					            Employee Name
				            </td>
				            <td>
                                <telerik:RadTextBox ID="txtEmpName" runat="server" Width="100%" 
                                    EmptyMessage="" Skin="Vista" ToolTip="Maximum text input is 30 chars." 
                                    Font-Names="Tahoma" Font-Size="9pt" MaxLength="30" />
				            </td>
				            <td colspan="2" />
				            <td />
			            </tr>
			            <tr style="height: 25px; vertical-align: bottom;">
				            <td style="text-align: right;">
                                
                            </td>
				            <td colspan="3">                                
                                <telerik:RadButton ID="btnSearch" runat="server" OnClick="btnSearch_Click" ToolTip="Search the record based on the set criteria"
                                    Text="Search" Skin="Office2010Silver" Font-Bold="False" Font-Size="9pt" Width="65px" 
                                    CssClass="RadButtonStyle" ValidationGroup="valPrimary" >
                                </telerik:RadButton>
                                <telerik:RadButton ID="btnReset" runat="server" OnClick="btnReset_Click" ToolTip="Reset the search criteria to its default values"
                                    Text="Reset" Skin="Office2010Silver" Font-Bold="False" Font-Size="9pt" Width="65px"  
                                    CssClass="RadButtonStyle" CausesValidation="false" >
                                </telerik:RadButton>                                
                                <telerik:RadButton ID="btnBack" runat="server" OnClick="btnBack_Click" ToolTip="Go back to previous page"
                                    Text="<< Back" Skin="Office2010Silver" Font-Bold="False" Font-Size="9pt" Width="70px" 
                                    CssClass="RadButtonStyle" CausesValidation="false" >
                                </telerik:RadButton>
				            </td>
                            <td />
			            </tr>
		            </table>
	            </asp:Panel>
            </td>
        </tr>

        <tr>
            <td style="padding-right: 43px; table-layout: fixed;">
                <asp:Panel ID="panResult" runat="server" CssClass="SearchResult" style="table-layout: fixed;" Width="100%">
		            <telerik:RadGrid ID="gvList" runat="server" AutoGenerateColumns="False" 
                        AllowPaging="True" Skin="Silk" Width="100%" GridLines="None"
                        DataSourceID="objEmployee" onitemcommand="gvList_ItemCommand" 
                        onitemdatabound="gvList_ItemDataBound" Font-Names="Tahoma" Font-Size="9pt" Font-Bold="False">
			            <ClientSettings AllowColumnsReorder="False" EnableRowHoverStyle="true">
				            <Scrolling AllowScroll="True" UseStaticHeaders="true" FrozenColumnsCount="3" SaveScrollPosition="true" ScrollHeight="" />
				            <Resizing AllowColumnResize="true" />
			            </ClientSettings>
			            <AlternatingItemStyle Font-Names="Tahoma" Font-Size="9pt" />
			            <MasterTableView DataKeyNames="EmpNo" DataSourceID="objEmployee" NoMasterRecordsText="Employee record not found or no cost center access." PagerStyle-AlwaysVisible="True" TableLayout="Fixed">
				            <CommandItemSettings ExportToPdfText="Export to Pdf" />
				            <RowIndicatorColumn>
				            </RowIndicatorColumn>
				            <ExpandCollapseColumn>
				            </ExpandCollapseColumn>
				            <Columns>
					            <telerik:GridButtonColumn ButtonType="LinkButton" CommandName="Select" Text="Select" UniqueName="SelectLinkButton">
                                    <HeaderStyle Width="55px" HorizontalAlign="Center" Font-Bold="true" Font-Size="9pt" />
                                    <ItemStyle HorizontalAlign="Center" Font-Bold="true" Font-Size="9pt" ForeColor="Blue" />
					            </telerik:GridButtonColumn>
					            <telerik:GridBoundColumn DataField="EmpNo" DataType="System.Int32" FilterControlAltText="Filter EmpNo column" HeaderText="Emp. No." ReadOnly="True" SortExpression="EmpNo" UniqueName="EmpNo">
						            <HeaderStyle HorizontalAlign="Left" Width="80px" Font-Bold="true" Font-Size="8.5pt" />
						            <ItemStyle HorizontalAlign="Left" />
					            </telerik:GridBoundColumn>
                                <telerik:GridTemplateColumn DataField="EmpName" FilterControlAltText="Filter Employee Name column" HeaderText="Emp. Name" 
                                    SortExpression="EmpName" UniqueName="EmpName">
								    <HeaderStyle Width="250px" HorizontalAlign="Left" Font-Bold="true" Font-Size="8.5pt" />
								    <ItemTemplate>
									    <div class="columnEllipsis" style="width: 240px; text-align: left;">
										    <asp:Literal ID="litEmpName" runat="server" Text='<%# Eval("EmpName") %>' />
									    </div>
								    </ItemTemplate>
							    </telerik:GridTemplateColumn>
					            <telerik:GridBoundColumn DataField="CostCenter" FilterControlAltText="Filter CostCenter column" HeaderText="Cost Center" ReadOnly="True" SortExpression="CostCenter" UniqueName="CostCenter">
						            <HeaderStyle Width="100px" Font-Bold="true" Font-Size="8.5pt" />
					            </telerik:GridBoundColumn>

                                <telerik:GridTemplateColumn DataField="CostCenterName" FilterControlAltText="Filter Cost Center Name column" HeaderText="Cost Center Name" 
                                    SortExpression="CostCenterName" UniqueName="CostCenterName">
								    <HeaderStyle Width="250px" HorizontalAlign="Left" Font-Bold="true" Font-Size="8.5pt" />
								    <ItemTemplate>
									    <div class="columnEllipsis" style="width: 240px; text-align: left;">
										    <asp:Literal ID="litCostCenterName" runat="server" Text='<%# Eval("CostCenterName") %>' />
									    </div>
								    </ItemTemplate>
							    </telerik:GridTemplateColumn>
					            <telerik:GridBoundColumn DataField="WorkCostCenter" FilterControlAltText="Filter WorkCostCenter column" HeaderText="Working Cost Center" ReadOnly="True" SortExpression="WorkCostCenter" UniqueName="WorkCostCenter">
						            <HeaderStyle Width="170px" Font-Bold="true" Font-Size="8.5pt" />
					            </telerik:GridBoundColumn>

					            <telerik:GridBoundColumn DataField="Company" FilterControlAltText="Filter Company column" HeaderText="Company" ReadOnly="True" SortExpression="Company" UniqueName="Company">
						            <HeaderStyle Width="70px" Font-Bold="true" Font-Size="8.5pt" />
					            </telerik:GridBoundColumn>
					            <telerik:GridBoundColumn DataField="TelephoneExt" DataType="System.Int32" FilterControlAltText="Filter TelephoneExt column" HeaderText="Tel. Ext." ReadOnly="True" SortExpression="TelephoneExt" UniqueName="TelephoneExt">
						            <HeaderStyle Width="70px" Font-Bold="true" Font-Size="8.5pt" />
					            </telerik:GridBoundColumn>

                                <telerik:GridTemplateColumn DataField="EmpPositionDesc" FilterControlAltText="Filter Position column" HeaderText="Position" 
                                    SortExpression="EmpPositionDesc" UniqueName="EmpPositionDesc">
								    <HeaderStyle Width="230px" HorizontalAlign="Left" Font-Bold="true" Font-Size="8.5pt" />
								    <ItemTemplate>
									    <div class="columnEllipsis" style="width: 220px; text-align: left;">
										    <asp:Literal ID="litEmpPositionDesc" runat="server" Text='<%# Eval("EmpPositionDesc") %>' />
									    </div>
								    </ItemTemplate>
							    </telerik:GridTemplateColumn>
					            <telerik:GridBoundColumn DataField="SupervisorNo" DataType="System.Int32" FilterControlAltText="Filter SupervisorNo column" HeaderText="SupervisorNo" ReadOnly="True" SortExpression="SupervisorNo" UniqueName="SupervisorNo" Display="false" />
                                <telerik:GridBoundColumn DataField="SupervisorName" FilterControlAltText="Filter SupervisorName column" HeaderText="SupervisorEmpName" ReadOnly="True" UniqueName="SupervisorEmpName" Visible="false" />

                                <telerik:GridTemplateColumn DataField="SupervisorName" FilterControlAltText="Filter Immediate Supervisor column" HeaderText="Immediate Supervisor" 
                                    SortExpression="SupervisorName" UniqueName="SupervisorName">
								    <HeaderStyle Width="250px" HorizontalAlign="Left" Font-Bold="true" Font-Size="8.5pt" />
								    <ItemTemplate>
									    <div class="columnEllipsis" style="width: 240px; text-align: left;">
										    <asp:Literal ID="litSupervisorName" runat="server" Text='<%# Eval("SupervisorName") %>' />
									    </div>
								    </ItemTemplate>
							    </telerik:GridTemplateColumn>	
					            <telerik:GridBoundColumn DataField="SuperintendentNo" DataType="System.Int32" 
                                    FilterControlAltText="Filter SuperintendentNo column" HeaderText="SuperintendentNo" 
                                    ReadOnly="True" SortExpression="SuperintendentNo" UniqueName="SuperintendentNo" Display="false" />

                                <telerik:GridTemplateColumn DataField="SuperintendentName" FilterControlAltText="Filter Superintendent Name column" HeaderText="Superintendent" 
                                    SortExpression="SuperintendentName" UniqueName="SuperintendentName">
								    <HeaderStyle Width="250px" HorizontalAlign="Left" Font-Bold="true" Font-Size="8.5pt" />
								    <ItemTemplate>
									    <div class="columnEllipsis" style="width: 240px; text-align: left;">
										    <asp:Literal ID="litSuperintendentName" runat="server" Text='<%# Eval("SuperintendentName") %>' />
									    </div>
								    </ItemTemplate>
							    </telerik:GridTemplateColumn>	
					            <telerik:GridBoundColumn DataField="ManagerNo" DataType="System.Int32" FilterControlAltText="Filter ManagerNo column" 
                                    HeaderText="ManagerNo" ReadOnly="True" SortExpression="ManagerNo" UniqueName="ManagerNo" Display="false" />

                                <telerik:GridTemplateColumn DataField="ManagerName" FilterControlAltText="Filter Manager column" HeaderText="Manager" 
                                    SortExpression="ManagerName" UniqueName="ManagerName">
								    <HeaderStyle Width="250px" HorizontalAlign="Left" Font-Bold="true" Font-Size="8.5pt" />
								    <ItemTemplate>
									    <div class="columnEllipsis" style="width: 240px; text-align: left;">
										    <asp:Literal ID="litManagerName" runat="server" Text='<%# Eval("ManagerName") %>' />
									    </div>
								    </ItemTemplate>
							    </telerik:GridTemplateColumn>	
                                <telerik:GridBoundColumn DataField="PayGrade" DataType="System.Int32" FilterControlAltText="Filter Grade column" HeaderText="Pay Grade" ReadOnly="True" SortExpression="PayGrade" UniqueName="PayGrade" Visible="false" />
                                <telerik:GridBoundColumn DataField="Status" DataType="System.String" HeaderText="Status" UniqueName="Status" Visible="false" />   
				            </Columns>
				            <EditFormSettings>
					            <EditColumn FilterControlAltText="Filter EditCommandColumn column">
					            </EditColumn>
				            </EditFormSettings>
				            <PagerStyle AlwaysVisible="True" />
			            </MasterTableView>
			            <HeaderStyle Font-Bold="True" />
			            <ItemStyle Font-Names="Tahoma" Font-Size="9pt" />
			            <FilterMenu EnableImageSprites="False">
			            </FilterMenu>
			            <HeaderContextMenu CssClass="GridContextMenu GridContextMenu_Windows7"></HeaderContextMenu>
		            </telerik:RadGrid>
	            </asp:Panel>
            </td>
        </tr>

        <tr>
            <td>
                <input type="hidden" id="hidAjaxID" runat="server" value="" />
	            <input type="hidden" id="hidControlID" runat="server" />
	            <input type="hidden" id="hidControlContent" runat="server" />
            </td>
        </tr>

        <tr>
            <td>
                <asp:Panel ID="panObjDS" runat="server">
		            <asp:ObjectDataSource ID="objEmployee" runat="server" 
                        OldValuesParameterFormatString="" SelectMethod="GetEmployee" 
                        TypeName="GARMCO.Common.DAL.Employee.EmployeeBLL" 
                        onselected="objEmployee_Selected">
			            <SelectParameters>
				            <asp:Parameter Name="costCenter" Type="String" />
				            <asp:Parameter DefaultValue="-1" Name="empNo" Type="Int32" />
				            <asp:Parameter DefaultValue="" Name="empName" Type="String" />
				            <asp:Parameter DefaultValue="true" Name="activeOnly" Type="Boolean" />
				            <asp:Parameter Name="sort" Type="String" />
			            </SelectParameters>
		            </asp:ObjectDataSource>
		            <asp:ObjectDataSource ID="objCostCenter" runat="server" 
                        OldValuesParameterFormatString="" 
                        SelectMethod="GetCostCenter" TypeName="GARMCO.Common.DAL.Employee.EmployeeBLL"> 
			            <SelectParameters>
				            <asp:Parameter Name="costCenter" Type="String" />
				            <asp:Parameter Name="costCenterName" Type="String" />
				            <asp:Parameter Name="sort" Type="String" />
			            </SelectParameters>
		            </asp:ObjectDataSource>
	            </asp:Panel>
            </td>
        </tr>

        <tr>
            <td>
                <telerik:RadAjaxManager ID="ajaxMngr" runat="server">
		            <AjaxSettings>
			            <telerik:AjaxSetting AjaxControlID="btnSearch">
				            <UpdatedControls>
					            <telerik:AjaxUpdatedControl ControlID="panResult" LoadingPanelID="loadingPanel" />
				            </UpdatedControls>
			            </telerik:AjaxSetting>
			            <telerik:AjaxSetting AjaxControlID="btnReset">
				            <UpdatedControls>
					            <telerik:AjaxUpdatedControl ControlID="panSearch" />
				            </UpdatedControls>
			            </telerik:AjaxSetting>
			            <telerik:AjaxSetting AjaxControlID="panResult">
				            <UpdatedControls>
					            <telerik:AjaxUpdatedControl ControlID="panResult" LoadingPanelID="loadingPanel" />
				            </UpdatedControls>
			            </telerik:AjaxSetting>
                        <telerik:AjaxSetting AjaxControlID="SearchResult">
				            <UpdatedControls>
					            <telerik:AjaxUpdatedControl ControlID="panResult" LoadingPanelID="loadingPanel" />
				            </UpdatedControls>
			            </telerik:AjaxSetting>
                        
		            </AjaxSettings>
	            </telerik:RadAjaxManager>
                <telerik:RadAjaxLoadingPanel ID="loadingPanel" runat="server" Skin="Office2010Silver" />
            </td>
        </tr>
    </table>

     <asp:Panel ID="panHidden" runat="server" style="display: none;">
        <asp:TextBox ID="txtGeneric" runat="server" Width="100%" Visible="false" />     
    </asp:Panel>
</asp:Content>
