//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace GARMCO.AMS.TAS.DAL.DataModels
{
    using System;
    
    public partial class Pr_GetAttendanceHistory_Result1
    {
        public int AutoID { get; set; }
        public int EmpNo { get; set; }
        public string BusinessUnit { get; set; }
        public Nullable<bool> IsLastRow { get; set; }
        public Nullable<bool> Processed { get; set; }
        public string CorrectionCode { get; set; }
        public Nullable<System.DateTime> DT { get; set; }
        public Nullable<System.DateTime> dtIN { get; set; }
        public Nullable<System.DateTime> dtOUT { get; set; }
        public Nullable<System.DateTime> Shaved_IN { get; set; }
        public Nullable<System.DateTime> Shaved_OUT { get; set; }
        public string ShiftPatCode { get; set; }
        public string ShiftCode { get; set; }
        public string Actual_ShiftCode { get; set; }
        public string OTtype { get; set; }
        public Nullable<System.DateTime> OTstartTime { get; set; }
        public Nullable<System.DateTime> OTendTime { get; set; }
        public Nullable<int> NoPayHours { get; set; }
        public string AbsenceReasonCode { get; set; }
        public string LeaveType { get; set; }
        public string DIL_Entitlement { get; set; }
        public string RemarkCode { get; set; }
        public string LastUpdateUser { get; set; }
        public Nullable<System.DateTime> LastUpdateTime { get; set; }
    }
}