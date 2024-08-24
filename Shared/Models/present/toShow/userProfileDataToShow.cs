using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorGoogleLogin.Shared.Models.present.toShow
{
    public class userProfileDataToShow
    {
        public int id { get; set; }
        public string? firstName { get; set; }
        public string? lastName { get; set; }

        
        public string? profilePicOrIcon { get; set; }

        public DateTime signUpDate { get; set; }

        public int? monthStartDate { get; set; }
        public string? streakStatus { get; set; }

        public bool passedOnboarding { get; set; }

        public List<TagsToShow> userTags { get; set; }
    }
}
