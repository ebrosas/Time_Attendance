<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/TASMaster.Master" AutoEventWireup="true" CodeBehind="AssignWorkingCostCenterEntry.aspx.cs" 
    Inherits="GARMCO.AMS.TAS.UI.Views.HRFunctions.AssignWorkingCostCenterEntry" StylesheetTheme="Standard" %>

<%@ MasterType VirtualPath="~/Views/Shared/TASMaster.Master" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <title>Shift Pattern Changes Entry</title>
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
                            Assign Temporary Working Cost Center & Special Job Catalog (Data Entry)
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
            <asp:Panel ID="panEmployee" runat="server" BorderStyle="None" style="padding: 0px; margin: 0px;">
                <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                    <tr style="height: 23px;">
                        <td class="LabelBold" style="width: 140px;">
                            <asp:CustomValidator ID="cusValEmpNo" runat="server" ControlToValidate="txtGeneric" 
                                CssClass="LabelValidationError" Display="Dynamic" 
                                ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                                ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                            Employee No.                 
                        </td>
                        <td style="width: 230px;">
                            <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                                <tr style="vertical-align: top; margin: 0px; padding: 0px;">
                                    <td style="width: 130px; text-align: left;">
                                        <telerik:RadNumericTextBox ID="txtEmpNo" runat="server" width="130px" 
                                            MinValue="0" ToolTip="(Note: Employee No. must start with 1000. Example: 10003632)" 
                                            Skin="Office2010Silver" DataType="System.Int32" MaxLength="8" MaxValue="99999999" 
                                            EmptyMessage="1000xxxx" BackColor="Yellow" >
                                            <NumberFormat ZeroPattern="n" DecimalDigits="0" GroupSeparator="" />
                                        </telerik:RadNumericTextBox> 
                                    </td>
                                    <td style="width: 40px; text-align: left; padding-left: 3px; padding-top: 0px; vertical-align: top;">
                                        <telerik:RadButton ID="btnGet" runat="server" Skin="Office2010Silver" Width="100%" 
                                            Text="Get" ToolTip="Get employee info based on entered Employee No." Enabled="true" 
                                            Font-Bold="False" Font-Size="9pt" CausesValidation="false"
                                            onclick="btnGet_Click">
                                        </telerik:RadButton>
                                    </td> 
                                    <td style="text-align: left; width: 30px; padding-left: 3px; padding-top: 0px; vertical-align: top;">
                                        <telerik:RadButton ID="btnFindEmployee" runat="server" Skin="Office2010Silver" 
                                            Text="..." ToolTip="Click here to search for an employee." Enabled="true" 
                                            Width="100%" Font-Bold="False" Font-Size="9pt" CausesValidation="false"
                                            onclick="btnFindEmployee_Click">
                                        </telerik:RadButton>
                                    </td> 
                                    <td />
                                </tr>
                            </table>
                        </td>
                        <td class="LabelBold" style="width: 150px;">
                            Position
                        </td>
                        <td class="TextNormal" style="width: 300px;">
                            <asp:Literal ID="litPosition" runat="server" Text="Not defined" />                                  
                        </td>
                        <td />
                    </tr>
                    <tr style="height: 23px;">
                        <td class="LabelBold">
                            Employee Name                 
                        </td>
                        <td class="TextNormal">
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
                </table>
            </asp:Panel>

            <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                <tr style="height: 23px;">
                    <td class="LabelBold" style="width: 140px;">
                        Special Job Catg.                 
                    </td>
                    <td style="width: 230px;">
                        <telerik:RadComboBox ID="cboJobCatalog" runat="server"
                            DropDownWidth="225px" 
                            HighlightTemplatedItems="True" 
                            Skin="Office2010Silver" 
                            Width="100%" 
                            EmptyMessage="Select Job Catalog"
                            EnableVirtualScrolling="True" AutoPostBack="True" OnSelectedIndexChanged="cboJobCatalog_SelectedIndexChanged" >
                            <Items>
                                <telerik:RadComboBoxItem runat="server" Text="Permanent" Value="D" />
                                <telerik:RadComboBoxItem runat="server" Text="Temporary" Value="T" />
                            </Items>
                        </telerik:RadComboBox>               
                    </td>
                    <td class="LabelBold" style="width: 150px;">
                        <asp:CustomValidator ID="cusValWorkingCC" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                        Working Cost Center    
                    </td>
                    <td class="TextNormal" style="width: 300px;">
                        <telerik:RadComboBox ID="cboCostCenter" runat="server" 
                            DropDownWidth="330px"    
                            Width="220px" 
                            Height="200px"                                
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
                        <asp:CustomValidator ID="cusValCatgStartDate" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                        Catg. Effective Date
                    </td>
                    <td style="padding-left: 0px;">
                        <telerik:RadDatePicker ID="dtpStartDate" runat="server"
                            Width="120px" Skin="Windows7">
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
                       Last Update User
                    </td>
                    <td class="TextNormal">
                        <asp:Literal ID="litUpdateUser" runat="server" Text="Not defined" />
                    </td>
                    <td>
                        
                    </td>
                </tr>
                <tr style="height: 23px;">
                    <td class="LabelBold">
                        <asp:CustomValidator ID="cusValCatgEndDate" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                        Catg. Ending Date
                    </td>
                    <td style="padding-left: 0px;">
                        <telerik:RadDatePicker ID="dtpEndDate" runat="server"
                            Width="120px" Skin="Windows7">
                            <Calendar ID="Calendar1" runat="server" Skin="Windows7" UseColumnHeadersAsSelectors="False" 
                                UseRowHeadersAsSelectors="False" ViewSelectorText="x">
                            </Calendar>
                            <DateInput ID="DateInput1" runat="server" DateFormat="d/M/yyyy" DisplayDateFormat="d/M/yyyy">
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
                       Last Update Time
                    </td>
                    <td class="TextNormal">
                        <asp:Literal ID="litLastUpdateTime" runat="server" Text="Not defined" />
                    </td>
                    <td>
                        
                    </td>
                </tr>
            </table>                                 
        </asp:Panel>

        <asp:Panel ID="panButton" runat="server" BorderStyle="None" style="padding-left: 3px; margin-top: 5px;">
            <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                <tr style="height: 23px;">
                    <td class="LabelBold" style="width: 135px;">
                        <asp:CustomValidator ID="cusValButton" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                    </td>
                    <td style="width: 750px;">                        
                        <telerik:RadButton ID="btnSave" runat="server" ToolTip="Save data" Width="80px"
                            Text="Save" Skin="Office2010Silver" Font-Bold="False" Font-Size="9pt" 
                            CssClass="RadButtonStyle" ValidationGroup="valPrimary"
                            OnClick="btnSave_Click" Enabled="False">
                        </telerik:RadButton>
                        <telerik:RadButton ID="btnReset" runat="server" ToolTip="Clear data entry form"
                            Text="Reset" Skin="Office2010Silver" Font-Bold="False" Font-Size="9pt"
                            CssClass="RadButtonStyle" CausesValidation="false" Width="80px"
                            OnClick="btnReset_Click" Enabled="False">
                        </telerik:RadButton> 
                        <telerik:RadButton ID="btnBack" runat="server" ToolTip="Go back to previous page"
                            Text="<< Back" Skin="Office2010Silver" Font-Bold="False" Font-Size="9pt" Width="80px"
                            CssClass="RadButtonStyle" CausesValidation="false" OnClick="btnBack_Click">
                        </telerik:RadButton>  
                    </td>
                    <td />
                </tr>                                
            </table>    
        </asp:Panel>  

        <table border="0" style="width: 100%; text-align: left; margin-top: 0px; table-layout: fixed;">
            <tr>
                <td class="LabelBold" style="text-align: left; padding-left: 20px; color: silver;">
                    NOTES:
                </td>
            </tr>
            <tr>
                <td class="LabelNotes" style="text-align: left; color: silver; font-style: normal; padding-left: 20px;">
                    - The minimum date value for the catalog effective and ending date is equal to tomorrow's date
                </td>
            </tr>
        </table>     
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
                <telerik:AjaxSetting AjaxControlID="btnSave">
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
                <telerik:AjaxSetting AjaxControlID="btnGet">
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
