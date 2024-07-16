using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorGoogleLogin.Shared.Models.present.toShow
{
    public class UserStreakData
    {
        public bool current_streak_group { get; set; }
        public int streak_length { get; set; }
    }
}
