using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GARMCO.AMS.TAS.BL.Entities
{
    [Serializable]
    public class EmployeeAbsentEntity
    {
        #region Properties
        public int AutoID { get; set; }
        public int EmpNo { get; set; }
        public string EmpName { get; set; }
        public string BusinessUnit { get; set; }
        public string BusinessUnitName { get; set; }
        public DateTime? AbsentDate { get; set; }
        public string ShiftCode { get; set; }
        public string ShiftPatCode { get; set; }
        public int PayGrade { get; set; }
        public string Remarks { get; set; }
        public string Position { get; set; }
        public int SupervisorNo { get; set; }
        public string SupervisorName { get; set; }
        #endregion
    }
}
