<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/TASMaster.Master" AutoEventWireup="true" CodeBehind="FireTeamFireWatch.aspx.cs" 
    Inherits="GARMCO.AMS.TAS.UI.Views.SecurityModule.FireTeamFireWatch" %>

<%@ MasterType VirtualPath="~/Views/Shared/TASMaster.Master" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <title>Emergency Response Team</title>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="mainContent" runat="server">
    <telerik:RadFormDecorator ID="formDecor" runat="server" DecoratedControls="Buttons" Skin="Office2010Silver" />

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
                            View the Fire Team and Fire Watch member employees and their contact information 
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
                        <asp:CustomValidator ID="cusValGroupType" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                        Group Type
                    </td>
                    <td style="width: 230px; padding-left: 0px;">
                        <telerik:RadComboBox ID="cboGroupType" runat="server"
                            DropDownWidth="250px" 
                            HighlightTemplatedItems="True" 
                            Skin="Office2010Silver" 
                            Width="100%" 
                            EmptyMessage="Select Group Type"
                            EnableVirtualScrolling="True">  
                             <Items>
                                 <telerik:RadComboBoxItem runat="server" Value="valAll" />
                                 <telerik:RadComboBoxItem runat="server" Selected="true" Text="Currently Available Fire Team" Value="valAvailableFireTeam" />
                                 <telerik:RadComboBoxItem runat="server" Text="Currently Available Fire Watch" Value="valAvailableFireWatch" />
                                 <telerik:RadComboBoxItem runat="server" Text="Currently Available Fire Team &amp; Fire Watch" Value="valAvailableFireTeamFireWatch" />
                                 <telerik:RadComboBoxItem runat="server" Text="All Fire Team Members" Value="valAllFireTeam" />
                                 <telerik:RadComboBoxItem runat="server" Text="All Fire Watch Members" Value="valAllFireWatch" />
                             </Items>
                         </telerik:RadComboBox>    
                    </td>
                    <td class="LabelBold" style="width: 90px;">
                        Work Shift
                    </td>
                    <td style="width: 250px;">
                        <telerik:RadComboBox ID="cboShift" runat="server"
                            DropDownWidth="240px" 
                            HighlightTemplatedItems="True" 
                            Skin="Office2010Silver" 
                            Width="100%" 
                            EmptyMessage="Select Shift"
                            EnableVirtualScrolling="True" 
                            CheckBoxes="True" 
                            CheckedItemsTexts="DisplayAllInInput">         
                            <Items>
                                <telerik:RadComboBoxItem runat="server" Text="Morning Shift (7:00 AM to 3:00 PM)" Value="M" />
                                <telerik:RadComboBoxItem runat="server" Text="Evening Shift (3:00 PM to 11:00 PM)" Value="E" />
                                <telerik:RadComboBoxItem runat="server" Text="Night Shift (11:00 PM to 7:00 AM)" Value="N" />
                                <telerik:RadComboBoxItem runat="server" Text="Day Shift" Value="D" />
                            </Items>
                        </telerik:RadComboBox>
                    </td>                    
                    <td />
                </tr> 
                <tr style="height: 23px;">
                    <td id="tdEmployeeTitle" runat="server" class="LabelBold">
                        Employee No.      
                    </td>
                    <td style="padding-left: 0px;">
                        <table id="tdEmployee" runat="server" border="0" style="width: 100%; text-align: left; margin: 0px; padding: 0px; table-layout: fixed;">
                            <tr style="vertical-align: top; margin: 0px; padding: 0px;">
                                <td style="width: 120px; text-align: left;">
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
                    <td class="LabelBold">
                        Cost Center
                    </td>
                    <td>
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
                    </td>
                    <td>
                        <asp:RadioButtonList ID="rblFilter" runat="server" RepeatDirection="Horizontal" Visible="false"                             
                            Width="700px" OnSelectedIndexChanged="rblFilter_SelectedIndexChanged" AutoPostBack="True">
                            <asp:ListItem Text="All Team Members" Value="valAll" />
                            <asp:ListItem Text="Currently Available Fire Team" Value="valFireTeam" Selected="True" />
                            <asp:ListItem Text="Currently Available Fire Watch" Value="valFireWatch" />
                            <asp:ListItem Text="Both Fire Team / Fire Watch" Value="valBoth" />
                        </asp:RadioButtonList> 
                    </td>
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
                            AllowPaging = "true" BorderStyle="Outset" BorderWidth="1px"
                            AllowCustomPaging="True" VirtualItemCount="1">
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
								        <HeaderStyle Width="40px" HorizontalAlign="Left" />
								        <ItemTemplate>
									        <div style="width: 40px; text-align: left;">    
                                                <asp:Image ID="imgAvailability" runat="server" 
                                                    ImageUrl='<%# Eval("EmpAttendanceFlag") %>'
                                                    ToolTip='<%# Eval("EmpAttendanceNotes") %>'
                                                    ImageAlign="Left" Height="20px" Width="20px" />   
									        </div>
								        </ItemTemplate>
							        </telerik:GridTemplateColumn> 
                                    <telerik:GridTemplateColumn DataField="EmpPhotoPath" HeaderText="" 
                                        SortExpression="EmpPhotoPath" UniqueName="EmpPhotoPath">
								        <HeaderStyle Width="65px" HorizontalAlign="Left" />
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
                                        <HeaderStyle Width="90px" Font-Bold="True" HorizontalAlign="Right"  />
                                        <ItemStyle HorizontalAlign="Right" />
                                    </telerik:GridBoundColumn>     
                                    <telerik:GridTemplateColumn DataField="EmpName" HeaderText="Employee Name" 
                                        SortExpression="EmpName" UniqueName="EmpName">
								        <HeaderStyle Width="280px" HorizontalAlign="Left"  />
								        <ItemTemplate>
									        <div class="columnEllipsis" style="width: 270px; text-align: left;">
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
                                        <HeaderStyle Width="60px" Font-Bold="True" HorizontalAlign="Center"  />
                                        <ItemStyle HorizontalAlign="Center" />
                                    </telerik:GridBoundColumn>   
                                    <telerik:GridBoundColumn DataField="ShiftCode" DataType="System.String" HeaderText="Shift Code" 
                                        ReadOnly="True" SortExpression="ShiftCode" UniqueName="ShiftCode">
                                        <HeaderStyle Width="60px" Font-Bold="True" HorizontalAlign="Center" />
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
                                    <telerik:GridTemplateColumn DataField="SupervisorFullName" HeaderText="Supervisor" 
                                        SortExpression="SupervisorFullName" UniqueName="SupervisorFullName">
								        <HeaderStyle Width="280px" HorizontalAlign="Left"  />
								        <ItemTemplate>
									        <div class="columnEllipsis" style="width: 270px; text-align: left;">
										        <asp:Literal ID="litSupervisorFullName" runat="server" Text='<%# Eval("SupervisorFullName") %>' />
									        </div>
								        </ItemTemplate>
							        </telerik:GridTemplateColumn>
                                    <telerik:GridTemplateColumn DataField="SuperintendentFullName" HeaderText="Area Manager" 
                                        SortExpression="SuperintendentFullName" UniqueName="SuperintendentFullName">
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
