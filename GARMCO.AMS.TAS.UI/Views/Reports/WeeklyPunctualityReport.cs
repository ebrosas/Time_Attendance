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
    public partial class WeeklyPunctualityReport : Telerik.Reporting.Report
    {
        #region Properties
        public List<PunctualityEntity> ReportDataList { get; set; }
        #endregion

        public WeeklyPunctualityReport()
        {
            //
            // Required for telerik Reporting designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
        }

        private void groupHeaderSection_ItemDataBinding(object sender, EventArgs e)
        {
            Telerik.Reporting.Processing.ReportSection section = sender as
               Telerik.Reporting.Processing.ReportSection;
            if (section != null)
            {
                #region Fill data to Swipes History section
                Telerik.Reporting.Processing.Table tblHeader = section.ChildElements["tblHeader"] as Telerik.Reporting.Processing.Table;
                if (tblHeader != null)
                {
                    #region Build the datasource collection for the group header
                    if (this.ReportDataList.Count > 0)
                    {
                        //List<PunctualityEntity> groupHeaderDataSource = new List<PunctualityEntity>();
                        var groupHeaderDataSource = (from a in this.ReportDataList
                                                     select new
                                                     {
                                                         Day1 = a.Day1,
                                                         Day2 = a.Day2,
                                                         Day3 = a.Day3,
                                                         Day4 = a.Day4,
                                                         Day5 = a.Day5,
                                                         Day6 = a.Day6,
                                                         Day7 = a.Day7
                                                     })                                                     
                                                    .Distinct()
                                                    .ToList();
                        tblHeader.DataSource = groupHeaderDataSource;
                    }
                    #endregion
                }
                #endregion
            }
        }
    }
}