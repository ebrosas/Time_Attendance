using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GARMCO.AMS.TAS.BL.Entities
{
    [Serializable]
    public class DeathReasonOfAbsenceEntity
    {
        #region Properties
        public int ReasonAbsenceID { get; set; }
        public int EmpNo { get; set; }
        public DateTime? DT { get; set; }
        public string CostCenter { get; set; }
        public string CorrectionCode { get; set; }
        public string ShiftPatCode { get; set; }
        public string ShiftCode { get; set; }
        public string RelativeTypeCode { get; set; }
        public string OtherRelativeType { get; set; }
        public string Remarks { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? CreatedByEmpNo { get; set; }
        public string CreatedByEmpName { get; set; }
        public string CreatedByUserID { get; set; }
        public DateTime? LastUpdateDate { get; set; }
        public int? LastUpdateEmpNo { get; set; }
        public string LastUpdateEmpName { get; set; }
        public string LastUpdateUserID { get; set; }
        #endregion
    }
}
