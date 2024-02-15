using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GARMCO.AMS.TAS.UI.Models
{
    public class POEntity
    {
        #region Properties
        public double PONumber { get; set; }
        public DateTime? PROrderDate { get; set; }
        public int? PRReqTypeID { get; set; }
        public string PRReqTypeName { get; set; }
        public string PROrderType { get; set; }
        public string PRStockType { get; set; }
        public string PRItemType { get; set; }
        public string PRItemDesc { get; set; }
        public double? SupplierNo { get; set; }
        public string SupplierName { get; set; }
        public int? OriginatorNo { get; set; }
        public string OriginatorName { get; set; }
        public string PRCostCenter { get; set; }
        public string PRChargeCostCenter { get; set; }
        public int? PRBuyerEmpNo { get; set; }
        public string PRBuyerEmpName { get; set; }
        public bool? PRIsBuyerAssigned { get; set; }
        public double? PROriginalPRNo { get; set; }
        public string PRReqStatusCode { get; set; }
        public string StatusDesc { get; set; }
        public string StatusHandlingCode { get; set; }

        public string PurchaseOrderDetails
        {
            get
            {
                return string.Format("PO#: {0} (Order Date: {1}, Item Type: {2})",
                    PONumber,
                    PROrderDate.HasValue ? PROrderDate.Value.ToString("dd-MMM-yyyy") : null, 
                    PRItemDesc);
            }
        }
        #endregion
    }
}