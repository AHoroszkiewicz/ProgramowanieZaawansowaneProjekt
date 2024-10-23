using PartyTavern.Models;
using System;

namespace PartyTaveern.Models
{
    public class Post
    {
        public int Id { get; set; }                // Unikalny identyfikator posta
        public string Title { get; set; }          // Tytuł posta
        public string Content { get; set; }        // Treść posta
        public DateTime CreatedAt { get; set; }    // Data utworzenia posta
        public int UserId { get; set; }            // Id użytkownika, który stworzył post

        public User User { get; set; }
    }
}
