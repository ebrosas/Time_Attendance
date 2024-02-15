namespace GARMCO.AMS.TAS.UI.Views.Reports
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;
    using BL.Entities;
    using Telerik.Reporting;
    using Telerik.Reporting.Drawing;
    using System.Collections.Generic;
    using Models;

    /// <summary>
    /// Summary description for ContractorDetailsReport.
    /// </summary>
    public partial class ContractorDetailsReport : Telerik.Reporting.Report
    {
        #region Properties
        public List<LicenseEntity> LicenseList { get; set; }
        #endregion

        public ContractorDetailsReport()
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
            if (section != null)
            {
                #region Fill data to Swipes History section
                Telerik.Reporting.Processing.Table tblLicense = section.ChildElements["tblLicense"] as Telerik.Reporting.Processing.Table;
                if (tblLicense != null)
                {
                    tblLicense.DataSource = this.LicenseList;                                       
                }
                #endregion
            }
        }
    }
}