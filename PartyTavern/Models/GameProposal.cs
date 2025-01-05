using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PartyTavern.Models
{
    public class GameProposal
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } // Nazwa gry

        [MaxLength(50)]
        public string Genre { get; set; } // Gatunek gry

        [Required]
        public string Description { get; set; } // Opis gry

        public string SubmittedByUserId { get; set; } // Identyfikator użytkownika zgłaszającego

        [ForeignKey("SubmittedByUserId")]
        public IdentityUser SubmittedByUser { get; set; } // Dane użytkownika zgłaszającego

        public bool IsApproved { get; set; } // Czy propozycja została zatwierdzona

        public DateTime SubmittedDate { get; set; } = DateTime.UtcNow; // Data zgłoszenia
    }
}
