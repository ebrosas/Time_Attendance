<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ContractorReportViewer.aspx.cs" Inherits="GARMCO.AMS.TAS.UI.Views.SecurityModule.ContractorReportViewer" %>
<%@ Register Assembly="Telerik.ReportViewer.WebForms" Namespace="Telerik.ReportViewer.WebForms" TagPrefix="telerik" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<!DOCTYPE html>

<html lang="en">
<head runat="server">
    <title>ID Card Report Viewer</title>

    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />

    <%--CSS file references--%>
    <link rel="stylesheet" href="../../Content/lib/bootstrap/css/bootstrap.min.css" />
    <link rel="stylesheet" href="../../Content/lib/fontawesome/all.min.css" />
    <link rel="stylesheet" href="../../Content/lib/loader/waitMe.min.css" />
    <link rel="stylesheet" href="../../Content/lib/toastr/toastr.min.css" />
    <link rel="stylesheet" href="../../Styles/reportViewer.css" />

    <%--JavaScript file references--%>
    <script src="../../Content/lib/jquery/jquery-3.6.0.min.js"></script>
    <script src="../../Content/lib/bootstrap/js/bootstrap.bundle.min.js"></script>    
    <script src="../../Content/lib/fontawesome/all.min.js"></script>
    <script src="../../Content/lib/loader/waitMe.min.js"></script>
    <script src="../../Content/lib/toastr/toastr.min.js"></script>
</head>
<body>
    <form id="mainForm" class="formWrapper" runat="server">
        <asp:ScriptManager ID="scriptMngr" runat="server" />
        
        <div class="container-fluid mt-4 mb-5">
            <div class="form-row">                
                <div class="col-sm-12">
                    <div class="groupHeader">
                        <span><i class="fas fa-print fa-fw fa-lg"></i></span>&nbsp;
                        Report Viewer Form
                    </div>
                    <div class="groupBody reportPanel" style="height: 600px;">
                        <telerik:ReportViewer ID="repViewer" runat="server" BorderColor="#E0E0E0" BorderStyle="None" Height="100%" Skin="WebBlue" Width="100%" ZoomPercent="100">
                        </telerik:ReportViewer>
                    </div>                    
                </div>
            </div>
            <div class="form-row">
                <div class="col-12">
                    <button type="button" id="btnBack" class="form-control btn btn-dark actionButton" tabindex="1">
                        <span><i class="fas fa-arrow-circle-left fa-fw fa-lg"></i></span>&nbsp; 
                        Go Back
                    </button>
                    <input type="reset" id="btnHiddenReset" class="btn btn-warning text-white" value="Reset" hidden />   
                </div>
            </div>
            <%--<div class="form-row">
                <div class="col-sm-2">
                    <a href="#mainForm">Click here to go to top</a>
                </div>
            </div>--%>
        </div>

        <input type="hidden" id="hidEmpNo" runat="server" /> 
        <input type="hidden" id="hidIsContractor" runat="server" /> 
        <input type="hidden" id="hidCurrentUserID" runat="server" /> 
        <input type="hidden" id="hidCurrentUserEmpNo" runat="server" /> 
        <input type="hidden" id="hidCurrentUserEmpName" runat="server" /> 
    </form>

    <%--Local JS file reference--%>
    <script src="../../Scripts/Contractor/common.js?v=<%=JSVersion%>"></script>
    <script src="../../Scripts/Contractor/ReportViewer.js?v=<%=JSVersion%>"></script>
</body>
</html>
