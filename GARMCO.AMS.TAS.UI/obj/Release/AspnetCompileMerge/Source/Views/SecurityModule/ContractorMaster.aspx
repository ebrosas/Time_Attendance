<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/TASMaster.Master" AutoEventWireup="true" CodeBehind="ContractorMaster.aspx.cs" 
    Inherits="GARMCO.AMS.TAS.UI.Views.SecurityModule.ContractorMaster" %>

<%@ MasterType VirtualPath="~/Views/Shared/TASMaster.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <title>Contractor Registration</title>

    <link rel="stylesheet" href="../../Content/lib/bootstrap/css/bootstrap.min.css" />
    <link rel="stylesheet" href="../../Content/lib/fontawesome/all.min.css" />
    <link rel="stylesheet" href="../../Content/lib/loader/waitMe.min.css" />
    <link rel="stylesheet" href="../../Styles/common.css" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="mainContent" runat="server">
    <asp:ScriptManagerProxy ID="scriptMain" runat="server">
        <Scripts>
            <asp:ScriptReference Path="~/Content/lib/jquery/jquery-3.6.0.min.js" />
            <asp:ScriptReference Path="~/Content/lib/bootstrap/js/bootstrap.bundle.min.js" />
            <asp:ScriptReference Path="~/Content/lib/fontawesome/all.min.js" />
            <asp:ScriptReference Path="~/Content/lib/loader/waitMe.min.js" />
        </Scripts>
    </asp:ScriptManagerProxy>

    <div class="container-fluid formWrapper">
        <div class="row">
            <div class="col-12">
                <div class="adminInnerFrame">
                    <iframe id="ifInnerFrame" class="ifInnerFrame" src="" runat="server"></iframe>
                </div>
            </div>
        </div>
    </div>

    <input type="hidden" id="hidFormName" runat="server" /> 

    <%--Local JS file reference--%>
    <script src="../../Scripts/Contractor/common.js?v=<%=JSVersion%>"></script>
    <script src="../../Scripts/Contractor/ContractMaster.js?v=<%=JSVersion%>"></script>
</asp:Content>
