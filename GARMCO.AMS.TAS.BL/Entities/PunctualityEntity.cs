using GARMCO.AMS.TAS.BL.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GARMCO.AMS.TAS.BL.Entities
{
    [Serializable]
    public class PunctualityEntity
    {
        #region Properties
        public int EmpNo { get; set; }
        public string EmpName { get; set; }
        public string CostCenter { get; set; }
        public string CostCenterName { get; set; }
        public string CostCenterFullName { get; set; }
        public DateTime? Day1 { get; set; }
        public DateTime? Day1TimeIn { get; set; }
        public DateTime? Day1TimeOut { get; set; }
        public string Day1Remarks { get; set; }
        public bool Day1TimeInLate { get; set; }
        public bool Day1TimeOutEarly { get; set; }
        public DateTime? Day2 { get; set; }
        public DateTime? Day2TimeIn { get; set; }
        public DateTime? Day2TimeOut { get; set; }
        public string Day2Remarks { get; set; }
        public bool Day2TimeInLate { get; set; }
        public bool Day2TimeOutEarly { get; set; }
        public DateTime? Day3 { get; set; }
        public DateTime? Day3TimeIn { get; set; }
        public DateTime? Day3TimeOut { get; set; }
        public string Day3Remarks { get; set; }
        public bool Day3TimeInLate { get; set; }
        public bool Day3TimeOutEarly { get; set; }
        public DateTime? Day4 { get; set; }
        public DateTime? Day4TimeIn { get; set; }
        public DateTime? Day4TimeOut { get; set; }
        public string Day4Remarks { get; set; }
        public bool Day4TimeInLate { get; set; }
        public bool Day4TimeOutEarly { get; set; }
        public DateTime? Day5 { get; set; }
        public DateTime? Day5TimeIn { get; set; }
        public DateTime? Day5TimeOut { get; set; }
        public string Day5Remarks { get; set; }
        public bool Day5TimeInLate { get; set; }
        public bool Day5TimeOutEarly { get; set; }
        public DateTime? Day6 { get; set; }
        public DateTime? Day6TimeIn { get; set; }
        public DateTime? Day6TimeOut { get; set; }
        public string Day6Remarks { get; set; }
        public bool Day6TimeInLate { get; set; }
        public bool Day6TimeOutEarly { get; set; }
        public DateTime? Day7 { get; set; }
        public DateTime? Day7TimeIn { get; set; }
        public DateTime? Day7TimeOut { get; set; }
        public string Day7Remarks { get; set; }
        public bool Day7TimeInLate { get; set; }
        public bool Day7TimeOutEarly { get; set; }
        public int TotalLateForWeek { get; set; }
        #endregion

        #region Extended Properties
        public string ShiftPatCode { get; set; }
        public string ShiftCode { get; set; }
        public string ActualShiftCode { get; set; }
        public DateTime? DT { get; set; }
        public DateTime? dtIN { get; set; }
        public DateTime? dtOUT { get; set; }
        public DateTime? MaxArrivalTime { get; set; }
        public DateTime? RequiredTimeOut { get; set; }
        public int ArrivalTimeDiff { get; set; }
        public int DepartureTimeDiff { get; set; }
        public string Remarks { get; set; }
        public string LeaveType { get; set; }
        public string UDCCode { get; set; }
        public string UDCDescription { get; set; }
        public int TotalLostTime { get; set; }
        public int ReportOccurenceCount { get; set; }
        public byte PunctualityTypeID { get; set; }
        public bool TimeInUnpunctual { get; set; }
        public bool TimeOutUnpunctual { get; set; }
        #endregion
    }
}
