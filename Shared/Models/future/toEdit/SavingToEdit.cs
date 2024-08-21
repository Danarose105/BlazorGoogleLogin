using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorGoogleLogin.Shared.Models.future.toEdit
{
    public class SavingToEdit
    {
        public int id { get; set; }
        public string savingsTitle { get; set; }
        public int savingsSum { get; set; }
        public string? savingLocation { get; set; }
    }
}
