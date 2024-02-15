using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GARMCO.AMS.TAS.BL.Entities
{
    [Serializable]
    public class FireTeamMember
    {
        #region Properties
        public DateTime? SwipeDate { get; set; }
        public DateTime? SwipeTime { get; set; }
        public int EmpNo { get; set; }
        public string EmpName { get; set; }
        public string EmpFullName { get; set; }
        public string Position { get; set; }
        public string Extension { get; set; }
        public string MobileNo { get; set; }
        public int GradeCode { get; set; }
        public string PayStatus { get; set; }
        public string CostCenter { get; set; }
        public string CostCenterName { get; set; }
        public string CostCenterFullName { get; set; }
        public int? SupervisorEmpNo { get; set; }
        public string SupervisorEmpName { get; set; }
        public string SupervisorFullName { get; set; }
        public int SuperintendentEmpNo { get; set; }
        public string SuperintendentEmpName { get; set; }
        public string SuperintendentFullName { get; set; }
        public int? ManagerEmpNo { get; set; }
        public string ManagerEmpName { get; set; }
        public string ManagerFullName { get; set; }
        public string ShiftPatCode { get; set; }
        public string ShiftCode { get; set; }
        public int ShiftPointer { get; set; }
        public string ShiftTiming { get; set; }
        public string SwipeLocation { get; set; }
        public string SwipeType { get; set; }
        public string SwipeSummary { get; set; }
        public string Notes { get; set; }
        public string EmpImagePath { get; set; }
        public string PhotoTooltip { get; set; }
        public string EmpAttendanceFlag { get; set; }
        public string EmpAttendanceNotes { get; set; }
        public bool IsPresent { get; set; }
        public int TotalRecords { get; set; }
        public string GroupType { get; set; }
        #endregion
    }
}
