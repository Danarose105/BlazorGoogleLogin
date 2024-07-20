using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorGoogleLogin.Shared.Models.present.toShow
{
    public class CategoryToShow
    {
        public int id { get; set; }
        public string categroyTitle { get; set; }
        public string? icon { get; set; }
        public string? color { get; set; }
    }
}
