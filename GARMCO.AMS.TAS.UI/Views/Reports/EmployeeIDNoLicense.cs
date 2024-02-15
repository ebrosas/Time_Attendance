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
    public partial class EmployeeIDNoLicense : Telerik.Reporting.Report
    {
        #region Fields
        private const int CONST_TEXT_MARGIN = 3;
        #endregion

        #region Properties
        public EmployeeEntity ReportData { get; set; }
        public List<LicenseEntity> LicenseList { get; set; }
        public string ImageFileName { get; set; }
        #endregion

        public EmployeeIDNoLicense()
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
            Telerik.Reporting.Processing.DetailSection detailSection = (sender as Telerik.Reporting.Processing.DetailSection);

            try
            {
                if (detailSection != null)
                {
                    #region Get the employee photo
                    Telerik.Reporting.Processing.PictureBox picEmpPhoto = detailSection.ChildElements[0].ChildElements["picEmpPhoto"] as Telerik.Reporting.Processing.PictureBox;
                    if (picEmpPhoto != null && !string.IsNullOrWhiteSpace(ImageFileName))
                    {
                        string photoFolder = ConfigurationManager.AppSettings["EmpPhotoFolder"];
                        string imagePath = HttpContext.Current.Server.MapPath(string.Format("{0}/{1}", photoFolder, ImageFileName));
                        if (File.Exists(imagePath))
                        {
                            using (Image myImg = Image.FromFile(imagePath))
                            {
                                picEmpPhoto.Image = myImg;
                            }
                        }
                    }
                    #endregion

                    #region Format font size of the display fields
                    Telerik.Reporting.Processing.Panel panelFront = (Telerik.Reporting.Processing.Panel)Telerik.Reporting.Processing.ElementTreeHelper.GetChildByName(detailSection, "panelFront");
                    if (panelFront != null)
                    {
                        Telerik.Reporting.Processing.Panel panRepHeader = (Telerik.Reporting.Processing.Panel)Telerik.Reporting.Processing.ElementTreeHelper.GetChildByName(panelFront, "panRepHeader");
                        if (panRepHeader != null)
                        {
                            int textWidth = 0;
                            int textHeight = 0;
                            int textWidthPerRow = 0;
                            int controlWidthPixels = 0;
                            decimal widthVariance = 0;

                            #region Resize Employee Name field 
                            Telerik.Reporting.Processing.TextBox txtEmpName = (Telerik.Reporting.Processing.TextBox)Telerik.Reporting.Processing.ElementTreeHelper.GetChildByName(panRepHeader, "txtEmpName");
                            if (txtEmpName != null && !string.IsNullOrWhiteSpace(this.ReportData.EmpName))
                            {
                                // Get the width of the textbox control in pixels
                                controlWidthPixels = Convert.ToInt32((txtEmpName.Width.Value * 96) / 2.54);

                                if (this.ReportData.EmpName.Trim().Length >= 20 && this.ReportData.EmpName.Trim().Length <= 25)
                                {
                                    txtEmpName.Style.Font.Size = new Unit(17, UnitType.Point);
                                    textWidth = TextRenderer.MeasureText(this.ReportData.EmpName, new System.Drawing.Font("Calibri", 17)).Width; //- CONST_TEXT_MARGIN;
                                    textHeight = TextRenderer.MeasureText(this.ReportData.EmpName, new System.Drawing.Font("Calibri", 17)).Height;
                                    textWidthPerRow = Math.Abs(textWidth / controlWidthPixels) * textHeight;
                                    widthVariance = Convert.ToDecimal(textWidth) / Convert.ToDecimal(controlWidthPixels);

                                    //if (textWidth >= controlWidthPixels)
                                    if (widthVariance > 2)
                                        txtEmpName.Style.Font.Size = new Unit(16, UnitType.Point);
                                }
                                else if (this.ReportData.EmpName.Trim().Length >= 26 && this.ReportData.EmpName.Trim().Length <= 29)
                                {
                                    txtEmpName.Style.Font.Size = new Unit(16, UnitType.Point);
                                    textWidth = TextRenderer.MeasureText(this.ReportData.EmpName, new System.Drawing.Font("Calibri", 16)).Width; // - CONST_TEXT_MARGIN;
                                    textHeight = TextRenderer.MeasureText(this.ReportData.EmpName, new System.Drawing.Font("Calibri", 16)).Height;
                                    textWidthPerRow = Math.Abs(textWidth / controlWidthPixels) * textHeight;
                                    widthVariance = Convert.ToDecimal(textWidth) / Convert.ToDecimal(controlWidthPixels);

                                    //if (textWidth >= controlWidthPixels)
                                    if (widthVariance > 2)
                                        txtEmpName.Style.Font.Size = new Unit(15, UnitType.Point);
                                }
                                else if (this.ReportData.EmpName.Trim().Length >= 30 && this.ReportData.EmpName.Trim().Length <= 33)
                                {
                                    txtEmpName.Style.Font.Size = new Unit(15, UnitType.Point);
                                    textWidth = TextRenderer.MeasureText(this.ReportData.EmpName, new System.Drawing.Font("Calibri", 15)).Width; // - CONST_TEXT_MARGIN;
                                    textHeight = TextRenderer.MeasureText(this.ReportData.EmpName, new System.Drawing.Font("Calibri", 15)).Height;
                                    textWidthPerRow = Math.Abs(textWidth / controlWidthPixels) * textHeight;
                                    widthVariance = Convert.ToDecimal(textWidth) / Convert.ToDecimal(controlWidthPixels);

                                    //if (textWidth >= controlWidthPixels)
                                    if (widthVariance > 2)
                                        txtEmpName.Style.Font.Size = new Unit(14, UnitType.Point);
                                }
                                else if (this.ReportData.EmpName.Trim().Length >= 34 && this.ReportData.EmpName.Trim().Length <= 37)
                                {
                                    txtEmpName.Style.Font.Size = new Unit(14, UnitType.Point);
                                    textWidth = TextRenderer.MeasureText(this.ReportData.EmpName, new System.Drawing.Font("Calibri", 14)).Width; // - CONST_TEXT_MARGIN;
                                    textHeight = TextRenderer.MeasureText(this.ReportData.EmpName, new System.Drawing.Font("Calibri", 14)).Height;
                                    textWidthPerRow = Math.Abs(textWidth / controlWidthPixels) * textHeight;
                                    widthVariance = Convert.ToDecimal(textWidth) / Convert.ToDecimal(controlWidthPixels);

                                    //if (textWidth >= controlWidthPixels)
                                    if (widthVariance > 2)
                                        txtEmpName.Style.Font.Size = new Unit(13, UnitType.Point);
                                }
                                else if (this.ReportData.EmpName.Trim().Length >= 38)
                                {
                                    txtEmpName.Style.Font.Size = new Unit(13, UnitType.Point);
                                    textWidth = TextRenderer.MeasureText(this.ReportData.EmpName, new System.Drawing.Font("Calibri", 13)).Width; // - CONST_TEXT_MARGIN;
                                    textHeight = TextRenderer.MeasureText(this.ReportData.EmpName, new System.Drawing.Font("Calibri", 13)).Height;
                                    textWidthPerRow = Math.Abs(textWidth / controlWidthPixels) * textHeight;
                                    widthVariance = Convert.ToDecimal(textWidth) / Convert.ToDecimal(controlWidthPixels);

                                    if (widthVariance > 2)
                                        txtEmpName.Style.Font.Size = new Unit(12, UnitType.Point);
                                }
                                else
                                {
                                    txtEmpName.Style.Font.Size = new Unit(18, UnitType.Point);     // Default fontsize
                                    textWidth = TextRenderer.MeasureText(this.ReportData.EmpName, new System.Drawing.Font("Calibri", 18)).Width; // - CONST_TEXT_MARGIN;
                                    textHeight = TextRenderer.MeasureText(this.ReportData.EmpName, new System.Drawing.Font("Calibri", 18)).Height;
                                    textWidthPerRow = Math.Abs(textWidth / controlWidthPixels) * textHeight;
                                    widthVariance = Convert.ToDecimal(textWidth) / Convert.ToDecimal(controlWidthPixels);

                                    //if (textWidth >= controlWidthPixels)
                                    if (widthVariance > 2)
                                        txtEmpName.Style.Font.Size = new Unit(17, UnitType.Point);
                                }
                            }
                            #endregion

                            #region Resize the Position field
                            Telerik.Reporting.Processing.TextBox txtPosition = (Telerik.Reporting.Processing.TextBox)Telerik.Reporting.Processing.ElementTreeHelper.GetChildByName(panRepHeader, "txtPosition");
                            if (txtPosition != null && !string.IsNullOrWhiteSpace(this.ReportData.Position))
                            {
                                // Get the width of the textbox control in pixels
                                controlWidthPixels = Convert.ToInt32((txtPosition.Width.Value * 96) / 2.54);

                                if (this.ReportData.Position.Trim().Length >= 24 && this.ReportData.Position.Trim().Length <= 25)
                                {
                                    txtPosition.Style.Font.Size = new Unit(11, UnitType.Point);
                                    textWidth = TextRenderer.MeasureText(this.ReportData.Position, new System.Drawing.Font("Calibri", 11)).Width - CONST_TEXT_MARGIN;
                                    if (textWidth >= controlWidthPixels)
                                        txtPosition.Style.Font.Size = new Unit(10, UnitType.Point);
                                }
                                else if (this.ReportData.Position.Trim().Length >= 26 && this.ReportData.Position.Trim().Length <= 28)
                                {
                                    txtPosition.Style.Font.Size = new Unit(10, UnitType.Point);
                                    textWidth = TextRenderer.MeasureText(this.ReportData.Position, new System.Drawing.Font("Calibri", 10)).Width - CONST_TEXT_MARGIN;
                                    if (textWidth >= controlWidthPixels)
                                        txtPosition.Style.Font.Size = new Unit(9, UnitType.Point);
                                }
                                else if (this.ReportData.Position.Trim().Length >= 29 && this.ReportData.Position.Trim().Length <= 31)
                                {
                                    txtPosition.Style.Font.Size = new Unit(9, UnitType.Point);
                                    textWidth = TextRenderer.MeasureText(this.ReportData.Position, new System.Drawing.Font("Calibri", 9)).Width - CONST_TEXT_MARGIN;
                                    if (textWidth >= controlWidthPixels)
                                        txtPosition.Style.Font.Size = new Unit(8, UnitType.Point);
                                }
                                else if (this.ReportData.Position.Trim().Length >= 32 && this.ReportData.Position.Trim().Length <= 35)
                                {
                                    txtPosition.Style.Font.Size = new Unit(8, UnitType.Point);
                                    textWidth = TextRenderer.MeasureText(this.ReportData.Position, new System.Drawing.Font("Calibri", 8)).Width - CONST_TEXT_MARGIN;
                                    if (textWidth >= controlWidthPixels)
                                        txtPosition.Style.Font.Size = new Unit(7, UnitType.Point);
                                }
                                else if (this.ReportData.Position.Trim().Length >= 36 && this.ReportData.Position.Trim().Length <= 38)
                                {
                                    txtPosition.Style.Font.Size = new Unit(7, UnitType.Point);
                                    textWidth = TextRenderer.MeasureText(this.ReportData.Position, new System.Drawing.Font("Calibri", 7)).Width - CONST_TEXT_MARGIN;
                                    if (textWidth >= controlWidthPixels)
                                        txtPosition.Style.Font.Size = new Unit(6, UnitType.Point);
                                }
                                else if (this.ReportData.Position.Trim().Length >= 39)
                                {
                                    txtPosition.Style.Font.Size = new Unit(6, UnitType.Point);
                                }
                                else
                                {
                                    txtPosition.Style.Font.Size = new Unit(12, UnitType.Point);     // Default fontsize
                                    textWidth = TextRenderer.MeasureText(this.ReportData.Position, new System.Drawing.Font("Calibri", 12)).Width - CONST_TEXT_MARGIN;
                                    if (textWidth >= controlWidthPixels)
                                        txtPosition.Style.Font.Size = new Unit(11, UnitType.Point);
                                }
                            }
                            #endregion

                            #region Resize the Cost Center field
                            Telerik.Reporting.Processing.TextBox txtCostCenter = (Telerik.Reporting.Processing.TextBox)Telerik.Reporting.Processing.ElementTreeHelper.GetChildByName(panRepHeader, "txtCostCenter");
                            if (txtCostCenter != null && !string.IsNullOrWhiteSpace(this.ReportData.CustomCostCenter))
                            {
                                // Get the width of the textbox control in pixels
                                controlWidthPixels = Convert.ToInt32((txtCostCenter.Width.Value * 96) / 2.54);

                                if (this.ReportData.CustomCostCenter.Trim().Length >= 27 && this.ReportData.CustomCostCenter.Trim().Length <= 31)
                                {
                                    txtCostCenter.Style.Font.Size = new Unit(9, UnitType.Point);
                                    textWidth = TextRenderer.MeasureText(this.ReportData.CustomCostCenter, new System.Drawing.Font("Calibri", 9)).Width - CONST_TEXT_MARGIN;
                                    if (textWidth >= controlWidthPixels)
                                        txtCostCenter.Style.Font.Size = new Unit(8, UnitType.Point);
                                }
                                else if (this.ReportData.CustomCostCenter.Trim().Length >= 32 && this.ReportData.CustomCostCenter.Trim().Length <= 35)
                                {
                                    txtCostCenter.Style.Font.Size = new Unit(8, UnitType.Point);
                                    textWidth = TextRenderer.MeasureText(this.ReportData.CustomCostCenter, new System.Drawing.Font("Calibri", 8)).Width - CONST_TEXT_MARGIN;
                                    if (textWidth >= controlWidthPixels)
                                        txtCostCenter.Style.Font.Size = new Unit(7, UnitType.Point);
                                }
                                else if (this.ReportData.CustomCostCenter.Trim().Length >= 36 && this.ReportData.CustomCostCenter.Trim().Length <= 40)
                                {
                                    txtCostCenter.Style.Font.Size = new Unit(7, UnitType.Point);
                                    textWidth = TextRenderer.MeasureText(this.ReportData.CustomCostCenter, new System.Drawing.Font("Calibri", 7)).Width - CONST_TEXT_MARGIN;
                                    if (textWidth >= controlWidthPixels)
                                        txtCostCenter.Style.Font.Size = new Unit(6, UnitType.Point);
                                }
                                else if (this.ReportData.CustomCostCenter.Trim().Length >= 41)
                                {
                                    txtCostCenter.Style.Font.Size = new Unit(6, UnitType.Point);
                                    textWidth = TextRenderer.MeasureText(this.ReportData.CustomCostCenter, new System.Drawing.Font("Calibri", 6)).Width - CONST_TEXT_MARGIN;
                                    if (textWidth >= controlWidthPixels)
                                        txtCostCenter.Style.Font.Size = new Unit(5, UnitType.Point);
                                }
                                else
                                {
                                    txtCostCenter.Style.Font.Size = new Unit(10, UnitType.Point);     // Default fontsize
                                    textWidth = TextRenderer.MeasureText(this.ReportData.CustomCostCenter, new System.Drawing.Font("Calibri", 10)).Width - CONST_TEXT_MARGIN;
                                    if (textWidth >= controlWidthPixels)
                                        txtCostCenter.Style.Font.Size = new Unit(9, UnitType.Point);
                                }
                            }
                            #endregion                            
                        }
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}