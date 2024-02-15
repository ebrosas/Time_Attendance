using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GARMCO.AMS.TAS.BL.Entities
{
    [Serializable]
    public class SystemValueEntity
    {
        #region Properties
        public DateTime? ShiftPatternLastUpdated { get; set; }
        public DateTime? ManualTimeSheetLastEntered { get; set; }
        public DateTime? SwipeLastProcessDate { get; set; }
        public DateTime? SwipeNewProcessDate { get; set; }
        #endregion
    }
}
