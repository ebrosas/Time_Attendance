using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GARMCO.AMS.TAS.BL.Entities
{
    [Serializable]
    public class DILEntity
    {
        #region Properties   
        public int AutoID { get; set; }
        public int EmpNo { get; set; }
        public DateTime? EntitlementDate { get; set; }
        public string DILCode { get; set; }
        public string Remarks { get; set; }
        #endregion
    }
}
