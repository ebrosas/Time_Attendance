using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GARMCO.AMS.TAS.BL.Entities
{
    [Serializable]
    public class ShiftPatternEntity
    {
        #region Properties
        public int AutoID { get; set; }
        public int XID_AutoID { get; set; }
        public int EmpNo { get; set; }
        public string EmpName { get; set; }
        public string EmpFullName { get; set; }
        public string Position { get; set; }
        public string ShiftPatCode { get; set; }
        public string ShiftPatDesc { get; set; }
        public string ShiftCode { get; set; }
        public string ShiftCodeArray { get; set; }
        public int ShiftPointer { get; set; }
        public string ShiftPointerCode { get; set; }
        public string WorkingCostCenter { get; set; }
        public string WorkingCostCenterName { get; set; }
        public string WorkingCostCenterFullName { get; set; }
        public string LastUpdateUser { get; set; }
        public DateTime? LastUpdateTime { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? EndingDate { get; set; }
        public string ChangeType { get; set; }
        public string ChangeTypeDesc { get; set; }
        public int TotalRecords { get; set; }
        public string CostCenter { get; set; }
        public string CostCenterName { get; set; }
        public string CostCenterFullName { get; set; }
        public int SupervisorNo { get; set; }
        public string SupervisorName { get; set; }
        public string SupervisorFullName { get; set; }
        #endregion

        #region Extended Properties
        public string ActionMachineName { get; set; }
        public string ActionType { get; set; }
        public string ActionTypeDesc { get; set; }
        public DateTime? ActionDateTime { get; set; }
        public byte RestrictionType { get; set; }
        public string RestrictedEmpNoArray { get; set; }
        public string RestrictedCostCenterArray { get; set; }
        public string RestrictionMessage { get; set; }
        #endregion

        #region Extended Properties used in "View Current Shift Pattern (Employee)" form
        public string SpecialJobCatalog { get; set; }
        public string ParentCostCenter { get; set; }
        #endregion
    }
}
