using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GARMCO.AMS.TAS.BL.Entities
{
    [Serializable]
    public class DutyROTAEntity
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
        public string DutyType { get; set; }
        public string DutyDescription { get; set; }
        public double? DutyAllowance { get; set; }
        public string LastUpdateUser { get; set; }
        public DateTime? LastUpdateTime { get; set; }
        public int TotalRecords { get; set; }
        #endregion
    }
}
