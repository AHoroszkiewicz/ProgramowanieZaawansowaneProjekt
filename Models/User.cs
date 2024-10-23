using Microsoft.Extensions.Hosting;
using PartyTaveern.Models;
using System.Collections.Generic;

namespace PartyTavern.Models
{ 
    public class User
    {
        public int Id { get; set; }                // Unikalny identyfikator użytkownika
        public string Username { get; set; }       // Nazwa użytkownika
        public string Password { get; set; }        // Hasło użytkownika (rozważ użycie szyfrowania)

        // Związek jeden-do-wielu z postami
        public ICollection<Post> Posts { get; set; } // Lista postów stworzonych przez użytkownika

        public User()
        {
            Posts = new List<Post>();
        }
    }
}
