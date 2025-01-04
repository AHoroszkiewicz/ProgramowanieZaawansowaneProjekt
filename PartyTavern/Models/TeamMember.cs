using Microsoft.AspNetCore.Identity;

namespace PartyTavern.Models
{
    public class TeamMember
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public IdentityUser User { get; set; }

        public int TeamPostId { get; set; }
        public TeamPost TeamPost { get; set; }
    }
}
