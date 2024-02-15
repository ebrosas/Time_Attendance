using GARMCO.AMS.TAS.UI.Helpers;
using GARMCO.AMS.TAS.UI.Models;
using GARMCO.AMS.TAS.UI.Repositories;
using GARMCO.AMS.TAS.UI.Views.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Reporting;
using Telerik.ReportViewer.WebForms;

namespace GARMCO.AMS.TAS.UI.Views.SecurityModule
{
    public partial class ContractorReportViewer : System.Web.UI.Page
    {
        #region Fields
        private ContractorRepository _repository;
        public enum ReportType
        {
            IDCardLicenseReport,
            LicenseOnlyReport,
            IDCardOnlyReport,
            ContractorDetailsReport
        }
        #endregion

        #region Properties
        public string JSVersion { get { return Session[UIHelper.CONST_JS_VERSION].ToString(); } }

        private ContractorRepository Repository
        {
            get
            {
                if (_repository == null)
                    _repository = new ContractorRepository();
                return _repository;
            }
            set
            {
                _repository = value;
            }
        }

        public ReportType CurrentReportType { get; set; }
        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                this.hidEmpNo.Value = GetQueryStringValue("empNo");
                this.hidIsContractor.Value = GetQueryStringValue("isContractor");

                string fileName = GetQueryStringValue("fileName");
                int empConNo = UIHelper.ConvertObjectToInt(this.hidEmpNo.Value);

                CurrentReportType = (ReportType) Enum.Parse(typeof(ReportType), GetQueryStringValue("reporttype"));
                switch(CurrentReportType)
                {
                    case ReportType.IDCardLicenseReport:
                        #region Display both ID Card and License Info 
                        if (empConNo > 0)
                        {
                            if (UIHelper.ConvertObjectToBolean(this.hidIsContractor.Value))
                                DisplayContractorIDCard(empConNo, fileName, true);
                            else
                                DisplayEmployeeIDCard(empConNo, fileName, true);
                        }
                        break;
                        #endregion

                    case ReportType.LicenseOnlyReport:
                        #region Display License Info only
                        if (empConNo > 0)
                        {
                            if (UIHelper.ConvertObjectToBolean(this.hidIsContractor.Value))
                                DisplayContractorLicenseCard(empConNo);
                            else
                                DisplayEmployeeLicenseCard(empConNo);
                        }
                        break;
                        #endregion

                    case ReportType.IDCardOnlyReport:
                        #region Display ID Card only
                        if (empConNo > 0)
                        {
                            if (UIHelper.ConvertObjectToBolean(this.hidIsContractor.Value))
                                DisplayContractorIDCard(empConNo, fileName, false);
                            else
                                DisplayEmployeeIDCard(empConNo, fileName, false);
                        }
                        break;
                    #endregion

                    case ReportType.ContractorDetailsReport:
                        DisplayContractorReport(UIHelper.ConvertObjectToInt(this.hidEmpNo.Value));
                        break;
                }                
            }
        }

        #region Private Functions
        private void DisplayContractorIDCard(int contractorNo, string fileName, bool showLicense)
        {
            try
            {
                // Initialize report variables
                Report rep = null;
                dynamic reportDoc;
                
                ContractorRegistryEntity model = Repository.GetContractorDetails(contractorNo);
                if (model != null)
                {
                    if (showLicense)
                    {
                        if (model.licenseList != null && model.licenseList.Count > 2)
                        {
                            if (model.licenseList.Count <= 5)
                                reportDoc = new ContractorIDCard5License();
                            else
                                reportDoc = new ContractorIDCardMoreLicense();

                            // Initialize parameters
                            reportDoc.ReportParameters["bloodGroupDesc"].Value = model.bloodGroupDesc;
                            reportDoc.ReportParameters["idNumber"].Value = model.idNumber;
                            reportDoc.ReportParameters["IDTypeDescription"].Value = model.IDTypeDescription;
                        }
                        else
                        {
                            reportDoc = new ContractorIDCard();
                        }
                    }
                    else
                        reportDoc = new ContractorIDNoLicense();

                    // Sets the data source
                    reportDoc.DataSource = model;
                    reportDoc.ReportData = model;
                    reportDoc.LicenseList = model.licenseList;
                    reportDoc.ImageFileName = fileName;

                    // Sets the report
                    rep = reportDoc;
                }

                #region Sets the report to the viewer
                InstanceReportSource instanceRepSource = new InstanceReportSource();
                instanceRepSource.ReportDocument = rep;
                this.repViewer.ReportSource = instanceRepSource;
                this.repViewer.ViewMode = ViewMode.PrintPreview;
                instanceRepSource = null;
                #endregion
            }
            catch (Exception ex)
            {
                UIHelper.DisplayJavaScriptMessage(this, ex.Message.ToString());
            }
        }

        private void DisplayContractorLicenseCard(int contractorNo)
        {
            try
            {
                // Initialize report variables
                Report rep = null;
                dynamic reportDoc;

                ContractorRegistryEntity model = Repository.GetContractorDetails(contractorNo);
                if (model != null)
                {
                    if (model.licenseList != null && model.licenseList.Count > 2)
                    {
                        if (model.licenseList.Count <= 5)
                            reportDoc = new ContractorFiveLicenseCard();
                        else
                            reportDoc = new ContractorMoreLicenseCard();
                    }
                    else
                    {
                        reportDoc = new ContractorLicenseCard();
                    }

                    // Initialize parameters
                    reportDoc.ReportParameters["UserName"].Value = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_FULLNAME]);

                    // Sets the data source
                    reportDoc.DataSource = model;
                    reportDoc.ContractorDetails = model;
                    reportDoc.LicenseList = model.licenseList;

                    // Sets the report
                    rep = reportDoc;
                }

                #region Sets the report to the viewer
                InstanceReportSource instanceRepSource = new InstanceReportSource();
                instanceRepSource.ReportDocument = rep;
                this.repViewer.ReportSource = instanceRepSource;
                this.repViewer.ViewMode = ViewMode.PrintPreview;
                instanceRepSource = null;
                #endregion
            }
            catch (Exception ex)
            {
                UIHelper.DisplayJavaScriptMessage(this, ex.Message.ToString());
            }
        }

        private void DisplayEmployeeIDCard(int empNo, string fileName, bool showLicense)
        {
            try
            {
                // Initialize report variables
                Report rep = null;
                dynamic reportDoc; 

                EmployeeEntity model = Repository.SearchIDCard(empNo);
                if (model != null)
                {
                    if (showLicense)
                    {
                        if (model.LicenseList != null && model.LicenseList.Count > 2)
                        {
                            if (model.LicenseList.Count <= 5)
                                reportDoc = new EmployeeIDCardFiveLicense();
                            else
                                reportDoc = new EmployeeIDCardMoreLicense();

                            // Initialize parameters
                            reportDoc.ReportParameters["bloodGroupDesc"].Value = model.BloodGroupDesc;
                            reportDoc.ReportParameters["cprNo"].Value = model.CPRNo;
                        }
                        else
                        {
                            reportDoc = new EmployeeIDCard();                            
                        }
                    }
                    else
                        reportDoc = new EmployeeIDNoLicense();

                    // Decode the custom cost center
                    if (!string.IsNullOrEmpty(model.CustomCostCenter))
                        model.CustomCostCenter = Server.HtmlDecode(model.CustomCostCenter);

                    // Sets the data source
                    reportDoc.DataSource = model;
                    reportDoc.ReportData = model;
                    reportDoc.LicenseList = model.LicenseList;
                    reportDoc.ImageFileName = fileName;

                    // Sets the report
                    rep = reportDoc;
                }

                #region Sets the report to the viewer
                InstanceReportSource instanceRepSource = new InstanceReportSource();
                instanceRepSource.ReportDocument = rep;
                this.repViewer.ReportSource = instanceRepSource;
                this.repViewer.ViewMode = ViewMode.PrintPreview;
                instanceRepSource = null;
                #endregion
            }
            catch (Exception ex)
            {
                UIHelper.DisplayJavaScriptMessage(this, ex.Message.ToString());
            }
        }

        private void DisplayEmployeeLicenseCard(int empNo)
        {
            try
            {
                // Initialize report variables
                Report rep = null;
                dynamic reportDoc;

                EmployeeEntity model = Repository.SearchIDCard(empNo);
                if (model != null)
                {
                    if (model.LicenseList != null && model.LicenseList.Count > 2)
                    {
                        if (model.LicenseList.Count <= 5)
                            reportDoc = new EmployeeLicenseCardFive();
                        else
                            reportDoc = new EmployeeLicenseCardMore();
                    }
                    else
                    {
                        reportDoc = new EmployeeLicenseCard();
                    }

                    // Initialize parameters
                    reportDoc.ReportParameters["UserName"].Value = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_FULLNAME]);

                    // Decode the custom cost center
                    if (!string.IsNullOrEmpty(model.CustomCostCenter))
                        model.CustomCostCenter = Server.HtmlDecode(model.CustomCostCenter);

                    // Sets the data source
                    reportDoc.DataSource = model;
                    reportDoc.EmployeeDetails = model;
                    reportDoc.LicenseList = model.LicenseList;

                    // Sets the report
                    rep = reportDoc;
                }

                #region Sets the report to the viewer
                InstanceReportSource instanceRepSource = new InstanceReportSource();
                instanceRepSource.ReportDocument = rep;
                this.repViewer.ReportSource = instanceRepSource;
                this.repViewer.ViewMode = ViewMode.PrintPreview;
                instanceRepSource = null;
                #endregion
            }
            catch (Exception ex)
            {
                UIHelper.DisplayJavaScriptMessage(this, ex.Message.ToString());
            }
        }

        private void DisplayContractorReport(int contractorNo)
        {
            try
            {
                // Initialize report variables
                Report rep = null;
                ContractorDetailsReport repContractor = new ContractorDetailsReport();

                ContractorRegistryEntity model = Repository.GetContractorDetails(contractorNo);
                if (model != null)
                {
                    #region Sets the parameters
                    repContractor.ReportParameters["UserID"].Value = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]);
                    repContractor.ReportParameters["MachineName"].Value = System.Environment.MachineName;
                    #endregion

                    // Sets the data source
                    repContractor.DataSource = model;
                    repContractor.LicenseList = model.licenseList;

                    // Sets the report
                    rep = repContractor;
                }

                #region Sets the report to the viewer
                InstanceReportSource instanceRepSource = new InstanceReportSource();
                instanceRepSource.ReportDocument = rep;
                this.repViewer.ReportSource = instanceRepSource;
                this.repViewer.ViewMode = ViewMode.PrintPreview;
                instanceRepSource = null;
                #endregion
            }
            catch (Exception ex)
            {
                UIHelper.DisplayJavaScriptMessage(this, ex.Message.ToString());
            }
        }                               

        private string GetQueryStringValue(string key)
        {
            return string.IsNullOrWhiteSpace(Request.QueryString[key]) ? string.Empty : Request.QueryString[key];
        }
        #endregion
    }
}