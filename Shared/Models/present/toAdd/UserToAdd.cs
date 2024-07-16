using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorGoogleLogin.Shared.Models.present.toAdd
{
    public class UserToAdd
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string profilePicOrIcon { get; set; }
        public int monthStartDate { get; set; }
       
    }
}