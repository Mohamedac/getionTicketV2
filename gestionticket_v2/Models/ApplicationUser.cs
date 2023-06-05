using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace gestionticket_v2.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? Nom { get; set; }
        public string? Prenom { get; set; }
        public string? UserType { get; set; }
        public virtual ICollection<Ticket> TicketsAssignes { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; }
        [NotMapped]
        public IList<string> Roles { get; set; } 
    }


}
