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

    /// <summary>
    /// Summary description for VisitorPassSummaryReport.
    /// </summary>
    public partial class DailyAttendanceReportNS : Telerik.Reporting.Report
    {
        public DailyAttendanceReportNS()
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