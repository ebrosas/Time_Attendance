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
    
    public partial class Master_EmployeeAdditional
    {
        public int AutoID { get; set; }
        public int EmpNo { get; set; }
        public string ShiftPatCode { get; set; }
        public Nullable<decimal> ShiftPointer { get; set; }
        public string WorkingBusinessUnit { get; set; }
        public string SpecialJobCatg { get; set; }
        public string LastUpdateUser { get; set; }
        public Nullable<System.DateTime> LastUpdateTime { get; set; }
        public Nullable<System.DateTime> CatgEffectiveDate { get; set; }
        public Nullable<System.DateTime> CatgEndingDate { get; set; }
    }
}