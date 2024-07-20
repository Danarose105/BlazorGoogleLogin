using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorGoogleLogin.Shared.Models.present.toShow
{
    public class TagsAndSpendingsToShow
    {
        public int tagID { get; set; }
        public string tagTitle { get; set; }
        public string tagColor { get; set; }
        public double totalValue { get; set; }
    }
}
