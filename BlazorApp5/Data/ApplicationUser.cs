using Microsoft.AspNetCore.Identity;

namespace BlazorApp5.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public string UserCode {  get; set; }
    }

}
