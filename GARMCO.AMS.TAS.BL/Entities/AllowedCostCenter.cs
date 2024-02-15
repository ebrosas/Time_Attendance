using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GARMCO.AMS.TAS.BL.Entities
{
    [Serializable]
    public class AllowedCostCenter
    {
        #region Properties
        public int PermitID { get; set; }
        public int PermitEmpNo { get; set; }
        public int PermitAppID { get; set; }
        public string PermitCostCenter { get; set; }
        #endregion
    }
}
