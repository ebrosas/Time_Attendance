using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GARMCO.AMS.TAS.BL.Entities
{
    [Serializable]
    public class MasterShiftPatternEntity
    {
        #region Properties
        public int AutoID { get; set; }
        public string ShiftPatCode { get; set; }
        public string ShiftPatDescription { get; set; }
        public string ShiftPatternFullName { get; set; }
        public bool IsDayShift { get; set; }
        public bool IsFlexitime { get; set; }
        public string ShiftCode { get; set; }
        public string ShiftDescription { get; set; }
        public string ShiftFullDescription { get; set; }
        public int ShiftPointer { get; set; }
        public DateTime? ArrivalFrom { get; set; }
        public DateTime? ArrivalTo { get; set; }
        public DateTime? DepartFrom { get; set; }
        public DateTime? DepartTo { get; set; }
        public int DurationNormalDay { get; set; }
        public string DurationNormalDayString { get; set; }
        public DateTime? RArrivalFrom { get; set; }
        public DateTime? RArrivalTo { get; set; }
        public DateTime? RDepartFrom { get; set; }
        public DateTime? RDepartTo { get; set; }
        public int DurationRamadanDay { get; set; }
        public string DurationRamadanDayString { get; set; }
        public int CreatedByEmpNo { get; set; }
        public string CreatedByUserID { get; set; }
        public string CreatedByEmpName { get; set; }
        public string CreatedByFullName { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? LastUpdateTime { get; set; }
        public int LastUpdateEmpNo { get; set; }
        public string LastUpdateUserID { get; set; }
        public string LastUpdateEmpName { get; set; }
        public string LastUpdateFullName { get; set; }
        public bool IsDirty { get; set; }
        #endregion
    }
}
