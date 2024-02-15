<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/TASMaster.Master" AutoEventWireup="true" CodeBehind="AssignWorkingCostCenterInq.aspx.cs" 
    Inherits="GARMCO.AMS.TAS.UI.Views.HRFunctions.AssignWorkingCostCenterInq" StylesheetTheme="Standard" %>

<%@ MasterType VirtualPath="~/Views/Shared/TASMaster.Master" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <title>Assign Temporary Working Cost Center & Special Job Catalog (Inquiry)</title>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="mainContent" runat="server">
    <telerik:RadFormDecorator ID="formDecor" runat="server" DecoratedControls="Buttons" Skin="Office2010Silver" />

    <table border="0" style="width: 100%; text-align: left; margin-top: 5px; margin-left: 5px;">
        <tr>
            <td colspan="2" style="padding-left: 5px;">
                <table border="0" style="width: 100%; text-align: left;">
                    <tr>
                        <td style="width: 50px; text-align: right; padding-right: 5px;" rowspan="2">
                            <img alt="" src="../../Images/employee_setup_icon.png" />
                        </td>
                        <td class="PageTitleLabel" style="vertical-align: bottom; padding-left: 5px; width: 900px; font-size: 11pt;">
                            Assign Temporary Working Cost Center & Special Job Catalog (Inquiry)
                        </td>
                        <td style="width: 50px;">
                            <asp:LinkButton ID="lnkMoveUp" runat="server" />
                        </td>
                        <td />
                    </tr>
                    <tr>
                        <td class="PageDescriptionHeader" style="text-align: left; vertical-align: top; padding-left: 5px; margin: 0px;">
                            Allows an Administrator to specify the working cost center and special job catalog of an employee
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
                    <td id="tdEmployeeTitle" runat="server" class="LabelBold" style="width: 130px;">
                        Employee No.      
                    </td>
                    <td style="width: 160px;">
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
                    <td class="LabelBold" style="width: 130px;">
                        Parent Cost Center
                    </td>
                    <td style="width: 200px;">
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
                        Special Job Catg.             
                    </td>
                    <td style="padding-left: 4px;">
                        <telerik:RadComboBox ID="cboJobCatalog" runat="server"
                            DropDownWidth="145px" 
                            HighlightTemplatedItems="True" 
                            Skin="Office2010Silver" 
                            Width="150px" 
                            EmptyMessage="Select Job Catalog"
                            EnableVirtualScrolling="True" >
                            <Items>
                                <telerik:RadComboBoxItem runat="server" Text="Permanent" Value="D" />
                                <telerik:RadComboBoxItem runat="server" Text="Temporary" Value="T" />
                            </Items>
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
                        <telerik:RadButton ID="btnNew" runat="server" Text="Create New..." ToolTip="Add new record"  Width="95px" Visible="false"
                            CssClass="RadButtonStyle" ValidationGroup="valPrimary" Font-Size="9pt" OnClick="btnNew_Click" Skin="Office2010Silver" />                                            
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
                        <telerik:RadGrid ID="gridResults" runat="server"
                            AllowSorting="true" AllowMultiRowSelection="true"
                            PageSize="10" AutoGenerateColumns="false" Skin="Silk" 
                            GridLines="None" Width="100%" Height="" CellSpacing="0"
                            onpageindexchanged="gridResults_PageIndexChanged" 
                            onpagesizechanged="gridResults_PageSizeChanged" 
                            onsortcommand="gridResults_SortCommand" 
                            onitemcommand="gridResults_ItemCommand" 
                            onitemdatabound="gridResults_ItemDataBound" 
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
                                    <telerik:GridButtonColumn ButtonType="LinkButton" CommandName="Select" Text="View History" UniqueName="ViewLinkButton" HeaderTooltip="Open the Assign Temporary Working Cost Center & Special Job Catalog (History)  page">
                                        <HeaderStyle Width="95px" HorizontalAlign="Center" Font-Bold="true" Font-Size="9pt" />
                                        <ItemStyle HorizontalAlign="Center" Font-Bold="true" Font-Size="9pt" ForeColor="Blue" />
					                </telerik:GridButtonColumn>  
                                    <telerik:GridButtonColumn ButtonType="LinkButton" CommandName="Select" Text="Edit" UniqueName="EditLinkButton" HeaderTooltip="Edit selected record">
                                        <HeaderStyle Width="45px" HorizontalAlign="Center" Font-Bold="true" Font-Size="9pt" />
                                        <ItemStyle HorizontalAlign="Center" Font-Bold="true" Font-Size="9pt" ForeColor="Blue" />
					                </telerik:GridButtonColumn>     
                                    <telerik:GridBoundColumn DataField="EmpNo" DataType="System.Int32" HeaderText="Emp. No." 
                                        ReadOnly="True" SortExpression="EmpNo" UniqueName="EmpNo">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="90px" Font-Bold="True" />
                                        <ItemStyle Font-Bold="true" ForeColor="Purple" />
                                    </telerik:GridBoundColumn>                                                                        
                                    <telerik:GridBoundColumn DataField="EmpName" DataType="System.String" HeaderText="Emp. Name" 
                                        ReadOnly="True" SortExpression="EmpName" UniqueName="EmpName">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="270px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
                                    </telerik:GridBoundColumn>
                                    <telerik:GridBoundColumn DataField="ShiftPatCode" DataType="System.String" HeaderText="Shift Pat. Code" 
                                        ReadOnly="True" SortExpression="ShiftPatCode" UniqueName="ShiftPatCode">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="108px" Font-Bold="True" Font-Size="8pt" HorizontalAlign="Center" />
                                        <ItemStyle HorizontalAlign="Center" />
                                    </telerik:GridBoundColumn> 
                                    <telerik:GridBoundColumn DataField="ShiftPointer" DataType="System.Int32" HeaderText="Shift Pointer" 
                                        ReadOnly="True" SortExpression="ShiftPointer" UniqueName="ShiftPointer">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="100px" Font-Bold="True" Font-Size="8pt" HorizontalAlign="Center" />
                                        <ItemStyle HorizontalAlign="Center" />
                                    </telerik:GridBoundColumn> 
                                    <telerik:GridBoundColumn DataField="CostCenterFullName" DataType="System.String" HeaderText="Parent Cost Center" 
                                        ReadOnly="True" SortExpression="CostCenterFullName" UniqueName="CostCenterFullName">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="300px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
                                    </telerik:GridBoundColumn>
                                    <telerik:GridBoundColumn DataField="WorkingCostCenterFullName" DataType="System.String" HeaderText="Working Cost Center" 
                                        ReadOnly="True" SortExpression="WorkingCostCenterFullName" UniqueName="WorkingCostCenterFullName">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="300px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
                                    </telerik:GridBoundColumn>
                                    <telerik:GridBoundColumn DataField="SpecialJobCatgDesc" DataType="System.String" HeaderText="Special Job Catalog" 
                                        ReadOnly="True" SortExpression="SpecialJobCatgDesc" UniqueName="SpecialJobCatgDesc">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="160px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
                                    </telerik:GridBoundColumn>
                                    <telerik:GridBoundColumn DataField="CatgEffectiveDate" HeaderText="Catg. Effective Date"
                                        DataFormatString="{0:dd-MMM-yyyy}" DataType="System.DateTime" 
                                        ReadOnly="True" SortExpression="CatgEffectiveDate" UniqueName="CatgEffectiveDate">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="140px" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma"></HeaderStyle>
                                    </telerik:GridBoundColumn>   
                                    <telerik:GridBoundColumn DataField="CatgEndingDate" HeaderText="Catg. Ending Date"
                                        DataFormatString="{0:dd-MMM-yyyy}" DataType="System.DateTime" 
                                        ReadOnly="True" SortExpression="CatgEndingDate" UniqueName="CatgEndingDate">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="130px" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma"></HeaderStyle>
                                    </telerik:GridBoundColumn>
                                    <telerik:GridBoundColumn DataField="LastUpdateUser" DataType="System.String" HeaderText="Last Update User" 
                                        ReadOnly="True" SortExpression="LastUpdateUser" UniqueName="LastUpdateUser">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="140px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
                                    </telerik:GridBoundColumn>

                                    <telerik:GridBoundColumn DataField="LastUpdateTime" HeaderText="Last Modified Date"
                                        DataFormatString="{0:dd-MMM-yyyy h:mm tt}" DataType="System.DateTime" 
                                        FilterControlAltText="Filter Modified Date column" ReadOnly="True" SortExpression="LastUpdateTime" UniqueName="LastUpdateTime">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" Width="160px"></HeaderStyle>
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
                <telerik:AjaxSetting AjaxControlID="btnNew">
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
                <telerik:AjaxSetting AjaxControlID="gridResults">
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
        <asp:ObjectDataSource ID="objCostCenter" runat="server" OnSelected="objCostCenter_Selected" OldValuesParameterFormatString="" SelectMethod="GetCostCenter" TypeName="GARMCO.Common.DAL.Employee.EmployeeBLL">
			<SelectParameters>
				<asp:Parameter Name="costCenter" Type="String" />
				<asp:Parameter Name="costCenterName" Type="String" />
				<asp:Parameter Name="sort" Type="String" />
			</SelectParameters>
		</asp:ObjectDataSource>
    </asp:Panel>
</asp:Content>
