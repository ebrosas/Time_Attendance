using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GARMCO.AMS.TAS.BL.Entities
{
    [Serializable]
    public class TrainingRecordEntity
    {
        #region Properties
        public long TrainingRecordID { get; set; }
        public int EmpNo { get; set; }
        public string EmpName { get; set; }
        public string EmpPosition { get; set; }
        public string EmpFullName { get; set; }
        public int SupervisorNo { get; set; }
        public string SupervisorName { get; set; }
        public string SupervisorFullName { get; set; }
        public int ManagerNo { get; set; }
        public string ManagerName { get; set; }
        public string ManagerFullName { get; set; }
        public string CostCenter { get; set; }
        public string CostCenterName { get; set; }
        public string CostCenterFullName { get; set; }
        public int TrainingProgramID { get; set; }
        public string CourseTitle { get; set; }
        public string CourseDescription { get; set; }
        public string CourseCode { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? Duration { get; set; }
        public string DurationCode { get; set; }
        public string DurationDesc { get; set; }
        public string DurationDetails { get; set; }
        public int TrainingProviderID { get; set; }
        public string TrainingProviderName { get; set; }
        public string TrainingProviderCode { get; set; }
        public string TrainingProviderDesc { get; set; }
        public string QualificationCode { get; set; }
        public string QualificationDesc { get; set; }
        public string TypeOfTrainingCode { get; set; }
        public string TypeOfTrainingDesc { get; set; }
        public string StatusCode { get; set; }
        public string StatusDesc { get; set; }
        public double? Cost { get; set; }
        public string Remarks { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int CreatedByEmpNo { get; set; }
        public string CreatedByUserID { get; set; }
        public string CreatedByEmpName { get; set; }
        public string CreatedByFullName { get; set; }
        public string CreatedByEmpEmail { get; set; }
        public DateTime? LastUpdateTime { get; set; }
        public int LastUpdateEmpNo { get; set; }
        public string LastUpdateUserID { get; set; }
        public string LastUpdateEmpName { get; set; }
        public string LastUpdateFullName { get; set; }
        public string LastUpdateEmpEmail { get; set; }
        #endregion

        #region Extended Properties
        public int CourseSessionID { get; set; }
        public string CourseSessionDetail { get; set; }
        public string DatesAttendedArray { get; set; }
        public int? DurationAttended { get; set; }
        public string DurationAttendedCode { get; set; }
        public string DurationAttendedDesc { get; set; }
        public string DurationAttendedDetails { get; set; }
        public string Location { get; set; }
        public int TotalRecords { get; set; }
        public byte AttendeeeType { get; set; }
        public int ContractorID { get; set; }
        public string ContractorName { get; set; }
        public string ContractorOccupation { get; set; }
        public string ContractorOtherID { get; set; }
        public int TraineeID { get; set; }
        public string TraineeName { get; set; }
        public string TraineeFullName { get; set; }
        public string TraineePosition { get; set; }
        #endregion
    }
}
