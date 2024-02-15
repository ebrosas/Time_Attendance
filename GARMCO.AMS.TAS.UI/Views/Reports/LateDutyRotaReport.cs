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
    public partial class rptLateDutyRotaReport : Telerik.Reporting.Report
    {
        #region Properties
        public List<EmployeeAttendanceEntity> ReportDataList { get; set; }
        #endregion

        public rptLateDutyRotaReport()
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
            //    Telerik.Reporting.Processing.ReportSection;
            //if (section != null)
            //{
            //    #region Fill data to DIL table
            //    Telerik.Reporting.Processing.Table tblDILData = section.ChildElements["tblDILData"] as Telerik.Reporting.Processing.Table;
            //    if (tblDILData != null)
            //    {
            //        tblDILData.DataSource = this.ReportDataList;
            //    }
            //    #endregion

            //    #region Fill data to Duty Rota table
            //    Telerik.Reporting.Processing.Table tblDutyRotaData = section.ChildElements["tblDutyRotaData"] as Telerik.Reporting.Processing.Table;
            //    if (tblDutyRotaData != null)
            //    {
            //        tblDutyRotaData.DataSource = this.ReportDataList;
            //    }
            //    #endregion
            //}
        }
    }
}