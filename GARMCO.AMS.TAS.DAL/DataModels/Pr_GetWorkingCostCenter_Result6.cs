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
    
    public partial class Pr_GetWorkingCostCenter_Result6
    {
        public int AutoID { get; set; }
        public int EmpNo { get; set; }
        public string EmpName { get; set; }
        public string Position { get; set; }
        public string CostCenter { get; set; }
        public string CostCenterName { get; set; }
        public string ShiftPatCode { get; set; }
        public Nullable<decimal> ShiftPointer { get; set; }
        public string WorkingBusinessUnit { get; set; }
        public string WorkingBusinessUnitName { get; set; }
        public string SpecialJobCatg { get; set; }
        public string SpecialJobCatgDesc { get; set; }
        public string LastUpdateUser { get; set; }
        public Nullable<System.DateTime> LastUpdateTime { get; set; }
        public Nullable<System.DateTime> CatgEffectiveDate { get; set; }
        public Nullable<System.DateTime> CatgEndingDate { get; set; }
    }
}