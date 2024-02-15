<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/TASMaster.Master" AutoEventWireup="true" CodeBehind="ShiftPatternChangeEntry.aspx.cs" 
    Inherits="GARMCO.AMS.TAS.UI.Views.HRFunctions.ShiftPatternChangeEntry" StylesheetTheme="Standard" %>

<%@ MasterType VirtualPath="~/Views/Shared/TASMaster.Master" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <title>Shift Pattern Changes Entry</title>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="mainContent" runat="server">
    <telerik:RadFormDecorator ID="formDecor" runat="server" DecoratedControls="Buttons" Skin="Office2010Silver" />

    <table border="0" style="width: 100%; text-align: left; margin-top: 5px; margin-left: 5px;">
        <tr>
            <td colspan="2" style="padding-left: 10px;">
                <table border="0" style="width: 100%; text-align: left;">
                    <tr>
                        <td style="width: 50px; text-align: right; padding-right: 5px;" rowspan="2">
                            <img alt="" src="../../Images/shift_pattern_icon.jpg" />
                        </td>
                        <td id="tdPageTitle" runat="server"  class="PageTitleLabel" style="vertical-align: bottom; padding-left: 5px; width: 900px; font-size: 11pt;">
                            Shift Pattern Change Entry
                        </td>
                        <td style="width: 50px;">
                            <asp:LinkButton ID="lnkMoveUp" runat="server" />
                        </td>
                        <td />
                    </tr>
                    <tr>
                        <td class="PageDescriptionHeader" style="text-align: left; vertical-align: top; padding-left: 5px; margin: 0px;">
                            Manage the Shift Pattern of an Employee or Fire Team Member
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
            <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed; display: none;">
                <tr style="height: 23px;">
                    <td class="LabelBold" style="width: 130px;">
                        Filter Option
                    </td>
                    <td class="TextNormal" style="width: 300px; padding-left: 0px; margin-left: 0px;">
                        <asp:RadioButtonList ID="rblOption" runat="server" 
                            RepeatDirection="Horizontal" AutoPostBack="True" OnSelectedIndexChanged="rblOption_SelectedIndexChanged">                            
                            <asp:ListItem Text="Employee" Value="valEmployee" Selected="True" />
                            <asp:ListItem Text="Fire Team Member" Value="valFireTeamMember" />
                        </asp:RadioButtonList>
                    </td>
                    <td />
                </tr>
            </table>

            <asp:Panel ID="panEmployee" runat="server" BorderStyle="None" style="padding: 0px; margin: 0px;">
                <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                    <tr style="height: 23px;">
                        <td class="LabelBold" style="width: 130px;">
                             <asp:CustomValidator ID="cusValEmpNo" runat="server" ControlToValidate="txtGeneric" 
                                CssClass="LabelValidationError" Display="Dynamic" 
                                ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                                ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                            Employee No.                 
                        </td>
                        <td style="width: 280px;">
                            <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                                <tr style="vertical-align: top; margin: 0px; padding: 0px;">
                                    <td style="width: 130px; text-align: left;">
                                        <telerik:RadNumericTextBox ID="txtEmpNo" runat="server" width="130px" 
                                            MinValue="0" ToolTip="(Note: Employee No. must start with 1000. Example: 10003632)" 
                                            Skin="Office2010Silver" DataType="System.Int32" MaxLength="8" MaxValue="99999999" 
                                            EmptyMessage="1000xxxx" BackColor="Yellow" AutoPostBack="True" OnTextChanged="txtEmpNo_TextChanged" >
                                            <NumberFormat ZeroPattern="n" DecimalDigits="0" GroupSeparator="" />
                                        </telerik:RadNumericTextBox> 
                                    </td>
                                    <td style="width: 40px; text-align: left; padding-left: 3px; padding-top: 0px; vertical-align: top; display: none;">
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
                        <td class="LabelBold" style="width: 140px;">
                            Current Shift Pattern
                        </td>
                        <td class="LabelBold" style="width: 400px; color: purple; text-align: left; font-size: 9pt;">
                            <asp:Literal ID="litCurrentShiftPattern" runat="server" Text="Unknown" />                                  
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
                            Direct Supervisor
                        </td>
                        <td class="TextNormal">
                            <asp:Literal ID="litSupervisor" runat="server" Text="Not defined" />                                  
                        </td>
                        <td />
                    </tr>
                    <tr style="height: 23px;">
                        <td class="LabelBold">
                            Position        
                        </td>
                        <td class="TextNormal">
                            <asp:Literal ID="litPosition" runat="server" Text="Not defined" />                                  
                        </td>
                        <td class="LabelBold">
                            Cost Center
                        </td>
                        <td class="TextNormal">
                            <asp:Literal ID="litCostCenter" runat="server" Text="Not defined" />                                  
                        </td>
                        <td />
                    </tr>
                </table>
            </asp:Panel>

            <asp:Panel ID="panFireTeamMember" runat="server" BorderStyle="None" style="padding: 0px; margin: 0px;">
                <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                    <tr style="height: 23px;">
                        <td class="LabelBold" style="width: 130px;">
                             <asp:CustomValidator ID="cusValFireTeamMember" runat="server" ControlToValidate="txtGeneric" 
                                CssClass="LabelValidationError" Display="Dynamic" 
                                ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                                ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                            Fire Team Member             
                        </td>
                        <td style="width: 280px;">
                            <telerik:RadComboBox ID="cboFireTeamMeber" runat="server" 
                                DropDownWidth="330px"    
                                Width="100%" Height="200px"                                
                                Filter="Contains" Skin="Office2010Silver" 
                                EmptyMessage="Select Fire Team Member"                               
                                HighlightTemplatedItems="True" 
                                MarkFirstMatch="True" EnableVirtualScrolling="true" BackColor="Yellow">
						        <HeaderTemplate>
							        <table border="0" style="width: 100%">
								        <tr>
									        <td style="width: 70px;">
										        Emp. No.
									        </td>
									        <td>
										        Emp. Name
									        </td>
								        </tr>
							        </table>
						        </HeaderTemplate>
						        <ItemTemplate>
							        <table border="0" style="width: 100%">
								        <tr>
									        <td style="width: 70px;">
										        <%# DataBinder.Eval(Container.DataItem, "EmpNo")%>
									        </td>
									        <td>
										        <%# DataBinder.Eval(Container.DataItem, "EmpName")%>
									        </td>
								        </tr>
							        </table>
						        </ItemTemplate>
					        </telerik:RadComboBox>
                        </td>
                        <td class="LabelBold" style="width: 140px;">
                            
                        </td>
                        <td class="TextNormal" style="width: 400px;">
                                                          
                        </td>
                        <td />
                    </tr>
                    
                </table>
            </asp:Panel>

            <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                <tr style="height: 23px;">
                    <td class="LabelBold" style="width: 130px;">
                        <asp:CustomValidator ID="cusValEffectiveDate" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                        Effective Date                         
                    </td>
                    <td style="width: 280px;">
                        <telerik:RadDatePicker ID="dtpEffectiveDate" runat="server"
                            Width="130px" Skin="Office2010Silver" Culture="en-US" Enabled="False">
                            <Calendar ID="Calendar1" runat="server" Skin="Office2010Silver" UseColumnHeadersAsSelectors="False" 
                                UseRowHeadersAsSelectors="False" ViewSelectorText="x">
                            </Calendar>
                            <DateInput ID="DateInput1" runat="server" DateFormat="d/M/yyyy" DisplayDateFormat="d/M/yyyy" BackColor="Yellow">
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
                    <td class="LabelBold" style="width: 140px;">
                        <asp:CustomValidator ID="cusValChangeType" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                        Change Type
                    </td>
                    <td class="TextNormal" style="width: 400px;">
                        <telerik:RadComboBox ID="cboChangeType" runat="server"
                            DropDownWidth="135px" 
                            HighlightTemplatedItems="True" 
                            Skin="Office2010Silver" 
                            Width="140px" 
                            EmptyMessage="Select Change Type"
                            EnableVirtualScrolling="True" AutoPostBack="True" OnSelectedIndexChanged="cboChangeType_SelectedIndexChanged" BackColor="Yellow" >
                            <Items>
                                <telerik:RadComboBoxItem runat="server" Text="" Value="" />
                                <telerik:RadComboBoxItem runat="server" Text="Permanent" Value="D" />
                                <telerik:RadComboBoxItem runat="server" Text="Temporary" Value="T" />
                            </Items>
                        </telerik:RadComboBox>
                    </td>
                    <td />
                </tr>
                <tr style="height: 23px;">
                    <td class="LabelBold">
                         <asp:CustomValidator ID="cusValEndingDate" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                        Ending Date
                    </td>
                    <td class="TextNormal"  style="padding-left: 0px;">
                        <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                            <tr style="vertical-align: top; margin: 0px; padding: 0px;">
                                <td style="width: 130px; text-align: left; margin: 0px; padding: 0px;">
                                    <telerik:RadDatePicker ID="dtpEndingDate" runat="server"
                                        Width="130px" Skin="Office2010Silver" Culture="en-US" Enabled="False">
                                        <Calendar ID="Calendar2" runat="server" Skin="Office2010Silver" UseColumnHeadersAsSelectors="False" 
                                            UseRowHeadersAsSelectors="False" ViewSelectorText="x">
                                        </Calendar>
                                        <DateInput ID="DateInput2" runat="server" DateFormat="d/M/yyyy" DisplayDateFormat="d/M/yyyy">
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
                                <td style="text-align: right; vertical-align: bottom; font-size: 9pt;">
                                    <asp:LinkButton ID="lnkViewShiftPatDetail" runat="server" Text="View Shift Details" OnClick="lnkViewShiftPatDetail_Click" />
                                </td>
                            </tr>
                        </table>                        
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
                        <asp:CustomValidator ID="cusValShiftPatCode" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                        Shift Pat. Code
                    </td>
                    <td class="TextNormal"  style="padding-left: 0px;">
                        <telerik:RadComboBox ID="cboShiftPatCode" runat="server"
                            DropDownWidth="350px" 
                            HighlightTemplatedItems="True" 
                            MarkFirstMatch="false" 
                            Skin="Office2010Silver" 
                            Width="100%" 
                            EmptyMessage="Select Shift Pattern"
                            EnableLoadOnDemand="false"
                            EnableVirtualScrolling="true"
                            AutoPostBack="True" 
                            OnSelectedIndexChanged="cboShiftPatCode_SelectedIndexChanged"
                            BackColor="Yellow"
                            Height="180px" />
                    </td>
                    <td class="LabelBold">
                        Last Update Date
                    </td>
                    <td class="TextNormal">
                        <asp:Literal ID="litLastUpdateTime" runat="server" Text="Not defined" />
                    </td>
                    <td />
                </tr>
                <tr style="height: 23px;">
                    <td class="LabelBold">
                         <asp:CustomValidator ID="cusValShiftPointer" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                        Shift Pointer
                    </td>
                    <td class="TextNormal"  style="padding-left: 0px;">
                        <telerik:RadComboBox ID="cboShiftPointer" runat="server"
                            DropDownWidth="125px" 
                            HighlightTemplatedItems="True" 
                            MarkFirstMatch="false" 
                            Skin="Office2010Silver" 
                            Width="130px" 
                            EmptyMessage="Select Shift Pointer"
                            EnableLoadOnDemand="false"
                            EnableVirtualScrolling="true"
                            BackColor="Yellow" />
                    </td>
                    <td class="LabelBold">
                        
                    </td>
                    <td class="TextNormal">
                        
                    </td>
                    <td />
                </tr>
            </table>            
        </asp:Panel>

        <asp:Panel ID="panButton" runat="server" BorderStyle="None" style="padding-left: 3px; padding-bottom: 30px; margin-top: 5px;">
            <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                <tr style="height: 23px;">
                    <td class="LabelBold" style="width: 125px;">
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
                        <telerik:RadButton ID="btnDelete" runat="server" ToolTip="Delete record"
                            Text="Delete" Skin="Office2010Silver" Font-Bold="False" Font-Size="9pt" 
                            CssClass="RadButtonStyle" ValidationGroup="valPrimary" Width="80px"
                            OnClick="btnDelete_Click" Enabled="False">
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
            
            <table border="0" style="width: 100%; text-align: left; margin-top: 0px; table-layout: fixed;">
                <tr>
                    <td class="LabelBold" style="text-align: left; padding-left: 15px; color: silver;">
                        NOTES:
                    </td>
                </tr>
                <tr>
                    <td class="LabelNotes" style="text-align: left; color: silver; font-style: normal; padding-left: 20px;">
                        - Deletion of the shift pattern shange record wherein the Effective Date is less than the current date is not allowed.
                        <br />
                        - The Employee No., Effective Date, Change Type, Shift Pattern Code, and Shift Pointer fields are mandatory.
                        <br />
                        - The minimun value of the Effective and Ending Date fields is today's date plus 1 day.
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
        <telerik:RadButton ID="btnDeleteDummy" runat="server" Text="" Skin="Office2010Silver" CssClass="HideButton" ValidationGroup="valPrimary" onclick="btnDeleteDummy_Click" />   
        <telerik:RadButton ID="btnRebind" runat="server" Text="" Skin="Office2010Silver" CausesValidation="false" onclick="btnRebind_Click" />
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
                <telerik:AjaxSetting AjaxControlID="btnBack">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting> 
                <telerik:AjaxSetting AjaxControlID="btnGet">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panButton" />   
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting> 
                <telerik:AjaxSetting AjaxControlID="btnFindEmployee">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panButton" />   
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
                <telerik:AjaxSetting AjaxControlID="lnkViewShiftPatDetail">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting> 
                <telerik:AjaxSetting AjaxControlID="cboShiftPatCode">
				    <UpdatedControls>
                        <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>     
                <telerik:AjaxSetting AjaxControlID="cboChangeType">
				    <UpdatedControls>
                        <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <%--<telerik:AjaxUpdatedControl ControlID="panValidator" />--%>                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>
                <telerik:AjaxSetting AjaxControlID="rblOption">
				    <UpdatedControls>
                        <telerik:AjaxUpdatedControl ControlID="panSearchCriteria" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>   
                <telerik:AjaxSetting AjaxControlID="txtEmpNo">
				    <UpdatedControls>
                        <telerik:AjaxUpdatedControl ControlID="panEmployee" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panButton" />                                                  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>   
            </AjaxSettings>
        </telerik:RadAjaxManager>
        <telerik:RadAjaxLoadingPanel ID="loadingPanel" runat="server" Skin="Office2010Silver"></telerik:RadAjaxLoadingPanel>
    </asp:Panel>
</asp:Content>
