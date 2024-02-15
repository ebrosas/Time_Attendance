<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="IDCardGenerator.aspx.cs" Inherits="GARMCO.AMS.TAS.UI.Views.SecurityModule.IDCardGenerator" %>

<!DOCTYPE html>

<html lang="en">
<head runat="server">
    <title>ID Card Generator</title>

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
    <link rel="stylesheet" href="../../Styles/cardGenerator.css" />

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
        <%--Card History Entry Form--%> 
        <div class="modal fade" id="modCardInfo">
            <div class="modal-dialog modal-dialog-centered">
                <div class="modal-content">
                    <%--Modal Header--%>
                    <div class="modal-header bg-info">
                        &nbsp;&nbsp;
                        <span class="text-white"><i class='fas fa-id-card fa-3x fa-fw'></i></span>
                        <span class="modal-title text-white modalHeader" style="line-height: 2.5;">&nbsp;&nbsp;Manage Card Information</span>
                        <button type="button" class="close" data-dismiss="modal">
                            <span class="text-danger"><i class="fas fa-window-close"></i></span>
                        </button>
                    </div>

                    <%--Modal Body--%>
                    <div class="modal-body">                       
                        <div class="form-row my-1">
                            <div class="col-sm-3 modalFieldTitle">
                                <label for="txtCardRefNo" class="col-form-label" data-field="cardrefno">
                                    <span><i class="fas fa-asterisk fa-fw fa-xs text-danger"></i></span>
                                    Card Ref. #
                                </label>   
                            </div>
                            <div class="col-sm-9">
                                <input type="text" id="txtCardRefNo" class="form-control form-control-sm fieldValue" maxlength="20" tabindex="27" data-primarykey="yes" required style="font-size: 14px;" />
                                <div class="form-row errorPanel" id="cardRefNoValid" hidden>
                                    <div class="col-sm-12">
                                        <div class="alert alert-danger alert-dismissible mt-1 fade show">
                                            <span><i class="fas fa-times-circle fa-fw fa-lg"></i></span>
                                            <span class="errorText"></span>
                                        </div>
                                    </div>
                                </div>    
                            </div>
                        </div>
                        <div class="form-row my-1">
                            <div class="col-sm-3 modalFieldTitle">
                                <label for="txtCardRemarks">Remarks</label>   
                            </div>
                            <div class="col-sm-9">
                                <textarea id="txtCardRemarks" rows="7" class="form-control form-control-sm fieldValue" maxlength="300" tabindex="30"></textarea>
                            </div>
                        </div>
                        <div class="form-row pl-1 mt-2">
                            <div class="col-sm-12 text-danger clearfix">
                                <span class="float-left ml-1"><i class="fas fa-asterisk fa-fw fa-xs"></i></span>                                                            
                                <span class="font-italic float-left">- indicates a required field</span>                                
                            </div>
                        </div>   
                        <input type="hidden" id="hidCardGUID" />
                    </div>

                    <div class="modal-footer">
                        <button type="button" class="btn btn-link font-weight-bold modalButton" data-button-value="modalCancel" data-dismiss="modal" tabindex="32" style="width: 90px;">
                            Cancel
                        </button>
                        <button type="button" class="btn btn-sm btn-danger modalButton" data-button-value="modalDelete" tabindex="33" style="width: 110px;">
                            <span><i class="fas fa-trash fa-fw fa-1x"></i></span>&nbsp;Delete
                        </button>
                        <button type="button" class="btn btn-sm btn-primary modalButton" data-button-value="modalSave" tabindex="34" style="width: 110px;">
                            <span><i class="fas fa-thumbs-up fa-fw fa-1x"></i></span>&nbsp;Save
                        </button>
                    </div>
                </div>
            </div>
        </div>

        <%--License Registration Form--%> 
        <div class="modal fade" id="modLicenseRegistration">
            <div class="modal-dialog modal-lg modal-dialog-centered">
                <div class="modal-content">
                    <%--Modal Header--%>
                    <div class="modal-header bg-info">
                        &nbsp;&nbsp;
                        <span class="text-white"><i class='fas fa-id-card fa-3x fa-fw'></i></span>
                        <span class="modal-title text-white modalHeader" style="line-height: 2.5;">&nbsp;&nbsp;License Registration</span>
                        <button type="button" class="close" data-dismiss="modal">
                            <span class="text-danger"><i class="fas fa-window-close"></i></span>
                        </button>
                    </div>

                    <%--Modal Body--%>
                    <div class="modal-body">                       
                        <div class="form-row my-1">
                            <div class="col-sm-2 modalFieldTitle">
                                <label for="cboLicenseType" class="col-form-label" data-field="LicenseType">
                                    <span><i class="fas fa-asterisk fa-fw fa-xs text-danger"></i></span>
                                    License Type
                                </label>   
                            </div>
                            <div class="col-sm-4">
                                <select id="cboLicenseType" class="form-control custom-select custom-select-sm fieldValue" name="costCenter" data-primarykey="yes" tabindex="26">
                                </select>
                                <div class="form-row errorPanel" id="licenseTypeValid" hidden>
                                    <div class="col-sm-12">
                                        <div class="alert alert-danger alert-dismissible mt-1 fade show">
                                            <span><i class="fas fa-times-circle fa-fw fa-lg"></i></span>
                                            <span class="errorText"></span>
                                        </div>
                                    </div>
                                </div>    
                            </div>
                            <div class="col-sm-2 modalFieldTitle">
                                <label for="txtLicenseNo" class="col-form-label" data-field="LicenseNo">
                                    <span><i class="fas fa-asterisk fa-fw fa-xs text-danger"></i></span>
                                    License No.
                                </label>   
                            </div>
                            <div class="col-sm-4 pr-3">
                                <input type="text" id="txtLicenseNo" class="form-control form-control-sm fieldValue" maxlength="20" tabindex="27" data-primarykey="yes" style="font-size: 14px;" />
                                <div class="form-row errorPanel" id="licenseNoValid" hidden>
                                    <div class="col-sm-12">
                                        <div class="alert alert-danger alert-dismissible mt-1 fade show">
                                            <span><i class="fas fa-times-circle fa-fw fa-lg"></i></span>
                                            <span class="errorText"></span>
                                        </div>
                                    </div>
                                </div>    
                            </div>
                        </div>
                        <div class="form-row my-1">
                            <div class="col-sm-2 modalFieldTitle">
                                <label for="txtIssuedDate" class="col-form-label" data-field="IssuedDate">
                                    <span><i class="fas fa-asterisk fa-fw fa-xs text-danger"></i></span>
                                    Issued Date
                                </label>   
                            </div>
                            <div class="col-sm-4">
                                <div class="input-group input-group-sm mt-1">
                                    <div class="input-group-prepend">
                                        <span class="input-group-text inputGroupTitle">
                                            <i class="far fa-calendar-alt fa-1x fa-fw"></i>
                                        </span>
                                    </div>
                                    <input class="borderLess fieldValue form-control-sm" type="text" id="txtIssuedDate" maxlength="10" tabindex="28" readonly 
                                        style="width: 120px; font-size: 14px; text-align: center;" />
                                    <input type="hidden" id="hdnIssuedDate" />
                                </div> 
                                <div class="form-row errorPanel" id="issuedDateValid" hidden>
                                    <div class="col-sm-12">
                                        <div class="alert alert-danger alert-dismissible mt-1 fade show">
                                            <span><i class="fas fa-times-circle fa-fw fa-lg"></i></span>
                                            <span class="errorText"></span>
                                        </div>
                                    </div>
                                </div> 
                            </div>
                            <div class="col-sm-2 modalFieldTitle">
                                <label for="txtExpiryDate" class="col-form-label" data-field="ExpiryDate">
                                    <span><i class="fas fa-asterisk fa-fw fa-xs text-danger"></i></span>
                                    Expiry Date
                                </label>   
                            </div>
                            <div class="col-sm-4">
                                <div class="input-group input-group-sm mt-1">
                                    <div class="input-group-prepend">
                                        <span class="input-group-text inputGroupTitle">
                                            <i class="far fa-calendar-alt fa-1x fa-fw"></i>
                                        </span>
                                    </div>
                                    <input class="borderLess fieldValue form-control-sm" type="text" id="txtExpiryDate" maxlength="10" tabindex="29" readonly 
                                        style="width: 120px; font-size: 14px; text-align: center;" />
                                    <input type="hidden" id="hdnExpiryDate" />
                                </div> 
                                <div class="form-row errorPanel" id="expiryDateValid" hidden>
                                    <div class="col-sm-12">
                                        <div class="alert alert-danger alert-dismissible mt-1 fade show">
                                            <span><i class="fas fa-times-circle fa-fw fa-lg"></i></span>
                                            <span class="errorText"></span>
                                        </div>
                                    </div>
                                </div> 
                            </div>
                        </div>
                        <div class="form-row mt-2">
                            <div class="col-sm-2 modalFieldTitle">
                                <label for="txtIssuingAuthority" data-field="IssueAuthority">Issuing Authority</label>   
                            </div>
                            <div class="col-sm-4">
                                <textarea id="txtIssuingAuthority" rows="4" class="form-control form-control-sm fieldValue" maxlength="200" tabindex="30"></textarea>
                            </div>
                            <div class="col-sm-2 modalFieldTitle">
                                <label for="txtNotes" data-field="Notes">Notes</label>   
                            </div>
                            <div class="col-sm-4">
                                <textarea id="txtNotes" rows="4" class="form-control form-control-sm fieldValue" maxlength="300" tabindex="31"></textarea>
                            </div>
                        </div>
                        <div class="form-row pl-1 mt-2">
                            <div class="col-sm-12 text-danger clearfix">
                                <span class="float-left ml-1"><i class="fas fa-asterisk fa-fw fa-xs"></i></span>                            
                                <span class="font-italic float-left">- indicates a required field</span>
                            </div>
                        </div>   
                        <input type="hidden" id="hidLicenseGUID" />
                    </div>

                    <div class="modal-footer">
                        <button type="button" class="btn btn-link font-weight-bold modalButton" data-button-value="modalCancel" data-dismiss="modal" tabindex="32" style="width: 90px;">
                            Cancel
                        </button>
                        <button type="button" class="btn btn-sm btn-danger modalButton" data-button-value="modalDelete" tabindex="33" style="width: 110px;">
                            <span><i class="fas fa-trash fa-fw fa-1x"></i></span>&nbsp;Delete
                        </button>
                        <button type="button" class="btn btn-sm btn-primary modalButton" data-button-value="modalSave" tabindex="34" style="width: 110px;">
                            <span><i class="fas fa-thumbs-up fa-fw fa-1x"></i></span>&nbsp;Save
                        </button>
                    </div>
                </div>
            </div>
        </div>

        <%--Pop-up Confirmation Form--%>
        <div class="modal fade" id="modalConfirmation">
            <div class="modal-dialog modal-dialog-centered">
                <div class="modal-content">
                    <%--Modal Header--%>
                    <div class="modal-header">
                        <span class="text-danger"><i id="modalConfirmationIcon" class='fas fa-info-circle fa-2x fa-fw'></i></span>
                        <span class="modal-title text-danger modalHeader">&nbsp;Confirmation</span>
                        <button type="button" class="close" data-dismiss="modal">
                            <span class="text-danger"><i class="fas fa-window-close"></i></span>
                        </button>
                    </div>

                    <%--Modal Body--%>
                    <div class="modal-body">
                        <p></p>
                    </div>

                    <div class="modal-footer">
                        <button type="button" class="btn btn-danger modalButton" data-button-value="modalNo" data-dismiss="modal">
                            <span><i class="fas fa-thumbs-down fa-fw fa-1x"></i></span>&nbsp;No
                        </button>
                        <button type="button" class="btn btn-success modalButton" data-button-value="modalYes" data-dismiss="modal">
                            <span><i class="fas fa-thumbs-up fa-fw fa-1x"></i></span>&nbsp;Yes
                        </button>
                    </div>
                </div>
            </div>
        </div>
          
        <div class="container-fluid mt-2 mb-4 px-4">
            <div class="row no-gutters">
                <div class="col-sm-12">
                    <div class="formHeader">
                        <span><i class="fas fa-id-card-alt fa-lg fa-fw"></i></span>&nbsp;
                        ID Card Generator (Employee / Contractor)
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

                <div class="container-fluid pl-5 pr-0 pb-3 pt-0">      
                    <div class="form-row pr-4 ml-0 mt-0">
                        <div class="col-sm-12 text-danger clearfix">
                            <span class="font-italic float-right">- indicates a required field</span>
                            <span class="float-right ml-1"><i class="fas fa-asterisk fa-fw fa-xs"></i></span>                            
                        </div>
                    </div>  
                    <div class="form-row pt-0 mt-1 pl-0 ml-0 pb-2">
                        <div class="col-12">
                             <div class="custom-control custom-switch">
                                <input type="checkbox" class="custom-control-input" id="conEmpSwitch" checked>
                                <label class="custom-control-label groupTitle" for="conEmpSwitch" data-switch-value="ShowHideContractEmp">Manage Contractor ID Card</label>
                              </div>
                        </div>
                    </div>              

                    <div class="form-row">
                        <div class="col-sm-7">
                            <div id="collapseContractor" class="collapse show">
                                <div class="form-row py-1">
                                    <div class="col-sm-3 fieldTitle pr-2">
                                        <span><i class="fas fa-asterisk fa-fw fa-xs text-danger"></i></span>
                                        <label for="txtContractorNo" class="col-form-label" data-field="ContractorNo">Contractor No.</label>                        
                                    </div>
                                    <div class="col-sm-8">         
                                        <div class="form-row">
                                            <div class="col-sm-5">
                                                <div class="input-group input-group-sm">
                                                    <input type="text" class="form-control form-control-sm fieldValue" id="txtContractorNo" placeholder="6xxxx" tabindex="1" maxlength="5" 
                                                        title="(Note: Accepts numbers only with a maximum length of 5 digits." required                                             
                                                        style="font-size: 14px; font-weight: 800;" />
                                                    <div class="input-group-append pl-1">
                                                        <button type="button" id="btnFind" class="btn btn-sm btn-success formButton"> 
                                                            Go
                                                        </button>                                        
                                                    </div>                                        
                                                    <div class="input-group-append pl-0">
                                                        <button type="button" id="btnSearch" class="btn btn-sm btn-success formButton"> 
                                                            <span><i class="fas fa-search fa-fw"></i></span>
                                                        </button>                                        
                                                    </div>    
                                                </div>
                                            </div>
                                        </div>   
                                        <div class="form-row errorPanel" id="contractorNoValid" hidden>
                                            <div class="col-sm-12">
                                                <div class="alert alert-danger alert-dismissible mt-1 fade show">
                                                    <span><i class="fas fa-times-circle fa-fw fa-lg"></i></span>
                                                    <span class="errorText"></span>
                                                </div>
                                            </div>
                                        </div>                                                                  
                                    </div>
                                    <div class="col-sm-1">

                                    </div>
                                </div>
                                <div class="form-row py-1">
                                    <div class="col-sm-3 fieldTitle pr-2">
                                        <label for="txtContractorName" class="col-form-label" data-field="ContractorName">Contractor Name</label>                        
                                    </div>
                                    <div class="col-sm-8">    
                                        <input type="text" id="txtContractorName" class="form-control form-control-sm fieldValue" maxlength="30" placeholder="full name..." readonly 
                                            style="font-size: 14px;" />
                                    </div>
                                    <div class="col-sm-1">

                                    </div>
                                </div>
                                <div class="form-row py-1">
                                    <div class="col-sm-3 fieldTitle pr-2">
                                        <label for="txtIDNumber" class="col-form-label" data-field="IDNumber">ID Number</label>                        
                                    </div>
                                    <div class="col-sm-8">    
                                        <input type="text" id="txtIDNumber" class="form-control form-control-sm fieldValue" maxlength="30" placeholder="cpr or passport no..." readonly
                                            style="font-size: 14px;" />
                                    </div>
                                    <div class="col-sm-1">

                                    </div>
                                </div>
                                <div class="form-row py-1">
                                    <div class="col-sm-3 fieldTitle pr-2">
                                        <label for="txtJobTitle" class="col-form-label" data-field="JobTitle">Job Title</label>                        
                                    </div>
                                    <div class="col-sm-8">    
                                        <input type="text" id="txtJobTitle" class="form-control form-control-sm fieldValue" maxlength="30" placeholder="position..." readonly 
                                            style="font-size: 14px;" />
                                    </div>
                                    <div class="col-sm-1">

                                    </div>
                                </div>
                                <div class="form-row py-1">
                                    <div class="col-sm-3 fieldTitle pr-2">
                                        <label for="txtCompanyName" class="col-form-label" data-field="CompanyName">Company Name</label>                        
                                    </div>
                                    <div class="col-sm-8">    
                                        <input type="text" id="txtCompanyName" class="form-control form-control-sm fieldValue" maxlength="30" placeholder="registered company name..." readonly 
                                            style="font-size: 14px;" />
                                    </div>
                                    <div class="col-sm-1">

                                    </div>
                                </div>
                                <div class="form-row py-1">
                                    <div class="col-sm-3 fieldTitle pr-2">
                                        <label for="txtVisitedDept" class="col-form-label" data-field="CostCenter">Visited Department</label>                        
                                    </div>
                                    <div class="col-sm-8">    
                                        <input type="text" id="txtVisitedDept" class="form-control form-control-sm fieldValue" maxlength="30" placeholder="cost center..." readonly 
                                            style="font-size: 14px;" />
                                        <%--<select id="cboCostCenter" class="form-control custom-select custom-select-sm fieldValue" name="costCenter" tabindex="7" disabled>
                                        </select>--%>
                                    </div>
                                    <div class="col-sm-1">

                                    </div>
                                </div>
                            </div>                            

                            <div id="collapseEmployee" class="collapse">
                                <div class="form-row py-1">
                                    <div class="col-sm-2 fieldTitle pr-2">
                                        <span><i class="fas fa-asterisk fa-fw fa-xs text-danger"></i></span>
                                        <label for="txtEmpNo" class="col-form-label" data-field="EmpNo">Employee No.</label>                        
                                    </div>
                                    <div class="col-sm-8">         
                                        <div class="form-row">
                                            <div class="col-sm-6">
                                                <div class="input-group input-group-sm">
                                                    <input type="text" class="form-control form-control-sm fieldValue" id="txtEmpNo" placeholder="1000xxxx" tabindex="2" maxlength="8"
                                                        title="(Note: Accepts numbers only with a maximum length of 8 digits." required                                              
                                                        style="font-size: 14px; font-weight: 800;" />
                                                    <div class="input-group-append pl-1">
                                                        <button type="button" id="btnFindEmp" class="btn btn-sm btn-success formButton"> 
                                                            Go <%--<span><i class="fas fa-binoculars fa-fw"></i></span>--%>
                                                        </button>                                        
                                                    </div>                                        
                                                </div>
                                            </div>
                                        </div>   
                                        <div class="form-row errorPanel" id="empNoValid" hidden>
                                            <div class="col-sm-12">
                                                <div class="alert alert-danger alert-dismissible mt-1 fade show">
                                                    <span><i class="fas fa-times-circle fa-fw fa-lg"></i></span>
                                                    <span class="errorText"></span>
                                                </div>
                                            </div>
                                        </div>                                                                  
                                    </div>
                                    <div class="col-sm-2">

                                    </div>
                                </div>
                                <div class="form-row py-1">
                                    <div class="col-sm-2 fieldTitle pr-2">
                                        <span><i class="fas fa-asterisk fa-fw fa-xs text-danger"></i></span>
                                        <label for="txtEmpName" class="col-form-label" data-field="EmpName">Employee Name</label>                        
                                    </div>
                                    <div class="col-sm-8">    
                                        <input type="text" id="txtEmpName" class="form-control form-control-sm fieldValue" maxlength="100" placeholder="full name..." data-entry="yes" required readonly 
                                            style="font-size: 14px;" />
                                        <div class="form-row errorPanel" id="empNameValid" hidden>
                                            <div class="col-sm-12">
                                                <div class="alert alert-danger alert-dismissible mt-1 fade show">
                                                    <span><i class="fas fa-times-circle fa-fw fa-lg"></i></span>
                                                    <span class="errorText"></span>
                                                </div>
                                            </div>
                                        </div>   
                                    </div>
                                    <div class="col-sm-2">

                                    </div>
                                </div>
                                <div class="form-row py-1">
                                    <div class="col-sm-2 fieldTitle pr-2">
                                        <span><i class="fas fa-asterisk fa-fw fa-xs text-danger"></i></span>
                                        <label for="txtPosition" class="col-form-label" data-field="JobTitle">Position</label>                        
                                    </div>
                                    <div class="col-sm-8">    
                                        <input type="text" id="txtPosition" class="form-control form-control-sm fieldValue" maxlength="50" placeholder="job title..." data-entry="yes" required readonly 
                                            style="font-size: 14px;" />
                                        <div class="form-row errorPanel" id="positionValid" hidden>
                                            <div class="col-sm-12">
                                                <div class="alert alert-danger alert-dismissible mt-1 fade show">
                                                    <span><i class="fas fa-times-circle fa-fw fa-lg"></i></span>
                                                    <span class="errorText"></span>
                                                </div>
                                            </div>
                                        </div>   
                                    </div>
                                    <div class="col-sm-2">

                                    </div>
                                </div>
                                <div class="form-row py-1">
                                    <div class="col-sm-2 fieldTitle pr-2">
                                        <span><i class="fas fa-asterisk fa-fw fa-xs text-danger"></i></span>
                                        <label for="txtCostCenter" class="col-form-label" data-field="CostCenter">Cost Center</label>                        
                                    </div>
                                    <div class="col-sm-8">    
                                        <input type="text" id="txtCostCenter" class="form-control form-control-sm fieldValue" maxlength="100" placeholder="department..." data-entry="yes" required readonly 
                                            style="font-size: 14px;" />
                                        <div class="form-row errorPanel" id="costCenterValid" hidden>
                                            <div class="col-sm-12">
                                                <div class="alert alert-danger alert-dismissible mt-1 fade show">
                                                    <span><i class="fas fa-times-circle fa-fw fa-lg"></i></span>
                                                    <span class="errorText"></span>
                                                </div>
                                            </div>
                                        </div>   
                                    </div>
                                    <div class="col-sm-2">

                                    </div>
                                </div>
                                <div class="form-row py-1">
                                    <div class="col-sm-2 fieldTitle pr-2">
                                        <span><i class="fas fa-asterisk fa-fw fa-xs text-danger"></i></span>
                                        <label for="txtCPRNo" class="col-form-label" data-field="CPRNo">CPR No.</label>                        
                                    </div>
                                    <div class="col-sm-8">    
                                        <input type="text" id="txtCPRNo" class="form-control form-control-sm fieldValue" maxlength="9" placeholder="cpr number..." data-entry="yes" required readonly 
                                            style="font-size: 14px;" />
                                        <div class="form-row errorPanel" id="cprValid" hidden>
                                            <div class="col-sm-12">
                                                <div class="alert alert-danger alert-dismissible mt-1 fade show">
                                                    <span><i class="fas fa-times-circle fa-fw fa-lg"></i></span>
                                                    <span class="errorText"></span>
                                                </div>
                                            </div>
                                        </div>   
                                    </div>
                                    <div class="col-sm-2">

                                    </div>
                                </div>
                                <div class="form-row py-1">
                                    <div class="col-sm-2 fieldTitle pr-2">
                                        <label for="cboBloodGroup" class="col-form-label" data-field="BloodType">Blood Type</label>                        
                                    </div>
                                    <div class="col-sm-8">    
                                        <select id="cboBloodGroup" class="form-control custom-select custom-select-sm fieldValue" name="costCenter" tabindex="3" disabled style="width: 130px;">
                                        </select>
                                    </div>
                                    <div class="col-sm-2">

                                    </div>
                                </div>
                                <div class="form-row py-1" style="display: none;">
                                    <div class="col-sm-2 fieldTitle pr-2">
                                        <label for="txtSupervisor" class="col-form-label" data-field="Supervisor">Immediate Supervisor</label>                        
                                    </div>
                                    <div class="col-sm-8">    
                                        <input type="text" id="txtSupervisor" class="form-control form-control-sm fieldValue" maxlength="100" tabindex="3" placeholder="supervisor name..." readonly 
                                            style="font-size: 14px;" />
                                    </div>
                                    <div class="col-sm-2">

                                    </div>
                                </div>
                                <div class="form-row py-1" style="display: none;">
                                    <div class="col-sm-2 fieldTitle pr-2">
                                        <label for="txtManager" class="col-form-label" data-field="Supervisor">Cost Center Manager</label>                        
                                    </div>
                                    <div class="col-sm-8">    
                                        <input type="text" id="txtManager" class="form-control form-control-sm fieldValue" maxlength="100" placeholder="manager name..." readonly 
                                            style="font-size: 14px;" />
                                    </div>
                                    <div class="col-sm-2">

                                    </div>
                                </div>                                
                            </div>     

                            <div id="showCardPanel" class="form-row my-1 pl-0 ml-0" hidden>
                                <div class="col-sm-9">
                                     <div class="custom-control custom-switch">
                                        <input type="checkbox" class="custom-control-input" id="cardInfoSwitch">
                                        <label class="custom-control-label groupTitle text-primary" for="cardInfoSwitch" data-switch-value="ShowHideCardInfo">Show Card Information</label>
                                      </div>
                                </div>
                                <div id="panEditDetails" class="col-sm-2 pr-1 mr-0" style="text-align: right;">
                                    <button type="button" class="btn btn-info form-control-sm linkTitle" hidden>Edit Details</button>
                                </div>
                                <div class="col-1"></div>
                            </div>  

                            <div id="collapseCardInfo" class="collapse mt-2">
                                <div class="form-row mt-1 mb-0 groupTitle2">
                                    <div class="col-sm-12">
                                        <span>Card Information:</span> &nbsp;
                                        <span class="float-sm-right">
                                            <button type="button" class="btn btn-link pr-4 mr-4 linkTitle" style="color: #FF2768;">Add New Card</button>
                                        </span>
                                    </div>
                                </div>

                                <div class="form-row mt-0 pt-0 mb-2">                        
                                    <div class="col-sm-12 pr-5 pl-0 ml-0">
                                        <div class="container-fluid tablePanel">
                                            <div class="table-responsive py-3" style="-ms-overflow-style: auto;">
                                                <table id="cardHistoryTable" class="generalTable display nowrap stripe row-border  table-bordered" style="width: 100%;">
                                                    <thead>
                                                        <tr>
                                                            <th class="centeredColumn" style="width: 130px;">
                                                               Card Ref. No.
                                                            </th>                                                
                                                        
                                                            <th class="centeredColumn doNotOrder" style="width: 300px;">
                                                                Remarks
                                                            </th>
                                                            <th class="centeredColumn" style="width: 120px;">
                                                                Create Date
                                                            </th>
                                                            <th class="centeredColumn doNotOrder" style="width: 250px;">
                                                                Created By
                                                            </th>
                                                            <th class="hiddenColumn" style="width: 100px; display: none;">
                                                                Emp. No.
                                                            </th>
                                                            <th class="hiddenColumn" style="width: 100px; display: none;">
                                                                History ID
                                                            </th>
                                                            <th class="hiddenColumn" style="width: 100px; display: none;">
                                                                Card GUID
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
                        <div class="col-sm-1"></div>
                        <div class="col-sm-3">
                            <div class="groupHeader mt-n4">
                                Picture Information
                            </div>
                            <div class="groupBody pictureFrame">
                                <div class="form-row">
                                    <div class="col-12">
                                        <div style="height: 200px">
                                            <img src="../../Images/no_photo_big.png" class="rounded-circle img-thumbnail mx-auto d-block" alt="Employee Photo" style="width: 200px; height: 190px;" />
                                        </div>                                    
                                    </div>
                                    <div class="col-sm-12">
                                        <div class="btn-group-sm mx-auto text-center">
                                            <input type="file" id="uploadPhoto" onchange="encodeImageFileAsURL(this)" 
                                                name="imagePath" accept="image/x-png,image/gif,image/jpeg,image/bmp" hidden />
                                            <button id="btnBrowse" type="button" class="btn btn-outline-secondary pictureButton">
                                                <span hidden><i class="fas fa-spinner fa-spin"></i></span>
                                                Browse...
                                            </button>
                                            <button id="btnRemovePhoto" type="button" class="btn btn-outline-secondary pictureButton">
                                                Remove 
                                            </button>
                                        </div>
                                    </div>
                                </div>                                
                            </div>
                        </div>
                        <div class="col-sm-1"></div>
                    </div>   

                    <div class="form-row my-1 pl-0 ml-0" hidden>
                        <div class="col-12">
                            <div class="custom-control custom-switch">
                                <input type="checkbox" class="custom-control-input" id="licenseInfoSwitch">
                                <label class="custom-control-label groupTitle text-info" for="licenseInfoSwitch" data-switch-value="ShowHideLicenseInfo">Show License Information</label>
                            </div>
                        </div>
                    </div>  

                    <div class="form-row">
                        <div class="col-sm-12">
                            <div id="collapseLicense" class="collapse show">
                                <div class="container-fluid p-0 m-0">
                                    <div class="form-row mt-1 mb-0 groupTitle2">
                                        <div class="col-sm-12">
                                            <span>License Information:</span> &nbsp;
                                            <span class="float-sm-right">
                                                <button type="button" class="btn btn-link pr-4 mr-4 linkTitle" hidden disabled>Add New License</button>
                                            </span>
                                        </div>
                                    </div>

                                    <div class="form-row mt-0 pt-0 mb-1">                        
                                        <div class="col-sm-12 pr-5 pl-0 ml-0">
                                            <div class="container-fluid tablePanel">
                                                <div class="table-responsive py-3" style="-ms-overflow-style: auto;">
                                                    <table id="licenseTable" class="generalTable display nowrap stripe row-border  table-bordered" style="width: 100%;">
                                                        <thead>
                                                            <tr>
                                                                <th class="hiddenColumn" style="width: 100px; display: none;">
                                                                    Registry ID
                                                                </th>
                                                                <th class="hiddenColumn" style="width: 100px; display: none;">
                                                                    Contractor No.
                                                                </th>
                                                                <th class="centeredColumn" style="width: 120px;">
                                                                    License Type
                                                                </th>
                                                                <th class="centeredColumn" style="width: 100px;">
                                                                    License No.
                                                                </th>
                                                                <th class="centeredColumn" style="width: 100px;">
                                                                    Issued Date
                                                                </th>
                                                                <th class="centeredColumn" style="width: 100px;">
                                                                    Expiry Date
                                                                </th>
                                                                <th class="centeredColumn doNotOrder" style="width: 250px;">
                                                                    Issuing Authority
                                                                </th>
                                                                <th class="centeredColumn doNotOrder" style="width: 300px;">
                                                                    Notes
                                                                </th>
                                                                <th class="centeredColumn" style="width: 100px;">
                                                                    Created Date
                                                                </th>
                                                                <th class="centeredColumn doNotOrder" style="width: 300px;">
                                                                    Created By
                                                                </th>
                                                                <th class="hiddenColumn" style="width: 100px; display: none;">
                                                                    Last Update Date
                                                                </th>
                                                                <th class="hiddenColumn" style="width: 200px; display: none;">
                                                                    Last Updated By
                                                                </th>
                                                                <th class="hiddenColumn" style="width: 100px; display: none;">
                                                                    License GUID
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
                    </div>
                </div>   
                
                <div class="form-row mt-0 mb-3 ml-4">
                    <div class="col-sm-12 ml-2">
                        <div class="btn-group-sm">                             
                            <button type="button" id="btnCreateNew" class="form-control btn btn-primary border-0 actionButton" tabindex="4">
                                <span><i class="fas fa-edit fa-fw fa-1x"></i></span>&nbsp;
                                Create New
                            </button>
                            <button type="button" id="btnSave" class="form-control btn btn-success border-0 actionButton" tabindex="5" hidden>
                                <span><i class="fas fa-archive fa-fw fa-1x"></i></span>&nbsp;
                                Save
                            </button>
                            <button type="button" id="btnUpdate" class="form-control btn btn-success border-0 actionButton" tabindex="6" hidden>
                                <span><i class="fas fa-archive fa-fw fa-1x"></i></span>&nbsp;
                                Update
                            </button>
                            <button type="button" id="btnDelete" class="form-control btn btn-danger border-0 actionButton" tabindex="7" hidden>
                                <span><i class="far fa-trash-alt fa-fw fa-1x"></i></span>&nbsp;
                                Delete
                            </button>                            
                            <button type="button" id="btnReset" class="form-control btn btn-warning border-0 text-white actionButton" tabindex="8">
                                <span><i class="fas fa-sync fa-fw fa-1x"></i></span>&nbsp;
                                Reset
                            </button>
                            <div class="btn-group" id="printBtnGroup" hidden>
                                <button type="button" id="btnPrint" class="btn btn-secondary border-0 actionButton" style="margin: 0px; width: 150px;" tabindex="9">
                                    <span><i class="fas fa-print fa-fw fa-1x"></i></span>&nbsp;
                                    Print Card
                                </button>
                                <button type="button" class="btn btn-secondary border-0 dropdown-toggle dropdown-toggle-split" data-toggle="dropdown">
                                    <span class="caret"></span>
                                </button>
                                <div class="dropdown-menu">
                                    <a id="btnPrintLicenseOnly" class="dropdown-item link-basic" href="#">Print License Card only</a> 
                                    <a id="btnPrintIDOnly" class="dropdown-item link-basic" href="#">Print ID Card only</a> 
                                </div>
                            </div>
                            <button type="button" id="btnBack" class="form-control btn btn-outline-secondary actionButton" tabindex="10" hidden>
                                <span><i class="fas fa-arrow-circle-left fa-fw fa-1x"></i></span>&nbsp; 
                                Go Back
                            </button>
                            <input type="reset" id="btnHiddenReset" class="btn btn-warning text-white" value="Reset" hidden />   
                        </div>                                                      
                    </div>
                </div>     
            </div>
        </div>
        
        <input type="hidden" id="hidBase64Photo" /> 
        <input type="hidden" id="hidImageFileName" /> 
        <input type="hidden" id="hidCardRegistryID" /> 
        <input type="hidden" id="hidRegistryID" /> 
        <input type="hidden" id="hidCardHistoryID" /> 
        <input type="hidden" id="hidFormCode" value="CONTIDCARD" />
        <input type="hidden" id="hidCurrentUserID" runat="server" /> 
        <input type="hidden" id="hidCurrentUserEmpNo" runat="server" /> 
        <input type="hidden" id="hidCurrentUserEmpName" runat="server" /> 
        <input type="hidden" id="hidCostCenter" runat="server" />  
    </form>

    <%--Local JS file reference--%>
    <script src="../../Scripts/Contractor/common.js?v=<%=JSVersion%>"></script>
    <script src="../../Scripts/Contractor/CardGenerator.js?v=<%=JSVersion%>"></script>
</body>
</html>
