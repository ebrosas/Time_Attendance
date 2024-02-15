using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GARMCO.AMS.TAS.BL.Entities
{
    [Serializable]
    public class RoutineHistoryEntity
    {
        #region Properties
        public int AutoID { get; set; }
        public long OTRequestNo { get; set; }
        public int TS_AutoID { get; set; }
        public DateTime? RequestSubmissionDate { get; set; }
        public string HistDescription { get; set; }
        public int HistCreatedBy { get; set; }
        public string HistCreatedName { get; set; }
        public string HistCreatedFullName { get; set; }
        public DateTime? HistCreatedDate { get; set; }
        #endregion
    }
}
