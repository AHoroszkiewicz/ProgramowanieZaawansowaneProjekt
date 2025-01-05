using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace PartyTavern.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(500)]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; }

        public string UserId { get; set; }

        public virtual IdentityUser User { get; set; }

        public int PostId { get; set; }
        public virtual TeamPost Post { get; set; }

        public bool wasEdited { get; set; }
    }
}
