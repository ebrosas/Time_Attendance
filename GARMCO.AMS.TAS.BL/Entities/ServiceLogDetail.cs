using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GARMCO.AMS.TAS.BL.Entities
{
    [Serializable]
    public class ServiceLogDetail
    {
        #region Properties
        public int MessageID { get; set; }
        public int ProcessID { get; set; }
        public DateTime? ProcessDate { get; set; }
        public string Message { get; set; }
        public int AutoID { get; set; }
        public DateTime? LogDate { get; set; }
        public DateTime? SPUDate { get; set; }
        public int TransactionID { get; set; }
        public string LogDescription { get; set; }
        public int LogErrorNo { get; set; }
        public string LogErrorDesc { get; set; }
        public string ShiftPatCode { get; set; }
        public string ShiftCode { get; set; }
        public int ShiftPointer { get; set; }
        public int EmpCount { get; set; }
        #endregion
    }
}
