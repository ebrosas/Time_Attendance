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

    /// <summary>
    /// Summary description for VisitorLogReport.
    /// </summary>
    public partial class VisitorLogReport : Telerik.Reporting.Report
    {
        #region Properties
        public List<VisitorSwipeEntity> SwipeDataList { get; set; }
        #endregion

        public VisitorLogReport()
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
                Telerik.Reporting.Processing.Table tblSwipeSummary = section.ChildElements["tblSwipeSummary"] as Telerik.Reporting.Processing.Table;
                if (tblSwipeSummary != null)
                {
                    tblSwipeSummary.DataSource = this.SwipeDataList;

                    //if (this.SwipeDataList != null 
                    //    && this.SwipeDataList.Count > 0)
                    //{
                    //    tblSwipeSummary.Visible = true;
                    //}
                    //else
                    //{
                    //    tblSwipeSummary.Visible = false;
                    //}
                }
                #endregion
            }
        }
    }
}