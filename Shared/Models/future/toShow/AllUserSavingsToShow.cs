using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorGoogleLogin.Shared.Models.future.toShow
{
    public class AllUserSavingsToShow
    {
        public int id { get; set; }
        public int userID { get; set; }
        public string savingsTitle { get; set; }
        public int savingsSum { get; set; }

    }
}
