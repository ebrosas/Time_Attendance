<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ContractorInquiry.aspx.cs" Inherits="GARMCO.AMS.TAS.UI.Views.SecurityModule.ContractorInquiry" %>

<!DOCTYPE html>

<html lang="en">
<head runat="server">
    <title>Contractor Inquiry</title>

    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />

    <%--CSS file references--%>
    <link rel="stylesheet" href="../../Content/lib/bootstrap/css/bootstrap.min.css" />
    <link rel="stylesheet" href="../../Content/lib/jqueryui/jquery-ui.min.css" />
    <link rel="stylesheet" href="../../Content/lib/datatables/datatables.min.css" />
    <link rel="stylesheet" href="../../Content/lib/datatables/buttons.dataTables.min.css" />
    <link rel="stylesheet" href="../../Content/lib/fontawesome/all.min.css" />
    <link rel="stylesheet" href="../../Content/lib/loader/waitMe.min.css" />
    <link rel="stylesheet" href="../../Content/lib/toastr/toastr.min.css" />
    <link rel="stylesheet" href="../../Styles/contractInquiry.css" />

    <%--JavaScript file references--%>
    <script src="../../Content/lib/jquery/jquery-3.6.0.min.js"></script>
    <script src="../../Content/lib/bootstrap/js/bootstrap.bundle.min.js"></script>    
    <script src="../../Content/lib/jqueryui/jquery-ui.min.js"></script>
    <script src="../../Content/lib/datatables/datatables.min.js"></script>
    <script src="../../Content/lib/datatables/dataTables.buttons.min.js"></script>
    <script src="../../Content/lib/fontawesome/all.min.js"></script>
    <script src="../../Content/lib/loader/waitMe.min.js"></script>
    <script src="../../Content/lib/toastr/toastr.min.js"></script>
    <script src="../../Content/lib/moment/moment.js"></script>
    <script src="../../Content/lib/moment/ellipsis.js"></script>
    <script src="../../Content/lib/sweetalert/sweetalert.min.js"></script>
</head>
<body>    
    <form id="mainForm" class="formWrapper">      
        <div class="container-fluid mt-2 mb-4 px-4">
            <div class="row no-gutters">
                <div class="col-sm-12">
                    <div class="pageTitlePanel">
                        <span><i class="fas fa-search-plus fa-lg fa-fw"></i></span>
                        Contractor Inquiry 
                    </div>
                </div>
            </div>

            <div class="pageBodyPanel">
                <div class="form-row">
                    <div class="col-12 mx-auto px-5">
                        <div class="alert alert-danger alert-dismissible fade show errorMsgBox" hidden>
                            <button type="button" class="close" data-dismiss="alert">&times;</button>
                            <strong>Error! </strong>&nbsp;<span class="errorMsg">Test error!</span>
                        </div>
                        <div class="alert alert-success alert-dismissible fade show successMsgBox" hidden>
                            <button type="button" class="close" data-dismiss="alert">&times;</button>
                            <strong>Success!</strong>&nbsp;<span class="successMsg">Test success!</span>
                        </div>
                    </div>
                </div>
                <div class="container-fluid pl-5 pr-0 py-3">                    
                    <div class="form-row pt-1">
                        <div class="col-sm-1 fieldTitle pr-2">
                            <label for="txtContractorNo" class="col-form-label" data-field="ContractorNo">Contractor No.</label>                        
                        </div>
                        <div class="col-sm-4">         
                            <div class="form-row">
                                <div class="col-sm-5">
                                    <div class="input-group input-group-sm">
                                        <input type="text" class="form-control form-control-sm fieldValue" id="txtContractorNo" name="contractorNo" placeholder="6xxxx" tabindex="1" maxlength="5" autofocus                                            
                                            style="font-size: 14px;" />
                                    </div>
                                </div>
                            </div>   
                            <div class="form-row errorPanel" id="contractorNoValid" hidden>
                                <div class="col-sm-12">
                                    <div class="alert alert-danger alert-dismissible mt-1 fade show">
                                        <strong>Error!</strong>&nbsp;<span class="errorText"></span>
                                    </div>
                                </div>
                            </div>                                                                  
                        </div>
                        <div class="col-sm-2 fieldTitle pr-2">
                            <label class="col-form-label" data-field="IDNo">Identification No.</label>
                        </div>
                        <div class="col-sm-4">                            
                            <div class="form-row">
                                <div class="col-sm-6">        
                                    <div id="idNoInput" class="input-group input-group-sm">
                                        <input type="text" id="txtIDNo" name="idNo" class="form-control form-control-sm fieldValue" style="font-size: 14px;" maxlength="9" tabindex="2" placeholder="id number..." 
                                            title="Notes: Valid input are numbers from (0-9)." data-readonly-field="false" />         
                                    </div>                                                                
                                </div>
                                <div class="col"></div>
                            </div>     
                            <div class="form-row errorPanel" id="idNoValid" hidden>
                                <div class="col-sm-12">
                                    <div class="alert alert-danger alert-dismissible mt-1 fade show">
                                        <strong>Error!</strong>&nbsp;<span id="idError" class="errorText"></span>
                                    </div>
                                </div>
                            </div>                       
                        </div>
                        <div class="col-sm-1"></div>
                    </div>
                    <div class="form-row my-1">
                        <div class="col-sm-1 fieldTitle pr-2">
                            <label for="txtContractorName" class="col-form-label" data-field="FirstName">Name</label>
                        </div>
                        <div class="col-sm-4">
                            <input type="text" id="txtContractorName" class="form-control form-control-sm fieldValue" maxlength="30" tabindex="3" data-readonly-field="false" placeholder="contractor name..." style="font-size: 14px;" />
                            <div class="form-row errorPanel" id="contractorNameValid" hidden>
                                <div class="col-sm-12">
                                    <div class="alert alert-danger alert-dismissible mt-1 fade show">
                                        <strong>Error!</strong>&nbsp;<span class="errorText"></span>
                                    </div>
                                </div>
                            </div>     
                        </div>
                        <div class="col-sm-2 fieldTitle pr-2">
                            <label for="txtCompanyName" class="col-form-label" data-field="CompanyName">Company Name</label>
                        </div>
                        <div class="col-sm-4">
                            <input id="txtCompanyName" class="form-control form-control-sm fieldValue" type="text"  maxlength="50" tabindex="4" data-readonly-field="false"
                                title="Tips:" data-toggle="popover" data-content="Type the company name to filter the list" data-trigger="hover" data-placement="top"
                                name="companyName" value="" placeholder="company name..." style="font-size: 14px;" />
                            <input type="hidden" id="hidCompanyID" /> 
                            <div class="form-row errorPanel" id="companyNameValid" hidden>
                                <div class="col-sm-12">
                                    <div class="alert alert-danger alert-dismissible mt-1 fade show">
                                        <strong>Error!</strong>&nbsp;<span class="errorText"></span>
                                    </div>
                                </div>
                            </div> 
                        </div>
                        <div class="col-sm-1"></div>
                    </div>
                    <div class="form-row my-1">
                        <div class="col-sm-1 fieldTitle pr-2">
                            <label class="col-form-label" data-field="Duration">Duration</label>
                        </div>
                        <div class="col-sm-4">
                            <div class="input-group input-group-sm mt-1">
                                <div class="input-group-prepend">
                                    <span class="input-group-text inputGroupTitle">
                                        <i class="far fa-calendar-alt fa-1x fa-fw"></i>
                                        From                                        
                                    </span>
                                </div>
                                <input class="borderLess fieldValue form-control-sm" type="text" id="txtContractStartDate" maxlength="10" tabindex="5" data-readonly-field="true" readonly 
                                    style="width: 120px; font-size: 14px; text-align: center;" />
                                <input type="hidden" id="hdnStartDate" />
                                <div class="input-group-prepend ml-2">
                                    <span class="input-group-text inputGroupTitle">
                                        <i class="far fa-calendar-alt fa-fw"></i>
                                        To                                        
                                    </span>
                                </div>
                                <input class="borderLess fieldValue form-control-sm" type="text" id="txtContractEndDate" maxlength="10" tabindex="6" data-readonly-field="true" readonly 
                                    style="width: 120px; font-size: 14px; text-align: center;" />
                                <input type="hidden" id="hdnEndDate" />
                            </div> 
                            <div class="form-row errorPanel" id="durationValid" hidden>
                                <div class="col-sm-12">
                                    <div class="alert alert-danger alert-dismissible mt-1 fade show">
                                        <strong>Error!</strong>&nbsp;<span class="errorText"></span>
                                    </div>
                                </div>
                            </div> 
                        </div>
                        <div class="col-sm-2 fieldTitle my-1 pr-2">
                            <label for="txtCompanyCR" class="col-form-label">Visited Department</label>
                        </div>
                        <div class="col-sm-4">
                            <select id="cboCostCenter" class="form-control custom-select custom-select-sm fieldValue" name="costCenter" tabindex="7">
                            </select>
                            <div class="form-row errorPanel" id="costCenterValid" hidden>
                                <div class="col-sm-12">
                                    <div class="alert alert-danger alert-dismissible mt-1 fade show">
                                        <strong>Error!</strong>&nbsp;<span class="errorText"></span>
                                    </div>
                                </div>
                            </div>  
                        </div>
                        <div class="col-sm-1"></div>
                    </div>
                    <div class="form-row my-1">
                        <div class="col-sm-1 fieldTitle pr-2">
                            <label for="txtJobTitle" class="col-form-label" data-field="JobTitle">Job Title</label>
                        </div>
                        <div class="col-sm-4">
                            <input type="text" id="txtJobTitle" name="jobTitle" class="form-control form-control-sm fieldValue" maxlength="50" tabindex="8" data-readonly-field="false" placeholder="position..." style="font-size: 14px;" />
                            <div class="form-row errorPanel" id="jobTitleValid" hidden>
                                <div class="col-sm-12">
                                    <div class="alert alert-danger alert-dismissible mt-1 fade show">
                                        <strong>Error!</strong>&nbsp;<span class="errorText"></span>
                                    </div>
                                </div>
                            </div> 
                        </div>
                        <div class="col-sm-2 fieldTitle pr-2">
                            <label for="txtPONumber" class="col-form-label">Supervisor In-Charge</label>
                        </div>
                        <div class="col-sm-4">
                            <input id="txtSupervisor" class="form-control form-control-sm fieldValue" type="text" maxlength="100" tabindex="9" data-readonly-field="false" 
                                title="Notes:" data-toggle="popover" data-content="Please enter the employee no. or type the name to filter the list." data-trigger="hover" data-placement="top"
                                name="supervisor" value="" placeholder="employee name..." style="font-size: 14px;" />
                            <div class="form-row errorPanel" id="supervisorValid" hidden>
                                <div class="col-sm-12">
                                    <div class="alert alert-danger alert-dismissible mt-1 fade show">
                                        <strong>Error!</strong>&nbsp;<span class="errorText"></span>
                                    </div>
                                </div>
                            </div>   
                        </div>
                        <div class="col-sm-1"></div>
                    </div>
                    <div class="form-row my-1">
                        <div class="col">

                        </div>
                        <div class="col-11 mt-1">
                            <button type="button" id="btnSearch" class="form-control btn btn-sm btn-success border-0 actionButton" tabindex="10">
                                <span><i class="fas fa-search fa-fw fa-lg"></i></span>&nbsp;
                                Search
                            </button>                            
                            <button type="button" id="btnReset" class="form-control btn btn-sm btn-warning text-white border-0 actionButton" tabindex="11">
                                <span><i class="fas fa-sync fa-fw fa-lg"></i></span>&nbsp;
                                Reset
                            </button>
                            <button type="button" id="btnDelete" class="form-control btn btn-sm btn-danger border-0 actionButton" tabindex="13" hidden>
                                <span><i class="far fa-trash-alt fa-fw fa-lg"></i></span>&nbsp;
                                Delete
                            </button>                            
                            <button type="button" id="btnPrint" class="form-control actionButton btn btn-sm btn-secondary" tabindex="14" hidden>
                                <span><i class="fas fa-print fa-fw fa-lg"></i></span>&nbsp;
                                Print Report
                            </button>
                            <button type="button" id="btnCreateNew" class="form-control btn btn-sm btn-primary border-0 actionButton" tabindex="12" hidden>
                                <span><i class="fas fa-edit fa-fw fa-lg"></i></span>&nbsp;
                                Create New
                            </button>
                            <button type="button" id="btnBack" class="form-control btn btn-sm btn-outline-secondary actionButton" tabindex="15" hidden>
                                <span><i class="fas fa-arrow-circle-left fa-fw fa-lg"></i></span>&nbsp; 
                                Go Back
                            </button>                            
                            <input type="reset" id="btnHiddenReset" class="btn btn-sm btn-warning text-white border-0" value="Reset" hidden />           
                        </div>
                    </div>

                    <div class="form-row mt-2 mb-1 groupTitle">
                        <div class="col-sm-12">
                            <span>Search Results:</span> &nbsp;
                            <span class="float-sm-right">
                                <button type="button" class="btn btn-link pr-4 mr-4 linkTitle">Add New Contractor</button>
                            </span>
                        </div>
                    </div>

                    <div class="form-row mt-0 pt-0 mb-2">                        
                        <div class="col-sm-12 pr-5 pl-0 ml-0">
                            <div class="container-fluid tablePanel">
                                <div class="table-responsive py-3" style="-ms-overflow-style: auto;">
                                    <table id="contractorTable" class="generalTable display nowrap stripe row-border  table-bordered" style="width: 100%;">
                                        <thead>
                                            <tr>
                                                <th class="centeredColumn" style="width: 200px;">
                                                    Contractor No.
                                                </th>                                                
                                                <th class="centeredColumn" style="width: 200px;">
                                                    First Name
                                                </th>
                                                <th class="centeredColumn" style="width: 200px;">
                                                    Last Name
                                                </th>
                                                <th class="centeredColumn" style="width: 280px;">
                                                    Company Name
                                                </th>
                                                <th class="centeredColumn doNotOrder" style="width: 200px;">
                                                    Company Contact No.
                                                </th>
                                                <th class="centeredColumn doNotOrder" style="width: 280px;">
                                                    Job Title
                                                </th>
                                                <th class="centeredColumn" style="width: 150px;">
                                                    Contract Start Date
                                                </th>
                                                <th class="centeredColumn" style="width: 150px;">
                                                    Contract End Date
                                                </th>
                                                <th class="centeredColumn" style="width: 100px;">
                                                    Work Duration (hh:mm)
                                                </th>
                                                <th class="centeredColumn doNotOrder" style="width: 120px;">
                                                    ID Number
                                                </th>
                                                <th class="centeredColumn doNotOrder" style="width: 100px;">
                                                    ID Type
                                                </th>
                                                <th class="centeredColumn doNotOrder" style="width: 120px;">
                                                    Mobile No.
                                                </th>
                                                <th class="centeredColumn doNotOrder" style="width: 120px;">
                                                    PO No.
                                                </th>
                                                <th class="centeredColumn doNotOrder" style="width: 200px;">
                                                    Visited Department
                                                </th>
                                                <th class="centeredColumn doNotOrder" style="width: 300px;">
                                                    Purpose of Visit
                                                </th>
                                                <th class="centeredColumn doNotOrder" style="width: 200px;">
                                                    Supervisor In-charge
                                                </th>                                                
                                                 <th class="centeredColumn" style="width: 130px;">
                                                    Registration Date
                                                </th>
                                                <th class="centeredColumn" style="width: 100px;">
                                                    Create Date
                                                </th>
                                                <th class="centeredColumn doNotOrder" style="width: 200px;">
                                                    Created By
                                                </th>
                                                 <th class="hiddenColumn" style="width: 100px; display: none;">
                                                    Registry ID
                                                </th>
                                            </tr>
                                        </thead>
                                    </table>
                                </div>
                            </div>
                        </div>
                    </div>  
                </div>            
            </div>
        </div>

        <input type="hidden" id="hidRegistryID" /> 
        <input type="hidden" id="hidFormCode" value="CONTRCTINQ" />
        <input type="hidden" id="hidCurrentUserID" runat="server" /> 
        <input type="hidden" id="hidCurrentUserEmpNo" runat="server" /> 
        <input type="hidden" id="hidCurrentUserEmpName" runat="server" /> 
        <input type="hidden" id="hidCostCenter" runat="server" />         
    </form>

    <%--Local JS file reference--%>
    <script src="../../Scripts/Contractor/common.js?v=<%=JSVersion%>"></script>
    <script src="../../Scripts/Contractor/ContractInquiry.js?v=<%=JSVersion%>"></script>
</body>
</html>
