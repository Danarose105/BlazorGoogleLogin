using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorGoogleLogin.Shared.Models.present.toShow
{
    public class TransactionOverviewToShow
    {
        public int id { get; set; }
        public int transType { get; set; }
        public double transValue { get; set; }
        public string valueType { get; set; }
        public string transDate { get; set; }
        public bool? fixedMonthly { get; set; }
        public bool splitPayment { get; set; }
        public int? tagID { get; set; }
        public string? tagTitle { get; set; }
        public string? tagColor { get; set; }
        public string transTitle { get; set; }
        public int? parentTransID { get; set; }
        public string? description { get; set; }
    }
}
