using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorGoogleLogin.Shared.Models.future.toAdd
{
    public class SavingToAdd
    {
        public int id { get; set; }
        public int userID { get; set; }
        public string savingsTitle { get; set; }
        public int savingsSum { get; set; }
    }
}
