using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorGoogleLogin.Shared.Models.present.toShow
{
    public class OverBudgetToShow
    {
        public int id { get; set; }
        public string subCategoryTitle { get; set; }
        public double monthlyPlannedBudget { get; set; }
        public double remainingBudget { get; set; }
    }
}
