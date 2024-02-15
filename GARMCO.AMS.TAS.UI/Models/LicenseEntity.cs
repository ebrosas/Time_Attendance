using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GARMCO.AMS.TAS.UI.Models
{
    public class LicenseEntity
    {
        public int registryID { get; set; }
        public int empNo { get; set; }
        public string licenseNo { get; set; }
        public string licenseTypeCode { get; set; }
        public string licenseTypeDesc { get; set; }
        public string issuingAuthority { get; set; }
        public DateTime issuedDate { get; set; }
        public DateTime expiryDate { get; set; }
        public string remarks { get; set; }
        public string licenseGUID { get; set; }
        public DateTime? createdDate { get; set; }
        public int createdByEmpNo { get; set; }
        public string createdByEmpName { get; set; }
        public string createdByUser { get; set; }
        public DateTime? lastUpdatedDate { get; set; }
        public int? lastUpdatedByEmpNo { get; set; }
        public string lastUpdatedByEmpName { get; set; }
        public string lastUpdatedByUser { get; set; }
    }
}