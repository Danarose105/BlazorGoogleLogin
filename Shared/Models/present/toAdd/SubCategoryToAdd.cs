using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorGoogleLogin.Shared.Models.present.toAdd
{
    public class SubCategoryToAdd
    {
        public int id { get; set; }
        public int categoryID { get; set; }
        public string subCategoryTitle { get; set; }
        public double? monthlyPlannedBudget { get; set; }
        public int? importance { get; set; }
    }
}
