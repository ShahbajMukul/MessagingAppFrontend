using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagingApp.Shared.Models.Payload
{
    public class RegistrationPayload
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(20, MinimumLength = 5)]
        public string Username { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(20)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(20)]
        public string LastName { get; set; }

        [Required(ErrorMessage = "An email address is required")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }

        [Required(ErrorMessage ="Failed to generate the RSA keys. System may not be supported. Please download our software")]
        public string PublicKey { get; set; }
    }
}
