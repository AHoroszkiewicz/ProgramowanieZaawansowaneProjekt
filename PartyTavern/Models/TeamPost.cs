using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PartyTavern.Models
{
    public class TeamPost
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Wybierz grę.")]
        [ForeignKey("Game")]
        public int GameId { get; set; }
        public Game? Game { get; set; }

        [Required(ErrorMessage = "Wybierz rozmiar drużyny.")]
        [Range(2, 20, ErrorMessage = "Drużyna nie może być mniejsza niż 2 i większa niż 20.")]
        public int TeamSize { get; set; }

        public int CurrentTeamSize { get; set; }

        [Required]
        [StringLength(500, ErrorMessage = "Opis nie może być dłuższy niż 500 znaków.")]
        public string Description { get; set; } = string.Empty;

        public DateTime NeededBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string UserId { get; set; } = string.Empty;

        // Powiązanie z członkami drużyny
        public ICollection<TeamMember> TeamMembers { get; set; } = new List<TeamMember>();
    }
}
