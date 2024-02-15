using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GARMCO.AMS.TAS.BL.Entities
{
    [Serializable]
    public class ContractorEntity
    {
        #region Properties
        public int AutoID { get; set; }
        public int ContractorNo { get; set; }
        public string ContractorName { get; set; }
        public string GroupCode { get; set; }
        public string GroupDesc { get; set; }
        public double? SupplierNo { get; set; }
        public string SupplierName { get; set; }
        public DateTime? DateJoined { get; set; }
        public DateTime? DateResigned { get; set; }
        public string ShiftPatCode { get; set; }
        public int? ShiftPointer { get; set; }
        public string ReligionCode { get; set; }
        public string ReligionDesc { get; set; }
        public string LastUpdateUser { get; set; }
        public DateTime? LastUpdateTime { get; set; }
        public int TotalRecords { get; set; }
        #endregion
    }
}
