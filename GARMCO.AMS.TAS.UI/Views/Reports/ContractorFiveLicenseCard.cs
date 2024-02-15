namespace GARMCO.AMS.TAS.UI.Views.Reports
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Collections.Generic;
    using BL.Entities;
    using Telerik.Reporting;
    using Telerik.Reporting.Drawing;
    using Models;
    using System.Configuration;
    using System.Web;
    using System.IO;

    /// <summary>
    /// Summary description for VisitorPassSummaryReport.
    /// </summary>
    public partial class ContractorFiveLicenseCard : Telerik.Reporting.Report
    {
        #region Properties
        public List<LicenseEntity> LicenseList { get; set; }
        public ContractorRegistryEntity ContractorDetails { get; set; }
        #endregion

        public ContractorFiveLicenseCard()
        {
            //
            // Required for telerik Reporting designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
        }

        private void detail_ItemDataBinding(object sender, EventArgs e)
        {
            Telerik.Reporting.Processing.ReportSection section = sender as
               Telerik.Reporting.Processing.ReportSection;
            if (section != null && section.ChildElements.Count > 0)
            {
                #region Fill data to License table
                Telerik.Reporting.Processing.Table tblDetail = section.ChildElements[0].ChildElements["tblDetail"] as Telerik.Reporting.Processing.Table;
                if (tblDetail != null)
                {
                    tblDetail.DataSource = this.ContractorDetails;
                }
                #endregion

                #region Fill data to License table
                Telerik.Reporting.Processing.Table tblLicense = section.ChildElements[1].ChildElements["tblLicense"] as Telerik.Reporting.Processing.Table;
                if (tblLicense != null)
                {
                    tblLicense.DataSource = this.LicenseList;
                }
                #endregion
            }
        }
    }
}