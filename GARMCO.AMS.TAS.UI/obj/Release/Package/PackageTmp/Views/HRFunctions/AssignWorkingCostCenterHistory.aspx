<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/TASMaster.Master" AutoEventWireup="true" CodeBehind="AssignWorkingCostCenterHistory.aspx.cs" 
    Inherits="GARMCO.AMS.TAS.UI.Views.HRFunctions.AssignWorkingCostCenterHistory" StylesheetTheme="Standard" %>

<%@ MasterType VirtualPath="~/Views/Shared/TASMaster.Master" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <title>Attendance History</title>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="mainContent" runat="server">
    <table border="0" style="width: 100%; text-align: left; margin-top: 5px; margin-left: 5px;">
        <tr>
            <td colspan="2" style="padding-left: 5px;">
                <table border="0" style="width: 100%; text-align: left;">
                    <tr>
                        <td style="width: 50px; text-align: right; padding-right: 5px;" rowspan="2">
                            <img alt="" src="../../Images/attendance_history_icon.png" />
                        </td>
                        <td id="tdPageTitle" runat="server" class="PageTitleLabel" style="vertical-align: bottom; padding-left: 5px; width: 900px; font-size: 11pt;">
                            Assign Temporary Working Cost Center & Special Job Catalog (History) 
                        </td>
                        <td style="width: 50px;">
                            <asp:LinkButton ID="lnkMoveUp" runat="server" />
                        </td>
                        <td />
                    </tr>
                    <tr>
                        <td class="PageDescriptionHeader" style="text-align: left; vertical-align: top; padding-left: 5px; margin: 0px;">
                            View the history of all data changes applied to the employee's working cost center or job catalog
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
                <tr style="height: 20px;">
                    <td class="LabelBold" style="width: 90px;">
                         <asp:CustomValidator ID="cusValEmpNo" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" />
                        Emp. No. 
                    </td>
                    <td style="width: 300px;">
                        <table border="0" style="width: 100%; text-align: left; margin: 0px; padding: 0px; table-layout: fixed;">
                            <tr style="vertical-align: top; margin: 0px; padding: 0px;">
                                <td style="width: 110px; text-align: left;">
                                    <telerik:RadNumericTextBox ID="txtEmpNo" runat="server" width="100%" 
                                        MinValue="0" ToolTip="(Note: Employee No. must start with 1000. Example: 10003632)" 
                                        Skin="Office2010Silver" DataType="System.Int32" MaxLength="8" MaxValue="99999999" 
                                        EmptyMessage="1000xxxx" BackColor="Yellow" >
                                        <NumberFormat ZeroPattern="n" DecimalDigits="0" GroupSeparator="" />
                                    </telerik:RadNumericTextBox> 
                                </td>
                                <td style="width: 40px; text-align: left; padding-left: 1px; padding-top: 1px; vertical-align: top;">
                                    <telerik:RadButton ID="btnGet" runat="server" Skin="Office2010Silver" Width="100%" 
                                        Text="Get" ToolTip="Get employee info based on entered Employee No." Enabled="true" 
                                        Font-Bold="False" Font-Size="9pt" CausesValidation="false"
                                        onclick="btnGet_Click">
                                    </telerik:RadButton>
                                </td> 
                                <td style="text-align: left; width: auto; padding-left: 1px; padding-top: 1px; vertical-align: top;">
                                    <telerik:RadButton ID="btnFindEmployee" runat="server" Skin="Office2010Silver" 
                                        Text="..." ToolTip="Click here to search for an employee." Enabled="true" 
                                        Width="30px" Font-Bold="False" Font-Size="9pt" CausesValidation="false"
                                        onclick="btnFindEmployee_Click">
                                    </telerik:RadButton>
                                </td> 
                            </tr>
                        </table>
                    </td>
                    <td class="LabelBold" style="width: 130px;">
                        Position
                    </td>
                    <td class="TextNormal" style="width: 300px;">
                        <asp:Literal ID="litPosition" runat="server" Text="Not defined" />                               
                    </td>
                    <td />
                </tr>
                <tr style="height: 20px;">
                    <td class="LabelBold">
                        Emp. Name
                    </td>
                    <td class="TextNormal" style="padding-left: 3px;">
                        <asp:Literal ID="litEmpName" runat="server" Text="Not defined" />                     
                    </td>
                    <td class="LabelBold">
                        Parent Cost Center
                    </td>
                    <td class="TextNormal">
                        <asp:Literal ID="litCostCenter" runat="server" Text="Not defined" />    
                    </td>
                    <td />
                </tr>
                <tr id="trButtons" runat="server" style="height: 20px;">
                    <td class="LabelBold">
                        <asp:CustomValidator ID="cusValButton" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                    </td>
                    <td colspan="3" style="padding-left: 2px; padding-top: 3px;">
                        <telerik:RadButton ID="btnSearch" runat="server" Text="Search" ToolTip="Search matching database records" Width="70px" 
                            CssClass="RadButtonStyle" ValidationGroup="valPrimary" Font-Size="9pt" OnClick="btnSearch_Click" Skin="Office2010Silver" />
                        <telerik:RadButton ID="btnReset" runat="server" Text="Reset" ToolTip="Reset filter criterias" Width="70px" Visible="false" 
                            CssClass="RadButtonStyle" ValidationGroup="valPrimary" Font-Size="9pt" OnClick="btnReset_Click" Skin="Office2010Silver" />                                                
                        <telerik:RadButton ID="btnBack" runat="server" Text="<< Back" ToolTip="Go back to previous page" Width="70px" 
                            CssClass="RadButtonStyle" ValidationGroup="valPrimary" Font-Size="9pt" OnClick="btnBack_Click" Skin="Office2010Silver" />                                                
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
                    <telerik:RadTab Text="Data Change History" Font-Size="9pt" Font-Bold="True" Selected="True" Value="valTimesheetHistory">
                    </telerik:RadTab>                                     
                </Tabs>
            </telerik:RadTabStrip>

            <telerik:RadMultiPage ID="MyMultiPage" runat="server" SelectedIndex="0" Width="100%" style="padding-top: 5px; padding-left: 0px; padding-right: 10px;">
                <telerik:RadPageView ID="HistoryView" runat="server">
                    <asp:Panel ID="panHistory" runat="server" BorderStyle="None" style="padding-left: 10px; padding-right: 10px; padding-top: 5px; margin: 0px;">
                        <table border="0" style="width: 100%; text-align: left; margin-top: 0px; table-layout: fixed; display: none;">
                            <tr>
                                <td style="text-align: left; color: Purple; font-weight: bold; font-size: 9pt; padding-left: 5px;">
                                    <asp:Label ID="lblRecordCount" runat="server" Text="0 record found" Width="100%" />                         
                                </td>
                            </tr>
                        </table>
                        <table id="tblGrid" runat="server" border="0" style="width: 100%; text-align: left; margin-top: 0px; table-layout: fixed;">
                            <tr>
                                <td>
                                    <telerik:RadGrid ID="gridHistory" runat="server"
                                        AllowSorting="true" AllowMultiRowSelection="true"
                                        PageSize="10" AutoGenerateColumns="false" Skin="Silk" 
                                        GridLines="None" Width="100%" Height="" CellSpacing="0"
                                        onpageindexchanged="gridHistory_PageIndexChanged" 
                                        onpagesizechanged="gridHistory_PageSizeChanged" 
                                        onsortcommand="gridHistory_SortCommand" 
                                        onitemcommand="gridHistory_ItemCommand" 
                                        onitemdatabound="gridHistory_ItemDataBound" 
                                        Font-Names="Tahoma" Font-Size="9pt" Font-Bold="False" 
                                        AllowPaging = "true" BorderStyle="Outset" BorderWidth="1px" AllowCustomPaging="True" VirtualItemCount="1">
                                        <ExportSettings ExportOnlyData="true" IgnorePaging="true" OpenInNewWindow="true" FileName="AttendanceList" HideStructureColumns="true">
                                            <Pdf PageHeight="210mm" PageWidth="310mm" PageTitle="Attendance List" DefaultFontFamily="Arial Unicode MS"
                                            PageBottomMargin="20mm" PageTopMargin="20mm" PageLeftMargin="20mm" PageRightMargin="20mm" PaperSize="A4" AllowPrinting="true" />
                                        </ExportSettings>
                                        <AlternatingItemStyle Font-Names="Tahoma" Font-Size="9pt" Wrap="True" />
                                        <MasterTableView DataKeyNames="AutoID" ClientDataKeyNames="AutoID" 
                                            NoMasterRecordsText="No records found!" 
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
                                                    ReadOnly="True" SortExpression="AutoID" UniqueName="AutoID" Visible="false">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Width="80px" Font-Bold="True" />                                                    
                                                </telerik:GridBoundColumn>       
                                                <telerik:GridBoundColumn DataField="EmpNo" DataType="System.Int32" HeaderText="Employee No." 
                                                    ReadOnly="True" SortExpression="EmpNo" UniqueName="EmpNo" Visible="false">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Width="90px" Font-Bold="True" />
                                                    <ItemStyle Font-Bold="true" ForeColor="Purple" />
                                                </telerik:GridBoundColumn>      
                                                <telerik:GridBoundColumn DataField="ShiftPatCode" DataType="System.String" HeaderText="Shift Pat. Code" 
                                                    ReadOnly="True" SortExpression="ShiftPatCode" UniqueName="ShiftPatCode">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Width="110px" Font-Bold="True" Font-Size="8pt" HorizontalAlign="Center" />
                                                    <ItemStyle HorizontalAlign="Center" />
                                                </telerik:GridBoundColumn> 
                                                <telerik:GridBoundColumn DataField="ShiftPointer" DataType="System.Int32" HeaderText="Shift Pointer" Visible="false" 
                                                    ReadOnly="True" SortExpression="ShiftPointer" UniqueName="ShiftPointer">
                                                    <ColumnValidationSettings>
                                                        <ModelErrorMessage Text="" />
                                                    </ColumnValidationSettings>
                                                    <HeaderStyle Width="100px" Font-Bold="True" Font-Size="8pt" HorizontalAlign="Center" />
                                                    <ItemStyle HorizontalAlign="Center" />
                                                </telerik:GridBoundColumn>    
                                                <telerik:GridTemplateColumn DataField="WorkingCostCenterFullName" HeaderText="Working Cost Center" 
                                                    SortExpression="WorkingCostCenterFullName" UniqueName="WorkingCostCenterFullName">
								                    <HeaderStyle Width="250px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								                    <ItemTemplate>
									                    <div class="columnEllipsis" style="width: 240px; text-align: left;">
										                    <asp:Literal ID="litWorkingCostCenterFullName" runat="server" Text='<%# Eval("WorkingCostCenterFullName") %>' />
									                    </div>
								                    </ItemTemplate>
							                    </telerik:GridTemplateColumn> 
                                                <telerik:GridTemplateColumn DataField="SpecialJobCatgDesc" HeaderText="Special Job Catalog" 
                                                    SortExpression="SpecialJobCatgDesc" UniqueName="SpecialJobCatgDesc">
								                    <HeaderStyle Width="170px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								                    <ItemTemplate>
									                    <div class="columnEllipsis" style="width: 160px; text-align: left;">
										                    <asp:Literal ID="litSpecialJobCatgDesc" runat="server" Text='<%# Eval("SpecialJobCatgDesc") %>' />
									                    </div>
								                    </ItemTemplate>
							                    </telerik:GridTemplateColumn> 
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
                                                    <HeaderStyle Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma"></HeaderStyle>
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
                </telerik:RadPageView>
            </telerik:RadMultiPage>
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
                <telerik:AjaxSetting AjaxControlID="btnGet">
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
                <telerik:AjaxSetting AjaxControlID="gridHistory">
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
            </AjaxSettings>
        </telerik:RadAjaxManager>
        <telerik:RadAjaxLoadingPanel ID="loadingPanel" runat="server" Skin="Office2010Silver"></telerik:RadAjaxLoadingPanel>
    </asp:Panel>
</asp:Content>
