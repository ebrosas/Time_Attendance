<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/TASMaster.Master" AutoEventWireup="true" CodeBehind="OvertimeApprovalHistory.aspx.cs" 
    Inherits="GARMCO.AMS.TAS.UI.Views.UserFunctions.OvertimeApprovalHistory" StylesheetTheme="Standard" %>

<%@ MasterType VirtualPath="~/Views/Shared/TASMaster.Master" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <title>Overtime Approval History</title>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="mainContent" runat="server">
    <telerik:RadFormDecorator ID="formDecor" runat="server" DecoratedControls="Buttons" Skin="Office2010Silver" />

    <table border="0" style="width: 100%; text-align: left; margin-top: 5px; margin-left: 5px;">
        <tr>
            <td colspan="2" style="padding-left: 5px;">
                <table border="0" style="width: 100%; text-align: left;">
                    <tr>
                        <td style="width: 50px; text-align: right; padding-right: 0px;" rowspan="2">
                            <img alt="" src="../../Images/approval_history.png" />
                        </td>
                        <td id="tdPageTitle" runat="server" class="PageTitleLabel" style="vertical-align: bottom; padding-left: 0px; width: 900px; font-size: 11pt;">
                            Overtime & Meal Voucher Approval History
                        </td>
                        <td style="width: 50px;">
                            <asp:LinkButton ID="lnkMoveUp" runat="server" />
                        </td>
                        <td />
                    </tr>
                    <tr>
                        <td class="PageDescriptionHeader" style="text-align: left; vertical-align: top; padding-left: 0px; margin: 0px;">
                            View the workflow approval history of specific overtime requisition
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

    <asp:Panel ID="panMain" runat="server" style="margin-top: 0px; padding-bottom: 5px;"> 
        <asp:Panel ID="panDetails" runat="server" BorderStyle="None" style="padding: 0px; margin-top: 0px;">
            <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                <tr style="height: 23px;">
                    <td class="LabelBold" style="width: 150px;">
                        <asp:CustomValidator ID="cusValEmpNo" runat="server" ControlToValidate="txtGeneric" 
                            CssClass="LabelValidationError" Display="Dynamic" 
                            ErrorMessage="" SetFocusOnError="true" Text="*" ToolTip="" 
                            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" /> 
                        Employee No.
                    </td>
                    <td style="width: 250px; padding-right: 5px;">
                        <table border="0" style="width: 100%; text-align: left; margin: 0px; table-layout: fixed;">
                            <tr style="margin: 0px; padding: 0px;">
                                <td style="width: 130px; text-align: left;">
                                    <telerik:RadNumericTextBox ID="txtEmpNo" runat="server" width="130px" 
                                        MinValue="0" ToolTip="(Note: Employee No. must start with 1000. Example: 10003632)" 
                                        Skin="Sunset" DataType="System.Int32" MaxLength="8" MaxValue="99999999" ReadOnly="true" 
                                        EmptyMessage="1000xxxx" BackColor="Yellow" >
                                        <NumberFormat ZeroPattern="n" DecimalDigits="0" GroupSeparator="" />
                                    </telerik:RadNumericTextBox> 
                                </td>
                                <td style="width: 40px; text-align: left; padding-left: 3px; padding-top: 0px; vertical-align: top;">
                                    <telerik:RadButton ID="btnGet" runat="server" Skin="Office2010Silver" Enabled="false" 
                                        Text="Get" ToolTip="Get employee info based on entered Employee No."
                                        Font-Bold="False" Font-Size="9pt" CausesValidation="false"
                                        onclick="btnGet_Click">
                                    </telerik:RadButton>
                                </td> 
                                <td style="text-align: left; padding-left: 3px; padding-top: 0px; vertical-align: top;">
                                    <telerik:RadButton ID="btnFindIssuer" runat="server" Skin="Office2010Silver" 
                                        Text="Find..." ToolTip="Click to open the Employee Search page" Enabled="false" 
                                        Font-Bold="False" Font-Size="9pt" CausesValidation="false"
                                        onclick="btnFindIssuer_Click">
                                    </telerik:RadButton>
                                </td> 
                            </tr>
                        </table>
                    </td>
                    <td class="LabelBold" style="width: 155px;">
                        Requisition No.
                    </td>
                    <td class="TextNormal" style="width: 150px; color: blue; font-weight: bold;">
                        <asp:Literal ID="litRequisitionNo" runat="server" Text="Not defined" /> 
                    </td>
                    <td />
                </tr>
                <tr style="height: 23px;">
                    <td class="LabelBold">
                        Employee Name
                    </td>
                    <td class="TextNormal"  style="padding-right: 5px;">
                        <asp:Literal ID="litEmpName" runat="server" Text="Not defined" />         
                    </td>
                    <td class="LabelBold">
                        Date Submitted
                    </td>
                    <td class="TextNormal">
                        <asp:Literal ID="litSubmittedDate" runat="server" Text="Not defined" />       
                    </td>
                    <td>
                        
                    </td>   
                </tr>
                <tr style="height: 23px;">
                    <td class="LabelBold">
                        Position
                    </td>
                    <td class="TextNormal"  style="padding-right: 5px;">
                        <asp:Literal ID="litPosition" runat="server" Text="Not defined" />                                  
                    </td>
                    <td class="LabelBold">
                        OT Date
                    </td>
                    <td class="TextNormal">
                        <asp:Literal ID="litDate" runat="server" Text="Not defined" /> 
                    </td>
                    <td>
                        
                    </td>
                </tr>
                <tr style="height: 23px;">
                    <td class="LabelBold">
                        Pay Grade
                    </td>
                    <td class="TextNormal"  style="padding-right: 5px;">
                        <asp:Literal ID="litPayGrade" runat="server" Text="Not defined" />                                  
                    </td>
                    <td class="LabelBold">
                        OT Start Time
                    </td>
                    <td id="tdTimeInWP" runat="server" class="TextNormal">
                        <asp:Literal ID="litOTStartTime" runat="server" Text="Not defined" />                                  
                    </td>
                    <td />
                </tr>   
                <tr style="height: 23px;">
                    <td class="LabelBold">
                        Cost Center
                    </td>
                    <td class="TextNormal"  style="padding-right: 5px;">
                        <asp:Literal ID="litCostCenter" runat="server" Text="Not defined" />                                                          
                    </td>
                    <td class="LabelBold">
                        OT End Time
                    </td>
                    <td id="tdTimeOutWP" runat="server" class="TextNormal">
                        <asp:Literal ID="litOTEndTime" runat="server" Text="Not defined" />                                                                                          
                    </td>
                    <td />
                </tr>  
                <tr style="height: 23px;">
                    <td class="LabelBold">
                        Shift Pat. Code
                    </td>
                    <td class="TextNormal"  style="padding-right: 5px;">
                        <asp:Literal ID="litShiftPatCode" runat="server" Text="Not defined" />                                                                               
                    </td>
                    <td class="LabelBold">
                        Duration (hh:mm)
                    </td>
                    <td class="TextNormal">
                        <asp:Literal ID="litOTDuration" runat="server" Text="Not defined" /> 
                    </td>
                    <td />
                </tr>  
                <tr style="height: 23px;">
                    <td class="LabelBold">
                        Sched. Shift / Act. Shift
                    </td>
                    <td class="TextNormal"  style="padding-right: 5px;">
                        <asp:Literal ID="litShiftCode" runat="server" Text="Not defined" /> 
                    </td>
                    <td class="LabelBold">
                        OT Approved?
                    </td>
                    <td class="TextNormal" style="color: red; font-weight: bold;">
                        <asp:Literal ID="litOTApproved" runat="server" Text="Not defined" /> 
                    </td>
                    <td />
                </tr>  
                <tr style="height: 23px;">
                    <td class="LabelBold">
                        Current Status
                    </td>
                    <td class="TextNormal">
                        <asp:Literal ID="litStatus" runat="server" Text="Not defined" /> 
                    </td>
                    <td class="LabelBold">
                        Meal Voucher Approved?
                    </td>
                    <td class="TextNormal" style="color: red; font-weight: bold;">
                        <asp:Literal ID="litMealVoucherApproved" runat="server" Text="Not defined" /> 
                    </td>
                    <td />
                </tr>  
            </table>
        </asp:Panel>

        <asp:Panel ID="panBody" runat="server" CssClass="PanelNoIcon" style="margin-top: 5px; margin-right: 20px; margin-left: 20px; padding-bottom: 10px;">        
            <telerik:RadTabStrip ID="tabMain" runat="server" SelectedIndex="0"   
                MultiPageID="MyMultiPage" ReorderTabsOnSelect="True" Skin="Silk" 
                CausesValidation="False" ontabclick="tabMain_TabClick" 
                style="padding-top: 0px; padding-left: 0px; padding-right: 0px;">
                <Tabs>
                    <telerik:RadTab Text="Routine History" Font-Size="9pt" Font-Bold="True">
                    </telerik:RadTab> 
                    <telerik:RadTab Text="Approval" Font-Size="9pt" Font-Bold="True">
                    </telerik:RadTab>
                    <telerik:RadTab Text="Process Workflow Status" Font-Size="9pt" Font-Bold="True">
                    </telerik:RadTab> 
                </Tabs>
            </telerik:RadTabStrip>

            <telerik:RadMultiPage ID="MyMultiPage" runat="server" SelectedIndex="0" Width="100%" style="padding-top: 10px; padding-left: 10px; padding-right: 10px;">
                <telerik:RadPageView ID="HistoryView" runat="server">
                    <asp:Panel ID="panelRoutine" runat="server" style="width: 100%; text-align: left;
                        margin: 0px; padding: 0px; table-layout: fixed;">
                        <table border="0" style="width: 100%; text-align: left; table-layout: fixed;">
                            <tr>
                                <td style="padding-left: 0px; table-layout: fixed;">
                                    <telerik:RadGrid ID="gridHistory" runat="server" AllowPaging="true" 
                                        AllowSorting="true" AutoGenerateColumns="false" CellSpacing="0" 
                                        Font-Names="Tahoma" Font-Size="9pt" GridLines="None" Height=""                                         
                                        onpageindexchanged="gridHistory_PageIndexChanged" 
                                        onpagesizechanged="gridHistory_PageSizeChanged" 
                                        onsortcommand="gridHistory_SortCommand" PageSize="10" Skin="Silk" Width="99%">
					                    <AlternatingItemStyle Font-Names="Tahoma" Font-Size="9pt" />
					                    <mastertableview commanditemsettings-showrefreshbutton="false" DataKeyNames="AutoID" ClientDataKeyNames="AutoID" 
                                            nomasterrecordstext="No routine history record found." tablelayout="Fixed"
                                        PagerStyle-AlwaysVisible="True">
						                    <commanditemsettings exporttopdftext="Export to PDF" />
						                    <rowindicatorcolumn>
							                    <HeaderStyle Width="20px" />
						                    </rowindicatorcolumn>
						                    <expandcollapsecolumn>
							                    <HeaderStyle Width="20px" />
						                    </expandcollapsecolumn>
						                    <Columns>
							                    <telerik:GridBoundColumn DataField="HistCreatedDate" 
                                                    DataFormatString="{0:dd-MMM-yyyy h:mm:ss tt}" DataType="System.DateTime" 
                                                    FilterControlAltText="Filter Date column" HeaderText="Date" 
                                                    UniqueName="HistCreatedDate">
								                    <HeaderStyle Font-Bold="True" Font-Size="8pt" Width="160px" HorizontalAlign="Left" Font-Names="Tahoma" />
								                    <ItemStyle HorizontalAlign="Left" />
							                    </telerik:GridBoundColumn>
							                    <telerik:GridBoundColumn DataField="HistCreatedBy" DataType="System.Int32" 
                                                    FilterControlAltText="Filter HistCreatedBy column" HeaderText="HistCreatedBy" 
                                                    UniqueName="HistCreatedBy" Visible="false" />
							                    <telerik:GridBoundColumn DataField="HistCreatedFullName" 
                                                    FilterControlAltText="Filter Employee column" HeaderText="Employee" 
                                                    UniqueName="HistCreatedFullName">
								                    <HeaderStyle Font-Bold="True"  Font-Size="8pt" Width="360px" HorizontalAlign="Left" Font-Names="Tahoma" />
							                    </telerik:GridBoundColumn>
							                    <telerik:GridBoundColumn DataField="HistDescription" 
                                                    FilterControlAltText="Filter Description column" HeaderText="Description" 
                                                    UniqueName="HistDescription">
								                    <HeaderStyle Font-Bold="True"  Font-Size="8pt" HorizontalAlign="Left" Font-Names="Tahoma" />
							                    </telerik:GridBoundColumn>
						                    </Columns>
						                    <editformsettings>
							                    <editcolumn filtercontrolalttext="Filter EditCommandColumn column">
							                    </editcolumn>
						                    </editformsettings>
					                    </mastertableview>
					                    <ClientSettings AllowColumnsReorder="False" EnableRowHoverStyle="true">
                                            <Selecting AllowRowSelect="True" UseClientSelectColumnOnly="True" />
                                            <Scrolling AllowScroll="true" UseStaticHeaders="true" SaveScrollPosition="true" ScrollHeight="" FrozenColumnsCount="2" />
			                                <Resizing AllowColumnResize="true" />   
                                        </ClientSettings>
					                    <ItemStyle Font-Names="Tahoma" Font-Size="9pt" />
					                    <filtermenu enableimagesprites="False">
					                    </filtermenu>
					                    <HeaderStyle HorizontalAlign="Center" />
					                    <headercontextmenu cssclass="GridContextMenu GridContextMenu_Windows7">
					                    </headercontextmenu>
				                    </telerik:RadGrid>
                                </td>
                            </tr>
                        </table>                                
                    </asp:Panel>
                </telerik:RadPageView>

                <telerik:RadPageView ID="ApprovalView" runat="server">
                    <asp:Panel ID="panelApproval" runat="server" style="width: 100%; text-align: left; 
                        margin: 0px; padding: 0px; table-layout: fixed;">
                        <table border="0" style="width: 100%; text-align: left; table-layout: fixed;">
                            <tr>
                                <td style="table-layout: fixed;">
                                    <telerik:RadGrid ID="gridApproval" runat="server" AllowPaging="true" 
                                        AllowSorting="true" AutoGenerateColumns="false" CellSpacing="0" 
                                        Font-Names="Tahoma" Font-Size="9pt" GridLines="None" Height=""                                         
                                        onitemdatabound="gridApproval_ItemDataBound" 
                                        onpageindexchanged="gridApproval_PageIndexChanged" 
                                        onpagesizechanged="gridApproval_PageSizeChanged" 
                                        onsortcommand="gridApproval_SortCommand" 
                                        PageSize="10" Skin="Silk" Width="99%">
					                    <AlternatingItemStyle Font-Names="Tahoma" Font-Size="9pt" />
					                    <mastertableview commanditemsettings-showrefreshbutton="false" DataKeyNames="AutoID" ClientDataKeyNames="AutoID"  
                                            nomasterrecordstext="No approval record found." tablelayout="Fixed"
                                        PagerStyle-AlwaysVisible="True">
						                    <commanditemsettings exporttopdftext="Export to PDF" />
						                    <rowindicatorcolumn>
							                    <HeaderStyle Width="20px" />
						                    </rowindicatorcolumn>
						                    <expandcollapsecolumn>
							                    <HeaderStyle Width="20px" />
						                    </expandcollapsecolumn>
						                    <Columns>
							                    <telerik:GridBoundColumn DataField="AppCreatedDate" 
                                                    DataFormatString="{0:dd-MMM-yyyy h:mm:ss tt}" DataType="System.DateTime" 
                                                    FilterControlAltText="Filter AppCreatedDate column" HeaderText="Date" 
                                                    UniqueName="AppCreatedDate">
								                    <HeaderStyle Font-Bold="True"  Font-Size="8pt" Width="170px" Font-Names="Tahoma" HorizontalAlign="Left" />
								                    <ItemStyle HorizontalAlign="Left" />
							                    </telerik:GridBoundColumn>
							                    <telerik:GridBoundColumn DataField="AppCreatedBy" DataType="System.Int32" 
                                                    FilterControlAltText="Filter AppCreatedBy column" HeaderText="AppCreatedBy" 
                                                    UniqueName="AppCreatedBy" Visible="false" />
                                                <telerik:GridTemplateColumn DataField="AppCreatedFullName" HeaderText="Approver's Name"
                                                    SortExpression="AppCreatedFullName" UniqueName="AppCreatedFullName">
								                    <HeaderStyle Width="280px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								                    <ItemTemplate>
									                    <div class="columnEllipsis" style="width: 270px; text-align: left;">
										                    <asp:Literal ID="litApproverName" runat="server" Text='<%# Eval("AppCreatedFullName") %>' />
									                    </div>
								                    </ItemTemplate>
							                    </telerik:GridTemplateColumn> 
                                                <telerik:GridTemplateColumn DataField="ApproverPosition" HeaderText="Designation"
                                                    SortExpression="ApproverPosition" UniqueName="ApproverPosition">
								                    <HeaderStyle Width="200px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								                    <ItemTemplate>
									                    <div class="columnEllipsis" style="width: 190px; text-align: left;">
										                    <asp:Literal ID="litApproverPosition" runat="server" Text='<%# Eval("ApproverPosition") %>' />
									                    </div>
								                    </ItemTemplate>
							                    </telerik:GridTemplateColumn> 
							                    <telerik:GridTemplateColumn DataField="ApprovalRole" HeaderText="Approval Level"
                                                    SortExpression="ApprovalRole" UniqueName="ApprovalRole">
								                    <HeaderStyle Width="250px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								                    <ItemTemplate>
									                    <div class="columnEllipsis" style="width: 240px; text-align: left;">
										                    <asp:Literal ID="litApprovalLevel" runat="server" Text='<%# Eval("ApprovalRole") %>' />
									                    </div>
								                    </ItemTemplate>
							                    </telerik:GridTemplateColumn> 
							                    <telerik:GridBoundColumn DataField="AppApproved" 
                                                    FilterControlAltText="Filter AppApproved column" HeaderText="Approved?" 
                                                    UniqueName="AppApproved">
								                    <HeaderStyle Font-Bold="True"  Font-Size="8pt" Width="90px" Font-Names="Tahoma" HorizontalAlign="Center" />
								                    <ItemStyle HorizontalAlign="Center" />
							                    </telerik:GridBoundColumn>
                                                <telerik:GridTemplateColumn DataField="AppRemarks" HeaderText="Remarks"
                                                    SortExpression="AppRemarks" UniqueName="AppRemarks">
								                    <HeaderStyle HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								                    <ItemTemplate>
									                    <div class="columnEllipsis" style="width: 390px; text-align: left;">
										                    <asp:Literal ID="litAppRemarks" runat="server" Text='<%# Eval("AppRemarks") %>' />
									                    </div>
								                    </ItemTemplate>
							                    </telerik:GridTemplateColumn> 
						                    </Columns>
						                    <editformsettings>
							                    <editcolumn filtercontrolalttext="Filter EditCommandColumn column">
							                    </editcolumn>
						                    </editformsettings>
					                    </mastertableview>
					                    <ClientSettings AllowColumnsReorder="False" EnableRowHoverStyle="true">
                                            <Selecting AllowRowSelect="True" UseClientSelectColumnOnly="True" />
                                            <Scrolling AllowScroll="true" UseStaticHeaders="true" SaveScrollPosition="true" ScrollHeight="" FrozenColumnsCount="2" />
			                                <Resizing AllowColumnResize="true" />   
                                        </ClientSettings>
					                    <ItemStyle Font-Names="Tahoma" Font-Size="9pt" />
					                    <filtermenu enableimagesprites="False">
					                    </filtermenu>
					                    <HeaderStyle HorizontalAlign="Center" />
					                    <headercontextmenu cssclass="GridContextMenu GridContextMenu_Windows7">
					                    </headercontextmenu>
				                    </telerik:RadGrid>
                                </td>
                            </tr>
                        </table>                                                                      
                    </asp:Panel>
                </telerik:RadPageView>
                
                <telerik:RadPageView ID="WorkflowView" runat="server">
                    <asp:Panel ID="panelWorkflow" runat="server" style="width: 100%; text-align: left; 
                        margin: 0px; padding: 0px; table-layout: fixed;">
                        <table border="0" style="width: 100%; text-align: left; table-layout: fixed;">                        
                            <tr>
                                <td style="padding-left: 0px; table-layout: fixed;">
                                    <telerik:RadGrid ID="gridWorkflow" runat="server" AllowPaging="true" 
                                        AllowSorting="true" AutoGenerateColumns="false" CellSpacing="0" 
                                        Font-Names="Tahoma" Font-Size="9pt" GridLines="None" Height="" 
                                        onitemdatabound="gridWorkflow_ItemDataBound" 
                                        onpageindexchanged="gridWorkflow_PageIndexChanged" 
                                        onpagesizechanged="gridWorkflow_PageSizeChanged" 
                                        onsortcommand="gridWorkflow_SortCommand" PageSize="10" Skin="Silk" Width="99%">
					                    <AlternatingItemStyle Font-Names="Tahoma" Font-Size="9pt" />
					                    <MasterTableView commanditemsettings-showrefreshbutton="false" DataKeyNames="WorkflowTransactionID" ClientDataKeyNames="WorkflowTransactionID" 
                                            NoMasterRecordsText="No workflow history record found." TableLayout="Fixed">
                                            <PagerStyle AlwaysVisible="True" Mode="NextPrevAndNumeric" />
						                    <commanditemsettings exporttopdftext="Export to PDF" />
						                    <rowindicatorcolumn>
							                    <HeaderStyle Width="20px" />
						                    </rowindicatorcolumn>
						                    <expandcollapsecolumn>
							                    <HeaderStyle Width="20px" />
						                    </expandcollapsecolumn>
						                    <Columns>                                                
                                                <telerik:GridBoundColumn DataField="SequenceNo" 
                                                    FilterControlAltText="Filter Activity Code column" HeaderText="Sequence No." 
                                                    UniqueName="SequenceNo">
								                    <HeaderStyle Font-Bold="True" Width="110px" HorizontalAlign="Left" Font-Size="8pt" Font-Names="Tahoma" />
                                                    <ItemStyle HorizontalAlign="Left" />
							                    </telerik:GridBoundColumn>
                                                <telerik:GridBoundColumn DataField="ActivityCode" 
                                                    FilterControlAltText="Filter Activity Code column" HeaderText="Activity Code" 
                                                    UniqueName="ActivityCode">
								                    <HeaderStyle Width="180px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
                                                    <ItemStyle HorizontalAlign="Left" />
							                    </telerik:GridBoundColumn>
                                                <telerik:GridBoundColumn DataField="ActivityTypeDesc" 
                                                    FilterControlAltText="Filter Activity Type column" HeaderText="Type of Activity" 
                                                    UniqueName="ActivityTypeDesc">
								                    <HeaderStyle Width="180px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
                                                    <ItemStyle HorizontalAlign="Left" />
							                    </telerik:GridBoundColumn>
                                                <telerik:GridBoundColumn DataField="NextActivityCode" Display="false" 
                                                    FilterControlAltText="Filter Next Activity Code column" HeaderText="Next Activity Code" 
                                                    UniqueName="NextActivityCode">
								                    <HeaderStyle Width="150px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
                                                    <ItemStyle HorizontalAlign="Left" />
							                    </telerik:GridBoundColumn>
                                                <telerik:GridBoundColumn DataField="ActivityDesc1" 
                                                    FilterControlAltText="Filter Activity Description column" HeaderText="Activity Description" 
                                                    UniqueName="ActivityDesc1">
								                    <HeaderStyle Width="400px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
                                                    <ItemStyle HorizontalAlign="Left" />
							                    </telerik:GridBoundColumn>                                           
                                                <telerik:GridBoundColumn DataField="StatusDesc" 
                                                    FilterControlAltText="Filter Status column" HeaderText="Status" 
                                                    UniqueName="StatusDesc">
								                    <HeaderStyle Width="130px" HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
                                                    <ItemStyle HorizontalAlign="Left" />
							                    </telerik:GridBoundColumn>
                                                <telerik:GridBoundColumn DataField="CompletionDate" 
                                                    DataFormatString="{0:dd-MMM-yyyy h:mm:ss tt}" DataType="System.DateTime" 
                                                    FilterControlAltText="Filter Completion Date column" HeaderText="Date Completed" 
                                                    UniqueName="CompletionDate">
								                    <HeaderStyle HorizontalAlign="Left" Font-Bold="True" Font-Size="8pt" Font-Names="Tahoma" />
								                    <ItemStyle HorizontalAlign="Left" />
							                    </telerik:GridBoundColumn>
                                                <telerik:GridBoundColumn DataField="WorkflowTransactionID" DataType="System.Int64" 
                                                    HeaderText="WorkflowTransactionID" UniqueName="WorkflowTransactionID" Visible="false" />
                                                <telerik:GridBoundColumn DataField="OTRequestNo" DataType="System.Int64" 
                                                    HeaderText="OTRequestNo" UniqueName="OTRequestNo" Visible="false" />
                                                <telerik:GridBoundColumn DataField="TS_AutoID" DataType="System.Int32" 
                                                    HeaderText="TS_AutoID" UniqueName="TS_AutoID" Visible="false" />
                                                <telerik:GridBoundColumn DataField="WFModuleCode" DataType="System.String" 
                                                    HeaderText="WFModuleCode" UniqueName="WFModuleCode" Visible="false" />
                                                <telerik:GridBoundColumn DataField="ActStatusID" DataType="System.Int32" 
                                                    HeaderText="ActStatusID" UniqueName="ActStatusID" Visible="false" />
						                    </Columns>
						                    <editformsettings>
							                    <editcolumn filtercontrolalttext="Filter EditCommandColumn column">
							                    </editcolumn>
						                    </editformsettings>
					                    </MasterTableView>
					                    <ClientSettings AllowColumnsReorder="False" EnableRowHoverStyle="true">
                                            <Selecting AllowRowSelect="True" UseClientSelectColumnOnly="True" />
                                            <Scrolling AllowScroll="true" UseStaticHeaders="true" SaveScrollPosition="true" ScrollHeight="" FrozenColumnsCount="2" />
			                                <Resizing AllowColumnResize="true" />   
                                        </ClientSettings>
					                    <ItemStyle Font-Names="Tahoma" Font-Size="9pt" />
					                    <filtermenu enableimagesprites="False">
					                    </filtermenu>
					                    <HeaderStyle HorizontalAlign="Center" />
					                    <headercontextmenu cssclass="GridContextMenu GridContextMenu_Windows7">
					                    </headercontextmenu>
				                    </telerik:RadGrid>
                                </td>
                            </tr>
                        </table>                                
                    </asp:Panel>
                </telerik:RadPageView>
            </telerik:RadMultiPage>
        </asp:Panel>  
    </asp:Panel>  

    <asp:Panel ID="panButtons" runat="server" BorderStyle="None" Direction="LeftToRight" style="padding-left: 20px; padding-right: 20px; 
        padding-top: 0px; padding-bottom: 40px; width: 100%;">
        <asp:CustomValidator ID="cusValButton" runat="server" 
            ControlToValidate="txtGeneric" CssClass="LabelValidationError" 
            Display="Dynamic" SetFocusOnError="true" Text="*"                         
            ValidationGroup="valPrimary" onservervalidate="cusGenericValidator_ServerValidate" />
        <telerik:RadButton ID="btnRefresh" runat="server" Text="Refresh Data" ToolTip="Refresh page" Visible="false" 
            Skin="Office2010Silver" Font-Bold="False" Font-Size="9pt" CausesValidation="false" Width="100px" 
            CssClass="RadButtonStyle" onclick="btnRefresh_Click" />          
        <telerik:RadButton ID="btnBack" runat="server" Text="<< Back" ToolTip="Go back to previous page" 
            Skin="Office2010Silver" Font-Bold="False" Font-Size="9pt" CausesValidation="false" Width="75px" 
            CssClass="RadButtonStyle" onclick="btnBack_Click" />          
    </asp:Panel>

    <asp:Panel ID="panHidden" runat="server" style="display: none;">
        <input type="hidden" id="hidFormAccess" runat="server" value="" />
        <input type="hidden" id="hidFormCode" runat="server" value="INCINQUIRY" />
        <input type="hidden" id="hidForm" runat="server" value="Service Request Inquiry" />
        <input type="hidden" id="hidSearchUrl" runat="server" value="" />
        <input type="hidden" id="hidRequestFlag" runat="server" value="0" />     
        <asp:TextBox ID="txtGeneric" runat="server" Width="100%" Visible="false" />     
    </asp:Panel>

    <asp:Panel ID="panAjaxManager" runat="server">
        <telerik:RadAjaxManager ID="MyAjaxManager" runat="server">
            <AjaxSettings>
                <telerik:AjaxSetting AjaxControlID="btnBack">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting> 
                <telerik:AjaxSetting AjaxControlID="btnRefresh">
				    <UpdatedControls>                        
					    <telerik:AjaxUpdatedControl ControlID="panMain" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting> 
                <telerik:AjaxSetting AjaxControlID="tabMain">
				    <UpdatedControls>
                        <telerik:AjaxUpdatedControl ControlID="panBody" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting>   
                <telerik:AjaxSetting AjaxControlID="gridHistory">
				    <UpdatedControls>
                        <telerik:AjaxUpdatedControl ControlID="gridHistory" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting> 
                <telerik:AjaxSetting AjaxControlID="gridApproval">
				    <UpdatedControls>
                        <telerik:AjaxUpdatedControl ControlID="gridApproval" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting> 
                <telerik:AjaxSetting AjaxControlID="gridWorkflow">
				    <UpdatedControls>
                        <telerik:AjaxUpdatedControl ControlID="gridWorkflow" LoadingPanelID="loadingPanel" />  
                        <telerik:AjaxUpdatedControl ControlID="panValidator" />                          
				    </UpdatedControls>
			    </telerik:AjaxSetting> 
            </AjaxSettings>
        </telerik:RadAjaxManager>
        <telerik:RadAjaxLoadingPanel ID="loadingPanel" runat="server" Skin="Vista"></telerik:RadAjaxLoadingPanel>
    </asp:Panel>
</asp:Content>
