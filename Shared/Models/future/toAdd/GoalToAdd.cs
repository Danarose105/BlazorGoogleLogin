using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorGoogleLogin.Shared.Models.future.toAdd
{
    public class GoalToAdd
    {
            public int id { get; set; }
            public int userID { get; set; }
            public string goalTitle { get; set; }
            public int budget { get; set; }
            public int dueYear { get; set; }
            public string? color { get; set; }         
        
    }
}
