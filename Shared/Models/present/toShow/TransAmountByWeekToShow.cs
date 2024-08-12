using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorGoogleLogin.Shared.Models.present.toShow
{
    public class TransAmountByWeekToShow
    {
        public DateTime weekStartDate { get; set; }
        public int transactionCount { get; set; }
    }
}
