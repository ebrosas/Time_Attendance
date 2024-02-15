using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GARMCO.AMS.TAS.BL.Entities
{
    [Serializable]
    public class ReasonOfAbsenceEntity
    {
        #region Properties
        public int AutoID { get; set; }
        public int EmpNo { get; set; }
        public string EmpName { get; set; }
        public string Position { get; set; }
        public string CostCenter { get; set; }
        public string CostCenterName { get; set; }
        public string CostCenterFullName { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? EndingDate { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string DayOfWeek { get; set; }
        public string AbsenceReasonCode { get; set; }
        public string AbsenceReasonDesc { get; set; }
        public string AbsenceReasonFullName { get; set; }
        public int? XID_TS_DIL_ENT { get; set; }
        public int? XID_TS_DIL_USD { get; set; }
        public string LastUpdateUser { get; set; }
        public DateTime? LastUpdateTime { get; set; }
        public string DIL_ENT_CODE { get; set; }
        public int TotalRecords { get; set; }
        #endregion
    }
}
