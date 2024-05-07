using Microsoft.AspNetCore.Identity;

namespace PANOPA.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string NameSurname { get; set; }
    }
}
