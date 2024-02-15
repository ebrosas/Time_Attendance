using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GARMCO.AMS.TAS.BL.Entities
{
    [Serializable]
    public class TASJDEComparisonEntity
    {
        #region Properties
        public string PDBA { get; set; }
        public string PDBAName { get; set; }
        public int TASCount { get; set; }
        public int DiffTAS { get; set; }
        public int JDECount { get; set; }
        public int DiffJDE { get; set; }
        public int TotalDiff { get; set; }
        #endregion

        #region Extended Properties
        public int AutoID { get; set; }
        public int EmpNo { get; set; }
        public DateTime? DT { get; set; }
        public DateTime? OTFrom { get; set; }
        public DateTime? OTTo { get; set; }
        public string Txt { get; set; }
        public string JPDBA { get; set; }
        public int? JEmpNo { get; set; }
        public int? JAutoID { get; set; }
        public double? Jhours { get; set; }
        public string XXXXX { get; set; }
        #endregion
    }
}
