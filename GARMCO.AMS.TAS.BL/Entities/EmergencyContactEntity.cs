﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GARMCO.AMS.TAS.BL.Entities
{
    [Serializable]
    public class EmergencyContactEntity
    {
        #region Properties
        public int EmpNo { get; set; }
        public string EmpName { get; set; }
        public int RelatedPersonID { get; set; }
        public string RelatedPersonName { get; set; }
        public int RelationTypeID { get; set; }
        public string RelationTypeDesc { get; set; }
        public int LineNumberID { get; set; }
        public string PhoneNumberType { get; set; }
        public string PhoneNumberDesc { get; set; }
        public string PhonePrefix { get; set; }
        public string PhoneNumber { get; set; }
        #endregion

        #region Extended Properties
        public string CostCenter { get; set; }
        public string Position { get; set; }
        public string CostCenterName { get; set; }
        public string Religion { get; set; }
        public string Sex { get; set; }
        public int? PayGrade { get; set; }
        public DateTime? DateJoined { get; set; }
        public double? YearsOfService { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public double? Age { get; set; }
        public string TelephoneExt { get; set; }
        public string MobileNo { get; set; }
        public string TelNo { get; set; }
        public string FaxNo { get; set; }
        public string Email { get; set; }
        public string JobCategory { get; set; }
        public string EmployeeImagePath { get; set; }
        public string EmployeeImageTooltip { get; set; }
        public int? SupervisorNo { get; set; }
        public string SupervisorName { get; set; }
        public string SupervisorFullName { get; set; }
        public bool IsShowPhoto { get; set; }
        #endregion
    }
}