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
    using System.Linq;

    /// <summary>
    /// Summary description for VisitorPassSummaryReport.
    /// </summary>
    public partial class EmployeeAbsencesReport : Telerik.Reporting.Report
    {
        #region Properties
        public List<EmployeeAbsentEntity> ReportDataList { get; set; }
        #endregion

        public EmployeeAbsencesReport()
        {
            //
            // Required for telerik Reporting designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
        }
    }
}