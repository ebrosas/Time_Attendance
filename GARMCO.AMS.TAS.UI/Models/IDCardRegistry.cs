//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace GARMCO.AMS.TAS.UI.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class IDCardRegistry
    {
        public int RegistryID { get; set; }
        public int EmpNo { get; set; }
        public bool IsContractor { get; set; }
        public byte[] EmpPhoto { get; set; }
        public string ImageFileName { get; set; }
        public string ImageFileExt { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public int CreatedByEmpNo { get; set; }
        public string CreatedByUser { get; set; }
        public Nullable<System.DateTime> LastUpdatedDate { get; set; }
        public Nullable<int> LastUpdatedByEmpNo { get; set; }
        public string LastUpdatedByUser { get; set; }
        public string Base64Photo { get; set; }
        public string EmpName { get; set; }
        public string Position { get; set; }
        public string CustomCostCenter { get; set; }
        public string CPRNo { get; set; }
        public string BloodGroup { get; set; }
    }
}