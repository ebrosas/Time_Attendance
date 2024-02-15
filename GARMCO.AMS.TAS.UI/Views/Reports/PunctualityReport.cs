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
    public partial class PunctualityReport : Telerik.Reporting.Report
    {
        #region Properties
        public List<AttendanceStatisticsEntity> ReportDataList { get; set; }
        #endregion

        public PunctualityReport()
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
            //Telerik.Reporting.Processing.ReportSection section = sender as
            //   Telerik.Reporting.Processing.ReportSection;
            //if (section != null)
            //{
            //    #region Fill data to Swipes History section
            //    Telerik.Reporting.Processing.Table tblAttendance = section.ChildElements["tblAttendance"] as Telerik.Reporting.Processing.Table;
            //    if (tblAttendance != null)
            //    {
            //        tblAttendance.DataSource = this.ReportDataList;
            //    }
            //    #endregion
            //}
        }
    }
}