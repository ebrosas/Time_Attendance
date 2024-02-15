<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/TASMaster.Master" AutoEventWireup="true" CodeBehind="UserFormAccessInq.aspx.cs" 
    Inherits="GARMCO.AMS.TAS.UI.Views.AdminFunctions.UserFormAccessInq" StylesheetTheme="Standard" %>

<%@ MasterType VirtualPath="~/Views/Shared/TASMaster.Master" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <title>User Form Access</title>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="mainContent" runat="server">
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
                            User Form Access
                        </td>
                        <td style="width: 50px;">
                            <asp:LinkButton ID="lnkMoveUp" runat="server" />
                        </td>
                        <td />
                    </tr>
                    <tr>
                        <td class="PageDescriptionHeader" style="text-align: left; vertical-align: top; padding-left: 2px; margin: 0px;">
                            Allows a System Administrator to manage the form level permission given to an employee
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
                    <td runat="server" class="LabelBold" style="width: 120px;">
                        Application Name      
                    </td>
                    <td style="width: 250px;">
                        <telerik:RadComboBox ID="cboApplication" runat="server"
                            DropDownWidth="300px" 
                            HighlightTemplatedItems="True" 
                            Skin="Office2010Silver" 
                            Width="100%" 
                            EmptyMessage="Select Application"
                            EnableVirtualScrolling="True" AutoPostBack="True" Enabled="False" OnSelectedIndexChanged="cboApplication_SelectedIndexChanged" MaxHeight="250px" >
                        </telerik:RadComboBox> 
                    </td>
                    <td class="LabelBold" style="width: 130px;">
                        Employee No.
                    </td>
                    <td style="width: 250px; padding-left: 0px; margin-left: 0px;">
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
                    <td />
                </tr>    
                <tr style="height: 23px;">
                    <td class="LabelBold">
                        Form Name
                    </td>
                    <td>
                        <telerik:RadComboBox ID="cboFormName" runat="server"
                            DropDownWidth="350px" 
                            HighlightTemplatedItems="True" 
                            MarkFirstMatch="true" 
                            Skin="Office2010Silver" 
                            Width="100%" 
                            EmptyMessage="Select Form"
                            EnableLoadOnDemand="false"
                            EnableVirtualScrolling="true" MaxHeight="250px" />
                    </td>
                    <td class="LabelBold">
                        Employee Name
                    </td>
                    <td class="TextNormal" style="padding-left: 3px;">
                         <asp:Literal ID="litEmpName" runat="server" Text="Not defined" /> 
                    </td>                    
                    <td />
                </tr>    
                <tr style="height: 30px; vertical-align: bottom;">
                    <td class="LabelBold">
                        <asp:CustomValidator ID="cusValButton" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                    </td>
                    <td colspan="3">
                        <telerik:RadButton ID="btnSearch" runat="server" Text="Search" ToolTip="Search matching database records" Width="70px" 
                            CssClass="RadButtonStyle" ValidationGroup="valPrimary" Font-Size="9pt" OnClick="btnSearch_Click" Skin="Office2010Silver" />                        
                        <telerik:RadButton ID="btnReset" runat="server" Text="Reset" ToolTip="Reset filter criterias" Width="70px" 
                            CssClass="RadButtonStyle" CausesValidation="false" Font-Size="9pt" OnClick="btnReset_Click" Skin="Office2010Silver" />    
                        <telerik:RadButton ID="btnUpdateAll" runat="server" Text="Save Changes" ToolTip="Save changes to all rows" Width="105px" 
                            CssClass="RadButtonStyle" ValidationGroup="valPrimary" Font-Size="9pt" OnClick="btnUpdateAll_Click" Skin="Office2010Silver" />                                                                                                          
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
                        <telerik:RadGrid ID="gridUserFormAccess" runat="server"
                            AllowSorting="true" AllowMultiRowSelection="true"
                            PageSize="10" AutoGenerateColumns="false" Skin="Silk" 
                            GridLines="None" Width="100%" Height="" CellSpacing="0"
                            onpageindexchanged="gridUserFormAccess_PageIndexChanged" 
                            onpagesizechanged="gridUserFormAccess_PageSizeChanged" 
                            onsortcommand="gridUserFormAccess_SortCommand" 
                            onitemcommand="gridUserFormAccess_ItemCommand" 
                            onitemdatabound="gridUserFormAccess_ItemDataBound" 
                            Font-Names="Tahoma" Font-Size="9pt" Font-Bold="False" 
                            AllowPaging = "true" BorderStyle="Outset" BorderWidth="1px">
                            <ExportSettings ExportOnlyData="true" IgnorePaging="true" OpenInNewWindow="true" FileName="ShiftPatternChangeList" HideStructureColumns="true">
                                <Pdf PageHeight="210mm" PageWidth="310mm" PageTitle="Shift Pattern Changes List" DefaultFontFamily="Arial Unicode MS"
                                PageBottomMargin="20mm" PageTopMargin="20mm" PageLeftMargin="20mm" PageRightMargin="20mm" PaperSize="A4" AllowPrinting="true" />
                            </ExportSettings>
                            <AlternatingItemStyle Font-Names="Tahoma" Font-Size="9pt" Wrap="True" />
                            <MasterTableView DataKeyNames="FormCode" ClientDataKeyNames="FormCode" 
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
                                    <telerik:GridTemplateColumn DataField="ApplicationName" HeaderText="Application Name" 
                                        SortExpression="ApplicationName" UniqueName="ApplicationName">
								        <HeaderStyle Width="160px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								        <ItemTemplate>
									        <div class="columnEllipsis" style="width: 150px; text-align: left;">
                                                <asp:Literal ID="litApplicationName" runat="server" Text='<%# Eval("ApplicationName") %>' />                                               
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
                                    <telerik:GridTemplateColumn DataField="EmpName" HeaderText="Employee Name" 
                                        SortExpression="EmpName" UniqueName="EmpName">
								        <HeaderStyle Width="250px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								        <ItemTemplate>
									        <div class="columnEllipsis" style="width: 240px; text-align: left;">
                                                <asp:Literal ID="litEmpName" runat="server" Text='<%# Eval("EmpName") %>' />                                               
									        </div>
								        </ItemTemplate>
							        </telerik:GridTemplateColumn>                       
                                    <telerik:GridBoundColumn DataField="CostCenter" DataType="System.String" HeaderText="Cost Center" 
                                        ReadOnly="True" SortExpression="CostCenter" UniqueName="CostCenter">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Width="90px" Font-Bold="True" />
                                    </telerik:GridBoundColumn> 
                                    <telerik:GridBoundColumn DataField="FormCode" DataType="System.String" HeaderText="Form Code" 
                                        ReadOnly="True" SortExpression="FormCode" UniqueName="FormCode">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Font-Bold="True" Width="100px" />
                                    </telerik:GridBoundColumn> 
                                    <telerik:GridTemplateColumn DataField="FormName" HeaderText="Form Name" 
                                        SortExpression="FormName" UniqueName="FormName">
								        <HeaderStyle Width="250px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								        <ItemTemplate>
									        <div class="columnEllipsis" style="width: 240px; text-align: left;">
                                                <asp:Literal ID="litFormName" runat="server" Text='<%# Eval("FormName") %>' />                                               
									        </div>
								        </ItemTemplate>
							        </telerik:GridTemplateColumn>                                      
                                    <telerik:GridTemplateColumn HeaderText="View Access" DataField="HasViewAccess" SortExpression="HasViewAccess" UniqueName="HasViewAccess">
								        <HeaderStyle Width="90px" HorizontalAlign="Right" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" ForeColor="Red" />
                                        <HeaderTemplate>
                                            <asp:CheckBox ID="chkViewAccessHeader" runat="server" AutoPostBack="true" 
                                                Checked="False" Text="" Font-Names="Tahoma" Font-Size="8pt" Font-Bold="true"
                                                OnCheckedChanged="chkViewAccessHeader_CheckedChanged" />
                                        </HeaderTemplate>
								        <ItemTemplate>     
                                            <asp:CheckBox ID="chkViewAccessItem" runat="server" Checked='<%# Convert.ToBoolean(Eval("HasViewAccess")) %>' Enabled='<%# Convert.ToBoolean(Eval("ViewAccessEnable")) %>' 
                                                Text="" Font-Names="Tahoma" Font-Size="8pt" Font-Bold="true" AutoPostBack="true" OnCheckedChanged="chkViewAccessItem_CheckedChanged" />                                   
								        </ItemTemplate>
                                        <ItemStyle HorizontalAlign="Right" />
							        </telerik:GridTemplateColumn> 

                                    <telerik:GridTemplateColumn DataField="HasCreateAccess" HeaderText="Create Access" 
                                        SortExpression="HasCreateAccess" UniqueName="HasCreateAccess">
								        <HeaderStyle Width="90px" HorizontalAlign="Right" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" ForeColor="Red" />
                                        <HeaderTemplate>
                                            <asp:CheckBox ID="chkCreateAccessHeader" runat="server" AutoPostBack="true" 
                                                Checked="False" Text="" Font-Names="Tahoma" Font-Size="8pt" Font-Bold="true"
                                                OnCheckedChanged="chkCreateAccessHeader_CheckedChanged" />
                                        </HeaderTemplate>
								        <ItemTemplate>     
                                            <asp:CheckBox ID="chkCreateAccessItem" runat="server" Checked='<%# Convert.ToBoolean(Eval("HasCreateAccess")) %>' Enabled='<%# Convert.ToBoolean(Eval("CreateAccessEnable")) %>'  
                                                Text="" Font-Names="Tahoma" Font-Size="8pt" Font-Bold="true" AutoPostBack="true" OnCheckedChanged="chkCreateAccessItem_CheckedChanged" />                                   
								        </ItemTemplate>
                                        <ItemStyle HorizontalAlign="Right" />
							        </telerik:GridTemplateColumn> 
                                    <telerik:GridTemplateColumn DataField="HasUpdateAccess" HeaderText="Update Access" 
                                        SortExpression="HasUpdateAccess" UniqueName="HasUpdateAccess">
								        <HeaderStyle Width="90px" HorizontalAlign="Right" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" ForeColor="Red" />
                                        <HeaderTemplate>
                                            <asp:CheckBox ID="chkUpdateAccessHeader" runat="server" AutoPostBack="true" 
                                                Checked="False" Text="" Font-Names="Tahoma" Font-Size="8pt" Font-Bold="true"
                                                OnCheckedChanged="chkUpdateAccessHeader_CheckedChanged" />
                                        </HeaderTemplate>
								        <ItemTemplate>     
                                            <asp:CheckBox ID="chkUpdateAccessItem" runat="server" Checked='<%# Convert.ToBoolean(Eval("HasUpdateAccess")) %>' Enabled='<%# Convert.ToBoolean(Eval("UpdateAccessEnable")) %>'  
                                                Text="" Font-Names="Tahoma" Font-Size="8pt" Font-Bold="true" AutoPostBack="true" OnCheckedChanged="chkUpdateAccessItem_CheckedChanged" />                                   
								        </ItemTemplate>
                                        <ItemStyle HorizontalAlign="Right" />
							        </telerik:GridTemplateColumn> 
                                    <telerik:GridTemplateColumn DataField="HasDeleteAccess" HeaderText="Delete Access" 
                                        SortExpression="HasDeleteAccess" UniqueName="HasDeleteAccess">
								        <HeaderStyle Width="90px" HorizontalAlign="Right" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" ForeColor="Red" />
                                        <HeaderTemplate>
                                            <asp:CheckBox ID="chkDeleteAccessHeader" runat="server" AutoPostBack="true" 
                                                Checked="False" Text="" Font-Names="Tahoma" Font-Size="8pt" Font-Bold="true"
                                                OnCheckedChanged="chkDeleteAccessHeader_CheckedChanged" />
                                        </HeaderTemplate>
								        <ItemTemplate>     
                                            <asp:CheckBox ID="chkDeleteAccessItem" runat="server" Checked='<%# Convert.ToBoolean(Eval("HasDeleteAccess")) %>' Enabled='<%# Convert.ToBoolean(Eval("DeleteAccessEnable")) %>'  
                                                Text="" Font-Names="Tahoma" Font-Size="8pt" Font-Bold="true" AutoPostBack="true" OnCheckedChanged="chkDeleteAccessItem_CheckedChanged" />                                   
								        </ItemTemplate>
                                        <ItemStyle HorizontalAlign="Right" />
							        </telerik:GridTemplateColumn> 
                                    <telerik:GridTemplateColumn DataField="HasPrintAccess" HeaderText="Print Access" 
                                        SortExpression="HasPrintAccess" UniqueName="HasPrintAccess">
								        <HeaderStyle Width="90px" HorizontalAlign="Right" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" ForeColor="Red" />
                                        <HeaderTemplate>
                                            <asp:CheckBox ID="chkPrintAccessHeader" runat="server" AutoPostBack="true" 
                                                Checked="False" Text="" Font-Names="Tahoma" Font-Size="8pt" Font-Bold="true"
                                                OnCheckedChanged="chkPrintAccessHeader_CheckedChanged"  />
                                        </HeaderTemplate>
								        <ItemTemplate>     
                                            <asp:CheckBox ID="chkPrintAccessItem" runat="server" Checked='<%# Convert.ToBoolean(Eval("HasPrintAccess")) %>' Enabled='<%# Convert.ToBoolean(Eval("PrintAccessEnable")) %>'  
                                                Text="" Font-Names="Tahoma" Font-Size="8pt" Font-Bold="true" AutoPostBack="true"  OnCheckedChanged="chkPrintAccessItem_CheckedChanged" />                                   
								        </ItemTemplate>
                                        <ItemStyle HorizontalAlign="Right" />
							        </telerik:GridTemplateColumn> 
                                    <telerik:GridTemplateColumn DataField="CreatedByFullName" HeaderText="Created By" DataType="System.String" 
                                        SortExpression="CreatedByFullName" UniqueName="CreatedByFullName">
								        <HeaderStyle Width="240px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								        <ItemTemplate>
									        <div class="columnEllipsis" style="width: 230px; text-align: left;">
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
                                    <telerik:GridTemplateColumn DataField="LastUpdatedByFullName" HeaderText="Last Updated By" DataType="System.String" 
                                        SortExpression="LastUpdatedByFullName" UniqueName="LastUpdatedByFullName">
								        <HeaderStyle Width="240px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								        <ItemTemplate>
									        <div class="columnEllipsis" style="width: 230px; text-align: left;">
										        <asp:Literal ID="litLastUpdateUser" runat="server" Text='<%# Eval("LastUpdatedByFullName") %>' />
									        </div>
								        </ItemTemplate>
							        </telerik:GridTemplateColumn> 
                                    <telerik:GridBoundColumn DataField="LastUpdatedDate" HeaderText="Last Modified Date"
                                        DataFormatString="{0:dd-MMM-yyyy h:mm tt}" DataType="System.DateTime" 
                                        ReadOnly="True" SortExpression="LastUpdatedDate" UniqueName="LastUpdatedDate">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                        <HeaderStyle Font-Bold="True" Font-Size="8pt" Width="150px" Font-Names="Tahoma"></HeaderStyle>
                                    </telerik:GridBoundColumn>   
                                    <telerik:GridTemplateColumn DataField="FormPublic" HeaderText="Is Public?" 
                                        SortExpression="FormPublic" UniqueName="FormPublic">
								        <HeaderStyle Width="77px" HorizontalAlign="Center" />
								        <ItemTemplate>
									        <div style="width: 60px; text-align: center;">
										        <asp:Label ID="lblFormPublic" runat="server" 
                                                    Text='<%# Convert.ToBoolean(Eval("FormPublic")) == true ? "Yes" : "No" %>'>
										        </asp:Label>  
									        </div>
								        </ItemTemplate>
                                        <ItemStyle HorizontalAlign="Center" />
							        </telerik:GridTemplateColumn> 
                                    <telerik:GridBoundColumn DataField="IsDirty" DataType="System.Boolean" HeaderText="IsDirty" 
                                        ReadOnly="True" SortExpression="IsDirty" UniqueName="IsDirty" Visible="false">
                                        <ColumnValidationSettings>
                                            <ModelErrorMessage Text="" />
                                        </ColumnValidationSettings>
                                    </telerik:GridBoundColumn>                                                                         
                                </Columns>
                            </MasterTableView>
                            <ClientSettings AllowColumnsReorder="False" EnableRowHoverStyle="true" EnablePostBackOnRowClick="false">
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

        <asp:Panel ID="panButton" runat="server" BorderStyle="None" style="padding-left: 15px; margin: 0px; display: none;">
            <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                <tr>                    
                    <td style="padding-top: 3px; text-align: left;">
                        <asp:CustomValidator ID="cusValSaveButton" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" />                         
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
                <telerik:AjaxSetting AjaxControlID="btnUpdateAll">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>   
                <telerik:AjaxSetting AjaxControlID="gridUserFormAccess">
				    <UpdatedControls>
                        <telerik:AjaxUpdatedControl ControlID="gridUserFormAccess" />  
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
