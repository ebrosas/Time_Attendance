using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GARMCO.AMS.TAS.BL.Entities
{
    [Serializable]
    public class CostCenterAccessEntity
    {
        #region Properties
        public int PermitID { get; set; }
        public int EmpNo { get; set; }
        public string EmpName { get; set; }
        public string CostCenter { get; set; }
        public string CostCenterName { get; set; }
        public int? CreatedByEmpNo { get; set; }
        public string CreatedByEmpName { get; set; }
        public string CreatedByFullName { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedByEmpNo { get; set; }
        public string ModifiedByEmpName { get; set; }
        public string ModifiedByFullName { get; set; }
        public DateTime? ModifiedDate { get; set; }
        #endregion
    }
}
