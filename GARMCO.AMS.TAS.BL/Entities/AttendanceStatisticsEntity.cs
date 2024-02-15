using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GARMCO.AMS.TAS.BL.Entities
{
    [Serializable]
    public class AttendanceStatisticsEntity
    {
        #region Properties
        public string CostCenter { get; set; }
        public string AttendanceDate { get; set; }
        public double Total_0700_Below { get; set; }
        public double Total_0700_0710 { get; set; }
        public double Total_0710_0720 { get; set; }
        public double Total_0720_0730 { get; set; }
        public double Total_0730_0740 { get; set; }
        public double Total_0740_0750 { get; set; }
        public double Total_0750_0800 { get; set; }
        public double Total_0800_Above { get; set; }
        public double Total_Absent { get; set; }
        public double TotalNumber { get; set; }
        #endregion
    }
}
