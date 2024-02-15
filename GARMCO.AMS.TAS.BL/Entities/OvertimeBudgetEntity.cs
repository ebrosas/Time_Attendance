using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GARMCO.AMS.TAS.BL.Entities
{
    [Serializable]
    public class OvertimeBudgetEntity
    {
        #region Properties
        public int FiscalYear { get; set; }
        public string FiscalYearDesc { get; set; }
        public decimal JanBudget { get; set; }
        public decimal FebBudget { get; set; }
        public decimal MarBudget { get; set; }
        public decimal AprBudget { get; set; }
        public decimal MayBudget { get; set; }
        public decimal JunBudget { get; set; }
        public decimal JulBudget { get; set; }
        public decimal AugBudget { get; set; }
        public decimal SepBudget { get; set; }
        public decimal OctBudget { get; set; }
        public decimal NovBudget { get; set; }
        public decimal DecBudget { get; set; }
        public decimal JanActual { get; set; }
        public decimal FebActual { get; set; }
        public decimal MarActual { get; set; }
        public decimal AprActual { get; set; }
        public decimal MayActual { get; set; }
        public decimal JunActual { get; set; }
        public decimal JulActual { get; set; }
        public decimal AugActual { get; set; }
        public decimal SepActual { get; set; }
        public decimal OctActual { get; set; }
        public decimal NovActual { get; set; }
        public decimal DecActual { get; set; }
        public decimal TotalBudgetAmount { get; set; }
        public decimal TotalActualAmount { get; set; }
        public decimal TotalBalanceAmount { get; set; }
        public string CostCenter { get; set; }
        public decimal TotalBudgetHour { get; set; }
        public decimal TotalActualHour { get; set; }
        public decimal TotalBalanceHour { get; set; }
        #endregion
    }
}
