using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorGoogleLogin.Shared.Models.present.toAdd
{
    public class TransactionToAdd
    {
        public string transTitle { get; set; }
        public int subCategoryID { get; set; }
        public int transType { get; set; }
        public double transValue { get; set; }
        public string valueType { get; set; } //cash, credit, else
        public DateTime transDate { get; set; }
        public string? description { get; set; }
        public bool? fixedMonthly { get; set; }
        public bool splitPayment { get; set; }
        public int? parentTransID { get; set; }
        public int? tagID { get; set; }
        public string? tagTitle { get; set; }
        public string? tagColor { get; set; }
    }


}
