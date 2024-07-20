using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorGoogleLogin.Shared.Models.present.toShow
{
    public class StorySubCategoryTotals
    {
        public string subCategoryTitle { get; set; }
        public double currentMonthTotal { get; set; }
        public double lastMonthTotal { get; set; }
       
    }
}
