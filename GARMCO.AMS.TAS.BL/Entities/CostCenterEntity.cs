using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GARMCO.AMS.TAS.BL.Entities
{
    [Serializable]
    public class CostCenterEntity
    {
        #region Properties
        public string CompanyCode { get; set; }
        public string CompanyName { get; set; }
        public string CostCenter { get; set; }
        public string CostCenterName { get; set; }
        public string CostCenterFullName { get; set; }
        public int? SuperintendentEmpNo { get; set; }
        public string SuperintendentEmpName { get; set; }
        public string SuperintendentFullName { get; set; }
        public int? ManagerEmpNo { get; set; }
        public string ManagerEmpName { get; set; }
        public string ManagerFullName { get; set; }
        public string ParentCostCenter { get; set; }
        #endregion
    }
}
