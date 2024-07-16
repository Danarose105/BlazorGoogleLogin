using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorGoogleLogin.Shared.Models.present.toEdit
{
    public class SubCategoryToEdit
    {
        public int id { get; set; }
        public string subCategoryTitle { get; set; }
        public int categoryID { get; set; }
        public double? monthlyPlannedBudget { get; set; }
        public int importance { get; set; }
        public string categroyTitle { get; set; }
    }
}
