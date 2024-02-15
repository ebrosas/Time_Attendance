using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GARMCO.AMS.TAS.BL.Entities
{
    [Serializable]
    public class UserDefinedCodes
    {
        #region Properties
        public int UDCGID { get; set; }
        public int UDCID { get; set; }
        public string UDCCode { get; set; }
        public string UDCDesc1 { get; set; }
        public string UDCDesc2 { get; set; }
        public string UDCSpecialHandlingCode { get; set; }
        public DateTime? UDCDate { get; set; }
        public decimal? UDCAmount { get; set; }
        public string UDCField { get; set; }
        public int? UDCSequenceNo { get; set; }
        public bool? IsIncluded { get; set; }
        public bool? IsDefaultSelection { get; set; }
        public string UDCFullName { get; set; }
        #endregion
    }
}
