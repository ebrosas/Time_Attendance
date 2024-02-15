using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GARMCO.AMS.TAS.BL.Entities
{
    [Serializable]
    public class EmployeeDetail
    {
        #region Properties
        public int EmpNo { get; set; }
        public string EmpName { get; set; }
        public string EmpFullName { get; set; }
        public string EmpUserID { get; set; }
        public string EmpEmail { get; set; }
        public string CostCenter { get; set; }
        public string CostCenterName { get; set; }
        public string CostCenterFullName { get; set; }
        public string ActualCostCenter { get; set; }
        public int? SupervisorEmpNo { get; set; }
        public string SupervisorEmpName { get; set; }
        public string SupervisorFullName { get; set; }
        public int SuperintendentEmpNo { get; set; }
        public string SuperintendentEmpName { get; set; }
        public string SuperintendentFullName { get; set; }
        public int? ManagerEmpNo { get; set; }
        public string ManagerEmpName { get; set; }
        public string ManagerFullName { get; set; }
        public string Position { get; set; }
        public string PhoneExtension { get; set; }
        public string WorkingCostCenter { get; set; }
        public string WorkingCostCenterName { get; set; }
        public string WorkingCostCenterFullName { get; set; }
        public int? PayGrade { get; set; }
        public string PositionID { get; set; }
        public string Gender { get; set; }
        public string Destination { get; set; }
        public string EmployeeClass { get; set; }
        public string TicketClass { get; set; }
        public string EmployeeStatus { get; set; }
        public string Username { get; set; }
        public DateTime? DateJoined { get; set; }
        public double YearsOfService { get; set; }
        public string ShiftPatCode { get; set; }
        public string ShiftCode { get; set; }
        public int ShiftPointer { get; set; }
        public string SpecialJobCatg { get; set; }
        public string SpecialJobCatgDesc { get; set; }
        public int AutoID { get; set; }
        public string LastUpdateUser { get; set; }
        public DateTime? LastUpdateTime { get; set; }
        public DateTime? CatgEffectiveDate { get; set; }
        public DateTime? CatgEndingDate { get; set; }
        #endregion

        #region Contractor Properties 
        public string ContractorEmpName { get; set; }
        public double? ContractorNumber { get; set; }
        public string GroupCode { get; set; }
        public DateTime? DateResigned { get; set; }
        public string ReligionCode { get; set; }
        #endregion
    }
}
