using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GARMCO.AMS.TAS.UI.Models
{
    public class GenericEntity
    {
        #region Properties
        public int SupplierCode { get; set; }
        public string SupplierName { get; set; }
        public int? contractorNo { get; set; }
        public string idNumber { get; set; }
        public string contractorName { get; set; }
        public string companyName { get; set; }
        public string costCenter { get; set; }
        public string jobTitle { get; set; }
        public string supervisorName { get; set; }
        public string contractStartDateStr { get; set; }
        public string contractEndDateStr { get; set; }
        #endregion
    }
}