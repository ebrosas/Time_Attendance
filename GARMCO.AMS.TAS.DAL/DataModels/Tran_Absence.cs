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
    using System.Collections.Generic;
    
    public partial class Tran_Absence
    {
        public int AutoID { get; set; }
        public int EmpNo { get; set; }
        public System.DateTime EffectiveDate { get; set; }
        public System.DateTime EndingDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string DayOfWeek { get; set; }
        public string AbsenceReasonCode { get; set; }
        public Nullable<int> XID_TS_DIL_ENT { get; set; }
        public Nullable<int> XID_TS_DIL_USD { get; set; }
        public string LastUpdateUser { get; set; }
        public Nullable<System.DateTime> LastUpdateTime { get; set; }
        public string DIL_ENT_CODE { get; set; }
    }
}