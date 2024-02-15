using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GARMCO.AMS.TAS.BL.Entities
{
    [Serializable]
    public class DependentEntity
    {
        #region Properties
        public int EmpNo { get; set; }
        public double DependentNo { get; set; }
        public string DependentName { get; set; }
        public string Relationship { get; set; }
        public string RelationshipID { get; set; }
        public string Sex { get; set; }
        public DateTime? DOB { get; set; }
        public string CPRNo { get; set; }
        public DateTime? CPRExpDate { get; set; }
        public DateTime? ResPermitExpDate { get; set; }
        #endregion
    }
}
