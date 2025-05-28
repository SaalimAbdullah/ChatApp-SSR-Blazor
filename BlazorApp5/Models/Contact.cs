using BlazorApp5.Data;

namespace BlazorApp5.Models
{
    public class Contact
    {
        public int Id { get; set; }

        public string OwnerUserId { get; set; }         // FK to logged-in user
        public string ContactUserCode { get; set; }     // 4-digit code of the contact
        public string ContactDisplayName { get; set; }  // optional nickname

        // Navigation (optional)
        public ApplicationUser Owner { get; set; }
    }



}
