using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorGoogleLogin.Shared.Models.present.toShow
{
    public class userToShow
    {
        public int id { get; set; }
        public string firstName { get; set; }
        public string profilePicOrIcon { get; set; }
        public double spendingValueFullList { get; set; }
        public double incomeValueFullList { get; set; }
        public double budgetFullValue { get; set; }
        public string? streakStatus { get; set; }
        public List<CategoryToShow> categoriesFullList { get; set; }
    }
}
