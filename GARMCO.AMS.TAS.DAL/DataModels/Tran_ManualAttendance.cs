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
    
    public partial class Tran_ManualAttendance
    {
        public int AutoID { get; set; }
        public int EmpNo { get; set; }
        public Nullable<System.DateTime> dtIN { get; set; }
        public string timeIN { get; set; }
        public Nullable<System.DateTime> dtOUT { get; set; }
        public string timeOUT { get; set; }
        public Nullable<bool> xxxxxxxxxxxxxx { get; set; }
        public string LastUpdateUser { get; set; }
        public Nullable<System.DateTime> LastUpdateTime { get; set; }
        public string CreatedUser { get; set; }
        public Nullable<System.DateTime> CreatedTime { get; set; }
    }
}