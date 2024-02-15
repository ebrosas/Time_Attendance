<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/TASMaster.Master" AutoEventWireup="true" CodeBehind="CostCenterPermissionEntry.aspx.cs" 
    Inherits="GARMCO.AMS.TAS.UI.Views.AdminFunctions.CostCenterPermissionEntry" StylesheetTheme="Standard" %>

<%@ MasterType VirtualPath="~/Views/Shared/TASMaster.Master" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <title>Cost Center Security Setup</title>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="mainContent" runat="server">
    <table border="0" style="width: 100%; text-align: left; margin-top: 5px; margin-left: 0px;">
        <tr>
            <td colspan="2" style="padding-left: 10px;">
                <table border="0" style="width: 100%; text-align: left;">
                    <tr>
                        <td style="width: 50px; text-align: right; padding-right: 5px;" rowspan="2">
                            <img alt="" src="../../Images/cost_center_security_icon.png" />
                        </td>
                        <td id="tdPageTitle" runat="server" class="PageTitleLabel" style="vertical-align: bottom; padding-left: 2px; width: 900px; font-size: 11pt;">
                            Cost Center Security Setup
                        </td>
                        <td style="width: 50px;">
                            <asp:LinkButton ID="lnkMoveUp" runat="server" />
                        </td>
                        <td />
                    </tr>
                    <tr>
                        <td class="PageDescriptionHeader" style="text-align: left; vertical-align: top; padding-left: 2px; margin: 0px;">
                            Allow a System Administrator to manage the cost center permission given to an employee
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
                    <td class="LabelBold" style="width: 110px;">
                         <asp:CustomValidator ID="cusValEmpNo" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" />
                        Employee No. 
                    </td>
                    <td style="width: 300px;">
                        <table id="tdEmployee" runat="server" border="0" style="width: 100%; text-align: left; margin: 0px; padding: 0px; table-layout: fixed;">
                            <tr style="vertical-align: top; margin: 0px; padding: 0px;">
                                <td style="width: 110px; text-align: left;">
                                    <telerik:RadNumericTextBox ID="txtEmpNo" runat="server" width="100%" 
                                        MinValue="0" ToolTip="(Note: Employee No. must start with 1000. Example: 10003632)" 
                                        Skin="Office2010Silver" DataType="System.Int32" MaxLength="8" MaxValue="99999999" 
                                        EmptyMessage="1000xxxx" AutoPostBack="True" OnTextChanged="txtEmpNo_TextChanged">
                                        <NumberFormat ZeroPattern="n" DecimalDigits="0" GroupSeparator="" />
                                    </telerik:RadNumericTextBox> 
                                </td>
                                <td style="text-align: left; width: 30px; padding-left: 3px;">
                                    <telerik:RadButton ID="btnFindEmployee" runat="server" Skin="Office2010Silver" 
                                        Text="..." ToolTip="Click to open the Employee Search page." Enabled="False" 
                                        Width="100%" Font-Bold="False" Font-Size="9pt" CausesValidation="false"
                                        onclick="btnFindEmployee_Click">
                                    </telerik:RadButton>
                                </td> 
                                <td />
                            </tr>
                        </table>
                    </td>
                    <td />
                </tr>                         
                <tr style="height: 23px;">
                    <td class="LabelBold">
                        Employee Name
                    </td>
                    <td class="TextNormal" style="padding-left: 3px;">
                        <asp:Literal ID="litEmpName" runat="server" Text="Not defined" /> 
                    </td>
                    <td />
                </tr>      
            </table>
        </asp:Panel>

        <asp:Panel ID="panBody" runat="server" CssClass="PanelNoIcon" style="margin-top: 10px; margin-right: 10px; margin-left: 10px; padding-bottom: 10px;">        
            <telerik:RadTabStrip ID="tabMain" runat="server" SelectedIndex="0"   
                MultiPageID="MyMultiPage" ReorderTabsOnSelect="True" 
                CausesValidation="False" ontabclick="tabMain_TabClick" 
                style="padding-top: 0px; padding-left: 0px; padding-right: 0px;" 
                Skin="Silk">
                <Tabs>
                    <telerik:RadTab Text="Allowed Cost Center List" Font-Size="9pt" Font-Bold="True" Selected="True" Value="valAllowedCostCenter">
                    </telerik:RadTab>                                     
                </Tabs>
            </telerik:RadTabStrip>

            <telerik:RadMultiPage ID="MyMultiPage" runat="server" SelectedIndex="0" Width="100%" style="padding-top: 5px; padding-left: 0px; padding-right: 10px;">
                <telerik:RadPageView ID="AllowedCostCenterView" runat="server">
                    <asp:Panel ID="panAllowedCostCenter" runat="server" BorderStyle="None" style="padding-left: 10px; padding-right: 0px; padding-top: 5px; margin: 0px;">                        
                        <table border="0" style="width: 100%; text-align: left; margin-top: 5px; table-layout: fixed;">
                            <tr>
                                <td class="LabelBold" style="width: 87px;">
                                    <asp:CustomValidator ID="cusValCostCenter" runat="server" ControlToValidate="txtGeneric" 
                                        CssClass="LabelValidationError" Display="Dynamic" 
                                        ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                                        ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" />
                                    Cost Center
                                </td>
                                <td style="width: 300px;">
                                    <telerik:RadComboBox ID="cboCostCenter" runat="server"
							            DropDownWidth="350px" 
							            HighlightTemplatedItems="True" 
							            MarkFirstMatch="true" 
							            Skin="Office2010Silver" 
							            Width="100%"     
                                        Height="200px"                        
							            EmptyMessage="Select Cost Center"
							            EnableLoadOnDemand="false"
							            EnableVirtualScrolling="true" 
                                        AutoPostBack="False" Font-Names="Tahoma" Font-Size="9pt" />
                                </td>
                                <td style="width: 70px; padding-left: 5px;">
                                    <telerik:RadButton ID="btnAdd" runat="server" Text="Add" ToolTip="Add selected cost center" Width="100%" 
                                        CssClass="RadButtonStyle" ValidationGroup="valPrimary" Font-Size="9pt" OnClick="btnAdd_Click" Skin="Office2010Silver" />
                                </td>
                                <td />
                            </tr>
                        </table>

                        <table id="tblGrid" runat="server" border="0" style="width: 100%; text-align: left; margin-top: 0px; table-layout: fixed;">
                            <tr>
                                <td>
                                    <telerik:RadGrid ID="gridPermission" runat="server"
                                        AllowSorting="true" AllowMultiRowSelection="true"
                                        PageSize="10" AutoGenerateColumns="false" Skin="Silk" 
                                        GridLines="None" Width="100%" Height="" CellSpacing="0"
                                        onpageindexchanged="gridPermission_PageIndexChanged" 
                                        onpagesizechanged="gridPermission_PageSizeChanged" 
                                        onsortcommand="gridPermission_SortCommand" 
                                        onitemcommand="gridPermission_ItemCommand" 
                                        onitemdatabound="gridPermission_ItemDataBound" 
                                        Font-Names="Tahoma" Font-Size="9pt" Font-Bold="False" 
                                        AllowPaging = "true" BorderStyle="Outset" BorderWidth="1px">
                                        <ExportSettings ExportOnlyData="true" IgnorePaging="true" OpenInNewWindow="true" FileName="Permitted_CostCenter_List" HideStructureColumns="true">
                                            <Pdf PageHeight="210mm" PageWidth="310mm" PageTitle="Cost Center Permission List" DefaultFontFamily="Arial Unicode MS"
                                            PageBottomMargin="20mm" PageTopMargin="20mm" PageLeftMargin="20mm" PageRightMargin="20mm" PaperSize="A4" AllowPrinting="true" />
                                        </ExportSettings>
                                        <AlternatingItemStyle Font-Names="Tahoma" Font-Size="9pt" Wrap="True" />
                                        <MasterTableView DataKeyNames="PermitID" ClientDataKeyNames="PermitID" 
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
                                                 <%--<telerik:GridTemplateColumn HeaderText="" UniqueName="RemoveLink" HeaderTooltip="Remove selected cost center">
								                    <HeaderStyle HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" Width="60px" />
								                    <ItemTemplate>
									                    <div style="text-align: left;">
                                                            <asp:LinkButton ID="lnkRemove" runat="server" Text="Remove" Width="60px" 
                                                                Font-Bold="true" ForeColor="Blue" OnClick="lnkRemove_Click" />
									                    </div>
								                    </ItemTemplate>
							                    </telerik:GridTemplateColumn> --%>  
                                                <telerik:GridClientSelectColumn HeaderText="Select" HeaderStyle-Width="50px" 
                                                    HeaderStyle-Font-Bold="true" HeaderStyle-Font-Size = "9pt" 
                                                    UniqueName="CheckboxSelectColumn" >                                                                                            
                                                    <HeaderStyle Font-Bold="True" Font-Size="9pt" Width="35px" HorizontalAlign="Center" />
                                                    <ItemStyle HorizontalAlign="Center" />
                                                </telerik:GridClientSelectColumn>                                                                                  
                                                <telerik:GridBoundColumn DataField="CostCenter" DataType="System.String" HeaderText="Cost Center" 
                                                    ReadOnly="True" SortExpression="CostCenter" UniqueName="CostCenter">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Width="110px" Font-Bold="True" />
                                                </telerik:GridBoundColumn> 
                                                 <telerik:GridBoundColumn DataField="CostCenterName" DataType="System.String" HeaderText="Cost Center Name" 
                                                    ReadOnly="True" SortExpression="CostCenterName" UniqueName="CostCenterName">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Font-Bold="True" />
                                                </telerik:GridBoundColumn> 
                                                <%--<telerik:GridTemplateColumn DataField="CostCenterName" HeaderText="Cost Center Name" 
                                                    SortExpression="CostCenterName" UniqueName="CostCenterName">
								                    <HeaderStyle Width="300px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								                    <ItemTemplate>
									                    <div class="columnEllipsis" style="width: 290px; text-align: left;">
										                    <asp:Literal ID="litCostCenterName" runat="server" Text='<%# Eval("CostCenterName") %>' />
									                    </div>
								                    </ItemTemplate>
							                    </telerik:GridTemplateColumn>--%>  
                                                <telerik:GridTemplateColumn DataField="CreatedByFullName" HeaderText="Created By" Visible="false" 
                                                    SortExpression="CreatedByFullName" UniqueName="CreatedByFullName">
								                    <HeaderStyle Width="250px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								                    <ItemTemplate>
									                    <div class="columnEllipsis" style="width: 240px; text-align: left;">
										                    <asp:Literal ID="litCreatedByFullName" runat="server" Text='<%# Eval("CreatedByFullName") %>' />
									                    </div>
								                    </ItemTemplate>
							                    </telerik:GridTemplateColumn> 
                                                <telerik:GridBoundColumn DataField="CreatedDate" HeaderText="Created Date" Visible="false" 
                                                    DataFormatString="{0:dd-MMM-yyyy h:mm tt}" DataType="System.DateTime" 
                                                    ReadOnly="True" SortExpression="CreatedDate" UniqueName="CreatedDate">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Width="150px" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma"></HeaderStyle>
                                                </telerik:GridBoundColumn>                                           
                                                <telerik:GridTemplateColumn DataField="ModifiedByFullName" HeaderText="Modified By" Visible="false" 
                                                    SortExpression="ModifiedByFullName" UniqueName="ModifiedByFullName">
								                    <HeaderStyle Width="250px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								                    <ItemTemplate>
									                    <div class="columnEllipsis" style="width: 240px; text-align: left;">
										                    <asp:Literal ID="litModifiedByFullName" runat="server" Text='<%# Eval("ModifiedByFullName") %>' />
									                    </div>
								                    </ItemTemplate>
							                    </telerik:GridTemplateColumn>   
                                                <telerik:GridBoundColumn DataField="ModifiedDate" HeaderText="Last Modified Date" Visible="false"
                                                    DataFormatString="{0:dd-MMM-yyyy h:mm tt}" DataType="System.DateTime" 
                                                    ReadOnly="True" SortExpression="ModifiedDate" UniqueName="ModifiedDate">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma"></HeaderStyle>
                                                </telerik:GridBoundColumn>      
                                                <telerik:GridBoundColumn DataField="EmpNo" DataType="System.Int32" HeaderText="Emp. No." 
                                                    ReadOnly="True" SortExpression="EmpNo" UniqueName="EmpNo" Visible="false">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Width="90px" Font-Bold="True" />
                                                    <ItemStyle Font-Bold="true" ForeColor="Purple" />
                                                </telerik:GridBoundColumn> 
                                                <telerik:GridTemplateColumn DataField="EmpName" HeaderText="Employee Name" 
                                                    SortExpression="EmpName" UniqueName="EmpName" Visible="false">
								                    <HeaderStyle Width="300px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								                    <ItemTemplate>
									                    <div class="columnEllipsis" style="width: 340px; text-align: left;">
                                                            <asp:Literal ID="litEmpName" runat="server" Text='<%# Eval("EmpName") %>' />
									                    </div>
								                    </ItemTemplate>
							                    </telerik:GridTemplateColumn>                                               
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
            </telerik:RadMultiPage>
        </asp:Panel>

        <asp:Panel ID="panButton" runat="server" BorderStyle="None" style="padding-left: 0px; margin: 0px;">
            <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                <tr>         
                    <td style="width: 15px; text-align: right;">
                         <asp:CustomValidator ID="cusValButton" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                    </td>           
                    <td style="padding-top: 3px; text-align: left;">
                        <telerik:RadButton ID="btnSave" runat="server" Text="Save" ToolTip="Save changes" Width="70px" 
                            CssClass="RadButtonStyle" ValidationGroup="valPrimary" Font-Size="9pt" OnClick="btnSave_Click" Skin="Office2010Silver" />
                        <telerik:RadButton ID="btnDelete" runat="server" Text="Delete" ToolTip="Delete current record" Width="70px" 
                            CssClass="RadButtonStyle" ValidationGroup="valPrimary" Font-Size="9pt" OnClick="btnDelete_Click" Skin="Office2010Silver" />
                        <telerik:RadButton ID="btnReset" runat="server" Text="Reset" ToolTip="Reset form" Width="70px" 
                            CssClass="RadButtonStyle" CausesValidation="false" Font-Size="9pt" OnClick="btnReset_Click" Skin="Office2010Silver" />                                                
                        <telerik:RadButton ID="btnBack" runat="server" Text="<< Back" ToolTip="Go back to previous page" Width="70px" 
                            CssClass="RadButtonStyle" CausesValidation="false" Font-Size="9pt" OnClick="btnBack_Click" Skin="Office2010Silver" />           
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
                <telerik:AjaxSetting AjaxControlID="btnAdd">
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
                <telerik:AjaxSetting AjaxControlID="btnReset">
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
                <telerik:AjaxSetting AjaxControlID="btnRebind">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>    
                <telerik:AjaxSetting AjaxControlID="gridPermission">
				    <UpdatedControls>
                        <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>   
                <telerik:AjaxSetting AjaxControlID="tabMain">
				    <UpdatedControls>
                        <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panBody" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>    
                <telerik:AjaxSetting AjaxControlID="txtEmpNo">
				    <UpdatedControls>
                        <telerik:AjaxUpdatedControl ControlID="panSearchCriteria" LoadingPanelID="loadingPanel" />  
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
