using System.ComponentModel.DataAnnotations;

namespace CSUSK.Core.ViewModels
{
    public class ContactViewModel
    {
        [Required(ErrorMessage = "Please enter your name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please enter your email address")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; }

        [Required]
        [MaxLength(500, ErrorMessage = "Your message must be 500 characters or less")]
        public string Message { get; set; }

        public string SiteKey { get; set; }

        public string SendButtonDisabled
        {
            get
            {
                if(!string.IsNullOrEmpty(SiteKey))
                {
                    return "disabled";
                }
                return string.Empty;
            }
        }

    }
}
